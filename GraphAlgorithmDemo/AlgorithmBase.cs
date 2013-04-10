using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using GraphForms;
using GraphForms.Algorithms;

namespace GraphAlgorithmDemo
{
    /// <summary>
    /// Contains data used by layout algorithms, which can be quickly swapped
    /// out for other instances containing different data.
    /// </summary>
    public class LayoutParameters : PropertyChangedNotifier
    {
        private float mX;
        /// <summary>
        /// X-coordinate of the upper-left corner of the <see cref="BoundingBox"/>.
        /// </summary>
        public float X
        {
            get { return this.mX; }
            set
            {
                if (this.mX != value)
                {
                    this.mX = value;
                    this.OnPropertyChanged("X");
                }
            }
        }

        private float mY;
        public float Y
        {
            get { return this.mY; }
            set
            {
                if (this.mY != value)
                {
                    this.mY = value;
                    this.OnPropertyChanged("Y");
                }
            }
        }

        private float mWidth;
        /// <summary>
        /// Width of the <see cref="BoundingBox"/>.
        /// </summary>
        public float Width
        {
            get { return this.mWidth; }
            set
            {
                if (this.mWidth != value)
                {
                    this.mWidth = value;
                    this.OnPropertyChanged("Width");
                }
            }
        }

        private float mHeight;
        /// <summary>
        /// Height of the <see cref="BoundingBox"/>.
        /// </summary>
        public float Height
        {
            get { return this.mHeight; }
            set
            {
                if (this.mHeight != value)
                {
                    this.mHeight = value;
                    this.OnPropertyChanged("Height");
                }
            }
        }

        /// <summary>
        /// The scene-level bounding box which constrains the positions of the
        /// nodes in graph of being changed by a layout algorithm. This should
        /// include any margins and padding applied to the scene's bounding
        /// box before it's used in the layout computation.
        /// </summary>
        public RectangleF BoundingBox
        {
            get { return new RectangleF(0, 0, this.mWidth, this.mHeight); }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="LayoutParameters"/> with
        /// its <see cref="Bounds"/> rectangle set to <paramref name="bounds"/>.
        /// </summary>
        /// <param name="bounds">The <see cref="RectangleF"/> value for the
        /// <see cref="Bounds"/> property.</param>
        public LayoutParameters(RectangleF bounds)
        {
            this.mWidth = bounds.Width;
            this.mHeight = bounds.Height;
        }
    }

    /// <summary>
    /// This interface helps define nodes used by layout algorithms
    /// </summary>
    public interface ILayoutNode
    {
        /// <summary>
        /// Whether this node is fixed to its current position and thereby
        /// unaffected by any layout algorithm applied to it.
        /// </summary><remarks>
        /// This should be true for any nodes being dragged by the mouse when 
        /// the layout algorithm is running.
        /// </remarks>
        bool PositionFixed { get; }

        /// <summary>
        /// The temporary new X-coordinate of the position of this node.
        /// </summary>
        float NewX { get; set; }
        /// <summary>
        /// The temporary new Y-coordinate of the position of this node.
        /// </summary>
        float NewY { get; set; }
    }

    public interface IUpdateable
    {
        void Update();
    }

