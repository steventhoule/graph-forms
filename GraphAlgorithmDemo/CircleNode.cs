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
        private Digraph<CircleNode, ArrowEdge>.GNode mGraphNode;

        private float mRadius;
        private string mAName;

        public CircleNode(CircleNodeScene scene)
            : this(scene, 15, null)
        {
        }

        public CircleNode(CircleNodeScene scene, string aName)
            : this(scene, 15, aName)
        {
        }

        public CircleNode(CircleNodeScene scene, float radius, string aName)
        {
            this.mScene = scene;

            Digraph<CircleNode, ArrowEdge> graph = scene.Graph;
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

            this.mAName = aName;

            this.InitializeTextStuff();
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

        public string AName
        {
            get { return this.mAName; }
            set { this.mAName = value; }
        }

        private SolidBrush mMarkerBrush = new SolidBrush(Color.Transparent);

        public Color MarkerColor
        {
            get { return this.mMarkerBrush.Color; }
            set
            {
                if (this.mMarkerBrush.Color != value)
                {
                    this.mMarkerBrush.Color = value;
                    this.Invalidate();
                }
            }
        }

        private Pen mBorderPen = new Pen(Color.Black, 2f);

        public Color BorderColor
        {
            get { return this.mBorderPen.Color; }
            set
            {
                if (this.mBorderPen.Color != value)
                {
                    this.mBorderPen.Color = value;
                    this.Invalidate();
                }
            }
        }

        private SolidBrush mTextBrush;
        private Font mTextFont;
        private StringFormat mTextFormat;
        private string mTextString;

        private void InitializeTextStuff()
        {
            this.mTextBrush = new SolidBrush(Color.Black);
            this.mTextFont = new Font(FontFamily.GenericSansSerif, 5f);
            this.mTextFormat = new StringFormat();
            this.mTextFormat.Alignment = StringAlignment.Center;
            this.mTextString = null;
        }

        public string TextString
        {
            get { return this.mTextString; }
            set
            {
                if (this.mTextString != value)
                {
                    this.mTextString = value;
                    this.Invalidate();
                }
            }
        }

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

        private static readonly float sInsetPos
            = (float)(Math.Sqrt(2.0) / -2.0);
        private static readonly float sInsetDim
            = (float)(Math.Sqrt(2.0));

        protected override void OnDrawBackground(
            System.Windows.Forms.PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.SmoothingMode = SmoothingMode.AntiAlias;

            g.FillEllipse(Brushes.DarkGray, -mRadius + 3, -mRadius + 3, 
                2 * mRadius, 2 * mRadius);

            g.FillEllipse(
                this.bMouseGrabbed ? sNodeBrushSunken : sNodeBrushRisen, 
                -mRadius, -mRadius, 2 * mRadius, 2 * mRadius);
            g.DrawEllipse(this.mBorderPen, -mRadius, -mRadius, 
                2 * mRadius, 2 * mRadius);

            g.FillEllipse(this.mMarkerBrush,
                -mRadius / 2, -mRadius / 2, mRadius, mRadius);

            string text = this.mTextString ?? this.mAName;
            if (!string.IsNullOrEmpty(text))
            {
                RectangleF inset = new RectangleF(
                    mRadius * sInsetPos, mRadius * sInsetPos,
                    mRadius * sInsetDim, mRadius * sInsetDim);
                try
                {
                    g.DrawString(text, this.mTextFont, this.mTextBrush,
                        inset, this.mTextFormat);
                }
                catch (OverflowException)
                {
                }
            }
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
            this.mScene.OnNodeMouseUp(this);
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
        }

        public float NewY
        {
            get { return this.mNewY; }
        }

        public void SetNewPosition(float newX, float newY)
        {
            this.mNewX = newX;
            this.mNewY = newY;
        }
        #endregion

        protected override void OnPositionChanged()
        {
            if (this.bMouseGrabbed && this.mScene != null)
            {
                int i;
                Digraph<CircleNode, ArrowEdge>.GEdge[] edges
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
                this.mScene.OnNodeMovedByMouse(this);
            }
            // Layout algorithm handles edge updating otherwise
            base.OnPositionChanged();
        }
    }
}
