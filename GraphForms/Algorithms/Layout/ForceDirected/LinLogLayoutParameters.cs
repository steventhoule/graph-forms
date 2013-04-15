using System;

namespace GraphForms.Algorithms.Layout.ForceDirected
{
    public class LinLogLayoutParameters : ForceDirectedLayoutParameters
    {
        private float mAttractionExponent = 1;

        public float AttractionExponent
        {
            get { return mAttractionExponent; }
            set
            {
                mAttractionExponent = value;
                this.OnPropertyChanged("AttractionExponent");
            }
        }

        private float mRepulsiveExponent = 1;

        public float RepulsiveExponent
        {
            get { return mRepulsiveExponent; }
            set
            {
                mRepulsiveExponent = value;
                this.OnPropertyChanged("RepulsiveExponent");
            }
        }

        private float mGravitationMultiplier = 0.1f;

        public float GravitationMultiplier
        {
            get { return mGravitationMultiplier; }
            set
            {
                mGravitationMultiplier = value;
                this.OnPropertyChanged("GravitationMultiplier");
            }
        }

        public int IterationCount
        {
            get { return this.mMaxIterations; }
            set { base.mMaxIterations = value; }
        }

        public LinLogLayoutParameters() : base(0, 0, 300, 300, 100) { }
    }
}
