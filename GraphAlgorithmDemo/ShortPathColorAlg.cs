using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using GraphForms.Algorithms;
using GraphForms.Algorithms.Path;

namespace GraphAlgorithmDemo
{
    public abstract class ShortPathColorAlg
    {
        private static Color sSrcColor = Color.Blue;
        private static Color sDstColor = Color.Orange;
        private static Color sPathColor = Color.LightGreen;

        public bool Directed = true;
        public bool Reversed = false;
        public bool IsDirty = true;

        private AAllShortestPaths<CircleNode, ArrowEdge> mAlg;

        protected abstract AAllShortestPaths<CircleNode, ArrowEdge> Create(
            CircleNodeScene scene);

        public void RefreshAlgorithm(CircleNodeScene scene)
        {
            if (this.IsDirty)
            {
                this.mAlg = this.Create(scene);
                this.mAlg.Compute();
                this.IsDirty = false;
            }
        }

        public void ColorShortestPath(CircleNodeScene scene)
        {
            int i;
            Digraph<CircleNode, ArrowEdge>.GNode[] nodes
                = scene.Graph.InternalNodes;
            for (i = 0; i < nodes.Length; i++)
            {
                nodes[i].Data.BorderColor = Color.Black;
            }
            int src = scene.MouseUpHistory[1];
            int dst = scene.MouseUpHistory[0];
            CircleNode node;
            if (src >= 0 && dst >= 0 && src != dst)
            {
                node = scene.Graph.NodeAt(src);
                node.BorderColor = sSrcColor;
                node = scene.Graph.NodeAt(dst);
                node.BorderColor = sDstColor;
                ArrowEdge[] edges = scene.Graph.Edges;
                for (i = 0; i < edges.Length; i++)
                {
                    edges[i].LineColor = Color.Black;
                }
                //string test = this.PrintDistances();
                edges = this.mAlg.TryGetEdgePath(src, dst);
                if (edges != null)
                {
                    for (i = 0; i < edges.Length; i++)
                    {
                        edges[i].LineColor = sPathColor;
                    }
                }
                int[] path = this.mAlg.TryGetNodePath(src, dst);
                if (path != null)
                {
                    for (i = path.Length - 2; i >= 1; i--)
                    {
                        nodes[path[i]].Data.BorderColor = sPathColor;
                    }
                }
            }
            else if (dst >= 0)
            {
                node = scene.Graph.NodeAt(dst);
                node.BorderColor = sSrcColor;
            }
        }

        public string PrintPathNodes()
        {
            if (this.mAlg != null)
            {
                int[][] pathNodes = this.mAlg.PathNodes;
                if (pathNodes != null)
                {
                    int i, j;
                    int[] pNodes;
                    StringBuilder builder = new StringBuilder();
                    for (i = 0; i < pathNodes.Length; i++)
                    {
                        pNodes = pathNodes[i];
                        builder.Append("| ");
                        for (j = 0; j < pNodes.Length; j++)
                        {
                            builder.Append(string.Concat(
                                pNodes[j].ToString().PadLeft(2), " | "));
                        }
                        builder.AppendLine();
                    }
                    return builder.ToString();
                }
            }
            return "";
        }

        public string PrintDistances()
        {
            if (this.mAlg != null)
            {
                float[][] distances = this.mAlg.Distances;
                if (distances != null)
                {
                    int i, j;
                    float[] dists;
                    StringBuilder builder = new StringBuilder();
                    for (i = 0; i < distances.Length; i++)
                    {
                        dists = distances[i];
                        builder.Append("| ");
                        for (j = 0; j < dists.Length; j++)
                        {
                            if (dists[j] == double.MaxValue)
                                builder.Append("   | ");
                            else
                                builder.Append(string.Concat(
                                    dists[j].ToString().PadLeft(2), " | "));
                        }
                        builder.AppendLine();
                    }
                    return builder.ToString();
                }
            }
            return "";
        }
    }

    public class FloydWarshallShortPathColorAlg
        : ShortPathColorAlg
    {
        protected override AAllShortestPaths<CircleNode, ArrowEdge> Create(
            CircleNodeScene scene)
        {
            return new FloydWarshallAllShortestPaths<CircleNode, ArrowEdge>(
                scene.Graph, this.Directed, this.Reversed);
        }

        public override string ToString()
        {
            return "Floyd Warshall";
        }
    }

    public class BellManFordShortPathColorAlg
        : ShortPathColorAlg
    {
        protected override AAllShortestPaths<CircleNode, ArrowEdge> Create(
            CircleNodeScene scene)
        {
            return new BasicAllShortestPaths<CircleNode, ArrowEdge>(
                new BellmanFordShortestPath<CircleNode, ArrowEdge>(
                    scene.Graph, this.Directed, this.Reversed));
        }

        public override string ToString()
        {
            return "Bellman Ford";
        }
    }

    public class DijkstraShortPathColorAlg
        : ShortPathColorAlg
    {
        protected override AAllShortestPaths<CircleNode, ArrowEdge> Create(
            CircleNodeScene scene)
        {
            return new BasicAllShortestPaths<CircleNode, ArrowEdge>(
                new DijkstraShortestPath<CircleNode, ArrowEdge>(
                    scene.Graph, this.Directed, this.Reversed));
        }

        public override string ToString()
        {
            return "Dijkstra";
        }
    }

}
