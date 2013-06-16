using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Layout.Tree
{
    public class BalloonTreeLayoutAlgorithm<Node, Edge>
        //: LayoutAlgorithm<Node, Edge, BalloonTreeLayoutParameters>
        : LayoutAlgorithm<Node, Edge>
        where Node : ILayoutNode
        where Edge : IGraphEdge<Node>, IUpdateable
    {
        private class BalloonData
        {
            /// <summary>
            /// Diameter
            /// </summary>
            public int d;
            /// <summary>
            /// Radius
            /// </summary>
            public int r;
            public float a;
            public float c;
            public float f;

            public void Adjust(float s)
            {
                if (s > Math.PI)
                {
                    this.c = (float)Math.PI / s;
                    this.f = 0;
                }
                else
                {
                    this.c = 1;
                    this.f = (float)Math.PI - s;
                }
            }
        }
        // dist: the distance of the node to its root node.
        // gapAngle: the angle that of the gap that to the wedge of the previous child node.
        // lowerAngle: the lower angle of the subtree wedge rooted at the node.
        // upperAngle: the upper angle of the subtree wedge rooted at the node.

        private int mMinRadius = 2;
        private float mBorder = 20.0f;

        private Digraph<Node, Edge>.GNode mRoot;
        private BalloonData[] mDatas;

        /*public BalloonTreeLayoutAlgorithm(Digraph<Node, Edge> graph)
            : base(graph, null)
        {
        }

        public BalloonTreeLayoutAlgorithm(Digraph<Node, Edge> graph,
            BalloonTreeLayoutParameters oldParameters)
            : base(graph, oldParameters)
        {
        }/* */

        public BalloonTreeLayoutAlgorithm(Digraph<Node, Edge> graph,
            IClusterNode clusterNode)
            : base(graph, clusterNode)
        {
            //this.Spring = new LayoutLinearSpring();
        }

        public BalloonTreeLayoutAlgorithm(Digraph<Node, Edge> graph,
            System.Drawing.RectangleF boundingBox)
            : base(graph, boundingBox)
        {
            //this.Spring = new LayoutLinearSpring();
        }

        public int MinRadius
        {
            get { return this.mMinRadius; }
            set
            {
                if (value != this.mMinRadius)
                {
                    this.mMinRadius = value;
                    this.MarkDirty();
                }
            }
        }

        public float Border
        {
            get { return this.mBorder; }
            set
            {
                if (value != this.mBorder)
                {
                    this.mBorder = value;
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
            //this.mMinRadius = this.Parameters.MinRadius;

            Digraph<Node, Edge>.GNode node;
            Digraph<Node, Edge>.GNode[] nodes 
                = this.mGraph.InternalNodes;
            int i;

            this.mDatas = new BalloonData[nodes.Length];
            for (i = 0; i < nodes.Length; i++)
            {
                this.mDatas[i] = new BalloonData();
                node = nodes[i];
                node.Index = i;
                node.Color = GraphColor.White;
            }

            this.mRoot = this.TryGetGraphRoot();

            this.FirstWalk(this.mRoot);

            for (i = 0; i < nodes.Length; i++)
            {
                nodes[i].Color = GraphColor.White;
            }

            this.SecondWalk(this.mRoot, null, 0, 0, 1, 0);
        }

        private void FirstWalk(Digraph<Node, Edge>.GNode v)
        {
            BalloonData otherData, data = this.mDatas[v.Index];
            v.Color = GraphColor.Gray;
            data.d = 0;

            float s = 0;

            Digraph<Node, Edge>.GNode otherNode;
            Digraph<Node, Edge>.GEdge[] outEdges
                = v.AllInternalEdges(false);
            for (int i = 0; i < outEdges.Length; i++)
            {
                otherNode = outEdges[i].DstNode;
                if (otherNode.Index == v.Index)
                    otherNode = outEdges[i].SrcNode;
                if (otherNode.Index == v.Index)
                    continue;
                otherData = this.mDatas[otherNode.Index];

                if (otherNode.Color == GraphColor.White)
                {
                    this.FirstWalk(otherNode);
                    data.d = Math.Max(data.d, otherData.r);
                    otherData.a = (float)Math.Atan2(otherData.r, data.d + otherData.r);
                    s += otherData.a;
                }
            }

            data.Adjust(s);
            data.r = Math.Max(data.d / 2, this.mMinRadius);
        }

        private void SecondWalk(Digraph<Node, Edge>.GNode v,
            Digraph<Node, Edge>.GNode r, float x, float y,
            float l, float t)
        {
            //v.Data.NewX = x;
            //v.Data.NewY = y;
            v.Data.SetNewPosition(x, y);
            //this.NewXPositions[v.Index] = x;
            //this.NewYPositions[v.Index] = y;
            v.Color = GraphColor.Gray;

            if (v.AllEdgeCount > 0)
            {
                BalloonData otherData, data = this.mDatas[v.Index];
                Digraph<Node, Edge>.GNode otherNode;
                Digraph<Node, Edge>.GEdge[] outEdges
                    = v.AllInternalEdges(false);
                float dd = l * data.d;
                float p = (float)(t + Math.PI);
                float pr = 0;
                float fs = 0;
                float aa, xx, yy;
                double rr;
                int i;

                // Initialize force by weight of outward edges
                for (i = 0; i < outEdges.Length; i++)
                {
                    fs += outEdges[i].Data.Weight;
                }
                fs = data.f / fs;

                
                for (i = 0; i < outEdges.Length; i++)
                {
                    otherNode = outEdges[i].DstNode;
                    if (otherNode.Index == v.Index)
                        otherNode = outEdges[i].SrcNode;
                    if (otherNode.Color == GraphColor.Gray)
                        continue;

                    otherData = this.mDatas[otherNode.Index];
                    aa = data.c * otherData.a;
                    p += pr + aa + fs;

                    rr = l * data.d * Math.Tan(aa) / (1.0 - Math.Tan(aa)) + dd;
                    xx = (float)(rr * Math.Cos(p));
                    yy = (float)(rr * Math.Sin(p));
                    pr = aa;
                    this.SecondWalk(otherNode, v, x + xx, y + yy, l * data.c, p);
                }
            }
        }
    }
}
