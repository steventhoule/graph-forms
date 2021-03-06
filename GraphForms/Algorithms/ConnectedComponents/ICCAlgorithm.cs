﻿using System;

namespace GraphForms.Algorithms.ConnectedComponents
{
    /// <summary>
    /// This interface defines a template for algorithms that calculate
    /// connected components of a graph and return them as groups of nodes
    /// in that graph. The properties of the connected components can differ 
    /// between algorithms that implement this interface.
    /// </summary>
    /// <typeparam name="Node">The type of nodes in the graph that the
    /// connected component algorithm operates on.</typeparam>
    public interface ICCAlgorithm<Node, Edge> : IAlgorithm
        where Edge : IGraphEdge<Node>
    {
        int ComponentCount { get; }

        int[] ComponentIds { get; }

        /// <summary>
        /// The connected components of a graph, returned as a series of 
        /// <typeparamref name="Node"/> arrays containing the nodes in each 
        /// connected component.
        /// </summary>
        Digraph<Node, Edge>.GNode[][] Components { get; }

        /// <summary>
        /// The roots or starting points of each connected component,
        /// in the same order as <see cref="Components"/>.
        /// </summary>
        Digraph<Node, Edge>.GNode[] ComponentRoots { get; }
    }
}
