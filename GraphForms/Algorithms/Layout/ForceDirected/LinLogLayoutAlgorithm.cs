﻿using System;

namespace GraphForms.Algorithms.Layout.ForceDirected
{
    // Copied from Graph#, which was copied from this:
    // https://code.google.com/p/linloglayout/source/browse/trunk/src/MinimizerBarnesHut.java
    public partial class LinLogLayoutAlgorithm<Node, Edge>
        : LayoutAlgorithm<Node, Edge>
        where Node : class, ILayoutNode
        where Edge : IGraphEdge<Node>, IUpdateable
    {
        private class LinLogNode
        {
            public int Index;
            public Digraph<Node, Edge>.GNode OriginalNode;
            public LinLogEdge[] Attractions;
            public float RepulsionWeight;
            public Vec2F Position;
        }

        private class LinLogEdge
        {
            public LinLogNode Target;
            public double AttractionWeight;
        }

        // Parameters
        private float mFinalAttrExponent = 1;
        private float mFinalRepuExponent = 0;
        private float mGravMult = 0.1f;

        private LinLogNode[] mNodes;
        private Vec2F mBarycenter;
        private double mRepulsionMultiplier;

        // These change every iteration as graph cools down
        private double mRepuExponent;
        private double mAttrExponent;

        private bool bDirty = true;

        public LinLogLayoutAlgorithm(Digraph<Node, Edge> graph,
            IClusterNode clusterNode)
            : base(graph, clusterNode)
        {
            this.MaxIterations = 100;
        }

        public LinLogLayoutAlgorithm(Digraph<Node, Edge> graph,
            Box2F boundingBox)
            : base(graph, boundingBox)
        {
            this.MaxIterations = 100;
        }

        public float AttractionExponent
        {
            get { return this.mFinalAttrExponent; }
            set
            {
                if (this.mFinalAttrExponent != value)
                {
                    this.mFinalAttrExponent = value;
                }
            }
        }

        public float RepulsiveExponent
        {
            get { return this.mFinalRepuExponent; }
            set
            {
                if (this.mFinalRepuExponent != value)
                {
                    this.mFinalRepuExponent = value;
                }
            }
        }

        public float GravitationMultiplier
        {
            get { return this.mGravMult; }
            set
            {
                if (this.mGravMult != value)
                {
                    this.mGravMult = value;
                    this.bDirty = true;
                }
            }
        }

        protected override void InitializeAlgorithm()
        {
            //base.InitializeAlgorithm();
            this.mRepuExponent = this.mFinalRepuExponent;
            this.mAttrExponent = this.mFinalAttrExponent;
        }

        protected override void PerformPrecalculations(
            uint lastNodeVersion, uint lastEdgeVersion)
        {
            bool nodesDirty = false;
            if (this.mGraph.NodeVersion != lastNodeVersion ||
                this.mGraph.EdgeVersion != lastEdgeVersion)
            {
                this.InitAlgorithm();
                nodesDirty = true;
            }
            if (this.bDirty || nodesDirty)
            {
                LinLogNode n;
                for (int i = 0; i < this.mNodes.Length; i++)
                {
                    n = this.mNodes[i];
                    n.RepulsionWeight = Math.Max(n.RepulsionWeight, this.mGravMult);
                }
                this.mRepulsionMultiplier = this.ComputeRepulsionMultiplier();
            }
            this.bDirty = false;
        }

        protected override void PerformIteration(uint iteration)//, int maxIterations)
        {
            LinLogNode n;
            int i;
            for (i = 0; i < this.mNodes.Length; i++)
            {
                n = this.mNodes[i];
                if (n != null)
                {
                    //n.Position = n.OriginalNode.Position;
                    n.Position = new Vec2F(n.OriginalNode.Data.X, n.OriginalNode.Data.Y);
                }
            }

            this.ComputeBarycenter();
            QuadTree quadTree = this.BuildQuadTree();

            // cooling function definition
            if (this.MaxIterations >= 50 && this.mFinalRepuExponent < 1.0)
            {
                this.mAttrExponent = this.mFinalAttrExponent;
                this.mRepuExponent = this.mFinalRepuExponent;
                if (iteration <= 0.6 * this.MaxIterations)
                {
                    // use energy model with few local minima
                    this.mAttrExponent += 1.1 * (1.0 - this.mFinalAttrExponent);
                    this.mRepuExponent += 0.9 * (1.0 - this.mFinalRepuExponent);
                }
                else if (iteration <= 0.9 * this.MaxIterations)
                {
                    // gradually move to final energy model
                    this.mAttrExponent += 1.1 * (1.0 - this.mFinalAttrExponent)
                        * (0.9 - iteration / (double)this.MaxIterations) / 0.3;
                    this.mRepuExponent += 0.9 * (1.0 - this.mFinalRepuExponent)
                        * (0.9 - iteration / (double)this.MaxIterations) / 0.3;
                }
            }

            // Move each node
            Vec2F bestDir, oldPos;
            double oldEnergy, bestEnergy, curEnergy;
            int multiple, bestMultiple;
            for (i = 0; i < this.mNodes.Length; i++)
            {
                n = this.mNodes[i];
                if (n == null || n.OriginalNode.Data.PositionFixed)
                    continue;
                oldEnergy = this.GetEnergy(i, quadTree);

                // compute direction of the move of the node
                bestDir = this.GetDirection(i, quadTree);

                // line search: compute length of the move
                oldPos = n.Position;

                bestEnergy = oldEnergy;
                bestMultiple = 0;
                bestDir.X = bestDir.X / 32;
                bestDir.Y = bestDir.Y / 32;
                // small movements, in the best case definition
                for (multiple = 32; multiple >= 1 &&
                    (bestMultiple == 0 || bestMultiple / 2 == multiple);
                    multiple /= 2)
                {
                    n.Position.X = oldPos.X + bestDir.X * multiple;
                    n.Position.Y = oldPos.Y + bestDir.Y * multiple;
                    curEnergy = this.GetEnergy(i, quadTree);
                    if (curEnergy < bestEnergy)
                    {
                        bestEnergy = curEnergy;
                        bestMultiple = multiple;
                    }
                }

                // if there is a large movement on the right solution?
                for (multiple = 64; multiple <= 128 &&
                    bestMultiple == multiple / 2; multiple *= 2)
                {
                    n.Position.X = oldPos.X + bestDir.X * multiple;
                    n.Position.Y = oldPos.Y + bestDir.Y * multiple;
                    curEnergy = this.GetEnergy(i, quadTree);
                    if (curEnergy < bestEnergy)
                    {
                        bestEnergy = curEnergy;
                        bestMultiple = multiple;
                    }
                }

                // best solution to move
                n.Position.X = oldPos.X + bestDir.X * bestMultiple;
                n.Position.Y = oldPos.Y + bestDir.Y * bestMultiple;
                if (bestMultiple > 0)
                {
                    quadTree.MoveNode(oldPos, n.Position, n.RepulsionWeight);
                }
            }

            // copy positions
            for (i = 0; i < this.mNodes.Length; i++)
            {
                n = this.mNodes[i];
                if (n != null)
                {
                    n.OriginalNode.Data.SetPosition(n.Position.X, n.Position.Y);//SetNewPosition(n.Position.X, n.Position.Y);
                    if (float.IsNaN(n.Position.X))
                        throw new Exception();
                }
            }
        }

        private Vec2F GetDirection(int index, QuadTree quadTree)
        {
            double[] dir = new double[] { 0, 0 };

            double dir2 = AddRepulsionDirection(index, quadTree, dir);
            dir2 += AddAttractionDirection(index, dir);
            dir2 += AddGravitationDirection(index, dir);

            if (dir2 != 0.0)
            {
                dir[0] = dir[0] / dir2;
                dir[1] = dir[1] / dir2;

                double length = Math.Max(Math.Sqrt(dir[0] * dir[0] + dir[1] * dir[1]), 0.000001);
                if (length > quadTree.Width / 8)
                {
                    length /= quadTree.Width / 8;
                    dir[0] = dir[0] / length;
                    dir[1] = dir[1] / length;
                }
                return new Vec2F((float)dir[0], (float)dir[1]);
            }
            return new Vec2F(0, 0);
        }

        private double AddGravitationDirection(int index, double[] dir)
        {
            LinLogNode n = this.mNodes[index];
            double gravX = this.mBarycenter.X - n.Position.X;
            double gravY = this.mBarycenter.Y - n.Position.Y;
            double dist = Math.Max(Math.Sqrt(gravX * gravX + gravY * gravY), 0.000001);
            double tmp = this.mGravMult * this.mRepulsionMultiplier *
                Math.Max(n.RepulsionWeight, 1) * Math.Pow(dist, this.mAttrExponent - 2);
            dir[0] = dir[0] + gravX * tmp;
            dir[1] = dir[1] + gravY * tmp;

            return tmp * Math.Abs(this.mAttrExponent - 1);
        }

        private double AddAttractionDirection(int index, double[] dir)
        {
            double attrX, attrY, dist, tmp, dir2 = 0.0;
            LinLogNode n = this.mNodes[index];
            LinLogEdge e;
            for (int i = 0; i < n.Attractions.Length; i++)
            {
                e = n.Attractions[i];
                // leave loop
                if (e.Target == n)
                    continue;

                attrX = e.Target.Position.X - n.Position.X;
                attrY = e.Target.Position.Y - n.Position.Y;
                dist = Math.Sqrt(attrX * attrX + attrY * attrY);
                if (dist <= 0)
                    continue;

                tmp = e.AttractionWeight * Math.Pow(dist, this.mAttrExponent - 2);
                dir2 += tmp * Math.Abs(this.mAttrExponent - 1);

                dir[0] = dir[0] + attrX * tmp;
                dir[1] = dir[1] + attrY * tmp;
            }
            return dir2;
        }

        /// <summary>
        /// Calculates the force the node at <paramref name="index"/> can apply
        /// to the numbered points using <paramref name="quadTree"/>.
        /// </summary>
        /// <param name="index">The node number to which you want to calculate the 
        /// repulsion strength.</param>
        /// <param name="quadTree"></param>
        /// <param name="dir">The repulsive strength is added to this vector.</param>
        /// <returns>Repulsion estimate of the second derivative of the energy.</returns>
        private double AddRepulsionDirection(int index, QuadTree quadTree, double[] dir)
        {
            LinLogNode n = this.mNodes[index];

            if (quadTree == null || quadTree.Index == index || n.RepulsionWeight <= 0)
                return 0.0;

            double repuX = quadTree.Position.X - n.Position.X;
            double repuY = quadTree.Position.Y - n.Position.Y;
            double dist = Math.Sqrt(repuX * repuX + repuY * repuY);
            if (quadTree.Index < 0 && dist < 2.0 * quadTree.Width)
            {
                double dir2 = 0.0;
                for (int i = 0; i < quadTree.Children.Length; i++)
                    dir2 += AddRepulsionDirection(index, quadTree.Children[i], dir);
                return dir2;
            }

            if (dist != 0.0)
            {
                double tmp = this.mRepulsionMultiplier * n.RepulsionWeight * quadTree.Weight
                    * Math.Pow(dist, this.mRepuExponent - 2);
                dir[0] = dir[0] - repuX * tmp;
                dir[1] = dir[1] - repuY * tmp;
                return tmp * Math.Abs(this.mRepuExponent - 1);
            }

            return 0.0;
        }

        private double GetEnergySum(QuadTree q)
        {
            double sum = 0;
            for (int i = 0; i < this.mNodes.Length; i++)
                if (this.mNodes[i] != null)
                    sum += GetEnergy(i, q);
            return sum;
        }

        private double GetEnergy(int index, QuadTree q)
        {
            return GetRepulsionEnergy(index, q)
                + GetAttractionEnergy(index) + GetGravitationEnergy(index);
        }

        private double GetGravitationEnergy(int index)
        {
            LinLogNode n = this.mNodes[index];

            double dx = n.Position.X - this.mBarycenter.X;
            double dy = n.Position.Y - this.mBarycenter.Y;
            double dist = Math.Sqrt(dx * dx + dy * dy);
            return this.mGravMult * this.mRepulsionMultiplier *
                Math.Max(n.RepulsionWeight, 1) * Math.Pow(dist, this.mAttrExponent) / 
                this.mAttrExponent;
        }

        private double GetAttractionEnergy(int index)
        {
            double dx, dy, dist, energy = 0.0;
            LinLogNode n = this.mNodes[index];
            LinLogEdge e;
            for (int i = 0; i < n.Attractions.Length; i++)
            {
                e = n.Attractions[i];
                if (e.Target == n)
                    continue;
                dx = e.Target.Position.X - n.Position.X;
                dy = e.Target.Position.Y - n.Position.Y;
                dist = Math.Sqrt(dx * dx + dy * dy);
                energy += e.AttractionWeight * Math.Pow(dist, this.mAttrExponent) / 
                    this.mAttrExponent;
            }
            return energy;
        }

        private double GetRepulsionEnergy(int index, QuadTree tree)
        {
            if (tree == null || tree.Index == index || index >= this.mNodes.Length)
                return 0.0;

            LinLogNode n = this.mNodes[index];
            double dx = n.Position.X - tree.Position.X;
            double dy = n.Position.Y - tree.Position.Y;
            double dist = Math.Max(Math.Sqrt(dx * dx + dy * dy), 0.000001);
            if (tree.Index < 0 && dist < (2 * tree.Width))
            {
                double energy = 0.0;
                for (int i = 0; i < tree.Children.Length; i++)
                {
                    energy += GetRepulsionEnergy(index, tree.Children[i]);
                }
                return energy;
            }

            if (this.mRepuExponent == 0)
                return -this.mRepulsionMultiplier * n.RepulsionWeight * tree.Weight * Math.Log(dist);

            return -this.mRepulsionMultiplier * n.RepulsionWeight * tree.Weight *
                Math.Pow(dist, this.mRepuExponent) / this.mRepuExponent;
        }

        private void InitAlgorithm()
        {
            Digraph<Node, Edge>.GEdge edge;
            int eCount = this.mGraph.EdgeCount;

            Digraph<Node, Edge>.GNode node;
            int nCount = this.mGraph.NodeCount;

            this.mNodes = new LinLogNode[nCount];

            LinLogEdge e;
            LinLogNode n;
            float weight;
            int i, j, attrIndex;
            // nodes indexed
            for (i = 0; i < nCount; i++)
            {
                node = this.mGraph.InternalNodeAt(i);
                if (!node.Hidden)
                {
                    n = new LinLogNode();
                    n.Index = i;
                    n.OriginalNode = node;
                    n.Attractions = new LinLogEdge[node.TotalEdgeCount(true)];
                    n.RepulsionWeight = 0;
                    //n.Position = n.OriginalNode.Position;
                    this.mNodes[i] = n;
                }
            }
            for (i = 0; i < this.mNodes.Length; i++)
            {
                node = this.mGraph.InternalNodeAt(i);
                if (node.Hidden)
                    continue;
                n = this.mNodes[i];
                // each node builds an attractionWeights, attractionIndexes,
                // and repulsionWeights structure 
                // and copies the position of the node
                attrIndex = 0;
                for (j = 0; j < eCount; j++)
                {
                    edge = this.mGraph.InternalEdgeAt(j);
                    e = null;
                    if (edge.SrcNode.Index == node.Index)
                    {
                        e = new LinLogEdge();
                        e.Target = this.mNodes[edge.DstNode.Index];
                        if (e.Target == null)
                            e = null;
                    }
                    else if (edge.DstNode.Index == node.Index)
                    {
                        e = new LinLogEdge();
                        e.Target = this.mNodes[edge.SrcNode.Index];
                        if (e.Target == null)
                            e = null;
                    }
                    if (e != null)
                    {
                        weight = edge.Data.Weight;
                        e.AttractionWeight = weight;
                        n.Attractions[attrIndex++] = e;
                        // TODO: look at this line below
                        //n.RepulsionWeight += weight;
                        n.RepulsionWeight += 1;
                    }
                }
                if (attrIndex < n.Attractions.Length)
                {
                    LinLogEdge[] attrs = new LinLogEdge[attrIndex];
                    if (attrIndex > 0)
                        Array.Copy(n.Attractions, 0, attrs, 0, attrIndex);
                    n.Attractions = attrs;
                }
            }
        }

        private void ComputeBarycenter()
        {
            this.mBarycenter = new Vec2F(0, 0);
            double baryX = 0.0, baryY = 0.0;
            double repWeightSum = 0.0;
            LinLogNode n;
            for (int i = 0; i < this.mNodes.Length; i++)
            {
                n = this.mNodes[i];
                if (n != null)
                {
                    repWeightSum += n.RepulsionWeight;
                    baryX += n.Position.X * n.RepulsionWeight;
                    baryY += n.Position.Y * n.RepulsionWeight;
                }
            }
            if (repWeightSum > 0.0)
            {
                this.mBarycenter.X = (float)(baryX / repWeightSum);
                this.mBarycenter.Y = (float)(baryY / repWeightSum);
            }
            else
            {
                this.mBarycenter.X = (float)baryX;
                this.mBarycenter.Y = (float)baryY;
            }
        }

        private double ComputeRepulsionMultiplier()
        {
            double attractionSum = 0;
            double repulsionSum = 0;
            LinLogEdge[] attractions;
            int i, j;
            for (i = 0; i < this.mNodes.Length; i++)
            {
                if (this.mNodes[i] != null)
                {
                    attractions = this.mNodes[i].Attractions;
                    for (j = 0; j < attractions.Length; j++)
                    {
                        attractionSum += attractions[j].AttractionWeight;
                    }
                    repulsionSum += this.mNodes[i].RepulsionWeight;
                }
            }

            if (repulsionSum > 0 && attractionSum > 0)
                return attractionSum * Math.Pow(repulsionSum,
                    0.5 * (this.mAttrExponent - this.mRepuExponent) - 2);
            return 1;
        }

        /// <summary>
        /// To build a Quadtree (like the OctTree only in 2D).
        /// </summary>
        /// <returns></returns>
        private QuadTree BuildQuadTree()
        {
            // Calculation of the minimum and maximum positions
            Vec2F minPos = new Vec2F(float.MaxValue, float.MaxValue);
            Vec2F maxPos = new Vec2F(-float.MaxValue, -float.MaxValue);

            LinLogNode n;
            int i, count = this.mNodes.Length;
            for (i = 0; i < count; i++)
            {
                n = this.mNodes[i];
                if (n != null && n.RepulsionWeight > 0)
                {
                    minPos.X = Math.Min(minPos.X, n.Position.X);
                    minPos.Y = Math.Min(minPos.Y, n.Position.Y);
                    maxPos.X = Math.Max(maxPos.X, n.Position.X);
                    maxPos.Y = Math.Max(maxPos.Y, n.Position.Y);
                }
            }

            // Add nodes with a non-zero RepulsionWeight to the QuadTree
            QuadTree result = null;
            for (i = 0; i < count; i++)
            {
                n = this.mNodes[i];
                if (n != null && n.RepulsionWeight > 0)
                {
                    if (result == null)
                        result = new QuadTree(n.Index, n.Position, n.RepulsionWeight, minPos, maxPos);
                    else
                        result.AddNode(n.Index, n.Position, n.RepulsionWeight, 0);
                }
            }
            return result;
        }
    }
}
