using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Layout.ForceDirected
{
    /// <summary>
    /// Bounded version of the Fruchterman-Reingold Force-Directed Algorithm,
    /// which calculates its ideal edge length and initial temperature based 
    /// on the size of its bounding box.
    /// </summary>
    public class FRBoundedLayoutAlgorithm<Node, Edge>
        : FRLayoutAlgorithm<Node, Edge>
        where Node : class, ILayoutNode
        where Edge : IGraphEdge<Node>, IUpdateable
    {
        private float mK;
        private float mInitTemp;

        public FRBoundedLayoutAlgorithm(Digraph<Node, Edge> graph,
            IClusterNode clusterNode)
            : base(graph, clusterNode)
        {
        }

        public FRBoundedLayoutAlgorithm(Digraph<Node, Edge> graph,
            Box2F boundingBox)
            : base(graph, boundingBox)
        {
        }

        /// <summary>
        /// Ideal edge length, which equals 
        /// <code>Sqrt(height * width / nodeCount)</code>.
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
        /// <code>Min(width, height) / 10</code>.
        /// </summary>
        public override float InitialTemperature
        {
            get { return this.mInitTemp; }
        }

        private void CalcParameters()
        {
            Box2F bbox = this.mClusterNode == null
                ? this.BoundingBox : this.mClusterNode.LayoutBBox;

            this.mK = (float)Math.Sqrt(bbox.W * bbox.H
                / this.mGraph.NodeCount);
            this.mInitTemp = Math.Min(bbox.W, bbox.H) / 10;
            this.UpdateParameters();
        }

        protected override void InitializeAlgorithm()
        {
            this.CalcParameters();
            base.InitializeAlgorithm();
        }

        protected override void OnBeginIteration(uint iteration, bool dirty, 
            int lastNodeCount, int lastEdgeCount)
        {
            this.CalcParameters();

            base.OnBeginIteration(iteration, dirty, 
                lastNodeCount, lastEdgeCount);
        }
    }
}
