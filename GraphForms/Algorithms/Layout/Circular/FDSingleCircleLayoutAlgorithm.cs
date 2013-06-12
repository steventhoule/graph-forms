using System;
using System.Collections.Generic;
using System.Text;
using GraphForms.Algorithms.Layout.ForceDirected;
using GraphForms.Algorithms.Path;

namespace GraphForms.Algorithms.Layout.Circular
{
    public class FDSingleCircleLayoutAlgorithm<Node, Edge>
        : ForceDirectedLayoutAlgorithm<Node, Edge, FDSingleCircleLayoutParameters>
        where Node : GraphElement, ILayoutNode
        where Edge : class, IGraphEdge<Node>, IUpdateable
    {
        private Digraph<Node, Edge>.GNode[] mEmbeddingCircle;

        private double mMinRadius;
        private double mFreeArc;

        private double mRadius;
        private double[] mAngles;

        private bool bCalcCenter;
        private double mCenterX;
        private double mCenterY;
        private double mSpringMult;
        private double mMagnetMult;
        private double mAngleExp;

        public FDSingleCircleLayoutAlgorithm(Digraph<Node, Edge> graph)
            : base(graph, null)
        {
        }

        public FDSingleCircleLayoutAlgorithm(Digraph<Node, Edge> graph,
            FDSingleCircleLayoutParameters oldParameters)
            : base(graph, oldParameters)
        {
        }

        Digraph<Node, Edge>.GNode[] CalculateEmbeddingCircle()
        {
            if (this.mGraph.EdgeCount == 0)
                return this.mGraph.InternalNodes;
            // Calculate an initial embedding circle based on
            // the longest path in the graph.
            DFLongestPath<Node, Edge> alg 
                = new DFLongestPath<Node, Edge>(this.mGraph, 
                    false, false);
            alg.Compute();
            int[] nis = alg.PathNodeIndexes;

            int j, count = this.mGraph.NodeCount;
            List<Digraph<Node, Edge>.GNode> nodes 
                = new List<Digraph<Node, Edge>.GNode>(count + 1);
            bool[] flags = new bool[count];
            // Here flags are used to determine which nodes in the graph
            // are not currently part of the embedding circle.
            for (j = 0; j < nis.Length; j++)
            {
                flags[nis[j]] = true;
                nodes.Add(this.mGraph.InternalNodeAt(nis[j]));
            }
            // Find all nodes not currently in the embedding circle
            Digraph<Node, Edge>.GNode[] remNodes 
                = new Digraph<Node, Edge>.GNode[count - nis.Length];
            count = 0;
            for (j = 0; j < flags.Length; j++)
            {
                if (!flags[j])
                {
                    remNodes[count] = this.mGraph.InternalNodeAt(j);
                    count++;
                }
            }
            // Here flags are used to determine which nodes are neighbors 
            // of the current node to be added to the embedding circle.
            Digraph<Node, Edge>.GEdge[] edges;
            bool placed;
            for (int k = 0; k < remNodes.Length; k++)
            {
                // Clear neighbor flags from previous iteration
                for (j = 0; j < flags.Length; j++)
                {
                    flags[j] = false;
                }
                // Set which nodes are neighbors to the current node
                count = 0;
                edges = remNodes[k].InternalDstEdges;
                for (j = 0; j < edges.Length; j++)
                {
                    flags[edges[j].mDstNode.Index] = true;
                }
                if (flags[remNodes[k].Index])
                {
                    flags[remNodes[k].Index] = false;
                    count++;
                }
                edges = remNodes[k].InternalSrcEdges;
                for (j = 0; j < edges.Length; j++)
                {
                    flags[edges[j].mSrcNode.Index] = true;
                }
                if (flags[remNodes[k].Index])
                {
                    flags[remNodes[k].Index] = false;
                    count++;
                }
                count = remNodes[k].AllEdgeCount - count;
                placed = false;
                // Look for two consecutive neighbors in the list
                if (count >= 2)
                {
                    for (j = 0; j < nodes.Count; j++)
                    {
                        if (flags[nodes[j].Index] &&
                            flags[nodes[(j + 1) % nodes.Count].Index])
                        {
                            nodes.Insert((j + 1) % nodes.Count, remNodes[k]);
                            placed = true;
                            break;
                        }
                    }
                }
                // Find any neighbor in the list
                if (!placed && count > 0)
                {
                    for (j = 0; j < nodes.Count; j++)
                    {
                        if (flags[nodes[j].Index])
                        {
                            nodes.Insert((j + 1) % nodes.Count, remNodes[k]);
                            placed = true;
                            break;
                        }
                    }
                }
                // Place all orphaned nodes at the end of the list
                if (!placed)
                    nodes.Add(remNodes[k]);
            }
            this.Swapping(nodes, 50);
            return nodes.ToArray();
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
                                if (x.Index == v.Index)
                                    continue;
                                // posX = (pos[x] - pos[u]) mod n
                                posX = (pos[x.Index] + offset) % n;
                                vEdges = v.InternalDstEdges;
                                for (m = 0; m < vEdges.Length; m++)
                                {
                                    y = vEdges[m].mDstNode;
                                    if (y.Index == u.Index || 
                                        y.Index == x.Index)
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
                                        y.Index == x.Index)
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
            int i;
            double perimeter, angle, a;
            double[] halfSize = new double[nodes.Length];

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
            this.mAngles = new double[nodes.Length];

            // precalculation
            angle = 0;
            for (i = 0; i < nodes.Length; i++)
            {
                angle += Math.Sin(halfSize[i] / this.mRadius) * 4;
            }

            //base.EndIteration(0, 0.5, "Precalculation done.");

            // recalculate radius
            this.mRadius = angle / (2 * Math.PI) * this.mRadius;

            // calculation
            angle = -Math.PI;
            for (i = 0; i < nodes.Length; i++)
            {
                a = Math.Sin(halfSize[i] / this.mRadius) * 2;
                angle += a;
                this.mAngles[nodes[i].Index] = angle;
                angle += a;
            }

            this.mRadius = Math.Max(this.mRadius, this.mMinRadius);

            //base.EndIteration(1, 1, "Calculation done.");
        }

        protected override bool OnBeginIteration(bool paramsDirty, 
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
                this.mEmbeddingCircle = this.CalculateEmbeddingCircle();
            }
            if (recalc || lastNodeCount != this.mGraph.NodeCount)
            {
                this.InitCircle();
            }
            return base.OnBeginIteration(paramsDirty, 
                lastNodeCount, lastEdgeCount);
        }