    public abstract class LayoutAlgorithmBase<Node, Edge, Params>
        : AAlgorithm, IPropertyChangedListener
        where Node : GraphElement, ILayoutNode
        where Edge : IGraphEdge<Node>, IUpdateable
        where Params : LayoutParameters
    {
        protected readonly DirectionalGraph<Node, Edge> mGraph;

        private Params mParameters;
        /// <summary>
        /// Whether the parameters have changed and need to be refreshed
        /// on the next iteration.
        /// </summary>
        private bool mParamsDirty = true;
        /// <summary>
        /// The number of nodes in the graph during the last iteration.
        /// </summary>
        private int mLastNodeCount = 0;
        /// <summary>
        /// The number of edges in the graph during the last iteration.
        /// </summary>
        private int mLastEdgeCount = 0;

        public LayoutAlgorithmBase(DirectionalGraph<Node, Edge> graph)
        {
            this.mGraph = graph;
        }

        public DirectionalGraph<Node, Edge> Graph
        {
            get { return this.mGraph; }
        }

        public Params Parameters
        {
            get { return this.mParameters; }
            set
            {
                if (this.mParameters != value)
                {
                    this.mParameters.Listener = null;
                    value.Listener = this;
                    this.mParameters = value;
                    this.mParamsDirty = true;
                }
            }
        }

        protected int LastNodeCount
        {
            get { return this.mLastNodeCount; }
        }

        protected int LastEdgeCount
        {
            get { return this.mLastEdgeCount; }
        }

        public void OnPropertyChanged(PropertyChangedNotifier sender, 
            string propertyName)
        {
            if (sender == this.mParameters)
                this.mParamsDirty = true;
        }

        protected virtual void InitializeWithRandomPositions()
        {
            this.InitializeWithRandomPositions(
                this.mParameters.X, this.mParameters.Y,
                this.mParameters.Width, this.mParameters.Height);
        }

        protected virtual void InitializeWithRandomPositions(float x, float y, 
            float width, float height)
        {
            Random rnd = new Random(DateTime.Now.Millisecond);
            DirectionalGraph<Node, Edge>.GraphNode[] nodes = this.mGraph.InternalNodes;
            Node node;
            for (int i = 0; i < nodes.Length; i++)
            {
                node = nodes[i].Data;
                node.NewX = (float)(rnd.NextDouble() * width + x);
                node.NewY = (float)(rnd.NextDouble() * height + y);
            }
        }

        /// <summary>
        /// Initializes extra data associated with values in this algorithm's
        /// <see cref="Parameters"/>. This is called before the extra node and
        /// edge data is initialized in case they depend on the parameters.
        /// </summary><remarks>
        /// This is called before <see cref="InitNodeData()"/> and 
        /// <see cref="InitEdgeData()"/>, but the other functions are only 
        /// called whenever their respective data needs re-initialization.
        /// </remarks>
        protected virtual void InitParameters()
        {
        }

        /// <summary>
        /// Initialize extra data for each <typeparamref name="Node"/> instance
        /// in this algorithm's graph. This is called after the parameters have
        /// been initialized, but before the extra edge data is initialized.
        /// </summary><remarks>
        /// This is called after <see cref="InitParameters()"/> and before <see
        /// cref="InitEdgeData()"/>, but the other functions are only called
        /// whenever their respective data needs re-initialization.
        /// </remarks>
        protected virtual void InitNodeData()
        {
        }

        /// <summary>
        /// Initialize extra data for each <typeparamref name="Edge"/> instance
        /// in this algorithm's graph.
        /// </summary><remarks>
        /// This is called after <see cref="InitParameters()"/> and <see 
        /// cref="InitNodeData()"/>, but the other functions are only called
        /// whenever their respective data needs re-initialization.
        /// </remarks>
        protected virtual void InitEdgeData()
        {
        }

        /// <summary>
        /// Begins an iteration by re-initializing any localized data with
        /// a source which has changed since the last time an iteration began,
        /// including the <see cref="Parameters"/> and node and edge data.
        /// </summary>
        protected void BeginIteration()
        {
            if (this.mParamsDirty)
                this.InitParameters();
            int count = this.mGraph.NodeCount;
            if (count != this.mLastNodeCount)
                this.InitNodeData();
            this.mLastNodeCount = count;
            count = this.mGraph.EdgeCount;
            if (count != this.mLastEdgeCount)
                this.InitEdgeData();
            this.mLastEdgeCount = count;
            this.OnBeginIteration();
        }

        /// <summary>
        /// Reimplement this function to trigger events and other actions
        /// that occur after all localized data has been re-initialized
        /// and right before the iteration actually begins (before
        /// <see cref="PeformIteration(int,int)"/> is called).
        /// </summary>
        protected virtual void OnBeginIteration()
        {
        }

        protected void EndIteration(int iteration, double statusInPercent, 
            string message, bool abort)
        {
            Node[] nodes = this.mGraph.Nodes;
            Edge[] edges = this.mGraph.Edges;
            float pbL = this.mParameters.X;
            float pbR = this.mParameters.X + this.mParameters.Width;
            float pbT = this.mParameters.Y;
            float pbB = this.mParameters.Y + this.mParameters.Height;
            float dx = this.mParameters.Width;
            float dy = this.mParameters.Height;
            float dist = 0f;
            float maxDist = nodes.Length * (dx * dx + dy * dy);
            Node node;
            int i;
            // Check for moved nodes
            bool nodeMoved = false;
            PointF np;
            for (i = 0; i < nodes.Length; i++)
            {
                node = nodes[i];
                // Constrain to scene bounding box
                dx = node.NewX;
                dy = node.NewY;
                np = node.MapToScene(new PointF(dx, dy));
                dx = dx + Math.Min(Math.Max(np.X, pbL), pbR) - np.X;
                dy = dy + Math.Min(Math.Max(np.Y, pbT), pbB) - np.Y;
                node.NewX = dx;
                node.NewY = dy;
                if (node.X != dx || node.Y != dy)
                {
                    nodeMoved = true;
                    dx = node.X - dx;
                    dy = node.Y - dy;
                    dist += dx * dx + dy * dy;
                }
            }
            if (nodeMoved)
            {
                // Mark edges attached to nodes that have moved
                bool[] dirtyEdge = new bool[edges.Length];
                for (i = 0; i < edges.Length; i++)
                {
                    node = edges[i].SrcNode;
                    if (node.X != node.NewX || node.Y != node.NewY)
                    {
                        dirtyEdge[i] = true;
                        continue;
                    }
                    node = edges[i].DstNode;
                    if (node.X != node.NewX || node.Y != node.NewY)
                    {
                        dirtyEdge[i] = true;
                        continue;
                    }
                    dirtyEdge[i] = false;
                }
                // Move nodes to their new positions
                for (i = 0; i < nodes.Length; i++)
                {
                    node = nodes[i];
                    node.SetPosition(node.NewX, node.NewY);
                }
                // Update edges attached to nodes that have moved
                for (i = 0; i < edges.Length; i++)
                {
                    if (dirtyEdge[i])
                        edges[i].Update();
                }
            }
        }
    }

