using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Collections
{
    /// <summary>
    /// Specifies the order in which a Prioritized Heap will dequeue items.
    /// </summary>
    public enum HeapDirection
    {
        /// <summary>
        /// Items are dequeued in Increasing order from least to greatest.
        /// </summary>
        Increasing,
        /// <summary>
        /// Items are dequeued in Decreasing order, from greatest to least.
        /// </summary>
        Decreasing
    }
}
