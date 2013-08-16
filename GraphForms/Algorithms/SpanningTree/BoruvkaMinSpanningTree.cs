using System;
using GraphForms.Algorithms.Collections;

namespace GraphForms.Algorithms.SpanningTree
{
    public class BoruvkaMinSpanningTree<Node, Edge>
        : AAlgorithm, ISpanningTreeAlgorithm<Node, Edge>
        where Edge : IGraphEdge<Node>
    {
        /*private class NodeData
        {
            public int CompId;
            public int NewId;

            public NodeData(int id)
            {
                this.CompId = id;
                this.NewId = -1;
            }
        }/* */
        private static readonly int[] sEmptyIds = new int[0];
        private static readonly Digraph<Node, Edge>.GEdge[] sEmptySTEs
            = new Digraph<Node, Edge>.GEdge[0];

        private readonly Digraph<Node, Edge> mGraph;
        //private NodeData[] mDatas;
        private int[] mCmpIds;
        //private int[] mNewIds;
        private Digraph<Node, Edge> mSpanningTree;

        private int mSTECount;
        private Digraph<Node, Edge>.GEdge[] mSpanTreeEdges;

        public BoruvkaMinSpanningTree(Digraph<Node, Edge> graph)
        {
            this.mGraph = graph;
            this.mCmpIds = sEmptyIds;
            //this.mNewIds = sEmptyIds;

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
            double weight;
            int i, j, compCount = this.mGraph.NodeCount;
            Digraph<Node, Edge>.GNode node;
            if (this.mCmpIds.Length < compCount)
            {
                this.mCmpIds = new int[compCount];
                //this.mNewIds = new int[compCount];
            }
            this.mSpanningTree 
                = new Digraph<Node,Edge>(compCount, compCount - 1);
            compCount = 0;
            for (i = this.mGraph.NodeCount - 1; i >= 0; i--)
            {
                //nodes[i].Index = i;
                node = this.mGraph.InternalNodeAt(i);
                if (!node.Hidden)
                {
                    this.mCmpIds[i] = compCount++;
                    //this.mNewIds[i] = -1;
                    this.mSpanningTree.AddNode(node.Data);
                }
            }
            if (compCount < 2 || this.mGraph.EdgeCount == 0)
            {
                // There are no visible nodes, 
                // so it's as if the graph is empty
                this.mSTECount = 0;
                return;
            }
            Digraph<Node, Edge>.GEdge edge;
            Digraph<Node, Edge>.GEdge[] edges
                = new Digraph<Node, Edge>.GEdge[this.mGraph.EdgeCount];
            int edgeCount = 0;
            for (i = this.mGraph.EdgeCount - 1; i >= 0; i--)
            {
                edge = this.mGraph.InternalEdgeAt(i);
                if (!edge.Hidden &&
                    !edge.SrcNode.Hidden && !edge.DstNode.Hidden)
                {
                    weight = edge.Data.Weight;
                    j = edgeCount - 1;
                    while (j >= 0 && edges[j].Data.Weight < weight)
                    {
                        j--;
                    }
                    j++;
                    Array.Copy(edges, j, edges, j + 1, edgeCount - j);
                    edges[j] = edge;
                    edgeCount++;
                }
            }
            if (edgeCount == 0)
            {
                // There are no visible edges,
                // so the spanning tree won't have any edges.
                this.mSTECount = 0;
                return;
            }
            // Sort the visible edges from lowest to highest weight
            //Array.Sort<Digraph<Node, Edge>.GEdge>(edges, 0, edgeCount,
            //    new EdgeWeightComparer<Node, Edge>(true));

            this.mSTECount = Math.Min(compCount - 1, edgeCount);
            if (this.mSpanTreeEdges.Length < this.mSTECount)
            {
                this.mSpanTreeEdges 
                    = new Digraph<Node, Edge>.GEdge[this.mSTECount];
            }
            this.mSTECount = 0;

            int k, si, di, comp, newComp;
            // TODO: Does checking edges.Length fully compensate for
            // a graph with multiple connected components?
            while (compCount > 1 && edgeCount > 0)
            {
                newComp = 0;
                for (i = 0; i < compCount; i++)
                {
                    edge = null;
                    si = -1;
                    di = -1;
                    for (j = edgeCount - 1; j >= 0; j--)
                    {
                        edge = edges[j];
                        si = edge.SrcNode.Index;
                        di = edge.DstNode.Index;
                        this.OnExamineEdge(edge);
                        if (this.mCmpIds[si] == i)//this.mDatas[si].CompId == i)
                        {
                            if (this.mCmpIds[di] == i)
                            {
                                edgeCount--;
                                Array.Copy(edges, j + 1, edges, j, edgeCount - j);
                                this.OnNonTreeEdge(edge);
                            }
                            else//if (this.mDatas[di].CompId != i)
                            {
                                if (this.mCmpIds[di] < i)
                                {
                                    comp = this.mCmpIds[si];
                                    for (k = this.mCmpIds.Length - 1;
                                         k >= 0; k--)
                                    {
                                        if (this.mCmpIds[k] == comp)
                                            this.mCmpIds[k] = this.mCmpIds[di];
                                    }
                                }
                                else
                                {
                                    comp = this.mCmpIds[si];
                                    if (comp != newComp)
                                    {
                                        for (k = this.mCmpIds.Length - 1;
                                             k >= 0; k--)
                                        {
                                            if (this.mCmpIds[k] == comp)
                                                this.mCmpIds[k] = newComp;
                                        }
                                    }
                                    comp = this.mCmpIds[di];
                                    if (comp != newComp)
                                    {
                                        for (k = this.mCmpIds.Length - 1;
                                             k >= 0; k--)
                                        {
                                            if (this.mCmpIds[k] == comp)
                                                this.mCmpIds[k] = newComp;
                                        }
                                    }
                                    newComp++;
                                }
                                edgeCount--;
                                Array.Copy(edges, j + 1, edges, j, edgeCount - j);
                                this.mSpanningTree.AddEdge(edge.Data);
                                this.mSpanTreeEdges[this.mSTECount++] = edge;
                                this.OnTreeEdge(edge);
                                break;
                            }
                        }
                        else if (this.mCmpIds[di] == i)//this.mDatas[di].CompId == i)
                        {
                            if (this.mCmpIds[si] == i)
                            {
                                edgeCount--;
                                Array.Copy(edges, j + 1, edges, j, edgeCount - j);
                                this.OnNonTreeEdge(edge);
                            }
                            else//if (this.mDatas[si].CompId != i)
                            {
                                if (this.mCmpIds[si] < i)
                                {
                                    comp = this.mCmpIds[di];
                                    for (k = this.mCmpIds.Length - 1;
                                         k >= 0; k--)
                                    {
                                        if (this.mCmpIds[k] == comp)
                                            this.mCmpIds[k] = this.mCmpIds[si];
                                    }
                                }
                                else
                                {
                                    comp = this.mCmpIds[si];
                                    if (comp != newComp)
                                    {
                                        for (k = this.mCmpIds.Length - 1;
                                             k >= 0; k--)
                                        {
                                            if (this.mCmpIds[k] == comp)
                                                this.mCmpIds[k] = newComp;
                                        }
                                    }
                                    comp = this.mCmpIds[di];
                                    if (comp != newComp)
                                    {
                                        for (k = this.mCmpIds.Length - 1; 
                                             k >= 0; k--)
                                        {
                                            if (this.mCmpIds[k] == comp)
                                                this.mCmpIds[k] = newComp;
                                        }
                                    }
                                    newComp++;
                                }
                                edgeCount--;
                                Array.Copy(edges, j + 1, edges, j, edgeCount - j);
                                this.mSpanningTree.AddEdge(edge.Data);
                                this.mSpanTreeEdges[this.mSTECount++] = edge;
                                this.OnTreeEdge(edge);
                                break;
                            }
                        }
                    }
                    /*if (j >= 0)
                    {
                        if (this.mNewIds[si] == -1 && this.mNewIds[di] == -1)
                            //this.mDatas[si].NewId == -1 &&
                            //this.mDatas[di].NewId == -1)
                        {
                            //this.mDatas[si].NewId = newCompCount;
                            //this.mDatas[di].NewId = newCompCount;
                            this.mNewIds[si] = newCompCount;
                            this.mNewIds[di] = newCompCount;
                            newCompCount++;
                            edgeCount--;
                            Array.Copy(edges, j + 1, edges, j, edgeCount - j);
                            this.mSpanningTree.AddEdge(edge.Data);
                            //this.mSpanTreeEdges[this.mSTECount++] = edge;
                            this.OnTreeEdge(edge.Data, si, di);
                        }
                        else if (this.mNewIds[si] == -1)//this.mDatas[si].NewId == -1)
                        {
                            //this.mDatas[si].NewId = this.mDatas[di].NewId;
                            this.mNewIds[si] = this.mNewIds[di];
                            edgeCount--;
                            Array.Copy(edges, j + 1, edges, j, edgeCount - j);
                            this.mSpanningTree.AddEdge(edge.Data);
                            //this.mSpanTreeEdges[this.mSTECount++] = edge;
                            this.OnTreeEdge(edge.Data, si, di);
                        }
                        else if (this.mNewIds[di] == -1)//this.mDatas[di].NewId == -1)
                        {
                            //this.mDatas[di].NewId = this.mDatas[si].NewId;
                            this.mNewIds[di] = this.mNewIds[si];
                            edgeCount--;
                            Array.Copy(edges, j + 1, edges, j, edgeCount - j);
                            this.mSpanningTree.AddEdge(edge.Data);
                            //this.mSpanTreeEdges[this.mSTECount++] = edge;
                            this.OnTreeEdge(edge.Data, si, di);
                        }
                    }/* */
                }
                /*for (i = 0; i < this.mDatas.Length; i++)
                {
                    this.mDatas[i].CompId = this.mDatas[i].NewId;
                    this.mDatas[i].NewId = -1;
                }/* */
                /*for (i = this.mCmpIds.Length - 1; i >= 0; i--)
                {
                    this.mCmpIds[i] = this.mNewIds[i];
                    this.mNewIds[i] = -1;
                }/* */
                compCount = newComp;
            }
        }
    }
}
