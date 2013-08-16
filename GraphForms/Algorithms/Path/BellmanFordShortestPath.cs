using System;

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
            Digraph<Node, Edge>.GEdge edge;
            int i, j, si, di, edgeCount = this.mGraph.EdgeCount;
            float dist;
            // Relax edges repeatedly
            bool hasTreeEdge = true;
            for (j = 1; j < nodeCount && hasTreeEdge; j++)
            {
                hasTreeEdge = false;
                if (this.bUndirected || !this.bReversed)
                {
                    for (i = 0; i < edgeCount; i++)
                    {
                        edge = this.mGraph.InternalEdgeAt(i);
                        // TODO: Also check if edge.SrcNode.Hidden?
                        if (edge.Hidden || edge.DstNode.Hidden)
                            continue;
                        si = edge.SrcNode.Index;
                        di = edge.DstNode.Index;
                        dist = this.mDistances[si] + edge.Data.Weight;
                        if (dist < this.mDistances[di])
                        {
                            this.mDistances[di] = dist;
                            this.mPathNodes[di] = si;
                            this.mPathEdges[di] = edge.Data;
                            hasTreeEdge = true;
                        }
                    }
                }
                if (this.bUndirected || this.bReversed)
                {
                    for (i = 0; i < edgeCount; i++)
                    {
                        edge = this.mGraph.InternalEdgeAt(i);
                        // TODO: Also check if edge.DstNode.Hidden?
                        if (edge.Hidden || edge.SrcNode.Hidden)
                            continue;
                        si = edge.SrcNode.Index;
                        di = edge.DstNode.Index;
                        dist = this.mDistances[di] + edge.Data.Weight;
                        if (dist < this.mDistances[si])
                        {
                            this.mDistances[si] = dist;
                            this.mPathNodes[si] = di;
                            this.mPathEdges[si] = edge.Data;
                            hasTreeEdge = true;
                        }
                    }
                }
            }
            // Check for negative-weight cycles
            this.bHasNegativeCycle = false;
            for (i = 0; i < edgeCount; i++)
            {
                edge = this.mGraph.InternalEdgeAt(i);
                if (edge.Hidden || 
                    edge.SrcNode.Hidden || edge.DstNode.Hidden)
                    continue;
                si = edge.SrcNode.Index;
                di = edge.DstNode.Index;
                dist = this.mDistances[si] + edge.Data.Weight;
                if (dist < this.mDistances[di])
                {
                    this.bHasNegativeCycle = true;
                    break;
                }
            }
        }
    }
}
