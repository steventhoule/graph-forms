﻿using System;
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
            Color.Orange, Color.Magenta, Color.Cyan
        };

        private bool bDirected;
        private bool bReversed;

        public StyleAlgorithm(bool directed, bool reversed)
        {
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

        public abstract void Compute(CircleNodeScene scene);
    }

    public class BCCStyleAlgorithm : StyleAlgorithm
    {
        private BCCAlgorithm<CircleNode, ArrowEdge> mAlg;

        public BCCStyleAlgorithm()
            : base(false, false)
        {
        }

        public override bool EnableDirected
        {
            get { return false; }
        }

        public override void Compute(CircleNodeScene scene)
        {
            this.mAlg = new BCCAlgorithm<CircleNode, ArrowEdge>(
                scene.Graph, this.Reversed);
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

        public SCCStyleAlgorithm()
            : base(true, false)
        {
        }

        public override bool EnableDirected
        {
            get { return false; }
        }

        public override void Compute(CircleNodeScene scene)
        {
            this.mAlg = new SCCAlgorithm<CircleNode, ArrowEdge>(
                scene.Graph, this.Reversed);
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

    public class WCCStyleAlgorithm : StyleAlgorithm
    {
        private WCCAlgorithm<CircleNode, ArrowEdge> mAlg;

        public WCCStyleAlgorithm()
            : base(true, false)
        {
        }

        public override bool EnableDirected
        {
            get { return false; }
        }

        public override void Compute(CircleNodeScene scene)
        {
            this.mAlg = new WCCAlgorithm<CircleNode, ArrowEdge>(
                scene.Graph, this.Reversed);
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
            /*nodes = this.mAlg.Roots;
            for (i = 0; i < nodes.Length; i++)
            {
                nodes[i].MarkerColor = sLineColors[0];
            }/* */
        }

        public override string ToString()
        {
            return "Weakly Connected Components";
        }
    }

    public abstract class SpanTreeStyleAlgorithm : StyleAlgorithm
    {
        private ISpanningTreeAlgorithm<CircleNode, ArrowEdge> mAlg;

        public SpanTreeStyleAlgorithm(bool directed, bool reversed)
            : base(directed, reversed)
        {
        }

        protected abstract ISpanningTreeAlgorithm<CircleNode, ArrowEdge> a(
            CircleNodeScene scene);

        public override void Compute(CircleNodeScene scene)
        {
            this.mAlg = a(scene);
            this.mAlg.Compute();
            int i;
            Digraph<CircleNode, ArrowEdge>.GEdge[] edges
                = scene.Graph.InternalEdges;
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
        public BFSpanTreeStyleAlgorithm()
            : base(true, false)
        {
        }

        protected override ISpanningTreeAlgorithm<CircleNode, ArrowEdge> a(
            CircleNodeScene scene)
        {
            return new BFSpanningTreeAlgorithm<CircleNode, ArrowEdge>(
                scene.Graph, this.Directed, this.Reversed);
        }

        public override string ToString()
        {
            return "Breadth First Spanning Tree";
        }
    }

    public class DFSpanTreeStyleAlgorithm : SpanTreeStyleAlgorithm
    {
        public DFSpanTreeStyleAlgorithm()
            : base(true, false)
        {
        }

        protected override ISpanningTreeAlgorithm<CircleNode, ArrowEdge> a(
            CircleNodeScene scene)
        {
            return new DFSpanningTreeAlgorithm<CircleNode, ArrowEdge>(
                scene.Graph, this.Directed, this.Reversed);
        }

        public override string ToString()
        {
            return "Depth First Spanning Tree";
        }
    }

    public class KruskalSpanTreeStyleAlgorithm : SpanTreeStyleAlgorithm
    {
        public KruskalSpanTreeStyleAlgorithm()
            : base(false, false)
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

        protected override ISpanningTreeAlgorithm<CircleNode, ArrowEdge> a(
            CircleNodeScene scene)
        {
            return new KruskalMinSpanningTreeAlgorithm<CircleNode, ArrowEdge>(
                scene.Graph);
        }

        public override string ToString()
        {
            return "Kruskal Minimum Spanning Tree";
        }
    }

    public class BoruvkaSpanTreeStyleAlgorithm : SpanTreeStyleAlgorithm
    {
        public BoruvkaSpanTreeStyleAlgorithm()
            : base(false, false)
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

        protected override ISpanningTreeAlgorithm<CircleNode, ArrowEdge> a(
            CircleNodeScene scene)
        {
            return new BoruvkaMinSpanningTreeAlgorithm<CircleNode, ArrowEdge>(
                scene.Graph);
        }

        public override string ToString()
        {
            return "Borůvka Minimum Spanning Tree";
        }
    }

    public class DFLongestPathStyleAlgorithm : StyleAlgorithm
    {
        private DFLongestPathAlgorithm<CircleNode, ArrowEdge> mAlg;

        public DFLongestPathStyleAlgorithm()
            : base(true, false)
        {
        }

        public override void Compute(CircleNodeScene scene)
        {
            this.mAlg = new DFLongestPathAlgorithm<CircleNode, ArrowEdge>(
                scene.Graph, this.Directed, this.Reversed);
            this.mAlg.Compute();
            Digraph<CircleNode, ArrowEdge>.GEdge[] edges
                = scene.Graph.InternalEdges;
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
        public ClearAllStyleAlgorithm()
            : base(false, false)
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

        public override void Compute(CircleNodeScene scene)
        {
            Digraph<CircleNode, ArrowEdge>.GEdge[] edges
                = scene.Graph.InternalEdges;
            for (int i = 0; i < edges.Length; i++)
            {
                edges[i].Data.LineColor = Color.Black;
                edges[i].Data.LineDashStyle = DashStyle.Solid;
            }
            Digraph<CircleNode, ArrowEdge>.GNode[] nodes
                = scene.Graph.InternalNodes;
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
