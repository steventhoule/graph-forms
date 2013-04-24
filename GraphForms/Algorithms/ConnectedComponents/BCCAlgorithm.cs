using System;
using System.Collections.Generic;
using System.Text;
using GraphForms.Algorithms.Search;

namespace GraphForms.Algorithms.ConnectedComponents
{
    // This article was the biggest help in implementing this algorithm:
    // http://www.cs.umd.edu/class/fall2005/cmsc451/biconcomps.pdf
    public class BCCAlgorithm<Node, Edge> 
        : ARootedAlgorithm<Node>
        where Node : class
        where Edge : class, IGraphEdge<Node>
    {
        private class NodeData
        {
            public int Depth;
            public int LowPoint;
            //public DirectionalGraph<Node, Edge>.GraphNode Parent;
        }

        private DirectionalGraph<Node, Edge> mGraph;

        private Stack<Edge> mEdgeStack;
        private NodeData[] mDatas;
        private int mDepth;

        private List<Edge[]> mComponents;

        public BCCAlgorithm(DirectionalGraph<Node, Edge> graph)
        {
            this.mGraph = graph;
            this.mEdgeStack = new Stack<Edge>(graph.EdgeCount + 1);
            this.mComponents = new List<Edge[]>();
        }

        public Edge[][] Components
        {
            get { return this.mComponents.ToArray(); }
        }

        private void Initialize()
        {
            this.mDepth = 0;
            this.mEdgeStack.Clear();
            DirectionalGraph<Node, Edge>.GraphNode[] nodes
                = this.mGraph.InternalNodes;
            this.mDatas = new NodeData[nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i].Color = GraphColor.White;
                nodes[i].Index = i;
                this.mDatas[i] = new NodeData();
            }
            DirectionalGraph<Node, Edge>.GraphEdge[] edges
                = this.mGraph.InternalEdges;
            for (int j = 0; j < edges.Length; j++)
            {
                edges[j].Color = GraphColor.White;
            }
        }

        protected override void InternalCompute()
        {
            if (this.mGraph.NodeCount == 0 || this.mGraph.EdgeCount == 0)
                return;

            this.Initialize();

            DirectionalGraph<Node, Edge>.GraphNode node;

            // if there is a starting vertex, start with it
            if (this.HasRoot)
            {
                // equeue select root only
                int index = this.mGraph.IndexOfNode(this.TryGetRoot());
                if (index >= 0)
                {
                    node = this.mGraph.InternalNodeAt(index);
                    this.Visit(node);
                }
            }

            // process each node
            DirectionalGraph<Node, Edge>.GraphNode[] nodes
                = this.mGraph.InternalNodes;
            for (int i = 0; i < nodes.Length; i++)
            {
                if (this.State == ComputeState.Aborting)
                    return;
                node = nodes[i];
                if (!node.Visited)
                {
                    this.Visit(node);
                }
            }
        }

        private void Visit(DirectionalGraph<Node, Edge>.GraphNode u)
        {
            u.Color = GraphColor.Gray;
            NodeData vData, uData = this.mDatas[u.Index];
            uData.Depth = this.mDepth;
            uData.LowPoint = this.mDepth;
            this.mDepth++;

            DirectionalGraph<Node, Edge>.GraphNode v;
            DirectionalGraph<Node, Edge>.GraphEdge e;
            DirectionalGraph<Node, Edge>.GraphEdge[] edges
                = u.AllInternalEdges(false);
            for (int i = 0; i < edges.Length; i++)
            {
                e = edges[i];
                if (e.Color == GraphColor.Black) // edge already visited
                    continue;
                e.Color = GraphColor.Black;

                v = e.DstNode;
                if (v.Equals(u))
                    v = e.SrcNode;
                vData = this.mDatas[v.Index];

                switch (v.Color)
                {
                    case GraphColor.White:
                        this.mEdgeStack.Push(e.Data);
                        this.Visit(v);
                        if (vData.LowPoint >= uData.Depth)
                            this.OnComponent(e.Data);
                        uData.LowPoint = Math.Min(uData.LowPoint, vData.LowPoint);
                        break;
                    case GraphColor.Gray:
                        this.mEdgeStack.Push(e.Data);
                        uData.LowPoint = Math.Min(uData.LowPoint, vData.Depth);
                        break;
                    case GraphColor.Black:
                        this.mEdgeStack.Push(e.Data);
                        uData.LowPoint = Math.Min(uData.LowPoint, vData.Depth);
                        break;
                }
                /*if (!node.Visited)
                {
                    this.mEdgeStack.Push(edge.Data);
                    vData.Parent = root;
                    this.Visit(node);
                    if (vData.LowPoint >= uData.Depth)
                        this.OnComponent(edge.Data);
                    uData.LowPoint = Math.Min(uData.LowPoint, vData.LowPoint);
                }
                else if (uData.Parent != node && vData.Depth < uData.Depth)
                {
                    // (u,v) is a back edge from u to its ancestor v
                    this.mEdgeStack.Push(edge.Data);
                    uData.LowPoint = Math.Min(uData.LowPoint, vData.Depth);
                }/* */
            }
            u.Color = GraphColor.Black;
        }

        private void OnComponent(Edge edge)
        {
            List<Edge> comp = new List<Edge>(this.mEdgeStack.Count + 1);
            Edge e = null;
            while (e != edge)
            {
                e = this.mEdgeStack.Pop();
                comp.Add(e);
            }
            this.mComponents.Add(comp.ToArray());
        }
    }

    public class BCCAlgorithm2<Node, Edge> 
        : DepthFirstSearchAlgorithm<Node, Edge>
        where Node : class
        where Edge : class, IGraphEdge<Node>
    {
        private class NodeData
        {
            public int Depth;
            public int LowPoint;
        }

        private Stack<Edge> mEdgeStack;
        private NodeData[] mDatas;
        private int mDepth;

        private List<Edge[]> mComponents;

        public BCCAlgorithm2(DirectionalGraph<Node, Edge> graph)
            : base(graph, true, false)
        {
            this.mEdgeStack = new Stack<Edge>(graph.EdgeCount + 1);
            this.mComponents = new List<Edge[]>();
        }

        public Edge[][] Components
        {
            get { return this.mComponents.ToArray(); }
        }

        public override void Initialize()
        {
            this.mDepth = 0;
            this.mEdgeStack.Clear();
            int count = this.mGraph.NodeCount;
            this.mDatas = new NodeData[count];
            for (int i = 0; i < count; i++)
            {
                this.mDatas[i] = new NodeData();
            }
            base.Initialize();
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
                this.OnComponent(e);
            uData.LowPoint = Math.Min(uData.LowPoint, vData.LowPoint);
            base.OnFinishEdge(e, srcIndex, dstIndex, reversed);
        }

        private void OnComponent(Edge edge)
        {
            List<Edge> comp = new List<Edge>(this.mEdgeStack.Count + 1);
            Edge e = null;
            while (e != edge)
            {
                e = this.mEdgeStack.Pop();
                comp.Add(e);
            }
            this.mComponents.Add(comp.ToArray());
        }

        protected override void OnBackEdge(Edge e, 
            int srcIndex, int dstIndex, bool reversed)
        {
            this.mEdgeStack.Push(e);
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
