using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using GraphForms;
using GraphForms.Algorithms;
using GraphForms.Algorithms.Layout;

namespace GraphAlgorithmDemo
{
    public class CircleNode : GraphElement, ILayoutNode
    {
        private static Brush sNodeBrushRisen;
        private static Brush sNodeBrushSunken;

        static CircleNode()
        {
            sNodeBrushRisen = new SolidBrush(Color.DarkGoldenrod);
            sNodeBrushSunken = new SolidBrush(Color.Goldenrod);
        }

        private CircleNodeScene mScene;
        private DirectionalGraph<CircleNode, ArrowEdge>.GraphNode mGraphNode;

        private float mRadius;

        public CircleNode(CircleNodeScene scene)
            : this(scene, 15)
        {
        }

        public CircleNode(CircleNodeScene scene, float radius)
        {
            this.mScene = scene;

            DirectionalGraph<CircleNode, ArrowEdge> graph = scene.Graph;
            int index = graph.IndexOfNode(this);
            if (index < 0)
            {
                graph.AddNode(this);
                index = graph.IndexOfNode(this);
            }
            this.mGraphNode = graph.InternalNodeAt(index);

            this.Radius = radius;
            this.Zvalue = -1;

            this.mScene.AddItem(this);
        }

        public float Radius
        {
            get { return this.mRadius; }
            set
            {
                if (this.mRadius != value)
                {
                    this.mRadius = value;
                    float rad = value + 2;
                    this.BoundingBox = new RectangleF(-rad, -rad, 
                        2 * rad + 3, 2 * rad + 3);
                }
            }
        }

        /*public override bool Contains(PointF point)
        {
            float dx = point.X - this.X;
            float dy = point.Y - this.Y;
            return Math.Sqrt(dx * dx + dy * dy) <= this.mRadius;
        }/* */

        public override Region Shape()
        {
            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(-this.mRadius, -this.mRadius, 
                2 * this.mRadius, 2 * this.mRadius);
            return new Region(path);
        }

        public override bool Contains(PointF point)
        {
            return GraphHelpers.Length(point) <= this.mRadius;
        }

        protected override void OnDrawBackground(System.Windows.Forms.PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            e.Graphics.FillEllipse(Brushes.DarkGray, -mRadius + 3, -mRadius + 3, 
                2 * mRadius, 2 * mRadius);

            e.Graphics.FillEllipse(
                this.bMouseGrabbed ? sNodeBrushSunken : sNodeBrushRisen, 
                -mRadius, -mRadius, 2 * mRadius, 2 * mRadius);
            e.Graphics.DrawEllipse(Pens.Black, -mRadius, -mRadius, 
                2 * mRadius, 2 * mRadius);
        }

        private bool bMouseGrabbed = false;

        public bool MouseGrabbed
        {
            get { return this.bMouseGrabbed; }
            set 
            {
                if (this.bMouseGrabbed != value)
                {
                    this.bMouseGrabbed = value;
                    this.Invalidate();
                }
            }
        }

        protected override void OnMouseDown(GraphMouseEventArgs e)
        {
            if (!this.bMouseGrabbed)
            {
                this.bMouseGrabbed = true;
                this.Invalidate();
            }
            e.Handled = true;
        }

        protected override void OnMouseUp(GraphMouseEventArgs e)
        {
            if (this.bMouseGrabbed)
            {
                this.bMouseGrabbed = false;
                this.Invalidate();
                e.Handled = true;
            }
        }

        #region ILayoutNode Members

        public bool PositionFixed
        {
            get { return this.bMouseGrabbed; }
        }

        private float mNewX;
        private float mNewY;

        public float NewX
        {
            get { return this.mNewX; }
            set { this.mNewX = value; }
        }

        public float NewY
        {
            get { return this.mNewY; }
            set { this.mNewY = value; }
        }

        #endregion

        protected override void OnPositionChanged()
        {
            if (this.bMouseGrabbed)
            {
                int i;
                DirectionalGraph<CircleNode, ArrowEdge>.GraphEdge[] edges
                    = this.mGraphNode.InternalDstEdges;
                for (i = 0; i < edges.Length; i++)
                {
                    edges[i].Data.Update();
                }
                edges = this.mGraphNode.InternalSrcEdges;
                for (i = 0; i < edges.Length; i++)
                {
                    edges[i].Data.Update();
                }
                this.mScene.OnNodeMoved(this);
            }
            // Layout algorithm handles edge updating otherwise
            base.OnPositionChanged();
        }
    }
}
