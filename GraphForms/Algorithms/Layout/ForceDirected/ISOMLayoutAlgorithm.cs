using System;
using System.Collections.Generic;
using System.Drawing;

namespace GraphForms.Algorithms.Layout.ForceDirected
{
    // Inverted Self-Organizing Maps
    // http://www.csse.monash.edu.au/~berndm/ISOM/
    public class ISOMLayoutAlgorithm<Node, Edge>
        //: ForceDirectedLayoutAlgorithm<Node, Edge, ISOMLayoutParameters<Node>>
        : LayoutAlgorithm<Node, Edge>
        where Node : class, ILayoutNode
        where Edge : IGraphEdge<Node>, IUpdateable
    {
        private readonly Random mRnd = new Random(DateTime.Now.Millisecond);
        private Queue<int> mQueue;

        private Digraph<Node, Edge>.GNode[] mNodes;
        private int[] mDistances;
        private int mRadius;
        private double mAdapt;
        private double mGlobalX;
        private double mGlobalY;

        private int mRadiusConstantTime = 100;
        private int mInitialRadius = 5;
        private int mMinRadius = 1;
        private float mInitAdapt = 0.9f;
        private float mMinAdapt = 0;
        private float mCoolingFactor = 2;
        //private Node mBary = null;

        //private Node mLastBarycenter = null;
        //private Digraph<Node, Edge>.GNode mBarycenter;

        /*public ISOMLayoutAlgorithm(Digraph<Node, Edge> graph)
            : base(graph, null)
        {
            this.mQueue = new Queue<Digraph<Node, Edge>.GNode>();
        }

        public ISOMLayoutAlgorithm(Digraph<Node, Edge> graph,
            ISOMLayoutParameters<Node> oldParameters)
            : base(graph, oldParameters)
        {
            this.mQueue = new Queue<Digraph<Node, Edge>.GNode>();
        }/* */

        public ISOMLayoutAlgorithm(Digraph<Node, Edge> graph,
            IClusterNode clusterNode)
            : base(graph, clusterNode)
        {
            this.mQueue = new Queue<int>();
            this.mDistances = new int[0];
        }

        public ISOMLayoutAlgorithm(Digraph<Node, Edge> graph,
            RectangleF boundingBox)
            : base(graph, boundingBox)
        {
            this.mQueue = new Queue<int>();
            this.mDistances = new int[0];
        }

        /// <summary>
        /// Radius constant time. Default value is 100.
        /// </summary>
        public int RadiusConstantTime
        {
            get { return this.mRadiusConstantTime; }
            set
            {
                if (this.mRadiusConstantTime != value)
                {
                    this.mRadiusConstantTime = value;
                }
            }
        }

        /// <summary>
        /// Initial radius. Default value is 5.
        /// </summary>
        public int InitialRadius
        {
            get { return this.mInitialRadius; }
            set
            {
                if (this.mInitialRadius != value)
                {
                    this.mInitialRadius = value;
                }
            }
        }

        /// <summary>
        /// Minimum radius. Default value is 1.
        /// </summary>
        public int MinRadius
        {
            get { return this.mMinRadius; }
            set
            {
                this.mMinRadius = value;
            }
        }

        /// <summary>
        /// Initial adaptation. Default value is 0.9.
        /// </summary>
        public float InitialAdaptation
        {
            get { return this.mInitAdapt; }
            set
            {
                this.mInitAdapt = value;
            }
        }

        /// <summary>
        /// Minimum Adaptation. Default value is 0.
        /// </summary>
        public float MinAdaptation
        {
            get { return this.mMinAdapt; }
            set
            {
                this.mMinAdapt = value;
            }
        }

        /// <summary>
        /// Cooling factor. Default value is 2.
        /// </summary>
        public float CoolingFactor
        {
            get { return this.mCoolingFactor; }
            set
            {
                this.mCoolingFactor = value;
            }
        }

        /*/// <summary>
        /// The <typeparamref name="Node"/> at the center of the ISOM
        /// Layout Algorithm, which is picked at random on each iteration
        /// if this is null. Default value is null.
        /// </summary>
        public Node Barycenter
        {
            get { return this.mBary; }
            set
            {
                if (!this.mBary.Equals(value))
                {
                    this.mBary = value;
                    this.MarkDirty();
                }
            }
        }

        /// <summary>
        /// Re-finds the barycenter on the chance that the old barycenter was
        /// removed from the graph or orphaned by the removal of its only 
        /// connecting edge from the graph, meaning that it can no longer 
        /// validly function as the barycenter.
        /// </summary>
        private void FindBarycenter()
        {
            Node node = this.mBary;//this.Parameters.Barycenter;
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
        }/* */

        protected override void InitializeAlgorithm()
        {
            base.InitializeAlgorithm();
            //ISOMLayoutParameters<Node> param = this.Parameters;
            this.mRadius = this.mInitialRadius;//param.InitialRadius;
            this.mAdapt = this.mInitAdapt;//param.InitialAdaptation;
        }

        /*protected override bool OnBeginIteration(bool paramsDirty,
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
        }/* */

        protected override void PerformIteration(uint iteration)
        {
            this.mNodes = this.mGraph.InternalNodes;

            this.Adjust();

            // Update Parameters
            double factor = Math.Exp(
                -this.mCoolingFactor * iteration / this.MaxIterations);
            this.mAdapt = Math.Max(this.mMinAdapt, factor * this.mInitAdapt);
            if (this.mRadius > this.mMinRadius && 
                iteration % this.mRadiusConstantTime == 0)
            {
                this.mRadius--;
            }
        }

        private Digraph<Node, Edge>.GNode GetClosest(double x, double y)
        {
            Digraph<Node, Edge>.GNode n, node = null;
            double dx, dy, dist, minDist = double.MaxValue;

            // find the closest node
            //Digraph<Node, Edge>.GNode[] nodes = this.mGraph.InternalNodes;
            for (int i = 0; i < this.mNodes.Length; i++)
            {
                n = this.mNodes[i];
                //dist = GraphHelpers.Length(n.Data.MapFromScene(position));
                dx = n.Data.X - x;
                dy = n.Data.Y - y;
                dist = dx * dx + dy * dy;
                if (dist < minDist)
                {
                    node = n;
                    minDist = dist;
                }
            }
            return node;
        }

        private void Adjust()
        {
            Digraph<Node, Edge>.GNode closest = null;
            RectangleF bbox = this.mClusterNode == null 
                ? this.BoundingBox : this.mClusterNode.BoundingBox;
            while (closest == null || closest.AllEdgeCount == 0)
            {
                // get a random point in the container
                this.mGlobalX = (0.1 + 0.8 * this.mRnd.NextDouble()) 
                    * bbox.Width + bbox.X;
                this.mGlobalY = (0.1 + 0.8 * this.mRnd.NextDouble()) 
                    * bbox.Height + bbox.Y;

                // find the closest node to this random point
                closest = this.GetClosest(this.mGlobalX, this.mGlobalY);
            }

            // Adjust the nodes to the selected node
            //Digraph<Node, Edge>.GNode[] nodes = this.mGraph.InternalNodes;
            if (this.mDistances.Length < this.mNodes.Length)
            {
                this.mDistances = new int[this.mNodes.Length];
            }
            for (int i = 0; i < this.mNodes.Length; i++)
            {
                this.mNodes[i].Index = i;
                this.mNodes[i].Color = GraphColor.White;
                this.mDistances[i] = 0;
            }
            this.AdjustNode(closest);
        }

        private void AdjustNode(Digraph<Node, Edge>.GNode closest)
        {
            this.mQueue.Clear();
            this.mDistances[closest.Index] = 0;
            closest.Color = GraphColor.Gray;
            this.mQueue.Enqueue(closest.Index);

            //float[] newXs = this.NewXPositions;
            //float[] newYs = this.NewYPositions;
            
            Node curr;
            int i, ci, dist;
            //PointF force;
            double dx, dy, posX, posY, factor;
            Digraph<Node, Edge>.GNode n;
            Digraph<Node, Edge>.GEdge[] edges;
            while (this.mQueue.Count > 0)
            {
                ci = this.mQueue.Dequeue();
                curr = this.mNodes[ci].Data;
                dist = this.mDistances[ci];
                posX = curr.X;
                posY = curr.Y;
                if (!curr.PositionFixed)
                {
                    //force = node.MapFromScene(this.mTempPos);
                    dx = this.mGlobalX - posX;
                    dy = this.mGlobalY - posY;
                    factor = this.mAdapt / Math.Pow(2, dist);

                    posX += factor * dx;//force.X;
                    posY += factor * dy;//force.Y;
                }
                //curr.NewX = (float)posX;
                //curr.NewY = (float)posY;
                curr.SetNewPosition((float)posX, (float)posY);
                //newXs[ci] = (float)posX;
                //newYs[ci] = (float)posY;

                // only if the node is within range
                if (dist < this.mRadius)
                {
                    // iterate through all its neighbors
                    edges = this.mNodes[ci].AllInternalEdges(false);
                    for (i = 0; i < edges.Length; i++)
                    {
                        n = edges[i].DstNode;
                        if (n.Index == ci)
                            n = edges[i].SrcNode;
                        if (n.Color == GraphColor.White)
                        {
                            this.mDistances[n.Index] = dist + 1;
                            n.Color = GraphColor.Gray;
                            this.mQueue.Enqueue(n.Index);
                        }
                        /*else if (dist + 1 < this.mDistances[n.Index])
                        {
                            this.mDistances[n.Index] = dist + 1;
                            if (n.Color == GraphColor.Black)
                            {
                                n.Color = GraphColor.Gray;
                                this.mQueue.Enqueue(n.Index);
                            }
                        }/* */
                    }
                }
                this.mNodes[ci].Color = GraphColor.Black;
            }
        }
    }
}
