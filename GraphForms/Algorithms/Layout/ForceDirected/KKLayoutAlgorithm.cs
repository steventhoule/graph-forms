using System;
using GraphForms.Algorithms.Path;

namespace GraphForms.Algorithms.Layout.ForceDirected
{
    public class KKLayoutAlgorithm<Node, Edge>
        : LayoutAlgorithm<Node, Edge>
        where Node : class, ILayoutNode
        where Edge : IGraphEdge<Node>, IUpdateable
    {
        private AAllShortestPaths<Node, Edge> mShortestPathsAlg;
        /// <summary>
        /// Minimal distances between the nodes;
        /// </summary>
        private float[][] mDistances;
        // cache for speed-up
        //private Digraph<Node, Edge>.GNode[] mNodes;
        private int mNodeCount;
        /// <summary>
        /// Scene-level X-coordinates of new node positions
        /// </summary>
        private double[] mXs;
        /// <summary>
        /// Scene-level Y-coordinates of new node positions
        /// </summary>
        private double[] mYs;
        /// <summary>
        /// Whether or not each node is currently hidden.
        /// </summary>
        private bool[] mHidden;

        private float mK = 1;
        private bool bAdjustForGravity;
        private bool bExchangeVertices;
        private float mLengthFactor = 1;
        private float mDisconnectedMultiplier = 0.5f;

        private double mDiameter;
        private double mIdealEdgeLength;
        private double mMaxDistance;

        private bool bDirty = true;

        public KKLayoutAlgorithm(Digraph<Node, Edge> graph,
            IClusterNode clusterNode)
            : base(graph, clusterNode)
        {
            this.mShortestPathsAlg
                = new BasicAllShortestPaths<Node, Edge>(
                    new DijkstraShortestPath<Node, Edge>(
                        graph, false, false));
            this.mXs = new double[0];
            this.mYs = new double[0];
            this.mHidden = new bool[0];
            this.MaxIterations = 200;
        }

        public KKLayoutAlgorithm(Digraph<Node, Edge> graph,
            Box2F boundingBox)
            : base(graph, boundingBox)
        {
            this.mShortestPathsAlg
                = new BasicAllShortestPaths<Node, Edge>(
                    new DijkstraShortestPath<Node, Edge>(
                        graph, false, false));
            this.mXs = new double[0];
            this.mYs = new double[0];
            this.mHidden = new bool[0];
            this.MaxIterations = 200;
        }

        public float K
        {
            get { return this.mK; }
            set
            {
                if (this.mK != value)
                {
                    this.mK = value;
                }
            }
        }

        /// <summary>
        /// If true, then after the layout process, the nodes will be moved, 
        /// so the barycenter will be in the center point of the 
        /// this algorithm's cluster node's bounding box or its own
        /// bounding box if its cluster node is null. </summary>
        public bool AdjustForGravity
        {
            get { return this.bAdjustForGravity; }
            set
            {
                if (this.bAdjustForGravity != value)
                {
                    this.bAdjustForGravity = value;
                }
            }
        }

        public bool ExchangeVertices
        {
            get { return this.bExchangeVertices; }
            set
            {
                if (this.bExchangeVertices != value)
                {
                    this.bExchangeVertices = value;
                }
            }
        }
        
        /// <summary>
        /// Multiplier of the ideal edge length. 
        /// (With this parameter the user can modify the ideal edge length).
        /// </summary>
        public float LengthFactor
        {
            get { return this.mLengthFactor; }
            set
            {
                if (this.mLengthFactor != value)
                {
                    this.mLengthFactor = value;
                    this.bDirty = true;
                }
            }
        }
        
        /// <summary>
        /// Ideal distance between the disconnected points 
        /// (1 is equal the ideal edge length).
        /// </summary>
        public float DisconnectedMultiplier
        {
            get { return this.mDisconnectedMultiplier; }
            set
            {
                if (this.mDisconnectedMultiplier != value)
                {
                    this.mDisconnectedMultiplier = value;
                    this.bDirty = true;
                }
            }
        }

        protected override void PerformPrecalculations(
            uint lastNodeVersion, uint lastEdgeVersion)
        {
            bool isDirty = lastNodeVersion != this.mGraph.NodeVersion;
            if (isDirty)
            {
                Digraph<Node, Edge>.GNode gNode;
                this.mNodeCount = this.mGraph.NodeCount;
                // Cache the nodes for speed-up and in case the graph
                // is modified during the iteration.
                if (this.mHidden.Length < this.mNodeCount)
                {
                    this.mXs = new double[this.mNodeCount];
                    this.mYs = new double[this.mNodeCount];
                    this.mHidden = new bool[this.mNodeCount];
                }
                for (int k = 0; k < this.mNodeCount; k++)
                {
                    gNode = this.mGraph.InternalNodeAt(k);
                    this.mXs[k] = gNode.Data.X;
                    this.mYs[k] = gNode.Data.Y;
                    this.mHidden[k] = gNode.Hidden;
                }
                this.bDirty = true;
            }

            isDirty = isDirty || lastEdgeVersion != this.mGraph.EdgeVersion;
            if (isDirty)
            {
                // Calculate the distances and diameter of the graph.
                this.mShortestPathsAlg.Reset();
                this.mShortestPathsAlg.Compute();
                this.mDistances = this.mShortestPathsAlg.Distances;
                this.mDiameter = this.mShortestPathsAlg.GetDiameter();
                this.bDirty = true;
            }

            if (this.bDirty)
            {
                Box2F bbox = this.mClusterNode == null
                    ? this.BoundingBox : this.mClusterNode.LayoutBBox;

                // L0 is the length of a side of the display area
                float L0 = Math.Min(bbox.W, bbox.H);

                // ideal length = L0 / max distance
                this.mIdealEdgeLength 
                    = L0 * this.mLengthFactor / this.mDiameter;

                this.mMaxDistance 
                    = this.mDiameter * this.mDisconnectedMultiplier;

                // Calculate the ideal distances between the nodes
                /*float dist;
                int i, j;
                for (i = 0; i < this.mNodeCount; i++)
                {
                    for (j = 0; j < this.mNodeCount; j++)
                    {
                        // distance between non-adjacent nodes
                        dist = (float)Math.Min(this.mDistances[i][j], 
                                               this.mMaxDistance);
                        // calculate the minimal distance between the vertices
                        this.mDistances[i][j] = dist;
                    }
                }/* */
            }
            this.bDirty = false;
        }

        // TODO: Can the position copying from mXPositions and mYPositions
        // be reduced to the barycenter node that changes position and any nodes
        // which have been swapped on each iteration?
        protected override void PerformIteration(uint iteration)
        {
            int i, j;
            Node node;

            // copy positions into array
            // necessary each time because node positions can change outside
            // this algorithm, by constraining to bbox and by user code.
            for (i = 0; i < this.mNodeCount; i++)
            {
                if (!this.mHidden[i])
                {
                    node = this.mGraph.NodeAt(i);
                    if (node.PositionFixed)
                    {
                        this.mXs[i] = node.X;
                        this.mYs[i] = node.Y;
                    }
                }
            }

            double nx, ny, deltaM, maxDeltaM = double.NegativeInfinity;
            int pm = -1;

            // get the 'p' with max delta_m
            for (i = 0; i < this.mNodeCount; i++)
            {
                if (!this.mHidden[i])
                {
                    node = this.mGraph.NodeAt(i);
                    if (!node.PositionFixed)
                    {
                        deltaM = this.CalculateEnergyGradient(i);
                        if (maxDeltaM < deltaM)
                        {
                            maxDeltaM = deltaM;
                            pm = i;
                        }
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

            // Perform the bounding constraints normally done by base layout
            // algorithm. It needs to be done here so cached positions don't
            // need to be copied over on every single iteration, but still
            // allow the energy calculations to compensate for it.
            node = this.mGraph.NodeAt(pm);
            Box2F nbox = node.LayoutBBox;
            if (this.mClusterNode == null)
            {
                double d;
                Box2F bbox = this.BoundingBox;
                d = nbox.W;
                this.mXs[pm] = Math.Min(Math.Max(this.mXs[pm] + nbox.X, bbox.X) + d, bbox.Right) - nbox.X - d;
                d = nbox.H;
                this.mYs[pm] = Math.Min(Math.Max(this.mYs[pm] + nbox.Y, bbox.Y) + d, bbox.Bottom) - nbox.Y - d;
            }
            else
            {
                this.mClusterNode.LearnNodePos(
                    (float)this.mXs[pm], (float)this.mYs[pm], nbox);
                Vec2F pos = this.mClusterNode.AugmentNodePos(
                    (float)this.mXs[pm], (float)this.mYs[pm]);
                this.mXs[pm] = pos.X;
                this.mYs[pm] = pos.Y;
            }

            // What if two of the nodes were exchanged?
            if (this.bExchangeVertices && maxDeltaM < double.Epsilon)
            {
                double xenergy, energy = CalcEnergy();
                bool noSwaps = true;
                for (i = 0; i < this.mNodeCount && noSwaps; i++)
                {
                    if (!this.mHidden[i])
                    {
                        for (j = 0; j < this.mNodeCount && noSwaps; j++)
                        {
                            if (i == j || this.mHidden[i])
                                continue;
                            xenergy = CalcEnergyIfExchanged(i, j);
                            if (energy > xenergy)
                            {
                                deltaM = this.mXs[i];
                                this.mXs[i] = this.mXs[j];
                                this.mXs[j] = deltaM;
                                deltaM = this.mYs[i];
                                this.mYs[i] = this.mYs[j];
                                this.mYs[j] = deltaM;
                                noSwaps = false;
                            }
                        }
                    }
                }
            }
            // Use a linear transition to make the animation smooth
            // pos = pos + 0.1 * (newPos - pos) = 0.9 * pos + 0.1 * newPos
            for (i = 0; i < this.mNodeCount; i++)
            {
                if (!this.mHidden[i])
                {
                    node = this.mGraph.NodeAt(i);
                    if (!node.PositionFixed)
                    {
                        //node.SetPosition((float)this.mXPositions[i], 
                        //                 (float)this.mYPositions[i]);
                        nx = 0.9 * node.X + 0.1 * this.mXs[i];
                        ny = 0.9 * node.Y + 0.1 * this.mYs[i];
                        node.SetPosition((float)nx, (float)ny);
                    }
                }
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
            int i, j, ii, jj;
            for (i = 0; i < this.mNodeCount; i++)
            {
                if (this.mHidden[i])
                    continue;
                for (j = 0; j < this.mNodeCount; j++)
                {
                    if (i == j || this.mHidden[j])
                        continue;
                    ii = (i == p) ? q : i;
                    jj = (j == q) ? p : j;
                    k_ij = Math.Min(this.mDistances[i][j], this.mMaxDistance);
                    l_ij = this.mIdealEdgeLength * k_ij;
                    k_ij = this.mK / (k_ij * k_ij);
                    dx = this.mXs[ii] - this.mXs[jj];
                    dy = this.mYs[ii] - this.mYs[jj];

                    energy += k_ij / 2 * (dx * dx + dy * dy + l_ij * l_ij
                        - 2 * l_ij * Math.Sqrt(dx * dx + dy * dy));
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
            int i, j;
            for (i = 0; i < this.mNodeCount; i++)
            {
                if (this.mHidden[i])
                    continue;
                for (j = 0; j < this.mNodeCount; j++)
                {
                    if (i == j || this.mHidden[j])
                        continue;
                    dist = Math.Min(this.mDistances[i][j], this.mMaxDistance);
                    l_ij = this.mIdealEdgeLength * dist;
                    k_ij = this.mK / (dist * dist);
                    dx = this.mXs[i] - this.mXs[j];
                    dy = this.mYs[i] - this.mYs[j];

                    energy += k_ij / 2 * (dx * dx + dy * dy + l_ij * l_ij 
                        - 2 * l_ij * Math.Sqrt(dx * dx + dy * dy));
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
            for (int i = 0; i < this.mNodeCount; i++)
            {
                if (i != m && !this.mHidden[i])
                {
                    //common things
                    k = Math.Min(this.mDistances[m][i], this.mMaxDistance);
                    l = this.mIdealEdgeLength * k;
                    k = this.mK / (k * k);
                    dx = this.mXs[m] - this.mXs[i];
                    dy = this.mYs[m] - this.mYs[i];

                    //distance between the points
                    d = Math.Max(Math.Sqrt(dx * dx + dy * dy), 0.000001);
                    ddd = d * d * d;

                    dxm += k * (1 - l / d) * dx;
                    dym += k * (1 - l / d) * dy;
                    // TODO: isn't it wrong?
                    d2xm += k * (1 - l * dy * dy / ddd);
                    //d2E_d2xm += k_mi * (1 - l_mi / d + l_mi * dx * dx / ddd);
                    dxmdym += k * l * dx * dy / ddd;
                    //d2E_d2ym += k_mi * (1 - l_mi / d + l_mi * dy * dy / ddd);
                    // TODO: isn't it wrong?
                    d2ym += k * (1 - l * dx * dx / ddd);
                }
            }
            // d2E_dymdxm equals to d2E_dxmdym
            dymdxm = dxmdym;

            double denomi = d2xm * d2ym - dxmdym * dymdxm;
            this.mXs[m] = this.mXs[m] 
                + (dxmdym * dym - d2ym * dxm) / denomi;
            this.mYs[m] = this.mYs[m] 
                + (dymdxm * dxm - d2xm * dym) / denomi;
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
            for (int i = 0; i < this.mNodeCount; i++)
            {
                if (i == m || this.mHidden[i])
                    continue;

                //differences of the positions
                dx = this.mXs[m] - this.mXs[i];
                dy = this.mYs[m] - this.mYs[i];

                //distances of the two vertex (by positions)
                d = Math.Max(Math.Sqrt(dx * dx + dy * dy), 0.000001);
                factor = Math.Min(this.mDistances[m][i], this.mMaxDistance);
                factor = (this.mK / (factor * factor)) *
                    (1 - this.mIdealEdgeLength * factor / d);
                dxm += factor * dx;
                dym += factor * dy;
            }
            // delta_m = sqrt((dE/dx)^2 + (dE/dy)^2)
            return Math.Sqrt(dxm * dxm + dym * dym);
        }
    }
}
