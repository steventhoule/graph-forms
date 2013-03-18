﻿using System;
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

                this.Invalidate(invalid);
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

        public void MoveBy(float dx, float dy)
        {
            this.SetPosition(this.mPosX + dx, this.mPosY + dy);
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
                // TODO: There could be a flaw in this algorithm in that
                // it might not compensate for relative position.
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

                this.OnPositionChanged();
            }
        }

        /// <summary>
        /// Reimplement this function to trigger events and other reactions
        /// to any change in this element's position in its 
        /// <see cref="Parent"/> element's coordinate system.
        /// </summary>
        protected virtual void OnPositionChanged()
        {
        }
        #endregion

        private float mScaleX = 1f;
        private float mScaleY = 1f;

        public SizeF Scale
        {
            get { return new SizeF(this.mScaleX, this.mScaleY); }
            set { this.SetScale(value.Width, value.Height); }
            
        }

        public virtual void SetScale(float sx, float sy)
        {
            if (this.mScaleX != sx || this.mScaleY != sy)
            {
                RectangleF bbox = this.bClipsChildrenToShape ?
                    this.BoundingBox : this.ChildrenBoundingBox();
                if (sx < this.mScaleX)
                    bbox.Width *= this.mScaleX / sx;
                if (sy < this.mScaleY)
                    bbox.Height *= this.mScaleY / sy;

                this.mScaleX = sx;
                this.mScaleY = sy;

                this.Invalidate(bbox);

                this.OnScaleChanged();
            }
        }

        protected virtual void OnScaleChanged()
        {
        }

        #region Mapping
        /// <summary>
        /// Calculates a vector for translating points from this element's
        /// local coordinate system to the coordinate system of the view
        /// visualizing parentless root (the scene element).
        /// </summary>
        /// <returns>A translating vector from this element to the control
        /// visualizing it.</returns>
        public SizeF SceneTranslate()
        {
            float dx = this.mPosX;
            float dy = this.mPosY;
            GraphElement p = this.parent;
            while (p != null)
            {
                dx += p.mPosX;
                dy += p.mPosY;
                p = p.parent;
            }
            return new SizeF(dx, dy);
        }

        public Matrix SceneTransform()
        {
            Matrix transform = new Matrix();
            transform.Scale(this.mScaleX, this.mScaleY);
            transform.Translate(this.mPosX, this.mPosY);
            GraphElement p = this.parent;
            while (p != null)
            {
                transform.Scale(p.mScaleX, p.mScaleY);
                transform.Translate(p.mPosX, p.mPosY);
                p = p.parent;
            }
            return transform;
        }

        /// <summary>
        /// Calculates a vector for translating points from this element's
        /// local coordinate system to <paramref name="other"/>'s 
        /// local coordinate system. If <paramref name="other"/> is null,
        /// a zero vector is return.
        /// </summary>
        /// <param name="other">The other coordinate system to translate
        /// local points into.</param>
        /// <returns>A translating vector from this element to the 
        /// <paramref name="other"/> element.</returns>
        public SizeF ItemTranslate(GraphElement other)
        {
            // Catch simple cases first.
            if (other == null || other == this)
            {
                return new SizeF(0, 0);
            }

            // This is other's child
            if (this.parent == other)
            {
                return new SizeF(this.mPosX, this.mPosY);
            }

            // This is other's parent
            if (other.parent == this)
            {
                return new SizeF(-other.mPosX, -other.mPosY);
            }

            // Siblings
            if (this.parent == other.parent)
            {
                return new SizeF(this.mPosX - other.mPosX,
                    this.mPosY - other.mPosY);
            }

            // Find the closest common ancestor.
            GraphElement thisw = this;
            GraphElement otherw = other;
            int thisDepth = this.Depth;
            int otherDepth = other.Depth;
            float thisX = 0, thisY = 0;
            float otherX = 0, otherY = 0;
            while (thisDepth > otherDepth)
            {
                thisX += thisw.mPosX; thisY += this.mPosY;
                thisw = thisw.parent;
                --thisDepth;
            }
            while (otherDepth > thisDepth)
            {
                otherX += other.mPosX; otherY += other.mPosY;
                otherw = other.parent;
                --otherDepth;
            }
            while (thisw != null && thisw != otherw)
            {
                thisX += thisw.mPosX; thisY += this.mPosY;
                thisw = thisw.parent;
                otherX += other.mPosX; otherY += other.mPosY;
                otherw = otherw.parent;
            }
            return new SizeF(thisX - otherX, thisY - otherY);
        }

        public Matrix ItemTransform(GraphElement other)
        {
            // Catch simple cases first.
            if (other == null || other == this)
            {
                return new Matrix();
            }

            Matrix transform = new Matrix();

            // This is other's child
            if (this.parent == other)
            {
                transform.Scale(this.mScaleX, this.mScaleY);
                transform.Translate(this.mPosX, this.mPosY);
                return transform;
            }

            // This is other's parent
            if (other.parent == this)
            {
                transform.Scale(1f / other.mScaleX, 1f / other.mScaleY);
                transform.Translate(-other.mPosX, -other.mPosY);
                return transform;
            }

            // Siblings
            if (this.parent == other.parent)
            {
                transform.Scale(this.mScaleX / other.mScaleX, this.mScaleY / other.mScaleY);
                transform.Translate(this.mPosX - other.mPosX, this.mPosY - other.mPosY);
                return transform;
            }

            // Find the closest common ancestor.
            GraphElement thisw = this;
            GraphElement otherw = other;
            int thisDepth = this.Depth;
            int otherDepth = other.Depth;
            Matrix otransform = new Matrix();
            while (thisDepth > otherDepth)
            {
                transform.Scale(thisw.mScaleX, thisw.mScaleY);
                transform.Translate(thisw.mPosX, thisw.mPosY);
                thisw = thisw.parent;
                --thisDepth;
            }
            while (otherDepth > thisDepth)
            {
                otransform.Scale(otherw.mScaleX, otherw.mScaleY);
                otransform.Translate(otherw.mPosX, otherw.mPosY);
                otherw = other.parent;
                --otherDepth;
            }
            while (thisw != null && thisw != otherw)
            {
                transform.Scale(thisw.mScaleX, thisw.mScaleY);
                transform.Translate(thisw.mPosX, thisw.mPosY);
                thisw = thisw.parent;
                otransform.Scale(otherw.mScaleX, otherw.mScaleY);
                otransform.Translate(otherw.mPosX, otherw.mPosY);
                otherw = otherw.parent;
            }
            otransform.Invert();
            transform.Multiply(otransform);
            return transform;
        }

        private PointF[] mMapHolder = new PointF[1];

        /// <summary>
        /// Maps a given point from this element's local coordinate system to
        /// the local coordinate system of its parent element.
        /// If this element has no parent, <paramref name="point"/> will be
        /// mapped to the coordinate system of the control visualizing this
        /// element.</summary>
        /// <param name="point">A point to map from this element to its parent
        /// (or the visualizing control if the parent is null).</param>
        /// <returns>The given <paramref name="point"/> mapped to the 
        /// coordinate system of this element's parent.</returns>
        public PointF MapToParent(PointF point)
        {
            return new PointF(point.X * this.mScaleX + this.mPosX, 
                point.Y * this.mScaleY + this.mPosY);
        }

        /// <summary>
        /// Maps a given point from this element's local coordinate system to
        /// the coordinate system of the control visualizing this element.
        /// </summary>
        /// <param name="point">A point to map from this element to the
        /// control visualizing this element.</param>
        /// <returns>The given <paramref name="point"/> mapped to the 
        /// coordinate system of the visualizing control.</returns>
        public PointF MapToScene(PointF point)
        {
            //return PointF.Add(point, this.SceneTranslate());
            Matrix transform = this.SceneTransform();
            this.mMapHolder[0] = point;
            transform.TransformPoints(this.mMapHolder);
            return this.mMapHolder[0];
        }

        /// <summary>
        /// Maps a given point from this element's local coordinate system to
        /// the local coordinate system of the given <paramref name="item"/>.
        /// If the given item is null, <paramref name="point"/> is mapped to
        /// the coordinate system of the control visualizing this element.
        /// </summary>
        /// <param name="item">The element to map the given 
        /// <paramref name="point"/> into from this element's local coordinate 
        /// system.</param>
        /// <param name="point">A point to map from this element into
        /// the given <paramref name="item"/>.</param>
        /// <returns>The given <paramref name="point"/> mapped to the local
        /// coordinate system of the given <paramref name="item"/>.</returns>
        public PointF MapToItem(GraphElement item, PointF point)
        {
            //if (item != null)
            //    return PointF.Add(point, this.ItemTranslate(item));
            //return PointF.Add(point, this.SceneTranslate());
            Matrix transform;
            if (item != null)
                transform = this.ItemTransform(item);
            else
                transform = this.SceneTransform();
            this.mMapHolder[0] = point;
            transform.TransformPoints(this.mMapHolder);
            return this.mMapHolder[0];
        }

        public PointF MapFromParent(PointF point)
        {
            return new PointF((point.X - this.mPosX) / this.mScaleX, 
                (point.Y - this.mPosY) / this.mScaleY);
        }

        public PointF MapFromScene(PointF point)
        {
            //return PointF.Subtract(point, this.SceneTranslate());
            Matrix transform = this.SceneTransform();
            transform.Invert();
            this.mMapHolder[0] = point;
            transform.TransformPoints(this.mMapHolder);
            return this.mMapHolder[0];
        }

        public PointF MapFromItem(GraphElement item, PointF point)
        {
            //if (item != null)
            //    return PointF.Add(point, item.ItemTranslate(this));
            //return PointF.Subtract(point, this.SceneTranslate());
            Matrix transform;
            if (item != null)
                transform = item.ItemTransform(this);
            else
            {
                transform = this.SceneTransform();
                transform.Invert();
            }
            this.mMapHolder[0] = point;
            transform.TransformPoints(this.mMapHolder);
            return this.mMapHolder[0];
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
                rect.Width *= p.mScaleX;
                rect.Height *= p.mScaleY;
                rect.Offset(p.mPosX, p.mPosY);
                scene = p;
                p = p.parent;
            }
            int x = (int)Math.Floor(rect.X);
            int y = (int)Math.Floor(rect.Y);
            int w = (int)Math.Ceiling(rect.X + rect.Width) - x;
            int h = (int)Math.Ceiling(rect.Y + rect.Height) - y;
            scene.InvalidateScene(new Rectangle(x, y, w, h));
        }

        /// <summary>
        /// This function is called at the end of propagation chain of one of
        /// this element's children. It is only invoked in parentless elements,
        /// and is meant to be reimplemented to signal the 
        /// <see cref="System.Windows.Forms.Control"/> visualizing this "scene"
        /// element and its children to invalidate the given area.
        /// </summary>
        /// <param name="rect">The area to invalidate in coordinate system of
        /// the control visualizing this parentless element and its children.
        /// </param>
        protected virtual void InvalidateScene(Rectangle rect)
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
                clip.Width = (int)(clip.Width / child.mScaleX);
                clip.Height = (int)(clip.Height / child.mScaleY);
                g.ScaleTransform(child.mScaleX, child.mScaleY);
                g.TranslateTransform(child.mPosX, child.mPosY);
                // Paint child
                child.OnPaint(new PaintEventArgs(g, clip));
                // Restore event args
                g.TranslateTransform(-child.mPosX, -child.mPosY);
                g.ScaleTransform(1f / child.mScaleX, 1f / child.mScaleY);
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
                clip.Width = (int)(clip.Width / child.mScaleX);
                clip.Height = (int)(clip.Height / child.mScaleY);
                g.ScaleTransform(child.mScaleX, child.mScaleY);
                g.TranslateTransform(child.mPosX, child.mPosY);
                // Paint child
                child.OnPaint(new PaintEventArgs(g, clip));
                // Restore event args
                g.TranslateTransform(-child.mPosX, -child.mPosY);
                g.ScaleTransform(1f / child.mScaleX, 1f / child.mScaleY);
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
        /// <see cref="UserDrawBackground(System.Drawing.Graphics)"/>,
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

        private delegate void MouseEventTrigger(GraphElement sender, GraphMouseEventArgs e);

        public GraphMouseEventArgs FixMouseEventArgs(GraphMouseEventArgs e)
        {
            return new GraphMouseEventArgs(e, 
                (e.X - this.mPosX) / this.mScaleX, 
                (e.Y - this.mPosY) / this.mScaleY);
        }

        public GraphMouseEventArgs FixMouseEventArgs(MouseEventArgs e)
        {
            return new GraphMouseEventArgs(e, 
                (e.X - this.mPosX) / this.mScaleX, 
                (e.Y - this.mPosY) / this.mScaleY);
        }

        private bool FireMouseEvent(MouseEventTrigger trigger, GraphMouseEventArgs e)
        {
            bool inShape = this.Contains(e.Pos);
            if (this.HasChildren && (inShape || !this.bClipsChildrenToShape))
            {
                GraphElement child;
                GraphElement[] children = this.Children;
                int i = children.Length - 1;
                for (; i >= 0; i--)
                {
                    child = children[i];
                    if (child.bStacksBehindParent)
                        break;
                    if (child.FireMouseEvent(trigger, 
                        child.FixMouseEventArgs(e)) &&
                        e.Handled)
                        return true;
                }
                if (this.BoundingBox.Contains(e.Location) &&
                    (inShape || !this.bClipsToShape))
                {
                    trigger(this, e);
                    if (e.Handled)
                        return true;
                }
                for (; i >= 0; i--)
                {
                    child = children[i];
                    if (child.FireMouseEvent(trigger, 
                        child.FixMouseEventArgs(e))&& 
                        e.Handled)
                        return true;
                }
            }
            if (this.BoundingBox.Contains(e.Location) && 
                (inShape || !this.bClipsToShape))
            {
                trigger(this, e);
                if (e.Handled)
                    return true;
            }
            return false;
        }

        public bool FireMouseClick(GraphMouseEventArgs e)
        {
            return this.FireMouseEvent(TriggerMouseClick, e);
        }

        private static void TriggerMouseClick(GraphElement sender, GraphMouseEventArgs e)
        {
            sender.OnMouseClick(e);
        }

        protected virtual void OnMouseClick(GraphMouseEventArgs e)
        {
            e.Handled = true;
        }

        public bool FireMouseDoubleClick(GraphMouseEventArgs e)
        {
            return this.FireMouseEvent(TriggerMouseDoubleClick, e);
        }

        private static void TriggerMouseDoubleClick(GraphElement sender, GraphMouseEventArgs e)
        {
            sender.OnMouseDoubleClick(e);
        }

        protected virtual void OnMouseDoubleClick(GraphMouseEventArgs e)
        {
            e.Handled = true;
        }

        public bool FireMouseDown(GraphMouseEventArgs e)
        {
            return this.FireMouseEvent(TriggerMouseDown, e);
        }

        private static void TriggerMouseDown(GraphElement sender, GraphMouseEventArgs e)
        {
            sender.OnMouseDown(e);
        }

        protected virtual void OnMouseDown(GraphMouseEventArgs e)
        {
            e.Handled = true;
        }

        public bool FireMouseMove(GraphMouseEventArgs e)
        {
            return this.FireMouseEvent(TriggerMouseMove, e);
        }

        private static void TriggerMouseMove(GraphElement sender, GraphMouseEventArgs e)
        {
            sender.OnMouseMove(e);
        }

        protected virtual void OnMouseMove(GraphMouseEventArgs e)
        {
            e.Handled = true;
        }

        public bool FireMouseUp(GraphMouseEventArgs e)
        {
            return this.FireMouseEvent(TriggerMouseUp, e);
        }

        private static void TriggerMouseUp(GraphElement sender, GraphMouseEventArgs e)
        {
            sender.OnMouseUp(e);
        }

        protected virtual void OnMouseUp(GraphMouseEventArgs e)
        {
            e.Handled = true;
        }

        public bool FireMouseWheel(GraphMouseEventArgs e)
        {
            return this.FireMouseEvent(TriggerMouseWheel, e);
        }

        private static void TriggerMouseWheel(GraphElement sender, GraphMouseEventArgs e)
        {
            sender.OnMouseWheel(e);
        }

        protected virtual void OnMouseWheel(GraphMouseEventArgs e)
        {
            e.Handled = true;
        }
    }
}