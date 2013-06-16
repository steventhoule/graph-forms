using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace GraphForms.Algorithms.Layout
{
    /// <summary>
    /// This interface acts as a virtual spring, creating force vectors 
    /// that are proportional to given translation vectors and that
    /// decrease as the spring reaches equilibrium (as the translation
    /// vector's length approaches zero).
    /// </summary>
    public interface ILayoutSpring
    {
        /// <summary>
        /// Given a translation vector from the current position of an 
        /// <see cref="ILayoutNode"/> to its new position, this function 
        /// calculates a "force" vector in the same direction that is
        /// added to the layout node's current position to get
        /// an intermediate position for a "twaining" animation.
        /// </summary>
        /// <param name="dx">The horizontal difference between
        /// the layout node's new position and current position.</param>
        /// <param name="dy">The vertical difference between
        /// the layout node's new position and current position.</param>
        /// <returns>A "force" vector that will be added to the 
        /// current position of the layout node being moved.</returns>
        PointF GetSpringForce(float dx, float dy);
    }
}
