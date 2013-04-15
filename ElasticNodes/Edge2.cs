using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using GraphForms;

namespace ElasticNodes
{
    public class Edge2 : Edge
    {
        private static float arrowSize = 5;

        private static Pen linePen;

        static Edge2()
        {
            GraphicsPath hPath = new GraphicsPath();

            // Create the outline for the custom end cap.
            float sin = arrowSize * (float)Math.Sin(Math.PI / 3);
            float cos = arrowSize * 0.5f;
            hPath.AddPolygon(new PointF[] { new PointF(0, 0), new PointF(cos, -sin), new PointF(-cos, -sin) });

            // Construct the hook-shaped end cap.
            CustomLineCap HookCap = new CustomLineCap(hPath, null);

            // Set the start cap and end cap of the HookCap to be rounded.
            HookCap.SetStrokeCaps(LineCap.Round, LineCap.Round);

            linePen = new Pen(Color.Black, 1f);
            linePen.StartCap = LineCap.RoundAnchor;
            linePen.CustomEndCap = HookCap;
            linePen.LineJoin = LineJoin.Round;
        }

        private double mAngle = 20 * Math.PI / 180;

        private PointF mSrcPoint1 = new PointF(0, 0);
        private PointF mDstPoint1 = new PointF(0, 0);

        private PointF mSrcPoint2 = new PointF(0, 0);
        private PointF mDstPoint2 = new PointF(0, 0);

        public Edge2(Node sourceNode, Node destNode)
            : base(sourceNode, destNode)
        {
        }

        public override void Adjust()
        {
            SizeF srcPoint = this.SourceNode.ItemTranslate(this);
            SizeF dstPoint = this.DestNode.ItemTranslate(this);

            double dx = dstPoint.Width - srcPoint.Width;
            double dy = dstPoint.Height - srcPoint.Height;

            double length = Math.Sqrt(dx * dx + dy * dy);
            dx = dx / length;
            dy = dy / length;

            double cos = Math.Cos(this.mAngle);
            double sin = Math.Sin(this.mAngle);

            double rad = 10;
            this.mSrcPoint1.X = srcPoint.Width  + (float)(rad * (cos * dx - sin * dy));
            this.mSrcPoint1.Y = srcPoint.Height + (float)(rad * (cos * dy + sin * dx));
            this.mSrcPoint2.X = dstPoint.Width  - (float)(rad * (cos * dx - sin * dy));
            this.mSrcPoint2.Y = dstPoint.Height - (float)(rad * (cos * dy + sin * dx));

            rad = 10;
            this.mDstPoint1.X = dstPoint.Width  - (float)(rad * (cos * dx + sin * dy));
            this.mDstPoint1.Y = dstPoint.Height - (float)(rad * (cos * dy - sin * dx));
            this.mDstPoint2.X = srcPoint.Width  + (float)(rad * (cos * dx + sin * dy));
            this.mDstPoint2.Y = srcPoint.Height + (float)(rad * (cos * dy - sin * dx));

            this.BoundingBox = RectangleF.Union(CalcBBox1(), CalcBBox2());
        }

        private RectangleF CalcBBox1()
        {
            float extra = 1f + 2f * arrowSize;

            float x = Math.Min(mSrcPoint1.X, mDstPoint1.X) - extra / 2f;
            float y = Math.Min(mSrcPoint1.Y, mDstPoint1.Y) - extra / 2f;
            float w = Math.Abs(mDstPoint1.X - mSrcPoint1.X) + extra;
            float h = Math.Abs(mDstPoint1.Y - mSrcPoint1.Y) + extra;

            return new RectangleF(x, y, w, h);
        }

        private RectangleF CalcBBox2()
        {
            float extra = 1f + 2f * arrowSize;

            float x = Math.Min(mSrcPoint2.X, mDstPoint2.X) - extra / 2f;
            float y = Math.Min(mSrcPoint2.Y, mDstPoint2.Y) - extra / 2f;
            float w = Math.Abs(mDstPoint2.X - mSrcPoint2.X) + extra;
            float h = Math.Abs(mDstPoint2.Y - mSrcPoint2.Y) + extra;

            return new RectangleF(x, y, w, h);
        }

        protected override void OnDrawBackground(System.Windows.Forms.PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Draw the lines themselves
            e.Graphics.DrawLine(linePen, this.mSrcPoint1, this.mDstPoint1);
            e.Graphics.DrawLine(linePen, this.mSrcPoint2, this.mDstPoint2);
        }
    }
}
