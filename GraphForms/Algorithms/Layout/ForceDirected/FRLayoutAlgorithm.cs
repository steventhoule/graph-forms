using System;
using System.Drawing;

namespace GraphForms.Algorithms.Layout.ForceDirected
{
    public abstract class FRLayoutAlgorithm<Node, Edge>
        //: ForceDirectedLayoutAlgorithm<Node, Edge, FRLayoutParameters>
        : LayoutAlgorithm<Node, Edge>
        where Node : ILayoutNode
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

        /*protected override FRLayoutParameters DefaultParameters
        {
            get { return new FRFreeLayoutParameters(); }
        }

        public FRLayoutAlgorithm(Digraph<Node, Edge> graph)
            : base(graph, null)
        {
        }

        public FRLayoutAlgorithm(Digraph<Node, Edge> graph,
            FRLayoutParameters oldParameters)
            : base(graph, oldParameters)
        {
        }/* */

        public FRLayoutAlgorithm(Digraph<Node, Edge> graph,
            IClusterNode clusterNode)
            : base(graph, clusterNode)
        {
            this.MaxIterations = 200;
        }

        public FRLayoutAlgorithm(Digraph<Node, Edge> graph,
            RectangleF boundingBox)
            : base(graph, boundingBox)
        {
            this.MaxIterations = 200;
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
            this.mTemperature = this.InitialTemperature;//this.Parameters.InitialTemperature;
            this.mMinimalTemperature = this.mTemperature * 0.01;
        }

        /*protected override bool OnBeginIteration(bool paramsDirty, 
            int lastNodeCount, int lastEdgeCount)
        {
            if (paramsDirty)
            {
                FRLayoutParameters param = this.Parameters;
                // Update the node count just in case it changed,
                // in order to force parameter recalculation.
                param.NodeCount = this.mGraph.NodeCount;
                // Don't worry about making NodeCount public,
                // since the above action will erase any user changes 
                // to the NodeCount value between iterations.

                this.mMinimalTemperature = param.InitialTemperature * 0.01;
                this.mCoA = param.ConstantOfAttraction;
                this.mCoR = param.ConstantOfRepulsion;
                this.mLambda = param.Lambda;
                this.mCoolingFunction = param.CoolingFunction;
            }
            return base.OnBeginIteration(paramsDirty, lastNodeCount, lastEdgeCount);
        }/* */

        protected override bool CanIterate()
        {
            return this.mTemperature > this.mMinimalTemperature;
        }

        protected override void PerformIteration(uint iteration)//, int maxIterations)
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

        private static readonly PointF sZero = new PointF(0, 0);

        private void IterateOne()
        {
            int i, j;
            //SizeF delta;
            double forceX, forceY, dx, dy, length, factor;

            //float[] newXs = this.NewXPositions;
            //float[] newYs = this.NewYPositions;
            Digraph<Node, Edge>.GNode[] nodes = this.mGraph.InternalNodes;
            Digraph<Node, Edge>.GEdge[] edges;
            Node u, v;

            // Repulsive forces
            for (i = 0; i < nodes.Length; i++)
            {
                v = nodes[i].Data;
                if (v.PositionFixed)
                    continue;
                nodes[i].Index = i;
                forceX = 0; forceY = 0;
                for (j = 0; j < nodes.Length; j++)
                {
                    // doesn't repulse itself
                    if (j != i)
                    {
                        // calculating repulsive force
                        u = nodes[j].mData;
                        //delta = v.ItemTranslate(u);
                        dx = v.X - u.X;//delta.Width;
                        dy = v.Y - u.Y;//delta.Height;
                        length = Math.Max(dx * dx + dy * dy, 0.000001);
                        factor = this.mCoR / length;

                        forceX += dx * factor;
                        forceY += dy * factor;
                    }
                }
                //v.NewX = (float)forceX;
                //v.NewY = (float)forceY;
                v.SetNewPosition((float)forceX, (float)forceY);
                //newXs[i] = (float)forceX;
                //newYs[i] = (float)forceY;
            }

            // Attractive forces
            edges = this.mGraph.InternalEdges;
            for (i = 0; i < edges.Length; i++)
            {
                v = edges[i].SrcNode.Data;
                if (v.PositionFixed)
                    continue;
                u = edges[i].DstNode.Data;
                if (edges[i].SrcNode.Index == edges[i].DstNode.Index)
                    continue;

                // calculating attractive forces between two nodes
                //delta = v.ItemTranslate(u);
                dx = v.X - u.X;//delta.Width;
                dy = v.Y - u.Y;//delta.Height;
                length = Math.Sqrt(dx * dx + dy * dy);
                factor = edges[i].Data.Weight;
                factor = Math.Max(length / this.mCoA * factor, 0.000001);

                //v.NewX = v.NewX - (float)(dx * factor);
                //v.NewY = v.NewY - (float)(dy * factor);
                v.SetNewPosition(v.NewX - (float)(dx / factor), 
                                 v.NewY - (float)(dy / factor));
                //j = edges[i].SrcNode.Index;
                //newXs[j] = newXs[j] - (float)(dx / factor);
                //newYs[j] = newYs[j] - (float)(dy / factor);
            }

            // Limit Displacement
            for (i = 0; i < nodes.Length; i++)
            {
                // limit force to ambient temperature.
                v = nodes[i].Data;
                if (v.PositionFixed)
                {
                    //v.NewX = v.X;
                    //v.NewY = v.Y;
                    v.SetNewPosition(v.X, v.Y);
                    //newXs[i] = v.X;
                    //newYs[i] = v.Y;
                    continue;
                }
                dx = v.NewX;
                dy = v.NewY;
                //dx = newXs[i];
                //dy = newYs[i];
                length = Math.Max(Math.Sqrt(dx * dx + dy * dy), 0.000001);
                factor = Math.Min(length, this.mTemperature) / length;

                // Add the force to the old position
                //v.NewX = (float)(v.X + dx * factor);
                //v.NewY = (float)(v.Y + dy * factor);
                v.SetNewPosition((float)(v.X + dx * factor), 
                                 (float)(v.Y + dy * factor));
                //newXs[i] = (float)(v.X + dx * factor);
                //newYs[i] = (float)(v.Y + dy * factor);

                // Constraining new position to within scene bounding box
                // is already handled by ForceDirLayoutAlgorithm
            }
        }
    }
}
