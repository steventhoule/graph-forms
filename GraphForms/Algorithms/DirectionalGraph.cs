﻿using System;
using System.Collections.Generic;

namespace GraphForms.Algorithms
{
    /// <summary>
    /// A container for a list of <typeparamref name="Node"/> instances 
    /// connected by one-way <typeparamref name="Edge"/> instances, along with
    /// algorithms for traversing those <typeparamref name="Node"/> instances 
    /// along with <typeparamref name="Edge"/> instances.
    /// </summary>
    /// <typeparam name="Node">The type of vertices contained in this 
    /// directional graph.</typeparam>
    /// <typeparam name="Edge">The type of edges connecting the 
    /// vertices contained in this directional graph.</typeparam>
    public class DirectionalGraph<Node, Edge>
        where Node : class
        where Edge : class, IGraphEdge<Node>
    {
        /// <summary>
        /// This class is used to store an <typeparamref name="Edge"/> instance
        /// along with <see cref="GraphNode"/> instances that store the edge's
        /// <see cref="P:IGraphEdge`1{Node}.SrcNode"/> and 
        /// <see cref="P:IGraphEdge`1{Node}.DstNode"/> in their respective
        /// <see cref="GraphNode.Data"/>.
        /// </summary>
        public class GraphEdge
        {
            /// <summary>
            /// The source node of this edge; its parent
            /// </summary>
            internal GraphNode mSrcNode;
            /// <summary>
            /// The destination node of this edge; its child
            /// </summary>
            internal GraphNode mDstNode;
            /// <summary>
            /// The underlying data that this edge represents.
            /// </summary>
            internal Edge mData;
            /// <summary>
            /// Marks the state of visitation of this <see cref="GraphEdge"/>
            /// instance when it is being traversed by a search algorithm or
            /// some other manner of data processing.
            /// </summary>
            public GraphColor Color;

            /// <summary>
            /// Initializes a new <see cref="GraphEdge"/> instance with the 
            /// given source and destination <see cref="GraphNode"/> instances
            /// and the given <typeparamref name="Edge"/> instance.
            /// </summary>
            /// <param name="srcNode">The source node.</param>
            /// <param name="dstNode">The destination node.</param>
            /// <param name="data">The underlying edge data.</param>
            /// <remarks>
            /// The <see cref="GraphNode.Data"/> of <paramref name="srcNode"/>
            /// and <paramref name="dstNode"/> must match the respective 
            /// <see cref="P:IGraphEdge{Node}`1.SrcNode"/> and 
            /// <see cref="P:IGraphEdge`1{Node}.DstNode"/> of 
            /// <paramref name="data"/>.</remarks>
            internal GraphEdge(GraphNode srcNode, GraphNode dstNode, Edge data)
            {
                this.mSrcNode = srcNode;
                this.mDstNode = dstNode;
                this.mData = data;
            }
            /// <summary>
            /// The underlying <typeparamref name="Edge"/> instance that this
            /// <see cref="GraphEdge"/> instance represents.
            /// </summary>
            public Edge Data
            {
                get { return this.mData; }
            }
            /// <summary>
            /// The <see cref="GraphNode"/> instance that stores the
            /// <see cref="P:IGraphEdge`1{Node}.SrcNode"/> of 
            /// <see cref="Data"/>.
            /// </summary>
            public GraphNode SrcNode
            {
                get { return this.mSrcNode; }
            }
            /// <summary>
            /// The <see cref="GraphNode"/> instance that stores the
            /// <see cref="P:IGraphEdge`1{Node}.DstNode"/> of 
            /// <see cref="Data"/>.
            /// </summary>
            public GraphNode DstNode
            {
                get { return this.mDstNode; }
            }
        }

        private static int IndexOfSrc(List<GraphEdge> nodes, Node data)
        {
            int count = nodes.Count;
            for (int i = 0; i < count; i++)
            {
                if (nodes[i].mSrcNode.mData == data)
                    return i;
            }
            return -1;
        }

        private static int IndexOfDst(List<GraphEdge> nodes, Node data)
        {
            int count = nodes.Count;
            for (int i = 0; i < count; i++)
            {
                if (nodes[i].mDstNode.mData == data)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// This class is used internally to store instances of the
        /// <typeparamref name="Node"/> class along with additional data
        /// used for traversing its containing 
        /// <see cref="T:DirectionalGraph`2{Node,Edge}"/> instance.
        /// </summary>
        public class GraphNode : IEquatable<GraphNode>
        {
            internal DirectionalGraph<Node, Edge> mGraph;
            /// <summary>
            /// The underlying data that this node represents.
            /// </summary>
            internal Node mData;

            /// <summary>
            /// Temporary storage for this node's index in its containing
            /// directional graph's internal list of 
            /// <see cref="P:DirectionalGraph`2{Node,Edge}.InternalNodes"/>.
            /// </summary>
            public int Index;
            /// <summary>
            /// Marks the state of visitation of this <see cref="GraphNode"/>
            /// instance when it is being traversed by a search algorithm or
            /// some other manner of data processing.
            /// </summary>
            public GraphColor Color;
            /// <summary>
            /// Flags whether or not this <see cref="GraphNode"/> has already
            /// been visited by a graph traversal algorithm.
            /// </summary>
            public bool Visited;
            /// <summary>
            /// Temporary storage for this node's theoretical distance from a
            /// starting root <see cref="GraphNode"/> of a traversal algorithm
            /// based on the sum of the <see cref="P:IGraphEdge`1{Node}.Weight"/>s
            /// along the (usually shortest) path connecting them.
            /// </summary>
            public float Distance;

            internal List<GraphEdge> mSrcEdges = new List<GraphEdge>();
            internal List<GraphEdge> mDstEdges = new List<GraphEdge>();

            /// <summary>
            /// Initializes a new <see cref="GraphNode"/> instance contained
            /// within the given <paramref name="graph"/> and representing
            /// the given <paramref name="data"/>.
            /// </summary>
            /// <param name="graph">The graph that contains this node.</param>
            /// <param name="data">The data that this node represents.</param>
            internal GraphNode(DirectionalGraph<Node, Edge> graph, Node data)
            {
                this.mGraph = graph;
                this.mData = data;
            }

            #region Properties

            /// <summary>
            /// The <see cref="T:DirectionalGraph`2{Node,Edge}"/> instance within
            /// which this <see cref="GraphNode"/> instance was created.
            /// </summary>
            public DirectionalGraph<Node, Edge> Graph
            {
                get { return this.mGraph; }
            }

            /// <summary>
            /// The underlying <typeparamref name="Node"/> data that this
            /// <see cref="GraphNode"/> instance represents.
            /// </summary>
            public Node Data
            {
                get { return this.mData; }
            }

            #region Source Traversal
            /// <summary>
            /// The current number of <typeparamref name="Edge"/> instances
            /// that have this <see cref="GraphNode"/>'s <see cref="Data"/>
            /// as their <see cref="P:IGraphEdge`1{Node}.DstNode"/>.
            /// </summary>
            public int SrcEdgeCount
            {
                get { return this.mSrcEdges.Count; }
            }

            /// <summary>
            /// An array of all <typeparamref name="Edge"/> instances that
            /// have this <see cref="GraphNode"/>'s <see cref="Data"/> as
            /// their <see cref="P:IGraphNode`1{Node}.DstNode"/>.
            /// </summary>
            public Edge[] SrcEdges
            {
                get
                {
                    Edge[] srcEdges = new Edge[this.mSrcEdges.Count];
                    for (int i = 0; i < this.mSrcEdges.Count; i++)
                        srcEdges[i] = this.mSrcEdges[i].mData;
                    return srcEdges;
                }
            }

            /// <summary>
            /// An array of all <see cref="GraphEdge"/> instances that
            /// has this <see cref="GraphNode"/> as their
            /// <see cref="P:GraphEdge.DstNode"/>.
            /// </summary>
            public GraphEdge[] InternalSrcEdges
            {
                get { return this.mSrcEdges.ToArray(); }
            }

            /// <summary>
            /// An array of all <see cref="GraphNode"/> instances connected
            /// to this <see cref="GraphNode"/> instance by an <typeparamref 
            /// name="Edge"/> instance with this node as its destination.
            /// </summary>
            public GraphNode[] SrcNodes
            {
                get
                {
                    GraphNode[] srcNodes = new GraphNode[this.mSrcEdges.Count];
                    for (int i = 0; i < this.mSrcEdges.Count; i++)
                        srcNodes[i] = this.mSrcEdges[i].mSrcNode;
                    return srcNodes;
                }
            }
            #endregion

            #region Destination Traversal
            /// <summary>
            /// The current number of <typeparamref name="Edge"/> instances
            /// that have this <see cref="GraphNode"/>'s <see cref="Data"/>
            /// as their <see cref="P:IGraphEdge`1{Node}.SrcNode"/>.
            /// </summary>
            public int DstEdgeCount
            {
                get { return this.mDstEdges.Count; }
            }

            /// <summary>
            /// An array of all <typeparamref name="Edge"/> instances that
            /// have this <see cref="GraphNode"/>'s <see cref="Data"/> as
            /// their <see cref="P:IGraphNode`1{Node}.SrcNode"/>.
            /// </summary>
            public Edge[] DstEdges
            {
                get
                {
                    Edge[] dstEdges = new Edge[this.mDstEdges.Count];
                    for (int i = 0; i < this.mDstEdges.Count; i++)
                        dstEdges[i] = this.mDstEdges[i].mData;
                    return dstEdges;
                }
            }

            /// <summary>
            /// An array of all <see cref="GraphEdge"/> instances that
            /// has this <see cref="GraphNode"/> as their
            /// <see cref="P:GraphEdge.SrcNode"/>.
            /// </summary>
            public GraphEdge[] InternalDstEdges
            {
                get { return this.mDstEdges.ToArray(); }
            }

            /// <summary>
            /// An array of all <see cref="GraphNode"/> instances connected
            /// to this <see cref="GraphNode"/> instance by an <typeparamref 
            /// name="Edge"/> instance with this node as its source.
            /// </summary>
            public GraphNode[] DstNodes
            {
                get
                {
                    GraphNode[] dstNodes = new GraphNode[this.mDstEdges.Count];
                    for (int i = 0; i < this.mDstEdges.Count; i++)
                        dstNodes[i] = this.mDstEdges[i].mDstNode;
                    return dstNodes;
                }
            }
            #endregion

            #region Undirected Traversal
            /// <summary>
            /// A combined array of both <see cref="SrcEdges"/> and
            /// <see cref="DstEdges"/>, ordered by the given 
            /// <paramref name="srcFirst"/> flag.
            /// </summary>
            /// <param name="srcFirst">Whether <see cref="SrcEdges"/> comes 
            /// before <see cref="DstEdges"/> in the returned array.</param>
            /// <returns>A union of the <see cref="SrcEdges"/> and 
            /// <see cref="DstEdges"/> arrays.</returns>
            public Edge[] AllEdges(bool srcFirst)
            {
                int i;
                int srcCount = this.mSrcEdges.Count;
                int dstCount = this.mDstEdges.Count;
                Edge[] edges = new Edge[srcCount + dstCount];
                if (srcFirst)
                {
                    for (i = 0; i < srcCount; i++)
                        edges[i] = this.mSrcEdges[i].mData;
                    for (i = 0; i < dstCount; i++)
                        edges[i + srcCount] = this.mDstEdges[i].mData;
                }
                else
                {
                    for (i = 0; i < dstCount; i++)
                        edges[i] = this.mDstEdges[i].mData;
                    for (i = 0; i < srcCount; i++)
                        edges[i + dstCount] = this.mSrcEdges[i].mData;
                }
                return edges;
            }

            /// <summary>
            /// A combined array of both <see cref="InternalSrcEdges"/> and
            /// <see cref="InternalDstEdges"/>, ordered by the given
            /// <paramref name="srcFirst"/> flag.
            /// </summary>
            /// <param name="srcFirst">Whether <see cref="InternalSrcEdges"/>
            /// comes before <see cref="InternalDstEdges"/> in the returned
            /// array.</param>
            /// <returns>A union of the <see cref="InternalSrcEdges"/> and 
            /// <see cref="InternalDstEdges"/> arrays.</returns>
            public GraphEdge[] AllInternalEdges(bool srcFirst)
            {
                GraphEdge[] edges = new GraphEdge[this.mSrcEdges.Count
                    + this.mDstEdges.Count];
                if (srcFirst)
                {
                    Array.Copy(this.mSrcEdges.ToArray(), 0,
                        edges, 0, this.mSrcEdges.Count);
                    Array.Copy(this.mDstEdges.ToArray(), 0,
                        edges, this.mSrcEdges.Count, this.mDstEdges.Count);
                }
                else
                {
                    Array.Copy(this.mDstEdges.ToArray(), 0,
                        edges, 0, this.mDstEdges.Count);
                    Array.Copy(this.mSrcEdges.ToArray(), 0,
                        edges, this.mDstEdges.Count, this.mSrcEdges.Count);
                }
                return edges;
            }

            /// <summary>
            /// A combined array of both <see cref="SrcNodes"/> and
            /// <see cref="DstNodes"/>, ordered by the given 
            /// <paramref name="srcFirst"/> flag.
            /// </summary>
            /// <param name="srcFirst">Whether <see cref="SrcNodes"/> comes 
            /// before <see cref="DstNodes"/> in the returned array.</param>
            /// <returns>A union of the <see cref="SrcNodes"/> and 
            /// <see cref="DstNodes"/> arrays.</returns>
            public GraphNode[] AllNodes(bool srcFirst)
            {
                int i;
                int srcCount = this.mSrcEdges.Count;
                int dstCount = this.mDstEdges.Count;
                GraphNode[] nodes = new GraphNode[srcCount + dstCount];
                if (srcFirst)
                {
                    for (i = 0; i < srcCount; i++)
                        nodes[i] = this.mSrcEdges[i].mSrcNode;
                    for (i = 0; i < dstCount; i++)
                        nodes[i + srcCount] = this.mDstEdges[i].mDstNode;
                }
                else
                {
                    for (i = 0; i < dstCount; i++)
                        nodes[i] = this.mDstEdges[i].mDstNode;
                    for (i = 0; i < srcCount; i++)
                        nodes[i + dstCount] = this.mSrcEdges[i].mSrcNode;
                }
                return nodes;
            }
            #endregion

            #endregion

            /// <summary>
            /// Tests whether or not this <see cref="GraphNode"/> instance's
            /// <see cref="Data"/> is equal the <paramref name="other"/>
            /// instance's Data.
            /// </summary>
            /// <param name="other">The other <see cref="GraphNode"/> instance
            /// to compare <see cref="Data"/> with.</param>
            /// <returns>true if this instance's <see cref="Data"/> is equal to
            /// the <paramref name="other"/> instance's Data, false otherwise.
            /// </returns>
            public bool Equals(GraphNode other)
            {
                return this.mData == other.mData;
            }

            /// <summary>
            /// Disconnects this <see cref="GraphNode"/> instance from all
            /// other instances in its containing <see cref="Graph"/>,
            /// ophaning it, usually in preparation for removal.
            /// </summary>
            /// <returns>A list of additional nodes which have also been
            /// orphaned by this function because they connected to this
            /// node only beforehand.</returns>
            internal List<GraphNode> Disconnect()
            {
                int count = this.mSrcEdges.Count;
                if (count == 0 && this.mDstEdges.Count == 0)
                {
                    return null;
                }
                GraphNode n;
                int i, index;
                List<GraphNode> orphans = new List<GraphNode>(
                    count + this.mDstEdges.Count + 2);
                // Disconnect this node from its source nodes
                for (i = 0; i < count; i++)
                {
                    n = this.mSrcEdges[i].mSrcNode;
                    index = IndexOfDst(n.mDstEdges, this.mData);
                    n.mDstEdges.RemoveAt(index);
                    if (n.mDstEdges.Count == 0)
                        orphans.Add(n);
                }
                this.mSrcEdges.Clear();
                // Disconnect this node from its destination nodes
                count = this.mDstEdges.Count;
                for (i = 0; i < count; i++)
                {
                    n = this.mDstEdges[i].mDstNode;
                    index = IndexOfSrc(n.mSrcEdges, this.mData);
                    n.mSrcEdges.RemoveAt(index);
                    if (n.mSrcEdges.Count == 0)
                        orphans.Add(n);
                }
                this.mDstEdges.Clear();
                // Tally orphanced source and destination nodes
                count = orphans.Count;
                GraphNode[] temp = orphans.ToArray();
                orphans.Clear();
                for (i = 0; i < count; i++)
                {
                    n = temp[i];
                    if (n.mSrcEdges.Count == 0 && n.mDstEdges.Count == 0)
                        orphans.Add(n);
                }
                return orphans;
            }
        }

        private List<GraphNode> mNodes;
        private List<GraphEdge> mEdges;

        /// <summary>
        /// Initializes a new <see cref="T:DirectionalGraph`2{Node,Edge}"/>
        /// instance that is empty and has the default initial capacities
        /// for its internal node and edge lists.
        /// </summary>
        public DirectionalGraph()
        {
            this.mNodes = new List<GraphNode>();
            this.mEdges = new List<GraphEdge>();
        }

        /// <summary>
        /// Initializes a new <see cref="T:DirectionalGraph`2{Node,Edge}"/>
        /// instance that is empty and has the specified initial capacities
        /// for its internal node and edge lists.
        /// </summary>
        /// <param name="nodeCapacity">The number of nodes that the new graph
        /// can initially store.</param>
        /// <param name="edgeCapacity">The number of edges that the new graph
        /// can initially store.</param>
        public DirectionalGraph(int nodeCapacity, int edgeCapacity)
        {
            this.mNodes = new List<GraphNode>(nodeCapacity);
            this.mEdges = new List<GraphEdge>(edgeCapacity);
        }

        #region Graph Traversal

        #region Source Node Traversal
        /// <summary>
        /// Retrieves all <typeparamref name="Node"/> instances that are 
        /// connected to the given <paramref name="node"/> by an <typeparamref 
        /// name="Edge"/> instance which has <paramref name="node"/> as its
        /// <see cref="P:IGraphEdge`1{Node}.DstNode"/>.
        /// </summary>
        /// <param name="node">A <typeparamref name="Node"/> instance to 
        /// retrieve all source nodes of.</param>
        /// <returns>An array of all <typeparamref name="Node"/> instances
        /// that are connected to <paramref name="node"/> by an <typeparamref 
        /// name="Edge"/> instance which has <paramref name="node"/> as its
        /// destination.</returns>
        /// <seealso cref="GetSrcNodesAt(int)"/>
        /// <seealso cref="GetSrcEdges(Node)"/>
        /// <seealso cref="GetDstNodes(Node)"/>
        public Node[] GetSrcNodes(Node node)
        {
            int index = this.IndexOfNode(node);
            if (index < 0) 
                return null;
            return this.InternalGetSrcNodesAt(index);
        }

        /// <summary>
        /// Retrieves all <typeparamref name="Node"/> instances that are 
        /// connected to the <typeparamref name="Node"/> instance at the given 
        /// <paramref name="nodeIndex"/> by an <typeparamref name="Edge"/> 
        /// instance which has that <typeparamref name="Node"/> instance as its
        /// <see cref="P:IGraphEdge`1{Node}.DstNode"/>.</summary>
        /// <param name="nodeIndex">An index of a <typeparamref name="Node"/>
        /// instance in <see cref="Nodes"/> to retrieve all source nodes of.</param>
        /// <returns>An array of all <typeparamref name="Node"/> instances
        /// that are connected to the <typeparamref name="Node"/> instance at
        /// <paramref name="nodeIndex"/> by an <typeparamref name="Edge"/>
        /// instance which has the <typeparamref name="Node"/> instance at
        /// <paramref name="nodeIndex"/> as its destination.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref 
        /// name="nodeIndex"/> is less than 0 or <paramref name="nodeIndex"/>
        /// is greater than or equal to <see cref="NodeCount"/>.</exception>
        /// <seealso cref="GetSrcNodes(Node)"/>
        /// <seealso cref="GetSrcEdgesAt(int)"/>
        /// <seealso cref="GetDstNodesAt(int)"/>
        public Node[] GetSrcNodesAt(int nodeIndex)
        {
            if (nodeIndex < 0 || nodeIndex > this.mNodes.Count)
                throw new ArgumentOutOfRangeException("nodeIndex");
            return this.InternalGetSrcNodesAt(nodeIndex);
        }

        private Node[] InternalGetSrcNodesAt(int index)
        {
            if (this.mEdges.Count == 0)
                return new Node[0];
            GraphNode node = this.mNodes[index];
            Node[] srcNodes = new Node[node.mSrcEdges.Count];
            for (int i = 0; i < node.mSrcEdges.Count; i++)
                srcNodes[i] = node.mSrcEdges[i].mSrcNode.mData;
            return srcNodes;
        }
        #endregion

        #region Source Edge Traversal
        /// <summary>
        /// Retrieves all <typeparamref name="Edge"/> instances that have
        /// <paramref name="node"/> as their <see 
        /// cref="P:IGraphEdge`1{Node}.DstNode"/>.</summary>
        /// <param name="node">A <typeparamref name="Node"/> instance to 
        /// retrieve all source edges of.</param>
        /// <returns>An array of all <typeparamref name="Edge"/> instances
        /// that have <paramref name="node"/> as their destination, or 
        /// null if <paramref name="node"/> isn't contained in this
        /// <see cref="T:DirectionalGraph`2{Node,Edge}"/>.</returns>
        /// <seealso cref="GetSrcEdgesAt(int)"/>
        /// <seealso cref="GetSrcNodes(Node)"/>
        /// <seealso cref="GetDstEdges(Node)"/>
        public Edge[] GetSrcEdges(Node node)
        {
            int index = this.IndexOfNode(node);
            if (index < 0) 
                return null;
            return this.mNodes[index].SrcEdges;
        }

        /// <summary>
        /// Retrieves all <typeparamref name="Edge"/> instances that have
        /// <typeparamref name="Node"/> instance at the given <paramref 
        /// name="nodeIndex"/> as their <see 
        /// cref="P:IGraphEdge`1{Node}.DstNode"/>.</summary>
        /// <param name="nodeIndex">An index of a <typeparamref name="Node"/>
        /// instance in <see cref="Nodes"/> to retrieve all source edges of.</param>
        /// <returns>An array of all <typeparamref name="Edge"/> instances
        /// that have the <typeparamref name="Node"/> instance at <paramref 
        /// name="nodeIndex"/> as their destination.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref 
        /// name="nodeIndex"/> is less than 0 or <paramref name="nodeIndex"/>
        /// is greater than or equal to <see cref="NodeCount"/>.</exception>
        /// <seealso cref="GetSrcEdges(Node)"/>
        /// <seealso cref="GetSrcNodesAt(int)"/>
        /// <seealso cref="GetDstEdgesAt(int)"/>
        public Edge[] GetSrcEdgesAt(int nodeIndex)
        {
            if (nodeIndex < 0 || nodeIndex >= this.mNodes.Count)
                throw new ArgumentOutOfRangeException("nodeIndex");
            return this.mNodes[nodeIndex].SrcEdges;
        }
        #endregion

        #region Destination Node Traversal
        /// <summary>
        /// Retrieves all <typeparamref name="Node"/> instances that are 
        /// connected to the given <paramref name="node"/> by an <typeparamref 
        /// name="Edge"/> instance which has <paramref name="node"/> as its
        /// <see cref="P:IGraphEdge`1{Node}.SrcNode"/>.
        /// </summary>
        /// <param name="node">A <typeparamref name="Node"/> instance to 
        /// retrieve all destination nodes of.</param>
        /// <returns>An array of all <typeparamref name="Node"/> instances
        /// that are connected to <paramref name="node"/> by an <typeparamref 
        /// name="Edge"/> instance which has <paramref name="node"/> as its
        /// source.</returns>
        /// <seealso cref="GetDstNodesAt(int)"/>
        /// <seealso cref="GetDstEdges(Node)"/>
        /// <seealso cref="GetSrcNodes(Node)"/>
        public Node[] GetDstNodes(Node node)
        {
            int index = this.IndexOfNode(node);
            if (index < 0) 
                return null;
            return this.InternalGetDstNodesAt(index);
        }

        /// <summary>
        /// Retrieves all <typeparamref name="Node"/> instances that are 
        /// connected to the <typeparamref name="Node"/> instance at the given 
        /// <paramref name="nodeIndex"/> by an <typeparamref name="Edge"/> 
        /// instance which has that <typeparamref name="Node"/> instance as its
        /// <see cref="P:IGraphEdge`1{Node}.SrcNode"/>.</summary>
        /// <param name="nodeIndex">An index of a <typeparamref name="Node"/>
        /// instance in <see cref="Nodes"/> to retrieve all destination nodes of.</param>
        /// <returns>An array of all <typeparamref name="Node"/> instances
        /// that are connected to the <typeparamref name="Node"/> instance at
        /// <paramref name="nodeIndex"/> by an <typeparamref name="Edge"/>
        /// instance which has the <typeparamref name="Node"/> instance at
        /// <paramref name="nodeIndex"/> as its source.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref 
        /// name="nodeIndex"/> is less than 0 or <paramref name="nodeIndex"/>
        /// is greater than or equal to <see cref="NodeCount"/>.</exception>
        /// <seealso cref="GetDstNodes(Node)"/>
        /// <seealso cref="GetDstEdgesAt(int)"/>
        /// <seealso cref="GetSrcNodesAt(int)"/>
        public Node[] GetDstNodesAt(int nodeIndex)
        {
            if (nodeIndex < 0 || nodeIndex > this.mNodes.Count)
                throw new ArgumentOutOfRangeException("nodeIndex");
            return this.InternalGetDstNodesAt(nodeIndex);
        }

        private Node[] InternalGetDstNodesAt(int index)
        {
            if (this.mEdges.Count == 0)
                return new Node[0];
            GraphNode node = this.mNodes[index];
            Node[] dstNodes = new Node[node.mDstEdges.Count];
            for (int i = 0; i < node.mDstEdges.Count; i++)
                dstNodes[i] = node.mDstEdges[i].mDstNode.mData;
            return dstNodes;
        }
        #endregion

        #region Destination Edge Traversal
        /// <summary>
        /// Retrieves all <typeparamref name="Edge"/> instances that have
        /// <paramref name="node"/> as their <see 
        /// cref="P:IGraphEdge`1{Node}.SrcNode"/>.</summary>
        /// <param name="node">A <typeparamref name="Node"/> instance to 
        /// retrieve all destination edges of.</param>
        /// <returns>An array of all <typeparamref name="Edge"/> instances
        /// that have <paramref name="node"/> as their source, or 
        /// null if <paramref name="node"/> isn't contained in this
        /// <see cref="T:DirectionalGraph`2{Node,Edge}"/>.</returns>
        /// <seealso cref="GetDstEdgesAt(int)"/>
        /// <seealso cref="GetDstNodes(Node)"/>
        /// <seealso cref="GetSrcEdges(Node)"/>
        public Edge[] GetDstEdges(Node node)
        {
            int index = this.IndexOfNode(node);
            if (index < 0) 
                return null;
            return this.mNodes[index].DstEdges;
        }

        /// <summary>
        /// Retrieves all <typeparamref name="Edge"/> instances that have
        /// <typeparamref name="Node"/> instance at the given <paramref 
        /// name="nodeIndex"/> as their <see 
        /// cref="P:IGraphEdge`1{Node}.SrcNode"/>.</summary>
        /// <param name="nodeIndex">An index of a <typeparamref name="Node"/>
        /// instance in <see cref="Nodes"/> to retrieve all destination edges of.</param>
        /// <returns>An array of all <typeparamref name="Edge"/> instances
        /// that have the <typeparamref name="Node"/> instance at <paramref 
        /// name="nodeIndex"/> as their source.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref 
        /// name="nodeIndex"/> is less than 0 or <paramref name="nodeIndex"/>
        /// is greater than or equal to <see cref="NodeCount"/>.</exception>
        /// <seealso cref="GetDstEdges(Node)"/>
        /// <seealso cref="GetDstNodesAt(int)"/>
        /// <seealso cref="GetSrcEdgesAt(int)"/>
        public Edge[] GetDstEdgesAt(int nodeIndex)
        {
            if (nodeIndex < 0 || nodeIndex >= this.mNodes.Count)
                throw new ArgumentOutOfRangeException("nodeIndex");
            return this.mNodes[nodeIndex].DstEdges;
        }
        #endregion

        #endregion

        #region Theoretical Distances
        /// <summary>
        /// Returns the diameter of a directional graph based on the given 
        /// theoretical <paramref name="distances"/> between the connected
        /// <typeparamref name="Node"/> instances, obtained from the
        /// <see cref="GetDistances()"/> function.
        /// </summary>
        /// <param name="distances">The theoretical distances between connected
        /// <typeparamref name="Node"/> instances contained in a 
        /// <see cref="T:DirectionalGraph`2{Node,Edge}"/> instance.</param>
        /// <returns>The maximum value within the given 
        /// <paramref name="distances"/> which doesn't equal 
        /// <see cref="float.MaxValue"/>, or <see cref="float.NegativeInfinity"/>
        /// if all distances given equal <see cref="float.MaxValue"/>.</returns>
        public static float GetDiameter(float[,] distances)
        {
            if (distances.Length == 0)
                return 0;
            float diameter = float.NegativeInfinity;
            int i, j, width = distances.GetLength(0);
            int height = distances.GetLength(1);
            for (i = 0; i < width; i++)
            {
                for (j = 0; j < height; j++)
                {
                    if (distances[i, j] != float.MaxValue)
                        diameter = Math.Max(diameter, distances[i, j]);
                }
            }
            return diameter;
        }

        /// <summary>
        /// Returns the theoretical distances between all connected 
        /// <typeparamref name="Node"/> instances contained in this directional
        /// graph.
        /// </summary>
        /// <returns>A <see cref="NodeCount"/> by <see cref="NodeCount"/>
        /// two-dimensional array containing the theoretical distances between
        /// all <typeparamref name="Node"/> instances in this directional graph.
        /// </returns>
        /// <remarks>
        /// The theoretical distance between two different <typeparamref 
        /// name="Node"/> instances is either the sum of the <see 
        /// cref="P:IGraphEdge`1.Weight"/>s of the edges along the shortest path
        /// connecting the two nodes, or <see cref="float.MaxValue"/> if the
        /// two nodes are not connected by a path.</remarks>
        /// <seealso cref="GetDistances(int)"/>
        public float[,] GetDistances()
        {
            int count = this.mNodes.Count;
            if (count == 0)
            {
                return new float[0, 0];
            }
            int i, index;
            float[,] distances = new float[count, count];
            if (this.mEdges.Count == 0)
            {
                for (index = 0; index < count; index++)
                    for (i = 0; i < count; i++)
                        distances[index, i] = i == index ? 0 : float.MaxValue;
            }
            float distance;
            GraphNode n, current;
            for (i = 0; i < count; i++)
            {
                current = this.mNodes[i];
                current.Color = GraphColor.White;
                current.Distance = float.MaxValue;
            }
            Queue<GraphNode> queue = new Queue<GraphNode>(count);
            for (index = 0; index < count; index++)
            {
                current = this.mNodes[index];
                current.Distance = 0f;
                current.Color = GraphColor.Gray;
                queue.Enqueue(current);
                while (queue.Count > 0)
                {
                    current = queue.Dequeue();
                    count = current.mDstEdges.Count;
                    for (i = 0; i < count; i++)
                    {
                        n = current.mDstEdges[i].mDstNode;
                        distance = current.Distance
                            + current.mDstEdges[i].mData.Weight;
                        if (n.Color == GraphColor.White)
                        {
                            n.Color = GraphColor.Gray;
                            n.Distance = distance;
                            queue.Enqueue(n);
                        }
                        else if (distance < n.Distance)
                        {
                            n.Distance = distance;
                            if (!queue.Contains(n))
                                queue.Enqueue(n);
                        }
                    }
                }
                count = this.mNodes.Count;
                for (i = 0; i < count; i++)
                    distances[index, i] = this.mNodes[i].Distance;
            }
            for (index = 0; index < count; index++)
            {
                for (i = 0; i < count; i++)
                {
                    if (i != index && distances[index, i] == 0)
                        distances[index, i] = distances[i, index];
                }
            }
            return distances;
        }

        /// <summary>
        /// Returns the theoretical distances between the <typeparamref 
        /// name="Node"/> instance at the given <paramref name="nodeIndex"/>
        /// and all other instances contained in this directional graph.
        /// </summary>
        /// <param name="nodeIndex">The index of the <typeparamref 
        /// name="Node"/> instance in <see cref="Nodes"/> from which to measure
        /// the theoretical distances to all other nodes.</param>
        /// <returns>An array of <see cref="NodeCount"/> elements containing
        /// the theoretical distances between the specified <typeparamref 
        /// name="Node"/> instance and all others in this directional graph.
        /// </returns>
        /// <remarks>
        /// See the remarks on <see cref="GetDistances()"/> for an explanation
        /// of the meaning of the theoretical distances returned.
        /// </remarks>
        /// <seealso cref="GetDistances()"/>
        /// <exception cref="ArgumentOutOfRangeException"><paramref 
        /// name="nodeIndex"/> is less than 0 or <paramref name="nodeIndex"/>
        /// is greater than or equal to <see cref="NodeCount"/>.</exception>
        public float[] GetDistances(int nodeIndex)
        {
            int count = this.mNodes.Count;
            if (count == 0)
                return new float[0];
            if (nodeIndex < 0 || nodeIndex > count)
                throw new ArgumentOutOfRangeException("nodeIndex");
            int i;
            if (this.mEdges.Count == 0)
            {
                float[] dist = new float[count];
                for (i = 0; i < count; i++)
                    dist[i] = i == nodeIndex ? 0 : float.MaxValue;
            }
            float distance;
            GraphNode n, current;
            for (i = 0; i < count; i++)
            {
                current = this.mNodes[i];
                current.Visited = false;
                current.Distance = float.MaxValue;
            }
            Queue<GraphNode> queue = new Queue<GraphNode>(count);
            current = this.mNodes[nodeIndex];
            current.Distance = 0f;
            current.Visited = true;
            queue.Enqueue(current);
            while (queue.Count > 0)
            {
                current = queue.Dequeue();
                count = current.mDstEdges.Count;
                for (i = 0; i < count; i++)
                {
                    n = current.mDstEdges[i].mDstNode;
                    distance = current.Distance
                        + current.mDstEdges[i].mData.Weight;
                    if (!n.Visited)
                    {
                        n.Visited = true;
                        n.Distance = distance;
                        queue.Enqueue(n);
                    }
                    else if (distance < n.Distance)
                    {
                        n.Distance = distance;
                        if (!queue.Contains(n))
                            queue.Enqueue(n);
                    }
                }
            }
            count = this.mNodes.Count;
            float[] distances = new float[count];
            for (i = 0; i < count; i++)
                distances[i] = this.mNodes[i].Distance;
            return distances;
        }
        #endregion

        #region Node List Manipulation

        #region Node List Properties
        /// <summary>
        /// The number of <typeparamref name="Node"/> instances currently
        /// contained in this <see cref="T:DirectionalGraph`2{Node,Edge}"/>.
        /// </summary>
        public int NodeCount
        {
            get { return this.mNodes.Count; }
        }

        /// <summary>
        /// Gets or sets the total number of nodes this graph's 
        /// internal node list can hold without resizing.
        /// </summary><value>
        /// The number of nodes that this graph can contain 
        /// before resizing is required.
        /// </value><exception cref="ArgumentOutOfRangeException">
        /// <see cref="NodeCapacity"/> is set to a value that is 
        /// less than <see cref="NodeCount"/>.
        /// </exception><exception cref="OutOfMemoryException">
        /// There is not enough memory available on the system.
        /// </exception>
        public int NodeCapacity
        {
            get { return this.mNodes.Capacity; }
            set { this.mNodes.Capacity = value; }
        }

        /// <summary>
        /// An array of all the <typeparamref name="Node"/> instances
        /// contained in this <see cref="T:DirectionalGraph`2{Node,Edge}"/>,
        /// in the same order as all functions with a <c>nodeIndex</c>
        /// argument.</summary><seealso cref="InternalNodes"/>
        /// <seealso cref="Edges"/>
        public Node[] Nodes
        {
            get
            {
                int count = this.mNodes.Count;
                Node[] nodes = new Node[count];
                for (int i = 0; i < count; i++)
                    nodes[i] = this.mNodes[i].mData;
                return nodes;
            }
        }

        /// <summary>
        /// Retrieves the <typeparamref name="Node"/> instance at the given
        /// <paramref name="nodeIndex"/> in this directional graph's
        /// internal list of <see cref="Nodes"/>.
        /// </summary>
        /// <param name="nodeIndex">The index of the <typeparamref name="Node"/>
        /// instance to retrieve from this directional graph's internal list of
        /// <see cref="Nodes"/>.</param>
        /// <returns>The <typeparamref name="Node"/> instance at 
        /// <paramref name="nodeIndex"/> in this directional graph's internal
        /// list of <see cref="Nodes"/>.</returns>
        public Node NodeAt(int nodeIndex)
        {
            return this.mNodes[nodeIndex].Data;
        }

        /// <summary>
        /// An array of all the <see cref="GraphNode"/> instances currently
        /// contained within this <see cref="T:DirectionalGraph`2{Node,Edge}"/>,
        /// in the same order as all functions with a <c>nodeIndex</c>
        /// argument.</summary><seealso cref="Nodes"/>
        /// <seealso cref="InternalEdges"/>
        public GraphNode[] InternalNodes
        {
            get { return this.mNodes.ToArray(); }
        }

        /// <summary>
        /// Retrieves the <see cref="GraphNode"/> instance at the given
        /// <paramref name="nodeIndex"/> in this directional graph's
        /// internal list of <see cref="InternalNodes"/>.
        /// </summary>
        /// <param name="nodeIndex">The index of the <see cref="GraphNode"/>
        /// instance to retrieve from this directional graph's internal list of
        /// <see cref="InternalNodes"/>.</param>
        /// <returns>The <see cref="GraphNode"/> instance at 
        /// <paramref name="nodeIndex"/> in this directional graph's internal
        /// list of <see cref="InternalNodes"/>.</returns>
        public GraphNode InternalNodeAt(int nodeIndex)
        {
            return this.mNodes[nodeIndex];
        }
        #endregion

        #region Searching for Nodes
        /// <summary>
        /// Searches for the specified <typeparamref name="Node"/> instance
        /// and returns the zero-based index of its occurrence within this
        /// <see cref="T:DirectionalGraph`2{Node,Edge}"/>'s internal list of 
        /// <see cref="Nodes"/>.</summary>
        /// <param name="node">The <typeparamref name="Node"/> to locate
        /// in this <see cref="T:DirectionalGraph`2{Node,Edge}"/>.</param>
        /// <returns>The zero-based index of the occurrence of 
        /// <paramref name="node"/> within this <see 
        /// cref="T:DirectionalGraph`2{Node,Edge}"/>, if found; otherwise, -1.
        /// </returns><seealso cref="IndexOfEdge(Node,Node)"/>
        public int IndexOfNode(Node node)
        {
            for (int i = 0; i < this.mNodes.Count; i++)
            {
                if (this.mNodes[i].mData == node)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Determines whether the given <typeparamref name="Node"/> is in
        /// this <see cref="T:DirectionalGraph`2{Node,Edge}"/>.
        /// </summary>
        /// <param name="node">The <typeparamref name="Node"/> instance to
        /// locate in this <see cref="T:DirectionalGraph`2{Node,Edge}"/>.
        /// </param>
        /// <returns>true if <paramref name="node"/> is found in this
        /// <see cref="T:DirectionalGraph`2{Node,Edge}"/>; otherwise, false.
        /// </returns><remarks>
        /// This is really just a convenience function for testing whether the
        /// <see cref="IndexOfNode(Node)"/> is greater than or equal to zero.
        /// </remarks>
        public bool ContainsNode(Node node)
        {
            return this.IndexOfNode(node) >= 0;
        }
        #endregion

        #region Adding Nodes
        /// <summary>
        /// Adds a <typeparamref name="Node"/> instance to the end of this
        /// <see cref="T:DirectionalGraph`2{Node,Edge}"/>, if it isn't already
        /// contained in this graph.</summary>
        /// <param name="node">The <typeparamref name="Node"/> instance to be
        /// added to the end of this <see cref="T:DirectionalGraph`2{Node,Edge}"/>.
        /// </param>
        /// <returns>true if the <paramref name="node"/> is added to the end of
        /// this <see cref="T:DirectionalGraph`2{Node,Edge}"/>, false if it's already
        /// contained in it.</returns><seealso cref="AddEdge(Edge)"/>
        public bool AddNode(Node node)
        {
            // Don't allow duplicates
            if (this.IndexOfNode(node) < 0)
            {
                this.mNodes.Add(new GraphNode(this, node));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds the <typeparamref name="Node"/> instances in the specified
        /// array to the end of this <see cref="T:DirectionalGraph`2{Node,Edge}"/>,
        /// if they aren't already contained in this graph.
        /// </summary>
        /// <param name="nodes">The array of <typeparamref name="Node"/>
        /// instances that should be added to the end of this 
        /// <see cref="T:DirectionalGraph`2{Node,Edge}"/>.</param>
        /// <returns>The number of <typeparamref name="Node"/> instances that
        /// are successfully added to the end of this 
        /// <see cref="T:DirectionalGraph`2{Node,Edge}"/>.</returns>
        public int AddNodeRange(Node[] nodes)
        {
            if (nodes == null || nodes.Length == 0)
                return 0;
            // Don't allow duplicates
            Node node;
            List<GraphNode> gNodes = new List<GraphNode>(nodes.Length + 1);
            int i, j, index;
            for (i = 0; i < nodes.Length; i++)
            {
                node = nodes[i];
                if (this.IndexOfNode(node) < 0)
                {
                    index = -1;
                    for (j = 0; j < gNodes.Count && index < 0; j++)
                    {
                        if (gNodes[j].mData == node)
                            index = j;
                    }
                    if (index < 0)
                    {
                        gNodes.Add(new GraphNode(this, nodes[i]));
                    }
                }
            }
            this.mNodes.AddRange(gNodes);
            return gNodes.Count;
        }
        #endregion

        /// <summary>
        /// If <paramref name="oldNode"/> is found within this
        /// <see cref="T:DirectionalGraph`2{Node,Edge}"/>, it is replaced with 
        /// <paramref name="newNode"/>, but only if <paramref name="newNode"/>
        /// isn't already contained in this <see cref="T:DirectionalGraph`2{Node,Edge}"/>.
        /// </summary>
        /// <param name="oldNode">The <typeparamref name="Node"/> instance to
        /// be replaced with <paramref name="newNode"/>.</param>
        /// <param name="newNode">The <typeparamref name="Node"/> instance to
        /// replace <paramref name="oldNode"/>.</param>
        /// <returns>true if the <paramref name="oldNode"/> was successfully
        /// replaced with <paramref name="newNode"/>, false otherwise
        /// (<paramref name="oldNode"/> isn't in this graph or <paramref 
        /// name="newNode"/> already is in it).</returns>
        /// <remarks><para>
        /// This function's main purpose is to obviate the task of otherwise
        /// having to meticulously replace every single <typeparamref 
        /// name="Edge"/> instance connecting <paramref name="oldNode"/> to the
        /// rest of this graph with new <typeparamref name="Edge"/> instances
        /// connecting <paramref name="newNode"/> to the same <typeparamref 
        /// name="Node"/> instances <paramref name="oldNode"/> is connected to,
        /// since using <see cref="RemoveNode(Node)"/> or any related function
        /// also removes all of the node's connections.
        /// </para><para>
        /// However, in order to do this, this function relies heavily on the
        /// implementation of the <see cref="M:IGraphEdge`1{Node}.Copy`1{Edge}(Node,Node)"/>
        /// function in the <typeparamref name="Edge"/> class to create
        /// replacement connections. So, ensure that function is properly
        /// implemented before using this function in order to avoid 
        /// erraneous results.</para></remarks>
        /// <seealso cref="M:IGraphEdge`1{Node}.Copy`1{Edge}(Node,Node)"/>
        public bool ReplaceNode(Node oldNode, Node newNode)
        {
            if (newNode == oldNode)
                return true;
            // Don't allow duplicates
            int i = this.IndexOfNode(newNode);
            if (i >= 0)
            {
                // TODO: Should we remove the oldNode anyway?
                return false;
            }
            i = this.IndexOfNode(oldNode);
            if (i < 0)
                return false;
            this.mNodes[i].mData = newNode;
            int count = this.mEdges.Count;
            Edge e;
            for (i = 0; i < count; i++)
            {
                e = this.mEdges[i].mData;
                if (e.SrcNode == oldNode)
                {
                    this.mEdges[i].mData = e.Copy<Edge>(newNode,
                        e.DstNode == oldNode ? newNode : e.DstNode);
                }
                else if (e.DstNode == oldNode)
                {
                    this.mEdges[i].mData = e.Copy<Edge>(e.SrcNode, newNode);
                }
            }
            return true;
        }

        #region Removing Nodes
        /// <summary>
        /// Removes the specified <typeparamref name="Node"/> instance from
        /// this <see cref="T:DirectionalGraph`2{Node,Edge}"/>, along with all
        /// <typeparamref name="Edge"/> instances connecting it to the rest of 
        /// this graph, but doesn't remove any nodes orphaned by the removal
        /// of <paramref name="node"/>.</summary>
        /// <param name="node">The <typeparamref name="Node"/> instance to 
        /// remove from this <see cref="T:DirectionalGraph`2{Node,Edge}"/>.</param>
        /// <returns>true if <paramref name="node"/> is successfully removed;
        /// otherwise false if <paramref name="node"/> wasn't found in this
        /// <see cref="T:DirectionalGraph`2{Node,Edge}"/>.</returns>
        /// <seealso cref="RemoveNodeAt(int)"/>
        /// <seealso cref="OrphanNode(Node)"/>
        /// <seealso cref="RemoveEdge(Node,Node)"/>
        public bool RemoveNode(Node node)
        {
            int i = this.IndexOfNode(node);
            if (i < 0) 
                return false;
            this.InternalRemoveNodeAt(i, false);
            return true;
        }

        /// <summary>
        /// Removes the specified <typeparamref name="Node"/> instance from
        /// this <see cref="T:DirectionalGraph`2{Node,Edge}"/>, along with all
        /// <typeparamref name="Edge"/> instances connecting it to the rest of 
        /// this graph.</summary>
        /// <param name="node">The <typeparamref name="Node"/> instance to 
        /// remove from this <see cref="T:DirectionalGraph`2{Node,Edge}"/>.</param>
        /// <param name="removeOrphans">Whether or not to remove any 
        /// <typeparamref name="Node"/> instances orphaned by the removal of
        /// <paramref name="node"/>.</param>
        /// <returns>true if <paramref name="node"/> is successfully removed;
        /// otherwise false if <paramref name="node"/> wasn't found in this
        /// <see cref="T:DirectionalGraph`2{Node,Edge}"/>.</returns>
        /// <seealso cref="RemoveNodeAt(int,bool)"/>
        /// <seealso cref="OrphanNode(Node)"/>
        /// <seealso cref="RemoveEdge(Node,Node,bool)"/>
        public bool RemoveNode(Node node, bool removeOrphans)
        {
            int i = this.IndexOfNode(node);
            if (i < 0) 
                return false;
            this.InternalRemoveNodeAt(i, removeOrphans);
            return true;
        }

        /// <summary>
        /// Removes the <typeparamref name="Node"/> instance at the specified
        /// index in this <see cref="T:DirectionalGraph`2{Node,Edge}"/>'s list of
        /// <see cref="Nodes"/>, along with all <typeparamref name="Edge"/>
        /// instances connecting it to the rest of this graph, but doesn't 
        /// remove any any nodes orphaned by the removal of the node at 
        /// <paramref name="nodeIndex"/>.</summary>
        /// <param name="nodeIndex">The zero-based index of the <typeparamref 
        /// name="Node"/> instance to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref 
        /// name="nodeIndex"/> is less than 0 or <paramref name="nodeIndex"/>
        /// is greater than or equal to <see cref="NodeCount"/>.</exception>
        /// <seealso cref="RemoveNode(Node)"/>
        /// <seealso cref="RemoveEdgeAt(int)"/>
        /// <seealso cref="OrphanNodeAt(int)"/>
        public void RemoveNodeAt(int nodeIndex)
        {
            if (nodeIndex < 0 || nodeIndex >= this.mNodes.Count)
                throw new ArgumentOutOfRangeException("nodeIndex");
            this.InternalRemoveNodeAt(nodeIndex, false);
        }

        /// <summary>
        /// Removes the <typeparamref name="Node"/> instance at the specified
        /// index in this <see cref="T:DirectionalGraph`2{Node,Edge}"/>'s list of
        /// <see cref="Nodes"/>, along with all <typeparamref name="Edge"/>
        /// instances connecting it to the rest of this graph.</summary>
        /// <param name="nodeIndex">The zero-based index of the <typeparamref 
        /// name="Node"/> instance to remove.</param>
        /// <param name="removeOrphans">Whether or not to remove any 
        /// <typeparamref name="Node"/> instances orphaned by the removal of
        /// the node at <paramref name="nodeIndex"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref 
        /// name="nodeIndex"/> is less than 0 or <paramref name="nodeIndex"/>
        /// is greater than or equal to <see cref="NodeCount"/>.</exception>
        /// <seealso cref="RemoveNode(Node,bool)"/>
        /// <seealso cref="RemoveEdgeAt(int,bool)"/>
        /// <seealso cref="OrphanNodeAt(int)"/>
        public void RemoveNodeAt(int nodeIndex, bool removeOrphans)
        {
            if (nodeIndex < 0 || nodeIndex >= this.mNodes.Count)
                throw new ArgumentOutOfRangeException("nodeIndex");
            this.InternalRemoveNodeAt(nodeIndex, removeOrphans);
        }

        private void InternalRemoveNodeAt(int i, bool removeOrphans)
        {
            int count;
            Node node = this.mNodes[i].mData;
            if (removeOrphans)
            {
                GraphNode n = this.mNodes[i];
                List<GraphNode> orphans = n.Disconnect();
                if (!orphans.Contains(n))
                    orphans.Add(n);
                count = this.mNodes.Count;
                List<GraphNode> newNodes = new List<GraphNode>(
                    count - orphans.Count + 1);
                for (i = 0; i < count; i++)
                {
                    n = this.mNodes[i];
                    if (!orphans.Contains(n))
                        newNodes.Add(n);
                    else
                        n.mGraph = null;
                }
                this.mNodes = newNodes;
            }
            else
            {
                this.mNodes[i].Disconnect();
                this.mNodes.RemoveAt(i);
            }
            count = this.mEdges.Count;
            List<GraphEdge> newEdges = new List<GraphEdge>(count + 1);
            Edge e;
            for (i = 0; i < count; i++)
            {
                e = this.mEdges[i].mData;
                if (e.SrcNode != node && e.DstNode != node)
                    newEdges.Add(this.mEdges[i]);
            }
            this.mEdges = newEdges;
        }
        #endregion

        #region Orphan Nodes
        /// <summary>
        /// Removes all <typeparamref name="Edge"/> instances connecting the
        /// given <typeparamref name="Node"/> instance to all other instances 
        /// in this <see cref="T:DirectionalGraph`2{Node,Edge}"/>, essentially
        /// orphaning it from the rest of this graph.</summary>
        /// <param name="node">The <typeparamref name="Node"/> instance to be
        /// orphaned from the rest of this <see cref="T:DirectionalGraph`2{Node,Edge}"/>.
        /// </param>
        /// <returns>true if <paramref name="node"/> is successfully orphaned;
        /// otherwise, false if <paramref name="node"/> wasn't found in this
        /// <see cref="T:DirectionalGraph`2{Node,Edge}"/>.</returns>
        /// <seealso cref="OrphanNodeAt(int)"/>
        /// <seealso cref="RemoveNode(Node,bool)"/>
        /// <seealso cref="RemoveEdge(Node,Node,bool)"/>
        public bool OrphanNode(Node node)
        {
            int i = this.IndexOfNode(node);
            if (i < 0) 
                return false;
            this.InternalOrphanNodeAt(i);
            return true;
        }

        /// <summary>
        /// Removes all <typeparamref name="Edge"/> instances connecting the
        /// <typeparamref name="Node"/> instance at the given index to all 
        /// other instances in this <see cref="T:DirectionalGraph`2{Node,Edge}"/>,
        /// essentially orphaning it from the rest of this graph.</summary>
        /// <param name="nodeIndex">The zero-based index of the <typeparamref 
        /// name="Node"/> instance to be orphaned.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref 
        /// name="nodeIndex"/> is less than 0 or <paramref name="nodeIndex"/>
        /// is greater than or equal to <see cref="NodeCount"/>.</exception>
        /// <seealso cref="RemoveNodeAt(int,bool)"/>
        public void OrphanNodeAt(int nodeIndex)
        {
            if (nodeIndex < 0 || nodeIndex >= this.mNodes.Count)
                throw new ArgumentOutOfRangeException("nodeIndex");
            this.InternalOrphanNodeAt(nodeIndex);
        }

        private void InternalOrphanNodeAt(int i)
        {
            GraphNode n = this.mNodes[i];
            if (n.mSrcEdges.Count == 0 && n.mDstEdges.Count == 0)
                return;
            n.Disconnect();
            Node node = n.mData;
            int count = this.mEdges.Count;
            List<GraphEdge> newEdges = new List<GraphEdge>(count + 1);
            Edge e;
            for (i = 0; i < count; i++)
            {
                e = this.mEdges[i].mData;
                if (e.SrcNode != node && e.DstNode != node)
                    newEdges.Add(this.mEdges[i]);
            }
            this.mEdges = newEdges;
        }

        /// <summary>
        /// Retrieves all orphaned <typeparamref name="Node"/> instances (nodes
        /// without any <typeparamref name="Edge"/> instances connecting them
        /// to any other <typeparamref name="Node"/> instances) from this
        /// <see cref="T:DirectionalGraph`2{Node,Edge}"/>.
        /// </summary>
        /// <returns>An array containing all orphaned <typeparamref 
        /// name="Node"/> currently in this <see cref="T:DirectionalGraph`2{Node,Edge}"/>.
        /// </returns><seealso cref="Nodes"/>
        /// <seealso cref="OrphanNode(Node)"/>
        public Node[] FindOrphanedNodes()
        {
            if (this.mEdges.Count == 0)
                return this.Nodes;
            int count = this.mNodes.Count;
            if (count == 0)
                return new Node[0];
            List<Node> orphans = new List<Node>(count + 1);
            GraphNode n;
            for (int i = 0; i < count; i++)
            {
                n = this.mNodes[i];
                if (n.mSrcEdges.Count == 0 && n.mDstEdges.Count == 0)
                    orphans.Add(n.mData);
            }
            return orphans.ToArray();
        }

        /// <summary>
        /// Retrieves all orphaned <see cref="GraphNode"/> instances (nodes
        /// without any <typeparamref name="Edge"/> instances connecting them
        /// to any other <see cref="GraphNode"/> instances) from this
        /// <see cref="T:DirectionalGraph`2{Node,Edge}"/>.
        /// </summary>
        /// <returns>An array containing all orphaned <typeparamref 
        /// name="Node"/> currently in this <see cref="T:DirectionalGraph`2{Node,Edge}"/>.
        /// </returns><seealso cref="InternalNodes"/>
        /// <seealso cref="OrphanNode(Node)"/>
        public GraphNode[] FindOrphanedInternalNodes()
        {
            if (this.mEdges.Count == 0)
                return this.mNodes.ToArray();
            int count = this.mNodes.Count;
            if (count == 0)
                return new GraphNode[0];
            List<GraphNode> orphans = new List<GraphNode>(count + 1);
            GraphNode n;
            for (int i = 0; i < count; i++)
            {
                n = this.mNodes[i];
                if (n.mSrcEdges.Count == 0 && n.mDstEdges.Count == 0)
                    orphans.Add(n);
            }
            return orphans.ToArray();
        }

        /// <summary>
        /// Removes all orphaned <typeparamref name="Node"/> instances (nodes
        /// without any <typeparamref name="Edge"/> instances connecting them
        /// to any other <typeparamref name="Node"/> instances) from this
        /// <see cref="T:DirectionalGraph`2{Node,Edge}"/>.
        /// </summary><remarks>
        /// Remember that this will remove any <typeparamref name="Node"/>
        /// instances recently added without any connections yet, as well as
        /// any orphaned as a side-effect of a function, including
        /// <see cref="RemoveNode(Node)"/>, <see cref="RemoveEdge(Node,Node)"/>
        /// and <see cref="OrphanNode(Node)"/>.</remarks>
        public void ClearOrphanedNodes()
        {
            if (this.mEdges.Count == 0)
                this.ClearNodes();
            GraphNode n;
            int i, count = this.mNodes.Count;
            List<GraphNode> newNodes = new List<GraphNode>(count + 1);
            for (i = 0; i < count; i++)
            {
                n = this.mNodes[i];
                if (n.mSrcEdges.Count == 0 && n.mDstEdges.Count == 0)
                {
                    //n.Dispose();//No need; lists are already cleared
                }
                else
                {
                    newNodes.Add(n);
                }
            }
            this.mNodes = newNodes;
        }
        #endregion

        /// <summary>
        /// Removes all <typeparamref name="Node"/> instances from this <see 
        /// cref="T:DirectionalGraph`2{Node,Edge}"/>, as well as all <typeparamref 
        /// name="Edge"/> instances, since they're meaningless without any
        /// <typeparamref name="Node"/> instances.
        /// </summary><seealso cref="ClearOrphanedNodes()"/>
        /// <seealso cref="ClearEdges()"/>.
        public void ClearNodes()
        {
            if (this.mEdges.Count > 0)
            {
                int count = this.mNodes.Count;
                for (int i = 0; i < count; i++)
                    this.mNodes[i].Disconnect();
            }
            this.mNodes.Clear();
            this.mEdges.Clear();
        }

        #endregion

        #region Edge List Manipulation

        #region Edge List Properties
        /// <summary>
        /// The number of <typeparamref name="Edge"/> instances currently
        /// contained in this <see cref="T:DirectionalGraph`2{Node,Edge}"/>.
        /// </summary><seealso cref="NodeCount"/>
        public int EdgeCount
        {
            get { return this.mEdges.Count; }
        }

        /// <summary>
        /// Gets or sets the total number of edges this graph's 
        /// internal edge list can hold without resizing.
        /// </summary><value>
        /// The number of edges that this graph can contain 
        /// before resizing is required.
        /// </value><exception cref="ArgumentOutOfRangeException">
        /// <see cref="EdgeCapacity"/> is set to a value that is 
        /// less than <see cref="EdgeCount"/>.
        /// </exception><exception cref="OutOfMemoryException">
        /// There is not enough memory available on the system.
        /// </exception>
        public int EdgeCapacity
        {
            get { return this.mEdges.Capacity; }
            set { this.mEdges.Capacity = value; }
        }

        /// <summary>
        /// An array of all the <typeparamref name="Edge"/> instances
        /// contained in this <see cref="T:DirectionalGraph`2{Node,Edge}"/>,
        /// in the same order as all functions with an <c>edgeIndex</c>
        /// argument.</summary><seealso cref="InternalEdges"/>
        /// <seealso cref="Nodes"/>
        public Edge[] Edges
        {
            get
            {
                int count = this.mEdges.Count;
                Edge[] edges = new Edge[count];
                for (int i = 0; i < count; i++)
                    edges[i] = this.mEdges[i].mData;
                return edges;
            }
        }

        /// <summary>
        /// Retrieves the <typeparamref name="Edge"/> instance at the given
        /// <paramref name="edgeIndex"/> in this directional graph's
        /// internal list of <see cref="Edges"/>.
        /// </summary>
        /// <param name="edgeIndex">The index of the <typeparamref name="Edge"/>
        /// instance to retrieve from this directional graph's internal list of
        /// <see cref="Edges"/>.</param>
        /// <returns>The <typeparamref name="Edge"/> instance at 
        /// <paramref name="edgeIndex"/> in this directional graph's internal
        /// list of <see cref="Edges"/>.</returns>
        public Edge EdgeAt(int edgeIndex)
        {
            return this.mEdges[edgeIndex].Data;
        }

        /// <summary>
        /// An array of all the <see cref="GraphEdge"/> instances currently
        /// contained within this <see cref="T:DirectionalGraph`2{Node,Edge}"/>,
        /// in the same order as all functions with an <c>edgeIndex</c>
        /// argument.</summary><seealso cref="Edges"/>
        /// <seealso cref="InternalNodes"/>
        public GraphEdge[] InternalEdges
        {
            get { return this.mEdges.ToArray(); }
        }

        /// <summary>
        /// Retrieves the <see cref="GraphEdge"/> instance at the given
        /// <paramref name="edgeIndex"/> in this directional graph's
        /// internal list of <see cref="InternalEdges"/>.
        /// </summary>
        /// <param name="edgeIndex">The index of the <see cref="GraphEdge"/>
        /// instance to retrieve from this directional graph's internal list of
        /// <see cref="InternalEdges"/>.</param>
        /// <returns>The <see cref="GraphEdge"/> instance at 
        /// <paramref name="edgeIndex"/> in this directional graph's internal
        /// list of <see cref="InternalEdges"/>.</returns>
        public GraphEdge InternalEdgeAt(int edgeIndex)
        {
            return this.mEdges[edgeIndex];
        }
        #endregion

        #region Searching for Edges
        /// <summary>
        /// Searches for the <typeparamref name="Edge"/> instance with the
        /// specified source and destination <typeparamref name="Node"/>s
        /// and returns the zero-based index of its occurrence within this
        /// <see cref="T:DirectionalGraph`2{Node,Edge}"/>'s internal list of 
        /// <see cref="Edges"/>.</summary>
        /// <param name="srcNode">The <see cref="P:IGraphEdge`1{Node}.SrcNode"/>
        /// of the <typeparamref name="Edge"/> instance to locate.</param>
        /// <param name="dstNode">The <see cref="P:IGraphEdge`1{Node}.DstNode"/>
        /// of the <typeparamref name="Edge"/> instance to locate.</param>
        /// <returns>The zero-based index of the occurrence of the
        /// edge with the given <paramref name="srcNode"/> source and
        /// <paramref name="dstNode"/> destination within this <see 
        /// cref="T:DirectionalGraph`2{Node,Edge}"/>, if found; otherwise, -1.
        /// </returns>
        /// <seealso cref="IndexOfNode(Node)"/>
        public int IndexOfEdge(Node srcNode, Node dstNode)
        {
            Edge edge;
            for (int i = 0; i < this.mEdges.Count; i++)
            {
                edge = this.mEdges[i].mData;
                if (edge.SrcNode == srcNode &&
                    edge.DstNode == dstNode)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Determines whether an <typeparamref name="Edge"/> instance with the
        /// specified source and destination <typeparamref name="Node"/>s is in
        /// this <see cref="T:DirectionalGraph`2{Node,Edge}"/>.
        /// </summary>
        /// <param name="srcNode">The <see cref="P:IGraphEdge`1{Node}.SrcNode"/>
        /// of the <typeparamref name="Edge"/> instance to locate.</param>
        /// <param name="dstNode">The <see cref="P:IGraphEdge`1{Node}.DstNode"/>
        /// of the <typeparamref name="Edge"/> instance to locate.</param>
        /// <returns>true if an <typeparamref name="Edge"/> instance with the
        /// given <paramref name="srcNode"/> source and 
        /// <paramref name="dstNode"/> destination is in this 
        /// <see cref="T:DirectionalGraph`2{Node,Edge}"/>; otherwise, false.
        /// </returns><remarks>
        /// This is really just a convenience function for testing whether the
        /// <see cref="IndexOfEdge(Node,Node)"/> is greater than or equal to 
        /// zero. </remarks>
        public bool ContainsEdge(Node srcNode, Node dstNode)
        {
            return this.IndexOfEdge(srcNode, dstNode) >= 0;
        }

        /// <summary>
        /// Searches for the <typeparamref name="Edge"/> instance with the
        /// specified sourc and destination <typeparamref name="Node"/>s
        /// and returns that instance if its found, or the default value of
        /// the <typeparamref name="Edge"/> type if it isn't found.
        /// </summary>
        /// <param name="srcNode">The <see cref="P:IGraphEdge`1{Node}.SrcNode"/>
        /// of the <typeparamref name="Edge"/> instance to locate.</param>
        /// <param name="dstNode">The <see cref="P:IGraphEdge`1{Node}.DstNode"/>
        /// of the <typeparamref name="Edge"/> instance to locate.</param>
        /// <returns>The <typeparamref name="Edge"/> instance with the given
        /// <paramref name="srcNode"/> source and <paramref name="dstNode"/>
        /// destination in this <see cref="T:DirectionalGraph`2{Node,Edge}"/>,
        /// if found; otherwise, the default value of the <typeparamref 
        /// name="Edge"/> type.</returns>
        public Edge FindEdge(Node srcNode, Node dstNode)
        {
            int index = this.IndexOfEdge(srcNode, dstNode);
            return index < 0 ? default(Edge) : this.mEdges[index].mData;
        }
        #endregion

        #region Adding Edges
        /// <summary>
        /// Adds an <typeparamref name="Edge"/> instance to the end of this
        /// <see cref="T:DirectionalGraph`2{Node,Edge}"/>, or replaces the 
        /// <typeparamref name="Edge"/> instance with the same <see 
        /// cref="P:IGraphEdge`1{Node}.SrcNode"/> and <see 
        /// cref="P:IGraphNode`1{Node}.DstNode"/> already in this graph with the
        /// given <paramref name="edge"/>.</summary>
        /// <param name="edge">The <typeparamref name="Edge"/> instance to be
        /// added to the end of this <see cref="T:DirectionalGraph`2{Node,Edge}"/>, or 
        /// replace the instance with the same source and destination.</param>
        /// <seealso cref="AddEdge(Edge,bool)"/><seealso cref="AddNode(Node)"/>
        public void AddEdge(Edge edge)
        {
            this.AddEdge(edge, true);
        }

        /// <summary>
        /// Adds an <typeparamref name="Edge"/> instance to the end of this
        /// <see cref="T:DirectionalGraph`2{Node,Edge}"/>, or replaces the 
        /// <typeparamref name="Edge"/> instance with the same <see 
        /// cref="M:IGraphEdge`1{Node}.SrcNode"/> and <see 
        /// cref="M:IGraphNode`1{Node}.DstNode"/> already in this graph with the
        /// given <paramref name="edge"/>.</summary>
        /// <param name="edge">The <typeparamref name="Edge"/> instance to be
        /// added to the end of this <see cref="T:DirectionalGraph`2{Node,Edge}"/>, or to
        /// replace the instance with the same source and destination.</param>
        /// <param name="replace">Whether or not <paramref name="edge"/>
        /// replaces any existing <typeparamref name="Edge"/> instance with the
        /// same source and destination <typeparamref name="Node"/>s.</param>
        /// <returns>true if the <paramref name="edge"/> is added to this <see 
        /// cref="T:DirectionalGraph`2{Node,Edge}"/>, false if it's already contained in it 
        /// and replacement isn't allowed.</returns>
        /// <seealso cref="AddEdge(Edge)"/><seealso cref="AddNode(Node)"/>
        public bool AddEdge(Edge edge, bool replace)
        {
            // Replace if edge already exists
            int index = this.IndexOfEdge(edge.SrcNode, edge.DstNode);
            GraphNode src, dst;
            if (index >= 0)
            {
                if (!replace)
                    return false;
                this.mEdges[index].mData = edge;
                return true;
            }
            index = this.IndexOfNode(edge.SrcNode);
            if (index < 0)
            {
                src = new GraphNode(this, edge.SrcNode);
                this.mNodes.Add(src);
            }
            else
            {
                src = this.mNodes[index];
            }
            index = this.IndexOfNode(edge.DstNode);
            if (index < 0)
            {
                dst = new GraphNode(this, edge.DstNode);
                this.mNodes.Add(dst);
            }
            else
            {
                dst = this.mNodes[index];
            }
            GraphEdge e = new GraphEdge(src, dst, edge);
            this.mEdges.Add(e);
            src.mDstEdges.Add(e);
            dst.mSrcEdges.Add(e);
            return true;
        }
        #endregion

        #region Removing Edges
        /// <summary>
        /// Removes the <typeparamref name="Edge"/> instance with the specified
        /// <paramref name="srcNode"/> and <paramref name="dstNode"/> from
        /// this <see cref="T:DirectionalGraph`2{Node,Edge}"/>, but doesn't remove 
        /// any nodes orphaned by the removal of the edge.</summary>
        /// <param name="srcNode">The <see cref="P:IGraphEdge`1{Node}.SrcNode"/>
        /// of the <typeparamref name="Edge"/> instance to remove.</param>
        /// <param name="dstNode">The <see cref="P:IGraphEdge`1{Node}.DstNode"/>
        /// of the <typeparamref name="Edge"/> instance to remove.</param>
        /// <returns>true if the <typeparamref name="Edge"/> instance is 
        /// successfully removed; otherwise false if there is no instance with
        /// specified source and destination <typeparamref name="Node"/>
        /// instances in this <see cref="T:DirectionalGraph`2{Node,Edge}"/>.</returns>
        /// <seealso cref="RemoveEdgeAt(int)"/>
        /// <seealso cref="OrphanNode(Node)"/>
        /// <seealso cref="RemoveNode(Node)"/>
        public bool RemoveEdge(Node srcNode, Node dstNode)
        {
            int index = this.IndexOfEdge(srcNode, dstNode);
            if (index < 0) 
                return false;
            this.InternalRemoveEdgeAt(index, srcNode, dstNode, false);
            return true;
        }

        /// <summary>
        /// Removes the <typeparamref name="Edge"/> instance with the specified
        /// <paramref name="srcNode"/> and <paramref name="dstNode"/> from
        /// this <see cref="T:DirectionalGraph`2{Node,Edge}"/>.</summary>
        /// <param name="srcNode">The <see cref="P:IGraphEdge`1{Node}.SrcNode"/>
        /// of the <typeparamref name="Edge"/> instance to remove.</param>
        /// <param name="dstNode">The <see cref="P:IGraphEdge`1{Node}.DstNode"/>
        /// of the <typeparamref name="Edge"/> instance to remove.</param>
        /// <param name="removeOrphans">Whether or not to also remove <paramref
        /// name="srcNode"/> or <paramref name="dstNode"/> if either is 
        /// orphaned by the removal of the edge connecting them.</param>
        /// <returns>true if the <typeparamref name="Edge"/> instance is 
        /// successfully removed; otherwise false if there is no instance with
        /// specified source and destination <typeparamref name="Node"/>
        /// instances in this <see cref="T:DirectionalGraph`2{Node,Edge}"/>.</returns>
        /// <seealso cref="RemoveEdgeAt(int,bool)"/>
        /// <seealso cref="OrphanNode(Node)"/>
        /// <seealso cref="RemoveNode(Node,bool)"/>
        public bool RemoveEdge(Node srcNode, Node dstNode, bool removeOrphans)
        {
            int index = this.IndexOfEdge(srcNode, dstNode);
            if (index < 0) 
                return false;
            this.InternalRemoveEdgeAt(index, srcNode, dstNode, removeOrphans);
            return true;
        }

        /// <summary>
        /// Removes the <typeparamref name="Edge"/> instance at the specified
        /// index in this <see cref="T:DirectionalGraph`2{Node,Edge}"/>'s list of
        /// <see cref="Edges"/>, but doesn't remove any any nodes orphaned by 
        /// the removal of the edge at <paramref name="edgeIndex"/>.</summary>
        /// <param name="edgeIndex">The zero-based index of the <typeparamref 
        /// name="Edge"/> instance to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref 
        /// name="edgeIndex"/> is less than 0 or <paramref name="edgeIndex"/>
        /// is greater than or equal to <see cref="NodeCount"/>.</exception>
        /// <seealso cref="RemoveEdge(Node,Node)"/>
        /// <seealso cref="RemoveNodeAt(int)"/>
        /// <seealso cref="OrphanNodeAt(int)"/>
        public void RemoveEdgeAt(int edgeIndex)
        {
            if (edgeIndex < 0 || edgeIndex >= this.mEdges.Count)
                throw new ArgumentOutOfRangeException("edgeIndex");
            Edge e = this.mEdges[edgeIndex].mData;
            this.InternalRemoveEdgeAt(edgeIndex, e.SrcNode, e.DstNode, false);
        }

        /// <summary>
        /// Removes the <typeparamref name="Edge"/> instance at the specified
        /// index in this <see cref="T:DirectionalGraph`2{Node,Edge}"/>'s list of
        /// <see cref="Edges"/>, but doesn't remove any any nodes orphaned by 
        /// the removal of the edge at <paramref name="edgeIndex"/>.</summary>
        /// <param name="edgeIndex">The zero-based index of the <typeparamref 
        /// name="Edge"/> instance to remove.</param>
        /// <param name="removeOrphans">Whether or not to also remove the
        /// <see cref="P:IGraphEdge`1{Node}.SrcNode"/> or <see cref="P:IGraphEdge`1{Node}.DstNode"/>
        /// of the <typeparamref name="Edge"/> instance at <paramref 
        /// name="edgeIndex"/> if either is orphaned by its removal.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref 
        /// name="edgeIndex"/> is less than 0 or <paramref name="edgeIndex"/>
        /// is greater than or equal to <see cref="NodeCount"/>.</exception>
        /// <seealso cref="RemoveEdge(Node,Node,bool)"/>
        /// <seealso cref="RemoveNodeAt(int,bool)"/>
        /// <seealso cref="OrphanNodeAt(int)"/>
        public void RemoveEdgeAt(int edgeIndex, bool removeOrphans)
        {
            if (edgeIndex < 0 || edgeIndex >= this.mEdges.Count)
                throw new ArgumentOutOfRangeException("edgeIndex");
            Edge e = this.mEdges[edgeIndex].mData;
            this.InternalRemoveEdgeAt(edgeIndex, e.SrcNode, e.DstNode, 
                removeOrphans);
        }

        private void InternalRemoveEdgeAt(int index, Node src, Node dst,
            bool removeOrphans)
        {
            GraphEdge edge = this.mEdges[index];
            this.mEdges.RemoveAt(index);
            index = IndexOfDst(edge.mSrcNode.mDstEdges, dst);
            edge.mSrcNode.mDstEdges.RemoveAt(index);
            index = IndexOfSrc(edge.mDstNode.mSrcEdges, src);
            edge.mDstNode.mSrcEdges.RemoveAt(index);
            if (removeOrphans)
            {
                if (edge.mSrcNode.mDstEdges.Count == 0 && 
                    edge.mSrcNode.mSrcEdges.Count == 0)
                {
                    index = this.IndexOfNode(src);
                    this.mNodes.RemoveAt(index);
                }
                if (edge.mDstNode.mSrcEdges.Count == 0 && 
                    edge.mDstNode.mDstEdges.Count == 0)
                {
                    index = this.IndexOfNode(dst);
                    this.mNodes.RemoveAt(index);
                }
            }
        }
        #endregion

        /// <summary>
        /// Removes all <typeparamref name="Edge"/> instances from this <see 
        /// cref="T:DirectionalGraph`2{Node,Edge}"/>, effectively orphaning
        /// every <typeparamref name="Node"/> instance in this graph.
        /// </summary><remarks>
        /// This is equivalent to calling <see cref="OrphanNode(Node)"/> for
        /// every <typeparamref name="Node"/> instance in this <see 
        /// cref="T:DirectionalGraph`2{Node,Edge}"/>, except it's much faster.
        /// </remarks><seealso cref="ClearOrphanedNodes()"/>
        /// <seealso cref="ClearNodes()"/>.
        public void ClearEdges()
        {
            if (this.mEdges.Count > 0)
            {
                this.mEdges.Clear();
                int i, count = this.mNodes.Count;
                for (i = 0; i < count; i++)
                {
                    this.mNodes[i].mSrcEdges.Clear();
                    this.mNodes[i].mDstEdges.Clear();
                }
            }
        }

        #endregion
    }
}