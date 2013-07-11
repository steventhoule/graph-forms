using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Layout.Circular
{
    /// <summary>
    /// Used to specify which method a circular layout algorithm uses to
    /// calculate the angles between subtrees/leaves/nodes around the
    /// root of their shared circular tree or the center of their circle.
    /// </summary>
    public enum CircleSpacing
    {
        /// <summary>
        /// The angles between all subtrees/leaves/nodes are equal and
        /// based on the number of subtrees/leaves/nodes around their
        /// shared root/center.</summary>
        Fractal,
        /// <summary>
        /// Subtrees of Nonuniform Size. The angles between two 
        /// subtrees/leaves/nodes around their shared root/center are 
        /// based on their "sizes" (calculated with their bounding box
        /// and/or convex hull or shape), which usually results in more 
        /// compact graphs.</summary>
        SNS
    }
}
