using System;
using System.Collections.Generic;
using System.Text;
using GraphForms.Algorithms.Collections;
using GraphForms.Algorithms.ConnectedComponents;
using GraphForms.Algorithms.SpanningTree;

namespace GraphForms.Algorithms.Layout.Tree
{
    public abstract class ATreeLayoutAlgorithm<Node, Edge, Geom>
        : LayoutAlgorithm<Node, Edge>
        where Node : class, ILayoutNode
        where Edge : class, IGraphEdge<Node>, IUpdateable
    {
        private SpanningTreeGen mSpanTreeGen = SpanningTreeGen.DFS;
        private TreeRootFinding mRootFindingMethod = TreeRootFinding.Center;

        private bool bSpanTreeDirty = true;
        private ISpanningTreeAlgorithm<Node, Edge> mSpanTreeAlg;
        private Digraph<Node, Edge>.GEdge[] mSpanTreeEdges;
        private int mSTECount;

        private bool bDataTreeDirty = true;
        private CCAlgorithm<Node, Edge> mCCAlg;
        private int[] mCompIds;
        private GTree<Node, Edge, Geom> mDataTree;

        public ATreeLayoutAlgorithm(Digraph<Node, Edge> graph,
            IClusterNode clusterNode)
            : base(graph, clusterNode)
        {
            this.mCCAlg = new CCAlgorithm<Node, Edge>(graph, false, false);
        }

        public ATreeLayoutAlgorithm(Digraph<Node, Edge> graph,
            Box2F boundingBox)
            : base(graph, boundingBox)
        {
            this.mCCAlg = new CCAlgorithm<Node, Edge>(graph, false, false);
        }

        /// <summary>
        /// Gets or sets the method this algorithm uses to build its internal
        /// sparsely connected spanning tree for traversing its graph.
        /// </summary>
        public SpanningTreeGen SpanningTreeGeneration
        {
            get { return this.mSpanTreeGen; }
            set
            {
                if (this.mSpanTreeGen != value)
                {
                    this.mSpanTreeGen = value;
                    this.bSpanTreeDirty = true;
                }
            }
        }
        /// <summary>
        /// Gets or sets the method this algorithm uses to choose the root
        /// node from which all subtrees branch off and orbit around.
        /// </summary>
        public TreeRootFinding RootFindingMethod
        {
            get { return this.mRootFindingMethod; }
            set
            {
                if (this.mRootFindingMethod != value)
                {
                    this.mRootFindingMethod = value;
                    this.bDataTreeDirty = true;
                }
            }
        }

        protected GTree<Node, Edge, Geom> DataTree
        {
            get
            {
                this.PerformPrecalculations();
                return this.mDataTree;
            }
        }

        protected override void OnRootInserted(int index,
            Digraph<Node, Edge>.GNode root)
        {
            if (this.mRootFindingMethod == TreeRootFinding.UserDefined)
            {
                this.bDataTreeDirty = true;
            }
            base.OnRootInserted(index, root);
        }

        protected override void OnRootRemoved(Digraph<Node, Edge>.GNode root)
        {
            if (this.mRootFindingMethod == TreeRootFinding.UserDefined)
            {
                this.bDataTreeDirty = true;
            }
            base.OnRootRemoved(root);
        }

        protected override void OnRootsCleared()
        {
            if (this.mRootFindingMethod == TreeRootFinding.UserDefined)
            {
                this.bDataTreeDirty = true;
            }
            base.OnRootsCleared();
        }

        protected override void PerformPrecalculations(
            uint lastNodeVersion, uint lastEdgeVersion)
        {
            if (this.bSpanTreeDirty)
            {
                ISpanningTreeAlgorithm<Node, Edge> alg = null;
                switch (this.mSpanTreeGen)
                {
                    case SpanningTreeGen.BFS:
                        BFSpanningTree<Node, Edge> bfst
                            = new BFSpanningTree<Node, Edge>(
                                this.mGraph, false, false);
                        bfst.RootCapacity = this.RootCount;
                        Digraph<Node, Edge>.GNode r1;
                        for (int i = this.RootCount - 1; i >= 0; i--)
                        {
                            r1 = this.RootAt(i);
                            bfst.AddRoot(r1.Index);
                        }
                        this.mSpanTreeAlg = bfst;
                        break;
                    case SpanningTreeGen.DFS:
                        DFSpanningTree<Node, Edge> dfst
                            = new DFSpanningTree<Node, Edge>(
                                this.mGraph, false, false);
                        dfst.RootCapacity = this.RootCount;
                        Digraph<Node, Edge>.GNode r2;
                        for (int j = this.RootCount - 1; j >= 0; j--)
                        {
                            r2 = this.RootAt(j);
                            dfst.AddRoot(r2.Index);
                        }
                        this.mSpanTreeAlg = dfst;
                        break;
                    case SpanningTreeGen.Boruvka:
                        this.mSpanTreeAlg 
                            = new BoruvkaMinSpanningTree<Node, Edge>(mGraph);
                        break;
                    case SpanningTreeGen.Kruskal:
                        this.mSpanTreeAlg 
                            = new KruskalMinSpanningTree<Node, Edge>(mGraph);
                        break;
                    case SpanningTreeGen.Prim:
                        this.mSpanTreeAlg 
                            = new PrimMinSpanningTree<Node, Edge>(mGraph);
                        break;
                }
            }
            if (this.bSpanTreeDirty ||
                lastNodeVersion != this.mGraph.NodeVersion ||
                lastEdgeVersion != this.mGraph.EdgeVersion)
            {
                this.mSpanTreeAlg.Reset();
                this.mSpanTreeAlg.Compute();
                this.mSpanTreeEdges = this.mSpanTreeAlg.SpanningTreeEdges;
                this.mSTECount = this.mSpanTreeEdges.Length;
                this.bDataTreeDirty = true;
            }
            if (this.bDataTreeDirty)
            {
                this.BuildDataTree();
            }
            this.PerformTreePrecalculations(this.mDataTree,
                lastNodeVersion, lastEdgeVersion);
            this.bSpanTreeDirty = false;
            this.bDataTreeDirty = false;
        }

        protected virtual void PerformTreePrecalculations(
            GTree<Node, Edge, Geom> dataTree,
            uint lastNodeVersion, uint lastEdgeVersion)
        {
        }

        private void BuildDataTree()
        {
            this.mCCAlg.Reset();
            this.mCCAlg.Compute();
            this.mCompIds = this.mCCAlg.ComponentIds;

            int i, j, count;
            int tIndex = 0;
            int tMaxSize = 0;
            int tCount = this.mCCAlg.ComponentCount;
            Digraph<Node, Edge>.GNode root;
            Digraph<Node, Edge>.GEdge edge;
            GTree<Node, Edge, Geom> tree;
            GTree<Node, Edge, Geom>[] trees
                = new GTree<Node, Edge, Geom>[tCount];

            count = this.mGraph.NodeCount;
            for (i = 0; i < count; i++)
            {
                root = this.mGraph.InternalNodeAt(i);
                if (root.Hidden)
                    this.mCompIds[i] = -1;
                root.Color = GraphColor.White;
            }

            GTree<Node, Edge, Geom>[] ts;
            Digraph<Node, Edge>.GEdge[] stEdges;
            switch (this.mRootFindingMethod)
            {
                case TreeRootFinding.UserDefined:
                    tCount = 0;
                    count = this.RootCount;
                    for (i = 0; i < count; i++)
                    {
                        root = this.RootAt(i);
                        if (root.Color == GraphColor.White && !root.Hidden)
                        {
                            tree = this.BuildDataBranch(root, 
                                    null, true, true);
                            trees[tCount++] = tree;
                            if (tree.TreeSize > tMaxSize)
                            {
                                tMaxSize = tree.TreeSize;
                                tIndex = tCount - 1;
                            }
                        }
                    }
                    count = this.mGraph.NodeCount;
                    for (i = 0; i < count; i++)
                    {
                        root = this.mGraph.InternalNodeAt(i);
                        if (root.Color == GraphColor.White && !root.Hidden)
                        {
                            tree = this.BuildDataBranch(root, 
                                    null, true, true);
                            trees[tCount++] = tree;
                            if (tree.TreeSize > tMaxSize)
                            {
                                tMaxSize = tree.TreeSize;
                                tIndex = tCount - 1;
                            }
                        }
                    }
                    break;
                case TreeRootFinding.SourceDirected:
                    tCount = 0;
                    for (i = 0; i < count; i++)
                    {
                        root = this.mGraph.InternalNodeAt(i);
                        if (root.Color == GraphColor.White && !root.Hidden &&
                            root.IncomingEdgeCount == 0)
                        {
                            tree = this.BuildDataBranch(root, 
                                    null, false, false);
                            if (tCount == trees.Length)
                            {
                                ts = new GTree<Node, Edge, Geom>[2 * tCount];
                                Array.Copy(trees, 0, ts, 0, tCount);
                                trees = ts;
                            }
                            trees[tCount++] = tree;
                            if (tree.TreeSize > tMaxSize)
                            {
                                tMaxSize = tree.TreeSize;
                                tIndex = tCount - 1;
                            }
                        }
                    }
                    for (i = 0; i < count; i++)
                    {
                        root = this.mGraph.InternalNodeAt(i);
                        if (root.Color == GraphColor.White && !root.Hidden)
                        {
                            tree = this.BuildDataBranch(root, 
                                    null, false, false);
                            if (tCount == trees.Length)
                            {
                                ts = new GTree<Node, Edge, Geom>[2 * tCount];
                                Array.Copy(trees, 0, ts, 0, tCount);
                                trees = ts;
                            }
                            trees[tCount++] = tree;
                            if (tree.TreeSize > tMaxSize)
                            {
                                tMaxSize = tree.TreeSize;
                                tIndex = tCount - 1;
                            }
                        }
                    }
                    break;
                case TreeRootFinding.SinkDirected:
                    tCount = 0;
                    for (i = 0; i < count; i++)
                    {
                        root = this.mGraph.InternalNodeAt(i);
                        if (root.Color == GraphColor.White && !root.Hidden &&
                            root.OutgoingEdgeCount == 0)
                        {
                            tree = this.BuildDataBranch(root, 
                                    null, false, true);
                            if (tCount == trees.Length)
                            {
                                ts = new GTree<Node, Edge, Geom>[2 * tCount];
                                Array.Copy(trees, 0, ts, 0, tCount);
                                trees = ts;
                            }
                            trees[tCount++] = tree;
                            if (tree.TreeSize > tMaxSize)
                            {
                                tMaxSize = tree.TreeSize;
                                tIndex = tCount - 1;
                            }
                        }
                    }
                    for (i = 0; i < count; i++)
                    {
                        root = this.mGraph.InternalNodeAt(i);
                        if (root.Color == GraphColor.White && !root.Hidden)
                        {
                            tree = this.BuildDataBranch(root, 
                                    null, false, true);
                            if (tCount == trees.Length)
                            {
                                ts = new GTree<Node, Edge, Geom>[2 * tCount];
                                Array.Copy(trees, 0, ts, 0, tCount);
                                trees = ts;
                            }
                            trees[tCount++] = tree;
                            if (tree.TreeSize > tMaxSize)
                            {
                                tMaxSize = tree.TreeSize;
                                tIndex = tCount - 1;
                            }
                        }
                    }
                    break;
                case TreeRootFinding.Center:
                    stEdges = this.mSpanTreeEdges;
                    this.mSpanTreeEdges
                        = new Digraph<Node, Edge>.GEdge[stEdges.Length];
                    count = tCount;
                    tCount = 0;
                    for (i = 0; i < count; i++)
                    {
                        this.mSTECount = 0;
                        for (j = 0; j < stEdges.Length; j++)
                        {
                            edge = stEdges[j];
                            if (this.mCompIds[edge.SrcNode.Index] == i &&
                                this.mCompIds[edge.DstNode.Index] == i)
                            {
                                this.mSpanTreeEdges[this.mSTECount++] = edge;
                            }
                        }
                        root = this.FindCenter(i);
                        // TODO: What if calculated center is already gray?
                        tree = this.BuildDataBranch(root, 
                                null, true, true);
                        trees[tCount++] = tree;
                        if (tree.TreeSize > tMaxSize)
                        {
                            tMaxSize = tree.TreeSize;
                            tIndex = tCount - 1;
                        }
                    }
                    this.mSpanTreeEdges = stEdges;
                    this.mSTECount = stEdges.Length;
                    break;
                case TreeRootFinding.PathCenter:
                    stEdges = this.mSpanTreeEdges;
                    this.mSpanTreeEdges
                        = new Digraph<Node, Edge>.GEdge[stEdges.Length];
                    count = tCount;
                    tCount = 0;
                    for (i = 0; i < count; i++)
                    {
                        this.mSTECount = 0;
                        for (j = 0; j < stEdges.Length; j++)
                        {
                            edge = stEdges[j];
                            if (this.mCompIds[edge.SrcNode.Index] == i &&
                                this.mCompIds[edge.DstNode.Index] == i)
                            {
                                this.mSpanTreeEdges[this.mSTECount++] = edge;
                            }
                        }
                        root = this.FindPathCenter(i);
                        // TODO: What if calculated center is already gray?
                        tree = this.BuildDataBranch(root, 
                                null, true, true);
                        trees[tCount++] = tree;
                        if (tree.TreeSize > tMaxSize)
                        {
                            tMaxSize = tree.TreeSize;
                            tIndex = tCount - 1;
                        }
                    }
                    this.mSpanTreeEdges = stEdges;
                    this.mSTECount = stEdges.Length;
                    break;
            }
            this.mDataTree = trees[tIndex];
            for (i = 0; i < tCount; i++)
            {
                if (i != tIndex)
                    trees[i].SetRoot(this.mDataTree);
            }
        }

        private Digraph<Node, Edge>.GNode FindCenter(int compId)
        {
            if (this.mGraph.NodeCount == 0)
            {
                return null;
            }
            int i, nCount = this.mGraph.NodeCount;
            int degree = 0;
            for (i = 0; i < nCount; i++)
            {
                if (this.mCompIds[i] == compId)
                    degree++;
            }
            if (degree == 0)
            {
                // There are no visible nodes, 
                // so it's as if the graph is empty.
                return null;
            }
            else if (degree == 1 || this.mSTECount == 0)
            {
                // There aren't enough visible nodes to warrant pruning,
                // so just return the first visible node found.
                for (i = 0; i < nCount; i++)
                {
                    if (this.mCompIds[i] == compId)
                        return this.mGraph.InternalNodeAt(i);
                }
            }
            Digraph<Node, Edge>.GEdge edge;
            int u, v, w = -1;
            int leafIndex = 0;
            int leafCount = 0;
            int[] degrees = new int[nCount];
            int[] leaves = new int[degree];
            for (i = 0; i < nCount; i++)
            {
                degrees[i] = 0;
            }
            for (i = 0; i < this.mSTECount; i++)
            {
                edge = this.mSpanTreeEdges[i];
                u = edge.SrcNode.Index;
                v = edge.DstNode.Index;
                //if (u != v)
                {
                    //if (this.mCompIds[v] == compId)
                    {
                        degree = degrees[v];
                        degrees[v] = degree + 1;
                    }
                    //if (this.mCompIds[u] == compId)
                    {
                        degree = degrees[u];
                        degrees[u] = degree + 1;
                    }
                }
            }
            for (i = 0; i < nCount; i++)
            {
                if (degrees[i] == 1)
                    leaves[leafCount++] = i;
            }
            if (leafCount == 0)
            {
                // There are no leaves to prune,
                // so just return the first visible node found.
                for (i = 0; i < nCount; i++)
                {
                    if (this.mCompIds[i] == compId)
                        return this.mGraph.InternalNodeAt(i);
                }
            }
            while (leafIndex < leafCount)
            {
                w = leaves[leafIndex++];
                for (i = 0; i < this.mSTECount; i++)
                {
                    edge = this.mSpanTreeEdges[i];
                    u = edge.SrcNode.Index;
                    v = edge.DstNode.Index;
                    //if (u != v)
                    {
                        if (v == w)// && this.mCompIds[u] == compId)
                        {
                            degree = degrees[u] - 1;
                            degrees[u] = degree;
                            if (degree == 1)
                                leaves[leafCount++] = u;
                        }
                        else if (u == w)// && this.mCompIds[v] == compId)
                        {
                            degree = degrees[v] - 1;
                            degrees[v] = degree;
                            if (degree == 1)
                                leaves[leafCount++] = v;
                        }
                    }
                }
            }
            return w == -1 ? null : this.mGraph.InternalNodeAt(w);
        }

        private Digraph<Node, Edge>.GNode FindPathCenter(int compId)
        {
            if (this.mGraph.NodeCount == 0)
            {
                return null;
            }
            int i, j = -1;
            int count = 0;
            int nCount = this.mGraph.NodeCount;
            for (i = 0; i < nCount; i++)
            {
                if (this.mCompIds[i] == compId)
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
            else if (count < 3 || this.mSTECount < 2)
            {
                // There are not enough edges or visible nodes to make
                // a path with an intermediate node as their center,
                // so just return the first visible node found.
                return this.mGraph.InternalNodeAt(j);
            }
            int pathCount, maxPathCount = -1;
            int u, v, ni, pi, size, n1, n2, center = 0;
            Digraph<Node, Edge>.GEdge edge;
            int[] sizes = new int[nCount];
            for (i = 0; i < nCount; i++)
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
                    edge = this.mSpanTreeEdges[i - 1];
                    n1 = edge.SrcNode.Index;
                    if (n1 == ni)
                        n1 = edge.DstNode.Index;
                    size += sizes[n1];
                }
                while (i < this.mSTECount)
                {
                    edge = this.mSpanTreeEdges[i];
                    u = edge.SrcNode.Index;
                    v = edge.DstNode.Index;
                    i++;
                    //if (this.mCompIds[u] == compId &&
                    //    this.mCompIds[v] == compId)
                    {
                        n1 = -1;
                        if (u == ni && v != pi)// && v != ni)
                        {
                            n1 = v;
                        }
                        else if (v == ni && u != pi)
                        {
                            n1 = u;
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
                pathCount = size * (nCount - 1 - size);
                // The rest of the path count is calculated from the products
                // of the permutations of each child branch of the current
                // node in the DFS tree, as there are undirected paths
                // connecting each child in each branch to the children in
                // all the other branches that go through the current node.
                for (i = 0; i < this.mSTECount; i++)
                {
                    edge = this.mSpanTreeEdges[i];
                    u = edge.SrcNode.Index;
                    v = edge.DstNode.Index;
                    //if (this.mCompIds[u] == compId &&
                    //    this.mCompIds[v] == compId)
                    {
                        n1 = -1;
                        if (u == ni && v != pi)// && v != ni)
                        {
                            n1 = v;
                        }
                        else if (v == ni && u != pi)
                        {
                            n1 = u;
                        }
                        if (n1 != -1)
                        {
                            for (j = i + 1; j < this.mSTECount; j++)
                            {
                                edge = this.mSpanTreeEdges[j];
                                u = edge.SrcNode.Index;
                                v = edge.DstNode.Index;
                                //if (this.mCompIds[u] == compId &&
                                //    this.mCompIds[v] == compId)
                                {
                                    n2 = -1;
                                    if (u == ni && v != pi)// && v != ni)
                                    {
                                        n2 = v;
                                    }
                                    else if (v == ni && u != pi)
                                    {
                                        n2 = u;
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
            return this.mGraph.InternalNodeAt(center);
        }

        private GTree<Node, Edge, Geom> BuildDataBranch(
            Digraph<Node, Edge>.GNode root, Edge edge,
            bool undirected, bool reversed)
        {
            root.Color = GraphColor.Gray;

            GTree<Node, Edge, Geom> child, parent
                = this.CreateTree(root, edge);

            Digraph<Node, Edge>.GEdge gEdge;
            Digraph<Node, Edge>.GNode gNode;

            int i, rootIndex = root.Index;
            if (undirected || !reversed)
            {
                for (i = 0; i < this.mSTECount; i++)
                {
                    gEdge = this.mSpanTreeEdges[i];
                    if (gEdge.SrcNode.Index == rootIndex)
                    {
                        gNode = gEdge.DstNode;
                        if (gNode.Color == GraphColor.White)
                        {
                            child = this.BuildDataBranch(gNode, gEdge.Data,
                                undirected, reversed);
                            child.SetRoot(parent);
                        }
                    }
                }
            }
            if (undirected || reversed)
            {
                for (i = 0; i < this.mSTECount; i++)
                {
                    gEdge = this.mSpanTreeEdges[i];
                    if (gEdge.DstNode.Index == rootIndex)
                    {
                        gNode = gEdge.SrcNode;
                        if (gNode.Color == GraphColor.White)
                        {
                            child = this.BuildDataBranch(gNode, gEdge.Data, 
                                        undirected, reversed);
                            child.SetRoot(parent);
                        }
                    }
                }
            }
            root.Color = GraphColor.Black;

            return parent;
        }

        protected abstract GTree<Node, Edge, Geom> CreateTree(
            Digraph<Node, Edge>.GNode node, Edge edge);
    }
}
