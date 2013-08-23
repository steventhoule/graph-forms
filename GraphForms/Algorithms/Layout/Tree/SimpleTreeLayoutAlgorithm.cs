using System;
using GraphForms.Algorithms.Collections;
using GraphForms.Algorithms.Search;
using GraphForms.Algorithms.SpanningTree;

namespace GraphForms.Algorithms.Layout.Tree
{
    public class SimpleTreeLayoutAlgorithm<Node, Edge>
        : ATreeLayoutAlgorithm<Node, Edge, RectGeom<Node, Edge>>
        where Node : class, ILayoutNode
        where Edge : class, IGraphEdge<Node>, IUpdateable
    {
        private class Layer
        {
            public float Size;
            public float NextPosition;
            //public Digraph<Node, Edge>.GNode[] Nodes;
            public GTree<Node, Edge, RectGeom<Node, Edge>>[] Nodes;
            public int NodeCount;
            public float LastTranslate;

            public Layer()
            {
                //this.Nodes = new Digraph<Node, Edge>.GNode[2];
                this.Nodes = new GTree<Node, Edge, RectGeom<Node, Edge>>[2];
                this.NodeCount = 0;
                this.LastTranslate = 0;
            }
        }

        private class NodeData
        {
            //public Digraph<Node, Edge>.GNode parent;
            public NodeData parent;
            public float translate;
            public float posX;
            public float posY;
        }

        private float mVertexGap = 10;
        private float mLayerGap = 10;
        private LayoutDirection mDirection = LayoutDirection.TopToBottom;
        //private SpanningTreeGen mSpanTreeGen = SpanningTreeGen.DFS;

        private bool bAdaptToSizeChanges = false;
        private bool bAdjustRoots = true;
        private double mSpringMult = 10;
        private double mMagnetMult = 100;
        private double mMagnetExp = 1;

        //private Digraph<Node, Edge> mSpanningTree;
        //private Digraph<Node, Edge>.GEdge[] mSpanTreeEdges;
        //private Vec2F[] mSizes;
        private NodeData[] mDatas;
        //private float[] mSizeWs;
        //private float[] mSizeHs;
        private int mDir;
        private bool bHorizontal;
        private Layer[] mLayers;
        private int mLayerCount;

        private bool bTreeDirty = true;
        private bool bDirty = true;

        public SimpleTreeLayoutAlgorithm(Digraph<Node, Edge> graph,
            IClusterNode clusterNode)
            : base(graph, clusterNode)
        {
            //this.Spring = new LayoutLinearSpring();
            this.mDatas = new NodeData[0];
            //this.mSizeWs = new float[0];
            //this.mSizeHs = new float[0];
        }

        public SimpleTreeLayoutAlgorithm(Digraph<Node, Edge> graph,
            Box2F boundingBox)
            : base(graph, boundingBox)
        {
            //this.Spring = new LayoutLinearSpring();
            this.mDatas = new NodeData[0];
            //this.mSizeWs = new float[0];
            //this.mSizeHs = new float[0];
        }

        /// <summary>
        /// Gets or sets the gap between the vertices.
        /// </summary>
        public float VertexGap
        {
            get { return this.mVertexGap; }
            set
            {
                if (this.mVertexGap != value)
                {
                    this.mVertexGap = value;
                    this.bDirty = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the gap between the layers.
        /// </summary>
        public float LayerGap
        {
            get { return this.mLayerGap; }
            set
            {
                if (this.mLayerGap != value)
                {
                    this.mLayerGap = value;
                    this.bDirty = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the direction of the layout.
        /// </summary>
        public LayoutDirection Direction
        {
            get { return this.mDirection; }
            set
            {
                if (this.mDirection != value)
                {
                    this.mDirection = value;
                    this.bDirty = true;
                }
            }
        }

        /*/// <summary>
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
                    this.bDirty = true;
                }
            }
        }/* */

        public bool AdaptToSizeChanges
        {
            get { return this.bAdaptToSizeChanges; }
            set
            {
                if (this.bAdaptToSizeChanges != value)
                {
                    this.bAdaptToSizeChanges = value;
                }
            }
        }

        public bool AdjustRootCenters
        {
            get { return this.bAdjustRoots; }
            set
            {
                if (this.bAdjustRoots != value)
                {
                    this.bAdjustRoots = value;
                }
            }
        }

        public double SpringMultiplier
        {
            get { return this.mSpringMult; }
            set
            {
                if (this.mSpringMult != value)
                {
                    this.mSpringMult = value;
                }
            }
        }

        public double MagneticMultiplier
        {
            get { return this.mMagnetMult; }
            set
            {
                if (this.mMagnetMult != value)
                {
                    this.mMagnetMult = value;
                }
            }
        }

        public double MagneticExponent
        {
            get { return this.mMagnetExp; }
            set
            {
                if (this.mMagnetExp != value)
                {
                    this.mMagnetExp = value;
                }
            }
        }

        protected override GTree<Node, Edge, RectGeom<Node, Edge>> CreateTree(
            Digraph<Node, Edge>.GNode node, Edge edge)
        {
            this.bTreeDirty = true;
            RectGeom<Node, Edge> geom 
                = new RectGeom<Node, Edge>(node.Data.LayoutBBox);
            GTree<Node, Edge, RectGeom<Node, Edge>> tree
                = new GTree<Node, Edge, RectGeom<Node, Edge>>(
                    node.Index, node.Data, edge, geom, 
                    node.TotalEdgeCount(false));
            geom.SetOwner(tree);
            return tree;
        }

        protected override void PerformTreePrecalculations(
            GTree<Node, Edge, RectGeom<Node, Edge>> dataTree, 
            uint lastNodeVersion, uint lastEdgeVersion)
        {
            if (this.bTreeDirty)
            {
                this.bDirty = true;
            }
            else if (this.bAdaptToSizeChanges)
            {
                this.RefreshBBox(dataTree);
                if (dataTree.GeomData.TreeBoundingBoxDirty)
                {
                    this.bDirty = true;
                    // This has to be done in order to reset 
                    // dTree.TreeBoundingBoxDirty to false in order to
                    // accurately check for node size changes on the next
                    // iteration. Otherwise, it remains true and the 
                    // positions will keep getting recalculated on each
                    // iteration even if no size changes have occurred.
                    dataTree.GeomData.CalculateTreeBoundingBox();
                }
            }
            if (this.bDirty)
            {
                this.ComputePositions(dataTree);
            }
            this.bTreeDirty = false;
            this.bDirty = false;
        }

        private GTree<Node, Edge, RectGeom<Node, Edge>>[] mStackQueue
            = new GTree<Node, Edge, RectGeom<Node, Edge>>[0];

        protected override void PerformIteration(uint iteration)
        {
            GTree<Node, Edge, RectGeom<Node, Edge>> dTree = this.DataTree;
            /*if (this.bTreeDirty)
            {
                this.bDirty = true;
            }
            else if (this.bAdaptToSizeChanges)
            {
                this.RefreshBBox(dTree);
                if (dTree.GeomData.TreeBoundingBoxDirty)
                {
                    this.bDirty = true;
                    // This has to be done in order to reset 
                    // dTree.TreeBoundingBoxDirty to false in order to
                    // accurately check for node size changes on the next
                    // iteration. Otherwise, it remains true and the 
                    // positions will keep getting recalculated on each
                    // iteration even if no size changes have occurred.
                    dTree.GeomData.CalculateTreeBoundingBox();
                }
            }
            if (this.bDirty)
            {
                this.ComputePositions(dTree);
            }
            this.bTreeDirty = false;
            this.bDirty = false;/* */

            Node node;
            int i, sqIndex, sqCount;
            double cx, cy, dx, dy, fx, fy, r, dist, force;
            GTree<Node, Edge, RectGeom<Node, Edge>> ct, root;
            GTree<Node, Edge, RectGeom<Node, Edge>>[] branches;

            sqCount = this.mGraph.NodeCount;
            if (this.mStackQueue.Length < sqCount)
            {
                this.mStackQueue 
                    = new GTree<Node, Edge, RectGeom<Node, Edge>>[sqCount];
            }
            if (this.bAdjustRoots)
            {
                // Pull roots towards their fixed branches
                sqCount = 1;
                this.mStackQueue[0] = dTree;
                while (sqCount > 0)
                {
                    ct = this.mStackQueue[--sqCount];
                    if (ct.NodeData.PositionFixed)
                    {
                        root = ct.Root;
                        while (root != null)
                        {
                            // Calculate force on root
                            node = root.NodeData;
                            dx = ct.GeomData.OffsetX;
                            dy = ct.GeomData.OffsetY;
                            dist = Math.Sqrt(dx * dx + dy * dy);
                            dx = node.X - ct.NodeData.X;
                            dy = node.Y - ct.NodeData.Y;
                            if (dx == 0 && dy == 0)
                            {
                                fx = fy = dist / 10;
                            }
                            else
                            {
                                // Magnetic Force
                                r = dx * dx + dy * dy;
                                force = ct.GeomData.OffsetX + dx;
                                fx = this.mMagnetMult * 
                                    Math.Pow(force, this.mMagnetExp) / r;
                                force = ct.GeomData.OffsetY + dy;
                                fy = this.mMagnetMult *
                                    Math.Pow(force, this.mMagnetExp) / r;
                                // Spring Force
                                r = Math.Sqrt(r);
                                force = this.mSpringMult *
                                    Math.Log(r / dist);
                                fx += force * dx / r;
                                fy += force * dy / r;/* */
                            }/* */
                            /*dist = ct.GeomData.OffsetX;
                            r = node.X - ct.NodeData.Data.X;
                            fx = r == 0 ? dist / 10
                                : Math.Sign(dist) * this.mSpringMult * Math.Log(Math.Abs(r / dist));
                            dist = ct.GeomData.OffsetY;
                            r = node.Y - ct.NodeData.Data.Y;
                            fy = r == 0 ? dist / 10
                                : Math.Sign(dist) * this.mSpringMult * Math.Log(Math.Abs(r / dist));/* */
                            // Apply force to root position
                            node.SetPosition(node.X - (float)fx, 
                                             node.Y - (float)fy);
                            // Progress up the ancestry chain
                            ct = root;
                            root = ct.Root;
                        }
                    }
                    else if (ct.BranchCount > 0)
                    {
                        branches = ct.Branches;
                        for (i = branches.Length - 1; i >= 0; i--)
                        {
                            this.mStackQueue[sqCount++] = branches[i];
                        }
                    }
                }
            }
            // Pull movable branches towards their roots
            sqIndex = 0;
            sqCount = 1;
            this.mStackQueue[0] = dTree;
            while (sqIndex < sqCount)
            {
                ct = this.mStackQueue[sqIndex++];
                cx = ct.NodeData.X;
                cy = ct.NodeData.Y;
                branches = ct.Branches;
                for (i = 0; i < branches.Length; i++)
                {
                    ct = branches[i];
                    node = ct.NodeData;
                    if (!node.PositionFixed)
                    {
                        fx = node.X;
                        fy = node.Y;
                        dx = ct.GeomData.OffsetX;
                        dy = ct.GeomData.OffsetY;
                        dist = Math.Sqrt(dx * dx + dy * dy);
                        dx = cx - fx;
                        dy = cy - fy;
                        if (dx == 0 && dy == 0)
                        {
                            fx += dist / 10;
                            fy += dist / 10;
                        }
                        else
                        {
                            // Magnetic Force
                            r = dx * dx + dy * dy;
                            force = ct.GeomData.OffsetX + dx;
                            fx += this.mMagnetMult *
                                Math.Pow(force, this.mMagnetExp) / r;
                            force = ct.GeomData.OffsetY + dy;
                            fy += this.mMagnetMult *
                                Math.Pow(force, this.mMagnetExp) / r;
                            // Spring Force
                            r = Math.Sqrt(r);
                            force = this.mSpringMult *
                                Math.Log(r / dist);
                            fx += force * dx / r;
                            fy += force * dy / r;/* */
                        }/* */
                        /*dist = ct.GeomData.OffsetX;
                        r = cx - fx;
                        fx += r == 0 ? dist / 10
                            : Math.Sign(dist) * this.mSpringMult * Math.Log(Math.Abs(r / dist));
                        dist = ct.GeomData.OffsetY;
                        r = cy - fy;
                        fy += r == 0 ? dist / 10
                            : Math.Sign(dist) * this.mSpringMult * Math.Log(Math.Abs(r / dist));/* */
                        // Apply force to node position
                        node.SetPosition((float)fx, (float)fy);
                    }
                    this.mStackQueue[sqCount++] = ct;
                }
            }
        }

        private void RefreshBBox(GTree<Node, Edge, RectGeom<Node, Edge>> root)
        {
            if (root.BranchCount > 0)
            {
                GTree<Node, Edge, RectGeom<Node, Edge>>[] branches
                    = root.Branches;
                for (int i = branches.Length - 1; i >= 0; i--)
                {
                    this.RefreshBBox(branches[i]);
                }
            }
            root.GeomData.BoundingBox = root.NodeData.LayoutBBox;
        }

        private void ComputePositions(
            GTree<Node, Edge, RectGeom<Node, Edge>> root)
        {
            //SimpleTreeLayoutParameters param = this.Parameters;
            //this.mVertexGap = param.VertexGap;
            //this.mLayerGap = param.LayerGap;
            //this.mDirection = param.Direction;
            //this.mSpanTreeGen = param.SpanningTreeGeneration;

            //Digraph<Node, Edge>.GNode node;
            int count = this.mGraph.NodeCount;
            
            if (this.mDatas.Length < count)
            {
                this.mDatas = new NodeData[count];
                //this.mSizeWs = new float[count];
                //this.mSizeHs = new float[count];
            }
            //this.mSizes = new Vec2F[count];
            //this.mDatas = new NodeData[count];
            /*Box2F bbox;
            if (this.mDirection == LayoutDirection.LeftToRight ||
                this.mDirection == LayoutDirection.RightToLeft)
            {
                for (i = 0; i < count; i++)
                {
                    bbox = this.mGraph.NodeAt(i).LayoutBBox;
                    //this.mSizes[i] = new Vec2F(bbox.H, bbox.W);
                    this.mSizeWs[i] = bbox.H;
                    this.mSizeHs[i] = bbox.W;
                }
            }
            else
            {
                for (i = 0; i < count; i++)
                {
                    bbox = this.mGraph.NodeAt(i).LayoutBBox;
                    //this.mSizes[i] = new Vec2F(bbox.W, bbox.H);
                    this.mSizeWs[i] = bbox.W;
                    this.mSizeHs[i] = bbox.H;
                }
            }/* */
            this.bHorizontal =
                this.mDirection == LayoutDirection.LeftToRight ||
                this.mDirection == LayoutDirection.RightToLeft;

            if (this.mDirection == LayoutDirection.RightToLeft ||
                this.mDirection == LayoutDirection.BottomToTop)
                this.mDir = -1;
            else
                this.mDir = 1;

            //this.GenerateSpanningTree();

            if (this.mLayers == null)
                this.mLayers = new Layer[2];
            this.mLayerCount = 0;

            this.CalculatePosition(root, 0);

            //this.mSpanTreeEdges = this.mSpanningTree.InternalEdges;

            /*count = this.mSpanningTree.NodeCount;
            for (i = 0; i < count; i++)
            {
                node = this.mGraph.InternalNodeAt(i);
                //nodes[i].Index = i;
                node.Color = GraphColor.White;
            }
            // first layout the nodes with 0 src edges
            for (i = 0; i < count; i++)
            {
                node = this.mSpanningTree.InternalNodeAt(i);
                if (node.IncomingEdgeCount == 0)
                    this.CalculatePosition(node, null, 0);
            }
            // then layout the other nodes
            for (i = 0; i < count; i++)
            {
                node = this.mSpanningTree.InternalNodeAt(i);
                this.CalculatePosition(node, null, 0);
            }

            this.mSpanTreeEdges = null;/* */

            this.AssignPositions(root);
        }

        /*private void GenerateSpanningTree()
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
                    alg = bfst;
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
                    alg = dfst;
                    break;
                case SpanningTreeGen.Boruvka:
                    alg = new BoruvkaMinSpanningTree<Node, Edge>(mGraph);
                    break;
                case SpanningTreeGen.Kruskal:
                    alg = new KruskalMinSpanningTree<Node, Edge>(mGraph);
                    break;
                case SpanningTreeGen.Prim:
                    alg = new PrimMinSpanningTree<Node, Edge>(mGraph);
                    break;
            }
            alg.Compute();
            this.mSpanningTree = alg.SpanningTree;
        }/* */

        //private float CalculatePosition(Digraph<Node, Edge>.GNode n,
        //    Digraph<Node, Edge>.GNode parent, int lnum)
        private float CalculatePosition(
            GTree<Node, Edge, RectGeom<Node, Edge>> root, int lnum)
        {
            //if (n.Color == GraphColor.Gray)
            //    return -1; // this node is already laid out

            if (lnum >= this.mLayerCount)
            {
                if (lnum >= this.mLayers.Length)
                {
                    Layer[] layers = new Layer[2 * this.mLayers.Length];
                    Array.Copy(this.mLayers, 0, layers, 0, this.mLayerCount);
                    this.mLayers = layers;
                }
                for (int j = this.mLayerCount; j <= lnum; j++)
                {
                    this.mLayers[j] = new Layer();
                }
                this.mLayerCount = lnum + 1;
            }

            Layer layer = this.mLayers[lnum];
            //Vec2F size = this.mSizes[n.Index];
            //float width = this.mSizeWs[root.NodeData.Index];
            float width = this.bHorizontal 
                ? root.GeomData.BBoxH : root.GeomData.BBoxW;
            float height = this.bHorizontal
                ? root.GeomData.BBoxW : root.GeomData.BBoxH;
            NodeData d = new NodeData();
            d.parent = lnum == 0
                ? null : this.mDatas[root.Root.NodeIndex];
            this.mDatas[root.NodeIndex] = d;
            //d.parent = parent;
            //this.mDatas[n.Index] = d;
            //n.Color = GraphColor.Gray;

            //layer.NextPosition += size.X / 2.0;
            layer.NextPosition += width / 2;
            if (lnum > 0)
            {
                layer.NextPosition += this.mLayers[lnum - 1].LastTranslate;
                this.mLayers[lnum - 1].LastTranslate = 0;
            }
            //layer.Size = Math.Max(layer.Size, size.Y + this.mLayerGap);
            layer.Size = Math.Max(layer.Size, height + this.mLayerGap);
                //this.mSizeHs[root.NodeData.Index] + this.mLayerGap);
            if (layer.NodeCount == layer.Nodes.Length)
            {
                //Digraph<Node, Edge>.GNode[] nodes
                //    = new Digraph<Node, Edge>.GNode[2 * layer.NodeCount];
                int count = 2 * layer.NodeCount;
                GTree<Node, Edge, RectGeom<Node, Edge>>[] nodes
                    = new GTree<Node, Edge, RectGeom<Node, Edge>>[count];
                Array.Copy(layer.Nodes, 0, nodes, 0, layer.NodeCount);
                layer.Nodes = nodes;
            }
            layer.Nodes[layer.NodeCount++] = root;//n;
            //if (n.OutgoingEdgeCount == 0)
            if (root.BranchCount == 0)
            {
                d.posX = layer.NextPosition;
                layer.NextPosition += width / 2 + this.mVertexGap;
            }
            else
            {
                int i;
                float pos;
                float minPos = float.MaxValue;
                float maxPos = -float.MaxValue;
                // first put the children
                GTree<Node, Edge, RectGeom<Node, Edge>> child;
                GTree<Node, Edge, RectGeom<Node, Edge>>[] branches 
                    = root.Branches;
                //Digraph<Node, Edge>.GNode child;
                //for (int i = 0; i < this.mSpanTreeEdges.Length; i++)
                for (i = 0; i < branches.Length; i++)
                {
                    /*child = this.mSpanTreeEdges[i].SrcNode;
                    if (child.Index != n.Index)
                        continue;
                    child = this.mSpanTreeEdges[i].DstNode;/* */
                    //childPos = this.CalculatePosition(child, n, lnum + 1);
                    child = branches[i];
                    if (child.EdgeData != null)
                    {
                        pos = this.CalculatePosition(child, lnum + 1);
                        //if (childPos >= 0)
                        {
                            minPos = Math.Min(minPos, pos);
                            maxPos = Math.Max(maxPos, pos);
                        }
                    }
                }
                //if (minPos != float.MaxValue)
                    d.posX = (minPos + maxPos) / 2;
                //else
                //    d.position = layer.NextPosition;
                d.translate = Math.Max(layer.NextPosition - d.posX, 0);

                layer.LastTranslate = d.translate;
                d.posX += d.translate;
                layer.NextPosition = d.posX;

                layer.NextPosition += width / 2 + this.mVertexGap;

                for (i = 0; i < branches.Length; i++)
                {
                    child = branches[i];
                    if (child.EdgeData == null)
                    {
                        this.CalculatePosition(child, lnum);
                    }
                }
            }
            //layer.NextPosition += size.X / 2.0 + this.mVertexGap;

            return d.posX;
        }

        private void AssignPositions(
            GTree<Node, Edge, RectGeom<Node, Edge>> root)
        {
            float layerSize = 0;
            //bool horizontal = 
            //    this.mDirection == LayoutDirection.LeftToRight || 
            //    this.mDirection == LayoutDirection.RightToLeft;

            //float[] newXs = this.NewXPositions;
            //float[] newYs = this.NewYPositions;
            //Digraph<Node, Edge>.GNode[] nodes;
            //Digraph<Node, Edge>.GNode node;
            //Vec2F size;
            NodeData d;
            float height;
            //float x, y, height;
            int i, j, nodeCount;
            GTree<Node, Edge, RectGeom<Node, Edge>> node;
            GTree<Node, Edge, RectGeom<Node, Edge>>[] nodes;
            for (i = 0; i < this.mLayerCount; i++)
            {
                nodes = this.mLayers[i].Nodes;
                nodeCount = this.mLayers[i].NodeCount;
                for (j = 0; j < nodeCount; j++)
                {
                    node = nodes[j];
                    //size = this.mSizes[node.Index];
                    //height = this.mSizeHs[node.NodeData.Index];
                    height = this.bHorizontal 
                        ? node.GeomData.BBoxW 
                        : node.GeomData.BBoxH;
                    d = this.mDatas[node.NodeIndex];
                    if (d.parent != null)
                    {
                        //d.position += this.mDatas[d.parent.Index].translate;
                        //d.translate += this.mDatas[d.parent.Index].translate;
                        d.posX += d.parent.translate;
                        d.translate += d.parent.translate;
                    }
                    d.posY = this.mDir * (layerSize + height / 2);
                    /*if (horizontal)
                    {
                        x = this.mDir * (layerSize + size.Y / 2.0);
                        y = d.position;
                    }
                    else
                    {
                        x = d.position;
                        y = this.mDir * (layerSize + size.Y / 2.0);
                    }/* */
                    //node.Data.NewX = (float)x;
                    //node.Data.NewY = (float)y;
                    //node.Data.SetPosition((float)x, (float)y);
                    //newXs[node.Index] = (float)x;
                    //newYs[node.Index] = (float)y;
                }
                if (i == 0)
                {
                    NodeData pd = this.mDatas[root.NodeIndex];
                    if (this.bHorizontal)
                    {
                        for (j = 0; j < nodeCount; j++)
                        {
                            node = nodes[j];
                            d = this.mDatas[node.NodeIndex];
                            node.GeomData.OffsetX = d.posY - pd.posY;
                            node.GeomData.OffsetY = d.posX - pd.posX;
                        }
                    }
                    else
                    {
                        for (j = 0; j < nodeCount; j++)
                        {
                            node = nodes[j];
                            d = this.mDatas[node.NodeIndex];
                            node.GeomData.OffsetX = d.posX - pd.posX;
                            node.GeomData.OffsetY = d.posY - pd.posY;
                        }
                    }
                }
                else if (this.bHorizontal)
                {
                    for (j = 0; j < nodeCount; j++)
                    {
                        node = nodes[j];
                        d = this.mDatas[node.NodeIndex];
                        node.GeomData.OffsetX = d.posY - d.parent.posY;
                        node.GeomData.OffsetY = d.posX - d.parent.posX;
                    }
                }
                else
                {
                    for (j = 0; j < nodeCount; j++)
                    {
                        node = nodes[j];
                        d = this.mDatas[node.NodeIndex];
                        node.GeomData.OffsetX = d.posX - d.parent.posX;
                        node.GeomData.OffsetY = d.posY - d.parent.posY;
                    }
                }
                layerSize += this.mLayers[i].Size;
            }
        }
    }
}
