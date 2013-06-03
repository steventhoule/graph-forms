using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Search
{
    public abstract class AGraphTraversalAlgorithm<Node, Edge>
        : ARootedAlgorithm<Node>
        where Node : class
        where Edge : class, IGraphEdge<Node>
    {
        protected readonly Digraph<Node, Edge> mGraph;

        /// <summary>
        /// If true, the graph is traversed from both the source edges and
        /// destination edges of each node instead of just one or the other,
        /// but <see cref="Reversed"/> still determines which edge list is
        /// traversed first on each node.
        /// </summary>
        protected readonly bool bUndirected;
        /// <summary>
        /// If true, the graph is traversed from destination nodes to source
        /// nodes instead of from source nodes to destination nodes.
        /// </summary>
        protected readonly bool bReversed;

        /*public AGraphTraversalAlgorithm(Digraph<Node, Edge> graph)
        {
            this.mGraph = graph;
            this.bDirected = true;
            this.bReversed = false;
        }/* */

        public AGraphTraversalAlgorithm(Digraph<Node, Edge> graph,
            bool directed, bool reversed)
        {
            this.mGraph = graph;
            this.bUndirected = !directed;
            this.bReversed = reversed;
        }

        /// <summary>
        /// If true, the graph is traversed from destination nodes to source
        /// nodes instead of from source nodes to destination nodes.
        /// </summary>
        public bool Reversed
        {
            get { return this.bReversed; }
        }

        /// <summary>
        /// If false, the graph is traversed from both the source edges and
        /// destination edges of each node instead of just one or the other,
        /// but <see cref="Reversed"/> still determines which edge list is
        /// traversed first on each node.
        /// </summary>
        public bool Directed
        {
            get { return !this.bUndirected; }
        }

        #region Events
        protected virtual void OnInitializeNode(Node n, int index)
        {
        }

        protected virtual void OnStartNode(Node n, int index)
        {
        }

        protected virtual void OnDiscoverNode(Node n, int index)
        {
        }

        protected virtual void OnFinishNode(Node n, int index)
        {
        }

        protected virtual void OnExamineEdge(Edge e, 
            int srcIndex, int dstIndex, bool reversed)
        {
        }

        protected virtual void OnTreeEdge(Edge e, 
            int srcIndex, int dstIndex, bool reversed)
        {
        }

        protected virtual void OnGrayEdge(Edge e,
            int srcIndex, int dstIndex, bool reversed)
        {
        }

        protected virtual void OnBlackEdge(Edge e,
            int srcIndex, int dstIndex, bool reversed)
        {
        }
        #endregion

        public virtual void Initialize()
        {
            Digraph<Node, Edge>.GNode[] nodes
                = this.mGraph.InternalNodes;
            for (int i = 0; i < nodes.Length; i++)
            {
                if (this.State == ComputeState.Aborting)
                    return;
                nodes[i].Color = GraphColor.White;
                nodes[i].Index = i;
                this.OnInitializeNode(nodes[i].mData, i);
            }
        }
    }
}
