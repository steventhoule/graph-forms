using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms
{
    /// <summary>
    /// An extension of <see cref="IAlgorithm"/> which also acts as a wrapper
    /// for a <typeparamref name="Node"/> instance that can be set null like
    /// a reference type, and used to initiate the algorithm as its root
    /// starting point.</summary>
    /// <typeparam name="Node">The underlying type of the root starting point
    /// of this algorithm.</typeparam>
    public interface IRootedAlgorithm<Node> : IAlgorithm
    {
        /// <summary>
        /// Indicates whether this rooted algorithm has a root value.
        /// </summary>
        bool HasRoot { get; }

        /// <summary>
        /// Tries to get the current root value of this rooted algorithm, 
        /// or returns the default value of the <typeparamref name="Node"/> 
        /// type if <see cref="HasRoot"/> is false.
        /// </summary>
        /// <returns>The current root value, or the default of the
        /// <typeparamref name="Node"/> type if this rooted algorithm 
        /// doesn't have a root value.</returns>
        Node TryGetRoot();

        /// <summary>
        /// Sets the root value of this rooted algorithm to the given 
        /// <typeparamref name="Node"/> instance.
        /// </summary>
        /// <param name="root">The new root value of this rooted algorithm.
        /// </param>
        void SetRoot(Node root);

        /// <summary>
        /// Clears the current root value of this rooted algorithm, setting 
        /// it back to the default value for the <typeparamref name="Node"/> 
        /// type and setting <see cref="HasRoot"/> to false.
        /// </summary>
        void ClearRoot();
    }
}