    public class FDLayoutParameters : LayoutParameters
    {
        private int mMaxIterations;

        public int MaxIterations
        {
            get { return this.mMaxIterations; }
            set
            {
                if (this.mMaxIterations != value)
                {
                    this.mMaxIterations = value;
                    this.OnPropertyChanged("MaxIterations");
                }
            }
        }

        public FDLayoutParameters(RectangleF bounds, int maxIterations)
            : base(bounds)
        {
            this.mMaxIterations = maxIterations;
        }
    }

    public abstract class FDLayoutAlgorithmBase<Node, Edge, Params>
        : LayoutAlgorithmBase<Node, Edge, Params>
        where Node : GraphElement, ILayoutNode
        where Edge : IGraphEdge<Node>, IUpdateable
        where Params : FDLayoutParameters
    {
        private int mMaxIterations;

        public FDLayoutAlgorithmBase(DirectionalGraph<Node, Edge> graph)
            : base(graph)
        {
        }

        protected override void InitParameters()
        {
            base.InitParameters();
            this.mMaxIterations = this.Parameters.MaxIterations;
        }

        protected virtual bool IsFinished()
        {
            return true;
        }

        protected abstract bool PerformIteration(int iteration, int maxIterations);

        protected override void InternalCompute()
        {
            bool abort = false;
            for (int i = 0; i < this.mMaxIterations && 
                this.State != ComputeState.Aborting && 
                this.IsFinished(); i++)
            {
                this.BeginIteration();
                abort = !this.PerformIteration(i, this.mMaxIterations);
                this.EndIteration(i, i / (double)this.mMaxIterations, null, abort);
            }
        }
    }

    public class ElasticLayoutParameters : FDLayoutParameters
    {
        private float mForceMultiplier = 75f;
        private float mWeightMultiplier = 10f;

        public float ForceMultiplier
        {
            get { return this.mForceMultiplier; }
        }

        public float WeightMultiplier
        {
            get { return this.mWeightMultiplier; }
        }

        public ElasticLayoutParameters() 
            : base(new RectangleF(0, 0, 300, 300), 2000)
        {
        }

        public ElasticLayoutParameters(RectangleF bounds, int maxIterations)
            : base(bounds, maxIterations)
        {
        }
    }

