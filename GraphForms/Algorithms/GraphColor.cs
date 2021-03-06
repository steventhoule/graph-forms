﻿using System;

namespace GraphForms.Algorithms
{
    /// <summary>
    /// Enumeration of the different states of visitation of a node or edge 
    /// in a graph being traversed by an algorithm for searching or other 
    /// data processing.
    /// </summary>
    public enum GraphColor : byte
    {
        /// <summary>
        /// The node/edge has not yet been visited by the algorithm.
        /// </summary>
        White = 0x00,
        /// <summary>
        /// The node/edge is currently being visited by the algorithm.
        /// </summary>
        Gray  = 0x01,
        /// <summary>
        /// The node/edge has already been visited by the algorithm.
        /// </summary>
        Black = 0x02
    }
}
