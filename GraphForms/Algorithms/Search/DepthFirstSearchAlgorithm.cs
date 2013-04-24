using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Search
{
    public class DepthFirstSearchAlgorithm<Node, Edge>
        : AGraphTraversalAlgorithm<Node, Edge>
        where Node : class
        where Edge : class, IGraphEdge<Node>
    {
        private bool bImplicit = false;
        private int mMaxDepth = int.MaxValue;

        public DepthFirstSearchAlgorithm(DirectionalGraph<Node, Edge> graph)
            : base(graph)
        {
        }

        public DepthFirstSearchAlgorithm(DirectionalGraph<Node, Edge> graph,
            bool undirected, bool reversed)
            : base(graph, undirected, reversed)
        {
        }

        public bool Implicit
        {
            get { return this.bImplicit; }
            set
            {
                if (this.State != ComputeState.Running)
                    this.bImplicit = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum exploration depth from the start vertex.
        /// Default is <see cref="int.MaxValue"/>.
        /// </summary>
        /// <value>
        /// Maximum exploration depth.
        /// </value>
        public int MaxDepth
        {
            get { return this.mMaxDepth; }
            set 
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("MaxDepth");
                this.mMaxDepth = value; 
            }
        }

        #region Events
        protected virtual void OnFinishEdge(Edge e, 
            int srcIndex, int dstIndex, bool reversed)
        {
        }

        protected virtual void OnBackEdge(Edge e, 
            int srcIndex, int dstIndex, bool reversed)
        {
        }

        protected virtual void OnForwardOrCrossEdge(Edge e,
            int srcIndex, int dstIndex, bool reversed)
        {
        }
        #endregion

        protected override void InternalCompute()
        {
            if (this.mGraph.NodeCount == 0 || this.mGraph.EdgeCount == 0)
                return;

            // put all nodes to white
            this.Initialize();

            DirectionalGraph<Node, Edge>.GraphNode node;

            // if there is a starting vertex, start with it
            if (this.HasRoot)
            {
                // equeue select root only
                int index = this.mGraph.IndexOfNode(this.TryGetRoot());
                if (index >= 0)
                {
                    node = this.mGraph.InternalNodeAt(index);
                    this.OnStartNode(node.mData, index);
                    if (this.bImplicit)
                        this.ImplicitVisit(node, 0);
                    else
                        this.Visit(node);
                }
            }

            // process each node
            DirectionalGraph<Node, Edge>.GraphNode[] nodes
                = this.mGraph.InternalNodes;
            for (int i = 0; i < nodes.Length; i++)
            {
                if (this.State == ComputeState.Aborting)
                    return;
                node = nodes[i];
                if (node.Color == GraphColor.White)
                {
                    this.OnStartNode(node.mData, i);
                    if (this.bImplicit)
                        this.ImplicitVisit(node, 0);
                    else
                        this.Visit(node);
                }
            }
        }

        private class SearchFrame
        {
            public readonly DirectionalGraph<Node, Edge>.GraphNode Node;
            public readonly DirectionalGraph<Node, Edge>.GraphEdge[] Edges;
            public readonly int Depth;
            public readonly int EdgeIndex;
            public readonly bool Reversed;

            public SearchFrame(DirectionalGraph<Node, Edge>.GraphNode node,
                DirectionalGraph<Node, Edge>.GraphEdge[] edges,
                int depth, int edgeIndex, bool reversed)
            {
                this.Node = node;
                this.Edges = edges;
                this.Depth = depth;
                this.EdgeIndex = edgeIndex;
                this.Reversed = reversed;
            }
        }

        private void Visit(DirectionalGraph<Node, Edge>.GraphNode root)
        {
            Stack<SearchFrame> todo = new Stack<SearchFrame>();
            root.Color = GraphColor.Gray;
            this.OnDiscoverNode(root.Data, root.Index);

            SearchFrame frame;
            DirectionalGraph<Node, Edge>.GraphNode u, v;
            DirectionalGraph<Node, Edge>.GraphEdge e;
            DirectionalGraph<Node, Edge>.GraphEdge[] edges;
            int depth, edgeIndex;
            bool rev;

            if (this.bUndirected)
                edges = root.AllInternalEdges(this.bReversed);
            else if (this.bReversed)
                edges = root.InternalSrcEdges;
            else
                edges = root.InternalDstEdges;
            
            todo.Push(new SearchFrame(root, edges, 0, 0, false));
            while (todo.Count > 0)
            {
                if (this.State == ComputeState.Aborting)
                    return;

                frame = todo.Pop();
                u = frame.Node;
                depth = frame.Depth;
                edges = frame.Edges;
                edgeIndex = frame.EdgeIndex;
                if (edgeIndex > 0)
                {
                    e = edges[edgeIndex - 1];
                    this.OnFinishEdge(e.mData, e.mSrcNode.Index, 
                        e.mDstNode.Index, frame.Reversed);
                }

                if (depth > this.mMaxDepth)
                {
                    u.Color = GraphColor.Black;
                    this.OnFinishNode(u.Data, u.Index);
                    continue;
                }

                while (edgeIndex < edges.Length)
                {
                    e = edges[edgeIndex];
                    edgeIndex++;
                    if (this.State == ComputeState.Aborting)
                        return;
                    if (e.Color == GraphColor.Black) // edge already visited
                        continue;
                    e.Color = GraphColor.Black;

                    v = e.mDstNode;
                    rev = v.Equals(u);
                    if (rev)
                        v = e.mSrcNode;
                    this.OnExamineEdge(e.mData, e.mSrcNode.Index,
                        e.mDstNode.Index, rev);
                    
                    switch (v.Color)
                    {
                        case GraphColor.White:
                            this.OnTreeEdge(e.mData, e.mSrcNode.Index,
                                e.mDstNode.Index, rev);
                            todo.Push(new SearchFrame(u, edges, depth, 
                                edgeIndex, rev));
                            u = v;
                            edgeIndex = 0;
                            depth++;
                            if (this.bUndirected)
                                edges = u.AllInternalEdges(this.bReversed);
                            else if (this.bReversed)
                                edges = u.InternalSrcEdges;
                            else
                                edges = u.InternalDstEdges;
                            u.Color = GraphColor.Gray;
                            this.OnDiscoverNode(u.mData, u.Index);
                            break;
                        case GraphColor.Gray:
                            this.OnBackEdge(e.mData, e.mSrcNode.Index,
                                e.mDstNode.Index, rev);
                            break;
                        case GraphColor.Black:
                            this.OnForwardOrCrossEdge(e.mData, e.mSrcNode.Index,
                                e.mDstNode.Index, rev);
                            break;
                    }
                }
                u.Color = GraphColor.Black;
                this.OnFinishNode(u.mData, u.Index);
            }
        }

        private void ImplicitVisit(DirectionalGraph<Node, Edge>.GraphNode u,
                                   int depth)
        {
            if (depth > this.mMaxDepth)
                return;

            u.Color = GraphColor.Gray;
            this.OnDiscoverNode(u.mData, u.Index);

            DirectionalGraph<Node, Edge>.GraphNode v;
            DirectionalGraph<Node, Edge>.GraphEdge e;
            DirectionalGraph<Node, Edge>.GraphEdge[] edges;
            if (this.bUndirected)
                edges = u.AllInternalEdges(this.bReversed);
            else if (this.bReversed)
                edges = u.InternalSrcEdges;
            else
                edges = u.InternalDstEdges;

            bool reversed;
            for (int i = 0; i < edges.Length; i++)
            {
                e = edges[i];
                if (this.State == ComputeState.Aborting)
                    return;
                if (e.Color == GraphColor.Black) // edge already visited
                    continue;
                e.Color = GraphColor.Black;

                v = e.mDstNode;
                reversed = v.Equals(u);
                if (reversed)
                    v = e.mSrcNode;
                this.OnExamineEdge(e.mData, e.mSrcNode.Index, 
                    e.mDstNode.Index, reversed);

                switch (v.Color)
                {
                    case GraphColor.White:
                        this.OnTreeEdge(e.mData, e.mSrcNode.Index, 
                            e.mDstNode.Index, reversed);
                        this.ImplicitVisit(v, depth + 1);
                        this.OnFinishEdge(e.mData, e.mSrcNode.Index, 
                            e.mDstNode.Index, reversed);
                        break;
                    case GraphColor.Gray:
                        this.OnBackEdge(e.mData, e.mSrcNode.Index, 
                            e.mDstNode.Index, reversed);
                        break;
                    case GraphColor.Black:
                        this.OnForwardOrCrossEdge(e.mData, e.mSrcNode.Index, 
                            e.mDstNode.Index, reversed);
                        break;
                }
            }
            u.Color = GraphColor.Black;
            this.OnFinishNode(u.mData, u.Index);
        }
    }
}
