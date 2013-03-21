using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;

namespace GraphForms
{
    /// <summary>
    /// A global class containing static functions for helping with
    /// mathematics and drawing element manipulation.
    /// </summary>
    public static class GraphHelpers
    {
        #region Fuzziness
        private const float kSingleFuzziness = 0.00001f;
        private const double kDoubleFuzziness = 0.000000000001;

        /// <summary>
        /// Compares the two given numbers in a relative way,
        /// where the exactness is stronger the smaller the numbers are.
        /// </summary>
        /// <param name="p1">A number to compare with <paramref name="p2"/>.
        /// </param>
        /// <param name="p2">A number to compare with <paramref name="p1"/>.
        /// </param>
        /// <returns>True if <paramref name="p1"/> and <paramref name="p2"/>
        /// are considered equal, otherwise false.</returns>
        /// <remarks>
        /// Note that comparing values where either <paramref name="p1"/> or
        /// <paramref name="p2"/> is 0.0 will not work. The solution to this
        /// is to compare against values greater than or equal to 1.0.
        /// </remarks>
        public static bool FuzzyCompare(float p1, float p2)
        {
            return Math.Abs(p1 - p2) <= kSingleFuzziness * 
                Math.Min(Math.Abs(p1), Math.Abs(p2));
        }

        /// <summary>
        /// Compares the two given numbers in a relative way,
        /// where the exactness is stronger the smaller the numbers are.
        /// </summary>
        /// <param name="p1">A number to compare with <paramref name="p2"/>.
        /// </param>
        /// <param name="p2">A number to compare with <paramref name="p1"/>.
        /// </param>
        /// <returns>True if <paramref name="p1"/> and <paramref name="p2"/>
        /// are considered equal, otherwise false.</returns>
        /// <remarks>
        /// Note that comparing values where either <paramref name="p1"/> or
        /// <paramref name="p2"/> is 0.0 will not work. The solution to this
        /// is to compare against values greater than or equal to 1.0.
        /// </remarks>
        public static bool FuzzyCompare(double p1, double p2)
        {
            return Math.Abs(p1 - p2) <= kDoubleFuzziness * 
                Math.Min(Math.Abs(p1), Math.Abs(p2));
        }

        /// <summary>
        /// Returns whether or not the given number is relatively
        /// equal to zero (less than or equal to the single precision
        /// fuzziness factor 0.00001).
        /// </summary>
        /// <param name="f">The number to test if it's relatively zero.</param>
        /// <returns>True if <c><paramref name="f"/> &lt;= 0.00001</c>,
        /// false otherwise.</returns>
        public static bool FuzzyIsNull(float f)
        {
            return Math.Abs(f) <= kSingleFuzziness;
        }

        /// <summary>
        /// Returns whether or not the given number is relatively
        /// equal to zero (less than or equal to the double precision
        /// fuzziness factor 0.000000000001).
        /// </summary>
        /// <param name="d">The number to test if it's relatively zero.</param>
        /// <returns>True if <c><paramref name="d"/> &lt;= 0.000000000001</c>,
        /// false otherwise.</returns>
        public static bool FuzzyIsNull(double d)
        {
            return Math.Abs(d) <= kDoubleFuzziness;
        }
        #endregion

        /// <summary>
        /// Constrains a given <paramref name="value"/> to be between the 
        /// given <paramref name="min"/> and <paramref name="max"/> values.
        /// </summary>
        /// <param name="value">The value to constrain.</param>
        /// <param name="min">The minimum allowable value.</param>
        /// <param name="max">The maximum allowable value.</param>
        /// <returns><paramref name="min"/> if <paramref name="value"/> is
        /// less than <paramref name="min"/>, or <paramref name="max"/> if
        /// <paramref name="value"/> is greater than <paramref name="max"/>, 
        /// or <paramref name="value"/> in all other cases.</returns>
        public static float Clamp(float value, float min, float max)
        {
            return value > max ? max : (value < min ? min : value);
            //return Math.Max(Math.Min(value, max), min);
        }

        /// <summary>
        /// Constrains a given <paramref name="value"/> to be between the 
        /// given <paramref name="min"/> and <paramref name="max"/> values.
        /// </summary>
        /// <param name="value">The value to constrain.</param>
        /// <param name="min">The minimum allowable value.</param>
        /// <param name="max">The maximum allowable value.</param>
        /// <returns><paramref name="min"/> if <paramref name="value"/> is
        /// less than <paramref name="min"/>, or <paramref name="max"/> if
        /// <paramref name="value"/> is greater than <paramref name="max"/>, 
        /// or <paramref name="value"/> in all other cases.</returns>
        public static int Clamp(int value, int min, int max)
        {
            return value > max ? max : (value < min ? min : value);
            //return Math.Max(Math.Min(value, max), min);
        }

