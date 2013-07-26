using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Layout.Tree
{
    /// <summary>
    /// Used to specify which method a tree-based layout algorithm uses to 
    /// choose the root node from which all subtrees branch off.
    /// </summary>
    public enum TreeRootFinding
    {
        /// <summary>
        /// Chooses a root based on the last value set with
        /// <see cref="M:ARootedAlgorithm`1{Node}.SetRoot(Node)"/>.
        /// </summary>
        UserDefined,
        /// <summary>
        /// Chooses a root such that the depth of the tree is minimized.
        /// </summary>
        Center,
    }
}
