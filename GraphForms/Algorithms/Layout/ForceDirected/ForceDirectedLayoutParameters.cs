using System;

namespace GraphForms.Algorithms.Layout.ForceDirected
{
    /// <summary>
    /// Contains data used by force-directed layout algorithms, which can be 
    /// quickly swapped out for other instances containing different data.
    /// </summary>
    public class ForceDirectedLayoutParameters : LayoutParameters
    {
        /// <summary>
        /// Maximum number of iterations.
        /// </summary>
        protected int mMaxIterations;

        /// <summary>
        /// Maximum number of iterations.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <see cref="MaxIterations"/> is set to a value less than zero.
        /// </exception>
        public virtual int MaxIterations
        {
            get { return this.mMaxIterations; }
            set
            {
                if (this.mMaxIterations != value)
                {
                    if (value < 0)
                        throw new ArgumentOutOfRangeException("value");
                    this.mMaxIterations = value;
                    this.OnPropertyChanged("MaxIterations");
                }
            }
        }

        /// <summary>
        /// Initializes a new <see cref="ForceDirectedLayoutParameters"/>
        /// instance with its <see cref="LayoutParameters.BoundingBox"/> set 
        /// to the given rectangle and its <see cref="MaxIterations"/> set to 
        /// <paramref name="maxIterations"/>.</summary>
        /// <param name="x">X-coordinate of the upper-left corner of the
        /// <see cref="LayoutParameters.BoundingBox"/>.</param>
        /// <param name="y">Y-coordinate of the upper-left corner of the
        /// <see cref="LayoutParameters.BoundingBox"/>.</param>
        /// <param name="width">Width of the 
        /// <see cref="LayoutParameters.BoundingBox"/>.</param>
        /// <param name="height">Height of the 
        /// <see cref="LayoutParameters.BoundingBox"/>.</param>
        /// <param name="maxIterations">maximum number of iterations allowed
        /// in the force-directed algorithm before it stops its computation.</param>
        public ForceDirectedLayoutParameters(float x, float y, float width, float height,
            int maxIterations)
            : base(x, y, width, height)
        {
            this.mMaxIterations = maxIterations;
        }
    }
}
