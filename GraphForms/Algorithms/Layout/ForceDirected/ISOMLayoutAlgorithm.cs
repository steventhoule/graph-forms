using System;

namespace GraphForms.Algorithms.Layout.ForceDirected
{
    // Inverted Self-Organizing Maps
    // http://www.csse.monash.edu.au/~berndm/ISOM/
    public class ISOMLayoutAlgorithm<Node, Edge>
        : LayoutAlgorithm<Node, Edge>
        where Node : class, ILayoutNode
        where Edge : IGraphEdge<Node>, IUpdateable
    {
        private readonly Random mRnd = new Random(DateTime.Now.Millisecond);
        //private Queue<int> mQueue;
        private Digraph<Node, Edge>.GNode[] mQueue;

        private int mNodeCount;
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

        public ISOMLayoutAlgorithm(Digraph<Node, Edge> graph,
            IClusterNode clusterNode)
            : base(graph, clusterNode)
        {
            this.mQueue = new Digraph<Node, Edge>.GNode[0];
            this.mDistances = new int[0];
        }

        public ISOMLayoutAlgorithm(Digraph<Node, Edge> graph,
            Box2F boundingBox)
            : base(graph, boundingBox)
        {
            this.mQueue = new Digraph<Node, Edge>.GNode[0];
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

        protected override void InitializeAlgorithm()
        {
            base.InitializeAlgorithm();
            this.mRadius = this.mInitialRadius;
            this.mAdapt = this.mInitAdapt;
        }

        protected override void PerformIteration(uint iteration)
        {
            this.mNodeCount = this.mGraph.NodeCount;

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
            for (int i = 0; i < this.mNodeCount; i++)
            {
                n = this.mGraph.InternalNodeAt(i);
                if (!n.Hidden)
                {
                    dx = n.Data.X - x;
                    dy = n.Data.Y - y;
                    dist = dx * dx + dy * dy;
                    if (dist < minDist)
                    {
                        node = n;
                        minDist = dist;
                    }
                }
            }
            return node;
        }

        private void Adjust()
        {
            Digraph<Node, Edge>.GNode closest;
            if (this.mDistances.Length < this.mNodeCount)
            {
                this.mDistances = new int[this.mNodeCount];
            }
            for (int i = 0; i < this.mNodeCount; i++)
            {
                closest = this.mGraph.InternalNodeAt(i);
                //this.mNodes[i].Index = i;
                closest.Color = GraphColor.White;
                this.mDistances[i] = 0;
            }
            closest = null;
            Box2F bbox = this.mClusterNode == null 
                ? this.BoundingBox : this.mClusterNode.LayoutBBox;
            while (closest == null || closest.TotalEdgeCount(false) == 0)
            {
                // get a random point in the container
                this.mGlobalX = (0.1 + 0.8 * this.mRnd.NextDouble())
                    * bbox.W + bbox.X;
                this.mGlobalY = (0.1 + 0.8 * this.mRnd.NextDouble()) 
                    * bbox.H + bbox.Y;

                // find the closest node to this random point
                closest = this.GetClosest(this.mGlobalX, this.mGlobalY);
            }

            // Adjust the nodes to the selected node
            
            this.AdjustNode(closest);
        }

        private void AdjustNode(Digraph<Node, Edge>.GNode closest)
        {
            //this.mQueue.Clear();
            if (this.mQueue.Length < this.mNodeCount)
            {
                this.mQueue = new Digraph<Node, Edge>.GNode[this.mNodeCount];
            }
            this.mDistances[closest.Index] = 0;
            closest.Color = GraphColor.Gray;
            //this.mQueue.Enqueue(closest.Index);
            int qIndex = 0;
            int qCount = 1;
            this.mQueue[0] = closest;
            
            int i, ci, dist;
            //PointF force;
            double dx, dy, posX, posY, factor;
            Digraph<Node, Edge>.GNode n, curr;
            Digraph<Node, Edge>.GEdge[] edges
                = this.mGraph.InternalEdges;
            while (qIndex < qCount)//this.mQueue.Count > 0)
            {
                curr = this.mQueue[qIndex++];//this.mQueue.Dequeue();
                ci = curr.Index;
                dist = this.mDistances[ci];
                posX = curr.Data.X;
                posY = curr.Data.Y;
                if (!curr.Data.PositionFixed)
                {
                    dx = this.mGlobalX - posX;
                    dy = this.mGlobalY - posY;
                    factor = this.mAdapt / Math.Pow(2, dist);

                    posX += factor * dx;
                    posY += factor * dy;
                }
                curr.Data.SetPosition((float)posX, (float)posY);

                // only if the node is within range
                if (dist < this.mRadius)
                {
                    // iterate through all its neighbors
                    for (i = 0; i < edges.Length; i++)
                    {
                        if (edges[i].Hidden)
                            continue;
                        if (edges[i].SrcNode.Index == ci)
                            n = edges[i].DstNode;
                        else if (edges[i].DstNode.Index == ci)
                            n = edges[i].SrcNode;
                        else
                            continue;
                        if (n.Color == GraphColor.White && !n.Hidden)
                        {
                            this.mDistances[n.Index] = dist + 1;
                            n.Color = GraphColor.Gray;
                            //this.mQueue.Enqueue(n.Index);
                            this.mQueue[qCount++] = n;
                        }
                        /*else if (dist + 1 < this.mDistances[n.Index])
                        {
                            this.mDistances[n.Index] = dist + 1;
                            if (n.Color == GraphColor.Black && !n.Hidden)
                            {
                                n.Color = GraphColor.Gray;
                                this.mQueue.Enqueue(n.Index);
                            }
                        }/* */
                    }
                }
                curr.Color = GraphColor.Black;
            }
        }
    }
}
