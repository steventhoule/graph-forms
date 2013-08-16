using System;
using System.Collections.Generic;
using GraphForms.Algorithms.Search;

namespace GraphForms.Algorithms.ConnectedComponents
{
    // TODO: Does this need to compensate for self-loop edges?
    // This article was the biggest help in implementing this algorithm:
    // http://www.cs.umd.edu/class/fall2005/cmsc451/biconcomps.pdf
    public class BCCAlgorithm<Node, Edge> 
        : DepthFirstSearch<Node, Edge>
        where Edge : IGraphEdge<Node>
    {
        private class NodeData
        {
            public Node Data;
            public int Depth;
            public int LowLink;
            public int Parent;

            public bool IsCut;
            public int GroupID;
            // Only used for strict articulation points?
            public int[] GIDs;

            public NodeData(Node data)
            {
                this.Data = data;
                this.Parent = -1;
                this.GroupID = -1;
            }
        }

        /*private class EdgeData
        {
            public readonly Edge Data;
            public readonly int SrcIndex;
            public readonly int DstIndex;

            public EdgeData(Edge data, int srcIndex, int dstIndex)
            {
                this.Data = data;
                this.SrcIndex = srcIndex;
                this.DstIndex = dstIndex;
            }
        }/* */

        //private Stack<NodeData> mNodeStack;
        //private Stack<Digraph<Node, Edge>.GEdge> mEdgeStack;
        private int mStackCount;
        private NodeData[] mNodeStack;
        private Digraph<Node, Edge>.GEdge[] mEdgeStack;
        private NodeData[] mDatas;
        private int mDepth;
        private bool mFlag = false;

        private List<Node> mArtNodes;
        private List<Edge[]> mComponents;
        private List<Node[]> mCompGroups;

        public BCCAlgorithm(Digraph<Node, Edge> graph, bool reversed)
            : base(graph, false, reversed)
        {
            this.mDatas = new NodeData[0];
            this.mNodeStack = new NodeData[0];
            this.mEdgeStack = new Digraph<Node, Edge>.GEdge[0];
            this.mArtNodes = new List<Node>();
            this.mComponents = new List<Edge[]>();
            this.mCompGroups = new List<Node[]>();
        }

        public Edge[][] Components
        {
            get { return this.mComponents.ToArray(); }
        }

        public Node[] ArticulationNodes
        {
            get { return this.mArtNodes.ToArray(); }
        }

        public Node[][] CompactGroups
        {
            get
            {
                return this.mCompGroups.ToArray();
            }
        }

        public int[] CompactGroupIds
        {
            get
            {
                int[] gids = new int[this.mDatas.Length];
                for (int i = 0; i < this.mDatas.Length; i++)
                {
                    gids[i] = this.mDatas[i].GroupID;
                }
                return gids;
            }
        }

        public int CompactGroupCount
        {
            get { return this.mCompGroups.Count; }
        }

        public void ArticulateToLargerCompactGroups()
        {
            switch (this.State)
            {
                case ComputeState.None:
                case ComputeState.Running:
                case ComputeState.Aborting:
                    throw new InvalidOperationException();
            }
            int j, index, size;
            for (int i = 0; i < this.mDatas.Length; i++)
            {
                if (this.mDatas[i].GIDs != null)
                {
                    int[] gids = this.mDatas[i].GIDs;
                    index = 0;
                    size = this.mCompGroups[gids[0]].Length;
                    for (j = 1; j < gids.Length; j++)
                    {
                        if (this.mCompGroups[gids[j]].Length > size)
                        {
                            size = this.mCompGroups[gids[j]].Length;
                            index = j;
                        }
                    }
                    if (index != 0)
                    {
                        size = this.mDatas[i].GroupID;
                        //this.mCompGroups[size].Remove(this.mDatas[i].Data);
                        int len = this.mCompGroups[size].Length;
                        j = Array.IndexOf<Node>(this.mCompGroups[size], 
                            this.mDatas[i].Data, 0, len);
                        Node[] nodes = new Node[len - 1];
                        Array.Copy(this.mCompGroups[size], 0, nodes, 0, j);
                        Array.Copy(this.mCompGroups[size], j + 1, nodes, j, 
                            len - j - 1);
                        this.mCompGroups[size] = nodes;

                        size = this.mDatas[i].GIDs[index];
                        //this.mCompGroups[size].Add(this.mDatas[i].Data);
                        len = this.mCompGroups[size].Length;
                        nodes = new Node[len + 1];
                        Array.Copy(this.mCompGroups[size], 0, nodes, 0, len);
                        nodes[len] = this.mDatas[i].Data;
                        this.mCompGroups[size] = nodes;

                        this.mDatas[i].GroupID = size;
                    }
                }
            }
        }

        public Node[][] IsolatedGroups
        {
            get
            {
                int i, count = this.mArtNodes.Count + this.mComponents.Count;
                List<Node>[] grps = new List<Node>[count];
                for (i = 0; i < count; i++)
                {
                    grps[i] = new List<Node>(2);
                }
                count = this.mComponents.Count;
                for (i = 0; i < this.mDatas.Length; i++)
                {
                    if (this.mDatas[i].IsCut)
                    {
                        grps[count++].Add(this.mDatas[i].Data);
                    }
                    else
                    {
                        grps[this.mDatas[i].GroupID].Add(this.mDatas[i].Data);
                    }
                }
                count = this.mArtNodes.Count + this.mComponents.Count;
                Node[][] isoGroups = new Node[count][];
                for (i = 0; i < count; i++)
                {
                    isoGroups[i] = grps[i].ToArray();
                }
                return isoGroups;
            }
        }

        public int[] IsolatedGroupIds
        {
            get
            {
                int count = this.mComponents.Count;
                int[] gids = new int[this.mDatas.Length];
                for (int i = 0; i < this.mDatas.Length; i++)
                {
                    gids[i] = this.mDatas[i].IsCut 
                        ? count++ : this.mDatas[i].GroupID;
                }
                return gids;
            }
        }

        public int IsolatedGroupCount
        {
            get { return this.mArtNodes.Count + this.mComponents.Count; }
        }

        public override void Initialize()
        {
            base.Initialize();
            int count = this.mGraph.NodeCount;
            this.mDepth = 0;
            //this.mNodeStack.Clear();
            //this.mEdgeStack.Clear();
            if (this.mDatas.Length < count)
            {
                this.mDatas = new NodeData[count];
            }
            for (int i = 0; i < count; i++)
            {
                this.mDatas[i] = new NodeData(this.mGraph.NodeAt(i));
            }
            count = this.mGraph.EdgeCount;
            if (this.mNodeStack.Length < count)
            {
                this.mNodeStack = new NodeData[count];
                this.mEdgeStack = new Digraph<Node, Edge>.GEdge[count];
            }
            this.mStackCount = 0;
        }

        protected override void OnStartNode(Digraph<Node, Edge>.GNode n)
        {
            this.mFlag = false;
        }

        protected override void OnDiscoverNode(
            Digraph<Node, Edge>.GNode n, uint depth)
        {
            NodeData nData = this.mDatas[n.Index];
            nData.Depth = this.mDepth;
            nData.LowLink = this.mDepth;
            this.mDepth++;
        }

        protected override void OnTreeEdge(Digraph<Node, Edge>.GEdge e, 
            bool reversed, uint depth)
        {
            //this.mEdgeStack.Push(e);
            this.mEdgeStack[this.mStackCount] = e;
            NodeData vData 
                = this.mDatas[reversed ? e.SrcNode.Index : e.DstNode.Index];//srcIndex : dstIndex];
            vData.Parent = reversed ? e.DstNode.Index : e.SrcNode.Index;//dstIndex : srcIndex;
            //this.mNodeStack.Push(vData);
            this.mNodeStack[this.mStackCount] = vData;
            this.mStackCount++;
        }

        protected override void OnFinishEdge(Digraph<Node, Edge>.GEdge e, 
            bool reversed, uint depth)
        {
            NodeData uDat, vDat;
            int srcIndex = e.SrcNode.Index;
            int dstIndex = e.DstNode.Index;
            if (reversed)
            {
                uDat = this.mDatas[dstIndex];
                vDat = this.mDatas[srcIndex];
            }
            else
            {
                uDat = this.mDatas[srcIndex];
                vDat = this.mDatas[dstIndex];
            }
            if (vDat.LowLink >= uDat.Depth)
            {
                this.OnComponent(e.Data, srcIndex, dstIndex, reversed);
            }
            uDat.LowLink = Math.Min(uDat.LowLink, vDat.LowLink);
        }

        private void OnComponent(Edge edge, int src, int dst, bool rev)
        {
            Node[] cGrp = new Node[this.mStackCount];
            Edge[] comp = new Edge[this.mStackCount];
            int cGrpCount = 0;
            int compCount = 0;
            NodeData data;
            //EdgeData e = new EdgeData(edge, -1, -1);
            //while (e.SrcIndex != src || e.DstIndex != dst)
            Digraph<Node, Edge>.GEdge e;
            do
            {
                this.mStackCount--;
                e = this.mEdgeStack[this.mStackCount];//this.mEdgeStack.Pop();
                comp[compCount++] = e.Data;
                data = this.mNodeStack[this.mStackCount];//this.mNodeStack.Pop();
                if (data.GroupID == -1)
                {
                    cGrp[cGrpCount++] = data.Data;
                    data.GroupID = this.mCompGroups.Count;
                }
                else if (data.GroupID != this.mCompGroups.Count)
                {
                    if (data.GIDs == null)
                    {
                        data.GIDs = new int[2];
                        data.GIDs[0] = data.GroupID;
                        data.GIDs[1] = this.mCompGroups.Count;
                    }
                    else
                    {
                        int count = data.GIDs.Length;
                        int[] gids = new int[count + 1];
                        Array.Copy(data.GIDs, 0, gids, 0, count);
                        gids[count] = this.mCompGroups.Count;
                        data.GIDs = gids;
                    }
                }
            }
            while (e.SrcNode.Index != src || e.DstNode.Index != dst);

            Edge[] comp2 = new Edge[compCount];
            Array.Copy(comp, 0, comp2, 0, compCount);
            this.mComponents.Add(comp2);
            if (this.mStackCount == 0)//this.mEdgeStack.Count == 0)
            {
                if (this.mFlag)
                {
                    this.OnArticulationNode(rev ? dst : src);
                }
                else
                {
                    this.mFlag = true;
                }
            }
            else
            {
                this.OnArticulationNode(rev ? dst : src);
            }
            Node[] cGrp2 = new Node[cGrpCount];
            Array.Copy(cGrp, 0, cGrp2, 0, cGrpCount);
            this.mCompGroups.Add(cGrp2);
        }

        private void OnArticulationNode(int index)
        {
            if (!this.mDatas[index].IsCut)
            {
                this.mDatas[index].IsCut = true;
                this.mArtNodes.Add(this.mDatas[index].Data);
            }
        }

        protected override void OnGrayEdge(Digraph<Node, Edge>.GEdge e, 
            bool reversed, uint depth)
        {
            
            NodeData uDat;
            int v;
            if (reversed)
            {
                uDat = this.mDatas[e.DstNode.Index];//dstIndex];
                v = e.SrcNode.Index;//srcIndex;
            }
            else
            {
                uDat = this.mDatas[e.SrcNode.Index];//srcIndex];
                v = e.DstNode.Index;//dstIndex;
            }
            if (uDat.Parent != v)
            {
                //this.mEdgeStack.Push(e);
                //this.mNodeStack.Push(this.mDatas[v]);
                this.mEdgeStack[this.mStackCount] = e;
                this.mNodeStack[this.mStackCount] = this.mDatas[v];
                this.mStackCount++;
                uDat.LowLink = Math.Min(uDat.LowLink, this.mDatas[v].Depth);
            }
        }
    }
}
