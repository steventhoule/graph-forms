﻿using System;
using System.Drawing;

namespace GraphForms.Algorithms.Layout.ForceDirected
{
    public class KKLayoutAlgorithm<Node, Edge>
        : ForceDirectedLayoutAlgorithm<Node, Edge, KKLayoutParameters>
        where Node : GraphElement, ILayoutNode
        where Edge : class, IGraphEdge<Node>, IUpdateable
    {
        /// <summary>
        /// Minimal distances between the nodes;
        /// </summary>
        private float[,] mDistances;
#if KKExtraCache
        private float[,] mEdgeLengths;
        private float[,] mSpringConstants;
#endif
        // cache for speed-up
        private Node[] mNodes;
        /// <summary>
        /// Scene-level X-coordinates of new node positions
        /// </summary>
        private double[] mXPositions;
        /// <summary>
        /// Scene-level Y-coordinates of new node positions
        /// </summary>
        private double[] mYPositions;

        private double mK;
        private bool bAdjustForGravity;
        private bool bExchangeVertices;
        private float mDiameter;
        private float mIdealEdgeLength;
        private float mMaxDistance;

        public KKLayoutAlgorithm(DirectionalGraph<Node, Edge> graph)
            : base(graph, null)
        {
        }

        public KKLayoutAlgorithm(DirectionalGraph<Node, Edge> graph,
            KKLayoutParameters oldParameters)
            : base(graph, oldParameters)
        {
        }

        protected override bool OnBeginIteration(bool paramsDirty,
            int lastNodeCount, int lastEdgeCount)
        {
            bool dirty = lastNodeCount != this.mGraph.NodeCount;
            if (dirty)
            {
                // Cache the nodes for speed-up and in case the graph
                // is modified during the iteration.
                this.mNodes = this.mGraph.Nodes;
                this.mXPositions = new double[this.mNodes.Length];
                this.mYPositions = new double[this.mNodes.Length];
            }

            dirty = dirty || lastEdgeCount != this.mGraph.EdgeCount;
            if (dirty)
            {
                // Calculate the distances and diameter of the graph.
                this.mDistances = this.mGraph.GetDistances();
                this.mDiameter = DirectionalGraph<Node, Edge>.GetDiameter(this.mDistances);
            }

            if (paramsDirty)
            {
                KKLayoutParameters param = this.Parameters;
                this.mK = param.K;
                this.bAdjustForGravity = param.AdjustForGravity;
                this.bExchangeVertices = param.ExchangeVertices;

                // L0 is the length of a side of the display area
                float L0 = Math.Min(param.Width, param.Height);

                // ideal length = L0 / max distance
                this.mIdealEdgeLength = L0 * param.LengthFactor / this.mDiameter;

                this.mMaxDistance = this.mDiameter * param.DisconnectedMultiplier;

                // Calculate the ideal distances between the nodes
                float dist;
                int i, j, count = this.mNodes.Length;
                for (i = 0; i < count; i++)
                {
                    for (j = 0; j < count; j++)
                    {
                        // distance between non-adjacent nodes
                        dist = Math.Min(this.mDistances[i, j], this.mMaxDistance);

                        // calculate the minimal distance between the vertices
                        this.mDistances[i, j] = dist;
#if KKExtraCache
                    this.mEdgeLengths[i, j] = this.mIdealEdgeLength * dist;
                    this.mSpringConstants[i, j] = (float)(this.mK / (dist * dist));
#endif
                    }
                }
            }
            return base.OnBeginIteration(paramsDirty, lastNodeCount, lastEdgeCount);
        }

        // TODO: Can the position copying from mXPositions and mYPositions
        // be reduced to the barycenter node that changes position and any nodes
        // which have been swapped on each iteration?
        protected override void PerformIteration(int iteration, int maxIterations)
        {
            int i, j, count = this.mNodes.Length;

            // copy positions into array
            // necessary each time because node positions can change outside
            // this algorithm, by constraining to bbox and by user code.
            SizeF pos;
            for (i = 0; i < count; i++)
            {
                pos = this.mNodes[i].SceneTranslate();
                this.mXPositions[i] = pos.Width;
                this.mYPositions[i] = pos.Height;
            }

            double deltaM, maxDeltaM = double.NegativeInfinity;
            int pm = -1;

            // get the 'p' with max delta_m
            for (i = 0; i < count; i++)
            {
                if (!this.mNodes[i].PositionFixed)
                {
                    deltaM = this.CalculateEnergyGradient(i);
                    if (maxDeltaM < deltaM)
                    {
                        maxDeltaM = deltaM;
                        pm = i;
                    }
                }
            }
            // TODO: is this needed?
            if (pm == -1)
            {
                this.Abort();
                return;
            }

            // Calculate the delta_x & delta_y with the Newton-Raphson method
            // Upper-bound for the while (deltaM > epsilon) {...} cycle (100)
            for (i = 0; i < 100; i++)
            {
                this.CalcDeltaXY(pm);

                deltaM = this.CalculateEnergyGradient(pm);
                // real stop condition
                if (deltaM < double.Epsilon)
                    break;
            }

            // What if two of the nodes were exchanged?
            if (this.bExchangeVertices && maxDeltaM < double.Epsilon)
            {
                double xenergy, energy = CalcEnergy();
                bool noSwaps = true;
                for (i = 0; i < count && noSwaps; i++)
                {
                    for (j = 0; j < count && noSwaps; j++)
                    {
                        if (i == j)
                            continue;
                        xenergy = CalcEnergyIfExchanged(i, j);
                        if (energy > xenergy)
                        {
                            deltaM = this.mXPositions[i];
                            this.mXPositions[i] = this.mXPositions[j];
                            this.mXPositions[j] = deltaM;
                            deltaM = this.mYPositions[i];
                            this.mYPositions[i] = this.mYPositions[j];
                            this.mYPositions[j] = deltaM;
                            noSwaps = false;
                        }
                    }
                }
            }
            float[] newXs = this.NewXPositions;
            float[] newYs = this.NewYPositions;
            Node node;
            SizeF sPos;
            for (i = 0; i < count; i++)
            {
                node = this.mNodes[i];
                sPos = node.SceneTranslate();
                //node.NewX = node.X + (float)this.mXPositions[i] - sPos.Width;
                //node.NewY = node.Y + (float)this.mYPositions[i] - sPos.Height;
                newXs[i] = node.X + (float)this.mXPositions[i] - sPos.Width;
                newYs[i] = node.Y + (float)this.mYPositions[i] - sPos.Height;
            }
        }

        /// <summary>
        /// Calculates the energy of the state where the positions of the nodes
        /// at <paramref name="p"/> and <paramref name="q"/> are exchanged.
        /// </summary>
        /// <param name="p">The index of the node exchanged with 
        /// the node at <paramref name="q"/>.</param>
        /// <param name="q">The index of the node exchanged with 
        /// the node at <paramref name="p"/>.</param>
        /// <returns>The energy of the state where the positions of the nodes
        /// at <paramref name="p"/> and <paramref name="q"/> are exchanged.
        /// </returns>
        private double CalcEnergyIfExchanged(int p, int q)
        {
            double l_ij, k_ij, dx, dy, energy = 0;
            int i, j, ii, jj, count = this.mNodes.Length;
            for (i = 0; i < count; i++)
            {
                for (j = 0; j < count; j++)
                {
                    if (i == j)
                        continue;
                    ii = (i == p) ? q : i;
                    jj = (j == q) ? p : j;
#if KKExtraCache
                    l_ij = this.mEdgeLengths[i, j];
                    k_ij = this.mSpringConstants[i, j];
#else
                    k_ij = Math.Min(this.mDistances[i, j], this.mMaxDistance);
                    l_ij = this.mIdealEdgeLength * k_ij;
                    k_ij = this.mK / (k_ij * k_ij);
#endif
                    dx = this.mXPositions[ii] - this.mXPositions[jj];
                    dy = this.mYPositions[ii] - this.mYPositions[jj];

                    energy += k_ij / 2 * (dx * dx + dy * dy + l_ij * l_ij -
                                           2 * l_ij * Math.Sqrt(dx * dx + dy * dy));
                }
            }
            return energy;
        }

        /// <summary>
        /// Calculates the energy of the spring system.
        /// </summary>
        /// <returns>The energy of the spring system.</returns>
        private double CalcEnergy()
        {
            double energy = 0, dist, l_ij, k_ij, dx, dy;
            int i, j, count = this.mNodes.Length;
            for (i = 0; i < count; i++)
            {
                for (j = 0; j < count; j++)
                {
                    if (i == j)
                        continue;
                    dist = Math.Min(this.mDistances[i, j], this.mMaxDistance);
#if KKExtraCache
                    l_ij = this.mEdgeLengths[i, j];
                    k_ij = this.mSpringConstants[i, j];
#else
                    l_ij = this.mIdealEdgeLength * dist;
                    k_ij = this.mK / (dist * dist);
#endif
                    dx = this.mXPositions[i] - this.mXPositions[j];
                    dy = this.mYPositions[i] - this.mYPositions[j];

                    energy += k_ij / 2 * (dx * dx + dy * dy + l_ij * l_ij -
                                           2 * l_ij * Math.Sqrt(dx * dx + dy * dy));
                }
            }
            return energy;
        }

        /// <summary>
        /// Determines a step to a new position for a node, 
        /// and adds that step to the new position of that node.
        /// </summary>
        /// <param name="m">The index of the node.</param>
        /// <returns>The step to a new position for the node at 
        /// <paramref name="m"/>.</returns>
        private void CalcDeltaXY(int m)
        {
            double dxm = 0, dym = 0, d2xm = 0, dxmdym = 0, dymdxm = 0, d2ym = 0;
            double l, k, dx, dy, d, ddd;
            Node node = this.mNodes[m];
            for (int i = 0; i < this.mNodes.Length; i++)
            {
                if (i != m)
                {
                    //common things
#if KKExtraCache
                    l = this.mEdgeLengths[m, i];
                    k = this.mSpringConstants[m, i];
#else
                    k = Math.Min(this.mDistances[m, i], this.mMaxDistance);
                    l = this.mIdealEdgeLength * k;
                    k = this.mK / (k * k);
#endif
                    dx = this.mXPositions[m] - this.mXPositions[i];
                    dy = this.mYPositions[m] - this.mYPositions[i];

                    //distance between the points
                    d = Math.Max(Math.Sqrt(dx * dx + dy * dy), 0.000001);
                    ddd = d * d * d;

                    dxm += k * (1 - l / d) * dx;
                    dym += k * (1 - l / d) * dy;
                    // TODO: isn't it wrong?
                    d2xm += k * (1 - l * dy * dy / ddd);
                    // d2E_d2xm += k_mi * ( 1 - l_mi / d + l_mi * dx * dx / ddd );
                    dxmdym += k * l * dx * dy / ddd;
                    // d2E_d2ym += k_mi * ( 1 - l_mi / d + l_mi * dy * dy / ddd );
                    // TODO: isn't it wrong?
                    d2ym += k * (1 - l * dx * dx / ddd);
                }
            }
            // d2E_dymdxm equals to d2E_dxmdym
            dymdxm = dxmdym;

            double denomi = d2xm * d2ym - dxmdym * dymdxm;
            this.mXPositions[m] = this.mXPositions[m] + (dxmdym * dym - d2ym * dxm) / denomi;
            this.mYPositions[m] = this.mYPositions[m] + (dymdxm * dxm - d2xm * dym) / denomi;
        }

        /// <summary>
        /// Calculates the gradient energy of a node.
        /// </summary>
        /// <param name="m">The index of the node.</param>
        /// <returns>The gradient energy of the node at <paramref name="m"/>.
        /// </returns>
        private double CalculateEnergyGradient(int m)
        {
            double dxm = 0, dym = 0, dx, dy, d, factor;
            //        {  1, if m < i
            // sign = { 
            //        { -1, if m > i
            for (int i = 0; i < this.mNodes.Length; i++)
            {
                if (i == m)
                    continue;

                //differences of the positions
                dx = this.mXPositions[m] - this.mXPositions[i];
                dy = this.mYPositions[m] - this.mYPositions[i];

                //distances of the two vertex (by positions)
                d = Math.Max(Math.Sqrt(dx * dx + dy * dy), 0.000001);
#if KKExtraCache
                factor = this.mSpringConstants[m, i] * (1 - this.mEdgeLengths[m, i] / d);
#else
                factor = Math.Min(this.mDistances[m, i], this.mMaxDistance);
                factor = (this.mK / (factor * factor)) *
                    (1 - this.mIdealEdgeLength * factor / d);
#endif
                dxm += factor * dx;
                dym += factor * dy;
            }
            // delta_m = sqrt((dE/dx)^2 + (dE/dy)^2)
            return Math.Sqrt(dxm * dxm + dym * dym);
        }
    }
}
