using System;

namespace GraphForms.Algorithms.Path
{
    public abstract class AShortestPath<Node, Edge>
        : AGraphAlgorithm<Node, Edge>
        where Edge : IGraphEdge<Node>
    {
        protected readonly bool bUndirected;
        protected readonly bool bReversed;

        protected float[] mDistances;
        protected int[] mPathNodes;
        protected Edge[] mPathEdges;

        public AShortestPath(Digraph<Node, Edge> graph, 
            bool directed, bool reversed)
            : base(graph)
        {
            this.bUndirected = !directed;
            this.bReversed = reversed;
        }

        public bool Directed
        {
            get { return !this.bUndirected; }
        }

        public bool Reversed
        {
            get { return this.bReversed; }
        }

        public float[] Distances
        {
            get { return this.mDistances; }
        }

        public int[] PathNodes
        {
            get { return this.mPathNodes; }
        }

        public Edge[] PathEdges
        {
            get { return this.mPathEdges; }
        }

        protected override void InternalCompute()
        {
            if (this.mGraph.NodeCount == 0)
            {
                return;
            }
            Digraph<Node, Edge>.GNode node;
            int i, root, count = this.mGraph.NodeCount;
            
            this.mDistances = new float[count];
            this.mPathNodes = new int[count];
            this.mPathEdges = new Edge[count];
            for (i = 0; i < count; i++)
            {
                node = this.mGraph.InternalNodeAt(i);
                //nodes[i].Index = i;
                node.Color = GraphColor.White;
                this.mDistances[i] = float.MaxValue;
                this.mPathNodes[i] = -1;
            }
            if (this.mGraph.EdgeCount == 0)
            {
                // There are no edges, so there will be no paths
                return;
            }
            if (this.RootCount == 0)
            {
                // There are no roots,
                // so start from the first visible node found
                for (root = 0; root < count; root++)
                {
                    node = this.mGraph.InternalNodeAt(root);
                    if (!node.Hidden)
                        break;
                }
                if (root == count)
                {
                    // There are no visible nodes, 
                    // so it's as if the graph is empty
                    return;
                }
            }
            else
            {
                node = null;
                root = this.RootCount;
                for (i = 0; i < root; i++)
                {
                    node = this.RootAt(i);
                    if (!node.Hidden)
                        break;
                }
                if (i == root)
                {
                    // There are no visible roots,
                    // so start from the first visible node found
                    for (root = 0; root < count; root++)
                    {
                        node = this.mGraph.InternalNodeAt(root);
                        if (!node.Hidden)
                            break;
                    }
                    if (root == count)
                    {
                        // There are no visible nodes, 
                        // so it's as if the graph is empty
                        return;
                    }
                }
                else
                {
                    root = node.Index;
                }
            }
            this.mDistances[root] = 0;

            int si, di;
            count = this.mGraph.EdgeCount;
            Digraph<Node, Edge>.GEdge edge;
            if (this.bUndirected || !this.bReversed)
            {
                for (i = 0; i < count; i++)
                {
                    edge = this.mGraph.InternalEdgeAt(i);
                    if (!edge.DstNode.Hidden)
                    {
                        // If edge.SrcNode is the root,
                        // we already know it's not hidden
                        si = edge.SrcNode.Index;
                        if (si == root)
                        {
                            di = edge.DstNode.Index;
                            this.mDistances[di] = edge.Data.Weight;
                            this.mPathEdges[di] = edge.Data;
                        }
                    }
                }
            }
            if (this.bUndirected || this.bReversed)
            {
                float weight;
                for (i = 0; i < count; i++)
                {
                    edge = this.mGraph.InternalEdgeAt(i);
                    if (!edge.SrcNode.Hidden)
                    {
                        // If edge.DstNode is the root,
                        // we already know it's not hidden
                        si = edge.SrcNode.Index;
                        di = edge.DstNode.Index;
                        weight = edge.Data.Weight;
                        if (di == root && weight < this.mDistances[si])
                        {
                            this.mDistances[si] = weight;
                            this.mPathEdges[si] = edge.Data;
                        }
                    }
                }
            }

            this.ComputeFromRoot(root);
        }

        protected abstract void ComputeFromRoot(int root);
    }
}
