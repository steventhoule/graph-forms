using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using GraphForms.Algorithms.Layout.ForceDirected;
using GraphForms.Algorithms.Path;

namespace GraphForms.Algorithms.Layout.Circular
{
    public class FDSingleCircleLayoutAlgorithm<Node, Edge>
        //: ForceDirectedLayoutAlgorithm<Node, Edge, FDSingleCircleLayoutParameters>
        : LayoutAlgorithm<Node, Edge>
        where Node : GraphElement, ILayoutNode
        where Edge : class, IGraphEdge<Node>, IUpdateable
    {
        private Digraph<Node, Edge>.GNode[] mEmbeddingCircle;
        private Digraph<Node, Edge>.GNode[] mPortNodes;

        private double mMinRadius = 10;
        private double mFreeArc = 5;

        private double mRadius;
        private double[] mAngles;

        private bool bCalcCenter = false;
        private double mCenterX;
        private double mCenterY;
        private double mSpringMult = 1;
        private double mMagnetMult = 1;
        private double mAngleExp = 1;

        /*public FDSingleCircleLayoutAlgorithm(Digraph<Node, Edge> graph)
            : base(graph, null)
        {
        }

        public FDSingleCircleLayoutAlgorithm(Digraph<Node, Edge> graph,
            FDSingleCircleLayoutParameters oldParameters)
            : base(graph, oldParameters)
        {
        }/* */

        public FDSingleCircleLayoutAlgorithm(Digraph<Node, Edge> graph,
            IClusterNode clusterNode)
            : base(graph, clusterNode)
        {
        }

        public FDSingleCircleLayoutAlgorithm(Digraph<Node, Edge> graph,
            RectangleF boundingBox)
            : base(graph, boundingBox)
        {
        }

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

        public double AngleExponent
        {
            get { return this.mAngleExp; }
            set
            {
                if (this.mAngleExp != value)
                {
                    this.mAngleExp = value;
                }
            }
        }

        public bool CenterInBounds
        {
            get { return !this.bCalcCenter; }
            set
            {
                if (this.bCalcCenter == value)
                {
                    this.bCalcCenter = !value;
                }
            }
        }

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
                this.mEmbeddingCircle = ecNodes.ToArray();
                this.mPortNodes = pNodes.ToArray();
                return;// ecNodes.ToArray();
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
            this.Swapping(ecNodes, 50);
            this.mEmbeddingCircle = ecNodes.ToArray();
            this.mPortNodes = pNodes.ToArray();
            //return ecNodes.ToArray();
        }
        
