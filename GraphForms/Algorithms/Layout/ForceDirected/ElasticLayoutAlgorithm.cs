using System;
using System.Drawing;

namespace GraphForms.Algorithms.Layout.ForceDirected
{
    public class ElasticLayoutAlgorithm<Node, Edge>
        : ForceDirectedLayoutAlgorithm<Node, Edge, ElasticLayoutParameters>
        where Node : GraphElement, ILayoutNode
        where Edge : class, IGraphEdge<Node>, IUpdateable
    {
        private float mForceMult;
        private float mWeightMult;

        public ElasticLayoutAlgorithm(DirectionalGraph<Node, Edge> graph)
            : base(graph, null)
        {
        }

        public ElasticLayoutAlgorithm(DirectionalGraph<Node, Edge> graph,
            ElasticLayoutParameters oldParameters)
            : base(graph, oldParameters)
        {
        }

        protected override bool OnBeginIteration(bool paramsDirty, int lastNodeCount, int lastEdgeCount)
        {
            if (paramsDirty)
            {
                this.mForceMult = this.Parameters.ForceMultiplier;
                this.mWeightMult = this.Parameters.WeightMultiplier;
            }
            return base.OnBeginIteration(paramsDirty, lastNodeCount, lastEdgeCount);
        }

        protected override void PerformIteration(int iteration, int maxIterations)
        {
            float[] newXs = this.NewXPositions;
            float[] newYs = this.NewYPositions;
            DirectionalGraph<Node, Edge>.GraphNode[] nodes
                = this.mGraph.InternalNodes;
            DirectionalGraph<Node, Edge>.GraphEdge[] edges;
            Node node, n;
            SizeF vec;
            float xvel, yvel, dx, dy, factor;
            int i, j;
            for (i = 0; i < nodes.Length; i++)
            {
                node = nodes[i].Data;
                if (node.PositionFixed)
                {
                    //node.NewX = node.X;
                    //node.NewY = node.Y;
                    newXs[i] = node.X;
                    newYs[i] = node.Y;
                    continue;
                }

                // Sum up all forces pushing this item away
                xvel = 0f;
                yvel = 0f;
                for (j = 0; j < nodes.Length; j++)
                {
                    n = nodes[j].Data;
                    if (n != node)
                    {
                        vec = node.ItemTranslate(n);
                        dx = vec.Width;
                        dy = vec.Height;
                        factor = dx * dx + dy * dy;
                        xvel += this.mForceMult * dx / factor;
                        yvel += this.mForceMult * dy / factor;
                    }
                }

                // Now subtract all forces pulling items together
                factor = 1;
                edges = nodes[i].InternalDstEdges;
                for (j = 0; j < edges.Length; j++)
                {
                    factor += edges[j].Data.Weight;
                }
                factor *= this.mWeightMult;
                for (j = 0; j < edges.Length; j++)
                {
                    vec = node.ItemTranslate(edges[j].DstNode.Data);
                    xvel -= vec.Width / factor;
                    yvel -= vec.Height / factor;
                }

                if (Math.Abs(xvel) < 0.1 && Math.Abs(yvel) < 0.1)
                    xvel = yvel = 0;

                //node.NewX = node.X + xvel;
                //node.NewY = node.Y + yvel;
                newXs[i] = node.X + xvel;
                newYs[i] = node.Y + yvel;
            }
        }
    }
}
