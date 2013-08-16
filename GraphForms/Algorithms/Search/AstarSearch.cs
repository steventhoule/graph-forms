using System;
using GraphForms.Algorithms.Collections;

namespace GraphForms.Algorithms.Search
{
    public abstract class AstarSearch<Node, Edge>
        : AGraphTraversalAlgorithm<Node, Edge>
        where Edge : IGraphEdge<Node>
    {
        private FibonacciNode<double, Digraph<Node, Edge>.GNode>.Heap mQueue;
        private FibonacciNode<double, Digraph<Node, Edge>.GNode>[] mQNodes;
        private double[] mDists;
        private int[] mPredecessors;

        public AstarSearch(Digraph<Node, Edge> graph, 
            bool directed, bool reversed)
            : base(graph, directed, reversed)
        {
            this.mQueue = new FibonacciNode<double, 
                Digraph<Node, Edge>.GNode>.Heap();
            this.mQNodes = new FibonacciNode<double, 
                Digraph<Node, Edge>.GNode>[0];
            this.mDists = new double[0];
            this.mPredecessors = new int[0];
        }

        public double[] TotalCosts
        {
            get
            {
                double[] totalCosts = new double[this.mQNodes.Length];
                for (int i = 0; i < totalCosts.Length; i++)
                {
                    totalCosts[i] = this.mQNodes[i].Priority;
                }
                return totalCosts;
            }
        }

        public double[] Distances
        {
            get { return this.mDists; }
        }

        public int[] Predecessors
        {
            get { return this.mPredecessors; }
        }

        #region Events
        protected virtual void OnExamineNode(
            Digraph<Node, Edge>.GNode n, uint depth)
        {
        }

        protected virtual void OnRelaxEdge(Digraph<Node, Edge>.GEdge e,
            bool reversed, uint depth)
        {
        }
        #endregion

        public override void Initialize()
        {
            base.Initialize();

            int count = this.mGraph.NodeCount;
            if (this.mQNodes.Length < count)
            {
                this.mQNodes = new FibonacciNode<double, 
                    Digraph<Node, Edge>.GNode>[count];
                this.mDists = new double[count];
                this.mPredecessors = new int[count];
            }
            for (int i = 0; i < count; i++)
            {
                this.mPredecessors[i] = -1;
            }
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

        protected virtual double Distance(Digraph<Node, Edge>.GEdge e,
            bool reversed, uint depth)
        {
            return e.Data.Weight;
        }

        protected abstract double HeuristicCost(Digraph<Node, Edge>.GNode n);

        private void EnqueueRoot(Digraph<Node, Edge>.GNode root)
        {
            //this.OnStartNode(s.mData, s.Index);

            root.Color = GraphColor.Gray;
            this.mDepths[root.Index] = 0;
            this.mDists[root.Index] = 0;
            double cost = this.HeuristicCost(root);
            this.OnDiscoverNode(root, 0);
            this.mQNodes[root.Index] 
                = this.mQueue.Enqueue(cost, root);
        }

        private void FlushVisitQueue()
        {
            bool rev;
            double dist, cost;
            uint depth, maxDepth = this.MaxDepth;
            int edgeCount = this.mGraph.EdgeCount;
            int i, j, stop = this.bUndirected ? 2 : 1;
            Digraph<Node, Edge>.GEdge e;
            Digraph<Node, Edge>.GNode u, v;

            while (this.mQueue.Count > 0 &&
                   this.State != ComputeState.Aborting)
            {
                u = this.mQueue.Dequeue().Value;
                depth = this.mDepths[u.Index];
                this.OnExamineNode(u, depth);

                rev = this.bReversed;
                for (j = 0; j < stop; j++)
                {
                    for (i = 0; i < edgeCount; i++)
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

                        dist = this.mDists[u.Index] 
                             + this.Distance(e, rev, depth);
                        switch (v.Color)
                        {
                            case GraphColor.White:
                                this.OnTreeEdge(e, rev, depth);
                                if (depth < maxDepth)
                                {
                                    v.Color = GraphColor.Gray;
                                    this.mDepths[v.Index] = depth + 1;
                                    this.mDists[v.Index] = dist;
                                    cost = dist + this.HeuristicCost(v);
                                    this.mPredecessors[v.Index] = u.Index;
                                    this.OnDiscoverNode(v, depth + 1);
                                    this.mQNodes[v.Index]
                                        = this.mQueue.Enqueue(cost, v);
                                }
                                break;
                            case GraphColor.Gray:
                                // OnNonTreeEdge
                                // OnBackEdge
                                this.OnGrayEdge(e, rev, depth);
                                if (dist < this.mDists[v.Index])
                                {
                                    this.mDists[v.Index] = dist;
                                    cost = dist + this.HeuristicCost(v);
                                    this.mPredecessors[v.Index] = u.Index;
                                    this.OnRelaxEdge(e, rev, depth);
                                    this.mQueue.ChangePriority(
                                        this.mQNodes[v.Index], cost);
                                }
                                break;
                            case GraphColor.Black:
                                // OnNonTreeEdge
                                // OnForwardOrCrossEdge
                                this.OnBlackEdge(e, rev, depth);
                                if (dist < this.mDists[v.Index])
                                {
                                    // TODO: Will this ever happen?
                                    this.mDists[v.Index] = dist;
                                    cost = dist + this.HeuristicCost(v);
                                    this.mPredecessors[v.Index] = u.Index;
                                    this.OnRelaxEdge(e, rev, depth);
                                    this.mQNodes[v.Index]
                                        = this.mQueue.Enqueue(cost, v);
                                }
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
