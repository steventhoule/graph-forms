using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Layout.ForceDirected
{
    /// <summary>
    /// Free version of the Fruchterman-Reingold Force-Directed Algorithm, 
    /// which lets the user set the ideal edge length and calculates the 
    /// initial temperature based on that length.
    /// </summary>
    public class FRFreeLayoutAlgorithm<Node, Edge>
        : FRLayoutAlgorithm<Node, Edge>
        where Node : class, ILayoutNode
        where Edge : IGraphEdge<Node>, IUpdateable
    {
        private float mK = 10;

        public FRFreeLayoutAlgorithm(Digraph<Node, Edge> graph,
            IClusterNode clusterNode)
            : base(graph, clusterNode)
        {
        }

        public FRFreeLayoutAlgorithm(Digraph<Node, Edge> graph,
            Box2F boundingBox)
            : base(graph, boundingBox)
        {
        }

        /// <summary>
        /// Gets the computed ideal edge length.
        /// </summary>
        public override float K
        {
            get { return this.mK; }
        }

        /// <summary>
        /// Gets the initial temperature of the mass, which equals 
        /// <code>Sqrt(Pow(<see cref="IdealEdgeLength"/>, 2) * nodeCount)
        /// </code>.
        /// </summary>
        public override float InitialTemperature
        {
            get 
            { 
                return (float)Math.Sqrt(this.mK * this.mK * 
                    this.mGraph.NodeCount); 
            }
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
                    this.UpdateParameters();
                }
            }
        }
    }
}
