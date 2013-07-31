using System;
using System.Collections.Generic;
using System.Text;
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
        private class CircleLayouter
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
        }

        private abstract class GNode : ILayoutNode
        {
            public readonly int CircleID;
            public double CircleAngle;

            public double StackAngle;

            public GNode(int circleID)
            {
                this.CircleID = circleID;
                this.CircleAngle = 0.0;
            }

            public abstract Node Data { get; }

            public abstract CircleLayouter Circle { get; }

            public abstract Box2F LayoutBBox { get; }

            public abstract float X { get; }

            public abstract float Y { get; }

            public abstract void SetPosition(float x, float y);

            private bool bFixed;
            private float mNewX;
            private float mNewY;

            public void CalcPositionFixed()
            {
                if (this.Circle != null)
                {
                    Digraph<Node, Edge>.GNode[] nodes
                        = this.Circle.Graph.InternalNodes;
                    this.bFixed = false;
                    for (int i = 0; i < nodes.Length && !this.bFixed; i++)
                    {
                        this.bFixed = nodes[i].mData.PositionFixed;
                    }
                }
                else
                {
                    this.bFixed = this.Data.PositionFixed;
                }
            }

            public bool PositionFixed
            {
                get { return this.bFixed; }
            }

            public virtual float NewX
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
            }
        }

        private class BalloonCircle : GNode
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
                    node = nodes[i].mData;
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

            /*public override bool PositionFixed
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
            }/* */

            public override string ToString()
            {
                Digraph<Node, Edge>.GNode[] nodes
                    = this.mCircle.Graph.InternalNodes;
                StringBuilder sb = new StringBuilder(5 * nodes.Length);
                for (int i = 0; i < nodes.Length - 1; i++)
                {
                    sb.Append(string.Concat(nodes[i].mData.ToString(), ","));
                }
                sb.Append(nodes[nodes.Length - 1].mData.ToString());
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

            /*public override bool PositionFixed
            {
                get { return this.mNode.PositionFixed; }
            }/* */

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
        }

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
                    if (this.mSrcNode.Circle != null)
                        this.SrcIndexes = new int[] { srcIndex };
                    if (this.mDstNode.Circle != null)
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
            private double mMaxDevAng = Math.PI / 2.0;

            public BalloonLayouter(Digraph<GNode, GEdge> graph,
                IClusterNode clusterNode)
                : base(graph, clusterNode)
            {
            }

            public BalloonLayouter(Digraph<GNode, GEdge> graph,
                Box2F boundingBox)
                : base(graph, boundingBox)
            {
            }

            public double MaximumDeviationAngle
            {
                get { return this.mMaxDevAng; }
                set
                {
                    if (this.mMaxDevAng != value)
                    {
                        this.mMaxDevAng = value;
                        this.MarkDirty();
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
                        this.MarkDirty();
                    }
                }
            }

            protected override double GetBoundingRadius(GNode node)
            {
                CircleLayouter circle = node.Circle;
                return circle == null 
                    ? base.GetBoundingRadius(node) : circle.BoundingRadius;
            }

            public CircleTree<GNode, GEdge> GetBalloonTree()
            {
                return this.BalloonTree;
            }

            public void CalcCircleRadii()
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

            /*public void SetAdjustCircleAngle(bool adjust)
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

            protected override void CalculateBranchPositions(
                CircleTree<GNode, GEdge>[] branches)
            {
                int i, j;
                double a, dx, dy;
                double[] circleAngles;
                GEdge edge;
                CircleTree<GNode, GEdge> ct;
                // Calculate the relative rotation angle of each leaf's
                // embedding circle based on the average of the angles of
                // the group edge's nodes around its center.
                for (i = 0; i < branches.Length; i++)
                {
                    ct = branches[i];
                    if (ct.BranchCount == 0 && ct.NodeData.Circle != null)
                    {
                        edge = ct.EdgeData;
                        circleAngles = ct.NodeData.Circle.Angles;
                        //wedge = 0.0;
                        dx = dy = 0.0;
                        if (ct.NodeData.CircleID == edge.DstNode.CircleID)
                        {
                            for (j = 0; j < edge.ECount; j++)
                            {
                                a = circleAngles[edge.DstIndexes[j]];
                                //wedge += a < 0.0 ? a + 2 * Math.PI : a;
                                dx -= Math.Cos(a);
                                dy += Math.Sin(a);
                            }
                        }
                        else
                        {
                            for (j = 0; j < edge.ECount; j++)
                            {
                                a = circleAngles[edge.SrcIndexes[j]];
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
                if (ct.NodeData.Circle == null)
                {
                    base.CalculateBranchPositions(branches);
                    return;
                }
                circleAngles = ct.NodeData.Circle.Angles;
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
                            a = circleAngles[edge.DstIndexes[j]];
                            //wedge += a < 0.0 ? a + 2 * Math.PI : a;
                            dx -= Math.Cos(a);
                            dy += Math.Sin(a);
                        }
                    }
                    else
                    {
                        for (j = 0; j < edge.ECount; j++)
                        {
                            a = circleAngles[edge.SrcIndexes[j]];
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
                            a = circleAngles[edge.SrcIndexes[j]];
                            //wedge += a < 0.0 ? a + 2 * Math.PI : a;
                            dx += Math.Cos(a);
                            dy += Math.Sin(a);
                        }
                    }
                    else
                    {
                        for (j = 0; j < edge.ECount; j++)
                        {
                            a = circleAngles[edge.DstIndexes[j]];
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
                PrintArray<Node> data = new PrintArray<Node>();
                CircleLayouter circle = root.NodeData.Circle;
                if (circle == null)
                {
                    data.Data = new Node[] { root.NodeData.Data };
                }
                else
                {
                    data.Data = circle.Graph.Nodes;
                }
                CircleTree<PrintArray<Node>, int> child, parent 
                    = new CircleTree<PrintArray<Node>, int>(data, 0, 
                        root.Radius, root.BranchCount);
                parent.Angle = root.Angle;
                parent.Distance = root.Distance;
                CircleTree<GNode, GEdge>[] branches
                    = root.Branches;
                for (int i = 0; i < branches.Length; i++)
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

        // Circle Position Calculation Paramaters
        private CircleSpacing mNodeSpacing = CircleSpacing.SNS;
        private double mMinRadius = 10;
        private double mFreeArc = 5;

        // Circle Physical Animation Parameters
        private bool bAdjustAngles = false;
        private double mSpringMult = 10;
        private double mMagnetMult = 100;
        private double mMagnetExp = 1;

        // Balloon Tree Sorting Parameters
        private bool bInSketchMode = false;

        // Flags and Calculated Values
        private BalloonLayouter mLayouter;
        private bool bLayouterDirty = true;
        private bool bCircleDirty = true;
        private bool bItemMoved;

        public BalloonCirclesLayoutAlgorithm(Digraph<Node, Edge> graph,
            IClusterNode clusterNode)
            : base(graph, clusterNode)
        {
            Digraph<GNode, GEdge> bGraph = new Digraph<GNode, GEdge>(
                    graph.NodeCount, graph.EdgeCount);
            this.mLayouter = new BalloonLayouter(bGraph, clusterNode);
            this.mLayouter.SpanningTreeGeneration = SpanningTreeGen.Prim;
        }

        public BalloonCirclesLayoutAlgorithm(Digraph<Node, Edge> graph,
            Box2F boundingBox)
            : base(graph, boundingBox)
        {
            Digraph<GNode, GEdge> bGraph = new Digraph<GNode, GEdge>(
                    graph.NodeCount, graph.EdgeCount);
            this.mLayouter = new BalloonLayouter(bGraph, boundingBox);
            this.mLayouter.SpanningTreeGeneration = SpanningTreeGen.Prim;
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
                    this.bCircleDirty = true;
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
            get { return this.mNodeSpacing; }
            set
            {
                if (this.mNodeSpacing != value)
                {
                    this.mNodeSpacing = value;
                    this.bCircleDirty = true;
                }
            }
        }
        /// <summary>
        /// Gets or sets the minimum radius of the embedding circle
        /// calculated by this layout algorithm.
        /// </summary>
        public double MinRadius
        {
            get { return this.mMinRadius; }
            set
            {
                if (this.mMinRadius != value)
                {
                    this.mMinRadius = value;
                    this.bCircleDirty = true;
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
            get { return this.mFreeArc; }
            set
            {
                if (this.mFreeArc != value)
                {
                    this.mFreeArc = value;
                    this.bCircleDirty = true;
                }
            }
        }
        #endregion

        #region Circle Physical Animation Parameters

        public bool AdjustCircleAngles
        {
            get { return this.bAdjustAngles; }
            set
            {
                if (this.bAdjustAngles != value)
                {
                    this.bAdjustAngles = value;
                }
            }
        }

        public double CircleSpringMultiplier
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

        public double CircleMagneticMultiplier
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

        public double CircleMagneticExponent
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
        #endregion

        #region Balloon Tree Generating Parameters
        /// <summary>
        /// Gets or sets the method this algorithm uses to build its internal
        /// sparsely connected spanning tree for traversing its graph.
        /// </summary>
        public SpanningTreeGen SpanningTreeGeneration
        {
            get { return this.mLayouter.SpanningTreeGeneration; }
            set { this.mLayouter.SpanningTreeGeneration = value; }
        }
        /// <summary>
        /// Gets or sets the method this algorithm uses to choose the root
        /// node from which all subtrees branch off and orbit around.
        /// </summary>
        public TreeRootFinding RootFindingMethod
        {
            get { return this.mLayouter.RootFindingMethod; }
            set { this.mLayouter.RootFindingMethod = value; }
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
                    this.mLayouter.InSketchMode = value;
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
            get { return this.mLayouter.BranchSpacing; }
            set { this.mLayouter.BranchSpacing = value; }
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
            get { return this.mLayouter.RootCentering; }
            set { this.mLayouter.RootCentering = value; }
        }
        /// <summary>
        /// Gets or sets the minimum length of an edge connecting a
        /// subtree/leaf node to its root node.
        /// </summary>
        public double MinimumEdgeLength
        {
            get { return this.mLayouter.MinimumEdgeLength; }
            set { this.mLayouter.MinimumEdgeLength = value; }
        }
        /// <summary>
        /// Gets or sets whether the lengths of distances between a set of
        /// subtree/leaf nodes and their shared root node are equalized or
        /// minimized.</summary>
        public bool EqualizeBranchLengths
        {
            get { return this.mLayouter.EqualizeBranchLengths; }
            set { this.mLayouter.EqualizeBranchLengths = value; }
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
            get { return this.mLayouter.MaximumRootWedge; }
            set { this.mLayouter.MaximumRootWedge = value; }
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
            get { return this.mLayouter.MaximumTreeWedge; }
            set { this.mLayouter.MaximumTreeWedge = value; }
        }

        public double MaximumDeviationAngle
        {
            get { return this.mLayouter.MaximumDeviationAngle; }
            set { this.mLayouter.MaximumDeviationAngle = value; }
        }
        #endregion

        #region Balloon Tree Physical Animation Parameters
        /// <summary>
        /// Gets or sets whether the roots of any leaf/subtree nodes with
        /// fixed positions are first repositioned to attempt to set the
        /// local polar coordinate positions of their fixed branches equal
        /// to those calculated for the balloon tree.</summary>
        public bool AdjustRootCenters
        {
            get { return this.mLayouter.AdjustRootCenters; }
            set { this.mLayouter.AdjustRootCenters = value; }
        }

        public bool AdjustRootAngle
        {
            get { return this.mLayouter.AdjustRootAngle; }
            set { this.mLayouter.AdjustRootAngle = value; }
        }

        public double SpringMultiplier
        {
            get { return this.mLayouter.SpringMultiplier; }
            set { this.mLayouter.SpringMultiplier = value; }
        }

        public double MagneticMultiplier
        {
            get { return this.mLayouter.MagneticMultiplier; }
            set { this.mLayouter.MagneticMultiplier = value; }
        }

        public double MagneticExponent
        {
            get { return this.mLayouter.MagneticExponent; }
            set { this.mLayouter.MagneticExponent = value; }
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
            get { return this.mLayouter.RootAngle; }
            set { this.mLayouter.RootAngle = value; }
        }
        #endregion

        #region Balloon Tree Angle Parameters in Degrees
        /// <summary>
        /// Gets or sets the <see cref="MaximumRootWedge"/> parameter of this
        /// layout algorithm in degrees instead of radians for debugging.
        /// </summary>
        public double DegMaxRootWedge
        {
            get { return this.mLayouter.DegMaxRootWedge; }
            set { this.mLayouter.DegMaxRootWedge = value; }
        }
        /// <summary>
        /// Gets or sets the <see cref="MaximumTreeWedge"/> parameter of this
        /// layout algorithm in degrees instead of radians for debugging.
        /// </summary>
        public double DegMaxTreeWedge
        {
            get { return this.mLayouter.DegMaxTreeWedge; }
            set { this.mLayouter.DegMaxTreeWedge = value; }
        }
        /// <summary>
        /// Gets or sets the <see cref="MaximumDeviationAngle"/> parameter
        /// of this layout algorithm in degrees instead of radians for 
        /// debugging.</summary>
        public double DegMaxDeviationAngle
        {
            get { return this.mLayouter.DegMaxDeviationAngle; }
            set { this.mLayouter.DegMaxDeviationAngle = value; }
        }
        /// <summary>
        /// Gets or sets the <see cref="RootAngle"/> parameter of this
        /// layout algorithm in degrees instead of radians for debugging.
        /// </summary>
        public double DegRootAngle
        {
            get { return this.mLayouter.DegRootAngle; }
            set { this.mLayouter.DegRootAngle = value; }
        }
        #endregion

        #endregion

        public CircleTree<PrintArray<Node>, int> DebugTree
        {
            get
            {
                if (this.mLayouter == null)
                    return null;
                return this.mLayouter.DebugTree;
            }
        }

        public Vec2F DebugRootPosition
        {
            get
            {
                if (this.mLayouter == null)
                    return null;
                return this.mLayouter.DebugRootPosition;
            }
        }

        protected override void InitializeAlgorithm()
        {
            this.bItemMoved = true;
        }

        protected override bool CanIterate()
        {
            return this.bItemMoved;
        }

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

        protected override void OnBeginIteration(uint iteration, bool dirty, 
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
                    = this.mLayouter.Graph.InternalNodes;
            if (this.bCircleDirty && !graphDirty && !this.bLayouterDirty)
            {
                // This is also done in the BalloonCircle
                // constructor in InitBalloonGraph()
                for (i = 0; i < nodes.Length; i++)
                {
                    circle = nodes[i].mData.Circle;
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
                    circle = nodes[i].mData.Circle;
                    if (circle != null)
                    {
                        circle.CallBeginIteration(iteration);
                    }
                }
            }
            if (this.bCircleDirty)
            {
                // Radii are also set in this.mLayouter.BuildBalloonTree()
                this.mLayouter.CalcCircleRadii();
            }

            this.mLayouter.CallBeginIteration(iteration);

            /*if (this.bCircleDirty || graphDirty)
            {
                this.mLayouter.SetCircleAngles();
            }/* */

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
                = this.mLayouter.Graph.InternalNodes;
            // Figure out whether the position of each balloon circle is
            // "fixed" (whether it contains a fixed node) at the start, 
            // so that the calculation doesn't have to be run each time 
            // the PositionFixed property of balloon circle is retrieved.
            for (i = 0; i < nodes.Length; i++)
            {
                node = nodes[i].mData;
                node.CalcPositionFixed();
            }

            // Pull the centers of balloon circles containing  
            // fixed nodes towards those fixed nodes.
            if (this.mStack == null)
            {
                this.mStack 
                    = new Stack<CircleTree<GNode, GEdge>>(nodes.Length);
            }
            ct = this.mLayouter.GetBalloonTree();
            ct.NodeData.StackAngle = this.mLayouter.RootAngle;
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
                            this.mLayouter.AdjustRootAngle)
                        {
                            this.mLayouter.RootAngle
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
            this.mLayouter.CallPerformIteration(iteration);

            // Readjust the root circle just in case
            // one of its branches was also fixed.
            ct = this.mLayouter.GetBalloonTree();
            if (ct.NodeData.PositionFixed && 
                this.mLayouter.AdjustRootAngle)
            {
                circle = ct.NodeData.Circle;
                if (circle != null)
                {
                    circle.Angle = this.mLayouter.RootAngle
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
                    ang = this.mLayouter.RootAngle;
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
                = this.mLayouter.Graph.InternalNodes;
            for (int i = 0; i < nodes.Length; i++)
            {
                circle = nodes[i].mData.Circle;
                if (circle != null)
                {
                    circle.CallEndIteration(iteration);
                    this.bItemMoved = this.bItemMoved || circle.ItemMoved;
                }
            }
            this.mLayouter.CallEndIteration(iteration);
            this.bItemMoved = this.bItemMoved || this.mLayouter.ItemMoved;
        }

        private void InitBalloonGraph()
        {
            int i, j;
            Digraph<Node, Edge>.GNode[] nodes
                = this.mGraph.InternalNodes;
            Digraph<Node, Edge>.GEdge[] edges
                = this.mGraph.InternalEdges;

            BCCAlgorithm<Node, Edge> bccAlg
                = new BCCAlgorithm<Node, Edge>(this.mGraph);
            bccAlg.Compute();

            Node[][] bccGroups;
            int bccGroupCount;
            int[] bccGroupIds;
            if (this.mGroupingMethod == LayoutStyle.BccCompact)
            {
                bccAlg.ArticulateToLargerCompactGroups();
                bccGroups = bccAlg.CompactGroups;
                bccGroupCount = bccAlg.CompactGroupCount;
                bccGroupIds = bccAlg.CompactGroupIds;
            }
            else
            {
                bccGroups = bccAlg.IsolatedGroups;
                bccGroupCount = bccAlg.IsolatedGroupCount;
                bccGroupIds = bccAlg.IsolatedGroupIds;
            }
            Digraph<GNode, GEdge> bccGraph = this.mLayouter.Graph;
            bccGraph.ClearNodes();

            int[] circleIs = new int[nodes.Length];
            GEdge[][] bccEdges = new GEdge[bccGroupCount][];
            GNode[] bccNodes = new GNode[bccGroupCount];
            GNode bccNode;
            Digraph<Node, Edge> subGraph;
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
                    bccNode = new BalloonNode(i, bccGroup[0]);
                    circleIs[this.mGraph.IndexOfNode(bccGroup[0])] = 0;
                }
                else
                {
                    subGraph = new Digraph<Node, Edge>(
                        bccGroup.Length, bccGroup.Length / 2);
                    for (j = 0; j < bccGroup.Length; j++)
                    {
                        subGraph.AddNode(bccGroup[j]);
                        circleIs[this.mGraph.IndexOfNode(bccGroup[j])] = j;
                    }
                    bccNode = new BalloonCircle(i, subGraph, this);
                }
                // TODO: Make sure excluding null nodes doesn't cause other
                // bad stuff to happen later on.
                if (bccNode != null)
                    bccGraph.AddNode(bccNode);
                bccNodes[i] = bccNode;
                bccEdges[i] = new GEdge[bccGroupCount];
            }

            int si, di;
            bool reversed;
            Digraph<Node, Edge>.GEdge edge;
            GEdge bccEdge;
            for (i = 0; i < edges.Length; i++)
            {
                edge = edges[i];
                si = bccGroupIds[edge.mSrcNode.Index];
                di = bccGroupIds[edge.mDstNode.Index];
                if (si == di && edge.mSrcNode.Index != edge.mDstNode.Index)
                {
                    bccNodes[si].Circle.Graph.AddEdge(edges[i].mData);
                }
                else if (si != di)
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
                        bccEdge = new GEdge(bccNodes[si], bccNodes[di]);
                        bccGraph.AddEdge(bccEdge);
                        reversed = false;
                        bccEdges[si][di] = bccEdge;
                    }
                    if (reversed)
                    {
                        bccEdge.AddEdge(edge.mData,
                            circleIs[edge.mDstNode.Index],
                            circleIs[edge.mSrcNode.Index]);
                    }
                    else
                    {
                        bccEdge.AddEdge(edge.mData,
                            circleIs[edge.mSrcNode.Index],
                            circleIs[edge.mDstNode.Index]);
                    }
                }
            }

            this.SetBalloonRoot(this.TryGetGraphRoot());
        }

        protected override void OnRootChanged(Node oldRoot)
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
                    = this.mLayouter.Graph.InternalNodes;
                for (i = 0; i < nodes.Length && !flag; i++)
                {
                    node = nodes[i].mData;
                    if (node.Circle == null)
                    {
                        flag = root.mData.Equals(node.Data);
                    }
                    else
                    {
                        cNodes = node.Circle.Graph.InternalNodes;
                        for (j = 0; j < cNodes.Length && !flag; j++)
                        {
                            flag = root.mData.Equals(cNodes[j].mData);
                        }
                    }
                    if (flag)
                    {
                        this.mLayouter.SetRoot(node);
                    }
                }
            }
        }
    }
}
