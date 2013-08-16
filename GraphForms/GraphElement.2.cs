using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace GraphForms
{
    public abstract partial class GraphElement
    {
        #region Hit Testing
        private float mBX = 0;
        private float mBY = 0;
        private float mBW = 0;
        private float mBH = 0;

        /// <summary>
        /// This element's bounding box in its local coordinate system.
        /// This must be initialized as it's used for drawing invalidation
        /// and quick hit testing before the more complex hit test
        /// functions are called.
        /// </summary>
        public RectangleF BoundingBox
        {
            get 
            { 
                return new RectangleF(this.mBX, this.mBY, this.mBW, this.mBH);
            }
            set
            {
                if (this.mCacheList != null && this.mCacheList.Count > 0)
                {
                    for (int i = this.mCacheList.Count - 1; i >= 0; i--)
                        this.mCacheList[i].RefitCache(value);
                }

                RectangleF invalid = RectangleF.Union(this.BoundingBox, value);

                this.mBX = value.X;
                this.mBY = value.Y;
                this.mBW = value.Width;
                this.mBH = value.Height;

                this.Invalidate(invalid);
            }
        }

        /// <summary>
        /// This element's <see cref="BoundingBox"/> mapped to the coordinate 
        /// system of the "scene" (the control rendering this element).
        /// </summary>
        public RectangleF SceneBoundingBox
        {
            get
            {
                RectangleF rect = new RectangleF(this.mBX, this.mBY, 
                    this.mBW, this.mBH);
                GraphElement p = this;
                while (p != null)
                {
                    rect.Offset(p.mPosX, p.mPosY);
                    p = p.parent;
                }
                return rect;
            }
        }

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
                    this.OnClipsChildrenToShapeChanged();
                    this.Invalidate(this.ChildrenBoundingBox());
                }
            }
        }

        /// <summary>
        /// Reimplement this function to trigger events and other reactions to 
        /// any change in this element's <see cref="ClipsChildrenToShape"/>
        /// value before it's invalidated.
        /// </summary>
        protected virtual void OnClipsChildrenToShapeChanged()
        {
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
                    this.OnClipsToShapeChanged();
                    this.Invalidate();
                }
            }
        }

        /// <summary>
        /// Reimplement this function to trigger events and other reactions to 
        /// any change in this element's <see cref="ClipsToShape"/>
        /// value before it's invalidated.
        /// </summary>
        protected virtual void OnClipsToShapeChanged()
        {
        }
        #endregion

        #region Position
        private float mPosX;
        private float mPosY;

        /// <summary>
        /// The x-coordinate of this element's position in its 
        /// <see cref="Parent"/> element's coordinate system.
        /// </summary>
        public float X
        {
            get { return this.mPosX; }
            set { this.SetPosition(value, this.mPosY); }
        }

        /// <summary>
        /// The y-coordinate of this element's position in its 
        /// <see cref="Parent"/> element's coordinate system.
        /// </summary>
        public float Y
        {
            get { return this.mPosY; }
            set { this.SetPosition(this.mPosX, value); }
        }

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
        /// Moves this element by <paramref name="dx"/> points horizontally,
        /// and by <paramref name="dy"/> points vertically. This function is
        /// equivalent to calling 
        /// <see cref="SetPosition(System.Single,System.Single)"/> with 
        /// <c><see cref="X"/> + <paramref name="dx"/></c> and
        /// <c><see cref="Y"/> + <paramref name="dy"/></c> as the arguments.
        /// </summary>
        /// <param name="dx">The amount by which to move this element 
        /// horizontally.</param>
        /// <param name="dy">The amount by which to move this element
        /// vertically.</param>
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
                float offsetX = x - this.mPosX;
                float offsetY = y - this.mPosY;
                RectangleF bbox = this.bClipsChildrenToShape ?
                    this.BoundingBox : this.ChildrenBoundingBox();
                // Union of bbox and offset bbox (before and after movement)
                bbox.Inflate(Math.Abs(offsetX), Math.Abs(offsetY));

                this.mPosX = x;
                this.mPosY = y;

                // Notify and update
                this.OnPositionChanged();
                this.Invalidate(bbox);
            }
        }

        /// <summary>
        /// Reimplement this function to trigger events and other reactions to 
        /// any change in this element's <see cref="Position"/>
        /// value before it's invalidated.
        /// </summary>
        protected virtual void OnPositionChanged()
        {
        }
        #endregion

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

        /// <summary>
        /// Maps a given point in this element's local coordinate system to
        /// the local coordinate system of its parent element.
        /// If this element has no parent, <paramref name="point"/> will be
        /// mapped to the coordinate system of the control visualizing this
        /// element.</summary>
        /// <param name="point">A point in this element's local coordinate 
        /// system.</param>
        /// <returns>The given <paramref name="point"/> mapped to the 
        /// coordinate system of this element's parent.</returns>
        public PointF MapToParent(PointF point)
        {
            return new PointF(point.X + this.mPosX, point.Y + this.mPosY);
        }

        /// <summary>
        /// Maps the given point in this element's local coordinate system to
        /// the coordinate system of the control visualizing this element.
        /// </summary>
        /// <param name="point">A point in this element's local coordinate 
        /// system.</param>
        /// <returns>The given <paramref name="point"/> mapped to the 
        /// coordinate system of the visualizing control.</returns>
        public PointF MapToScene(PointF point)
        {
            return PointF.Add(point, this.SceneTranslate());
        }

        /// <summary>
        /// Maps the given point in this element's local coordinate system to
        /// the local coordinate system of the given <paramref name="item"/>.
        /// If the given item is null, <paramref name="point"/> is mapped to
        /// the coordinate system of the control visualizing this element.
        /// </summary>
        /// <param name="item">The element to map the given 
        /// <paramref name="point"/> into from this element's local coordinate 
        /// system.</param>
        /// <param name="point">A point in this element's local coordinate
        /// system.</param>
        /// <returns>The given <paramref name="point"/> mapped to the local
        /// coordinate system of the given <paramref name="item"/>.</returns>
        public PointF MapToItem(GraphElement item, PointF point)
        {
            if (item != null)
                return PointF.Add(point, this.ItemTranslate(item));
            return PointF.Add(point, this.SceneTranslate());
        }

        /// <summary>
        /// Maps the given point, which is in this element's parent's local 
        /// coordinate system, to this element's local coordinate system.
        /// </summary>
        /// <param name="point">A point in this element's parent's local
        /// coordinate system.</param>
        /// <returns>The given <paramref name="point"/> mapped to this
        /// element's local coordinate system.</returns>
        public PointF MapFromParent(PointF point)
        {
            return new PointF(point.X - this.mPosX, point.Y - this.mPosY);
        }

        /// <summary>
        /// Maps the given point, which is in the coordinate system of
        /// the control visualizing this element, to this element's
        /// local coordinate system.
        /// </summary>
        /// <param name="point">A point in the coordinate system of
        /// the control visualizing this element.</param>
        /// <returns>The given <paramref name="point"/> mapped to this
        /// element's local coordinate system.</returns>
        public PointF MapFromScene(PointF point)
        {
            return PointF.Subtract(point, this.SceneTranslate());;
        }

        /// <summary>
        /// Maps the given point, which is in the given 
        /// <paramref name="item"/>'s local coordinate system, 
        /// to this element's local coordinate system. 
        /// If <paramref name="item"/> is null, <paramref name="point"/>
        /// is assumed to be in the coordinate system of the control
        /// visualizing this element.</summary>
        /// <param name="item">The element that defines the local
        /// coordinate system that the given <paramref name="point"/> is in.
        /// </param>
        /// <param name="point">A point in the local coordinate system of
        /// the given <paramref name="item"/>.</param>
        /// <returns>The given <paramref name="point"/> mapped to this
        /// element's local coordinate system.</returns>
        public PointF MapFromItem(GraphElement item, PointF point)
        {
            if (item != null)
                return PointF.Add(point, item.ItemTranslate(this));
            return PointF.Subtract(point, this.SceneTranslate());
        }
        #endregion

        #region Updating
        /// <summary>
        /// Propagates this element's <see cref="BoundingBox"/> up its 
        /// ancestry chain, adjusting it to the coordinate system of the 
        /// parentless root (the scene element), which then calls its 
        /// <see cref="InvalidateScene(Rectangle)"/>
        /// function with the adjusted bounding box. Useful for completely
        /// refreshing this element.</summary>
        protected void Invalidate()
        {
            this.Invalidate(new RectangleF(this.mBX, this.mBY, this.mBW, this.mBH));
        }

        /// <summary>
        /// Propagates an invalid area in this element's 
        /// local coordinate system up its ancestry chain,
        /// adjusting it to the coordinate system of the parentless root
        /// (the scene element), which then calls its 
        /// <see cref="InvalidateScene(Rectangle)"/>
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
            scene.InvalidateScene(GraphHelpers.ToAlignedRect(rect));
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
        #endregion

        #region Drawing
        private const CombineMode kClipCombineMode = CombineMode.Replace;

        /// <summary>
        /// Whether or not this element is drawn.
        /// </summary>
        /// <returns>True if this element is drawn, false if it isn't drawn.
        /// </returns><remarks>
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
                this.mBW > 0 && this.mBH > 0 &&
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

        /// <summary>
        /// This base class is used for caching some or all of a
        /// <see cref="GraphElement"/>'s visual contents in order to accelerate
        /// the drawing process for the element.
        /// </summary><remarks>
        /// Descendents of this class are meant to be used in implementations of the 
        /// <see cref="OnDrawBackground(System.Windows.Forms.PaintEventArgs)"/> and
        /// <see cref="OnDrawForeground(System.Windows.Forms.PaintEventArgs)"/>
        /// functions by calling its 
        /// <see cref="Cache.OnDraw(System.Windows.Forms.PaintEventArgs)"/> function.
        /// </remarks>
        public abstract class Cache : IDisposable
        {
            private const System.Drawing.Imaging.PixelFormat kCachePixelFormat
                = System.Drawing.Imaging.PixelFormat.Format32bppArgb;

            private GraphElement mOwner;

            private bool bCached;
            private Bitmap mCache = null;
            private int mAdjustX;
            private int mAdjustY;

            /// <summary>
            /// Initializes a new instance of the <see cref="Cache"/> class
            /// with the given <paramref name="owner"/>.
            /// </summary>
            /// <param name="owner">The <see cref="GraphElement"/> that will
            /// own this cache instance and refit it whenever necessary.
            /// </param>
            public Cache(GraphElement owner) : this(owner, false) { }

            /// <summary>
            /// Initializes a new instance of the <see cref="Cache"/> class
            /// with the given <paramref name="owner"/> and cache mode.
            /// </summary>
            /// <param name="owner">The <see cref="GraphElement"/> that will
            /// own this cache instance and refit it whenever necessary.
            /// </param>
            /// <param name="cached">Whether or not this cache is cached in an
            /// image or redrawn every time its <see cref="Owner"/> is 
            /// invalidated.</param>
            public Cache(GraphElement owner, bool cached)
            {
                this.bCached = cached;
                this.SetOwner(owner);
            }

            /// <summary>
            /// The element that owns this cache and refits it every time its
            /// <see cref="GraphElement.BoundingBox"/> is changed.
            /// </summary>
            public GraphElement Owner
            {
                get { return this.mOwner; }
            }

            /// <summary>
            /// Sets this cache's owner to the given <paramref name="owner"/>,
            /// and attempts to refit its internal cache image to the new
            /// owner's <see cref="GraphElement.BoundingBox"/> and redraw it
            /// if <see cref="Cached"/> is true and it isn't already cached.
            /// </summary>
            /// <param name="owner">The new owner of this cache.</param>
            internal void SetOwner(GraphElement owner)
            {
                if (this.mOwner != owner)
                {
                    if (owner != null)
                        this.RefitCache(owner.BoundingBox);
                    this.mOwner = owner;
                    if (this.bCached && this.mCache == null)
                        this.bCached = this.RedrawCache();
                }
            }

            /// <summary>
            /// Disposes this cache's internal image data if it has any.
            /// </summary>
            public void Dispose()
            {
                if (this.mCache != null)
                {
                    this.mCache.Dispose();
                    this.mCache = null;
                    this.SetOwner(null);
                }
            }

            /// <summary>
            /// Whether or not this cache is cached in an image or redrawn 
            /// every time its <see cref="Owner"/> element is invalidated.
            /// </summary>
            /// <remarks>
            /// Beware that this is automatically set to false if there is not
            /// enough memory available to create the cache, so try to use caching
            /// sparingly, especially on any element with a large 
            /// <see cref="GraphElement.BoundingBox"/>.
            /// </remarks>
            public bool Cached
            {
                get { return this.bCached; }
                set
                {
                    if (this.bCached != value)
                    {
                        if (value)
                        {
                            this.bCached = this.RedrawCache();
                        }
                        else
                        {
                            if (this.mCache != null)
                            {
                                this.mCache.Dispose();
                                this.mCache = null;
                            }
                            this.bCached = false;
                        }
                    }
                }
            }

            private static void SetFastGraphicsMode(Graphics g)
            {
                g.SmoothingMode = SmoothingMode.HighSpeed;
                g.CompositingQuality = CompositingQuality.HighSpeed;
                g.InterpolationMode = InterpolationMode.Default;
            }

            /// <summary>
            /// The main drawing function, which draws into the given graphics
            /// from either a cached image or directly from the implementation
            /// of <see cref="UserDraw(System.Drawing.Graphics)"/>,
            /// depending on the <see cref="Cached"/> setting.
            /// </summary>
            /// <param name="e">The data from a 
            /// <see cref="E:System.Windows.Forms.Control.Paint"/> event,
            /// with its graphics and clipping rectangle adjusted to
            /// the <see cref="Owner"/> element's local coordinate system.
            /// </param>
            public void OnDraw(PaintEventArgs e)
            {
                GraphicsState prev = e.Graphics.Save();
                if (this.bCached)
                {
                    if (this.mCache == null &&
                        !this.RedrawCache())
                    {
                        this.bCached = false;
                        this.UserDraw(e.Graphics);
                        e.Graphics.Restore(prev);
                        return;
                    }
                    SetFastGraphicsMode(e.Graphics);
                    Rectangle src = new Rectangle(
                        -(int)Math.Floor(this.mOwner.mBX),
                        -(int)Math.Floor(this.mOwner.mBY),
                        e.ClipRectangle.Width, e.ClipRectangle.Height);
                    src.Offset(e.ClipRectangle.X + this.mAdjustX,
                               e.ClipRectangle.Y + this.mAdjustY);
                    e.Graphics.DrawImage(this.mCache,
                        e.ClipRectangle, src, GraphicsUnit.Pixel);
                }
                else
                {
                    this.UserDraw(e.Graphics);
                }
                e.Graphics.Restore(prev);
            }

            /// <summary>
            /// Attempts to refit the internal cache image inside 
            /// the new bounding box by cropping the current cache or
            /// adjusting the cache offsets or clearing the current cache
            /// so that it's redrawn on the next draw pass.
            /// </summary>
            /// <param name="newBBox">The new bounding box for the
            /// <see cref="Owner"/> element.</param>
            internal void RefitCache(RectangleF newBBox)
            {
                // Try to crop cache image if it already exists,
                // otherwise draw it.
                if (this.mOwner != null && this.bCached && this.mCache != null
                    /*&& newBBox.Width > 0 && newBBox.Height > 0/* */)
                {
                    Rectangle crop = GraphHelpers.ToAlignedRect(
                        newBBox.X - this.mOwner.mBX,
                        newBBox.Y - this.mOwner.mBY,
                        newBBox.Width, newBBox.Height);
                    crop.Offset(this.mAdjustX, this.mAdjustY);
                    if (crop.Width <= 0 || crop.Height <= 0)
                    {
                        // New bounding box is invalid, so dump cache
                        this.mCache.Dispose();
                        this.mCache = null;
                    }
                    else /* */if (crop.X > 0 && crop.Y > 0 &&
                        crop.Width < this.mCache.Width &&
                        crop.Height < this.mCache.Height)
                    {
                        try
                        {
                            Bitmap newCache = this.mCache.Clone(crop, 
                                kCachePixelFormat);
                            // If successful, 
                            // switch out cache for cropped cache
                            this.mCache.Dispose();
                            this.mCache = newCache;
                        }
                        catch
                        {
                            // Continue using current cache 
                            // with adjusted offsets
                            this.mAdjustX = crop.X;
                            this.mAdjustY = crop.Y;
                        }
                    }
                    else
                    {
                        // If the crop doesn't fit, redraw anyway
                        this.mCache.Dispose();
                        this.mCache = null;
                    }
                }
            }

            /// <summary>
            /// Clears the current background cache and redraws it using the
            /// <see cref="UserDraw(System.Drawing.Graphics)"/> function.
            /// </summary>
            /// <returns>True if the cache image was successfully re-allocated
            /// and drawn to, false otherwise (not enough memory or empty 
            /// <see cref="GraphElement.BoundingBox"/>).</returns>
            private bool RedrawCache()
            {
                if (mOwner != null && 
                    mOwner.mBW > 0 && mOwner.mBH > 0)
                {
                    if (this.mCache != null)
                    {
                        this.mCache.Dispose();
                        this.mCache = null;
                    }
                    try
                    {
                        int w = (int)Math.Floor(this.mOwner.mBX);
                        int h = (int)Math.Floor(this.mOwner.mBY);
                        w = (int)Math.Ceiling(this.mOwner.mBX + this.mOwner.mBW) - w;
                        h = (int)Math.Ceiling(this.mOwner.mBY + this.mOwner.mBH) - h;
                        this.mCache = new Bitmap(w, h, kCachePixelFormat);
                    }
                    catch
                    {
                        this.mCache = null;
                        return false;
                    }
                    Graphics g = Graphics.FromImage(this.mCache);
                    g.TranslateTransform(-mOwner.mBX, -mOwner.mBY);
                    this.UserDraw(g);
                    g.Dispose();
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Draws the visual contents of this cache into the provided 
            /// graphics in the <see cref="Owner"/> element's local coordinate 
            /// system.</summary>
            /// <param name="g">The graphics into which the visual contents 
            /// are drawn.</param>
            /// <remarks>Make sure to constrain all everything drawn to within
            /// the boundaries of the <see cref="Owner"/> element's 
            /// <see cref="GraphElement.BoundingBox"/>.
            /// Otherwise, anything outside it could be clipped by the cache
            /// or cause rendering artifacts, depending on the 
            /// <see cref="Cached"/> setting.</remarks>
            protected abstract void UserDraw(Graphics g);
        }

        private volatile List<Cache> mCacheList = null;

        /// <summary>
        /// This element's internal list of all the <see cref="Cache"/> 
        /// instances it owns.
        /// </summary>
        public Cache[] CacheList
        {
            get 
            { 
                return this.mCacheList == null 
                    ? null : this.mCacheList.ToArray(); 
            }
        }

        /// <summary>
        /// Adds the given <paramref name="cache"/> to this element, 
        /// removing it from its previous owner first if it has a different
        /// owner, and then setting its <see cref="Cache.Owner"/> to this
        /// element. </summary>
        /// <param name="cache">The cache to add to this element.</param>
        public void AddCache(Cache cache)
        {
            if (this.mCacheList == null || 
                !this.mCacheList.Contains(cache))
            {
                GraphElement pOwner = cache.Owner;
                if (pOwner != this)
                {
                    if (pOwner != null)
                        pOwner.RemoveCache(cache);
                    cache.SetOwner(this);
                }
                if (this.mCacheList == null)
                    this.mCacheList = new List<Cache>();
                this.mCacheList.Add(cache);
            }
        }

        /// <summary>
        /// Removes the given <paramref name="cache"/> from this element 
        /// if it is owned by this element and contained in this element's 
        /// internal cache list, and then sets its <see cref="Cache.Owner"/> 
        /// to null.</summary>
        /// <param name="cache">The cache to remove from this element.</param>
        /// <returns>True if the cache was owned by and contained in this
        /// element and successfully removed from it, false otherwise.
        /// </returns>
        public bool RemoveCache(Cache cache)
        {
            int index = this.mCacheList == null 
                ? -1 : this.mCacheList.IndexOf(cache);
            if (index >= 0)
            {
                this.mCacheList.RemoveAt(index);
                if (this.mCacheList.Count == 0)
                    this.mCacheList = null;
                cache.SetOwner(null);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes all <see cref="Cache"/> instances from this element's
        /// internal cache list and sets their <see cref="Cache.Owner"/>
        /// to null.
        /// </summary>
        public void ClearCache()
        {
            if (this.mCacheList != null)
            {
                for (int i = this.mCacheList.Count - 1; i >= 0; i--)
                    this.mCacheList[i].SetOwner(null);
                //this.mCacheList.Clear(); // No need; GC'd regardless
                this.mCacheList = null;
            }
        }

        // TODO: Add cache addition and removal functions

        /// <summary>
        /// Draws the visual contents of this element's background.
        /// </summary>
        /// <param name="e">The data from a 
        /// <see cref="E:System.Windows.Forms.Control.Paint"/> event,
        /// with its graphics and clipping rectangle adjusted to
        /// this element's local coordinate system.</param>
        protected abstract void OnDrawBackground(PaintEventArgs e);

        /// <summary>
        /// Draws the visual contents of this element's foreground.
        /// </summary>
        /// <param name="e">The data from a 
        /// <see cref="E:System.Windows.Forms.Control.Paint"/> event,
        /// with its graphics and clipping rectangle adjusted to
        /// this element's local coordinate system.</param>
        protected virtual void OnDrawForeground(PaintEventArgs e)
        {
        }
        #endregion

        #region Mouse Event Handling
        //private delegate void MouseEventTrigger(GraphElement sender, GraphMouseEventArgs e);

        private abstract class MouseEventTrigger
        {
            public abstract void Trigger(GraphElement sender, GraphMouseEventArgs e);
        }

        /// <summary>
        /// Offsets the <see cref="GraphMouseEventArgs.Pos"/> of the given 
        /// arguments, <paramref name="e"/>, so that the mouse position of the 
        /// new instance is in this element's local coordinate system.
        /// </summary>
        /// <param name="e">Event arguments with a mouse position to map into
        /// this element's local coordinate system.</param>
        /// <returns>A new <see cref="GraphMouseEventArgs"/> instance with its
        /// mouse position in this element's local coordinate system.</returns>
        public GraphMouseEventArgs FixMouseEventArgs(GraphMouseEventArgs e)
        {
            return new GraphMouseEventArgs(e, -this.mPosX, -this.mPosY);
        }

        private bool FireMouseEvent(MouseEventTrigger trigger, GraphMouseEventArgs e)
        {
            bool inShape = this.Contains(e.Pos);
            if (this.HasChildren && (inShape || !this.bClipsChildrenToShape))
            {
                GraphElement child;
                GraphElement[] children = this.Children;
                GraphMouseEventArgs ce;
                int i = children.Length - 1;
                for (; i >= 0; i--)
                {
                    child = children[i];
                    if (child.bStacksBehindParent)
                        break;
                    ce = child.FixMouseEventArgs(e);
                    if (child.FireMouseEvent(trigger, ce) && ce.Handled)
                        return true;
                }
                if (this.BoundingBox.Contains(e.X, e.Y) && inShape)
                    //(inShape || !this.bClipsToShape))
                {
                    trigger.Trigger(this, e);
                    if (e.Handled)
                        return true;
                }
                for (; i >= 0; i--)
                {
                    child = children[i];
                    ce = child.FixMouseEventArgs(e);
                    if (child.FireMouseEvent(trigger, ce) && ce.Handled)
                        return true;
                }
            }
            if (this.BoundingBox.Contains(e.X, e.Y) && inShape)
                //(inShape || !this.bClipsToShape))
            {
                trigger.Trigger(this, e);
                if (e.Handled)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Occurs when this element is clicked by the mouse.
        /// The default simply sets <paramref name="e"/>'s 
        /// <see cref="GraphEventArgs.Handled"/> value to true to avoid
        /// processing costs from further propagation.
        /// </summary>
        /// <param name="e">The mouse event data of this event.</param>
        protected virtual void OnMouseClick(GraphMouseEventArgs e)
        {
            //e.Handled = true;
        }

        private class MouseClickTrigger : MouseEventTrigger
        {
            public override void Trigger(GraphElement sender, GraphMouseEventArgs e)
            {
                sender.OnMouseClick(e);
            }
        }
        private static MouseEventTrigger sTriggerMouseClick = new MouseClickTrigger();

        /// <summary>
        /// Propagates a Mouse Click event to this element and its children, 
        /// calling the <see cref="OnMouseClick(GraphMouseEventArgs)"/> 
        /// function on any elements that contain <paramref name="e"/>'s 
        /// mouse position <see cref="GraphMouseEventArgs.Pos"/>.
        /// </summary>
        /// <param name="e">The event data of a Mouse Click event 
        /// to propagate.</param>
        /// <returns>True if the event was <see cref="GraphEventArgs.Handled"/>
        /// by one of the elements it was propagated to, or false if it was
        /// unhandled or no elements contained the event's mouse position
        /// <see cref="GraphMouseEventArgs.Pos"/>. </returns>
        protected bool FireMouseClick(GraphMouseEventArgs e)
        {
            return this.FireMouseEvent(sTriggerMouseClick, e);
        }

        /// <summary>
        /// Occurs when this element is double clicked by the mouse.
        /// The default simply sets <paramref name="e"/>'s 
        /// <see cref="GraphEventArgs.Handled"/> value to true to avoid
        /// processing costs from further propagation.
        /// </summary>
        /// <param name="e">The mouse event data of this event.</param>
        protected virtual void OnMouseDoubleClick(GraphMouseEventArgs e)
        {
            //e.Handled = true;
        }

        private class MouseDoubleClickTrigger : MouseEventTrigger
        {
            public override void Trigger(GraphElement sender, GraphMouseEventArgs e)
            {
                sender.OnMouseDoubleClick(e);
            }
        }
        private static MouseEventTrigger sTriggerMouseDoubleClick = new MouseDoubleClickTrigger();

        /// <summary>
        /// Propagates a Mouse DoubleClick event to this element and its children, 
        /// calling the <see cref="OnMouseDoubleClick(GraphMouseEventArgs)"/> 
        /// function on any elements that contain <paramref name="e"/>'s 
        /// mouse position <see cref="GraphMouseEventArgs.Pos"/>.
        /// </summary>
        /// <param name="e">The event data of a Mouse DoubleClick event 
        /// to propagate.</param>
        /// <returns>True if the event was <see cref="GraphEventArgs.Handled"/>
        /// by one of the elements it was propagated to, or false if it was
        /// unhandled or no elements contained the event's mouse position
        /// <see cref="GraphMouseEventArgs.Pos"/>. </returns>
        protected bool FireMouseDoubleClick(GraphMouseEventArgs e)
        {
            return this.FireMouseEvent(sTriggerMouseDoubleClick, e);
        }

        /// <summary>
        /// Occurs when the mouse pointer is over this element and
        /// a mouse button is pressed.
        /// The default simply sets <paramref name="e"/>'s 
        /// <see cref="GraphEventArgs.Handled"/> value to true to avoid
        /// processing costs from further propagation.
        /// </summary>
        /// <param name="e">The mouse event data of this event.</param>
        protected virtual void OnMouseDown(GraphMouseEventArgs e)
        {
            //e.Handled = true;
        }

        private class MouseDownTrigger : MouseEventTrigger
        {
            public override void Trigger(GraphElement sender, GraphMouseEventArgs e)
            {
                sender.OnMouseDown(e);
            }
        }
        private static MouseEventTrigger sTriggerMouseDown = new MouseDownTrigger();

        /// <summary>
        /// Propagates a Mouse Down event to this element and its children, 
        /// calling the <see cref="OnMouseDown(GraphMouseEventArgs)"/> 
        /// function on any elements that contain <paramref name="e"/>'s 
        /// mouse position <see cref="GraphMouseEventArgs.Pos"/>.
        /// </summary>
        /// <param name="e">The event data of a Mouse Down event 
        /// to propagate.</param>
        /// <returns>True if the event was <see cref="GraphEventArgs.Handled"/>
        /// by one of the elements it was propagated to, or false if it was
        /// unhandled or no elements contained the event's mouse position
        /// <see cref="GraphMouseEventArgs.Pos"/>. </returns>
        protected bool FireMouseDown(GraphMouseEventArgs e)
        {
            return this.FireMouseEvent(sTriggerMouseDown, e);
        }

        /// <summary>
        /// Occurs when the mouse pointer is moved over this element.
        /// The default simply sets <paramref name="e"/>'s 
        /// <see cref="GraphEventArgs.Handled"/> value to true to avoid
        /// processing costs from further propagation.
        /// </summary>
        /// <param name="e">The mouse event data of this event.</param>
        protected virtual void OnMouseMove(GraphMouseEventArgs e)
        {
            //e.Handled = true;
        }

        private class MouseMoveTrigger : MouseEventTrigger
        {
            public override void Trigger(GraphElement sender, GraphMouseEventArgs e)
            {
                sender.OnMouseMove(e);
            }
        }
        private static MouseEventTrigger sTriggerMouseMove = new MouseMoveTrigger();

        /// <summary>
        /// Propagates a Mouse Move event to this element and its children, 
        /// calling the <see cref="OnMouseMove(GraphMouseEventArgs)"/> 
        /// function on any elements that contain <paramref name="e"/>'s 
        /// mouse position <see cref="GraphMouseEventArgs.Pos"/>.
        /// </summary>
        /// <param name="e">The event data of a Mouse Move event 
        /// to propagate.</param>
        /// <returns>True if the event was <see cref="GraphEventArgs.Handled"/>
        /// by one of the elements it was propagated to, or false if it was
        /// unhandled or no elements contained the event's mouse position
        /// <see cref="GraphMouseEventArgs.Pos"/>. </returns>
        protected bool FireMouseMove(GraphMouseEventArgs e)
        {
            return this.FireMouseEvent(sTriggerMouseMove, e);
        }

        /// <summary>
        /// Occurs when the mouse pointer is over this element and
        /// a mouse button is released.
        /// The default simply sets <paramref name="e"/>'s 
        /// <see cref="GraphEventArgs.Handled"/> value to true to avoid
        /// processing costs from further propagation.
        /// </summary>
        /// <param name="e">The mouse event data of this event.</param>
        protected virtual void OnMouseUp(GraphMouseEventArgs e)
        {
            //e.Handled = true;
        }

        private class MouseUpTrigger : MouseEventTrigger
        {
            public override void Trigger(GraphElement sender, GraphMouseEventArgs e)
            {
                sender.OnMouseUp(e);
            }
        }
        private static MouseEventTrigger sTriggerMouseUp = new MouseUpTrigger();

        /// <summary>
        /// Propagates a Mouse Up event to this element and its children, 
        /// calling the <see cref="OnMouseUp(GraphMouseEventArgs)"/> 
        /// function on any elements that contain <paramref name="e"/>'s 
        /// mouse position <see cref="GraphMouseEventArgs.Pos"/>.
        /// </summary>
        /// <param name="e">The event data of a Mouse Up event 
        /// to propagate.</param>
        /// <returns>True if the event was <see cref="GraphEventArgs.Handled"/>
        /// by one of the elements it was propagated to, or false if it was
        /// unhandled or no elements contained the event's mouse position
        /// <see cref="GraphMouseEventArgs.Pos"/>. </returns>
        protected bool FireMouseUp(GraphMouseEventArgs e)
        {
            return this.FireMouseEvent(sTriggerMouseUp, e);
        }

        /// <summary>
        /// Occurs when the mouse pointer is over this element and
        /// the mouse wheel moves.
        /// The default simply sets <paramref name="e"/>'s 
        /// <see cref="GraphEventArgs.Handled"/> value to true to avoid
        /// processing costs from further propagation.
        /// </summary>
        /// <param name="e">The mouse event data of this event.</param>
        protected virtual void OnMouseWheel(GraphMouseEventArgs e)
        {
            //e.Handled = true;
        }

        private class MouseWheelTrigger : MouseEventTrigger
        {
            public override void Trigger(GraphElement sender, GraphMouseEventArgs e)
            {
                sender.OnMouseWheel(e);
            }
        }
        private static MouseEventTrigger sTriggerMouseWheel = new MouseWheelTrigger();

        /// <summary>
        /// Propagates a Mouse Wheel event to this element and its children, 
        /// calling the <see cref="OnMouseWheel(GraphMouseEventArgs)"/> 
        /// function on any elements that contain <paramref name="e"/>'s 
        /// mouse position <see cref="GraphMouseEventArgs.Pos"/>.
        /// </summary>
        /// <param name="e">The event data of a Mouse Wheel event 
        /// to propagate.</param>
        /// <returns>True if the event was <see cref="GraphEventArgs.Handled"/>
        /// by one of the elements it was propagated to, or false if it was
        /// unhandled or no elements contained the event's mouse position
        /// <see cref="GraphMouseEventArgs.Pos"/>. </returns>
        protected bool FireMouseWheel(GraphMouseEventArgs e)
        {
            return this.FireMouseEvent(sTriggerMouseWheel, e);
        }
        #endregion
    }
}