    public class ElasticLayoutAlgorithm<Node, Edge>
        : FDLayoutAlgorithmBase<Node, Edge, ElasticLayoutParameters>
        where Node : GraphElement, ILayoutNode
        where Edge : IGraphEdge<Node>, IUpdateable
    {
        private float mForceMult;
        private float mWeightMult;

        public ElasticLayoutAlgorithm(DirectionalGraph<Node, Edge> graph)
            : base(graph)
        {
        }

        protected override void InitParameters()
        {
            base.InitParameters();
            this.mForceMult = this.Parameters.ForceMultiplier;
            this.mWeightMult = this.Parameters.WeightMultiplier;
        }

        private static readonly PointF sZero = new PointF(0, 0);

        protected override bool PerformIteration(int iteration, int maxIterations)
        {
            DirectionalGraph<Node, Edge>.GraphNode[] nodes
                = this.mGraph.InternalNodes;
            DirectionalGraph<Node, Edge>.GraphEdge[] edges;
            Node node, n;
            PointF vec;
            float xvel, yvel, dx, dy, factor;
            int i, j;
            for (i = 0; i < nodes.Length; i++)
            {
                node = nodes[i].Data;
                if (node.PositionFixed)
                {
                    node.NewX = node.X;
                    node.NewY = node.Y;
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
                        vec = node.MapToItem(n, new PointF(0, 0));
                        dx = vec.X;
                        dy = vec.Y;
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
                    vec = node.MapToItem(edges[j].DstNode.Data, sZero);
                    xvel -= vec.X / factor;
                    yvel -= vec.Y / factor;
                }

                if (Math.Abs(xvel) < 0.1 && Math.Abs(yvel) < 0.1)
                    xvel = yvel = 0;

                node.NewX = node.X + xvel;
                node.NewY = node.Y + yvel;
            }
            return true;
        }
    }

