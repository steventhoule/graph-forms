using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Layout
{
    /// <summary>
    /// A enumeration of possible directions in which layout
    /// nodes are arranged by certain layout algorithms.
    /// </summary>
    public enum LayoutDirection
    {
        /// <summary>
        /// Arranged from the Left side of the layout 
        /// algorithm's boundary to its Right side.
        /// </summary>
        LeftToRight = 0,
        /// <summary>
        /// Arranged from the Top of the layout
        /// algorithm's boundary to its Bottom.
        /// </summary>
        TopToBottom = 1,
        /// <summary>
        /// Arranged from the Right side of the layout
        /// algorithm's boundary to its Left side.
        /// </summary>
        RightToLeft = 2,
        /// <summary>
        /// Arranged from the Bottom of the layout
        /// algorithm's boundary to its Top.
        /// </summary>
        BottomToTop = 3
    }
}
