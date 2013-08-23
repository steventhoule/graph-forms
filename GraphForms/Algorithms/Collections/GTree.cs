using System;

namespace GraphForms.Algorithms.Collections
{
    public class GTree<Node, Edge, Geom>
    {
        public readonly int NodeIndex;
        /// <summary>
        /// The <typeparamref name="Node"/> instance stored
        /// in this single element of a tree data structure.
        /// </summary>
        public readonly Node NodeData;
        /// <summary>
        /// The <typeparamref name="Edge"/> instance stored
        /// in this single element of a tree data structure.
        /// </summary>
        public readonly Edge EdgeData;
        /// <summary>
        /// The <typeparamref name="Geom"/> instance stored
        /// in this single element of a tree data structure.
        /// </summary>
        public readonly Geom GeomData;

        private GTree<Node, Edge, Geom> mRoot;
        private int mTreeSize;
        private int mDepth;
        private int mIndex;
        private GTree<Node, Edge, Geom>[] mBranches;
        private int mBCount;
        private uint mBVersion;

        /*/// <summary>
        /// A convenience field used to indicate that something about this
        /// graph tree and/or one or more of its branches has been changed,
        /// and data calculated based on that property and/or the properties
        /// of its <see cref="Branches"/> needs to be recalculated.
        /// </summary>
        public bool Dirty;/* */

        public GTree(int nIndex, Node nData, Edge eData, Geom gData, 
            int branchCapacity)
        {
            if (branchCapacity < 0)
                throw new ArgumentOutOfRangeException("capacity");
            this.NodeIndex = nIndex;
            this.NodeData = nData;
            this.EdgeData = eData;
            this.GeomData = gData;

            this.mRoot = null;
            this.mTreeSize = 1;
            this.mDepth = 0;
            this.mIndex = -1;
            this.mBranches = new GTree<Node, Edge, Geom>[branchCapacity];
            this.mBCount = 0;
            this.mBVersion = 0;

            //this.Dirty = true;
        }

        /*/// <summary>
        /// Recursively sets to true the <see cref="Dirty"/> field of this  
        /// graph tree's <see cref="Root"/> and ancestors all the way down
        /// to the root of the entire tree data structure.
        /// </summary>
        public void InvalidateParent()
        {
            GTree<Node, Edge, Geom> p = this.mRoot;
            while (p != null && !p.Dirty)
            {
                p.Dirty = true;
                p = p.mRoot;
            }
        }/* */

