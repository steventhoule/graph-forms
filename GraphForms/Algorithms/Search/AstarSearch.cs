using System;
using System.Collections.Generic;
using System.Text;
using GraphForms.Algorithms.Collections;

namespace GraphForms.Algorithms.Search
{
    public abstract class AstarSearch<Node, Edge>
        : AGraphTraversalAlgorithm<Node, Edge>
        where Edge : IGraphEdge<Node>
    {
        private FibonacciNode<double, int>.Heap mQueue;
        private FibonacciNode<double, int>[] mQueueNodes;
        private double[] mDists;
        private int[] mPredecessors;

        public AstarSearch(Digraph<Node, Edge> graph, 
            bool directed, bool reversed)
            : base(graph, directed, reversed)
        {
            this.mQueue = new FibonacciNode<double, int>.Heap();
        }

        public double[] TotalCosts
        {
            get
            {
                double[] totalCosts = new double[this.mQueueNodes.Length];
                for (int i = 0; i < totalCosts.Length; i++)
                {
                    totalCosts[i] = this.mQueueNodes[i].Priority;
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
        protected virtual void OnExamineNode(Node n, int index)
        {
        }

        protected virtual void OnRelaxEdge(Edge e,
            int srcIndex, int dstIndex, bool reversed)
        {
        }
        #endregion

        public override void Initialize()
        {
            base.Initialize();

            int count = this.mGraph.NodeCount;
            this.mQueueNodes = new FibonacciNode<double, int>[count];
            this.mDists = new double[count];
            this.mPredecessors = new int[count];
            for (int i = 0; i < count; i++)
            {
                this.mPredecessors[i] = -1;
            }
        }

        protected override void ComputeFromRoot(
            Digraph<Node, Edge>.GNode root)
        {
            this.EnqueueRoot(root);
            this.FlushVisitQueue();
        }

        protected virtual double Distance(Edge e,
            int srcIndex, int dstIndex, bool reversed)
        {
            return e.Weight;
        }

        protected abstract double HeuristicCost(Node n, int index);

        private void EnqueueRoot(Digraph<Node, Edge>.GNode s)
        {
            //this.OnStartNode(s.mData, s.Index);

            s.Color = GraphColor.Gray;

            this.mDists[s.Index] = 0;
            double cost = this.HeuristicCost(s.mData, s.Index);

            this.OnDiscoverNode(s.mData, s.Index);
            this.mQueueNodes[s.Index] 
                = this.mQueue.Enqueue(cost, s.Index);
        }

        private void FlushVisitQueue()
        {
            int i;
            bool reversed;
            double dist, cost;
            Digraph<Node, Edge>.GNode u, v;
            Digraph<Node, Edge>.GEdge e;
            Digraph<Node, Edge>.GEdge[] edges;
            Digraph<Node, Edge>.GNode[] nodes
                = this.mGraph.InternalNodes;

            while (this.mQueue.Count > 0 &&
                   this.State != ComputeState.Aborting)
            {
                u = nodes[this.mQueue.Dequeue().Value];
                this.OnExamineNode(u.mData, u.Index);

                if (this.bUndirected)
                    edges = u.AllInternalEdges(this.bReversed);
                else if (this.bReversed)
                    edges = u.InternalSrcEdges;
                else
                    edges = u.InternalDstEdges;
                for (i = 0; i < edges.Length; i++)
                {
                    e = edges[i];
                    v = e.DstNode;
                    reversed = v.Index == u.Index;//v.Equals(u);
                    if (reversed)
                        v = e.SrcNode;
                    if (this.bExSpecial && v.mData is ISpecialNode)
                        continue;
                    this.OnExamineEdge(e.mData, 
                        e.mSrcNode.Index, e.mDstNode.Index, reversed);
                    
                    dist = this.mDists[u.Index] + this.Distance(e.mData, 
                        e.mSrcNode.Index, e.mDstNode.Index, reversed);
                    switch (v.Color)
                    {
                        case GraphColor.White:
                            this.OnTreeEdge(e.mData, e.mSrcNode.Index, 
                                e.mDstNode.Index, reversed);
                            v.Color = GraphColor.Gray;
                            this.mDists[v.Index] = dist;
                            cost = dist + this.HeuristicCost(v.mData, v.Index);
                            this.mPredecessors[v.Index] = u.Index;
                            this.OnDiscoverNode(v.mData, v.Index);
                            this.mQueueNodes[v.Index] =
                                this.mQueue.Enqueue(cost, v.Index);
                            break;
                        case GraphColor.Gray:
                            // OnNonTreeEdge
                            // OnBackEdge
                            this.OnGrayEdge(e.mData, 
                                e.mSrcNode.Index,
                                e.mDstNode.Index, reversed);
                            if (dist < this.mDists[v.Index])
                            {
                                this.mDists[v.Index] = dist;
                                cost = dist + this.HeuristicCost(v.mData, v.Index);
                                this.mPredecessors[v.Index] = u.Index;
                                this.OnRelaxEdge(e.mData, e.mSrcNode.Index,
                                    e.mDstNode.Index, reversed);
                                this.mQueue.ChangePriority(
                                    this.mQueueNodes[v.Index], cost);
                            }
                            break;
                        case GraphColor.Black:
                            // OnNonTreeEdge
                            // OnForwardOrCrossEdge
                            this.OnBlackEdge(e.mData, e.mSrcNode.Index,
                                e.mDstNode.Index, reversed);
                            if (dist < this.mDists[v.Index])
                            {
                                // TODO: Will this ever happen?
                                this.mDists[v.Index] = dist;
                                cost = dist + this.HeuristicCost(v.mData, v.Index);
                                this.mPredecessors[v.Index] = u.Index;
                                this.OnRelaxEdge(e.mData, e.mSrcNode.Index,
                                    e.mDstNode.Index, reversed);
                                this.mQueueNodes[v.Index] 
                                    = this.mQueue.Enqueue(cost, v.Index);
                            }
                            break;
                    }
                }
                u.Color = GraphColor.Black;
                this.OnFinishNode(u.mData, u.Index);
            }
        }
    }
}
