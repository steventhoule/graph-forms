using System;
using System.Collections.Generic;
using System.Text;
using GraphForms.Algorithms.Search;

namespace GraphForms.Algorithms.ConnectedComponents
{
    // Based on Tarjan's Algorithm for Strongly Connected Components
    // http://en.wikipedia.org/wiki/Tarjan%27s_strongly_connected_components_algorithm
    public class SCCAlgorithm<Node, Edge>
        : DepthFirstSearch<Node, Edge>, ICCAlgorithm<Node>
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

        private Stack<int> mNodeStack;
        private NodeData[] mDatas;
        private int mDepth;

        private List<Node> mRoots;
        private List<Node[]> mComponents;

        public SCCAlgorithm(Digraph<Node, Edge> graph)
            : this(graph, false)
        {
        }

        public SCCAlgorithm(Digraph<Node, Edge> graph,
            bool reversed)
            : base(graph, true, reversed)
        {
            this.mNodeStack = new Stack<int>(graph.NodeCount + 1);
            this.mRoots = new List<Node>();
            this.mComponents = new List<Node[]>();
        }

        public Node[][] Components
        {
            get { return this.mComponents.ToArray(); }
        }

        public Node[] Roots
        {
            get { return this.mRoots.ToArray(); }
        }

        public override void Initialize()
        {
            this.mDepth = 0;
            Digraph<Node, Edge>.GNode[] nodes
                = this.mGraph.InternalNodes;
            this.mDatas = new NodeData[nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
            {
                this.mDatas[i] = new NodeData(nodes[i].mData);
            }
            base.Initialize();
        }

        protected override void OnDiscoverNode(Node n, int index)
        {
            NodeData data = this.mDatas[index];
            if (data.Depth == -1)
            {
                data.Depth = this.mDepth;
                data.LowLink = this.mDepth;
                this.mDepth++;
                this.mNodeStack.Push(index);
            }
            base.OnDiscoverNode(n, index);
        }

        protected override void OnFinishEdge(Edge e, int srcIndex, 
            int dstIndex, bool reversed)
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
            uDat.LowLink = Math.Min(uDat.LowLink, vDat.LowLink);
            base.OnFinishEdge(e, srcIndex, dstIndex, reversed);
        }

        protected override void OnBlackEdge(Edge e, int srcIndex, 
            int dstIndex, bool reversed)
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
            if (this.mNodeStack.Contains(v))
            {
                uDat.LowLink = Math.Min(uDat.LowLink, this.mDatas[v].Depth);
            }
            base.OnBlackEdge(e, srcIndex, dstIndex, reversed);
        }

        protected override void OnGrayEdge(Edge e, int srcIndex, 
            int dstIndex, bool reversed)
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
            if (this.mNodeStack.Contains(v))
            {
                uDat.LowLink = Math.Min(uDat.LowLink, this.mDatas[v].Depth);
            }
            base.OnGrayEdge(e, srcIndex, dstIndex, reversed);
        }

        protected override void OnFinishNode(Node n, int index)
        {
            if (this.mDatas[index].LowLink == this.mDatas[index].Depth)
            {
                this.mRoots.Add(n);
                List<Node> comp = new List<Node>();
                int i = -1;
                while (i != index)
                {
                    i = this.mNodeStack.Pop();
                    comp.Add(this.mDatas[i].Data);
                }
                this.mComponents.Add(comp.ToArray());
            }
            base.OnFinishNode(n, index);
        }
    }
}
