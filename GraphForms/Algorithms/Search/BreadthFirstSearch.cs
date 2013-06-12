using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Search
{
    public class BreadthFirstSearch<Node, Edge>
        : AGraphTraversalAlgorithm<Node, Edge>
        where Node : class
        where Edge : class, IGraphEdge<Node>
    {
        private Queue<Digraph<Node, Edge>.GNode> mNodeQueue;

        public BreadthFirstSearch(Digraph<Node, Edge> graph)
            : base(graph, true, false)
        {
            this.mNodeQueue = new Queue<Digraph<Node, 
                Edge>.GNode>(graph.NodeCount + 1);
        }

        public BreadthFirstSearch(Digraph<Node, Edge> graph,
            bool directed, bool reversed)
            : base(graph, directed, reversed)
        {
            this.mNodeQueue = new Queue<Digraph<Node, 
                Edge>.GNode>(graph.NodeCount + 1);
        }

        #region Events
        protected virtual void OnExamineNode(Node n, int index)
        {
        }
        #endregion

        protected override void ComputeFromRoot(
            Digraph<Node, Edge>.GNode root)
        {
            this.EnqueueRoot(root);
            this.FlushVisitQueue();
        }

        private void EnqueueRoot(Digraph<Node, Edge>.GNode s)
        {
            //this.OnStartNode(s.mData, s.Index);

            s.Color = GraphColor.Gray;

            this.OnDiscoverNode(s.mData, s.Index);
            this.mNodeQueue.Enqueue(s);
        }

        private void FlushVisitQueue()
        {
            int i;
            bool reversed;
            Digraph<Node, Edge>.GNode u, v;
            Digraph<Node, Edge>.GEdge e;
            Digraph<Node, Edge>.GEdge[] edges;

            while (this.mNodeQueue.Count > 0 &&
                   this.State != ComputeState.Aborting)
            {
                u = this.mNodeQueue.Dequeue();
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
                    this.OnExamineEdge(e.mData, e.mSrcNode.Index, 
                        e.mDstNode.Index, reversed);

                    switch (v.Color)
                    {
                        case GraphColor.White:
                            this.OnTreeEdge(e.mData, e.mSrcNode.Index,
                                e.mDstNode.Index, reversed);
                            v.Color = GraphColor.Gray;
                            this.OnDiscoverNode(v.mData, v.Index);
                            this.mNodeQueue.Enqueue(v);
                            break;
                        case GraphColor.Gray:
                            // OnNonTreeEdge
                            // OnBackEdge
                            this.OnGrayEdge(e.mData, e.mSrcNode.Index,
                                e.mDstNode.Index, reversed);
                            break;
                        case GraphColor.Black:
                            // OnNonTreeEdge
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
}
