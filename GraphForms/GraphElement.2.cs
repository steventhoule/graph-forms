using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace GraphForms
{
    public partial class GraphElement
    {
        #region Hit Testing
        private int mBoundingX = 0;
        private int mBoundingY = 0;
        private int mBoundingW = 0;
        private int mBoundingH = 0;

        /// <summary>
        /// This element's bounding box in its local coordinate system.
        /// This must be initialized as it's used for drawing invalidation
        /// and quick hit testing before the more complex hit test
        /// functions are called.
        /// </summary>
        public Rectangle BoundingBox
        {
            get 
            { 
                return new Rectangle(this.mBoundingX, this.mBoundingY, 
                    this.mBoundingW, this.mBoundingH); 
            }
            set
            {
                this.RefitBackgroundCache(value);

                Rectangle invalid = Rectangle.Union(this.BoundingBox, value);

                this.mBoundingX = value.X;
                this.mBoundingY = value.Y;
                this.mBoundingW = value.Width;
                this.mBoundingH = value.Height;
            }
        }

        /// <summary>
        /// This element's overall shape in its local coordinate system,
        /// used for hit testing and drawing (clipping).
        /// The default shape is this element's <see cref="BoundingBox"/>.
        /// </summary>
        /// <returns>This element's shape in its local coordinate system.
        /// </returns>
        public virtual Region Shape()
        {
            return new Region(this.BoundingBox);
        }

        /// <summary>
        /// This element's main hit testing function, 
        /// which is called if its <see cref="BoundingBox"/>
        /// contains the given <paramref name="point"/>.
        /// This should be as efficient as possible since it's called
        /// very often by mouse events.
        /// The default implementation tests whether or not this element's
        /// <see cref="Shape()"/> contains the given 
        /// <paramref name="point"/>.
        /// </summary>
        /// <param name="point">
        /// A point in the local coordinate system of the element.
        /// </param>
        /// <returns>True if the given <paramref name="point"/>
        /// is contained within this element, false otherwise.</returns>
        public virtual bool Contains(PointF point)
        {
            return this.Shape().IsVisible(point);
        }

        private bool bClipsChildrenToShape = false;

        /// <summary>
        /// Whether or not this element's children are clipped within its 
        /// <see cref="Shape()"/> for hit testing and drawing.
        /// </summary>
        public bool ClipsChildrenToShape
        {
            get { return this.bClipsChildrenToShape; }
            set
            {
                if (this.bClipsChildrenToShape != value)
                {
                    this.bClipsChildrenToShape = value;
                    this.Invalidate(this.ChildrenBoundingBox());
                }
            }
        }

        private bool bClipsToShape = false;

        /// <summary>
        /// Whether or not this element is clipped within its 
        /// <see cref="Shape()"/> for drawing.
        /// </summary>
        public bool ClipsToShape
        {
            get { return this.bClipsToShape; }
            set
            {
                if (this.bClipsToShape != value)
                {
                    this.bClipsToShape = value;
                    this.Invalidate(this.BoundingBox);
                }
            }
        }
        #endregion

        #region Position
        private float mPosX;
        private float mPosY;

        /// <summary>
        /// This element's position in its <see cref="Parent"/> 
        /// element's coordinate system.
        /// </summary>
        public PointF Position
        {
            get { return new PointF(this.mPosX, this.mPosY); }
            set { this.SetPosition(value.X, value.Y); }
        }

        /// <summary>
        /// Sets this element's position in its <see cref="Parent"/>
        /// element's coordinate system.
        /// </summary>
        /// <param name="x">The new horizontal offset from the 
        /// <see cref="Parent"/> element's origin.</param>
        /// <param name="y">The new vertical offset from the
        /// <see cref="Parent"/> element's origin.</param>
        public virtual void SetPosition(float x, float y)
        {
            if (this.mPosX != x || this.mPosY != y)
            {
                float offsetX = x - this.mPosX;
                float offsetY = y - this.mPosY;
                RectangleF bbox = this.bClipsChildrenToShape ?
                    this.BoundingBox : this.ChildrenBoundingBox();
                // Union of bbox and offset bbox (before and after movement)
                bbox.Inflate(Math.Abs(offsetX), Math.Abs(offsetY));
                bbox.Offset(offsetX < 0 ? offsetX : 0,
                            offsetY < 0 ? offsetY : 0);

                this.mPosX = x;
                this.mPosY = y;

                this.Invalidate(bbox);
            }
        }
        #endregion

        /// <summary>
        /// Returns the recursive union of this element's
        /// <see cref="BoundingBox"/> and the bounding boxes of all of its
        /// children, and their children, and so on.
        /// </summary>
        /// <returns>The recursive union of the bounding boxes of
        /// this element and all its children.</returns>
        RectangleF ChildrenBoundingBox()
        {
            RectangleF bbox = this.BoundingBox;
            if (this.HasChildren)
            {
                RectangleF childRect;
                GraphElement child;
                GraphElement[] children = this.Children;
                int i, length = children.Length;
                for (i = 0; i < length; i++)
                {
                    child = children[i];
                    childRect = child.ChildrenBoundingBox();
                    childRect.Offset(child.mPosX, child.mPosY);
                    bbox = RectangleF.Union(bbox, childRect);
                }
            }
            return bbox;
        }

        /// <summary>
        /// Propagates an invalid area in this element's 
        /// local coordinate system up its ancestry chain,
        /// adjusting it to the coordinate system of the parentless root
        /// (the scene element), which then calls its 
        /// <see cref="InvalidateScene(System.Drawing.RectangleF)"/>
        /// function with the adjusted area.</summary>
        /// <param name="rect">The invalid area in this element's
        /// local coordinate system to propagate up the ancestry chain.</param>
        protected void Invalidate(RectangleF rect)
        {
            GraphElement scene = null;
            GraphElement p = this;
            while (p != null)
            {
                rect.Offset(p.mPosX, p.mPosY);
                scene = p;
                p = p.parent;
            }
            scene.InvalidateScene(rect);
        }

        /// <summary>
        /// This function is called at the end of propagation chain of one of
        /// this element's children. It is only invoked in parentless elements,
        /// and is meant to be reimplemented to signal the 
        /// <see cref="System.Windows.Forms.Control"/> visualizing this "scene"
        /// element and its children to invalidate the given area.
        /// </summary>
        /// <param name="rect">The area to invalidate in coordinate system of
        /// this parentless element (in scene coordinates).</param>
        protected virtual void InvalidateScene(RectangleF rect)
        {
        }

        #region Drawing
        private const CombineMode kClipCombineMode = CombineMode.Replace;

        /// <summary>
        /// Whether or not this element is drawn.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Override in descendent classes to implement systems for
        /// visibility, opacity, etc. Please note that this affects
        /// this element only, while its children are drawn regardless
        /// (unless their own <see cref="IsDrawn()"/> implementations 
        /// return the same result).
        /// </remarks>
        protected virtual bool IsDrawn()
        {
            return true;
        }

        /// <summary>
        /// The main drawing method, which recursively draws this element
        /// and its children, handling clipping and transformation.
        /// </summary>
        /// <param name="e">The data from a 
        /// <see cref="E:System.Windows.Forms.Control.Paint"/> event,
        /// with its graphics and clipping rectangle adjusted to
        /// this element's local coordinate system.</param>
        public void OnPaint(PaintEventArgs e)
        {
            bool drawItem = this.IsDrawn() &&
                this.mBoundingW > 0 && this.mBoundingH > 0 &&
                this.BoundingBox.IntersectsWith(e.ClipRectangle);
            Region clipShape = this.Shape();
            bool inShape = clipShape.IsVisible(e.ClipRectangle);
            Graphics g = e.Graphics;

            if (!this.HasChildren || (!inShape && this.bClipsChildrenToShape))
            {
                // Just draw the element itself
                if (drawItem)
                {
                    if (this.bClipsToShape)
                    {
                        if (!inShape)
                            return;
                        GraphicsContainer con = g.BeginContainer();
                        g.SetClip(clipShape, kClipCombineMode);
                        this.OnDrawForeground(e);
                        this.OnDrawBackground(e);
                        g.EndContainer(con);
                    }
                    else
                    {
                        this.OnDrawForeground(e);
                        this.OnDrawBackground(e);
                    }
                }
                return;
            }

            GraphicsContainer clipContainer = null;

            if (this.bClipsChildrenToShape)
            {
                clipContainer = g.BeginContainer();
                g.SetClip(clipShape, kClipCombineMode);
            }

            GraphElement child;
            GraphElement[] children = this.Children;
            int i, length = children.Length;

            // Draw children behind
            for (i = 0; i < length; i++)
            {
                child = children[i];
                if (!child.bStacksBehindParent)
                    break;
                // Fix event args
                Rectangle clip = e.ClipRectangle; // copies it, right?
                clip.Offset(-(int)child.mPosX, -(int)child.mPosY);
                g.TranslateTransform(child.mPosX, child.mPosY);
                // Paint child
                child.OnPaint(new PaintEventArgs(g, clip));
                // Restore event args
                g.TranslateTransform(-child.mPosX, -child.mPosY);
            }

            // Draw the element's background
            if (drawItem && (inShape || !this.bClipsToShape))
            {
                if (clipContainer != null && !this.bClipsToShape)
                {
                    g.EndContainer(clipContainer);
                    clipContainer = null;
                }
                else if (clipContainer == null && this.bClipsToShape)
                {
                    clipContainer = g.BeginContainer();
                    g.SetClip(clipShape, kClipCombineMode);
                }

                this.OnDrawBackground(e);
            }

            if (clipContainer != null && !this.bClipsChildrenToShape)
            {
                g.EndContainer(clipContainer);
                clipContainer = null;
            }
            else if (clipContainer == null && this.bClipsChildrenToShape)
            {
                clipContainer = g.BeginContainer();
                g.SetClip(clipShape, kClipCombineMode);
            }

            // Draw children in front
            for (; i < length; i++)
            {
                child = children[i];
                // Fix event args
                Rectangle clip = e.ClipRectangle; // copies it, right?
                clip.Offset(-(int)child.mPosX, -(int)child.mPosY);
                g.TranslateTransform(child.mPosX, child.mPosY);
                // Paint child
                child.OnPaint(new PaintEventArgs(g, clip));
                // Restore event args
                g.TranslateTransform(-child.mPosX, -child.mPosY);
            }

            // Draw the element's foreground
            if (drawItem && (inShape || !this.bClipsToShape))
            {
                if (clipContainer != null && !this.bClipsToShape)
                {
                    g.EndContainer(clipContainer);
                    clipContainer = null;
                }
                else if (clipContainer == null && this.bClipsToShape)
                {
                    clipContainer = g.BeginContainer();
                    g.SetClip(clipShape, kClipCombineMode);
                }

                this.OnDrawForeground(e);
            }

            if (clipContainer != null)
            {
                g.EndContainer(clipContainer);
                clipContainer = null;
            }
        }

        internal static void SetGraphicsMode(Graphics g, bool fast)
        {
            if (fast)
            {
                g.SmoothingMode = SmoothingMode.HighSpeed;
                g.CompositingQuality = CompositingQuality.HighSpeed;
                g.InterpolationMode = InterpolationMode.Default;
            }
            else
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            }
        }

        private const System.Drawing.Imaging.PixelFormat kCachePixelFormat 
            = System.Drawing.Imaging.PixelFormat.Format32bppArgb;

        private bool bCacheBackground = false;
        private Bitmap mBackgroundCache = null;
        private int mBackAdjustX = 0;
        private int mBackAdjustY = 0;

        /// <summary>
        /// Whether or not this element's background is cached in an image or
        /// redrawn every time this element is invalidated.
        /// </summary>
        /// <remarks>
        /// Beware that this is automatically set to false if there is not
        /// enough memory available to create the cache, so try to use caching
        /// sparingly, especially on any element with a large 
        /// <see cref="BoundingBox"/>.
        /// </remarks>
        public bool CacheBackground
        {
            get { return this.bCacheBackground; }
            set
            {
                if (this.bCacheBackground != value)
                {
                    if (value)
                    {
                        this.bCacheBackground = this.RedrawBackgroundCache();
                    }
                    else
                    {
                        if (this.mBackgroundCache != null)
                        {
                            this.mBackgroundCache.Dispose();
                            this.mBackgroundCache = null;
                        }
                        this.bCacheBackground = false;
                    }
                }
            }
        }

        /// <summary>
        /// Draws this element's background from either a cached image or
        /// directly from the implementation of 
        /// <see cref="UserDrawBackground(System.Drawing.Graphics"/>,
        /// depending on the <see cref="CacheBackground"/> setting.
        /// </summary>
        /// <param name="e">The data from a 
        /// <see cref="E:System.Windows.Forms.Control.Paint"/> event,
        /// with its graphics and clipping rectangle adjusted to
        /// this element's local coordinate system.</param>
        protected virtual void OnDrawBackground(PaintEventArgs e)
        {
            GraphicsState prev = e.Graphics.Save();
            if (this.bCacheBackground)
            {
                if (this.mBackgroundCache == null && 
                    !this.RedrawBackgroundCache())
                {
                    this.bCacheBackground = false;
                    this.UserDrawBackground(e.Graphics);
                    e.Graphics.Restore(prev);
                    return;
                }
                SetGraphicsMode(e.Graphics, true);
                Rectangle src = new Rectangle(
                    e.ClipRectangle.X - this.mBoundingX + this.mBackAdjustX,
                    e.ClipRectangle.Y - this.mBoundingY + this.mBackAdjustY,
                    e.ClipRectangle.Width, e.ClipRectangle.Height);
                e.Graphics.DrawImage(this.mBackgroundCache, 
                    e.ClipRectangle, src, GraphicsUnit.Pixel);
            }
            else
            {
                this.UserDrawBackground(e.Graphics);
            }
            e.Graphics.Restore(prev);
        }

        /// <summary>
        /// Attempts to refit the background cache image inside 
        /// the new bounding box by cropping the current cache or
        /// adjusting the cache offsets or clearing the current cache
        /// so that it's redrawn on the next draw pass.
        /// </summary>
        /// <param name="newBBox">The new bounding box for this element.
        /// </param>
        private void RefitBackgroundCache(Rectangle newBBox)
        {
            if (this.bCacheBackground)
            {
                // Try to crop cache image if it already exists,
                // otherwise draw it.
                if (this.mBackgroundCache != null/* &&
                    newBBox.Width > 0 && newBBox.Height > 0/**/)
                {
                    Rectangle crop = new Rectangle(
                        newBBox.X - this.mBoundingX + this.mBackAdjustX,
                        newBBox.Y - this.mBoundingY + this.mBackAdjustY,
                        newBBox.Width, newBBox.Height);
                    if (crop.Width <= 0 || crop.Height <= 0)
                    {
                        // New bounding box is invalid, so dump cache
                        this.mBackgroundCache.Dispose();
                        this.mBackgroundCache = null;
                    }
                    else /**/if (crop.X > 0 && crop.Y > 0 &&
                        crop.Width < this.mBackgroundCache.Width &&
                        crop.Height < this.mBackgroundCache.Height)
                    {
                        try
                        {
                            Bitmap newCache = this.mBackgroundCache.Clone(crop, kCachePixelFormat);
                            // Switch out cache for cropped cache if successful
                            this.mBackgroundCache.Dispose();
                            this.mBackgroundCache = newCache;
                        }
                        catch
                        {
                            // Continue using current cache with adjusted offsets
                            this.mBackAdjustX = crop.X;
                            this.mBackAdjustY = crop.Y;
                        }
                    }
                    else
                    {
                        // If the crop doesn't fit, redraw anyway
                        this.mBackgroundCache.Dispose();
                        this.mBackgroundCache = null;
                    }
                }
            }
        }

        /// <summary>
        /// Clears the current background cache and redraws it using the
        /// <see cref="UserDrawBackground(System.Drawing.Graphics)"/> function.
        /// </summary>
        /// <returns>True if the cache image was successfully re-allocated
        /// and drawn to, false otherwise (not enough memory or empty 
        /// <see cref="BoundingBox"/>).</returns>
        private bool RedrawBackgroundCache()
        {
            if (this.mBoundingW > 0 && this.mBoundingH > 0)
            {
                if (this.mBackgroundCache != null)
                {
                    this.mBackgroundCache.Dispose();
                    this.mBackgroundCache = null;
                }
                try
                {
                    this.mBackgroundCache = new Bitmap(this.mBoundingW, this.mBoundingH, kCachePixelFormat);
                }
                catch
                {
                    this.mBackgroundCache = null;
                    return false;
                }
                Graphics g = Graphics.FromImage(this.mBackgroundCache);
                g.TranslateTransform(-this.mBoundingX, -this.mBoundingY);
                this.UserDrawBackground(g);
                g.Dispose();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Draws the visual contents of this element's background
        /// into the provided graphics in this element's local coordinate 
        /// system.</summary>
        /// <param name="g">The graphics into which the background is drawn.
        /// </param>
        /// <remarks>Make sure to constrain all everything drawn to within
        /// the boundaries of this element's <see cref="BoundingBox"/>.
        /// Otherwise, anything outside it could be clipped by the cache
        /// or cause rendering artifacts, depending on the 
        /// <see cref="CacheBackground"/> setting.</remarks>
        protected virtual void UserDrawBackground(Graphics g)
        {
        }

        protected virtual void OnDrawForeground(PaintEventArgs e)
        {
        }
        #endregion
    }
}