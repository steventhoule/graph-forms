using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Search
{
    public class UniformCostSearch<Node, Edge>
        : AstarSearch<Node, Edge>
        where Edge : IGraphEdge<Node>
    {
        private readonly double mCost;

        public UniformCostSearch(Digraph<Node, Edge> graph,
            double cost, bool directed, bool reversed)
            : base(graph, directed, reversed)
        {
            this.mCost = cost;
        }

        public double Cost
        {
            get { return this.mCost; }
        }

        protected override double HeuristicCost(Node n, int index)
        {
            return this.mCost;
        }
    }
}
