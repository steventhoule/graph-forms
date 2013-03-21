using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace GraphForms
{
    /// <summary>
    /// An interface for any class that is linked to a <see cref="GraphScene"/>
    /// instance.
    /// </summary><remarks>
    /// This is mainly meant to be implemented by 
    /// <see cref="System.Windows.Forms.Control"/> descendant classes, as it
    /// is used by <see cref="GraphScene"/> in its 
    /// <see cref="GraphScene.AddView(System.Windows.Forms.Control)"/> and 
    /// <see cref="GraphScene.RemoveView(System.Windows.Forms.Control)"/>
    /// functions for making sure the control instance is no longer linked to 
    /// the <see cref="GraphScene"/> instance.
    /// </remarks>
    public interface IHasGraphScene
    {
        /// <summary>
        /// The <see cref="GraphScene"/> instance that implementers of this 
        /// interface are linked to.
        /// </summary>
        GraphScene Scene { get; set; }
    }
}
