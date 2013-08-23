using System;

namespace GraphForms.Algorithms.Layout
{
    /// <summary>
    /// The base class for algorithms which calculate the layout of a given
    /// <see cref="T:Digraph`2{Node,Edge}"/> instance by setting the
    /// positions of its <typeparamref name="Node"/> instances based on the
    /// <typeparamref name="Edge"/> instances connecting them and their own
    /// unique parameters and constraints.
    /// </summary>
    /// <typeparam name="Node">The type of layout nodes in the graph 
    /// which are rearranged by this layout algorithm.</typeparam>
    /// <typeparam name="Edge">The type of edges that connect the 
    /// <typeparamref name="Node"/> instances that this algorithm 
    /// rearranges.</typeparam>
    public abstract class LayoutAlgorithm<Node, Edge>
        : AGraphAlgorithm<Node, Edge>
        where Node : class, ILayoutNode
        where Edge : IGraphEdge<Node>, IUpdateable
    {
        /*/// <summary>
        /// The graph which this layout algorithm operates on.
        /// </summary>
        protected readonly Digraph<Node, Edge> mGraph;/* */
        /// <summary>
        /// If <see cref="mGraph"/> is actually a sub-graph, then
        /// this is the node in the graph superstructure that encloses it.
        /// </summary>
        protected readonly IClusterNode mClusterNode;
        /// <summary>
        /// This bounding box is used as a substitute for 
        /// <see cref="mClusterNode"/> if it is null.
        /// </summary>
        private Box2F mBBox;

        /*/// <summary>
        /// Flags whether the fields of this algorithm have changed 
        /// and need to be refreshed on the next iteration.
        /// </summary>
        private bool bDirty = true;/* */
        /// <summary>
        /// The node version of the graph after the last precalculation.
        /// </summary>
        private uint mLastNVers;
        /// <summary>
        /// The edge version of the graph after the last precalculation.
        /// </summary>
        private uint mLastEVers;

        /// <summary>
        /// Whether the algorithm should reset back to its starting point
        /// and re-initialize before beginning the next iteration.
        /// </summary>
        private bool bResetting;
        /// <summary>
        /// Whether any nodes changed position during the last iteration.
        /// If nodes aren't moving, the algorithm has obviously reached an
        /// equilibrium and finished.
        /// </summary>
        private bool bItemMoved;
        /*/// <summary>
        /// Whether this layout algorithm is currently in an iteration and 
        /// executing code between <see cref="BeginIteration(uint)"/> and
        /// <see cref="EndIteration(uint)"/>.</summary>
        private bool bInIteration = false;/* */

        /// <summary>
        /// The X-coordinates of the positions of all the nodes 
        /// in the graph at the end of the last iteration.
        /// </summary>
        private float[] mLastXs;
        /// <summary>
        /// The Y-coordinates of the positions of all the nodes 
        /// in the graph at the end of the last iteration.
        /// </summary>
        private float[] mLastYs;

        private ComputeState mAsyncState = ComputeState.None;
        private uint mIter;

        private uint mMaxIterations = 2000;
        private ILayoutSpring mSpring = null;
        private double mMovementTolerance = 0.00001;

        /// <summary>
        /// Creates a new layout algorithm instance to operate on the given 
        /// <paramref name="graph"/> using the given 
        /// <see cref="IClusterNode"/> instance to affect the positions 
        /// of the graph's nodes.</summary>
        /// <param name="graph">The graph that this layout algorithm will
        /// operate on by setting the positions of its nodes.</param>
        /// <param name="clusterNode">The <see cref="IClusterNode"/>
        /// instance that affects the positions of the nodes in the
        /// <paramref name="graph"/>.</param>
        /// <seealso cref="Graph"/><seealso cref="ClusterNode"/>
        public LayoutAlgorithm(Digraph<Node, Edge> graph, 
            IClusterNode clusterNode)
            : base(graph)
        {
            if (clusterNode == null)
                throw new ArgumentNullException("clusterNode");
            this.mClusterNode = clusterNode;
            this.mBBox = new Box2F(0, 0, 0, 0);

            uint vers = graph.NodeVersion;
            this.mLastNVers = vers == 0 ? uint.MaxValue : vers - 1;
            vers = graph.EdgeVersion;
            this.mLastEVers = vers == 0 ? uint.MaxValue : vers - 1;

            this.mLastXs = new float[0];
            this.mLastYs = new float[0];
        }

        /// <summary>
        /// Creates a new layout algorithm instance to operate on the given
        /// <paramref name="graph"/> using the given <see cref="RectangleF"/>
        /// instance to constrain the positions of the graph's nodes to
        /// within its boundaries.
        /// </summary>
        /// <param name="graph">The graph that this layout algorithm will
        /// operate on by setting the positions of its nodes.</param>
        /// <param name="boundingBox">The <see cref="RectangleF"/> instance
        /// that constrains the positions of the nodes in the
        /// <paramref name="graph"/> to within its boundaries.</param>
        public LayoutAlgorithm(Digraph<Node, Edge> graph,
            Box2F boundingBox)
            : base(graph)
        {
            if (boundingBox == null)
                throw new ArgumentNullException("boundingBox");
            this.mClusterNode = null;
            this.mBBox = boundingBox;

            uint vers = graph.NodeVersion;
            this.mLastNVers = vers == 0 ? uint.MaxValue : vers - 1;
            vers = graph.EdgeVersion;
            this.mLastEVers = vers == 0 ? uint.MaxValue : vers - 1;

            this.mLastXs = new float[0];
            this.mLastYs = new float[0];
        }

        /*/// <summary>
        /// The graph that this layout algorithm operates on,
        /// computing and setting the positions of its 
        /// <typeparamref name="Node"/> instances.
        /// </summary>
        public Digraph<Node, Edge> Graph
        {
            get { return this.mGraph; }
        }/* */

        /// <summary>
        /// If <see cref="mGraph"/> is actually a sub-graph, then
        /// this is the node in the graph superstructure that encloses it.
        /// </summary>
        /// <remarks>
        /// This node is used to calculate the boundary positions of
        /// port nodes based on their angle around the center of its 
        /// bounding box.
        /// This node also affects the positioning each layout node when
        /// they are being moved or shuffled by this algorithm.
        /// </remarks>
        public IClusterNode ClusterNode
        {
            get { return this.mClusterNode; }
        }

        /// <summary>
        /// This bounding box is used as a substitute for 
        /// <see cref="ClusterNode"/> if it is null.
        /// </summary><remarks><para>
        /// When this bounding box is used in place of a cluster node, 
        /// the positions of layout nodes are restricted to inside it 
        /// when they are being moved and shuffled by this algorithm.
        /// </para><para>
        /// This box is also used in place of the cluster node for
        /// calculating the boundary positions of port nodes based on
        /// their angle around its center.
        /// </para></remarks>
        public Box2F BoundingBox
        {
            get 
            { 
                return new Box2F(this.mBBox); 
            }
            set
            {
                if (this.State != ComputeState.Running &&
                    this.mAsyncState != ComputeState.Running &&
                    value != null)
                {
                    this.mBBox = new Box2F(value);
                }
            }
        }

        /*/// <summary>
        /// Notifies this algorithm that it needs to be refreshed at the 
        /// beginning of the next iteration of its layout computation.
        /// </summary>
        protected void MarkDirty()
        {
            this.bDirty = true;
        }/* */

        /// <summary>
        /// Resets this algorithm back to its starting point, which causes it
        /// to re-initialize before beginning its next iteration.
        /// </summary><remarks>
        /// This function is useful for when the positions of one or more
        /// nodes are changed by external code (such as a user dragging them
        /// with their mouse) before this algorithm finishes or aborts its
        /// layout computation.</remarks>
        public void ResetAlgorithm()
        {
            this.bResetting = true;
        }

        /// <summary>
        /// Whether any nodes changed position during the last iteration.
        /// If nodes aren't moving, the algorithm has obviously reached an
        /// equilibrium and finished.
        /// </summary>
        public bool ItemMoved
        {
            get { return this.bItemMoved; }
        }

        /*/// <summary>
        /// Whether this layout algorithm is currently in an iteration.
        /// </summary>
        public bool InIteration
        {
            get { return this.bInIteration; }
        }/* */

        /// <summary>
        /// The current state of the asynchronous computation 
        /// of this layout algorithm.
        /// </summary>
        public ComputeState AsyncState
        {
            get { return this.mAsyncState; }
        }

        /// <summary>
        /// The current number of asynchronous iterations completed by the
        /// asynchronous computation of this layout algorithm.
        /// </summary>
        public uint AsyncIterations
        {
            get { return this.mIter; }
        }

        /// <summary>
        /// The maximum number of iterations this algorithm is allowed 
        /// to perform before finishing or aborting its layout computation.
        /// </summary>
        public uint MaxIterations
        {
            get { return this.mMaxIterations; }
            set
            {
                if (this.State != ComputeState.Running &&
                    this.mAsyncState != ComputeState.Running)
                    this.mMaxIterations = value;
            }
        }

        /// <summary>
        /// The layout spring used to create a "twaining" animation
        /// from each node's current position to their new position.
        /// If null, each node's position is immediately set to its
        /// new position.
        /// </summary>
        public ILayoutSpring Spring
        {
            get { return this.mSpring; }
            set
            {
                if (this.State != ComputeState.Running &&
                    this.mAsyncState != ComputeState.Running)
                    this.mSpring = value;
            }
        }

        /// <summary>
        /// This is the minimum distance squared that a node has have been
        /// moved by this layout algorithm or by <see cref="Spring"/> in
        /// order for the node to be considered "moved" on each iteration.
        /// If none of the nodes have moved a squared distance greater than
        /// this tolerance value, the algorithm stops iterating and finishes.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The new tolerance
        /// is less than zero.</exception>
        public double MovementTolerance
        {
            get { return this.mMovementTolerance; }
            set
            {
                if (this.State != ComputeState.Running &&
                    this.mAsyncState != ComputeState.Running)
                {
                    if (value < 0)
                    {
                        throw new ArgumentOutOfRangeException(
                            "MovementTolerance");
                    }
                    this.mMovementTolerance = value;
                }
            }
        }

        /*/// <summary>
        /// Tries to get the internal node in this layout algorithm's
        /// <see cref="Graph"/> that corresponds to the root node set with
        /// the <see cref="M:ARootedAlgorithm`1{Node}.SetRoot(Node)"/>
        /// function.
        /// </summary>
        /// <returns>The internal node in this algorithm's graph that 
        /// corresponds to this algorithm's root value, or the first node in
        /// the graph if there is corresponding node, or null if this
        /// algorithm's graph is currently empty.</returns>
        public Digraph<Node, Edge>.GNode TryGetGraphRoot()
        {
            if (this.mGraph.NodeCount == 0)
            {
                return null;
            }
            int index = this.HasRoot 
                ? this.mGraph.IndexOfNode(this.TryGetRoot()) : 0;
            if (index < 0)
                index = 0;
            Digraph<Node, Edge>.GNode root 
                = this.mGraph.InternalNodeAt(index);
            //root.Index = index;
            return root;
        }/* */

        /// <summary>
        /// Shuffles all layout nodes (but not port nodes) in this layout
        /// algorithm's <see cref="Graph"/> to random positions with
        /// minimal intersections of their bounding boxes.
        /// </summary>
        /// <param name="immediate">Whether the positions of the layout nodes
        /// are set after all their new positions have been randomly set.
        /// </param><remarks>
        /// The random positions of the nodes are restricted to within the
        /// bounding box of this algorithm's <see cref="ClusterNode"/>, or
        /// within this algorithm's <see cref="BoundingBox"/> if the cluster
        /// node is null.
        /// </remarks>
        public void ShuffleNodes(bool immediate)
        {
            Box2F bbox = this.mClusterNode == null
                ? this.mBBox : this.mClusterNode.LayoutBBox;
            this.ShuffleNodesInternal(bbox.X, bbox.Y,
                bbox.W, bbox.H, immediate);
        }

        /// <summary>
        /// Shuffles all layout nodes (but not port nodes) in this layout
        /// algorithm's <see cref="Graph"/> to random positions with
        /// minimal intersections of their bounding boxes.
        /// </summary>
        /// <param name="bbox">A rectangle used to restrict the randomly set
        /// positions of the layout nodes to within its boundaries.</param>
        /// <param name="immediate">Whether the positions of the layout nodes
        /// are set after all their new positions have been randomly set.
        /// </param><remarks>
        /// The random positions of the nodes are restricted to within the
        /// bounding box of this algorithm's <see cref="ClusterNode"/>, or
        /// within this algorithm's <see cref="BoundingBox"/> if the cluster
        /// node is null.
        /// </remarks>
        public void ShuffleNodes(Box2F bbox, bool immediate)
        {
            if (this.mClusterNode == null)
            {
                bbox = Box2F.Intersect(bbox, this.mBBox);
                if (bbox.W == 0 || bbox.H == 0)
                {
                    bbox = this.mBBox;
                }
            }
            else
            {
                bbox = Box2F.Intersect(bbox, 
                    this.mClusterNode.LayoutBBox);
                if (bbox.W == 0 || bbox.H == 0)
                {
                    bbox = this.mClusterNode.LayoutBBox;
                }
            }
            this.ShuffleNodesInternal(bbox.X, bbox.Y, 
                bbox.W, bbox.H, immediate);
        }

        private void ShuffleNodesInternal(float bbx, float bby, 
            float bbw, float bbh, bool immediate)
        {
            int i, j, iter;
            bool intersection;
            Node node;
            float x, y, bbxi, bbyi;
            Vec2F pos;
            Box2F bboxI, bboxJ;
            Random rnd = new Random(DateTime.Now.Millisecond);
            int count = this.mGraph.NodeCount;
            for (i = 0; i < count; i++)
            {
                node = this.mGraph.NodeAt(i);
                if (!node.PositionFixed)
                {
                    x = y = 0;
                    bboxI = new Box2F(node.LayoutBBox);
                    bbxi = bboxI.X; 
                    bbyi = bboxI.Y;
                    // Set the padding of the bounding box to bboxI
                    bbx -= bbxi;
                    bby -= bbyi;
                    bbw -= bboxI.W;
                    bbh -= bboxI.H;
                    // Iterate until a random position is found that doesn't
                    // intersect with any of the other already placed nodes.
                    intersection = true;
                    for (iter = 0; iter < 50 && intersection; iter++)
                    {
                        intersection = false;
                        x = (float)(rnd.NextDouble() * bbw + bbx);
                        y = (float)(rnd.NextDouble() * bbh + bby);
                        if (this.mClusterNode == null)
                        {
                            bboxI.X = bbxi + x;
                            bboxI.Y = bbyi + y;
                        }
                        else
                        {
                            pos = this.mClusterNode.AugmentNodePos(x, y);
                            bboxI.X = bbxi + pos.X;
                            bboxI.Y = bbyi + pos.Y;
                        }
                        for (j = 0; j < i && !intersection; j++)
                        {
                            node = this.mGraph.NodeAt(j);
                            //if (!(node is IPortNode))
                            {
                                bboxJ = new Box2F(node.LayoutBBox);
                                bboxJ.X += node.X;//node.NewX;
                                bboxJ.Y += node.Y;//node.NewY;
                                intersection = bboxI.IntersectsWith(bboxJ);
                                //intersection =
                                //    bboxJ.X < (bboxI.X + bboxI.Width) &&
                                //    bboxI.X < (bboxJ.X + bboxJ.Width) &&
                                //    bboxJ.Y < (bboxI.Y + bboxI.Height) &&
                                //    bboxI.Y < (bboxJ.Y + bboxJ.Height);
                            }
                        }
                    }
                    // Clear the padding of the bounding box
                    bbx += bbxi;
                    bby += bbyi;
                    bbw += bboxI.W;
                    bbh += bboxI.H;
                    // Set the node's new randomized position
                    node = this.mGraph.NodeAt(i);
                    node.SetPosition(x, y);//SetNewPosition(x, y);
                }
            }
            for (i = this.mGraph.EdgeCount - 1; i >= 0; i--)
            {
                this.mGraph.EdgeAt(i).Update();
            }
            /*if (immediate)
            {
                for (i = 0; i < nodes.Length; i++)
                {
                    node = nodes[i].Data;
                    if (!node.PositionFixed)
                        node.SetPosition(node.NewX, node.NewY);
                }
                Digraph<Node, Edge>.GEdge[] edges
                    = this.mGraph.InternalEdges;
                for (i = 0; i < edges.Length; i++)
                {
                    edges[i].Data.Update();
                }
            }/* */
        }

        /// <summary>
        /// Gets the position on the border of the given bounding box based
        /// on its intersection with a ray starting at its center point with
        /// the given rotational <paramref name="angle"/>.
        /// </summary>
        /// <param name="bbox">The bounding box to intersect with 
        /// a positioning ray.</param>
        /// <param name="angle">The angle of a positioning ray starting at
        /// the center of <paramref name="bbox"/>.</param>
        /// <returns>The intersection point of the given bounding box and
        /// a ray with the given <paramref name="angle"/> starting at the
        /// bounding box's center point.</returns>
        public static Vec2F GetBoundaryPosition(Box2F bbox, double angle)
        {
            double cx = bbox.X + bbox.W / 2.0;
            double cy = bbox.Y + bbox.H / 2.0;
            double dx = Math.Cos(angle);
            double dy = Math.Sin(angle);
            double t = Math.Min(Math.Abs(cx / dx), Math.Abs(cy / dy));
            return new Vec2F((float)(dx * t + cx), (float)(dy * t + cy));
        }

        /// <summary>
        /// Sets the positions of all the port nodes in this layout 
        /// algorithm's <see cref="Graph"/> to positions on the border of
        /// the shape of <see cref="ClusterNode"/> or the border of
        /// <see cref="BoundingBox"/> if the cluster node is null, based on
        /// their angle around the center point of the bounding box of 
        /// whichever is used.</summary>
        /// <param name="immediate">Whether the positions of the port nodes
        /// are set after all their new positions have been set based on
        /// their angles.</param>
        protected void SetPortNodePositions(bool immediate)
        {
            IPortNode pNode;
            Digraph<Node, Edge>.GNode node;
            int i, count = this.mGraph.NodeCount;
            if (this.mClusterNode == null)
            {
                double cx = this.mBBox.X + this.mBBox.W / 2.0;
                double cy = this.mBBox.Y + this.mBBox.H / 2.0;
                // x = dx * t + cx = 2 * cx; y = dy * t + cy = 2 * cy;
                // t = cx / dx; t = cy / dy;
                double t, dx, dy;
                for (i = 0; i < count; i++)
                {
                    node = this.mGraph.InternalNodeAt(i);
                    if (node.Data is IPortNode)
                    {
                        pNode = node.Data as IPortNode;
                        t = (pNode.MinAngle + pNode.MaxAngle) / 2;
                        dx = Math.Cos(t);
                        dy = Math.Sin(t);
                        t = Math.Min(Math.Abs(cx / dx), Math.Abs(cy / dy));
                        dx = dx * t + cx;
                        dy = dy * t + cy;
                        pNode.SetPosition((float)dx, (float)dy);//SetNewPosition((float)dx, (float)dy);
                    }
                }
            }
            else
            {
                Vec2F pos;
                for (i = 0; i < count; i++)
                {
                    node = this.mGraph.InternalNodeAt(i);
                    if (node.Data is IPortNode)
                    {
                        pNode = node.Data as IPortNode;
                        pos = this.mClusterNode.GetPortNodePos(
                            (pNode.MinAngle + pNode.MaxAngle) / 2);
                        pNode.SetPosition(pos.X, pos.Y);//SetNewPosition(pos.X, pos.Y);
                    }
                }
            }
            /*if (immediate)
            {
                for (i = 0; i < nodes.Length; i++)
                {
                    if (nodes[i].Data is IPortNode)
                    {
                        pNode = nodes[i].Data as IPortNode;
                        pNode.SetPosition(pNode.NewX, pNode.NewY);
                    }
                }
            }/* */
        }

        /// <summary>
        /// Reimplement this function to initialize any data needed before
        /// the algorithm begins its layout computation. This function is
        /// called after <see cref="M:AAlgorithm.OnStarted()"/> and whenever 
        /// the algorithm is reset using <see cref="ResetAlgorithm()"/>.
        /// </summary>
        protected virtual void InitializeAlgorithm()
        {
        }

        /// <summary>
        /// Whether the algorithm should perform the next iteration in its
        /// layout computation, or stop and finish.
        /// </summary>
        /// <returns>true if the algorithm should run its next iteration,
        /// or false if the algorithm should stop and finish.</returns>
        protected virtual bool CanIterate()
        {
            return true;
        }

        /*/// <summary>
        /// Performs all the necessary actions needed to start a single
        /// iteration calculation of this layout algorithm and then calls
        /// <see cref="OnBeginIteration(uint,bool,int,int)"/> to run any
        /// custom code.</summary>
        /// <param name="iteration">The current number of iterations that 
        /// have already occurred in this algorithm's computation.</param>
        /// <exception cref="InvalidOperationException">This layout algorithm
        /// is already in an iteration that hasn't been ended yet with the
        /// <see cref="EndIteration(uint)"/> function.</exception>
        protected void BeginIteration(uint iteration)
        {
            if (this.bInIteration)
            {
                throw new InvalidOperationException(
                    "Iteration already started.");
            }
            this.bInIteration = true;
            Digraph<Node, Edge>.GNode[] nodes
                = this.mGraph.InternalNodes;
            // Expand the last position logs if necessary
            if (nodes.Length > this.mLastXs.Length)
            {
                this.mLastXs = new float[nodes.Length];
                this.mLastYs = new float[nodes.Length];
            }
            // Log the previous positions of all the nodes
            for (int i = 0; i < nodes.Length; i++)
            {
                //nodes[i].Index = i;
                this.mLastXs[i] = nodes[i].Data.X;
                this.mLastYs[i] = nodes[i].Data.Y;
            }
            // Run any code unique to the specific layout algorithm.
            this.OnBeginIteration(iteration, this.bDirty,
                this.mLastNodeCount, this.mLastEdgeCount);
            this.bDirty = false;
            this.mLastNodeCount = nodes.Length;
            this.mLastEdgeCount = this.mGraph.EdgeCount;
        }

        /// <summary>
        /// Reimplement this function to initialize and refresh any data
        /// needed for each iteration calculation at the beginning of each
        /// iteration. Use the arguments to determine what data needs to
        /// be refreshed.</summary>
        /// <param name="iteration">The current number of iterations that 
        /// have already occurred in this algorithm's computation.</param>
        /// <param name="dirty">Whether this algorithm was marked dirty
        /// with <see cref="MarkDirty()"/> before this iteration began.
        /// </param>
        /// <param name="lastNodeCount">The number of nodes in this
        /// algorithm's <see cref="Graph"/> at the end of the last iteration.
        /// </param>
        /// <param name="lastEdgeCount">The number of edges in this
        /// algorithm's <see cref="Graph"/> at the end of the last iteration.
        /// </param>
        protected virtual void OnBeginIteration(uint iteration, 
            bool dirty, int lastNodeCount, int lastEdgeCount)
        {
        }/* */

        private void SetLastPositions()
        {
            if (this.mLastXs.Length < this.mGraph.NodeCount)
            {
                this.mLastXs = new float[this.mGraph.NodeCount];
                this.mLastYs = new float[this.mGraph.NodeCount];
            }
            Node node;
            for (int i = this.mGraph.NodeCount - 1; i >= 0; i--)
            {
                node = this.mGraph.NodeAt(i);
                this.mLastXs[i] = node.X;
                this.mLastYs[i] = node.Y;
            }
        }

        protected void PerformPrecalculations()
        {
            this.PerformPrecalculations(this.mLastNVers, this.mLastEVers);
            this.mLastNVers = this.mGraph.NodeVersion;
            this.mLastEVers = this.mGraph.EdgeVersion;
        }

        protected virtual void PerformPrecalculations(
            uint lastNodeVersion, uint lastEdgeVersion)
        {
        }

        /// <summary>
        /// Reimplement this function to perform any core calculations
        /// between the beginning and end of each iteration. This is where
        /// force-directed layout algorithms should set the new positions
        /// of each layout node as they approach equilibrium.
        /// </summary>
        /// <param name="iteration">The current number of iterations that
        /// have already occurred in this algorithm's computation.</param>
        protected abstract void PerformIteration(uint iteration);

        /// <summary>
        /// Performs all the necessary actions needed to finish a single
        /// iteration calculation of this layout algorithm and then calls
        /// <see cref="OnEndIteration(uint,double)"/> to run any
        /// custom code.</summary>
        /// <param name="iteration">The current number of iterations that 
        /// have already occurred in this algorithm's computation.</param>
        /// <exception cref="InvalidOperationException">This layout algorithm
        /// hasn't started started an iteration yet with the
        /// <see cref="BeginIteration(uint)"/> function.</exception>
        private void EndIteration(uint iteration)
        {
            /*if (!this.bInIteration)
            {
                throw new InvalidOperationException(
                    "Iteration already finished.");
            }/* */
            int i, count = this.mGraph.NodeCount;
            float dx, dy;
            Node node;
            // Step 1: Let the cluster node react to and change
            // the new positions of any or all of the layout nodes,
            // such as restricting them to within its bounds/shape or 
            // expanding its bounds/shape to fit their new positions.
            if (this.mClusterNode == null)
            {
                float d;
                Box2F nbox;
                // Calculate the bounding box of all the nodes
                float minX = float.MaxValue;
                float minY = float.MaxValue;
                float maxX = -float.MaxValue;
                float maxY = -float.MaxValue;
                /*for (i = 0; i < count; i++)
                {
                    node = this.mGraph.NodeAt(i);
                    nbox = node.LayoutBBox;
                    dx = node.X + nbox.X;
                    dy = node.Y + nbox.Y;
                    if (dx < minX)
                        minX = dx;
                    if (dy < minY)
                        minY = dy;
                    dx += nbox.W;
                    dy += nbox.H;
                    if (dx > maxX)
                        maxX = dx;
                    if (dy > maxY)
                        maxY = dy;
                }/* */
                minX = this.mBBox.X;
                minY = this.mBBox.Y;
                maxX = minX + this.mBBox.W;
                maxY = minY + this.mBBox.H;
                for (i = 0; i < count; i++)
                {
                    node = this.mGraph.NodeAt(i);
                    nbox = node.LayoutBBox;
                    //dx = Math.Min(Math.Max(node.NewX, minX), maxX);
                    //dy = Math.Min(Math.Max(node.NewY, minY), maxY);
                    //node.SetNewPosition(dx, dy);
                    d = nbox.W;
                    dx = Math.Min(Math.Max(node.X + nbox.X, minX) + d, maxX) - nbox.X - d;
                    d = nbox.H;
                    dy = Math.Min(Math.Max(node.Y + nbox.Y, minY) + d, maxY) - nbox.Y - d;
                    node.SetPosition(dx, dy);
                }
            }
            else
            {
                for (i = 0; i < count; i++)
                {
                    node = this.mGraph.NodeAt(i);
                    this.mClusterNode.LearnNodePos(
                        //node.NewX, node.NewY, node.LayoutBBox);
                        node.X, node.Y, node.LayoutBBox);
                }
                Vec2F pos;
                for (i = 0; i < count; i++)
                {
                    node = this.mGraph.NodeAt(i);
                    pos = this.mClusterNode.AugmentNodePos(
                        node.X, node.Y);//node.NewX, node.NewY);
                    node.SetPosition(pos.X, pos.Y);//SetNewPosition(pos.X, pos.Y);
                }
            }
            // Step 2: If there is an ILayoutSpring instance, use it to
            // perform a "twaining" animation; otherwise, directly set 
            // the position of each layout node to their new position.
            /*if (this.mSpring != null)
            {
                float x, y;
                Vec2F force;
                for (i = 0; i < count; i++)
                {
                    node = this.mGraph.NodeAt(i);
                    if (!node.PositionFixed)
                    {
                        x = node.X;
                        y = node.Y;
                        dx = node.NewX - x;
                        dy = node.NewY - y;
                        force = this.mSpring.GetSpringForce(dx, dy);
                        node.SetPosition(x + force.X, y + force.Y);
                    }
                }
            }
            else
            {
                for (i = 0; i < count; i++)
                {
                    node = this.mGraph.NodeAt(i);
                    if (!node.PositionFixed)
                    {
                        node.SetPosition(node.NewX, node.NewY);
                    }
                }
            }/* */
            // Step 3: Check each layout node for a change in position
            // since the last iteration and tally the total change in
            // the positions of all the layout nodes to determine whether
            // the layout algorithm should continue iterating.
            double dist, totalDist = 0;
            Digraph<Node, Edge>.GNode gNode;
            this.bItemMoved = false;
            for (i = 0; i < count; i++)
            {
                gNode = this.mGraph.InternalNodeAt(i);
                dx = this.mLastXs[i] - gNode.Data.X;
                dy = this.mLastYs[i] - gNode.Data.Y;
                dist = dx * dx + dy * dy;
                if (dist < this.mMovementTolerance)
                {
                    gNode.Color = GraphColor.White;
                }
                else
                {
                    this.bItemMoved = true;
                    gNode.Color = GraphColor.Gray;
                }
                totalDist += dist;
            }
            // Step 4: Update edges connected to nodes which have moved.
            if (this.bItemMoved)
            {
                Digraph<Node, Edge>.GEdge edge;
                count = this.mGraph.EdgeCount;
                for (i = 0; i < count; i++)
                {
                    edge = this.mGraph.InternalEdgeAt(i);
                    if (edge.SrcNode.Color == GraphColor.Gray ||
                        edge.DstNode.Color == GraphColor.Gray)
                    {
                        edge.Data.Update();
                    }
                }
            }
            // Step 5: Run any code unique to the specific layout algorithm.
            this.OnEndIteration(iteration, totalDist);
            // Step 6: End the iteration
            //this.bInIteration = false;
        }

        /// <summary>
        /// Reimplement this function to perform actions and trigger events
        /// at the very end of each iteration, after positions of all the
        /// layout nodes have been set.
        /// </summary>
        /// <param name="iteration">The current number of iterations that
        /// have already occurred in this algorithm's computation.</param>
        /// <param name="totalDistanceChange">The sum of the distances
        /// travelled by each layout node from their positions on the last
        /// iteration to their current positions.</param>
        protected virtual void OnEndIteration(uint iteration, 
            double totalDistanceChange)
        {
        }

        /// <summary>
        /// Initializes and synchronously runs the layout computation,
        /// iterating until <see cref="MaxIterations"/> is reached,
        /// the algorithm is aborted, no layout node has moved beyond
        /// <see cref="MovementTolerance"/>, or <see cref="CanIterate()"/>
        /// returns false.
        /// </summary>
        protected override void InternalCompute()
        {
            this.InitializeAlgorithm();
            this.bResetting = false;
            this.bItemMoved = true;
            for (uint i = this.mIter; i < this.mMaxIterations &&
                this.State != ComputeState.Aborting &&
                this.bItemMoved && this.CanIterate(); i++)
            {
                if (this.bResetting)
                {
                    this.InitializeAlgorithm();
                    i = 0;
                    this.bResetting = false;
                }
                this.SetLastPositions();
                this.PerformPrecalculations();
                if (this.mGraph.NodeCount > 0)
                {
                    this.PerformIteration(i);
                }
                this.EndIteration(i);
            }
        }

        /// <summary>
        /// Initializes the layout (if it's just started or has been reset) 
        /// and asynchronously runs one iteration, and returns whether it
        /// was able to run the current iteration. Returns false if 
        /// <see cref="MaxIterations"/> has been reached, the algorithm has
        /// been aborted, no layout node has moved beyond 
        /// <see cref="MovementTolerance"/>, or <see cref="CanIterate()"/>
        /// returns false.</summary>
        /// <param name="forceRestart">Whether to force this algorithm to
        /// start running again if it has finished or has been aborted.</param>
        /// <returns>true if the iteration is successfully run, or false if
        /// the algorithm has finished or has been aborted.</returns>
        public bool AsyncIterate(bool forceRestart)
        {
            if (this.State != ComputeState.None)
            {
                // Can't run asynchronously and synchronously 
                // simultaneously
                throw new InvalidOperationException();
            }
            switch (this.mAsyncState)
            {
                case ComputeState.None:
                    this.mAsyncState = ComputeState.Running;
                    this.OnStarted();
                    this.InitializeAlgorithm();
                    this.mIter = 0;
                    this.bResetting = false;
                    this.bItemMoved = true;
                    break;
                case ComputeState.Finished:
                case ComputeState.Aborted:
                    if (forceRestart)
                    {
                        this.mAsyncState = ComputeState.Running;
                        this.OnStarted();
                        this.InitializeAlgorithm();
                        this.mIter = 0;
                        this.bResetting = false;
                        this.bItemMoved = true;
                    }
                    break;
            }
            if (this.mIter < this.mMaxIterations &&
                this.mAsyncState == ComputeState.Running &&
                this.bItemMoved && this.CanIterate())
            {
                if (this.bResetting)
                {
                    this.InitializeAlgorithm();
                    this.mIter = 0;
                    this.bResetting = false;
                }
                this.SetLastPositions();
                this.PerformPrecalculations();
                if (this.mGraph.NodeCount > 0)
                {
                    this.PerformIteration(this.mIter);
                }
                this.EndIteration(this.mIter);
                this.mIter++;
                return true;
            }
            else
            {
                switch (this.mAsyncState)
                {
                    case ComputeState.Running:
                        this.mAsyncState = ComputeState.Finished;
                        this.OnFinished();
                        break;
                    case ComputeState.Aborting:
                        this.mAsyncState = ComputeState.Aborted;
                        this.OnAborted();
                        break;
                }
                return false;
            }
        }

        /// <summary>
        /// Stop running the algorithm and abort its computation,
        /// both synchronously and asychronously.
        /// </summary>
        public override void Abort()
        {
            if (this.mAsyncState == ComputeState.Running)
            {
                this.mAsyncState = ComputeState.Aborting;
            }
            base.Abort();
        }
    }
}
