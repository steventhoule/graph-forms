using System;
using System.Collections.Generic;
using System.Text;

namespace GraphAlgorithmDemo
{
    public interface IGraphCreator
    {
        void CreateGraph(CircleNodeScene scene);
    }

    public class RandomGraphCreator : IGraphCreator
    {
        private void GenerateRandomGraph(CircleNodeScene scene, 
            int minNodes, int maxNodes, int minEdgesPerNode, 
            int maxEdgeDiff, bool preventSelfLoops)
        {
            Random rnd = new Random(DateTime.Now.Millisecond);
            int i, count = rnd.Next() % (maxNodes - minNodes) + minNodes;
            CircleNode[] nodes = new CircleNode[count];
            CircleNode node;
            for (i = 0; i < count; i++)
            {
                node = new CircleNode(scene,
                    (i + 1).ToString("n00"));
                nodes[i] = node;
            }
            int j, k, eCount, mod = count - maxEdgeDiff - minEdgesPerNode;
            ArrowEdge edge;
            for (i = 0; i < count; i++)
            {
                node = nodes[i];
                eCount = rnd.Next() % mod + minEdgesPerNode;
                for (j = 0; j < eCount; j++)
                {
                    k = rnd.Next() % count;
                    while ((preventSelfLoops && k == i) ||
                        scene.Graph.IndexOfEdge(node, nodes[k]) >= 0)
                    {
                        k = rnd.Next() % count;
                    }
                    edge = new ArrowEdge(node, nodes[k], scene);
                }
            }
        }

        public void CreateGraph(CircleNodeScene scene)
        {
            this.GenerateRandomGraph(scene, 5, 15, 2, 2, true);
            scene.Layout.ShuffleNodePositions();
        }

        public override string ToString()
        {
            return "Random Graph";
        }
    }


    // Copied from Wikipedia article image in order to test BCC algorithm
    // Article: http://en.wikipedia.org/wiki/Biconnected_component
    public class BCCTestGraphCreator : IGraphCreator
    {
        public void CreateGraph(CircleNodeScene scene)
        {
            CircleNode n01 = new CircleNode(scene, "n01");
            CircleNode n02 = new CircleNode(scene, "n02");
            CircleNode n03 = new CircleNode(scene, "n03");
            CircleNode n04 = new CircleNode(scene, "n04");

            ArrowEdge e01 = new ArrowEdge(n01, n02, scene);
            ArrowEdge e02 = new ArrowEdge(n01, n03, scene);
            ArrowEdge e03 = new ArrowEdge(n02, n04, scene);
            ArrowEdge e04 = new ArrowEdge(n03, n04, scene);

            CircleNode n05 = new CircleNode(scene, "n05");
            ArrowEdge e05 = new ArrowEdge(n04, n05, scene);

            CircleNode n06 = new CircleNode(scene, "n06");
            ArrowEdge e06 = new ArrowEdge(n05, n06, scene);

            CircleNode n07 = new CircleNode(scene, "n07");
            CircleNode n08 = new CircleNode(scene, "n08");
            CircleNode n09 = new CircleNode(scene, "n09");
            CircleNode n10 = new CircleNode(scene, "n10");
            CircleNode n11 = new CircleNode(scene, "n11");
            CircleNode n12 = new CircleNode(scene, "n12");

            ArrowEdge e07 = new ArrowEdge(n07, n08, scene);
            ArrowEdge e08 = new ArrowEdge(n07, n09, scene);
            ArrowEdge e09 = new ArrowEdge(n08, n09, scene);
            ArrowEdge e10 = new ArrowEdge(n08, n10, scene);
            ArrowEdge e11 = new ArrowEdge(n10, n11, scene);
            ArrowEdge e12 = new ArrowEdge(n09, n12, scene);
            ArrowEdge e13 = new ArrowEdge(n11, n12, scene);

            ArrowEdge e14 = new ArrowEdge(n06, n10, scene);

            CircleNode n13 = new CircleNode(scene, "n13");
            ArrowEdge e15 = new ArrowEdge(n10, n13, scene);

            CircleNode n14 = new CircleNode(scene, "n14");
            ArrowEdge e16 = new ArrowEdge(n11, n14, scene);

            /*ArrowEdge e17 = new ArrowEdge(n02, n01, scene);
            ArrowEdge e18 = new ArrowEdge(n03, n01, scene);
            ArrowEdge e19 = new ArrowEdge(n04, n02, scene);
            ArrowEdge e20 = new ArrowEdge(n04, n03, scene);

            ArrowEdge e21 = new ArrowEdge(n05, n04, scene);

            ArrowEdge e22 = new ArrowEdge(n06, n05, scene);

            ArrowEdge e23 = new ArrowEdge(n08, n07, scene);
            ArrowEdge e24 = new ArrowEdge(n09, n07, scene);
            ArrowEdge e25 = new ArrowEdge(n09, n08, scene);
            ArrowEdge e26 = new ArrowEdge(n10, n08, scene);
            ArrowEdge e27 = new ArrowEdge(n11, n10, scene);
            ArrowEdge e28 = new ArrowEdge(n12, n09, scene);
            ArrowEdge e29 = new ArrowEdge(n12, n11, scene);

            ArrowEdge e30 = new ArrowEdge(n10, n06, scene);

            ArrowEdge e31 = new ArrowEdge(n13, n10, scene);

            ArrowEdge e32 = new ArrowEdge(n14, n11, scene);/* */

            float unit = 40;

            n01.SetPosition(2 * unit, 2 * unit);
            n02.SetPosition(1 * unit, 3 * unit);
            n03.SetPosition(3 * unit, 3 * unit);
            n04.SetPosition(2 * unit, 4 * unit);
            n05.SetPosition(3 * unit, 5 * unit);
            n06.SetPosition(4 * unit, 4 * unit);
            n07.SetPosition(7 * unit, 2 * unit);
            n08.SetPosition(5 * unit, 3 * unit);
            n09.SetPosition(8 * unit, 3 * unit);
            n10.SetPosition(6 * unit, 4 * unit);
            n11.SetPosition(7 * unit, 5 * unit);
            n12.SetPosition(9 * unit, 5 * unit);
            n13.SetPosition(5 * unit, 5 * unit);
            n14.SetPosition(6 * unit, 6 * unit);

            scene.UpdateEdges();
        }

