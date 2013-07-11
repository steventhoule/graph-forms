﻿using System;
using System.Collections.Generic;
using System.Text;
using GraphForms.Algorithms.ConnectedComponents;
using GraphForms.Algorithms.Path;

namespace GraphForms.Algorithms.Layout.Circular
{
    public class FDSingleCircleLayoutAlgorithm<Node, Edge>
        : LayoutAlgorithm<Node, Edge>
        where Node : class, ILayoutNode
        where Edge : IGraphEdge<Node>, IUpdateable
    {
        private Digraph<Node, Edge>.GNode[] mEmbedCircle;
        private Digraph<Node, Edge>.GNode[] mPortNodes;

        // Position Calculation Paramaters
        private CircleSpacing mNodeSpacing = CircleSpacing.SNS;
        private CircleCentering mCentering = CircleCentering.Centroid;
        private double mCenterX = 0;
        private double mCenterY = 0;
        private double mMinRadius = 10;
        private double mFreeArc = 5;

        // Physical Animation Parameters
        private bool bAdjustCenter = true;
        private bool bAdjustAngle = false;
        private double mSpringMult = 10;
        private double mMagnetMult = 100;
        private double mMagnetExp = 1;
        private double mAngle = 0;

        // Flags and Calculated Values
        private double mRadius;
        private double[] mAngles;
        private bool bCenterDirty = true;
        private double mCX;
        private double mCY;

        public FDSingleCircleLayoutAlgorithm(Digraph<Node, Edge> graph,
            IClusterNode clusterNode)
            : base(graph, clusterNode)
        {
        }

        public FDSingleCircleLayoutAlgorithm(Digraph<Node, Edge> graph,
            Box2F boundingBox)
            : base(graph, boundingBox)
        {
        }

        #region Parameters

        #region Postion Calculation Parameters
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
                    this.MarkDirty();
                }
            }
        }
        /// <summary>
        /// Gets or sets the method this algorithm uses to calculate the 
        /// initial position of the center of its embedding circle.
        /// </summary>
        public CircleCentering Centering
        {
            get { return this.mCentering; }
            set
            {
                if (this.mCentering != value)
                {
                    this.mCentering = value;
                    this.bCenterDirty = true;
                }
            }
        }
        /// <summary>
        /// Gets or sets the X-coordinate of the center of the embedding
        /// circle (in the local coordinate system of the graph), which is
        /// only used if <see cref="Centering"/> is set to
        /// <see cref="CircleCentering.Predefined"/>.</summary>
        public double CenterX
        {
            get { return this.mCenterX; }
            set
            {
                if (this.mCenterX != value)
                {
                    this.mCenterX = value;
                    if (this.mCentering == CircleCentering.Predefined)
                        this.bCenterDirty = true;
                }
            }
        }
        /// <summary>
        /// Gets or sets the Y-coordinate of the center of the embedding
        /// circle (in the local coordinate system of the graph), which is
        /// only used if <see cref="Centering"/> is set to
        /// <see cref="CircleCentering.Predefined"/>.</summary>
        public double CenterY
        {
            get { return this.mCenterY; }
            set
            {
                if (this.mCenterY != value)
                {
                    this.mCenterY = value;
                    if (this.mCentering == CircleCentering.Predefined)
                        this.bCenterDirty = true;
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
                    this.MarkDirty();
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
                    this.MarkDirty();
                }
            }
        }
        #endregion

        #region Physical Animation Parameters
        
        /// <summary>
        /// Gets or sets whether the center of the embedding circle is
        /// first repositioned to attempt to set the radii and angle
        /// of any fixed nodes on the embedding circle equal to those
        /// calculated for the embedding circle.</summary>
        public bool AdjustCenter
        {
            get { return this.bAdjustCenter; }
            set
            {
                if (this.bAdjustCenter != value)
                {
                    this.bAdjustCenter = value;
                }
            }
        }

        public bool AdjustAngle
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
        /// Gets or sets the angle which the nodes of the graph are rotated
        /// around the center of the embedding circle, measured in radians
        /// counterclockwise from the +X-axis.
        /// </summary><remarks>
        /// Since this angle is measured clockwise from the +X-axis on a
        /// standard drawing surface (because the Y-axis is negated),
        /// the final graph will appear as if it has been rotated by the
        /// negation of this angle on a conventional 2D Euclidean manifold. 
        /// </remarks>
        public double Angle
        {
            get { return this.mAngle; }
            set
            {
                if (this.mAngle != value)
                {
                    this.mAngle = value;
                }
            }
        }
        #endregion

        #region Angle Parameters in Degrees
        /// <summary>
        /// Gets or sets the <see cref="Angle"/> parameter of this layout
        /// algorithm in degrees instead of radians for debugging.
        /// </summary>
        public double DegAngle
        {
            get { return 180.0 * this.mAngle / Math.PI; }
            set
            {
                value = Math.PI * value / 180.0;
                if (this.mAngle != value)
                {
                    this.mAngle = value;
                }
            }
        }
        #endregion

        #endregion

        private void CalculateEmbeddingCircle()
        {
            Digraph<Node, Edge>.GNode[] nodes
                = this.mGraph.InternalNodes;
            List<Digraph<Node, Edge>.GNode> ecNodes, pNodes;
            int j, count = nodes.Length;
            if (this.mGraph.EdgeCount == 0)
            {
                ecNodes = new List<Digraph<Node, Edge>.GNode>(count + 1);
                pNodes = new List<Digraph<Node, Edge>.GNode>(count + 1);
                for (j = 0; j < count; j++)
                {
                    if (nodes[j].mData is IPortNode)
                        pNodes.Add(nodes[j]);
                    else
                        ecNodes.Add(nodes[j]);
                }
                this.mEmbedCircle = ecNodes.ToArray();
                this.mPortNodes = pNodes.ToArray();
                return;
            }
            // Calculate an initial embedding circle based on
            // the longest path in the graph.
            DFLongestPath<Node, Edge> alg 
                = new DFLongestPath<Node, Edge>(this.mGraph, 
                    false, false);
            alg.ExcludeSpecialNodes = true;
            alg.Compute();
            int[] nis = alg.PathNodeIndexes;
            ecNodes = new List<Digraph<Node, Edge>.GNode>(count + 1);
            bool[] flags = new bool[count];
            // Here flags are used to determine which nodes in the graph
            // are not currently part of the embedding circle.
            for (j = 0; j < nis.Length; j++)
            {
                flags[nis[j]] = true;
                ecNodes.Add(nodes[nis[j]]);
            }
            // Find all nodes not currently in the embedding circle
            
            Digraph<Node, Edge>.GNode[] remNodes 
                = new Digraph<Node, Edge>.GNode[count - nis.Length];
            pNodes = new List<Digraph<Node, Edge>.GNode>(count - nis.Length);
            count = 0;
            for (j = 0; j < flags.Length; j++)
            {
                if (!flags[j])
                {
                    remNodes[count] = nodes[j];
                    if (remNodes[count].mData is IPortNode)
                        pNodes.Add(nodes[j]);
                    else
                        count++;
                }
            }
            // Here flags are used to determine which nodes are neighbors 
            // of the current node to be added to the embedding circle.
            Digraph<Node, Edge>.GEdge[] edges;
            Digraph<Node, Edge>.GNode node;
            bool placed;
            int eCount;
            for (int k = 0; k < count; k++)
            {
                // Clear neighbor flags from previous iteration
                for (j = 0; j < flags.Length; j++)
                {
                    flags[j] = false;
                }
                // Set which nodes are neighbors to the current node
                eCount = 0;
                edges = remNodes[k].InternalDstEdges;
                for (j = 0; j < edges.Length; j++)
                {
                    node = edges[j].mDstNode;
                    flags[node.Index] = !(node.mData is IPortNode);
                }
                if (flags[remNodes[k].Index])
                {
                    flags[remNodes[k].Index] = false;
                    eCount++;
                }
                edges = remNodes[k].InternalSrcEdges;
                for (j = 0; j < edges.Length; j++)
                {
                    node = edges[j].mSrcNode;
                    flags[node.Index] = !(node.mData is IPortNode);
                }
                if (flags[remNodes[k].Index])
                {
                    flags[remNodes[k].Index] = false;
                    eCount++;
                }
                eCount = remNodes[k].AllEdgeCount - eCount;
                placed = false;
                // Look for two consecutive neighbors in the list
                if (eCount >= 2)
                {
                    for (j = 0; j < ecNodes.Count; j++)
                    {
                        if (flags[ecNodes[j].Index] &&
                            flags[ecNodes[(j + 1) % ecNodes.Count].Index])
                        {
                            ecNodes.Insert((j + 1) % ecNodes.Count, remNodes[k]);
                            placed = true;
                            break;
                        }
                    }
                }
                // Find any neighbor in the list
                if (!placed && eCount > 0)
                {
                    for (j = 0; j < ecNodes.Count; j++)
                    {
                        if (flags[ecNodes[j].Index])
                        {
                            ecNodes.Insert((j + 1) % ecNodes.Count, remNodes[k]);
                            placed = true;
                            break;
                        }
                    }
                }
                // Place all orphaned nodes at the end of the list
                if (!placed)
                    ecNodes.Add(remNodes[k]);
            }
            this.mEmbedCircle = ecNodes.ToArray();
            this.mPortNodes = pNodes.ToArray();
            this.Swapping(this.mEmbedCircle, 0, ecNodes.Count, 50);
            //this.Swapping(this.mEmbedCircle, ecNodes.Count, 50);

            if (this.mPortNodes.Length > 0)
            {
                this.ReducePortEdgeCrossings();
            }
            //this.BBCCompactTest();
        }

        private void BBCCompactTest()
        {
            BCCAlgorithm<Node, Edge> alg 
                = new BCCAlgorithm<Node, Edge>(this.mGraph);
            alg.Compute();
            alg.ArticulateToLargerCompactGroups();
            this.SeparateNodeGroups(
                alg.CompactGroupIds, alg.CompactGroupCount);
        }
        
        private void Swapping(Digraph<Node, Edge>.GNode[] nodes, 
            int start, int length, int maxIterations)
        {
            if (length > 3)
            {
                int i, j, k, m, posV, posX, posY, offset, improvedCrossings;
                Digraph<Node, Edge>.GNode u, v, x, y;
                Digraph<Node, Edge>.GEdge[] uEdges, vEdges;
                int uDstCount;

                int n = this.mGraph.NodeCount;
                int[] pos = new int[n];
                for (i = 0; i < n; i++)
                {
                    pos[i] = -1;
                }
                n = start + length;
                for (i = start; i < n; i++)
                {
                    pos[nodes[i].Index] = i;
                }

                //int[] indexes = new int[nodes.Count];
                bool improved = true;
                for (int iter = 0; iter < maxIterations && improved; iter++)
                {
                    //for (i = 0; i < n; i++)
                    //    indexes[i] = nodes[i].Index;
                    improved = false;
                    for (i = start; i < n; i++)
                    {
                        u = nodes[i];
                        // we fake a numbering around the circle
                        // starting with u at position 0
                        // using the formula: (pos[t] - offset) % n
                        // and: pos[u] + offset = n
                        offset = n - pos[u.Index];
                        uDstCount = u.DstEdgeCount;
                        uEdges = u.AllInternalEdges(false);
                        for (j = 0; j < uEdges.Length; j++)
                        {
                            // we try swapping u with a node that comes
                            // right before one of its neighbors
                            x = j < uDstCount ? uEdges[j].mDstNode 
                                              : uEdges[j].mSrcNode;
                            //if (x.mData is IPortNode)
                            if (pos[x.Index] == -1)
                                continue;
                            k = (pos[x.Index] + n - 1) % n;
                            if (k == i)
                                continue;
                            v = nodes[k];
                            posV = (k + offset) % n;
                            // we count how many crossings we save 
                            // when swapping u and v
                            improvedCrossings = 0;
                            for (k = 0; k < uEdges.Length; k++)
                            {
                                x = k < uDstCount ? uEdges[k].mDstNode
                                                  : uEdges[k].mSrcNode;
                                if (x.Index == v.Index ||
                                    //x.mData isIPortNode)
                                    pos[x.Index] == -1)
                                    continue;
                                // posX = (pos[x] - pos[u]) mod n
                                posX = (pos[x.Index] + offset) % n;
                                vEdges = v.InternalDstEdges;
                                for (m = 0; m < vEdges.Length; m++)
                                {
                                    y = vEdges[m].mDstNode;
                                    if (y.Index == u.Index || 
                                        y.Index == x.Index ||
                                        //y.mData is IPortNode)
                                        pos[y.Index] == -1)
                                        continue;
                                    // posY = (pos[y] - pos[u]) mod n
                                    posY = (pos[y.Index] + offset) % n;
                                    // All possible permutations:
                                    // ++: u v x y, u y x v
                                    // --: u v y x, u x y v
                                    // 00: u x v y, u y v x
                                    if (posX > posV && posY > posV)
                                    {
                                        if (posX > posY)
                                        {
                                            //   /-------------\
                                            //  /     /---\     \
                                            // u     v     y     x
                                            improvedCrossings--;
                                        }
                                        else
                                        {
                                            //   /-------\
                                            //  /     /---\-----\
                                            // u     v     x     y
                                            improvedCrossings++;
                                        }
                                    }
                                    else if (posX < posV && posY < posV)
                                    {
                                        if (posX > posY)
                                        {
                                            //   /-------\
                                            //  /     /---\-----\
                                            // u     y     x     v
                                            improvedCrossings++;
                                        }
                                        else
                                        {
                                            //  /---\       /---\
                                            // u     x     y     v
                                            improvedCrossings--;
                                        }
                                    }
                                }
                                vEdges = v.InternalSrcEdges;
                                for (m = 0; m < vEdges.Length; m++)
                                {
                                    y = vEdges[m].mSrcNode;
                                    if (y.Index == u.Index || 
                                        y.Index == x.Index ||
                                        //y.mData is IPortNode)
                                        pos[y.Index] == -1)
                                        continue;
                                    posY = (pos[y.Index] + offset) % n;
                                    if (posX > posV && posY > posV)
                                    {
                                        if (posX > posY)
                                            improvedCrossings--;
                                        else
                                            improvedCrossings++;
                                    }
                                    else if (posX < posV && posY < posV)
                                    {
                                        if (posX > posY)
                                            improvedCrossings++;
                                        else
                                            improvedCrossings--;
                                    }
                                }
                            }
                            if (improvedCrossings > 0)
                            {
                                improved = true;
                                // swap the nodes in the list
                                nodes[i] = v;
                                x = j < uDstCount ? uEdges[j].mDstNode
                                                  : uEdges[j].mSrcNode;
                                nodes[(pos[x.Index] + n - 1) % n] = u;
                                // swap tracked positions
                                offset = pos[u.Index];
                                pos[u.Index] = pos[v.Index];
                                pos[v.Index] = offset;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void SeparateWhiteAndGrayNodes()
        {
            // Isolate the gray nodes and reduce their edge crossings.
            Digraph<Node, Edge>.GNode node;
            int last = this.mEmbedCircle.Length - 1;
            while (last >= 0 &&
                this.mEmbedCircle[last].Color == GraphColor.White)
            {
                last--;
            }
            if (last >= 0)
            {
                int first = 0;
                while (first < last &&
                    this.mEmbedCircle[first].Color == GraphColor.White)
                {
                    first++;
                }
                for (int j = first; j < last; j++)
                {
                    if (this.mEmbedCircle[j].Color == GraphColor.White)
                    {
                        node = this.mEmbedCircle[j];
                        Array.Copy(this.mEmbedCircle, j + 1,
                                   this.mEmbedCircle, j, last - j);
                        this.mEmbedCircle[last] = node;
                        this.Swapping(this.mEmbedCircle, first, 
                            last - first + 1, 50);
                        last--;
                    }
                }
            }
        }

        private void SeparateNodeGroups(int[] groupIDs, int groupCount)
        {
            int i, j, count;
            Digraph<Node, Edge>.GNode[] nodes = this.mGraph.InternalNodes;
            for (i = 0; i < groupCount; i++)
            {
                count = 0;
                for (j = 0; j < nodes.Length; j++)
                {
                    if (groupIDs[nodes[j].Index] == i)
                    {
                        nodes[j].Color = GraphColor.Gray;
                        count++;
                    }
                    else
                    {
                        nodes[j].Color = GraphColor.White;
                    }
                }
                if (count > 1)
                {
                    GraphColor[] colors = new GraphColor[this.mEmbedCircle.Length];
                    for (j = 0; j < this.mEmbedCircle.Length; j++)
                        colors[j] = this.mEmbedCircle[j].Color;
                    this.SeparateWhiteAndGrayNodes();
                }
            }
        }

        private void ReducePortEdgeCrossings()
        {
            int i, j, count;
            Digraph<Node, Edge>.GEdge[] edges;
            for (i = 0; i < this.mPortNodes.Length; i++)
            {
                if (this.mPortNodes[i].AllEdgeCount == 0)
                    continue;
                // Reset all embedding circle nodes to white
                for (j = 0; j < this.mEmbedCircle.Length; j++)
                {
                    this.mEmbedCircle[j].Color = GraphColor.White;
                }
                // Mark gray all embedding circle nodes connected to port
                edges = this.mPortNodes[i].InternalDstEdges;
                for (j = 0; j < edges.Length; j++)
                {
                    edges[j].mDstNode.Color = GraphColor.Gray;
                }
                edges = this.mPortNodes[i].InternalSrcEdges;
                for (j = 0; j < edges.Length; j++)
                {
                    edges[j].mSrcNode.Color = GraphColor.Gray;
                }
                count = 0;
                for (j = 0; j < this.mEmbedCircle.Length; j++)
                {
                    if (this.mEmbedCircle[j].Color == GraphColor.Gray)
                    {
                        count++;
                    }
                }
                if (count > 1)
                {
                    this.SeparateWhiteAndGrayNodes();
                }
            }
        }

        private void CalculatePortNodeAngles()
        {
            if (this.mPortNodes.Length > 0)
            {
                double a1, a2;
                int i, j, count;
                double[] angs = new double[this.mPortNodes.Length];
                Digraph<Node, Edge>.GEdge[] edges;
                IPortNode pNode;
                for (i = 0; i < this.mPortNodes.Length; i++)
                {
                    a1 = 0;
                    count = 0;
                    edges = this.mPortNodes[i].InternalDstEdges;
                    for (j = 0; j < edges.Length; j++)
                    {
                        a1 += this.mAngles[edges[j].mDstNode.Index];
                        count++;
                    }
                    edges = this.mPortNodes[i].InternalSrcEdges;
                    for (j = 0; i < edges.Length; j++)
                    {
                        a1 += this.mAngles[edges[j].mSrcNode.Index];
                        count++;
                    }
                    angs[i] = a1 / count;
                }
                // Line tangent to embedding circle at port point
                // ArcTan(length of this line / this.mRadius)
                double maxA = Math.Atan(2);
                if (angs.Length > 1)
                {
                    count = angs.Length;
                    Array.Sort(angs, this.mPortNodes, 0, count, null);
                    a1 = angs[count - 1];
                    a2 = angs[1];
                    for (i = 0; i < count; i++)
                    {
                        pNode = this.mPortNodes[i].mData as IPortNode;
                        pNode.MinAngle
                            = Math.Max((a1 + angs[i]) / 2, angs[i] - maxA);
                        pNode.MaxAngle
                            = Math.Min((angs[i] + a2) / 2, angs[i] + maxA);
                        a1 = angs[i];
                        a2 = angs[(i + 1) % count];
                    }
                }
                else
                {
                    pNode = this.mPortNodes[0].mData as IPortNode;
                    pNode.MinAngle = angs[0] - maxA;
                    pNode.MaxAngle = angs[0] + maxA;
                }
            }
        }

        private void InitCircle()
        {
            Box2F bbox;
            Digraph<Node, Edge>.GNode[] nodes 
                = this.mEmbedCircle;
            int i, count = this.mGraph.NodeCount;
            double perimeter, ang, a;
            double[] halfSize = new double[count];

            // calculate the size of the circle
            perimeter = 0;
            for (i = 0; i < nodes.Length; i++)
            {
                bbox = nodes[i].mData.LayoutBBox;
                a = Math.Sqrt(bbox.W * bbox.W + bbox.H * bbox.H);
                perimeter += a;
                halfSize[i] = a / 4;
            }
            perimeter += nodes.Length * this.mFreeArc;

            this.mRadius = perimeter / (2 * Math.PI);
            this.mAngles = new double[count];

            // precalculation
            ang = 0;
            for (i = 0; i < nodes.Length; i++)
            {
                a = Math.Sin(halfSize[i] / this.mRadius) * 4;
                ang += a;
            }

            //base.EndIteration(0, 0.5, "Precalculation done.");

            // recalculate radius
            this.mRadius = ang / (2 * Math.PI) * this.mRadius;

            // calculation
            ang = -Math.PI;
            switch (this.mNodeSpacing)
            {
                case CircleSpacing.Fractal:
                    a = Math.PI / nodes.Length;
                    for (i = 0; i < nodes.Length; i++)
                    {
                        ang += a;
                        this.mAngles[nodes[i].Index] = ang;
                        ang += a;
                    }
                    break;
                case CircleSpacing.SNS:
                    for (i = 0; i < nodes.Length; i++)
                    {
                        a = Math.Sin(halfSize[i] / this.mRadius) * 2;
                        ang += a;
                        this.mAngles[nodes[i].Index] = ang;
                        ang += a;
                    }
                    break;
            }

            this.mRadius = Math.Max(this.mRadius, this.mMinRadius);

            this.CalculatePortNodeAngles();

            //base.EndIteration(1, 1, "Calculation done.");
        }

        /// <summary>
        /// If the number of nodes in the graph have changed, this function
        /// recalculates the embedding circle, minimizes the edge
        /// crossings in that circle, and then calculates the radius of the
        /// embedding circle and the angles of each node in that circle 
        /// around its center. </summary>
        /// <param name="iteration">The current number of iterations that 
        /// have already occurred in this algorithm's computation.</param>
        /// <param name="dirty">Whether this algorithm was marked dirty
        /// before this iteration began.</param>
        /// <param name="lastNodeCount">The number of nodes in this
        /// algorithm's graph at the end of the last iteration.
        /// </param>
        /// <param name="lastEdgeCount">The number of edges in this
        /// algorithm's graph at the end of the last iteration.</param>
        protected override void OnBeginIteration(uint iteration, 
            bool dirty, int lastNodeCount, int lastEdgeCount)
        {
            bool nodesDirty = lastNodeCount != this.mGraph.NodeCount;
            if (nodesDirty || lastEdgeCount != this.mGraph.EdgeCount)
            {
                this.CalculateEmbeddingCircle();
            }
            if (dirty || nodesDirty)
            {
                this.InitCircle();
            }
            if (this.bCenterDirty || nodesDirty)
            {
                // Compute Initial Center
                switch (this.mCentering)
                {
                    case CircleCentering.BBoxCenter:
                        Box2F bbox = this.mClusterNode == null
                            ? this.BoundingBox
                            : this.mClusterNode.LayoutBBox;
                        this.mCX = bbox.X + bbox.W / 2.0;
                        this.mCY = bbox.Y + bbox.H / 2.0;
                        break;
                    case CircleCentering.Centroid:
                        Node node;
                        Digraph<Node, Edge>.GNode[] nodes 
                            = this.mGraph.InternalNodes;
                        // Initialize the center as the centroid of the nodes
                        this.mCX = this.mCY = 0;
                        for (int i = 0; i < nodes.Length; i++)
                        {
                            node = nodes[i].mData;
                            //pos = nodes[i].SceneTranslate();
                            this.mCX += node.X;//pos.Width;
                            this.mCY += node.Y;//pos.Height;
                        }
                        this.mCX /= nodes.Length;
                        this.mCY /= nodes.Length;
                        break;
                    case CircleCentering.Predefined:
                    default:
                        this.mCX = this.mCenterX;
                        this.mCY = this.mCenterY;
                        break;
                }
                this.bCenterDirty = false;
            }
            base.OnBeginIteration(iteration, dirty, 
                lastNodeCount, lastEdgeCount);
        }

        protected override void PerformIteration(uint iteration)
        {
            //System.Drawing.SizeF pos;
            int i;
            Node node;
            double dx, dy, r, force, fx, fy;
            Digraph<Node, Edge>.GNode[] nodes = this.mGraph.InternalNodes;
            // Pull the center towards fixed nodes
            if (this.bAdjustCenter)
            {
                for (i = 0; i < nodes.Length; i++)
                {
                    node = nodes[i].mData;
                    if (node.PositionFixed)
                    {
                        dx = this.mCX - node.X;
                        dy = this.mCY - node.Y;
                        if (dx == 0 && dy == 0)
                        {
                            fx = fy = this.mRadius / 10;
                        }
                        else
                        {
                            r = dx * dx + dy * dy;
                            if (this.bAdjustAngle)
                            {
                                this.mAngle = Math.Atan2(-dy, -dx)
                                    - this.mAngles[nodes[i].Index];
                                while (this.mAngle < -Math.PI)
                                    this.mAngle += 2 * Math.PI;
                                while (this.mAngle > Math.PI)
                                    this.mAngle -= 2 * Math.PI;
                                fx = fy = 0;
                            }
                            else
                            {
                                // Magnetic Torque
                                force = this.mAngles[nodes[i].Index]
                                      + this.mAngle;
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
                                Math.Log(r / this.mRadius);
                            fx += force * dx / r;
                            fy += force * dy / r;
                        }
                        // Apply force to center position
                        this.mCX -= fx;
                        this.mCY -= fy;/* */
                    }
                }
            }
            // Pull movable nodes towards the center
            for (i = 0; i < nodes.Length; i++)
            {
                node = nodes[i].mData;
                if (node.PositionFixed)
                {
                    node.SetNewPosition(node.X, node.Y);
                }
                else
                {
                    dx = this.mCX - node.X;
                    dy = this.mCY - node.Y;
                    if (dx == 0 && dy == 0)
                    {
                        fx = fy = this.mRadius / 10;
                    }
                    else
                    {
                        r = dx * dx + dy * dy;
                        // Magnetic Torque
                        force = this.mAngles[nodes[i].Index] + this.mAngle;
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
                            Math.Log(r / this.mRadius);
                        fx += force * dx / r;
                        fy += force * dy / r;
                    }
                    // Add force to position
                    node.SetNewPosition((float)(node.X + fx), 
                                        (float)(node.Y + fy));/* */
                    
                    /*force = this.mAngles[nodes[i].Index] + this.mAngle;
                    dx = cx + this.mRadius * Math.Cos(force);
                    dy = cy + this.mRadius * Math.Sin(force);
                    node.SetNewPosition((float)dx, (float)dy);/* */
                }
            }
        }
    }
}
