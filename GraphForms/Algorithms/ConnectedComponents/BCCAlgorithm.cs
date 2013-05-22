using System;
using System.Collections.Generic;
using System.Text;
using GraphForms.Algorithms.Search;

namespace GraphForms.Algorithms.ConnectedComponents
{
    // This article was the biggest help in implementing this algorithm:
    // http://www.cs.umd.edu/class/fall2005/cmsc451/biconcomps.pdf
    public class BCCAlgorithm<Node, Edge> 
        : DepthFirstSearchAlgorithm<Node, Edge>
        where Node : class
        where Edge : class, IGraphEdge<Node>
    {
        private class NodeData
        {
            public int Depth;
            public int LowPoint;
            public bool IsCut;

            public Node Data;
            public int GroupID;
            public List<int> GIDs;

            public NodeData(Node data)
            {
                this.Data = data;
                this.GroupID = -1;
            }
        }

        private Stack<NodeData> mNodeStack;
        private Stack<Edge> mEdgeStack;
        private NodeData[] mDatas;
        private int mDepth;
        private bool mFlag = false;

        private List<Node> mArtNodes;
        private List<Edge[]> mComponents;
        private List<List<Node>> mCompGroups;

        public BCCAlgorithm(DirectionalGraph<Node, Edge> graph)
            : base(graph, true, false)
        {
            this.mNodeStack = new Stack<NodeData>(graph.NodeCount + 1);
            this.mEdgeStack = new Stack<Edge>(graph.EdgeCount + 1);
            this.mArtNodes = new List<Node>();
            this.mComponents = new List<Edge[]>();
            this.mCompGroups = new List<List<Node>>();
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
                Node[][] compGroups = new Node[this.mCompGroups.Count][];
                for (int i = 0; i < this.mCompGroups.Count; i++)
                {
                    compGroups[i] = this.mCompGroups[i].ToArray();
                }
                return compGroups;
            }
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
            int j, k, size;
            for (int i = 0; i < this.mDatas.Length; i++)
            {
                if (this.mDatas[i].GIDs != null)
                {
                    List<int> gids = this.mDatas[i].GIDs;
                    k = 0;
                    size = this.mCompGroups[gids[0]].Count;
                    for (j = 1; j < gids.Count; j++)
                    {
                        if (this.mCompGroups[gids[j]].Count > size)
                        {
                            size = this.mCompGroups[gids[j]].Count;
                            k = j;
                        }
                    }
                    if (k != 0)
                    {
                        size = this.mDatas[i].GroupID;
                        this.mCompGroups[size].Remove(this.mDatas[i].Data);
                        size = this.mDatas[i].GIDs[k];
                        this.mCompGroups[size].Add(this.mDatas[i].Data);
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
                    //if (this.mDatas[i].GIDs != null)
                    {
                        grps[count].Add(mDatas[i].Data);
                        count++;
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
                /*List<Node[]> isoGroups = new List<Node[]>(
                    this.mArtNodes.Count + this.mComponents.Count + 1);
                int i, j;
                for (i = 0; i < this.mArtNodes.Count; i++)
                {
                    isoGroups.Add(new Node[] { this.mArtNodes[i] });
                }
                List<Node> nodes;
                Edge[] edges;
                Node node;
                for (i = 0; i < this.mComponents.Count; i++)
                {
                    nodes = new List<Node>();
                    edges = this.mComponents[i];
                    for (j = 0; j < edges.Length; j++)
                    {
                        node = edges[j].SrcNode;
                        if (!this.mArtNodes.Contains(node) && 
                            !nodes.Contains(node))
                            nodes.Add(node);
                        node = edges[j].DstNode;
                        if (!this.mArtNodes.Contains(node) &&
                            !nodes.Contains(node))
                            nodes.Add(node);
                    }
                    isoGroups.Add(nodes.ToArray());
                }
                return isoGroups.ToArray();/* */
            }
        }

        public override void Initialize()
        {
            this.mDepth = 0;
            this.mNodeStack.Clear();
            this.mEdgeStack.Clear();
            DirectionalGraph<Node, Edge>.GraphNode[] nodes
                = this.mGraph.InternalNodes;
            this.mDatas = new NodeData[nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
            {
                this.mDatas[i] = new NodeData(nodes[i].mData);
            }
            base.Initialize();
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
            nData.LowPoint = this.mDepth;
            this.mDepth++;
            base.OnDiscoverNode(n, index);
        }

        protected override void OnTreeEdge(Edge e, 
            int srcIndex, int dstIndex, bool reversed)
        {
            this.mEdgeStack.Push(e);
            this.mNodeStack.Push(this.mDatas[reversed ? srcIndex : dstIndex]);
            base.OnTreeEdge(e, srcIndex, dstIndex, reversed);
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
            if (vData.LowPoint >= uData.Depth)
                this.OnComponent(e, reversed ? dstIndex : srcIndex);
            uData.LowPoint = Math.Min(uData.LowPoint, vData.LowPoint);
            base.OnFinishEdge(e, srcIndex, dstIndex, reversed);
        }

        // TODO: For compact groups, figure out a way to get larger groups
        // to take articulation nodes from smaller adjacent groups.
        // Will this make the graph tidier in the circular balloon layout?
        private void OnComponent(Edge edge, int fromNodeIndex)
        {
            List<Node> cGrp = new List<Node>(this.mNodeStack.Count + 1);
            List<Edge> comp = new List<Edge>(this.mEdgeStack.Count + 1);
            NodeData data;
            Edge e = null;
            while (e != edge)
            {
                e = this.mEdgeStack.Pop();
                comp.Add(e);
                data = this.mNodeStack.Pop();
                if (data.GroupID == -1)
                {
                    cGrp.Add(data.Data);
                    data.GroupID = this.mCompGroups.Count;
                }
                else
                {
                    if (data.GIDs == null)
                    {
                        //this.mArtNodes.Add(data.Data);
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
                    this.OnArticulationNode(fromNodeIndex);
                }
                else
                {
                    this.mFlag = true;
                }
            }
            else
            {
                this.OnArticulationNode(fromNodeIndex);
            }/* */
            this.mCompGroups.Add(cGrp);
        }

        private void OnArticulationNode(int index)
        {
            if (!this.mDatas[index].IsCut)
            {
                this.mDatas[index].IsCut = true;
                Node node = this.mGraph.NodeAt(index);
                this.mArtNodes.Add(node);
            }
        }/* */

        protected override void OnFinishNode(Node n, int index)
        {
            
            base.OnFinishNode(n, index);
        }

        protected override void OnBackEdge(Edge e, 
            int srcIndex, int dstIndex, bool reversed)
        {
            this.mEdgeStack.Push(e);
            this.mNodeStack.Push(this.mDatas[reversed ? srcIndex : dstIndex]);
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
            uData.LowPoint = Math.Min(uData.LowPoint, vData.Depth);
            base.OnBackEdge(e, srcIndex, dstIndex, reversed);
        }

        protected override void OnForwardOrCrossEdge(Edge e, 
            int srcIndex, int dstIndex, bool reversed)
        {
            this.mEdgeStack.Push(e);
            this.mNodeStack.Push(this.mDatas[reversed ? srcIndex : dstIndex]);
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
            uData.LowPoint = Math.Min(uData.LowPoint, vData.Depth);
            base.OnForwardOrCrossEdge(e, srcIndex, dstIndex, reversed);
        }
    }
}
