using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Layout.ForceDirected
{
    public class ElasticLayoutParameters : ForceDirectedLayoutParameters
    {
        private float mForceMultiplier = 75f;
        private float mWeightMultiplier = 10f;

        public float ForceMultiplier
        {
            get { return this.mForceMultiplier; }
            set
            {
                if (this.mForceMultiplier != value)
                {
                    this.mForceMultiplier = value;
                    this.OnPropertyChanged("ForceMultiplier");
                }
            }
        }

        public float WeightMultiplier
        {
            get { return this.mWeightMultiplier; }
            set
            {
                if (this.mWeightMultiplier != value)
                {
                    this.mWeightMultiplier = value;
                    this.OnPropertyChanged("WeightMultiplier");
                }
            }
        }

        public ElasticLayoutParameters()
            : base(0, 0, 300, 300, 2000)
        {
        }
    }
}
