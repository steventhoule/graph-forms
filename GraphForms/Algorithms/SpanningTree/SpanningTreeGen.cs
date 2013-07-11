using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.SpanningTree
{
    /// <summary>
    /// Used by algorithms, particularly tree-based graph layout algorithms,
    /// to allow the user to specify which method is used to generate 
    /// a sparsely connected spanning tree from a graph.
    /// </summary>
    public enum SpanningTreeGen
    {
        /// <summary>
        /// Generate the tree with a breadth first traversal of the graph.
        /// </summary>
        BFS,
        /// <summary>
        /// Generate the tree with a depth first traversal of the graph.
        /// </summary>
        DFS,
        /// <summary>
        /// Generate the tree with a 
        /// <see cref="T:BoruvkaMinSpanningTree`2{Node,Edge}"/> algorithm.
        /// </summary>
        Boruvka,
        /// <summary>
        /// Generate the tree with a 
        /// <see cref="T:KruskalMinSpanningTree`2{Node,Edge}"/> algorithm.
        /// </summary>
        Kruskal,
        /// <summary>
        /// Generate the tree with a 
        /// <see cref="T:PrimMinSpanningTree`2{Node,Edge}"/> algorithm.
        /// </summary>
        Prim
    }
}
