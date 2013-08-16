using System;

namespace GraphForms.Algorithms.Path
{
    public abstract class AAllShortestPaths<Node, Edge>
        : AAlgorithm
        where Edge : IGraphEdge<Node>
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

        public abstract float[][] Distances
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

        public float GetDiameter()
        {
            int i, j;
            float diameter = -float.MaxValue;
            float[] dists;
            float[][] distances = this.Distances;
            for (i = 0; i < distances.Length; i++)
            {
                dists = distances[i];
                for (j = 0; j < dists.Length; j++)
                {
                    if (dists[j] != float.MaxValue)
                        diameter = Math.Max(dists[j], diameter);
                }
            }
            if (diameter == -float.MaxValue)
                diameter = 0;
            return diameter;
        }

        /*private class StackFrame
        {
            public int Src;
            public int Dst;

            public StackFrame(int src, int dst)
            {
                this.Src = src;
                this.Dst = dst;
            }
        }/* */

        public int[] TryGetNodePath(int srcNode, int dstNode)
        {
            if (srcNode == dstNode)
            {
                return new int[] { srcNode };
            }
            if (srcNode < 0 || srcNode >= this.mGraph.NodeCount)
            {
                throw new ArgumentOutOfRangeException("srcNode");
            }
            if (dstNode < 0 || dstNode >= this.mGraph.NodeCount)
            {
                throw new ArgumentOutOfRangeException("dstNode");
            }
            float[][] dists = this.Distances;
            if (dists[srcNode][dstNode] == float.MaxValue)
            {
                return null;
            }
            int[][] preds = this.PathNodes;
            //StackFrame curr;
            int currSrc, currDst, intermediate, pathCount = 0;
            int[] path = new int[this.mGraph.NodeCount];
            path[pathCount++] = srcNode;
            int todoCount = 1;
            int[] todoSrc = new int[this.mGraph.NodeCount];
            int[] todoDst = new int[this.mGraph.NodeCount];
            todoSrc[0] = srcNode;
            todoDst[0] = dstNode;
            //Stack<StackFrame> todo = new Stack<StackFrame>();
            //todo.Push(new StackFrame(srcNode, dstNode));
            while (todoCount > 0)
            {
                //curr = todo.Pop();
                todoCount--;
                currSrc = todoSrc[todoCount];
                currDst = todoDst[todoCount];
                if (dists[currSrc][currDst] < float.MaxValue)
                {
                    intermediate = preds[currSrc][currDst];
                    if (intermediate > -1)
                    {
                        // explore sub-paths
                        //todo.Push(new StackFrame(intermediate, curr.Dst));
                        //todo.Push(new StackFrame(curr.Src, intermediate));
                        todoSrc[todoCount] = intermediate;
                        todoDst[todoCount] = currDst;
                        todoCount++;
                        todoSrc[todoCount] = currSrc;
                        todoDst[todoCount] = intermediate;
                        todoCount++;
                    }
                    else
                    {
                        // add edge to path
                        path[pathCount++] = currDst;
                    }
                }
                else
                {
                    return null;
                }
            }
            int[] newPath = new int[pathCount];
            Array.Copy(path, 0, newPath, 0, pathCount);
            return newPath;
        }

        public Edge[] TryGetEdgePath(int srcNode, int dstNode)
        {
            if (srcNode == dstNode)
            {
                return new Edge[0];
            }
            if (srcNode < 0 || srcNode >= this.mGraph.NodeCount)
            {
                throw new ArgumentOutOfRangeException("srcNode");
            }
            if (dstNode < 0 || dstNode >= this.mGraph.NodeCount)
            {
                throw new ArgumentOutOfRangeException("dstNode");
            }
            float[][] dists = this.Distances;
            if (dists[srcNode][dstNode] == float.MaxValue)
            {
                return null;
            }
            int[][] preds = this.PathNodes;
            Edge[][] edges = this.PathEdges;
            //StackFrame curr;
            int currSrc, currDst, intermediate, pathCount = 0;
            Edge[] path = new Edge[this.mGraph.NodeCount - 1];
            int todoCount = 1;
            int[] todoSrc = new int[this.mGraph.NodeCount];
            int[] todoDst = new int[this.mGraph.NodeCount];
            todoSrc[0] = srcNode;
            todoDst[0] = dstNode;
            //Stack<StackFrame> todo = new Stack<StackFrame>();
            //todo.Push(new StackFrame(srcNode, dstNode));
            while (todoCount > 0)
            {
                //curr = todo.Pop();
                todoCount--;
                currSrc = todoSrc[todoCount];
                currDst = todoDst[todoCount];
                if (dists[currSrc][currDst] < float.MaxValue)
                {
                    intermediate = preds[currSrc][currDst];
                    if (intermediate > -1)
                    {
                        // explore sub-paths
                        //todo.Push(new StackFrame(intermediate, curr.Dst));
                        //todo.Push(new StackFrame(curr.Src, intermediate));
                    }
                    else
                    {
                        // add edge to path
                        path[pathCount++] = edges[currSrc][currDst];
                    }
                }
                else
                {
                    return null;
                }
            }
            Edge[] newPath = new Edge[pathCount];
            Array.Copy(path, 0, newPath, 0, pathCount);
            return newPath;
        }
    }
}