        #region RectangleF Functions
        /// <summary>
        /// Returns the smallest possible integer rectangle that
        /// completely contains the given <paramref name="rect"/>.
        /// </summary>
        /// <param name="rect">The rectangle to be aligned.</param>
        /// <returns>The smallest possible integer rectangle that
        /// completely contains the given <paramref name="rect"/>.
        /// </returns>
        public static Rectangle ToAlignedRect(RectangleF rect)
        {
            int x = (int)Math.Floor(rect.X);
            int y = (int)Math.Floor(rect.Y);
            return new Rectangle(x, y,
                (int)Math.Ceiling(rect.Right) - x,
                (int)Math.Ceiling(rect.Bottom) - y);
        }

        /// <summary>
        /// Returns the smallest possible integer rectangle that
        /// completely contains the given rectangle.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of 
        /// the rectangle to be aligned.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of
        /// the rectangle to be aligned.</param>
        /// <param name="width">The width of the rectangle to be aligned.
        /// </param>
        /// <param name="height">The height of the rectangle to be aligned.
        /// </param>
        /// <returns>The smallest possible integer rectangle that
        /// completely contains the given rectangle.</returns>
        public static Rectangle ToAlignedRect(float x, float y, 
            float width, float height)
        {
            int xi = (int)Math.Floor(x);
            int yi = (int)Math.Floor(y);
            return new Rectangle(xi, yi,
                (int)Math.Ceiling(x + width) - xi,
                (int)Math.Ceiling(y + height) - yi);
        }

        /// <summary>
        /// Returns the largest possible integer rectangle that is
        /// completely contained within the given <paramref name="rect"/>.
        /// </summary>
        /// <param name="rect">The rectangle to be inset.</param>
        /// <returns>The largest possible integer rectangle that is
        /// completely contained within the given <paramref name="rect"/>.
        /// </returns>
        public static Rectangle ToInsetRect(RectangleF rect)
        {
            int x = (int)Math.Ceiling(rect.X);
            int y = (int)Math.Ceiling(rect.Y);
            return new Rectangle(x, y,
                (int)Math.Floor(rect.Right) - x,
                (int)Math.Floor(rect.Bottom) - y);
        }

        /// <summary>
        /// Returns the largest possible integer rectangle that is
        /// completely contained within the given rectangle.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of 
        /// the rectangle to be inset.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of
        /// the rectangle to be inset.</param>
        /// <param name="width">The width of the rectangle to be inset.
        /// </param>
        /// <param name="height">The height of the rectangle to be inset.
        /// </param>
        /// <returns>The largest possible integer rectangle that is
        /// completely contained within the given rectangle.</returns>
        public static Rectangle ToInsetRect(float x, float y,
            float width, float height)
        {
            int xi = (int)Math.Ceiling(x);
            int yi = (int)Math.Ceiling(y);
            return new Rectangle(xi, yi,
                (int)Math.Floor(x + width) - xi,
                (int)Math.Floor(y + height) - yi);
        }

        /// <summary>
        /// Returns the integer rectangle that results from rounding all four
        /// corners of the given <paramref name="rect"/> to the nearest 
        /// integer.</summary>
        /// <param name="rect">The rectangle to be rounded.</param>
        /// <returns>the integer rectangle that results from rounding all four
        /// corners of the given <paramref name="rect"/> to the nearest 
        /// integer.</returns>
        public static Rectangle ToRoundedRect(RectangleF rect)
        {
            int x = (int)Math.Round(rect.X);
            int y = (int)Math.Round(rect.Y);
            return new Rectangle(x, y,
                (int)Math.Round(rect.Right) - x,
                (int)Math.Round(rect.Bottom) - y);
        }

