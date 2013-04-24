using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Layout.Tree
{
    public class BalloonTreeLayoutAlgorithm<Node, Edge>
        : LayoutAlgorithm<Node, Edge, BalloonTreeLayoutParameters>
        where Node : GraphElement, ILayoutNode
        where Edge : class, IGraphEdge<Node>, IUpdateable
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

        private int mMinRadius;
        private float mBorder;

        private DirectionalGraph<Node, Edge>.GraphNode mRoot;
        private BalloonData[] mDatas;

        public BalloonTreeLayoutAlgorithm(DirectionalGraph<Node, Edge> graph)
            : base(graph, null)
        {
        }

        public BalloonTreeLayoutAlgorithm(DirectionalGraph<Node, Edge> graph,
            BalloonTreeLayoutParameters oldParameters)
            : base(graph, oldParameters)
        {
        }

        protected override void InternalCompute()
        {
            this.mMinRadius = this.Parameters.MinRadius;

            DirectionalGraph<Node, Edge>.GraphNode node;
            DirectionalGraph<Node, Edge>.GraphNode[] nodes 
                = this.mGraph.InternalNodes;
            int i;

            this.mDatas = new BalloonData[nodes.Length];
            for (i = 0; i < nodes.Length; i++)
            {
                this.mDatas[i] = new BalloonData();
                node = nodes[i];
                node.Index = i;
                node.Visited = false;
            }

            this.FirstWalk(this.mRoot);

            for (i = 0; i < nodes.Length; i++)
            {
                nodes[i].Visited = false;
            }

            this.SecondWalk(this.mRoot, null, 0, 0, 1, 0);
        }

        private void FirstWalk(DirectionalGraph<Node, Edge>.GraphNode v)
        {
            BalloonData otherData, data = this.mDatas[v.Index];
            v.Visited = true;
            data.d = 0;

            float s = 0;

            DirectionalGraph<Node, Edge>.GraphNode otherNode;
            DirectionalGraph<Node, Edge>.GraphEdge[] outEdges
                = v.InternalDstEdges;
            for (int i = 0; i < outEdges.Length; i++)
            {
                otherNode = outEdges[i].DstNode;
                otherData = this.mDatas[otherNode.Index];

                if (!otherNode.Visited)
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

        private void SecondWalk(DirectionalGraph<Node, Edge>.GraphNode v,
            DirectionalGraph<Node, Edge>.GraphNode r, float x, float y,
            float l, float t)
        {
            //v.Data.NewX = x;
            //v.Data.NewY = y;
            this.NewXPositions[v.Index] = x;
            this.NewYPositions[v.Index] = y;
            v.Visited = true;

            if (v.DstEdgeCount > 0)
            {
                BalloonData otherData, data = this.mDatas[v.Index];
                DirectionalGraph<Node, Edge>.GraphNode otherNode;
                DirectionalGraph<Node, Edge>.GraphEdge[] outEdges
                    = v.InternalDstEdges;
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
                    if (otherNode.Visited)
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
