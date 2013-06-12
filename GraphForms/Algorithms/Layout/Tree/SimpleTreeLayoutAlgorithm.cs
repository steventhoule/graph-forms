using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using GraphForms.Algorithms.Search;
using GraphForms.Algorithms.SpanningTree;

namespace GraphForms.Algorithms.Layout.Tree
{
    public class SimpleTreeLayoutAlgorithm<Node, Edge>
        : LayoutAlgorithm<Node, Edge, SimpleTreeLayoutParameters>
        where Node : GraphElement, ILayoutNode
        where Edge : class, IGraphEdge<Node>, IUpdateable
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

        private double mVertexGap;
        private double mLayerGap;
        private LayoutDirection mDirection;
        private SearchMethod mSpanTreeGen;

        private Digraph<Node, Edge> mSpanningTree;
        private SizeF[] mSizes;
        private NodeData[] mDatas;
        private int mDir;
        private List<Layer> mLayers = new List<Layer>();
        

        public SimpleTreeLayoutAlgorithm(Digraph<Node, Edge> graph)
            : base(graph, null)
        {
        }

        public SimpleTreeLayoutAlgorithm(Digraph<Node, Edge> graph,
            SimpleTreeLayoutParameters oldParameters)
            : base(graph, oldParameters)
        {
        }

        protected override void InternalCompute()
        {
            SimpleTreeLayoutParameters param = this.Parameters;
            this.mVertexGap = param.VertexGap;
            this.mLayerGap = param.LayerGap;
            this.mDirection = param.Direction;
            this.mSpanTreeGen = param.SpanningTreeGeneration;

            int i;
            Digraph<Node, Edge>.GNode[] nodes
                = this.mGraph.InternalNodes;

            this.mSizes = new SizeF[nodes.Length];
            this.mDatas = new NodeData[nodes.Length];
            RectangleF bbox;
            if (this.mDirection == LayoutDirection.LeftToRight ||
                this.mDirection == LayoutDirection.RightToLeft)
            {
                for (i = 0; i < nodes.Length; i++)
                {
                    bbox = nodes[i].Data.BoundingBox;
                    this.mSizes[i] = new SizeF(bbox.Height, bbox.Width);
                    this.mDatas[i] = new NodeData();
                }
            }
            else
            {
                for (i = 0; i < nodes.Length; i++)
                {
                    bbox = nodes[i].Data.BoundingBox;
                    this.mSizes[i] = new SizeF(bbox.Width, bbox.Height);
                    this.mDatas[i] = new NodeData();
                }
            }

            if (this.mDirection == LayoutDirection.RightToLeft ||
                this.mDirection == LayoutDirection.BottomToTop)
                this.mDir = -1;
            else
                this.mDir = 1;

            this.GenerateSpanningTree();

            nodes = this.mSpanningTree.InternalNodes;
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
                case SearchMethod.BFS:
                    alg = new BFSpanningTree<Node, Edge>(this.mGraph);
                    break;
                case SearchMethod.DFS:
                    alg = new DFSpanningTree<Node, Edge>(this.mGraph);
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
            SizeF size = this.mSizes[n.Index];
            NodeData d = new NodeData();
            d.parent = parent;
            this.mDatas[n.Index] = d;
            n.Color = GraphColor.Gray;

            layer.NextPosition += size.Width;
            if (l > 0)
            {
                layer.NextPosition += this.mLayers[l - 1].LastTranslate;
                this.mLayers[l - 1].LastTranslate = 0;
            }
            layer.Size = Math.Max(layer.Size, size.Height + this.mLayerGap);
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
                    d.position = (minPos + maxPos) / 2;
                else
                    d.position = layer.NextPosition;
                d.translate = Math.Max(layer.NextPosition - d.position, 0);

                layer.LastTranslate = d.translate;
                d.position += d.translate;
                layer.NextPosition = d.position;
            }
            layer.NextPosition += size.Width / 2 + this.mVertexGap;

            return d.position;
        }

        private void AssignPositions()
        {
            double layerSize = 0;
            bool hori = 
                this.mDirection == LayoutDirection.LeftToRight || 
                this.mDirection == LayoutDirection.RightToLeft;

            float[] newXs = this.NewXPositions;
            float[] newYs = this.NewYPositions;
            List<Digraph<Node, Edge>.GNode> nodes;
            Digraph<Node, Edge>.GNode node;
            SizeF size;
            NodeData d;
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

                    //node.Data.NewX = (float)(hori ? this.mDir * (layerSize + size.Height / 2) : d.position);
                    //node.Data.NewY = (float)(hori ? d.position : this.mDir * (layerSize + size.Height / 2));
                    newXs[node.Index] = (float)(hori ? this.mDir * (layerSize + size.Height / 2) : d.position);
                    newYs[node.Index] = (float)(hori ? d.position : this.mDir * (layerSize + size.Height / 2));
                }
                layerSize += this.mLayers[i].Size;
            }
        }
    }
}
