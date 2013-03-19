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

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphMouseEventArgs"/>
        /// class. </summary>
        /// <param name="button">One of the 
        /// <see cref="System.Windows.Forms.MouseButtons"/> values indicating 
        /// which mouse button was pressed.</param>
        /// <param name="clicks">The number of times a mouse button was 
        /// pressed.</param>
        /// <param name="sceneX">The x-coordinate of a mouse click, in pixels, 
        /// relative to the <see cref="GraphScene"/>'s view's coordinate 
        /// system. </param>
        /// <param name="sceneY">The y-coordinate of a mouse click, in pixels, 
        /// relative to the <see cref="GraphScene"/>'s view's coordinate 
        /// system. </param>
        /// <param name="x">The x-coordinate of a mouse click, in pixels, 
        /// relative to the <see cref="GraphElement"/>'s coordinate system.
        /// </param>
        /// <param name="y">The y-coordinate of a mouse click, in pixels, 
        /// relative to the <see cref="GraphElement"/>'s coordinate system.
        /// </param>
        /// <param name="delta">A signed count of the number of detents the 
        /// wheel has rotated.</param>
        public GraphMouseEventArgs(MouseButtons button, int clicks, 
            int sceneX, int sceneY, float x, float y, int delta)
        {
            this.mButton = button;
            this.mClicks = clicks;
            this.mSceneX = sceneX;
            this.mSceneY = sceneY;
            this.mX = x;
            this.mY = y;
            this.mDelta = delta;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphMouseEventArgs"/>
        /// class by copying the values of the given instance of the same class.
        /// </summary>
        /// <param name="e">The <see cref="GraphMouseEventArgs"/> instance
        /// whose values are cloned into this <see cref="GraphMouseEventArgs"/>
        /// instance.</param>
        public GraphMouseEventArgs(GraphMouseEventArgs e) : this(e, 0, 0) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphMouseEventArgs"/>
        /// class by copying the values of the given instance of the same class,
        /// and offsets the new instance's <see cref="Pos"/> by the given
        /// values <paramref name="dx"/> and <paramref name="dy"/>.
        /// </summary>
        /// <param name="e">The <see cref="GraphMouseEventArgs"/> instance
        /// whose values are cloned into this <see cref="GraphMouseEventArgs"/>
        /// instance.</param>
        /// <param name="dx">The horizontal offset to add to 
        /// <paramref name="e"/>'s <see cref="X"/> value to get this instance's
        /// <see cref="X"/> value. </param>
        /// <param name="dy">The vertical offset to add to 
        /// <paramref name="e"/>'s <see cref="Y"/> value to get this instance's
        /// <see cref="Y"/> value. </param>
        public GraphMouseEventArgs(GraphMouseEventArgs e, float dx, float dy)
        {
            this.Handled = e.Handled;
            this.mButton = e.mButton;
            this.mClicks = e.mClicks;
            this.mSceneX = e.mSceneX;
            this.mSceneY = e.mSceneY;
            this.mX = e.X + dx;
            this.mY = e.Y + dy;
            this.mDelta = e.mDelta;
        }

        /// <summary>
        /// Gets which mouse button was pressed.
        /// </summary><value>
        /// One of the <see cref="System.Windows.Forms.MouseButtons"/> values.
        /// </value>
        public MouseButtons Button
        {
            get { return this.mButton; }
        }

        /// <summary>
        /// Gets the number of times the mouse button was pressed and released.
        /// </summary><value>
        /// An <see cref="System.Int32"/> containing the number of times the
        /// mouse button was pressed and released.
        /// </value>
        public int Clicks
        {
            get { return this.mClicks; }
        }

        /// <summary>
        /// Gets the x-coordinate of the mouse during the generating mouse 
        /// event, relative to the <see cref="GraphScene"/>'s view's
        /// coordinate system. </summary><value>
        /// The x-coordinate of the mouse, in pixels, relative to the 
        /// <see cref="GraphScene"/>'s local coordinate system.</value>
        public int SceneX
        {
            get { return this.mSceneX; }
        }

        /// <summary>
        /// Gets the y-coordinate of the mouse during the generating mouse 
        /// event, relative to the <see cref="GraphScene"/>'s view's
        /// coordinate system. </summary><value>
        /// The y-coordinate of the mouse, in pixels, relative to the 
        /// <see cref="GraphScene"/>'s local coordinate system.</value>
        public int SceneY
        {
            get { return this.mSceneY; }
        }

        /// <summary>
        /// Gets the position of the mouse during the generating mouse 
        /// event, relative to the <see cref="GraphScene"/>'s view's
        /// coordinate system. </summary><value>
        /// A <see cref="System.Drawing.Point"/> containing the
        /// x- and y- coordinates of the mouse, in pixels, relative to 
        /// the <see cref="GraphScene"/>'s view's local coordinate system.
        /// </value>
        public Point ScenePos
        {
            get { return new Point(this.mSceneX, this.mSceneY); }
        }

        /// <summary>
        /// Gets the x-coordinate of the mouse during the generating mouse 
        /// event, relative to the <see cref="GraphElement"/>'s local
        /// coordinate system. </summary><value>
        /// The x-coordinate of the mouse, in pixels, relative to the 
        /// <see cref="GraphElement"/>'s local coordinate system.</value>
        public float X
        {
            get { return this.mX; }
        }

        /// <summary>
        /// Gets the y-coordinate of the mouse during the generating mouse 
        /// event, relative to the <see cref="GraphElement"/>'s local
        /// coordinate system. </summary><value>
        /// The y-coordinate of the mouse, in pixels, relative to the 
        /// <see cref="GraphElement"/>'s local coordinate system.</value>
        public float Y
        {
            get { return this.mY; }
        }

        /// <summary>
        /// Gets the position of the mouse during the generating mouse 
        /// event, relative to the <see cref="GraphElement"/>'s local
        /// coordinate system. </summary><value>
        /// A <see cref="System.Drawing.PointF"/> containing the
        /// x- and y- coordinates of the mouse, in pixels, relative to 
        /// the <see cref="GraphElement"/>'s local coordinate system.
        /// </value>
        public PointF Pos
        {
            get { return new PointF(this.mX, this.mY); }
        }

        /// <summary>
        /// Gets the location of the mouse during the generating mouse 
        /// event, relative to the <see cref="GraphElement"/>'s local
        /// coordinate system. </summary><value>
        /// A <see cref="System.Drawing.Point"/> containing the
        /// x- and y- coordinates of the mouse, in pixels, relative to 
        /// the <see cref="GraphElement"/>'s local coordinate system.
        /// </value>
        public Point Location
        {
            get { return new Point((int)this.mX, (int)this.mY); }
        }

        /// <summary>
        /// Gets a signed count of the number of detents the mouse wheel has 
        /// rotated. A detent is one notch of the mouse wheel.
        /// </summary><value>
        /// A signed count of the number of detents the mouse wheel has 
        /// rotated.</value>
        public int Delta
        {
            get { return this.mDelta; }
        }
    }
}
