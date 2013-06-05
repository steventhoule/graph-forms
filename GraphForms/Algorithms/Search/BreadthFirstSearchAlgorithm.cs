using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Search
{
    public class BreadthFirstSearchAlgorithm<Node, Edge>
        : AGraphTraversalAlgorithm<Node, Edge>
        where Node : class
        where Edge : class, IGraphEdge<Node>
    {
        private Queue<Digraph<Node, Edge>.GNode> mNodeQueue;

        public BreadthFirstSearchAlgorithm(Digraph<Node, Edge> graph)
            : base(graph, true, false)
        {
            this.mNodeQueue = new Queue<Digraph<Node, 
                Edge>.GNode>(graph.NodeCount + 1);
        }

        public BreadthFirstSearchAlgorithm(Digraph<Node, Edge> graph,
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
                    this.EnqueueRoot(node);
                    this.FlushVisitQueue();
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
                    this.EnqueueRoot(node);
                    this.FlushVisitQueue();
                }
            }
        }

        private void EnqueueRoot(Digraph<Node, Edge>.GNode s)
        {
            this.OnStartNode(s.mData, s.Index);

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
