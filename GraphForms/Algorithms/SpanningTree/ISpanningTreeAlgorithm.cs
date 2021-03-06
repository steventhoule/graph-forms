﻿using System;

namespace GraphForms.Algorithms.SpanningTree
{
    /// <summary>
    /// Algorithm classes that implement this interface are designed to
    /// generate a spanning tree from a connected graph, which is a 
    /// sub-graph of that graph that connects all its vertices together
    /// with a minimal subset of edges.
    /// </summary>
    /// <typeparam name="Node">The type of vertices in the spanning tree 
    /// generated by this algorithm.</typeparam>
    /// <typeparam name="Edge">The type of edges in the spanning tree 
    /// generated by this algorithm.</typeparam>
    /// <remarks>
    /// Algorithm classes that implement this interface should also be 
    /// designed to process non-connected graphs and return a graph
    /// containing multiple spanning trees, one for each (weakly) 
    /// connected component of the input graph.
    /// </remarks>
    public interface ISpanningTreeAlgorithm<Node, Edge> : IAlgorithm
        where Edge : IGraphEdge<Node>
    {
        /// <summary>
        /// A sub-graph of the original connected graph that connects all
        /// its vertices together with a minimal subset of its edges.
        /// </summary><remarks>
        /// If the original graph isn't connected, this graph should contain
        /// multiple spanning trees, one for each (weakly) connected
        /// component of the original graph.
        /// </remarks>
        Digraph<Node, Edge> SpanningTree { get; }

        Digraph<Node, Edge>.GEdge[] SpanningTreeEdges { get; }
    }
}
