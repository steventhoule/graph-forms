using System;
using System.Drawing;

namespace GraphForms.Algorithms.Layout
{
    /// <summary>
    /// The base class for algorithms which calculate the layout of a given
    /// <see cref="T:Digraph`2{Node,Edge}"/> instance by setting the
    /// positions of its <typeparamref name="Node"/> instances based on the
    /// <typeparamref name="Edge"/> instances connecting them and the 
    /// parameter values within the <typeparamref name="Params"/> instance.
    /// </summary>
    /// <typeparam name="Node">The class of <see cref="GraphElement"/> nodes
    /// which are rearranged by this layout algorithm.</typeparam>
    /// <typeparam name="Edge">The class of <see cref="T:IGraphEdge`1{Node}"/>
    /// edges that connect the <typeparamref name="Node"/> instances that this
    /// algorithm rearranges.</typeparam>
    /// <typeparam name="Params">The <see cref="LayoutParameters"/> class with
    /// properties that influence how this layout algorithm arranges its 
    /// graph's <typeparamref name="Node"/> instances.</typeparam>
    public abstract class LayoutAlgorithm<Node, Edge, Params>
        : AAlgorithm, ILayoutAlgorithm, IPropertyChangedListener
        where Node : GraphElement, ILayoutNode
        where Edge : class, IGraphEdge<Node>, IUpdateable
        where Params : LayoutParameters
    {
        /// <summary>
        /// The graph which this layout algorithm operates on.
        /// </summary>
        protected readonly Digraph<Node, Edge> mGraph;

        private Params mParameters;
        /// <summary>
        /// Whether the parameters have changed and need to be refreshed
        /// on the next iteration.
        /// </summary>
        private bool mParamsDirty = true;
        /// <summary>
        /// The number of nodes in the graph during the last iteration.
        /// </summary>
        private int mLastNodeCount = 0;
        /// <summary>
        /// The number of edges in the graph during the last iteration.
        /// </summary>
        private int mLastEdgeCount = 0;

        private float[] mNewXs;
        private float[] mNewYs;

        /// <summary>
        /// The new X-coordinates for each node in this layout algorithm's 
        /// graph, the same size and in the same order as the graph's
        /// <see cref="P:Digraph`2{Node,Edge}.Nodes"/> list.
        /// </summary>
        protected float[] NewXPositions
        {
            get { return this.mNewXs; }
        }

        /// <summary>
        /// The new Y-coordinates for each node in this layout algorithm's 
        /// graph, the same size and in the same order as the graph's
        /// <see cref="P:Digraph`2{Node,Edge}.Nodes"/> list.
        /// </summary>
        protected float[] NewYPositions
        {
            get { return this.mNewYs; }
        }

        /// <summary>
        /// Initializes a new <see cref="T:LayoutAlgorithm`3{Node,Edge,Params}"/>
        /// instance that arranges the <typeparamref name="Node"/> instances
        /// within the given <paramref name="graph"/>, influenced by its
        /// <see cref="DefaultParameters"/>.
        /// </summary>
        /// <param name="graph">The graph that will be rearranged by this
        /// layout algorithm.</param>
        public LayoutAlgorithm(Digraph<Node, Edge> graph)
            : this(graph, null)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="T:LayoutAlgorithm`3{Node,Edge,Params}"/>
        /// instance that arranges the <typeparamref name="Node"/> instances
        /// within the given <paramref name="graph"/>, influenced by parameter
        /// values within the given <paramref name="oldParameters"/>.
        /// </summary>
        /// <param name="graph">The graph that will be rearranged by this
        /// layout algorithm.</param>
        /// <param name="oldParameters">The parameters that will influence how
        /// this layout algorithm rearranges the <paramref name="graph"/>.
        /// If null, the <see cref="DefaultParameters"/> are used instead.
        /// </param>
        public LayoutAlgorithm(Digraph<Node, Edge> graph,
            Params oldParameters)
        {
            this.mGraph = graph;
            if (oldParameters == null)
                this.Parameters = this.DefaultParameters;
            else
                this.Parameters = oldParameters;
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
        /// Creates the default <typeparamref name="Params"/> instance for
        /// an instance of this layout algorithm when one isn't given in the
        /// constructor.
        /// </summary>
        protected virtual Params DefaultParameters
        {
            get { return Activator.CreateInstance<Params>(); }
        }

        /// <summary>
        /// The <see cref="LayoutParameters"/> instance used to influence the
        /// behavior of this layout algorithm and how it arranges the 
        /// <typeparamref name="Node"/> instances in its <see cref="Graph"/>.
        /// </summary>
        public Params Parameters
        {
            get { return this.mParameters; }
            set
            {
                if (this.mParameters != value)
                {
                    if (this.mParameters != null)
                        this.mParameters.Listener = null;
                    this.mParameters = value;
                    if (this.mParameters != null)
                        this.mParameters.Listener = this;
                    this.mParamsDirty = true;
                }
            }
        }

        /// <summary>
        /// The <see cref="LayoutParameters"/> instance used to influence the
        /// behavior of this layout algorithm and how it arranges the nodes
        /// instances in its graph.
        /// </summary>
        LayoutParameters ILayoutAlgorithm.Parameters
        {
            get { return this.mParameters as LayoutParameters; }
        }

        /// <summary>
        /// The implementation of the main function of the 
        /// <see cref="IPropertyChangedListener"/> interface,
        /// which is used to monitor for changes in values contained in this
        /// layout algorithm's <see cref="Parameters"/>.
        /// </summary>
        /// <param name="sender">The <see cref="PropertyChangedNotifier"/>
        /// instance that called this method, which will always be this
        /// layout algorithm's <see cref="Parameters"/>.</param>
        /// <param name="propertyName">The name of the property of the
        /// <paramref name="sender"/> with a value that changed.</param>
        public void OnPropertyChanged(PropertyChangedNotifier sender,
            string propertyName)
        {
            if (sender == this.mParameters)
            {
                this.mParamsDirty = true;
            }
        }

        /// <summary>
        /// Shuffles all the <typeparamref name="Node"/> instances in this
        /// algorithm's <see cref="Graph"/> by setting their positions
        /// to random points within the 
        /// <see cref="LayoutParameters.BoundingBox"/> of this algorithm's
        /// <see cref="Parameters"/>.</summary>
        public void ShuffleNodePositions()
        {
            this.ShuffleNodePositions(
                this.mParameters.X, this.mParameters.Y,
                this.mParameters.Width, this.mParameters.Height);
        }

        /// <summary>
        /// Shuffles all the <typeparamref name="Node"/> instances in this
        /// algorithm's <see cref="Graph"/> by setting their positions
        /// to random points within the given bounding box in scene-level 
        /// coordinates.</summary>
        /// <param name="x">The X-coordinate (in scene coordinates) of 
        /// the upper-left corner of the bounding box.</param>
        /// <param name="y">The Y-coordinate (in scene coordinates) of 
        /// the upper-left corner of the bounding box.</param>
        /// <param name="width">The width of the bounding box.</param>
        /// <param name="height">The height of the bounding box.</param>
        /// <remarks>
        /// Note that this may cause elements to "disappear" if they are 
        /// clipped inside a parent element that's smaller than the given
        /// scene-level bounding box.
        /// </remarks>
        public void ShuffleNodePositions(float x, float y,
            float width, float height)
        {
            GraphElement p;
            bool xNaN, yNaN;
            Random rnd = new Random(DateTime.Now.Millisecond);
            Digraph<Node, Edge>.GNode[] nodes = this.mGraph.InternalNodes;
            Node node;
            SizeF pos;
            int i;
            for (i = 0; i < nodes.Length; i++)
            {
                node = nodes[i].Data;
                if (!node.PositionFixed)
                {
                    // Fix invalid positions created by algorithm errors
                    p = node;
                    while (p != null)
                    {
                        xNaN = float.IsNaN(p.X) || float.IsInfinity(p.X);
                        yNaN = float.IsNaN(p.Y) || float.IsInfinity(p.Y);
                        p.SetPosition(xNaN ? 0 : p.X, yNaN ? 0 : p.Y);
                        p = p.Parent;
                    }
                    // New random position relative to scene-level bounding box
                    pos = node.SceneTranslate();
                    pos.Width = node.X + (float)(rnd.NextDouble() * width + x) - pos.Width;
                    pos.Height = node.Y + (float)(rnd.NextDouble() * height + y) - pos.Height;
                    node.SetPosition(pos.Width, pos.Height);
                }
            }
            Digraph<Node, Edge>.GEdge[] edges = this.mGraph.InternalEdges;
            for (i = 0; i < edges.Length; i++)
            {
                edges[i].Data.Update();
            }
        }

        /// <summary>
        /// Begins an iteration by re-initializing any localized data with
        /// a source which has changed since the last time an iteration began,
        /// including the <see cref="Parameters"/> and node and edge data.
        /// </summary>
        protected void BeginIteration()
        {
            int nodeCount = this.mGraph.NodeCount;
            if (nodeCount > this.mLastNodeCount)
            {
                this.mNewXs = new float[nodeCount];
                this.mNewYs = new float[nodeCount];
            }
            if (!this.OnBeginIteration(this.mParamsDirty,
                this.mLastNodeCount, this.mLastEdgeCount))
            {
                this.Abort();
            }
            this.mParamsDirty = false;
            this.mLastNodeCount = nodeCount;
            this.mLastEdgeCount = this.mGraph.EdgeCount;
        }

        /// <summary><para>
        /// Reimplement this function to trigger events and other actions
        /// that occur after all localized data has been re-initialized
        /// and right before the iteration actually begins.
        /// </para><para>
        /// If this function returns false, the entire algorithm is aborted
        /// as soon as possible.</para></summary>
        /// <param name="paramsDirty">Whether the values of one or more 
        /// properties on the <see cref="Parameters"/> were changed during
        /// the previous iteration.</param>
        /// <param name="lastNodeCount">The number of <typeparamref 
        /// name="Node"/> instances in the <see cref="Graph"/> at the
        /// beginning of the last iteration.</param>
        /// <param name="lastEdgeCount">The number of <typeparamref 
        /// name="Edge"/> instances in the <see cref="Graph"/> at the
        /// beginning of the last iteration.</param>
        /// <returns>true to begin the iteration, false to abort the entire
        /// algorithm as soon as possible.</returns>
        protected virtual bool OnBeginIteration(bool paramsDirty,
            int lastNodeCount, int lastEdgeCount)
        {
            return true;
        }

        /// <summary>
        /// Ends an iteration by setting the positions of every 
        /// <typeparamref name="Node"/> instance to the new positions
        /// calculated by this algorithm, then updating every
        /// <typeparamref name="Edge"/> instance connected to a node that has
        /// been moved by this algorithm, and then notifies the user of this
        /// algorithm's progress.</summary>
        /// <param name="iteration">The total number of iterations that have
        /// occurred since this algorithm started its computation.</param>
        /// <param name="statusInPercent">The status of this algorithm in
        /// percent, which is usually <paramref name="iteration"/> divided by
        /// the maximum number of iterations specified in the parameters.</param>
        /// <param name="message">The textual representation of the status of
        /// this algorithm, which usually describes its current computation 
        /// step or phase or iteration.</param>
        protected void EndIteration(int iteration, double statusInPercent,
            string message)
        {
            // Take the quick way out if possible
            if (this.mGraph.NodeCount == 0)
            {
                if (!this.OnIterationEnded(iteration, statusInPercent,
                    0, 0, message))
                {
                    this.Abort();
                }
            }
            Digraph<Node, Edge>.GNode[] nodes 
                = this.mGraph.InternalNodes;
            Digraph<Node, Edge>.GEdge[] edges 
                = this.mGraph.InternalEdges;
            float pbL = this.mParameters.X;
            float pbR = this.mParameters.X + this.mParameters.Width;
            float pbT = this.mParameters.Y;
            float pbB = this.mParameters.Y + this.mParameters.Height;
            float dx = this.mParameters.Width;
            float dy = this.mParameters.Height;
            double dist = 0f;
            double maxDist = nodes.Length * (double)(dx * dx + dy * dy);
            Node node;
            int i;
            // Check for moved nodes
            // In this code GraphNode.Visited is used to flag 
            // whether that node has moved
            bool nodeMoved = false;
            PointF np;
            for (i = 0; i < nodes.Length; i++)
            {
                node = nodes[i].mData;
                // Constrain to scene bounding box
                //dx = node.NewX;
                //dy = node.NewY;
                dx = this.mNewXs[i];
                dy = this.mNewYs[i];
                np = node.MapToScene(new PointF(dx - node.X, dy - node.Y));
                dx = dx + Math.Min(Math.Max(np.X, pbL), pbR) - np.X;
                dy = dy + Math.Min(Math.Max(np.Y, pbT), pbB) - np.Y;
                //node.NewX = dx;
                //node.NewY = dy;
                this.mNewXs[i] = dx;
                this.mNewYs[i] = dy;
                if (node.X != dx || node.Y != dy)
                {
                    nodeMoved = true;
                    dx = node.X - dx;
                    dy = node.Y - dy;
                    dist += dx * dx + dy * dy;
                    nodes[i].Color = GraphColor.Gray;
                }
                else
                {
                    nodes[i].Color = GraphColor.White;
                }
            }
            if (nodeMoved)
            {
                // Move nodes to their new positions
                for (i = 0; i < nodes.Length; i++)
                {
                    if (nodes[i].Color == GraphColor.Gray)
                    {
                        node = nodes[i].mData;
                        //node.SetPosition(node.NewX, node.NewY);
                        node.SetPosition(this.mNewXs[i], this.mNewYs[i]);
                    }
                }
                // Update edges attached to nodes that have moved
                for (i = 0; i < edges.Length; i++)
                {
                    //if (dirtyEdge[i])
                    if (edges[i].mSrcNode.Color == GraphColor.Gray ||
                        edges[i].mDstNode.Color == GraphColor.Gray)
                        edges[i].mData.Update();
                }
            }
            if (!this.OnIterationEnded(iteration, statusInPercent,
                dist, maxDist, message))
            {
                this.Abort();
            }
        }

        /// <summary><para>
        /// Reimplement this function to trigger events and other actions
        /// that occur after an iteration has ended and all the nodes and
        /// edges in the graph have been updated with their new positions.
        /// </para><para>
        /// If this function returns false, the entire algorithm is aborted
        /// as soon as possible.</para></summary>
        /// <param name="iteration">The total number of iterations that have
        /// occurred since this algorithm started its computation.</param>
        /// <param name="statusInPercent">The status of this algorithm in
        /// percent, which is usually <paramref name="iteration"/> divided by
        /// the maximum number of iterations specified in the parameters.</param>
        /// <param name="distanceChange">The total change in the positions
        /// of every <typeparamref name="Node"/> instance since the last 
        /// iteration; sum of squared distances between every old and new 
        /// position.</param>
        /// <param name="maxDistanceChange">The maximum possible total change
        /// in the positions of every <typeparamref name="Node"/> instance;
        /// squared length of bounding box diagonal times number of nodes.
        /// </param>
        /// <param name="message">The textual representation of the status of
        /// this algorithm, which usually describes its current computation 
        /// step or phase or iteration.</param>
        /// <returns>true to continue the computation after the iteration has 
        /// ended, or false to abort the entire algorithm as soon as possible.
        /// </returns>
        protected virtual bool OnIterationEnded(int iteration, double statusInPercent,
            double distanceChange, double maxDistanceChange, string message)
        {
            return true;
        }
    }
}
