using System;
using System.Collections.Generic;
using System.Text;
using GraphForms.Algorithms.Layout.ForceDirected;

namespace GraphForms.Algorithms.Layout.Circular
{
    public class FDSingleCircleLayoutParameters 
        : ForceDirectedLayoutParameters
    {
        private double mMinRadius = 10;

        public double MinRadius
        {
            get { return this.mMinRadius; }
            set
            {
                if (this.mMinRadius != value)
                {
                    this.mMinRadius = value;
                    this.OnPropertyChanged("MinRadius");
                }
            }
        }

        private double mFreeArc = 5;

        public double FreeArc
        {
            get { return this.mFreeArc; }
            set
            {
                if (this.mFreeArc != value)
                {
                    this.mFreeArc = value;
                    this.OnPropertyChanged("FreeArc");
                }
            }
        }

        private double mSpringMultiplier = 1;

        public double SpringMultiplier
        {
            get { return this.mSpringMultiplier; }
            set
            {
                if (this.mSpringMultiplier != value)
                {
                    this.mSpringMultiplier = value;
                    this.OnPropertyChanged("SpringMultiplier");
                }
            }
        }

        private double mMagneticMultiplier = 1;

        public double MagneticMultiplier
        {
            get { return this.mMagneticMultiplier; }
            set
            {
                if (this.mMagneticMultiplier != value)
                {
                    this.mMagneticMultiplier = value;
                    this.OnPropertyChanged("MagneticMultiplier");
                }
            }
        }

        private double mAngleExponent = 1;

        public double AngleExponent
        {
            get { return this.mAngleExponent; }
            set
            {
                if (this.mAngleExponent != value)
                {
                    this.mAngleExponent = value;
                    this.OnPropertyChanged("AngleExponent");
                }
            }
        }

        private bool bCenterInBounds = true;

        public bool CenterInBounds
        {
            get { return this.bCenterInBounds; }
            set
            {
                if (this.bCenterInBounds != value)
                {
                    this.bCenterInBounds = value;
                    this.OnPropertyChanged("CenterInBounds");
                }
            }
        }

        public FDSingleCircleLayoutParameters()
            : base(0, 0, 300, 300, 2000)
        {
        }
    }
}
