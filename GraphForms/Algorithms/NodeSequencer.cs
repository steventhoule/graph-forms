using System;
using GraphForms.Algorithms.Path;

namespace GraphForms.Algorithms
{
    public class NodeSequencer<Node, Edge>
        where Edge : IGraphEdge<Node>
    {
        protected static int ArrangeByLongestPath(
            Digraph<Node, Edge> graph, Digraph<Node, Edge>.GNode[] nodes)
        {
            if (graph == null)
                throw new ArgumentNullException("graph");
            if (nodes == null)
                throw new ArgumentNullException("vNodes");
            // Take the easy way out if possible
            if (graph.NodeCount == 0 || graph.EdgeCount == 0)
            {
                return 0;
            }
            int j, k, count, vCount = 0;
            int nCount = graph.NodeCount;
            int eCount = graph.EdgeCount;
            Digraph<Node, Edge>.GNode gNode;
            Digraph<Node, Edge>.GEdge gEdge;
            // Check the capacity of vNodes
            for (j = 0; j < nCount; j++)
            {
                if (!graph.InternalNodeAt(j).Hidden)
                    vCount++;
            }
            if (vCount > nodes.Length)
            {
                throw new ArgumentException(
                    "Array too small to hold all visible nodes", "vNodes");
            }
            // Compute the longest path in the graph
            DFLongestPath<Node, Edge> alg
                = new DFLongestPath<Node, Edge>(graph, false, false);
            alg.Compute();
            int[] nis = alg.PathNodeIndexes;
            // Initially color all nodes White.
            for (j = 0; j < nCount; j++)
            {
                graph.InternalNodeAt(j).Color = GraphColor.White;
            }
            // Add all the nodes in the longest path to the embedding circle
            // and color them Black.
            vCount = nis.Length;
            for (j = 0; j < vCount; j++)
            {
                gNode = graph.InternalNodeAt(nis[j]);
                gNode.Color = GraphColor.Black;
                nodes[j] = gNode;
            }
            bool[] flags = new bool[nCount];
            // Here Gray is used to determine which nodes are neighbors 
            // of the current node to be added to the embedding circle.
            for (k = 0; k < nCount; k++)
            {
                gNode = graph.InternalNodeAt(k);
                if (gNode.Color != GraphColor.Black && !gNode.Hidden)
                {
                    // Clear neighbor flags from previous iteration,
                    // so that all nodes are now either Black or White.
                    for (j = 0; j < nCount; j++)
                    {
                        flags[j] = false;
                    }
                    // Set which nodes are neighbors to the current node.
                    count = 0;
                    for (j = 0; j < eCount; j++)
                    {
                        gEdge = graph.InternalEdgeAt(j);
                        if (gEdge.Hidden)
                            continue;
                        if (gEdge.SrcNode.Index == k &&
                            gEdge.DstNode.Color == GraphColor.Black)
                        {
                            flags[gEdge.DstNode.Index] = true;
                            count++;
                        }
                        else if (gEdge.DstNode.Index == k &&
                            gEdge.SrcNode.Color == GraphColor.Black)
                        {
                            flags[gEdge.SrcNode.Index] = true;
                            count++;
                        }
                    }
                    // Look for two consecutive neighbors in the list
                    // and insert the current node between them.
                    if (count >= 2)
                    {
                        for (j = 0; j < vCount; j++)
                        {
                            if (flags[nodes[j].Index] &&
                                flags[nodes[(j + 1) % vCount].Index])
                            {
                                j++;
                                Array.Copy(nodes, j,
                                    nodes, j + 1, vCount - j);
                                nodes[j] = gNode;
                                vCount++;
                                gNode.Color = GraphColor.Black;
                                break;
                            }
                        }
                    }
                    // Look for any neighbor in the list
                    // and insert the current node after it.
                    if (gNode.Color != GraphColor.Black && count > 0)
                    {
                        for (j = 0; j < vCount; j++)
                        {
                            if (flags[nodes[j].Index])
                            {
                                j++;
                                Array.Copy(nodes, j,
                                    nodes, j + 1, vCount - j);
                                nodes[j] = gNode;
                                vCount++;
                                gNode.Color = GraphColor.Black;
                                break;
                            }
                        }
                    }
                    // Place all orphaned nodes at the end of the list
                    if (gNode.Color != GraphColor.Black)
                    {
                        nodes[vCount++] = gNode;
                        gNode.Color = GraphColor.Black;
                    }
                }
            }
            for (k = vCount; k < nodes.Length; k++)
            {
                nodes[k] = null;
            }
            return vCount;
        }

        protected static void ReduceEdgeCrossings(
            Digraph<Node, Edge> graph, Digraph<Node, Edge>.GNode[] nodes, 
            int start, int length, uint maxIterations)
        {
            if (graph == null)
                throw new ArgumentNullException("graph");
            if (nodes == null)
                throw new ArgumentNullException("vNodes");
            if (start < 0 || start >= nodes.Length)
                throw new ArgumentOutOfRangeException("start");
            if (length < 0 || length + start > nodes.Length)
                throw new ArgumentOutOfRangeException("length");
            // Check if there is even enough nodes to create edge crossings
            if (graph.NodeCount < 4 || graph.EdgeCount < 4 || length < 4)
            {
                return;
            }
            int eCount = graph.EdgeCount;
            int i, j, k, m, posV, posX, posY, offset, improvedCrossings;
            Digraph<Node, Edge>.GNode u, v, x, y;
            Digraph<Node, Edge>.GEdge edge;

            int n = graph.NodeCount;
            int[] pos = new int[n];
            for (i = 0; i < n; i++)
            {
                pos[i] = -1;
            }
            n = start + length - 1;
            // Find the first non-null and visible node from the end
            while (n >= start && (nodes[n] == null || nodes[n].Hidden))
            {
                n--;
            }
            // Sequester null and hidden nodes by moving them to the end
            for (i = n - 1; i >= start; i--)
            {
                u = nodes[i];
                if (u == null || u.Hidden)
                {
                    Array.Copy(nodes, i + 1, nodes, i, n - i);
                    nodes[n] = u;
                    n--;
                }
            }
            // Check if there is still enough nodes to create edge crossings
            if (n - start < 3)
            {
                return;
            }
            n++;
            for (i = start; i < n; i++)
            {
                pos[nodes[i].Index] = i;
            }

            bool improved = true;
            for (uint iter = 0; iter < maxIterations && improved; iter++)
            {
                improved = false;
                for (i = start; i < n; i++)
                {
                    u = nodes[i];
                    // we fake a numbering around the circle
                    // starting with u at position 0
                    // using the formula: (pos[t] - offset) % n
                    // and: pos[u] + offset = n
                    offset = n - pos[u.Index];
                    for (j = 0; j < eCount; j++)
                    {
                        // we try swapping u with a node that comes
                        // right before one of its neighbors
                        edge = graph.InternalEdgeAt(j);
                        if (edge.Hidden)
                            continue;
                        if (edge.SrcNode.Index == u.Index)
                            x = edge.DstNode;
                        else if (edge.DstNode.Index == u.Index)
                            x = edge.SrcNode;
                        else
                            continue;
                        if (pos[x.Index] == -1)
                            continue;
                        k = (pos[x.Index] + n - 1) % n;
                        if (k == i)
                            continue;
                        v = nodes[k];
                        posV = (k + offset) % n;
                        // we count how many crossings we save 
                        // when swapping u and v
                        improvedCrossings = 0;
                        for (k = 0; k < eCount; k++)
                        {
                            edge = graph.InternalEdgeAt(k);
                            if (edge.Hidden)
                                continue;
                            if (edge.SrcNode.Index == u.Index)
                                x = edge.DstNode;
                            else if (edge.DstNode.Index == u.Index)
                                x = edge.SrcNode;
                            else
                                continue;
                            if (x.Index == v.Index ||
                                pos[x.Index] == -1)
                                continue;
                            // posX = (pos[x] - pos[u]) mod n
                            posX = (pos[x.Index] + offset) % n;
                            for (m = 0; m < eCount; m++)
                            {
                                edge = graph.InternalEdgeAt(m);
                                if (edge.Hidden)
                                    continue;
                                if (edge.SrcNode.Index == v.Index)
                                    y = edge.DstNode;
                                else if (edge.DstNode.Index == v.Index)
                                    y = edge.SrcNode;
                                else
                                    continue;
                                if (y.Index == u.Index ||
                                    y.Index == x.Index ||
                                    pos[y.Index] == -1)
                                    continue;
                                // posY = (pos[y] - pos[u]) mod n
                                posY = (pos[y.Index] + offset) % n;
                                // All possible permutations:
                                // ++: u v x y, u y x v
                                // --: u v y x, u x y v
                                // 00: u x v y, u y v x
                                if (posX > posV && posY > posV)
                                {
                                    if (posX > posY)
                                    {
                                        //   /-------------\
                                        //  /     /---\     \
                                        // u     v     y     x
                                        improvedCrossings--;
                                    }
                                    else
                                    {
                                        //   /-------\
                                        //  /     /---\-----\
                                        // u     v     x     y
                                        improvedCrossings++;
                                    }
                                }
                                else if (posX < posV && posY < posV)
                                {
                                    if (posX > posY)
                                    {
                                        //   /-------\
                                        //  /     /---\-----\
                                        // u     y     x     v
                                        improvedCrossings++;
                                    }
                                    else
                                    {
                                        //  /---\       /---\
                                        // u     x     y     v
                                        improvedCrossings--;
                                    }
                                }
                            }
                        }
                        if (improvedCrossings > 0)
                        {
                            improved = true;
                            // swap the nodes in the list
                            nodes[i] = v;
                            edge = graph.InternalEdgeAt(j);
                            if (edge.SrcNode.Index == u.Index)
                                x = edge.DstNode;
                            else if (edge.DstNode.Index == u.Index)
                                x = edge.SrcNode;
                            nodes[(pos[x.Index] + n - 1) % n] = u;
                            // swap tracked positions
                            offset = pos[u.Index];
                            pos[u.Index] = pos[v.Index];
                            pos[v.Index] = offset;
                            break;
                        }
                    }
                }
            }
        }

        protected static void SeparateWhiteAndDarkNodes(
            Digraph<Node, Edge> graph, Digraph<Node, Edge>.GNode[] nodes, 
            uint maxIterations)
        {
            if (graph == null)
                throw new ArgumentNullException("graph");
            if (nodes == null)
                throw new ArgumentNullException("vNodes");
            // Take the easy way out if possible
            if (graph.NodeCount < 4 || graph.EdgeCount < 4 || 
                nodes.Length < 4)
            {
                return;
            }
            int ii, last = nodes.Length - 1;
            Digraph<Node, Edge>.GNode node;
            // Find the first non-null and visible node from the end
            while (last >= 0 && (nodes[last] == null || nodes[last].Hidden))
            {
                last--;
            }
            // Sequester null and hidden nodes by moving them to the end
            for (ii = last - 1; ii >= 0; ii--)
            {
                node = nodes[ii];
                if (node == null || node.Hidden)
                {
                    Array.Copy(nodes, ii + 1, nodes, ii, last - ii);
                    nodes[last] = node;
                    last--;
                }
            }
            // Find the first dark node from the end
            while (last >= 0 && nodes[last].Color == GraphColor.White)
            {
                last--;
            }
            if (last >= 0)
            {
                // Find the first dark node from the beginning
                ii = 0;
                while (ii < last && nodes[ii].Color == GraphColor.White)
                {
                    ii++;
                }
                // Isolate the dark nodes and reduce their edge crossings
                for (int j = ii; j < last; j++)
                {
                    node = nodes[j];
                    if (node.Color == GraphColor.White)
                    {
                        // Remove the white node from the dark section
                        // Reinsert it after the end of the dark section
                        // Reduce the edge crossings created by this move
                        Array.Copy(nodes, j + 1, nodes, j, last - j);
                        nodes[last] = node;
                        ReduceEdgeCrossings(graph, nodes, ii,
                            last + 1 - ii, maxIterations);
                        last--;
                    }
                }
            }
        }

        public virtual void ArrangeNodes(Digraph<Node, Edge> graph, 
            Digraph<Node, Edge>.GNode[] nodes)
        {
            int count = ArrangeByLongestPath(graph, nodes);
            ReduceEdgeCrossings(graph, nodes, 0, count, 50);
        }
    }
}
