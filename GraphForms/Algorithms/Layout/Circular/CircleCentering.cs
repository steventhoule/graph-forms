using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Layout.Circular
{
    /// <summary>
    /// Used to specify which method a circular layout algorithm uses to
    /// calculate the initial position of the center of a circle of nodes
    /// or the root node of a circular tree of nodes.
    /// </summary>
    public enum CircleCentering
    {
        /// <summary>
        /// Use a center position that has already been predefined by
        /// parameters of the circular layout algorithm and/or by
        /// the position of the root node of the circular tree.
        /// </summary>
        Predefined,
        /// <summary>
        /// Calculate the center from the numeric averages of the
        /// coordinates of the positions of all the nodes in the graph
        /// being operated on by the circular layout algorithm.
        /// </summary>
        Centroid,
        /// <summary>
        /// Use the calculated center of the
        /// <see cref="P:ILayoutNode.LayoutBBox"/> of the
        /// <see cref="P:LayoutAlgorithm`2.ClusterNode"/>
        /// of the circular layout algorithm, or its
        /// <see cref="P:LayoutAlgorithm`2.BoundingBox"/>
        /// if it does not have a cluster node.
        /// </summary>
        BBoxCenter
    }
}
