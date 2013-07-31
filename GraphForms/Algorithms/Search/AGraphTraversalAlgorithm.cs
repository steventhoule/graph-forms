﻿using System;

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
        : ARootedAlgorithm<Node>
        where Edge : IGraphEdge<Node>
    {
        /// <summary>
        /// The graph traversed by this algorithm.
        /// </summary>
        protected readonly Digraph<Node, Edge> mGraph;
        /// <summary>
        /// A copy of the <see cref="P:Digraph`2.InternalEdges"/> of the 
        /// <see cref="mGraph"/> made at the beginning of this algorithm's 
        /// internal computation for traversing the graph with minimal 
        /// memory usage.</summary>
        protected Digraph<Node, Edge>.GEdge[] mGraphEdges;

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
        /// <summary>
        /// If true, all nodes in the graph that implement the
        /// <see cref="ISpecialNode"/> interface, along with the edges 
        /// connecting them to the rest of the graph are skipped over 
        /// in the traversal and left unexplored.
        /// </summary>
        protected bool bExSpecial = false;

        private bool bRootOnly;

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
        {
            this.mGraph = graph;
            this.bUndirected = !directed;
            this.bReversed = reversed;
            this.bRootOnly = false;
        }

        /// <summary>
        /// The graph traversed by this algorithm.
        /// </summary>
        public Digraph<Node, Edge> Graph
        {
            get { return this.mGraph; }
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
        /// If true, all nodes in the graph that implement the
        /// <see cref="ISpecialNode"/> interface, along with the edges 
        /// connecting them to the rest of the graph are skipped over 
        /// in the traversal and left unexplored.
        /// </summary>
        public bool ExcludeSpecialNodes
        {
            get { return this.bExSpecial; }
            set
            {
                if (this.State != ComputeState.Running)
                    this.bExSpecial = value;
            }
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

        #region Events
        /// <summary>
        /// Called just after the given node has finished being initialized,
        /// before this algorithm begins
        /// </summary>
        /// <param name="n">The <typeparamref name="Node"/> instance that  
        /// has just finished initializing.</param>
        /// <param name="index">The index of the node that has just 
        /// finished initializing.</param>
        protected virtual void OnInitializeNode(Node n, int index)
        {
        }

        /// <summary>
        /// Called right before the traversal starts at the given root node,
        /// <paramref name="n"/>.
        /// </summary>
        /// <param name="n">The root <typeparamref name="Node"/> instance
        /// at which the algorithm starts exploring the graph.</param>
        /// <param name="index">The index of the root node at which the 
        /// algorithm starts exploring the graph.</param>
        protected virtual void OnStartNode(Node n, int index)
        {
        }

        /// <summary>
        /// Called when an unexplored node is first encountered during the
        /// traversal of the graph, before this algorithm begins to explore
        /// its neighbors.
        /// </summary>
        /// <param name="n">The unexplored <typeparamref name="Node"/>
        /// instance that has just been discovered in the graph traversal.
        /// </param>
        /// <param name="index">The index of the unexplored node that has
        /// just been discovered in the graph traversal.</param>
        protected virtual void OnDiscoverNode(Node n, int index)
        {
        }

        /// <summary>
        /// Called after the algorithm has finished exploring all of the
        /// neighbors of the given node <paramref name="n"/> and returns
        /// to it.
        /// </summary>
        /// <param name="n">The <typeparamref name="Node"/> instance that 
        /// this graph traversal algorithm has just finished with and 
        /// won't be visiting again.</param>
        /// <param name="index">The index of the node instance that this
        /// graph traversal algorithm has just finished with and won't be
        /// visiting again.</param>
        protected virtual void OnFinishNode(Node n, int index)
        {
        }

        /// <summary>
        /// Called whenever the algorithm explores any edge in the graph,
        /// right before more specific actions are taken based on whether
        /// the edge's target is unexplored, being explored, or already
        /// has been explored.
        /// </summary>
        /// <param name="e">The <typeparamref name="Edge"/> instance being
        /// explored by this graph traversal algorithm.</param>
        /// <param name="srcIndex">The index of the <see 
        /// cref="P:IGraphEdge`1{Node}.SrcNode"/> of the edge <paramref 
        /// name="e"/>.</param>
        /// <param name="dstIndex">The index of the <see 
        /// cref="P:IGraphEdge`1{Node}.DstNode"/> of the edge <paramref 
        /// name="e"/>.</param>
        /// <param name="reversed">True if the edge is being explored from 
        /// destination to source instead of from source to destination.
        /// </param>
        protected virtual void OnExamineEdge(Edge e, 
            int srcIndex, int dstIndex, bool reversed)
        {
        }

        /// <summary>
        /// Called whenever the algorithm explores an edge in the graph
        /// connected to an unexplored node.
        /// </summary>
        /// <param name="e">The <typeparamref name="Edge"/> instance being
        /// explored by this graph traversal algorithm.</param>
        /// <param name="srcIndex">The index of the <see 
        /// cref="P:IGraphEdge`1{Node}.SrcNode"/> of the edge <paramref 
        /// name="e"/>.</param>
        /// <param name="dstIndex">The index of the <see 
        /// cref="P:IGraphEdge`1{Node}.DstNode"/> of the edge <paramref 
        /// name="e"/>.</param>
        /// <param name="reversed">True if the edge is being explored from 
        /// destination to source instead of from source to destination.
        /// </param>
        protected virtual void OnTreeEdge(Edge e, 
            int srcIndex, int dstIndex, bool reversed)
        {
        }

        /// <summary>
        /// Called whenever the algorithm explores an edge in the graph
        /// connected to a node that is currently being explored.
        /// </summary>
        /// <param name="e">The <typeparamref name="Edge"/> instance being
        /// explored by this graph traversal algorithm.</param>
        /// <param name="srcIndex">The index of the <see 
        /// cref="P:IGraphEdge`1{Node}.SrcNode"/> of the edge <paramref 
        /// name="e"/>.</param>
        /// <param name="dstIndex">The index of the <see 
        /// cref="P:IGraphEdge`1{Node}.DstNode"/> of the edge <paramref 
        /// name="e"/>.</param>
        /// <param name="reversed">True if the edge is being explored from 
        /// destination to source instead of from source to destination.
        /// </param>
        protected virtual void OnGrayEdge(Edge e,
            int srcIndex, int dstIndex, bool reversed)
        {
        }

        /// <summary>
        /// Called whenever the algorithm explores an edge in the graph
        /// connected to a node that has already been explored.
        /// </summary>
        /// <param name="e">The <typeparamref name="Edge"/> instance being
        /// explored by this graph traversal algorithm.</param>
        /// <param name="srcIndex">The index of the <see 
        /// cref="P:IGraphEdge`1{Node}.SrcNode"/> of the edge <paramref 
        /// name="e"/>.</param>
        /// <param name="dstIndex">The index of the <see 
        /// cref="P:IGraphEdge`1{Node}.DstNode"/> of the edge <paramref 
        /// name="e"/>.</param>
        /// <param name="reversed">True if the edge is being explored from 
        /// destination to source instead of from source to destination.
        /// </param>
        protected virtual void OnBlackEdge(Edge e,
            int srcIndex, int dstIndex, bool reversed)
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
            Digraph<Node, Edge>.GNode[] nodes
                = this.mGraph.InternalNodes;
            for (int i = 0; i < nodes.Length; i++)
            {
                if (this.State == ComputeState.Aborting)
                    return;
                nodes[i].Color = GraphColor.White;
                nodes[i].Index = i;
                this.OnInitializeNode(nodes[i].mData, i);
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
                return;

            // put all nodes to white
            this.Initialize();

            this.mGraphEdges = this.mGraph.InternalEdges;

            Digraph<Node, Edge>.GNode node;

            // if there is a starting node, start with it
            if (this.HasRoot || this.bRootOnly)
            {
                int index;
                if (!this.HasRoot)
                {
                    index = 0;
                }
                else
                {
                    index = this.mGraph.IndexOfNode(this.TryGetRoot());
                    if (index < 0 && this.bRootOnly)
                    {
                        index = 0;
                    }
                }
                if (index >= 0)
                {
                    node = this.mGraph.InternalNodeAt(index);
                    if (this.bExSpecial && node.mData is ISpecialNode)
                    {
                        int count = this.mGraph.NodeCount;
                        for (index = 0; index < count && 
                            node.mData is ISpecialNode; index++)
                        {
                            node = this.mGraph.InternalNodeAt(index);
                        }
                        if (index == count)
                            return;
                        else
                            index--;
                    }
                    this.OnStartNode(node.mData, index);//node.Index);
                    this.ComputeFromRoot(node);
                }
            }

            // process each node
            if (!this.bRootOnly)
            {
                Digraph<Node, Edge>.GNode[] nodes
                    = this.mGraph.InternalNodes;
                for (int i = 0; i < nodes.Length; i++)
                {
                    if (this.State == ComputeState.Aborting)
                        return;
                    node = nodes[i];
                    if (node.Color == GraphColor.White &&
                        !(this.bExSpecial && node.mData is ISpecialNode))
                    {
                        this.OnStartNode(node.mData, i);//node.Index);
                        this.ComputeFromRoot(node);
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
    }
}
