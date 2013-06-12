using System;
using System.Collections.Generic;
using System.Text;
using GraphForms.Algorithms.ConnectedComponents;

namespace GraphForms.Algorithms.Layout.Circular
{
    public class CircularLayoutAlgorithm<Node, Edge>
        : LayoutAlgorithm<Node, Edge, CircularLayoutParameters>
        where Node : GraphElement, ILayoutNode
        where Edge : class, IGraphEdge<Node>, IUpdateable
    {
        public CircularLayoutAlgorithm(Digraph<Node, Edge> graph)
            : base(graph, null)
        {
        }

        public CircularLayoutAlgorithm(Digraph<Node, Edge> graph,
            CircularLayoutParameters oldParameters)
            : base(graph, oldParameters)
        {
        }

        protected override void InternalCompute()
        {
            System.Drawing.RectangleF bbox;
            Node[] nodes = this.mGraph.Nodes;
            int i;
            double perimeter, radius, angle, a;
            double[] halfSize = new double[nodes.Length];

            // calculate the size of the circle
            perimeter = 0;
            for (i = 0; i < nodes.Length; i++)
            {
                bbox = nodes[i].BoundingBox;
                halfSize[i] = Math.Sqrt(bbox.Width * bbox.Width + bbox.Height * bbox.Height) * 0.5;
                perimeter += halfSize[i] * 2;
            }

            radius = perimeter / (2 * Math.PI);

            float[] newXs = this.NewXPositions;
            float[] newYs = this.NewYPositions;

            // precalculation
            angle = 0;
            for (i = 0; i < nodes.Length; i++)
            {
                a = Math.Sin(halfSize[i] * 0.5 / radius) * 2;
                angle += a;

                //nodes[i].NewX = (float)(Math.Cos(angle) * radius + radius);
                //nodes[i].NewY = (float)(Math.Sin(angle) * radius + radius);
                newXs[i] = (float)(Math.Cos(angle) * radius + radius);
                newYs[i] = (float)(Math.Sin(angle) * radius + radius);

                angle += a;
            }

            base.EndIteration(0, 0.5, "Precalculation done.");

            // recalculate radius
            radius = angle / (2 * Math.PI) * radius;

            // calculation
            angle = 0;
            for (i = 0; i < nodes.Length; i++)
            {
                a = Math.Sin(halfSize[i] * 0.5 / radius) * 2;
                angle += a;

                //nodes[i].NewX = (float)(Math.Cos(angle) * radius + radius);
                //nodes[i].NewY = (float)(Math.Sin(angle) * radius + radius);
                newXs[i] = (float)(Math.Cos(angle) * radius + radius);
                newYs[i] = (float)(Math.Sin(angle) * radius + radius);

                angle += a;
            }

            base.EndIteration(1, 1, "Calculation done.");
        }

        public class BCCNode : GraphElement
        {
            private Digraph<Node, Edge> mGraph;

            public BCCNode(int nodeCapacity)
            {
                this.mGraph = new Digraph<Node, Edge>(
                    nodeCapacity, nodeCapacity / 2);
            }

            public Digraph<Node, Edge> Graph
            {
                get { return this.mGraph; }
            }

            protected override void OnDrawBackground(
                System.Windows.Forms.PaintEventArgs e)
            {
                throw new NotImplementedException();
            }
        }

        public class BCCEdge
            : IGraphEdge<BCCNode>, IUpdateable
        {
            private BCCNode mSrcNode;
            private BCCNode mDstNode;
            private List<Digraph<Node, Edge>.GEdge> mEdges;

            public BCCEdge(BCCNode srcNode, BCCNode dstNode)
            {
                this.mSrcNode = srcNode;
                this.mDstNode = dstNode;
                this.mEdges = new List<Digraph<Node, Edge>.GEdge>();
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

            public void AddEdge(Edge e, Node srcNode, Node dstNode)
            {
                Digraph<Node, Edge>.GNode src 
                    = this.mSrcNode.Graph.InternalNodeFor(srcNode);
                Digraph<Node, Edge>.GNode dst
                    = this.mDstNode.Graph.InternalNodeFor(dstNode);
                this.mEdges.Add(new Digraph<Node, Edge>.GEdge(src, dst, e));
            }

            public Edge Copy<Edge>(BCCNode srcNode, BCCNode dstNode) 
                where Edge : class, IGraphEdge<BCCNode>
            {
                if (typeof(BCCEdge).Equals(typeof(Edge)))
                {
                    BCCEdge edge = new BCCEdge(srcNode, dstNode);
                    return edge as Edge;
                }
                return null;
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
        public static Digraph<BCCNode, BCCEdge> BCCCompactGraph(
            Digraph<Node, Edge> graph)
        {
            BCCAlgorithm<Node, Edge> bccAlg 
                = new BCCAlgorithm<Node, Edge>(graph);
            bccAlg.Compute();
            bccAlg.ArticulateToLargerCompactGroups();
            Node[][] bccGroups = bccAlg.CompactGroups;
            int[] bccGroupIds = bccAlg.CompactGroupIds;
            Digraph<BCCNode, BCCEdge> bccGraph 
                = new Digraph<BCCNode, BCCEdge>(
                    bccGroups.Length, bccGroups.Length / 2);

            int i, j;
            BCCEdge[][] bccEdges = new BCCEdge[bccGroups.Length][];
            BCCNode[] bccNodes = new BCCNode[bccGroups.Length];
            BCCNode bccNode;
            Node[] bccGroup;
            for (i = 0; i < bccGroups.Length; i++)
            {
                bccGroup = bccGroups[i];
                bccNode = new BCCNode(bccGroup.Length);
                for (j = 0; j < bccGroup.Length; j++)
                {
                    bccNode.Graph.AddNode(bccGroup[j]);
                }
                bccGraph.AddNode(bccNode);
                bccEdges[i] = new BCCEdge[bccGroups.Length];
                bccNodes[i] = bccNode;
            }
            
            int si, di;
            Digraph<Node, Edge>.GEdge[] edges
                = graph.InternalEdges;
            Digraph<Node, Edge>.GEdge edge;
            for (i = 0; i < edges.Length; i++)
            {
                edge = edges[i];
                si = bccGroupIds[edge.mSrcNode.Index];
                di = bccGroupIds[edge.mDstNode.Index];
                if (si == di)
                {
                    bccNodes[si].Graph.AddEdge(edge.mData);
                }
                else
                {
                    if (bccEdges[si][di] == null)
                    {
                        bccEdges[si][di] 
                            = new BCCEdge(bccNodes[si], bccNodes[di]);
                    }
                    bccEdges[si][di].AddEdge(edge.mData, 
                        edge.mSrcNode.mData, edge.mDstNode.mData);
                }
            }

            for (i = 0; i < bccGroups.Length; i++)
            {
                for (j = 0; j < bccGroups.Length; j++)
                {
                    if (bccEdges[i][j] != null)
                        bccGraph.AddEdge(bccEdges[i][j]);
                }
            }
            return bccGraph;
        }
    }
}
