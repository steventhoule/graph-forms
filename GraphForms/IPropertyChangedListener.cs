using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms
{
    /// <summary>
    /// Classes that implement this interface are able to be attached to
    /// <see cref="PropertyChangedNotifier"/> instances and are notified 
    /// whenever a property on one of those instances is changed.
    /// </summary>
    public interface IPropertyChangedListener
    {
        /// <summary>
        /// This method is called whenever a property is changed on any
        /// <see cref="PropertyChangedNotifier"/> that this listener is
        /// attached to.</summary>
        /// <param name="sender">The <see cref="PropertyChangedNotifier"/>
        /// instance that called this function.</param>
        /// <param name="propertyName">The name of the property of on 
        /// <paramref name="sender"/> that was changed.</param>
        void OnPropertyChanged(PropertyChangedNotifier sender, string propertyName);
    }
}
