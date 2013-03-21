using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GraphForms
{
    /// <summary><para>
    /// A simple double-buffered extension of the 
    /// <see cref="System.Windows.Forms.Panel"/> class,
    /// which also doesn't allow auto-scrolling.
    /// </para><para>
    /// This class is mainly meant for rendering and relaying mouse events
    /// to <see cref="GraphElement"/> instances.
    /// </para></summary>
    [ToolboxBitmap(typeof(Panel))]
    public class GraphPanel : Panel, IHasGraphScene
    {
        /// <summary>
        /// Initializes a new instance of <see cref="GraphPanel"/> class.
        /// </summary>
        public GraphPanel()
        {
            base.SetStyle(ControlStyles.DoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.SupportsTransparentBackColor |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint
                , true);
        }

        /// <summary>
        /// Always returns false because this control doesn't allow the user
        /// to scroll to any controls placed outside of its visible boundaries.
        /// Technically this container shouldn't contain any other controls.
        /// </summary>
        public override bool AutoScroll
        {
            get
            {
                return base.AutoScroll;
            }
            set
            {
            }
        }

        private GraphScene mScene;

        /// <summary>
        /// The <see cref="GraphScene"/> instance that this graph panel is
        /// attached to for rendering it and relaying mouse events to it.
        /// </summary>
        public virtual GraphScene Scene
        {
            get { return this.mScene; }
            set { this.mScene = value; }
        }
    }
}
