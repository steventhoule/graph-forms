using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

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
        : ARootedAlgorithm<Node>
        where Node : ILayoutNode
        where Edge : IGraphEdge<Node>, IUpdateable
    {
        /// <summary>
        /// The graph which this layout algorithm operates on.
        /// </summary>
        protected readonly Digraph<Node, Edge> mGraph;
        /// <summary>
        /// If <see cref="mGraph"/> is actually a sub-graph, then
        /// this is the node in the graph superstructure that encloses it.
        /// </summary>
        protected readonly IClusterNode mClusterNode;
        /// <summary>
        /// This bounding box is used as a substitute for 
        /// <see cref="mClusterNode"/> if it is null.
        /// </summary>
        private RectangleF mBBox;

        /// <summary>
        /// Flags whether the fields of this algorithm have changed 
        /// and need to be refreshed on the next iteration.
        /// </summary>
        private bool bDirty = true;
        /// <summary>
        /// The number of nodes in the graph during the last iteration.
        /// </summary>
        private int mLastNodeCount = 0;
        /// <summary>
        /// The number of edges in the graph during the last iteration.
        /// </summary>
        private int mLastEdgeCount = 0;

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
        {
            if (graph == null)
                throw new ArgumentNullException("graph");
            if (clusterNode == null)
                throw new ArgumentNullException("clusterNode");
            this.mGraph = graph;
            this.mClusterNode = clusterNode;
            this.mBBox = RectangleF.Empty;
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
            RectangleF boundingBox)
        {
            if (graph == null)
                throw new ArgumentNullException("graph");
            this.mGraph = graph;
            this.mClusterNode = null;
            this.mBBox = boundingBox;
        }

        /// <summary>
        /// The graph that this layout algorithm operates on,
        /// computing and setting the positions of its 
        /// <typeparamref name="Node"/> instances.
        /// </summary>
        public Digraph<Node, Edge> Graph
        {
            get { return this.mGraph; }
        }

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
        public RectangleF BoundingBox
        {
            get 
            { 
                return new RectangleF(
                    this.mBBox.X, this.mBBox.Y, 
                    this.mBBox.Width, this.mBBox.Height); 
            }
            set
            {
                if (this.State != ComputeState.Running &&
                    this.mAsyncState != ComputeState.Running)
                {
                    this.mBBox = new RectangleF(
                        value.X, value.Y, value.Width, value.Height);
                }
            }
        }

        /// <summary>
        /// Notifies this algorithm that it needs to be refreshed at the 
        /// beginning of the next iteration of its layout computation.
        /// </summary>
        protected void MarkDirty()
        {
            this.bDirty = true;
        }

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
        /// The current state of the asynchronous computation 
        /// of this layout algorithm.
        /// </summary>
        public ComputeState AsyncState
        {
            get { return this.mAsyncState; }
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
        public double MovementTolerance
        {
            get { return this.mMovementTolerance; }
            set
            {
                if (this.State != ComputeState.Running &&
                    this.mAsyncState != ComputeState.Running &&
                    value >= 0)
                    this.mMovementTolerance = value;
            }
        }

        /// <summary>
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
            root.Index = index;
            return root;
        }

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
            RectangleF bbox = this.mClusterNode == null
                ? this.mBBox : this.mClusterNode.BoundingBox;
            this.ShuffleNodesInternal(bbox.X, bbox.Y,
                bbox.Width, bbox.Height, immediate);
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
        public void ShuffleNodes(RectangleF bbox, bool immediate)
        {
            if (this.mClusterNode == null)
            {
                bbox = RectangleF.Intersect(bbox, 
                    this.mBBox);
            }
            else
            {
                bbox = RectangleF.Intersect(bbox, 
                    this.mClusterNode.BoundingBox);
            }
            this.ShuffleNodesInternal(bbox.X, bbox.Y, 
                bbox.Width, bbox.Height, immediate);
        }

        private void ShuffleNodesInternal(float bbx, float bby, 
            float bbw, float bbh, bool immediate)
        {
            int i, j, iter;
            bool intersection;
            Node node;
            float x, y, bbxi, bbyi;
            PointF pos;
            RectangleF bboxI, bboxJ;
            Random rnd = new Random(DateTime.Now.Millisecond);
            Digraph<Node, Edge>.GNode[] nodes = this.mGraph.InternalNodes;
            for (i = 0; i < nodes.Length; i++)
            {
                if (!(nodes[i].mData is IPortNode))
                {
                    x = y = 0;
                    bboxI = nodes[i].mData.BoundingBox;
                    bbxi = bboxI.X;
                    bbyi = bboxI.Y;
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
                            node = nodes[j].mData;
                            if (!(node is IPortNode))
                            {
                                bboxJ = node.BoundingBox;
                                bboxJ.Offset(node.NewX, node.NewY);
                                intersection = bboxI.IntersectsWith(bboxJ);
                                //intersection =
                                //    bboxJ.X < (bboxI.X + bboxI.Width) &&
                                //    bboxI.X < (bboxJ.X + bboxJ.Width) &&
                                //    bboxJ.Y < (bboxI.Y + bboxI.Height) &&
                                //    bboxI.Y < (bboxJ.Y + bboxJ.Height);
                            }
                        }
                    }
                    nodes[i].mData.SetNewPosition(x, y);
                }
            }
            if (immediate)
            {
                for (i = 0; i < nodes.Length; i++)
                {
                    node = nodes[i].mData;
                    if (!(node is IPortNode))
                        node.SetPosition(node.NewX, node.NewY);
                }
                Digraph<Node, Edge>.GEdge[] edges
                    = this.mGraph.InternalEdges;
                for (i = 0; i < edges.Length; i++)
                {
                    edges[i].mData.Update();
                }
            }
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
            Digraph<Node, Edge>.GNode[] nodes
                = this.mGraph.InternalNodes;
            int i;
            IPortNode pNode;
            if (this.mClusterNode == null)
            {
                double cx = (this.mBBox.X + this.mBBox.Width) / 2.0;
                double cy = (this.mBBox.Y + this.mBBox.Height) / 2.0;
                // x = dx * t + cx = 2 * cx; y = dy * t + cy = 2 * cy;
                // t = cx / dx; t = cy / dy;
                double t, dx, dy;
                for (i = 0; i < nodes.Length; i++)
                {
                    if (nodes[i].mData is IPortNode)
                    {
                        pNode = nodes[i].mData as IPortNode;
                        t = (pNode.MinAngle + pNode.MaxAngle) / 2;
                        dx = Math.Cos(t);
                        dy = Math.Sin(t);
                        t = Math.Min(Math.Abs(cx / dx), Math.Abs(cy / dy));
                        dx = dx * t + cx;
                        dy = dy * t + cy;
                        pNode.SetNewPosition((float)dx, (float)dy);
                        pNode.SetPosition((float)dx, (float)dy);
                    }
                }
            }
            else
            {
                PointF pos;
                for (i = 0; i < nodes.Length; i++)
                {
                    if (nodes[i].mData is IPortNode)
                    {
                        pNode = nodes[i].mData as IPortNode;
                        pos = this.mClusterNode.GetPortNodePos(
                            (pNode.MinAngle + pNode.MaxAngle) / 2);
                        pNode.SetNewPosition(pos.X, pos.Y);
                        pNode.SetPosition(pos.X, pos.Y);
                    }
                }
            }
            if (immediate)
            {
                for (i = 0; i < nodes.Length; i++)
                {
                    if (nodes[i].mData is IPortNode)
                    {
                        pNode = nodes[i].mData as IPortNode;
                        pNode.SetPosition(pNode.NewX, pNode.NewY);
                    }
                }
            }
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

        private void BeginIteration(uint iteration)
        {
            Digraph<Node, Edge>.GNode[] nodes
                = this.mGraph.InternalNodes;
            // Expand the last position logs if necessary
            if (nodes.Length > this.mLastNodeCount)
            {
                this.mLastXs = new float[nodes.Length];
                this.mLastYs = new float[nodes.Length];
            }
            // Log the previous positions of all the nodes
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i].Index = i;
                this.mLastXs[i] = nodes[i].mData.X;
                this.mLastYs[i] = nodes[i].mData.Y;
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
        /// algorithm's <see cref="Graph"/> at the end of last iteration.
        /// </param>
        /// <param name="lastEdgeCount">The number of edges in this
        /// algorithm's <see cref="Graph"/> at the </param>
        protected virtual void OnBeginIteration(uint iteration, 
            bool dirty, int lastNodeCount, int lastEdgeCount)
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
        protected virtual void PerformIteration(uint iteration)
        {
        }

        private void EndIteration(uint iteration)
        {
            int i;
            float dx, dy;
            Node node;
            Digraph<Node, Edge>.GNode[] nodes
                = this.mGraph.InternalNodes;
            // Step 1: Let the cluster node react to and change
            // the new positions of any or all of the layout nodes,
            // such as restricting them to within its bounds/shape or 
            // expanding its bounds/shape to fit their new positions.
            if (this.mClusterNode == null)
            {
                float minX = this.mBBox.X;
                float minY = this.mBBox.Y;
                float maxX = minX + this.mBBox.Width;
                float maxY = minY + this.mBBox.Height;
                //PointF np;
                for (i = 0; i < nodes.Length; i++)
                {
                    node = nodes[i].mData;
                    dx = Math.Min(Math.Max(node.NewX, minX), maxX);
                    dy = Math.Min(Math.Max(node.NewY, minY), maxY);
                    //dx = node.NewX;
                    //dy = node.NewY;
                    //np = (node as GraphElement).MapToScene(new PointF(dx - node.X, dy - node.Y));
                    //dx = dx + Math.Min(Math.Max(np.X, minX), maxX) - np.X;
                    //dy = dy + Math.Min(Math.Max(np.Y, minY), maxY) - np.Y;
                    node.SetNewPosition(dx, dy);
                }
            }
            else
            {
                PointF pos;
                for (i = 0; i < nodes.Length; i++)
                {
                    node = nodes[i].mData;
                    pos = this.mClusterNode.AugmentNodePos(
                        node.NewX, node.NewY);
                    node.SetNewPosition(pos.X, pos.Y);
                }
            }
            // Step 2: If there is an ILayoutSpring instance, use it to
            // perform a "twaining" animation; otherwise, directly set 
            // the position of each layout node to their new position.
            if (this.mSpring != null)
            {
                float x, y;
                PointF force;
                for (i = 0; i < nodes.Length; i++)
                {
                    node = nodes[i].mData;
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
                for (i = 0; i < nodes.Length; i++)
                {
                    node = nodes[i].mData;
                    if (!node.PositionFixed)
                    {
                        node.SetPosition(node.NewX, node.NewY);
                    }
                }
            }
            // Step 3: Check each layout node for a change in position
            // since the last iteration and tally the total change in
            // the positions of all the layout nodes to determine whether
            // the layout algorithm should continue iterating.
            double dist, totalDist = 0;
            this.bItemMoved = false;
            for (i = 0; i < nodes.Length; i++)
            {
                dx = this.mLastXs[i] - nodes[i].mData.X;
                dy = this.mLastYs[i] - nodes[i].mData.Y;
                dist = dx * dx + dy * dy;
                if (dist < this.mMovementTolerance)
                {
                    nodes[i].Color = GraphColor.White;
                }
                else
                {
                    this.bItemMoved = true;
                    nodes[i].Color = GraphColor.Gray;
                }
                totalDist += dist;
            }
            // Step 4: Update edges connected to nodes which have moved.
            if (this.bItemMoved)
            {
                Digraph<Node, Edge>.GEdge[] edges
                    = this.mGraph.InternalEdges;
                for (i = 0; i < edges.Length; i++)
                {
                    if (edges[i].mSrcNode.Color == GraphColor.Gray ||
                        edges[i].mDstNode.Color == GraphColor.Gray)
                    {
                        edges[i].mData.Update();
                    }
                }
            }
            // Step 5: Run any code unique to the specific layout algorithm.
            this.OnEndIteration(iteration, totalDist);
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
                this.BeginIteration(i);
                if (this.mLastNodeCount > 0 && this.mLastEdgeCount > 0)
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
                this.BeginIteration(this.mIter);
                if (this.mLastNodeCount > 0 && this.mLastEdgeCount > 0)
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
