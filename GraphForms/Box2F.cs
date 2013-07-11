using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms
{
    /// <summary>
    /// Stores a set of four single-precision floating-point numbers
    /// that represent the location and size of a bounding box.
    /// </summary>
    public class Box2F : IEquatable<Box2F>
    {
        #region Fields and Constructors
        /// <summary>
        /// The X-coordinate of the upper-left corner of this bounding box.
        /// </summary>
        public float X;
        /// <summary>
        /// The Y-coordinate of the upper-left corner of this bounding box.
        /// </summary>
        public float Y;
        /// <summary>
        /// The width of this bounding box.
        /// </summary>
        private float mW;
        /// <summary>
        /// The height of this bounding box.
        /// </summary>
        private float mH;

        /// <summary>
        /// Initializes a new instance of <see cref="Box2F"/>
        /// with the specified location and size.
        /// </summary>
        /// <param name="x">The X-coordinate of 
        /// the upper-left corner of the bounding box.</param>
        /// <param name="y">The Y-coordinate of
        /// the upper-left corner of the bounding box.</param>
        /// <param name="w">The width of the bounding box.</param>
        /// <param name="h">The height of the bounding box.</param>
        public Box2F(float x, float y, float w, float h)
        {
            if (w < 0)
            {
                this.X = x + w;
                this.mW = -w;
            }
            else
            {
                this.X = x;
                this.mW = w;
            }
            if (h < 0)
            {
                this.Y = y + h;
                this.mH = -h;
            }
            else
            {
                this.Y = y;
                this.mH = h;
            }
        }
        /// <summary>
        /// Initializes a new instance of <see cref="Box2F"/>
        /// with the specified location and size.
        /// </summary>
        /// <param name="location">A <see cref="Vec2F"/> that represents
        /// the upper-left corner of the bounding box.</param>
        /// <param name="size">A <see cref="Vec2F"/> that represents
        /// the width and height of the bounding box.</param>
        public Box2F(Vec2F location, Vec2F size)
        {
            this.X = location.X;
            this.Y = location.Y;
            this.mW = size.X;
            this.mH = size.Y;
        }
        /// <summary>
        /// Initializes a new instance of <see cref="Box2F"/> 
        /// with a location and size equal to those of the given 
        /// <paramref name="box"/>.</summary>
        /// <param name="box">The box to copy.</param>
        public Box2F(Box2F box)
        {
            this.X = box.X;
            this.Y = box.Y;
            this.mH = box.mH;
            this.mW = box.mW;
        }

        /// <summary>
        /// Creates a new <see cref="Box2F"/> with its upper-left corner 
        /// and lower-right corner at the specified locations.
        /// </summary>
        /// <param name="left">The X-coordinate of
        /// the upper-left corner of the bounding box.</param>
        /// <param name="top">The Y-coordinate of 
        /// the upper-left corner of the bounding box.</param>
        /// <param name="right">The X-coordinate of
        /// the lower-right corner of the bounding box.</param>
        /// <param name="bottom">The Y-coordinate of
        /// the lower-right corner of the bounding box.</param>
        /// <returns>The new <see cref="Box2F"/> that this method creates.
        /// </returns>
        public static Box2F FromLTRB(float left, float top,
            float right, float bottom)
        {
            return new Box2F(left, top, right - left, bottom - top);
        }
        #endregion

        #region Location and Size Properties
        /// <summary>
        /// Gets or sets the width of this bounding box.
        /// </summary>
        /// <remarks><para>
        /// If the user attempts to set a negative width, that negative 
        /// value is instead added to this bounding box's <see cref="X"/> and
        /// this bounding box's width is set to the negation of that value.
        /// </para><para>
        /// This ensures that this bounding box remains "normalized" so that
        /// its hit testing and binary operations function properly.
        /// </para></remarks>
        public float W
        {
            get { return this.mW; }
            set
            {
                if (value < 0)
                {
                    this.X += value;
                    this.mW = -value;
                }
                else
                {
                    this.mW = value;
                }
            }
        }
        /// <summary>
        /// Gets or sets the height of this bounding box.
        /// </summary>
        /// <remarks><para>
        /// If the user attempts to set a negative height, that negative 
        /// value is instead added to this bounding box's <see cref="Y"/> and
        /// this bounding box's height is set to the negation of that value.
        /// </para><para>
        /// This ensures that this bounding box remains "normalized" so that
        /// its hit testing and binary operations function properly.
        /// </para></remarks>
        public float H
        {
            get { return this.mH; }
            set
            {
                if (value < 0)
                {
                    this.Y += value;
                    this.mH = -value;
                }
                else
                {
                    this.mH = value;
                }
            }
        }
        /// <summary>
        /// Tests whether the width or height of this bounding box 
        /// has a value of zero.
        /// </summary>
        public bool IsEmpty
        {
            get { return this.mW == 0 || this.mH == 0; }
        }
        /// <summary>
        /// Gets or sets the coordinates of 
        /// the upper-left corner of this box.
        /// </summary>
        public Vec2F Location
        {
            get { return new Vec2F(this.X, this.Y); }
            set
            {
                this.X = value.X;
                this.Y = value.Y;
            }
        }
        /// <summary>
        /// Gets or sets the size of this box.
        /// </summary>
        /// <remarks><para>
        /// If either component of the given size is negative, that component
        /// is instead added to the respective component in this bounding
        /// box's <see cref="Location"/> and the respective component of this
        /// bounding box's size is set to the negation of that component.
        /// </para><para>
        /// This ensures that this bounding box remains "normalized" so that
        /// its hit testing and binary operations function properly.
        /// </para></remarks>
        public Vec2F Size
        {
            get { return new Vec2F(this.mW, this.mH); }
            set
            {
                if (value.X < 0)
                {
                    this.X += value.X;
                    this.mW = -value.X;
                }
                else
                {
                    this.mW = value.X;
                }
                if (value.Y < 0)
                {
                    this.Y += value.Y;
                    this.mH = -value.Y;
                }
                else
                {
                    this.mH = value.Y;
                }
            }
        }
        /// <summary>
        /// Gets the X-coordinate of the left edge of this box.
        /// </summary>
        public float Left
        {
            get { return this.X; }
        }
        /// <summary>
        /// Gets the Y-coordinate of the top edge of this box.
        /// </summary>
        public float Top
        {
            get { return this.Y; } 
        }
        /// <summary>
        /// Gets the X-coordinate of the right edge of this box,
        /// which is the sum of <see cref="X"/> and <see cref="W"/>.
        /// </summary>
        public float Right
        {
            get { return this.X + this.mW; }
        }
        /// <summary>
        /// Gets the Y-coordinate of the bottom edge of this box,
        /// which is the sum of <see cref="Y"/> and <see cref="H"/>.
        /// </summary>
        public float Bottom
        {
            get { return this.Y + this.mH; }
        }
        #endregion

        /// <summary>
        /// Inflates this bounding box by the specified amount.
        /// </summary>
        /// <param name="x">The amount to inflate this bounding box 
        /// vertically.</param>
        /// <param name="y">The amount to inflate this bounding box 
        /// horizontally.</param>
        public void Inflate(float x, float y)
        {
            this.X -= x;
            this.Y -= y;
            this.mW += 2 * x;
            this.mH += 2 * y;
        }

        #region Hit Testing
        /// <summary>
        /// Determines if the specified point is contained within this box.
        /// </summary>
        /// <param name="x">The X-coordinate of the point to test.</param>
        /// <param name="y">The Y-coordinate of the point to test.</param>
        /// <returns>True if the given point is contained within this box;
        /// otherwise false.</returns>
        public bool Contains(float x, float y)
        {
            return this.X < x && x < (this.X + this.mW)
                && this.Y < y && y < (this.Y + this.mH);
        }
        /// <summary>
        /// Determines if the specified point is contained within this box.
        /// </summary>
        /// <param name="pt">The point to test.</param>
        /// <returns>True if the given point is contained within this box;
        /// otherwise false.</returns>
        public bool Contains(Vec2F pt)
        {
            return this.X < pt.X && pt.X < (this.X + this.mW)
                && this.Y < pt.Y && pt.Y < (this.Y + this.mH);
        }
        /// <summary>
        /// Determines if the bounding box represented by 
        /// <paramref name="box"/> is entirely contained within this box.
        /// </summary>
        /// <param name="box">The box to test.</param>
        /// <returns>True if the specified bounding box is entirely contained
        /// within this bounding box; otherwise false.</returns>
        public bool Contains(Box2F box)
        {
            return this.X <= box.X && (box.X + box.mW) <= (this.X + this.mW)
                && this.Y <= box.Y && (box.Y + box.mH) <= (this.Y + this.mH);
        }
        /// <summary>
        /// Determines if this bounding box intersects with the bounding box
        /// represented by <paramref name="box"/>.
        /// </summary>
        /// <param name="box">The bounding box to test.</param>
        /// <returns>True if there is any intersection between this bounding
        /// box and the specified bounding box; otherwise false.</returns>
        public bool IntersectsWith(Box2F box)
        {
            return box.X < (this.X + this.mW) && this.X < (box.X + box.mW)
                && box.Y < (this.Y + this.mH) && this.Y < (box.Y + box.mH);
        }
        #endregion

        #region Binary Operations
        /// <summary>
        /// Creates a bounding box that represents the intersection of the
        /// two specified bounding boxes.  If there is no intersection,
        /// an empty bounding box is returned.
        /// </summary>
        /// <param name="boxA">A bounding box to intersect.</param>
        /// <param name="boxB">A bounding box to intersect.</param>
        /// <returns>A new <see cref="Box2F"/> which represents the 
        /// overlapped area of the two specified bounding boxes.</returns>
        public static Box2F Intersect(Box2F boxA, Box2F boxB)
        {
            float x = Math.Max(boxA.X, boxB.X);
            float r = Math.Min(boxA.X + boxA.mW, boxB.X + boxB.Y);
            float y = Math.Max(boxA.Y, boxB.Y);
            float b = Math.Min(boxA.Y + boxA.mH, boxB.Y + boxB.mH);
            if (x < r && y < b)
            {
                return new Box2F(x, y, r - x, b - y);
            }
            return new Box2F(0, 0, 0, 0);
        }
        /// <summary>
        /// Creates a bounding box that represents the union of the two
        /// specified bounding boxes, which is the smallest possible bounding
        /// box that can contain both <paramref name="boxA"/> and
        /// <paramref name="boxB"/>.
        /// </summary>
        /// <param name="boxA">A bounding box to unite.</param>
        /// <param name="boxB">A bounding box to unite.</param>
        /// <returns>A new <see cref="Box2F"/> which represents the smallest
        /// possible bounding box that can contain both of the specified
        /// bounding boxes.</returns>
        public static Box2F Union(Box2F boxA, Box2F boxB)
        {
            float x = Math.Min(boxA.X, boxB.X);
            float r = Math.Max(boxA.X + boxA.mW, boxB.X + boxB.mW);
            float y = Math.Min(boxA.Y, boxB.Y);
            float b = Math.Max(boxA.Y + boxA.mH, boxB.Y + boxB.mH);
            return new Box2F(x, y, r - x, b - y);
        }
        #endregion

        #region Equality Testing
        /// <summary>
        /// Tests whether <paramref name="other"/> is a bounding box
        /// with the same location and size as this bounding box.
        /// </summary>
        /// <param name="other">The bounding box to test.</param>
        /// <returns>True if the <see cref="X"/>, <see cref="Y"/>, 
        /// <see cref="W"/>, and <see cref="H"/> values of 
        /// <paramref name="other"/> are equal to the corresponding values of
        /// this bounding box; otherwise false.</returns>
        public bool Equals(Box2F other)
        {
            return this.X == other.X && this.Y == other.Y &&
                this.mW == other.mW && this.mH == other.mH;
        }
        #endregion
    }
}
