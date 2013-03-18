using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using GraphForms;

namespace ElasticNodes
{
    public class Node : GraphElement
    {
        private List<Edge> edgeList = new List<Edge>();
        private PointF newPos;
        private NodeScene scene;

        private PathGradientBrush nodeBrushRisen;
        private PathGradientBrush nodeBrushSunken;

        public Node(NodeScene scene)
        {
            GraphicsPath gp = new GraphicsPath();
            gp.AddEllipse(-10, -10, 20, 20);

            nodeBrushSunken = new PathGradientBrush(gp);
            nodeBrushSunken.CenterPoint = new PointF(3, 3);
            //nodeBrushSunken.CenterColor = Color.DarkGoldenrod;
            //nodeBrushSunken.SurroundColors = new Color[] { Color.Goldenrod };
            nodeBrushSunken.CenterColor = Color.Yellow;
            nodeBrushSunken.SurroundColors = new Color[] { Color.LightYellow };

            nodeBrushRisen = new PathGradientBrush(gp);
            nodeBrushRisen.CenterPoint = new PointF(-3, -3);
            nodeBrushRisen.CenterColor = Color.Yellow;
            nodeBrushRisen.SurroundColors = new Color[] { Color.FromArgb(0x80, 0x80, 0) };

            const int adjust = 2;
            this.BoundingBox = new Rectangle(-10 - adjust, -10 - adjust, 25 + adjust, 25 + adjust);

            this.scene = scene;
            this.Zvalue = -1;
        }

        public override Region Shape()
        {
            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(-10, -10, 20, 20);
            return new Region(path);
        }

        public void AddEdge(Edge edge)
        {
            this.edgeList.Add(edge);
            //edge.Adjust(); // Redundant?
        }

        public List<Edge> Edges
        {
            get { return this.edgeList; }
        }

        public void CalculateForces()
        {
            if (this.Parent == null || this.mouseGrabbed)
            {
                this.newPos = this.Position;
                return;
            }

            // Sum up all forces pushing this item away
            float xvel = 0f;
            float yvel = 0f;
            PointF vec;
            foreach (GraphElement item in this.scene.Children)
            {
                Node node = item as Node;
                if (node == null)
                    continue;

                vec = MapToItem(node, new PointF(0, 0));
                float dx = vec.X;
                float dy = vec.Y;
                float l = 2f * (dx * dx + dy * dy);
                if (l > 0)
                {
                    xvel += (dx * 150f) / l;
                    yvel += (dy * 150f) / l;
                }
            }

            // Now subtract all forces pulling items together
            float weight = (this.edgeList.Count + 1) * 10;
            foreach (Edge edge in this.edgeList)
            {
                if (edge.SourceNode == this)
                    vec = MapToItem(edge.DestNode, new PointF(0, 0));
                else
                    vec = MapToItem(edge.SourceNode, new PointF(0, 0));
                xvel -= vec.X / weight;
                yvel -= vec.Y / weight;
            }

            if (Math.Abs(xvel) < 0.1 && Math.Abs(yvel) < 0.1)
                xvel = yvel = 0;

            RectangleF sceneRect = this.scene.BoundingBox;
            this.newPos = new PointF(this.Position.X + xvel, this.Position.Y + yvel);
            this.newPos.X = Math.Min(Math.Max(newPos.X, sceneRect.Left + 10), sceneRect.Right - 10);
            this.newPos.Y = Math.Min(Math.Max(newPos.Y, sceneRect.Top + 10), sceneRect.Bottom - 10);
        }

        public bool Advance()
        {
            if (this.newPos == this.Position)
                return false;

            this.Position = this.newPos;
            return true;
        }

        protected override void OnDrawBackground(System.Windows.Forms.PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            e.Graphics.FillEllipse(Brushes.DarkGray, -7, -7, 20, 20);

            e.Graphics.FillEllipse(mouseGrabbed ? nodeBrushSunken : nodeBrushRisen, -10, -10, 20, 20);
            e.Graphics.DrawEllipse(Pens.Black, -10, -10, 20, 20);
        }

        protected override void OnPositionChanged()
        {
            foreach (Edge edge in this.edgeList)
                edge.Adjust();
            this.scene.ItemMoved();

            base.OnPositionChanged();
        }

        private float lastMouseX, lastMouseY;
        private bool mouseGrabbed = false;

        public bool MouseGrabbed
        {
            get { return this.mouseGrabbed; }
            set { this.mouseGrabbed = value; }
        }

        protected override void OnMouseDown(GraphMouseEventArgs e)
        {
            if (!this.mouseGrabbed)
            {
                this.mouseGrabbed = true;
                this.lastMouseX = e.SceneX;
                this.lastMouseY = e.SceneY;
            }
            e.Handled = true;
        }

        protected override void OnMouseMove(GraphMouseEventArgs e)
        {
            if (this.mouseGrabbed)
            {
                PointF pos = this.Position;
                PointF sp = this.MapToParent(this.MapFromScene(e.ScenePos));
                this.Position = new PointF(pos.X + sp.X - lastMouseX, pos.Y + sp.Y - lastMouseY);
                this.lastMouseX = sp.X;
                this.lastMouseY = sp.Y;
            }
            e.Handled = true;
        }

        protected override void OnMouseUp(GraphMouseEventArgs e)
        {
            if (this.mouseGrabbed)
            {
                this.mouseGrabbed = false;
                e.Handled = true;
            }
        }
    }
}
