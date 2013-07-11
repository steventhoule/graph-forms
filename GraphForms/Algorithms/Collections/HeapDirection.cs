using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Collections
{
    /// <summary>
    /// Specifies the order in which a prioritized queue will serve items.
    /// </summary>
    public enum HeapDirection
    {
        /// <summary>
        /// Items are served in Increasing order from least to greatest
        /// or lowest priority to highest priority.
        /// </summary>
        Increasing,
        /// <summary>
        /// Items are served in Decreasing order, from greatest to least
        /// or highest priority to lowest priority.
        /// </summary>
        Decreasing
    }
}
