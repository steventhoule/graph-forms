using System;

namespace GraphForms.Algorithms.Layout.ForceDirected
{
    public abstract class FRLayoutAlgorithm<Node, Edge>
        : LayoutAlgorithm<Node, Edge>
        where Node : class, ILayoutNode
        where Edge : IGraphEdge<Node>, IUpdateable
    {
        public enum Cooling
        {
            Linear,
            Exponential
        }

        /// <summary>
        /// Actual temperature of the 'mass'. Used for cooling.
        /// </summary>
        private double mTemperature;

        private double mMinimalTemperature;
        /// <summary>
        /// Constant of Attraction calculated from the parameters
        /// </summary>
        private float mCoA;
        /// <summary>
        /// Constant of Repulsion calculated from the parameters
        /// </summary>
        private float mCoR;

        private float mAttractionMultiplier = 1.2f;
        private float mRepulsiveMultiplier = 0.6f;
        private float mLambda = 0.95f;
        private Cooling mCoolingFunction = Cooling.Exponential;

        private static readonly double[] sEmptyCoords = new double[0];
        private double[] mNewXs;
        private double[] mNewYs;

        public FRLayoutAlgorithm(Digraph<Node, Edge> graph,
            IClusterNode clusterNode)
            : base(graph, clusterNode)
        {
            this.MaxIterations = 200;
            this.mNewXs = sEmptyCoords;
            this.mNewYs = sEmptyCoords;
        }

        public FRLayoutAlgorithm(Digraph<Node, Edge> graph,
            Box2F boundingBox)
            : base(graph, boundingBox)
        {
            this.MaxIterations = 200;
            this.mNewXs = sEmptyCoords;
            this.mNewYs = sEmptyCoords;
        }

        /// <summary>
        /// Recalculates all parameters that are dependent on the values
        /// of other parameters, including <see cref="ConstantOfRepulsion"/>
        /// and <see cref="ConstantOfAttraction"/>.
        /// </summary>
        protected void UpdateParameters()
        {
            this.CalculateConstantOfRepulsion();
            this.CalculateConstantOfAttraction();
            this.mMinimalTemperature = this.InitialTemperature * 0.01;
        }

        private void CalculateConstantOfRepulsion()
        {
            this.mCoR = (float)Math.Pow(this.K *
                this.mRepulsiveMultiplier, 2);
        }

        private void CalculateConstantOfAttraction()
        {
            this.mCoA = this.K * this.mAttractionMultiplier;
        }

        /// <summary>
        /// Gets the computed ideal edge length.
        /// </summary>
        public abstract float K { get; }

        /// <summary>
        /// Gets the initial temperature of the mass.
        /// </summary>
        public abstract float InitialTemperature { get; }

        /// <summary>
        /// Constant of the attraction, which equals <code><see cref="K"/> * 
        /// <see cref="AttractionMultiplier"/></code>.
        /// </summary>
        public float ConstantOfAttraction
        {
            get { return this.mCoA; }
        }

        /// <summary>
        /// Multiplier of the attraction. Default value is 2.
        /// </summary>
        public float AttractionMultiplier
        {
            get { return mAttractionMultiplier; }
            set
            {
                if (this.mAttractionMultiplier != value)
                {
                    this.mAttractionMultiplier = value;
                    this.CalculateConstantOfAttraction();
                }
            }
        }

        /// <summary>
        /// Constant of the repulsion, which equals <code>Pow(<see cref="K"/> *
        /// <see cref="RepulsiveMultiplier"/>, 2)</code>.
        /// </summary>
        public float ConstantOfRepulsion
        {
            get { return this.mCoR; }
        }

        /// <summary>
        /// Multiplier of the repulsion. Default value is 1.
        /// </summary>
        public float RepulsiveMultiplier
        {
            get { return this.mRepulsiveMultiplier; }
            set
            {
                if (this.mRepulsiveMultiplier != value)
                {
                    this.mRepulsiveMultiplier = value;
                    this.CalculateConstantOfRepulsion();
                }
            }
        }

        /// <summary>
        /// Lambda for the cooling function. Default value is 0.95.
        /// </summary>
        public float Lambda
        {
            get { return this.mLambda; }
            set
            {
                if (this.mLambda != value)
                {
                    this.mLambda = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the cooling function 
        /// which could be Linear or Exponential.
        /// </summary>
        public Cooling CoolingFunction
        {
            get { return this.mCoolingFunction; }
            set
            {
                if (this.mCoolingFunction != value)
                {
                    this.mCoolingFunction = value;
                }
            }
        }

        protected override void InitializeAlgorithm()
        {
            //base.InitializeAlgorithm();
            this.mTemperature = this.InitialTemperature;
            this.mMinimalTemperature = this.mTemperature * 0.01;
        }

        protected override bool CanIterate()
        {
            return this.mTemperature > this.mMinimalTemperature;
        }

        protected override void PerformIteration(uint iteration)
        {
            this.IterateOne();

            // cool down graph
            switch (this.mCoolingFunction)
            {
                case Cooling.Linear:
                    this.mTemperature *= (1.0 - (double)iteration / this.MaxIterations);
                    break;
                case Cooling.Exponential:
                    this.mTemperature *= this.mLambda;
                    break;
            }
        }

        private void IterateOne()
        {
            int i, j;
            double forceX, forceY, dx, dy, length, factor;

            Digraph<Node, Edge>.GEdge edge;
            Digraph<Node, Edge>.GNode u, v;

            i = this.mGraph.NodeCount;
            if (this.mNewXs.Length < i)
            {
                this.mNewXs = new double[i];
                this.mNewYs = new double[i];
            }

            // Repulsive forces
            for (i = this.mGraph.NodeCount - 1; i >= 0; i--)
            {
                v = this.mGraph.InternalNodeAt(i);
                if (v.Data.PositionFixed || v.Hidden)
                    continue;
                //nodes[i].Index = i;
                forceX = 0; forceY = 0;
                for (j = this.mGraph.NodeCount - 1; j >= 0; j--)
                {
                    // doesn't repulse itself
                    u = this.mGraph.InternalNodeAt(j);
                    if (j != i && !u.Hidden)
                    {
                        // calculating repulsive force
                        dx = v.Data.X - u.Data.X;
                        dy = v.Data.Y - u.Data.Y;
                        length = Math.Max(dx * dx + dy * dy, 0.000001);
                        factor = this.mCoR / length;

                        forceX += dx * factor;
                        forceY += dy * factor;
                    }
                }
                //v.Data.SetNewPosition((float)forceX, (float)forceY);
                this.mNewXs[i] = forceX;
                this.mNewYs[i] = forceY;
            }

            // Attractive forces
            for (i = this.mGraph.EdgeCount - 1; i >= 0; i--)
            {
                edge = this.mGraph.InternalEdgeAt(i);
                if (edge.Hidden)
                    continue;
                v = edge.SrcNode;
                if (v.Data.PositionFixed || v.Hidden)
                    continue;
                u = edge.DstNode;
                if (u.Index == v.Index)
                    continue;

                // calculating attractive forces between two nodes
                dx = v.Data.X - u.Data.X;
                dy = v.Data.Y - u.Data.Y;
                length = Math.Sqrt(dx * dx + dy * dy);
                factor = edge.Data.Weight;
                factor = Math.Max(length / this.mCoA * factor, 0.000001);

                //v.SetNewPosition(v.NewX - (float)(dx / factor), 
                //                 v.NewY - (float)(dy / factor));
                forceX = this.mNewXs[v.Index] - dx / factor;
                forceY = this.mNewYs[v.Index] - dy / factor;
                this.mNewXs[v.Index] = forceX;
                this.mNewYs[v.Index] = forceY;
            }

            // Limit Displacement
            for (i = this.mGraph.NodeCount - 1; i >= 0; i--)
            {
                // limit force to ambient temperature.
                v = this.mGraph.InternalNodeAt(i);
                if (v.Data.PositionFixed || v.Hidden)
                {
                    //v.SetNewPosition(v.X, v.Y);
                    continue;
                }
                dx = this.mNewXs[i];//v.NewX;
                dy = this.mNewYs[i];//v.NewY;
                //dx = newXs[i];
                //dy = newYs[i];
                length = Math.Max(Math.Sqrt(dx * dx + dy * dy), 0.000001);
                factor = Math.Min(length, this.mTemperature) / length;

                // Add the force to the old position
                v.Data.SetPosition((float)(v.Data.X + dx * factor),
                                   (float)(v.Data.Y + dy * factor));

                // Constraining new position to within scene bounding box
                // is already handled by LayoutAlgorithm base
            }
        }
    }
}
