using System;
using GraphForms.Algorithms.Search;

namespace GraphForms.Algorithms.ConnectedComponents
{
    public class CCAlgorithm<Node, Edge>
        : DepthFirstSearch<Node, Edge>, ICCAlgorithm<Node, Edge>
        where Edge : IGraphEdge<Node>
    {
        private int mCompCount;
        private int[] mCompIds;
        private Digraph<Node, Edge>.GNode[] mRoots;

        public CCAlgorithm(Digraph<Node, Edge> graph,
            bool directed, bool reversed)
            : base(graph, directed, reversed)
        {
            this.mCompCount = 0;
            this.mCompIds = new int[0];
            this.mRoots = new Digraph<Node, Edge>.GNode[0];
        }

        public int ComponentCount
        {
            get { return this.mCompCount; }
        }

        public int[] ComponentIds
        {
            get
            {
                int count = this.mGraph.NodeCount;
                int[] compIds = new int[count];
                Array.Copy(this.mCompIds, 0, compIds, 0, count);
                return compIds;
            }
        }

        public Digraph<Node, Edge>.GNode[][] Components
        {
            get
            {
                int i, j, count, nCount = this.mGraph.NodeCount;
                Digraph<Node, Edge>.GNode[] comp
                    = new Digraph<Node, Edge>.GNode[nCount];
                Digraph<Node, Edge>.GNode[][] comps
                    = new Digraph<Node,Edge>.GNode[this.mCompCount][];
                for (i = 0; i < this.mCompCount; i++)
                {
                    count = 0;
                    for (j = 0; j < nCount; j++)
                    {
                        if (this.mCompIds[j] == i)
                        {
                            comp[count++] = this.mGraph.InternalNodeAt(j);
                        }
                    }
                    comps[i] = new Digraph<Node, Edge>.GNode[count];
                    Array.Copy(comp, 0, comps[i], 0, count);
                }
                return comps;
            }
        }

        public Digraph<Node, Edge>.GNode[] Roots
        {
            get 
            {
                Digraph<Node, Edge>.GNode[] roots
                    = new Digraph<Node, Edge>.GNode[this.mCompCount];
                Array.Copy(this.mRoots, 0, roots, 0, this.mCompCount);
                return roots;
            }
        }

        public override void Initialize()
        {
            int count = this.mGraph.NodeCount;
            if (this.mCompIds.Length < count)
            {
                this.mCompIds = new int[count];
            }
            for (int i = 0; i < count; i++)
            {
                this.mCompIds[i] = -1;
            }
            this.mCompCount = 0;
            base.Initialize();
        }

        protected override void OnStartNode(Digraph<Node, Edge>.GNode n)
        {
            if (this.mCompCount == this.mRoots.Length)
            {
                Digraph<Node, Edge>.GNode[] roots;
                if (this.mCompCount == 0)
                {
                    roots = new Digraph<Node, Edge>.GNode[4];
                }
                else
                {
                    roots = new Digraph<Node, Edge>.GNode[2 * this.mCompCount];
                    Array.Copy(this.mRoots, 0, roots, 0, this.mCompCount);
                }
                this.mRoots = roots;
            }
            this.mRoots[this.mCompCount++] = n;
        }

        protected override void OnDiscoverNode(
            Digraph<Node, Edge>.GNode n, uint depth)
        {
            this.mCompIds[n.Index] = this.mCompCount - 1;
        }
    }
}