        protected override void PerformIteration(int iteration, 
            int maxIterations)
        {
            System.Drawing.SizeF pos;
            double dx, dy, r, force, fx, fy;
            Node[] nodes = this.mGraph.Nodes;
            int i;
            // Compute Center
            if (this.bCalcCenter)
            {
                this.mCenterX = 0;
                this.mCenterY = 0;
                for (i = 0; i < nodes.Length; i++)
                {
                    pos = nodes[i].SceneTranslate();
                    this.mCenterX += pos.Width;
                    this.mCenterY += pos.Height;
                }
                this.mCenterX /= nodes.Length;
                this.mCenterY /= nodes.Length;
            }
            // Compute new positions 
            float[] newXs = this.NewXPositions;
            float[] newYs = this.NewYPositions;
            for (i = 0; i < nodes.Length; i++)
            {
                if (nodes[i].PositionFixed)
                {
                    newXs[i] = nodes[i].X;
                    newYs[i] = nodes[i].Y;
                }
                else
                {
                    // TODO: make sure all the signs (+/-) are right
                    pos = nodes[i].SceneTranslate();
                    dx = this.mCenterX - pos.Width;
                    dy = this.mCenterY - pos.Height;
                    r = Math.Max(dx * dx + dy * dy, 0.000001);
                    // Magnetic Torque
                    force = Math.Atan2(dy, dx) - this.mAngles[i];
                    force = this.mMagnetMult * Math.Pow(force, this.mAngleExp) / r;
                    fx = force * -dy;
                    fy = force * dx;
                    // Spring Force
                    r = Math.Sqrt(r);
                    force = this.mSpringMult * Math.Log(r / this.mRadius);
                    fx += force * dx / r;
                    fy += force * dy / r;
                    // Add force to position
                    newXs[i] = (float)(nodes[i].X + fx);
                    newYs[i] = (float)(nodes[i].Y + fy);
                }
            }
        }
    }
}