    public class ISOMLayoutParameters<Node> : FDLayoutParameters
        where Node : class
    {
        private int mRadiusConstantTime = 100;
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
                    this.OnPropertyChanged("RadiusConstantTime");
                }
            }
        }

        private int mInitialRadius = 5;
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
                    this.OnPropertyChanged("InitialRadius");
                }
            }
        }

        private int mMinRadius = 1;
        /// <summary>
        /// Minimum radius. Default value is 1.
        /// </summary>
        public int MinRadius
        {
            get { return this.mMinRadius; }
            set
            {
                this.mMinRadius = value;
                this.OnPropertyChanged("MinRadius");
            }
        }

        private double mInitialAdaptation = 0.9;
        /// <summary>
        /// Initial adaption. Default value is 0.9.
        /// </summary>
        public double InitialAdaptation
        {
            get { return this.mInitialAdaptation; }
            set
            {
                this.mInitialAdaptation = value;
                this.OnPropertyChanged("InitialAdaptation");
            }
        }

        private double mMinAdaptation;
        /// <summary>
        /// Minimum Adaption. Default value is 0.
        /// </summary>
        public double MinAdaptation
        {
            get { return this.mMinAdaptation; }
            set
            {
                this.mMinAdaptation = value;
                this.OnPropertyChanged("MinAdaptation");
            }
        }

        private double mCoolingFactor = 2;
        /// <summary>
        /// Cooling factor. Default value is 2.
        /// </summary>
        public double CoolingFactor
        {
            get { return this.mCoolingFactor; }
            set
            {
                this.mCoolingFactor = value;
                this.OnPropertyChanged("CoolingFactor");
            }
        }

        private Node mBarycenter = null;
        /// <summary>
        /// The <typeparamref name="Node"/> at the center of the ISOM
        /// Layout Algorithm, which is picked at random on each iteration
        /// if this is null. Default value is null.
        /// </summary>
        public Node Barycenter
        {
            get { return this.mBarycenter; }
            set
            {
                if (this.mBarycenter != value)
                {
                    this.mBarycenter = value;
                    this.OnPropertyChanged("Barycenter");
                }
            }
        }

        public ISOMLayoutParameters()
            : base(new RectangleF(0, 0, 300, 300), 2000)
        {
        }
    }

    public class ISOMLayoutAlgorithm<Node, Edge>
        : FDLayoutAlgorithmBase<Node, Edge, ISOMLayoutParameters<Node>>
        where Node : GraphElement, ILayoutNode
        where Edge : IGraphEdge<Node>, IUpdateable
    {
        private Queue<DirectionalGraph<Node, Edge>.GraphNode> mQueue;
        private readonly Random mRnd = new Random(DateTime.Now.Millisecond);
        private PointF mTempPos;
        private int mMaxIterations;
        private int mRadiusConstantTime;
        private int mRadius;
        private int mMinRadius;
        private double mAdaptation;
        private double mInitialAdaptation;
        private double mMinAdaptation;
        private double mCoolingFactor;

        private bool mBarycenterFound = false;
        private DirectionalGraph<Node, Edge>.GraphNode mBarycenter;

        public ISOMLayoutAlgorithm(DirectionalGraph<Node, Edge> graph)
            : base(graph)
        {
            this.mQueue = new Queue<DirectionalGraph<Node, Edge>.GraphNode>();
        }

        private void FindBarycenter()
        {
            Node node = this.Parameters.Barycenter;
            if (node == null)
            {
                this.mBarycenter = null;
            }
            else
            {
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

        protected override void InitParameters()
        {
            base.InitParameters();

            this.mMaxIterations = this.Parameters.MaxIterations;
            this.mRadiusConstantTime = this.Parameters.RadiusConstantTime;
            this.mRadius = this.Parameters.InitialRadius;
            this.mMinRadius = this.Parameters.MinRadius;
            this.mAdaptation = this.Parameters.InitialAdaptation;
            this.mInitialAdaptation = this.Parameters.InitialAdaptation;
            this.mMinAdaptation = this.Parameters.MinAdaptation;
            this.mCoolingFactor = this.Parameters.CoolingFactor;

            this.FindBarycenter();
            this.mBarycenterFound = true;

            // Initialize vertex positions
            this.InitializeWithRandomPositions();
        }

        /// <summary>
        /// Re-finds the barycenter on the chance that the old barycenter was
        /// removed from the graph.
        /// </summary>
        protected override void InitNodeData()
        {
            base.InitNodeData();
            if (!this.mBarycenterFound)
            {
                this.FindBarycenter();
                this.mBarycenterFound = true;
            }
        }

        /// <summary>
        /// Re-finds the barycenter on the chance that the old barycenter was
        /// orphaned by the removal of its only connecting edge from the graph,
        /// meaning that it can no longer validly function as the barycenter.
        /// </summary>
        protected override void InitEdgeData()
        {
            base.InitEdgeData();
            if (!this.mBarycenterFound)
            {
                this.FindBarycenter();
                this.mBarycenterFound = true;
            }
        }

        protected override void OnBeginIteration()
        {
            base.OnBeginIteration();
            this.mBarycenterFound = false;
        }

        protected override bool PerformIteration(int iteration, int maxIterations)
        {
            this.Adjust();

            // Update Parameters
            double factor = Math.Exp(-this.mCoolingFactor * iteration / maxIterations);
            this.mAdaptation = Math.Max(this.mMinAdaptation, factor * this.mInitialAdaptation);
            if (this.mRadius > this.mMinRadius && iteration % this.mRadiusConstantTime == 0)
            {
                this.mRadius--;
            }
            return true;
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

            DirectionalGraph<Node, Edge>.GraphNode current, n;
            Node node;
            float posX, posY;
            double factor;
            PointF force;
            DirectionalGraph<Node, Edge>.GraphEdge[] edges;
            int i;
            while (this.mQueue.Count > 0)
            {
                current = this.mQueue.Dequeue();
                node = current.Data;
                if (node.PositionFixed)
                {
                    node.NewX = node.X;
                    node.NewY = node.Y;
                }
                else
                {
                    posX = node.X;
                    posY = node.Y;

                    force = node.MapFromScene(this.mTempPos);
                    factor = this.mAdaptation / Math.Pow(2, current.Distance);

                    node.NewX = (float)(posX + factor * force.X);
                    node.NewY = (float)(posY + factor * force.Y);
                }

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
        /// <see cref="Parameters"/>.</param>
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
