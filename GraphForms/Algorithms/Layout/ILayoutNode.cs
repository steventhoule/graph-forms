using System;
using System.Collections.Generic;
using System.Drawing;
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
        /// This layout node's bounding box in its local coordinate system.
        /// </summary>
        Box2F LayoutBBox { get; }

        /// <summary>
        /// The x-coordinate of this layout node's position 
        /// in its graph's coordinate system.
        /// </summary>
        float X { get; }
        /// <summary>
        /// The y-coordinate of this layout node's position
        /// in its graph's coordinate system.
        /// </summary>
        float Y { get; }

        /// <summary>
        /// Sets the position of this layout node
        /// in its graph's coordinate system.
        /// </summary>
        /// <param name="x">The new horizontal offset from the origin
        /// of the coordinate system of this layout node's graph.</param>
        /// <param name="y">The new vertical offset from the origin
        /// of the coordinate system of this layout node's graph.</param>
        void SetPosition(float x, float y);

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
        float NewX { get; }
        /// <summary>
        /// The temporary new Y-coordinate of the position of this node.
        /// </summary>
        float NewY { get; }

        /// <summary>
        /// Set the temporary new position of this node, which is then used
        /// by the layout algorithm to set the position of this node.
        /// </summary>
        /// <param name="newX">The temporary new X-coordinate.</param>
        /// <param name="newY">The temporary new Y-coordinate.</param>
        void SetNewPosition(float newX, float newY);
    }
}
