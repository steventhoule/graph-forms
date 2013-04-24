using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using GraphForms.Algorithms;
using GraphForms.Algorithms.ConnectedComponents;
using GraphForms.Algorithms.SpanningTree;

namespace GraphAlgorithmDemo
{
    public abstract class StyleAlgorithm
    {
        protected readonly CircleNodeScene mScene;

        public StyleAlgorithm(CircleNodeScene scene)
        {
            this.mScene = scene;
        }

        public abstract void Compute();
    }

    public class BCCStyleAlgorithm : StyleAlgorithm
    {
        private static readonly Color[] sLineColors = new Color[]
        {
            Color.Black, Color.Red, Color.Green, Color.Blue,
            Color.Yellow, Color.Magenta, Color.Cyan
        };

        private BCCAlgorithm2<CircleNode, ArrowEdge> mAlg;

        public BCCStyleAlgorithm(CircleNodeScene scene)
            : base(scene)
        {
        }

        public override void Compute()
        {
            this.mAlg = new BCCAlgorithm2<CircleNode, ArrowEdge>(
                this.mScene.Graph);
            this.mAlg.Compute();
            ArrowEdge[] comp;
            ArrowEdge[][] comps = this.mAlg.Components;
            int i, j, sC = sLineColors.Length;
            for (i = 0; i < comps.Length; i++)
            {
                comp = comps[i];
                for (j = 0; j < comp.Length; j++)
                {
                    comp[j].LineColor = sLineColors[i % sC];
                }
            }
        }

        public override string ToString()
        {
            return "Biconnected Components";
        }
    }

    public class BFSpanTreeStyleAlgorithm : StyleAlgorithm
    {
        private BFSpanningTreeAlgorithm<CircleNode, ArrowEdge> mAlg;

        public BFSpanTreeStyleAlgorithm(CircleNodeScene scene)
            : base(scene)
        {
        }

        public override void Compute()
        {
            this.mAlg = new BFSpanningTreeAlgorithm<CircleNode, ArrowEdge>(
                this.mScene.Graph);
            this.mAlg.Compute();

            int i;
            DirectionalGraph<CircleNode, ArrowEdge>.GraphEdge[] edges
                = this.mScene.Graph.InternalEdges;
            for (i = 0; i < edges.Length; i++)
            {
                edges[i].Data.LineDashStyle = DashStyle.Solid;
            }
            edges = this.mAlg.SpanningTree.InternalEdges;
            for (i = 0; i < edges.Length; i++)
            {
                edges[i].Data.LineDashStyle = DashStyle.Dash;
            }
        }

        public override string ToString()
        {
            return "Breadth First Spanning Tree";
        }
    }

    public class DFSpanTreeStyleAlgorithm : StyleAlgorithm
    {
        private DFSpanningTreeAlgorithm<CircleNode, ArrowEdge> mAlg;

        public DFSpanTreeStyleAlgorithm(CircleNodeScene scene)
            : base(scene)
        {
        }

        public override void Compute()
        {
            this.mAlg = new DFSpanningTreeAlgorithm<CircleNode, ArrowEdge>(
                this.mScene.Graph);
            this.mAlg.Compute();

            int i;
            DirectionalGraph<CircleNode, ArrowEdge>.GraphEdge[] edges
                = this.mScene.Graph.InternalEdges;
            for (i = 0; i < edges.Length; i++)
            {
                edges[i].Data.LineDashStyle = DashStyle.Solid;
            }
            edges = this.mAlg.SpanningTree.InternalEdges;
            for (i = 0; i < edges.Length; i++)
            {
                edges[i].Data.LineDashStyle = DashStyle.Dash;
            }
        }

        public override string ToString()
        {
            return "Depth First Spanning Tree";
        }
    }

    public class ClearAllStyleAlgorithm : StyleAlgorithm
    {
        public ClearAllStyleAlgorithm(CircleNodeScene scene)
            : base(scene)
        {
        }

        public override void Compute()
        {
            DirectionalGraph<CircleNode, ArrowEdge>.GraphEdge[] edges
                = this.mScene.Graph.InternalEdges;
            for (int i = 0; i < edges.Length; i++)
            {
                edges[i].Data.LineColor = Color.Black;
                edges[i].Data.LineDashStyle = DashStyle.Solid;
            }
        }

        public override string ToString()
        {
            return "Clear All Styles";
        }
    }
}
