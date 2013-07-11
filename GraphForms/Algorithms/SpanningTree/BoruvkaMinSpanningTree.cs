using System;
using System.Collections.Generic;
using System.Text;
using GraphForms.Algorithms.Collections;

namespace GraphForms.Algorithms.SpanningTree
{
    public class BoruvkaMinSpanningTree<Node, Edge>
        : AAlgorithm, ISpanningTreeAlgorithm<Node, Edge>
        where Edge : IGraphEdge<Node>
    {
        private class NodeData
        {
            public int CompId;
            public int NewId;

            public NodeData(int id)
            {
                this.CompId = id;
                this.NewId = -1;
            }
        }

        private readonly Digraph<Node, Edge> mGraph;
        private NodeData[] mDatas;
        private Digraph<Node, Edge> mSpanningTree;

        public BoruvkaMinSpanningTree(Digraph<Node, Edge> graph)
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
                new EdgeWeightComparer<Node, Edge>());

            this.mDatas = new NodeData[nodes.Length];
            this.mSpanningTree = new Digraph<Node,Edge>(
                nodes.Length, edges.Length / 2);
            int i, compCount = nodes.Length;
            for (i = 0; i < compCount; i++)
            {
                nodes[i].Index = i;
                this.mDatas[i] = new NodeData(i);
                this.mSpanningTree.AddNode(nodes[i].mData);
            }

            Digraph<Node, Edge>.GEdge edge;
            int j, si, di, newCompCount;
            int edgeCount = edges.Length;
            bool[] removeEdge = new bool[edgeCount];
            // TODO: Does checking edges.Length fully compensate for
            // a graph with multiple weakly connected components?
            while (compCount > 1 && edgeCount > 0)
            {
                newCompCount = 0;
                for (i = 0; i < edgeCount; i++)
                {
                    removeEdge[i] = false;
                }
                for (i = 0; i < compCount; i++)
                {
                    edge = null;
                    si = -1;
                    di = -1;
                    for (j = 0; j < edgeCount; j++)
                    {
                        edge = edges[j];
                        si = edge.mSrcNode.Index;
                        di = edge.mDstNode.Index;
                        this.OnExamineEdge(edge.mData, si, di);
                        if (this.mDatas[si].CompId == i)
                        {
                            removeEdge[j] = true;
                            if (this.mDatas[di].CompId != i)
                                break;
                        }
                        else if (this.mDatas[di].CompId == i)
                        {
                            removeEdge[j] = true;
                            if (this.mDatas[si].CompId != i)
                                break;
                        }
                    }
                    if (j < edgeCount)
                    {
                        if (this.mDatas[si].NewId == -1 &&
                            this.mDatas[di].NewId == -1)
                        {
                            this.mDatas[si].NewId = newCompCount;
                            this.mDatas[di].NewId = newCompCount;
                            newCompCount++;
                        }
                        else if (this.mDatas[si].NewId == -1)
                        {
                            this.mDatas[si].NewId = this.mDatas[di].NewId;
                        }
                        else if (this.mDatas[di].NewId == -1)
                        {
                            this.mDatas[di].NewId = this.mDatas[si].NewId;
                        }
                        this.mSpanningTree.AddEdge(edge.mData);
                        this.OnTreeEdge(edge.mData, si, di);
                    }
                }
                for (j = edgeCount - 1; j >= 0; j--)
                {
                    if (removeEdge[j])
                    {
                        edgeCount--;
                        Array.Copy(edges, j + 1, edges, j, edgeCount - j);
                    }
                }
                for (i = 0; i < this.mDatas.Length; i++)
                {
                    this.mDatas[i].CompId = this.mDatas[i].NewId;
                    this.mDatas[i].NewId = -1;
                }
                compCount = newCompCount;
            }
        }
    }
}
