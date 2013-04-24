using System;
using System.Collections.Generic;
using System.Text;
using GraphForms.Algorithms.Search;

namespace GraphForms.Algorithms.SpanningTree
{
    public class BFSpanningTreeAlgorithm<Node, Edge> 
        : BreadthFirstSearchAlgorithm<Node, Edge>, 
          ISpanningTreeAlgorithm<Node, Edge>
        where Node : class
        where Edge : class, IGraphEdge<Node>
    {
        private DirectionalGraph<Node, Edge> mSpanningTree;

        public BFSpanningTreeAlgorithm(DirectionalGraph<Node, Edge> graph)
            : base(graph)
        {
            this.mSpanningTree = new DirectionalGraph<Node, Edge>(
                graph.NodeCount, graph.EdgeCount / 2);
            //this.mSpanningTree.AddNodeRange(graph.Nodes);
        }

        public BFSpanningTreeAlgorithm(DirectionalGraph<Node, Edge> graph,
            bool undirected, bool reversed)
            : base(graph, undirected, reversed)
        {
            this.mSpanningTree = new DirectionalGraph<Node, Edge>(
                graph.NodeCount, graph.EdgeCount / 2);
            //this.mSpanningTree.AddNodeRange(graph.Nodes);
        }

        public DirectionalGraph<Node, Edge> SpanningTree
        {
            get { return this.mSpanningTree; }
        }

        protected override void OnTreeEdge(Edge e, 
            int srcIndex, int dstIndex, bool reversed)
        {
            this.mSpanningTree.AddEdge(e);
            base.OnTreeEdge(e, srcIndex, dstIndex, reversed);
        }
    }
}
