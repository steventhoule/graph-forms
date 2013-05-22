using System;
using System.Collections.Generic;
using System.Text;
using GraphForms.Algorithms.Layout.ForceDirected;

namespace GraphForms.Algorithms.Layout.Circular
{
    public class FDSingleCircleLayoutAlgorithm<Node, Edge>
        : ForceDirectedLayoutAlgorithm<Node, Edge, FDSingleCircleLayoutParameters>
        where Node : GraphElement, ILayoutNode
        where Edge : class, IGraphEdge<Node>, IUpdateable
    {
        private double mMinRadius;
        private double mFreeArc;

        private double mRadius;
        private double[] mAngles;

        private bool bCalcCenter;
        private double mCenterX;
        private double mCenterY;
        private double mSpringMult;
        private double mMagnetMult;
        private double mAngleExp;

        public FDSingleCircleLayoutAlgorithm(DirectionalGraph<Node, Edge> graph)
            : base(graph, null)
        {
        }

        public FDSingleCircleLayoutAlgorithm(DirectionalGraph<Node, Edge> graph,
            FDSingleCircleLayoutParameters oldParameters)
            : base(graph, oldParameters)
        {
        }

        private void InitCircle()
        {
            System.Drawing.RectangleF bbox;
            DirectionalGraph<Node, Edge>.GraphNode[] nodes 
                = this.mGraph.InternalNodes;
            int i;
            double perimeter, angle, a;
            double[] halfSize = new double[nodes.Length];

            // calculate the size of the circle
            perimeter = 0;
            for (i = 0; i < nodes.Length; i++)
            {
                bbox = nodes[i].mData.BoundingBox;
                a = Math.Sqrt(bbox.Width * bbox.Width + bbox.Height * bbox.Height);
                perimeter += a;
                halfSize[i] = a / 4;
            }
            perimeter += nodes.Length * this.mFreeArc;

            this.mRadius = perimeter / (2 * Math.PI);
            this.mAngles = new double[nodes.Length];

            // precalculation
            angle = 0;
            for (i = 0; i < nodes.Length; i++)
            {
                angle += Math.Sin(halfSize[i] / this.mRadius) * 4;
                //a = Math.Sin(halfSize[i] / this.mRadius) * 2;
                //angle += a;
                //this.mAngles[i] = angle;
                //angle += a;
            }

            //base.EndIteration(0, 0.5, "Precalculation done.");

            // recalculate radius
            this.mRadius = Math.Max(angle / (2 * Math.PI) * this.mRadius, this.mMinRadius);

            // calculation
            angle = -Math.PI;
            for (i = 0; i < nodes.Length; i++)
            {
                a = Math.Sin(halfSize[i] / this.mRadius) * 2;
                angle += a;
                this.mAngles[i] = angle;
                angle += a;
            }

            //base.EndIteration(1, 1, "Calculation done.");
        }

        protected override bool OnBeginIteration(bool paramsDirty, int lastNodeCount, int lastEdgeCount)
        {
            bool recalc = false;
            if (paramsDirty)
            {
                FDSingleCircleLayoutParameters param = this.Parameters;
                if (this.mMinRadius != param.MinRadius)
                {
                    this.mMinRadius = param.MinRadius;
                    recalc = true;
                }
                if (this.mFreeArc != param.FreeArc)
                {
                    this.mFreeArc = param.FreeArc;
                    recalc = true;
                }
                this.mCenterX = param.X + param.Width / 2;
                this.mCenterY = param.Y + param.Height / 2;
                this.mSpringMult = param.SpringMultiplier;
                this.mMagnetMult = param.MagneticMultiplier;
                this.mAngleExp = param.AngleExponent;
                this.bCalcCenter = !param.CenterInBounds;
            }
            if (recalc || lastNodeCount != this.mGraph.NodeCount)
            {
                this.InitCircle();
            }
            return base.OnBeginIteration(paramsDirty, lastNodeCount, lastEdgeCount);
        }

        protected override void PerformIteration(int iteration, int maxIterations)
        {
            System.Drawing.SizeF pos;
            double dx, dy, r, force, fx, fy;
            Node[] nodes = this.mGraph.Nodes;
            int i;
            // Compute Center
            if (this.bCalcCenter)
            {
                this.mCenterX = 0;
                this.mCenterY = 0;
                for (i = 0; i < nodes.Length; i++)
                {
                    pos = nodes[i].SceneTranslate();
                    this.mCenterX += pos.Width;
                    this.mCenterY += pos.Height;
                }
                this.mCenterX /= nodes.Length;
                this.mCenterY /= nodes.Length;
            }
            // Compute new positions 
            float[] newXs = this.NewXPositions;
            float[] newYs = this.NewYPositions;
            for (i = 0; i < nodes.Length; i++)
            {
                if (nodes[i].PositionFixed)
                {
                    newXs[i] = nodes[i].X;
                    newYs[i] = nodes[i].Y;
                }
                else
                {
                    // TODO: make sure all the signs (+/-) are right
                    pos = nodes[i].SceneTranslate();
                    dx = this.mCenterX - pos.Width;
                    dy = this.mCenterY - pos.Height;
                    r = Math.Max(dx * dx + dy * dy, 0.000001);
                    // Magnetic Torque
                    force = Math.Atan2(dy, dx) - this.mAngles[i];
                    force = this.mMagnetMult * Math.Pow(force, this.mAngleExp) / r;
                    fx = force * -dy;
                    fy = force * dx;
                    // Spring Force
                    r = Math.Sqrt(r);
                    force = this.mSpringMult * Math.Log(r / this.mRadius);
                    fx += force * dx / r;
                    fy += force * dy / r;
                    // Add force to position
                    newXs[i] = (float)(nodes[i].X + fx);
                    newYs[i] = (float)(nodes[i].Y + fy);
                }
            }
        }
    }
}
