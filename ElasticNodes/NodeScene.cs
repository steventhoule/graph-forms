using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using GraphForms;

namespace ElasticNodes
{
    public class NodeScene : GraphScene
    {
        private Random mRand = new Random();
        private Timer mTimer = new Timer();

        public NodeScene()
        {
            this.mTimer.Interval = 1000 / 25;
            this.mTimer.Tick += new EventHandler(timerTick);
        }

        public void ItemMoved()
        {
            if (!this.mTimer.Enabled)
                this.mTimer.Start();
        }

        public void Shuffle()
        {
            foreach (GraphElement item in this.Children)
            {
                if (item is Node)
                    item.Position = new PointF(-150 + mRand.Next(300),
                                               -150 + mRand.Next(300));
            }
        }

        private void timerTick(object sender, EventArgs e)
        {
            List<Node> nodes = new List<Node>(9);
            foreach (GraphElement item in this.Children)
            {
                Node node = item as Node;
                if (node != null)
                    nodes.Add(node);
            }

            foreach (Node node in nodes)
                node.CalculateForces();

            bool itemsMoved = false;
            foreach (Node node in nodes)
            {
                if (node.Advance())
                    itemsMoved = true;
            }

            if (!itemsMoved)
            {
                this.mTimer.Stop();
            }
        }

        protected override void OnMouseUp(GraphMouseEventArgs e)
        {
            foreach (GraphElement item in this.Children)
            {
                Node node = item as Node;
                if (node != null)
                    node.MouseGrabbed = false;
            }
            e.Handled = true;
        }

        protected override void OnDrawBackground(PaintEventArgs e)
        {
            // Shadow
            RectangleF sceneRect = this.BoundingBox;
            RectangleF rightShadow = new RectangleF(sceneRect.Right, sceneRect.Top + 5, 5, sceneRect.Height);
            RectangleF bottomShadow = new RectangleF(sceneRect.Left + 5, sceneRect.Bottom, sceneRect.Width, 5);
            if (rightShadow.IntersectsWith(e.ClipRectangle) || rightShadow.Contains(e.ClipRectangle))
                e.Graphics.FillRectangle(Brushes.DarkGray, rightShadow);
            if (bottomShadow.IntersectsWith(e.ClipRectangle) || bottomShadow.Contains(e.ClipRectangle))
                e.Graphics.FillRectangle(Brushes.DarkGray, bottomShadow);

            // Fill
            LinearGradientBrush gradient = new LinearGradientBrush(sceneRect,
                Color.White, Color.LightGray, LinearGradientMode.Horizontal);
            e.Graphics.FillRectangle(gradient, sceneRect);

            // Text
            RectangleF textRect = new RectangleF(sceneRect.Left + 4, sceneRect.Top + 4,
                                                 sceneRect.Width - 4, sceneRect.Height - 4);
            string message = "Click and drag the nodes around";//, and zoom with the mouse " +
                             //"wheel or the '+' and '-' keys";

            Font font = new Font(FontFamily.GenericSansSerif, 14, FontStyle.Bold);
            textRect.Offset(2, 2);
            e.Graphics.DrawString(message, font, Brushes.LightGray, textRect);
            textRect.Offset(-2, -2);
            e.Graphics.DrawString(message, font, Brushes.Black, textRect);

        }
    }
}
