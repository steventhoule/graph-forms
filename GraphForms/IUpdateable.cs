using System;

namespace GraphForms
{
    /// <summary>
    /// An interface for classes that can be told to update, usually by
    /// external code which also causes outside changes that will effect
    /// an <see cref="IUpdateable"/> instance, but are outside that
    /// instance's awareness.
    /// </summary>
    public interface IUpdateable
    {
        /// <summary>
        /// Called when this <see cref="IUpdateable"/> should be updated,
        /// particularly after external changes have occurred that will
        /// effect this instance, but are outside this instance's awareness.
        /// </summary>
        void Update();
    }
}
