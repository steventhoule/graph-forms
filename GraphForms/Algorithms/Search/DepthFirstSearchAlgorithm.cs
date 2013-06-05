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

        public DepthFirstSearchAlgorithm(Digraph<Node, Edge> graph)
            : base(graph, true, false)
        {
        }

        public DepthFirstSearchAlgorithm(Digraph<Node, Edge> graph,
            bool directed, bool reversed)
            : base(graph, directed, reversed)
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
        #endregion

        protected override void InternalCompute()
        {
            if (this.mGraph.NodeCount == 0 || this.mGraph.EdgeCount == 0)
                return;

            // put all nodes to white
            this.Initialize();

            Digraph<Node, Edge>.GNode node;

            // if there is a starting node, start with it
            if (this.HasRoot)
            {
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
            Digraph<Node, Edge>.GNode[] nodes
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
            public readonly Digraph<Node, Edge>.GNode Node;
            public readonly Digraph<Node, Edge>.GEdge[] Edges;
            public readonly int Depth;
            public readonly int EdgeIndex;
            public readonly bool Reversed;

            public SearchFrame(Digraph<Node, Edge>.GNode node,
                Digraph<Node, Edge>.GEdge[] edges,
                int depth, int edgeIndex, bool reversed)
            {
                this.Node = node;
                this.Edges = edges;
                this.Depth = depth;
                this.EdgeIndex = edgeIndex;
                this.Reversed = reversed;
            }
        }

        private void Visit(Digraph<Node, Edge>.GNode root)
        {
            Stack<SearchFrame> todo = new Stack<SearchFrame>();
            root.Color = GraphColor.Gray;
            this.OnDiscoverNode(root.Data, root.Index);

            SearchFrame frame;
            Digraph<Node, Edge>.GNode u, v;
            Digraph<Node, Edge>.GEdge e;
            Digraph<Node, Edge>.GEdge[] edges;
            int depth, edgeIndex;
            bool reversed;

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

                    v = e.mDstNode;
                    reversed = v.Index == u.Index;//v.Equals(u);
                    if (reversed)
                        v = e.mSrcNode;
                    this.OnExamineEdge(e.mData, e.mSrcNode.Index,
                        e.mDstNode.Index, reversed);
                    
                    switch (v.Color)
                    {
                        case GraphColor.White:
                            this.OnTreeEdge(e.mData, e.mSrcNode.Index,
                                e.mDstNode.Index, reversed);
                            todo.Push(new SearchFrame(u, edges, depth, 
                                edgeIndex, reversed));
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
                            // OnBackEdge
                            this.OnGrayEdge(e.mData, e.mSrcNode.Index,
                                e.mDstNode.Index, reversed);
                            break;
                        case GraphColor.Black:
                            // OnForwardOrCrossEdge
                            this.OnBlackEdge(e.mData, e.mSrcNode.Index,
                                e.mDstNode.Index, reversed);
                            break;
                    }
                }
                u.Color = GraphColor.Black;
                this.OnFinishNode(u.mData, u.Index);
            }
        }

        private void ImplicitVisit(Digraph<Node, Edge>.GNode u,
                                   int depth)
        {
            if (depth > this.mMaxDepth)
                return;

            u.Color = GraphColor.Gray;
            this.OnDiscoverNode(u.mData, u.Index);

            Digraph<Node, Edge>.GNode v;
            Digraph<Node, Edge>.GEdge e;
            Digraph<Node, Edge>.GEdge[] edges;
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

                v = e.mDstNode;
                reversed = v.Index == u.Index;//v.Equals(u);
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
                        // OnBackEdge
                        this.OnGrayEdge(e.mData, e.mSrcNode.Index, 
                            e.mDstNode.Index, reversed);
                        break;
                    case GraphColor.Black:
                        // OnForwardOrCrossEdge
                        this.OnBlackEdge(e.mData, e.mSrcNode.Index, 
                            e.mDstNode.Index, reversed);
                        break;
                }
            }
            u.Color = GraphColor.Black;
            this.OnFinishNode(u.mData, u.Index);
        }
    }
}
