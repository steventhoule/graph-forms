using System;

namespace GraphForms.Algorithms.Layout.ForceDirected
{
    /// <summary>
    /// Parameters for the Fruchterman-Reingold Force-Directed Algorithm, 
    /// free version, which lets the user set the ideal edge length and
    /// calculates the initial temperature based on that length.
    /// </summary>
    public class FRFreeLayoutParameters : FRLayoutParameters
    {
        private float mK = 10;

        /// <summary>
        /// Gets the computed ideal edge length.
        /// </summary>
        public override float K
        {
            get { return this.mK; }
        }

        /// <summary>
        /// Gets the initial temperature of the mass, which equals 
        /// <code>Sqrt(Pow(<see cref="K"/>, 2) * 
        /// <see cref="FRLayoutParameters.NodeCount"/></code>.
        /// </summary>
        public override float InitialTemperature
        {
            get { return (float)Math.Sqrt(this.mK * this.mK * this.NodeCount); }
        }

        /// <summary>
        /// Constant which represents the ideal length of the edges.
        /// Default value is 10.
        /// </summary>
        public float IdealEdgeLength
        {
            get { return this.mK; }
            set
            {
                if (this.mK != value)
                {
                    this.mK = value;
                    this.OnPropertyChanged("K");
                    this.UpdateParameters();
                }
            }
        }
    }
}
