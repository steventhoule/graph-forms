using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Layout.Circular
{
    public class CircularLayoutAlgorithm<Node, Edge>
        : LayoutAlgorithm<Node, Edge, CircularLayoutParameters>
        where Node : GraphElement, ILayoutNode
        where Edge : class, IGraphEdge<Node>, IUpdateable
    {
        public CircularLayoutAlgorithm(DirectionalGraph<Node, Edge> graph)
            : base(graph, null)
        {
        }

        public CircularLayoutAlgorithm(DirectionalGraph<Node, Edge> graph,
            CircularLayoutParameters oldParameters)
            : base(graph, oldParameters)
        {
        }

        protected override void InternalCompute()
        {
            System.Drawing.RectangleF bbox;
            Node[] nodes = this.mGraph.Nodes;
            int i;
            double perimeter, radius, angle, a;
            double[] halfSize = new double[nodes.Length];

            // calculate the size of the circle
            perimeter = 0;
            for (i = 0; i < nodes.Length; i++)
            {
                bbox = nodes[i].BoundingBox;
                halfSize[i] = Math.Sqrt(bbox.Width * bbox.Width + bbox.Height * bbox.Height) * 0.5;
                perimeter += halfSize[i] * 2;
            }

            radius = perimeter / (2 * Math.PI);

            float[] newXs = this.NewXPositions;
            float[] newYs = this.NewYPositions;

            // precalculation
            angle = 0;
            for (i = 0; i < nodes.Length; i++)
            {
                a = Math.Sin(halfSize[i] * 0.5 / radius) * 2;
                angle += a;

                //nodes[i].NewX = (float)(Math.Cos(angle) * radius + radius);
                //nodes[i].NewY = (float)(Math.Sin(angle) * radius + radius);
                newXs[i] = (float)(Math.Cos(angle) * radius + radius);
                newYs[i] = (float)(Math.Sin(angle) * radius + radius);

                angle += a;
            }

            base.EndIteration(0, 0.5, "Precalculation done.");

            // recalculate radius
            radius = angle / (2 * Math.PI) * radius;

            // calculation
            angle = 0;
            for (i = 0; i < nodes.Length; i++)
            {
                a = Math.Sin(halfSize[i] * 0.5 / radius) * 2;
                angle += a;

                //nodes[i].NewX = (float)(Math.Cos(angle) * radius + radius);
                //nodes[i].NewY = (float)(Math.Sin(angle) * radius + radius);
                newXs[i] = (float)(Math.Cos(angle) * radius + radius);
                newYs[i] = (float)(Math.Sin(angle) * radius + radius);

                angle += a;
            }

            base.EndIteration(1, 1, "Calculation done.");
        }
    }
}
