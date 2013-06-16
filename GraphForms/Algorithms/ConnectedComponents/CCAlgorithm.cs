using System;
using System.Collections.Generic;
using System.Text;
using GraphForms.Algorithms.Search;

namespace GraphForms.Algorithms.ConnectedComponents
{
    public class CCAlgorithm<Node, Edge>
        : DepthFirstSearch<Node, Edge>, ICCAlgorithm<Node>
        where Edge : IGraphEdge<Node>
    {
        private List<Node> mRoots;
        private List<List<Node>> mComponents;

        public CCAlgorithm(Digraph<Node, Edge> graph)
            : this(graph, false, false)
        {
        }

        public CCAlgorithm(Digraph<Node, Edge> graph,
            bool directed, bool reversed)
            : base(graph, directed, reversed)
        {
            this.mComponents = new List<List<Node>>();
        }

        public Node[][] Components
        {
            get
            {
                Node[][] comps = new Node[this.mComponents.Count][];
                for (int i = 0; i < this.mComponents.Count; i++)
                    comps[i] = this.mComponents[i].ToArray();
                return comps;
            }
        }

        public Node[] Roots
        {
            get { return this.mRoots.ToArray(); }
        }

        protected override void OnStartNode(Node n, int index)
        {
            this.mRoots.Add(n);
            this.mComponents.Add(new List<Node>());
            base.OnStartNode(n, index);
        }

        protected override void OnDiscoverNode(Node n, int index)
        {
            this.mComponents[this.mComponents.Count - 1].Add(n);
            base.OnDiscoverNode(n, index);
        }
    }
}
