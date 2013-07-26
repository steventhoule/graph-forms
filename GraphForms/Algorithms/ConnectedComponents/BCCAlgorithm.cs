using System;
using System.Collections.Generic;
using System.Text;
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
            public List<int> GIDs;

            public NodeData(Node data)
            {
                this.Data = data;
                this.Parent = -1;
                this.GroupID = -1;
            }
        }

        private class EdgeData
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
        }

        private Stack<NodeData> mNodeStack;
        private Stack<EdgeData> mEdgeStack;
        private NodeData[] mDatas;
        private int mDepth;
        private bool mFlag = false;

        private List<Node> mArtNodes;
        private List<Edge[]> mComponents;
        private List<Node[]> mCompGroups;

        public BCCAlgorithm(Digraph<Node, Edge> graph)
            : this(graph, false)
        {
        }

        public BCCAlgorithm(Digraph<Node, Edge> graph,
            bool reversed)
            : base(graph, false, reversed)
        {
            this.mNodeStack = new Stack<NodeData>(graph.NodeCount + 1);
            this.mEdgeStack = new Stack<EdgeData>(graph.EdgeCount + 1);
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
                    List<int> gids = this.mDatas[i].GIDs;
                    index = 0;
                    size = this.mCompGroups[gids[0]].Length;
                    for (j = 1; j < gids.Count; j++)
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
            this.mDepth = 0;
            this.mNodeStack.Clear();
            this.mEdgeStack.Clear();
            Digraph<Node, Edge>.GNode[] nodes
                = this.mGraph.InternalNodes;
            this.mDatas = new NodeData[nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
            {
                this.mDatas[i] = new NodeData(nodes[i].mData);
            }
        }

        protected override void OnStartNode(Node n, int index)
        {
            this.mFlag = false;
            base.OnStartNode(n, index);
        }

        protected override void OnDiscoverNode(Node n, int index)
        {
            NodeData nData = this.mDatas[index];
            nData.Depth = this.mDepth;
            nData.LowLink = this.mDepth;
            this.mDepth++;
            base.OnDiscoverNode(n, index);
        }

        protected override void OnTreeEdge(Edge e, 
            int srcIndex, int dstIndex, bool reversed)
        {
            this.mEdgeStack.Push(new EdgeData(e, srcIndex, dstIndex));
            NodeData vData = this.mDatas[reversed ? srcIndex : dstIndex];
            vData.Parent = reversed ? dstIndex : srcIndex;
            this.mNodeStack.Push(vData);
            base.OnTreeEdge(e, srcIndex, dstIndex, reversed);
        }

        protected override void OnFinishEdge(Edge e, 
            int srcIndex, int dstIndex, bool reversed)
        {
            NodeData uDat, vDat;
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
                this.OnComponent(e, srcIndex, dstIndex, reversed);
            }
            uDat.LowLink = Math.Min(uDat.LowLink, vDat.LowLink);
            base.OnFinishEdge(e, srcIndex, dstIndex, reversed);
        }

        private void OnComponent(Edge edge, int src, int dst, bool rev)
        {
            List<Node> cGrp = new List<Node>(this.mNodeStack.Count + 1);
            List<Edge> comp = new List<Edge>(this.mEdgeStack.Count + 1);
            NodeData data;
            EdgeData e = new EdgeData(edge, -1, -1);
            while (e.SrcIndex != src || e.DstIndex != dst)
            {
                e = this.mEdgeStack.Pop();
                comp.Add(e.Data);
                data = this.mNodeStack.Pop();
                if (data.GroupID == -1)
                {
                    cGrp.Add(data.Data);
                    data.GroupID = this.mCompGroups.Count;
                }
                else if (data.GroupID != this.mCompGroups.Count)
                {
                    if (data.GIDs == null)
                    {
                        data.GIDs = new List<int>();
                        data.GIDs.Add(data.GroupID);
                    }
                    data.GIDs.Add(this.mCompGroups.Count);
                }
            }
            this.mComponents.Add(comp.ToArray());
            if (this.mEdgeStack.Count == 0)
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
            this.mCompGroups.Add(cGrp.ToArray());
        }

        private void OnArticulationNode(int index)
        {
            if (!this.mDatas[index].IsCut)
            {
                this.mDatas[index].IsCut = true;
                this.mArtNodes.Add(this.mDatas[index].Data);
            }
        }

        protected override void OnGrayEdge(Edge e, 
            int srcIndex, int dstIndex, bool reversed)
        {
            
            NodeData uDat;
            int v;
            if (reversed)
            {
                uDat = this.mDatas[dstIndex];
                v = srcIndex;
            }
            else
            {
                uDat = this.mDatas[srcIndex];
                v = dstIndex;
            }
            if (uDat.Parent != v)
            {
                this.mEdgeStack.Push(new EdgeData(e, srcIndex, dstIndex));
                this.mNodeStack.Push(this.mDatas[v]);
                uDat.LowLink = Math.Min(uDat.LowLink, this.mDatas[v].Depth);
            }
            base.OnGrayEdge(e, srcIndex, dstIndex, reversed);
        }
    }
}
