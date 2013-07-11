using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms
{
    /// <summary>
    /// Defines a vector with two single-precision components.
    /// </summary>
    public class Vec2F : IEquatable<Vec2F>
    {
        /// <summary>
        /// The X-coordinate of the vector.
        /// </summary>
        public float X;
        /// <summary>
        /// The Y-coordinate of the vector.
        /// </summary>
        public float Y;

        /// <summary>
        /// Initializes a new vector with the two given single-precision
        /// components.
        /// </summary>
        /// <param name="x">The X-coordinate of the vector.</param>
        /// <param name="y">The Y-coordinate of the vector.</param>
        public Vec2F(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        #region Constants
        /// <summary>
        /// Creates a vector with all its components set to zero.
        /// </summary>
        public static Vec2F Zero
        {
            get { return new Vec2F(0, 0); }
        }
        /// <summary>
        /// Creates a vector with all its components set to one.
        /// </summary>
        public static Vec2F One
        {
            get { return new Vec2F(1, 1); }
        }
        /// <summary>
        /// Creates the unit vector for the Euclidean X-axis.
        /// </summary>
        public static Vec2F UnitX
        {
            get { return new Vec2F(1, 0); }
        }
        /// <summary>
        /// Creates the unit vector for the Euclidean Y-axis.
        /// </summary>
        public static Vec2F UnitY
        {
            get { return new Vec2F(0, 1); }
        }
        #endregion

        #region Length and Distance Functions
        /// <summary>
        /// Gets the length of this vector.
        /// </summary>
        public float Length
        {
            get 
            { 
                return (float)Math.Sqrt(this.X * this.X + this.Y * this.Y); 
            }
        }

        /// <summary>
        /// Gets the squared length of this vector.
        /// </summary>
        public float LengthSquared
        {
            get { return this.X * this.X + this.Y * this.Y; }
        }

        /// <summary>
        /// Gets the Manhattan length of this vector, which is the sum of 
        /// the absolute values of its components.
        /// </summary>
        public float ManhattanLength
        {
            get { return Math.Abs(this.X) + Math.Abs(this.Y); }
        }

        /// <summary>
        /// Calculates the distance between the given source 
        /// and destination points.
        /// </summary>
        /// <param name="u">The source point.</param>
        /// <param name="v">The destination point.</param>
        /// <returns>The distance between 
        /// <paramref name="u"/> and <paramref name="v"/>.</returns>
        public static double Distance(Vec2F u, Vec2F v)
        {
            return Math.Sqrt((v.X - u.X) * (v.X - u.X) + 
                             (v.Y - u.Y) * (v.Y - u.Y));
        }

        /// <summary>
        /// Calculates the squared distance between the given source 
        /// and destination points.
        /// </summary>
        /// <param name="u">The source point.</param>
        /// <param name="v">The destination point.</param>
        /// <returns>The squared distance between 
        /// <paramref name="u"/> and <paramref name="v"/>.</returns>
        public static float DistanceSquared(Vec2F u, Vec2F v)
        {
            return (v.X - u.X) * (v.X - u.X) + (v.Y - u.Y) * (v.Y - u.Y);
        }

        /// <summary>
        /// Calculates the Manhattan distance between the given source 
        /// and destination points.
        /// </summary>
        /// <param name="u">The source point.</param>
        /// <param name="v">The destination point.</param>
        /// <returns>The Manhattan distance between 
        /// <paramref name="u"/> and <paramref name="v"/>.</returns>
        public static float ManhattanDistance(Vec2F u, Vec2F v)
        {
            return Math.Abs(v.X - u.X) + Math.Abs(v.Y - u.Y);
        }
        #endregion

        #region Binary Vector Operations
        /// <summary>
        /// Calculates the dot/inner product of the vectors 
        /// <paramref name="u"/> and <paramref name="v"/>.
        /// </summary>
        /// <param name="u">A source vector.</param>
        /// <param name="v">A source vector.</param>
        /// <returns>The dot/inner product of the two given vectors.
        /// </returns>
        public static float DotProduct(Vec2F u, Vec2F v)
        {
            return u.X * v.X + u.Y * v.Y;
        }

        /// <summary>
        /// Calculates the cross product of the vectors 
        /// <paramref name="u"/> and <paramref name="v"/>.
        /// </summary>
        /// <param name="u">A source vector.</param>
        /// <param name="v">A source vector.</param>
        /// <returns>The cross product of the two given vectors.
        /// </returns>
        public static float CrossProduct(Vec2F u, Vec2F v)
        {
            return u.X * v.Y - u.Y * v.X;
        }
        #endregion

        #region Binary Arithmetic Operations
        /// <summary>
        /// Adds the vector <paramref name="v"/>
        /// to the vector <paramref name="u"/>.
        /// </summary>
        /// <param name="u">A source vector.</param>
        /// <param name="v">A source vector.</param>
        /// <returns>The sum of the two given vectors.</returns>
        public static Vec2F Add(Vec2F u, Vec2F v)
        {
            return new Vec2F(u.X + v.X, u.Y + v.Y);
        }

        /// <summary>
        /// Subtracts the vector <paramref name="v"/>
        /// from the vector <paramref name="u"/>.
        /// </summary>
        /// <param name="u">A source vector.</param>
        /// <param name="v">A source vector.</param>
        /// <returns>The difference between the two given vectors.</returns>
        public static Vec2F Subtract(Vec2F u, Vec2F v)
        {
            return new Vec2F(u.X - v.X, u.Y - v.Y);
        }
        #endregion

        #region Equality Testing
        /// <summary>
        /// Tests whether the components of the given vector are equal to
        /// the respective components of this vector.
        /// </summary>
        /// <param name="other">The vector to test.</param>
        /// <returns>True if the <see cref="X"/> and <see cref="Y"/>
        /// components of <paramref name="other"/> are equal to the
        /// corresponding components of this vector; otherwise false.
        /// </returns>
        public bool Equals(Vec2F other)
        {
            return this.X == other.X && this.Y == other.Y;
        }
        #endregion
    }
}