        private void Swapping(List<Digraph<Node, Edge>.GNode> nodes,
            int maxIterations)
        {
            if (nodes.Count > 3)
            {
                int i, j, k, m, posV, posX, posY, offset, improvedCrossings;
                Digraph<Node, Edge>.GNode u, v, x, y;
                Digraph<Node, Edge>.GEdge[] uEdges, vEdges;
                int uDstCount;

                int n = nodes.Count;
                int[] pos = new int[this.mGraph.NodeCount];
                for (i = 0; i < n; i++)
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
                    for (i = 0; i < n; i++)
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
                            if (x.mData is IPortNode)
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
                                    x.mData is IPortNode)
                                    continue;
                                // posX = (pos[x] - pos[u]) mod n
                                posX = (pos[x.Index] + offset) % n;
                                vEdges = v.InternalDstEdges;
                                for (m = 0; m < vEdges.Length; m++)
                                {
                                    y = vEdges[m].mDstNode;
                                    if (y.Index == u.Index || 
                                        y.Index == x.Index ||
                                        y.mData is IPortNode)
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
                                        y.mData is IPortNode)
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

        private void InitCircle()
        {
            System.Drawing.RectangleF bbox;
            Digraph<Node, Edge>.GNode[] nodes 
                = this.mEmbeddingCircle;
            int i, count = this.mGraph.NodeCount;
            double perimeter, ang, a;
            double[] halfSize = new double[count];

            // calculate the size of the circle
            perimeter = 0;
            for (i = 0; i < nodes.Length; i++)
            {
                bbox = nodes[i].mData.BoundingBox;
                a = Math.Sqrt(bbox.Width * bbox.Width 
                            + bbox.Height * bbox.Height);
                perimeter += a;
                halfSize[nodes[i].Index] = a / 4;
            }
            perimeter += nodes.Length * this.mFreeArc;

            this.mRadius = perimeter / (2 * Math.PI);
            this.mAngles = new double[count];

            // precalculation
            ang = 0;
            for (i = 0; i < nodes.Length; i++)
            {
                a = Math.Sin(halfSize[nodes[i].Index] / this.mRadius) * 4;
                ang += a;
            }

            //base.EndIteration(0, 0.5, "Precalculation done.");

            // recalculate radius
            this.mRadius = ang / (2 * Math.PI) * this.mRadius;

            // calculation
            ang = -Math.PI;
            for (i = 0; i < nodes.Length; i++)
            {
                a = Math.Sin(halfSize[nodes[i].Index] / this.mRadius) * 2;
                ang += a;
                this.mAngles[nodes[i].Index] = ang;
                ang += a;
            }

            this.mRadius = Math.Max(this.mRadius, this.mMinRadius);

            this.CalculatePortAngles();

            //base.EndIteration(1, 1, "Calculation done.");
        }

        private void CalculatePortAngles()
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
                count = angs.Length;
                if (count > 1)
                {
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

        /*protected override bool OnBeginIteration(bool paramsDirty, 
            int lastNodeCount, int lastEdgeCount)
        {
            bool recalc = false;
            if (paramsDirty)
            {
                FDSingleCircleLayoutParameters param = this.Parameters;
                if (this.mMinRadius != param.MinRadius)
                {
                    this.mMinRadius = param.MinRadius;
                    recalc = true;
                }
                if (this.mFreeArc != param.FreeArc)
                {
                    this.mFreeArc = param.FreeArc;
                    recalc = true;
                }
                this.mCenterX = param.X + param.Width / 2;
                this.mCenterY = param.Y + param.Height / 2;
                this.mSpringMult = param.SpringMultiplier;
                this.mMagnetMult = param.MagneticMultiplier;
                this.mAngleExp = param.AngleExponent;
                this.bCalcCenter = !param.CenterInBounds;
            }
            if (lastNodeCount != this.mGraph.NodeCount ||
                lastEdgeCount != this.mGraph.EdgeCount)
            {
                this.CalculateEmbeddingCircle();
            }
            if (recalc || lastNodeCount != this.mGraph.NodeCount)
            {
                this.InitCircle();
            }
            return base.OnBeginIteration(paramsDirty, 
                lastNodeCount, lastEdgeCount);
        }/* */

        protected override void OnBeginIteration(uint iteration, 
            bool dirty, int lastNodeCount, int lastEdgeCount)
        {
            RectangleF bbox = this.mClusterNode == null
                ? this.BoundingBox : this.mClusterNode.BoundingBox;
            this.mCenterX = (bbox.X + bbox.Width) / 2;
            this.mCenterY = (bbox.Y + bbox.Height) / 2;

            bool nodesDirty = lastNodeCount != this.mGraph.NodeCount;
            if (nodesDirty || lastEdgeCount != this.mGraph.EdgeCount)
            {
                this.CalculateEmbeddingCircle();
            }
            if (dirty || nodesDirty)
            {
                this.InitCircle();
            }
            base.OnBeginIteration(iteration, dirty, 
                lastNodeCount, lastEdgeCount);
        }

        protected override void PerformIteration(uint iteration)
        {
            //System.Drawing.SizeF pos;
            double dx, dy, r, force, fx, fy;
            Node node;
            Digraph<Node, Edge>.GNode[] nodes 
                = this.mGraph.InternalNodes;
            int i;
            // Compute Center
            if (this.bCalcCenter)
            {
                this.mCenterX = 0;
                this.mCenterY = 0;
                for (i = 0; i < nodes.Length; i++)
                {
                    node = nodes[i].mData;
                    //pos = nodes[i].SceneTranslate();
                    this.mCenterX += node.X;//pos.Width;
                    this.mCenterY += node.Y;//pos.Height;
                }
                this.mCenterX /= nodes.Length;
                this.mCenterY /= nodes.Length;
            }
            // Compute new positions 
            //float[] newXs = this.NewXPositions;
            //float[] newYs = this.NewYPositions;
            for (i = 0; i < nodes.Length; i++)
            {
                node = nodes[i].mData;
                if (node.PositionFixed)
                {
                    //newXs[i] = nodes[i].X;
                    //newYs[i] = nodes[i].Y;
                    node.SetNewPosition(node.NewX, node.NewY);
                }
                else
                {
                    // TODO: make sure all the signs (+/-) are right
                    //pos = node.SceneTranslate();
                    dx = this.mCenterX - node.X;//pos.Width;
                    dy = this.mCenterY - node.Y;//pos.Height;
                    r = Math.Max(dx * dx + dy * dy, 0.000001);
                    // Magnetic Torque
                    force = Math.Atan2(dy, dx) - this.mAngles[nodes[i].Index];
                    force = this.mMagnetMult * Math.Pow(force, this.mAngleExp) / r;
                    fx = force * -dy;
                    fy = force * dx;
                    // Spring Force
                    r = Math.Sqrt(r);
                    force = this.mSpringMult * Math.Log(r / this.mRadius);
                    fx += force * dx / r;
                    fy += force * dy / r;
                    // Add force to position
                    node.SetNewPosition((float)(node.X + fx), 
                                        (float)(node.Y + fy));
                    //newXs[i] = (float)(node.X + fx);
                    //newYs[i] = (float)(node.Y + fy);
                }
            }
        }
    }
}
