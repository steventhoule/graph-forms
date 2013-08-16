using System;
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
            bool rev;
            Digraph<Node, Edge>.GNode node;
            Digraph<Node, Edge>.GEdge edge;
            int count = this.mGraph.NodeCount;
            int i, j, u, v, stop = this.bUndirected ? 2 : 1;

            FibonacciNode<float, int>.Heap queue 
                = new FibonacciNode<float, int>.Heap();
            FibonacciNode<float, int>[] queueNodes
                = new FibonacciNode<float, int>[count];
            for (i = 0; i < count; i++)
            {
                node = this.mGraph.InternalNodeAt(i);
                if (!node.Hidden)
                    queueNodes[i] = queue.Enqueue(this.mDistances[i], i);
            }

            float dist;
            count = this.mGraph.EdgeCount;
            while (queue.Count > 0)
            {
                u = queue.Dequeue().Value;
                if (this.mDistances[u] == float.MaxValue)
                    break;
                rev = this.bReversed;
                for (j = 0; j < stop; j++)
                {
                    for (i = 0; i < count; i++)
                    {
                        edge = this.mGraph.InternalEdgeAt(i);
                        if (edge.Hidden)
                            continue;
                        node = rev ? edge.DstNode : edge.SrcNode;
                        if (node.Index != u)
                            continue;
                        node = rev ? edge.SrcNode : edge.DstNode;
                        if (node.Hidden)
                            continue;
                        v = node.Index;
                        dist = this.mDistances[u] + edge.Data.Weight;
                        if (dist < this.mDistances[v])
                        {
                            this.mDistances[v] = dist;
                            this.mPathNodes[v] = u;
                            this.mPathEdges[v] = edge.Data;
                            queue.ChangePriority(queueNodes[v], dist);
                        }
                    }
                    rev = !rev;
                }
            }
        }

        private void RegularQueueCompute(int root)
        {
            bool rev;
            float dist;
            Digraph<Node, Edge>.GNode node;
            Digraph<Node, Edge>.GEdge edge;
            int i, j, u, v, stop = this.bUndirected ? 2 : 1;
            int qIndex, qCount, count = this.mGraph.NodeCount;
            //Queue<int> queue = new Queue<int>(count);
            int[] queue = new int[count];
            node = this.mGraph.InternalNodeAt(root);
            node.Color = GraphColor.Gray;
            //queue.Enqueue(root);
            qIndex = 0;
            qCount = 1;
            queue[0] = root;
            count = this.mGraph.EdgeCount;
            while (qIndex < qCount)//queue.Count > 0)
            {
                u = queue[qIndex++];//queue.Dequeue();
                rev = this.bReversed;
                for (j = 0; j < stop; j++)
                {
                    for (i = 0; i < count; i++)
                    {
                        edge = this.mGraph.InternalEdgeAt(i);
                        if (edge.Hidden)
                            continue;
                        node = rev ? edge.DstNode : edge.SrcNode;
                        if (node.Index != u)
                            continue;
                        node = rev ? edge.SrcNode : edge.DstNode;
                        if (node.Hidden)
                            continue;
                        v = node.Index;
                        dist = this.mDistances[u] + edge.Data.Weight;
                        if (dist < this.mDistances[v])
                        {
                            this.mDistances[v] = dist;
                            this.mPathNodes[v] = u;
                            this.mPathEdges[v] = edge.Data;
                            if (node.Color != GraphColor.Gray)
                            {
                                node.Color = GraphColor.Gray;
                                queue[qCount++] = v;//queue.Enqueue(v);
                            }
                        }
                    }
                    rev = !rev;
                }
                //nodes[u].Color = GraphColor.Black;
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
