using System;
using GraphForms.Algorithms.Search;

namespace GraphForms.Algorithms.Path
{
    public class DFLongestPath<Node, Edge>
        : DepthFirstSearch<Node, Edge>
        where Edge : IGraphEdge<Node>
    {
        private class NodeData
        {
            public double Length;
            public Edge Next;
            public int NextNode;

            public NodeData()
            {
                //this.Length = 0;
                //this.Next = null;
                this.NextNode = -1;
            }

            public NodeData(double length, Edge next, int nextNode)
            {
                this.Length = length;
                this.Next = next;
                this.NextNode = nextNode;
            }

            public NodeData(NodeData data)
            {
                this.Length = data.Length;
                this.Next = data.Next;
                this.NextNode = data.NextNode;
            }
        }

        private bool bUseWeights = false;
        /// <summary>
        /// The index of the node where the DFS started
        /// </summary>
        private int mStartIndex = -1;
        /// <summary>
        /// The 2nd longest branch attached to node where the DFS started
        /// </summary>
        private NodeData mStart = null;
        private NodeData[] mDatas;

        private int[] mPathNodeIndexes;
        private Node[] mPathNodes;
        private Edge[] mPathEdges;

        public DFLongestPath(Digraph<Node, Edge> graph,
            bool directed, bool reversed)
            : base(graph, directed, reversed)
        {
        }

        public bool UseWeights
        {
            get { return this.bUseWeights; }
            set
            {
                if (this.State != ComputeState.Running)
                    this.bUseWeights = value;
            }
        }

        public int[] PathNodeIndexes
        {
            get { return this.mPathNodeIndexes; }
        }

        public Node[] PathNodes
        {
            get { return this.mPathNodes; }
        }

        public Edge[] PathEdges
        {
            get { return this.mPathEdges; }
        }

        public override void Initialize()
        {
            this.mDatas = new NodeData[this.mGraph.NodeCount];
            for (int i = 0; i < this.mDatas.Length; i++)
            {
                this.mDatas[i] = new NodeData();
            }
            base.Initialize();
        }

        protected override void OnStartNode(Digraph<Node, Edge>.GNode n)
        {
            this.mStartIndex = n.Index;
            this.mStart = new NodeData();
        }

        protected override void OnFinishNode(
            Digraph<Node, Edge>.GNode n, uint depth)
        {
            if (this.bUndirected && this.mStartIndex == n.Index)
            {
                int index = n.Index;
                double len = this.mDatas[index].Length;
                Edge next = this.mStart.Next;
                int nextNode = this.mStart.NextNode;
                while (nextNode != -1)
                {
                    NodeData data = this.mDatas[nextNode];
                    len += this.bUseWeights ? next.Weight : 1;
                    this.mDatas[nextNode] = new NodeData(len, next, index);
                    next = data.Next;
                    index = nextNode;
                    nextNode = data.NextNode;
                }
            }
        }

        protected override void OnFinishEdge(Digraph<Node, Edge>.GEdge e, 
            bool reversed, uint depth)
        {
            NodeData uData, vData;
            int srcIndex = e.SrcNode.Index;
            int dstIndex = e.DstNode.Index;
            if (reversed)
            {
                uData = this.mDatas[dstIndex];
                vData = this.mDatas[srcIndex];
            }
            else
            {
                uData = this.mDatas[srcIndex];
                vData = this.mDatas[dstIndex];
            }
            double len = vData.Length 
                       + (this.bUseWeights ? e.Data.Weight : 1.0);
            if (this.bUndirected && uData.NextNode != -1 &&
                this.mStartIndex == (reversed ? dstIndex : srcIndex) &&
                len > this.mStart.Length)
            {
                if (len > uData.Length)
                {
                    this.mStart = new NodeData(uData);
                }
                else
                {
                    this.mStart = new NodeData();
                    this.mStart.Length = len;
                    this.mStart.Next = e.Data;
                    this.mStart.NextNode = reversed ? srcIndex : dstIndex;
                }
            }
            if (len > uData.Length)
            {
                uData.Length = len;
                uData.Next = e.Data;
                uData.NextNode = reversed ? srcIndex : dstIndex;
            }
        }

        protected override void OnBlackEdge(Digraph<Node, Edge>.GEdge e, 
            bool reversed, uint depth)
        {
            NodeData uData, vData;
            int srcIndex = e.SrcNode.Index;
            int dstIndex = e.DstNode.Index;
            if (reversed)
            {
                uData = this.mDatas[dstIndex];
                vData = this.mDatas[srcIndex];
            }
            else
            {
                uData = this.mDatas[srcIndex];
                vData = this.mDatas[dstIndex];
            }
            double len = vData.Length 
                       + (this.bUseWeights ? e.Data.Weight : 1.0);
            // Putting the mStart block here causes problems when the edge
            // is connected to a node in the longest path from the root
            if (len > uData.Length)
            {
                uData.Length = len;
                uData.Next = e.Data;
                uData.NextNode = reversed ? srcIndex : dstIndex;
            }
        }

        protected override void OnFinished()
        {
            this.CompilePaths();
            base.OnFinished();
        }

        private void CompilePaths()
        {
            double len = -1;
            int j, root = -1;
            int count = this.mDatas.Length;
            for (j = 0; j < count; j++)
            {
                if (this.mDatas[j].Length > len)
                {
                    root = j;
                    len = this.mDatas[j].Length;
                }
            }
            int[] pIndexes = new int[count];
            Node[] pNodes = new Node[count];
            Edge[] pEdges = new Edge[count];
            count = 0;
            j = root;
            while (j != -1)
            {
                pIndexes[count] = j;
                pNodes[count] = this.mGraph.NodeAt(j);
                pEdges[count] = this.mDatas[j].Next;
                count++;
                j = this.mDatas[j].NextNode;
            }
            this.mPathNodeIndexes = new int[count];
            Array.Copy(pIndexes, 0, this.mPathNodeIndexes, 0, count);
            this.mPathNodes = new Node[count];
            Array.Copy(pNodes, 0, this.mPathNodes, 0, count);
            count--;
            this.mPathEdges = new Edge[count];
            Array.Copy(pEdges, 0, this.mPathEdges, 0, count);
        }
    }
}
