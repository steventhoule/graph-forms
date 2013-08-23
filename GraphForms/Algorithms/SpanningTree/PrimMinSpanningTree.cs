using System;
using GraphForms.Algorithms.Collections;

namespace GraphForms.Algorithms.SpanningTree
{
    public class PrimMinSpanningTree<Node, Edge>
        : AAlgorithm, ISpanningTreeAlgorithm<Node, Edge>
        where Edge : IGraphEdge<Node>
    {
        private static readonly Digraph<Node, Edge>.GEdge[] sEmptySTEs
            = new Digraph<Node, Edge>.GEdge[0];

        private readonly Digraph<Node, Edge> mGraph;

        private FibonacciNode<float, int>.Heap mQueue;
        private FibonacciNode<float, int>[] mMinWeights;
        private Digraph<Node, Edge>.GEdge[] mMinEdges;
        private Digraph<Node, Edge> mSpanningTree;

        private int mSTECount;
        private Digraph<Node, Edge>.GEdge[] mSpanTreeEdges;

        public PrimMinSpanningTree(Digraph<Node, Edge> graph)
        {
            this.mGraph = graph;
            this.mQueue = new FibonacciNode<float, int>.Heap();
            this.mMinWeights = new FibonacciNode<float, int>[0];
            this.mMinEdges = new Digraph<Node, Edge>.GEdge[0];

            this.mSTECount = 0;
            this.mSpanTreeEdges = sEmptySTEs;
        }
        /// <summary>
        /// A sub-graph of the original connected graph that connects all
        /// its vertices together with a minimal subset of its edges.
        /// </summary><remarks>
        /// If the original graph isn't connected, this graph will contain
        /// multiple spanning trees, one for each weakly connected
        /// component of the original graph.
        /// </remarks>
        public Digraph<Node, Edge> SpanningTree
        {
            get { return this.mSpanningTree; }
        }

        public Digraph<Node, Edge>.GEdge[] SpanningTreeEdges
        {
            get
            {
                Digraph<Node, Edge>.GEdge[] stEdges
                    = new Digraph<Node, Edge>.GEdge[this.mSTECount];
                if (this.mSTECount > 0)
                {
                    Array.Copy(this.mSpanTreeEdges, 0,
                        stEdges, 0, this.mSTECount);
                }
                return stEdges;
            }
        }

        protected virtual void OnExamineEdge(Digraph<Node, Edge>.GEdge e)
        {
        }

        protected virtual void OnTreeEdge(Digraph<Node, Edge>.GEdge e)
        {
        }

        protected virtual void OnNonTreeEdge(Digraph<Node, Edge>.GEdge e)
        {
        }

        protected override void InternalCompute()
        {
            int i, count = this.mGraph.NodeCount;
            Digraph<Node, Edge>.GNode node;
            if (this.mMinWeights.Length < count)
            {
                this.mMinWeights = new FibonacciNode<float, int>[count];
                this.mMinEdges = new Digraph<Node, Edge>.GEdge[count];
            }
            this.mSpanningTree = new Digraph<Node, Edge>(count, count - 1);
            count = 0;
            for (i = this.mGraph.NodeCount - 1; i >= 0; i--)
            {
                //nodes[i].Index = i;
                node = this.mGraph.InternalNodeAt(i);
                if (!node.Hidden)
                {
                    count++;
                    this.mMinWeights[i] 
                        = this.mQueue.Enqueue(float.MaxValue, i);
                    this.mMinEdges[i] = null;
                    this.mSpanningTree.AddNode(node.Data);
                }
            }
            if (count < 2 || this.mGraph.EdgeCount == 0)
            {
                // There are no visible nodes, 
                // so it's as if the graph is empty
                this.mSTECount = 0;
                return;
            }
            this.mSTECount = count - 1;
            count = this.mGraph.EdgeCount;

            this.mSTECount = Math.Min(this.mSTECount, count);
            if (this.mSpanTreeEdges.Length < this.mSTECount)
            {
                this.mSpanTreeEdges
                    = new Digraph<Node, Edge>.GEdge[this.mSTECount];
            }
            this.mSTECount = 0;

            int u, v;
            float weight;
            Digraph<Node, Edge>.GEdge edge, e;
            while (this.mQueue.Count > 0)
            {
                u = this.mQueue.Dequeue().Value;
                if (this.mMinWeights[u].Priority != float.MaxValue)
                {
                    edge = this.mMinEdges[u];
                    this.mSpanningTree.AddEdge(edge.Data);
                    this.mSpanTreeEdges[this.mSTECount++] = edge;
                    this.OnTreeEdge(edge);
                }
                this.mQueue.ChangePriority(
                    this.mMinWeights[u], -float.MaxValue);

                //node = this.mGraph.InternalNodeAt(u);
                for (i = 0; i < count; i++)
                {
                    edge = this.mGraph.InternalEdgeAt(i);
                    //if (!edge.Hidden)
                    {
                        if (edge.SrcNode.Index == u)//node.Index)
                        {
                            this.OnExamineEdge(edge);
                            v = edge.DstNode.Index;
                            if (v == u)
                            {
                                this.OnNonTreeEdge(edge);
                            }
                            else if (!edge.DstNode.Hidden)
                            {
                                weight = edge.Data.Weight;
                                if (weight < this.mMinWeights[v].Priority)
                                {
                                    this.mQueue.ChangePriority(
                                        this.mMinWeights[v], weight);
                                    if (this.mMinEdges[v] != null)
                                    {
                                        e = this.mMinEdges[v];
                                        this.OnNonTreeEdge(e);
                                    }
                                    this.mMinEdges[v] = edge;
                                }
                            }
                        }
                        else if (edge.DstNode.Index == u)//node.Index)
                        {
                            this.OnExamineEdge(edge);
                            v = edge.SrcNode.Index;
                            if (!edge.SrcNode.Hidden)
                            {
                                weight = edge.Data.Weight;
                                if (weight < this.mMinWeights[v].Priority)
                                {
                                    this.mQueue.ChangePriority(
                                        this.mMinWeights[v], weight);
                                    if (this.mMinEdges[v] != null)
                                    {
                                        e = this.mMinEdges[v];
                                        this.OnNonTreeEdge(e);
                                    }
                                    this.mMinEdges[v] = edge;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
