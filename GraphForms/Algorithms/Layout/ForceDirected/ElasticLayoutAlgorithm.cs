using System;

namespace GraphForms.Algorithms.Layout.ForceDirected
{
    public class ElasticLayoutAlgorithm<Node, Edge>
        : LayoutAlgorithm<Node, Edge>
        where Node : class, ILayoutNode
        where Edge : IGraphEdge<Node>, IUpdateable
    {
        private float mForceMult = 75f;
        private float mWeightMult = 10f;

        public ElasticLayoutAlgorithm(Digraph<Node, Edge> graph,
            IClusterNode clusterNode)
            : base(graph, clusterNode)
        {
        }

        public ElasticLayoutAlgorithm(Digraph<Node, Edge> graph,
            Box2F boundingBox)
            : base(graph, boundingBox)
        {
        }

        public float ForceMultiplier
        {
            get { return this.mForceMult; }
            set
            {
                if (this.mForceMult != value)
                {
                    this.mForceMult = value;
                }
            }
        }

        public float WeightMultiplier
        {
            get { return this.mWeightMult; }
            set
            {
                if (this.mWeightMult != value)
                {
                    this.mWeightMult = value;
                }
            }
        }

        protected override void PerformIteration(uint iteration)
        {
            Digraph<Node, Edge>.GEdge edge;
            Digraph<Node, Edge>.GNode node, n;
            float xvel, yvel, dx, dy, factor;
            int i, j;
            for (i = this.mGraph.NodeCount - 1; i >= 0; i--)
            {
                node = this.mGraph.InternalNodeAt(i);
                if (node.Data.PositionFixed || node.Hidden)
                {
                    continue;
                }

                // Sum up all forces pushing this item away
                xvel = 0f;
                yvel = 0f;
                for (j = this.mGraph.NodeCount - 1; j >= 0; j--)
                {
                    if (i != j)
                    {
                        n = this.mGraph.InternalNodeAt(j);
                        dx = node.Data.X - n.Data.X;
                        dy = node.Data.Y - n.Data.Y;
                        factor = Math.Max(dx * dx + dy * dy, float.Epsilon);
                        xvel += this.mForceMult * dx / factor;
                        yvel += this.mForceMult * dy / factor;
                    }
                }

                // Now subtract all forces pulling items together
                factor = 1;
                for (j = this.mGraph.EdgeCount - 1; j >= 0; j--)
                {
                    edge = this.mGraph.InternalEdgeAt(j);
                    if (edge.Hidden)
                        continue;
                    if (edge.DstNode.Index == i ||
                        edge.SrcNode.Index == i)
                    {
                        factor += edge.Data.Weight;
                    }
                }
                factor *= this.mWeightMult;
                for (j = this.mGraph.EdgeCount - 1; j >= 0; j--)
                {
                    edge = this.mGraph.InternalEdgeAt(j);
                    if (edge.Hidden)
                        continue;
                    if (edge.SrcNode.Index == i)
                        n = edge.DstNode;
                    else if (edge.DstNode.Index == i)
                        n = edge.SrcNode;
                    else
                        continue;
                    xvel -= (node.Data.X - n.Data.X) / factor;
                    yvel -= (node.Data.Y - n.Data.Y) / factor;
                }

                node.Data.SetPosition(node.Data.X + xvel, node.Data.Y + yvel);
            }
        }
    }
}
