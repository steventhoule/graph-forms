using System;

namespace GraphForms.Algorithms.Search
{
    public class DepthFirstSearch<Node, Edge>
        : AGraphTraversalAlgorithm<Node, Edge>
        where Edge : IGraphEdge<Node>
    {
        private bool bImplicit = false;
        private SFrame[] mStack;

        public DepthFirstSearch(Digraph<Node, Edge> graph,
            bool directed, bool reversed)
            : base(graph, directed, reversed)
        {
            this.mStack = new SFrame[0];
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

        #region Events
        protected virtual void OnFinishEdge(Digraph<Node, Edge>.GEdge e, 
            bool reversed, uint depth)
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

        private class SFrame
        {
            public readonly Digraph<Node, Edge>.GNode Node;
            //public readonly uint Depth;
            public readonly int EdgeIndex;
            public readonly bool Reversed;

            public SFrame(Digraph<Node, Edge>.GNode node, //uint depth,
                int edgeIndex, bool reversed)
            {
                this.Node = node;
                //this.Depth = depth;
                this.EdgeIndex = edgeIndex;
                this.Reversed = reversed;
            }
        }

        private void Visit(Digraph<Node, Edge>.GNode root)
        {
            if (this.mStack.Length < this.mGraph.NodeCount)
            {
                this.mStack = new SFrame[this.mGraph.NodeCount];
            }
            root.Color = GraphColor.Gray;
            this.mDepths[root.Index] = 0;
            this.OnDiscoverNode(root, 0);

            SFrame frame;
            Digraph<Node, Edge>.GEdge e;
            Digraph<Node, Edge>.GNode u, v;

            bool rev = this.bReversed;
            uint depth, maxDepth = this.MaxDepth;
            int edgeCount = this.mGraph.EdgeCount;
            int i, j, edgeIndex, stop = this.bUndirected ? 2 : 1;
            
            // Find the first edge connected to the root
            /*rev = this.bReversed;
            for (j = 0; j < stop; j++)
            {
                for (i = 0; i < this.mGraphEdges.Length; i++)
                {
                    e = this.mGraphEdges[i];
                    u = rev ? e.DstNode : e.SrcNode;
                    if (u.Index == root.Index)
                        break;
                }
                if (i < this.mGraphEdges.Length)
                    break;
                rev = !rev;
            }/* */

            int todoCount = 1;
            this.mStack[0] = new SFrame(root, 0, rev);
            //todo.Push(new SFrame(root, 0, rev));
            while (todoCount > 0)
            {
                if (this.State == ComputeState.Aborting)
                    return;

                frame = this.mStack[--todoCount];//todo.Pop();
                u = frame.Node;
                depth = this.mDepths[u.Index];//frame.Depth;
                if (frame.EdgeIndex > 0)
                {
                    e = this.mGraph.InternalEdgeAt(frame.EdgeIndex - 1);
                    this.OnFinishEdge(e, frame.Reversed, depth);
                }

                /*if (depth > maxDepth)
                {
                    u.Color = GraphColor.Black;
                    this.OnFinishNode(u, depth);
                    continue;
                }/* */

                edgeIndex = frame.EdgeIndex;
                rev = frame.Reversed;
                for (j = rev == this.bReversed ? 0 : 1; j < stop; j++)
                {
                    for (i = edgeIndex; i < edgeCount; i++)
                    {
                        e = this.mGraph.InternalEdgeAt(i);
                        if (e.Hidden)
                            continue;
                        v = rev ? e.DstNode : e.SrcNode;
                        if (v.Index != u.Index)
                            continue;
                        v = rev ? e.SrcNode : e.DstNode;
                        if (v.Hidden)
                            continue;
                        this.OnExamineEdge(e, rev, depth);

                        switch (v.Color)
                        {
                            case GraphColor.White:
                                this.OnTreeEdge(e, rev, depth);
                                if (depth < maxDepth)
                                {
                                    //todo.Push(new SFrame(u, i + 1, rev));
                                    this.mStack[todoCount++]
                                        = new SFrame(u, i + 1, rev);
                                    u = v;
                                    depth++;
                                    u.Color = GraphColor.Gray;
                                    this.mDepths[u.Index] = depth;
                                    this.OnDiscoverNode(u, depth);
                                    // break or reset the loops appropriately
                                    i = edgeCount;
                                    j = -1;
                                    rev = !this.bReversed;
                                }
                                break;
                            case GraphColor.Gray:
                                // OnBackEdge
                                this.OnGrayEdge(e, rev, depth);
                                break;
                            case GraphColor.Black:
                                // OnForwardOrCrossEdge
                                this.OnBlackEdge(e, rev, depth);
                                break;
                        }
                    }
                    edgeIndex = 0;
                    rev = !rev;
                }
                u.Color = GraphColor.Black;
                this.OnFinishNode(u, depth);
            }
        }

        private void ImplicitVisit(Digraph<Node, Edge>.GNode u, uint depth)
        {
            if (depth > this.MaxDepth)
            {
                return;
            }
            u.Color = GraphColor.Gray;
            this.mDepths[u.Index] = depth;
            this.OnDiscoverNode(u, depth);

            Digraph<Node, Edge>.GEdge e;
            Digraph<Node, Edge>.GNode v;
            bool reversed = this.bReversed;
            int edgeCount = this.mGraph.EdgeCount;
            int i, j, stop = this.bUndirected ? 2 : 1;
            for (j = 0; j < stop; j++)
            {
                for (i = 0; i < edgeCount; i++)
                {
                    if (this.State == ComputeState.Aborting)
                        return;
                    e = this.mGraph.InternalEdgeAt(i);
                    if (e.Hidden)
                        continue;
                    v = reversed ? e.DstNode : e.SrcNode;
                    if (v.Index != u.Index)
                        continue;
                    v = reversed ? e.SrcNode : e.DstNode;
                    if (v.Hidden)
                        continue;
                    this.OnExamineEdge(e, reversed, depth);

                    switch (v.Color)
                    {
                        case GraphColor.White:
                            this.OnTreeEdge(e, reversed, depth);
                            this.ImplicitVisit(v, depth + 1);
                            this.OnFinishEdge(e, reversed, depth);
                            break;
                        case GraphColor.Gray:
                            // OnBackEdge
                            this.OnGrayEdge(e, reversed, depth);
                            break;
                        case GraphColor.Black:
                            // OnForwardOrCrossEdge
                            this.OnBlackEdge(e, reversed, depth);
                            break;
                    }
                }
                reversed = !reversed;
            }
            u.Color = GraphColor.Black;
            this.OnFinishNode(u, depth);
        }
    }
}
