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
            Digraph<Node, Edge>.GNode[] nodes
                = this.mGraph.InternalNodes;
            Digraph<Node, Edge>.GEdge[] edges
                = this.mGraph.InternalEdges;
            Node node, n;
            float xvel, yvel, dx, dy, factor;
            int i, j;
            for (i = 0; i < nodes.Length; i++)
            {
                node = nodes[i].mData;
                if (node.PositionFixed)
                {
                    node.SetNewPosition(node.X, node.Y);
                    continue;
                }

                // Sum up all forces pushing this item away
                xvel = 0f;
                yvel = 0f;
                for (j = 0; j < nodes.Length; j++)
                {
                    n = nodes[j].mData;
                    if (i != j)
                    {
                        dx = node.X - n.X;
                        dy = node.Y - n.Y;
                        factor = Math.Max(dx * dx + dy * dy, float.Epsilon);
                        xvel += this.mForceMult * dx / factor;
                        yvel += this.mForceMult * dy / factor;
                    }
                }

                // Now subtract all forces pulling items together
                factor = 1;
                for (j = 0; j < edges.Length; j++)
                {
                    if (edges[j].mDstNode.Index == i ||
                        edges[j].mSrcNode.Index == i)
                    {
                        factor += edges[j].mData.Weight;
                    }
                }
                factor *= this.mWeightMult;
                for (j = 0; j < edges.Length; j++)
                {
                    if (edges[j].mSrcNode.Index == i)
                        n = edges[j].mDstNode.mData;
                    else if (edges[j].mDstNode.Index == i)
                        n = edges[j].mSrcNode.mData;
                    else
                        continue;
                    xvel -= (node.X - n.X) / factor;
                    yvel -= (node.Y - n.Y) / factor;
                }

                node.SetNewPosition(node.X + xvel, node.Y + yvel);
            }
        }
    }
}
