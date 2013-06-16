﻿using System;
using System.Collections.Generic;
using System.Text;
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

        public DFLongestPath(Digraph<Node, Edge> graph)
            : base(graph)
        {
        }

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

        protected override void OnStartNode(Node n, int index)
        {
            this.mStartIndex = index;
            this.mStart = new NodeData();
            base.OnStartNode(n, index);
        }

        protected override void OnFinishNode(Node n, int index)
        {
            if (this.bUndirected && this.mStartIndex == index)
            {
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
            base.OnFinishNode(n, index);
        }

        protected override void OnFinishEdge(Edge e, 
            int srcIndex, int dstIndex, bool reversed)
        {
            NodeData uData, vData;
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
            double len = vData.Length + (this.bUseWeights ? e.Weight : 1);
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
                    this.mStart.Next = e;
                    this.mStart.NextNode = reversed ? srcIndex : dstIndex;
                }
            }
            if (len > uData.Length)
            {
                uData.Length = len;
                uData.Next = e;
                uData.NextNode = reversed ? srcIndex : dstIndex;
            }
            base.OnFinishEdge(e, srcIndex, dstIndex, reversed);
        }

        protected override void OnBlackEdge(Edge e, 
            int srcIndex, int dstIndex, bool reversed)
        {
            NodeData uData, vData;
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
            double len = vData.Length + (this.bUseWeights ? e.Weight : 1);
            // Putting the mStart block here causes problems when the edge
            // is connected to a node in the longest path from the root
            if (len > uData.Length)
            {
                uData.Length = len;
                uData.Next = e;
                uData.NextNode = reversed ? srcIndex : dstIndex;
            }
            base.OnBlackEdge(e, srcIndex, dstIndex, reversed);
        }

        protected override void OnFinished()
        {
            this.CompilePaths();
            base.OnFinished();
        }

        private void CompilePaths()
        {
            int j, root = -1;
            double len = -1;
            for (j = 0; j < this.mDatas.Length; j++)
            {
                if (this.mDatas[j].Length > len)
                {
                    root = j;
                    len = this.mDatas[j].Length;
                }
            }
            List<int> pIndexes = new List<int>();
            List<Node> pNodes = new List<Node>();
            List<Edge> pEdges = new List<Edge>();
            j = root;
            while (j != -1)
            {
                pIndexes.Add(j);
                pNodes.Add(this.mGraph.NodeAt(j));
                pEdges.Add(this.mDatas[j].Next);
                j = this.mDatas[j].NextNode;
            }
            this.mPathNodeIndexes = pIndexes.ToArray();
            this.mPathNodes = pNodes.ToArray();
            this.mPathEdges = new Edge[pEdges.Count - 1];
            pEdges.CopyTo(0, this.mPathEdges, 0, pEdges.Count - 1);
        }
    }
}
