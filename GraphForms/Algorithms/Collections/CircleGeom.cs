using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Collections
{
    /// <summary>
    /// A tree data structure used for storing a circle with a distinct
    /// radius and position around its parent in polar coordinates,
    /// which can then compute the net convex hull of its circle and the
    /// circles on all of its branches and return that hull as a series of 
    /// arcs for processing.
    /// </summary>
    /// <typeparam name="Node">The type of data stored in each circle
    /// in the tree, which usually represent something inscribed within
    /// each circle.</typeparam>
    /// <typeparam name="Edge">The type of data that connects each circle
    /// back to its root in this tree.</typeparam>
    public class CircleGeom<Node, Edge>
    {
        /// <summary><para>
        /// This data structure represents an individual arc in the convex
        /// hull of a circle tree instance.</para><para>
        /// It includes the arc's radius, its polar coordinates around the 
        /// center of its parent convex hull, and angles that represent its 
        /// endpoints around its own center.</para><para>
        /// Unless stated otherwise, all fields that store an angular value
        /// are in radians in the range [-π,π] ( [-180°,180°] ).
        /// </para></summary>
        public class CHArc
        {
            /// <summary>
            /// The <see cref="P:CircleTree`2{Node,Edge}.NodeData"/> of the 
            /// circle that formed this arc in its convex hull and/or the 
            /// convex hulls of its ancestors.
            /// </summary>
            public Node Data;
            /// <summary>
            /// The radius this arc.
            /// </summary>
            public double Rad;
            /// <summary>
            /// The distance between this arc's center and the center of
            /// its parent convex hull.
            /// </summary>
            public double Dst;
            /// <summary>
            /// The angle of the center of this arc around the center of its
            /// parent convex hull, measured in radians counterclockwise from
            /// the +X-axis of the local coordinate system of its parent 
            /// convex hull.</summary>
            public double Ang;
            /// <summary><para>
            /// The wedge between this arc's lower endpoint and the infinite
            /// ray from the center of its parent convex hull through its own
            /// center,</para><para>measured in radians clockwise from
            /// the latter ray around this arc's own center (not the center
            /// of its parent convex hull).</para></summary>
            public double LowerWedge;
            /// <summary><para>
            /// The wedge between this arc's upper endpoint and the infinite
            /// ray from the center of its parent convex hull through its own
            /// center,</para><para>measured in radians counterclockwise from
            /// the latter ray around this arc's own center (not the center 
            /// of its parent convex hull).</para></summary>
            public double UpperWedge;
            /// <summary><para>
            /// The angle between an infinite ray from the center of this
            /// arc's parent convex hull through a point intersecting this
            /// arc and the ray from the center of this arc's parent convex
            /// hull to its own center,</para><para>measured in radians 
            /// counterclockwise from the latter ray around the center of 
            /// this arc's parent convex hull.</para>
            /// Unlike all the other angular fields, this field has a range
            /// of [-2π,2π] ( [-360°,360°] ) instead of [-π,π] 
            /// ( [-180°,180°] ).<para></para></summary><remarks><para>
            /// This field is almost always zero, and it is really only used
            /// when there are two or more arcs with the same 
            /// <see cref="Ang"/> and <see cref="Dst"/>, but different
            /// <see cref="LowerWedge"/> and <see cref="UpperWedge"/> values
            /// as a result of being split by another arc. In this case,
            /// these two new arcs need to be differentiated from each other
            /// and have a way being arranged both before and after the arc
            /// that split them when sorted by their angle around the center
            /// of their parent convex hull.</para><para>
            /// This is why the arcs that form a convex hull are sorted by
            /// the sum of their <see cref="Ang"/> and this wedge offset,
            /// instead of just their <see cref="Ang"/>, which might not be
            /// unique.</para><para>
            /// This field is not needed when the arc's center is also the
            /// center of its parent convex hull (its <see cref="Dst"/> is
            /// zero), as the <see cref="Ang"/> values of the two new arcs
            /// resulting from a split can be set to be centered within
            /// their respective wedges.</para></remarks>
            /// <seealso cref="SortAngle"/>
            public double WedgeOffset;
            /// <summary>
            /// Creates a new arc with the given <see cref="Data"/> and 
            /// <see cref="Radius"/> and all its other fields set to zero.
            /// </summary>
            /// <param name="data">The data of the circle tree leaf/subtree
            /// that created this arc in the convex hull.</param>
            /// <param name="rad">The radius of this arc.</param>
            public CHArc(Node data, double rad)
            {
                this.Data = data;
                this.Rad = rad;
            }
            /// <summary>
            /// Creates a new arc with all its values equal to those of the
            /// given <paramref name="arc"/>.
            /// </summary>
            /// <param name="arc">The arc to copy.</param>
            public CHArc(CHArc arc)
            {
                this.Data = arc.Data;
                this.Rad = arc.Rad;
                this.Dst = arc.Dst;
                this.Ang = arc.Ang;
                this.LowerWedge = arc.LowerWedge;
                this.UpperWedge = arc.UpperWedge;
                this.WedgeOffset = arc.WedgeOffset;
            }
            /// <summary>
            /// The sum of this arc's <see cref="Ang"/> and 
            /// <see cref="WedgeOffset"/>, which is a unique value used
            /// to sort the arcs that form a convex hull. This value is 
            /// almost always equal to <see cref="Ang"/>, as
            /// <see cref="WedgeOffset"/> is almost always zero.
            /// </summary><seealso cref="WedgeOffset"/><seealso cref="Ang"/>
            public double ZSortAngle
            {
                get { return this.Ang + this.WedgeOffset; }
            }

            #region Debugging
            /// <summary>
            /// The <see cref="Ang"/> field in degrees 
            /// instead of radians for debugging.
            /// </summary>
            public double DegAng
            {
                get { return 180.0 * this.Ang / Math.PI; }
            }
            /// <summary>
            /// The <see cref="LowerWedge"/> field in degrees
            /// instead of radians for debugging.
            /// </summary>
            public double DegLowerWedge
            {
                get { return 180.0 * this.LowerWedge / Math.PI; }
            }
            /// <summary>
            /// The <see cref="UpperWedge"/> field in degrees
            /// instead of radians for debugging.
            /// </summary>
            public double DegUpperWedge
            {
                get { return 180.0 * this.UpperWedge / Math.PI; }
            }
            /// <summary>
            /// The <see cref="WedgeOffset"/> field in degrees
            /// instead of radians for debugging.
            /// </summary>
            public double DegWedgeOffset
            {
                get { return 180.0 * this.WedgeOffset / Math.PI; }
            }
            /// <summary>
            /// The <see cref="ZSortAngle"/> property in degrees
            /// instead of radians for debugging.
            /// </summary>
            public double DegZSortAngle
            {
                get 
                { 
                    return 180.0 * (this.Ang + this.WedgeOffset) / 
                        Math.PI; 
                }
            }
            /// <summary>
            /// Returns the results of the <see cref="Object.ToString()"/>
            /// method of the <see cref="Data"/> field for debugging.
            /// </summary>
            /// <returns>The <see cref="Data"/> field as a string.</returns>
            public override string ToString()
            {
                string str = "";
                try { str = this.Data.ToString(); }
                catch (NullReferenceException) { str = "{NULL}"; }
                return str;
            }
            #endregion
        }
        /// <summary>
        /// This comparer is used to sort an array of convex hull circles
        /// by decreasing radius.
        /// </summary>
        private class CHRadComp : IComparer<CHArc>
        {
            /// <summary>
            /// Static singleton instance of this comparer,
            /// used to conserve memory.
            /// </summary>
            public static CHRadComp S = new CHRadComp();

            public int Compare(CHArc x, CHArc y)
            {
                return x.Rad == y.Rad
                    ? (x.Dst == y.Dst ? 0 : (x.Dst > y.Dst ? -1 : 1))
                    : (x.Rad > y.Rad ? -1 : 1);/* */
                /*return (x.Rad == y.Rad ? 0 : (x.Rad > y.Rad ? -2 : 2))
                     + (x.Dst == y.Dst ? 0 : (x.Dst > y.Dst ? -1 : 1));/* */
            }
        }
        /// <summary>
        /// This comparer is used to sort an array of convex hull arcs
        /// by increasing sort angle (angle + wedge offset).
        /// </summary>
        private class CHAngComp : IComparer<CHArc>
        {
            private double xAng;
            private double yAng;

            public int Compare(CHArc x, CHArc y)
            {
                xAng = x.Ang + x.WedgeOffset;
                yAng = y.Ang + y.WedgeOffset;
                return xAng == yAng ? 0 : (xAng > yAng ? 1 : -1);
            }
        }
        private CHAngComp mCHAngComp;

        #region Fields and Constructors
        private GTree<Node, Edge, CircleGeom<Node, Edge>> mOwner;
        //private Node mNodeData;
        //private Edge mEdgeData;

        /*private CircleTree<Node, Edge> mRoot;
        private int mIndex;
        private CircleTree<Node, Edge>[] mBranches;
        private int mBCount;/* */

        private double mRad;
        private double mDst;
        private double mAng;

        private uint mLastBVers;
        private bool bCHDirty;
        private CHArc[] mConvexHull;
        private CHArc mCHCircle;

        private bool bWedgeDirty;
        private double mUpperWedge;
        private double mLowerWedge;
        private double mUpperLength;
        private double mLowerLength;

        /*/// <summary>
        /// Creates a new circle tree with the given <paramref name="nData"/>
        /// that has a circle of the given <paramref name="radius"/> at its 
        /// center.</summary>
        /// <param name="nData">The <see cref="NodeData"/> stored in this 
        /// circle tree instance.</param>
        /// <param name="eData">The <see cref="EdgeData"/> stored in this
        /// circle tree instance.</param>
        /// <param name="radius">The radius of the circle at the center
        /// of the new circle tree.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="radius"/> is less than or equal to zero.
        /// </exception>
        public CircleTree(Node nData, Edge eData, double radius)
            : this(nData, eData, radius, 0)
        {
        }/* */
        /// <summary>
        /// Creates a new circle tree with the given <paramref name="nData"/>
        /// that has a circle of the given <paramref name="radius"/> at its 
        /// center and the specified initial capacity for storing its 
        /// branches.</summary>
        /// <param name="nData">The <see cref="NodeData"/> stored in this 
        /// circle tree instance.</param>
        /// <param name="eData">The <see cref="EdgeData"/> stored in this
        /// circle tree instance.</param>
        /// <param name="radius">The radius of the circle at the center
        /// of the new circle tree.</param>
        /// <param name="capacity">The number of branches that the new
        /// circle tree can initially store without resizing.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="radius"/> is less than or equal to zero or 
        /// <paramref name="capacity"/> is less than zero.</exception>
        public CircleGeom(double radius)
        //public CircleTree(Node nData, Edge eData, double radius, int capacity)
        {
            if (radius <= 0.0)
                throw new ArgumentOutOfRangeException("radius");
            /*if (capacity < 0)
                throw new ArgumentOutOfRangeException("capacity");

            this.mNodeData = nData;
            this.mEdgeData = eData;

            this.mRoot = null;
            this.mIndex = -1;
            this.mBranches = new CircleTree<Node, Edge>[capacity];
            this.mBCount = 0;/* */

            this.mRad = radius;
            this.mDst = 0.0;
            this.mAng = 0.0;

            this.bCHDirty = true;
            this.mConvexHull = new CHArc[0];
            this.mCHCircle = null;

            this.bWedgeDirty = true;
            this.mUpperWedge = 0.0;
            this.mLowerWedge = 0.0;
            this.mUpperLength = 0.0;
            this.mLowerLength = 0.0;
        }
        #endregion

        /*private void InvalidateParent()
        {
            CircleTree<Node, Edge> p = this.mRoot;
            while (p != null && !p.bCHDirty)
            {
                p.bCHDirty = true;
                p = p.mRoot;
            }
        }/* */

        private void InvalidateParent()
        {
            GTree<Node, Edge, CircleGeom<Node, Edge>> p = this.mOwner.Root;
            while (p != null && !p.GeomData.bCHDirty)
            {
                p.GeomData.bCHDirty = true;
                p = p.Root;
            }
        }

        public GTree<Node, Edge, CircleGeom<Node, Edge>> Owner
        {
            get { return this.mOwner; }
        }

        public void SetOwner(GTree<Node, Edge, CircleGeom<Node, Edge>> owner)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");
            if (this.mOwner != owner)
            {
                if (owner.GeomData != this)
                {
                    throw new ArgumentException(
                        "Owner must have this instance as its GeomData", 
                        "owner");
                }
                this.mOwner = owner;
                this.mLastBVers = owner.BranchVersion;
                this.bCHDirty = true;
                this.bWedgeDirty = true;
                this.InvalidateParent();
            }
        }

        /*/// <summary>
        /// The node data stored in this circle tree instance, which usually
        /// represents something inside the circle at the center of this
        /// circle tree.</summary>
        public Node NodeData
        {
            get { return this.mNodeData; }
        }
        /// <summary>
        /// The edge data stored in this circle tree instance, which usually
        /// represents something that connects this circle to its
        /// <see cref="Root"/>.</summary>
        public Edge EdgeData
        {
            get { return this.mEdgeData; }
        }

        #region Tree Properties
        /// <summary>
        /// Gets the root (parent) of this circle tree, which is invalidated
        /// (along with its root through the ancestry chain) every time this
        /// circle is invalidated or its <see cref="Distance"/> or
        /// <see cref="Angle"/> is changed.</summary>
        public CircleTree<Node, Edge> Root
        {
            get { return this.mRoot; }
        }
        /// <summary>
        /// Sets the <see cref="Root"/> of this circle tree to the given
        /// <paramref name="root"/> value. If this circle tree's old root
        /// isn't null, this circle tree is removed from its 
        /// <see cref="Branches"/> and it is invalidated. If the new 
        /// <paramref name="root"/> is not null, this circle tree
        /// is then added to its <see cref="Branches"/> and it is then
        /// invalidated as well.</summary>
        /// <param name="root">The new <see cref="Root"/> 
        /// of this circle tree.</param>
        public void SetRoot(CircleTree<Node, Edge> root)
        {
            if (this.mRoot != root)
            {
                CircleTree<Node, Edge> p;
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
                    this.mIndex = -1;
                    this.InvalidateParent();
                }
                this.mRoot = root;
                if (this.mRoot != null)
                {
                    p = this.mRoot;
                    if (p.mBCount == p.mBranches.Length)
                    {
                        if (p.mBCount == 0)
                        {
                            p.mBranches = new CircleTree<Node, Edge>[4];
                        }
                        else
                        {
                            CircleTree<Node, Edge>[] children
                                = new CircleTree<Node, Edge>[2 * p.mBCount];
                            Array.Copy(p.mBranches, 0,
                                children, 0, p.mBCount);
                            p.mBranches = children;
                        }
                    }
                    this.mIndex = p.mBCount;
                    p.mBranches[p.mBCount++] = this;
                    this.InvalidateParent();
                }
            }
        }
        /// <summary>
        /// Returns the index of this circle in the <see cref="Branches"/>
        /// of its <see cref="Root"/>, or -1 if this circle tree doesn't
        /// have a root.</summary>
        public int BranchIndex
        {
            get { return this.mIndex; }
        }
        /// <summary>
        /// Gets an array containing all circle trees instances that
        /// currently have this circle tree as their <see cref="Root"/>.
        /// </summary><remarks>
        /// Whenever any of these branches are invalidated or their
        /// <see cref="Distance"/> or <see cref="Angle"/> are changed,
        /// this circle tree and all of its ancestors are also invalidated,
        /// which means their <see cref="ConvexHull"/>s will be
        /// recalculated.</remarks>
        public CircleTree<Node, Edge>[] Branches
        {
            get
            {
                CircleTree<Node, Edge>[] branches
                    = new CircleTree<Node, Edge>[this.mBCount];
                if (this.mBCount > 0)
                    Array.Copy(this.mBranches, 0, branches, 0, this.mBCount);
                return branches;
            }
        }
        /// <summary>
        /// Gets the number of <see cref="Branches"/>
        /// that this circle tree currently has.
        /// </summary>
        public int BranchCount
        {
            get { return this.mBCount; }
        }
        /// <summary>
        /// Gets or sets the maximum number of <see cref="Branches"/>
        /// that this tree is currently capable of holding without
        /// resizing its internal array.
        /// </summary>
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
                    throw new ArgumentOutOfRangeException("Capacity");
                if (value != this.mBranches.Length)
                {
                    CircleTree<Node, Edge>[] branches
                        = new CircleTree<Node, Edge>[value];
                    if (this.mBCount > 0)
                    {
                        Array.Copy(this.mBranches, 0, 
                            branches, 0, this.mBCount);
                    }
                    this.mBranches = branches;
                }
            }
        }
        #endregion
        /* */

        #region Circle Properties
        /// <summary>
        /// The radius of the circle at the center of this circle tree.
        /// </summary><exception cref="ArgumentOutOfRangeException">
        /// <see cref="Radius"/> is set to a value that is less than or 
        /// equal to zero.</exception><remarks>
        /// Changing this value invalidates the <see cref="ConvexHull"/>
        /// of this tree and all its ancestors, causing them to be
        /// recalculated the next time their values are retrieved.
        /// </remarks>
        public double Radius
        {
            get { return this.mRad; }
            set
            {
                if (value <= 0.0)
                    throw new ArgumentOutOfRangeException("Radius");
                if (this.mRad != value)
                {
                    this.mRad = value;
                    this.bCHDirty = true;
                    this.InvalidateParent();
                }
            }
        }
        /// <summary>
        /// The distance between the centers of this circle tree and its 
        /// <see cref="Root"/>. Attempting to set a negative value causes
        /// the distance to be set to the inverse value and the 
        /// <see cref="Angle"/> to be increased by π radians (180°).
        /// </summary><remarks><para>
        /// This value is part of the polar coordinates that define the
        /// position of this circle tree.</para><para>
        /// Changing this value invalidates the <see cref="ConvexHull"/>
        /// of all the ancestors of this tree, causing them to be
        /// recalculated the next time their values are retrieved.
        /// </para></remarks>
        public double Distance
        {
            get { return this.mDst; }
            set
            {
                if (this.mDst != value)
                {
                    if (value < 0.0)
                    {
                        this.mDst = -value;
                        if (this.mAng < 0.0)
                            this.mAng += Math.PI;
                        else
                            this.mAng -= Math.PI;
                    }
                    else
                    {
                        this.mDst = value;
                    }
                    this.bWedgeDirty = true;
                    this.InvalidateParent();
                }
            }
        }
        /// <summary>
        /// The positional angle of this circle tree's center around the 
        /// center of its <see cref="Root"/> in radians measured 
        /// counterclockwise from the +X-axis of local coordinate system of
        /// <see cref="Root"/>. This value is normalized to within the range 
        /// [-π,π] ([-180°,180°]).</summary><remarks><para>
        /// This value is part of the polar coordinates that define the
        /// position of this circle tree.</para><para>
        /// Changing this value invalidates the <see cref="ConvexHull"/>
        /// of all the ancestors of this tree, causing them to be
        /// recalculated the next time their values are retrieved.
        /// </para></remarks>
        public double Angle
        {
            get { return this.mAng; }
            set
            {
                if (this.mAng != value)
                {
                    this.mAng = value;
                    while (this.mAng < -Math.PI)
                        this.mAng += 2 * Math.PI;
                    while (this.mAng > Math.PI)
                        this.mAng -= 2 * Math.PI;
                    this.InvalidateParent();
                }
            }
        }
        #endregion

        #region Convex Hull
        /// <summary>
        /// Gets whether this circle tree's <see cref="ConvexHull"/> will be
        /// recalculated the next time it is retrieved because either its
        /// <see cref="Radius"/> or <see cref="Branches"/> have been changed
        /// since the last time its convex hull was calculated.
        /// </summary>
        public bool ConvexHullDirty
        {
            get 
            {
                if (this.mLastBVers != this.mOwner.BranchVersion)
                {
                    this.mLastBVers = this.mOwner.BranchVersion;
                    this.bCHDirty = true;
                    this.InvalidateParent();
                }
                return this.bCHDirty; 
            }
        }
        /// <summary>
        /// The convex hull that encloses the circle at the center of this
        /// circle tree and all the circles on all the branches of this
        /// circle tree, ordered by increasing <see cref="CHArc.ZSortAngle"/>.
        /// </summary><remarks>
        /// This forces the circle tree to recalculate its convex hull 
        /// if it hasn't done so already or if its <see cref="Radius"/> or
        /// branches have been changed since the last time its convex hull
        /// was calculated.
        /// </remarks>
        public CHArc[] ConvexHull
        {
            get
            {
                if (this.ConvexHullDirty)
                    this.CalculateConvexHull();
                return this.mConvexHull;
            }
        }/* */
        /// <summary>
        /// The bounding circle of the convex hull of this circle tree, 
        /// which is the smallest circle that can completely enclose this 
        /// circle tree's <see cref="ConvexHull"/>.
        /// </summary><remarks>
        /// This forces the circle tree to recalculate its convex hull 
        /// if it hasn't done so already or if its <see cref="Radius"/> or
        /// branches have been changed since the last time its convex hull
        /// was calculated.
        /// </remarks>
        public CHArc ConvexHullBoundingCircle
        {
            get
            {
                if (this.ConvexHullDirty)
                    this.CalculateConvexHull();
                return new CHArc(this.mCHCircle);
            }
        }
        /// <summary>
        /// Forces this circle tree to calculate its <see cref="ConvexHull"/>
        /// and causes all of its branches which have been invalidated to 
        /// also calculate their convex hulls.</summary><remarks><para>
        /// This algorithm is based on the following scientific article,
        /// which helped a lot in figuring out how to implement it:
        /// </para><para>
        /// Incremental Algorithms for Finding the Convex Hulls of Circles 
        /// and the Lower Envelopes of Parabolas </para><para> 
        /// Olivier Devillers, Mordecai J Golin </para><para>
        /// Information Processing Letters 56 (3), 157-164, 1995</para><para>
        /// http://hal.archives-ouvertes.fr/docs/00/41/31/63/PDF/dg-iafch-95.pdf
        /// </para></remarks>
        public void CalculateConvexHull()
        {
            #region Initialization and Quick Completions
            // TODO: Will any of these calculations be FUBAR'd if the
            // radius, distance, or angle for any of the circles are
            // really really small?
            //if (this.mBCount == 0)
            if (this.mOwner.BranchCount == 0)
            {
                //this.mCHCircle = new CHArc(this.mNodeData, this.mRad);
                this.mCHCircle 
                    = new CHArc(this.mOwner.NodeData, this.mRad);
                this.mCHCircle.LowerWedge = Math.PI;
                this.mCHCircle.UpperWedge = Math.PI;
                this.mConvexHull = new CHArc[] { this.mCHCircle };
                return;
            }
#if DEBUG
            string cvHullStr;
#endif
            bool split1, split3;
            int i, j, i1, i3, root, len = 1;
            double a1, a2, hyp, ch3LoW, ch1UpW;
            CHArc ch1 = null;
            CHArc ch2 = null;
            CHArc ch3 = null;
            CircleGeom<Node, Edge> child;
            GTree<Node, Edge, CircleGeom<Node, Edge>>[] branches
                = this.mOwner.Branches;
            // Temp holder for child convex hulls, sorted by
            // decreasing radius (largest first)
            CHArc[] chByRad;
            //for (i = 0; i < this.mBCount; i++)
            for (i = 0; i < branches.Length; i++)
            {
                //child = this.mBranches[i];
                child = branches[i].GeomData;
                if (child.ConvexHullDirty)
                    child.CalculateConvexHull();
                chByRad = child.mConvexHull;
                len += chByRad.Length;
            }
            chByRad = new CHArc[len];
            //chByRad[0] = new CHArc(this.mNodeData, this.mRad);
            chByRad[0] = new CHArc(this.mOwner.NodeData, this.mRad);
            chByRad[0].Ang = 16.0;
            len = 1;
            //for (i = 0; i < this.mBCount; i++)
            for (i = 0; i < branches.Length; i++)
            {
                //child = this.mBranches[i];
                child = branches[i].GeomData;
                for (j = 0; j < child.mConvexHull.Length; j++)
                {
                    ch1 = child.mConvexHull[j];
                    ch2 = new CHArc(ch1.Data, ch1.Rad);
                    a1 = Math.PI - Math.Abs(ch1.Ang);
                    ch2.Dst = Math.Sqrt(child.mDst * child.mDst
                        + ch1.Dst * ch1.Dst
                        - 2 * child.mDst * ch1.Dst * Math.Cos(a1));
                    a1 = Math.Asin(ch1.Dst * Math.Sin(a1) / ch2.Dst);
                    ch2.Ang = ch1.Ang < 0.0
                        ? child.mAng - a1 : child.mAng + a1;
                    while (ch2.Ang < -Math.PI)
                        ch2.Ang += 2 * Math.PI;
                    while (ch2.Ang > Math.PI)
                        ch2.Ang -= 2 * Math.PI;
                    //chByRad[len++] = ch2;
                    i1 = len - 1;
                    while (i1 >= 0 && chByRad[i1].Rad < ch2.Rad)
                    {
                        i1--;
                    }
                    while (i1 >= 0 && chByRad[i1].Rad == ch2.Rad &&
                                      chByRad[i1].Dst < ch2.Dst)
                    {
                        i1--;
                    }
                    i1++;
                    Array.Copy(chByRad, i1, chByRad, i1 + 1, len - i1);
                    chByRad[i1] = ch2;
                    len++;
                }
            }
            // Sort the list of arcs by decreasing radius and distance
            //Array.Sort<CHArc>(chByRad, 0, len, CHRadComp.S);
            // Find the root's index
            root = -1;
            for (i = 0; i < len && root < 0; i++)
            {
                if (chByRad[i].Ang == 16.0)
                    root = i;
            }
            // If the root isn't zero, check if it should be and make it zero
            ch1 = chByRad[0];
            if (root != 0 && ch1.Rad == chByRad[root].Rad)
            {
                ch1 = chByRad[root];
                Array.Copy(chByRad, 0, chByRad, 1, root);
                chByRad[0] = ch1;
                root = 0;
            }
            // Offset the circles so that the largest one is centered
            if (root == 0)
            {
                ch1.Ang = 0.0;
            }
            else
            {
                double x1, y1;
                a1 = ch1.Dst * Math.Cos(ch1.Ang);
                a2 = ch1.Dst * Math.Sin(ch1.Ang);
                for (i = 1; i < len; i++)
                {
                    ch2 = chByRad[i];
                    x1 = ch2.Dst * Math.Cos(ch2.Ang) - a1;
                    y1 = ch2.Dst * Math.Sin(ch2.Ang) - a2;
                    ch2.Dst = Math.Sqrt(x1 * x1 + y1 * y1);
                    ch2.Ang = Math.Atan2(y1, x1);
                }
                ch1.Ang = ch1.Dst = 0.0;
            }
            // Find the first circle intersecting or outside the
            // largest one
            for (i = 1; i < len; i++)
            {
                ch2 = chByRad[i];
                if (ch2.Dst + ch2.Rad > ch1.Rad)
                    break;
            }
            if (i == len)
            {
                // All other circles are inside the largest one
                if (root == 0)
                {
                    ch1.LowerWedge = ch1.UpperWedge = Math.PI;
                    this.mConvexHull = new CHArc[] { ch1 };
                    this.mCHCircle = ch1;
                }
                else
                {
                    ch1.Ang = ch1.Ang < 0.0
                        ? ch1.Ang + Math.PI : ch1.Ang - Math.PI;
                    ch1.LowerWedge = Math.PI;
                    ch1.UpperWedge = Math.PI;
                    this.mConvexHull = new CHArc[] { ch1 };
                    this.mCHCircle = ch1;
                }
                return;
            }
            #endregion

            #region Main Computation Loop
            // The final convex hull, sorted counterclockwise
            // by angle (-PI to PI).
            // TODO: Find the optimal length for this.
            // The final convex hull will have no more than
            // 2 * len - 1 arcs, but extra length might be 
            // needed for Array.Copy operations.
            CHArc[] cvHull = new CHArc[(len << 1) + (len >> 1)];
            // angle between line connecting their centers
            // and ch1's radial line tangent to their tube
            // boundary line
            a1 = Math.Acos((ch1.Rad - ch2.Rad) / ch2.Dst);
            ch1.LowerWedge = Math.PI - a1;
            ch1.UpperWedge = Math.PI - a1;
            ch2.LowerWedge = a1;
            ch2.UpperWedge = a1;
            if (ch2.Ang < 0.0)
            {
                ch1.Ang = ch2.Ang + Math.PI;
                cvHull[0] = ch2;
                cvHull[1] = ch1;
            }
            else
            {
                ch1.Ang = ch2.Ang - Math.PI;
                cvHull[0] = ch1;
                cvHull[1] = ch2;
            }
            len = 2;
            // Add further circles to the convex hull.
            for (i++; i < chByRad.Length; i++)
            {
#if DEBUG
                cvHullStr = PrintConvexHull(cvHull, len);
#endif
                #region Initialization and Fast Concavity Tests
                ch2 = chByRad[i];
                // Within largest circle, so within convex hull
                if (ch2.Dst + ch2.Rad <= chByRad[0].Rad)
                    continue;
                // Find the two arcs the circle lies between.
                // TODO: Does WedgeOffset fully compensate for multiple 
                // arcs with the same angle but different wedges?
                // TODO: Would WedgeOffset ever FUBAR this algorithm
                // in any case where it causes i1 == i3 ?
                a1 = cvHull[0].Ang + cvHull[0].WedgeOffset;
                a2 = cvHull[len - 1].Ang + cvHull[len - 1].WedgeOffset;
                if (ch2.Ang < a1 || ch2.Ang > a2)
                {
                    i1 = len - 1;
                    i3 = 0;
                }
                else if (ch2.Ang == a1)
                {
                    i1 = i3 = 0;
                }
                else if (ch2.Ang == a2)
                {
                    i1 = i3 = len - 1;
                }
                else
                {
                    // Binary search where j is median
                    i1 = 0;
                    i3 = len - 1;
                    while (i3 - i1 > 1)
                    {
                        j = i1 + ((i3 - i1) >> 1);
                        a1 = cvHull[j].Ang + cvHull[j].WedgeOffset;
                        if (ch2.Ang < a1)
                            i3 = j;
                        else if (ch2.Ang > a1)
                            i1 = j;
                        else
                            i1 = i3 = j;
                    }
                }
                ch1 = cvHull[i1];
                ch3 = cvHull[i3];

                // Perform fast concavity tests prior to split tests
                // and more complex wedge-based concavity tests
                if (ch1.Dst == 0.0 && ch2.Dst + ch2.Rad <= ch1.Rad)
                {
                    // ch2's circle is inside ch1's circle
                    continue;
                }
                else if (ch1.Dst > 0.0)
                {
                    // TODO: Ensure this subtract always 
                    // works correctly and efficiently
                    /*a1 = ch2.Ang - ch1.Ang;
                    while (a1 < -Math.PI)
                        a1 += 2 * Math.PI;
                    while (a1 > Math.PI)
                        a1 -= 2 * Math.PI;/* */
                    a1 = Math.Abs(ch2.Ang - ch1.Ang);
                    if (a1 > Math.PI)
                        a1 = 2 * Math.PI - a1;/* */
                    if (a1 == 0.0 &&
                        ch2.Dst + ch2.Rad <= ch1.Dst + ch1.Rad)
                    {
                        // ch2's circle is inside ch1's circle
                        // or it's inside the convex hull itself
                        continue;
                    }
                    // length of line connecting ch2 and ch1
                    hyp = Math.Sqrt(ch2.Dst * ch2.Dst
                        + ch1.Dst * ch1.Dst
                        - 2 * ch2.Dst * ch1.Dst * Math.Cos(a1));
                    if (hyp + ch2.Rad <= ch1.Rad)
                    {
                        // ch2's circle is inside ch1's circle
                        continue;
                    }
                }
                if (i3 != i1)
                {
                    if (ch3.Dst == 0.0 && ch2.Dst + ch2.Rad <= ch3.Rad)
                    {
                        // ch2's circle is inside ch3's circle
                        continue;
                    }
                    else if (ch3.Dst > 0.0)
                    {
                        // TODO: Ensure this subtract always 
                        // works correctly and efficiently
                        a1 = ch3.Ang - ch2.Ang;
                        while (a1 < -Math.PI)
                            a1 += 2 * Math.PI;
                        while (a1 > Math.PI)
                            a1 -= 2 * Math.PI;/* */
                        /*a1 = Math.Abs(ch3.Ang - ch2.Ang);
                        if (a1 > Math.PI)
                            a1 = 2 * Math.PI - a1;/* */
                        if (a1 == 0.0 &&
                            ch2.Dst + ch2.Rad <= ch3.Dst + ch3.Rad)
                        {
                            // ch2's circle is inside ch3's circle
                            // or it's inside the convex hull itself
                            continue;
                        }
                        // length of line connecting ch2 and ch3
                        hyp = Math.Sqrt(ch2.Dst * ch2.Dst
                            + ch3.Dst * ch3.Dst
                            - 2 * ch2.Dst * ch3.Dst * Math.Cos(a1));
                        if (hyp + ch2.Rad < ch3.Rad)
                        {
                            // ch2's circle is inside ch3's circle
                            continue;
                        }
                        if (ch1.Dst > 0.0)
                        {
                            // angle between line connecting ch2 and ch3
                            // and line connecting ch3 to the CH center
                            a2 = (hyp * hyp + ch3.Dst * ch3.Dst
                                - ch2.Dst * ch2.Dst) / (2 * hyp * ch3.Dst);
                            a2 = Math.Acos(//a2);
                                a2 > 1.0 ? 1.0 : (a2 < -1.0 ? -1.0 : a2));
                            if (a1 < 0.0)
                                a2 = 2 * Math.PI - a2;

                            a1 = ch3.Ang - ch1.Ang;
                            // length of line connecting ch1 and ch3
                            hyp = Math.Sqrt(ch1.Dst * ch1.Dst
                                + ch3.Dst * ch3.Dst
                                - 2 * ch1.Dst * ch3.Dst * Math.Cos(a1));
                            // angle between line connecting ch1 and ch3
                            // and line connecting ch3 to the CH center
                            a1 = (hyp * hyp + ch3.Dst * ch3.Dst
                                - ch1.Dst * ch1.Dst) / (2 * hyp * ch3.Dst);
                            a1 = Math.Acos(//a1);
                                a1 > 1.0 ? 1.0 : (a1 < -1.0 ? -1.0 : a1));
                            if (a2 <= a1)
                            {
                                // ch2's circle is inside the triangle
                                // formed by ch1, ch3 and the CH center
                                continue;
                            }
                        }
                    }
                }
                #endregion

                #region Counterclockwise Split Test
                // Test if circle splits the ch1 arc
                split1 = false;
                if (ch1.Dst == 0.0)
                {
                    // angle between line connecting ch2 and ch1
                    // and ch1's radial line tangent to their tube
                    ch2.UpperWedge 
                        = Math.Acos((ch1.Rad - ch2.Rad) / ch2.Dst);
#if DEBUG
                    if (double.IsNaN(ch2.UpperWedge))
                        throw new Exception();
#endif
                    a1 = ch2.Ang + ch2.UpperWedge;
                    a2 = ch1.Ang + ch1.UpperWedge;
                    // TODO: Is normalization of a1 & a2 needed?
                    if (ch1.Ang > ch2.Ang)
                        a2 -= 2 * Math.PI;

                    // Test if ch2's shadow is within the ch1 arc
                    if (a1 < a2)
                    {
                        // Potential new ch1.UpperWedge [-PI,PI]
                        a1 = (ch2.Ang + ch2.UpperWedge) - ch1.Ang;
                        while (a1 < -Math.PI)
                            a1 += 2 * Math.PI;
                        while (a1 > Math.PI)
                            a1 -= 2 * Math.PI;
                        // Upper Wedge of copy is only needed for initial
                        // concavity test, and will be reset or deleted
                        ch3 = new CHArc(ch1);
                        // Shrink ch1 to outside ch2's shadow
                        ch1.LowerWedge = (ch1.UpperWedge - a1) / 2.0;
                        ch1.Ang += a1 + ch1.LowerWedge;
                        ch1.UpperWedge = ch1.LowerWedge;
                        // Insert copy before ch1; i1 should point to copy
                        Array.Copy(cvHull, i1, cvHull, i1 + 1, len - i1);
                        cvHull[i1] = ch3;
                        len++;
                        // ..., copy1, ch13, ...
                        // ..., copy1, ch1, ch3, ...
                        // ch3, ..., copy1, ch1
                        i3 = i1 <= i3 ? i3 + 1 : i3;
                        // TODO: Will this only happen if i1 == len - 1 ?
                        if (ch1.Ang > Math.PI)
                        {
#if DEBUG
                            if (i1 != len - 2) throw new Exception();
#endif
                            ch1.Ang -= 2 * Math.PI;
                            Array.Copy(cvHull, 0, cvHull, 1, len);
                            cvHull[0] = ch1;
                            i1 = (i1 + 1) % len;
                            i3 = (i3 + 1) % len;
                            // ch13, ..., copy1
                            // ch1, ch3, ..., copy1
                        }
                        ch1 = ch3;
                        ch3 = cvHull[i3];
                        split1 = true;
                    }
                }
                else
                {
                    // TODO: Ensure this subtract always 
                    // works correctly and efficiently
                    a2 = ch2.Ang - ch1.Ang;
                    while (a2 < -Math.PI)
                        a2 += 2 * Math.PI;
                    while (a2 > Math.PI)
                        a2 -= 2 * Math.PI;/* */
                    /*a2 = Math.Abs(ch2.Ang - ch1.Ang);
                    if (a2 > Math.PI)
                        a2 = 2 * Math.PI - a2;/* */
                    if (a2 == 0.0)
                    {
                        ch2.UpperWedge = Math.Acos(
                            (ch1.Rad - ch2.Rad) /
                            (ch2.Dst - ch1.Dst));
#if DEBUG
                        if (double.IsNaN(ch2.UpperWedge))
                            throw new Exception();
#endif
                        // Potential new ch1.UpperWedge [-PI,PI]
                        a1 = ch2.UpperWedge;
                    }
                    else
                    {
                        // length of line connecting ch2 and ch1
                        hyp = Math.Sqrt(ch1.Dst * ch1.Dst
                            + ch2.Dst * ch2.Dst
                            - 2 * ch1.Dst * ch2.Dst * Math.Cos(a2));
                        // angle between line connecting ch2 and ch1
                        // and ch1's radial line tangent to their tube
                        a1 = Math.Acos((ch1.Rad - ch2.Rad) / hyp);
                        if (a2 > 0.0)
                        {
                            // previous angle plus
                            // angle between line connecting ch2 and ch1
                            // and line connecting ch2 to CH center
                            a2 = (hyp * hyp + ch2.Dst * ch2.Dst
                                - ch1.Dst * ch1.Dst) / (2.0 * hyp * ch2.Dst);
                            ch2.UpperWedge = a1 + Math.Acos(//a2);
                                a2 > 1.0 ? 1.0 : (a2 < -1.0 ? -1.0 : a2));
#if DEBUG
                            if (double.IsNaN(ch2.UpperWedge))
                                throw new Exception();
#endif
                            // angle between line connecting ch2 and ch1
                            // and line connecting ch1 to CH center
                            a2 = (hyp * hyp + ch1.Dst * ch1.Dst
                                - ch2.Dst * ch2.Dst) / (2.0 * hyp * ch1.Dst);
                            a2 = Math.Acos(//a2);
                                a2 > 1.0 ? 1.0 : (a2 < -1.0 ? -1.0 : a2));
                        }
                        else
                        {
                            // previous angle minus
                            // angle between line connecting ch2 and ch1
                            // and line connecting ch2 to CH center
                            a2 = (hyp * hyp + ch2.Dst * ch2.Dst
                                - ch1.Dst * ch1.Dst) / (2.0 * hyp * ch2.Dst);
                            ch2.UpperWedge = a1 - Math.Acos(//a2);
                                a2 > 1.0 ? 1.0 : (a2 < -1.0 ? -1.0 : a2));
#if DEBUG
                            if (double.IsNaN(ch2.UpperWedge))
                                throw new Exception();
#endif
                            // angle between line connecting ch2 and ch1
                            // and line connecting ch1 to CH center
                            a2 = (hyp * hyp + ch1.Dst * ch1.Dst
                                - ch2.Dst * ch2.Dst) / (2.0 * hyp * ch1.Dst);
                            a2 = 2 * Math.PI - Math.Acos(//a2);
                                a2 > 1.0 ? 1.0 : (a2 < -1.0 ? -1.0 : a2));
                        }
                        // Potential new ch1.UpperWedge [-PI,PI]
                        a1 += Math.PI - a2;
                    }
                    // Test if ch2's shadow is within the ch1 arc
                    if (a1 < ch1.UpperWedge && -a1 < ch1.LowerWedge)
                    {
                        // Upper Wedge of copy is only needed for initial
                        // concavity test, and will be reset or deleted
                        ch3 = new CHArc(ch1);
                        // Shrink ch1 to outside ch2's shadow
                        ch1.LowerWedge = -a1;
#if DEBUG
                        if (double.IsNaN(ch1.LowerWedge))
                            throw new Exception();
#endif
                        // absolute value of angle of midpoint of ch1
                        a1 = ch1.LowerWedge < 0.0
                            ? (ch1.UpperWedge - ch1.LowerWedge) / 2.0
                            : (ch1.LowerWedge - ch1.UpperWedge) / 2.0;
                        // length of line connecting midpoint of ch1
                        // to CH center
                        hyp = Math.Sqrt(ch1.Dst * ch1.Dst
                            + ch1.Rad * ch1.Rad
                            - 2 * ch1.Dst * ch1.Rad * Math.Cos(a1));
                        // angle between line connecting ch1 to CH center
                        // & line connecting midpoint of ch1 to CH center
                        a1 = Math.Asin(ch1.Rad * Math.Sin(a1) / hyp);
                        if (ch1.LowerWedge < 0.0)
                            ch1.WedgeOffset = a1;
                        else
                            ch1.WedgeOffset = -a1;
#if DEBUG
                        if (double.IsNaN(ch1.WedgeOffset))
                            throw new Exception();
#endif
                        // Insert copy before ch1; i1 should point to copy
                        Array.Copy(cvHull, i1, cvHull, i1 + 1, len - i1);
                        cvHull[i1] = ch3;
                        len++;
                        // ..., copy1, ch13, ...
                        // ..., copy1, ch1, ch3, ...
                        // ch3, ..., copy1, ch1
                        i3 = i1 <= i3 ? i3 + 1 : i3;
                        // TODO: Will this only happen if i1 == len - 1 ?
                        if (ch1.Ang + ch1.WedgeOffset > Math.PI)
                        {
#if DEBUG
                            if (i1 != len - 2) throw new Exception();
#endif
                            ch1.WedgeOffset -= 2 * Math.PI;
                            Array.Copy(cvHull, 0, cvHull, 1, len);
                            cvHull[0] = ch1;
                            i1 = (i1 + 1) % len;
                            i3 = (i3 + 1) % len;
                            // ch13, ..., copy1
                            // ch1, ch3, ..., copy1
                        }
                        ch1 = ch3;
                        ch3 = cvHull[i3];
                        split1 = true;
                    }
                }
                #endregion

                #region Clockwise Split Test
                // Test if circle splits the ch3 arc
                split3 = false;
                if (ch3.Dst == 0.0)
                {
                    // angle between line connecting ch2 and ch3
                    // and ch3's radial line tangent to their tube
                    ch2.LowerWedge 
                        = Math.Acos((ch3.Rad - ch2.Rad) / ch2.Dst);
#if DEBUG
                    if (double.IsNaN(ch2.LowerWedge))
                        throw new Exception();
#endif
                    a1 = ch2.Ang - ch2.LowerWedge;
                    a2 = ch3.Ang - ch3.LowerWedge;
                    // TODO: Is normalization of a1 & a2 needed?
                    if (ch3.Ang < ch2.Ang)
                        a2 += 2 * Math.PI;

                    // Test if ch2's shadow is within the ch3 arc
                    if (a2 < a1)
                    {
                        // Potential new ch3.LowerWedge [-PI,PI]
                        a1 = ch3.Ang - (ch2.Ang - ch2.LowerWedge);
                        while (a1 < -Math.PI)
                            a1 += 2 * Math.PI;
                        while (a1 > Math.PI)
                            a1 -= 2 * Math.PI;
                        // Lower Wedge of copy is only needed for initial
                        // concavity test, and will be reset or deleted
                        ch1 = new CHArc(ch3);
                        // Shrink ch3 to outside ch2's shadow
                        ch3.UpperWedge = (ch3.LowerWedge - a1) / 2.0;
                        ch3.Ang -= a1 + ch3.UpperWedge;
                        ch3.LowerWedge = ch3.UpperWedge;
                        // Insert copy after ch3; i3 should point to copy
                        i3++;
                        Array.Copy(cvHull, i3, cvHull, i3 + 1, len - i3);
                        cvHull[i3] = ch1;
                        len++;
                        //     ..., ch13, copy3, ...
                        // ..., ch1, ch3, copy3, ...
                        // ch3, copy3, ..., ch1
                        // ..., copy1, ch13, copy3, ...
                        // ..., copy1, ch1, ch3, copy3, ...
                        // ch13, copy3, ..., copy1
                        // ch1, ch3, copy3, ..., copy1
                        // ch3, copy3, ..., copy1, ch1
                        i1 = i3 <= i1 ? i1 + 1 : i1;
                        // TODO: Will this only happen if i3 == 0 ?
                        if (ch3.Ang < -Math.PI)
                        {
#if DEBUG
                            if (i3 != 1) throw new Exception();
#endif
                            ch3.Ang += 2 * Math.PI;
                            Array.Copy(cvHull, 1, cvHull, 0, len - 1);
                            cvHull[len - 1] = ch3;
                            i1 = (i1 - 1 + len) % len;
                            i3 = (i3 - 1 + len) % len;
                            // copy3, ..., ch13
                            // copy3, ..., ch1, ch3
                            // copy3, ..., copy1, ch13
                            // copy3, ..., copy1, ch1, ch3
                        }
                        ch3 = ch1;
                        ch1 = cvHull[i1];
                        split3 = true;
                    }
                }
                else
                {
                    // TODO: Ensure this subtract always 
                    // works correctly and efficiently
                    a2 = ch3.Ang - ch2.Ang;
                    while (a2 < -Math.PI)
                        a2 += 2 * Math.PI;
                    while (a2 > Math.PI)
                        a2 -= 2 * Math.PI;/* */
                    /*a2 = Math.Abs(ch3.Ang - ch2.Ang);
                    if (a2 > Math.PI)
                        a2 = 2 * Math.PI - a2;/* */
                    if (a2 == 0.0)
                    {
                        // angle between line connecting ch2 and ch3
                        // and ch3's radial line tangent to their tube
                        ch2.LowerWedge = Math.Acos(
                            (ch3.Rad - ch2.Rad) /
                            (ch2.Dst - ch3.Dst));
#if DEBUG
                        if (double.IsNaN(ch2.LowerWedge))
                            throw new Exception();
#endif
                        // Potential new ch3.LowerWedge [-PI,PI]
                        a1 = ch2.LowerWedge;
                    }
                    else
                    {
                        // length of line connecting ch2 and ch3
                        hyp = Math.Sqrt(ch3.Dst * ch3.Dst
                            + ch2.Dst * ch2.Dst
                            - 2 * ch3.Dst * ch2.Dst * Math.Cos(a2));
                        // angle between line connecting ch2 and ch3
                        // and ch3's radial line tangent to their tube
                        a1 = Math.Acos((ch3.Rad - ch2.Rad) / hyp);
                        if (a2 > 0.0)
                        {
                            // previous angle plus
                            // angle between line connecting ch2 and ch3
                            // and line connecting ch2 to CH center
                            a2 = (hyp * hyp + ch2.Dst * ch2.Dst
                                - ch3.Dst * ch3.Dst) / (2.0 * hyp * ch2.Dst);
                            ch2.LowerWedge = a1 + Math.Acos(//a2);
                                a2 > 1.0 ? 1.0 : (a2 < -1.0 ? -1.0 : a2));
#if DEBUG
                            if (double.IsNaN(ch2.LowerWedge))
                                throw new Exception();
#endif
                            // angle between line connecting ch2 and ch3
                            // and line connecting ch3 to CH center
                            a2 = (hyp * hyp + ch3.Dst * ch3.Dst
                                - ch2.Dst * ch2.Dst) / (2.0 * hyp * ch3.Dst);
                            a2 = Math.Acos(//a2);
                                a2 > 1.0 ? 1.0 : (a2 < -1.0 ? -1.0 : a2));
                        }
                        else
                        {
                            // previous angle minus
                            // angle between line connecting ch2 and ch3
                            // and line connecting ch2 to CH center
                            a2 = (hyp * hyp + ch2.Dst * ch2.Dst
                                - ch3.Dst * ch3.Dst) / (2.0 * hyp * ch2.Dst);
                            ch2.LowerWedge = a1 - Math.Acos(//a2);
                                a2 > 1.0 ? 1.0 : (a2 < -1.0 ? -1.0 : a2));
#if DEBUG
                            if (double.IsNaN(ch2.LowerWedge))
                                throw new Exception();
#endif
                            // angle between line connecting ch2 and ch3
                            // and line connecting ch3 to CH center
                            a2 = (hyp * hyp + ch3.Dst * ch3.Dst
                                - ch2.Dst * ch2.Dst) / (2.0 * hyp * ch3.Dst);
                            a2 = 2 * Math.PI - Math.Acos(//a2);
                                a2 > 1.0 ? 1.0 : (a2 < -1.0 ? -1.0 : a2));
                        }
                        // Potential new ch3.LowerWedge [-PI,PI]
                        a1 += Math.PI - a2;
                    }
                    // Test if ch2's shadow is within the ch3 arc
                    if (a1 < ch3.LowerWedge && -a1 < ch3.UpperWedge)
                    {
                        // Lower Wedge of copy is only needed for initial
                        // concavity test, and will be reset or deleted
                        ch1 = new CHArc(ch3);
                        // Shrink ch3 to outside ch2's shadow
                        ch3.UpperWedge = -a1;
#if DEBUG
                        if (double.IsNaN(ch3.UpperWedge))
                            throw new Exception();
#endif
                        // absolute value angle of midpoint of ch3
                        a1 = ch3.UpperWedge < 0.0
                            ? (ch3.LowerWedge - ch3.UpperWedge) / 2.0
                            : (ch3.UpperWedge - ch3.LowerWedge) / 2.0;
                        // length of line connecting midpoint of ch3
                        // to CH center
                        hyp = Math.Sqrt(ch3.Dst * ch3.Dst
                            + ch3.Rad * ch3.Rad
                            - 2 * ch3.Dst * ch3.Rad * Math.Cos(a1));
                        // angle between line connecting ch3 to CH center
                        // & line connecting midpoint of ch3 to CH center
                        a1 = Math.Asin(ch3.Rad * Math.Sin(a1) / hyp);
                        if (ch3.UpperWedge < 0.0)
                            ch3.WedgeOffset = -a1;
                        else
                            ch3.WedgeOffset = a1;
#if DEBUG
                        if (double.IsNaN(ch3.WedgeOffset))
                            throw new Exception();
#endif
                        // Insert copy after ch3; i3 should point to copy
                        i3++;
                        Array.Copy(cvHull, i3, cvHull, i3 + 1, len - i3);
                        cvHull[i3] = ch1;
                        len++;
                        //     ..., ch13, copy3, ...
                        // ..., ch1, ch3, copy3, ...
                        // ch3, copy3, ..., ch1
                        // ..., copy1, ch13, copy3, ...
                        // ..., copy1, ch1, ch3, copy3, ...
                        // ch13, copy3, ..., copy1
                        // ch1, ch3, copy3, ..., copy1
                        // ch3, copy3, ..., copy1, ch1
                        i1 = i3 <= i1 ? i1 + 1 : i1;
                        // TODO: Will this only happen if i3 == 0 ?
                        if (ch3.Ang + ch3.WedgeOffset < -Math.PI)
                        {
#if DEBUG
                            if (i3 != 1) throw new Exception();
#endif
                            ch1.WedgeOffset += 2 * Math.PI;
                            Array.Copy(cvHull, 1, cvHull, 0, len - 1);
                            cvHull[len - 1] = ch3;
                            i1 = (i1 - 1 + len) % len;
                            i3 = (i3 - 1 + len) % len;
                            // copy3, ..., ch13
                            // copy3, ..., ch1, ch3
                            // copy3, ..., copy1, ch13
                            // copy3, ..., copy1, ch1, ch3
                        }
                        ch3 = ch1;
                        ch1 = cvHull[i1];
                        split3 = true;
                    }
                }
                #endregion

                #region Split Test Post Processing
                // TODO: Does this only happen when i1 == i3 
                // before the split tests occur?
                if (split1 && split3)
                {
                    // Remove copies created by the splitting, because
                    // they're only needed for traversal in the direction
                    // opposite to each split, which won't happen now.
                    // i1/ch1 and i3/ch3 currently point to the copies
                    if (i1 < i3)
                    {
                        // ..., copy1, ch13, copy3, ...
                        // ..., copy1, ch1, ch3, copy3, ... 
                        Array.Copy(cvHull, i3 + 1, cvHull, i3, len - i3);
                        i3 = i3 - 2;
                        len--;
                        Array.Copy(cvHull, i1 + 1, cvHull, i1, len - i1);
                        // no need to change i1
                        len--;
                    }
                    else// if (i3 < i1)
                    {
                        // ch13, copy3, ..., copy1
                        // ch1, ch3, copy3, ..., copy1
                        // ch3, copy3, ..., copy1, ch1
                        // copy3, ..., copy1, ch13
                        // copy3, ..., copy1, ch1, ch3
                        Array.Copy(cvHull, i1 + 1, cvHull, i1, len - i1);
                        len--;
                        i1 = i1 == len ? 0 : i1;
                        Array.Copy(cvHull, i3 + 1, cvHull, i3, len - i3);
                        len--;
                        i3 = (i3 - 1 + len) % len;
                    }
                    ch1 = cvHull[i1];
                    ch3 = cvHull[i3];
                }
                else if (split1)
                {
                    // i1 points to the copy of ch1
                    // i3 should point to the original ch1, right?
                    // ..., copy1, ch13, ...
                    // ..., copy1, ch1, ch3, ...
                    // ch13, ..., copy1
                    // ch3, ..., copy1, ch1
                    i3 = (i1 + 1) % len;
                    ch3 = cvHull[i3];
                }
                else if (split3)
                {
                    // i3 points to the copy of ch3
                    // i1 should point to the original ch3, right?
                    // ..., ch13, copy3, ...
                    // ..., ch1, ch3, copy3, ...
                    // ch3, copy3, ..., ch1
                    // copy3, ..., ch13
                    // copy3, ..., ch1, ch3
                    i1 = (i3 - 1 + len) % len;
                    ch1 = cvHull[i1];
                }
                #endregion

                #region Counterclockwise Concavity Test
                ch3LoW = double.NaN;
                if (!split1)
                {
                    // Test concavity (whether inside convex hull)
                    if (ch3.Dst == 0.0)
                    {
                        // angle between line connecting ch2 and ch3
                        // and ch3's radial line tangent to their tube
                        ch2.UpperWedge
                            = Math.Acos((ch3.Rad - ch2.Rad) / ch2.Dst);
#if DEBUG
                        if (double.IsNaN(ch2.UpperWedge))
                            throw new Exception();
#endif
                        // Potential new ch3.LowerWedge [-PI,PI]
                        a1 = ch3.Ang - (ch2.Ang + ch2.UpperWedge);
                        while (a1 < -Math.PI)
                            a1 += 2 * Math.PI;
                        while (a1 > Math.PI)
                            a1 -= 2 * Math.PI;
                        // Test if ch2's shadow on ch3 is inside CH
                        /*if (ch3.LowerWedge <= a1)
                            continue;
                        else
                            ch3LoW = a1;/* */
                        if (ch3.LowerWedge > a1)
                        {
                            ch3LoW = a1;
                        }
                        else if (ch3.LowerWedge == a1)
                        {
                            // Are ch2 and ch3 on opposite sides of ch1?
                            a2 = Math.Abs(ch3.Ang - ch2.Ang);
                            if (a2 > Math.PI)
                                a2 = 2 * Math.PI - a2;
                            if (a2 > Math.PI / 2)
                                ch3LoW = a1;
                            else
                                continue;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        // TODO: Ensure this subtract always 
                        // works correctly and efficiently
                        a2 = ch3.Ang - ch2.Ang;
                        while (a2 < -Math.PI)
                            a2 += 2 * Math.PI;
                        while (a2 > Math.PI)
                            a2 -= 2 * Math.PI;/* */
                        /*a2 = Math.Abs(ch3.Ang - ch2.Ang);
                        if (a2 > Math.PI)
                            a2 = 2 * Math.PI - a2;/* */
                        if (a2 == 0.0)
                        {
                            // angle between line connecting ch2 and ch3
                            // and ch3's radial line tangent to their tube
                            ch2.UpperWedge = Math.Acos(
                                (ch3.Rad - ch2.Rad) /
                                (ch2.Dst - ch3.Dst));
#if DEBUG
                            if (double.IsNaN(ch2.UpperWedge))
                                throw new Exception();
#endif
                            // Potential new ch3.LowerWedge [-PI,PI]
                            ch3LoW = -ch2.UpperWedge;
                            // TODO: Make sure preliminary concavity tests
                            // eliminate all cases that could make this NaN.
                        }
                        else
                        {
                            // length of line connecting ch2 and ch3
                            hyp = Math.Sqrt(ch3.Dst * ch3.Dst
                                + ch2.Dst * ch2.Dst
                                - 2 * ch3.Dst * ch2.Dst * Math.Cos(a2));
                            // angle between line connecting ch2 and ch3
                            // and ch3's radial line tangent to their tube
                            a1 = Math.Acos((ch3.Rad - ch2.Rad) / hyp);
                            if (a2 > 0.0)
                            {
                                // previous angle minus
                                // angle between line connecting ch2 and ch3
                                // and line connecting ch2 to CH center
                                a2 = (hyp * hyp + ch2.Dst * ch2.Dst
                                    - ch3.Dst * ch3.Dst) / (2 * hyp * ch2.Dst);
                                ch2.UpperWedge = a1 - Math.Acos(//a2);
                                    a2 > 1.0 ? 1.0 : (a2 < -1.0 ? -1.0 : a2));
#if DEBUG
                                if (double.IsNaN(ch2.UpperWedge))
                                    throw new Exception();
#endif
                                // angle between line connecting ch2 and ch3
                                // and line connecting ch3 to CH center
                                a2 = (hyp * hyp + ch3.Dst * ch3.Dst
                                    - ch2.Dst * ch2.Dst) / (2 * hyp * ch3.Dst);
                                a2 = Math.Acos(//a2);
                                    a2 > 1.0 ? 1.0 : (a2 < -1.0 ? -1.0 : a2));
                            }
                            else
                            {
                                // previous angle plus
                                // angle between line connecting ch2 and ch3
                                // and line connecting ch2 to CH center
                                a2 = (hyp * hyp + ch2.Dst * ch2.Dst
                                    - ch3.Dst * ch3.Dst) / (2 * hyp * ch2.Dst);
                                ch2.UpperWedge = a1 - Math.Acos(//a2);
                                    a2 > 1.0 ? 1.0 : (a2 < -1.0 ? -1.0 : a2));
#if DEBUG
                                if (double.IsNaN(ch2.UpperWedge))
                                    throw new Exception();
#endif
                                // angle between line connecting ch2 and ch3
                                // and line connecting ch3 to CH center
                                a2 = (hyp * hyp + ch3.Dst * ch3.Dst
                                    - ch2.Dst * ch2.Dst) / (2 * hyp * ch3.Dst);
                                a2 = 2 * Math.PI - Math.Acos(//a2);
                                    a2 > 1.0 ? 1.0 : (a2 < -1.0 ? -1.0 : a2));
                            }
                            // Potential new ch3.LowerWedge [-PI,PI]
                            a1 = Math.PI - (a1 + a2);
#if DEBUG
                            if (double.IsNaN(a1))
                                throw new Exception();
#endif
                            // Test if ch2's shadow on ch3 is inside CH
                            /*if (ch3.LowerWedge <= a1)
                                continue;
                            else
                                ch3LoW = a1;/* */
                            if (ch3.LowerWedge > a1)
                            {
                                ch3LoW = a1;
                            }
                            else if (ch3.LowerWedge == a1)
                            {
                                // Are ch2 and ch3 on opposite sides of ch1?
                                a2 = Math.Abs(ch3.Ang - ch2.Ang);
                                if (a2 > Math.PI)
                                    a2 = 2 * Math.PI - a2;
                                if (a2 > Math.PI / 2)
                                    ch3LoW = a1;
                                else
                                    continue;
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                }
                #endregion

                #region Clockwise Concavity Test
                ch1UpW = double.NaN;
                if (!split3)
                {
                    // Test concavity (whether inside convex hull)
                    if (ch1.Dst == 0.0)
                    {
                        // angle between line connecting ch2 and ch1
                        // and ch1's radial line tangent to their tube
                        ch2.LowerWedge
                            = Math.Acos((ch1.Rad - ch2.Rad) / ch2.Dst);
#if DEBUG
                        if (double.IsNaN(ch2.LowerWedge))
                            throw new Exception();
#endif
                        // Potential new ch1.UpperWedge [-PI,PI]
                        a1 = (ch2.Ang - ch2.LowerWedge) - ch1.Ang;
                        while (a1 < -Math.PI)
                            a1 += 2 * Math.PI;
                        while (a1 > Math.PI)
                            a1 -= 2 * Math.PI;
                        // Test if ch2's shadow on ch1 is inside CH
                        /*if (ch1.UpperWedge <= a1)
                            continue;
                        else
                            ch1UpW = a1;/* */
                        if (ch1.UpperWedge > a1)
                        {
                            ch1UpW = a1;
                        }
                        else if (ch1.UpperWedge == a1)
                        {
                            // Are ch2 and ch1 on opposite sides of ch3?
                            a2 = Math.Abs(ch2.Ang - ch1.Ang);
                            if (a2 > Math.PI)
                                a2 = 2 * Math.PI - a2;
                            if (a2 > Math.PI / 2)
                                ch1UpW = a1;
                            else
                                continue;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        // TODO: Ensure this subtract always 
                        // works correctly and efficiently
                        a2 = ch2.Ang - ch1.Ang;
                        while (a2 < -Math.PI)
                            a2 += 2 * Math.PI;
                        while (a2 > Math.PI)
                            a2 -= 2 * Math.PI;/* */
                        /*a2 = Math.Abs(ch2.Ang - ch1.Ang);
                        if (a2 > Math.PI)
                            a2 = 2 * Math.PI - a2;/* */
                        if (a2 == 0.0)
                        {
                            // angle between line connecting ch2 and ch1
                            // and ch3's radial line tangent to their tube
                            ch2.LowerWedge = Math.Acos(
                                (ch1.Rad - ch2.Rad) /
                                (ch2.Dst - ch1.Dst));
#if DEBUG
                            if (double.IsNaN(ch2.LowerWedge))
                                throw new Exception();
#endif
                            // Potential new ch1.UpperWedge [-PI,PI]
                            ch1UpW = -ch2.LowerWedge;
                            // TODO: Make sure preliminary concavity tests
                            // eliminate all cases that could make this NaN.
                        }
                        else
                        {
                            // length of line connecting ch2 and ch1
                            hyp = Math.Sqrt(ch1.Dst * ch1.Dst
                                + ch2.Dst * ch2.Dst
                                - 2 * ch1.Dst * ch2.Dst * Math.Cos(a2));
                            // angle between line connecting ch2 and ch1
                            // and ch1's radial line tangent to their tube
                            a1 = Math.Acos((ch1.Rad - ch2.Rad) / hyp);
                            if (a2 > 0.0)
                            {
                                // previous angle minus
                                // angle between line connecting ch2 and ch1
                                // and line connecting ch2 to CH center
                                a2 = (hyp * hyp + ch2.Dst * ch2.Dst
                                    - ch1.Dst * ch1.Dst) / (2 * hyp * ch2.Dst);
                                ch2.LowerWedge = a1 - Math.Acos(//a2);
                                    a2 > 1.0 ? 1.0 : (a2 < -1.0 ? -1.0 : a2));
#if DEBUG
                                if (double.IsNaN(ch2.LowerWedge))
                                    throw new Exception();
#endif
                                // angle between line connecting ch2 and ch1
                                // and line connecting ch1 to CH center
                                a2 = (hyp * hyp + ch1.Dst * ch1.Dst
                                    - ch2.Dst * ch2.Dst) / (2 * hyp * ch1.Dst);
                                a2 = Math.Acos(//a2);
                                    a2 > 1.0 ? 1.0 : (a2 < -1.0 ? -1.0 : a2));
                            }
                            else
                            {
                                // previous angle plus
                                // angle between line connecting ch2 and ch1
                                // and line connecting ch2 to CH center
                                a2 = (hyp * hyp + ch2.Dst * ch2.Dst
                                    - ch1.Dst * ch1.Dst) / (2 * hyp * ch2.Dst);
                                ch2.LowerWedge = a1 + Math.Acos(//a2);
                                    a2 > 1.0 ? 1.0 : (a2 < -1.0 ? -1.0 : a2));
#if DEBUG
                                if (double.IsNaN(ch2.LowerWedge))
                                    throw new Exception();
#endif
                                // angle between line connecting ch2 and ch1
                                // and line connecting ch1 to CH center
                                //a2 = Math.Asin(ch2.Dst * Math.Sin(a2) / hyp);
                                a2 = (hyp * hyp + ch1.Dst * ch1.Dst
                                    - ch2.Dst * ch2.Dst) / (2 * hyp * ch1.Dst);
                                a2 = 2 * Math.PI - Math.Acos(//a2);
                                    a2 > 1.0 ? 1.0 : (a2 < -1.0 ? -1.0 : a2));
                            }
                            // Potential new ch1.UpperWedge [-PI,PI]
                            a1 = Math.PI - (a1 + a2);
#if DEBUG
                            if (double.IsNaN(a1))
                                throw new Exception();
#endif
                            // Test if ch2's shadow on ch1 is inside CH
                            /*if (ch1.UpperWedge <= a1)
                                continue;
                            else
                                ch1UpW = a1;/* */
                            if (ch1.UpperWedge > a1)
                            {
                                ch1UpW = a1;
                            }
                            else if (ch1.UpperWedge == a1)
                            {
                                // Are ch2 and ch1 on opposite sides of ch3?
                                a2 = Math.Abs(ch2.Ang - ch1.Ang);
                                if (a2 > Math.PI)
                                    a2 = 2 * Math.PI - a2;
                                if (a2 > Math.PI / 2)
                                    ch1UpW = a1;
                                else
                                    continue;
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                }
                #endregion

                #region Counterclockwise Traversal
                // Step through arc chain counterclockwise from ch3
                if (!split1)
                {
                    a1 = ch3LoW;
#if DEBUG
                    if (double.IsNaN(a1))
                        throw new Exception();
#endif
                    // Iterate until ch2's shadow is within an arc
                    j = i3;
                    while (ch3.UpperWedge < -a1)
                    {
                        j = (j + 1) % len;
                        ch3 = cvHull[j];
                        if (ch3.Dst == 0.0)
                        {
                            // angle between line connecting ch2 and ch3
                            // and ch3's radial line tangent to the tube
                            ch2.UpperWedge 
                               = Math.Acos((ch3.Rad - ch2.Rad) / ch2.Dst);
#if DEBUG
                            if (double.IsNaN(ch2.UpperWedge))
                                throw new Exception();
#endif
                            // Potential new ch3.LowerWedge [-PI,PI]
                            a1 = ch3.Ang - (ch2.Ang + ch2.UpperWedge);
                            while (a1 < -Math.PI)
                                a1 += 2 * Math.PI;
                            while (a1 > Math.PI)
                                a1 -= 2 * Math.PI;
                        }
                        else
                        {
                            // TODO: Ensure this subtract always 
                            // works correctly and efficiently
                            a2 = ch3.Ang - ch2.Ang;
                            while (a2 < -Math.PI)
                                a2 += 2 * Math.PI;
                            while (a2 > Math.PI)
                                a2 -= 2 * Math.PI;/* */
                            /*a2 = Math.Abs(ch3.Ang - ch2.Ang);
                            if (a2 > Math.PI)
                                a2 = 2 * Math.PI - a2;/* */
                            if (a2 == 0.0)
                            {
                                // angle between line connecting ch2 and ch3
                                // and ch3's radial line tangent to the tube
                                ch2.UpperWedge = Math.Acos(
                                    (ch3.Rad - ch2.Rad) / 
                                    (ch2.Dst - ch3.Dst));
#if DEBUG
                                if (double.IsNaN(ch2.UpperWedge))
                                    throw new Exception();
#endif
                                // Potential new ch3.LowerWedge [-PI,PI]
                                a1 = ch2.UpperWedge;
                            }
                            else
                            {
                                // length of line connecting ch2 and ch3
                                hyp = Math.Sqrt(ch3.Dst * ch3.Dst
                                    + ch2.Dst * ch2.Dst
                                    - 2 * ch3.Dst * ch2.Dst * Math.Cos(a2));
                                // angle between line connecting ch2 and ch3
                                // and ch3's radial line tangent to the tube
                                a1 = Math.Acos((ch3.Rad - ch2.Rad) / hyp);
                                if (a2 > 0.0)
                                {
                                    // previous angle minus
                                    // angle between line connecting ch2 and ch3
                                    // and line connecting ch2 to CH center
                                    a2 = (hyp * hyp + ch2.Dst * ch2.Dst
                                        - ch3.Dst * ch3.Dst) / (2 * hyp * ch2.Dst);
                                    ch2.UpperWedge = a1 - Math.Acos(//a2);
                                        a2 > 1.0 ? 1.0 : (a2 < -1.0 ? -1.0 : a2));
#if DEBUG
                                    if (double.IsNaN(ch2.UpperWedge))
                                        throw new Exception();
#endif
                                    // angle between line connecting ch2 and ch3
                                    // and line connecting ch3 to CH center
                                    a2 = (hyp * hyp + ch3.Dst * ch3.Dst
                                        - ch2.Dst * ch2.Dst) / (2 * hyp * ch3.Dst);
                                    a2 = Math.Acos(//a2);
                                        a2 > 1.0 ? 1.0 : (a2 < -1.0 ? -1.0 : a2));
                                }
                                else
                                {
                                    // previous angle plus
                                    // angle between line connecting ch2 and ch3
                                    // and line connecting ch2 to CH center
                                    a2 = (hyp * hyp + ch2.Dst * ch2.Dst
                                        - ch3.Dst * ch3.Dst) / (2 * hyp * ch2.Dst);
                                    ch2.UpperWedge = a1 + Math.Acos(//a2);
                                        a2 > 1.0 ? 1.0 : (a2 < -1.0 ? -1.0 : a2));
#if DEBUG
                                    if (double.IsNaN(ch2.UpperWedge))
                                        throw new Exception();
#endif
                                    // angle between line connecting ch2 and ch3
                                    // and line connecting ch3 to CH center
                                    a2 = (hyp * hyp + ch3.Dst * ch3.Dst
                                        - ch2.Dst * ch2.Dst) / (2 * hyp * ch3.Dst);
                                    a2 = 2 * Math.PI - Math.Acos(//a2);
                                        a2 > 1.0 ? 1.0 : (a2 < -1.0 ? -1.0 : a2));
                                }
                                // Potential new ch3.LowerWedge [-PI,PI]
                                a1 = Math.PI - (a1 + a2);
#if DEBUG
                                if (double.IsNaN(a1))
                                    throw new Exception();
#endif
                            }
                        }
                    }
                    if (ch3.UpperWedge == -a1)
                    {
                        j = (j + 1) % len;
                    }
                    // Remove all arcs between j and i3,
                    // replacing arc at i3 with arc at j.
                    // TODO: When working correctly, will j ever "lap" i1?
                    // If it does, how do we find the new i1/ch1?
                    // TODO: Make sure i1 & i3 are correctly updated
                    // so that ch1 == cvHull[i1] && ch3 == cvHull[i3]
                    // Since j is the source index of the copy,
                    // i3 needs to point to the destination index
                    if (i3 < j)
                    {
                        if (i1 == i3 && !split3)
                        {
                            // Don't remove arc at i1 as it's still needed
                            // for clockwise traversal
                            Array.Copy(cvHull, j, cvHull, i3 + 1, len - j);
                            len = len + i3 - j + 1;
                            i3++;//i3 = j - 1;
                        }
                        else
                        {
                            Array.Copy(cvHull, j, cvHull, i3, len - j);
                            len = len + i3 - j;
                            // TODO: What if i1 >= i3 && i1 < j, 
                            // which would make it one of the arcs removed?
#if DEBUG
                            if (i1 >= i3 && i1 < j) throw new Exception();
#endif
                            if (i1 >= j)
                                i1 -= j - i3;
                        }
                    }
                    else if (j < i3)
                    {
                        if (i1 == i3 && !split3)
                        {
                            // Don't remove arc at i1 as it's still needed
                            // for clockwise traversal
                            // TODO: Is this code necessary, 
                            // or is i1 copied over by the ELSE code
                            // and len just needs to be incremented?
                            Array.Copy(cvHull, j, cvHull, 0, i3 - j + 1);
                            len = i3 - j + 1;
                        }
                        else
                        {
                            Array.Copy(cvHull, j, cvHull, 0, i3 - j);
                            len = i3 - j;
                        }
                        // TODO: What if i1 < j || i1 >= i3,
                        // which would make it one of the arcs removed?
#if DEBUG
                        if (i1 < j || i1 > i3) throw new Exception();
#endif
                        if (i1 >= j)
                            i1 -= j;
                        i3 = 0;
                    }
                    // Update the arc at new i3
                    if (ch3.UpperWedge > -a1)
                    {
                        ch3.LowerWedge = a1;
                        if (a1 <= 0.0 && ch3.Dst == 0.0)
                        {
                            ch3.UpperWedge = (a1 + ch3.UpperWedge) / 2.0;
                            ch3.Ang += ch3.UpperWedge - a1;
                            ch3.LowerWedge = ch3.UpperWedge;
                            // TODO: Will this only happen if i3 == len - 1 ?
                            if (ch3.Ang > Math.PI)
                            {
#if DEBUG
                                if (i3 != len - 1) throw new Exception();
#endif
                                ch3.Ang -= 2 * Math.PI;
                                Array.Copy(cvHull, 0, cvHull, 1, len - 1);
                                cvHull[0] = ch3;
                                i1 = (i1 + 1) % len;
                                i3 = (i3 + 1) % len;
                            }
                        }
                        else if (a1 <= 0.0)
                        {
                            // absolute value of angle of midpoint of ch3
                            a2 = (ch3.UpperWedge - a1) / 2.0;
                            // length of line connecting midpoint of ch3
                            // to CH center
                            hyp = Math.Sqrt(ch3.Dst * ch3.Dst
                                + ch3.Rad * ch3.Rad
                                - 2 * ch3.Dst * ch3.Rad * Math.Cos(a2));
                            // angle between line connecting ch3 to CH center
                            // & line connecting midpoint of ch3 to CH center
                            ch3.WedgeOffset
                                = Math.Asin(ch3.Rad * Math.Sin(a2) / hyp);
#if DEBUG
                            if (double.IsNaN(ch3.WedgeOffset))
                                throw new Exception();
#endif
                            // TODO: Will this only happen if i3 == len - 1 ?
                            if (ch3.Ang + ch3.WedgeOffset > Math.PI)
                            {
#if DEBUG
                                if (i3 != len - 1) throw new Exception();
#endif
                                ch3.WedgeOffset -= 2 * Math.PI;
                                Array.Copy(cvHull, 0, cvHull, 1, len - 1);
                                cvHull[0] = ch3;
                                i1 = (i1 + 1) % len;
                                i3 = (i3 + 1) % len;
                            }
                        }
                    }
                }
                #endregion

                #region Clockwise Traversal
                // Step through arc chain clockwise from ch1
                if (!split3)
                {
                    a1 = ch1UpW;
#if DEBUG
                    if (double.IsNaN(a1))
                        throw new Exception();
#endif
                    // Iterate until ch2's shadow is within an arc
                    j = i1;
                    while (ch1.LowerWedge < -a1)
                    {
                        j = (j - 1 + len) % len;
                        ch1 = cvHull[j];
                        if (ch1.Dst == 0.0)
                        {
                            // angle between line connecting ch2 and ch1
                            // and ch1's radial line tangent to the tube
                            ch2.LowerWedge 
                                = Math.Acos((ch1.Rad - ch2.Rad) / ch2.Dst);
#if DEBUG
                            if (double.IsNaN(ch2.LowerWedge))
                                throw new Exception();
#endif
                            // Potential new ch1.UpperWedge [-PI,PI]
                            a1 = (ch2.Ang - ch2.LowerWedge) - ch1.Ang;
                            while (a1 < -Math.PI)
                                a1 += 2 * Math.PI;
                            while (a1 > Math.PI)
                                a1 -= 2 * Math.PI;
                        }
                        else
                        {
                            // TODO: Ensure this subtract always 
                            // works correctly and efficiently
                            a2 = ch2.Ang - ch1.Ang;
                            while (a2 < -Math.PI)
                                a2 += 2 * Math.PI;
                            while (a2 > Math.PI)
                                a2 -= 2 * Math.PI;/* */
                            /*a2 = Math.Abs(ch2.Ang - ch1.Ang);
                            if (a2 > Math.PI)
                                a2 = 2 * Math.PI - a2;/* */
                            if (a2 == 0.0)
                            {
                                // angle between line connecting ch2 and ch1
                                // and ch3's radial line tangent to the tube
                                ch2.LowerWedge = Math.Acos(
                                    (ch1.Rad - ch2.Rad) /
                                    (ch2.Dst - ch1.Dst));
#if DEBUG
                                if (double.IsNaN(ch2.LowerWedge))
                                    throw new Exception();
#endif
                                // Potential new ch1.UpperWedge [-PI,PI]
                                a1 = ch2.LowerWedge;
                            }
                            else
                            {
                                // length of line connecting ch2 and ch1
                                hyp = Math.Sqrt(ch1.Dst * ch1.Dst
                                    + ch2.Dst * ch2.Dst
                                    - 2 * ch1.Dst * ch2.Dst * Math.Cos(a2));
                                // angle between line connecting ch2 and ch1
                                // and ch1's radial line tangent to the tube
                                a1 = Math.Acos((ch1.Rad - ch2.Rad) / hyp);
                                if (a2 > 0.0)
                                {
                                    // previous angle minus
                                    // angle between line connecting ch2 and ch1
                                    // and line connecting ch2 to CH center
                                    a2 = (hyp * hyp + ch2.Dst * ch2.Dst
                                        - ch1.Dst * ch1.Dst) / (2 * hyp * ch2.Dst);
                                    ch2.LowerWedge = a1 - Math.Acos(//a2);
                                        a2 > 1.0 ? 1.0 : (a2 < -1.0 ? -1.0 : a2));
#if DEBUG
                                    if (double.IsNaN(ch2.LowerWedge))
                                        throw new Exception();
#endif
                                    // angle between line connecting ch2 and ch1
                                    // and line connecting ch1 to CH center
                                    a2 = (hyp * hyp + ch1.Dst * ch1.Dst
                                        - ch2.Dst * ch2.Dst) / (2 * hyp * ch1.Dst);
                                    a2 = Math.Acos(//a2);
                                        a2 > 1.0 ? 1.0 : (a2 < -1.0 ? -1.0 : a2));
                                }
                                else
                                {
                                    // previous angle plus
                                    // angle between line connecting ch2 and ch1
                                    // and line connecting ch2 to CH center
                                    a2 = (hyp * hyp + ch2.Dst * ch2.Dst
                                        - ch1.Dst * ch1.Dst) / (2 * hyp * ch2.Dst);
                                    ch2.LowerWedge = a1 + Math.Acos(//a2);
                                        a2 > 1.0 ? 1.0 : (a2 < -1.0 ? -1.0 : a2));
#if DEBUG
                                    if (double.IsNaN(ch2.LowerWedge))
                                        throw new Exception();
#endif
                                    // angle between line connecting ch2 and ch1
                                    // and line connecting ch1 to CH center
                                    a2 = (hyp * hyp + ch1.Dst * ch1.Dst
                                        - ch2.Dst * ch2.Dst) / (2 * hyp * ch1.Dst);
                                    a2 = 2 * Math.PI - Math.Acos(//a2);
                                        a2 > 1.0 ? 1.0 : (a2 < -1.0 ? -1.0 : a2));
                                }
                                // Potential new ch1.UpperWedge [-PI,PI]
                                a1 = Math.PI - (a1 + a2);
#if DEBUG
                                if (double.IsNaN(a1))
                                    throw new Exception();
#endif
                            }
                        }
                    }
                    if (ch1.LowerWedge == -a1)
                    {
                        j = (j - 1 + len) % len;
                    }
                    // Remove all arcs between j and i1,
                    // replacing arc at i1 with arc at j.
                    // TODO: When working correctly, will j ever "lap" i3?
                    // If it does, how do we find the new i3/ch3?
                    // TODO: Make sure i1 & i3 are correctly updated
                    // so that ch1 == cvHull[i1] && ch3 == cvHull[i3]
                    if (j < i1)
                    {
                        Array.Copy(cvHull, i1 + 1, cvHull, j + 1, len - i1 - 1);
                        len = len + j - i1;
                        // TODO: What if i3 >= j + 1 && i3 < i1 + 1, 
                        // which would make it one of the arcs removed?
#if DEBUG
                        if (i3 > j && i3 <= i1) throw new Exception();
#endif
                        if (i3 > i1)
                            i3 -= i1 - j;
                        i1 = j;
                    }
                    else if (i1 < j)
                    {
                        Array.Copy(cvHull, i1 + 1, cvHull, 0, j - i1);
                        len = j - i1;
                        // TODO: What if i3 < i1 + 1 || i3 > j,
                        // which would make it one of the arcs removed?
#if DEBUG
                        if (i3 <= i1 || i3 > j) throw new Exception();
#endif
                        if (i3 > i1)
                            i3 -= i1 + 1;
                        i1 = len - 1;
                    }
                    // Update the arc at j
                    if (ch1.LowerWedge > -a1)
                    {
                        ch1.UpperWedge = a1;
                        if (a1 <= 0.0 && ch1.Dst == 0.0)
                        {
                            ch1.LowerWedge = (ch1.LowerWedge + a1) / 2.0;
                            ch1.Ang -= ch1.LowerWedge - a1;
                            ch1.UpperWedge = ch1.LowerWedge;
                            // TODO: Will this only happen if i1 == 0 ?
                            if (ch1.Ang < -Math.PI)
                            {
#if DEBUG
                                if (i1 != 0) throw new Exception();
#endif
                                ch1.Ang += 2 * Math.PI;
                                Array.Copy(cvHull, 1, cvHull, 0, len - 1);
                                cvHull[len - 1] = ch1;
                                i1 = (i1 - 1 + len) % len;
                                i3 = (i3 - 1 + len) % len;
                            }
                        }
                        else if (a1 <= 0.0)
                        {
                            // absolute value of angle of midpoint of ch1
                            a2 = (ch1.LowerWedge - a1) / 2.0;
                            // length of line connecting midpoint of ch1
                            // to CH center
                            hyp = Math.Sqrt(ch1.Dst * ch1.Dst
                                + ch1.Rad * ch1.Rad
                                - 2 * ch1.Dst * ch1.Rad * Math.Cos(a2));
                            // angle between line connecting ch3 to CH center
                            // & line connecting midpoint of ch3 to CH center
                            ch1.WedgeOffset
                                = -Math.Asin(ch1.Rad * Math.Sin(a2) / hyp);
#if DEBUG
                            if (double.IsNaN(ch1.WedgeOffset))
                                throw new Exception();
#endif
                            // TODO: Will this only happen if i1 == 0 ?
                            if (ch1.Ang + ch1.WedgeOffset < -Math.PI)
                            {
#if DEBUG
                                if (i1 != 0) throw new Exception();
#endif
                                ch1.WedgeOffset += 2 * Math.PI;
                                Array.Copy(cvHull, 1, cvHull, 0, len - 1);
                                cvHull[len - 1] = ch1;
                                i1 = (i1 - 1 + len) % len;
                                i3 = (i3 - 1 + len) % len;
                            }
                        }
                    }
                }
                #endregion

                #region Arc Insertion
                // Insert the new arc into the convex hull
                // TODO: Make sure Mod(i3 - i1) <= 1 still holds here
                if (i3 == 0)
                {
                    if (ch2.Ang < 0.0)
                    {
                        Array.Copy(cvHull, 0, cvHull, 1, len);
                        cvHull[0] = ch2;
                    }
                    else
                    {
                        cvHull[len] = ch2;
                    }
                }
                else
                {
                    Array.Copy(cvHull, i3, cvHull, i3 + 1, len - i3);
                    cvHull[i3] = ch2;
                }
                len++;
                #endregion
            }
#if DEBUG
            cvHullStr = PrintConvexHull(cvHull, len);
#endif
            #endregion

            #region Finalization
            // Calculate the bounding circle
            //this.mCHCircle = new CHArc(this.mNodeData, chByRad[0].Rad);
            this.mCHCircle = new CHArc(this.mOwner.NodeData, chByRad[0].Rad);
            this.mCHCircle.LowerWedge = Math.PI;
            this.mCHCircle.UpperWedge = Math.PI;
            for (i = 0; i < len; i++)
            {
                ch2 = cvHull[i];
                if (this.mCHCircle.Rad < ch2.Rad + ch2.Dst)
                    this.mCHCircle.Rad = ch2.Rad + ch2.Dst;
            }
            // Offset the convex hull back to the original root
            if (root != 0)
            {
                // TODO: Make sure transitions to and from centering are
                // handled properly.
                double x2, y2, ang;
                ch2 = chByRad[root];
                a1 = ch2.Dst * Math.Cos(ch2.Ang);
                a2 = ch2.Dst * Math.Sin(ch2.Ang);
                // Offset the bounding circle
                this.mCHCircle.Ang = Math.Atan2(-a2, -a1);
                this.mCHCircle.Dst = Math.Sqrt(a1 * a1 + a2 * a2);
                // Offset the convex hull
                for (i = 0; i < len; i++)
                {
#if DEBUG
                    cvHullStr = PrintConvexHull(cvHull, len);
#endif
                    ch2 = cvHull[i];
                    x2 = ch2.Dst * Math.Cos(ch2.Ang) - a1;
                    y2 = ch2.Dst * Math.Sin(ch2.Ang) - a2;
                    if (x2 != 0.0 || y2 != 0.0)
                    {
                        ang = ch2.Ang - Math.Atan2(y2, x2);
                        // TODO: Make sure this math is right
                        ch2.LowerWedge -= ang;
                        while (ch2.LowerWedge < -Math.PI)
                            ch2.LowerWedge += 2 * Math.PI;
                        while (ch2.LowerWedge > Math.PI)
                            ch2.LowerWedge -= 2 * Math.PI;
                        ch2.UpperWedge += ang;
                        while (ch2.UpperWedge < -Math.PI)
                            ch2.UpperWedge += 2 * Math.PI;
                        while (ch2.UpperWedge > Math.PI)
                            ch2.UpperWedge -= 2 * Math.PI;
                        // No need to normalize ch2.Ang, as Atan2 does that
                        ch2.Ang -= ang;
                    }
                    ch2.Dst = Math.Sqrt(x2 * x2 + y2 * y2);
                    // TODO: Make sure wedge offsets are assigned to arcs
                    // that were previously centered.
                    if (ch2.LowerWedge < 0.0 && ch2.UpperWedge < 0.0)
                    {
                        ang = -ch2.LowerWedge;
                        ch2.LowerWedge = -ch2.UpperWedge;
                        ch2.UpperWedge = ang;
                        ch2.WedgeOffset = -ch2.WedgeOffset;
                    }
                    else if (ch2.LowerWedge < 0.0 || ch2.UpperWedge < 0.0)
                    {
                        if (ch2.Dst == 0.0)
                        {
                            ang = (ch2.UpperWedge - ch2.LowerWedge) / 2.0;
                            ch2.Ang += ang;
                            while (ch2.Ang < -Math.PI)
                                ch2.Ang += 2 * Math.PI;
                            while (ch2.Ang > Math.PI)
                                ch2.Ang -= 2 * Math.PI;
                            ang = (ch2.UpperWedge + ch2.LowerWedge) / 2.0;
                            ch2.LowerWedge = ang;
                            ch2.UpperWedge = ang;
                            ch2.WedgeOffset = 0.0;
                        }
                        else
                        {
                            // absolute value of angle of midpoint of ch2
                            ang = ch2.LowerWedge < 0.0
                                ? (ch2.UpperWedge - ch2.LowerWedge) / 2.0
                                : (ch2.LowerWedge - ch2.UpperWedge) / 2.0;
                            // length of line connecting midpoint of ch2
                            // to CH center
                            hyp = Math.Sqrt(ch2.Rad * ch2.Rad
                                + ch2.Dst * ch2.Dst
                                - 2 * ch2.Rad * ch2.Dst * Math.Cos(ang));
                            // angle between line connecting ch2 to CH center
                            // & line connecting midpoint of ch2 to CH center
                            ang = Math.Asin(ch2.Rad * Math.Sin(ang) / hyp);
                            if (ch2.LowerWedge < 0.0)
                                ch2.WedgeOffset = ang;
                            else
                                ch2.WedgeOffset = -ang;
                            while (ch2.Ang + ch2.WedgeOffset < -Math.PI)
                                ch2.WedgeOffset += 2 * Math.PI;
                            while (ch2.Ang + ch2.WedgeOffset > Math.PI)
                                ch2.WedgeOffset -= 2 * Math.PI;
                        }
                    }
                }
#if DEBUG
                cvHullStr = PrintConvexHull(cvHull, len);
#endif
                // Re-sort the convex hull by angle
                // TODO: Can this be faster/simpler?
                /*if (this.mCHAngComp == null)
                    this.mCHAngComp = new CHAngComp();
                Array.Sort<CHArc>(cvHull, 0, len, this.mCHAngComp);/* */

                i1 = 0;
                a1 = cvHull[0].Ang + cvHull[0].WedgeOffset;
                for (i = 1; i < len; i++)
                {
                    a2 = cvHull[i].Ang + cvHull[i].WedgeOffset;
                    if (a1 > a2)
                    {
                        i1 = i;
                        break;
                    }
                    a1 = a2;
                }
                // Set the convex hull value for this instance
                this.mConvexHull = new CHArc[len];
                if (i1 > 0)
                {
                    Array.Copy(cvHull, i1, this.mConvexHull, 0, len - i1);
                    Array.Copy(cvHull, 0, this.mConvexHull, len - i1, i1);
                }
                else
                {
                    Array.Copy(cvHull, 0, this.mConvexHull, 0, len);
                }
            }
            else
            {
                // Set the convex hull value for this instance
                this.mConvexHull = new CHArc[len];
                Array.Copy(cvHull, 0, this.mConvexHull, 0, len);
            }
            #endregion
#if DEBUG
            cvHullStr = PrintConvexHull(this.mConvexHull, len);
#endif
            // Update the invalidation flags
            this.bCHDirty = false;
            this.bWedgeDirty = true;
        }

        private static string PrintConvexHull(CHArc[] cvHull, int len)
        {
            // Assuming a maximum precision of 17 for double
            // double.ToString() seems to print with max precision of 15
            CHArc arc;
            double ang;
            int decPlace;
            string angStr, decSep = System.Globalization.
                NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
            StringBuilder sb = new StringBuilder((len + 1) * 150);
            sb.Append("| i  | Radius   | ");
            sb.Append("Angle                    | Distance                | ");
            sb.Append("Lower Wedge              | Upper Wedge              | ");
            sb.Append("Sort Angle               | ");
            sb.AppendLine();
            for (int i = 0; i < len; i++)
            {
                sb.AppendFormat("| {0:00} | ", i);
                arc = cvHull[i];
                if (arc == null)
                {
                    sb.AppendFormat(
                        "{0,-9}| {0,-25}| {0,-24}| {0,-25}| {0,-25}| {0,-25}|", "NULL");
                    sb.AppendLine();
                    continue;
                }
                // Print Radius
                angStr = arc.Rad.ToString("0.0###");
                decPlace = angStr.IndexOf('.');
                sb.Append(string.Concat(
                    new string(' ', 3 - decPlace), angStr).PadRight(9));
                sb.Append("| ");
                // Print Angle
                ang = 180.0 * arc.Ang / Math.PI;
                // Print in scientific notation if really small
                if (ang != 0.0 && Math.Abs(ang) < 0.00001)
                    angStr = ang.ToString("0.0##############e+0");
                else
                    angStr = ang.ToString("0.0###################");
                decPlace = angStr.IndexOf('.');
                sb.Append(string.Concat(
                    new string(' ', 4 - decPlace), angStr).PadRight(25));
                sb.Append("| ");
                // Print Distance
                ang = arc.Dst;
                // Print in scientific notation if really small
                if (ang != 0.0 && Math.Abs(ang) < 0.000001)
                    angStr = ang.ToString("0.0##############e+0");
                else
                    angStr = ang.ToString("0.0###################");
                decPlace = angStr.IndexOf(decSep);
                sb.Append(string.Concat(
                    new string(' ', 3 - decPlace), angStr).PadRight(24));
                sb.Append("| ");
                // Print Lower Wedge
                ang = 180.0 * arc.LowerWedge / Math.PI;
                // Print in scientific notation if really small
                if (ang != 0.0 && Math.Abs(ang) < 0.00001)
                    angStr = ang.ToString("0.0##############e+0");
                else
                    angStr = ang.ToString("0.0###################");
                decPlace = angStr.IndexOf('.');
                sb.Append(string.Concat(
                    new string(' ', 4 - decPlace), angStr).PadRight(25));
                sb.Append("| ");
                // Print Upper Wedge
                ang = 180.0 * arc.UpperWedge / Math.PI;
                // Print in scientific notation if really small
                if (ang != 0.0 && Math.Abs(ang) < 0.00001)
                    angStr = ang.ToString("0.0##############e+0");
                else
                    angStr = ang.ToString("0.0###################");
                decPlace = angStr.IndexOf('.');
                sb.Append(string.Concat(
                    new string(' ', 4 - decPlace), angStr).PadRight(25));
                sb.Append("| ");
                // Print Sort Angle
                ang = 180.0 * (arc.Ang + arc.WedgeOffset) / Math.PI;
                // Print in scientific notation if really small
                if (ang != 0.0 && Math.Abs(ang) < 0.00001)
                    angStr = ang.ToString("0.0##############e+0");
                else
                    angStr = ang.ToString("0.0###################");
                decPlace = angStr.IndexOf('.');
                sb.Append(string.Concat(
                    new string(' ', 4 - decPlace), angStr).PadRight(25));
                sb.Append("| ");/* */
                // Print Data
                sb.Append(arc.Data.ToString());
                sb.AppendLine(" ");
            }
            return sb.ToString();
        }
        #endregion

        #region Bounding Wedge
        /// <summary>
        /// The smallest angle in radians measured counterclockwise
        /// from the X-axis to a ray starting at the point 
        /// <c>(-<see cref="Distance"/>,0)</c> that does not intersect
        /// the <see cref="ConvexHull"/>, but is tangent to one or more 
        /// of its arcs.</summary><remarks>
        /// This forces the circle tree to recalculate its bounding wedge
        /// (<see cref="UpperWedge"/>, <see cref="LowerWedge"/>,
        ///  <see cref="UpperLength"/>, and <see cref="LowerLength"/>)
        /// if it hasn't already done so or if its <see cref="Distance"/>
        /// or <see cref="ConvexHull"/> have changed since the last time
        /// its bounding wedge was calculated.
        /// </remarks><seealso cref="CalculateBoundingWedge()"/>
        public double UpperWedge
        {
            get
            {
                if (this.bWedgeDirty || this.ConvexHullDirty)
                    this.CalculateBoundingWedge();
                return this.mUpperWedge; 
            }
        }
        /// <summary>
        /// The smallest angle in radians measured clockwise
        /// from the X-axis to a ray starting at the point 
        /// <c>(-<see cref="Distance"/>,0)</c> that does not intersect
        /// the <see cref="ConvexHull"/>, but is tangent to one or more 
        /// of its arcs.</summary><remarks>
        /// This forces the circle tree to recalculate its bounding wedge
        /// (<see cref="UpperWedge"/>, <see cref="LowerWedge"/>,
        ///  <see cref="UpperLength"/>, and <see cref="LowerLength"/>)
        /// if it hasn't already done so or if its <see cref="Distance"/>
        /// or <see cref="ConvexHull"/> have changed since the last time
        /// its bounding wedge was calculated.
        /// </remarks><seealso cref="CalculateBoundingWedge()"/>
        public double LowerWedge
        {
            get 
            {
                if (this.bWedgeDirty || this.ConvexHullDirty)
                    this.CalculateBoundingWedge();
                return this.mLowerWedge; 
            }
        }
        /// <summary>
        /// The length of the ray that forms the <see cref="UpperWedge"/> of
        /// the bounding wedge of this circle tree's 
        /// <see cref="ConvexHull"/>.</summary><remarks>
        /// This forces the circle tree to recalculate its bounding wedge
        /// (<see cref="UpperWedge"/>, <see cref="LowerWedge"/>,
        ///  <see cref="UpperLength"/>, and <see cref="LowerLength"/>)
        /// if it hasn't already done so or if its <see cref="Distance"/>
        /// or <see cref="ConvexHull"/> have changed since the last time
        /// its bounding wedge was calculated.
        /// </remarks><seealso cref="CalculateBoundingWedge()"/>
        public double UpperLength
        {
            get
            {
                if (this.bWedgeDirty || this.ConvexHullDirty)
                    this.CalculateBoundingWedge();
                return this.mUpperLength;
            }
        }
        /// <summary>
        /// The length of the ray that forms the <see cref="LowerWedge"/> of
        /// the bounding wedge of this circle tree's 
        /// <see cref="ConvexHull"/>.</summary><remarks>
        /// This forces the circle tree to recalculate its bounding wedge
        /// (<see cref="UpperWedge"/>, <see cref="LowerWedge"/>,
        ///  <see cref="UpperLength"/>, and <see cref="LowerLength"/>)
        /// if it hasn't already done so or if its <see cref="Distance"/>
        /// or <see cref="ConvexHull"/> have changed since the last time
        /// its bounding wedge was calculated.
        /// </remarks><seealso cref="CalculateBoundingWedge()"/>
        public double LowerLength
        {
            get
            {
                if (this.bWedgeDirty || this.ConvexHullDirty)
                    this.CalculateBoundingWedge();
                return this.mLowerLength;
            }
        }
        /// <summary>
        /// Forces this circle tree to calculate the angles and lengths of 
        /// the two rays that form the bounding wedge of its 
        /// <see cref="ConvexHull"/>.</summary><remarks>
        /// This forces the circle tree to recalculate its convex hull 
        /// if it hasn't done so already or if its <see cref="Radius"/> or
        /// branches have been changed since the last time its convex hull
        /// was calculated.</remarks>
        /// <seealso cref="UpperWedge"/><seealso cref="LowerWedge"/>
        /// <seealso cref="UpperLength"/><seealso cref="LowerLength"/>
        public void CalculateBoundingWedge()
        {
            double rad = this.mDst;
            if (this.ConvexHullDirty)
            {
                this.CalculateConvexHull();
            }
            if (this.mConvexHull.Length == 0)
            {
                if (rad <= this.mRad)
                {
                    this.mUpperWedge = Math.PI / 2;
                    this.mUpperLength = 0.0;
                    //this.Distance = this.mRad;
                }
                else
                {
                    this.mUpperWedge = Math.Asin(this.mRad / rad);
                    this.mUpperLength 
                        = Math.Sqrt(rad * rad - this.mRad * this.mRad);
                    //this.Distance = rad;
                }
                this.mLowerWedge = this.mUpperWedge;
                this.mLowerLength = this.mUpperLength;
                this.bWedgeDirty = false;
                return;
            }
            else if (this.mConvexHull.Length == 1)
            {
                CHArc arc = this.mConvexHull[0];
                if (arc.Dst == 0.0)
                {
                    // Angle between ray tangent to arc & X-axis
                    if (rad <= arc.Rad)
                    {
                        this.mUpperWedge = Math.PI / 2;
                        this.mUpperLength = 0.0;
                    }
                    else
                    {
                        this.mUpperWedge = Math.Asin(arc.Rad / rad);
                        this.mUpperLength
                            = Math.Sqrt(rad * rad - arc.Rad * arc.Rad);
                    }
                    this.mLowerWedge = this.mUpperWedge;
                    this.mLowerLength = this.mUpperLength;
                }
                else if (rad <= arc.Rad - arc.Dst * Math.Cos(arc.Ang))
                {
                    this.mUpperWedge = Math.PI / 2;
                    this.mUpperLength = 0.0;
                    this.mLowerWedge = this.mUpperWedge;
                    this.mLowerLength = 0.0;
                }
                else
                {
                    // Distance between ray origin & arc's center
                    // Sqrt(rad^2 + Dst^2 - 2 rad Dst Cos(PI - Ang))
                    double h = Math.Sqrt(rad * rad + arc.Dst * arc.Dst
                        + 2 * rad * arc.Dst * Math.Cos(arc.Ang));
                    if (arc.Ang < 0.0)
                    {
                        // Angle between next ray & ray tangent to arc +
                        // Angle between ray to arc's center & X-axis
                        // Asin(Dst Sin(PI + Ang)/hyp) + Asin(Rad/hyp)
                        this.mLowerWedge = Math.Asin(arc.Rad / h) +
                            Math.Asin(arc.Dst * Math.Sin(arc.Ang) / -h);
                        // Angle between next ray & ray tangent to arc -
                        // Angle between ray to arc's center & X-axis
                        // Asin(Rad/hyp) - Asin(Dst Sin(PI - Ang)/hyp)
                        this.mUpperWedge = Math.Asin(arc.Rad / h) -
                            Math.Asin(arc.Dst * Math.Sin(arc.Ang) / h);
                    }
                    else
                    {
                        // Angle between next ray & ray tangent to arc -
                        // Angle between ray to arc's center & X-axis
                        // Asin(Rad/hyp) - Asin(Dst Sin(PI - Ang)/hyp)
                        this.mLowerWedge = Math.Asin(arc.Rad / h) -
                            Math.Asin(arc.Dst * Math.Sin(arc.Ang) / h);
                        // Angle between next ray & ray tangent to arc +
                        // Angle between ray to arc's center & X-axis
                        // Asin(Dst Sin(PI - Ang)/hyp) + Asin(Rad/hyp)
                        this.mUpperWedge = Math.Asin(arc.Rad / h) +
                            Math.Asin(arc.Dst * Math.Sin(arc.Ang) / h);
                    }
                    this.mUpperLength = Math.Sqrt(h * h - arc.Rad * arc.Rad);
                    this.mLowerLength = this.mUpperLength;
                }
                this.bWedgeDirty = false;
                return;
            }
            int i = 0;
            double hyp, ang, currA, prevA, currL, prevL;
            // Test if rad is <= the left edge of the bounding box
            CHArc pt = this.mConvexHull[i++];
            currA = prevA = 0.0;
            while (currA <= prevA)
            {
                prevA = currA;
                currA = pt.Dst * Math.Cos(pt.Ang) - pt.Rad;
                if (i == this.mConvexHull.Length)
                    break;
                pt = this.mConvexHull[i++];
            }
            // Equality test ensures that cvHull[len - 1] gets tested
            if (i <= this.mConvexHull.Length)
            {
                i = this.mConvexHull.Length - 1;
                currA = prevA;
                pt = this.mConvexHull[i--];
                while (currA <= prevA)
                {
                    prevA = currA;
                    currA = pt.Dst * Math.Cos(pt.Ang) - pt.Rad;
                    if (i < 0)
                        break;
                    pt = this.mConvexHull[i--];
                }
            }
            if (rad <= -prevA)
            {
                this.mUpperWedge = Math.PI / 2;
                this.mUpperLength = 0.0;
                this.mLowerWedge = this.mUpperWedge;
                this.mLowerLength = 0.0;
                this.bWedgeDirty = false;
                return;
            }
            // Test if the arc with greatest positive angle crosses over
            // the -X-axis, and if so, calculate the angle
            pt = this.mConvexHull[this.mConvexHull.Length - 1];
            if (pt.Ang <= 0.0 || pt.UpperWedge <= 0.0)
            {
                currA = 0.0;
                currL = 0.0;
            }
            else
            {
                if (pt.Dst == 0.0)
                {
                    // Distance between pt's upper endpnt & CH center
                    //hyp = pt.Rad;
                    // Angle between pt's upper endpnt & +X-axis
                    ang = pt.Ang + pt.UpperWedge;
                }
                else
                {
                    // Distance between pt's upper endpnt & CH center
                    // Sqrt(Rad^2 + Dst^2 - 2 Rad Dst Cos(PI - UpperW))
                    hyp = Math.Sqrt(pt.Rad * pt.Rad + pt.Dst * pt.Dst
                        + 2 * pt.Rad * pt.Dst * Math.Cos(pt.UpperWedge));
                    // Angle between pt's upper endpnt & +X-axis
                    // Ang + Asin(Rad Sin(PI - UpperW) / hyp)
                    ang = pt.Ang + Math.Asin(pt.Rad *
                        Math.Sin(pt.UpperWedge) / hyp);
                }
                if (ang < Math.PI)
                {
                    currA = 0.0;
                    currL = 0.0;
                }
                else
                {
                    // Distance between ray origin & pt's upper endpnt
                    // Sqrt(rad^2 + hyp^2 - 2 rad hyp Cos(prevA - PI))
                    /*currA = Math.Sqrt(rad * rad + hyp * hyp
                        + 2 * rad * hyp * Math.Cos(prevA));
                    // Angle between ray to pt's upper endpnt & X-axis
                    // Asin(hyp Sin(prevA - PI)/currA)
                    currA = Math.Asin(hyp * -Math.Sin(prevA) / currA);
                    if (currA + prevA > 3 * Math.PI / 2)
                    {
                        currA = 0.0;
                    }
                    else/* */if (pt.Dst == 0.0)
                    {
                        // Angle between ray tangent to pt & X-axis
                        currA = Math.Asin(pt.Rad / rad);
                        currL = Math.Sqrt(rad * rad - pt.Rad * pt.Rad);
                    }
                    else
                    {
                        // Distance between ray origin & pt's center
                        // Sqrt(rad^2 + Dst^2 - 2 rad Dst Cos(PI - Ang))
                        hyp = Math.Sqrt(rad * rad + pt.Dst * pt.Dst
                            + 2 * rad * pt.Dst * Math.Cos(pt.Ang));
                        // Angle between next ray & ray tangent to pt -
                        // Angle between ray to pt's center & X-axis
                        // Asin(Rad/hyp) - Asin(Dst Sin(PI - Ang)/hyp)
                        currA = Math.Asin(pt.Rad / hyp) -
                            Math.Asin(pt.Dst * Math.Sin(pt.Ang) / hyp);
                        currL = Math.Sqrt(hyp * hyp - pt.Rad * pt.Rad);
                    }
                }
            }
            // Calculate the lower angle
            i = 0;
            prevA = 0.0;
            prevL = 0.0;
            pt = this.mConvexHull[i++];
            if (pt.Dst == 0.0)
            {
                // Distance between pt's lower endpnt & CH Center
                //hyp = pt.Rad;
                // Angle between pt's lower endpnt & +X-axis
                ang = pt.Ang - pt.LowerWedge;
            }
            else
            {
                // Distance between pt's lower endpnt & CH Center
                // Sqrt(Rad^2 + Dst^2 - 2 Rad Dst Cos(PI - LowerW))
                hyp = Math.Sqrt(pt.Rad * pt.Rad + pt.Dst * pt.Dst
                    + 2 * pt.Rad * pt.Dst * Math.Cos(pt.LowerWedge));
                // Angle between pt's lower endpnt & +X-axis
                // Ang - Asin(Rad Sin(PI - LowerW) / hyp)
                ang = pt.Ang - Math.Asin(pt.Rad * 
                    Math.Sin(pt.LowerWedge) / hyp);
            }
            while (ang <= 0.0 && prevA <= currA)
            {
                // Distance between ray origin & pt's center
                // Sqrt(rad^2 + Dst^2 - 2 rad Dst Cos(PI + Ang))
                hyp = Math.Sqrt(rad * rad + pt.Dst * pt.Dst
                    + 2 * rad * pt.Dst * Math.Cos(pt.Ang));
                prevA = currA;
                prevL = currL;
                // Angle between next ray & ray tangent to pt + 
                // Angle between ray to pt's center & X-axis
                // Asin(Dst Sin(PI + Ang)/hyp) + Asin(Rad/hyp)
                currA = Math.Asin(pt.Rad / hyp) + 
                    Math.Asin(pt.Dst * Math.Sin(pt.Ang) / -hyp);
                currL = Math.Sqrt(hyp * hyp - pt.Rad * pt.Rad);
                if (i == this.mConvexHull.Length)
                    break;
                pt = this.mConvexHull[i++];
                if (pt.Dst == 0.0)
                {
                    // Distance between pt's lower endpnt & CH Center
                    //hyp = pt.Rad;
                    // Angle between pt's lower endpnt & +X-axis
                    ang = pt.Ang - pt.LowerWedge;
                }
                else
                {
                    // Distance between pt's lower endpnt & CH Center
                    // Sqrt(Rad^2 + Dst^2 - 2 Rad Dst Cos(PI - LowerW))
                    hyp = Math.Sqrt(pt.Rad * pt.Rad + pt.Dst * pt.Dst
                        + 2 * pt.Rad * pt.Dst * Math.Cos(pt.LowerWedge));
                    // Angle between pt's lower endpnt & +X-axis
                    // Ang - Asin(Rad Sin(PI - LowerW) / hyp)
                    ang = pt.Ang - Math.Asin(pt.Rad *
                        Math.Sin(pt.LowerWedge) / hyp);
                }
            }
            this.mLowerWedge = prevA;
            this.mLowerLength = prevL;

            // Test if the arc with greatest negative angle crosses over
            // the -X-axis, and if so, calculate the angle
            pt = this.mConvexHull[0];
            if (pt.Ang >= 0.0 || pt.LowerWedge <= 0.0)
            {
                currA = 0.0;
                currL = 0.0;
            }
            else
            {
                if (pt.Dst == 0.0)
                {
                    // Distance between pt's lower endpnt & CH center
                    //hyp = pt.Rad;
                    // Angle between pt's lower endpnt & +X-axis
                    ang = pt.Ang - pt.LowerWedge;
                }
                else
                {
                    // Distance between pt's lower endpnt & CH center
                    // Sqrt(Rad^2 + Dst^2 - 2 Rad Dst Cos(PI - LowerW))
                    hyp = Math.Sqrt(pt.Rad * pt.Rad + pt.Dst * pt.Dst
                        + 2 * pt.Rad * pt.Dst * Math.Cos(pt.LowerWedge));
                    // Angle between pt's lower endpnt & +X-axis
                    // Ang - Asin(Rad Sin(PI - LowerW) / hyp)
                    ang = pt.Ang - Math.Asin(pt.Rad * 
                        Math.Sin(pt.LowerWedge) / hyp);
                }
                if (ang > -Math.PI)
                {
                    currA = 0.0;
                    currL = 0.0;
                }
                else
                {
                    /*prevA = -Math.PI - prevA;
                    // Distance between ray origin & pt's lower endpnt
                    // Sqrt(rad^2 + hyp^2 - 2 rad hyp Cos(prevA))
                    currA = Math.Sqrt(rad * rad + hyp * hyp
                        - 2 * rad * hyp * Math.Cos(prevA));
                    // Angle between ray to pt's upper endpnt & X-axis
                    // Asin(hyp Sin(prevA)/currA)
                    currA = Math.Asin(hyp * Math.Sin(prevA) / currA);
                    if (currA + prevA > Math.PI / 2)
                    {
                        currA = 0.0;
                    }
                    else/* */if (pt.Dst == 0.0)
                    {
                        // Angle between ray tangent to pt & X-axis
                        currA = Math.Asin(pt.Rad / rad);
                        currL = Math.Sqrt(rad * rad - pt.Rad * pt.Rad);
                    }
                    else
                    {
                        // Distance between ray origin & pt's center
                        // Sqrt(rad^2 + Dst^2 - 2 rad Dst Cos(PI - Ang))
                        hyp = Math.Sqrt(rad * rad + pt.Dst * pt.Dst
                            + 2 * rad * pt.Dst * Math.Cos(pt.Ang));
                        // Angle between next ray & ray tangent to pt -
                        // Angle between ray to pt's center & X-axis
                        // Asin(Rad/hyp) - Asin(Dst Sin(PI - Ang)/hyp)
                        currA = Math.Asin(pt.Rad / hyp) -
                            Math.Asin(pt.Dst * Math.Sin(pt.Ang) / hyp);
                        currL = Math.Sqrt(hyp * hyp - pt.Rad * pt.Rad);
                    }
                }
            }
            // Calculate the upper angle
            i = this.mConvexHull.Length - 1;
            prevA = 0.0;
            prevL = 0.0;
            pt = this.mConvexHull[i--];
            if (pt.Dst == 0.0)
            {
                // Distance between pt's upper endpnt & CH center
                //hyp = pt.Rad;
                // Angle between pt's upper endpnt & +X-axis
                ang = pt.Ang + pt.UpperWedge;
            }
            else
            {
                // Distance between pt's upper endpnt & CH center
                // Sqrt(Rad^2 + Dst^2 - 2 Rad Dst Cos(PI - UpperW))
                hyp = Math.Sqrt(pt.Rad * pt.Rad + pt.Dst * pt.Dst
                    + 2 * pt.Rad * pt.Dst * Math.Cos(pt.UpperWedge));
                // Angle between pt's upper endpnt & +X-axis
                // Ang + Asin(Rad Sin(PI - UpperW) / hyp)
                ang = pt.Ang + Math.Asin(pt.Rad *
                    Math.Sin(pt.UpperWedge) / hyp);
            }
            while (ang >= 0.0 && prevA <= currA)
            {
                // Distance between ray origin & pt's center
                // Sqrt(rad^2 + Dst^2 - 2 rad Dst Cos(PI - Ang))
                hyp = Math.Sqrt(rad * rad + pt.Dst * pt.Dst
                    + 2 * rad * pt.Dst * Math.Cos(pt.Ang));
                prevA = currA;
                // Angle between next ray & ray tangent to pt +
                // Angle between ray to pt's center & X-axis
                // Asin(Dst Sin(PI - Ang)/hyp) + Asin(Rad/hyp)
                currA = Math.Asin(pt.Rad / hyp) + 
                    Math.Asin(pt.Dst * Math.Sin(pt.Ang) / hyp);
                currL = Math.Sqrt(hyp * hyp - pt.Rad * pt.Rad);
                if (i < 0)
                    break;
                pt = this.mConvexHull[i--];
                if (pt.Dst == 0.0)
                {
                    // Distance between pt's upper endpnt & CH center
                    //hyp = pt.Rad;
                    // Angle between pt's upper endpnt & +X-axis
                    ang = pt.Ang + pt.UpperWedge;
                }
                else
                {
                    // Distance between pt's upper endpnt & CH center
                    // Sqrt(Rad^2 + Dst^2 - 2 Rad Dst Cos(PI - UpperW))
                    hyp = Math.Sqrt(pt.Rad * pt.Rad + pt.Dst * pt.Dst
                        + 2 * pt.Rad * pt.Dst * Math.Cos(pt.UpperWedge));
                    // Angle between pt's upper endpnt & +X-axis
                    // Ang + Asin(Rad Sin(PI - UpperW) / hyp)
                    ang = pt.Ang + Math.Asin(pt.Rad *
                        Math.Sin(pt.UpperWedge) / hyp);
                }
            }
            this.mUpperWedge = prevA;
            this.mUpperLength = prevL;

            this.bWedgeDirty = false;
        }
        #endregion

        /// <summary>
        /// Calculates the smallest rectangle that can completely enclose
        /// this circle tree's <see cref="ConvexHull"/>.
        /// </summary>
        /// <returns>The smallest rectangle that can completely enclose
        /// this circle tree's <see cref="ConvexHull"/>.</returns><remarks>
        /// This forces the circle tree to recalculate its convex hull 
        /// if it hasn't done so already or if its <see cref="Radius"/> or
        /// branches have been changed since the last time its convex hull
        /// was calculated.</remarks>
        public Box2F CalculateBoundingBox()
        {
            if (this.bCHDirty)
            {
                this.CalculateConvexHull();
            }
            if (this.mConvexHull.Length == 0)
            {
                float rad = -(float)this.mRad;
                return new Box2F(rad, rad, -2 * rad, -2 * rad);
            }
            else
            {
                double x, y;
                double minX = double.MaxValue;
                double minY = double.MaxValue;
                double maxX = -double.MaxValue;
                double maxY = -double.MaxValue;
                CHArc arc;
                for (int i = 0; i < this.mConvexHull.Length; i++)
                {
                    arc = this.mConvexHull[i];
                    x = arc.Dst * Math.Cos(arc.Ang);
                    y = arc.Dst * Math.Sin(arc.Ang);
                    if (x - arc.Rad < minX)
                        minX = x - arc.Rad;
                    if (y - arc.Rad < minY)
                        minY = y - arc.Rad;
                    if (x + arc.Rad > maxX)
                        maxX = x + arc.Rad;
                    if (y + arc.Rad > maxY)
                        maxY = y + arc.Rad;
                }
                return new Box2F((float)minX, (float)minY,
                    (float)(maxX - minX), (float)(maxY - minY));
            }
        }

        #region Debugging
        /// <summary>
        /// Gets this circle tree's <see cref="Angle"/> in degrees 
        /// instead of radians for debugging.
        /// </summary>
        public double DegAngle
        {
            get { return 180.0 * this.mAng / Math.PI; }
        }
        /// <summary>
        /// Gets this circle tree's <see cref="UpperWedge"/> in degrees
        /// instead of radians for debugging.
        /// </summary>
        public double DegUpperWedge
        {
            get
            {
                if (this.bWedgeDirty || this.ConvexHullDirty)
                    this.CalculateBoundingWedge();
                return 180.0 * this.mUpperWedge / Math.PI; 
            }
        }
        /// <summary>
        /// Gets this circle tree's <see cref="LowerWedge"/> in degrees
        /// instead of radians for debugging.
        /// </summary>
        public double DegLowerWedge
        {
            get
            {
                if (this.bWedgeDirty || this.ConvexHullDirty)
                    this.CalculateBoundingWedge();
                return 180.0 * this.mLowerWedge / Math.PI;
            }
        }
        #endregion
    }
}
