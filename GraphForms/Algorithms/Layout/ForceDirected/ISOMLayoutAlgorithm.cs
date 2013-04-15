using System;
using System.Collections.Generic;
using System.Drawing;

namespace GraphForms.Algorithms.Layout.ForceDirected
{
    public class ISOMLayoutAlgorithm<Node, Edge>
        : ForceDirectedLayoutAlgorithm<Node, Edge, ISOMLayoutParameters<Node>>
        where Node : GraphElement, ILayoutNode
        where Edge : class, IGraphEdge<Node>, IUpdateable
    {
        private Queue<DirectionalGraph<Node, Edge>.GraphNode> mQueue;
        private readonly Random mRnd = new Random(DateTime.Now.Millisecond);

        private PointF mTempPos;
        private int mRadius;
        private double mAdaptation;

        private int mRadiusConstantTime;
        private int mMinRadius;
        private double mInitialAdaptation;
        private double mMinAdaptation;
        private double mCoolingFactor;

        private Node mLastBarycenter = null;
        private DirectionalGraph<Node, Edge>.GraphNode mBarycenter;

        public ISOMLayoutAlgorithm(DirectionalGraph<Node, Edge> graph)
            : base(graph, null)
        {
            this.mQueue = new Queue<DirectionalGraph<Node, Edge>.GraphNode>();
        }

        public ISOMLayoutAlgorithm(DirectionalGraph<Node, Edge> graph,
            ISOMLayoutParameters<Node> oldParameters)
            : base(graph, oldParameters)
        {
            this.mQueue = new Queue<DirectionalGraph<Node, Edge>.GraphNode>();
        }

        /// <summary>
        /// Re-finds the barycenter on the chance that the old barycenter was
        /// removed from the graph or orphaned by the removal of its only 
        /// connecting edge from the graph, meaning that it can no longer 
        /// validly function as the barycenter.
        /// </summary>
        private void FindBarycenter()
        {
            Node node = this.Parameters.Barycenter;
            if (node == null)
            {
                if (this.mBarycenter == null)
                    this.mLastBarycenter = null;
                else
                    this.mLastBarycenter = this.mBarycenter.Data;
                this.mBarycenter = null;
            }
            else if (node != this.mLastBarycenter)
            {
                if (this.mBarycenter == null)
                    this.mLastBarycenter = null;
                else
                    this.mLastBarycenter = this.mBarycenter.Data;
                int index = this.mGraph.IndexOfNode(node);
                if (index < 0)
                {
                    this.mBarycenter = null;
                }
                else
                {
                    this.mBarycenter = this.mGraph.InternalNodes[index];
                    // Orphaned nodes can't function as a barycenter.
                    if (this.mBarycenter.DstEdgeCount == 0)
                        this.mBarycenter = null;
                }
            }
        }

        protected override void InitializeAlgorithm()
        {
            base.InitializeAlgorithm();
            ISOMLayoutParameters<Node> param = this.Parameters;
            this.mRadius = param.InitialRadius;
            this.mAdaptation = param.InitialAdaptation;
        }

        protected override bool OnBeginIteration(bool paramsDirty,
            int lastNodeCount, int lastEdgeCount)
        {
            if (paramsDirty)
            {
                ISOMLayoutParameters<Node> param = this.Parameters;
                this.mRadiusConstantTime = param.RadiusConstantTime;
                this.mMinRadius = param.MinRadius;
                this.mInitialAdaptation = param.InitialAdaptation;
                this.mMinAdaptation = param.MinAdaptation;
                this.mCoolingFactor = param.CoolingFactor;
            }
            if (this.mGraph.NodeCount != lastNodeCount ||
                this.mGraph.EdgeCount != lastEdgeCount || paramsDirty)
            {
                this.FindBarycenter();
            }
            return base.OnBeginIteration(paramsDirty,
                lastNodeCount, lastEdgeCount);
        }

        protected override void PerformIteration(int iteration, int maxIterations)
        {
            this.Adjust();

            // Update Parameters
            double factor = Math.Exp(-this.mCoolingFactor * iteration / maxIterations);
            this.mAdaptation = Math.Max(this.mMinAdaptation, factor * this.mInitialAdaptation);
            if (this.mRadius > this.mMinRadius && iteration % this.mRadiusConstantTime == 0)
            {
                this.mRadius--;
            }
        }

        private void Adjust()
        {
            DirectionalGraph<Node, Edge>.GraphNode closest = null;
            if (this.mBarycenter == null)
            {
                while (closest == null || closest.DstEdgeCount == 0)
                {
                    // get a random point in the container
                    this.mTempPos.X = (float)((0.1 + 0.8 * this.mRnd.NextDouble()) * this.Parameters.Width + this.Parameters.X);
                    this.mTempPos.Y = (float)((0.1 + 0.8 * this.mRnd.NextDouble()) * this.Parameters.Height + this.Parameters.Y);

                    // find the closest node to this random point
                    closest = this.GetClosest(this.mTempPos);
                }
            }
            else
            {
                closest = this.mBarycenter;
                this.mTempPos = closest.Data.Position;
            }

            // Adjust the nodes to the selected node
            DirectionalGraph<Node, Edge>.GraphNode[] nodes = this.mGraph.InternalNodes;
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i].Index = i;
                nodes[i].Distance = 0;
                nodes[i].Visited = false;
            }
            this.AdjustNode(closest);
        }

        private void AdjustNode(DirectionalGraph<Node, Edge>.GraphNode closest)
        {
            this.mQueue.Clear();
            closest.Distance = 0;
            closest.Visited = true;
            this.mQueue.Enqueue(closest);

            float[] newXs = this.NewXPositions;
            float[] newYs = this.NewYPositions;
            DirectionalGraph<Node, Edge>.GraphNode current, n;
            Node node;
            double posX, posY, factor;
            PointF force;
            DirectionalGraph<Node, Edge>.GraphEdge[] edges;
            int i;
            while (this.mQueue.Count > 0)
            {
                current = this.mQueue.Dequeue();
                node = current.Data;
                posX = node.X;
                posY = node.Y;
                if (!node.PositionFixed)
                {
                    force = node.MapFromScene(this.mTempPos);
                    factor = this.mAdaptation / Math.Pow(2, current.Distance);

                    posX += factor * force.X;
                    posY += factor * force.Y;
                }
                //node.NewX = (float)posX;
                //node.NewY = (float)posY;
                newXs[current.Index] = (float)posX;
                newYs[current.Index] = (float)posY;

                // only if the node is within range
                if (current.Distance < this.mRadius)
                {
                    // iterate through all its neighbors
                    edges = current.InternalDstEdges;
                    for (i = 0; i < edges.Length; i++)
                    {
                        factor = current.Distance + edges[i].Data.Weight;
                        n = edges[i].DstNode;
                        if (!n.Visited)
                        {
                            n.Visited = true;
                            n.Distance = (float)factor;
                            this.mQueue.Enqueue(n);
                        }
                        else if (n.Distance < factor)
                        {
                            n.Distance = (float)factor;
                            if (!this.mQueue.Contains(n))
                                this.mQueue.Enqueue(n);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finds the closest node to the given <paramref name="position"/>.
        /// </summary>
        /// <param name="position">A point in scene coordinates within the 
        /// <see cref="LayoutParameters.BoundingBox"/> of the 
        /// <see cref="P:LayoutAlgorithm`3.Parameters"/>.</param>
        /// <returns>Returns the closest node to the given 
        /// <paramref name="position"/>.</returns>
        public DirectionalGraph<Node, Edge>.GraphNode GetClosest(PointF position)
        {
            DirectionalGraph<Node, Edge>.GraphNode n, node = null;
            double d, distance = double.MaxValue;

            // find the closest node
            DirectionalGraph<Node, Edge>.GraphNode[] nodes = this.mGraph.InternalNodes;
            for (int i = 0; i < nodes.Length; i++)
            {
                n = nodes[i];
                d = GraphHelpers.Length(n.Data.MapFromScene(position));
                if (d < distance)
                {
                    node = n;
                    distance = d;
                }
            }
            return node;
        }
    }
}
