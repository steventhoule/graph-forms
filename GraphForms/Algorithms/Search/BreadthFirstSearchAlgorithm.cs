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
            : base(graph)
        {
            this.mNodeQueue = new Queue<DirectionalGraph<Node, 
                Edge>.GraphNode>(graph.NodeCount + 1);
        }

        public BreadthFirstSearchAlgorithm(DirectionalGraph<Node, Edge> graph,
            bool undirected, bool reversed)
            : base(graph, undirected, reversed)
        {
            this.mNodeQueue = new Queue<DirectionalGraph<Node, 
                Edge>.GraphNode>(graph.NodeCount + 1);
        }

        #region Events
        protected virtual void OnExamineNode(Node n, int index)
        {
        }

        protected virtual void OnNonTreeEdge(Edge e, 
            int srcIndex, int dstIndex, bool reversed)
        {
        }

        protected virtual void OnGrayTarget(Edge e, 
            int srcIndex, int dstIndex, bool reversed)
        {
        }

        protected virtual void OnBlackTarget(Edge e, 
            int srcIndex, int dstIndex, bool reversed)
        {
        }
        #endregion

        protected override void InternalCompute()
        {
            if (this.mGraph.NodeCount == 0 || this.mGraph.EdgeCount == 0)
                return;

            this.Initialize();

            bool hasRoot = this.HasRoot;
            if (hasRoot)
            {
                // equeue select root only
                int index = this.mGraph.IndexOfNode(this.TryGetRoot());
                hasRoot = index >= 0;
                if (hasRoot)
                    this.Visit(this.mGraph.InternalNodeAt(index));
            }
            if (!hasRoot)
            {
                DirectionalGraph<Node, Edge>.GraphNode node;
                DirectionalGraph<Node, Edge>.GraphNode[] nodes
                    = this.mGraph.InternalNodes;
                for (int i = 0; i < nodes.Length; i++)
                {
                    node = nodes[i];
                    if (node.SrcEdgeCount == 0 && node.DstEdgeCount > 0)
                        this.EnqueueRoot(node);
                }
                this.FlushVisitQueue();
            }
        }

        public void Visit(DirectionalGraph<Node, Edge>.GraphNode s)
        {
            this.EnqueueRoot(s);
            this.FlushVisitQueue();
        }

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

                    if (v.Color == GraphColor.White)
                    {
                        this.OnTreeEdge(edge.mData, edge.mSrcNode.Index,
                            edge.mDstNode.Index, reversed);
                        v.Color = GraphColor.Gray;
                        this.OnDiscoverNode(v.mData, v.Index);
                        this.mNodeQueue.Enqueue(v);
                    }
                    else
                    {
                        this.OnNonTreeEdge(edge.mData, edge.mSrcNode.Index,
                            edge.mDstNode.Index, reversed);
                        if (v.Color == GraphColor.Gray)
                        {
                            this.OnGrayTarget(edge.Data, edge.mSrcNode.Index,
                                edge.mDstNode.Index, reversed);
                        }
                        else
                        {
                            this.OnBlackTarget(edge.Data, edge.mSrcNode.Index,
                                edge.mDstNode.Index, reversed);
                        }
                    }
                }
                u.Color = GraphColor.Black;
                this.OnFinishNode(u.mData, u.Index);
            }
        }
    }
}
