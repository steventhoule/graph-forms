using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Collections
{
    /// <summary>
    /// This data structure represents a single element in a Fibonacci Heap,
    /// an efficient implementation of a priority queue. It stores a value 
    /// along with a "priority" that determines the order in which it is 
    /// removed from its heap relative to other elements in its heap. 
    /// </summary>
    /// <typeparam name="P">A comparable type used to represent the 
    /// "priorities" of elements which determine the order in which they
    /// are removed from their Fibonacci Heap.</typeparam>
    /// <typeparam name="V">A type used to represent the values stored in
    /// each element in a Fibonacci Heap.</typeparam>
    /// <remarks>
    /// For more information on Fibonacci Heaps, see the Wikipedia article:
    /// http://en.wikipedia.org/wiki/Fibonacci_heap
    /// </remarks>
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

        /// <summary>
        /// The "priority" of this element, which determines the order in
        /// which it is removed from its Fibonacci Heap in relation to
        /// other elements in its Fibonacci Heap.
        /// </summary>
        public P Priority
        {
            get { return this.mPriority; }
        }
        /// <summary>
        /// Gets the value stored in this element.
        /// </summary>
        public V Value
        {
            get { return this.mValue; }
        }
        /*/// <summary>
        /// Sets the <see cref="Priority"/> of this element, but only
        /// if it has already been removed from its Fibonacci Heap.
        /// </summary><param name="priority">The new value of the 
        /// <see cref="Priority"/> of this element.</param>
        /// <returns>True if this element's <see cref="Priority"/> was
        /// successfully set to <paramref name="priority"/> because it has
        /// already been <see cref="Removed"/> from its Fibonacci Heap;
        /// otherwise false because it's still in its heap.</returns>
        /// <remarks><para>
        /// This function is limited because it can only set the value of 
        /// this element's <see cref="Priority"/> after it has been
        /// <see cref="Removed"/> from the <see cref="Heap"/> instance that
        /// created it, and 
        /// <see cref="M:Heap.ChangePriority(FibonacciNode`2,P)"/> has to be
        /// used to set the priority before it is removed.
        /// </para><para>
        /// This is really more of a convenience function for algorithms that
        /// store the <see cref="T:FibonacciNode`2"/> instances created by
        /// their <see cref="Heap"/> instance in order to conserve memory, 
        /// rather than storing copies of the <see cref="Priority"/> of each 
        /// element in separate arrays and making sure those priorities 
        /// always match their counterparts in the elements the heap. 
        /// </para></remarks>
        public bool SetPriority(P priority)
        {
            if (this.Removed)
            {
                this.mPriority = priority;
                return true;
            }
            return false;
        }/* */

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

        /// <summary>
        /// A Fibonacci Heap data structure that consists of a collection of
        /// trees. It is an efficient implementation of a priority queue,
        /// which is like a regular queue, but each element also has a
        /// "priority" associated with it, and these "priorities" are used
        /// to determine the order in which the elements are removed.
        /// </summary><remarks>
        /// For more information on Fibonacci Heaps, see the Wikipedia 
        /// article: http://en.wikipedia.org/wiki/Fibonacci_heap
        /// </remarks>
        public class Heap : IEnumerable<FibonacciNode<P, V>>
        {
            private readonly HeapDirection mOrder;
            private readonly IComparer<P> mComparer;
            /// <summary>
            /// Used to control the direction of the heap.
            /// Set to 1 if the Heap is increasing, -1 if it's decreasing.
            /// We use the approach to avoid unneccesary branches.
            /// </summary>
            private int mMult;

            private NodeList mNodes;
            private FibonacciNode<P, V> mNext;
            private int mCount;

            private FibonacciNode<P, V>[] mDegToNode;

            /// <summary>
            /// Initializes an empty Fibonacci Heap in which its elements 
            /// will be prioritized in increasing order using the default
            /// comparer for the priority type <typeparamref name="P"/>.
            /// </summary>
            public Heap()
                : this(HeapDirection.Increasing, Comparer<P>.Default)
            {
            }
            /// <summary>
            /// Initializes an empty Fibonacci Heap in which its elements 
            /// will be prioritized in the given <paramref name="order"/> 
            /// using the default comparer for the priority type 
            /// <typeparamref name="P"/>.
            /// </summary>
            /// <param name="order">The order in which elements are
            /// removed based on the comparison of their "priorities".
            /// </param>
            public Heap(HeapDirection order)
                : this(order, Comparer<P>.Default)
            {
            }
            /// <summary>
            /// Initializes an empty Fibonacci Heap in which its elements 
            /// will be prioritized in the given <paramref name="order"/> 
            /// using the given <paramref name="comparer"/> for the 
            /// priority type <typeparamref name="P"/>.
            /// </summary>
            /// <param name="order">The order in which elements are
            /// removed based on the comparison of their "priorities".
            /// </param>
            /// <param name="comparer">The comparer used to prioritize
            /// elements by determining the relation between the 
            /// "priorities" of different elements.</param>
            public Heap(HeapDirection order, IComparer<P> comparer)
            {
                this.mOrder = order;
                this.mComparer = comparer;
                this.mMult = order == HeapDirection.Increasing ? 1 : -1;

                this.mNodes = new NodeList();
                this.mNext = null;
                this.mCount = 0;

                this.mDegToNode = new FibonacciNode<P, V>[4];
            }
            /// <summary>
            /// The order in which elements are removed from this 
            /// Fibonacci Heap based on the comparison of their 
            /// <see cref="P:FibonacciNode`2.Priority"/> values.
            /// </summary>
            public HeapDirection Order
            {
                get { return this.mOrder; }
            }
            /// <summary>
            /// The comparer used to prioritize the elements in
            /// this Fibonacci Heap by determining whether the 
            /// <see cref="P:FibonacciNode`2.Priority"/> of one 
            /// element is higher than, lower than, or equal to 
            /// that of another element.</summary>
            public IComparer<P> PriorityComparer
            {
                get { return this.mComparer; }
            }

            #region Internal Functions
            /// <summary>
            /// Ensures that the internal degree to node array can store
            /// a node reference at the given degree by resizing it if 
            /// necessary.</summary>
            /// <param name="degree">The degree of the node reference to be
            /// stored in the internal degree to node array.</param>
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
            /// <param name="parentNode">The parent node.</param>
            /// <param name="childNode">The child node.</param>
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
                            node.mPriority) * this.mMult <= 0)
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
                        this.mNext.mPriority) * this.mMult < 0)
                    {
                        this.mNext = node;
                    }
                    node = node.Next;
                }
            }
            /// <summary>
            /// Updates the degree of a node, cascading to update the degree 
            /// of its ancestors if neccesary.
            /// </summary>
            /// <param name="parentNode">The node that will have its degree
            /// updated (as well as its ancestors if necessary).</param>
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
            /// <summary>
            /// Whether this Fibonacci Heap is currently empty and does not 
            /// contain any elements.
            /// </summary>
            public bool IsEmpty
            {
                get { return this.mNodes.First == null; }
            }
            /// <summary>
            /// Gets the node currently at the front of this Fibonacci Heap,
            /// which is node with the highest (or lowest, depending on the
            /// <see cref="Order"/>) "priority" and the next node to be
            /// removed from this Fibonacci Heap.
            /// </summary><remarks>
            /// Retrieving the value of this property is basically the same
            /// as using the <see cref="Peek()"/> function.</remarks>
            /// <seealso cref="Peek()"/><seealso cref="Dequeue()"/>
            public FibonacciNode<P, V> Top
            {
                get { return this.mNext; }
            }
            /// <summary>
            /// Gets the number of elements currently stored in this 
            /// Fibonacci Heap.
            /// </summary>
            public int Count
            {
                get { return this.mCount; }
            }
            /// <summary>
            /// Adds a new element to this Fibonacci Heap, which stores the
            /// given <paramref name="value"/> with the given
            /// <paramref name="priority"/>, which is used to determine its
            /// ordering relative to other elements already in this heap.
            /// </summary>
            /// <param name="priority">The "priority" of the element, which
            /// determines its order in this Fibonacci Heap.</param>
            /// <param name="value">The value stored in the element.</param>
            /// <returns>A <see cref="T:FibonacciNode`2"/> instance that
            /// represents the new element added to this Fibonacci Heap.
            /// </returns><remarks>
            /// This operation is done in <c>O(1)</c> time, and it is also
            /// fast as well because it does not do any book keeping or
            /// maintenance of the heap. It just adds the new element to 
            /// the end of the list of heaps, updating <see cref="Top"/>
            /// if necessary (if the new element has the highest/lowest
            /// <see cref="P:FibonacciNode`2.Priority"/> of all the
            /// elements in this Fibonacci Heap).
            /// </remarks>
            public FibonacciNode<P, V> Enqueue(P priority, V value)
            {
                FibonacciNode<P, V> newNode 
                    = new FibonacciNode<P, V>(priority, value);
                // We don't do any book keeping or 
                // maintenance of the heap on Enqueue.
                // We just add this node to the end of the list of Heaps, 
                // updating the Next if required
                this.mNodes.AddLast(newNode);
                if (this.mNext == null || this.mComparer.Compare(priority, 
                    this.mNext.mPriority) * this.mMult < 0)
                {
                    this.mNext = newNode;
                }
                this.mCount++;
                return newNode;
            }
            /// <summary>
            /// Returns the next element to be removed from this Fibonacci
            /// Heap without removing it, which is the element with the 
            /// highest (or lowest, depending on the <see cref="Order"/>)
            /// <see cref="P:FibonacciNode`2.Priority"/> out of all the
            /// elements in this Fibonacci Heap.
            /// </summary><returns>
            /// The next element to be removed from this Fibonacci Heap, 
            /// which is the element with the highest/lowest "priority".
            /// </returns><remarks>
            /// This operation is done in <c>O(1)</c> time because all
            /// Fibonacci Heaps maintain a pointer to the next element 
            /// to be removed.
            /// </remarks>
            public FibonacciNode<P, V> Peek()
            {
                if (this.mCount == 0)
                    throw new InvalidOperationException();
                return this.mNext;
            }
            /// <summary>
            /// Removes the element currently at the front of this Fibonacci 
            /// Heap, which is element with the highest (or lowest, depending
            /// on the <see cref="Order"/>) "priority".
            /// </summary><returns>
            /// The element that was just removed from this Fibonacci Heap,
            /// which was the element with the highest/lowest "priority".
            /// </returns><remarks><para>
            /// This operation is done in <c>O(log n)</c> amortized time,
            /// because it operates in three phases, first removing the
            /// <see cref="Top"/> element and making its children the roots
            /// of new trees in the internal tree list of this Fibonacci
            /// Heap, which is done in <c>O(log n)</c> amortized time. 
            /// </para><para>
            /// In the second phase, the internal tree list is compressed by
            /// successively linking together roots of the same degree, which
            /// is done in <c>O(log n)</c> amortized time. 
            /// </para><para>
            /// In the final phase, the new <see cref="Top"/> element is
            /// located among the remaining roots, which is done in 
            /// <c>O(log n)</c> time, since that's the maximum number of 
            /// remaining roots at the end of the second phase.
            /// </para></remarks><seealso cref="M:Delete(FibonacciNode`2)"/>
            /// <exception cref="InvalidOperationException">This Fibonacci
            /// Heap is empty and has no elements to remove.</exception>
            public FibonacciNode<P, V> Dequeue()
            {
                if (this.mCount == 0)
                    throw new InvalidOperationException();

                FibonacciNode<P, V> result = this.mNext;

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
            /// <summary>
            /// Removes the given <paramref name="node"/> from this Fibonacci
            /// Heap if it hasn't already been removed.
            /// </summary>
            /// <param name="node">The <see cref="T:FibonacciNode`2"/> 
            /// instance to be removed from this Fibonacci Heap.</param>
            /// <remarks>
            /// This operation is done in <c>O(log n)</c> amortized time,
            /// since it is basically a combination of the 
            /// <see cref="M:ChangePriority(FibonacciNode`2,P)"/> and 
            /// <see cref="Dequeue()"/> operations. The priority of the given
            /// <paramref name="node"/> is first set so that it becomes the
            /// new <see cref="Top"/> element, which takes <c>O(1)</c>
            /// amortized time, and then it is removed from this Fibonacci
            /// Heap, which takes <c>O(log n)</c> amortized time.
            /// </remarks><seealso cref="Dequeue()"/>
            /// <seealso cref="M:ChangePriority(FibonacciNode`2,P)"/>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="node"/> is <c>null</c>.</exception>
            public void Delete(FibonacciNode<P, V> node)
            {
                if (node == null)
                {
                    throw new ArgumentNullException("node");
                }
                if (!node.Removed)
                {
                    this.ChangeKeyInternal(node, default(P), true);
                    this.Dequeue();
                }
            }
            /// <summary>
            /// Sets the <see cref="P:FibonacciNode`2.Priority"/> of the
            /// given <paramref name="node"/> to the given 
            /// <paramref name="priority"/>, which will affect the order in
            /// which affect the order in which <paramref name="node"/> is
            /// removed from this heap if it hasn't already been removed.
            /// </summary>
            /// <param name="node">The <see cref="T:FibonacciNode`2"/>
            /// instance that will have its priority changed.</param>
            /// <param name="priority">The new priority for the given
            /// <paramref name="node"/>.</param><remarks>
            /// This operation is done in <c>O(1)</c> amortized time,
            /// since the new <paramref name="priority"/> for the given
            /// <paramref name="node"/> could violate the heap property
            /// by being out of order with the priority of the parent of
            /// <paramref name="node"/>, which means that the operation
            /// could take longer by having to alter the internal tree list
            /// of this Fibonacci Heap.</remarks>
            /// <seealso cref="M:Delete(FibonacciNode`2)"/>
            /// <exception cref="ArgumentNullException">
            /// <paramref name="node"/> is <c>null</c>.</exception>
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
                if (node.Removed)
                {
                    node.mPriority = nk;
                    return;
                }
                int delta = Math.Sign(
                    this.mComparer.Compare(node.mPriority, nk));
                if ((delta == this.mMult || delete) && delta != 0)
                {
                    //New value is in the same direction as the heap
                    node.mPriority = nk;
                    FibonacciNode<P, V> parent = node.Parent;
                    if (parent != null && (this.mComparer.Compare(nk, 
                        parent.mPriority) * this.mMult < 0 || delete))
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
                        this.mNext.mPriority) * this.mMult < 0)
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
                        int count = 0;
                        FibonacciNode<P, V>[] toUpdate 
                            = new FibonacciNode<P, V>[this.mCount];
                        FibonacciNode<P, V> child = node.Children.First;
                        while (child != null)
                        {
                            if (this.mComparer.Compare(node.mPriority, 
                                child.mPriority) * this.mMult > 0)
                            {
                                toUpdate[count++] = child;
                            }
                            child = child.Next;
                        }

                        if (count > 0)
                        {
                            node.Marked = true;
                            for (int i = 0; i < count; i++)
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
            /// <summary>
            /// Merges this Fibonacci Heap with the <paramref name="other"/>
            /// Fibonacci Heap by concatenating the elements of the other
            /// heap with the elements of this Fibonacci Heap.
            /// </summary>
            /// <param name="other">The Fibonacci Heap to merge with this
            /// heap.</param>
            /// <remarks>
            /// This operation is done in <c>O(1)</c> time because all it
            /// does is concatenate the internal list of trees in the
            /// <paramref name="other"/> Fibonacci Heap with the internal
            /// list of trees in this Fibonacci Heap, which is a quick
            /// operation since they are doubly linked lists.
            /// </remarks>
            public void Merge(Heap other)
            {
                if (other == this)
                {
                    throw new ArgumentException(//"Error: " + 
                        "Attempted to merge this heap with itself", "other");
                }
                if (other.mOrder != this.mOrder)
                {
                    throw new Exception(//"Error: " + 
                        "Heaps must go in the same direction when merging");
                }
                this.mNodes.MergeLists(other.mNodes);
                if (this.mComparer.Compare(other.mNext.mPriority, 
                    this.mNext.mPriority) * this.mMult < 0)
                {
                    this.mNext = other.mNext;
                }
                this.mCount += other.mCount;
                // TODO: Should the other heap be emptied?
                // Could running any operation on the other heap
                // after the merger FUBAR this heap?
            }
            #endregion

            #region Iteration and Enumeration
            private Heap CreateClone()
            {
                int stackCount = 0;
                Heap clone = new Heap(this.mOrder, this.mComparer);
                FibonacciNode<P, V>[] nodeStack 
                    = new FibonacciNode<P, V>[this.mCount];
                FibonacciNode<P, V> node = this.mNodes.First;
                while (node != null)
                {
                    //nodeStack.Push(node);
                    nodeStack[stackCount++] = node;
                    node = node.Next;
                }
                FibonacciNode<P, V> topNode;
                while (stackCount > 0)//nodeStack.Count > 0)
                {
                    topNode = nodeStack[--stackCount];//nodeStack.Pop();
                    clone.Enqueue(topNode.mPriority, topNode.mValue);
                    node = topNode.Children.First;
                    while (node != null)
                    {
                        //nodeStack.Push(node);
                        nodeStack[stackCount++] = node;
                        node = node.Next;
                    }
                }
                return clone;
            }
            /// <summary>
            /// Copies the elements of this Fibonacci Heap to an array in the
            /// order in which they are to be removed (basically sorted by
            /// their priorities) without removing them from this Fibonacci
            /// Heap.</summary><returns>
            /// An array of the elements in this Fibonacci Heap, sorted by
            /// the order in which they are to be removed from this Fibonacci
            /// Heap.</returns><remarks>
            /// Be aware that this function works by creating a "clone" of
            /// this Fibonacci Heap that takes up as much memory as it does,
            /// and then repeatedly removing elements from that "clone" and
            /// adding them to the returned array, so this function is both
            /// processor and memory intensive.
            /// </remarks><seealso cref="DestroyToArray(bool)"/>
            /// <seealso cref="ToUnsortedArray()"/>
            public FibonacciNode<P, V>[] ToArray()
            {
                Heap clone = this.CreateClone();
                FibonacciNode<P, V>[] nodes 
                    = new FibonacciNode<P, V>[this.mCount];
                for (int i = 0; i < nodes.Length; i++)
                {
                    nodes[i] = clone.Dequeue();
                }
                return nodes;
            }
            /// <summary>
            /// Copies the elements of this Fibonacci Heap to an array in the
            /// order in which they are to be removed (basically sorted by
            /// their priorities) by repeatedly removing them from this 
            /// Fibonacci Heap until it is empty.</summary>
            /// <param name="restore">If true, this function re-adds every
            /// element it removed from this Fibonacci Heap to create the
            /// returned array.</param><returns>
            /// An array of the elements in this Fibonacci Heap, sorted by
            /// the order in which they were removed from this Fibonacci
            /// Heap.</returns><remarks>
            /// Be aware that this function works by repeatedly removing
            /// elements from this Fibonacci Heap until it is empty and then
            /// adding them to the returned array, which basically "destroys"
            /// this Fibonacci Heap in the process.</remarks>
            /// <seealso cref="ToArray()"/>
            /// <seealso cref="ToUnsortedArray()"/>
            public FibonacciNode<P, V>[] DestroyToArray(bool restore)
            {
                int i;
                FibonacciNode<P, V>[] nodes 
                    = new FibonacciNode<P, V>[this.mCount];
                for (i = 0; i < nodes.Length; i++)
                {
                    nodes[i] = this.Dequeue();
                }
                if (restore)
                {
                    for (i = 0; i < nodes.Length; i++)
                    {
                        this.Enqueue(nodes[i].Priority, nodes[i].Value);
                    }
                }
                return nodes;
            }
            /// <summary>
            /// Copies the elements of this Fibonacci Heap to an array in the
            /// order in which they are stored in its internal list of trees,
            /// which might not be the order in which they are to be removed.
            /// </summary><returns>
            /// An unsorted array of the elements in this Fibonacci Heap.
            /// </returns><remarks><para>
            /// This is a quick and clean function for creating an array of
            /// all the elements of this Fibonacci Heap without altering it. 
            /// This function is much less processor and memory intensive 
            /// than both <see cref="DestroyToArray(bool)"/> and 
            /// <see cref="ToArray()"/>. </para><para>
            /// The only catch is that the elements in the returned array 
            /// might not be in the order in which they would be removed 
            /// from this heap. However, that could be easily fixed by just 
            /// sorting the elements of the returned array by their keys 
            /// using this heap's <see cref="PriorityComparer"/> and then 
            /// reversing it if this heap's <see cref="Order"/> is 
            /// <see cref="HeapDirection.Decreasing"/> instead of
            /// <see cref="HeapDirection.Increasing"/>.</para></remarks>
            /// <seealso cref="ToArray()"/>
            /// <seealso cref="DestroyToArray(bool)"/>
            public FibonacciNode<P, V>[] ToUnsortedArray()
            {
                int i = 0;
                int stackCount = 0;
                FibonacciNode<P, V>[] nodes
                    = new FibonacciNode<P, V>[this.mCount];
                FibonacciNode<P, V>[] nodeStack
                    = new FibonacciNode<P, V>[this.mCount];
                FibonacciNode<P, V> node = this.mNodes.First;
                while (node != null)
                {
                    //nodeStack.Push(node);
                    nodeStack[stackCount++] = node;
                    node = node.Next;
                }
                FibonacciNode<P, V> topNode;
                while (stackCount > 0)//nodeStack.Count > 0)
                {
                    topNode = nodeStack[--stackCount];//nodeStack.Pop();
                    nodes[i++] = topNode;
                    node = topNode.Children.First;
                    while (node != null)
                    {
                        //nodeStack.Push(node);
                        nodeStack[stackCount++] = node;
                        node = node.Next;
                    }
                }
                return nodes;
            }
            /// <summary>
            /// Enumerates the elements of a <see cref="Heap"/>.
            /// </summary><remarks>
            /// Be aware that this enumerator works by repeatedly removing 
            /// elements via the <see cref="Dequeue()"/> function on either 
            /// the <see cref="Heap"/> instance that created it or a "clone"
            /// of that heap instance, which can make it both processor and
            /// memory intensive.
            /// </remarks>
            public class Enumerator : IEnumerator<FibonacciNode<P, V>>
            {
                private readonly Heap mHeap;
                private readonly bool bIsDestructive;
                private FibonacciNode<P, V> mCurrent;

                private Enumerator(Heap heap,
                    bool isDestructive)
                {
                    this.mHeap = heap;
                    this.bIsDestructive = isDestructive;
                    this.mCurrent = null;
                }
                /// <summary>
                /// Creates a new "non-destructive" <see cref="Enumerator"/>
                /// instance which enumerates the elements of the given
                /// <paramref name="heap"/> without removing them.
                /// </summary>
                /// <param name="heap">The Fibonacci Heap that will be
                /// enumerated by the returned <see cref="Enumerator"/>
                /// instance.</param><returns>
                /// A new <see cref="Enumerator"/> that enumerates the 
                /// elements of <paramref name="heap"/> without altering it.
                /// </returns><remarks>
                /// Be aware that the <see cref="Enumerator"/> returned by
                /// this function operates by removing elements from a
                /// "clone" of the given <paramref name="heap"/> in order
                /// to keep the original Fibonacci Heap unaltered. This
                /// "clone" takes up as much memory as the original heap,
                /// and the act of creating it takes additional processing
                /// time.</remarks>
                public static Enumerator Create(Heap heap)
                {
                    return new Enumerator(heap.CreateClone(), false);
                }
                /// <summary>
                /// Creates a new "destructive" <see cref="Enumerator"/>
                /// instance which enumerates the elements of the given
                /// <paramref name="heap"/> by repeatedly removing them.
                /// </summary>
                /// <param name="heap">The Fibonacci Heap that will be
                /// enumerated by the returned <see cref="Enumerator"/>
                /// instance.</param><returns>
                /// A new <see cref="Enumerator"/> that enumerates the 
                /// elements of <paramref name="heap"/> by emptying it.
                /// </returns><remarks>
                /// Be aware the the <see cref="Enumerator"/> returned by
                /// this function operates by repeatedly removing elements
                /// from the given <paramref name="heap"/> until it is
                /// empty, which makes it a one use only enumerator that
                /// basically "destroys" the original heap.
                /// </remarks>
                public static Enumerator CreateDestructive(Heap heap)
                {
                    return new Enumerator(heap, true);
                }
                /// <summary>
                /// Gets the element at the current position of this
                /// enumerator.
                /// </summary>
                public FibonacciNode<P, V> Current
                {
                    get { return this.mCurrent; }
                }
                /// <summary>
                /// Gets the <see cref="P:FibonacciNode`2.Priority"/> of the
                /// element at the current position of this enumerator.
                /// </summary>
                public P Priority
                {
                    get { return this.mCurrent.mPriority; }
                }
                /// <summary>
                /// Gets the <see cref="P:FibonacciNode`2.Value"/> of the
                /// element at the current position of this enumerator.
                /// </summary>
                public V Value
                {
                    get { return this.mCurrent.mValue; }
                }
                /// <summary>
                /// Releases all resources used by this enumerator, which
                /// includes removing any remaining elements from the
                /// internal
                /// </summary><remarks>
                /// If this enumerator is non-destructive, this empties the
                /// internal clone of the <see cref="Heap"/> that created it.
                /// </remarks>
                public void Dispose()
                {
                    if (!this.bIsDestructive)
                    {
                        while (this.mHeap.mCount > 0)
                        {
                            this.mHeap.Dequeue();
                        }
                        this.mCurrent = null;
                    }
                }
                /// <summary>
                /// Advances the enumerator to the next element in the 
                /// <see cref="Heap"/> that created it.
                /// </summary>
                /// <returns>The next element in the <see cref="Heap"/>
                /// that created this enumerator.</returns>
                public bool MoveNext()
                {
                    if (this.mHeap.mCount > 0)
                    {
                        this.mCurrent = this.mHeap.Dequeue();
                        return true;
                    }
                    this.mCurrent = null;
                    return false;
                }

                object System.Collections.IEnumerator.Current
                {
                    get { return this.mCurrent; }
                }
                void System.Collections.IEnumerator.Reset()
                {
                    throw new NotSupportedException();
                }
            }

            /// <summary>
            /// Creates a new "non-destructive" <see cref="Enumerator"/>
            /// instance which enumerates the elements of this Fibonacci
            /// Heap without removing them.
            /// </summary><returns>
            /// A new <see cref="Enumerator"/> that enumerates the  
            /// elements of this Fibonacci Heap without altering it.
            /// </returns><remarks>
            /// Be aware that the <see cref="Enumerator"/> returned by this
            /// function operates by removing elements from a "clone" of this
            /// Fibonacci Heap in order to keep it unaltered. This "clone"
            /// takes up as much memory as this Fibonacci Heap, and the act
            /// of creating it takes additional processing time.</remarks>
            public Enumerator GetEnumerator()
            {
                return Enumerator.Create(this);
            }
            /// <summary>
            /// Creates a new "destructive" <see cref="Enumerator"/>
            /// instance which enumerates the elements of this Fibonacci
            /// Heap by repeatedly removing them.
            /// </summary><returns>
            /// A new <see cref="Enumerator"/> that enumerates the 
            /// elements of this Fibonacci Heap by emptying it.
            /// </returns><remarks>
            /// Be aware the the <see cref="Enumerator"/> returned by this
            /// function operates by repeatedly removing elements from this
            /// Fibonacci Heap until it is empty, which makes it a one use 
            /// only enumerator that basically "destroys" this Fibonacci 
            /// Heap.</remarks>
            public Enumerator GetDestructiveEnumerator()
            {
                return Enumerator.CreateDestructive(this);
            }

            IEnumerator<FibonacciNode<P, V>> 
                IEnumerable<FibonacciNode<P, V>>.GetEnumerator()
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
            /// <summary>
            /// Creates a string that is a visualization (when printed in a 
            /// monospaced font, such as Courier New) of the internal list 
            /// of trees of elements in this Fibonacci Heap, with each node
            /// represented by its <see cref="P:FibonacciNode`2.Priority"/>
            /// in string form, and followed by an asterisk if the node has
            /// had a child cut from it before.
            /// </summary><returns>
            /// A string that is a visualization (when monospaced) of the
            /// internal tree list data structure of this Fibonacci Heap.
            /// </returns>
            public string DrawHeap()
            {
                int lineCount = 0;
                string[] lines = new string[this.mCount];
                int lineNum = 0;
                int columnPosition = 0;
                int stackCount = 0;
                NodeLevel[] stack = new NodeLevel[this.mCount];
                FibonacciNode<P, V> node = this.mNodes.Last;
                while (node != null)
                {
                    //stack.Push(new NodeLevel(node, 0));
                    stack[stackCount++] = new NodeLevel(node, 0);
                    node = node.Prev;
                }
                NodeLevel currCell;
                string currLine, nodeString;
                while (stackCount > 0)
                {
                    currCell = stack[--stackCount];//stack.Pop();
                    lineNum = currCell.Level;
                    if (lineCount <= lineNum)
                        lines[lineCount++] = string.Empty;
                    currLine = lines[lineNum];
                    currLine = currLine.PadRight(columnPosition, ' ');
                    nodeString = currCell.Node.mPriority.ToString()
                        + (currCell.Node.Marked ? "*" : "") + " ";
                    currLine += nodeString;
                    if (currCell.Node.Children != null &&
                        currCell.Node.Children.Last != null)
                    {
                        node = currCell.Node.Children.Last;
                        while (node != null)
                        {
                            //stack.Push(new NodeLevel(node, 
                            //    currentCell.Level + 1));
                            stack[stackCount++] 
                                = new NodeLevel(node, currCell.Level + 1);
                            node = node.Prev;
                        }
                    }
                    else
                    {
                        columnPosition += nodeString.Length;
                    }
                    lines[lineNum] = currLine;
                }
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < lineCount; i++)
                    builder.AppendLine(lines[i]);
                return builder.ToString();
            }
            #endregion
        }
    }
}
