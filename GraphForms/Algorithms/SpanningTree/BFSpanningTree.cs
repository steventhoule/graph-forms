using System;
using System.Collections.Generic;
using System.Text;
using GraphForms.Algorithms.Search;

namespace GraphForms.Algorithms.SpanningTree
{
    public class BFSpanningTree<Node, Edge> 
        : BreadthFirstSearch<Node, Edge>, 
          ISpanningTreeAlgorithm<Node, Edge>
        where Node : class
        where Edge : class, IGraphEdge<Node>
    {
        private Digraph<Node, Edge> mSpanningTree;

        public BFSpanningTree(Digraph<Node, Edge> graph)
            : base(graph)
        {
            this.mSpanningTree = new Digraph<Node, Edge>(
                graph.NodeCount, graph.EdgeCount / 2);
            //this.mSpanningTree.AddNodeRange(graph.Nodes);
        }

        public BFSpanningTree(Digraph<Node, Edge> graph,
            bool directed, bool reversed)
            : base(graph, directed, reversed)
        {
            this.mSpanningTree = new Digraph<Node, Edge>(
                graph.NodeCount, graph.EdgeCount / 2);
            //this.mSpanningTree.AddNodeRange(graph.Nodes);
        }

        public Digraph<Node, Edge> SpanningTree
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
