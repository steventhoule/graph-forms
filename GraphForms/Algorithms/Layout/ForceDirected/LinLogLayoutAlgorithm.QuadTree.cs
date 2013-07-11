using System;

namespace GraphForms.Algorithms.Layout.ForceDirected
{
	public partial class LinLogLayoutAlgorithm<Node, Edge>
        where Node : class, ILayoutNode
        where Edge : IGraphEdge<Node>, IUpdateable
	{
        private class QuadTree
        {
            #region Properties
            private readonly QuadTree[] mChildren = new QuadTree[4];
            public QuadTree[] Children
            {
                get { return this.mChildren; }
            }

            private int mIndex;
            public int Index
            {
                get { return this.mIndex; }
            }

            private Vec2F mPosition;

            public Vec2F Position
            {
                get { return this.mPosition; }
            }

            private float mWeight;

            public float Weight
            {
                get { return this.mWeight; }
            }

            private Vec2F mMinPos;
            private Vec2F mMaxPos;

            #endregion

            public double Width
            {
                get
                {
                    return Math.Max(this.mMaxPos.X - this.mMinPos.X, 
                                    this.mMaxPos.Y - this.mMinPos.Y);
                }
            }

            protected const int kMaxDepth = 20;

            public QuadTree(int index, Vec2F position, float weight, 
                Vec2F minPos, Vec2F maxPos)
            {
                this.mIndex = index;
                this.mPosition = position;
                this.mWeight = weight;
                this.mMinPos = minPos;
                this.mMaxPos = maxPos;
            }

            public void AddNode(int nodeIndex, Vec2F nodePos, float nodeWeight, int depth)
            {
                if (depth > kMaxDepth)
                    return;

                if (this.mIndex >= 0)
                {
                    AddNode2(this.mIndex, this.mPosition, this.mWeight, depth);
                    mIndex = -1;
                }

                this.mPosition.X = (this.mPosition.X * this.mWeight + nodePos.X * nodeWeight) / (this.mWeight + nodeWeight);
                this.mPosition.Y = (this.mPosition.Y * this.mWeight + nodePos.Y * nodeWeight) / (this.mWeight + nodeWeight);
                this.mWeight += nodeWeight;

                AddNode2(nodeIndex, nodePos, nodeWeight, depth);
            }

            protected void AddNode2(int nodeIndex, Vec2F nodePos, float nodeWeight, int depth)
            {
                //Debug.WriteLine( string.Format( "AddNode2 {0} {1} {2} {3}", nodeIndex, nodePos, nodeWeight, depth ) );
                int childIndex = 0;
                float middleX = (mMinPos.X + mMaxPos.X) / 2;
                float middleY = (mMinPos.Y + mMaxPos.Y) / 2;

                if (nodePos.X > middleX)
                    childIndex += 1;

                if (nodePos.Y > middleY)
                    childIndex += 2;

                //Debug.WriteLine( string.Format( "childIndex: {0}", childIndex ) );               


                if (this.mChildren[childIndex] == null)
                {
                    var newMin = new Vec2F(0, 0);
                    var newMax = new Vec2F(0, 0);
                    if (nodePos.X <= middleX)
                    {
                        newMin.X = this.mMinPos.X;
                        newMax.X = middleX;
                    }
                    else
                    {
                        newMin.X = middleX;
                        newMax.X = this.mMaxPos.X;
                    }
                    if (nodePos.Y <= middleY)
                    {
                        newMin.Y = this.mMinPos.Y;
                        newMax.Y = middleY;
                    }
                    else
                    {
                        newMin.Y = middleY;
                        newMax.Y = this.mMaxPos.Y;
                    }
                    this.mChildren[childIndex] = new QuadTree(nodeIndex, nodePos, nodeWeight, newMin, newMax);
                }
                else
                {
                    this.mChildren[childIndex].AddNode(nodeIndex, nodePos, nodeWeight, depth + 1);
                }
            }

            /// <summary>
            /// The position of the sub recalculated minus the moved node part.
            /// </summary>
            /// <param name="oldPos"></param>
            /// <param name="newPos"></param>
            /// <param name="nodeWeight"></param>
            public void MoveNode(Vec2F oldPos, Vec2F newPos, float nodeWeight)
            {
                this.mPosition.X += ((newPos.X - oldPos.X) * (nodeWeight / this.mWeight));
                this.mPosition.Y += ((newPos.Y - oldPos.Y) * (nodeWeight / this.mWeight));

                int childIndex = 0;
                double middleX = (this.mMinPos.X + this.mMaxPos.X) / 2;
                double middleY = (this.mMinPos.Y + this.mMaxPos.Y) / 2;

                if (oldPos.X > middleX)
                    childIndex += 1;
                if (oldPos.Y > middleY)
                    childIndex += 1 << 1;

                if (this.mChildren[childIndex] != null)
                    this.mChildren[childIndex].MoveNode(oldPos, newPos, nodeWeight);
            }
        }
	}
}