        public override string ToString()
        {
            return "BCC Test Graph";
        }
    }

    // Copied from Wikipedia article image in order to test SCC algorithm
    // Article: http://en.wikipedia.org/wiki/Tarjan's_strongly_connected_components_algorithm
    public class SCCTestGraphCreator : IGraphCreator
    {
        public void CreateGraph(CircleNodeScene scene)
        {
            CircleNode n01 = new CircleNode(scene, "n01");
            CircleNode n02 = new CircleNode(scene, "n02");
            CircleNode n03 = new CircleNode(scene, "n03");
            ArrowEdge e01 = new ArrowEdge(n01, n02, scene);
            ArrowEdge e02 = new ArrowEdge(n02, n03, scene);
            ArrowEdge e03 = new ArrowEdge(n03, n01, scene);

            CircleNode n04 = new CircleNode(scene, "n04");
            CircleNode n05 = new CircleNode(scene, "n05");
            ArrowEdge e04 = new ArrowEdge(n04, n02, scene);
            ArrowEdge e05 = new ArrowEdge(n04, n03, scene);
            ArrowEdge e06 = new ArrowEdge(n04, n05, scene);
            ArrowEdge e07 = new ArrowEdge(n05, n04, scene);

            CircleNode n06 = new CircleNode(scene, "n06");
            CircleNode n07 = new CircleNode(scene, "n07");
            ArrowEdge e08 = new ArrowEdge(n05, n06, scene);
            ArrowEdge e09 = new ArrowEdge(n06, n07, scene);
            ArrowEdge e10 = new ArrowEdge(n07, n06, scene);
            ArrowEdge e11 = new ArrowEdge(n06, n03, scene);

            CircleNode n08 = new CircleNode(scene, "n08");
            ArrowEdge e12 = new ArrowEdge(n08, n05, scene);
            ArrowEdge e13 = new ArrowEdge(n08, n07, scene);
            ArrowEdge e14 = new ArrowEdge(n08, n08, scene);

            float unit = 50;

            n01.SetPosition(1 * unit, 3 * unit);
            n02.SetPosition(1 * unit, 5 * unit);
            n03.SetPosition(3 * unit, 3 * unit);
            n04.SetPosition(3 * unit, 5 * unit);
            n05.SetPosition(5 * unit, 5 * unit);
            n06.SetPosition(5 * unit, 3 * unit);
            n07.SetPosition(7 * unit, 3 * unit);
            n08.SetPosition(7 * unit, 5 * unit);

            scene.UpdateEdges();
        }

        public override string ToString()
        {
            return "SCC Test Graph";
        }
    }

