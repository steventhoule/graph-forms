using System;
using System.Collections.Generic;
using System.Text;
using GraphForms.Algorithms.ConnectedComponents;

namespace GraphForms.Algorithms.Layout.Circular
{
    public class CircularLayoutAlgorithm<Node, Edge>
        : LayoutAlgorithm<Node, Edge>
        where Node : class, ILayoutNode
        where Edge : IGraphEdge<Node>, IUpdateable
    {
        private CircularLayoutAlgorithm(Digraph<Node, Edge> graph,
            IClusterNode clusterNode)
            : base(graph, clusterNode)
        {
        }

        private CircularLayoutAlgorithm(Digraph<Node, Edge> graph,
            Box2F boundingBox)
            : base(graph, boundingBox)
        {
        }

        protected override void PerformIteration(uint iteration)
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

            public Box2F LayoutBBox
            {
                get { return this.mNode.LayoutBBox; }
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

        private class BCCNode : IClusterNode
        {
            /// <summary>
            /// The index of this cluster node in graph superstructure
            /// </summary>
            public readonly int Index;
            /// <summary>
            /// The sub-graph enclosed by this cluster node.
            /// </summary>
            public readonly Digraph<ILayoutNode, SubEdge> Subgraph;
            /// <summary>
            /// Each sub-graph could potentially contain a port node that
            /// represents any node not in the sub-graph.
            /// </summary>
            public readonly IPortNode[] PortNodes;

            private double mRadius;

            public BCCNode(int index, int nodeCapacity, int portCount)
            {
                this.Index = index;
                this.Subgraph = new Digraph<ILayoutNode, SubEdge>(
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

            #region IClusterNode Members

            public Vec2F GetPortNodePos(double angle)
            {
                return new Vec2F(
                    (float)(this.mRadius * Math.Cos(angle)),
                    (float)(this.mRadius * Math.Sin(angle)));
            }

            public void LearnNodePos(float x, float y, Box2F boundingBox)
            {
                double rad = Math.Sqrt(x * x + y * y);
                if (rad > this.mRadius)
                {
                    this.mRadius = rad;
                }
            }

            public Vec2F AugmentNodePos(float x, float y)
            {
                return new Vec2F(x, y);
            }

            #endregion

            #region ILayoutNode Members

            private static readonly double sqrt2 = Math.Sqrt(2.0);

            public Box2F LayoutBBox
            {
                get
                {
                    float r = (float)(sqrt2 * this.mRadius);
                    return new Box2F(r / -2, r / -2, r, r);
                }
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
                get { return false; }
            }

            /*private float mNewX;
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

                ILayoutNode node;
                Digraph<ILayoutNode, SubEdge>.GNode[] nodes
                    = this.Subgraph.InternalNodes;
                for (int i = 0; i < nodes.Length; i++)
                {
                    node = nodes[i].Data;
                    node.SetNewPosition(node.NewX + newX, node.NewY + newY);
                }
            }/* */

            #endregion
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
                = new BCCAlgorithm<Node, Edge>(graph, false);
            bccAlg.Compute();
            bccAlg.ArticulateToLargerCompactGroups();
            Digraph<Node, Edge>.GNode[][] bccGroups = bccAlg.CompactGroups;
            int bccGroupCount = bccAlg.CompactGroupCount;
            int[] bccGroupIds = bccAlg.CompactGroupIds;
            Digraph<BCCNode, BCCEdge> bccGraph 
                = new Digraph<BCCNode, BCCEdge>(
                    bccGroupCount, bccGroupCount / 2);

            PortNode[][] portNodes = new PortNode[bccGroupCount][];
            BCCEdge[][] bccEdges = new BCCEdge[bccGroupCount][];
            BCCNode[] bccNodes = new BCCNode[bccGroupCount];
            BCCNode bccNode;
            Digraph<Node, Edge>.GNode[] bccGroup;
            for (i = 0; i < bccGroupCount; i++)
            {
                bccGroup = bccGroups[i];
                bccNode = new BCCNode(i, bccGroup.Length, nodes.Length);
                for (j = 0; j < bccGroup.Length; j++)
                {
                    bccNode.Subgraph.AddNode(bccGroup[j].Data);
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
                si = bccGroupIds[edge.SrcNode.Index];
                di = bccGroupIds[edge.DstNode.Index];
                if (si == di)
                {
                    bccNodes[si].Subgraph.AddEdge(new SubEdge(
                        edge.SrcNode.Data, edge.DstNode.Data, 
                        edge.Data));
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
                    spn = portNodes[di][edge.SrcNode.Index];
                    if (spn == null)
                    {
                        spn = new PortNode(edge.SrcNode.Data, si);
                        bccNodes[di].Subgraph.AddNode(spn);
                        //bccNodes[di].PortNodes[dEdge.mSrcNode.Index] = spn;
                        portNodes[di][edge.SrcNode.Index] = spn;
                        bccEdge.AddDstPort(spn);
                    }
                    bccNodes[di].Subgraph.AddEdge(new SubEdge(
                        spn, edge.DstNode.Data, edge.Data));

                    //dpn = bccNodes[si].PortNodes[edge.mDstNode.Index];
                    dpn = portNodes[si][edge.DstNode.Index];
                    if (dpn == null)
                    {
                        dpn = new PortNode(edge.DstNode.Data, di);
                        bccNodes[si].Subgraph.AddNode(dpn);
                        //bccNodes[si].PortNodes[dEdge.mDstNode.Index] = dpn;
                        portNodes[si][edge.DstNode.Index] = dpn;
                        bccEdge.AddSrcPort(dpn);
                    }
                    bccNodes[si].Subgraph.AddEdge(new SubEdge(
                        edge.SrcNode.Data, dpn, edge.Data));
                }
            }
            return bccGraph;
        }
    }
}
