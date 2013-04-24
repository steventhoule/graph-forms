using System;
using System.Drawing;

namespace GraphForms.Algorithms.Layout.ForceDirected
{
    public class FRLayoutAlgorithm<Node, Edge>
        : ForceDirectedLayoutAlgorithm<Node, Edge, FRLayoutParameters>
        where Node : GraphElement, ILayoutNode
        where Edge : class, IGraphEdge<Node>, IUpdateable
    {
        /// <summary>
        /// Actual temperature of the 'mass'. Used for cooling.
        /// </summary>
        private double mTemperature;

        private double mMinimalTemperature;
        /// <summary>
        /// Constant of Attraction calculated from the parameters
        /// </summary>
        private double mCoA;
        /// <summary>
        /// Constant of Repulsion calculated from the parameters
        /// </summary>
        private double mCoR;
        private double mLambda;
        private FRLayoutParameters.Cooling mCoolingFunction;

        protected override FRLayoutParameters DefaultParameters
        {
            get { return new FRFreeLayoutParameters(); }
        }

        public FRLayoutAlgorithm(DirectionalGraph<Node, Edge> graph)
            : base(graph, null)
        {
        }

        public FRLayoutAlgorithm(DirectionalGraph<Node, Edge> graph,
            FRLayoutParameters oldParameters)
            : base(graph, oldParameters)
        {
        }

        protected override void InitializeAlgorithm()
        {
            base.InitializeAlgorithm();
            this.mTemperature = this.Parameters.InitialTemperature;
        }

        protected override bool OnBeginIteration(bool paramsDirty, int lastNodeCount, int lastEdgeCount)
        {
            if (paramsDirty)
            {
                FRLayoutParameters param = this.Parameters;
                // Update the node count just in case it changed,
                // in order to force parameter recalculation.
                param.NodeCount = this.mGraph.NodeCount;
                // Don't worry about making NodeCount public,
                // since the above action will erase any user changes 
                // to the NodeCount value between iterations.

                this.mMinimalTemperature = param.InitialTemperature * 0.01;
                this.mCoA = param.ConstantOfAttraction;
                this.mCoR = param.ConstantOfRepulsion;
                this.mLambda = param.Lambda;
                this.mCoolingFunction = param.CoolingFunction;
            }
            return base.OnBeginIteration(paramsDirty, lastNodeCount, lastEdgeCount);
        }

        protected override bool CanIterate()
        {
            return this.mTemperature > this.mMinimalTemperature;
        }

        protected override void PerformIteration(int iteration, int maxIterations)
        {
            this.IterateOne();

            // cool down graph
            switch (this.mCoolingFunction)
            {
                case FRLayoutParameters.Cooling.Linear:
                    this.mTemperature *= (1.0 - (double)iteration / maxIterations);
                    break;
                case FRLayoutParameters.Cooling.Exponential:
                    this.mTemperature *= this.mLambda;
                    break;
            }
        }

        private static readonly PointF sZero = new PointF(0, 0);

        private void IterateOne()
        {
            int i, j;
            SizeF delta;
            double forceX, forceY, dx, dy, length, factor;

            float[] newXs = this.NewXPositions;
            float[] newYs = this.NewYPositions;
            DirectionalGraph<Node, Edge>.GraphNode[] nodes = this.mGraph.InternalNodes;
            DirectionalGraph<Node, Edge>.GraphEdge[] edges;
            Node u, v;

            // Repulsive forces
            for (i = 0; i < nodes.Length; i++)
            {
                v = nodes[i].Data;
                if (v.PositionFixed)
                    continue;
                nodes[i].Index = i;
                forceX = 0; forceY = 0;
                for (j = 0; j < nodes.Length; j++)
                {
                    // doesn't repulse itself
                    if (j != i)
                    {
                        // calculating repulsive force
                        u = nodes[j].mData;
                        delta = v.ItemTranslate(u);
                        dx = delta.Width;
                        dy = delta.Height;
                        length = Math.Max(dx * dx + dy * dy, 0.000001);
                        factor = this.mCoR / length;

                        forceX += dx * factor;
                        forceY += dy * factor;
                    }
                }
                //v.NewX = (float)forceX;
                //v.NewY = (float)forceY;
                newXs[i] = (float)forceX;
                newYs[i] = (float)forceY;
            }

            // Attractive forces
            edges = this.mGraph.InternalEdges;
            for (i = 0; i < edges.Length; i++)
            {
                v = edges[i].SrcNode.Data;
                if (v.PositionFixed)
                    continue;
                u = edges[i].DstNode.Data;
                if (u == v)
                    continue;

                // calculating attractive forces between two nodes
                delta = v.ItemTranslate(u);
                dx = delta.Width;
                dy = delta.Height;
                length = Math.Sqrt(dx * dx + dy * dy);
                factor = edges[i].Data.Weight;
                factor = Math.Max(length / this.mCoA * factor, 0.000001);

                //v.NewX = v.NewX - (float)(dx * factor);
                //v.NewY = v.NewY - (float)(dy * factor);
                j = edges[i].SrcNode.Index;
                newXs[j] = newXs[j] - (float)(dx / factor);
                newYs[j] = newYs[j] - (float)(dy / factor);
            }

            // Limit Displacement
            for (i = 0; i < nodes.Length; i++)
            {
                // limit force to ambient temperature.
                v = nodes[i].Data;
                if (v.PositionFixed)
                {
                    //v.NewX = v.X;
                    //v.NewY = v.Y;
                    newXs[i] = v.X;
                    newYs[i] = v.Y;
                    continue;
                }
                //dx = v.NewX;
                //dy = v.NewY;
                dx = newXs[i];
                dy = newYs[i];
                length = Math.Max(Math.Sqrt(dx * dx + dy * dy), 0.000001);
                factor = Math.Min(length, this.mTemperature) / length;

                // Add the force to the old position
                //v.NewX = (float)(v.X + dx * factor);
                //v.NewY = (float)(v.Y + dy * factor);
                newXs[i] = (float)(v.X + dx * factor);
                newYs[i] = (float)(v.Y + dy * factor);

                // Constraining new position to within scene bounding box
                // is already handled by ForceDirLayoutAlgorithm
            }
        }
    }
}
