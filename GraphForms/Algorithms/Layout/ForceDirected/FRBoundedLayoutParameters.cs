using System;

namespace GraphForms.Algorithms.Layout.ForceDirected
{
    /// <summary>
    /// Parameters for the Fruchterman-Reingold Force-Directed Algorithm, 
    /// bounded version, which calculates the ideal edge length and 
    /// initial temperature based on the bounding box size.
    /// </summary>
    public class FRBoundedLayoutParameters : FRLayoutParameters
    {
        private float mK;

        /// <summary>
        /// Ideal edge length, which equals 
        /// <code>Sqrt(<see cref="FRLayoutParameters.Height"/> * 
        /// <see cref="FRLayoutParameters.Width"/> / nodeCount)</code>.
        /// </summary><remarks>
        /// <c>nodeCount</c> is the number of nodes in the graph of the 
        /// algorithm using these parameters.
        /// </remarks>
        public override float K
        {
            get { return this.mK; }
        }

        /// <summary>
        /// Gets the initial temperature of the mass, which equals 
        /// <code>Min(<see cref="FRLayoutParameters.Width"/>, 
        /// <see cref="FRLayoutParameters.Height"/>) / 10</code>.
        /// </summary>
        public override float InitialTemperature
        {
            get { return Math.Min(this.mWidth, this.mHeight) / 10; }
        }

        /// <summary>
        /// Recalculates all parameters that are dependent on the values
        /// of other parameters.
        /// </summary>
        /// <seealso cref="FRLayoutParameters.UpdateParameters()"/>
        protected override void UpdateParameters()
        {
            this.mK = (float)Math.Sqrt(this.mWidth * this.mHeight / this.NodeCount);
            this.OnPropertyChanged("K");
            base.UpdateParameters();
        }
    }
}
