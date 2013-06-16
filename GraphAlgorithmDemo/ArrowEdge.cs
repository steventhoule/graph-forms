using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using GraphForms;
using GraphForms.Algorithms;

namespace GraphAlgorithmDemo
{
    public class ArrowEdge : GraphElement, IGraphEdge<CircleNode>, IUpdateable
    {
        private static float sArrowSize = 5;
        private static Pen sLinePen;

        static ArrowEdge()
        {
            GraphicsPath ahPath = new GraphicsPath();
            float sin = (float)(sArrowSize * Math.Sin(Math.PI / 3.0));
            float cos = sArrowSize * 0.5f;
            ahPath.AddPolygon(new PointF[] 
            { 
                new PointF(0, 0), 
                new PointF(cos, -sin), 
                new PointF(-cos, -sin) 
            });
            CustomLineCap arrowHead = new CustomLineCap(ahPath, null);

            sLinePen = new Pen(Color.Black, 1f);
            sLinePen.StartCap = LineCap.RoundAnchor;
            sLinePen.CustomEndCap = arrowHead;
            sLinePen.LineJoin = LineJoin.Round;
        }

        private Color mLineColor = Color.Black;

        public Color LineColor
        {
            get { return this.mLineColor; }
            set
            {
                if (this.mLineColor != value)
                {
                    this.mLineColor = value;
                    this.Invalidate();
                }
            }
        }

        private DashStyle mLineDashStyle = DashStyle.Solid;

        public DashStyle LineDashStyle
        {
            get { return this.mLineDashStyle; }
            set
            {
                if (this.mLineDashStyle != value)
                {
                    this.mLineDashStyle = value;
                    this.Invalidate();
                }
            }
        }

        private CircleNode mSrcNode;
        private CircleNode mDstNode;
        private CircleNodeScene mScene;
        private float mWeight;

        private double mAngle;
        private PointF mSrcPoint;
        private PointF mDstPoint;

        private string mAName;

        public ArrowEdge(CircleNode srcNode, CircleNode dstNode,
            CircleNodeScene scene)
            : this(srcNode, dstNode, scene, 1, 30, null)
        {
        }

        public ArrowEdge(CircleNode srcNode, CircleNode dstNode,
            CircleNodeScene scene, float weight)
            : this(srcNode, dstNode, scene, weight, 30, null)
        {
        }

        public ArrowEdge(CircleNode srcNode, CircleNode dstNode,
            CircleNodeScene scene, float weight, string aName)
            : this(srcNode, dstNode, scene, weight, 30, aName)
        {
        }

        public ArrowEdge(CircleNode srcNode, CircleNode dstNode,
            CircleNodeScene scene, float weight, double angle)
            : this(srcNode, dstNode, scene, weight, angle, null)
        {
        }

        public ArrowEdge(CircleNode srcNode, CircleNode dstNode, 
            CircleNodeScene scene, float weight, double angle, string aName)
        {
            this.mSrcNode = srcNode;
            this.mDstNode = dstNode;
            this.mWeight = weight;
            this.mAngle = angle * Math.PI / 180.0;
            this.mAName = aName;

            this.mScene = scene;
            this.mScene.AddItem(this);
            this.mScene.Graph.AddEdge(this, false);
            this.Update();
        }

        public CircleNode SrcNode
        {
            get { return this.mSrcNode; }
        }

        public CircleNode DstNode
        {
            get { return this.mDstNode; }
        }

        public float Weight
        {
            get { return this.mWeight; }
        }

        public double Angle
        {
            get { return this.mAngle * 180.0 / Math.PI; }
            set
            {
                double newAngle = value * Math.PI / 180.0;
                if (this.mAngle != newAngle)
                {
                    this.mAngle = newAngle;
                    this.Update();
                }
            }
        }

        public string AName
        {
            get 
            {
                if (string.IsNullOrEmpty(this.mAName))
                    return string.Concat(this.mSrcNode.AName, "->", 
                                         this.mDstNode.AName);
                else
                    return this.mAName;
            }
            set { this.mAName = value; }
        }

        public void SetSrcNode(CircleNode srcNode)
        {
            this.mSrcNode = srcNode;
            this.Update();
        }

        public void SetDstNode(CircleNode dstNode)
        {
            this.mDstNode = dstNode;
            this.Update();
        }

        public void Update()
        {
            SizeF srcPt = this.mSrcNode.ItemTranslate(this);
            SizeF dstPt = this.mDstNode.ItemTranslate(this);

            double dx = dstPt.Width - srcPt.Width;
            double dy = dstPt.Height - srcPt.Height;

            double length = Math.Sqrt(dx * dx + dy * dy);
            dx = dx / length;
            dy = dy / length;

            double cos = Math.Cos(this.mAngle);
            double sin = Math.Sin(this.mAngle);

            double rad = this.mSrcNode.Radius;
            mSrcPoint.X = srcPt.Width  + (float)(rad * (cos * dx - sin * dy));
            mSrcPoint.Y = srcPt.Height + (float)(rad * (cos * dy + sin * dx));

            rad = this.mDstNode.Radius;
            mDstPoint.X = dstPt.Width  - (float)(rad * (cos * dx + sin * dy));
            mDstPoint.Y = dstPt.Height - (float)(rad * (cos * dy - sin * dx));

            this.BoundingBox = this.CalcBBox();
        }

        private RectangleF CalcBBox()
        {
            float extra = 1f + 2f * sArrowSize;

            float x = Math.Min(mSrcPoint.X, mDstPoint.X) - extra / 2f;
            float y = Math.Min(mSrcPoint.Y, mDstPoint.Y) - extra / 2f;
            float w = Math.Abs(mDstPoint.X - mSrcPoint.X) + extra;
            float h = Math.Abs(mDstPoint.Y - mSrcPoint.Y) + extra;

            return new RectangleF(x, y, w, h);
        }

        protected override void OnDrawBackground(System.Windows.Forms.PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            sLinePen.Color = this.mLineColor;
            sLinePen.DashStyle = this.mLineDashStyle;
            try
            {
                e.Graphics.DrawLine(sLinePen, this.mSrcPoint, this.mDstPoint);
            }
            catch (OverflowException)
            {
            }
        }
    }
}
