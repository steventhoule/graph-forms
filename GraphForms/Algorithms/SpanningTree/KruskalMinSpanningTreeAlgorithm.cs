using System;
using System.Collections.Generic;
using System.Text;
using GraphForms.Algorithms.Collections;

namespace GraphForms.Algorithms.SpanningTree
{
    public class KruskalMinSpanningTreeAlgorithm<Node, Edge>
        : AAlgorithm, ISpanningTreeAlgorithm<Node, Edge>
        where Node : class
        where Edge : class, IGraphEdge<Node>
    {
        protected readonly DirectionalGraph<Node, Edge> mGraph;
        private Tree<Node>[] mTrees;
        private DirectionalGraph<Node, Edge> mSpanningTree;

        public KruskalMinSpanningTreeAlgorithm(
            DirectionalGraph<Node, Edge> graph)
        {
            this.mGraph = graph;
        }

        public DirectionalGraph<Node, Edge> SpanningTree
        {
            get { return this.mSpanningTree; }
        }

        private bool AreInSameSet(int i1, int i2)
        {
            Tree<Node> t1 = this.mTrees[i1].FindSet();
            Tree<Node> t2 = this.mTrees[i2].FindSet();
            return t1.Value == t2.Value;
        }

        private bool Union(int i1, int i2)
        {
            return Tree<Node>.Union(this.mTrees[i1], this.mTrees[i2]);
        }

        protected virtual void OnExamineEdge(Edge e,
            int srcIndex, int dstIndex)
        {
        }

        protected virtual void OnTreeEdge(Edge e,
            int srcIndex, int dstIndex)
        {
        }

        protected override void InternalCompute()
        {
            DirectionalGraph<Node, Edge>.GraphNode[] nodes
                = this.mGraph.InternalNodes;
            DirectionalGraph<Node, Edge>.GraphEdge[] edges
                = this.mGraph.InternalEdges;
            Array.Sort<DirectionalGraph<Node, Edge>.GraphEdge>(edges,
                new EdgeWeightComparer<Node, Edge>(true));

            this.mTrees = new Tree<Node>[nodes.Length];
            this.mSpanningTree = new DirectionalGraph<Node, Edge>(
                this.mGraph.NodeCount, this.mGraph.EdgeCount / 2);
            int i, srcIndex, dstIndex;
            for (i = 0; i < nodes.Length; i++)
            {
                nodes[i].Index = i;
                this.mTrees[i] = new Tree<Node>(nodes[i].mData);
                this.mSpanningTree.AddNode(nodes[i].mData);
            }
            DirectionalGraph<Node, Edge>.GraphEdge edge;
            for (i = edges.Length - 1; i >= 0; i--)
            {
                edge = edges[i];
                srcIndex = edge.mSrcNode.Index;
                dstIndex = edge.mDstNode.Index;
                this.OnExamineEdge(edge.mData, srcIndex, dstIndex);
                if (!AreInSameSet(srcIndex, dstIndex))
                {
                    this.Union(srcIndex, dstIndex);
                    this.mSpanningTree.AddEdge(edge.mData);
                    this.OnTreeEdge(edge.mData, srcIndex, dstIndex);
                }
            }
        }
    }
}
