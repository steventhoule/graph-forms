using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Path
{
    public class BasicAllShortestPaths<Node, Edge>
        : AAllShortestPaths<Node, Edge>
        where Node : class
        where Edge : class, IGraphEdge<Node>
    {
        private AShortestPath<Node, Edge> mAlg;

        private double[][] mDistances;
        private int[][] mPathNodes;
        private Edge[][] mPathEdges;

        public BasicAllShortestPaths(AShortestPath<Node, Edge> alg)
            : base(alg.Graph, alg.Directed, alg.Reversed)
        {
            this.mAlg = alg;
        }

        public override double[][] Distances
        {
            get { return this.mDistances; }
        }

        public override int[][] PathNodes
        {
            get { return this.mPathNodes; }
        }

        public override Edge[][] PathEdges
        {
            get { return this.mPathEdges; }
        }

        protected override void InternalCompute()
        {
            Digraph<Node, Edge>.GNode[] nodes
                = this.mGraph.InternalNodes;
            this.mDistances = new double[nodes.Length][];
            this.mPathNodes = new int[nodes.Length][];
            this.mPathEdges = new Edge[nodes.Length][];
            for (int i = 0; i < nodes.Length; i++)
            {
                this.mAlg.Compute(nodes[i].mData);
                this.mDistances[i] = this.mAlg.Distances;
                this.mPathNodes[i] = this.mAlg.PathNodes;
                this.mPathEdges[i] = this.mAlg.PathEdges;
                this.mAlg.Reset();
            }
        }
    }
}
