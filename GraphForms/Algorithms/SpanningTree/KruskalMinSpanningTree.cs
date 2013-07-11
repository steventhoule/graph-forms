﻿using System;
using System.Collections.Generic;
using System.Text;
using GraphForms.Algorithms.Collections;

namespace GraphForms.Algorithms.SpanningTree
{
    public class KruskalMinSpanningTree<Node, Edge>
        : AAlgorithm, ISpanningTreeAlgorithm<Node, Edge>
        where Edge : IGraphEdge<Node>
    {
        private readonly Digraph<Node, Edge> mGraph;
        private Tree<int>[] mTrees;
        private Digraph<Node, Edge> mSpanningTree;

        public KruskalMinSpanningTree(Digraph<Node, Edge> graph)
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

            this.mTrees = new Tree<int>[nodes.Length];
            this.mSpanningTree = new Digraph<Node, Edge>(
                this.mGraph.NodeCount, this.mGraph.EdgeCount / 2);
            int i, srcIndex, dstIndex;
            for (i = 0; i < nodes.Length; i++)
            {
                nodes[i].Index = i;
                this.mTrees[i] = new Tree<int>(i);
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
