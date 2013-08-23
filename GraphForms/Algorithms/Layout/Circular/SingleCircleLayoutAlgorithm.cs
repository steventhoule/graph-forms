using System;

namespace GraphForms.Algorithms.Layout.Circular
{
    public class SingleCircleLayoutAlgorithm<Node, Edge>
        : LayoutAlgorithm<Node, Edge>
        where Node : class, ILayoutNode
        where Edge : IGraphEdge<Node>, IUpdateable
    {
        // Sorting Parameters
        private bool bInSketchMode = false;
        private NodeSequencer<Node, Edge> mSequencer
            = new NodeSequencer<Node, Edge>();

        // Position Calculation Paramaters
        private CircleSpacing mNodeSpacing = CircleSpacing.SNS;
        private double mMinRadius = 10;
        private double mFreeArc = 5;

        // Center Calculation Parameters
        private CircleCentering mCentering = CircleCentering.Centroid;
        private double mCenterX = 0;
        private double mCenterY = 0;

        // Physical Animation Parameters
        private bool bAdaptToSizeChanges = false;
        private bool bAdjustCenter = true;
        private bool bAdjustAngle = false;
        private double mSpringMult = 10;
        private double mMagnetMult = 100;
        private double mMagnetExp = 1;
        private double mAngle = 0;

        // Flags and Calculated Values
        private bool bEmbedCircleDirty = true;
        private int mEmbedCircleLength;
        private Digraph<Node, Edge>.GNode[] mEmbedCircle;

        private bool bPositionsDirty = true;
        private double mRadius;
        private double mBoundingRadius;
        private double[] mHalfSizes;
        private double[] mAngles;
        private double mCentroidX;
        private double mCentroidY;

        public SingleCircleLayoutAlgorithm(Digraph<Node, Edge> graph,
            IClusterNode clusterNode)
            : base(graph, clusterNode)
        {
            this.mEmbedCircle = new Digraph<Node, Edge>.GNode[0];
            this.mAngles = new double[0];
            this.mHalfSizes = new double[0];
        }

        public SingleCircleLayoutAlgorithm(Digraph<Node, Edge> graph,
            Box2F boundingBox)
            : base(graph, boundingBox)
        {
            this.mEmbedCircle = new Digraph<Node, Edge>.GNode[0];
            this.mAngles = new double[0];
            this.mHalfSizes = new double[0];
        }

        #region Parameters

        #region Sorting Parameters

        public bool InSketchMode
        {
            get { return this.bInSketchMode; }
            set
            {
                if (this.bInSketchMode != value &&
                    this.State != ComputeState.Running &&
                    this.AsyncState != ComputeState.Running)
                {
                    this.bInSketchMode = value;
                    this.bEmbedCircleDirty = true;
                }
            }
        }
        /// <summary><para>
        /// Gets or sets the node sequencer this algorithm uses to calculate
        /// the ordering of the nodes in the embedding circle.</para><para>
        /// If set to <c>null</c>, the nodes will be arranged in the same 
        /// order they were inserted into their graph.</para></summary>
        public NodeSequencer<Node, Edge> NodeSequencer
        {
            get { return this.mSequencer; }
            set
            {
                if (this.mSequencer != value)
                {
                    this.mSequencer = value;
                    this.bEmbedCircleDirty = true;
                }
            }
        }
        /// <summary>
        /// Forces this algorithm to recalculate the ordering of the nodes
        /// in the embedding circle on next iteration of its layout
        /// computation, even if it has already done so after the last time
        /// the <see cref="NodeSequencer"/> parameter was changed.
        /// </summary>
        public void ForceNodeSequencing()
        {
            this.bEmbedCircleDirty = true;
        }
        #endregion

        #region Position Calculation Parameters
        /// <summary>
        /// Gets or sets the method this algorithm uses to calculate the
        /// angles between nodes around the center of the embedding circle.
        /// </summary>
        public CircleSpacing NodeSpacing
        {
            get { return this.mNodeSpacing; }
            set
            {
                if (this.mNodeSpacing != value)
                {
                    this.mNodeSpacing = value;
                    this.bPositionsDirty = true;
                }
            }
        }
        /// <summary>
        /// Gets or sets the minimum radius of the embedding circle
        /// calculated by this layout algorithm.
        /// </summary>
        public double MinRadius
        {
            get { return this.mMinRadius; }
            set
            {
                if (this.mMinRadius != value)
                {
                    this.mMinRadius = value;
                    this.bPositionsDirty = true;
                }
            }
        }
        /// <summary>
        /// Gets or sets the minimum distance between nodes on the 
        /// embedding circle as measured along the arc between them
        /// on the embedding circle.
        /// </summary>
        public double FreeArc
        {
            get { return this.mFreeArc; }
            set
            {
                if (this.mFreeArc != value)
                {
                    this.mFreeArc = value;
                    this.bPositionsDirty = true;
                }
            }
        }
        #endregion

        #region Center Calculation Parameters
        /// <summary>
        /// Gets or sets the method this algorithm uses to calculate the 
        /// initial position of the center of its embedding circle.
        /// </summary>
        public CircleCentering Centering
        {
            get { return this.mCentering; }
            set
            {
                if (this.mCentering != value)
                {
                    this.mCentering = value;
                }
            }
        }
        /// <summary>
        /// Gets or sets the X-coordinate of the center of the embedding
        /// circle (in the local coordinate system of the graph).
        /// </summary><remarks>
        /// Be aware that this value might change on each iteration if
        /// <see cref="AdjustCenter"/> is true or <see cref="Centering"/>
        /// is not <see cref="CircleCentering.Predefined"/>.
        /// </remarks>
        public double CenterX
        {
            get { return this.mCenterX; }
            set
            {
                if (this.mCenterX != value)
                {
                    this.mCenterX = value;
                }
            }
        }
        /// <summary>
        /// Gets or sets the Y-coordinate of the center of the embedding
        /// circle (in the local coordinate system of the graph).
        /// </summary><remarks>
        /// Be aware that this value might change on each iteration if
        /// <see cref="AdjustCenter"/> is true or <see cref="Centering"/>
        /// is not <see cref="CircleCentering.Predefined"/>.
        /// </remarks>
        public double CenterY
        {
            get { return this.mCenterY; }
            set
            {
                if (this.mCenterY != value)
                {
                    this.mCenterY = value;
                }
            }
        }
        #endregion

        #region Physical Animation Parameters
        public bool AdaptToSizeChanges
        {
            get { return this.bAdaptToSizeChanges; }
            set
            {
                if (this.bAdaptToSizeChanges != value)
                {
                    this.bAdaptToSizeChanges = value;
                }
            }
        }
        /// <summary>
        /// Gets or sets whether the center of the embedding circle is
        /// first repositioned to attempt to set the radii and angle
        /// of any fixed nodes on the embedding circle equal to those
        /// calculated for the embedding circle.</summary>
        public bool AdjustCenter
        {
            get { return this.bAdjustCenter; }
            set
            {
                if (this.bAdjustCenter != value)
                {
                    this.bAdjustCenter = value;
                }
            }
        }

        public bool AdjustAngle
        {
            get { return this.bAdjustAngle; }
            set
            {
                if (this.bAdjustAngle != value)
                {
                    this.bAdjustAngle = value;
                }
            }
        }

        public double SpringMultiplier
        {
            get { return this.mSpringMult; }
            set
            {
                if (this.mSpringMult != value)
                {
                    this.mSpringMult = value;
                }
            }
        }

        public double MagneticMultiplier
        {
            get { return this.mMagnetMult; }
            set
            {
                if (this.mMagnetMult != value)
                {
                    this.mMagnetMult = value;
                }
            }
        }

        public double MagneticExponent
        {
            get { return this.mMagnetExp; }
            set
            {
                if (this.mMagnetExp != value)
                {
                    this.mMagnetExp = value;
                }
            }
        }
        /// <summary>
        /// Gets or sets the angle which the nodes of the graph are rotated
        /// around the center of the embedding circle, measured in radians
        /// counterclockwise from the +X-axis.
        /// </summary><remarks>
        /// Since this angle is measured clockwise from the +X-axis on a
        /// standard drawing surface (because the Y-axis is negated),
        /// the final graph will appear as if it has been rotated by the
        /// negation of this angle on a conventional 2D Euclidean manifold. 
        /// </remarks>
        public double RootAngle
        {
            get { return this.mAngle; }
            set
            {
                if (this.mAngle != value)
                {
                    this.mAngle = value;
                }
            }
        }
        #endregion

        #region Angle Parameters in Degrees
        /// <summary>
        /// Gets or sets the <see cref="RootAngle"/> parameter of this 
        /// layout algorithm in degrees instead of radians for debugging.
        /// </summary>
        public double DegRootAngle
        {
            get { return 180.0 * this.mAngle / Math.PI; }
            set
            {
                value = Math.PI * value / 180.0;
                if (this.mAngle != value)
                {
                    this.mAngle = value;
                }
            }
        }
        #endregion

        #endregion

        #region Calculated Properties
        /// <summary>
        /// Gets the current radius of the embedding circle of this layout
        /// algorithm, as calculated based on the values of
        /// <see cref="NodeSpacing"/>, <see cref="MinRadius"/>, and
        /// <see cref="FreeArc"/>.</summary>
        public double Radius
        {
            get 
            {
                this.PerformPrecalculations();
                return this.mRadius; 
            }
        }
        /// <summary>
        /// Gets the current radius of the smallest circle concentric with
        /// the embedding circle of this layout algorithm that can enclose
        /// all the nodes in the graph processed by this layout algorithm.
        /// </summary>
        public double BoundingRadius
        {
            get 
            {
                this.PerformPrecalculations();
                return this.mBoundingRadius; 
            }
        }
        /// <summary>
        /// Gets the calculated angles of the nodes of the graph of this
        /// layout algorithm around the center of its embedding circle, 
        /// measured in radians counterclockwise from the +X-axis, in the
        /// same order as their corresponding nodes are stored in the graph.
        /// </summary>
        public double[] Angles
        {
            get
            {
                this.PerformPrecalculations();
                double[] angles = new double[this.mAngles.Length];
                Array.Copy(this.mAngles, 0, angles, 0, this.mAngles.Length);
                return angles;
            }
        }
        /// <summary>
        /// Gets the calculated angle around the center of the embedding
        /// circle for the node at the given <paramref name="nodeIndex"/> 
        /// in the graph of this layout algorithm, measured in radians 
        /// counterclockwise from the +X-axis.</summary>
        /// <param name="nodeIndex">The index of the node in the graph
        /// of this layout algorithm.</param>
        /// <returns>The angle around the center of the embedding circle
        /// for the node at the given <paramref name="nodeIndex"/> in the
        /// graph of this layout algorithm, measured in radians
        /// counterclockwise from the +X-axis.</returns>
        public double AngleAt(int nodeIndex)
        {
            this.PerformPrecalculations();
            return this.mAngles[nodeIndex];
        }

        public double CentroidX
        {
            get
            {
                this.PerformPrecalculations();
                return this.mCentroidX;
            }
        }

        public double CentroidY
        {
            get
            {
                this.PerformPrecalculations();
                return this.mCentroidY;
            }
        }
        #endregion

        private void CalculateEmbeddingCircle()
        {
            int i, count = this.mGraph.NodeCount;
            Digraph<Node, Edge>.GNode gNode;
            if (this.mEmbedCircle.Length < count)
            {
                this.mEmbedCircle = new Digraph<Node, Edge>.GNode[count];
            }
            this.mEmbedCircleLength = 0;
            for (i = 0; i < count; i++)
            {
                gNode = this.mGraph.InternalNodeAt(i);
                if (!gNode.Hidden)
                {
                    this.mEmbedCircle[this.mEmbedCircleLength++] = gNode;
                }
            }
            if (this.bInSketchMode && this.mEmbedCircleLength > 0)
            {
                // Calculate the centroid of the nodes
                double cx = 0.0;
                double cy = 0.0;
                for (i = 0; i < this.mEmbedCircleLength; i++)
                {
                    gNode = this.mEmbedCircle[i];
                    cx += gNode.Data.X;
                    cy += gNode.Data.Y;
                }
                cx /= this.mEmbedCircleLength;
                cy /= this.mEmbedCircleLength;
                // Calculate the normalization angle
                double rootAngle = this.mAngle - Math.PI;
                while (rootAngle < -Math.PI)
                    rootAngle += 2 * Math.PI;
                while (rootAngle > Math.PI)
                    rootAngle -= 2 * Math.PI;
                // Calculate the angles of the nodes around their centroid
                double ang;
                double[] angles = new double[this.mEmbedCircleLength];
                for (i = 0; i < this.mEmbedCircleLength; i++)
                {
                    gNode = this.mEmbedCircle[i];
                    ang = Math.Atan2(gNode.Data.Y - cx, gNode.Data.X - cy);
                    if (ang < rootAngle)
                        ang += 2 * Math.PI;
                    angles[i] = ang;
                }
                Array.Sort(angles, this.mEmbedCircle, 0, 
                    this.mEmbedCircleLength, null);
            }
            else if (this.mSequencer != null)
            {
                this.mSequencer.ArrangeNodes(this.mGraph, this.mEmbedCircle);
            }
        }

        private void CalculatePositions()
        {
            //Box2F bbox;
            int i;//, count = this.mGraph.NodeCount;
            double perimeter, ang, a, size;
            /*if (this.mHalfSizes.Length < count)
            {
                this.mHalfSizes = new double[count];
            }/* */

            // calculate the size of the circle
            perimeter = 0;
            for (i = 0; i < this.mEmbedCircleLength; i++)
            {
                //bbox = this.mEmbedCircle[i].Data.LayoutBBox;
                //size = Math.Sqrt(bbox.W * bbox.W + bbox.H * bbox.H);
                //perimeter += size;
                //this.mHalfSizes[this.mEmbedCircle[i].Index] = size / 4;
                perimeter += 4 * this.mHalfSizes[this.mEmbedCircle[i].Index];
            }
            perimeter += this.mEmbedCircleLength * this.mFreeArc;

            this.mRadius = perimeter / (2 * Math.PI);
            if (this.mAngles.Length < this.mGraph.NodeCount)
            {
                this.mAngles = new double[this.mGraph.NodeCount];
            }

            // precalculation
            perimeter = 0;
            ang = 0;
            for (i = 0; i < this.mEmbedCircleLength; i++)
            {
                size = this.mHalfSizes[this.mEmbedCircle[i].Index];
                perimeter = Math.Max(2 * size, perimeter);
                //size += this.mFreeArc;
                a = Math.Sin((size + this.mFreeArc) / this.mRadius) * 4;
                ang += a;
                //this.mHalfSizes[this.mEmbedCircle[i].Index] = size;
            }

            //base.EndIteration(0, 0.5, "Precalculation done.");

            // recalculate radius
            this.mRadius = ang / (2 * Math.PI) * this.mRadius;

            // calculation
            ang = -Math.PI;
            switch (this.mNodeSpacing)
            {
                case CircleSpacing.Fractal:
                    a = Math.PI / this.mEmbedCircleLength;
                    for (i = 0; i < this.mEmbedCircleLength; i++)
                    {
                        ang += a;
                        this.mAngles[this.mEmbedCircle[i].Index] = ang;
                        ang += a;
                    }
                    break;
                case CircleSpacing.SNS:
                    for (i = 0; i < this.mEmbedCircleLength; i++)
                    {
                        size = this.mHalfSizes[this.mEmbedCircle[i].Index]
                             + this.mFreeArc;
                        a = Math.Sin(size / this.mRadius) * 2;
                        ang += a;
                        this.mAngles[this.mEmbedCircle[i].Index] = ang;
                        ang += a;
                    }
                    break;
            }

            this.mRadius = Math.Max(this.mRadius, this.mMinRadius);
            this.mBoundingRadius = this.mRadius + perimeter;

            //base.EndIteration(1, 1, "Calculation done.");
        }

        protected virtual double GetBoundingRadius(
            Digraph<Node, Edge>.GNode node)
        {
            Box2F bbox = node.Data.LayoutBBox;
            return Math.Sqrt(bbox.W * bbox.W + bbox.H * bbox.H) / 2.0;
        }

        protected override void PerformPrecalculations(
            uint lastNodeVersion, uint lastEdgeVersion)
        {
            int i;
            bool nDirty = this.mGraph.NodeVersion != lastNodeVersion;
            if (this.bEmbedCircleDirty || nDirty ||
                this.mGraph.EdgeVersion != lastEdgeVersion)
            {
                this.CalculateEmbeddingCircle();
                this.bPositionsDirty = true;
            }
            if (this.bPositionsDirty || this.bAdaptToSizeChanges)
            {
                double size;
                Digraph<Node, Edge>.GNode node;
                if (this.mHalfSizes.Length < this.mGraph.NodeCount)
                {
                    this.mHalfSizes = new double[this.mGraph.NodeCount];
                }
                for (i = 0; i < this.mEmbedCircleLength; i++)
                {
                    node = this.mEmbedCircle[i];
                    size = this.GetBoundingRadius(node) / 2.0;
                    if (this.mHalfSizes[node.Index] != size)
                    {
                        this.mHalfSizes[node.Index] = size;
                        this.bPositionsDirty = true;
                    }
                }
            }
            if (this.bPositionsDirty)
            {
                this.CalculatePositions();
            }
            if (nDirty && this.mEmbedCircleLength > 0)
            {
                // The centroid should not be recalculated on every iteration
                // of the computation, because doing so might cause a "drift"
                // phenomenon in which the graph continuously applies a large
                // force to itself in the direction of the change in position
                // from the old centroid to the new one.
                this.mCentroidX = this.mCentroidY = 0.0;
                for (i = 0; i < this.mEmbedCircleLength; i++)
                {
                    this.mCentroidX += this.mEmbedCircle[i].Data.X;
                    this.mCentroidY += this.mEmbedCircle[i].Data.Y;
                }
                this.mCentroidX /= this.mEmbedCircleLength;
                this.mCentroidY /= this.mEmbedCircleLength;
            }
            // Calculate the center if necessary
            Box2F bbox = this.mClusterNode == null
                ? this.BoundingBox : this.mClusterNode.LayoutBBox;
            switch (this.mCentering)
            {
                case CircleCentering.BBoxCenter:
                    this.mCenterX = bbox.X + bbox.W / 2.0;
                    this.mCenterY = bbox.Y + bbox.H / 2.0;
                    break;
                case CircleCentering.Centroid:
                    this.mCenterX = this.mCentroidX;
                    this.mCenterY = this.mCentroidY;
                    break;
            }
            // Normalize the center to within the bounding box
            /*double w = bbox.W;
            while (this.mCenterX > bbox.X)
                this.mCenterX -= w;
            while (this.mCenterX < bbox.X)
                this.mCenterX += w;

            double h = bbox.H;
            while (this.mCenterY > bbox.Y)
                this.mCenterY -= h;
            while (this.mCenterY < bbox.Y)
                this.mCenterY += h;
            /* */
            this.bEmbedCircleDirty = false;
            this.bPositionsDirty = false;
        }

        protected override void PerformIteration(uint iteration)
        {
            Digraph<Node, Edge>.GNode gNode;
            int i, nCount = this.mGraph.NodeCount;
            double dx, dy, r, force, fx, fy;
            // Pull the center towards fixed nodes
            if (this.bAdjustCenter)
            {
                for (i = 0; i < this.mEmbedCircleLength; i++)
                {
                    gNode = this.mEmbedCircle[i];
                    if (gNode.Data.PositionFixed)
                    {
                        dx = this.mCenterX - gNode.Data.X;
                        dy = this.mCenterY - gNode.Data.Y;
                        if (dx == 0 && dy == 0)
                        {
                            fx = fy = this.mRadius / 10;
                        }
                        else
                        {
                            r = dx * dx + dy * dy;
                            if (this.bAdjustAngle)
                            {
                                this.mAngle = Math.Atan2(-dy, -dx)
                                    - this.mAngles[gNode.Index];
                                while (this.mAngle < -Math.PI)
                                    this.mAngle += 2 * Math.PI;
                                while (this.mAngle > Math.PI)
                                    this.mAngle -= 2 * Math.PI;
                                fx = fy = 0;
                            }
                            else
                            {
                                // Magnetic Torque
                                force = this.mAngles[gNode.Index]
                                      + this.mAngle;
                                // dx and dy have to be negated here in
                                // order to get the same results as the 
                                // single step method that uses Cos/Sin
                                force = Math.Atan2(-dy, -dx) - force;
                                while (force < -Math.PI)
                                    force += 2 * Math.PI;
                                while (force > Math.PI)
                                    force -= 2 * Math.PI;
                                force = this.mMagnetMult *
                                    Math.Pow(force, this.mMagnetExp) / r;
                                fx = force * -dy;
                                fy = force * dx;
                            }
                            // Spring Force
                            r = Math.Sqrt(r);
                            force = this.mSpringMult *
                                Math.Log(r / this.mRadius);
                            fx += force * dx / r;
                            fy += force * dy / r;
                        }
                        // Apply force to center position
                        this.mCenterX -= fx;
                        this.mCenterY -= fy;/* */
                    }
                }
            }
            // Pull movable nodes towards the center
            for (i = 0; i < this.mEmbedCircleLength; i++)
            {
                gNode = this.mEmbedCircle[i];
                if (!gNode.Data.PositionFixed)
                {
                    fx = gNode.Data.X;
                    fy = gNode.Data.Y;
                    dx = this.mCenterX - fx;
                    dy = this.mCenterY - fy;
                    if (dx == 0 && dy == 0)
                    {
                        fx += this.mRadius / 10;
                        fy += this.mRadius / 10;
                    }
                    else
                    {
                        r = dx * dx + dy * dy;
                        // Magnetic Torque
                        force = this.mAngles[gNode.Index] + this.mAngle;
                        // dx and dy have to be negated here in
                        // order to get the same results as the 
                        // single step method that uses Cos/Sin
                        force = Math.Atan2(-dy, -dx) - force;
                        while (force < -Math.PI)
                            force += 2 * Math.PI;
                        while (force > Math.PI)
                            force -= 2 * Math.PI;
                        force = this.mMagnetMult *
                            Math.Pow(force, this.mMagnetExp) / r;
                        fx += force * -dy;
                        fy += force * dx;
                        // Spring Force
                        r = Math.Sqrt(r);
                        force = this.mSpringMult *
                            Math.Log(r / this.mRadius);
                        fx += force * dx / r;
                        fy += force * dy / r;
                    }
                    // Add force to position
                    gNode.Data.SetPosition((float)fx, (float)fy);/* */

                    /*force = this.mAngles[nodes[i].Index] + this.mAngle;
                    dx = this.mCX + this.mRadius * Math.Cos(force);
                    dy = this.mCY + this.mRadius * Math.Sin(force);
                    node.SetNewPosition((float)dx, (float)dy);/* */
                }
            }
        }
    }
}
