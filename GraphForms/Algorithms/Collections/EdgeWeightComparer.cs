using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Collections
{
    /// <summary>
    /// Compares the <see cref="P:IGraphEdge`1.Weight"/> values of
    /// graph edges for sorting them from least to greatest weight or
    /// from greatest to least weight.</summary>
    /// <typeparam name="Node">The type of graph nodes linked by the graph
    /// edges being compared.</typeparam>
    /// <typeparam name="Edge">The type of graph edges being compared and
    /// sorted by weight.</typeparam>
    public class EdgeWeightComparer<Node, Edge>
        : IComparer<Digraph<Node, Edge>.GEdge>,
          IComparer<Edge>
        where Edge : IGraphEdge<Node>
    {
        private bool bReversed;

        /// <summary>
        /// Initializes a new <see cref="T:EdgeWeightComparer`2"/> that can
        /// be used to sort a set of graph edges from least to greatest
        /// <see cref="P:IGraphEdge`1.Weight"/>.
        /// </summary>
        public EdgeWeightComparer()
        {
            this.bReversed = false;
        }
        /// <summary>
        /// Initializes a new <see cref="T:EdgeWeightComparer`2"/> that can
        /// be used to sort a set of graph edges from least to greatest
        /// <see cref="P:IGraphEdge`1.Weight"/>, or from greatest to least
        /// <see cref="P:IGraphEdge`1.Weight"/> if 
        /// <paramref name="reversed"/>.</summary>
        /// <param name="reversed">Whether to sort graph edges from
        /// greatest to least weight instead of from least to greatest
        /// weight.</param>
        public EdgeWeightComparer(bool reversed)
        {
            this.bReversed = reversed;
        }
        /// <summary>
        /// Whether this <see cref="T:EdgeWeightComparer`2"/> sorts edges
        /// from greatest to least <see cref="P:IGraphEdge`1.Weight"/>
        /// instead of from least to greatest
        /// <see cref="P:IGraphEdge`1.Weight"/>.</summary>
        public bool Reversed
        {
            get { return this.bReversed; }
        }
        /// <summary>
        /// Compares the <see cref="P:IGraphEdge`1{Node}.Weight"/> values of
        /// the <see cref="P:Digraph`2{Node,Edge}.GEdge.Data"/> values of
        /// the given <see cref="T:Digraph`2{Node,Edge}.GEdge"/> instances.
        /// </summary>
        /// <param name="x">The first edge to compare by weight.</param>
        /// <param name="y">The second edge to compare by weight.</param>
        /// <returns>Result of <c>x.Data.Weight.CompareTo(y.Data.Weight)</c>,
        /// or its negation if this comparer is <see cref="Reversed"/>.
        /// </returns><seealso cref="float.CompareTo(float)"/>
        public int Compare(Digraph<Node, Edge>.GEdge x, 
                           Digraph<Node, Edge>.GEdge y)
        {
            if (this.bReversed)
                return y.Data.Weight.CompareTo(x.Data.Weight);
            else
                return x.Data.Weight.CompareTo(y.Data.Weight);
        }
        /// <summary>
        /// Compares the <see cref="P:IGraphEdge`1{Node}.Weight"/> values of
        /// the given <typeparamref name="Edge"/> instances.
        /// </summary>
        /// <param name="x">The first edge to compare by weight.</param>
        /// <param name="y">The second edge to compare by weight.</param>
        /// <returns>Result of <c>x.Weight.CompareTo(y.Weight)</c>,
        /// or its negation if this comparer is <see cref="Reversed"/>.
        /// </returns><seealso cref="float.CompareTo(float)"/>
        public int Compare(Edge x, Edge y)
        {
            if (this.bReversed)
                return y.Weight.CompareTo(x.Weight);
            else
                return x.Weight.CompareTo(y.Weight);
        }
    }
}
