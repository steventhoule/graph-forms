using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace GraphForms.Algorithms.Layout
{
    /// <summary>
    /// This interface represents a special type of node used by layout
    /// algorithms to define a node in a graph superstructure that encloses
    /// a sub-graph.  Its main functions are to help augment the positions
    /// of layout nodes and determine the positions of port nodes in its
    /// sub-graph.
    /// </summary>
    public interface IClusterNode : ILayoutNode
    {
        /// <summary>
        /// Calculates the position of an <see cref="IPortNode"/> node on 
        /// the outer boundary of this node's shape, based on its angle 
        /// around the center of this node's bounding box.
        /// </summary>
        /// <param name="angle">The angle of the line between a port node's
        /// position and the center point of this node's bounding box.
        /// </param>
        /// <returns>A point on the boundary of this node's shape in this
        /// node's local coordinate system that forms an angle of 
        /// <paramref name="angle"/> radians with the center point of this
        /// node's bounding box.</returns>
        PointF GetPortNodePos(double angle);

        /// <summary>
        /// Processes a potential new position of a layout node in this
        /// node's local coordinate system and returns it, possibly modified
        /// to fit constraints.
        /// </summary>
        /// <param name="x">The X-coordinate of a potential new position of
        /// a layout node in this cluster node's sub-graph.</param>
        /// <param name="y">The Y-coordinate of a potential new position of
        /// a layout node in this cluster node's sub-graph.</param>
        /// <returns>A point in this node's local coordinate system that
        /// will be the new position of a layout node, modified to fit any
        /// constraints of this cluster node, such as fitting within its
        /// shape.</returns><remarks>
        /// This is not only where the cluster node can adjust a node's 
        /// position to fit within its shape, but also where it can adjust 
        /// its own shape to fit the node's position and return the position
        /// unmodified.</remarks>
        PointF AugmentNodePos(float x, float y);
    }
}
