using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Search
{
    public class DepthFirstSearch<Node, Edge>
        : AGraphTraversalAlgorithm<Node, Edge>
        where Edge : IGraphEdge<Node>
    {
        private bool bImplicit = false;
        private uint mMaxDepth = uint.MaxValue;

        public DepthFirstSearch(Digraph<Node, Edge> graph,
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
        /// Default is <see cref="uint.MaxValue"/>.
        /// </summary>
        /// <value>
        /// Maximum exploration depth.
        /// </value>
        public uint MaxDepth
        {
            get { return this.mMaxDepth; }
            set 
            {
                if (value == 0)
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

        protected override void ComputeFromRoot(
            Digraph<Node, Edge>.GNode root)
        {
            if (this.bImplicit)
                this.ImplicitVisit(root, 0);
            else
                this.Visit(root);
        }

        private class SearchFrame
        {
            public readonly Digraph<Node, Edge>.GNode Node;
            public readonly uint Depth;
            public readonly int EdgeIndex;
            public readonly int LastIndex;
            public readonly bool Reversed;

            public SearchFrame(Digraph<Node, Edge>.GNode node,
                uint depth, int edgeIndex, int lastIndex, bool reversed)
            {
                this.Node = node;
                this.Depth = depth;
                this.EdgeIndex = edgeIndex;
                this.LastIndex = lastIndex;
                this.Reversed = reversed;
            }
        }

        private void Visit(Digraph<Node, Edge>.GNode root)
        {
            Stack<SearchFrame> todo = new Stack<SearchFrame>();
            root.Color = GraphColor.Gray;
            this.OnDiscoverNode(root.Data, root.Index);

            uint depth;
            bool reversed;
            SearchFrame frame;
            Digraph<Node, Edge>.GEdge e;
            Digraph<Node, Edge>.GNode u, v;
            int i, j, edgeIndex, stop = this.bUndirected ? 2 : 1;
            
            // Find the first edge connected to the root
            reversed = this.bReversed;
            for (j = 0; j < stop; j++)
            {
                for (i = 0; i < this.mGraphEdges.Length; i++)
                {
                    e = this.mGraphEdges[i];
                    u = reversed ? e.mDstNode : e.mSrcNode;
                    if (u.Index == root.Index)
                        break;
                }
                if (i < this.mGraphEdges.Length)
                    break;
                reversed = !reversed;
            }
            
            todo.Push(new SearchFrame(root, 0, 0, -1, reversed));
            while (todo.Count > 0)
            {
                if (this.State == ComputeState.Aborting)
                    return;

                frame = todo.Pop();
                u = frame.Node;
                depth = frame.Depth;
                if (frame.LastIndex != -1)
                {
                    e = this.mGraphEdges[frame.LastIndex];
                    this.OnFinishEdge(e.mData, e.mSrcNode.Index, 
                        e.mDstNode.Index, frame.Reversed);
                }

                if (depth > this.mMaxDepth)
                {
                    u.Color = GraphColor.Black;
                    this.OnFinishNode(u.Data, u.Index);
                    continue;
                }

                edgeIndex = frame.EdgeIndex;
                reversed = frame.Reversed;
                for (j = reversed == this.bReversed ? 0 : 1; j < stop; j++)
                {
                    for (i = edgeIndex; i < this.mGraphEdges.Length; i++)
                    {
                        e = this.mGraphEdges[i];
                        v = reversed ? e.mDstNode : e.mSrcNode;
                        if (v.Index != u.Index)
                            continue;
                        v = reversed ? e.mSrcNode : e.mDstNode;

                        if (this.bExSpecial && v.mData is ISpecialNode)
                            continue;
                        this.OnExamineEdge(e.mData, e.mSrcNode.Index,
                            e.mDstNode.Index, reversed);

                        switch (v.Color)
                        {
                            case GraphColor.White:
                                this.OnTreeEdge(e.mData, e.mSrcNode.Index,
                                    e.mDstNode.Index, reversed);
                                todo.Push(new SearchFrame(
                                    u, depth, i + 1, i, reversed));
                                u = v;
                                depth++;
                                u.Color = GraphColor.Gray;
                                this.OnDiscoverNode(u.mData, u.Index);
                                // break or reset the loops appropriately
                                i = this.mGraphEdges.Length;
                                j = -1;
                                reversed = !this.bReversed;
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
                    edgeIndex = 0;
                    reversed = !reversed;
                }
                u.Color = GraphColor.Black;
                this.OnFinishNode(u.mData, u.Index);
            }
        }

        private void ImplicitVisit(Digraph<Node, Edge>.GNode u, uint depth)
        {
            if (depth > this.mMaxDepth)
                return;

            u.Color = GraphColor.Gray;
            this.OnDiscoverNode(u.mData, u.Index);

            Digraph<Node, Edge>.GEdge e;
            Digraph<Node, Edge>.GNode v;
            bool reversed = this.bReversed;
            int i, j, stop = this.bUndirected ? 2 : 1;
            for (j = 0; j < stop; j++)
            {
                for (i = 0; i < this.mGraphEdges.Length; i++)
                {
                    e = this.mGraphEdges[i];
                    if (this.State == ComputeState.Aborting)
                        return;

                    v = reversed ? e.mDstNode : e.mSrcNode;
                    if (v.Index != u.Index)
                        continue;
                    v = reversed ? e.mSrcNode : e.mDstNode;

                    if (this.bExSpecial && v.mData is ISpecialNode)
                        continue;
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
                reversed = !reversed;
            }
            u.Color = GraphColor.Black;
            this.OnFinishNode(u.mData, u.Index);
        }
    }
}
