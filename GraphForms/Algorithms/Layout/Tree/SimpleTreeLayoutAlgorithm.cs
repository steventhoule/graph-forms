using System;
using GraphForms.Algorithms.Search;
using GraphForms.Algorithms.SpanningTree;

namespace GraphForms.Algorithms.Layout.Tree
{
    public class SimpleTreeLayoutAlgorithm<Node, Edge>
        //: LayoutAlgorithm<Node, Edge, SimpleTreeLayoutParameters>
        : LayoutAlgorithm<Node, Edge>
        where Node : class, ILayoutNode
        where Edge : IGraphEdge<Node>, IUpdateable
    {
        private class Layer
        {
            public double Size;
            public double NextPosition;
            public Digraph<Node, Edge>.GNode[] Nodes;
            public int NodeCount;
            public double LastTranslate;

            public Layer()
            {
                this.Nodes = new Digraph<Node, Edge>.GNode[2];
                this.NodeCount = 0;
                this.LastTranslate = 0;
            }
        }

        private class NodeData
        {
            public Digraph<Node, Edge>.GNode parent;
            public double translate;
            public double position;
        }

        private double mVertexGap = 10;
        private double mLayerGap = 10;
        private LayoutDirection mDirection = LayoutDirection.TopToBottom;
        private SpanningTreeGen mSpanTreeGen = SpanningTreeGen.DFS;

        private Digraph<Node, Edge> mSpanningTree;
        private Digraph<Node, Edge>.GEdge[] mSpanTreeEdges;
        private Vec2F[] mSizes;
        private NodeData[] mDatas;
        private int mDir;
        private Layer[] mLayers;
        private int mLayerCount;

        private bool bDirty = true;

        public SimpleTreeLayoutAlgorithm(Digraph<Node, Edge> graph,
            IClusterNode clusterNode)
            : base(graph, clusterNode)
        {
            this.Spring = new LayoutLinearSpring();
        }

        public SimpleTreeLayoutAlgorithm(Digraph<Node, Edge> graph,
            Box2F boundingBox)
            : base(graph, boundingBox)
        {
            this.Spring = new LayoutLinearSpring();
        }

        /// <summary>
        /// Gets or sets the gap between the vertices.
        /// </summary>
        public double VertexGap
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
        public double LayerGap
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
                    this.bDirty = true;
                }
            }
        }

        protected override void PerformPrecalculations(
            uint lastNodeVersion, uint lastEdgeVersion)
        {
            if (lastNodeVersion != this.mGraph.NodeVersion ||
                lastEdgeVersion != this.mGraph.EdgeVersion || this.bDirty)
            {
                this.ComputePositions();
            }
            this.bDirty = false;
        }

        protected override void PerformIteration(uint iteration)
        {
        }

        private void ComputePositions()
        {
            //SimpleTreeLayoutParameters param = this.Parameters;
            //this.mVertexGap = param.VertexGap;
            //this.mLayerGap = param.LayerGap;
            //this.mDirection = param.Direction;
            //this.mSpanTreeGen = param.SpanningTreeGeneration;

            Digraph<Node, Edge>.GNode node;
            int i, count = this.mGraph.NodeCount;

            this.mSizes = new Vec2F[count];
            this.mDatas = new NodeData[count];
            Box2F bbox;
            if (this.mDirection == LayoutDirection.LeftToRight ||
                this.mDirection == LayoutDirection.RightToLeft)
            {
                for (i = 0; i < count; i++)
                {
                    bbox = this.mGraph.NodeAt(i).LayoutBBox;
                    this.mSizes[i] = new Vec2F(bbox.H, bbox.W);
                }
            }
            else
            {
                for (i = 0; i < count; i++)
                {
                    bbox = this.mGraph.NodeAt(i).LayoutBBox;
                    this.mSizes[i] = new Vec2F(bbox.W, bbox.H);
                }
            }

            if (this.mDirection == LayoutDirection.RightToLeft ||
                this.mDirection == LayoutDirection.BottomToTop)
                this.mDir = -1;
            else
                this.mDir = 1;

            this.GenerateSpanningTree();

            this.mLayers = new Layer[2];
            this.mLayerCount = 0;

            this.mSpanTreeEdges = this.mSpanningTree.InternalEdges;

            count = this.mSpanningTree.NodeCount;
            for (i = 0; i < count; i++)
            {
                node = this.mSpanningTree.InternalNodeAt(i);
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

            this.mSpanTreeEdges = null;

            this.AssignPositions();
        }

        private void GenerateSpanningTree()
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
        }

        private double CalculatePosition(Digraph<Node, Edge>.GNode n,
            Digraph<Node, Edge>.GNode parent, int lnum)
        {
            if (n.Color == GraphColor.Gray)
                return -1; // this node is already laid out

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
            Vec2F size = this.mSizes[n.Index];
            NodeData d = new NodeData();
            d.parent = parent;
            this.mDatas[n.Index] = d;
            n.Color = GraphColor.Gray;

            layer.NextPosition += size.X / 2.0;
            if (lnum > 0)
            {
                layer.NextPosition += this.mLayers[lnum - 1].LastTranslate;
                this.mLayers[lnum - 1].LastTranslate = 0;
            }
            layer.Size = Math.Max(layer.Size, size.Y + this.mLayerGap);
            if (layer.NodeCount == layer.Nodes.Length)
            {
                Digraph<Node, Edge>.GNode[] nodes
                    = new Digraph<Node, Edge>.GNode[2 * layer.NodeCount];
                Array.Copy(layer.Nodes, 0, nodes, 0, layer.NodeCount);
                layer.Nodes = nodes;
            }
            layer.Nodes[layer.NodeCount++] = n;
            if (n.OutgoingEdgeCount == 0)
            {
                d.position = layer.NextPosition;
            }
            else
            {
                double minPos = double.MaxValue;
                double maxPos = -double.MaxValue;
                // first put the children
                Digraph<Node, Edge>.GNode child;
                double childPos;
                for (int i = 0; i < this.mSpanTreeEdges.Length; i++)
                {
                    child = this.mSpanTreeEdges[i].SrcNode;
                    if (child.Index != n.Index)
                        continue;
                    child = this.mSpanTreeEdges[i].DstNode;
                    childPos = this.CalculatePosition(child, n, lnum + 1);
                    if (childPos >= 0)
                    {
                        minPos = Math.Min(minPos, childPos);
                        maxPos = Math.Max(maxPos, childPos);
                    }
                }
                if (minPos != double.MaxValue)
                    d.position = (minPos + maxPos) / 2.0;
                else
                    d.position = layer.NextPosition;
                d.translate = Math.Max(layer.NextPosition - d.position, 0);

                layer.LastTranslate = d.translate;
                d.position += d.translate;
                layer.NextPosition = d.position;
            }
            layer.NextPosition += size.X / 2.0 + this.mVertexGap;

            return d.position;
        }

        private void AssignPositions()
        {
            double layerSize = 0;
            bool horizontal = 
                this.mDirection == LayoutDirection.LeftToRight || 
                this.mDirection == LayoutDirection.RightToLeft;

            //float[] newXs = this.NewXPositions;
            //float[] newYs = this.NewYPositions;
            Digraph<Node, Edge>.GNode[] nodes;
            Digraph<Node, Edge>.GNode node;
            Vec2F size;
            NodeData d;
            double x, y;
            int i, j, nodeCount;
            for (i = 0; i < this.mLayerCount; i++)
            {
                nodes = this.mLayers[i].Nodes;
                nodeCount = this.mLayers[i].NodeCount;
                for (j = 0; j < nodeCount; j++)
                {
                    node = nodes[j];
                    size = this.mSizes[node.Index];
                    d = this.mDatas[node.Index];
                    if (d.parent != null)
                    {
                        d.position += this.mDatas[d.parent.Index].translate;
                        d.translate += this.mDatas[d.parent.Index].translate;
                    }
                    if (horizontal)
                    {
                        x = this.mDir * (layerSize + size.Y / 2.0);
                        y = d.position;
                    }
                    else
                    {
                        x = d.position;
                        y = this.mDir * (layerSize + size.Y / 2.0);
                    }
                    //node.Data.NewX = (float)x;
                    //node.Data.NewY = (float)y;
                    node.Data.SetPosition((float)x, (float)y);//SetNewPosition((float)x, (float)y);
                    //newXs[node.Index] = (float)x;
                    //newYs[node.Index] = (float)y;
                }
                layerSize += this.mLayers[i].Size;
            }
        }
    }
}
