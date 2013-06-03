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
        /// Initializes a copy of this edge, with the given class,
        /// source node and destination node, but with the same 
        /// <see cref="Weight"/> and whatever other internal data
        /// can be applied when this instance is cast to the given
        /// <typeparamref name="Edge"/> type.
        /// </summary>
        /// <typeparam name="Edge">The class to which a copy of this edge is cast.
        /// </typeparam>
        /// <param name="srcNode">The <see cref="SrcNode"/> of the copy.</param>
        /// <param name="dstNode">The <see cref="DstNode"/> of the copy.</param>
        /// <returns>A copy of this edge with the same internal data,
        /// but with the given source and destination nodes.</returns>
        /// <seealso cref="M:Digraph`2{Node,Edge}.Replace(Node,Node)"/>
        Edge Copy<Edge>(Node srcNode, Node dstNode) where Edge : class, IGraphEdge<Node>;
    }
}
