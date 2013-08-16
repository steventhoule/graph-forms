using System;
using System.Collections.Generic;

namespace GraphForms.Algorithms
{
    public abstract class AGraphAlgorithm<Node, Edge> : AAlgorithm
        where Edge : IGraphEdge<Node>
    {
        private static Digraph<Node, Edge>.GNode[] sEmptyRoots
            = new Digraph<Node, Edge>.GNode[0];

        private int mRootCount;
        private Digraph<Node, Edge>.GNode[] mRoots;
        /// <summary>
        /// The graph processed by this algorithm.
        /// </summary>
        protected readonly Digraph<Node, Edge> mGraph;

        public AGraphAlgorithm(Digraph<Node, Edge> graph)
        {
            if (graph == null)
                throw new ArgumentNullException("graph");
            this.mGraph = graph;
            this.mRoots = sEmptyRoots;
        }

        public AGraphAlgorithm(Digraph<Node, Edge> graph, int rootCapacity)
        {
            if (rootCapacity < 0)
                throw new ArgumentOutOfRangeException("rootCapacity");

            this.mGraph = graph;
            this.mRoots = rootCapacity == 0 ? sEmptyRoots
                : new Digraph<Node, Edge>.GNode[rootCapacity];
        }

        public Digraph<Node, Edge> Graph
        {
            get { return this.mGraph; }
        }

        #region Root List Properties

        public int RootCount
        {
            get { return this.mRootCount; }
        }

        public int RootCapacity
        {
            get { return this.mRoots.Length; }
            set
            {
                if (value < this.mRootCount)
                {
                    throw new ArgumentOutOfRangeException("RootCapacity");
                }
                if (value != this.mRoots.Length)
                {
                    Digraph<Node, Edge>.GNode[] roots
                        = new Digraph<Node, Edge>.GNode[value];
                    if (this.mRootCount > 0)
                    {
                        Array.Copy(this.mRoots, 0, 
                            roots, 0, this.mRootCount);
                    }
                    this.mRoots = roots;
                }
            }
        }

        public Digraph<Node, Edge>.GNode[] Roots
        {
            get
            {
                Digraph<Node, Edge>.GNode[] roots
                    = new Digraph<Node, Edge>.GNode[this.mRootCount];
                Array.Copy(this.mRoots, 0, roots, 0, this.mRootCount);
                return roots;
            }
        }

        public Digraph<Node, Edge>.GNode RootAt(int index)
        {
            if (index < 0 || index >= this.mRootCount)
                throw new ArgumentOutOfRangeException("index");
            return this.mRoots[index];
        }

        #endregion

        #region Root List Insertion

        public bool AddRoot(Node root)
        {
            int i = this.mGraph.IndexOfNode(root);
            if (i < 0)
            {
                return false;
            }
            return this.InsertRoot(this.mRootCount,
                this.mGraph.InternalNodeAt(i), false);
        }

        public bool AddRoot(Node root, bool removeBadRoots)
        {
            int i = this.mGraph.IndexOfNode(root);
            if (i < 0)
            {
                return false;
            }
            return this.InsertRoot(this.mRootCount,
                this.mGraph.InternalNodeAt(i), removeBadRoots);
        }

        public bool AddRoot(int rootIndex)
        {
            if (rootIndex < 0 || rootIndex >= this.mGraph.NodeCount)
            {
                return false;
            }
            return this.InsertRoot(this.mRootCount,
                this.mGraph.InternalNodeAt(rootIndex), false);
        }

        public bool AddRoot(int rootIndex, bool removeBadRoots)
        {
            if (rootIndex < 0 || rootIndex >= this.mGraph.NodeCount)
            {
                return false;
            }
            return this.InsertRoot(this.mRootCount,
                this.mGraph.InternalNodeAt(rootIndex), removeBadRoots);
        }

        public bool InsertRoot(int index, Node root)
        {
            if (index < 0 || index > this.mRootCount)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            int i = this.mGraph.IndexOfNode(root);
            if (i < 0)
            {
                return false;
            }
            return this.InsertRoot(index,
                this.mGraph.InternalNodeAt(i), false);
        }

        public bool InsertRoot(int index, Node root, bool removeBadRoots)
        {
            if (index < 0 || index > this.mRootCount)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            int i = this.mGraph.IndexOfNode(root);
            if (i < 0)
            {
                return false;
            }
            return this.InsertRoot(index,
                this.mGraph.InternalNodeAt(i), removeBadRoots);
        }

        public bool InsertRoot(int index, int rootIndex)
        {
            if (index < 0 || index > this.mRootCount)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if (rootIndex < 0 || rootIndex >= this.mGraph.NodeCount)
            {
                return false;
            }
            return this.InsertRoot(index,
                this.mGraph.InternalNodeAt(rootIndex), false);
        }

        public bool InsertRoot(int index, int rootIndex, bool removeBadRoots)
        {
            if (index < 0 || index > this.mRootCount)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if (rootIndex < 0 || rootIndex >= this.mGraph.NodeCount)
            {
                return false;
            }
            return this.InsertRoot(index,
                this.mGraph.InternalNodeAt(rootIndex), removeBadRoots);
        }

        private bool InsertRoot(int index, Digraph<Node, Edge>.GNode root,
            bool removeBadRoots)
        {
            int i, gIndex = -1;
            if (removeBadRoots)
            {
                Digraph<Node, Edge>.GNode gNode;
                // Check if this algorithm already has the given root and
                // Remove any roots that are no longer in the graph
                for (i = this.mRootCount - 1; i >= index; i--)
                {
                    if (this.mRoots[i].Index == -1)
                    {
                        gNode = this.mRoots[i];
                        this.mRootCount--;
                        Array.Copy(this.mRoots, i + 1, this.mRoots, i,
                            this.mRootCount - i);
                        this.mRoots[this.mRootCount] = null;
                        this.OnRootRemoved(gNode);
                    }
                    else if (this.mRoots[i].Index == root.Index)
                    {
                        gIndex = i;
                    }
                }
                for (i = index - 1; i >= 0; i--)
                {
                    if (this.mRoots[i].Index == -1)
                    {
                        index--;
                        gNode = this.mRoots[i];
                        this.mRootCount--;
                        Array.Copy(this.mRoots, i + 1, this.mRoots, i,
                            this.mRootCount - i);
                        this.mRoots[this.mRootCount] = null;
                        this.OnRootRemoved(gNode);
                    }
                    else if (this.mRoots[i].Index == root.Index)
                    {
                        gIndex = i;
                    }
                }
            }
            else
            {
                for (i = 0; i < this.mRootCount; i++)
                {
                    if (this.mRoots[i].Index == root.Index)
                    {
                        gIndex = i;
                        break;
                    }
                }
            }
            if (gIndex >= 0)
            {
                return false;
            }
            // Ensure the capacity of the root storage
            if (this.mRootCount == this.mRoots.Length)
            {
                if (this.mRootCount == 0)
                {
                    this.mRoots = new Digraph<Node, Edge>.GNode[4];
                }
                else
                {
                    Digraph<Node, Edge>.GNode[] roots 
                        = new Digraph<Node, Edge>.GNode[2 * this.mRootCount];
                    Array.Copy(this.mRoots, 0, roots, 0, this.mRootCount);
                    this.mRoots = roots;
                }
            }
            // Insert the new root
            if (index < this.mRootCount)
            {
                Array.Copy(this.mRoots, index, this.mRoots, index + 1, 
                    this.mRootCount - index);
            }
            this.mRoots[index] = root;
            this.mRootCount++;
            this.OnRootInserted(index, root);
            return true;
        }

        protected virtual void OnRootInserted(int index,
            Digraph<Node, Edge>.GNode root)
        {
        }
        #endregion

        #region Root List Deletion

        public bool RemoveRoot(Node root, bool removeBadRoots)
        {
            bool found = false;
            Digraph<Node, Edge>.GNode gRoot;
            EqualityComparer<Node> ec = EqualityComparer<Node>.Default;
            for (int i = this.mRootCount - 1; i >= 0; i--)
            {
                if (removeBadRoots && this.mRoots[i].Index == -1)
                {
                    gRoot = this.mRoots[i];
                    this.mRootCount--;
                    Array.Copy(this.mRoots, i + 1, this.mRoots, i,
                        this.mRootCount - i);
                    this.mRoots[this.mRootCount] = null;
                    this.OnRootRemoved(gRoot);
                }
                else if (ec.Equals(this.mRoots[i].Data, root))
                {
                    found = true;
                    gRoot = this.mRoots[i];
                    this.mRootCount--;
                    Array.Copy(this.mRoots, i + 1, this.mRoots, i,
                        this.mRootCount - i);
                    this.mRoots[this.mRootCount] = null;
                    this.OnRootRemoved(gRoot);
                }
            }
            return found;
        }

        public bool RemoveRoot(int rootIndex, bool removeBadRoots)
        {
            if (rootIndex < 0 || rootIndex >= this.mGraph.NodeCount)
            {
                return false;
            }
            bool found = false;
            Digraph<Node, Edge>.GNode gRoot;
            for (int i = this.mRootCount - 1; i >= 0; i--)
            {
                if (removeBadRoots && this.mRoots[i].Index == -1)
                {
                    gRoot = this.mRoots[i];
                    this.mRootCount--;
                    Array.Copy(this.mRoots, i + 1, this.mRoots, i,
                        this.mRootCount - i);
                    this.mRoots[this.mRootCount] = null;
                    this.OnRootRemoved(gRoot);
                }
                else if (this.mRoots[i].Index == rootIndex)
                {
                    found = true;
                    gRoot = this.mRoots[i];
                    this.mRootCount--;
                    Array.Copy(this.mRoots, i + 1, this.mRoots, i,
                        this.mRootCount - i);
                    this.mRoots[this.mRootCount] = null;
                    this.OnRootRemoved(gRoot);
                }
            }
            return found;
        }

        public void RemoveRootAt(int index, bool removeBadRoots)
        {
            if (index < 0 || index >= this.mRootCount)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            // Remove the root at the given index
            Digraph<Node, Edge>.GNode gRoot = this.mRoots[index];
            this.mRootCount--;
            if (index < this.mRootCount)
            {
                Array.Copy(this.mRoots, index + 1, this.mRoots, index, 
                    this.mRootCount - index);
            }
            this.mRoots[this.mRootCount] = null;
            this.OnRootRemoved(gRoot);
            if (removeBadRoots)
            {
                // Remove any roots that are no longer in the graph
                for (int i = this.mRootCount - 1; i >= 0; i--)
                {
                    if (this.mRoots[i].Index == -1)
                    {
                        gRoot = this.mRoots[i];
                        this.mRootCount--;
                        Array.Copy(this.mRoots, i + 1, this.mRoots, i,
                            this.mRootCount - i);
                        this.mRoots[this.mRootCount] = null;
                        this.OnRootRemoved(gRoot);
                    }
                }
            }
        }

        public void RemoveBadRoots()
        {
            Digraph<Node, Edge>.GNode gRoot;
            for (int i = this.mRootCount - 1; i >= 0; i--)
            {
                if (this.mRoots[i].Index == -1)
                {
                    gRoot = this.mRoots[i];
                    this.mRootCount--;
                    Array.Copy(this.mRoots, i + 1, this.mRoots, i,
                        this.mRootCount - i);
                    this.mRoots[this.mRootCount] = null;
                    this.OnRootRemoved(gRoot);
                }
            }
        }

        protected virtual void OnRootRemoved(Digraph<Node, Edge>.GNode root)
        {
        }

        public void ClearRoots()
        {
            if (this.mRootCount > 0)
            {
                Array.Clear(this.mRoots, 0, this.mRootCount);
                this.mRootCount = 0;
                this.OnRootsCleared();
            }
        }

        protected virtual void OnRootsCleared()
        {
        }
        #endregion
    }
}
