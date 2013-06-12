using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Path
{
    public class FloydWarshallAllShortestPaths<Node, Edge>
        : AAllShortestPaths<Node, Edge>
        where Node : class
        where Edge : class, IGraphEdge<Node>
    {
        private double[][] mDists;
        private int[][] mNextNodes;
        private Edge[][] mPathEdges;

        public FloydWarshallAllShortestPaths(Digraph<Node, Edge> graph,
            bool directed, bool reversed)
            : base(graph, directed, reversed)
        {
        }

        public override double[][] Distances
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
            int i, j, k;
            Digraph<Node, Edge>.GNode[] nodes
                = this.mGraph.InternalNodes;
            this.mDists = new double[nodes.Length][];
            this.mNextNodes = new int[nodes.Length][];
            this.mPathEdges = new Edge[nodes.Length][];
            for (i = 0; i < nodes.Length; i++)
            {
                nodes[i].Index = i;
                this.mDists[i] = new double[nodes.Length];
                this.mNextNodes[i] = new int[nodes.Length];
                this.mPathEdges[i] = new Edge[nodes.Length];
                for (j = 0; j < nodes.Length; j++)
                {
                    this.mDists[i][j] = double.MaxValue;
                    this.mNextNodes[i][j] = -1;
                }
                this.mDists[i][i] = 0;
            }
            Digraph<Node, Edge>.GEdge[] edges
                = this.mGraph.InternalEdges;
            // TODO: How should this algorithm handle self-loops?
            if (this.bUndirected || !this.bReversed)
            {
                for (i = 0; i < edges.Length; i++)
                {
                    j = edges[i].mSrcNode.Index;
                    k = edges[i].mDstNode.Index;
                    this.mDists[j][k] = edges[i].mData.Weight;
                    this.mPathEdges[j][k] = edges[i].mData;
                }
            }
            if (this.bUndirected || this.bReversed)
            {
                float weight;
                for (i = 0; i < edges.Length; i++)
                {
                    j = edges[i].mSrcNode.Index;
                    k = edges[i].mDstNode.Index;
                    weight = edges[i].mData.Weight;
                    if (weight < this.mDists[k][j])
                    {
                        this.mDists[k][j] = weight;
                        this.mPathEdges[k][j] = edges[i].mData;
                    }
                }
            }

            double dist;
            for (k = 0; k < nodes.Length; k++)
            {
                for (i = 0; i < nodes.Length; i++)
                {
                    for (j = 0; j < nodes.Length; j++)
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
