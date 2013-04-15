using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Layout.ForceDirected
{
    /// <summary>
    /// Parameters base for the Fruchterman-Reingold Force-Directed Algorithm.
    /// </summary>
    public abstract class FRLayoutParameters : ForceDirectedLayoutParameters
    {
        public enum Cooling
        {
            Linear,
            Exponential
        }

        private int mNodeCount = 1;
        private float mConstantOfAttraction;
        private float mConstantOfRepulsion;
        private float mAttractionMultiplier = 1.2f;
        private float mRepulsiveMultiplier = 0.6f;
        private float mLambda = 0.95f;
        private Cooling mCoolingFunction = Cooling.Exponential;

        /// <summary>
        /// Number of nodes in the graph being arranged
        /// (used to calculate the constants).
        /// Must be greater than zero.
        /// </summary>
        public int NodeCount
        {
            get { return this.mNodeCount; }
            set
            {
                if (this.mNodeCount != value)
                {
                    if (value <= 0)
                        throw new ArgumentOutOfRangeException("value");
                    this.mNodeCount = value;
                    this.OnPropertyChanged("NodeCount");
                    this.UpdateParameters();
                }
            }
        }

        /// <summary>
        /// Recalculates all parameters that are dependent on the values
        /// of other parameters, including <see cref="ConstantOfRepulsion"/>
        /// and <see cref="ConstantOfAttraction"/>.
        /// </summary>
        protected virtual void UpdateParameters()
        {
            CalculateConstantOfRepulsion();
            CalculateConstantOfAttraction();
        }

        private void CalculateConstantOfRepulsion()
        {
            this.mConstantOfRepulsion = (float)Math.Pow(this.K *
                this.mRepulsiveMultiplier, 2);
            this.OnPropertyChanged("ConstantOfRepulsion");
        }

        private void CalculateConstantOfAttraction()
        {
            this.mConstantOfAttraction = this.K * this.mAttractionMultiplier;
            this.OnPropertyChanged("ConstantOfAttraction");
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
            get { return this.mConstantOfAttraction; }
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
                    this.OnPropertyChanged("AttractionMultiplier");
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
            get { return this.mConstantOfRepulsion; }
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
                    this.OnPropertyChanged("RepulsiveMultiplier");
                    this.CalculateConstantOfRepulsion();
                }
            }
        }

        /// <summary>
        /// Limit of the iterations. Default value is 200.
        /// </summary>
        public int IterationLimit
        {
            get { return this.mMaxIterations; }
            set { this.MaxIterations = value; }
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
                    this.OnPropertyChanged("Lamdba");
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
                    this.OnPropertyChanged("CoolingFunction");
                }
            }
        }

        /// <summary>
        /// Width of the <see cref="LayoutParameters.BoundingBox"/>. 
        /// Default value is 100.
        /// </summary>
        public override float Width
        {
            get { return this.mWidth; }
            set
            {
                if (this.mWidth != value)
                {
                    this.mWidth = value;
                    this.OnPropertyChanged("Width");
                    this.UpdateParameters();
                }
            }
        }

        /// <summary>
        /// Height of the <see cref="LayoutParameters.BoundingBox"/>. 
        /// Default value is 100.
        /// </summary>
        public override float Height
        {
            get { return this.mHeight; }
            set
            {
                if (this.mHeight != value)
                {
                    this.mHeight = value;
                    this.OnPropertyChanged("Height");
                    this.UpdateParameters();
                }
            }
        }

        public FRLayoutParameters()
            : base(0, 0, 100, 100, 200)
        {
            //update the parameters
            this.UpdateParameters();
        }
    }
}
