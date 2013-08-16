﻿using System;
using GraphForms.Algorithms.Collections;
using GraphForms.Algorithms.ConnectedComponents;
using GraphForms.Algorithms.Layout.Tree;
using GraphForms.Algorithms.SpanningTree;

namespace GraphForms.Algorithms.Layout.Circular
{
    public class BalloonCirclesLayoutAlgorithm<Node, Edge>
        : LayoutAlgorithm<Node, Edge>
        where Node : class, ILayoutNode
        where Edge : IGraphEdge<Node>, IUpdateable
    {
        /*private class CircleLayouter
            : FDSingleCircleLayoutAlgorithm<Node, Edge>
        {
            public CircleLayouter(Digraph<Node, Edge> graph,
                IClusterNode clusterNode)
                : base(graph, clusterNode)
            {
            }

            public CircleLayouter(Digraph<Node, Edge> graph,
                Box2F boundingBox)
                : base(graph, boundingBox)
            {
            }

            public void CallBeginIteration(uint iter)
            {
                this.BeginIteration(iter);
            }

            public void CallPerformIteration(uint iter)
            {
                this.PerformIteration(iter);
            }

            public void CallEndIteration(uint iter)
            {
                this.EndIteration(iter);
            }
        }/* */

        private class CachedNodeSequencer : NodeSequencer<Node, Edge>
        {
            public readonly NodeSequencer<Node, Edge> Sequencer;

            private bool bDirty;
            private int mCacheCount;
            private Digraph<Node, Edge>.GNode[] mCache;

            public CachedNodeSequencer(NodeSequencer<Node, Edge> seq)
            {
                this.Sequencer = seq;
                this.bDirty = true;
                this.mCacheCount = 0;
                this.mCache = new Digraph<Node, Edge>.GNode[0];
            }

            public void SetDirty(bool dirty, Digraph<Node, Edge> graph)
            {
                this.bDirty = dirty;
                if (dirty)
                {
                    this.mCacheCount = 0;
                    int count = graph.NodeCount;
                    if (this.mCache.Length < count)
                    {
                        this.mCache = new Digraph<Node, Edge>.GNode[count];
                    }
                }
            }

            public override void ArrangeNodes(Digraph<Node, Edge> graph, 
                Digraph<Node, Edge>.GNode[] nodes)
            {
                int i;
                if (this.bDirty)
                {
                    this.Sequencer.ArrangeNodes(graph, nodes);
                    for (i = 0; i < nodes.Length; i++)
                    {
                        if (nodes[i] != null && !nodes[i].Hidden)
                            this.mCache[this.mCacheCount++] = nodes[i];
                    }
                }
                else
                {
                    int j = 0;
                    for (i = 0; i < this.mCacheCount; i++)
                    {
                        if (!this.mCache[i].Hidden)
                            nodes[j++] = this.mCache[i];
                    }
                }
            }
        }

        private class GNode : ILayoutNode
        {
            public readonly Node Data;
            public readonly int CircleID;
            public readonly int GroupSize;
            
            public double CircleAngle;
            public double CircleRadius;
            public double ECRadius;

            //public double StackAngle;

            public GNode(int circleID, int groupSize, Node data)
            {
                this.Data = data;
                this.CircleID = circleID;
                this.GroupSize = groupSize;
                this.CircleAngle = 0.0;
            }

            //public abstract Node Data { get; }

            //public abstract CircleLayouter Circle { get; }

            public Box2F LayoutBBox
            {
                get
                {
                    if (this.Data == null)
                    {
                        float rad = (float)(2.0 * this.CircleRadius);
                        return new Box2F(rad / -2, rad / -2, rad, rad);
                    }
                    return this.Data.LayoutBBox;
                }
            }

            private float mX;
            private float mY;

            public float X
            {
                get { return this.Data == null ? this.mX : this.Data.X; }
            }

            public float Y
            {
                get { return this.Data == null ? this.mY : this.Data.Y; }
            }

            public void SetPosition(float x, float y)
            {
                if (this.Data == null)
                {
                    this.mX = x;
                    this.mY = y;
                }
                else
                {
                    this.Data.SetPosition(x, y);
                }
            }

            private bool bFixed;
            //private float mNewX;
            //private float mNewY;

            /*public void CalcPositionFixed()
            {
                if (this.Circle != null)
                {
                    Digraph<Node, Edge>.GNode[] nodes
                        = this.Circle.Graph.InternalNodes;
                    this.bFixed = false;
                    for (int i = 0; i < nodes.Length && !this.bFixed; i++)
                    {
                        this.bFixed = nodes[i].Data.PositionFixed;
                    }
                }
                else
                {
                    this.bFixed = this.Data.PositionFixed;
                }
            }/* */

            public void SetPosFixed(bool posFixed)
            {
                this.bFixed = posFixed;
            }

            public bool PositionFixed
            {
                get { return this.bFixed; }
            }

            /*public virtual float NewX
            {
                get { return this.mNewX; }
            }

            public virtual float NewY
            {
                get { return this.mNewY; }
            }

            public virtual void SetNewPosition(float newX, float newY)
            {
                this.mNewX = newX;
                this.mNewY = newY;
            }/* */
        }

        /*private class BalloonCircle : GNode
        {
            private BalloonCirclesLayoutAlgorithm<Node, Edge> mOwner;
            private CircleLayouter mCircle;

            public BalloonCircle(int circleID, Digraph<Node, Edge> graph,
                BalloonCirclesLayoutAlgorithm<Node, Edge> owner)
                : base(circleID)
            {
                this.mOwner = owner;
                if (owner.mClusterNode == null)
                {
                    this.mCircle = new CircleLayouter(
                        graph, owner.BoundingBox);
                }
                else
                {
                    this.mCircle = new CircleLayouter(
                        graph, owner.mClusterNode);
                }
                this.mCircle.NodeSpacing = owner.mNodeSpacing;
                this.mCircle.Centering = CircleCentering.Predefined;
                this.mCircle.MinRadius = owner.mMinRadius;
                this.mCircle.FreeArc = owner.mFreeArc;

                this.mCircle.AdjustCenter = true;
                this.mCircle.AdjustAngle = false;
                this.mCircle.SpringMultiplier = owner.mSpringMult;
                this.mCircle.MagneticMultiplier = owner.mMagnetMult;
                this.mCircle.MagneticExponent = owner.mMagnetExp;

                Node node;
                double cx = 0.0;
                double cy = 0.0;
                Digraph<Node, Edge>.GNode[] nodes = graph.InternalNodes;
                for (int i = 0; i < nodes.Length; i++)
                {
                    node = nodes[i].Data;
                    cx += node.X;
                    cy += node.Y;
                }
                this.mCircle.CenterX = cx / nodes.Length;
                this.mCircle.CenterY = cy / nodes.Length;
            }

            public override Node Data
            {
                get { return null; }
            }

            public override CircleLayouter Circle
            {
                get { return this.mCircle; }
            }

            private static readonly double mul = 2.0 / Math.Sqrt(2.0);

            public override Box2F LayoutBBox
            {
                get
                {
                    //float rad = (float)(mul * this.mCircle.BoundingRadius);
                    float rad = (float)(2.0 * this.mCircle.BoundingRadius);
                    return new Box2F(rad / -2, rad / -2, rad, rad);
                }
            }

            public override float X
            {
                get { return (float)this.mCircle.CalcCenterX; }
            }

            public override float Y
            {
                get { return (float)this.mCircle.CalcCenterY; }
            }

            public override void SetPosition(float x, float y)
            {
                this.mCircle.CenterX = x;
                this.mCircle.CenterY = y;
            }

            public override bool PositionFixed
            {
                get 
                {
                    Digraph<Node, Edge>.GNode[] nodes
                        = this.mCircle.Graph.InternalNodes;
                    bool movable = true;
                    for (int i = 0; i < nodes.Length && movable; i++)
                    {
                        movable = !nodes[i].mData.PositionFixed;
                    }
                    return !movable;
                }
            }

            public override string ToString()
            {
                Digraph<Node, Edge>.GNode[] nodes
                    = this.mCircle.Graph.InternalNodes;
                StringBuilder sb = new StringBuilder(5 * nodes.Length);
                for (int i = 0; i < nodes.Length - 1; i++)
                {
                    sb.Append(string.Concat(nodes[i].Data.ToString(), ","));
                }
                sb.Append(nodes[nodes.Length - 1].Data.ToString());
                return sb.ToString();
            }
        }

        private class BalloonNode : GNode
        {
            private Node mNode;

            public BalloonNode(int circleID, Node node)
                : base(circleID)
            {
                this.mNode = node;
            }

            public override Node Data
            {
                get { return this.mNode; }
            }

            public override CircleLayouter Circle
            {
                get { return null; }
            }

            public override Box2F LayoutBBox
            {
                get { return this.mNode.LayoutBBox; }
            }

            public override float X
            {
                get { return this.mNode.X; }
            }

            public override float Y
            {
                get { return this.mNode.Y; }
            }

            public override void SetPosition(float x, float y)
            {
                this.mNode.SetPosition(x, y);
            }

            public override bool PositionFixed
            {
                get { return this.mNode.PositionFixed; }
            }

            public override float NewX
            {
                get { return this.mNode.NewX; }
            }

            public override float NewY
            {
                get { return this.mNode.NewY; }
            }

            public override void SetNewPosition(float newX, float newY)
            {
                this.mNode.SetNewPosition(newX, newY);
            }

            public override string ToString()
            {
                return this.mNode.ToString();
            }
        }/* */

        private class GEdge : IGraphEdge<GNode>, IUpdateable
        {
            private GNode mSrcNode;
            private GNode mDstNode;
            
            public Edge[] Edges;
            public int[] SrcIndexes;
            public int[] DstIndexes;
            public int ECount;

            public GEdge(GNode srcNode, GNode dstNode)
            {
                this.mSrcNode = srcNode;
                this.mDstNode = dstNode;
            }

            public void AddEdge(Edge edge, int srcIndex, int dstIndex)
            {
                if (this.Edges == null)
                {
                    this.SrcIndexes = new int[] { srcIndex };
                    this.DstIndexes = new int[] { dstIndex };
                    this.Edges = new Edge[] { edge };
                    this.ECount = 1;
                }
                else
                {
                    if (this.ECount == this.Edges.Length)
                    {
                        Edge[] edges = new Edge[2 * this.ECount];
                        Array.Copy(this.Edges, 0, edges, 0, this.ECount);
                        this.Edges = edges;

                        int[] ins;
                        if (this.SrcIndexes != null)
                        {
                            ins = new int[2 * this.ECount];
                            Array.Copy(this.SrcIndexes, 0, ins, 0, 
                                this.ECount);
                            this.SrcIndexes = ins;
                        }
                        if (this.DstIndexes != null)
                        {
                            ins = new int[2 * this.ECount];
                            Array.Copy(this.DstIndexes, 0, ins, 0, 
                                this.ECount);
                            this.DstIndexes = ins;
                        }
                    }
                    if (this.SrcIndexes != null)
                        this.SrcIndexes[this.ECount] = srcIndex;
                    if (this.DstIndexes != null)
                        this.DstIndexes[this.ECount] = dstIndex;
                    this.Edges[this.ECount++] = edge;
                }
            }

            public GNode SrcNode
            {
                get { return this.mSrcNode; }
            }

            public GNode DstNode
            {
                get { return this.mDstNode; }
            }

            public float Weight
            {
                get 
                {
                    double netWeight = 0.0;
                    for (int i = 0; i < this.ECount; i++)
                        netWeight += this.Edges[i].Weight;
                    return (float)(1.0 / netWeight);
                }
            }

            public void SetSrcNode(GNode srcNode)
            {
                this.mSrcNode = srcNode;
            }

            public void SetDstNode(GNode dstNode)
            {
                this.mDstNode = dstNode;
            }

            public void Update()
            {
                for (int i = 0; i < this.ECount; i++)
                    this.Edges[i].Update();
            }
        }

        private class BalloonLayouter
            : BalloonTreeLayoutAlgorithm<GNode, GEdge>
        {
            private readonly BalloonCirclesLayoutAlgorithm<Node, Edge> mOwner;

            private double mMaxDevAng = Math.PI / 2.0;

            public BalloonLayouter(BalloonCirclesLayoutAlgorithm<Node, Edge> owner,
                Digraph<GNode, GEdge> graph, IClusterNode clusterNode)
                : base(graph, clusterNode)
            {
                this.mOwner = owner;
            }

            public BalloonLayouter(BalloonCirclesLayoutAlgorithm<Node, Edge> owner,
                Digraph<GNode, GEdge> graph, Box2F boundingBox)
                : base(graph, boundingBox)
            {
                this.mOwner = owner;
            }

            public double MaximumDeviationAngle
            {
                get { return this.mMaxDevAng; }
                set
                {
                    if (this.mMaxDevAng != value)
                    {
                        this.mMaxDevAng = value;
                        this.ForceRecalculateBranchPositions();
                    }
                }
            }

            public double DegMaxDeviationAngle
            {
                get { return 180.0 * this.mMaxDevAng / Math.PI; }
                set
                {
                    value = Math.PI * value / 180.0;
                    if (this.mMaxDevAng != value)
                    {
                        this.mMaxDevAng = value;
                        this.ForceRecalculateBranchPositions();
                    }
                }
            }

            protected override double GetBoundingRadius(GNode node)
            {
                return node.Data == null ? node.CircleRadius
                    : base.GetBoundingRadius(node);
            }

            public CircleTree<GNode, GEdge> GetBalloonTree()
            {
                return this.BalloonTree;
            }

            /*public void CalcCircleRadii()
            {
                if (this.BalloonTree != null)
                {
                    this.CalcCircleRadii(this.BalloonTree);
                    this.MarkDirty();
                }
            }

            private void CalcCircleRadii(
                CircleTree<GNode, GEdge> root)
            {
                CircleLayouter circle = root.NodeData.Circle;
                if (circle != null)
                {
                    root.Radius = circle.BoundingRadius;
                }
                if (root.BranchCount > 0)
                {
                    CircleTree<GNode, GEdge>[] branches
                        = root.Branches;
                    for (int i = 0; i < branches.Length; i++)
                    {
                        this.CalcCircleRadii(branches[i]);
                    }
                }
            }

            public void SetAdjustCircleAngle(bool adjust)
            {
                if (this.BalloonTree != null)
                {
                    GNode root = this.BalloonTree.NodeData;
                    if (root.Circle != null)
                    {
                        root.Circle.AdjustAngle = adjust;
                    }
                }
            }

            public void AdjustRootAngleFromCircleAngle()
            {
                if (this.BalloonTree != null)
                {
                    GNode root = this.BalloonTree.NodeData;
                    if (root.Circle != null)
                    {
                        this.RootAngle = root.Circle.Angle 
                                       - root.CircleAngle;
                    }
                }
            }

            public void SetCircleAngles()
            {
                CircleTree<GNode, GEdge> root = this.BalloonTree;
                if (root != null)
                {
                    CircleLayouter circle = root.NodeData.Circle;
                    if (circle != null)
                    {
                        circle.Angle = this.RootAngle 
                                     + root.NodeData.CircleAngle;
                    }
                    if (root.BranchCount > 0)
                    {
                        CircleTree<GNode, GEdge>[] branches
                            = root.Branches;
                        for (int i = 0; i < branches.Length; i++)
                        {
                            this.SetCircleAngles(
                                this.RootAngle, branches[i]);
                        }
                    }
                }
            }

            private void SetCircleAngles(double angle,
                CircleTree<GNode, GEdge> root)
            {
                CircleLayouter circle = root.NodeData.Circle;
                angle += root.Angle;
                if (circle != null)
                {
                    circle.Angle = angle + root.NodeData.CircleAngle;
                }
                if (root.BranchCount > 0)
                {
                    CircleTree<GNode, GEdge>[] branches
                        = root.Branches;
                    for (int i = 0; i < branches.Length; i++)
                    {
                        this.SetCircleAngles(angle, branches[i]);
                    }
                }
            }/* */

            protected override double GetMaximumTreeWedge(
                CircleTree<GNode, GEdge> root)
            {
                if (root.NodeData.Data != null)
                {
                    return base.GetMaximumTreeWedge(root);
                }
                if (root.Root == null)
                {
                    return this.MaximumRootWedge;
                }
                return this.MaximumTreeWedge;
            }

            protected override void CalculateBranchPositions(
                CircleTree<GNode, GEdge>[] branches)
            {
                int i, j;
                double a, dx, dy;
                //CircleLayouter circle;
                SingleCircleLayoutAlgorithm<Node, Edge> circle
                    = this.mOwner.mCircleLayouter;
                GEdge edge;
                CircleTree<GNode, GEdge> ct;
                // Calculate the relative rotation angle of each leaf's
                // embedding circle based on the average of the angles of
                // the group edge's nodes around its center.
                for (i = 0; i < branches.Length; i++)
                {
                    ct = branches[i];
                    if (ct.BranchCount == 0 && ct.NodeData.Data == null)
                    {
                        edge = ct.EdgeData;
                        //circle = ct.NodeData.Circle;
                        //wedge = 0.0;
                        dx = dy = 0.0;
                        if (ct.NodeData.CircleID == edge.DstNode.CircleID)
                        {
                            for (j = 0; j < edge.ECount; j++)
                            {
                                a = circle.AngleAt(edge.DstIndexes[j]);
                                //wedge += a < 0.0 ? a + 2 * Math.PI : a;
                                dx -= Math.Cos(a);
                                dy += Math.Sin(a);
                            }
                        }
                        else
                        {
                            for (j = 0; j < edge.ECount; j++)
                            {
                                a = circle.AngleAt(edge.SrcIndexes[j]);
                                //wedge += a < 0.0 ? a + 2 * Math.PI : a;
                                dx -= Math.Cos(a);
                                dy += Math.Sin(a);
                            }
                        }
                        /*wedge = Math.PI - wedge / edge.EdgeCount;
                        while (wedge < -Math.PI)
                            wedge += 2 * Math.PI;
                        ct.NodeData.CircleAngle = wedge;/* */
                        ct.NodeData.CircleAngle = Math.Atan2(
                            dy / edge.ECount, dx / edge.ECount);
                    }
                }
                // Check if the root is a single node instead of a group of
                // nodes and can be calculated the original way.
                ct = branches[0].Root;
                if (ct.NodeData.Data != null)
                {
                    base.CalculateBranchPositions(branches);
                    return;
                }
                //circle = ct.NodeData.Circle;
                // Calculate the relative rotation angle of the root's
                // embedding circle based on the average of the angles of
                // the group edge's nodes around its center.
                if (ct.Root != null)
                {
                    edge = ct.EdgeData;
                    //wedge = 0.0;
                    dx = dy = 0.0;
                    if (ct.NodeData.CircleID == edge.DstNode.CircleID)
                    {
                        for (j = 0; j < edge.ECount; j++)
                        {
                            a = circle.AngleAt(edge.DstIndexes[j]);
                            //wedge += a < 0.0 ? a + 2 * Math.PI : a;
                            dx -= Math.Cos(a);
                            dy += Math.Sin(a);
                        }
                    }
                    else
                    {
                        for (j = 0; j < edge.ECount; j++)
                        {
                            a = circle.AngleAt(edge.SrcIndexes[j]);
                            //wedge += a < 0.0 ? a + 2 * Math.PI : a;
                            dx -= Math.Cos(a);
                            dy += Math.Sin(a);
                        }
                    }
                    /*wedge = Math.PI - wedge / edge.EdgeCount;
                    while (wedge < -Math.PI)
                        wedge += 2 * Math.PI;
                    ct.NodeData.CircleAngle = wedge;/* */
                    ct.NodeData.CircleAngle = Math.Atan2(
                            dy / edge.ECount, dx / edge.ECount);
                }
                // Set the ideal angles based on the average of the
                // angles of the group edge's nodes around the center
                // of the root group node's embedding circle.
                double[] branchAngles = new double[branches.Length];
                for (i = 0; i < branches.Length; i++)
                {
                    ct = branches[i];
                    edge = ct.EdgeData;
                    //wedge = 0.0;
                    dx = dy = 0.0;
                    if (ct.NodeData.CircleID == edge.DstNode.CircleID)
                    {
                        for (j = 0; j < edge.ECount; j++)
                        {
                            a = circle.AngleAt(edge.SrcIndexes[j]);
                            //wedge += a < 0.0 ? a + 2 * Math.PI : a;
                            dx += Math.Cos(a);
                            dy += Math.Sin(a);
                        }
                    }
                    else
                    {
                        for (j = 0; j < edge.ECount; j++)
                        {
                            a = circle.AngleAt(edge.DstIndexes[j]);
                            //wedge += a < 0.0 ? a + 2 * Math.PI : a;
                            dx += Math.Cos(a);
                            dy += Math.Sin(a);
                        }
                    }
                    /*wedge = wedge / edge.EdgeCount 
                          + ct.Root.NodeData.CircleAngle;
                    if (wedge > Math.PI)
                        wedge -= 2 * Math.PI;
                    branchAngles[i] = wedge;/* */
                    a = Math.Atan2(dy / edge.ECount, dx / edge.ECount)
                        + ct.Root.NodeData.CircleAngle;
                    while (a < -Math.PI)
                        a += 2 * Math.PI;
                    while (a > Math.PI)
                        a -= 2 * Math.PI;
                    branchAngles[i] = a;
                }
                // Set the initial distance of each branch based on MinEdgeLen
                Box2F bbox;
                double rootRad = branches[0].Root.Radius 
                    + this.MinimumEdgeLength;
                double maxDist = -double.MaxValue;
                for (i = 0; i < branches.Length; i++)
                {
                    ct = branches[i];
                    bbox = ct.CalculateBoundingBox();
                    ct.Distance = rootRad - bbox.X;
                    if (ct.Distance > maxDist)
                        maxDist = ct.Distance;
                }
                // Calculate the initial bounding wedges of the branches
                if (this.EqualizeBranchLengths)
                {
                    for (i = 0; i < branches.Length; i++)
                    {
                        ct = branches[i];
                        ct.Distance = maxDist;
                        ct.CalculateAngles();
                    }
                }
                else
                {
                    for (i = 0; i < branches.Length; i++)
                    {
                        branches[i].CalculateAngles();
                    }
                }
                // Sort the branches to their final order counterclockwise
                // around their root starting at -pi (-180 degrees).
                Array.Sort(branchAngles, branches, 0, branches.Length, null);
                // Get the max allowable wedge that the branches can occupy
                double maxWedge = this.GetMaximumTreeWedge(branches[0].Root);
                while (maxWedge < 0.0)
                    maxWedge += 2 * Math.PI;
                while (maxWedge > 2 * Math.PI)
                    maxWedge -= 2 * Math.PI;
                // Set the final polar coordinate positions of the branches
                double wedge, ratio;
                switch (this.BranchSpacing)
                {
                    case CircleSpacing.Fractal:
                        // Calculate the final distances of the branches
                        wedge = maxWedge / (2 * branches.Length);
                        ratio = Math.Sin(wedge);
                        for (i = 0; i < branches.Length; i++)
                        {
                            ct = branches[i];
                            a = Math.Max(ct.LowerWedge, ct.UpperWedge);
                            //while (a > wedge)
                            for (j = 0; j < 50 && a > wedge; j++)
                            {
                                ct.Distance = ct.Distance * Math.Sin(a) / ratio;
                                ct.CalculateAngles();
                                a = Math.Max(ct.LowerWedge, ct.UpperWedge);
                            }
                            if (ct.Distance > maxDist)
                                maxDist = ct.Distance;
                        }
                        if (this.EqualizeBranchLengths)
                        {
                            for (i = 0; i < branches.Length; i++)
                            {
                                branches[i].Distance = maxDist;
                            }
                        }
                        // Calculate the final angles of the branches
                        a = maxWedge / -2.0;
                        for (i = 0; i < branches.Length; i++)
                        {
                            a += wedge;
                            branches[i].Angle = a;
                            a += wedge;
                        }
                        break;
                    case CircleSpacing.SNS:
                        int k, index;
                        for (j = 0; j < 50; j++)
                        {
                            // Set the initial angles of the branches as
                            // close to their ideal angles as possible.
                            // Spread branches that share the same ideal
                            // angle by centering their total wedge at 
                            // their shared ideal angle.
                            /*for (i = 0; i < branches.Length; i++)
                            {
                                branches[i].Angle = branchAngles[i];
                            }/* */
                            index = 0;
                            a = branchAngles[0];
                            ct = branches[0];
                            ct.Angle = a;
                            wedge = ct.LowerWedge + ct.UpperWedge;
                            for (i = 1; i < branches.Length; i++)
                            {
                                ct = branches[i];
                                if (branchAngles[i] != a)
                                {
                                    if (i - index == 1)
                                    {
                                        ct.Angle = branchAngles[i];
                                    }
                                    else
                                    {
                                        a = a - wedge / 2.0;
                                        for (k = index; k < i; k++)
                                        {
                                            ct = branches[k];
                                            a += ct.LowerWedge;
                                            ct.Angle = a;
                                            a += ct.UpperWedge;
                                        }
                                        ct = branches[i];
                                    }
                                    index = i;
                                    a = branchAngles[i];
                                    wedge = 0.0;
                                }
                                wedge += ct.LowerWedge + ct.UpperWedge;
                            }
                            if (i - index == 1)
                            {
                                ct.Angle = branchAngles[index];
                            }
                            if (i - index > 1)
                            {
                                a = a - wedge / 2.0;
                                for (k = index; k < i; k++)
                                {
                                    ct = branches[k];
                                    a += ct.LowerWedge;
                                    ct.Angle = a;
                                    a += ct.UpperWedge;
                                }
                            }/* */
                            // Remove wedge overlap at the tail
                            // by moving the last branch clockwise
                            ct = branches[branches.Length - 1];
                            if (ct.Angle + ct.UpperWedge > maxWedge / 2.0)
                            {
                                ct.Angle = maxWedge / 2.0 - ct.UpperWedge;
                            }
                            // Remove wedge overlap in the body/middle
                            for (i = branches.Length - 2; i >= 1; i--)
                            {
                                ct = branches[i];
                                // Eliminate clockwise overlap
                                // by moving the branch counterclockwise
                                a = branches[i - 1].Angle
                                  + branches[i - 1].UpperWedge;
                                if (ct.Angle - ct.LowerWedge < a)
                                {
                                    ct.Angle = a + ct.LowerWedge;
                                }
                                // Eliminate counterclockwise overlap
                                // by moving the branch clockwise
                                a = branches[i + 1].Angle
                                  - branches[i + 1].LowerWedge;
                                if (ct.Angle + ct.UpperWedge > a)
                                {
                                    ct.Angle = a - ct.UpperWedge;
                                }
                            }
                            if (branches.Length > 1)
                            {
                                // Eliminate counterclockwise overlap at the
                                // head by moving the 1st branch clockwise
                                ct = branches[0];
                                a = branches[1].Angle 
                                  - branches[1].LowerWedge;
                                if (ct.Angle + ct.UpperWedge > a)
                                {
                                    ct.Angle = a - ct.UpperWedge;
                                }
                            }
                            // Calculate the current minimum possible wedge
                            wedge = 0.0;
                            for (i = 0; i < branches.Length; i++)
                            {
                                ct = branches[i];
                                wedge = ct.LowerWedge + ct.UpperWedge;
                            }
                            // Remove wedge overlap at the head by 
                            // eliminating gaps between wedges until it fits
                            if (ct.Angle - ct.LowerWedge < maxWedge / -2.0 &&
                                wedge <= maxWedge)
                            {
                                ct.Angle = ct.LowerWedge - maxWedge / 2.0;
                                for (i = 1; i < branches.Length; i++)
                                {
                                    ct = branches[i];
                                    // Eliminate clockwise overlap
                                    // by moving the branch counterclockwise
                                    a = branches[i - 1].Angle
                                      + branches[i - 1].UpperWedge;
                                    if (ct.Angle - ct.LowerWedge < a)
                                    {
                                        ct.Angle = a + ct.LowerWedge;
                                    }
                                }
                            }
                            // Calculate the maximum deviation angle and
                            // Normalize the angles of the branches
                            ratio = 0.0;
                            for (i = 0; i < branches.Length; i++)
                            {
                                ct = branches[i];
                                a = ct.Angle;
                                while (a < -Math.PI)
                                    a += 2 * Math.PI;
                                while (a > Math.PI)
                                    a -= 2 * Math.PI;
                                ct.Angle = a;
                                // TODO: Ensure this subtract always
                                // works correctly and efficiently
                                a = Math.Abs(a - branchAngles[i]);
                                if (a > Math.PI)
                                    a = 2 * Math.PI - a;
                                if (ratio < a)
                                    ratio = a;
                            }
                            if (wedge <= maxWedge &&
                                ratio <= this.mMaxDevAng)
                            {
                                break;
                            }
                            // Recalculate the distances of the branches
                            if (wedge > maxWedge || ratio > this.mMaxDevAng)
                            {
                                ratio = wedge > maxWedge ? wedge / maxWedge
                                    : (maxWedge + ratio - this.mMaxDevAng) 
                                     / maxWedge;
                                for (i = 0; i < branches.Length; i++)
                                {
                                    ct = branches[i];
                                    ct.Distance = ct.Distance * ratio;
                                    ct.CalculateAngles();
                                }
                            }
                        }
                        break;
                }
            }

            /*public void CallBeginIteration(uint iter)
            {
                this.BeginIteration(iter);
            }

            public void CallPerformIteration(uint iter)
            {
                this.PerformIteration(iter);
            }

            public void CallEndIteration(uint iter)
            {
                this.EndIteration(iter);
            }/* */

            public CircleTree<PrintArray<Node>, int> DebugTree
            {
                get
                {
                    if (this.BalloonTree == null)
                        return null;
                    return this.BuildDebugTree(this.BalloonTree);
                }
            }

            private CircleTree<PrintArray<Node>, int> BuildDebugTree(
                CircleTree<GNode, GEdge> root)
            {
                int i;
                PrintArray<Node> data = new PrintArray<Node>();
                //CircleLayouter circle = root.NodeData.Circle;
                if (root.NodeData.Data == null)
                {
                    int gid = root.NodeData.CircleID;
                    int count = 0;
                    Node[] nodes = new Node[root.NodeData.GroupSize];
                    for (i = 0; i < this.mOwner.mGroupIds.Length; i++)
                    {
                        if (this.mOwner.mGroupIds[i] == gid)
                        {
                            nodes[count++] = this.mOwner.mGraph.NodeAt(i);
                        }
                    }
                    data.Data = nodes;
                }
                else
                {
                    data.Data = new Node[] { root.NodeData.Data };
                }
                CircleTree<PrintArray<Node>, int> child, parent 
                    = new CircleTree<PrintArray<Node>, int>(data, 0, 
                        root.Radius, root.BranchCount);
                parent.Angle = root.Angle;
                parent.Distance = root.Distance;
                CircleTree<GNode, GEdge>[] branches
                    = root.Branches;
                for (i = 0; i < branches.Length; i++)
                {
                    child = this.BuildDebugTree(branches[i]);
                    child.SetRoot(parent);
                }
                return parent;
            }

            public Vec2F DebugRootPosition
            {
                get
                {
                    if (this.BalloonTree == null)
                        return null;
                    GNode root = this.BalloonTree.NodeData;
                    return new Vec2F(root.X, root.Y);
                }
            }
        }

        public enum LayoutStyle
        {
            BccCompact,
            BccIsolated,
            //SingleCircle,
        }

        // Balloon Tree Generating Parameters
        private LayoutStyle mGroupingMethod = LayoutStyle.BccCompact;

        // Physical Animation Parameters
        private bool bAdjustRoots = true;
        private bool bAdjustAngle = false;
        private double mSpringMult = 10;
        private double mMagnetMult = 100;
        private double mMagnetExp = 1;
        private double mRootAngle = 0;

        // Balloon Tree Sorting Parameters
        private bool bInSketchMode = false;

        // Flags and Calculated Values
        private BalloonLayouter mBalloonLayouter;
        private CachedNodeSequencer mNodeSequencer;
        private SingleCircleLayoutAlgorithm<Node, Edge> mCircleLayouter;
        private int[] mGroupIds;
        private GNode[] mGroupNodes;

        private bool bLayouterDirty = true;
        private bool bNodeSequencerDirty = true;
        private bool bCirclePositionsDirty = true;

        public BalloonCirclesLayoutAlgorithm(Digraph<Node, Edge> graph,
            IClusterNode clusterNode)
            : base(graph, clusterNode)
        {
            Digraph<GNode, GEdge> bGraph = new Digraph<GNode, GEdge>(
                    graph.NodeCount, graph.EdgeCount);
            this.mBalloonLayouter 
                = new BalloonLayouter(this, bGraph, clusterNode);
            this.mBalloonLayouter.SpanningTreeGeneration
                = SpanningTreeGen.Kruskal;
            this.mCircleLayouter 
                = new SingleCircleLayoutAlgorithm<Node, Edge>(graph, clusterNode);
            this.mNodeSequencer 
                = new CachedNodeSequencer(new NodeSequencer<Node, Edge>());
            this.mCircleLayouter.NodeSequencer = this.mNodeSequencer;
            this.mCircleLayouter.Centering = CircleCentering.Predefined;
        }

        public BalloonCirclesLayoutAlgorithm(Digraph<Node, Edge> graph,
            Box2F boundingBox)
            : base(graph, boundingBox)
        {
            Digraph<GNode, GEdge> bGraph = new Digraph<GNode, GEdge>(
                    graph.NodeCount, graph.EdgeCount);
            this.mBalloonLayouter 
                = new BalloonLayouter(this, bGraph, boundingBox);
            this.mBalloonLayouter.SpanningTreeGeneration
                = SpanningTreeGen.Kruskal;
            this.mCircleLayouter
                = new SingleCircleLayoutAlgorithm<Node, Edge>(graph, boundingBox);
            this.mNodeSequencer 
                = new CachedNodeSequencer(new NodeSequencer<Node, Edge>());
            this.mCircleLayouter.NodeSequencer = this.mNodeSequencer;
            this.mCircleLayouter.Centering = CircleCentering.Predefined;
        }

        #region Parameters

        public LayoutStyle GroupingMethod
        {
            get { return this.mGroupingMethod; }
            set
            {
                if (this.mGroupingMethod != value)
                {
                    this.mGroupingMethod = value;
                    this.bLayouterDirty = true;
                }
            }
        }

        public NodeSequencer<Node, Edge> NodeSequencer
        {
            get { return this.mNodeSequencer.Sequencer; }
            set
            {
                if (this.mNodeSequencer == null && value != null)
                {
                    this.mNodeSequencer = new CachedNodeSequencer(value);
                    this.mCircleLayouter.NodeSequencer = this.mNodeSequencer;
                    this.bNodeSequencerDirty = true;
                }
                else if (this.mNodeSequencer != null &&
                    this.mNodeSequencer.Sequencer != value)
                {
                    if (value == null)
                    {
                        this.mNodeSequencer = null;
                    }
                    else
                    {
                        this.mNodeSequencer = new CachedNodeSequencer(value);
                    }
                    this.mCircleLayouter.NodeSequencer = this.mNodeSequencer;
                    this.bNodeSequencerDirty = true;
                }
            }
        }

        #region Circle Position Calculation Parameters
        /// <summary>
        /// Gets or sets the method this algorithm uses to calculate the
        /// angles between nodes around the center of the embedding circle.
        /// </summary>
        public CircleSpacing NodeSpacing
        {
            get { return this.mCircleLayouter.NodeSpacing; }
            set
            {
                if (this.mCircleLayouter.NodeSpacing != value)
                {
                    this.mCircleLayouter.NodeSpacing = value;
                    this.bCirclePositionsDirty = true;
                }
            }
        }
        /// <summary>
        /// Gets or sets the minimum radius of the embedding circle
        /// calculated by this layout algorithm.
        /// </summary>
        public double MinRadius
        {
            get { return this.mCircleLayouter.MinRadius; }
            set 
            {
                if (this.mCircleLayouter.MinRadius != value)
                {
                    this.mCircleLayouter.MinRadius = value;
                    this.bCirclePositionsDirty = true;
                }
            }
        }
        /// <summary>
        /// Gets or sets the minimum distance between nodes on the 
        /// embedding circle as measured along the arc between them
        /// on the embedding circle.
        /// </summary>
        public double FreeArc
        {
            get { return this.mCircleLayouter.FreeArc; }
            set 
            {
                if (this.mCircleLayouter.FreeArc != value)
                {
                    this.mCircleLayouter.FreeArc = value;
                    this.bCirclePositionsDirty = true;
                }
            }
        }
        #endregion

        #region Balloon Tree Generating Parameters
        /// <summary>
        /// Gets or sets the method this algorithm uses to build its internal
        /// sparsely connected spanning tree for traversing its graph.
        /// </summary>
        public SpanningTreeGen SpanningTreeGeneration
        {
            get { return this.mBalloonLayouter.SpanningTreeGeneration; }
            set { this.mBalloonLayouter.SpanningTreeGeneration = value; }
        }
        /// <summary>
        /// Gets or sets the method this algorithm uses to choose the root
        /// node from which all subtrees branch off and orbit around.
        /// </summary>
        public TreeRootFinding RootFindingMethod
        {
            get { return this.mBalloonLayouter.RootFindingMethod; }
            set { this.mBalloonLayouter.RootFindingMethod = value; }
        }
        #endregion

        #region Balloon Tree Sorting Parameters
        /// <summary>
        /// Gets or sets whether this balloon tree layout algorithm is in
        /// "sketch mode", in which it arranges subtree/leaf nodes around
        /// their respective root nodes based on their original positions
        /// relative to their roots (in other words, from the original
        /// "sketch" of the graph before this layout algorithm was applied
        /// to it).</summary>
        public bool InSketchMode
        {
            get { return this.bInSketchMode; }
            set
            {
                if (this.bInSketchMode != value)
                {
                    this.bInSketchMode = value;
                    this.mBalloonLayouter.InSketchMode = value;
                }
            }
        }
        #endregion

        #region Balloon Tree Position Calculation Parameters
        /// <summary>
        /// Gets or sets the method this algorithm uses to calculate the
        /// angles between subtrees/leaves around their root.
        /// </summary><remarks>
        /// If set to <see cref="CircleSpacing.SNS"/>, the angle computation 
        /// will use the subtree's/leaf's bounding wedge defined by its 
        /// <see cref="P:CircleTree`2.UpperWedge"/> and 
        /// <see cref="P:CircleTree`2.LowerWedge"/> properties
        /// calculated by <see cref="M:CircleTree`2.CalculateAngles()"/>.
        /// </remarks>
        public CircleSpacing BranchSpacing
        {
            get { return this.mBalloonLayouter.BranchSpacing; }
            set { this.mBalloonLayouter.BranchSpacing = value; }
        }
        /// <summary>
        /// Gets or sets the method that this algorithm uses to calculate
        /// the initial position of the root of the entire balloon tree.
        /// </summary><remarks>
        /// Be aware that if the position of the root node of the entire 
        /// balloon tree is fixed,
        /// this value is irrelevant (or basically equivalent to 
        /// <see cref="CircleCentering.Predefined"/>, even if it isn't
        /// set to that value).</remarks>
        public CircleCentering RootCentering
        {
            get { return this.mBalloonLayouter.RootCentering; }
            set { this.mBalloonLayouter.RootCentering = value; }
        }
        /// <summary>
        /// Gets or sets the minimum length of an edge connecting a
        /// subtree/leaf node to its root node.
        /// </summary>
        public double MinimumEdgeLength
        {
            get { return this.mBalloonLayouter.MinimumEdgeLength; }
            set { this.mBalloonLayouter.MinimumEdgeLength = value; }
        }
        /// <summary>
        /// Gets or sets whether the lengths of distances between a set of
        /// subtree/leaf nodes and their shared root node are equalized or
        /// minimized.</summary>
        public bool EqualizeBranchLengths
        {
            get { return this.mBalloonLayouter.EqualizeBranchLengths; }
            set { this.mBalloonLayouter.EqualizeBranchLengths = value; }
        }
        /// <summary><para>
        /// Gets or sets the maximum allowable wedge in radians that the set
        /// of subtree/leaf nodes are allowed to occupy around the root node
        /// of the entire balloon tree.</para><para>
        /// Be aware that this angle will be normalized to the range [0,2π] 
        /// ( [0°,360°] ) when used.</para></summary><remarks>
        /// If the sum the bounding wedges of the set of subtree/leaf nodes
        /// exceeds this wedge, their <see cref="P:CircleTree`2.Distance"/>s
        /// are increased until all their bounding wedges can fit within it.
        /// Then their <see cref="P:CircleTree`2.Angle"/>s are set in the
        /// range <c>(-wedge / 2, wedge / 2)</c>, measured counterclockwise
        /// from the +X-axis of their shared root's local coordinate system.
        /// </remarks><seealso cref="MaximumTreeWedge"/>
        public double MaximumRootWedge
        {
            get { return this.mBalloonLayouter.MaximumRootWedge; }
            set { this.mBalloonLayouter.MaximumRootWedge = value; }
        }
        /// <summary><para>
        /// Gets or sets the maximum allowable wedge in radians that a set
        /// of subtree/leaf nodes are allowed to occupy around their 
        /// shared root node.</para><para>
        /// Be aware that this angle will be normalized to the range [0,2π] 
        /// ( [0°,360°] ) when used.</para></summary><remarks>
        /// If the sum the bounding wedges of the set of subtree/leaf nodes
        /// exceeds this wedge, their <see cref="P:CircleTree`2.Distance"/>s
        /// are increased until all their bounding wedges can fit within it.
        /// Then their <see cref="P:CircleTree`2.Angle"/>s are set in the
        /// range <c>(-wedge / 2, wedge / 2)</c>, measured counterclockwise
        /// from the +X-axis of their shared root's local coordinate system.
        /// </remarks><seealso cref="MaximumRootWedge"/>
        public double MaximumTreeWedge
        {
            get { return this.mBalloonLayouter.MaximumTreeWedge; }
            set { this.mBalloonLayouter.MaximumTreeWedge = value; }
        }

        public double MaximumDeviationAngle
        {
            get { return this.mBalloonLayouter.MaximumDeviationAngle; }
            set { this.mBalloonLayouter.MaximumDeviationAngle = value; }
        }
        #endregion

        #region Physical Animation Parameters
        /// <summary>
        /// Gets or sets whether the roots of any leaf/subtree nodes with
        /// fixed positions are first repositioned to attempt to set the
        /// local polar coordinate positions of their fixed branches equal
        /// to those calculated for the balloon tree.</summary>
        public bool AdjustRootCenters
        {
            get { return this.bAdjustRoots; }
            set
            {
                if (this.bAdjustRoots != value)
                {
                    this.bAdjustRoots = value;
                }
            }
        }

        public bool AdjustRootAngle
        {
            get { return this.bAdjustAngle; }
            set
            {
                if (this.bAdjustAngle != value)
                {
                    this.bAdjustAngle = value;
                }
            }
        }

        public double SpringMultiplier
        {
            get { return this.mSpringMult; }
            set
            {
                if (this.mSpringMult != value)
                {
                    this.mSpringMult = value;
                }
            }
        }

        public double MagneticMultiplier
        {
            get { return this.mMagnetMult; }
            set
            {
                if (this.mMagnetMult != value)
                {
                    this.mMagnetMult = value;
                }
            }
        }

        public double MagneticExponent
        {
            get { return this.mMagnetExp; }
            set
            {
                if (this.mMagnetExp != value)
                {
                    this.mMagnetExp = value;
                }
            }
        }
        /// <summary>
        /// Gets or sets the angle which the entire balloon tree is rotated
        /// around the center of its root node, measured in radians
        /// counterclockwise from the +X-axis.
        /// </summary><remarks>
        /// Since this angle is measured clockwise from the +X-axis on a
        /// standard drawing surface (because the Y-axis is negated),
        /// the final graph will appear as if it has been rotated by the
        /// negation of this angle on a conventional 2D Euclidean manifold. 
        /// </remarks>
        public double RootAngle
        {
            get { return this.mRootAngle; }
            set
            {
                if (this.mRootAngle != value)
                {
                    this.mRootAngle = value;
                }
            }
        }
        #endregion

        #region Angle Parameters in Degrees
        /// <summary>
        /// Gets or sets the <see cref="MaximumRootWedge"/> parameter of this
        /// layout algorithm in degrees instead of radians for debugging.
        /// </summary>
        public double DegMaxRootWedge
        {
            get { return this.mBalloonLayouter.DegMaxRootWedge; }
            set { this.mBalloonLayouter.DegMaxRootWedge = value; }
        }
        /// <summary>
        /// Gets or sets the <see cref="MaximumTreeWedge"/> parameter of this
        /// layout algorithm in degrees instead of radians for debugging.
        /// </summary>
        public double DegMaxTreeWedge
        {
            get { return this.mBalloonLayouter.DegMaxTreeWedge; }
            set { this.mBalloonLayouter.DegMaxTreeWedge = value; }
        }
        /// <summary>
        /// Gets or sets the <see cref="MaximumDeviationAngle"/> parameter
        /// of this layout algorithm in degrees instead of radians for 
        /// debugging.</summary>
        public double DegMaxDeviationAngle
        {
            get { return this.mBalloonLayouter.DegMaxDeviationAngle; }
            set { this.mBalloonLayouter.DegMaxDeviationAngle = value; }
        }
        /// <summary>
        /// Gets or sets the <see cref="RootAngle"/> parameter of this
        /// layout algorithm in degrees instead of radians for debugging.
        /// </summary>
        public double DegRootAngle
        {
            get { return 180.0 * this.mRootAngle / Math.PI; }
            set
            {
                value = Math.PI * value / 180.0;
                if (this.mRootAngle != value)
                {
                    this.mRootAngle = value;
                }
            }
        }
        #endregion

        #endregion

        public CircleTree<PrintArray<Node>, int> DebugTree
        {
            get
            {
                if (this.mBalloonLayouter == null)
                    return null;
                return this.mBalloonLayouter.DebugTree;
            }
        }

        public Vec2F DebugRootPosition
        {
            get
            {
                if (this.mBalloonLayouter == null)
                    return null;
                return this.mBalloonLayouter.DebugRootPosition;
            }
        }

        /*protected override void InitializeAlgorithm()
        {
            this.bItemMoved = true;
        }

        protected override bool CanIterate()
        {
            return this.bItemMoved;
        }/* */

        // Ideal order for OnBeginIteration:
        // this.InitBalloonGraph()
        //
        // each circle.Center = centroid of its nodes
        // each circle.CalculateEmbeddingCircle()
        // each circle.InitCircle() (calc BoundingRadius and Angles)
        // each circle.CalcCenter = circle.Center
        // 
        // this.mLayouter.GenerateSpanningTree()
        // this.mLayouter.BuildBalloonTree() (use CalcCenter in SketchMode)
        // this.mLayouter.ResetCircleTreeRadii() (use BoundingRadius)
        // this.mLayouter.CalculatePositions() (use Angles; calc CircleAngle)
        // this.mLayouter calculate center
        // this.mLayouter.CalcCircleAngles()

        protected override void PerformPrecalculations(
            uint lastNodeVersion, uint lastEdgeVersion)
        {
            bool graphDirty =
                this.mGraph.NodeVersion != lastNodeVersion ||
                this.mGraph.EdgeVersion != lastEdgeVersion;
            if (graphDirty || this.bLayouterDirty)
            {
                this.InitBalloonGraph();
                this.bLayouterDirty = true;
                this.bNodeSequencerDirty = true;
                this.bCirclePositionsDirty = true;
            }
            if (this.bNodeSequencerDirty && this.mNodeSequencer != null)
            {
                this.mNodeSequencer.SetDirty(true, this.mGraph);
                this.bCirclePositionsDirty = true;
            }
            if (this.bCirclePositionsDirty)
            {
                GNode bNode;
                int i, j, gid;
                // Hide all nodes in the graph
                this.mGraph.HideAllNodes();
                Digraph<GNode, GEdge> bGraph = this.mBalloonLayouter.Graph;
                for (i = bGraph.NodeCount - 1; i >= 0; i--)
                {
                    bNode = bGraph.NodeAt(i);
                    if (bNode.Data == null)
                    {
                        gid = bNode.CircleID;
                        // Unhide nodes in the current embedding circle
                        for (j = this.mGroupIds.Length - 1; j >= 0; j--)
                        {
                            if (this.mGroupIds[j] == gid)
                            {
                                this.mGraph.UnhideNodeAt(j);
                            }
                        }
                        // Causes mCircleLayouter to perform its precalculations,
                        // including calculating the bounding radius and angles,
                        // as well as resequencing the nodes if necessary.
                        this.mCircleLayouter.ForceNodeSequencing();
                        bNode.CircleRadius = this.mCircleLayouter.BoundingRadius;
                        bNode.ECRadius = this.mCircleLayouter.Radius;
                        if (this.bLayouterDirty)
                        {
                            bNode.SetPosition(
                                (float)this.mCircleLayouter.CentroidX,
                                (float)this.mCircleLayouter.CentroidY);
                        }
                        // Rehide nodes in the current embedding circle
                        for (j = this.mGroupIds.Length - 1; j >= 0; j--)
                        {
                            if (this.mGroupIds[j] == gid)
                            {
                                this.mGraph.HideNodeAt(j);
                            }
                        }
                    }
                }
                // Unhide all nodes that weren't originally hidden
                for (i = this.mGroupIds.Length - 1; i >= 0; i--)
                {
                    if (this.mGroupIds[i] != -1)
                        this.mGraph.UnhideNodeAt(i);
                }
            }
            if (this.bNodeSequencerDirty && this.mNodeSequencer != null)
            {
                this.mNodeSequencer.SetDirty(false, this.mGraph);
            }
            this.bLayouterDirty = false;
            this.bNodeSequencerDirty = false;
            this.bCirclePositionsDirty = false;
        }

        private CircleTree<GNode, GEdge>[] mStackQueue
            = new CircleTree<GNode, GEdge>[0];

        protected override void PerformIteration(uint iteration)
        {
            this.PerformPrecalculations();

            bool rev;
            int i, j, k, sqIndex, sqCount;
            Node node;
            GNode bNode;
            GEdge bEdge;
            CircleTree<GNode, GEdge> ct, root;
            CircleTree<GNode, GEdge>[] branches;
            double a, dist, ang, cx, cy, dx, dy, r, force, fx, fy;

            Digraph<GNode, GEdge> bGraph = this.mBalloonLayouter.Graph;
            CircleTree<GNode, GEdge> bTree 
                = this.mBalloonLayouter.GetBalloonTree();

            // Initially set new positions to current positions, since
            // some of them might be adjusted instead of directly set
            for (i = this.mGroupNodes.Length - 1; i >= 0; i--)
            {
                if (this.mGroupNodes[i] != null)
                    this.mGroupNodes[i].SetPosFixed(false);
            }
            for (i = this.mGraph.NodeCount - 1; i >= 0; i--)
            {
                node = this.mGraph.NodeAt(i);
                //node.SetNewPosition(node.X, node.Y);
                j = this.mGroupIds[i];
                if (j != -1 && node.PositionFixed)
                {
                    this.mGroupNodes[j].SetPosFixed(true);
                }
            }

            bNode = bTree.NodeData;
            if (bNode.Data == null || !bNode.Data.PositionFixed)
            {
                // Pull the root of the entire balloon tree
                // towards its calculated center.
                Box2F bbox = null;
                double cw, ch;
                cx = cy = cw = ch = 0.0;
                switch (this.mBalloonLayouter.RootCentering)
                {
                    case CircleCentering.BBoxCenter:
                        bbox = this.mClusterNode == null
                            ? this.BoundingBox
                            : this.mClusterNode.LayoutBBox;
                        if (bbox != null)
                        {
                            cx = bbox.X;
                            cy = bbox.Y;
                            cw = bbox.W;
                            ch = bbox.H;
                        }
                        break;
                    case CircleCentering.Centroid:
                        bbox = this.mClusterNode == null
                            ? this.BoundingBox
                            : this.mClusterNode.LayoutBBox;
                        if (bbox != null)
                        {
                            // TODO: What if centroid is outside the bbox?
                            cx = this.mBalloonLayouter.CentroidX;
                            cy = this.mBalloonLayouter.CentroidY;
                            dx = Math.Min(Math.Abs(cx - bbox.X),
                                          Math.Abs(bbox.Right - cx));
                            dy = Math.Min(Math.Abs(cy - bbox.Y),
                                          Math.Abs(bbox.Bottom - cy));
                            cx = cx - dx;
                            cy = cy - dy;
                            cw = 2.0 * dx;
                            ch = 2.0 * dy;
                        }
                        break;
                    // No need to calculate for Predefined, as the center 
                    // is the current position of the root node
                }
                if (bbox != null)
                {
                    ang = Math.Sqrt(cw * cw + ch * ch) / 2;
                    fx = fy = 0.0;
                    // Spring Force from Top Left Corner
                    dx = cx - bNode.X;
                    dy = cy - bNode.Y;
                    r = Math.Sqrt(dx * dx + dy * dy);
                    if (r > 0.0)
                    {
                        force = this.mSpringMult * Math.Log(r / ang);
                        fx += force * dx / r;
                        fy += force * dy / r;
                    }
                    // Spring Force from Top Right Corner
                    dx += cw;
                    r = Math.Sqrt(dx * dx + dy * dy);
                    if (r > 0.0)
                    {
                        force = this.mSpringMult * Math.Log(r / ang);
                        fx += force * dx / r;
                        fy += force * dy / r;
                    }
                    // Spring Force from Bottom Right Corner
                    dy += ch;
                    r = Math.Sqrt(dx * dx + dy * dy);
                    if (r > 0.0)
                    {
                        force = this.mSpringMult * Math.Log(r / ang);
                        fx += force * dx / r;
                        fy += force * dy / r;
                    }
                    // Spring Force from Bottom Left Corner
                    dx -= cw;
                    r = Math.Sqrt(dx * dx + dy * dy);
                    if (r > 0.0)
                    {
                        force = this.mSpringMult * Math.Log(r / ang);
                        fx += force * dx / r;
                        fy += force * dy / r;
                    }
                    // Apply force to root position
                    bNode.SetPosition(bNode.X + (float)fx,
                                      bNode.Y + (float)fy);
                }
            }
            if (this.mStackQueue.Length < bGraph.NodeCount)
            {
                this.mStackQueue
                    = new CircleTree<GNode, GEdge>[bGraph.NodeCount];
            }
            if (this.bAdjustRoots)
            {
                // Pull roots towards their fixed branches
                sqCount = 1;
                this.mStackQueue[0] = bTree;
                while (sqCount > 0)//this.mStack.Count > 0)
                {
                    ct = this.mStackQueue[--sqCount];//this.mStack.Pop();
                    if (ct.NodeData.PositionFixed)
                    {
                        root = ct.Root;
                        bNode = ct.NodeData;
                        if (root == null && bNode.Data == null)
                        {
                            // Calculate the force on the circle's center,
                            // which in this case is the center of the 
                            // entire balloon tree.
                            cx = bNode.X;
                            cy = bNode.Y;
                            for (i = this.mGroupIds.Length - 1; i >= 0; i--)
                            {
                                if (this.mGroupIds[i] == bNode.CircleID)
                                {
                                    node = this.mGraph.NodeAt(i);
                                    if (node.PositionFixed)
                                    {
                                        dx = cx - node.X;
                                        dy = cy - node.Y;
                                        if (dx == 0 && dy == 0)
                                        {
                                            fx = fy = bNode.ECRadius / 10;
                                        }
                                        else
                                        {
                                            r = dx * dx + dy * dy;
                                            if (this.bAdjustAngle)
                                            {
                                                ang = Math.Atan2(-dy, -dx)
                                                    - this.mCircleLayouter.AngleAt(i)
                                                    - bNode.CircleAngle;
                                                while (ang < -Math.PI)
                                                    ang += 2 * Math.PI;
                                                while (ang > Math.PI)
                                                    ang -= 2 * Math.PI;
                                                this.mRootAngle = ang;
                                                fx = fy = 0;
                                            }
                                            else
                                            {
                                                // Magnetic Torque
                                                force = this.mCircleLayouter.AngleAt(i)
                                                      + bNode.CircleAngle
                                                      + this.mRootAngle;
                                                // dx and dy have to be negated here in
                                                // order to get the same results as the 
                                                // single step method that uses Cos/Sin
                                                force = Math.Atan2(-dy, -dx) - force;
                                                while (force < -Math.PI)
                                                    force += 2 * Math.PI;
                                                while (force > Math.PI)
                                                    force -= 2 * Math.PI;
                                                force = this.mMagnetMult *
                                                    Math.Pow(force, this.mMagnetExp) / r;
                                                fx = force * -dy;
                                                fy = force * dx;
                                            }
                                            // Spring Force
                                            r = Math.Sqrt(r);
                                            force = this.mSpringMult *
                                                Math.Log(r / bNode.ECRadius);
                                            fx += force * dx / r;
                                            fy += force * dy / r;
                                        }
                                        // Apply the force to the circle's center
                                        cx -= fx;
                                        cy -= fy;
                                    }
                                }
                            }
                            bNode.SetPosition((float)cx, (float)cy);
                        }
                        while (root != null)
                        {
                            if (bNode.Data == null)
                            {
                                // Calculate the force on the circle's center
                                cx = bNode.X;
                                cy = bNode.Y;
                                ang = Math.Atan2(cy - root.NodeData.Y,
                                                 cx - root.NodeData.X)
                                    + bNode.CircleAngle;
                                for (i = this.mGroupIds.Length - 1; i >= 0; i--)
                                {
                                    if (this.mGroupIds[i] == bNode.CircleID)
                                    {
                                        node = this.mGraph.NodeAt(i);
                                        if (node.PositionFixed)
                                        {
                                            dx = cx - node.X;
                                            dy = cy - node.Y;
                                            if (dx == 0 && dy == 0)
                                            {
                                                fx = fy = bNode.ECRadius / 10;
                                            }
                                            else
                                            {
                                                r = dx * dx + dy * dy;
                                                // Magnetic Torque
                                                force = this.mCircleLayouter.AngleAt(i)
                                                      + ang;
                                                // dx and dy have to be negated here in
                                                // order to get the same results as the 
                                                // single step method that uses Cos/Sin
                                                force = Math.Atan2(-dy, -dx) - force;
                                                while (force < -Math.PI)
                                                    force += 2 * Math.PI;
                                                while (force > Math.PI)
                                                    force -= 2 * Math.PI;
                                                force = this.mMagnetMult *
                                                    Math.Pow(force, this.mMagnetExp) / r;
                                                fx = force * -dy;
                                                fy = force * dx;
                                                // Spring Force
                                                r = Math.Sqrt(r);
                                                force = this.mSpringMult *
                                                    Math.Log(r / bNode.ECRadius);
                                                fx += force * dx / r;
                                                fy += force * dy / r;
                                            }
                                            // Apply the force to the circle's center
                                            cx -= fx;
                                            cy -= fy;
                                        }
                                    }
                                }
                                bNode.SetPosition((float)cx, (float)cy);
                            }
                            // Calculate force on root
                            cx = root.NodeData.X;
                            cy = root.NodeData.Y;
                            if (root.Root == null)
                            {
                                ang = this.mRootAngle;
                            }
                            else
                            {
                                bNode = root.Root.NodeData;
                                ang = Math.Atan2(cy - bNode.Y,
                                                 cx - bNode.X);
                                bNode = ct.NodeData;
                            }
                            // TODO: make sure all the signs (+/-) are right
                            dx = cx - bNode.X;
                            dy = cy - bNode.Y;
                            if (dx == 0 && dy == 0)
                            {
                                fx = fy = ct.Distance / 10;
                            }
                            else
                            {
                                if (this.mBalloonLayouter.AdjustRootAngle && 
                                    root.Root == null)
                                {
                                    double rAng 
                                        = Math.Atan2(-dy, -dx) - ct.Angle;
                                    while (rAng < -Math.PI)
                                        rAng += 2 * Math.PI;
                                    while (rAng > Math.PI)
                                        rAng -= 2 * Math.PI;
                                    this.mBalloonLayouter.RootAngle = rAng;
                                    fx = fy = 0;
                                    r = Math.Sqrt(dx * dx + dy * dy);
                                }
                                else
                                {
                                    r = dx * dx + dy * dy;
                                    // Magnetic Torque
                                    force = ct.Angle + ang;
                                    // dx and dy have to be negated here in
                                    // order to get the same results as the 
                                    // single step method that uses Cos/Sin
                                    force = Math.Atan2(-dy, -dx) - force;
                                    while (force < -Math.PI)
                                        force += 2 * Math.PI;
                                    while (force > Math.PI)
                                        force -= 2 * Math.PI;
                                    force = this.mMagnetMult *
                                        Math.Pow(force, this.mMagnetExp) / r;
                                    fx = force * -dy;
                                    fy = force * dx;
                                    r = Math.Sqrt(r);
                                }
                                // Spring Force
                                force = this.mSpringMult *
                                    Math.Log(r / ct.Distance);
                                fx += force * dx / r;
                                fy += force * dy / r;
                            }
                            // Apply force to root position
                            bNode = root.NodeData;
                            bNode.SetPosition(bNode.X - (float)fx,
                                              bNode.Y - (float)fy);
                            // Progress up the ancestry chain
                            ct = root;
                            root = ct.Root;
                            bNode = ct.NodeData;
                        }
                    }
                    else if (ct.BranchCount > 0)
                    {
                        branches = ct.Branches;
                        for (i = branches.Length - 1; i >= 0; i--)
                        {
                            //this.mStack.Push(branches[i]);
                            this.mStackQueue[sqCount++] = branches[i];
                        }
                    }
                }
            }
            // Pull movable branches towards their roots
            sqIndex = 0;
            sqCount = 1;
            this.mStackQueue[0] = bTree;
            while (sqIndex < sqCount)//this.mQueue.Count > 0)
            {
                ct = this.mStackQueue[sqIndex++];//this.mQueue.Dequeue();
                bNode = ct.NodeData;
                cx = bNode.X;
                cy = bNode.Y;
                if (ct.Root == null)
                {
                    ang = this.mBalloonLayouter.RootAngle;
                }
                else
                {
                    bNode = ct.Root.NodeData;
                    ang = Math.Atan2(cy - bNode.Y, cx - bNode.X);
                    bNode = ct.NodeData;
                }
                if (bNode.Data == null)
                {
                    // Pull movable nodes towards the circle center
                    ang += bNode.CircleAngle;
                    for (i = this.mGroupIds.Length - 1; i >= 0; i--)
                    {
                        if (this.mGroupIds[i] == bNode.CircleID)
                        {
                            node = this.mGraph.NodeAt(i);
                            if (!node.PositionFixed)
                            {
                                fx = node.X;
                                fy = node.Y;
                                dx = cx - fx;
                                dy = cy - fy;
                                if (dx == 0 && dy == 0)
                                {
                                    fx += bNode.ECRadius / 10;
                                    fy += bNode.ECRadius / 10;
                                }
                                else
                                {
                                    r = dx * dx + dy * dy;
                                    // Magnetic Torque
                                    force = this.mCircleLayouter.AngleAt(i) + ang;
                                    // dx and dy have to be negated here in
                                    // order to get the same results as the 
                                    // single step method that uses Cos/Sin
                                    force = Math.Atan2(-dy, -dx) - force;
                                    while (force < -Math.PI)
                                        force += 2 * Math.PI;
                                    while (force > Math.PI)
                                        force -= 2 * Math.PI;
                                    force = this.mMagnetMult *
                                        Math.Pow(force, this.mMagnetExp) / r;
                                    fx += force * -dy;
                                    fy += force * dx;
                                    // Spring Force
                                    r = Math.Sqrt(r);
                                    force = this.mSpringMult *
                                        Math.Log(r / bNode.ECRadius);
                                    fx += force * dx / r;
                                    fy += force * dy / r;
                                }
                                // Apply the force to the node's position
                                node.SetPosition((float)fx, (float)fy);
                            }
                        }
                    }
                    ang -= bNode.CircleAngle;
                }
                root = ct;
                branches = ct.Branches;
                // Only Many -> One cases use a different pulling process.
                // Empty Edges, One -> Many, and Many -> Many get pulled
                // the same way as One -> One because the first has nothing 
                // to calculate from and the rest would just take longer 
                // to give roughly the same result in the end.
                for (i = 0; i < branches.Length; i++)
                {
                    ct = branches[i];
                    bNode = ct.NodeData;
                    bEdge = ct.EdgeData;
                    if (!bNode.PositionFixed && bEdge != null &&
                        bNode.Data != null && root.NodeData.Data == null)
                    {
                        fx = bNode.X;
                        fy = bNode.Y;
                        dx = cx - fx;
                        dy = cy - fy;
                        if (dx == 0 && dy == 0)
                        {
                            force = ct.Distance / 10;
                            fx += force;
                            fy += force;
                        }
                        else
                        {
                            bNode = root.NodeData;
                            rev = bNode.CircleID == bEdge.SrcNode.CircleID;
                            for (j = 0; j < bEdge.ECount; j++)
                            {
                                k = rev ? bEdge.SrcIndexes[j] 
                                        : bEdge.DstIndexes[j];
                                node = this.mGraph.NodeAt(k);
                                cx = node.X;
                                cy = node.Y;
                                // Goal angle of node around root center -
                                // Goal angle of branch around root center
                                a = bNode.CircleAngle 
                                  + this.mCircleLayouter.AngleAt(k) - ct.Angle;
                                while (a < -Math.PI)
                                    a += 2 * Math.PI;
                                while (a > Math.PI)
                                    a -= 2 * Math.PI;
                                dx = bNode.ECRadius;
                                dy = ct.Distance;
                                // Squared distance between goal position
                                // of node and goal position of branch
                                dist = dx * dx + dy * dy 
                                    - 2 * dx * dy * Math.Cos(a);
                                // Angle between root center to
                                // goal position of node to
                                // goal position of branch
                                dx = (dist + dx * dx - dy * dy) / 
                                     (2 * Math.Sqrt(dist) * dx);
                                dx = Math.Acos(//fx);
                                    dx > 1 ? 1 : (dx < -1 ? -1 : dx));
                                dist = Math.Sqrt(dist);

                                a = a < 0.0 ? Math.PI - dx : dx - Math.PI;

                                dx = cx - fx;
                                dy = cy - fy;
                                if (dx == 0 && dy == 0)
                                {
                                    fx += dist / 10;
                                    fy += dist / 10;
                                }
                                else
                                {
                                    r = dx * dx + dy * dy;
                                    // Magnetic Torque
                                    force = a + Math.Atan2(cy - bNode.Y,
                                                           cx - bNode.X);
                                    // dx and dy have to be negated here in
                                    // order to get the same results as the 
                                    // single step method that uses Cos/Sin
                                    force = Math.Atan2(-dy, -dx) - force;
                                    while (force < -Math.PI)
                                        force += 2 * Math.PI;
                                    while (force > Math.PI)
                                        force -= 2 * Math.PI;
                                    force = this.mMagnetMult *
                                        Math.Pow(force, this.mMagnetExp) / r;
                                    fx += force * -dy;
                                    fy += force * dx;
                                    // Spring Force
                                    r = Math.Sqrt(r);
                                    force = this.mSpringMult *
                                        Math.Log(r / ct.Distance);
                                    fx += force * dx / r;
                                    fy += force * dy / r;
                                }
                            }
                            cx = bNode.X;
                            cy = bNode.Y;
                            bNode = ct.NodeData;
                        }
                        bNode.SetPosition((float)fx, (float)fy);
                    }
                    else if (!bNode.PositionFixed || bNode.Data == null)
                    {
                        fx = bNode.X;
                        fy = bNode.Y;
                        // TODO: make sure all the signs (+/-) are right
                        dx = cx - fx;
                        dy = cy - fy;
                        if (dx == 0 && dy == 0)
                        {
                            force = ct.Distance / 10;
                            fx += force;
                            fy += force;
                        }
                        else
                        {
                            r = dx * dx + dy * dy;
                            // Magnetic Torque
                            force = ct.Angle + ang;
                            // dx and dy have to be negated here in
                            // order to get the same results as the 
                            // single step method that uses Cos/Sin
                            force = Math.Atan2(-dy, -dx) - force;
                            while (force < -Math.PI)
                                force += 2 * Math.PI;
                            while (force > Math.PI)
                                force -= 2 * Math.PI;
                            force = this.mMagnetMult *
                                Math.Pow(force, this.mMagnetExp) / r;
                            fx += force * -dy;
                            fy += force * dx;
                            // Spring Force
                            r = Math.Sqrt(r);
                            force = this.mSpringMult *
                                Math.Log(r / ct.Distance);
                            fx += force * dx / r;
                            fy += force * dy / r;
                        }
                        // Add force to position
                        bNode.SetPosition((float)fx, (float)fy);/* */

                        /*dx = cx + ct.Distance * Math.Cos(ct.Angle + ang);
                        dy = cy + ct.Distance * Math.Sin(ct.Angle + ang);
                        node.SetNewPosition((float)dx, (float)dy);/* */
                    }
                    //this.mQueue.Enqueue(ct);
                    this.mStackQueue[sqCount++] = ct;
                }
            }
        }

        /*protected override void OnBeginIteration(uint iteration, bool dirty, 
            int lastNodeCount, int lastEdgeCount)
        {
            bool graphDirty =
                this.mGraph.NodeCount != lastNodeCount ||
                this.mGraph.EdgeCount != lastEdgeCount;
            if (graphDirty || this.bLayouterDirty)
            {
                this.InitBalloonGraph();
            }
            int i;
            CircleLayouter circle;
            Digraph<GNode, GEdge>.GNode[] nodes
                    = this.mBalloonLayouter.Graph.InternalNodes;
            if (this.bCircleDirty && !graphDirty && !this.bLayouterDirty)
            {
                // This is also done in the BalloonCircle
                // constructor in InitBalloonGraph()
                for (i = 0; i < nodes.Length; i++)
                {
                    circle = nodes[i].Data.Circle;
                    if (circle != null)
                    {
                        circle.NodeSpacing = this.mNodeSpacing;
                        circle.MinRadius = this.mMinRadius;
                        circle.FreeArc = this.mFreeArc;
                        circle.CallBeginIteration(iteration);
                    }
                }
            }
            else
            {
                for (i = 0; i < nodes.Length; i++)
                {
                    circle = nodes[i].Data.Circle;
                    if (circle != null)
                    {
                        circle.CallBeginIteration(iteration);
                    }
                }
            }
            if (this.bCircleDirty)
            {
                // Radii are also set in this.mLayouter.BuildBalloonTree()
                this.mBalloonLayouter.CalcCircleRadii();
            }

            this.mBalloonLayouter.CallBeginIteration(iteration);

            if (this.bCircleDirty || graphDirty)
            {
                //this.mBalloonLayouter.SetCircleAngles();
            }

            this.bCircleDirty = false;
            this.bLayouterDirty = false;
        }

        private Stack<CircleTree<GNode, GEdge>> mStack;
        private Queue<CircleTree<GNode, GEdge>> mQueue;

        protected override void PerformIteration(uint iteration)
        {
            int i;
            GNode node;
            double ang, cx, cy;
            CircleLayouter circle;
            CircleTree<GNode, GEdge> ct;
            CircleTree<GNode, GEdge>[] branches;
            Digraph<GNode, GEdge>.GNode[] nodes
                = this.mBalloonLayouter.Graph.InternalNodes;
            // Figure out whether the position of each balloon circle is
            // "fixed" (whether it contains a fixed node) at the start, 
            // so that the calculation doesn't have to be run each time 
            // the PositionFixed property of balloon circle is retrieved.
            for (i = 0; i < nodes.Length; i++)
            {
                node = nodes[i].Data;
                node.CalcPositionFixed();
            }

            // Pull the centers of balloon circles containing  
            // fixed nodes towards those fixed nodes.
            if (this.mStack == null)
            {
                this.mStack 
                    = new Stack<CircleTree<GNode, GEdge>>(nodes.Length);
            }
            ct = this.mBalloonLayouter.GetBalloonTree();
            ct.NodeData.StackAngle = this.mBalloonLayouter.RootAngle;
            this.mStack.Push(ct);
            while (this.mStack.Count > 0)
            {
                ct = this.mStack.Pop();
                node = ct.NodeData;
                ang = node.StackAngle + ct.Angle;
                if (node.PositionFixed)
                {
                    circle = node.Circle;
                    if (circle != null)
                    {
                        // "Fixed" circles need to calculate their
                        // centers before the balloon tree operates
                        // on them because it uses their centers to
                        // adjust the positions of their tree roots.
                        circle.Angle = ang + node.CircleAngle;
                        circle.AdjustAngle = this.bAdjustAngles;
                        circle.SpringMultiplier = this.mSpringMult;
                        circle.MagneticMultiplier = this.mMagnetMult;
                        circle.MagneticExponent = this.mMagnetExp;
                        circle.CallPerformIteration(iteration);
                        // If the root of the entire balloon tree
                        // is a "fixed" circle, then the balloon 
                        // tree's root angle can be adjusted by it,
                        // but only after it has already been calculated
                        // by the "fixed" root circle's layout algorithm.
                        if (this.bAdjustAngles && ct.Root == null &&
                            this.mBalloonLayouter.AdjustRootAngle)
                        {
                            this.mBalloonLayouter.RootAngle
                                = circle.Angle - node.CircleAngle;
                        }
                    }
                }
                if (ct.BranchCount > 0)
                {
                    branches = ct.Branches;
                    for (i = branches.Length - 1; i >= 0; i--)
                    {
                        ct = branches[i];
                        ct.NodeData.StackAngle = ang;
                        this.mStack.Push(ct);
                    }
                }
            }

            // Run the balloon tree layout algorithm to adjust the centers
            // of the movable singular nodes and balloon circles
            this.mBalloonLayouter.CallPerformIteration(iteration);

            // Readjust the root circle just in case
            // one of its branches was also fixed.
            ct = this.mBalloonLayouter.GetBalloonTree();
            if (ct.NodeData.PositionFixed && 
                this.mBalloonLayouter.AdjustRootAngle)
            {
                circle = ct.NodeData.Circle;
                if (circle != null)
                {
                    circle.Angle = this.mBalloonLayouter.RootAngle
                        + ct.NodeData.CircleAngle;
                    circle.CallPerformIteration(iteration);
                }
            }

            // Pull the nodes in balloon circles towards their new centers
            // that were calculated by the balloon tree layout algorithm.
            if (this.mQueue == null)
            {
                this.mQueue
                    = new Queue<CircleTree<GNode, GEdge>>(nodes.Length);
            }
            this.mQueue.Enqueue(ct);
            while (this.mQueue.Count > 0)
            {
                ct = this.mQueue.Dequeue();
                cx = ct.NodeData.NewX;
                cy = ct.NodeData.NewY;
                if (ct.Root == null)
                {
                    ang = this.mBalloonLayouter.RootAngle;
                }
                else
                {
                    node = ct.Root.NodeData;
                    ang = Math.Atan2(cy - node.NewY, cx - node.NewX);
                }
                node = ct.NodeData;
                if (!node.PositionFixed)
                {
                    circle = node.Circle;
                    if (circle != null)
                    {
                        circle.CalcCenterX = node.NewX;
                        circle.CalcCenterY = node.NewY;
                        circle.Angle = ang + node.CircleAngle;
                        circle.AdjustAngle = this.bAdjustAngles;
                        circle.SpringMultiplier = this.mSpringMult;
                        circle.MagneticMultiplier = this.mMagnetMult;
                        circle.MagneticExponent = this.mMagnetExp;
                        circle.CallPerformIteration(iteration);
                    }
                }
                if (ct.BranchCount > 0)
                {
                    branches = ct.Branches;
                    for (i = 0; i < branches.Length; i++)
                    {
                        this.mQueue.Enqueue(branches[i]);
                    }
                }
            }
        }

        protected override void EndIteration(uint iteration)
        {
            this.bItemMoved = false;
            CircleLayouter circle;
            Digraph<GNode, GEdge>.GNode[] nodes
                = this.mBalloonLayouter.Graph.InternalNodes;
            for (int i = 0; i < nodes.Length; i++)
            {
                circle = nodes[i].Data.Circle;
                if (circle != null)
                {
                    circle.CallEndIteration(iteration);
                    this.bItemMoved = this.bItemMoved || circle.ItemMoved;
                }
            }
            this.mBalloonLayouter.CallEndIteration(iteration);
            this.bItemMoved = this.bItemMoved || this.mBalloonLayouter.ItemMoved;
        }/* */

        private void InitBalloonGraph()
        {
            int i, count = this.mGraph.NodeCount;

            BCCAlgorithm<Node, Edge> bccAlg
                = new BCCAlgorithm<Node, Edge>(this.mGraph, false);
            bccAlg.Compute();

            Node[][] bccGroups;
            int bccGroupCount;
            if (this.mGroupingMethod == LayoutStyle.BccCompact)
            {
                bccAlg.ArticulateToLargerCompactGroups();
                bccGroups = bccAlg.CompactGroups;
                bccGroupCount = bccAlg.CompactGroupCount;
                this.mGroupIds = bccAlg.CompactGroupIds;
            }
            else
            {
                bccGroups = bccAlg.IsolatedGroups;
                bccGroupCount = bccAlg.IsolatedGroupCount;
                this.mGroupIds = bccAlg.IsolatedGroupIds;
            }
            this.mGroupNodes = new GNode[bccGroupCount];
            for (i = 0; i < count; i++)
            {
                if (this.mGraph.InternalNodeAt(i).Hidden)
                    this.mGroupIds[i] = -1;
            }
            Digraph<GNode, GEdge> bccGraph = this.mBalloonLayouter.Graph;
            bccGraph.ClearNodes();

            GEdge[][] bccEdges = new GEdge[bccGroupCount][];
            GNode bccNode;
            //Digraph<Node, Edge> subGraph;
            Node[] bccGroup;
            for (i = 0; i < bccGroupCount; i++)
            {
                bccGroup = bccGroups[i];
                if (bccGroup.Length == 0)
                {
                    bccNode = null;
                }
                else if (bccGroup.Length == 1)
                {
                    bccNode = new GNode(i, 1, bccGroup[0]);
                }
                else
                {
                    /*subGraph = new Digraph<Node, Edge>(
                        bccGroup.Length, bccGroup.Length / 2);
                    for (j = 0; j < bccGroup.Length; j++)
                    {
                        subGraph.AddNode(bccGroup[j]);
                    }/* */
                    bccNode = new GNode(i, bccGroup.Length, null);
                }
                // TODO: Make sure excluding null nodes doesn't cause other
                // bad stuff to happen later on.
                if (bccNode != null)
                    bccGraph.AddNode(bccNode);
                this.mGroupNodes[i] = bccNode;
                bccEdges[i] = new GEdge[bccGroupCount];
            }

            int si, di;
            bool reversed;
            Digraph<Node, Edge>.GEdge edge;
            GEdge bccEdge;
            count = this.mGraph.EdgeCount;
            for (i = 0; i < count; i++)
            {
                edge = this.mGraph.InternalEdgeAt(i);
                si = this.mGroupIds[edge.SrcNode.Index];
                di = this.mGroupIds[edge.DstNode.Index];
                /*if (si == di && edge.SrcNode.Index != edge.DstNode.Index)
                {
                    this.mGroupNodes[si].Circle.Graph.AddEdge(edge.Data);
                }
                else/* */if (si != di)
                {
                    reversed = false;
                    bccEdge = bccEdges[si][di];
                    if (bccEdge == null)
                    {
                        // Check for a balloon edge in the reverse direction
                        reversed = true;
                        bccEdge = bccEdges[di][si];
                    }
                    if (bccEdge == null)
                    {
                        // Add a new balloon edge in the forward direction
                        bccEdge = new GEdge(this.mGroupNodes[si], 
                                            this.mGroupNodes[di]);
                        bccGraph.AddEdge(bccEdge);
                        reversed = false;
                        bccEdges[si][di] = bccEdge;
                    }
                    if (reversed)
                    {
                        bccEdge.AddEdge(edge.Data,
                            edge.DstNode.Index, edge.SrcNode.Index);
                    }
                    else
                    {
                        bccEdge.AddEdge(edge.Data,
                            edge.SrcNode.Index, edge.DstNode.Index);
                    }
                }
            }

            //this.SetBalloonRoot(this.TryGetGraphRoot());
            this.mBalloonLayouter.ClearRoots();
            if (this.RootCount > 0)
            {
                Digraph<Node, Edge>.GNode gNode;
                di = this.RootCount;
                for (i = 0; i < di; i++)
                {
                    gNode = this.RootAt(i);
                    si = this.mGroupIds[gNode.Index];
                    if (si != -1)
                    {
                        this.mBalloonLayouter.AddRoot(this.mGroupNodes[si]);
                    }
                }
            }
        }

        protected override void OnRootInserted(int index, 
            Digraph<Node, Edge>.GNode root)
        {
            int i = this.mGroupIds[root.Index];
            if (i != -1)
            {
                this.mBalloonLayouter.AddRoot(this.mGroupNodes[i], true);
            }
            base.OnRootInserted(index, root);
        }

        protected override void OnRootRemoved(Digraph<Node, Edge>.GNode root)
        {
            int i = this.mGroupIds[root.Index];
            if (i != -1)
            {
                this.mBalloonLayouter.RemoveRoot(this.mGroupNodes[i], true);
            }
            base.OnRootRemoved(root);
        }

        protected override void OnRootsCleared()
        {
            this.mBalloonLayouter.ClearRoots();
            base.OnRootsCleared();
        }

        /*protected override void OnRootChanged(Node oldRoot)
        {
            this.SetBalloonRoot(this.TryGetGraphRoot());
            base.OnRootChanged(oldRoot);
        }

        private void SetBalloonRoot(Digraph<Node, Edge>.GNode root)
        {
            if (root != null)
            {
                int i, j;
                bool flag = false;
                GNode node;
                Digraph<Node, Edge>.GNode[] cNodes;
                Digraph<GNode, GEdge>.GNode[] nodes
                    = this.mBalloonLayouter.Graph.InternalNodes;
                for (i = 0; i < nodes.Length && !flag; i++)
                {
                    node = nodes[i].Data;
                    if (node.Circle == null)
                    {
                        flag = root.Data.Equals(node.Data);
                    }
                    else
                    {
                        cNodes = node.Circle.Graph.InternalNodes;
                        for (j = 0; j < cNodes.Length && !flag; j++)
                        {
                            flag = root.Data.Equals(cNodes[j].Data);
                        }
                    }
                    if (flag)
                    {
                        this.mBalloonLayouter.SetRoot(node);
                    }
                }
            }
        }/* */
    }
}
