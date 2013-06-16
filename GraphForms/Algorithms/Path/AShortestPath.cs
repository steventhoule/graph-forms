using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Path
{
    public abstract class AShortestPath<Node, Edge>
        : ARootedAlgorithm<Node>
        where Edge : IGraphEdge<Node>
    {
        protected readonly Digraph<Node, Edge> mGraph;
        protected readonly bool bUndirected;
        protected readonly bool bReversed;

        protected double[] mDistances;
        protected int[] mPathNodes;
        protected Edge[] mPathEdges;

        public AShortestPath(Digraph<Node, Edge> graph, 
            bool directed, bool reversed)
        {
            this.mGraph = graph;
            this.bUndirected = !directed;
            this.bReversed = reversed;
        }

        public Digraph<Node, Edge> Graph
        {
            get { return this.mGraph; }
        }

        public bool Directed
        {
            get { return !this.bUndirected; }
        }

        public bool Reversed
        {
            get { return this.bReversed; }
        }

        public double[] Distances
        {
            get { return this.mDistances; }
        }

        public int[] PathNodes
        {
            get { return this.mPathNodes; }
        }

        public Edge[] PathEdges
        {
            get { return this.mPathEdges; }
        }

        protected override void InternalCompute()
        {
            int i, root;
            if (this.HasRoot)
            {
                root = this.mGraph.IndexOfNode(this.TryGetRoot());
                if (root < 0)
                {
                    root = 0;
                }
            }
            else
            {
                root = 0;
            }
            Digraph<Node, Edge>.GNode[] nodes
                = this.mGraph.InternalNodes;
            this.mDistances = new double[nodes.Length];
            this.mPathNodes = new int[nodes.Length];
            this.mPathEdges = new Edge[nodes.Length];
            for (i = 0; i < nodes.Length; i++)
            {
                nodes[i].Index = i;
                nodes[i].Color = GraphColor.White;
                this.mDistances[i] = double.MaxValue;
                this.mPathNodes[i] = -1;
            }
            this.mDistances[root] = 0;

            Digraph<Node, Edge>.GEdge[] edges
                = this.mGraph.InternalEdges;
            int si, di;
            if (this.bUndirected || !this.bReversed)
            {
                for (i = 0; i < edges.Length; i++)
                {
                    si = edges[i].mSrcNode.Index;
                    di = edges[i].mDstNode.Index;
                    if (si == root)
                    {
                        this.mDistances[di] = edges[i].mData.Weight;
                        this.mPathEdges[di] = edges[i].mData;
                    }
                }
            }
            if (this.bUndirected || this.bReversed)
            {
                float weight;
                for (i = 0; i < edges.Length; i++)
                {
                    si = edges[i].mSrcNode.Index;
                    di = edges[i].mDstNode.Index;
                    weight = edges[i].mData.Weight;
                    if (di == root && weight < this.mDistances[si])
                    {
                        this.mDistances[si] = weight;
                        this.mPathEdges[si] = edges[i].mData;
                    }
                }
            }

            this.ComputeFromRoot(root);
        }

        protected abstract void ComputeFromRoot(int root);
    }
}
