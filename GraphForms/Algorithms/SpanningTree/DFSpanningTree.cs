﻿using System;
using System.Collections.Generic;
using System.Text;
using GraphForms.Algorithms.Search;

namespace GraphForms.Algorithms.SpanningTree
{
    /// <summary>
    /// This algorithm uses a simple depth first traversal of a given graph
    /// to generate a spanning tree.</summary>
    /// <typeparam name="Node">The type of vertices in the spanning tree 
    /// generated by this algorithm.</typeparam>
    /// <typeparam name="Edge">The type of edges in the spanning tree 
    /// generated by this algorithm.</typeparam>
    public class DFSpanningTree<Node, Edge>
        : DepthFirstSearch<Node, Edge>,
          IRootedSpanningTreeAlgorithm<Node, Edge>
        where Edge : IGraphEdge<Node>
    {
        private Digraph<Node, Edge> mSpanningTree;

        public DFSpanningTree(Digraph<Node, Edge> graph)
            : base(graph, true, false)
        {
            this.mSpanningTree = new Digraph<Node, Edge>(
                graph.NodeCount, graph.EdgeCount / 2);
            //this.mSpanningTree.AddNodeRange(graph.Nodes);
        }

        public DFSpanningTree(Digraph<Node, Edge> graph,
            bool directed, bool reversed)
            : base(graph, directed, reversed)
        {
            this.mSpanningTree = new Digraph<Node, Edge>(
                graph.NodeCount, graph.EdgeCount / 2);
            //this.mSpanningTree.AddNodeRange(graph.Nodes);
        }

        /// <summary>
        /// A sub-graph of the original connected graph that connects all
        /// its vertices together with a minimal subset of its edges.
        /// </summary><remarks>
        /// If the original graph isn't connected, this graph will contain
        /// multiple spanning trees, one for each (weakly) connected
        /// component of the original graph.
        /// </remarks>
        public Digraph<Node, Edge> SpanningTree
        {
            get { return this.mSpanningTree; }
        }

        /// <summary>
        /// Called whenever the algorithm explores an edge in the graph
        /// connected to an unexplored node, that edge is then added to the
        /// spanning tree.
        /// </summary>
        /// <param name="e">The <typeparamref name="Edge"/> instance being
        /// explored by this graph traversal algorithm.</param>
        /// <param name="srcIndex">The index of the <see 
        /// cref="P:IGraphEdge`1{Node}.SrcNode"/> of the edge <paramref 
        /// name="e"/>.</param>
        /// <param name="dstIndex">The index of the <see 
        /// cref="P:IGraphEdge`1{Node}.DstNode"/> of the edge <paramref 
        /// name="e"/>.</param>
        /// <param name="reversed">True if the edge is being explored from 
        /// destination to source instead of from source to destination.
        /// </param>
        protected override void OnTreeEdge(Edge e, 
            int srcIndex, int dstIndex, bool reversed)
        {
            this.mSpanningTree.AddEdge(e);
            base.OnTreeEdge(e, srcIndex, dstIndex, reversed);
        }
    }
}
