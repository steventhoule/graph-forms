using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GraphForms
{
    /// <summary>
    /// This extension of the <see cref="GraphElement"/> class provides
    /// a way of managing a group of <see cref="GraphElement"/> instances and
    /// linking them to <see cref="System.Windows.Forms.Control"/> instances
    /// that render them with their 
    /// <see cref="E:System.Windows.Forms.Control.Paint"/> event and relay
    /// mouse events to them.
    /// </summary>
    public class GraphScene : GraphElement
    {
        /// <summary>
        /// Adds or moves the given <paramref name="item"/> and all its 
        /// children to this scene by setting this scene as the
        /// <paramref name="item"/>'s <see cref="GraphElement.Parent"/>.
        /// </summary>
        /// <param name="item">The <see cref="GraphElement"/> to add to
        /// this scene by making this scene its parent.</param>
        public void AddItem(GraphElement item)
        {
            item.SetParent(this);
        }

        /// <summary>
        /// Removes the given <paramref name="item"/> and all its children
        /// from this scene by setting the <paramref name="item"/>'s
        /// <see cref="GraphElement.Parent"/> to null (and thereby making
        /// that item its own scene in certain aspects).
        /// </summary>
        /// <param name="item">The <see cref="GraphElement"/> to remove from
        /// this scene by setting its parent to null.</param>
        public void RemoveItem(GraphElement item)
        {
            item.SetParent(null);
        }

        private List<Control> mViews = new List<Control>();

        /// <summary>
        /// This scene's list of all <see cref="System.Windows.Forms.Control"/>
        /// instances currently tied to it, which render and relay mouse events
        /// to its contents.
        /// </summary>
        public Control[] Views
        {
            get { return this.mViews.ToArray(); }
        }

        /// <summary>
        /// Adds the given <paramref name="view"/> to this scene's list of
        /// <see cref="System.Windows.Forms.Control"/> instances and starts
        /// listening to its <see cref="E:System.Windows.Forms.Control.Paint"/>
        /// event for rendering this scene and its contents and listening to
        /// its <see cref="System.Windows.Forms.MouseEventHandler"/> events
        /// to relay mouse events to this scene and its contents.
        /// </summary>
        /// <param name="view">The <see cref="System.Windows.Forms.Control"/>
        /// instance to add to this scene for rendering its contents and
        /// relaying mouse events to its contents.</param>
        /// <returns>True if the <paramref name="view"/> was successfully added
        /// to this scene or false if the <paramref name="view"/> is already
        /// tied to this scene (on this scene's view list).</returns>
        public bool AddView(Control view)
        {
            if (this.mViews.Contains(view))
                return false;

            this.mViews.Add(view);

            this.AttachView(view);

            return true;
        }

        /// <summary>
        /// Attaches the given <paramref name="view"/> to this scene
        /// right after it's added to the <see cref="Views"/> list,
        /// adding listeners to its <see cref="E:System.Windows.Forms.Control.Paint"/>
        /// event for rendering and to its other events for relaying messages
        /// to its contents. </summary>
        /// <param name="view">The <see cref="System.Windows.Forms.Control"/>
        /// instance to attach to this scene.</param>
        private void AttachView(Control view)
        {
            view.Paint += new PaintEventHandler(OnViewPaint);

            view.MouseClick += new MouseEventHandler(OnViewMouseClick);
            view.MouseDoubleClick += new MouseEventHandler(OnViewMouseDoubleClick);
            view.MouseDown += new MouseEventHandler(OnViewMouseDown);
            view.MouseMove += new MouseEventHandler(OnViewMouseMove);
            view.MouseUp += new MouseEventHandler(OnViewMouseUp);
            view.MouseWheel += new MouseEventHandler(OnViewMouseWheel);

            this.UserAttachView(view);
        }

        /// <summary>
        /// Reimplement to add additional event listeners to the given
        /// <paramref name="view"/> after it has been attached to this scene
        /// and added to this scene's <see cref="View"/> list.
        /// </summary>
        /// <param name="view">The view just attached to this scene to add
        /// additional event listeners to.</param>
        protected virtual void UserAttachView(Control view)
        {
        }

        /// <summary>
        /// Removes the given <paramref name="view"/> from this scene's list of
        /// <see cref="System.Windows.Forms.Control"/> instances and stops
        /// listening to its <see cref="E:System.Windows.Forms.Control.Paint"/>
        /// event and <see cref="System.Windows.Forms.MouseEventHandler"/>
        /// events so that the <paramref name="view"/> no longer renders or
        /// relays mouse events to this scene and its contents.
        /// </summary>
        /// <param name="view">The <see cref="System.Windows.Forms.Control"/>
        /// instance to remove from this scene so that it no longer renders or
        /// relays mouse events to this scene and its contents.</param>
        /// <returns>True if the <paramref name="view"/> was successfully
        /// removed from this scene or false if the <paramref name="view"/>
        /// wasn't already tied to this scene (on this scene's view list).
        /// </returns>
        public bool RemoveView(Control view)
        {
            int index = this.mViews.IndexOf(view);
            if (index >= 0)
            {
                this.DetachView(view);

                this.mViews.RemoveAt(index);

                return true;
            }
            return false;
        }

        /// <summary>
        /// Detaches the given <paramref name="view"/> from this scene
        /// right before it's removed from the <see cref="Views"/> list,
        /// removing listeners from its <see cref="E:System.Windows.Forms.Control.Paint"/>
        /// event for rendering and from its other events for relaying messages
        /// to its contents. </summary>
        /// <param name="view">The <see cref="System.Windows.Forms.Control"/>
        /// instance to detach from this scene.</param>
        private void DetachView(Control view)
        {
            view.Paint -= new PaintEventHandler(OnViewPaint);

            view.MouseClick -= new MouseEventHandler(OnViewMouseClick);
            view.MouseDoubleClick -= new MouseEventHandler(OnViewMouseDoubleClick);
            view.MouseDown -= new MouseEventHandler(OnViewMouseDown);
            view.MouseMove -= new MouseEventHandler(OnViewMouseMove);
            view.MouseUp -= new MouseEventHandler(OnViewMouseUp);
            view.MouseWheel -= new MouseEventHandler(OnViewMouseWheel);

            this.UserDetachView(view);
        }

        /// <summary>
        /// Reimplement to remove additional event listeners from the given
        /// <paramref name="view"/> after it has been detached from this scene,
        /// but before it has been removed from this scene's <see cref="View"/>
        /// list. </summary>
        /// <param name="view">The view just attached to this scene to add
        /// additional event listeners to.</param>
        protected virtual void UserDetachView(Control view)
        {
        }

        /// <summary>
        /// Removes all <see cref="System.Windows.Forms.Control"/> instances
        /// from this scene's <see cref="View"/> list and detaches them all
        /// from this scene so that they no longer render or relay events to
        /// its contents. </summary>
        public void ClearViews()
        {
            for (int i = this.mViews.Count - 1; i >= 0; i--)
                this.DetachView(this.mViews[i]);
            this.mViews.Clear();
        }

        /// <summary>
        /// Draws the visual contents of this scene's background.
        /// </summary>
        /// <param name="e">The data from a 
        /// <see cref="E:System.Windows.Forms.Control.Paint"/> event,
        /// with its graphics and clipping rectangle adjusted to
        /// this scene's local coordinate system.</param>
        protected override void OnDrawBackground(PaintEventArgs e)
        {
        }

        /// <summary>
        /// This function invalidates the given <paramref name="rect"/> area
        /// of every <see cref="System.Windows.Forms.Control"/> tied to this
        /// scene, prompting all of them to update that area at the next paint
        /// operation and causing a paint message to be sent to all of them.
        /// </summary>
        /// <param name="rect">A <see cref="System.Drawing.Rectangle"/> that
        /// represents the area to invalidate on every 
        /// <see cref="System.Windows.Forms.Control"/> tied to this scene.
        /// </param>
        protected override void InvalidateScene(Rectangle rect)
        {
            for (int i = this.mViews.Count - 1; i >= 0; i--)
            {
                this.mViews[i].Invalidate(rect);
            }
        }

        private void OnViewPaint(object sender, PaintEventArgs e)
        {
            PointF pos = this.Position;
            Rectangle clip = e.ClipRectangle;
            clip.Offset(-(int)pos.X, -(int)pos.Y);
            e.Graphics.TranslateTransform(pos.X, pos.Y);
            this.OnPaint(new PaintEventArgs(e.Graphics, clip));
            e.Graphics.TranslateTransform(-pos.X, -pos.Y);
        }

        private GraphMouseEventArgs FixMouseEventArgs(MouseEventArgs e)
        {
            return new GraphMouseEventArgs(e.Button, e.Clicks,
                e.X, e.Y, e.X - this.X, e.Y - this.Y, e.Delta);
        }

        private void OnViewMouseClick(object sender, MouseEventArgs e)
        {
            this.FireMouseClick(this.FixMouseEventArgs(e));
        }

        private void OnViewMouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.FireMouseDoubleClick(this.FixMouseEventArgs(e));
        }

        private void OnViewMouseDown(object sender, MouseEventArgs e)
        {
            this.FireMouseDown(this.FixMouseEventArgs(e));
        }

        private void OnViewMouseMove(object sender, MouseEventArgs e)
        {
            this.FireMouseMove(this.FixMouseEventArgs(e));
        }

        private void OnViewMouseUp(object sender, MouseEventArgs e)
        {
            this.FireMouseUp(this.FixMouseEventArgs(e));
        }

        private void OnViewMouseWheel(object sender, MouseEventArgs e)
        {
            this.FireMouseWheel(this.FixMouseEventArgs(e));
        }
    }
}
