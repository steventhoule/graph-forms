using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Search
{
    /// <summary>
    /// Enumeration of the major methods of traversing the nodes and edges
    /// in a <see cref="T:DirectionalGraph`2{Node,Edge}"/> for searching or
    /// other data processing.
    /// </summary>
    public enum SearchMethod
    {
        /// <summary>
        /// Traverse the graph breadth first.
        /// </summary>
        BFS,
        /// <summary>
        /// Traverse the graph depth first.
        /// </summary>
        DFS
    }
}
