using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Layout
{
    /// <summary>
    /// This interface helps define nodes used by layout algorithms.
    /// Its properties are critical for most layout algorithms.
    /// </summary>
    public interface ILayoutNode
    {
        /// <summary>
        /// Whether this node is fixed to its current position and thereby
        /// unaffected by any layout algorithm applied to it.
        /// </summary><remarks>
        /// This should be true for any nodes being dragged by the mouse when 
        /// the layout algorithm is running.
        /// </remarks>
        bool PositionFixed { get; }

        /// <summary>
        /// The temporary new X-coordinate of the position of this node.
        /// </summary>
        float NewX { get; set; }
        /// <summary>
        /// The temporary new Y-coordinate of the position of this node.
        /// </summary>
        float NewY { get; set; }
    }
}
