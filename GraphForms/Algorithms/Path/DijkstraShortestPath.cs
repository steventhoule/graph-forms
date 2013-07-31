using System;
using System.Collections.Generic;
using System.Text;
using GraphForms.Algorithms.Collections;

namespace GraphForms.Algorithms.Path
{
    public class DijkstraShortestPath<Node, Edge>
        : AShortestPath<Node, Edge>
        where Edge : IGraphEdge<Node>
    {
        private bool bUsePriorityQueue;

        public DijkstraShortestPath(Digraph<Node, Edge> graph)
            : base(graph, true, false)
        {
            this.bUsePriorityQueue = true;
        }

        public DijkstraShortestPath(Digraph<Node, Edge> graph,
            bool directed, bool reversed)
            : base(graph, directed, reversed)
        {
            this.bUsePriorityQueue = true;
        }

        public bool UsePriorityQueue
        {
            get { return this.bUsePriorityQueue; }
            set
            {
                if (this.State != ComputeState.Running)
                    this.bUsePriorityQueue = value;
            }
        }

        private void PriorityQueueCompute()
        {
            bool reversed;
            int i, j, u, v, stop = this.bUndirected ? 2 : 1;
            Digraph<Node, Edge>.GEdge[] edges
                = this.mGraph.InternalEdges;
            Digraph<Node, Edge>.GNode[] nodes
                = this.mGraph.InternalNodes;

            FibonacciNode<double, int>.Heap queue 
                = new FibonacciNode<double, int>.Heap();
            FibonacciNode<double, int>[] queueNodes
                = new FibonacciNode<double, int>[nodes.Length];
            for (i = 0; i < nodes.Length; i++)
            {
                queueNodes[i] = queue.Enqueue(this.mDistances[i], i);
            }

            double dist;
            while (queue.Count > 0)
            {
                u = queue.Dequeue().Value;
                if (this.mDistances[u] == double.MaxValue)
                    break;
                reversed = this.bReversed;
                for (j = 0; j < stop; j++)
                {
                    for (i = 0; i < edges.Length; i++)
                    {
                        v = reversed ? edges[i].mDstNode.Index 
                                     : edges[i].mSrcNode.Index;
                        if (v != u)
                            continue;
                        v = reversed ? edges[i].mSrcNode.Index
                                     : edges[i].mDstNode.Index;
                        dist = this.mDistances[u] + edges[i].mData.Weight;
                        if (dist < this.mDistances[v])
                        {
                            this.mDistances[v] = dist;
                            this.mPathNodes[v] = u;
                            this.mPathEdges[v] = edges[i].mData;
                            queue.ChangePriority(queueNodes[v], dist);
                        }
                    }
                    reversed = !reversed;
                }
            }
        }

        private void RegularQueueCompute(int root)
        {
            bool reversed;
            int i, j, u, v, stop = this.bUndirected ? 2 : 1;
            Digraph<Node, Edge>.GEdge[] edges
                = this.mGraph.InternalEdges;
            Digraph<Node, Edge>.GNode[] nodes
                = this.mGraph.InternalNodes;
            double dist;
            Queue<int> queue = new Queue<int>(nodes.Length);
            nodes[root].Color = GraphColor.Gray;
            queue.Enqueue(root);
            while (queue.Count > 0)
            {
                u = queue.Dequeue();
                reversed = this.bReversed;
                for (j = 0; j < stop; j++)
                {
                    for (i = 0; i < edges.Length; i++)
                    {
                        v = reversed ? edges[i].mDstNode.Index
                                     : edges[i].mSrcNode.Index;
                        if (v != u)
                            continue;
                        v = reversed ? edges[i].mSrcNode.Index
                                     : edges[i].mDstNode.Index;
                        dist = this.mDistances[u] + edges[i].mData.Weight;
                        if (dist < this.mDistances[v])
                        {
                            this.mDistances[v] = dist;
                            this.mPathNodes[v] = u;
                            this.mPathEdges[v] = edges[i].mData;
                            if (nodes[v].Color != GraphColor.Gray)
                            {
                                nodes[v].Color = GraphColor.Gray;
                                queue.Enqueue(v);
                            }
                        }
                    }
                    reversed = !reversed;
                }
                nodes[u].Color = GraphColor.Black;
            }
        }

        protected override void ComputeFromRoot(int root)
        {
            if (this.bUsePriorityQueue)
                this.PriorityQueueCompute();
            else
                this.RegularQueueCompute(root);
        }
    }
}
