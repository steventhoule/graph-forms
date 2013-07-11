using System;
using System.Collections.Generic;
using System.Text;
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
            public readonly List<Digraph<Node, Edge>.GNode> Nodes
                = new List<Digraph<Node, Edge>.GNode>();
            public double LastTranslate;

            public Layer()
            {
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
        private Vec2F[] mSizes;
        private NodeData[] mDatas;
        private int mDir;
        private List<Layer> mLayers = new List<Layer>();
        

        /*public SimpleTreeLayoutAlgorithm(Digraph<Node, Edge> graph)
            : base(graph, null)
        {
        }

        public SimpleTreeLayoutAlgorithm(Digraph<Node, Edge> graph,
            SimpleTreeLayoutParameters oldParameters)
            : base(graph, oldParameters)
        {
        }/* */

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
                    this.MarkDirty();
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
                    this.MarkDirty();
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
                    this.MarkDirty();
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
                    this.MarkDirty();
                }
            }
        }

        protected override void OnBeginIteration(uint iteration,
            bool dirty, int lastNodeCount, int lastEdgeCount)
        {
            if (lastNodeCount != this.mGraph.NodeCount ||
                lastEdgeCount != this.mGraph.EdgeCount || dirty)
            {
                this.ComputePositions();
            }
            base.OnBeginIteration(iteration, dirty,
                lastNodeCount, lastEdgeCount);
        }

        private void ComputePositions()
        {
            //SimpleTreeLayoutParameters param = this.Parameters;
            //this.mVertexGap = param.VertexGap;
            //this.mLayerGap = param.LayerGap;
            //this.mDirection = param.Direction;
            //this.mSpanTreeGen = param.SpanningTreeGeneration;

            int i;
            Digraph<Node, Edge>.GNode[] nodes
                = this.mGraph.InternalNodes;

            this.mSizes = new Vec2F[nodes.Length];
            this.mDatas = new NodeData[nodes.Length];
            Box2F bbox;
            if (this.mDirection == LayoutDirection.LeftToRight ||
                this.mDirection == LayoutDirection.RightToLeft)
            {
                for (i = 0; i < nodes.Length; i++)
                {
                    bbox = nodes[i].Data.LayoutBBox;
                    this.mSizes[i] = new Vec2F(bbox.H, bbox.W);
                }
            }
            else
            {
                for (i = 0; i < nodes.Length; i++)
                {
                    bbox = nodes[i].Data.LayoutBBox;
                    this.mSizes[i] = new Vec2F(bbox.W, bbox.H);
                }
            }

            if (this.mDirection == LayoutDirection.RightToLeft ||
                this.mDirection == LayoutDirection.BottomToTop)
                this.mDir = -1;
            else
                this.mDir = 1;

            this.GenerateSpanningTree();

            nodes = this.mSpanningTree.InternalNodes;
            for (i = 0; i < nodes.Length; i++)
            {
                nodes[i].Index = i;
                nodes[i].Color = GraphColor.White;
            }
            // first layout the nodes with 0 src edges
            for (i = 0; i < nodes.Length; i++)
            {
                if (nodes[i].SrcEdgeCount == 0)
                    this.CalculatePosition(nodes[i], null, 0);
            }
            // then layout the other nodes
            for (i = 0; i < nodes.Length; i++)
            {
                this.CalculatePosition(nodes[i], null, 0);
            }

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
                    Digraph<Node, Edge>.GNode r1 = this.TryGetGraphRoot();
                    bfst.SetRoot(r1.mData);
                    alg = bfst;
                    break;
                case SpanningTreeGen.DFS:
                    DFSpanningTree<Node, Edge> dfst 
                        = new DFSpanningTree<Node, Edge>(
                            this.mGraph, false, false);
                    Digraph<Node, Edge>.GNode r2 = this.TryGetGraphRoot();
                    dfst.SetRoot(r2.mData);
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
            Digraph<Node, Edge>.GNode parent, int l)
        {
            if (n.Color == GraphColor.Gray)
                return -1; // this node is already laid out

            while (l >= this.mLayers.Count)
                this.mLayers.Add(new Layer());

            Layer layer = this.mLayers[l];
            Vec2F size = this.mSizes[n.Index];
            NodeData d = new NodeData();
            d.parent = parent;
            this.mDatas[n.Index] = d;
            n.Color = GraphColor.Gray;

            layer.NextPosition += size.X / 2.0;
            if (l > 0)
            {
                layer.NextPosition += this.mLayers[l - 1].LastTranslate;
                this.mLayers[l - 1].LastTranslate = 0;
            }
            layer.Size = Math.Max(layer.Size, size.Y + this.mLayerGap);
            layer.Nodes.Add(n);
            if (n.DstEdgeCount == 0)
            {
                d.position = layer.NextPosition;
            }
            else
            {
                double minPos = double.MaxValue;
                double maxPos = -double.MaxValue;
                // first put the children
                Digraph<Node, Edge>.GNode child;
                Digraph<Node, Edge>.GEdge[] outEdges
                    = n.InternalDstEdges;
                double childPos;
                for (int i = 0; i < outEdges.Length; i++)
                {
                    child = outEdges[i].DstNode;
                    childPos = this.CalculatePosition(child, n, l + 1);
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
            List<Digraph<Node, Edge>.GNode> nodes;
            Digraph<Node, Edge>.GNode node;
            Vec2F size;
            NodeData d;
            double x, y;
            int i, j;
            for (i = 0; i < this.mLayers.Count; i++)
            {
                nodes = this.mLayers[i].Nodes;
                for (j = 0; j < nodes.Count; j++)
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
                    node.Data.SetNewPosition((float)x, (float)y);
                    //newXs[node.Index] = (float)x;
                    //newYs[node.Index] = (float)y;
                }
                layerSize += this.mLayers[i].Size;
            }
        }
    }
}
