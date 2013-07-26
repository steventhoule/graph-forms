using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms
{
    /// <summary>
    /// A simple class used for holding an array of values of type
    /// <typeparamref name="T"/> and recursively printing their
    /// <see cref="Object.ToString()"/> results in its own
    /// <see cref="M:PrintableArray`1.ToString()"/> method with a
    /// specified separating string between each value.
    /// </summary><typeparam name="T">The type of values stored in 
    /// this printable array container.</typeparam>
    public class PrintArray<T>
    {
        /// <summary>
        /// The array of <typeparamref name="T"/> instances
        /// stored in this printable array container.
        /// </summary>
        public T[] Data;
        /// <summary>
        /// The separator that is placed between the printed instances
        /// in the <see cref="Data"/> array when it is printed with
        /// the <see cref="ToString()"/> function.
        /// </summary>
        public string Separator;

        /// <summary>
        /// Initializes a new printable array instance with its
        /// <see cref="Data"/> set to <c>null</c> and its 
        /// <see cref="Separator"/> set to a single comma.
        /// </summary>
        public PrintArray()
        {
            this.Data = null;
            this.Separator = ",";
        }

        /// <summary>
        /// Initializes a new printable array instance with the given
        /// array of <paramref name="data"/> to be printed with its
        /// <see cref="ToString()"/> function with a single comma
        /// printed between each of them.</summary>
        /// <param name="data">The array of data to be printed 
        /// by this printable array container.</param>
        public PrintArray(T[] data)
        {
            this.Data = data;
            this.Separator = ",";
        }

        /// <summary>
        /// Initializes a new printable array instance with the given
        /// array of <paramref name="data"/> to be printed with its
        /// <see cref="ToString()"/> function with the given
        /// <paramref name="separator"/> printed between each of them.
        /// </summary>
        /// <param name="data">The array of data to be printed 
        /// by this printable array container.</param>
        /// <param name="separator">The string that printed between the
        /// string print out of each value in <paramref name="data"/>.
        /// </param>
        public PrintArray(T[] data, string separator)
        {
            this.Data = data;
            this.Separator = separator;
        }

        /// <summary>
        /// Iteratively prints each <typeparamref name="T"/> instance in
        /// this printable array's <see cref="Data"/> by using their
        /// implementation of the <see cref="Object.ToString()"/> method,
        /// and separates them with this printable array's
        /// <see cref="Separator"/> string.</summary>
        /// <returns>The string version of this printable array.</returns>
        public override string ToString()
        {
            if (this.Data == null || this.Data.Length == 0)
            {
                return "";
            }
            StringBuilder sb = new StringBuilder(
                this.Data.Length * (4 + this.Separator.Length));
            for (int i = 0; i < this.Data.Length - 1; i++)
            {
                sb.Append(string.Concat(this.Data[i], this.Separator));
            }
            sb.Append(this.Data[this.Data.Length - 1]);
            return sb.ToString();
        }
    }
}
