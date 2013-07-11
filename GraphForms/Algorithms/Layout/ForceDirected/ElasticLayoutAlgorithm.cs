using System;

namespace GraphForms.Algorithms.Layout.ForceDirected
{
    public class ElasticLayoutAlgorithm<Node, Edge>
        //: ForceDirectedLayoutAlgorithm<Node, Edge, ElasticLayoutParameters>
        : LayoutAlgorithm<Node, Edge>
        where Node : class, ILayoutNode
        where Edge : IGraphEdge<Node>, IUpdateable
    {
        private float mForceMult = 75f;
        private float mWeightMult = 10f;

        /*public ElasticLayoutAlgorithm(Digraph<Node, Edge> graph) 
            : base(graph, null)
        { 
        }

        public ElasticLayoutAlgorithm(Digraph<Node, Edge> graph,
            ElasticLayoutParameters oldParameters) 
            : base(graph, oldParameters) 
        { 
        }/* */

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

        /*protected override bool OnBeginIteration(bool paramsDirty, 
            int lastNodeCount, int lastEdgeCount)
        {
            if (paramsDirty)
            {
                this.mForceMult = this.Parameters.ForceMultiplier;
                this.mWeightMult = this.Parameters.WeightMultiplier;
            }
            return base.OnBeginIteration(paramsDirty, 
                lastNodeCount, lastEdgeCount);
        }/* */

        protected override void PerformIteration(uint iteration)//, int maxIterations)
        {
            //float[] newXs = this.NewXPositions;
            //float[] newYs = this.NewYPositions;
            Digraph<Node, Edge>.GNode[] nodes
                = this.mGraph.InternalNodes;
            Digraph<Node, Edge>.GEdge[] edges;
            Node node, n;
            //SizeF vec;
            float xvel, yvel, dx, dy, factor;
            int i, j;
            for (i = 0; i < nodes.Length; i++)
            {
                node = nodes[i].Data;
                if (node.PositionFixed)
                {
                    //node.NewX = node.X;
                    //node.NewY = node.Y;
                    node.SetNewPosition(node.X, node.Y);
                    //newXs[i] = node.X;
                    //newYs[i] = node.Y;
                    continue;
                }

                // Sum up all forces pushing this item away
                xvel = 0f;
                yvel = 0f;
                for (j = 0; j < nodes.Length; j++)
                {
                    n = nodes[j].Data;
                    //if (n != node)
                    if (i != j)
                    {
                        //vec = node.ItemTranslate(n);
                        dx = node.X - n.X;//vec.Width;
                        dy = node.Y - n.Y;//vec.Height;
                        factor = Math.Max(dx * dx + dy * dy, float.Epsilon);
                        xvel += this.mForceMult * dx / factor;
                        yvel += this.mForceMult * dy / factor;
                    }
                }

                // Now subtract all forces pulling items together
                factor = 1;
                edges = nodes[i].AllInternalEdges(false);
                for (j = 0; j < edges.Length; j++)
                {
                    factor += edges[j].Data.Weight;
                }
                factor *= this.mWeightMult;
                for (j = 0; j < edges.Length; j++)
                {
                    //vec = node.ItemTranslate(edges[j].DstNode.Data);
                    n = edges[j].DstNode.Data;
                    if (edges[j].DstNode.Index == i)
                        n = edges[j].SrcNode.Data;
                    xvel -= (node.X - n.X) / factor;//vec.Width / factor;
                    yvel -= (node.Y - n.Y) / factor;//vec.Height / factor;
                }

                //if (Math.Abs(xvel) < 0.1 && Math.Abs(yvel) < 0.1)
                //    xvel = yvel = 0;

                //node.NewX = node.X + xvel;
                //node.NewY = node.Y + yvel;
                node.SetNewPosition(node.X + xvel, node.Y + yvel);
                //newXs[i] = node.X + xvel;
                //newYs[i] = node.Y + yvel;
            }
        }
    }
}
