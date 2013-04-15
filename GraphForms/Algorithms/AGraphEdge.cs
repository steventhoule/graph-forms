using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms
{
    /// <summary>
    /// An abstract implementation of the <see cref="T:IGraphEdge`1{Node}"/>
    /// interface that defines a weighted edge with read-only properties
    /// linking two <typeparamref name="Node"/> instances in a directional
    /// graph.</summary>
    /// <typeparam name="Node"></typeparam>
    public abstract class AGraphEdge<Node> : IGraphEdge<Node>
    {
        private Node mSrcNode;
        private Node mDstNode;
        private float mWeight;

        /// <summary>
        /// Source Node; Parent of Connection
        /// </summary>
        public Node SrcNode
        {
            get { return this.mSrcNode; }
        }

        /// <summary>
        /// Destination Node; Child of Connection
        /// </summary>
        public Node DstNode
        {
            get { return this.mDstNode; }
        }

        /// <summary>
        /// Proportionality to other edges; 
        /// Ideal length of graphical edge
        /// </summary>
        public float Weight
        {
            get { return this.mWeight; }
        }

        /// <summary>
        /// Initializes a new <see cref="T:AGraphEdge`1{Node}"/> instance with the
        /// given source and destination nodes and a <see cref="Weight"/> of 1.
        /// </summary>
        /// <param name="srcNode">The source node of this edge.</param>
        /// <param name="dstNode">The destination node of this edge.</param>
        public AGraphEdge(Node srcNode, Node dstNode)
        {
            this.mSrcNode = srcNode;
            this.mDstNode = dstNode;
            this.mWeight = 1;
        }

        /// <summary>
        /// Initializes a new <see cref="T:AGraphEdge`1{Node}"/> instance with the
        /// given source and destination nodes and the given weight.
        /// </summary>
        /// <param name="srcNode">The source node of this edge.</param>
        /// <param name="dstNode">The destination node of this edge.</param>
        /// <param name="weight">The proportionality of this edge's
        /// theoretical length to that of other edges.</param>
        public AGraphEdge(Node srcNode, Node dstNode, float weight)
        {
            this.mSrcNode = srcNode;
            this.mDstNode = dstNode;
            this.mWeight = weight;
        }

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
        /// <seealso cref="M:DirectionalGraph`2{Node,Edge}.Replace(Node,Node)"/>
        public abstract Edge Copy<Edge>(Node srcNode, Node dstNode)
            where Edge : class, IGraphEdge<Node>;
    }
}
