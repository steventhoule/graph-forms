using System;

namespace GraphForms.Algorithms.Path
{
    public class BasicAllShortestPaths<Node, Edge>
        : AAllShortestPaths<Node, Edge>
        where Edge : IGraphEdge<Node>
    {
        private AShortestPath<Node, Edge> mAlg;

        private float[][] mDistances;
        private int[][] mPathNodes;
        private Edge[][] mPathEdges;

        public BasicAllShortestPaths(AShortestPath<Node, Edge> alg)
            : base(alg.Graph, alg.Directed, alg.Reversed)
        {
            this.mAlg = alg;
        }

        public override float[][] Distances
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
            Digraph<Node, Edge>.GNode node;
            int i, j, count = this.mGraph.NodeCount;
            this.mDistances = new float[count][];
            this.mPathNodes = new int[count][];
            this.mPathEdges = new Edge[count][];
            for (i = 0; i < count; i++)
            {
                node = this.mGraph.InternalNodeAt(i);
                if (node.Hidden)
                {
                    this.mDistances[i] = new float[count];
                    this.mPathNodes[i] = new int[count];
                    this.mPathEdges[i] = new Edge[count];
                    for (j = 0; j < count; j++)
                    {
                        this.mDistances[i][j] = float.MaxValue;
                        this.mPathNodes[i][j] = -1;
                    }
                }
                else
                {
                    this.mAlg.Reset();
                    this.mAlg.ClearRoots();
                    this.mAlg.AddRoot(i);
                    this.mAlg.Compute();
                    this.mDistances[i] = this.mAlg.Distances;
                    this.mPathNodes[i] = this.mAlg.PathNodes;
                    this.mPathEdges[i] = this.mAlg.PathEdges;
                }
            }
        }
    }
}
