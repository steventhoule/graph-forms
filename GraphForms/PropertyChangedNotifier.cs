using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms
{
    /// <summary>
    /// Notifies any attached <see cref="IPropertyChangedListener"/> instance 
    /// whenever a property of this class has changed.
    /// </summary>
    public class PropertyChangedNotifier
    {
        private IPropertyChangedListener mListener;

        /// <summary>
        /// An <see cref="IPropertyChangedListener"/> instance attached to this
        /// <see cref="PropertyChangedNotifier"/> instance, which is notified
        /// whenever a property of this instance has changed.
        /// </summary>
        public IPropertyChangedListener Listener
        {
            get { return this.mListener; }
            set { this.mListener = value; }
        }

        /// <summary>
        /// Notifies the attached <see cref="Listener"/> that the property
        /// with the given name has changed. This function must be called
        /// whenever any property in descendent classes is changed; otherwise
        /// it defeats the purpose of this class.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.
        /// </param>
        protected void OnPropertyChanged(string propertyName)
        {
            if (this.mListener != null)
                this.mListener.OnPropertyChanged(this, propertyName);
        }
    }
}
