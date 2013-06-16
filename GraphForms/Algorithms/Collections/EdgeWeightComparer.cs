using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Collections
{
    public class EdgeWeightComparer<Node, Edge>
        : IComparer<Digraph<Node, Edge>.GEdge>,
          IComparer<Edge>
        where Edge : IGraphEdge<Node>
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

        public int Compare(Digraph<Node, Edge>.GEdge x, 
                           Digraph<Node, Edge>.GEdge y)
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
