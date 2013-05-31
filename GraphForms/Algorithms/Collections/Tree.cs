using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Collections
{
    /// <summary>
    /// This tree data structure can be used to create a disjoint set forest.
    /// It can represent both single element and a set which contains both it
    /// and possibly other elements as well in a spaghetti stack structure.
    /// It contains fundamental functions for processing sets in the forest,
    /// including <c>FindSet</c> and <c>Union</c>.
    /// </summary>
    /// <typeparam name="T">The type of data stored in each tree.</typeparam>
    /// <remarks><para>
    /// See the Wikipedia article on Disjoint-Set Data Structure for details:
    /// </para><para>
    /// http://en.wikipedia.org/wiki/Disjoint-set_data_structure
    /// </para></remarks>
    public class Tree<T>
    {
        private Tree<T> mRoot;
        private int mRank;
        public T Value;

        /// <summary>
        /// Creates a new tree instance, which initially represents both the 
        /// single <typeparamref name="T"/> element and the singleton subset
        /// of the disjoint forest which contains this element.
        /// </summary>
        /// <param name="value">The element to insert into a singleton subset
        /// of a disjoint forest.</param>
        public Tree(T value)
        {
            this.mRoot = null;
            this.mRank = 1;
            this.Value = value;
        }

        /// <summary>
        /// Finds the subset of the disjoint forest which contains this tree.
        /// </summary>
        /// <returns>The element that represents the subset which contains
        /// both it and this element.</returns>
        /// <remarks>
        /// The element returned could be this tree element itself.
        /// This is always the case when this element is in a singleton,
        /// a set containing only one element.
        /// </remarks>
        public Tree<T> FindSet()
        {
            return FindSet(this);
        }

        /// <summary>
        /// Finds the subset of the disjoint forest which contains the given
        /// <paramref name="tree"/> element.
        /// </summary>
        /// <param name="tree">The element used to find its containing subset.
        /// </param>
        /// <returns>The element that represents the subset which contains
        /// both it and the given <paramref name="tree"/> element.</returns>
        /// <remarks><para>
        /// The element returned could be <paramref name="tree"/> itself.
        /// This is always the case when <paramref name="tree"/> is in a
        /// singleton, a set containing only one tree element.
        /// </para><para>
        /// This algorithm also performs path compression when possible for
        /// increased efficiency.
        /// </para></remarks>
        public static Tree<T> FindSet(Tree<T> tree)
        {
            Tree<T> temp = tree.mRoot;
            // Tree is a root set
            if (temp == null)
                return tree;
            Tree<T> root = tree;
            // Find the root set
            while (root.mRoot != null)
            {
                root = root.mRoot;
            }
            // Compress the paths of all subsets
            while (temp != root)
            {
                tree.mRoot = root;
                tree = temp;
                temp = tree.mRoot;
            }
            return root;
        }

        /// <summary>
        /// Tests whether the two given elements are contained in the same
        /// subset of the disjoint forest.
        /// </summary>
        /// <param name="tree1">The first element to test.</param>
        /// <param name="tree2">The second element to test.</param>
        /// <returns>True if <paramref name="tree1"/> and 
        /// <paramref name="tree2"/> are contained in the same subset of the
        /// disjoint forest; false if they are in seperate subsets.</returns>
        public static bool AreInSameSet(Tree<T> tree1, Tree<T> tree2)
        {
            return FindSet(tree1) == FindSet(tree2);
        }

        /// <summary>
        /// Joins the two subsets of the disjoint forest which contain the
        /// given elements into a single subset (if they are not already
        /// both contained in the same subset of the disjoint forest).
        /// </summary>
        /// <param name="tree1">The first element or subset to join.</param>
        /// <param name="tree2">The second element or subset to join.</param>
        /// <returns>True if subsets containing <paramref name="tree1"/> and
        /// <paramref name="tree2"/> are successfully united, or false if
        /// the two elements are already in the same subset.</returns>
        /// <remarks>
        /// This algorithm performs union by rank for increased efficiency.
        /// </remarks>
        public static bool Union(Tree<T> tree1, Tree<T> tree2)
        {
            tree1 = FindSet(tree1);
            tree2 = FindSet(tree2);
            if (tree1 == tree2)
                return false;
            if (tree1.mRank > tree2.mRank)
            {
                tree2.mRoot = tree1;
                tree1.mRank += tree2.mRank;
            }
            else
            {
                tree1.mRoot = tree2;
                tree2.mRank += tree1.mRank;
            }
            return true;
        }
    }
}
