using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Path
{
    public class BellmanFordShortestPath<Node, Edge>
        : AShortestPath<Node, Edge>
        where Edge : IGraphEdge<Node>
    {
        private bool bHasNegativeCycle;

        public BellmanFordShortestPath(Digraph<Node, Edge> graph, 
            bool directed, bool reversed)
            : base(graph, directed, reversed)
        {
        }

        public bool HasNegativeCycle
        {
            get { return this.bHasNegativeCycle; }
        }

        protected override void ComputeFromRoot(int root)
        {
            int nodeCount = this.mGraph.NodeCount;
            Digraph<Node, Edge>.GEdge[] edges
                = this.mGraph.InternalEdges;
            int i, j, si, di;
            double dist;
            // Relax edges repeatedly
            bool hasTreeEdge = true;
            for (j = 1; j < nodeCount && hasTreeEdge; j++)
            {
                hasTreeEdge = false;
                if (this.bUndirected || !this.bReversed)
                {
                    for (i = 0; i < edges.Length; i++)
                    {
                        si = edges[i].mSrcNode.Index;
                        di = edges[i].mDstNode.Index;
                        dist = this.mDistances[si] + edges[i].mData.Weight;
                        if (dist < this.mDistances[di])
                        {
                            this.mDistances[di] = dist;
                            this.mPathNodes[di] = si;
                            this.mPathEdges[di] = edges[i].mData;
                            hasTreeEdge = true;
                        }
                    }
                }
                if (this.bUndirected || this.bReversed)
                {
                    for (i = 0; i < edges.Length; i++)
                    {
                        si = edges[i].mSrcNode.Index;
                        di = edges[i].mDstNode.Index;
                        dist = this.mDistances[di] + edges[i].mData.Weight;
                        if (dist < this.mDistances[si])
                        {
                            this.mDistances[si] = dist;
                            this.mPathNodes[si] = di;
                            this.mPathEdges[si] = edges[i].mData;
                            hasTreeEdge = true;
                        }
                    }
                }
            }
            // Check for negative-weight cycles
            this.bHasNegativeCycle = false;
            for (i = 0; i < edges.Length; i++)
            {
                si = edges[i].mSrcNode.Index;
                di = edges[i].mDstNode.Index;
                dist = this.mDistances[si] + edges[i].mData.Weight;
                if (dist < this.mDistances[di])
                {
                    this.bHasNegativeCycle = true;
                    break;
                }
            }
        }
    }
}
