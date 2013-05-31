using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using GraphForms.Algorithms;
using GraphForms.Algorithms.ConnectedComponents;
using GraphForms.Algorithms.Path;
using GraphForms.Algorithms.SpanningTree;

namespace GraphAlgorithmDemo
{
    public abstract class StyleAlgorithm
    {
        public static readonly Color[] sLineColors = new Color[]
        {
            Color.Red, Color.Green, Color.Blue,
            Color.Yellow, Color.Magenta, Color.Cyan
        };

        protected readonly CircleNodeScene mScene;

        private bool bDirected;
        private bool bReversed;

        public StyleAlgorithm(CircleNodeScene scene,
            bool directed, bool reversed)
        {
            this.mScene = scene;
            this.bDirected = directed;
            this.bReversed = reversed;
        }

        public virtual bool EnableDirected
        {
            get { return true; }
        }

        public bool Directed
        {
            get { return this.bDirected; }
            set { this.bDirected = value; }
        }

        public virtual bool EnableReversed
        {
            get { return true; }
        }

        public bool Reversed
        {
            get { return this.bReversed; }
            set { this.bReversed = value; }
        }

        public abstract void Compute();
    }

    public class BCCStyleAlgorithm : StyleAlgorithm
    {
        private BCCAlgorithm<CircleNode, ArrowEdge> mAlg;

        public BCCStyleAlgorithm(CircleNodeScene scene)
            : base(scene, false, false)
        {
        }

        public override bool EnableDirected
        {
            get { return false; }
        }

        public override void Compute()
        {
            this.mAlg = new BCCAlgorithm<CircleNode, ArrowEdge>(
                this.mScene.Graph, this.Reversed);
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
            CircleNode[] nodes = this.mAlg.ArticulationNodes;
            for (i = 0; i < nodes.Length; i++)
            {
                nodes[i].MarkerColor = sLineColors[0];
            }
            this.mAlg.ArticulateToLargerCompactGroups();
            CircleNode[][] cGrps = this.mAlg.CompactGroups;
            for (i = 0; i < cGrps.Length; i++)
            {
                nodes = cGrps[i];
                for (j = 0; j < nodes.Length; j++)
                {
                    nodes[j].BorderColor = sLineColors[i % sC];
                }
            }
        }

        public override string ToString()
        {
            return "Biconnected Components";
        }
    }

    public class SCCStyleAlgorithm : StyleAlgorithm
    {
        private SCCAlgorithm<CircleNode, ArrowEdge> mAlg;

        public SCCStyleAlgorithm(CircleNodeScene scene)
            : base(scene, true, false)
        {
        }

        public override bool EnableDirected
        {
            get { return false; }
        }

        public override void Compute()
        {
            this.mAlg = new SCCAlgorithm<CircleNode, ArrowEdge>(
                this.mScene.Graph, this.Reversed);
            this.mAlg.Compute();
            CircleNode[] nodes;
            CircleNode[][] comps = this.mAlg.Components;
            int i, j, sC = sLineColors.Length;
            for (i = 0; i < comps.Length; i++)
            {
                nodes = comps[i];
                for (j = 0; j < nodes.Length; j++)
                {
                    nodes[j].BorderColor = sLineColors[i % sC];
                }
            }
            nodes = this.mAlg.Roots;
            for (i = 0; i < nodes.Length; i++)
            {
                nodes[i].MarkerColor = sLineColors[0];
            }
        }

        public override string ToString()
        {
            return "Strongly Connected Components";
        }
    }

    public abstract class SpanTreeStyleAlgorithm : StyleAlgorithm
    {
        private ISpanningTreeAlgorithm<CircleNode, ArrowEdge> mAlg;

        public SpanTreeStyleAlgorithm(CircleNodeScene scene,
            bool directed, bool reversed)
            : base(scene, directed, reversed)
        {
        }

        protected abstract ISpanningTreeAlgorithm<CircleNode, ArrowEdge> a();

        public override void Compute()
        {
            this.mAlg = a();
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
    }

    public class BFSpanTreeStyleAlgorithm : SpanTreeStyleAlgorithm
    {
        public BFSpanTreeStyleAlgorithm(CircleNodeScene scene)
            : base(scene, true, false)
        {
        }