    // Copied from Wikipedia article image in order to test Boruvka algorithm
    // Article: http://en.wikipedia.org/wiki/Bor%C5%AFvka%27s_algorithm
    public class MinSpanTreeTestGraphCreator : IGraphCreator
    {
        public void CreateGraph(CircleNodeScene scene)
        {
            float rad = 20;
            CircleNode nA = new CircleNode(scene, rad, "nA");
            CircleNode nB = new CircleNode(scene, rad, "nB");
            CircleNode nC = new CircleNode(scene, rad, "nC");
            CircleNode nD = new CircleNode(scene, rad, "nD");
            CircleNode nE = new CircleNode(scene, rad, "nE");
            CircleNode nF = new CircleNode(scene, rad, "nF");
            CircleNode nG = new CircleNode(scene, rad, "nG");

            ArrowEdge eAB = new ArrowEdge(nA, nB, scene, 7, "eAB");
            ArrowEdge eAD = new ArrowEdge(nA, nD, scene, 4, "eAD");
            ArrowEdge eBC = new ArrowEdge(nB, nC, scene, 11, "eBC");
            ArrowEdge eBD = new ArrowEdge(nB, nD, scene, 9, "eBD");
            ArrowEdge eBE = new ArrowEdge(nB, nE, scene, 10, "eBE");
            ArrowEdge eCE = new ArrowEdge(nC, nE, scene, 5, "eCE");
            ArrowEdge eDE = new ArrowEdge(nD, nE, scene, 15, "eDE");
            ArrowEdge eDF = new ArrowEdge(nD, nF, scene, 6, "eDF");
            ArrowEdge eEF = new ArrowEdge(nE, nF, scene, 12, "eEF");
            ArrowEdge eEG = new ArrowEdge(nE, nG, scene, 8, "eEG");
            ArrowEdge eFG = new ArrowEdge(nF, nG, scene, 13, "eFG");

            //ArrowEdge eAD = new ArrowEdge(nD, nA, scene,  4, "eDA");
            //ArrowEdge eBD = new ArrowEdge(nD, nB, scene,  9, "eDB");

            float unit = 50;

            nA.SetPosition(2 * unit, 2 * unit);
            nB.SetPosition(4 * unit, 3 * unit);
            nC.SetPosition(6 * unit, 2 * unit);
            nD.SetPosition(3 * unit, 4 * unit);
            nE.SetPosition(5 * unit, 4 * unit);
            nF.SetPosition(4 * unit, 5 * unit);
            nG.SetPosition(6 * unit, 6 * unit);

            scene.UpdateEdges();
        }

        public override string ToString()
        {
            return "Min Spanning Tree Test Graph";
        }
    }

