using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Layout.ForceDirected
{
    /// <summary>
    /// A simple interface for algorithms which calculate the layout of a given
    /// <see cref="T:DirectionalGraph`2{Node,Edge}"/> instance by using physics
    /// calculations to set the positions of its <c>Node</c> instances, 
    /// based on balancing forces carried along the <c>Edge</c> instances 
    /// connecting them and influenced by the parameter values within the 
    /// <see cref="ForceDirectedLayoutParameters"/> instance.
    /// </summary>
    public interface IForceDirectedLayoutAlgorithm : ILayoutAlgorithm
    {
        /// <summary>
        /// The <see cref="ForceDirectedLayoutParameters"/> instance used to 
        /// influence the behavior of this force-directed layout algorithm 
        /// and how it arranges the nodes in its graph.
        /// </summary>
        ForceDirectedLayoutParameters Parameters { get; }

        /// <summary>
        /// Resets this algorithm back to its starting point, which causes it
        /// to re-initialize before beginning its next iteration.
        /// </summary>
        void ResetAlgorithm();

        /// <summary>
        /// Runs a single iteration of this force-directed layout algorithm
        /// asynchronously.
        /// </summary>
        /// <param name="forceRestart">Whether to force this algorithm to
        /// start running again if it has finished or has been aborted.</param>
        /// <returns>true if the iteration is successfully run, or false if
        /// the algorithm has finished or has been aborted.</returns>
        bool AsyncIterate(bool forceRestart);
    }
}
