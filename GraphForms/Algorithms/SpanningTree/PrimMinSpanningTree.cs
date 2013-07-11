using System;
using System.Collections.Generic;
using System.Text;
using GraphForms.Algorithms.Collections;

namespace GraphForms.Algorithms.SpanningTree
{
    public class PrimMinSpanningTree<Node, Edge>
        : AAlgorithm, ISpanningTreeAlgorithm<Node, Edge>
        where Edge : IGraphEdge<Node>
    {
        private readonly Digraph<Node, Edge> mGraph;

        private FibonacciNode<float, int>.Heap mQueue;
        private FibonacciNode<float, int>[] mMinWeights;
        private Edge[] mMinEdges;
        private Digraph<Node, Edge> mSpanningTree;

        public PrimMinSpanningTree(Digraph<Node, Edge> graph)
        {
            this.mGraph = graph;
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

        protected override void InternalCompute()
        {
            Digraph<Node, Edge>.GNode[] nodes
                = this.mGraph.InternalNodes;

            this.mQueue = new FibonacciNode<float, int>.Heap();
            this.mMinWeights = new FibonacciNode<float, int>[nodes.Length];
            this.mMinEdges = new Edge[nodes.Length];
            this.mSpanningTree = new Digraph<Node, Edge>(
                this.mGraph.NodeCount, this.mGraph.EdgeCount / 2);
            int i;
            for (i = 0; i < nodes.Length; i++)
            {
                nodes[i].Index = i;
                this.mMinWeights[i] = this.mQueue.Enqueue(float.MaxValue, i);
                this.mSpanningTree.AddNode(nodes[i].mData);
            }

            Digraph<Node, Edge>.GEdge[] edges;
            int u, v;
            float weight;
            while (this.mQueue.Count > 0)
            {
                u = this.mQueue.Dequeue().Value;
                if (this.mMinWeights[u].Priority != float.MaxValue)
                    this.mSpanningTree.AddEdge(this.mMinEdges[u]);
                this.mQueue.ChangePriority(
                    this.mMinWeights[u], -float.MaxValue);

                edges = nodes[u].InternalDstEdges;
                for (i = 0; i < edges.Length; i++)
                {
                    v = edges[i].mDstNode.Index;
                    weight = edges[i].mData.Weight;
                    if (weight < this.mMinWeights[v].Priority)
                    {
                        this.mQueue.ChangePriority(
                            this.mMinWeights[v], weight);
                        this.mMinEdges[v] = edges[i].mData;
                    }
                }

                edges = nodes[u].InternalSrcEdges;
                for (i = 0; i < edges.Length; i++)
                {
                    v = edges[i].mSrcNode.Index;
                    weight = edges[i].mData.Weight;
                    if (weight < this.mMinWeights[v].Priority)
                    {
                        this.mQueue.ChangePriority(
                            this.mMinWeights[v], weight);
                        this.mMinEdges[v] = edges[i].mData;
                    }
                }
            }
        }
    }
}