        #region Tree Properties
        /// <summary>
        /// Gets the root (parent) of this graph tree, which is invalidated
        /// (along with its root through the ancestry chain) every time this
        /// graph tree is invalidated.</summary>
        public GTree<Node, Edge, Geom> Root
        {
            get { return this.mRoot; }
        }
        /// <summary>
        /// Sets the <see cref="Root"/> of this graph tree to the given
        /// <paramref name="root"/> value. If this graph tree's old root
        /// isn't null, this graph tree is removed from its 
        /// <see cref="Branches"/> and it is invalidated. If the new 
        /// <paramref name="root"/> is not null, this graph tree
        /// is then added to its <see cref="Branches"/> and it is then
        /// invalidated as well.</summary>
        /// <param name="root">The new <see cref="Root"/> 
        /// of this graph tree.</param>
        public void SetRoot(GTree<Node, Edge, Geom> root)
        {
            if (this.mRoot != root)
            {
                GTree<Node, Edge, Geom> p;
                if (this.mRoot != null)
                {
                    p = this.mRoot;
                    Array.Copy(p.mBranches, this.mIndex + 1,
                        p.mBranches, this.mIndex,
                        p.mBCount - this.mIndex);
                    p.mBCount--;
                    for (int i = this.mIndex; i < p.mBCount; i++)
                    {
                        p.mBranches[i].mIndex--;
                    }
                    p.AddToTreeSize(-this.mTreeSize);
                    this.mIndex = -1;
                    //this.InvalidateParent();
                    while (p != null)
                    {
                        p.mBVersion++;
                        p = p.mRoot;
                    }
                }
                this.mRoot = root;
                if (this.mRoot != null)
                {
                    p = this.mRoot;
                    if (p.mBCount == p.mBranches.Length)
                    {
                        if (p.mBCount == 0)
                        {
                            p.mBranches = new GTree<Node, Edge, Geom>[4];
                        }
                        else
                        {
                            GTree<Node, Edge, Geom>[] branches
                                = new GTree<Node, Edge, Geom>[2 * p.mBCount];
                            Array.Copy(p.mBranches, 0,
                                branches, 0, p.mBCount);
                            p.mBranches = branches;
                        }
                    }
                    p.AddToTreeSize(this.mTreeSize);
                    this.SetDepth(p.mDepth + 1);
                    this.mIndex = p.mBCount;
                    p.mBranches[p.mBCount++] = this;
                    //this.InvalidateParent();
                    while (p != null)
                    {
                        p.mBVersion++;
                        p = p.mRoot;
                    }
                }
                else
                {
                    this.SetDepth(0);
                }
            }
        }
        /// <summary>
        /// Gets the total number of nodes in this graph tree and all of its
        /// <see cref="Branches"/>. This equals one if this graph tree
        /// doesn't have any branches.
        /// </summary>
        public int TreeSize
        {
            get { return this.mTreeSize; }
        }
        private void AddToTreeSize(int size)
        {
            this.mTreeSize += size;
            GTree<Node, Edge, Geom> p = this.mRoot;
            while (p != null)
            {
                p.mTreeSize += size;
                p = p.mRoot;
            }
        }
        /// <summary>
        /// Gets the depth of this graph tree in its overall tree data
        /// structure, which is zero if this graph tree's <see cref="Root"/>
        /// is null.</summary>
        public int Depth
        {
            get { return this.mDepth; }
        }
        private void SetDepth(int depth)
        {
            this.mDepth = depth;
            if (this.mBCount > 0)
            {
                depth++;
                for (int i = 0; i < this.mBCount; i++)
                {
                    this.mBranches[i].SetDepth(depth);
                }
            }
        }
        /// <summary>
        /// Gets the index of this graph tree in the <see cref="Branches"/>
        /// of its <see cref="Root"/>, or -1 if this graph tree doesn't
        /// have a root.</summary>
        public int BranchIndex
        {
            get { return this.mIndex; }
        }
        /// <summary>
        /// Gets an array containing all graph tree instances that
        /// currently have this graph tree as their <see cref="Root"/>.
        /// </summary>
        public GTree<Node, Edge, Geom>[] Branches
        {
            get
            {
                GTree<Node, Edge, Geom>[] branches 
                    = new GTree<Node, Edge, Geom>[this.mBCount];
                if (this.mBCount > 0)
                    Array.Copy(this.mBranches, 0, branches, 0, this.mBCount);
                return branches;
            }
        }
        /// <summary>
        /// Gets the number of <see cref="Branches"/>
        /// that this graph tree currently has.
        /// </summary>
        public int BranchCount
        {
            get { return this.mBCount; }
        }
        /// <summary>
        /// Gets or sets the maximum number of <see cref="Branches"/> 
        /// that this graph tree is currently capable of holding without
        /// resizing its internal array.</summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <see cref="BranchCapacity"/> is set to a value that is
        /// less than <see cref="BranchCount"/>.</exception>
        /// <exception cref="OutOfMemoryException">There is not enough
        /// memory on the system.</exception><remarks>
        /// Be aware that this value is doubled every time a new branch 
        /// is added that would cause its <see cref="BranchCount"/> to  
        /// exceed its capacity.</remarks>
        public int BranchCapacity
        {
            get { return this.mBranches.Length; }
            set
            {
                if (value < this.mBCount)
                    throw new ArgumentOutOfRangeException("BranchCapacity");
                if (value != this.mBranches.Length)
                {
                    GTree<Node, Edge, Geom>[] branches
                        = new GTree<Node, Edge, Geom>[value];
                    if (this.mBCount > 0)
                    {
                        Array.Copy(this.mBranches, 0, 
                            branches, 0, this.mBCount);
                    }
                    this.mBranches = branches;
                }
            }
        }

        public uint BranchVersion
        {
            get { return this.mBVersion; }
        }
        #endregion
    }
}