        /// <summary>
        /// Returns the integer rectangle that results from rounding all four
        /// corners of the given rectangle to the nearest integer.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner of
        /// the rectangle to be rounded.</param>
        /// <param name="y">The y-coordinate of the upper-left corner of
        /// the rectangle to be rounded.</param>
        /// <param name="width">The width of the rectangle to be rounded.
        /// </param>
        /// <param name="height">The height of the rectangle to be rounded.
        /// </param>
        /// <returns>The integer rectangle that results from rounding all four
        /// corners of the given rectangle to the nearest integer.</returns>
        public static Rectangle ToRoundedRect(float x, float y,
            float width, float height)
        {
            int xi = (int)Math.Round(x);
            int yi = (int)Math.Round(y);
            return new Rectangle(xi, yi,
                (int)Math.Round(x + width) - xi,
                (int)Math.Round(y + height) - yi);
        }

        /// <summary>
        /// Uses the given <paramref name="matrix"/> to transform the corners
        /// of the given <paramref name="rect"/> and then returns the bounding
        /// box of that quadrilateral polygon. </summary>
        /// <param name="matrix">The matrix to use to transform the given
        /// <paramref name="rect"/>.</param>
        /// <param name="rect">The rectangle to be transformed by the given
        /// <paramref name="matrix"/>.</param>
        /// <returns>The bounding box of the resulting <paramref name="matrix"/>
        /// transformation of the given <paramref name="rect"/>. </returns>
        public static Rectangle MapRect(Matrix matrix, Rectangle rect)
        {
            if (matrix.IsIdentity)
                return rect;

            Point[] points = new Point[]
            {
                new Point(rect.X, rect.Y),
                new Point(rect.X + rect.Width, rect.Y),
                new Point(rect.X, rect.Y + rect.Height),
                new Point(rect.X + rect.Width, rect.Y + rect.Height)
            };
            matrix.TransformPoints(points);
            int x, y, xmin, xmax, ymin, ymax;
            xmin = xmax = points[0].X;
            ymin = ymax = points[0].Y;
            for (int i = 1; i < 4; i++)
            {
                x = points[i].X;
                y = points[i].Y;
                xmin = Math.Min(xmin, x);
                xmax = Math.Max(xmax, x);
                ymin = Math.Min(ymin, y);
                ymax = Math.Max(ymax, y);
            }
            return new Rectangle(xmin, ymin, xmax - xmin, ymax - ymin);
        }

        /// <summary>
        /// Uses the given <paramref name="matrix"/> to transform the corners
        /// of the given <paramref name="rect"/> and then returns the bounding
        /// box of that quadrilateral polygon. </summary>
        /// <param name="matrix">The matrix to use to transform the given
        /// <paramref name="rect"/>.</param>
        /// <param name="rect">The rectangle to be transformed by the given
        /// <paramref name="matrix"/>.</param>
        /// <returns>The bounding box of the resulting <paramref name="matrix"/>
        /// transformation of the given <paramref name="rect"/>. </returns>
        public static RectangleF MapRect(Matrix matrix, RectangleF rect)
        {
            if (matrix.IsIdentity)
                return rect;

            PointF[] points = new PointF[]
            {
                new PointF(rect.X, rect.Y),
                new PointF(rect.X + rect.Width, rect.Y),
                new PointF(rect.X, rect.Y + rect.Height),
                new PointF(rect.X + rect.Width, rect.Y + rect.Height)
            };
            matrix.TransformPoints(points);
            float x, y, xmin, xmax, ymin, ymax;
            xmin = xmax = points[0].X;
            ymin = ymax = points[0].Y;
            for (int i = 1; i < 4; i++)
            {
                x = points[i].X;
                y = points[i].Y;
                xmin = Math.Min(xmin, x);
                xmax = Math.Max(xmax, x);
                ymin = Math.Min(ymin, y);
                ymax = Math.Max(ymax, y);
            }
            return new RectangleF(xmin, ymin, xmax - xmin, ymax - ymin);
        }

        /// <summary>
        /// Returns the center point of the given <paramref name="rect"/>.
        /// </summary>
        /// <param name="rect">The rectangle to find the center of.</param>
        /// <returns>The center point of the given <paramref name="rect"/>.
        /// </returns>
        public static PointF Center(RectangleF rect)
        {
            return new PointF(rect.X + rect.Width / 2, 
                rect.Y + rect.Height / 2);
        }

        /// <summary>
        /// Returns the center point of the given <paramref name="rect"/>.
        /// </summary>
        /// <param name="rect">The rectangle to find the center of.</param>
        /// <returns>The center point of the given <paramref name="rect"/>.
        /// </returns>
        public static Point Center(Rectangle rect)
        {
            return new Point(rect.X + rect.Width / 2,
                rect.Y + rect.Height / 2);
        }
        #endregion
    }
}
