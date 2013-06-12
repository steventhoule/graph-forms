using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Path
{
    public abstract class AAllShortestPaths<Node, Edge>
        : AAlgorithm
        where Node : class
        where Edge : class, IGraphEdge<Node>
    {
        protected readonly Digraph<Node, Edge> mGraph;
        protected readonly bool bUndirected;
        protected readonly bool bReversed;

        public AAllShortestPaths(Digraph<Node, Edge> graph,
            bool directed, bool reversed)
        {
            this.mGraph = graph;
            this.bUndirected = !directed;
            this.bReversed = reversed;
        }

        public Digraph<Node, Edge> Graph
        {
            get { return this.mGraph; }
        }

        public bool Directed
        {
            get { return !this.bUndirected; }
        }

        public bool Reversed
        {
            get { return this.bReversed; }
        }

        public abstract double[][] Distances
        {
            get;
        }

        public abstract int[][] PathNodes
        {
            get;
        }

        public abstract Edge[][] PathEdges
        {
            get;
        }

        public double GetDiameter()
        {
            int i, j;
            double diameter = -double.MaxValue;
            double[] dists;
            double[][] distances = this.Distances;
            for (i = 0; i < distances.Length; i++)
            {
                dists = distances[i];
                for (j = 0; j < dists.Length; j++)
                {
                    if (dists[j] != double.MaxValue)
                        diameter = Math.Max(dists[j], diameter);
                }
            }
            if (diameter == -double.MaxValue)
                diameter = 0;
            return diameter;
        }

        private class StackFrame
        {
            public int Src;
            public int Dst;

            public StackFrame(int src, int dst)
            {
                this.Src = src;
                this.Dst = dst;
            }
        }

        public int[] TryGetNodePath(int srcNode, int dstNode)
        {
            double[][] dists = this.Distances;
            int[][] preds = this.PathNodes;
            int intermediate;
            StackFrame curr;
            List<int> path = new List<int>();
            path.Add(srcNode);
            Stack<StackFrame> todo = new Stack<StackFrame>();
            todo.Push(new StackFrame(srcNode, dstNode));
            while (todo.Count > 0)
            {
                curr = todo.Pop();
                if (dists[curr.Src][curr.Dst] < double.MaxValue)
                {
                    intermediate = preds[curr.Src][curr.Dst];
                    if (intermediate > -1)
                    {
                        // explore sub-paths
                        todo.Push(new StackFrame(intermediate, curr.Dst));
                        todo.Push(new StackFrame(curr.Src, intermediate));
                    }
                    else
                    {
                        // add edge to path
                        path.Add(curr.Dst);
                    }
                }
                else
                {
                    return null;
                }
            }
            return path.ToArray();
        }

        public Edge[] TryGetEdgePath(int srcNode, int dstNode)
        {
            double[][] dists = this.Distances;
            int[][] preds = this.PathNodes;
            Edge[][] edges = this.PathEdges;
            int intermediate;
            StackFrame curr;
            List<Edge> path = new List<Edge>();
            Stack<StackFrame> todo = new Stack<StackFrame>();
            todo.Push(new StackFrame(srcNode, dstNode));
            while (todo.Count > 0)
            {
                curr = todo.Pop();
                if (dists[curr.Src][curr.Dst] < double.MaxValue)
                {
                    intermediate = preds[curr.Src][curr.Dst];
                    if (intermediate > -1)
                    {
                        // explore sub-paths
                        todo.Push(new StackFrame(intermediate, curr.Dst));
                        todo.Push(new StackFrame(curr.Src, intermediate));
                    }
                    else
                    {
                        // add edge to path
                        path.Add(edges[curr.Src][curr.Dst]);
                    }
                }
                else
                {
                    return null;
                }
            }
            return path.ToArray();
        }
    }
}
