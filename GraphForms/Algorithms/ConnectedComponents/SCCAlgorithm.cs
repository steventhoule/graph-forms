using System;
using GraphForms.Algorithms.Search;

namespace GraphForms.Algorithms.ConnectedComponents
{
    // Based on Tarjan's Algorithm for Strongly Connected Components
    // http://en.wikipedia.org/wiki/Tarjan%27s_strongly_connected_components_algorithm
    public class SCCAlgorithm<Node, Edge>
        : DepthFirstSearch<Node, Edge>, ICCAlgorithm<Node, Edge>
        where Edge : IGraphEdge<Node>
    {
        private class NodeData : IEquatable<NodeData>
        {
            public int Depth;
            public int LowLink;
            public Node Data;

            public NodeData(Node data)
            {
                this.Depth = -1;
                this.LowLink = -1;
                this.Data = data;
            }

            public bool Equals(NodeData other)
            {
                return this.Depth == other.Depth;
            }
        }

        //private Stack<int> mStack;
        private int mStackCount;
        private int[] mStack;
        private NodeData[] mDatas;
        private int mDepth;

        private int mCompCount;
        private int[] mCompIds;
        private Digraph<Node, Edge>.GNode[] mRoots;

        public SCCAlgorithm(Digraph<Node, Edge> graph, bool reversed)
            : base(graph, true, reversed)
        {
            //this.mStack = new Stack<int>(graph.NodeCount + 1);
            this.mDatas = new NodeData[0];
            this.mStack = new int[0];
            this.mCompCount = 0;
            this.mCompIds = new int[0];
            this.mRoots = new Digraph<Node, Edge>.GNode[0];
        }

        public int ComponentCount
        {
            get { return this.mCompCount; }
        }

        public int[] ComponentIds
        {
            get
            {
                int count = this.mGraph.NodeCount;
                int[] compIds = new int[count];
                Array.Copy(this.mCompIds, 0, compIds, 0, count);
                return compIds;
            }
        }

        public Digraph<Node, Edge>.GNode[][] Components
        {
            get
            {
                int i, j, count, nCount = this.mGraph.NodeCount;
                Digraph<Node, Edge>.GNode[] comp
                    = new Digraph<Node, Edge>.GNode[nCount];
                Digraph<Node, Edge>.GNode[][] comps
                    = new Digraph<Node, Edge>.GNode[this.mCompCount][];
                for (i = 0; i < this.mCompCount; i++)
                {
                    count = 0;
                    for (j = 0; j < nCount; j++)
                    {
                        if (this.mCompIds[j] == i)
                        {
                            comp[count++] = this.mGraph.InternalNodeAt(j);
                        }
                    }
                    comps[i] = new Digraph<Node, Edge>.GNode[count];
                    Array.Copy(comp, 0, comps[i], 0, count);
                }
                return comps;
            }
        }

        public Digraph<Node, Edge>.GNode[] ComponentRoots
        {
            get
            {
                Digraph<Node, Edge>.GNode[] roots
                    = new Digraph<Node, Edge>.GNode[this.mCompCount];
                Array.Copy(this.mRoots, 0, roots, 0, this.mCompCount);
                return roots;
            }
        }

        public override void Initialize()
        {
            this.mDepth = 0;
            int count = this.mGraph.NodeCount;
            if (this.mDatas.Length < count)
            {
                this.mDatas = new NodeData[count];
                this.mStack = new int[count];
                this.mCompIds = new int[count];
            }
            for (int i = 0; i < count; i++)
            {
                this.mDatas[i] = new NodeData(this.mGraph.NodeAt(i));
                this.mCompIds[i] = -1;
            }
            this.mStackCount = 0;
            this.mCompCount = 0;
            base.Initialize();
        }

        protected override void OnDiscoverNode(
            Digraph<Node, Edge>.GNode n, uint depth)
        {
            NodeData data = this.mDatas[n.Index];
            if (data.Depth == -1)
            {
                data.Depth = this.mDepth;
                data.LowLink = this.mDepth;
                this.mDepth++;
                //this.mStack.Push(n.Index);
                this.mStack[this.mStackCount++] = n.Index;
            }
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
            uDat.LowLink = Math.Min(uDat.LowLink, vDat.LowLink);
        }

        protected override void OnBlackEdge(Digraph<Node, Edge>.GEdge e,
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
            int i;
            for (i = 0; i < this.mStackCount; i++)
            {
                if (this.mStack[i] == v)
                    break;
            }
            if (i < this.mStackCount)//this.mStack.Contains(v))
            {
                uDat.LowLink = Math.Min(uDat.LowLink, this.mDatas[v].Depth);
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
            int i;
            for (i = 0; i < this.mStackCount; i++)
            {
                if (this.mStack[i] == v)
                    break;
            }
            if (i < this.mStackCount)//this.mStack.Contains(v))
            {
                uDat.LowLink = Math.Min(uDat.LowLink, this.mDatas[v].Depth);
            }
        }

        protected override void OnFinishNode(
            Digraph<Node, Edge>.GNode n, uint depth)
        {
            int index = n.Index;
            if (this.mDatas[index].LowLink == this.mDatas[index].Depth)
            {
                /*this.mRoots.Add(n.Data);
                int count = 0;
                Node[] comp = new Node[this.mStackCount];
                int i = -1;
                while (i != index)
                {
                    i = this.mStack[--this.mStackCount];//this.mStack.Pop();
                    comp[count++] = this.mDatas[i].Data;
                }
                Node[] newComp = new Node[count];
                Array.Copy(comp, 0, newComp, 0, count);
                this.mComponents.Add(newComp);/* */
                
                if (this.mCompCount == this.mRoots.Length)
                {
                    Digraph<Node, Edge>.GNode[] roots;
                    if (this.mCompCount == 0)
                    {
                        roots = new Digraph<Node, Edge>.GNode[4];
                    }
                    else
                    {
                        roots = new Digraph<Node, Edge>.GNode[2 * this.mCompCount];
                        Array.Copy(this.mRoots, 0, roots, 0, this.mCompCount);
                    }
                    this.mRoots = roots;
                }
                this.mRoots[this.mCompCount] = n;

                int i = -1;
                while (i != index)
                {
                    i = this.mStack[--this.mStackCount];
                    this.mCompIds[i] = this.mCompCount;
                }
                this.mCompCount++;
            }
        }
    }
}
