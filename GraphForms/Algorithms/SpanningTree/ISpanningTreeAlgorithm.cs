using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.SpanningTree
{
    public interface ISpanningTreeAlgorithm<Node, Edge>
        where Node : class
        where Edge : class, IGraphEdge<Node>
    {
        DirectionalGraph<Node, Edge> SpanningTree { get; }
    }
}
