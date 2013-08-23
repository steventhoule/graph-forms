using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Collections
{
    public class RectGeom<Node, Edge>
    {
        private GTree<Node, Edge, RectGeom<Node, Edge>> mOwner;

        private float mX;
        private float mY;
        private float mW;
        private float mH;

        private float mOffsetX;
        private float mOffsetY;

        private uint mLastBVers;
        private bool bBBoxDirty;
        private float mBBoxX;
        private float mBBoxY;
        private float mBBoxW;
        private float mBBoxH;

        public RectGeom(Box2F boundingBox)
        {
            this.mX = boundingBox.X;
            this.mY = boundingBox.Y;
            this.mW = boundingBox.W;
            this.mH = boundingBox.H;

            this.mOffsetX = 0;
            this.mOffsetY = 0;

            this.bBBoxDirty = true;
            this.mBBoxX = this.mX;
            this.mBBoxY = this.mY;
            this.mBBoxW = this.mW;
            this.mBBoxH = this.mH;
        }

        private void InvalidateParent()
        {
            GTree<Node, Edge, RectGeom<Node, Edge>> p = this.mOwner.Root;
            while (p != null && !p.GeomData.bBBoxDirty)
            {
                p.GeomData.bBBoxDirty = true;
                p = p.Root;
            }
        }

        public GTree<Node, Edge, RectGeom<Node, Edge>> Owner
        {
            get { return this.mOwner; }
        }

        public void SetOwner(GTree<Node, Edge, RectGeom<Node, Edge>> owner)
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
                this.bBBoxDirty = true;
                this.InvalidateParent();
            }
        }

        public Box2F BoundingBox
        {
            get { return new Box2F(this.mX, this.mY, this.mW, this.mH); }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("BoundingBox");
                if (this.mX != value.X)
                {
                    this.mX = value.X;
                    this.bBBoxDirty = true;
                }
                if (this.mY != value.Y)
                {
                    this.mY = value.Y;
                    this.bBBoxDirty = true;
                }
                if (this.mW != value.W)
                {
                    this.mW = value.W;
                    this.bBBoxDirty = true;
                }
                if (this.mH != value.H)
                {
                    this.mH = value.H;
                    this.bBBoxDirty = true;
                }
                if (this.bBBoxDirty)
                {
                    this.InvalidateParent();
                }
            }
        }

        public float BBoxX
        {
            get { return this.mX; }
            set
            {
                if (this.mX != value)
                {
                    this.mX = value;
                    this.bBBoxDirty = true;
                    this.InvalidateParent();
                }
            }
        }

        public float BBoxY
        {
            get { return this.mY; }
            set
            {
                if (this.mY != value)
                {
                    this.mY = value;
                    this.bBBoxDirty = true;
                    this.InvalidateParent();
                }
            }
        }

        public float BBoxW
        {
            get { return this.mW; }
            set
            {
                if (this.mW != value)
                {
                    if (value < 0)
                    {
                        this.mX += value;
                        this.mW = -value;
                    }
                    else
                    {
                        this.mW = value;
                    }
                    this.bBBoxDirty = true;
                    this.InvalidateParent();
                }
            }
        }

        public float BBoxH
        {
            get { return this.mH; }
            set
            {
                if (this.mH != value)
                {
                    if (value < 0)
                    {
                        this.mY += value;
                        this.mH = -value;
                    }
                    else
                    {
                        this.mH = value;
                    }
                    this.bBBoxDirty = true;
                    this.InvalidateParent();
                }
            }
        }

        public float OffsetX
        {
            get { return this.mOffsetX; }
            set
            {
                if (this.mOffsetX != value)
                {
                    this.mOffsetX = value;
                    this.InvalidateParent();
                }
            }
        }

        public float OffsetY
        {
            get { return this.mOffsetY; }
            set
            {
                if (this.mOffsetY != value)
                {
                    this.mOffsetY = value;
                    this.InvalidateParent();
                }
            }
        }

        public bool TreeBoundingBoxDirty
        {
            get
            {
                if (this.mLastBVers != this.mOwner.BranchVersion)
                {
                    this.mLastBVers = this.mOwner.BranchVersion;
                    this.bBBoxDirty = true;
                    this.InvalidateParent();
                }
                return this.bBBoxDirty;
            }
        }

        public Box2F TreeBoundingBox
        {
            get
            {
                if (this.TreeBoundingBoxDirty)
                    this.CalculateTreeBoundingBox();
                return new Box2F(this.mBBoxX, this.mBBoxY, 
                                 this.mBBoxW, this.mBBoxH);
            }
        }

        public void CalculateTreeBoundingBox()
        {
            this.mBBoxX = this.mX;
            this.mBBoxY = this.mY;
            this.mBBoxW = this.mW;
            this.mBBoxH = this.mH;
            if (this.mOwner.BranchCount > 0)
            {
                float val;
                float maxX = this.mX + this.mW;
                float maxY = this.mY + this.mH;
                GTree<Node, Edge, RectGeom<Node, Edge>> branch;
                GTree<Node, Edge, RectGeom<Node, Edge>>[] branches 
                    = this.mOwner.Branches;
                for (int i = 0; i < branches.Length; i++)
                {
                    branch = branches[i];
                    if (branch.GeomData.TreeBoundingBoxDirty)
                        branch.GeomData.CalculateTreeBoundingBox();
                    val = branch.GeomData.mBBoxX + branch.GeomData.mOffsetX;
                    if (this.mBBoxX > val)
                        this.mBBoxX = val;
                    val += branch.GeomData.mBBoxW;
                    if (maxX < val)
                        maxX = val;
                    val = branch.GeomData.mBBoxY + branch.GeomData.mOffsetY;
                    if (this.mBBoxY > val)
                        this.mBBoxY = val;
                    val += branch.GeomData.mBBoxH;
                    if (maxY < val)
                        maxY = val;
                }
                this.mBBoxW = maxX - this.mBBoxX;
                this.mBBoxH = maxY - this.mBBoxY;
            }
            this.bBBoxDirty = false;
        }
    }
}
