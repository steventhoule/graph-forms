using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GraphForms
{
    public class GraphMouseEventArgs : GraphEventArgs
    {
        private MouseButtons mButton;
        private int mClicks;
        private int mSceneX;
        private int mSceneY;
        private float mX;
        private float mY;
        private int mDelta;

        public GraphMouseEventArgs(MouseButtons button, int clicks, int sceneX, int sceneY, float x, float y, int delta)
        {
            this.mButton = button;
            this.mClicks = clicks;
            this.mSceneX = sceneX;
            this.mSceneY = sceneY;
            this.mX = x;
            this.mY = y;
            this.mDelta = delta;
        }

        public GraphMouseEventArgs(MouseEventArgs e)
        {
            this.mButton = e.Button;
            this.mClicks = e.Clicks;
            this.mX = this.mSceneX = e.X;
            this.mY = this.mSceneY = e.Y;
            this.mDelta = e.Delta;
        }

        public GraphMouseEventArgs(MouseEventArgs e, float dx, float dy)
        {
            this.mButton = e.Button;
            this.mClicks = e.Clicks;
            this.mSceneX = e.X;
            this.mSceneY = e.Y;
            this.mX = e.X + dx;
            this.mY = e.Y + dy;
            this.mDelta = e.Delta;
        }

        public GraphMouseEventArgs(GraphMouseEventArgs e) : this(e, 0, 0) { }

        public GraphMouseEventArgs(GraphMouseEventArgs e, float dx, float dy)
        {
            this.Handled = e.Handled;
            this.mButton = e.mButton;
            this.mClicks = e.mClicks;
            this.mSceneX = e.mSceneX;
            this.mSceneY = e.mSceneY;
            this.mX = e.mX + dx;
            this.mY = e.mY + dy;
            this.mDelta = e.mDelta;
        }

        public MouseButtons Button
        {
            get { return this.mButton; }
        }

        public int Clicks
        {
            get { return this.mClicks; }
        }

        public int SceneX
        {
            get { return this.mSceneX; }
        }

        public int SceneY
        {
            get { return this.mSceneY; }
        }

        public Point ScenePos
        {
            get { return new Point(this.mSceneX, this.mSceneY); }
        }

        public float X
        {
            get { return this.mX; }
        }

        public float Y
        {
            get { return this.mY; }
        }

        public PointF Pos
        {
            get { return new PointF(this.mX, this.mY); }
        }

        public Point Location
        {
            get { return new Point((int)this.mX, (int)this.mY); }
        }

        public int Delta
        {
            get { return this.mDelta; }
        }
    }
}
