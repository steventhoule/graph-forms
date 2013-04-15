using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Layout.ForceDirected
{
    public class ISOMLayoutParameters<Node> : ForceDirectedLayoutParameters
        where Node : class
    {
        /// <summary>
        /// Maximum iteration number. Default value is 2000.
        /// </summary>
        public int MaxEpoch
        {
            get { return this.mMaxIterations; }
            set { this.MaxIterations = value; }
        }

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

        private float mInitialAdaptation = 0.9f;
        /// <summary>
        /// Initial adaptation. Default value is 0.9.
        /// </summary>
        public float InitialAdaptation
        {
            get { return this.mInitialAdaptation; }
            set
            {
                this.mInitialAdaptation = value;
                this.OnPropertyChanged("InitialAdaptation");
            }
        }

        private float mMinAdaptation;
        /// <summary>
        /// Minimum Adaptation. Default value is 0.
        /// </summary>
        public float MinAdaptation
        {
            get { return this.mMinAdaptation; }
            set
            {
                this.mMinAdaptation = value;
                this.OnPropertyChanged("MinAdaptation");
            }
        }

        private float mCoolingFactor = 2;
        /// <summary>
        /// Cooling factor. Default value is 2.
        /// </summary>
        public float CoolingFactor
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
            : base(0, 0, 300, 300, 2000)
        {
        }
    }
}
