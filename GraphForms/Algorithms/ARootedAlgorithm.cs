using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms
{
    /// <summary>
    /// An extension of <see cref="AAlgorithm"/> which also acts as a wrapper
    /// for a <typeparamref name="Node"/> instance that can be set null like
    /// a reference type, and used to initiate the algorithm as its root
    /// starting point.</summary>
    /// <typeparam name="Node">The underlying type of the root starting point
    /// of this algorithm.</typeparam>
    public abstract class ARootedAlgorithm<Node> 
        : AAlgorithm, IRootedAlgorithm<Node>
    {
        private Node mRoot;
        private bool bHasRoot;

        /// <summary>
        /// Indicates whether this rooted algorithm has a root value.
        /// </summary>
        public bool HasRoot
        {
            get { return this.bHasRoot; }
        }

        /// <summary>
        /// Tries to get the current root value of this rooted algorithm, 
        /// or returns the default value of the <typeparamref name="Node"/> 
        /// type if <see cref="HasRoot"/> is false.
        /// </summary>
        /// <returns>The current root value, or the default of the
        /// <typeparamref name="Node"/> type if this rooted algorithm 
        /// doesn't have a root value.</returns>
        public Node TryGetRoot()
        {
            if (this.bHasRoot)
                return this.mRoot;
            else
                return default(Node);
        }

        /// <summary>
        /// Sets the root value of this rooted algorithm to the given 
        /// <typeparamref name="Node"/> instance.
        /// </summary>
        /// <param name="root">The new root value of this rooted algorithm.
        /// </param>
        public void SetRoot(Node root)
        {
            bool changed = !object.Equals(this.mRoot, root);
            Node oldRoot = this.mRoot;
            this.mRoot = root;
            if (changed)
                this.OnRootChanged(oldRoot);
            this.bHasRoot = true;
        }

        /// <summary>
        /// Clears the current root value of this rooted algorithm, setting 
        /// it back to the default value for the <typeparamref name="Node"/> 
        /// type and setting <see cref="HasRoot"/> to false.
        /// </summary>
        public void ClearRoot()
        {
            this.mRoot = default(Node);
            this.bHasRoot = false;
        }

        /// <summary>
        /// Called whenever root value of this rooted algorithm is set to a 
        /// value that is different from its current root value, using the
        /// <see cref="SetRoot(Node)"/> function.
        /// </summary>
        /// <param name="oldRoot">The previous root value of this rooted
        /// algorithm before it was set to its current value.</param>
        protected virtual void OnRootChanged(Node oldRoot)
        {
        }

        /// <summary>
        /// Sets the root value of this rooted algorithm to the given 
        /// <typeparamref name="Node"/> instance to act as a starting point 
        /// for the computation, and then starts running this algorithm and
        /// its computation.</summary>
        /// <param name="root">The new root value for this rooted algorithm
        /// to act as a starting point for its computation.</param>
        public void Compute(Node root)
        {
            this.SetRoot(root);
            this.Compute();
        }
    }
}
