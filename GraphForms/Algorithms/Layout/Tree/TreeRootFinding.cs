using System;

namespace GraphForms.Algorithms.Layout.Tree
{
    /// <summary>
    /// Used to specify which method a tree-based layout algorithm uses to 
    /// choose the root node from which all subtrees branch off.
    /// </summary>
    public enum TreeRootFinding
    {
        /// <summary>
        /// Chooses a root based on the values set with
        /// <see cref="M:AGraphAlgorithm`2{Node,Edge}.AddRoot(Node)"/>.
        /// </summary>
        UserDefined,
        /// <summary>
        /// Chooses a root with no incoming edges, only outgoing ones.
        /// </summary>
        SourceDirected,
        /// <summary>
        /// Chooses a root with no outgoing edges, only incoming ones.
        /// </summary>
        SinkDirected,
        /// <summary>
        /// Chooses a root such that the depth of the tree is minimized.
        /// </summary>
        Center,
        /// <summary>
        /// Chooses a root that has the most undirected paths from one
        /// node to another going through it (the true tree root).
        /// </summary>
        PathCenter
    }
}
