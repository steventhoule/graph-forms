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
        protected readonly DirectionalGraph<Node, Edge> mGraph;

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

        public AGraphTraversalAlgorithm(DirectionalGraph<Node, Edge> graph)
        {
            this.mGraph = graph;
            this.bUndirected = false;
            this.bReversed = false;
        }

        public AGraphTraversalAlgorithm(DirectionalGraph<Node, Edge> graph,
            bool undirected, bool reversed)
        {
            this.mGraph = graph;
            this.bUndirected = undirected;
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
        /// If true, the graph is traversed from both the source edges and
        /// destination edges of each node instead of just one or the other,
        /// but <see cref="Reversed"/> still determines which edge list is
        /// traversed first on each node.
        /// </summary>
        public bool Undirected
        {
            get { return this.bUndirected; }
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
        #endregion

        public virtual void Initialize()
        {
            DirectionalGraph<Node, Edge>.GraphNode[] nodes
                = this.mGraph.InternalNodes;
            for (int i = 0; i < nodes.Length; i++)
            {
                if (this.State == ComputeState.Aborting)
                    return;
                nodes[i].Color = GraphColor.White;
                nodes[i].Index = i;
                this.OnInitializeNode(nodes[i].mData, i);
            }
            DirectionalGraph<Node, Edge>.GraphEdge[] edges
                = this.mGraph.InternalEdges;
            for (int j = 0; j < edges.Length; j++)
            {
                edges[j].Color = GraphColor.White;
            }
        }
    }
}
