using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Layout
{
    /// <summary>
    /// The base interface for algorithms which calculate the layout of a given
    /// <see cref="T:DirectionalGraph`2{Node,Edge}"/> instance by setting the
    /// positions of its <c>Node</c> instances based on the <c>Edge</c>
    /// instances connecting them and the parameter values within the 
    /// <see cref="LayoutParameters"/> instance.
    /// </summary>
    public interface ILayoutAlgorithm : IAlgorithm
    {
        /// <summary>
        /// The <see cref="LayoutParameters"/> instance used to influence the
        /// behavior of this layout algorithm and how it arranges the nodes
        /// in its graph.
        /// </summary>
        LayoutParameters Parameters { get; }

        /// <summary>
        /// Shuffles all the node instances in this algorithm's graph by 
        /// setting their positions to random points within the 
        /// <see cref="LayoutParameters.BoundingBox"/> of this algorithm's
        /// <see cref="Parameters"/>.</summary>
        void ShuffleNodePositions();

        /// <summary>
        /// Shuffles all the node instances in this algorithm's graph by 
        /// setting their positions to random points within the given 
        /// bounding box in scene-level coordinates.</summary>
        /// <param name="x">The X-coordinate (in scene coordinates) of 
        /// the upper-left corner of the bounding box.</param>
        /// <param name="y">The Y-coordinate (in scene coordinates) of 
        /// the upper-left corner of the bounding box.</param>
        /// <param name="width">The width of the bounding box.</param>
        /// <param name="height">The height of the bounding box.</param>
        void ShuffleNodePositions(float x, float y,
            float width, float height);
    }
}
