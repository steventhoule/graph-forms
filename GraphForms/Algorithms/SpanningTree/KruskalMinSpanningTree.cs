using System;
using System.Collections.Generic;
using System.Text;
using GraphForms.Algorithms.Collections;

namespace GraphForms.Algorithms.SpanningTree
{
    public class KruskalMinSpanningTree<Node, Edge>
        : AAlgorithm, ISpanningTreeAlgorithm<Node, Edge>
        where Node : class
        where Edge : class, IGraphEdge<Node>
    {
        private readonly Digraph<Node, Edge> mGraph;
        private Tree<Node>[] mTrees;
        private Digraph<Node, Edge> mSpanningTree;

        public KruskalMinSpanningTree(Digraph<Node, Edge> graph)
        {
            this.mGraph = graph;
        }

        public Digraph<Node, Edge> SpanningTree
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
            Digraph<Node, Edge>.GNode[] nodes
                = this.mGraph.InternalNodes;
            Digraph<Node, Edge>.GEdge[] edges
                = this.mGraph.InternalEdges;
            Array.Sort<Digraph<Node, Edge>.GEdge>(edges,
                new EdgeWeightComparer<Node, Edge>(true));

            this.mTrees = new Tree<Node>[nodes.Length];
            this.mSpanningTree = new Digraph<Node, Edge>(
                this.mGraph.NodeCount, this.mGraph.EdgeCount / 2);
            int i, srcIndex, dstIndex;
            for (i = 0; i < nodes.Length; i++)
            {
                nodes[i].Index = i;
                this.mTrees[i] = new Tree<Node>(nodes[i].mData);
                this.mSpanningTree.AddNode(nodes[i].mData);
            }
            Digraph<Node, Edge>.GEdge edge;
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
