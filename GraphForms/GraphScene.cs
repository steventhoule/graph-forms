using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GraphForms
{
    public class GraphScene : GraphElement
    {
        public void AddItem(GraphElement item)
        {
            item.SetParent(this);
        }

        public void RemoveItem(GraphElement item)
        {
            item.SetParent(null);
        }

        private List<Control> mViews = new List<Control>();

        public bool AddView(Control view)
        {
            if (this.mViews.Contains(view))
                return false;

            this.mViews.Add(view);

            view.Paint += new PaintEventHandler(OnViewPaint);

            view.MouseClick += new MouseEventHandler(OnViewMouseClick);
            view.MouseDoubleClick += new MouseEventHandler(OnViewMouseDoubleClick);
            view.MouseDown += new MouseEventHandler(OnViewMouseDown);
            view.MouseMove += new MouseEventHandler(OnViewMouseMove);
            view.MouseUp += new MouseEventHandler(OnViewMouseUp);
            view.MouseWheel += new MouseEventHandler(OnViewMouseWheel);

            return true;
        }

        public bool RemoveView(Control view)
        {
            int index = this.mViews.IndexOf(view);
            if (index >= 0)
            {
                view.Paint -= new PaintEventHandler(OnViewPaint);

                view.MouseClick -= new MouseEventHandler(OnViewMouseClick);
                view.MouseDoubleClick -= new MouseEventHandler(OnViewMouseDoubleClick);
                view.MouseDown -= new MouseEventHandler(OnViewMouseDown);
                view.MouseMove -= new MouseEventHandler(OnViewMouseMove);
                view.MouseUp -= new MouseEventHandler(OnViewMouseUp);
                view.MouseWheel -= new MouseEventHandler(OnViewMouseWheel);

                this.mViews.RemoveAt(index);

                return true;
            }
            return false;
        }

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
