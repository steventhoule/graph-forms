using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms
{
    /// <summary>
    /// Enumeration of the different states of visitation of a node or edge 
    /// in a graph being traversed by an algorithm for searching or other 
    /// data processing.
    /// </summary>
    public enum GraphColor
    {
        /// <summary>
        /// The node/edge has not yet been visited by the algorithm.
        /// </summary>
        White,
        /// <summary>
        /// The node/edge is currently being visited by the algorithm.
        /// </summary>
        Gray,
        /// <summary>
        /// The node/edge has already been visited by the algorithm.
        /// </summary>
        Black
    }
}
