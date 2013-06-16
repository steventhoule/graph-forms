using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms
{
    /// <summary>
    /// This interface defines the basic template for a weighted edge linking
    /// two <typeparamref name="Node"/> instances in a directional graph.
    /// </summary>
    /// <typeparam name="Node">The type of nodes connected together by this
    /// weighted edge.</typeparam>
    public interface IGraphEdge<Node>
    {
        /// <summary>
        /// Source Node; Parent of Connection
        /// </summary>
        Node SrcNode { get; }
        /// <summary>
        /// Destination Node; Child of Connection
        /// </summary>
        Node DstNode { get; }
        /// <summary>
        /// Proportionality to other edges; 
        /// Ideal length of graphical edge
        /// </summary>
        float Weight { get; }
        /// <summary>
        /// Sets the Source Node (Parent of Connection).
        /// </summary>
        /// <param name="srcNode">New source node and
        /// parent of this connection.</param>
        void SetSrcNode(Node srcNode);
        /// <summary>
        /// Sets the Destination Node (Child of Connection).
        /// </summary>
        /// <param name="dstNode">New destination node and
        /// child of this connection.</param>
        void SetDstNode(Node dstNode);
    }
}
