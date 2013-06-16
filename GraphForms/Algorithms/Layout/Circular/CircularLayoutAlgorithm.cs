using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using GraphForms.Algorithms.ConnectedComponents;

namespace GraphForms.Algorithms.Layout.Circular
{
    public class CircularLayoutAlgorithm<Node, Edge>
        : LayoutAlgorithm<Node, Edge>
        where Node : GraphElement, ILayoutNode
        where Edge : class, IGraphEdge<Node>, IUpdateable
    {
        private CircularLayoutAlgorithm(Digraph<Node, Edge> graph,
            IClusterNode clusterNode)
            : base(graph, clusterNode)
        {
        }

        private CircularLayoutAlgorithm(Digraph<Node, Edge> graph,
            RectangleF boundingBox)
            : base(graph, boundingBox)
        {
        }

        private class PortNode : IPortNode
        {
            private readonly Node mNode;
            private readonly int mClusterId;
            private double mMinAngle;
            private double mMaxAngle;

            public PortNode(Node node, int clusterId)
            {
                this.mNode = node;
                this.mClusterId = clusterId;
            }

            public Node Data
            {
                get { return this.mNode; }
            }

            public int ClusterId
            {
                get { return this.mClusterId; }
            }

            public double MinAngle
            {
                get { return this.mMinAngle; }
                set { this.mMinAngle = value; }
            }

            public double MaxAngle
            {
                get { return this.mMaxAngle; }
                set { this.mMaxAngle = value; }
            }

            public RectangleF BoundingBox
            {
                get { return this.mNode.BoundingBox; }
            }

            private float mX;
            private float mY;

            public float X
            {
                get { return this.mX; }
            }

            public float Y
            {
                get { return this.mY; }
            }

            public void SetPosition(float x, float y)
            {
                this.mX = x;
                this.mY = y;
            }

            public bool PositionFixed
            {
                get { return true; }
            }

            private float mNewX;
            private float mNewY;

            public float NewX
            {
                get { return this.mNewX; }
            }

            public float NewY
            {
                get { return this.mNewY; }
            }

            public void SetNewPosition(float newX, float newY)
            {
                this.mNewX = newX;
                this.mNewY = newY;
            }
        }

        private class SubEdge : IGraphEdge<ILayoutNode>
        {
            private ILayoutNode mSrcNode;
            private ILayoutNode mDstNode;
            private Edge mEdge;

            public SubEdge(ILayoutNode srcNode, ILayoutNode dstNode, Edge edge)
            {
                this.mSrcNode = srcNode;
                this.mDstNode = dstNode;
                this.mEdge = edge;
            }

            public Edge Data
            {
                get { return this.mEdge; }
            }

            public ILayoutNode SrcNode
            {
                get { return this.mSrcNode; }
            }

            public ILayoutNode DstNode
            {
                get { return this.mDstNode; }
            }

            public float Weight
            {
                get { return this.mEdge == null ? 1 : this.mEdge.Weight; }
            }

            public void SetSrcNode(ILayoutNode srcNode)
            {
                this.mSrcNode = srcNode;
            }

            public void SetDstNode(ILayoutNode dstNode)
            {
                this.mDstNode = dstNode;
            }
        }


        private class BCCNode : GraphElement
        {
            /// <summary>
            /// The index of this cluster node in graph superstructure
            /// </summary>
            public readonly int Index;
            /// <summary>
            /// The sub-graph enclosed by this cluster node.
            /// </summary>
            public readonly Digraph<ILayoutNode, SubEdge> Graph;
            /// <summary>
            /// Each sub-graph could potentially contain a port node that
            /// represents any node not in the sub-graph.
            /// </summary>
            public readonly IPortNode[] PortNodes;

            public BCCNode(int index, int nodeCapacity, int portCount)
            {
                this.Index = index;
                this.Graph = new Digraph<ILayoutNode, SubEdge>(
                    nodeCapacity, nodeCapacity / 2);
                this.PortNodes = new PortNode[portCount];
            }

            public IPortNode[] GetPorts(int clusterId)
            {
                int i, count = this.PortNodes.Length;
                List<IPortNode> ports = new List<IPortNode>(count);
                for (i = 0; i < count; i++)
                {
                    if (this.PortNodes[i] != null && 
                        this.PortNodes[i].ClusterId == clusterId)
                        ports.Add(this.PortNodes[i]);
                }
                return ports.ToArray();
            }

            protected override void OnDrawBackground(
                System.Windows.Forms.PaintEventArgs e)
            {
                throw new NotImplementedException();
            }
        }

        private class BCCEdge
            : IGraphEdge<BCCNode>, IUpdateable
        {
            private BCCNode mSrcNode;
            private BCCNode mDstNode;
            private List<IPortNode> mSrcPorts;
            private List<IPortNode> mDstPorts;

            public BCCEdge(BCCNode srcNode, BCCNode dstNode)
            {
                this.mSrcNode = srcNode;
                this.mDstNode = dstNode;
                this.mSrcPorts = new List<IPortNode>();
                this.mDstPorts = new List<IPortNode>();
            }

            public BCCNode SrcNode
            {
                get { return this.mSrcNode; }
            }

            public BCCNode DstNode
            {
                get { return this.mDstNode; }
            }

            public float Weight
            {
                get { return 1; }
            }

            public void SetSrcNode(BCCNode srcNode)
            {
                throw new InvalidOperationException();
            }

            public void SetDstNode(BCCNode dstNode)
            {
                throw new InvalidOperationException();
            }

            /// <summary>
            /// Ports in the source cluster node's sub-graph that point
            /// to the destination cluster node or nodes
            /// in the destination cluster node's sub-graph.
            /// </summary>
            public IPortNode[] SrcPorts
            {
                get 
                {
                    return this.mSrcPorts.ToArray();
                    //return this.mSrcNode.GetPorts(this.mDstNode.Index); 
                }
            }

            /// <summary>
            /// Ports in the destination cluster node's sub-graph that point
            /// to the source cluster node or nodes
            /// in the source cluster node's sub-graph.
            /// </summary>
            public IPortNode[] DstPorts
            {
                get 
                {
                    return this.mDstPorts.ToArray();
                    //return this.mDstNode.GetPorts(this.mSrcNode.Index); 
                }
            }

            public void AddSrcPort(IPortNode port)
            {
                this.mSrcPorts.Add(port);
            }

            public void AddDstPort(IPortNode port)
            {
                this.mDstPorts.Add(port);
            }

            public void Update()
            {
                throw new NotImplementedException();
            }
        }

        // Possible force-directed generalized three stage process:
        // 1. Calculate layout of Digraph<BCCNode, BCCEdge>
        // 2. In each BCCNode, for each edge connecting a node
        //    in that BCCNode to another BCCNode or outside Node,
        //    create a "port", which is a temporary node pinned on the
        //    outside rim of the BCCNode at the angle of the BCCEdge
        //    containing the edge connecting it to an outside BCCNode/Node
        // 3. Calculate the layout of graphs in each BCCNode
        //    and then remove the temporary "port" nodes and edges.
        // Perhaps "port" nodes might not even need to be temporary,
        // since the sub-graphs are created on-the-fly for the layout only.

        // Perhaps generalize to a three stage process:
        // 1. Calculate layout of graphs in each BCCNode
        // 2. Calculate layout of quasi-graphs in each BCCEdge;
        //    Perhaps special rules for edges connecting Nodes to BCCNodes?
        // 3. Calculate layout of Digraph<BCCNode, BCCEdge>
        // Perhaps effects as follows
        // 1. Position of each Node | Size of each BCCNode
        // 2. Position of each Node | Position? of each BCCNode
        // 3.                         Position of each BCCNode
        private static Digraph<BCCNode, BCCEdge> BCCCompactGraph(
            Digraph<Node, Edge> graph)
        {
            int i, j;
            Digraph<Node, Edge>.GNode[] nodes
                = graph.InternalNodes;
            Digraph<Node, Edge>.GEdge[] edges
                = graph.InternalEdges;

            BCCAlgorithm<Node, Edge> bccAlg 
                = new BCCAlgorithm<Node, Edge>(graph);
            bccAlg.Compute();
            bccAlg.ArticulateToLargerCompactGroups();
            Node[][] bccGroups = bccAlg.CompactGroups;
            int bccGroupCount = bccAlg.CompactGroupCount;
            int[] bccGroupIds = bccAlg.CompactGroupIds;
            Digraph<BCCNode, BCCEdge> bccGraph 
                = new Digraph<BCCNode, BCCEdge>(
                    bccGroupCount, bccGroupCount / 2);

            PortNode[][] portNodes = new PortNode[bccGroupCount][];
            BCCEdge[][] bccEdges = new BCCEdge[bccGroupCount][];
            BCCNode[] bccNodes = new BCCNode[bccGroupCount];
            BCCNode bccNode;
            Node[] bccGroup;
            for (i = 0; i < bccGroupCount; i++)
            {
                bccGroup = bccGroups[i];
                bccNode = new BCCNode(i, bccGroup.Length, nodes.Length);
                for (j = 0; j < bccGroup.Length; j++)
                {
                    bccNode.Graph.AddNode(bccGroup[j]);
                }
                bccGraph.AddNode(bccNode);
                bccNodes[i] = bccNode;
                bccEdges[i] = new BCCEdge[bccGroupCount];
                portNodes[i] = new PortNode[nodes.Length];
            }
            
            int si, di;
            Digraph<Node, Edge>.GEdge edge;
            BCCEdge bccEdge;
            PortNode spn, dpn;
            for (i = 0; i < edges.Length; i++)
            {
                edge = edges[i];
                si = bccGroupIds[edge.mSrcNode.Index];
                di = bccGroupIds[edge.mDstNode.Index];
                if (si == di)
                {
                    bccNodes[si].Graph.AddEdge(new SubEdge(
                        edge.mSrcNode.mData, edge.mDstNode.mData, 
                        edge.mData));
                }
                else
                {
                    bccEdge = bccEdges[si][di];
                    if (bccEdge == null)
                    {
                        bccEdge = new BCCEdge(bccNodes[si], bccNodes[di]);
                        bccGraph.AddEdge(bccEdge);
                        bccEdges[si][di] = bccEdge;
                    }
                    //spn = bccNodes[di].PortNodes[dEdge.mSrcNode.Index];
                    spn = portNodes[di][edge.mSrcNode.Index];
                    if (spn == null)
                    {
                        spn = new PortNode(edge.mSrcNode.mData, si);
                        bccNodes[di].Graph.AddNode(spn);
                        //bccNodes[di].PortNodes[dEdge.mSrcNode.Index] = spn;
                        portNodes[di][edge.mSrcNode.Index] = spn;
                        bccEdge.AddDstPort(spn);
                    }
                    bccNodes[di].Graph.AddEdge(new SubEdge(
                        spn, edge.mDstNode.mData, edge.mData));

                    //dpn = bccNodes[si].PortNodes[edge.mDstNode.Index];
                    dpn = portNodes[si][edge.mDstNode.Index];
                    if (dpn == null)
                    {
                        dpn = new PortNode(edge.mDstNode.mData, di);
                        bccNodes[si].Graph.AddNode(dpn);
                        //bccNodes[si].PortNodes[dEdge.mDstNode.Index] = dpn;
                        portNodes[si][edge.mDstNode.Index] = dpn;
                        bccEdge.AddSrcPort(dpn);
                    }
                    bccNodes[si].Graph.AddEdge(new SubEdge(
                        edge.mSrcNode.mData, dpn, edge.mData));
                }
            }
            return bccGraph;
        }
    }
}
