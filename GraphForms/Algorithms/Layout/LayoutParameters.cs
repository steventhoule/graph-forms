using System;
using System.Drawing;

namespace GraphForms.Algorithms.Layout
{
    /// <summary>
    /// Contains data used by layout algorithms, which can be quickly swapped
    /// out for other instances containing different data.
    /// </summary>
    public class LayoutParameters : PropertyChangedNotifier
    {
        /// <summary>
        /// X-coordinate of the upper-left corner
        /// of the <see cref="BoundingBox"/>.
        /// </summary>
        protected float mX;
        /// <summary>
        /// X-coordinate of the upper-left corner 
        /// of the <see cref="BoundingBox"/>.
        /// </summary>
        public virtual float X
        {
            get { return this.mX; }
            set
            {
                if (this.mX != value)
                {
                    this.mX = value;
                    this.OnPropertyChanged("X");
                }
            }
        }
        /// <summary>
        /// Y-coordinate of the upper-left corner 
        /// of the <see cref="BoundingBox"/>.
        /// </summary>
        protected float mY;
        /// <summary>
        /// Y-coordinate of the upper-left corner 
        /// of the <see cref="BoundingBox"/>.
        /// </summary>
        public virtual float Y
        {
            get { return this.mY; }
            set
            {
                if (this.mY != value)
                {
                    this.mY = value;
                    this.OnPropertyChanged("Y");
                }
            }
        }
        /// <summary>
        /// Width of the <see cref="BoundingBox"/>.
        /// </summary>
        protected float mWidth;
        /// <summary>
        /// Width of the <see cref="BoundingBox"/>.
        /// </summary>
        public virtual float Width
        {
            get { return this.mWidth; }
            set
            {
                if (this.mWidth != value)
                {
                    this.mWidth = value;
                    this.OnPropertyChanged("Width");
                }
            }
        }
        /// <summary>
        /// Height of the <see cref="BoundingBox"/>.
        /// </summary>
        protected float mHeight;
        /// <summary>
        /// Height of the <see cref="BoundingBox"/>.
        /// </summary>
        public virtual float Height
        {
            get { return this.mHeight; }
            set
            {
                if (this.mHeight != value)
                {
                    this.mHeight = value;
                    this.OnPropertyChanged("Height");
                }
            }
        }

        /// <summary>
        /// The scene-level bounding box which constrains the positions of the
        /// nodes in graph of being changed by a layout algorithm. This should
        /// include any margins and padding applied to the scene's bounding
        /// box before it's used in the layout computation.
        /// </summary>
        public RectangleF BoundingBox
        {
            get { return new RectangleF(this.mX, this.mY, this.mWidth, this.mHeight); }
            set
            {
                this.X = value.X;
                this.Y = value.Y;
                this.Width = value.Width;
                this.Height = value.Height;
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="LayoutParameters"/> with
        /// its <see cref="BoundingBox"/> set to the given rectangle. 
        /// </summary>
        /// <param name="x">X-coordinate of the upper-left corner of the
        /// <see cref="BoundingBox"/>.</param>
        /// <param name="y">Y-coordinate of the upper-left corner of the
        /// <see cref="BoundingBox"/>.</param>
        /// <param name="width">Width of the <see cref="BoundingBox"/>.</param>
        /// <param name="height">Height of the <see cref="BoundingBox"/>.</param>
        public LayoutParameters(float x, float y, float width, float height)
        {
            this.mX = x;
            this.mY = y;
            this.mWidth = width;
            this.mHeight = height;
        }
    }
}