        protected override ISpanningTreeAlgorithm<CircleNode, ArrowEdge> a()
        {
            return new BFSpanningTreeAlgorithm<CircleNode, ArrowEdge>(
                this.mScene.Graph, this.Directed, this.Reversed);
        }

        public override string ToString()
        {
            return "Breadth First Spanning Tree";
        }
    }

    public class DFSpanTreeStyleAlgorithm : SpanTreeStyleAlgorithm
    {
        public DFSpanTreeStyleAlgorithm(CircleNodeScene scene)
            : base(scene, true, false)
        {
        }

        protected override ISpanningTreeAlgorithm<CircleNode, ArrowEdge> a()
        {
            return new DFSpanningTreeAlgorithm<CircleNode, ArrowEdge>(
                this.mScene.Graph, this.Directed, this.Reversed);
        }

        public override string ToString()
        {
            return "Depth First Spanning Tree";
        }
    }

    public class KruskalSpanTreeStyleAlgorithm : SpanTreeStyleAlgorithm
    {
        public KruskalSpanTreeStyleAlgorithm(CircleNodeScene scene)
            : base(scene, false, false)
        {
        }

        public override bool EnableDirected
        {
            get { return false; }
        }

        public override bool EnableReversed
        {
            get { return false; }
        }

        protected override ISpanningTreeAlgorithm<CircleNode, ArrowEdge> a()
        {
            return new KruskalMinSpanningTreeAlgorithm<CircleNode, ArrowEdge>(
                this.mScene.Graph);
        }

        public override string ToString()
        {
            return "Kruskal Minimum Spanning Tree";
        }
    }

    public class BoruvkaSpanTreeStyleAlgorithm : SpanTreeStyleAlgorithm
    {
        public BoruvkaSpanTreeStyleAlgorithm(CircleNodeScene scene)
            : base(scene, false, false)
        {
        }

        public override bool EnableDirected
        {
            get { return false; }
        }

        public override bool EnableReversed
        {
            get { return false; }
        }

        protected override ISpanningTreeAlgorithm<CircleNode, ArrowEdge> a()
        {
            return new BoruvkaMinSpanningTreeAlgorithm<CircleNode, ArrowEdge>(
                this.mScene.Graph);
        }

        public override string ToString()
        {
            return "Borůvka Minimum Spanning Tree";
        }
    }

    public class DFLongestPathStyleAlgorithm : StyleAlgorithm
    {
        private DFLongestPathAlgorithm<CircleNode, ArrowEdge> mAlg;

        public DFLongestPathStyleAlgorithm(CircleNodeScene scene)
            : base(scene, true, false)
        {
        }

        public override void Compute()
        {
            this.mAlg = new DFLongestPathAlgorithm<CircleNode, ArrowEdge>(
                this.mScene.Graph, this.Directed, this.Reversed);
            this.mAlg.Compute();
            DirectionalGraph<CircleNode, ArrowEdge>.GraphEdge[] edges
                = this.mScene.Graph.InternalEdges;
            for (int i = 0; i < edges.Length; i++)
            {
                edges[i].Data.LineColor = Color.Black;
            }
            ArrowEdge[] pEdges = this.mAlg.PathEdges;
            for (int j = 0; j < pEdges.Length; j++)
            {
                pEdges[j].LineColor = Color.Red;
            }
        }

        public override string ToString()
        {
            return "Depth First Longest Path";
        }
    }

    public class ClearAllStyleAlgorithm : StyleAlgorithm
    {
        public ClearAllStyleAlgorithm(CircleNodeScene scene)
            : base(scene, false, false)
        {
        }

        public override bool EnableDirected
        {
            get { return false; }
        }

        public override bool EnableReversed
        {
            get { return false; }
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
            DirectionalGraph<CircleNode, ArrowEdge>.GraphNode[] nodes
                = this.mScene.Graph.InternalNodes;
            for (int j = 0; j < nodes.Length; j++)
            {
                nodes[j].Data.MarkerColor = Color.Transparent;
                nodes[j].Data.BorderColor = Color.Black;
                nodes[j].Data.TextString = null;
            }
        }

        public override string ToString()
        {
            return "Clear All Styles";
        }
    }
}
