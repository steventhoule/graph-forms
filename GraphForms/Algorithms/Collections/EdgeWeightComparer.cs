using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Collections
{
    public class EdgeWeightComparer<Node, Edge>
        : IComparer<DirectionalGraph<Node, Edge>.GraphEdge>,
          IComparer<Edge>
        where Node : class
        where Edge : class, IGraphEdge<Node>
    {
        private bool bReversed;

        public EdgeWeightComparer()
        {
            this.bReversed = false;
        }

        public EdgeWeightComparer(bool reversed)
        {
            this.bReversed = reversed;
        }

        public int Compare(DirectionalGraph<Node, Edge>.GraphEdge x, 
                           DirectionalGraph<Node, Edge>.GraphEdge y)
        {
            if (this.bReversed)
                return y.mData.Weight.CompareTo(x.mData.Weight);
            else
                return x.mData.Weight.CompareTo(y.mData.Weight);
        }

        public int Compare(Edge x, Edge y)
        {
            if (this.bReversed)
                return y.Weight.CompareTo(x.Weight);
            else
                return x.Weight.CompareTo(y.Weight);
        }
    }
}
