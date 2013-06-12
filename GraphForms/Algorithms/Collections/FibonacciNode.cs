using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Collections
{
    public class FibonacciNode<P, V>
    {
        private P mPriority;
        private V mValue;
        /// <summary>
        /// Determines if this node has had a child cut from it before.
        /// </summary>
        private bool Marked;
        /// <summary>
        /// Determines if this node has been removed from the heap.
        /// </summary>
        private bool Removed;
        /// <summary>
        /// Determines the depth/degree of this node.
        /// </summary>
        private int Deg;

        private FibonacciNode<P, V> Parent;
        private NodeList Children;
        private FibonacciNode<P, V> Next;
        private FibonacciNode<P, V> Prev;

        private FibonacciNode(P priority, V value)
        {
            this.mPriority = priority;
            this.mValue = value;
            this.Marked = false;
            this.Removed = false;
            this.Deg = 1;
            this.Parent = null;
            this.Children = new NodeList();
            this.Next = null;
            this.Prev = null;
        }

        public P Priority
        {
            get { return this.mPriority; }
        }

        public V Value
        {
            get { return this.mValue; }
        }

        public bool SetPriority(P priority)
        {
            if (this.Removed)
            {
                this.mPriority = priority;
                return true;
            }
            return false;
        }

        public KeyValuePair<P, V> ToKeyValuePair()
        {
            return new KeyValuePair<P, V>(this.mPriority, this.mValue);
        }

        private class NodeList
        {
            public FibonacciNode<P, V> First;
            public FibonacciNode<P, V> Last;

            public NodeList()
            {
                this.First = null;
                this.Last = null;
            }

            public int Count
            {
                get
                {
                    int count = 0;
                    FibonacciNode<P, V> node = this.First;
                    while (node != null)
                    {
                        count++;
                        node = node.Next;
                    }
                    return count;
                }
            }

            public void MergeLists(NodeList list)
            {
                if (list.First != null)
                {
                    if (this.Last != null)
                    {
                        this.Last.Next = list.First;
                    }
                    list.First.Prev = this.Last;
                    this.Last = list.Last;
                    if (this.First == null)
                    {
                        this.First = list.First;
                    }
                }
            }

            public void AddLast(FibonacciNode<P, V> node)
            {
                if (this.Last != null)
                {
                    this.Last.Next = node;
                }
                node.Prev = this.Last;
                this.Last = node;
                if (this.First == null)
                {
                    this.First = node;
                }
            }

            public void Remove(FibonacciNode<P, V> node)
            {
                if (node.Prev != null)
                {
                    node.Prev.Next = node.Next;
                }
                else if (this.First == node)
                {
                    this.First = node.Next;
                }

                if (node.Next != null)
                {
                    node.Next.Prev = node.Prev;
                }
                else if (this.Last == node)
                {
                    this.Last = node.Prev;
                }

                node.Next = null;
                node.Prev = null;
            }

            public int MaxDegree()
            {
                int max = int.MinValue;
                if (this.First == null)
                {
                    return 0;
                }
                FibonacciNode<P, V> node = this.First;
                while (node != null)
                {
                    max = Math.Max(max, node.Deg);
                    node = node.Next;
                }
                return max;
            }
        }

        public class Heap : IEnumerable<KeyValuePair<P, V>>
        {
            private readonly HeapDirection mDirection;
            private readonly IComparer<P> mComparer;
            /// <summary>
            /// Used to control the direction of the heap.
            /// Set to 1 if the Heap is increasing, -1 if it's decreasing.
            /// We use the approach to avoid unneccesary branches.
            /// </summary>
            private int mDirMult;

            private NodeList mNodes;
            private FibonacciNode<P, V> mNext;
            private int mCount;

            private FibonacciNode<P, V>[] mDegToNode;

            public Heap()
                : this(HeapDirection.Increasing, Comparer<P>.Default)
            {
            }

            public Heap(HeapDirection direction)
                : this(direction, Comparer<P>.Default)
            {
            }

            public Heap(HeapDirection direction, 
                IComparer<P> priorityComparer)
            {
                this.mDirection = direction;
                this.mComparer = priorityComparer;
                this.mDirMult 
                    = direction == HeapDirection.Increasing ? 1 : -1;

                this.mNodes = new NodeList();
                this.mNext = null;
                this.mCount = 0;

                this.mDegToNode = new FibonacciNode<P, V>[4];
            }

            public HeapDirection Direction
            {
                get { return this.mDirection; }
            }

            public IComparer<P> PriorityComparer
            {
                get { return this.mComparer; }
            }

            #region Internal Functions

            private void EnsureDegreeNodeStorage(int degree)
            {
                if (degree >= this.mDegToNode.Length)
                {
                    FibonacciNode<P, V>[] degreeToNode 
                        = new FibonacciNode<P, V>[degree + 1];
                    Array.Copy(this.mDegToNode, 0, degreeToNode, 0,
                        this.mDegToNode.Length);
                    this.mDegToNode = degreeToNode;
                }
            }

            /// <summary>
            /// Given two nodes, 
            /// adds the child node as a child of the parent node.
            /// </summary>
            /// <param name="parentNode"></param>
            /// <param name="childNode"></param>
            private void ReduceNodes(FibonacciNode<P, V> parentNode, 
                FibonacciNode<P, V> childNode)
            {
                this.mNodes.Remove(childNode);
                parentNode.Children.AddLast(childNode);
                childNode.Parent = parentNode;
                childNode.Marked = false;
                if (parentNode.Deg == childNode.Deg)
                {
                    parentNode.Deg += 1;
                    this.EnsureDegreeNodeStorage(parentNode.Deg);
                }
            }

            private void CompressHeap()
            {
                FibonacciNode<P, V> node = this.mNodes.First;
                FibonacciNode<P, V> nextNode, degreeNode;
                while (node != null)
                {
                    nextNode = node.Next;
                    degreeNode = this.mDegToNode[node.Deg];
                    while (degreeNode != null && degreeNode != node)
                    {
                        this.mDegToNode[node.Deg] = null;
                        if (this.mComparer.Compare(degreeNode.mPriority,
                            node.mPriority) * this.mDirMult <= 0)
                        {
                            if (node == nextNode)
                            {
                                nextNode = node.Next;
                            }
                            this.ReduceNodes(degreeNode, node);
                            node = degreeNode;
                        }
                        else
                        {
                            if (degreeNode == nextNode)
                            {
                                nextNode = degreeNode.Next;
                            }
                            this.ReduceNodes(node, degreeNode);
                        }
                        degreeNode = this.mDegToNode[node.Deg];
                    }
                    this.mDegToNode[node.Deg] = node;
                    node = nextNode;
                }
            }

            /// <summary>
            /// Updates the Next pointer, maintaining the heap
            /// by folding duplicate heap degrees into each other.
            /// Takes O(log(N)) time amortized.
            /// </summary>
            private void UpdateNext()
            {
                this.CompressHeap();
                FibonacciNode<P, V> node = this.mNodes.First;
                this.mNext = this.mNodes.First;
                while (node != null)
                {
                    if (this.mComparer.Compare(node.mPriority,
                        this.mNext.mPriority) * this.mDirMult < 0)
                    {
                        this.mNext = node;
                    }
                    node = node.Next;
                }
            }

            /// <summary>
            /// Updates the degree of a node, cascading to update the degree 
            /// of the parents if neccesary
            /// </summary>
            /// <param name="parentNode"></param>
            private void UpdateNodesDegree(FibonacciNode<P, V> parentNode)
            {
                int oldDegree = parentNode.Deg;
                parentNode.Deg = parentNode.Children.MaxDegree() + 1;
                FibonacciNode<P, V> degreeNode;
                if (oldDegree != parentNode.Deg)
                {
                    this.EnsureDegreeNodeStorage(parentNode.Deg);
                    degreeNode = this.mDegToNode[oldDegree];
                    if (degreeNode == parentNode)
                    {
                        this.mDegToNode[oldDegree] = null;
                    }
                    else if (parentNode.Parent != null)
                    {
                        this.UpdateNodesDegree(parentNode.Parent);
                    }
                }
            }
            #endregion

            #region Priority Queue Operations
            public bool IsEmpty
            {
                get { return this.mNodes.First == null; }
            }

            public FibonacciNode<P, V> Top
            {
                get { return this.mNext; }
            }

            public int Count
            {
                get { return this.mCount; }
            }

            public FibonacciNode<P, V> Enqueue(P priority, V value)
            {
                FibonacciNode<P, V> newNode 
                    = new FibonacciNode<P, V>(priority, value);
                //We don't do any book keeping or maintenance of the heap on Enqueue,
                //We just add this node to the end of the list of Heaps, updating the Next if required
                this.mNodes.AddLast(newNode);
                if (this.mNext == null || this.mComparer.Compare(priority, 
                    this.mNext.mPriority) * this.mDirMult < 0)
                {
                    this.mNext = newNode;
                }
                this.mCount++;
                return newNode;
            }

            public KeyValuePair<P, V> Peek()
            {
                if (this.mCount == 0)
                    throw new InvalidOperationException();
                return this.mNext.ToKeyValuePair();
            }

            public KeyValuePair<P, V> Dequeue()
            {
                if (this.mCount == 0)
                    throw new InvalidOperationException();

                KeyValuePair<P, V> result = this.mNext.ToKeyValuePair();

                this.mNodes.Remove(this.mNext);
                this.mNext.Parent = null;
                this.mNext.Next = null;
                this.mNext.Prev = null;
                this.mNext.Removed = true;
                FibonacciNode<P, V> node = this.mDegToNode[this.mNext.Deg];
                if (node == this.mNext)
                {
                    this.mDegToNode[this.mNext.Deg] = null;
                }

                node = this.mNext.Children.First;
                while (node != null)
                {
                    node.Parent = null;
                    node = node.Next;
                }
                this.mNodes.MergeLists(this.mNext.Children);
                this.mNext.Children = null;
                this.mCount--;
                this.UpdateNext();

                return result;
            }

            public void Delete(FibonacciNode<P, V> node)
            {
                if (node == null)
                {
                    throw new ArgumentNullException("node");
                }
                this.ChangeKeyInternal(node, default(P), true);
                this.Dequeue();
            }

            public void ChangePriority(FibonacciNode<P, V> node, P priority)
            {
                if (node == null)
                {
                    throw new ArgumentNullException("node");
                }
                this.ChangeKeyInternal(node, priority, false);
            }

            private void ChangeKeyInternal(
                FibonacciNode<P, V> node, P nk, bool delete)
            {
                int delta = Math.Sign(
                    this.mComparer.Compare(node.mPriority, nk));
                if ((delta == this.mDirMult || delete) && delta != 0)
                {
                    //New value is in the same direction as the heap
                    node.mPriority = nk;
                    FibonacciNode<P, V> parent = node.Parent;
                    if (parent != null && (this.mComparer.Compare(nk, 
                        parent.mPriority) * this.mDirMult < 0 || delete))
                    {
                        node.Marked = false;
                        parent.Children.Remove(node);
                        this.UpdateNodesDegree(parent);
                        node.Parent = null;
                        this.mNodes.AddLast(node);
                        //This loop is the cascading cut, we continue to cut
                        //ancestors of the node reduced until we hit a root 
                        //or we find an unmarked ancestor
                        FibonacciNode<P, V> temp;
                        while (parent.Marked && parent.Parent != null)
                        {
                            parent.Parent.Children.Remove(parent);
                            this.UpdateNodesDegree(parent);
                            parent.Marked = false;
                            this.mNodes.AddLast(parent);
                            temp = parent;
                            parent = parent.Parent;
                            temp.Parent = null;
                        }
                        if (parent.Parent != null)
                        {
                            //We mark this node to note that it's 
                            //had a child cut from it before
                            parent.Marked = true;
                        }
                    }
                    //Update next
                    if (delete || this.mComparer.Compare(nk,
                        this.mNext.mPriority) * this.mDirMult < 0)
                    {
                        this.mNext = node;
                    }
                }
                else if (delta != 0)
                {
                    //New value is in opposite direction of Heap, 
                    //cut all children violating heap condition
                    node.mPriority = nk;
                    if (node.Children != null)
                    {
                        List<FibonacciNode<P, V>> toUpdate 
                            = new List<FibonacciNode<P, V>>(this.mCount);
                        FibonacciNode<P, V> child = node.Children.First;
                        while (child != null)
                        {
                            if (this.mComparer.Compare(node.mPriority, 
                                child.mPriority) * this.mDirMult > 0)
                            {
                                toUpdate.Add(child);
                            }
                            child = child.Next;
                        }

                        if (toUpdate.Count > 0)
                        {
                            node.Marked = true;
                            for (int i = 0; i < toUpdate.Count; i++)
                            {
                                child = toUpdate[i];
                                //node.Marked = true;
                                node.Children.Remove(child);
                                child.Parent = null;
                                child.Marked = false;
                                this.mNodes.AddLast(child);
                                //this.UpdateNodesDegree(node);
                            }
                            this.UpdateNodesDegree(node);
                        }
                    }
                    this.UpdateNext();
                }
            }

            public void Merge(Heap other)
            {
                if (other.mDirection != this.mDirection)
                {
                    throw new Exception("Error: " + 
                        "Heaps must go in the same direction when merging");
                }
                this.mNodes.MergeLists(other.mNodes);
                if (this.mComparer.Compare(other.mNext.mPriority, 
                    this.mNext.mPriority) * this.mDirMult < 0)
                {
                    this.mNext = other.mNext;
                }
                this.mCount += other.mCount;
            }
            #endregion

            #region Iteration and Enumeration
            private Heap CreateClone()
            {
                Heap clone = new Heap(this.mDirection, this.mComparer);
                Stack<FibonacciNode<P, V>> nodeStack 
                    = new Stack<FibonacciNode<P, V>>(this.mCount);
                FibonacciNode<P, V> node = this.mNodes.First;
                while (node != null)
                {
                    nodeStack.Push(node);
                    node = node.Next;
                }
                FibonacciNode<P, V> topNode;
                while (nodeStack.Count > 0)
                {
                    topNode = nodeStack.Pop();
                    clone.Enqueue(topNode.mPriority, topNode.mValue);
                    node = topNode.Children.First;
                    while (node != null)
                    {
                        nodeStack.Push(node);
                        node = node.Next;
                    }
                }
                return clone;
            }

            public KeyValuePair<P, V>[] ToArray()
            {
                Heap clone = this.CreateClone();
                int count = clone.mCount;
                KeyValuePair<P, V>[] nodes = new KeyValuePair<P, V>[count];
                for (int i = 0; i < count; i++)
                {
                    nodes[i] = clone.Dequeue();
                }
                return nodes;
            }

            public KeyValuePair<P, V>[] DestroyToArray()
            {
                int count = this.mCount;
                KeyValuePair<P, V>[] nodes = new KeyValuePair<P, V>[count];
                for (int i = 0; i < count; i++)
                {
                    nodes[i] = this.Dequeue();
                }
                return nodes;
            }

            public class Enumerator : IEnumerator<KeyValuePair<P, V>>
            {
                private readonly Heap mHeap;
                private readonly bool bIsDestructive;
                private KeyValuePair<P, V> mCurrent;

                private Enumerator(Heap heap,
                    bool isDestructive)
                {
                    this.mHeap = heap;
                    this.bIsDestructive = isDestructive;
                    this.mCurrent = default(KeyValuePair<P, V>);
                }

                public static Enumerator Create(Heap heap)
                {
                    return new Enumerator(heap.CreateClone(), false);
                }

                public static Enumerator CreateDestructive(Heap heap)
                {
                    return new Enumerator(heap, true);
                }

                public KeyValuePair<P, V> Current
                {
                    get { return this.mCurrent; }
                }

                public P Priority
                {
                    get { return this.mCurrent.Key; }
                }

                public V Value
                {
                    get { return this.mCurrent.Value; }
                }

                public void Dispose()
                {
                    if (!this.bIsDestructive)
                    {
                        while (this.mHeap.mCount > 0)
                        {
                            this.mHeap.Dequeue();
                        }
                        this.mCurrent = default(KeyValuePair<P, V>);
                    }
                }

                object System.Collections.IEnumerator.Current
                {
                    get { return this.mCurrent; }
                }

                public bool MoveNext()
                {
                    if (this.mHeap.mCount > 0)
                    {
                        this.mCurrent = this.mHeap.Dequeue();
                        return true;
                    }
                    this.mCurrent = default(KeyValuePair<P, V>);
                    return false;
                }

                public void Reset()
                {
                    throw new NotSupportedException();
                }
            }

            public Enumerator GetEnumerator()
            {
                return Enumerator.Create(this);
            }

            public Enumerator GetDestructiveEnumerator()
            {
                return Enumerator.CreateDestructive(this);
            }

            IEnumerator<KeyValuePair<P, V>> 
                IEnumerable<KeyValuePair<P, V>>.GetEnumerator()
            {
                return Enumerator.Create(this);
            }

            System.Collections.IEnumerator 
                System.Collections.IEnumerable.GetEnumerator()
            {
                return Enumerator.Create(this);
            }
            #endregion

            #region String Functions
            private class NodeLevel
            {
                public readonly FibonacciNode<P, V> Node;
                public readonly int Level;
                public NodeLevel(FibonacciNode<P, V> node, int level)
                {
                    this.Node = node;
                    this.Level = level;
                }
            }

            public string DrawHeap()
            {
                List<string> lines = new List<string>();
                int lineNum = 0;
                int columnPosition = 0;
                Stack<NodeLevel> stack = new Stack<NodeLevel>();
                FibonacciNode<P, V> node = this.mNodes.Last;
                while (node != null)
                {
                    stack.Push(new NodeLevel(node, 0));
                    node = node.Prev;
                }
                NodeLevel currentCell;
                string currentLine, nodeString;
                while (stack.Count > 0)
                {
                    currentCell = stack.Pop();
                    lineNum = currentCell.Level;
                    if (lines.Count <= lineNum)
                        lines.Add(string.Empty);
                    currentLine = lines[lineNum];
                    currentLine = currentLine.PadRight(columnPosition, ' ');
                    nodeString = currentCell.Node.mPriority.ToString()
                        + (currentCell.Node.Marked ? "*" : "") + " ";
                    currentLine += nodeString;
                    if (currentCell.Node.Children != null &&
                        currentCell.Node.Children.Last != null)
                    {
                        node = currentCell.Node.Children.Last;
                        while (node != null)
                        {
                            stack.Push(new NodeLevel(node, 
                                currentCell.Level + 1));
                            node = node.Prev;
                        }
                    }
                    else
                    {
                        columnPosition += nodeString.Length;
                    }
                    lines[lineNum] = currentLine;
                }
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < lines.Count; i++)
                    builder.AppendLine(lines[i]);
                return builder.ToString();
            }
            #endregion
        }
    }
}
