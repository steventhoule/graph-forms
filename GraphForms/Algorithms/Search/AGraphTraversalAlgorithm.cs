using System;

namespace GraphForms.Algorithms.Search
{
    /// <summary>
    /// This is the base class for algorithms designed to traverse a graph
    /// and systematically explore its nodes and edges.  It has predefined
    /// events that derived classes should call as they explore its graph.
    /// </summary>
    /// <typeparam name="Node">The type of vertices in the graph traversed
    /// and explored by this algorithm.</typeparam>
    /// <typeparam name="Edge">The type of edges in the graph traversed
    /// and explored by this algorithm.</typeparam>
    public abstract class AGraphTraversalAlgorithm<Node, Edge>
        : AGraphAlgorithm<Node, Edge>
        where Edge : IGraphEdge<Node>
    {
        private static readonly uint[] sEmptyDepths = new uint[0];
        /// <summary>
        /// This array holds the depths at which nodes were encountered
        /// during the traversal of the graph, which is number of nodes in
        /// traversal path from the corresponding node to the root node
        /// where the traversal started.
        /// </summary>
        protected uint[] mDepths;
        /*/// <summary>
        /// A copy of the <see cref="P:Digraph`2.InternalEdges"/> of the 
        /// <see cref="mGraph"/> made at the beginning of this algorithm's 
        /// internal computation for traversing the graph with minimal 
        /// memory usage.</summary>
        protected Digraph<Node, Edge>.GEdge[] mGraphEdges;/* */
        /// <summary>
        /// If true, the graph is traversed from both the source edges and
        /// destination edges of each node instead of just one or the other,
        /// but <see cref="Reversed"/> still determines which edge list is
        /// traversed first on each node.
        /// </summary>
        protected readonly bool bUndirected;
        /// <summary>
        /// If true, the graph is traversed from destination nodes to source
        /// nodes instead of from source nodes to destination nodes.
        /// </summary>
        protected readonly bool bReversed;

        private bool bRootOnly;
        private uint mMaxDepth = uint.MaxValue;

        /// <summary>
        /// Initializes a new instance of an algorithm for traversing and
        /// exploring the given graph with the given directional constraints.
        /// </summary>
        /// <param name="graph">The graph to be explored by this algorithm.
        /// </param>
        /// <param name="directed">Whether the <paramref name="graph"/> is
        /// traversed as a directed graph or as an undirected one.</param>
        /// <param name="reversed">Whether the <paramref name="graph"/> is
        /// traversed in reverse, from edge destination to source, rather
        /// than from edge source to destination.</param>
        public AGraphTraversalAlgorithm(Digraph<Node, Edge> graph,
            bool directed, bool reversed)
            : base(graph)
        {
            this.mDepths = sEmptyDepths;
            this.bUndirected = !directed;
            this.bReversed = reversed;
            this.bRootOnly = false;
        }

        /// <summary>
        /// If true, the graph is traversed from destination nodes to source
        /// nodes instead of from source nodes to destination nodes.
        /// </summary>
        public bool Reversed
        {
            get { return this.bReversed; }
        }

        /// <summary>
        /// If false, the graph is traversed from both the source edges and
        /// destination edges of each node instead of just one or the other,
        /// but <see cref="Reversed"/> still determines which edge list is
        /// traversed first on each node.
        /// </summary>
        public bool Directed
        {
            get { return !this.bUndirected; }
        }

        /// <summary>
        /// If false (the default value), this algorithm continues to 
        /// traverse its graph after the root node and all nodes reachable 
        /// from it have been explored, restarting the traversal from the 
        /// next unexplored node found until all nodes in the graph have 
        /// been explored.
        /// </summary>
        public bool RootOnly
        {
            get { return this.bRootOnly; }
            set
            {
                if (this.State != ComputeState.Running)
                    this.bRootOnly = value;
            }
        }

        /// <summary><para>
        /// Gets or sets the maximum exploration depth from the root nodes
        /// at which the exploration starts.</para><para>
        /// Default is <see cref="uint.MaxValue"/>.</para></summary><value>
        /// Maximum exploration depth.</value>
        public uint MaxDepth
        {
            get { return this.mMaxDepth; }
            set
            {
                if (this.State != ComputeState.Running)
                    this.mMaxDepth = value;
            }
        }

        #region Events
        /// <summary><para>
        /// Called just after the given node has finished being initialized,
        /// before this algorithm begins its traversal of the graph.
        /// </para><para>
        /// DO NOT CHANGE THE COLOR OF <paramref name="n"/> UNLESS NECESSARY.
        /// </para></summary>
        /// <param name="n">The <typeparamref name="Node"/> instance that  
        /// has just finished initializing.</param><remarks>
        /// The <see cref="F:Digraph`2.GNode.Color"/> of <paramref name="n"/>
        /// will be <see cref="GraphColor.White"/> when this function is
        /// called. If it is set to any other color, it won't be explored by
        /// this algorithm unless it was set as a root. Other nodes
        /// connected to it might not be explored as well, depending on the
        /// structure of the graph being traversed by this algorithm.
        /// </remarks>
        protected virtual void OnInitializeNode(Digraph<Node, Edge>.GNode n)
        {
        }

        /// <summary><para>
        /// Called right before the traversal starts at the given root node,
        /// <paramref name="n"/>.</para><para>
        /// DO NOT CHANGE THE COLOR OF <paramref name="n"/> UNLESS NECESSARY.
        /// </para></summary>
        /// <param name="n">The root node at which the algorithm starts 
        /// exploring the graph.</param>
        protected virtual void OnStartNode(Digraph<Node, Edge>.GNode n)
        {
        }

        /// <summary><para>
        /// Called when an unexplored node is first encountered during the
        /// traversal of the graph, before this algorithm begins to explore
        /// its neighbors.</para><para>
        /// DO NOT CHANGE THE COLOR OF <paramref name="n"/> UNLESS NECESSARY.
        /// </para></summary>
        /// <param name="n">The unexplored node that has just been 
        /// discovered in the graph traversal.</param>
        protected virtual void OnDiscoverNode(
            Digraph<Node, Edge>.GNode n, uint depth)
        {
        }

        /// <summary><para>
        /// Called after the algorithm has finished exploring all of the
        /// neighbors of the given node <paramref name="n"/> and returns
        /// to it.</para><para>
        /// DO NOT CHANGE THE COLOR OF <paramref name="n"/> UNLESS NECESSARY.
        /// </para></summary>
        /// <param name="n">The node that 
        /// this graph traversal algorithm has just finished with and 
        /// won't be visiting again.</param>
        protected virtual void OnFinishNode(
            Digraph<Node, Edge>.GNode n, uint depth)
        {
        }

        /// <summary>
        /// Called whenever the algorithm explores any edge in the graph,
        /// right before more specific actions are taken based on whether
        /// the edge's target is unexplored, being explored, or already
        /// has been explored.
        /// </summary>
        /// <param name="e">The edge being explored by this graph traversal 
        /// algorithm.</param>
        /// <param name="reversed">True if the edge is being explored from 
        /// destination to source instead of from source to destination.
        /// </param>
        protected virtual void OnExamineEdge(Digraph<Node, Edge>.GEdge e,
            bool reversed, uint depth)
        {
        }

        /// <summary>
        /// Called whenever the algorithm explores an edge in the graph
        /// connected to an unexplored node.
        /// </summary>
        /// <param name="e">The edge being explored by this graph traversal 
        /// algorithm.</param>
        /// <param name="reversed">True if the edge is being explored from 
        /// destination to source instead of from source to destination.
        /// </param>
        protected virtual void OnTreeEdge(Digraph<Node, Edge>.GEdge e, 
            bool reversed, uint depth)
        {
        }

        /// <summary>
        /// Called whenever the algorithm explores an edge in the graph
        /// connected to a node that is currently being explored.
        /// </summary>
        /// <param name="e">The edge being explored by this graph traversal 
        /// algorithm.</param>
        /// <param name="reversed">True if the edge is being explored from 
        /// destination to source instead of from source to destination.
        /// </param>
        protected virtual void OnGrayEdge(Digraph<Node, Edge>.GEdge e,
            bool reversed, uint depth)
        {
        }

        /// <summary>
        /// Called whenever the algorithm explores an edge in the graph
        /// connected to a node that has already been explored.
        /// </summary>
        /// <param name="e">The edge being explored by this graph traversal 
        /// algorithm.</param>
        /// <param name="reversed">True if the edge is being explored from 
        /// destination to source instead of from source to destination.
        /// </param>
        protected virtual void OnBlackEdge(Digraph<Node, Edge>.GEdge e,
            bool reversed, uint depth)
        {
        }
        #endregion

        /// <summary>
        /// Initializes the graph traversal algorithm before it is performed,
        /// including setting data fields associated with each individual
        /// <typeparamref name="Node"/> instance in the <see cref="Graph"/>,
        /// particularly their Index and Color.
        /// </summary>
        public virtual void Initialize()
        {
            Digraph<Node, Edge>.GNode node;
            int count = this.mGraph.NodeCount;
            if (this.mDepths.Length < count)
            {
                this.mDepths = new uint[count];
            }
            for (int i = 0; i < count; i++)
            {
                if (this.State == ComputeState.Aborting)
                    return;
                node = this.mGraph.InternalNodeAt(i);
                node.Color = GraphColor.White;
                this.mDepths[node.Index] = 0;
                //nodes[i].Index = i;
                if (!node.Hidden)
                {
                    this.OnInitializeNode(node);
                }
            }
        }

        /// <summary>
        /// This function performs the core functions of a graph traversal
        /// algorithm, first calling <see cref="Initialize()"/> to initialize
        /// any needed data, and then attempting to run main traversal
        /// computation at the root node (if it's valid), and then restarting
        /// it at any nodes that haven't been explored.
        /// </summary>
        protected override void InternalCompute()
        {
            if (this.mGraph.NodeCount == 0 || this.mGraph.EdgeCount == 0)
            {
                return;
            }

            // put all nodes to white
            this.Initialize();

            //this.mGraphEdges = this.mGraph.InternalEdges;

            int i, count;
            Digraph<Node, Edge>.GNode node;

            // if there is a starting node, start with it
            if (this.RootCount > 0 || this.bRootOnly)
            {
                if (this.RootCount == 0)
                {
                    node = null;
                    count = this.mGraph.NodeCount;
                    for (i = 0; i < count; i++)
                    {
                        node = this.mGraph.InternalNodeAt(i);
                        if (!node.Hidden)
                            break;
                    }
                    if (i < count)
                    {
                        this.OnStartNode(node);
                        this.ComputeFromRoot(node);
                    }
                }
                else
                {
                    count = this.RootCount;
                    for (i = 0; i < count; i++)
                    {
                        if (this.State == ComputeState.Aborting)
                            return;
                        node = this.RootAt(i);
                        if (node.Color == GraphColor.White && !node.Hidden)
                        {
                            this.OnStartNode(node);
                            this.ComputeFromRoot(node);
                        }
                    }
                }
                this.ComputeFromRoots();
            }

            // process each node
            if (!this.bRootOnly)
            {
                count = this.mGraph.NodeCount;
                for (i = 0; i < count; i++)
                {
                    if (this.State == ComputeState.Aborting)
                        return;
                    node = this.mGraph.InternalNodeAt(i);
                    if (node.Color == GraphColor.White && !node.Hidden)
                    {
                        this.OnStartNode(node);
                        this.ComputeFromRoot(node);
                        this.ComputeFromRoots();
                    }
                }
            }
        }

        /// <summary>
        /// The implementation of this function should contain the core code  
        /// for traversing this algorithm's graph starting at the given 
        /// <paramref name="root"/> node and systematically traveling along
        /// edges to explore nodes, turning nodes gray as they are being
        /// explored and then black after finishing exploring them, and
        /// calling the exploration event functions along the way.
        /// </summary>
        /// <param name="root">The root node at which the traversal of the
        /// graph starts by following its edges to its unexplored neighboring
        /// nodes and so on until a dead end is reached.
        /// </param>
        protected abstract void ComputeFromRoot(
            Digraph<Node, Edge>.GNode root);

        protected virtual void ComputeFromRoots()
        {
        }
    }
}
