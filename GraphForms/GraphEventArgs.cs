using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms
{
    public class GraphEventArgs
    {
        private bool mHandled;

        public GraphEventArgs()
        {
            this.mHandled = false;
        }

        public GraphEventArgs(bool handled)
        {
            this.mHandled = handled;
        }

        public bool Handled
        {
            get { return this.mHandled; }
            set { this.mHandled = value; }
        }
    }
}