    // Copied from Wikipedia article image in order to test 
    // SPQR Tree (Triconnected Components) algorithm
    // Article: http://en.wikipedia.org/wiki/SPQR_tree
    public class SPQRTestGraphCreator : IGraphCreator
    {
        public void CreateGraph(CircleNodeScene scene)
        {
            float rad = 10;
            float ang = 10;

            CircleNode nA1 = new CircleNode(scene, rad, "A1");
            CircleNode nA2 = new CircleNode(scene, rad, "A2");
            CircleNode nA3 = new CircleNode(scene, rad, "A3");
            CircleNode nA4 = new CircleNode(scene, rad, "A4");
            CircleNode nA5 = new CircleNode(scene, rad, "A5");
            CircleNode nA6 = new CircleNode(scene, rad, "A6");
            CircleNode nA7 = new CircleNode(scene, rad, "A7");
            CircleNode nA8 = new CircleNode(scene, rad, "A8");

            ArrowEdge eA1A2 = new ArrowEdge(nA1, nA2, scene, 1, ang);
            ArrowEdge eA7A1 = new ArrowEdge(nA7, nA1, scene, 1, ang);
            ArrowEdge eA2A8 = new ArrowEdge(nA2, nA8, scene, 1, ang);
            ArrowEdge eA1A3 = new ArrowEdge(nA1, nA3, scene, 1, ang);
            ArrowEdge eA2A4 = new ArrowEdge(nA2, nA4, scene, 1, ang);
            ArrowEdge eA7A5 = new ArrowEdge(nA7, nA5, scene, 1, ang);
            ArrowEdge eA8A6 = new ArrowEdge(nA8, nA6, scene, 1, ang);
            ArrowEdge eA3A4 = new ArrowEdge(nA3, nA4, scene, 1, ang);
            ArrowEdge eA4A6 = new ArrowEdge(nA4, nA6, scene, 1, ang);
            ArrowEdge eA6A5 = new ArrowEdge(nA6, nA5, scene, 1, ang);
            ArrowEdge eA5A3 = new ArrowEdge(nA5, nA3, scene, 1, ang);

            CircleNode nB1 = new CircleNode(scene, rad, "B1");
            CircleNode nB2 = new CircleNode(scene, rad, "B2");
            CircleNode nB3 = new CircleNode(scene, rad, "B3");
            CircleNode nB4 = new CircleNode(scene, rad, "B4");
            CircleNode nB5 = new CircleNode(scene, rad, "B5");

            ArrowEdge eA8B1 = new ArrowEdge(nA8, nB1, scene, 1, ang);
            ArrowEdge eB1B5 = new ArrowEdge(nB1, nB5, scene, 1, ang);
            ArrowEdge eB5B4 = new ArrowEdge(nB5, nB4, scene, 1, ang);
            ArrowEdge eA8B2 = new ArrowEdge(nA8, nB2, scene, 1, ang);
            ArrowEdge eB1B2 = new ArrowEdge(nB1, nB2, scene, 1, ang);
            ArrowEdge eB5B3 = new ArrowEdge(nB5, nB3, scene, 1, ang);
            ArrowEdge eB4B3 = new ArrowEdge(nB4, nB3, scene, 1, ang);
            ArrowEdge eB2B3 = new ArrowEdge(nB2, nB3, scene, 1, ang);

            CircleNode nC1 = new CircleNode(scene, rad, "C1");
            CircleNode nC2 = new CircleNode(scene, rad, "C2");
            CircleNode nC3 = new CircleNode(scene, rad, "C3");

            ArrowEdge eB4C3 = new ArrowEdge(nB4, nC3, scene, 1, ang);
            ArrowEdge eC3C1 = new ArrowEdge(nC3, nC1, scene, 1, ang);
            ArrowEdge eC1C2 = new ArrowEdge(nC1, nC2, scene, 1, ang);
            ArrowEdge eB4C2 = new ArrowEdge(nB4, nC2, scene, 1, ang);
            ArrowEdge eC3C2 = new ArrowEdge(nC3, nC2, scene, 1, ang);

            ArrowEdge eB4C1 = new ArrowEdge(nB4, nC1, scene, 1, ang);

            ArrowEdge eC1A7 = new ArrowEdge(nC1, nA7, scene, 1, ang);

            float unit = 40;
            float left = 200 - 1.5f * unit;
            float top = 200 - 4.5f * unit;

            nA1.SetPosition(left + 0 * unit, top + 0 * unit);
            nA2.SetPosition(left + 3 * unit, top + 0 * unit);
            nA3.SetPosition(left + 1 * unit, top + 1 * unit);
            nA4.SetPosition(left + 2 * unit, top + 1 * unit);
            nA5.SetPosition(left + 1 * unit, top + 2 * unit);
            nA6.SetPosition(left + 2 * unit, top + 2 * unit);
            nA7.SetPosition(left + 0 * unit, top + 3 * unit);
            nA8.SetPosition(left + 3 * unit, top + 3 * unit);

            nB1.SetPosition(left + 5 * unit, top + 3 * unit);
            nB2.SetPosition(left + 4 * unit, top + 4 * unit);
            nB3.SetPosition(left + 4 * unit, top + 5 * unit);
            nB4.SetPosition(left + 3 * unit, top + 6 * unit);
            nB5.SetPosition(left + 5 * unit, top + 6 * unit);

            double sqrt3 = Math.Sqrt(3);

            nC1.SetPosition(left, top + 6 * unit);
            nC2.SetPosition(200f, (float)(top + (6 + 0.75 * sqrt3) * unit));
            nC3.SetPosition(200f, (float)(top + (6 + 1.50 * sqrt3) * unit));

            scene.UpdateEdges();
        }

        public override string ToString()
        {
            return "SPQR Test Graph";
        }
    }

    public class WagonWheelGraphCreator : IGraphCreator
    {
        private void GenerateWagonWheelGraph(CircleNodeScene scene, 
            int spokeCount)
        {
            ArrowEdge edge;
            CircleNode center = new CircleNode(scene, "n00");
            CircleNode first = new CircleNode(scene, "n01");
            edge = new ArrowEdge(first, center, scene);
            //edge = new ArrowEdge(center, first, scene);
            CircleNode curr, prev = first;
            for (int i = 1; i < spokeCount; i++)
            {
                curr = new CircleNode(scene, (i + 1).ToString("n00"));
                edge = new ArrowEdge(prev, curr, scene);
                //edge = new ArrowEdge(curr, prev, scene);
                edge = new ArrowEdge(curr, center, scene);
                //edge = new ArrowEdge(center, curr, scene);
                prev = curr;
            }
            edge = new ArrowEdge(prev, first, scene);
            //edge = new ArrowEdge(first, prev, scene);
        }

        public void CreateGraph(CircleNodeScene scene)
        {
            this.GenerateWagonWheelGraph(scene, 15);
            scene.Layout.ShuffleNodePositions();
        }

        public override string ToString()
        {
            return "Wagon Wheel Graph";
        }
    }
}
