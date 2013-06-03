﻿using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.SpanningTree
{
    public interface ISpanningTreeAlgorithm<Node, Edge> : IAlgorithm
        where Node : class
        where Edge : class, IGraphEdge<Node>
    {
        Digraph<Node, Edge> SpanningTree { get; }
    }
}
