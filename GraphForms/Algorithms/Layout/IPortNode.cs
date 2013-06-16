using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Layout
{
    /// <summary><para>
    /// This interface represents a special type of virtual node used by
    /// layout algorithms to convey positioning information between
    /// sub-graphs and their higher graph superstructure.
    /// </para><para>
    /// Beware that some layout algorithms will treat this port nodes
    /// differently than they treat <see cref="ILayoutNode"/> nodes.
    /// </para></summary>
    /// <remarks><para>
    /// Instances of classes implementing this interface are typically
    /// created when a graph is divided into sub-graphs enclosed within
    /// "cluster" nodes in the graph superstructure.
    /// </para><para>
    /// Port nodes essentially represent a node that is outside its sub-graph
    /// and connected to one or more nodes in its sub-graph, which could be
    /// either a node in another sub-graph or a "cluster" node enclosing
    /// another sub-graph in the graph superstructure.
    /// </para></remarks>
    public interface IPortNode : ILayoutNode, ISpecialNode
    {
        /// <summary>
        /// The unique identifier for the node in the graph superstructure
        /// that this port node represents, or which contains the node
        /// that this port node represents.
        /// </summary>
        /// <remarks>
        /// This identifier is used by some layout algorithms to make sure
        /// that port nodes affiliated with the same cluster node and edge
        /// in the graph superstructure are kept close together in their
        /// sub-graph instead of possibly ending up on opposite sides of it.
        /// </remarks>
        int ClusterId { get; }

        /// <summary>
        /// The minimum allowable angle of the position of this port node
        /// around the center of the bounding box of the cluster node
        /// that encloses the sub-graph of this port node.
        /// </summary>
        double MinAngle { get; set; }

        /// <summary>
        /// The maximum allowable angle of the position of this port node
        /// around the center of the bounding box of the cluster node
        /// that encloses the sub-graph of this port node.
        /// </summary>
        double MaxAngle { get; set; }
    }
}
