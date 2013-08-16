using System;
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
    public class Digraph<Node, Edge>
        where Edge : IGraphEdge<Node>
    {
        /// <summary>
        /// This class is used to store an <typeparamref name="Edge"/> instance
        /// along with <see cref="GNode"/> instances that store the edge's
        /// <see cref="P:IGraphEdge`1{Node}.SrcNode"/> and 
        /// <see cref="P:IGraphEdge`1{Node}.DstNode"/> in their respective
        /// <see cref="GNode.Data"/>.
        /// </summary>
        public class GEdge
        {
            /// <summary>
            /// The source node of this edge; its parent.
            /// </summary><remarks>
            /// The <see cref="GNode"/> instance that stores the
            /// <see cref="P:IGraphEdge`1{Node}.SrcNode"/> of 
            /// <see cref="Data"/>.
            /// </remarks>
            public readonly GNode SrcNode;
            /// <summary>
            /// The destination node of this edge; its child.
            /// </summary><remarks>
            /// The <see cref="GNode"/> instance that stores the
            /// <see cref="P:IGraphEdge`1{Node}.DstNode"/> of 
            /// <see cref="Data"/>.
            /// </remarks>
            public readonly GNode DstNode;
            /// <summary>
            /// The underlying data that this edge represents.
            /// </summary><remarks>
            /// The underlying <typeparamref name="Edge"/> instance that
            /// this <see cref="GEdge"/> instance represents.
            /// </remarks>
            public readonly Edge Data;
            /// <summary>
            /// If true, all graph processing algorithms in this library,
            /// including both graph theory and layout algorithms,
            /// will ignore this edge and skip over it.
            /// </summary>
            public bool Hidden;
            /*/// <summary>
            /// Marks the state of visitation of this <see cref="GEdge"/>
            /// instance when it is being traversed by a search algorithm or
            /// some other manner of data processing.
            /// </summary>
            public GraphColor Color;/* */

            /// <summary>
            /// Initializes a new <see cref="GEdge"/> instance with the 
            /// given source and destination <see cref="GNode"/> instances
            /// and the given <typeparamref name="Edge"/> instance.
            /// </summary>
            /// <param name="srcNode">The source node.</param>
            /// <param name="dstNode">The destination node.</param>
            /// <param name="data">The underlying edge data.</param>
            /// <remarks>
            /// The <see cref="GNode.Data"/> of <paramref name="srcNode"/>
            /// and <paramref name="dstNode"/> must match the respective 
            /// <see cref="P:IGraphEdge{Node}`1.SrcNode"/> and 
            /// <see cref="P:IGraphEdge`1{Node}.DstNode"/> of 
            /// <paramref name="data"/>.</remarks>
            public GEdge(GNode srcNode, GNode dstNode, Edge data)
            {
                this.SrcNode = srcNode;
                this.DstNode = dstNode;
                this.Data = data;
                this.Hidden = false;
            }

            public override string ToString()
            {
                string str;
                try
                {
                    str = this.Data.ToString();
                }
                catch (NullReferenceException)
                {
                    str = "{NULL}";
                }
                catch (Exception ex)
                {
                    str = ex.ToString();
                }
                return str;
            }
        }

        /// <summary>
        /// This class is used internally to store instances of the
        /// <typeparamref name="Node"/> class along with additional data
        /// used for traversing its containing 
        /// <see cref="T:Digraph`2{Node,Edge}"/> instance.
        /// </summary>
        public class GNode
        {
            /// <summary>
            /// The underlying <typeparamref name="Node"/> data that this
            /// <see cref="GNode"/> instance represents.
            /// </summary>
            public readonly Node Data;
            /// <summary>
            /// This node's index in its containing directional graph's 
            /// internal node list.
            /// </summary>
            internal int mIndex;
            /// <summary>
            /// Current number of edges that have this node as both their
            /// SrcNode and DstNode.
            /// </summary>
            internal int mLoopCount;
            /// <summary>
            /// Current number of edges that have this node as their SrcNode.
            /// </summary>
            internal int mSrcCount;
            /// <summary>
            /// Current number of edges that have this node as their DstNode.
            /// </summary>
            internal int mDstCount;
            
            internal bool mHidden;
            /// <summary>
            /// Marks the state of visitation of this <see cref="GNode"/>
            /// instance when it is being traversed by a search algorithm or
            /// some other manner of data processing.
            /// </summary>
            public GraphColor Color;

            /// <summary>
            /// Initializes a new <see cref="GNode"/> instance representing
            /// the given <paramref name="data"/> with the given initial
            /// <paramref name="index"/> in the internal node list of the
            /// <see cref="T:Digraph`2{Node,Edge}"/> that created it.
            /// </summary>
            /// <param name="data">The data that this node represents.</param>
            /// <param name="index">The initial index of this node in its
            /// parent graph's internal node list.</param>
            internal GNode(Node data, int index)
            {
                this.Data = data;
                this.mIndex = index;
                this.mLoopCount = 0;
                this.mSrcCount = 0;
                this.mDstCount = 0;
                this.mHidden = false;
            }

            #region Properties
            /// <summary>
            /// Gets the current index of this node in its graph's
            /// <see cref="P:Digraph`2{Node,Edge}.InternalNodes"/> list.
            /// </summary>
            public int Index
            {
                get { return this.mIndex; }
            }
            /// <summary>
            /// Whether this node all graph processing algorithms in this 
            /// library, including both graph theory and layout algorithms.
            /// If true, this node will be ignored and skipped over by them.
            /// </summary>
            public bool Hidden
            {
                get { return this.mHidden; }
            }
            /// <summary>
            /// Gets the current number of edges in this node's graph that
            /// have this node as their destination/target (their arrow heads
            /// touch this node in a visual representation of the graph).
            /// </summary>
            public int IncomingEdgeCount
            {
                get { return this.mDstCount; }
            }
            /// <summary>
            /// Gets the current number of edges in this node's graph that
            /// have this node as their source (their arrow tails
            /// touch this node in a visual representation of the graph).
            /// </summary>
            public int OutgoingEdgeCount
            {
                get { return this.mSrcCount; }
            }
            /// <summary>
            /// Gets the current number of edges in this node's graph that
            /// have this node as both their destination/target and their
            /// source (their arrow heads and tails would touch this node in
            /// a visual representation of the graph).
            /// </summary>
            public int SelfLoopEdgeCount
            {
                get { return this.mLoopCount; }
            }
            /// <summary>
            /// Gets the current total number of edges in this node's graph
            /// that are connected to this node in either direction (both
            /// incoming and outgoing).
            /// </summary>
            /// <param name="includeLoops">Whether to include the number of
            /// self-looping edges in the returned edge count.</param>
            /// <returns>The sum of the <see cref="IncomingEdgeCount"/> and
            /// <see cref="OutgoingEdgeCount"/> (and the
            /// <see cref="SelfLoopEdgeCount"/> if 
            /// <paramref name="includeLoops"/> is true).</returns>
            public int TotalEdgeCount(bool includeLoops)
            {
                return includeLoops
                    ? this.mSrcCount + this.mDstCount + this.mLoopCount
                    : this.mSrcCount + this.mDstCount;
            }

            #endregion

            public override string ToString()
            {
                string str;
                try
                {
                    str = this.Data.ToString();
                }
                catch (NullReferenceException)
                {
                    str = "{NULL}";
                }
                catch (Exception ex)
                {
                    str = ex.ToString();
                }
                return str;
            }
        }

        private static readonly GNode[] sEmptyNodes = new GNode[0];
        private static readonly GEdge[] sEmptyEdges = new GEdge[0];

        private GNode[] mNodes;
        private int mNCount;
        private uint mNVers;

        private GEdge[] mEdges;
        private int mECount;
        private uint mEVers;

        /// <summary>
        /// Initializes a new <see cref="T:Digraph`2{Node,Edge}"/>
        /// instance that is empty and has the default initial capacities
        /// for its internal node and edge lists.
        /// </summary>
        public Digraph()
        {
            this.mNodes = sEmptyNodes;
            this.mNCount = 0;
            this.mNVers = 0;

            this.mEdges = sEmptyEdges;
            this.mECount = 0;
            this.mEVers = 0;
        }

        /// <summary>
        /// Initializes a new <see cref="T:Digraph`2{Node,Edge}"/>
        /// instance that is empty and has the specified initial capacities
        /// for its internal node and edge lists.
        /// </summary>
        /// <param name="nodeCapacity">The number of nodes that the new graph
        /// can initially store.</param>
        /// <param name="edgeCapacity">The number of edges that the new graph
        /// can initially store.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="nodeCapacity"/> and/or 
        /// <paramref name="edgeCapacity"/> are less than zero.</exception>
        public Digraph(int nodeCapacity, int edgeCapacity)
        {
            if (nodeCapacity < 0)
                throw new ArgumentOutOfRangeException("nodeCapacity");
            if (edgeCapacity < 0)
                throw new ArgumentOutOfRangeException("edgeCapacity");

            this.mNodes = nodeCapacity == 0 
                ? sEmptyNodes : new GNode[nodeCapacity];
            this.mNCount = 0;
            this.mNVers = 0;

            this.mEdges = edgeCapacity == 0 
                ? sEmptyEdges : new GEdge[edgeCapacity];
            this.mECount = 0;
            this.mEVers = 0;
        }

        /// <summary>
        /// Finds the "center" node of this graph based on an undirected 
        /// breadth first "pruning" of edges, starting from its "leaf" nodes
        /// (nodes with only one edge connecting them to the rest of the 
        /// graph) and working its way inward until a single "center" node 
        /// is left.</summary>
        /// <returns>Null if this graph could not be "pruned" because it has 
        /// no nodes or no edges or no "leaf" nodes; otherwise the "center" 
        /// found via the undirected breadth first "pruning".</returns>
        public GNode FindCenter(bool undirected, bool reversed)
        {
            if (this.mNCount == 0)
            {
                return null;
            }
            int i, degree = 0;
            for (i = 0; i < this.mNCount; i++)
            {
                if (!this.mNodes[i].Hidden)
                    degree++;
            }
            if (degree == 0)
            {
                // There are no visible nodes, 
                // so it's as if the graph is empty.
                return null;
            }
            else if (degree == 1 || this.mECount == 0)
            {
                // There aren't enough visible nodes to warrant pruning,
                // so just return the first visible node found.
                for (i = 0; i < this.mNCount; i++)
                {
                    if (!this.mNodes[i].Hidden)
                        return this.mNodes[i];
                }
            }
            GNode node;
            GEdge edge;
            int u, v = -1;
            int leafIndex = 0;
            int leafCount = 0;
            int[] degrees = new int[this.mNCount];
            int[] leaves = new int[degree];
            for (i = 0; i < this.mNCount; i++)
            {
                degrees[i] = 0;
            }
            for (i = 0; i < this.mECount; i++)
            {
                edge = this.mEdges[i];
                if (!edge.Hidden && 
                    edge.SrcNode.mIndex != edge.DstNode.mIndex)
                {
                    if (!edge.DstNode.Hidden && (undirected || !reversed))
                    {
                        degree = degrees[edge.DstNode.mIndex];
                        degrees[edge.DstNode.mIndex] = degree + 1;
                    }
                    if (!edge.SrcNode.Hidden && (undirected || reversed))
                    {
                        degree = degrees[edge.SrcNode.mIndex];
                        degrees[edge.SrcNode.mIndex] = degree + 1;
                    }
                }
            }
            for (i = 0; i < this.mNCount; i++)
            {
                node = this.mNodes[i];
                if (!node.Hidden && degrees[node.mIndex] == 1)
                    leaves[leafCount++] = node.mIndex;
            }
            if (leafCount == 0)
            {
                // There are no leaves to prune,
                // so just return the first visible node found.
                for (i = 0; i < this.mNCount; i++)
                {
                    if (!this.mNodes[i].Hidden)
                        return this.mNodes[i];
                }
            }
            while (leafIndex < leafCount)
            {
                v = leaves[leafIndex++];
                for (i = 0; i < this.mECount; i++)
                {
                    edge = this.mEdges[i];
                    if (!edge.Hidden &&
                        edge.SrcNode.mIndex != edge.DstNode.mIndex)
                    {
                        if (edge.DstNode.mIndex == v && 
                            !edge.SrcNode.Hidden &&
                            (undirected || !reversed))
                        {
                            u = edge.SrcNode.mIndex;
                            degree = degrees[u] - 1;
                            degrees[u] = degree;
                            if (degree == 1)
                                leaves[leafCount++] = u;
                        }
                        else if (edge.SrcNode.mIndex == v &&
                            !edge.DstNode.Hidden &&
                            (undirected || reversed))
                        {
                            u = edge.DstNode.mIndex;
                            degree = degrees[u] - 1;
                            degrees[u] = degree;
                            if (degree == 1)
                                leaves[leafCount++] = u;
                        }
                    }
                }
            }
            return v == -1 ? null : this.mNodes[v];
        }

        /*private class StackFrame
        {
            public int RootIndex;
            public int NodeIndex;
            public int EdgeIndex;
            public int Size;

            public StackFrame(int rootIndex, int nodeIndex, int edgeIndex, int size)
            {
                this.RootIndex = rootIndex;
                this.NodeIndex = nodeIndex;
                this.EdgeIndex = edgeIndex;
                this.Size = size;
            }
        }/* */

        public GNode FindPathCenter()
        {
            if (this.mNCount == 0)
            {
                return null;
            }
            int i, j = -1;
            int count = 0;
            for (i = 0; i < this.mNCount; i++)
            {
                if (!this.mNodes[i].Hidden)
                {
                    count++;
                    if (j == -1)
                        j = i;
                }
            }
            if (count == 0)
            {
                // There are no visible nodes, 
                // so it's as if the graph is empty.
                return null;
            }
            else if (count < 3 || this.mECount < 2)
            {
                // There are not enough edges or visible nodes to make
                // a path with an intermediate node as their center,
                // so just return the first visible node found.
                return this.mNodes[j];
            }
            int pathCount, maxPathCount = -1;
            int ni, pi, size, n1, n2, center = 0;
            Digraph<Node, Edge>.GEdge edge1, edge2;
            int[] sizes = new int[this.mNCount];
            for (i = 0; i < this.mNCount; i++)
            {
                sizes[i] = -1;
            }
            // Calculate the number of child nodes of the current node in
            // the tree by using a DFS algorithm, as it is the sum of the
            // number of child nodes in each of its branches, which might
            // not be leaves.
            int[] frame;
            int[][] stack = new int[count][];
            //Stack<int[]> stack = new Stack<int[]>(this.mNCount);
            //stack.Push(new int[] { -1, 0, 0, 0 });
            stack[0] = new int[] { -1, j, 0, 0 };
            count = 1;
            while (count > 0)
            {
                frame = stack[--count];//stack.Pop();
                pi = frame[0];//frame.RootIndex;
                ni = frame[1];//frame.NodeIndex;
                i  = frame[2];//frame.EdgeIndex;
                size = frame[3];//frame.Size;
                if (i > 0)
                {
                    edge1 = this.mEdges[i - 1];
                    if (edge1.SrcNode.mIndex == ni)
                        n1 = edge1.DstNode.mIndex;
                    else
                        n1 = edge1.SrcNode.mIndex;
                    size += sizes[n1];
                }
                while (i < this.mECount)
                {
                    edge1 = this.mEdges[i];
                    i++;
                    if (!edge1.Hidden &&
                        !edge1.SrcNode.Hidden && !edge1.DstNode.Hidden)
                    {
                        n1 = -1;
                        if (edge1.SrcNode.mIndex == ni &&
                            edge1.DstNode.mIndex != ni &&
                            edge1.DstNode.mIndex != pi)
                        {
                            n1 = edge1.DstNode.mIndex;
                        }
                        else if (edge1.DstNode.mIndex == ni &&
                                 edge1.SrcNode.mIndex != pi)
                        {
                            n1 = edge1.SrcNode.mIndex;
                        }
                        if (n1 != -1)
                        {
                            //stack.Push(new int[] { pi, ni, ei, size });
                            stack[count++] = new int[] { pi, ni, i, size };
                            pi = ni;
                            ni = n1;
                            i = 0;
                            size = 0;
                        }
                    }
                }
                sizes[ni] = size + 1;
                // The initial path count is the product of the number of
                // nodes that come before the current node in the DFS tree
                // and the number of child nodes that come after it, 
                // as there are undirected paths connecting each of them 
                // that go through the current node.
                pathCount = size * (this.mNCount - 1 - size);
                // The rest of the path count is calculated from the products
                // of the permutations of each child branch of the current
                // node in the DFS tree, as there are undirected paths
                // connecting each child in each branch to the children in
                // all the other branches that go through the current node.
                for (i = 0; i < this.mECount; i++)
                {
                    edge1 = this.mEdges[i];
                    if (!edge1.Hidden &&
                        !edge1.SrcNode.Hidden && !edge1.DstNode.Hidden)
                    {
                        n1 = -1;
                        if (edge1.SrcNode.mIndex == ni &&
                            edge1.DstNode.mIndex != ni &&
                            edge1.DstNode.mIndex != pi)
                        {
                            n1 = edge1.DstNode.mIndex;
                        }
                        else if (edge1.DstNode.mIndex == ni &&
                                 edge1.SrcNode.mIndex != pi)
                        {
                            n1 = edge1.SrcNode.mIndex;
                        }
                        if (n1 != -1)
                        {
                            for (j = i + 1; j < this.mECount; j++)
                            {
                                edge2 = this.mEdges[j];
                                if (!edge2.Hidden && !edge2.SrcNode.Hidden &&
                                    !edge2.DstNode.Hidden)
                                {
                                    n2 = -1;
                                    if (edge2.SrcNode.mIndex == ni &&
                                        edge2.DstNode.mIndex != ni &&
                                        edge2.DstNode.mIndex != pi)
                                    {
                                        n2 = edge2.DstNode.mIndex;
                                    }
                                    else if (edge2.DstNode.mIndex == ni &&
                                             edge2.SrcNode.mIndex != pi)
                                    {
                                        n2 = edge2.SrcNode.mIndex;
                                    }
                                    if (n2 != -1)
                                    {
                                        pathCount += sizes[n1] * sizes[n2];
                                    }
                                }
                            }
                        }
                    }
                }
                if (pathCount > maxPathCount)
                {
                    maxPathCount = pathCount;
                    center = ni;
                }
            }
            return this.mNodes[center];
        }

        public GNode FindWeightedCenter(bool undirected, bool reversed)
        {
            int[] center = new int[1];
            int[] sizes = new int[this.mNCount];
            for (int i = 0; i < this.mNCount; i++)
                sizes[i] = -1;
            this.CalcPathCount(undirected, reversed, 0, -1, center, sizes, -1);
            return this.mNodes[center[0]];
        }

        private int CalcPathCount(bool undirected, bool reversed, int index,
            int pIndex, int[] center, int[] sizes, int maxPathCount)
        {
            int k, m, n, size = 0;
            int node1, node2;
            Digraph<Node, Edge>.GEdge edge1, edge2;
            for (m = 0; m < this.mECount; m++)
            {
                edge1 = this.mEdges[m];
                if (!edge1.Hidden)
                {
                    node1 = -1;
                    if ((undirected || !reversed) &&
                        edge1.SrcNode.mIndex == index &&
                        edge1.DstNode.mIndex != pIndex)
                    {
                        node1 = edge1.DstNode.mIndex;
                    }
                    else if ((undirected || reversed) &&
                        edge1.DstNode.mIndex == index &&
                        edge1.SrcNode.mIndex != pIndex)
                    {
                        node1 = edge1.SrcNode.mIndex;
                    }
                    if (node1 != -1)
                    {
                        k = CalcPathCount(undirected, reversed, node1, index, center, sizes, maxPathCount);
                        if (k > maxPathCount)
                            maxPathCount = k;
                        size += sizes[node1];
                    }
                }
            }
            int pathCount = size * (this.mNCount - 1 - size);
            for (m = 0; m < this.mECount; m++)
            {
                edge1 = this.mEdges[m];
                if (!edge1.Hidden)
                {
                    node1 = -1;
                    if ((undirected || !reversed) &&
                        edge1.SrcNode.mIndex == index &&
                        edge1.DstNode.mIndex != pIndex)
                    {
                        node1 = edge1.DstNode.mIndex;
                    }
                    else if ((undirected || reversed) &&
                        edge1.DstNode.mIndex == index &&
                        edge1.SrcNode.mIndex != pIndex)
                    {
                        node1 = edge1.SrcNode.mIndex;
                    }
                    if (node1 != -1)
                    {
                        for (n = m + 1; n < this.mECount; n++)
                        {
                            edge2 = this.mEdges[m];
                            if (!edge2.Hidden)
                            {
                                node2 = -1;
                                if ((undirected || !reversed) &&
                                    edge2.SrcNode.mIndex == index &&
                                    edge2.DstNode.mIndex != pIndex)
                                {
                                    node2 = edge2.DstNode.mIndex;
                                }
                                else if ((undirected || reversed) &&
                                    edge2.DstNode.mIndex == index &&
                                    edge2.SrcNode.mIndex != pIndex)
                                {
                                    node2 = edge2.SrcNode.mIndex;
                                }
                                if (node2 != null)
                                {
                                    pathCount += sizes[node1] * sizes[node2];
                                }
                            }
                        }
                    }
                }
            }
            sizes[index] = size + 1;
            if (pathCount > maxPathCount)
            {
                maxPathCount = pathCount;
                center[0] = index;
            }
            return maxPathCount;
        }

        #region Node List Manipulation

        #region Node List Properties
        /// <summary>
        /// The number of <typeparamref name="Node"/> instances currently
        /// contained in this <see cref="T:Digraph`2{Node,Edge}"/>.
        /// </summary>
        public int NodeCount
        {
            get { return this.mNCount; }
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
            get { return this.mNodes.Length; }
            set
            {
                if (value < this.mNCount)
                    throw new ArgumentOutOfRangeException("NodeCapacity");
                if (value != this.mNodes.Length)
                {
                    if (value > 0)
                    {
                        GNode[] nodes = new GNode[value];
                        if (this.mNCount > 0)
                        {
                            Array.Copy(this.mNodes, 0, nodes, 0, 
                                this.mNCount);
                        }
                        this.mNodes = nodes;
                    }
                    else
                    {
                        this.mNodes = sEmptyNodes;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the current "version" of this graph's internal node list,
        /// which is incremented each time a node is inserted into or 
        /// removed from this graph.</summary><seealso cref="EdgeVersion"/>
        public uint NodeVersion
        {
            get { return this.mNVers; }
        }

        /// <summary>
        /// An array of all the <typeparamref name="Node"/> instances
        /// contained in this <see cref="T:Digraph`2{Node,Edge}"/>,
        /// in the same order as all functions with a <c>nodeIndex</c>
        /// argument.</summary><seealso cref="InternalNodes"/>
        /// <seealso cref="Edges"/>
        public Node[] Nodes
        {
            get
            {
                Node[] nodes = new Node[this.mNCount];
                for (int i = 0; i < this.mNCount; i++)
                    nodes[i] = this.mNodes[i].Data;
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
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="nodeIndex"/> is less than <c>0</c> or greater 
        /// than or equal to <see cref="NodeCount"/>.</exception>
        public Node NodeAt(int nodeIndex)
        {
            if (nodeIndex < 0 || nodeIndex >= this.mNCount)
                throw new ArgumentOutOfRangeException("nodeIndex");
            return this.mNodes[nodeIndex].Data;
        }

        /// <summary>
        /// An array of all the <see cref="GNode"/> instances currently
        /// contained within this <see cref="T:Digraph`2{Node,Edge}"/>,
        /// in the same order as all functions with a <c>nodeIndex</c>
        /// argument.</summary><seealso cref="Nodes"/>
        /// <seealso cref="InternalEdges"/>
        public GNode[] InternalNodes
        {
            get 
            {
                GNode[] nodes = new GNode[this.mNCount];
                if (this.mNCount > 0)
                    Array.Copy(this.mNodes, 0, nodes, 0, this.mNCount);
                return nodes;
            }
        }

        /// <summary>
        /// Retrieves the <see cref="GNode"/> instance at the given
        /// <paramref name="nodeIndex"/> in this directional graph's
        /// internal list of <see cref="InternalNodes"/>.
        /// </summary>
        /// <param name="nodeIndex">The index of the <see cref="GNode"/>
        /// instance to retrieve from this directional graph's internal list of
        /// <see cref="InternalNodes"/>.</param>
        /// <returns>The <see cref="GNode"/> instance at 
        /// <paramref name="nodeIndex"/> in this directional graph's internal
        /// list of <see cref="InternalNodes"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="nodeIndex"/> is less than <c>0</c> or greater 
        /// than or equal to <see cref="NodeCount"/>.</exception>
        public GNode InternalNodeAt(int nodeIndex)
        {
            //if (nodeIndex < 0 || nodeIndex >= this.mNCount)
            //    throw new ArgumentOutOfRangeException("nodeIndex");
            return this.mNodes[nodeIndex];
        }

        /// <summary>
        /// A convenience function that retrieves the <see cref="GNode"/>
        /// instance that corresponds to and contains the given
        /// <paramref name="node"/>.  If <paramref name="node"/> is not in
        /// this graph, null is returned.
        /// </summary>
        /// <param name="node">The <typeparamref name="Node"/> instance to
        /// find the corresponding <see cref="GNode"/> instance.</param>
        /// <returns>The <see cref="GNode"/> instance corresponding to
        /// <paramref name="node"/>, or null if <paramref name="node"/> is not
        /// in this graph.</returns>
        public GNode InternalNodeFor(Node node)
        {
            int index = this.IndexOfNode(node);
            if (index < 0)
                return null;
            return this.mNodes[index];
        }
        #endregion

        #region Searching for Nodes
        /// <summary>
        /// Searches for the specified <typeparamref name="Node"/> instance
        /// and returns the zero-based index of its occurrence within this
        /// <see cref="T:Digraph`2{Node,Edge}"/>'s internal list of 
        /// <see cref="Nodes"/>.</summary>
        /// <param name="node">The <typeparamref name="Node"/> to locate
        /// in this <see cref="T:Digraph`2{Node,Edge}"/>.</param>
        /// <returns>The zero-based index of the occurrence of 
        /// <paramref name="node"/> within this <see 
        /// cref="T:Digraph`2{Node,Edge}"/>, if found; otherwise, -1.
        /// </returns><seealso cref="IndexOfEdge(Node,Node)"/>
        public int IndexOfNode(Node node)
        {
            EqualityComparer<Node> ec = EqualityComparer<Node>.Default;
            for (int i = 0; i < this.mNCount; i++)
            {
                if (ec.Equals(this.mNodes[i].Data, node))
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Determines whether the given <typeparamref name="Node"/> is in
        /// this <see cref="T:Digraph`2{Node,Edge}"/>.
        /// </summary>
        /// <param name="node">The <typeparamref name="Node"/> instance to
        /// locate in this <see cref="T:Digraph`2{Node,Edge}"/>.
        /// </param>
        /// <returns>true if <paramref name="node"/> is found in this
        /// <see cref="T:Digraph`2{Node,Edge}"/>; otherwise, false.
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
        /// <see cref="T:Digraph`2{Node,Edge}"/>, if it isn't already
        /// contained in this graph.</summary>
        /// <param name="node">The <typeparamref name="Node"/> instance to be
        /// added to the end of this <see cref="T:Digraph`2{Node,Edge}"/>.
        /// </param>
        /// <returns>true if the <paramref name="node"/> is added to the end of
        /// this <see cref="T:Digraph`2{Node,Edge}"/>, false if it's already
        /// contained in it.</returns><seealso cref="AddEdge(Edge)"/>
        public bool AddNode(Node node)
        {
            // Don't allow duplicates
            if (this.IndexOfNode(node) < 0)
            {
                // Ensure Capacity
                if (this.mNCount == this.mNodes.Length)
                {
                    if (this.mNCount == 0)
                    {
                        this.mNodes = new GNode[4];
                    }
                    else
                    {
                        GNode[] nodes = new GNode[2 * this.mNCount];
                        Array.Copy(this.mNodes, 0, nodes, 0, this.mNCount);
                        this.mNodes = nodes;
                    }
                }
                GNode gNode = new GNode(node, this.mNCount);
                this.mNodes[this.mNCount++] = gNode;
                this.mNVers++;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Inserts a <typeparamref name="Node"/> instance into this 
        /// <see cref="T:Digraph`2"/> at the specified 
        /// <paramref name="index"/>, if it isn't already contained 
        /// in this graph.</summary>
        /// <param name="index">The zero-based index at which 
        /// <paramref name="node"/> should be inserted.</param>
        /// <param name="node">The <typeparamref name="Node"/> instance
        /// to insert into this graph.</param>
        /// <returns>true if the given <paramref name="node"/> is inserted 
        /// into this <see cref="T:Digraph`2{Node,Edge}"/> at the given 
        /// <paramref name="index"/>, or false if it's already contained 
        /// in this graph.</returns>
        public bool InsertNode(int index, Node node)
        {
            if (index < 0 || index > this.mNCount)
                throw new ArgumentOutOfRangeException("index");
            // Don't allow duplicates
            if (this.IndexOfNode(node) < 0)
            {
                // Ensure Capacity
                if (this.mNCount == this.mNodes.Length)
                {
                    if (this.mNCount == 0)
                    {
                        this.mNodes = new GNode[4];
                    }
                    else
                    {
                        GNode[] nodes = new GNode[2 * this.mNCount];
                        Array.Copy(this.mNodes, 0, nodes, 0, this.mNCount);
                        this.mNodes = nodes;
                    }
                }
                if (index < this.mNCount)
                {
                    Array.Copy(this.mNodes, index, this.mNodes, index + 1, 
                        this.mNCount - index);
                }
                this.mNodes[index] = new GNode(node, index);
                this.mNCount++;
                for (int i = index + 1; i < this.mNCount; i++)
                {
                    this.mNodes[i].mIndex = i;
                }
                this.mNVers++;
                return true;
            }
            return false;
        }
        #endregion

        /// <summary>
        /// If <paramref name="oldNode"/> is found within this
        /// <see cref="T:Digraph`2{Node,Edge}"/>, it is replaced with 
        /// <paramref name="newNode"/>, but only if <paramref name="newNode"/>
        /// isn't already contained in this <see cref="T:Digraph`2{Node,Edge}"/>.
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
            EqualityComparer<Node> ec = EqualityComparer<Node>.Default;
            if (ec.Equals(oldNode, newNode))
                return true;
            // Don't allow duplicates
            int index = this.IndexOfNode(newNode);
            if (index >= 0)
            {
                // TODO: Should we remove the oldNode anyway?
                return false;
            }
            index = this.IndexOfNode(oldNode);
            if (index < 0)
                return false;
            GNode gNode = new GNode(newNode, index);
            gNode.mLoopCount = this.mNodes[index].mLoopCount;
            gNode.mSrcCount = this.mNodes[index].mSrcCount;
            gNode.mDstCount = this.mNodes[index].mDstCount;
            this.mNodes[index] = gNode;
            if (this.mECount > 0 &&
                gNode.TotalEdgeCount(true) > 0)
            {
                GEdge gEdge;
                for (int i = 0; i < this.mECount; i++)
                {
                    gEdge = this.mEdges[i];
                    if (gEdge.SrcNode.mIndex == index)
                    {
                        gEdge.Data.SetSrcNode(newNode);
                        if (gEdge.DstNode.mIndex == index)
                        {
                            this.mEdges[i] = new GEdge(
                                gNode, gNode, gEdge.Data);
                        }
                        else
                        {
                            this.mEdges[i] = new GEdge(
                                gNode, gEdge.DstNode, gEdge.Data);
                        }
                    }
                    else if (gEdge.DstNode.mIndex == index)
                    {
                        gEdge.Data.SetDstNode(newNode);
                        this.mEdges[i] = new GEdge(
                            gEdge.SrcNode, gNode, gEdge.Data);
                    }
                }
            }
            return true;
        }

        #region Hiding Nodes
        /// <summary>
        /// Sets to true the <see cref="GNode.Hidden"/> property of the 
        /// <see cref="GNode"/> corresponding to the given 
        /// <paramref name="node"/> in the graph if the given
        /// <paramref name="node"/> is in the graph.</summary>
        /// <param name="node">The <typeparamref name="Node"/> instance
        /// to hide from algorithms that will process this graph.</param>
        /// <seealso cref="UnhideNode(Node)"/>
        public void HideNode(Node node)
        {
            int index = this.IndexOfNode(node);
            if (index >= 0)
            {
                this.mNodes[index].mHidden = true;
                this.mNVers++;
            }
        }
        /// <summary>
        /// Sets to true the <see cref="GNode.Hidden"/> property of the
        /// <see cref="GNode"/> at the given <see cref="nodeIndex"/> in
        /// this graph's internal list of nodes.
        /// </summary>
        /// <param name="nodeIndex">The index of the node in this graph
        /// to hide from algorithms that will process this graph.</param>
        /// <seealso cref="UnhideNodeAt(int)"/>
        public void HideNodeAt(int nodeIndex)
        {
            if (nodeIndex < 0 || nodeIndex >= this.mNCount)
                throw new ArgumentOutOfRangeException("nodeIndex");
            this.mNodes[nodeIndex].mHidden = true;
            this.mNVers++;
        }
        /// <summary>
        /// Sets to true the <see cref="GNode.Hidden"/> property of every
        /// <see cref="GNode"/> in this graph, making it appear empty to
        /// any algorithm that will process it.</summary>
        /// <seealso cref="UnhideAllNodes()"/>
        public void HideAllNodes()
        {
            for (int i = 0; i < this.mNCount; i++)
            {
                this.mNodes[i].mHidden = true;
            }
            this.mNVers++;
        }
        /// <summary>
        /// Sets to false the <see cref="GNode.Hidden"/> property of the 
        /// <see cref="GNode"/> corresponding to the given 
        /// <paramref name="node"/> in the graph if the given
        /// <paramref name="node"/> is in the graph.</summary>
        /// <param name="node">The <typeparamref name="Node"/> instance
        /// to unhide from algorithms that will process this graph.</param>
        /// <seealso cref="HideNode(Node)"/>
        public void UnhideNode(Node node)
        {
            int index = this.IndexOfNode(node);
            if (index >= 0)
            {
                this.mNodes[index].mHidden = false;
                this.mNVers++;
            }
        }
        /// <summary>
        /// Sets to false the <see cref="GNode.Hidden"/> property of the
        /// <see cref="GNode"/> at the given <see cref="nodeIndex"/> in
        /// this graph's internal list of nodes.
        /// </summary>
        /// <param name="nodeIndex">The index of the node in this graph
        /// to unhide from algorithms that will process this graph.</param>
        /// <seealso cref="HideNodeAt(int)"/>
        public void UnhideNodeAt(int nodeIndex)
        {
            if (nodeIndex < 0 || nodeIndex >= this.mNCount)
                throw new ArgumentOutOfRangeException("nodeIndex");
            this.mNodes[nodeIndex].mHidden = false;
            this.mNVers++;
        }
        /// <summary>
        /// Sets to false the <see cref="GNode.Hidden"/> property of every
        /// <see cref="GNode"/> in this graph.</summary>
        /// <seealso cref="HideAllNodes()"/>
        public void UnhideAllNodes()
        {
            for (int i = 0; i < this.mNCount; i++)
            {
                this.mNodes[i].mHidden = false;
            }
            this.mNVers++;
        }

        #endregion

        #region Removing Nodes
        /// <summary>
        /// Removes the specified <typeparamref name="Node"/> instance from
        /// this <see cref="T:Digraph`2{Node,Edge}"/>, along with all
        /// <typeparamref name="Edge"/> instances connecting it to the rest of 
        /// this graph, but doesn't remove any nodes orphaned by the removal
        /// of <paramref name="node"/>.</summary>
        /// <param name="node">The <typeparamref name="Node"/> instance to 
        /// remove from this <see cref="T:Digraph`2{Node,Edge}"/>.</param>
        /// <returns>true if <paramref name="node"/> is successfully removed;
        /// otherwise false if <paramref name="node"/> wasn't found in this
        /// <see cref="T:Digraph`2{Node,Edge}"/>.</returns>
        /// <seealso cref="RemoveNodeAt(int)"/>
        /// <seealso cref="OrphanNode(Node)"/>
        /// <seealso cref="RemoveEdge(Node,Node)"/>
        public bool RemoveNode(Node node)
        {
            int i = this.IndexOfNode(node);
            if (i < 0) 
                return false;
            this.CoreRemoveNodeAt(i, false);
            return true;
        }

        /// <summary>
        /// Removes the specified <typeparamref name="Node"/> instance from
        /// this <see cref="T:Digraph`2{Node,Edge}"/>, along with all
        /// <typeparamref name="Edge"/> instances connecting it to the rest of 
        /// this graph.</summary>
        /// <param name="node">The <typeparamref name="Node"/> instance to 
        /// remove from this <see cref="T:Digraph`2{Node,Edge}"/>.</param>
        /// <param name="removeOrphans">Whether or not to remove any 
        /// <typeparamref name="Node"/> instances orphaned by the removal of
        /// <paramref name="node"/>.</param>
        /// <returns>true if <paramref name="node"/> is successfully removed;
        /// otherwise false if <paramref name="node"/> wasn't found in this
        /// <see cref="T:Digraph`2{Node,Edge}"/>.</returns>
        /// <seealso cref="RemoveNodeAt(int,bool)"/>
        /// <seealso cref="OrphanNode(Node)"/>
        /// <seealso cref="RemoveEdge(Node,Node,bool)"/>
        public bool RemoveNode(Node node, bool removeOrphans)
        {
            int i = this.IndexOfNode(node);
            if (i < 0) 
                return false;
            this.CoreRemoveNodeAt(i, removeOrphans);
            return true;
        }

        /// <summary>
        /// Removes the <typeparamref name="Node"/> instance at the specified
        /// index in this <see cref="T:Digraph`2{Node,Edge}"/>'s list of
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
            if (nodeIndex < 0 || nodeIndex >= this.mNCount)
                throw new ArgumentOutOfRangeException("nodeIndex");
            this.CoreRemoveNodeAt(nodeIndex, false);
        }

        /// <summary>
        /// Removes the <typeparamref name="Node"/> instance at the specified
        /// index in this <see cref="T:Digraph`2{Node,Edge}"/>'s list of
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
            if (nodeIndex < 0 || nodeIndex >= this.mNCount)
                throw new ArgumentOutOfRangeException("nodeIndex");
            this.CoreRemoveNodeAt(nodeIndex, removeOrphans);
        }

        private void CoreRemoveNodeAt(int index, bool removeOrphans)
        {
            int i;
            bool rem = false;
            for (i = 0; i < this.mNCount; i++)
            {
                this.mNodes[i].Color = GraphColor.White;
            }
            if (this.mECount > 0)
            {
                GEdge gEdge = this.mEdges[this.mECount - 1];
                if (gEdge.SrcNode.mIndex == index)
                {
                    this.mECount--;
                    this.mEdges[this.mECount] = null;
                    if (gEdge.DstNode.mIndex == index)
                    {
                        gEdge.SrcNode.mLoopCount--;
                    }
                    else
                    {
                        gEdge.SrcNode.mSrcCount--;
                        gEdge.DstNode.mDstCount--;
                    }
                    rem = true;
                }
                else if (gEdge.DstNode.mIndex == index)
                {
                    this.mECount--;
                    this.mEdges[this.mECount] = null;
                    gEdge.SrcNode.mSrcCount--;
                    gEdge.DstNode.mDstCount--;
                    rem = true;
                }
                for (i = this.mECount - (rem ? 1 : 2); i >= 0; i--)
                {
                    gEdge = this.mEdges[i];
                    if (gEdge.SrcNode.mIndex == index)
                    {
                        this.mECount--;
                        Array.Copy(this.mEdges, i + 1, this.mEdges, i,
                            this.mECount - i);
                        this.mEdges[this.mECount] = null;
                        if (gEdge.DstNode.mIndex == index)
                        {
                            gEdge.SrcNode.mLoopCount--;
                        }
                        else
                        {
                            gEdge.SrcNode.mSrcCount--;
                            gEdge.DstNode.mDstCount--;
                        }
                        rem = true;
                        // Mark the opposite node as a potential orphan
                        gEdge.DstNode.Color = GraphColor.Gray;
                    }
                    else if (gEdge.DstNode.mIndex == index)
                    {
                        this.mECount--;
                        Array.Copy(this.mEdges, i + 1, this.mEdges, i,
                            this.mECount - i);
                        this.mEdges[this.mECount] = null;
                        gEdge.SrcNode.mSrcCount--;
                        gEdge.DstNode.mDstCount--;
                        rem = true;
                        // Mark the opposite node as a potential orphan
                        gEdge.SrcNode.Color = GraphColor.Gray;
                    }
                }
                if (rem)
                {
                    this.mEVers++;
                    if (removeOrphans)
                    {
                        // Mark newly orphaned nodes for removal
                        GNode gNode;
                        for (i = 0; i < this.mNCount; i++)
                        {
                            gNode = this.mNodes[i];
                            if (gNode.Color == GraphColor.Gray &&
                                gNode.mSrcCount + gNode.mDstCount == 0)
                                gNode.Color = GraphColor.Black;
                        }
                        // Mark the node at the given index for removal
                        this.mNodes[index].Color = GraphColor.Black;
                        // Remove all nodes marked for removal
                        rem = false;
                        gNode = this.mNodes[this.mNCount - 1];
                        if (gNode.Color == GraphColor.Black)
                        {
                            this.mNCount--;
                            this.mNodes[this.mNCount] = null;
                            rem = true;
                        }
                        for (i = this.mNCount - (rem ? 1 : 2); i >= 0; i--)
                        {
                            if (this.mNodes[i].Color == GraphColor.Black)
                            {
                                this.mNodes[i].mIndex = -1;
                                this.mNCount--;
                                Array.Copy(this.mNodes, i + 1,
                                    this.mNodes, i, this.mNCount - i);
                                this.mNodes[this.mNCount] = null;
                            }
                        }
                        // Update the indexes of all the nodes
                        for (i = 0; i < this.mNCount; i++)
                        {
                            this.mNodes[i].mIndex = i;
                        }
                        rem = true;
                    }
                    else
                    {
                        rem = false;
                    }
                }
            }
            if (!rem)
            {
                this.mNodes[index].mIndex = -1;
                this.mNCount--;
                if (this.mNCount > 0)
                {
                    Array.Copy(this.mNodes, index + 1,
                        this.mNodes, index, this.mNCount - i);
                    // Update the indexes of the nodes
                    for (i = index; i < this.mNCount; i++)
                    {
                        this.mNodes[i].mIndex = i;
                    }
                }
                this.mNodes[this.mNCount] = null;
            }
            this.mNVers++;
        }
        #endregion

        #region Orphan Nodes
        /// <summary>
        /// Removes all <typeparamref name="Edge"/> instances connecting the
        /// given <typeparamref name="Node"/> instance to all other instances 
        /// in this <see cref="T:Digraph`2{Node,Edge}"/>, essentially
        /// orphaning it from the rest of this graph.</summary>
        /// <param name="node">The <typeparamref name="Node"/> instance to be
        /// orphaned from the rest of this <see cref="T:Digraph`2{Node,Edge}"/>.
        /// </param>
        /// <returns>true if <paramref name="node"/> is successfully orphaned;
        /// otherwise, false if <paramref name="node"/> wasn't found in this
        /// <see cref="T:Digraph`2{Node,Edge}"/>.</returns>
        /// <seealso cref="OrphanNodeAt(int)"/>
        /// <seealso cref="RemoveNode(Node,bool)"/>
        /// <seealso cref="RemoveEdge(Node,Node,bool)"/>
        public bool OrphanNode(Node node)
        {
            int i = this.IndexOfNode(node);
            if (i < 0) 
                return false;
            this.CoreOrphanNodeAt(i);
            return true;
        }

        /// <summary>
        /// Removes all <typeparamref name="Edge"/> instances connecting the
        /// <typeparamref name="Node"/> instance at the given index to all 
        /// other instances in this <see cref="T:Digraph`2{Node,Edge}"/>,
        /// essentially orphaning it from the rest of this graph.</summary>
        /// <param name="nodeIndex">The zero-based index of the <typeparamref 
        /// name="Node"/> instance to be orphaned.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref 
        /// name="nodeIndex"/> is less than 0 or <paramref name="nodeIndex"/>
        /// is greater than or equal to <see cref="NodeCount"/>.</exception>
        /// <seealso cref="RemoveNodeAt(int,bool)"/>
        public void OrphanNodeAt(int nodeIndex)
        {
            if (nodeIndex < 0 || nodeIndex >= this.mNCount)
                throw new ArgumentOutOfRangeException("nodeIndex");
            this.CoreOrphanNodeAt(nodeIndex);
        }

        private void CoreOrphanNodeAt(int index)
        {
            GNode gNode = this.mNodes[index];
            if (this.mECount > 0 && gNode.mSrcCount + gNode.mDstCount > 0)
            {
                int i;
                for (i = 0; i < this.mNCount; i++)
                {
                    this.mNodes[i].mIndex = i;
                }
                bool rem = false;
                GEdge gEdge = this.mEdges[this.mECount - 1];
                if (gEdge.SrcNode.mIndex == index &&
                    gEdge.DstNode.mIndex != index)
                {
                    this.mECount--;
                    this.mEdges[this.mECount] = null;
                    gEdge.SrcNode.mSrcCount--;
                    gEdge.DstNode.mDstCount--;
                    rem = true;
                }
                else if (gEdge.DstNode.mIndex == index)
                {
                    this.mECount--;
                    this.mEdges[this.mECount] = null;
                    gEdge.SrcNode.mSrcCount--;
                    gEdge.DstNode.mDstCount--;
                    rem = true;
                }
                for (i = this.mECount - (rem ? 1 : 2); i >= 0; i--)
                {
                    gEdge = this.mEdges[i];
                    if (gEdge.SrcNode.mIndex == index &&
                        gEdge.DstNode.mIndex != index)
                    {
                        this.mECount--;
                        Array.Copy(this.mEdges, i + 1, this.mEdges, i,
                            this.mECount - i);
                        this.mEdges[this.mECount] = null;
                        gEdge.SrcNode.mSrcCount--;
                        gEdge.DstNode.mDstCount--;
                    }
                    else if (gEdge.DstNode.mIndex == index)
                    {
                        this.mECount--;
                        Array.Copy(this.mEdges, i + 1, this.mEdges, i,
                            this.mECount - i);
                        this.mEdges[this.mECount] = null;
                        gEdge.SrcNode.mSrcCount--;
                        gEdge.DstNode.mDstCount--;
                    }
                }
                this.mEVers++;
            }
        }

        /// <summary>
        /// Retrieves all orphaned <typeparamref name="Node"/> instances (nodes
        /// without any <typeparamref name="Edge"/> instances connecting them
        /// to any other <typeparamref name="Node"/> instances) from this
        /// <see cref="T:Digraph`2{Node,Edge}"/>.
        /// </summary>
        /// <returns>An array containing all orphaned <typeparamref 
        /// name="Node"/> currently in this <see cref="T:Digraph`2{Node,Edge}"/>.
        /// </returns><seealso cref="Nodes"/>
        /// <seealso cref="OrphanNode(Node)"/>
        public Node[] FindOrphanedNodes()
        {
            if (this.mNCount == 0)
                return new Node[0];
            if (this.mECount == 0)
                return this.Nodes;
            Node[] orphans1 = new Node[this.mNCount];
            GNode gNode;
            int count = 0;
            for (int i = 0; i < count; i++)
            {
                gNode = this.mNodes[i];
                if (gNode.mSrcCount + gNode.mDstCount == 0)
                    orphans1[count++] = gNode.Data;
            }
            if (count == 0)
            {
                return new Node[0];
            }
            Node[] orphans2 = new Node[count];
            Array.Copy(orphans1, 0, orphans2, 0, count);
            return orphans2;
        }

        /// <summary>
        /// Retrieves all orphaned <see cref="GNode"/> instances (nodes
        /// without any <typeparamref name="Edge"/> instances connecting them
        /// to any other <see cref="GNode"/> instances) from this
        /// <see cref="T:Digraph`2{Node,Edge}"/>.
        /// </summary>
        /// <returns>An array containing all orphaned <typeparamref 
        /// name="Node"/> currently in this <see cref="T:Digraph`2{Node,Edge}"/>.
        /// </returns><seealso cref="InternalNodes"/>
        /// <seealso cref="OrphanNode(Node)"/>
        public GNode[] FindOrphanedInternalNodes()
        {
            if (this.mNCount == 0)
                return new GNode[0];
            if (this.mECount == 0)
                return this.InternalNodes;
            GNode[] orphans1 = new GNode[this.mNCount];
            GNode gNode;
            int count = 0;
            for (int i = 0; i < this.mNCount; i++)
            {
                gNode = this.mNodes[i];
                if (gNode.mSrcCount + gNode.mDstCount == 0)
                    orphans1[count++] = gNode;
            }
            if (count == 0)
            {
                return new GNode[0];
            }
            GNode[] orphans2 = new GNode[count];
            Array.Copy(orphans1, 0, orphans2, 0, count);
            return orphans2;
        }

        /// <summary>
        /// Removes all orphaned <typeparamref name="Node"/> instances (nodes
        /// without any <typeparamref name="Edge"/> instances connecting them
        /// to any other <typeparamref name="Node"/> instances) from this
        /// <see cref="T:Digraph`2{Node,Edge}"/>.
        /// </summary><remarks>
        /// Remember that this will remove any <typeparamref name="Node"/>
        /// instances recently added without any connections yet, as well as
        /// any orphaned as a side-effect of a function, including
        /// <see cref="RemoveNode(Node)"/>, <see cref="RemoveEdge(Node,Node)"/>
        /// and <see cref="OrphanNode(Node)"/>.</remarks>
        public void ClearOrphanedNodes()
        {
            if (this.mECount == 0)
                this.ClearNodes();
            bool rem = false;
            GNode gNode = this.mNodes[this.mNCount - 1];
            if (gNode.mSrcCount + gNode.mDstCount == 0)
            {
                this.mNCount--;
                this.mNodes[this.mNCount] = null;
                rem = true;
            }
            for (int i = this.mNCount - (rem ? 1 : 2); i >= 0; i--)
            {
                gNode = this.mNodes[i];
                if (gNode.mSrcCount + gNode.mDstCount == 0)
                {
                    this.mNCount--;
                    Array.Copy(this.mNodes, i + 1, this.mNodes, i,
                        this.mNCount - i);
                    this.mNodes[this.mNCount] = null;
                }
            }
        }
        #endregion

        /// <summary>
        /// Removes all <typeparamref name="Node"/> instances from this <see 
        /// cref="T:Digraph`2{Node,Edge}"/>, as well as all <typeparamref 
        /// name="Edge"/> instances, since they're meaningless without any
        /// <typeparamref name="Node"/> instances.
        /// </summary><seealso cref="ClearOrphanedNodes()"/>
        /// <seealso cref="ClearEdges()"/>.
        public void ClearNodes()
        {
            if (this.mECount > 0)
            {
                Array.Clear(this.mEdges, 0, this.mECount);
                this.mECount = 0;
                this.mEVers++;
            }
            if (this.mNCount > 0)
            {
                for (int i = 0; i < this.mNCount; i++)
                {
                    this.mNodes[i].mIndex = -1;
                }
                Array.Clear(this.mNodes, 0, this.mNCount);
                this.mNCount = 0;
                this.mNVers++;
            }
        }

        #endregion

        #region Edge List Manipulation

        #region Edge List Properties
        /// <summary>
        /// The number of <typeparamref name="Edge"/> instances currently
        /// contained in this <see cref="T:Digraph`2{Node,Edge}"/>.
        /// </summary><seealso cref="NodeCount"/>
        public int EdgeCount
        {
            get { return this.mECount; }
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
            get { return this.mEdges.Length; }
            set
            {
                if (value < this.mECount)
                    throw new ArgumentOutOfRangeException("NodeCapacity");
                if (value != this.mEdges.Length)
                {
                    if (value > 0)
                    {
                        GEdge[] edges = new GEdge[value];
                        if (this.mECount > 0)
                        {
                            Array.Copy(this.mEdges, 0, edges, 0,
                                this.mECount);
                        }
                        this.mEdges = edges;
                    }
                    else
                    {
                        this.mEdges = sEmptyEdges;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the current "version" of this graph's internal edge list,
        /// which is incremented each time an edge is inserted into or 
        /// removed from this graph.</summary><seealso cref="NodeVersion"/>
        public uint EdgeVersion
        {
            get { return this.mEVers; }
        }

        /// <summary>
        /// An array of all the <typeparamref name="Edge"/> instances
        /// contained in this <see cref="T:Digraph`2{Node,Edge}"/>,
        /// in the same order as all functions with an <c>edgeIndex</c>
        /// argument.</summary><seealso cref="InternalEdges"/>
        /// <seealso cref="Nodes"/>
        public Edge[] Edges
        {
            get
            {
                Edge[] edges = new Edge[this.mECount];
                for (int i = 0; i < this.mECount; i++)
                    edges[i] = this.mEdges[i].Data;
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
        /// An array of all the <see cref="GEdge"/> instances currently
        /// contained within this <see cref="T:Digraph`2{Node,Edge}"/>,
        /// in the same order as all functions with an <c>edgeIndex</c>
        /// argument.</summary><seealso cref="Edges"/>
        /// <seealso cref="InternalNodes"/>
        public GEdge[] InternalEdges
        {
            get
            {
                GEdge[] edges = new GEdge[this.mECount];
                if (this.mECount > 0)
                    Array.Copy(this.mEdges, 0, edges, 0, this.mECount);
                return edges;
            }
        }

        /// <summary>
        /// Retrieves the <see cref="GEdge"/> instance at the given
        /// <paramref name="edgeIndex"/> in this directional graph's
        /// internal list of <see cref="InternalEdges"/>.
        /// </summary>
        /// <param name="edgeIndex">The index of the <see cref="GEdge"/>
        /// instance to retrieve from this directional graph's internal list of
        /// <see cref="InternalEdges"/>.</param>
        /// <returns>The <see cref="GEdge"/> instance at 
        /// <paramref name="edgeIndex"/> in this directional graph's internal
        /// list of <see cref="InternalEdges"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="edgeIndex"/> is less than <c>0</c> or greater 
        /// than or equal to <see cref="EdgeCount"/>.</exception>
        public GEdge InternalEdgeAt(int edgeIndex)
        {
            //if (edgeIndex < 0 || edgeIndex >= this.mECount)
            //    throw new ArgumentOutOfRangeException("edgeIndex");
            return this.mEdges[edgeIndex];
        }
        #endregion

        #region Searching for Edges
        /// <summary>
        /// Searches for the <typeparamref name="Edge"/> instance with the
        /// specified source and destination <typeparamref name="Node"/>s
        /// and returns the zero-based index of its occurrence within this
        /// <see cref="T:Digraph`2{Node,Edge}"/>'s internal list of 
        /// <see cref="Edges"/>.</summary>
        /// <param name="srcNode">The <see cref="P:IGraphEdge`1{Node}.SrcNode"/>
        /// of the <typeparamref name="Edge"/> instance to locate.</param>
        /// <param name="dstNode">The <see cref="P:IGraphEdge`1{Node}.DstNode"/>
        /// of the <typeparamref name="Edge"/> instance to locate.</param>
        /// <returns>The zero-based index of the occurrence of the
        /// edge with the given <paramref name="srcNode"/> source and
        /// <paramref name="dstNode"/> destination within this <see 
        /// cref="T:Digraph`2{Node,Edge}"/>, if found; otherwise, -1.
        /// </returns>
        /// <seealso cref="IndexOfNode(Node)"/>
        public int IndexOfEdge(Node srcNode, Node dstNode)
        {
            Edge edge;
            EqualityComparer<Node> ec = EqualityComparer<Node>.Default;
            for (int i = 0; i < this.mECount; i++)
            {
                edge = this.mEdges[i].Data;
                if (ec.Equals(edge.SrcNode, srcNode) &&
                    ec.Equals(edge.DstNode, dstNode))
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Determines whether an <typeparamref name="Edge"/> instance with the
        /// specified source and destination <typeparamref name="Node"/>s is in
        /// this <see cref="T:Digraph`2{Node,Edge}"/>.
        /// </summary>
        /// <param name="srcNode">The <see cref="P:IGraphEdge`1{Node}.SrcNode"/>
        /// of the <typeparamref name="Edge"/> instance to locate.</param>
        /// <param name="dstNode">The <see cref="P:IGraphEdge`1{Node}.DstNode"/>
        /// of the <typeparamref name="Edge"/> instance to locate.</param>
        /// <returns>true if an <typeparamref name="Edge"/> instance with the
        /// given <paramref name="srcNode"/> source and 
        /// <paramref name="dstNode"/> destination is in this 
        /// <see cref="T:Digraph`2{Node,Edge}"/>; otherwise, false.
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
        /// destination in this <see cref="T:Digraph`2{Node,Edge}"/>,
        /// if found; otherwise, the default value of the <typeparamref 
        /// name="Edge"/> type.</returns>
        public Edge FindEdge(Node srcNode, Node dstNode)
        {
            int index = this.IndexOfEdge(srcNode, dstNode);
            return index < 0 ? default(Edge) : this.mEdges[index].Data;
        }
        #endregion

        #region Adding Edges
        /// <summary>
        /// Adds an <typeparamref name="Edge"/> instance to the end of this
        /// <see cref="T:Digraph`2{Node,Edge}"/>, or replaces the 
        /// <typeparamref name="Edge"/> instance with the same <see 
        /// cref="P:IGraphEdge`1{Node}.SrcNode"/> and <see 
        /// cref="P:IGraphNode`1{Node}.DstNode"/> already in this graph with the
        /// given <paramref name="edge"/>.</summary>
        /// <param name="edge">The <typeparamref name="Edge"/> instance to be
        /// added to the end of this <see cref="T:Digraph`2{Node,Edge}"/>, or 
        /// replace the instance with the same source and destination.</param>
        /// <seealso cref="AddEdge(Edge,bool)"/><seealso cref="AddNode(Node)"/>
        public void AddEdge(Edge edge)
        {
            this.InsertEdge(this.mECount, edge, true, false);
        }

        /// <summary>
        /// Adds an <typeparamref name="Edge"/> instance to the end of this
        /// <see cref="T:Digraph`2{Node,Edge}"/>, or replaces the 
        /// <typeparamref name="Edge"/> instance with the same <see 
        /// cref="M:IGraphEdge`1{Node}.SrcNode"/> and <see 
        /// cref="M:IGraphNode`1{Node}.DstNode"/> already in this graph with the
        /// given <paramref name="edge"/>.</summary>
        /// <param name="edge">The <typeparamref name="Edge"/> instance to be
        /// added to the end of this <see cref="T:Digraph`2{Node,Edge}"/>, or to
        /// replace the instance with the same source and destination.</param>
        /// <param name="replace">Whether or not <paramref name="edge"/>
        /// replaces any existing <typeparamref name="Edge"/> instance with the
        /// same source and destination <typeparamref name="Node"/>s.</param>
        /// <returns>true if the <paramref name="edge"/> is added to this <see 
        /// cref="T:Digraph`2{Node,Edge}"/>, false if it's already contained in it 
        /// and replacement isn't allowed.</returns>
        /// <seealso cref="AddEdge(Edge)"/><seealso cref="AddNode(Node)"/>
        public bool AddEdge(Edge edge, bool replace)
        {
            return this.InsertEdge(this.mECount, edge, replace, false);
        }

        /*public bool InsertEdge(int index, Edge edge)
        {
            return this.InsertEdge(index, edge, false, false);
        }

        public bool InsertEdge(int index, Edge edge, bool replace)
        {
            return this.InsertEdge(index, edge, replace, false);
        }/* */

        public bool InsertEdge(int index, Edge edge, bool replace, bool swap)
        {
            if (index > this.mECount)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            Node node;
            GNode gSrc, gDst;
            EqualityComparer<Node> ec = EqualityComparer<Node>.Default;
            // Take the easy way out if possible
            if (this.mNCount == 0)
            {
                // Ensure the capacity of the internal node list
                if (this.mNodes.Length < 2)
                    this.mNodes = new GNode[4];
                // Ensure the capacity of the internal edge list
                if (this.mEdges.Length < 1)
                    this.mEdges = new GEdge[4];
                // Insert the new nodes
                gSrc = new GNode(edge.SrcNode, 0);
                this.mNodes[0] = gSrc;
                node = edge.DstNode;
                if (ec.Equals(gSrc.Data, node))
                {
                    gDst = gSrc;
                    this.mNCount = 1;
                    gSrc.mLoopCount = 1;
                }
                else
                {
                    gDst = new GNode(node, 1);
                    this.mNodes[1] = gDst;
                    this.mNCount = 2;
                    gSrc.mSrcCount = 1;
                    gDst.mDstCount = 1;
                }
                this.mNVers++;
                // Insert the new edge
                this.mEdges[0] = new GEdge(gSrc, gDst, edge);
                this.mECount = 1;
                this.mEVers++;
            }
            int i;
            // Attempt to find the edge's source node
            gSrc = null;
            node = edge.SrcNode;
            for (i = 0; i < this.mNCount; i++)
            {
                if (ec.Equals(node, this.mNodes[i].Data))
                {
                    gSrc = this.mNodes[i];
                    break;
                }
            }
            // Attempt to find the edge's destination node
            gDst = null;
            if (ec.Equals(node, edge.DstNode))
            {
                gDst = gSrc;
            }
            else
            {
                node = edge.DstNode;
                for (i = 0; i < this.mNCount; i++)
                {
                    if (ec.Equals(node, this.mNodes[i].Data))
                    {
                        gDst = this.mNodes[i];
                        break;
                    }
                }
            }
            // Check if the edge is already in this graph
            if (gSrc != null && gDst != null)
            {
                GEdge gEdge;
                // Attempt to locate the pre-existing edge
                if (gSrc.mIndex == gDst.mIndex)
                {
                    for (i = 0; i < this.mECount; i++)
                    {
                        if (this.mEdges[i].SrcNode.mIndex == gSrc.mIndex)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    for (i = 0; i < this.mECount; i++)
                    {
                        gEdge = this.mEdges[i];
                        if (gEdge.SrcNode.mIndex == gSrc.mIndex &&
                            gEdge.DstNode.mIndex == gDst.mIndex)
                        {
                            break;
                        }
                    }
                }
                // Attempt to replace and swap the pre-existing edge
                if (i < this.mECount)
                {
                    if (!replace)
                    {
                        return false;
                    }
                    this.mEdges[i] = new GEdge(gSrc, gDst, edge);
                    if (i != index && swap)
                    {
                        gEdge = this.mEdges[index];
                        this.mEdges[index] = this.mEdges[i];
                        this.mEdges[i] = gEdge;
                    }
                    this.mEVers++;
                    return true;
                }
            }
            // Ensure the capacity of the internal node list
            if (gSrc == null && gDst == null &&
                this.mNCount >= this.mNodes.Length - 1)
            {
                GNode[] nodes = new GNode[2 * this.mNodes.Length];
                Array.Copy(this.mNodes, 0, nodes, 0, this.mNCount);
                this.mNodes = nodes;
            }
            else if (this.mNCount == this.mNodes.Length)
            {
                GNode[] nodes = new GNode[2 * this.mNodes.Length];
                Array.Copy(this.mNodes, 0, nodes, 0, this.mNCount);
                this.mNodes = nodes;
            }
            // Add the missing nodes to the end of the internal node list
            if (gSrc == null)
            {
                gSrc = new GNode(edge.SrcNode, this.mNCount);
                this.mNodes[this.mNCount++] = gSrc;
            }
            if (gDst == null)
            {
                gDst = new GNode(edge.DstNode, this.mNCount);
                this.mNodes[this.mNCount++] = gDst;
            }
            // Increment the number of edges connected to each node
            if (gSrc.mIndex == gDst.mIndex)
            {
                gSrc.mLoopCount++;
            }
            else
            {
                gSrc.mSrcCount++;
                gDst.mDstCount++;
            }
            this.mNVers++;
            // Ensure the capacity of the internal edge list
            if (this.mECount == this.mEdges.Length)
            {
                if (this.mECount == 0)
                {
                    this.mEdges = new GEdge[4];
                }
                else
                {
                    GEdge[] edges = new GEdge[2 * this.mECount];
                    Array.Copy(this.mEdges, 0, edges, 0, this.mECount);
                    this.mEdges = edges;
                }
            }
            // Insert the new edge into the internal edge list
            if (index < this.mECount)
            {
                Array.Copy(this.mEdges, index, this.mEdges, index + 1,
                    this.mECount - index);
            }
            this.mEdges[index] = new GEdge(gSrc, gDst, edge);
            this.mECount++;
            this.mEVers++;
            return true;
        }
        #endregion

        #region Removing Edges
        /// <summary>
        /// Removes the <typeparamref name="Edge"/> instance with the specified
        /// <paramref name="srcNode"/> and <paramref name="dstNode"/> from
        /// this <see cref="T:Digraph`2{Node,Edge}"/>, but doesn't remove 
        /// any nodes orphaned by the removal of the edge.</summary>
        /// <param name="srcNode">The <see cref="P:IGraphEdge`1{Node}.SrcNode"/>
        /// of the <typeparamref name="Edge"/> instance to remove.</param>
        /// <param name="dstNode">The <see cref="P:IGraphEdge`1{Node}.DstNode"/>
        /// of the <typeparamref name="Edge"/> instance to remove.</param>
        /// <returns>true if the <typeparamref name="Edge"/> instance is 
        /// successfully removed; otherwise false if there is no instance with
        /// specified source and destination <typeparamref name="Node"/>
        /// instances in this <see cref="T:Digraph`2{Node,Edge}"/>.</returns>
        /// <seealso cref="RemoveEdgeAt(int)"/>
        /// <seealso cref="OrphanNode(Node)"/>
        /// <seealso cref="RemoveNode(Node)"/>
        public bool RemoveEdge(Node srcNode, Node dstNode)
        {
            int index = this.IndexOfEdge(srcNode, dstNode);
            if (index < 0) 
                return false;
            this.CoreRemoveEdgeAt(index, false);
            return true;
        }

        /// <summary>
        /// Removes the <typeparamref name="Edge"/> instance with the specified
        /// <paramref name="srcNode"/> and <paramref name="dstNode"/> from
        /// this <see cref="T:Digraph`2{Node,Edge}"/>.</summary>
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
        /// instances in this <see cref="T:Digraph`2{Node,Edge}"/>.</returns>
        /// <seealso cref="RemoveEdgeAt(int,bool)"/>
        /// <seealso cref="OrphanNode(Node)"/>
        /// <seealso cref="RemoveNode(Node,bool)"/>
        public bool RemoveEdge(Node srcNode, Node dstNode, bool removeOrphans)
        {
            int index = this.IndexOfEdge(srcNode, dstNode);
            if (index < 0) 
                return false;
            this.CoreRemoveEdgeAt(index, removeOrphans);
            return true;
        }

        /// <summary>
        /// Removes the <typeparamref name="Edge"/> instance at the specified
        /// index in this <see cref="T:Digraph`2{Node,Edge}"/>'s list of
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
            if (edgeIndex < 0 || edgeIndex >= this.mECount)
                throw new ArgumentOutOfRangeException("edgeIndex");
            this.CoreRemoveEdgeAt(edgeIndex, false);
        }

        /// <summary>
        /// Removes the <typeparamref name="Edge"/> instance at the specified
        /// index in this <see cref="T:Digraph`2{Node,Edge}"/>'s list of
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
            if (edgeIndex < 0 || edgeIndex >= this.mECount)
                throw new ArgumentOutOfRangeException("edgeIndex");
            this.CoreRemoveEdgeAt(edgeIndex, removeOrphans);
        }

        private void CoreRemoveEdgeAt(int index, bool removeOrphans)
        {
            GEdge gEdge = this.mEdges[index];
            if (gEdge.SrcNode.mIndex == gEdge.DstNode.mIndex)
            {
                gEdge.SrcNode.mLoopCount--;
            }
            else
            {
                gEdge.SrcNode.mSrcCount--;
                gEdge.DstNode.mDstCount--;
                if (removeOrphans)
                {
                    int i;
                    GNode gSrc = gEdge.SrcNode;
                    GNode gDst = gEdge.DstNode;
                    if (gSrc.mSrcCount + gSrc.mDstCount == 0)
                    {
                        gSrc.mIndex = -1;
                        if (gDst.mSrcCount + gDst.mDstCount == 0)
                        {
                            gDst.mIndex = -1;
                            int min = Math.Max(gSrc.mIndex, gDst.mIndex);
                            int max = Math.Max(gSrc.mIndex, gDst.mIndex);
                            this.mNCount--;
                            if (max < this.mNCount)
                            {
                                Array.Copy(this.mNodes, max + 1, 
                                    this.mNodes, max, this.mNCount - max);
                            }
                            this.mNodes[this.mNCount] = null;
                            this.mNCount--;
                            if (min < this.mNCount)
                            {
                                Array.Copy(this.mNodes, min + 1, 
                                    this.mNodes, min, this.mNCount - min);
                            }
                            this.mNodes[this.mNCount] = null;
                            for (i = min; i < this.mNCount; i++)
                            {
                                this.mNodes[i].mIndex = i;
                            }
                        }
                        else
                        {
                            this.mNCount--;
                            if (gSrc.mIndex < this.mNCount)
                            {
                                Array.Copy(this.mNodes, gSrc.mIndex + 1,
                                    this.mNodes, gSrc.mIndex, 
                                    this.mNCount - gSrc.mIndex);
                            }
                            this.mNodes[this.mNCount] = null;
                            for (i = gSrc.mIndex; i < this.mNCount; i++)
                            {
                                this.mNodes[i].mIndex = i;
                            }
                        }
                        this.mNVers++;
                    }
                    else if (gDst.mSrcCount + gDst.mDstCount == 0)
                    {
                        gDst.mIndex = -1;
                        this.mNCount--;
                        if (gDst.mIndex < this.mNCount)
                        {
                            Array.Copy(this.mNodes, gDst.mIndex + 1,
                                this.mNodes, gDst.mIndex,
                                this.mNCount - gDst.mIndex);
                        }
                        this.mNodes[this.mNCount] = null;
                        for (i = gDst.mIndex; i < this.mNCount; i++)
                        {
                            this.mNodes[i].mIndex = i;
                        }
                    }
                    this.mNVers++;
                }
            }
            this.mECount--;
            if (index < this.mECount)
            {
                Array.Copy(this.mEdges, index + 1, this.mEdges, index,
                    this.mECount - index);
            }
            this.mEdges[this.mECount] = null;
            this.mEVers++;
        }
        #endregion

        /// <summary>
        /// Removes all <typeparamref name="Edge"/> instances from this <see 
        /// cref="T:Digraph`2{Node,Edge}"/>, effectively orphaning
        /// every <typeparamref name="Node"/> instance in this graph.
        /// </summary><remarks>
        /// This is equivalent to calling <see cref="OrphanNode(Node)"/> for
        /// every <typeparamref name="Node"/> instance in this <see 
        /// cref="T:Digraph`2{Node,Edge}"/>, except it's much faster.
        /// </remarks><seealso cref="ClearOrphanedNodes()"/>
        /// <seealso cref="ClearNodes()"/>.
        public void ClearEdges()
        {
            if (this.mECount > 0)
            {
                Array.Clear(this.mEdges, 0, this.mECount);
                this.mECount = 0;
                this.mEVers++;
                GNode gNode;
                for (int i = 0; i < this.mNCount; i++)
                {
                    gNode = this.mNodes[i];
                    gNode.mLoopCount = 0;
                    gNode.mSrcCount = 0;
                    gNode.mDstCount = 0;
                }
            }
        }

        #endregion
    }
}
