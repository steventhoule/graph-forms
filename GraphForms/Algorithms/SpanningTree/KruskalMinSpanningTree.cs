using System;
using GraphForms.Algorithms.Collections;

namespace GraphForms.Algorithms.SpanningTree
{
    public class KruskalMinSpanningTree<Node, Edge>
        : AAlgorithm, ISpanningTreeAlgorithm<Node, Edge>
        where Edge : IGraphEdge<Node>
    {
        private static readonly Digraph<Node, Edge>.GEdge[] sEmptySTEs
            = new Digraph<Node, Edge>.GEdge[0];

        private readonly Digraph<Node, Edge> mGraph;
        private Tree<int>[] mTrees;
        private Digraph<Node, Edge> mSpanningTree;

        private int mSTECount;
        private Digraph<Node, Edge>.GEdge[] mSpanTreeEdges;

        public KruskalMinSpanningTree(Digraph<Node, Edge> graph)
        {
            this.mGraph = graph;

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

        private bool AreInSameSet(int i1, int i2)
        {
            Tree<int> t1 = this.mTrees[i1].FindSet();
            Tree<int> t2 = this.mTrees[i2].FindSet();
            return t1.Value == t2.Value;
        }

        private bool Union(int i1, int i2)
        {
            return Tree<int>.Union(this.mTrees[i1], this.mTrees[i2]);
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
            double weight;
            int i, src, dst, count = this.mGraph.NodeCount;
            Digraph<Node, Edge>.GNode node;
            this.mTrees = new Tree<int>[count];
            this.mSpanningTree = new Digraph<Node, Edge>(count, count - 1);
            count = 0;
            for (i = this.mGraph.NodeCount - 1; i >= 0; i--)
            {
                //nodes[i].Index = i;
                node = this.mGraph.InternalNodeAt(i);
                if (!node.Hidden)
                {
                    count++;
                    this.mTrees[i] = new Tree<int>(i);
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
            Digraph<Node, Edge>.GEdge edge;
            Digraph<Node, Edge>.GEdge[] edges
                = new Digraph<Node, Edge>.GEdge[this.mGraph.EdgeCount];
            count = 0;
            for (i = this.mGraph.EdgeCount - 1; i >= 0; i--)
            {
                edge = this.mGraph.InternalEdgeAt(i);
                if (!edge.Hidden &&
                    !edge.SrcNode.Hidden && !edge.DstNode.Hidden)
                {
                    weight = edge.Data.Weight;
                    src = count - 1;
                    while (src >= 0 && edges[src].Data.Weight < weight)
                    {
                        src--;
                    }
                    src++;
                    Array.Copy(edges, src, edges, src + 1, count - src);
                    edges[src] = edge;
                    count++;
                }
            }
            if (count == 0)
            {
                // There are no visible edges,
                // so the spanning tree won't have any edges.
                this.mSTECount = 0;
                return;
            }
            //Array.Sort<Digraph<Node, Edge>.GEdge>(edges, 0, count, 
            //    new EdgeWeightComparer<Node, Edge>(true));

            this.mSTECount = Math.Min(this.mSTECount, count);
            if (this.mSpanTreeEdges.Length < this.mSTECount)
            {
                this.mSpanTreeEdges 
                    = new Digraph<Node, Edge>.GEdge[this.mSTECount];
            }
            this.mSTECount = 0;

            for (i = count - 1; i >= 0; i--)
            {
                edge = edges[i];
                src = edge.SrcNode.Index;
                dst = edge.DstNode.Index;
                this.OnExamineEdge(edge);
                if (AreInSameSet(src, dst))
                {
                    this.OnNonTreeEdge(edge);
                }
                else
                {
                    this.Union(src, dst);
                    this.mSpanningTree.AddEdge(edge.Data);
                    this.mSpanTreeEdges[this.mSTECount++] = edge;
                    this.OnTreeEdge(edge);
                }
            }
        }
    }
}
