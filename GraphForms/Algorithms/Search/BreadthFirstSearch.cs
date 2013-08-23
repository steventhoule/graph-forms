using System;

namespace GraphForms.Algorithms.Search
{
    public class BreadthFirstSearch<Node, Edge>
        : AGraphTraversalAlgorithm<Node, Edge>
        where Edge : IGraphEdge<Node>
    {
        //private Queue<int> mQueue;
        private Digraph<Node, Edge>.GNode[] mQueue;
        private int mQIndex;
        private int mQCount;

        public BreadthFirstSearch(Digraph<Node, Edge> graph,
            bool directed, bool reversed)
            : base(graph, directed, reversed)
        {
            //this.mQueue = new Queue<int>(graph.NodeCount);
            this.mQueue = new Digraph<Node, Edge>.GNode[0];
        }

        #region Events
        protected virtual void OnExamineNode(
            Digraph<Node, Edge>.GNode n, uint depth)
        {
        }
        #endregion

        public override void Initialize()
        {
            int count = this.mGraph.NodeCount;
            if (this.mQueue.Length < count)
            {
                this.mQueue = new Digraph<Node, Edge>.GNode[count];
            }
            this.mQIndex = 0;
            this.mQCount = 0;
            base.Initialize();
        }

        protected override void ComputeFromRoot(
            Digraph<Node, Edge>.GNode root)
        {
            this.EnqueueRoot(root);
            //this.FlushVisitQueue();
        }

        protected override void ComputeFromRoots()
        {
            this.FlushVisitQueue();
        }

        private void EnqueueRoot(Digraph<Node, Edge>.GNode root)
        {
            //this.OnStartNode(s.mData, s.Index);

            root.Color = GraphColor.Gray;
            this.mDepths[root.Index] = 0;
            this.OnDiscoverNode(root, 0);
            //this.mQueue.Enqueue(root.Index);
            this.mQueue[this.mQCount++] = root;
        }

        private void FlushVisitQueue()
        {
            bool rev;
            uint depth, maxDepth = this.MaxDepth;
            int edgeCount = this.mGraph.EdgeCount;
            int i, j, stop = this.bUndirected ? 2 : 1;
            Digraph<Node, Edge>.GEdge e;
            Digraph<Node, Edge>.GNode u, v;

            while (this.mQIndex < this.mQCount &&//this.mQueue.Count > 0 &&
                   this.State != ComputeState.Aborting)
            {
                u = this.mQueue[this.mQIndex++];//this.mQueue.Dequeue();
                depth = this.mDepths[u.Index];
                this.OnExamineNode(u, depth);

                rev = this.bReversed;
                for (j = 0; j < stop; j++)
                {
                    for (i = 0; i < edgeCount; i++)
                    {
                        e = this.mGraph.InternalEdgeAt(i);
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
                                    v.Color = GraphColor.Gray;
                                    this.mDepths[v.Index] = depth + 1;
                                    this.OnDiscoverNode(v, depth + 1);
                                    //this.mQueue.Enqueue(v.Index);
                                    this.mQueue[this.mQCount++] = v;
                                }
                                break;
                            case GraphColor.Gray:
                                // OnNonTreeEdge
                                // OnBackEdge
                                this.OnGrayEdge(e, rev, depth);
                                break;
                            case GraphColor.Black:
                                // OnNonTreeEdge
                                // OnForwardOrCrossEdge
                                this.OnBlackEdge(e, rev, depth);
                                break;
                        }
                    }
                    rev = !rev;
                }
                u.Color = GraphColor.Black;
                this.OnFinishNode(u, depth);
            }
        }
    }
}
