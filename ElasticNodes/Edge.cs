using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using GraphForms;

namespace ElasticNodes
{
    public class Edge : GraphElement
    {
        private static Pen linePen;

        static Edge()
        {
            linePen = new Pen(Color.Black, 1f);
            linePen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
            linePen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
            linePen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
            
        }

        private Node mSource, mDest;

        private PointF mSourcePoint;
        private PointF mDestPoint;
        private float mArrowSize;

        public Edge(Node sourceNode, Node destNode)
        {
            this.mArrowSize = 10f;
            this.mSource = sourceNode;
            this.mDest = destNode;
            this.mSource.AddEdge(this);
            this.mDest.AddEdge(this);
            this.Adjust();
        }

        public Node SourceNode
        {
            get { return this.mSource; }
        }

        public Node DestNode
        {
            get { return this.mDest; }
        }

        public virtual void Adjust()
        {
            if (this.mSource == null || this.mDest == null)
                return;

            PointF sourcePt = MapFromItem(mSource, new PointF(0, 0));
            PointF destPt = MapFromItem(mDest, new PointF(0, 0));

            float length = (float)Math.Sqrt(
                (destPt.X - sourcePt.X) * (destPt.X - sourcePt.X) + 
                (destPt.Y - sourcePt.Y) * (destPt.Y - sourcePt.Y));

            if (length > 20f)
            {
                float edgeOffsetX = (destPt.X - sourcePt.X) * 10f / length;
                float edgeOffsetY = (destPt.Y - sourcePt.Y) * 10f / length;
                this.mSourcePoint = new PointF(sourcePt.X + edgeOffsetX, sourcePt.Y + edgeOffsetY);
                this.mDestPoint = new PointF(destPt.X - edgeOffsetX, destPt.Y - edgeOffsetY);
            }
            else
            {
                this.mSourcePoint = this.mDestPoint = sourcePt;
            }

            this.BoundingBox = this.CalcBBox();
        }

        private RectangleF CalcBBox()
        {
            if (this.mSource == null || this.mDest == null)
                return new Rectangle();

            float extra = (1f + this.mArrowSize);

            float x = Math.Min(mSourcePoint.X, mDestPoint.X) - extra / 2f;
            float y = Math.Min(mSourcePoint.Y, mDestPoint.Y) - extra / 2f;
            float w = Math.Abs(mDestPoint.X - mSourcePoint.X) + extra;
            float h = Math.Abs(mDestPoint.Y - mSourcePoint.Y) + extra;

            return new RectangleF(x, y, w, h);
        }

        protected override void OnDrawBackground(System.Windows.Forms.PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Draw the line itself
            e.Graphics.DrawLine(linePen, this.mSourcePoint, this.mDestPoint);

            // Draw the arrows
            double length = Math.Sqrt(
                (mDestPoint.X - mSourcePoint.X) * (mDestPoint.X - mSourcePoint.X) +
                (mDestPoint.Y - mSourcePoint.Y) * (mDestPoint.Y - mSourcePoint.Y));
            double angle = Math.Acos((mDestPoint.X - mSourcePoint.X) / length);
            if ((mDestPoint.Y - mSourcePoint.Y) >= 0)
                angle = 2 * Math.PI - angle;

            PointF sourceArrowP1 = new PointF((float)(mSourcePoint.X + Math.Sin(angle + Math.PI / 3) * mArrowSize),
                                              (float)(mSourcePoint.Y + Math.Cos(angle + Math.PI / 3) * mArrowSize));
            PointF sourceArrowP2 = new PointF((float)(mSourcePoint.X + Math.Sin(angle + Math.PI - Math.PI / 3) * mArrowSize),
                                              (float)(mSourcePoint.Y + Math.Cos(angle + Math.PI - Math.PI / 3) * mArrowSize));
            PointF destArrowP1 = new PointF((float)(mDestPoint.X + Math.Sin(angle - Math.PI / 3) * mArrowSize),
                                            (float)(mDestPoint.Y + Math.Cos(angle - Math.PI / 3) * mArrowSize));
            PointF destArrowP2 = new PointF((float)(mDestPoint.X + Math.Sin(angle - Math.PI + Math.PI / 3) * mArrowSize),
                                            (float)(mDestPoint.Y + Math.Cos(angle - Math.PI + Math.PI / 3) * mArrowSize));
            e.Graphics.FillPolygon(Brushes.Black, new PointF[] { mSourcePoint, sourceArrowP1, sourceArrowP2 });
            e.Graphics.FillPolygon(Brushes.Black, new PointF[] { mDestPoint, destArrowP1, destArrowP2 });
        }
    }
}
