using System;

namespace GraphForms.Algorithms.Path
{
    public class FloydWarshallAllShortestPaths<Node, Edge>
        : AAllShortestPaths<Node, Edge>
        where Edge : IGraphEdge<Node>
    {
        private float[][] mDists;
        private int[][] mNextNodes;
        private Edge[][] mPathEdges;

        public FloydWarshallAllShortestPaths(Digraph<Node, Edge> graph,
            bool directed, bool reversed)
            : base(graph, directed, reversed)
        {
        }

        public override float[][] Distances
        {
            get { return this.mDists; }
        }

        public override int[][] PathNodes
        {
            get { return this.mNextNodes; }
        }

        public override Edge[][] PathEdges
        {
            get { return this.mPathEdges; }
        }

        protected override void InternalCompute()
        {
            int count = this.mGraph.NodeCount;
            int i, j, k, vCount = 0;
            Digraph<Node, Edge>.GNode node;
            this.mDists = new float[count][];
            this.mNextNodes = new int[count][];
            this.mPathEdges = new Edge[count][];
            for (i = 0; i < count; i++)
            {
                node = this.mGraph.InternalNodeAt(i);
                //nodes[i].Index = i;
                if (!node.Hidden)
                    vCount++;
                this.mDists[i] = new float[count];
                this.mNextNodes[i] = new int[count];
                this.mPathEdges[i] = new Edge[count];
                for (j = 0; j < count; j++)
                {
                    this.mDists[i][j] = float.MaxValue;
                    this.mNextNodes[i][j] = -1;
                }
                this.mDists[i][i] = 0;
            }
            if (vCount == 0 || this.mGraph.EdgeCount == 0)
            {
                // There are no visible nodes, 
                // so it's as if the graph is empty
                return;
            }
            Digraph<Node, Edge>.GEdge edge;
            vCount = this.mGraph.EdgeCount;
            // TODO: How should this algorithm handle self-loops?
            if (this.bUndirected || !this.bReversed)
            {
                for (i = 0; i < vCount; i++)
                {
                    edge = this.mGraph.InternalEdgeAt(i);
                    // TODO: Also check if edge.SrcNode.Hidden?
                    if (edge.Hidden || edge.DstNode.Hidden)
                        continue;
                    j = edge.SrcNode.Index;
                    k = edge.DstNode.Index;
                    this.mDists[j][k] = edge.Data.Weight;
                    this.mPathEdges[j][k] = edge.Data;
                }
            }
            if (this.bUndirected || this.bReversed)
            {
                float weight;
                for (i = 0; i < vCount; i++)
                {
                    edge = this.mGraph.InternalEdgeAt(i);
                    // TODO: Also check if edge.DstNode.Hidden?
                    if (edge.Hidden || edge.SrcNode.Hidden)
                        continue;
                    j = edge.SrcNode.Index;
                    k = edge.DstNode.Index;
                    weight = edge.Data.Weight;
                    if (weight < this.mDists[k][j])
                    {
                        this.mDists[k][j] = weight;
                        this.mPathEdges[k][j] = edge.Data;
                    }
                }
            }

            float dist;
            for (k = 0; k < count; k++)
            {
                for (i = 0; i < count; i++)
                {
                    for (j = 0; j < count; j++)
                    {
                        dist = this.mDists[i][k] + this.mDists[k][j];
                        if (dist < this.mDists[i][j])
                        {
                            this.mDists[i][j] = dist;
                            this.mNextNodes[i][j] = k;
                        }
                    }
                }
            }
        }
    }
}
