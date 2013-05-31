using System;
using System.Collections.Generic;
using System.Text;
using GraphForms.Algorithms.Collections;

namespace GraphForms.Algorithms.SpanningTree
{
    public class BoruvkaMinSpanningTreeAlgorithm<Node, Edge>
        : AAlgorithm, ISpanningTreeAlgorithm<Node, Edge>
        where Node : class
        where Edge : class, IGraphEdge<Node>
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

        private readonly DirectionalGraph<Node, Edge> mGraph;
        private NodeData[] mDatas;
        private DirectionalGraph<Node, Edge> mSpanningTree;

        public BoruvkaMinSpanningTreeAlgorithm(
            DirectionalGraph<Node, Edge> graph)
        {
            this.mGraph = graph;
        }

        public DirectionalGraph<Node, Edge> SpanningTree
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
            DirectionalGraph<Node, Edge>.GraphNode[] nodes
                = this.mGraph.InternalNodes;
            DirectionalGraph<Node, Edge>.GraphEdge[] edges
                = this.mGraph.InternalEdges;
            Array.Sort<DirectionalGraph<Node, Edge>.GraphEdge>(edges,
                new EdgeWeightComparer<Node, Edge>());

            this.mDatas = new NodeData[nodes.Length];
            this.mSpanningTree = new DirectionalGraph<Node,Edge>(
                this.mGraph.NodeCount, this.mGraph.EdgeCount / 2);
            int i, compCount = nodes.Length;
            for (i = 0; i < compCount; i++)
            {
                nodes[i].Index = i;
                this.mDatas[i] = new NodeData(i);
                this.mSpanningTree.AddNode(nodes[i].mData);
            }

            DirectionalGraph<Node, Edge>.GraphEdge edge;
            int j, si, di, newCompCount;
            List<int> remEdges;
            // TODO: Does checking edges.Length fully compensate for
            // a graph with multiple connected components?
            while (compCount > 1 && edges.Length > 0)
            {
                newCompCount = 0;
                remEdges = new List<int>(edges.Length);
                for (i = 0; i < compCount; i++)
                {
                    edge = null;
                    si = -1;
                    di = -1;
                    for (j = 0; j < edges.Length; j++)
                    {
                        edge = edges[j];
                        si = edge.mSrcNode.Index;
                        di = edge.mDstNode.Index;
                        this.OnExamineEdge(edge.mData, si, di);
                        if (this.mDatas[si].CompId == i)
                        {
                            remEdges.Add(j);
                            if (this.mDatas[di].CompId != i)
                                break;
                        }
                        else if (this.mDatas[di].CompId == i)
                        {
                            remEdges.Add(j);
                            if (this.mDatas[si].CompId != i)
                                break;
                        }
                    }
                    if (j < edges.Length)
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
                si = edges.Length;
                for (j = remEdges.Count - 1; j >= 0; j--)
                {
                    si--;
                    Array.Copy(edges, remEdges[j] + 1,
                        edges, remEdges[j], si - remEdges[j]);
                }
                DirectionalGraph<Node, Edge>.GraphEdge[] newEdges
                    = new DirectionalGraph<Node, Edge>.GraphEdge[si];
                Array.Copy(edges, 0, newEdges, 0, si);
                edges = newEdges;
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
