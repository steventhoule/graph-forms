using System;

namespace GraphForms.Algorithms.Search
{
    public class UniformCostSearch<Node, Edge>
        : AstarSearch<Node, Edge>
        where Edge : IGraphEdge<Node>
    {
        private double mCost;

        public UniformCostSearch(Digraph<Node, Edge> graph,
            double cost, bool directed, bool reversed)
            : base(graph, directed, reversed)
        {
            this.mCost = cost;
        }

        public double Cost
        {
            get { return this.mCost; }
            set
            {
                if (this.State != ComputeState.Running)
                    this.mCost = value;
            }
        }

        protected override double HeuristicCost(Digraph<Node, Edge>.GNode n)
        {
            return this.mCost;
        }
    }
}
