using System;

namespace GraphForms.Algorithms.Layout.ForceDirected
{
    public class KKLayoutParameters : ForceDirectedLayoutParameters
    {
        private float mK = 1;
        public float K
        {
            get { return this.mK; }
            set
            {
                if (this.mK != value)
                {
                    this.mK = value;
                    this.OnPropertyChanged("K");
                }
            }
        }

        private bool bAdjustForGravity;
        /// <summary>
        /// If true, then after the layout process, the nodes will be moved, 
        /// so the barycenter will be in the center point of the 
        /// <see cref="LayoutParameters.BoundingBox"/>. </summary>
        public bool AdjustForGravity
        {
            get { return this.bAdjustForGravity; }
            set
            {
                if (this.bAdjustForGravity != value)
                {
                    this.bAdjustForGravity = value;
                    this.OnPropertyChanged("AdjustForGravity");
                }
            }
        }

        private bool bExchangeVertices;
        public bool ExchangeVertices
        {
            get { return this.bExchangeVertices; }
            set
            {
                if (this.bExchangeVertices != value)
                {
                    this.bExchangeVertices = value;
                    this.OnPropertyChanged("ExchangeVertices");
                }
            }
        }

        private float mLengthFactor = 1;
        /// <summary>
        /// Multiplier of the ideal edge length. 
        /// (With this parameter the user can modify the ideal edge length).
        /// </summary>
        public float LengthFactor
        {
            get { return this.mLengthFactor; }
            set
            {
                if (this.mLengthFactor != value)
                {
                    this.mLengthFactor = value;
                    this.OnPropertyChanged("LengthFactor");
                }
            }
        }

        private float mDisconnectedMultiplier = 0.5f;
        /// <summary>
        /// Ideal distance between the disconnected points 
        /// (1 is equal the ideal edge length).
        /// </summary>
        public float DisconnectedMultiplier
        {
            get { return this.mDisconnectedMultiplier; }
            set
            {
                if (this.mDisconnectedMultiplier != value)
                {
                    this.mDisconnectedMultiplier = value;
                    this.OnPropertyChanged("DisconnectedMultiplier");
                }
            }
        }

        public KKLayoutParameters() : base(0, 0, 300, 300, 200) { }
    }
}
