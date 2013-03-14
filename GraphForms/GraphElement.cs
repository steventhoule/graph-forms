using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace GraphForms
{
    /// <summary>
    /// A class with traceable ancestry, 
    /// sorted within its parent based on its Z value 
    /// and specified stacking order.
    /// </summary>
    public partial class GraphElement
    {
        private GraphElement parent;
        private int siblingIndex;
        private List<GraphElement> children = new List<GraphElement>();

        private bool bStacksBehindParent = false;
        private bool bNegativeZStacksBehindParent = false;

        private float z = 0f;

        #region Depth
        /// <summary>
        /// The depth in the heirarchy tree of parents;
        /// 0 if parent is null; -1 if unresolved.
        /// </summary>
        private int itemDepth = -1;

        /// <summary>
        /// The depth in the heirarchy tree of parents,
        /// which is 0 if the parent is null.
        /// </summary>
        public int Depth
        {
            get
            {
                if (itemDepth == -1)
                    this.ResolveDepth();

                return itemDepth;
            }
        }

        /// <summary>
        /// Sets the depth of this object and all its children to -1.
        /// </summary>
        private void InvalidateDepthRecursively()
        {
            if (itemDepth == -1)
                return;

            itemDepth = -1;
            for (int i = 0; i < children.Count; ++i)
                children[i].InvalidateDepthRecursively();
        }

        /// <summary>
        /// Resolves the stacking depth of this object and all its ancestors.
        /// </summary>
        private void ResolveDepth()
        {
            if (parent == null)
                itemDepth = 0;
            else
            {
                if (parent.itemDepth == -1)
                    parent.ResolveDepth();
                itemDepth = parent.itemDepth + 1;
            }
        }
        #endregion

        #region Ancestry
        /// <summary>
        /// Tests if this object is an ancestor of <paramref name="child"/>
        /// (i.e., if this object is <paramref name="child"/>'s parent,
        /// or one of <paramref name="child"/>'s parent's ancestors).
        /// </summary>
        /// <param name="child">The potential descendent to test.</param>
        /// <returns>true if this object is an ancestor of 
        /// <paramref name="child"/>, false otherwise.</returns>
        public bool IsAncestorOf(GraphElement child)
        {
            if (child == null || child == this)
                return false;
            if (child.Depth < this.Depth)
                return false;
            GraphElement ancestor = child.parent;
            while (ancestor != null)
            {
                if (ancestor == this)
                    return true;
                ancestor = ancestor.parent;
            }
            return false;
        }

        /// <summary>
        /// Returns the closest common ancestor object 
        /// of this object and <paramref name="other"/>, 
        /// or null if <paramref name="other"/> is null, 
        /// or if there is no common ancestor.
        /// </summary>
        /// <param name="other">The other object to test.</param>
        /// <returns>The closest common ancestor object
        /// of this object and <paramref name="other"/></returns>
        public GraphElement CommonAncestor(GraphElement other)
        {
            if (other == null)
                return null;
            if (other == this)
                return this;
            GraphElement thisw = this;
            GraphElement otherw = other;
            int thisDepth = this.Depth;
            int otherDepth = other.Depth;
            while (thisDepth > otherDepth)
            {
                thisw = thisw.parent;
                --thisDepth;
            }
            while (otherDepth > thisDepth)
            {
                otherw = other.parent;
                --otherDepth;
            }
            while (thisw != null && thisw != otherw)
            {
                thisw = thisw.parent;
                otherw = otherw.parent;
            }
            return thisw;
        }
        #endregion

        #region Children Sorting
        /// <summary>
        /// Determines whether or not <paramref name="item1"/> is on top of
        /// its sibling, <paramref name="item2"/>, 
        /// based on their stacking properties, Z values, and sibling indexes.
        /// </summary>
        /// <param name="item1">The first sibling to compare.</param>
        /// <param name="item2">The second sibling to compare.</param>
        /// <returns>true if <paramref name="item1"/> is on top of 
        /// <paramref name="item2"/>, false otherwise.</returns>
        public static bool ClosestLeaf(GraphElement item1, GraphElement item2)
        {
            if (item1.bStacksBehindParent != item2.bStacksBehindParent)
                return item2.bStacksBehindParent;
            if (item1.z != item2.z)
                return item1.z > item2.z;
            return item1.siblingIndex > item2.siblingIndex;
        }

        /// <summary>
        /// Tests whether or not <paramref name="item1"/> 
        /// is on top of or stacked closer to the top 
        /// of the display than <paramref name="item2"/>, 
        /// in terms of Z-values and parent stacking rules.
        /// </summary>
        /// <param name="item1">The first specified item.</param>
        /// <param name="item2">The second specified item.</param>
        /// <returns>true if <paramref name="item1"/> is closer top
        /// than <paramref name="item2"/>, false otherwise.</returns>
        public static bool ClosestItemFirst(GraphElement item1, GraphElement item2)
        {
            // Siblings? Just check their z-values.
            if (item1.parent == item2.parent)
                return ClosestLeaf(item1, item2);

            // Find common ancestor, and each item's ancestor closest 
            // to the common ancestor.
            int item1Depth = item1.Depth;
            int item2Depth = item2.Depth;
            GraphElement p = item1.parent;
            GraphElement t1 = item1;
            while (item1Depth > item2Depth && p != null)
            {
                if (p == item2)
                {
                    // item2 is one of item1's ancestors; item1 is on top
                    return t1.bStacksBehindParent;
                }
                t1 = p;
                --item1Depth;
                p = p.parent;
            }
            p = item2.parent;
            GraphElement t2 = item2;
            while (item2Depth > item1Depth && p != null)
            {
                if (p == item1)
                {
                    // item1 is one of item2's ancestors; item2 is on top
                    return t2.bStacksBehindParent;
                }
                t2 = p;
                --item2Depth;
                p = p.parent;
            }

            // item1Ancestor is now at the same level as item2Ancestor, but not the same.
            GraphElement p1 = t1;
            GraphElement p2 = t2;
            while (t1 != null && t1 != t2)
            {
                p1 = t1;
                p2 = t2;
                t1 = t1.parent;
                t2 = t2.parent;
            }

            // in case we have a common ancestor, 
            // we compare the immediate children in the ancestor's path.
            // otherwise we compare the respective items' TopLevelItems directly.
            return ClosestLeaf(p1, p2);
        }

        private bool needSortChildren = false;
        /// <summary>
        /// Whether or not the sibling indexes of the children 
        /// match their ordering in their containing list.
        /// </summary>
        private bool sequentialOrdering = true;
        /// <summary>
        /// Whether or not there are gaps in the sibling indexes of the children.
        /// </summary>
        private bool holesInSiblingIndex = false;

        private static int notClosestLeaf(GraphElement item1, GraphElement item2)
        {
            return ClosestLeaf(item1, item2) ? -1 : 1;
        }
        /// <summary>
        /// Ensures the children are sorted in ascending order (bottom-most first)
        /// based on their stacking properties, Z values, and sibling indexes.
        /// </summary>
        private void EnsureSortedChildren()
        {
            if (this.needSortChildren)
            {
                this.needSortChildren = false;
                this.sequentialOrdering = true;
                if (children.Count == 0)
                    return;
                this.children.Sort(new Comparison<GraphElement>(notClosestLeaf));
                for (int i = 0; i < this.children.Count; ++i)
                {
                    if (this.children[i].siblingIndex != i)
                    {
                        this.sequentialOrdering = false;
                        break;
                    }
                }
            }
        }

        private static int insertOrder(GraphElement a, GraphElement b)
        {
            return a.siblingIndex - b.siblingIndex;
        }
        /// <summary>
        /// Ensures that the list of children is sorted by insertion order,
        /// and that the sibling indexes are packed (no gaps), and start at 0.
        /// </summary>
        private void EnsureSequentialSiblingIndex()
        {
            if (!this.sequentialOrdering)
            {
                this.children.Sort(new Comparison<GraphElement>(insertOrder));
                this.sequentialOrdering = true;
                this.needSortChildren = true;
            }
            if (this.holesInSiblingIndex)
            {
                this.holesInSiblingIndex = false;
                for (int i = 0; i < this.children.Count; ++i)
                    this.children[i].siblingIndex = i;
            }
        }

        /// <summary>
        /// Stacks this object before <paramref name="sibling"/>, which must be a sibling object 
        /// (i.e., the two objects must share the same parent, or must both be toplevel objects). 
        /// The <paramref name="sibling"/> must have the same Z value as this object, 
        /// otherwise calling this function will have no effect.
        /// </summary>
        /// <param name="sibling">The sibling behind which this object is stacked.</param>
        /// <returns>false if <paramref name="sibling"/> is not actually a sibling
        /// of this object, true otherwise.</returns>
        /// <remarks>
        /// By default, all sibling items are stacked by insertion order 
        /// (i.e., the first item you add is drawn before the next item you add). 
        /// If two items' Z values are different, then the item with the highest Z value is drawn on top. 
        /// When the Z values are the same, the insertion order will decide the stacking order.
        /// </remarks>
        public bool StackBefore(GraphElement sibling)
        {
            if (sibling == this)
                return false;
            if (sibling == null || this.parent != sibling.parent)
            {
                Debug.WriteLine("Warning: Cannot stack under given object, which must be a sibling");
                return false;
            }
            List<GraphElement> siblings = this.parent != null
                ? this.parent.children
                : null;//(this.scene != null ? this.scene.TopLevelItems : null);
            if (siblings == null)
            {
                Debug.WriteLine("Warning: Cannot stack under given object, which must be a sibling");
                return false;
            }

            // First, make sure that the sibling indexes have no holes. 
            // This also marks the children list for sorting.
            if (this.parent != null)
                this.parent.EnsureSequentialSiblingIndex();
            //else
            //    this.scene.ensureSequentialTopLevelSiblingIndexes();

            int i, index;
            // Only move items with the same Z value, and that need moving.
            int siblingIndex = sibling.siblingIndex;
            int myIndex = this.siblingIndex;
            if (myIndex >= siblingIndex)
            {
                //siblings.RemoveAt(myIndex);
                //siblings.Insert(siblingIndex, this);
                for (i = myIndex; i > siblingIndex; i--)
                {
                    siblings[i] = siblings[i - 1];
                }
                siblings[siblingIndex] = this;
                // Fixup the insertion ordering.
                for (i = 0; i < siblings.Count; i++)
                {
                    index = siblings[i].siblingIndex;
                    if (i != siblingIndex && index >= siblingIndex && index <= myIndex)
                    {
                        siblings[i].siblingIndex = ++index;
                    }
                }
                this.siblingIndex = siblingIndex;
                /*for (i = 0; i < siblings.Count; i++)
                {
                    index = siblings[i].siblingIndex;
                    if (i != siblingIndex && index >= siblingIndex && index <= myIndex)
                        siblings[i].siblingOrderChange();
                }
                this.siblingOrderChange();/**/
            }
            return true;
        }
        #endregion

        #region Flags
        /// <summary>
        /// Whether or not this object is stacked behind its parent.
        /// </summary><remarks>
        /// By default, children are stacked on top of their parent. 
        /// But setting this flag, the child will be stacked behind it. 
        /// This flag is useful for drop shadow effects and for decoration objects 
        /// that follow the parent object's geometry without drawing on top of it.
        /// </remarks>
        public bool StacksBehindParent
        {
            get { return this.bStacksBehindParent; }
            set
            {
                if (this.bStacksBehindParent != value)
                {
                    this.bStacksBehindParent = value;
                    // Ensure child item sorting is up to date when toggling this flag.
                    if (this.parent != null)
                        this.parent.needSortChildren = true;
                }
            }
        }

        /// <summary>
        /// The item automatically stacks behind it's parent if it's Z-value is negative. 
        /// This flag enables <see cref="StackableElement.Zvalue"/> 
        /// to toggle <see cref="StackableElement.StacksBehindParent"/>.
        /// </summary>
        public bool NegativeZStacksBehindParent
        {
            get { return this.bNegativeZStacksBehindParent; }
            set
            {
                if (this.bNegativeZStacksBehindParent != value)
                {
                    this.bNegativeZStacksBehindParent = value;
                    // Update stack-behind.
                    this.StacksBehindParent = this.z < 0f;
                }
            }
        }
        #endregion

        #region Z Value
        /// <summary>
        /// The Z-value of this object, which affects the stacking order 
        /// of sibling (neighboring) object. Higher Z-valued objects
        /// are stacked on top of lower Z-valued objects.
        /// The default Z-value is 0.
        /// </summary>
        public float Zvalue
        {
            get { return this.z; }
            set
            {
                // TODO: Insert pre-notification (with possible adjustment)

                this.z = value;
                if (this.parent != null)
                    this.parent.needSortChildren = true;

                // TODO: Insert post-notification

                if (this.bNegativeZStacksBehindParent)
                    this.StacksBehindParent = this.z < 0f;

                // TODO: Insert event trigger
            }
        }
        #endregion

        #region Parent and Children
        /// <summary>
        /// Returns an array of this object's children.
        /// The children are sorted by stacking order.
        /// This takes into account both the children's
        /// insertion order and their Z-values.
        /// </summary>
        public GraphElement[] Children
        {
            get
            {
                this.EnsureSortedChildren();
                return this.children.ToArray();
            }
        }

        /// <summary>
        /// Whether or not this object has currently has children.
        /// </summary>
        public bool HasChildren
        {
            get { return this.children.Count > 0; }
        }

        private void AddChild(GraphElement child)
        {
            // Remove all holes from the sibling index list. Now the max index
            // number is equal to the size of the children list.
            this.EnsureSequentialSiblingIndex();
            this.needSortChildren = true; // maybe false
            child.siblingIndex = children.Count;
            this.children.Add(child);
            //if (isObject)
            //    ((GraphObject)this).childrenChanged();
        }

        private void RemoveChild(GraphElement child)
        {
            // When removing elements in the middle of the children list,
            // there will be a "gap" in the list of sibling indexes (0,1,3,4).
            if (!this.holesInSiblingIndex)
                this.holesInSiblingIndex = child.siblingIndex != children.Count - 1;
            if (this.sequentialOrdering && !this.holesInSiblingIndex)
                this.children.RemoveAt(child.siblingIndex);
            else
                this.children.Remove(child);
            // NB! Do not use children.RemoveAt(child.siblingIndex) because
            // the child is not guaranteed to be at the index after the list is sorted.
            // (see ensureSortedChildren()).
            child.siblingIndex = -1;
            //if (isObject)
            //    ((GraphObject)this).childrenChanged();
        }

        public GraphElement Parent
        {
            get { return this.parent; }
        }

        public bool SetParent(GraphElement parent)
        {
            // TODO: Insert pre-notification (with possible adjustment?)

            if (parent == this)
            {
                Debug.WriteLine("Warning: Cannot assign object as a parent of itself");
                return false;
            }
            if (parent == this.parent)
                return false;

            this.OnParentChanging(parent);

            // Remove from current parent
            if (this.parent != null)
            {
                this.parent.RemoveChild(this);
            }

            // Resolve depth.
            InvalidateDepthRecursively();

            GraphElement oldParent = this.parent;
            this.parent = parent;

            if (this.parent != null)
            {
                this.parent.AddChild(this);
            }

            this.OnParentChanged(oldParent);

            // TODO: Insert post-notification

            return true;
        }

        protected virtual void OnParentChanging(GraphElement newParent)
        {
        }

        protected virtual void OnParentChanged(GraphElement oldParent)
        {
        }
        #endregion
    }
}
