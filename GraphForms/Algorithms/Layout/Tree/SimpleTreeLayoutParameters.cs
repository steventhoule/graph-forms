using System;
using System.Collections.Generic;
using System.Text;
using GraphForms.Algorithms.Search;

namespace GraphForms.Algorithms.Layout.Tree
{
    public class SimpleTreeLayoutParameters : LayoutParameters
    {
        private double mVertexGap = 10;
        /// <summary>
        /// Gets or sets the gap between the vertices.
        /// </summary>
        public double VertexGap
        {
            get { return this.mVertexGap; }
            set
            {
                if (this.mVertexGap != value)
                {
                    this.mVertexGap = value;
                    this.OnPropertyChanged("VertexGap");
                }
            }
        }

        private double mLayerGap = 10;
        /// <summary>
        /// Gets or sets the gap between the layers.
        /// </summary>
        public double LayerGap
        {
            get { return this.mLayerGap; }
            set
            {
                if (this.mLayerGap != value)
                {
                    this.mLayerGap = value;
                    this.OnPropertyChanged("LayerGap");
                }
            }
        }

        private LayoutDirection mDirection = LayoutDirection.TopToBottom;
        /// <summary>
        /// Gets or sets the direction of the layout.
        /// </summary>
        public LayoutDirection Direction
        {
            get { return this.mDirection; }
            set
            {
                if (this.mDirection != value)
                {
                    this.mDirection = value;
                    this.OnPropertyChanged("Direction");
                }
            }
        }

        private SearchMethod mSpanningTreeGeneration = SearchMethod.DFS;
        /// <summary>
        /// Gets or sets the search pattern the algorithm uses to build its
        /// internal sparsely connected spanning tree for traversing its graph.
        /// </summary>
        public SearchMethod SpanningTreeGeneration
        {
            get { return this.mSpanningTreeGeneration; }
            set
            {
                if (this.mSpanningTreeGeneration != value)
                {
                    this.mSpanningTreeGeneration = value;
                    this.OnPropertyChanged("SpanningTreeGeneration");
                }
            }
        }

        public SimpleTreeLayoutParameters()
            : base(0, 0, 300, 300)
        {
        }
    }
}
