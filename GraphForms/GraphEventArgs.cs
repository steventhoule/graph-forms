using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms
{
    /// <summary>
    /// Base class for arguments of events that are propagated to
    /// <see cref="GraphElement"/> instances down their ancestry chains.
    /// </summary>
    public class GraphEventArgs
    {
        private bool mHandled;

        /// <summary>
        /// Initializes a new unhandled instance of the 
        /// <see cref="GraphEventArgs"/> class.
        /// </summary>
        public GraphEventArgs()
        {
            this.mHandled = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphEventArgs"/>
        /// class with its <see cref="Handled"/> value set to 
        /// <paramref name="handled"/>. </summary>
        /// <param name="handled">Whether or not the event has already been 
        /// <see cref="Handled"/>.</param>
        public GraphEventArgs(bool handled)
        {
            this.mHandled = handled;
        }

        /// <summary>
        /// Whether or not the event has been handled. If false, the event
        /// continues to be propogated to other <see cref="GraphElement"/>
        /// instances until it is set to true.
        /// </summary>
        public bool Handled
        {
            get { return this.mHandled; }
            set { this.mHandled = value; }
        }
    }
}
