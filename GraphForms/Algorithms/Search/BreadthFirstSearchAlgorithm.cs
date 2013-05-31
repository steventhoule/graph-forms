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
        private Queue<DirectionalGraph<Node, Edge>.GraphNode> mNodeQueue;

        public BreadthFirstSearchAlgorithm(DirectionalGraph<Node, Edge> graph)
            : base(graph, true, false)
        {
            this.mNodeQueue = new Queue<DirectionalGraph<Node, 
                Edge>.GraphNode>(graph.NodeCount + 1);
        }

        public BreadthFirstSearchAlgorithm(DirectionalGraph<Node, Edge> graph,
            bool directed, bool reversed)
            : base(graph, directed, reversed)
        {
            this.mNodeQueue = new Queue<DirectionalGraph<Node, 
                Edge>.GraphNode>(graph.NodeCount + 1);
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

            DirectionalGraph<Node, Edge>.GraphNode node;

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
            DirectionalGraph<Node, Edge>.GraphNode[] nodes
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

        /*public void Visit(DirectionalGraph<Node, Edge>.GraphNode s)
        {
            this.EnqueueRoot(s);
            this.FlushVisitQueue();
        }/* */

        private void EnqueueRoot(DirectionalGraph<Node, Edge>.GraphNode s)
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
            DirectionalGraph<Node, Edge>.GraphNode u, v;
            DirectionalGraph<Node, Edge>.GraphEdge edge;
            DirectionalGraph<Node, Edge>.GraphEdge[] edges;

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
                    edge = edges[i];
                    v = edge.DstNode;
                    reversed = v.Equals(u);
                    if (reversed)
                        v = edge.SrcNode;
                    this.OnExamineEdge(edge.mData, edge.mSrcNode.Index, 
                        edge.mDstNode.Index, reversed);

                    switch (v.Color)
                    {
                        case GraphColor.White:
                            this.OnTreeEdge(edge.mData, edge.mSrcNode.Index,
                                edge.mDstNode.Index, reversed);
                            v.Color = GraphColor.Gray;
                            this.OnDiscoverNode(v.mData, v.Index);
                            this.mNodeQueue.Enqueue(v);
                            break;
                        case GraphColor.Gray:
                            // OnNonTreeEdge
                            // OnBackEdge
                            this.OnGrayEdge(edge.mData, edge.mSrcNode.Index,
                                edge.mDstNode.Index, reversed);
                            break;
                        case GraphColor.Black:
                            // OnNonTreeEdge
                            // OnForwardOrCrossEdge
                            this.OnBlackEdge(edge.mData, edge.mSrcNode.Index,
                                edge.mDstNode.Index, reversed);
                            break;
                    }
                }
                u.Color = GraphColor.Black;
                this.OnFinishNode(u.mData, u.Index);
            }
        }
    }
}
