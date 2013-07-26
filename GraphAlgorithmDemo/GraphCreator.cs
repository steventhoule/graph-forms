using System;
using System.Collections.Generic;
using System.Text;

namespace GraphAlgorithmDemo
{
    public interface IGraphCreator
    {
        void CreateGraph(CircleNodeScene scene, float rad, float ang);

        float DefaultNodeRad { get; }

        float DefaultEdgeAng { get; }

        float CenterX { get; set; }

        float CenterY { get; set; }
    }

    public abstract class AGraphCreator : IGraphCreator
    {
        public virtual float DefaultNodeRad
        {
            get { return 15; }
        }

        public virtual float DefaultEdgeAng
        {
            get { return 30; }//degrees
        }

        protected float mCenterX;
        protected float mCenterY;

        public AGraphCreator()
        {
            this.mCenterX = 200;
            this.mCenterY = 200;
        }

        public AGraphCreator(float cx, float cy)
        {
            this.mCenterX = cx;
            this.mCenterY = cy;
        }

        public float CenterX
        {
            get { return this.mCenterX; }
            set { this.mCenterX = value; }
        }

        public float CenterY
        {
            get { return this.mCenterY; }
            set { this.mCenterY = value; }
        }

        public abstract void CreateGraph(CircleNodeScene scene, 
            float rad, float ang);
    }

    public class RandomGraphCreator : AGraphCreator
    {
        public RandomGraphCreator() { }

        public RandomGraphCreator(float cx, float cy) : base(cx, cy) { }

        private void GenerateRandomGraph(CircleNodeScene scene, 
            float rad, float ang, int minNodes, int maxNodes, 
            int minEdgesPerNode, int maxEdgeDiff, bool preventSelfLoops)
        {
            Random rnd = new Random(DateTime.Now.Millisecond);
            int i, count = rnd.Next() % (maxNodes - minNodes) + minNodes;
            CircleNode[] nodes = new CircleNode[count];
            CircleNode node;
            for (i = 0; i < count; i++)
            {
                node = new CircleNode(scene, rad, (i + 1).ToString("n00"));
                nodes[i] = node;
            }
            int j, k, eCount, mod = count - maxEdgeDiff - minEdgesPerNode;
            float w = 1;
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
                    edge = new ArrowEdge(node, nodes[k], scene, w, ang);
                }
            }

            GraphForms.Box2F bbox = scene.LayoutBBox;
            int cols = (int)Math.Ceiling(Math.Sqrt(nodes.Length * bbox.W / bbox.H));
            int rows = (int)Math.Ceiling(Math.Sqrt(nodes.Length * bbox.H / bbox.W));

            float unit = 50; 
            float x, y = this.mCenterY - (unit * (cols - 1) / 2f);
            k = 0;
            for (i = 0; i < rows && k < nodes.Length; i++)
            {
                x = this.mCenterX - (unit * (rows - 1) / 2f);
                for (j = 0; j < cols && k < nodes.Length; j++)
                {
                    nodes[k].SetPosition(x, y);
                    k++;
                    x += unit;
                }
                y += unit;
            }
            scene.UpdateEdges();
        }

        public override void CreateGraph(CircleNodeScene scene, 
            float rad, float ang)
        {
            this.GenerateRandomGraph(scene, rad, ang, 5, 15, 2, 2, true);
        }

        public override string ToString()
        {
            return "Random Graph";
        }
    }


    // Copied from Wikipedia article image in order to test BCC algorithm
    // Article: http://en.wikipedia.org/wiki/Biconnected_component
    public class BCCTestGraphCreator : AGraphCreator
    {
        public BCCTestGraphCreator() { }

        public BCCTestGraphCreator(float cx, float cy) : base(cx, cy) { }

        private void CreateGraph(CircleNodeScene scene, float rad, float ang,
            bool undirected, bool reversed)
        {
            CircleNode n01 = new CircleNode(scene, rad, "n01");
            CircleNode n02 = new CircleNode(scene, rad, "n02");
            CircleNode n03 = new CircleNode(scene, rad, "n03");
            CircleNode n04 = new CircleNode(scene, rad, "n04");

            CircleNode n05 = new CircleNode(scene, rad, "n05");

            CircleNode n06 = new CircleNode(scene, rad, "n06");
            
            CircleNode n07 = new CircleNode(scene, rad, "n07");
            CircleNode n08 = new CircleNode(scene, rad, "n08");
            CircleNode n09 = new CircleNode(scene, rad, "n09");
            CircleNode n10 = new CircleNode(scene, rad, "n10");
            CircleNode n11 = new CircleNode(scene, rad, "n11");
            CircleNode n12 = new CircleNode(scene, rad, "n12");

            CircleNode n13 = new CircleNode(scene, rad, "n13");

            CircleNode n14 = new CircleNode(scene, rad, "n14");

            if (!reversed || undirected)
            {
                ArrowEdge e01 = new ArrowEdge(n01, n02, scene, 1, ang);
                ArrowEdge e02 = new ArrowEdge(n03, n01, scene, 1, ang);
                ArrowEdge e03 = new ArrowEdge(n02, n04, scene, 1, ang);
                ArrowEdge e04 = new ArrowEdge(n04, n03, scene, 1, ang);

                ArrowEdge e05 = new ArrowEdge(n04, n05, scene, 1, ang);

                ArrowEdge e06 = new ArrowEdge(n05, n06, scene, 1, ang);

                ArrowEdge e07 = new ArrowEdge(n07, n08, scene, 1, ang);
                ArrowEdge e08 = new ArrowEdge(n09, n07, scene, 1, ang);
                ArrowEdge e09 = new ArrowEdge(n08, n09, scene, 1, ang);
                ArrowEdge e10 = new ArrowEdge(n08, n10, scene, 1, ang);
                ArrowEdge e11 = new ArrowEdge(n10, n11, scene, 1, ang);
                ArrowEdge e12 = new ArrowEdge(n09, n12, scene, 1, ang);
                ArrowEdge e13 = new ArrowEdge(n11, n12, scene, 1, ang);

                ArrowEdge e14 = new ArrowEdge(n06, n10, scene, 1, ang);

                ArrowEdge e15 = new ArrowEdge(n10, n13, scene, 1, ang);

                ArrowEdge e16 = new ArrowEdge(n11, n14, scene, 1, ang);
            }
            if (reversed || undirected)
            {
                ArrowEdge e17 = new ArrowEdge(n02, n01, scene, 1, ang);
                ArrowEdge e18 = new ArrowEdge(n01, n03, scene, 1, ang);
                ArrowEdge e19 = new ArrowEdge(n04, n02, scene, 1, ang);
                ArrowEdge e20 = new ArrowEdge(n03, n04, scene, 1, ang);

                ArrowEdge e21 = new ArrowEdge(n05, n04, scene, 1, ang);

                ArrowEdge e22 = new ArrowEdge(n06, n05, scene, 1, ang);

                ArrowEdge e23 = new ArrowEdge(n08, n07, scene, 1, ang);
                ArrowEdge e24 = new ArrowEdge(n07, n09, scene, 1, ang);
                ArrowEdge e25 = new ArrowEdge(n09, n08, scene, 1, ang);
                ArrowEdge e26 = new ArrowEdge(n10, n08, scene, 1, ang);
                ArrowEdge e27 = new ArrowEdge(n11, n10, scene, 1, ang);
                ArrowEdge e28 = new ArrowEdge(n12, n09, scene, 1, ang);
                ArrowEdge e29 = new ArrowEdge(n12, n11, scene, 1, ang);

                ArrowEdge e30 = new ArrowEdge(n10, n06, scene, 1, ang);

                ArrowEdge e31 = new ArrowEdge(n13, n10, scene, 1, ang);

                ArrowEdge e32 = new ArrowEdge(n14, n11, scene, 1, ang);
            }

            float unit = 40;
            float left = this.mCenterX - 4 * unit;
            float top  = this.mCenterY - 2 * unit;

            n01.SetPosition(left + 1 * unit, top + 1 * unit);
            n02.SetPosition(left + 0 * unit, top + 2 * unit);
            n03.SetPosition(left + 2 * unit, top + 2 * unit);
            n04.SetPosition(left + 1 * unit, top + 3 * unit);
            n05.SetPosition(left + 2 * unit, top + 4 * unit);
            n06.SetPosition(left + 3 * unit, top + 3 * unit);
            n07.SetPosition(left + 6 * unit, top + 1 * unit);
            n08.SetPosition(left + 4 * unit, top + 2 * unit);
            n09.SetPosition(left + 7 * unit, top + 2 * unit);
            n10.SetPosition(left + 5 * unit, top + 3 * unit);
            n11.SetPosition(left + 6 * unit, top + 4 * unit);
            n12.SetPosition(left + 8 * unit, top + 4 * unit);
            n13.SetPosition(left + 4 * unit, top + 4 * unit);
            n14.SetPosition(left + 5 * unit, top + 5 * unit);

            scene.UpdateEdges();
        }

        public override void CreateGraph(CircleNodeScene scene, 
            float rad, float ang)
        {
            this.CreateGraph(scene, rad, ang, false, false);
        }

        public override string ToString()
        {
            return "BCC Test Graph";
        }
    }

    // Copied from Wikipedia article image in order to test SCC algorithm
    // Article: http://en.wikipedia.org/wiki/Tarjan's_strongly_connected_components_algorithm
    public class SCCTestGraphCreator : AGraphCreator
    {
        public SCCTestGraphCreator() { }

        public SCCTestGraphCreator(float cx, float cy) : base(cx, cy) { }

        public override void CreateGraph(CircleNodeScene scene, 
            float rad, float ang)
        {
            CircleNode n01 = new CircleNode(scene, rad, "n01");
            CircleNode n02 = new CircleNode(scene, rad, "n02");
            CircleNode n03 = new CircleNode(scene, rad, "n03");
            ArrowEdge e01 = new ArrowEdge(n01, n02, scene, 1, ang);
            ArrowEdge e02 = new ArrowEdge(n02, n03, scene, 1, ang);
            ArrowEdge e03 = new ArrowEdge(n03, n01, scene, 1, ang);

            CircleNode n04 = new CircleNode(scene, rad, "n04");
            CircleNode n05 = new CircleNode(scene, rad, "n05");
            ArrowEdge e04 = new ArrowEdge(n04, n02, scene, 1, ang);
            ArrowEdge e05 = new ArrowEdge(n04, n03, scene, 1, ang);
            ArrowEdge e06 = new ArrowEdge(n04, n05, scene, 1, ang);
            ArrowEdge e07 = new ArrowEdge(n05, n04, scene, 1, ang);

            CircleNode n06 = new CircleNode(scene, rad, "n06");
            CircleNode n07 = new CircleNode(scene, rad, "n07");
            ArrowEdge e08 = new ArrowEdge(n05, n06, scene, 1, ang);
            ArrowEdge e09 = new ArrowEdge(n06, n07, scene, 1, ang);
            ArrowEdge e10 = new ArrowEdge(n07, n06, scene, 1, ang);
            ArrowEdge e11 = new ArrowEdge(n06, n03, scene, 1, ang);

            CircleNode n08 = new CircleNode(scene, rad, "n08");
            ArrowEdge e12 = new ArrowEdge(n08, n05, scene, 1, ang);
            ArrowEdge e13 = new ArrowEdge(n08, n07, scene, 1, ang);
            ArrowEdge e14 = new ArrowEdge(n08, n08, scene, 1, ang);

            float unit = 50;
            float left = this.mCenterX - 3 * unit;
            float top  = this.mCenterY - unit;

            n01.SetPosition(left + 0 * unit, top + 0 * unit);
            n02.SetPosition(left + 0 * unit, top + 2 * unit);
            n03.SetPosition(left + 2 * unit, top + 0 * unit);
            n04.SetPosition(left + 2 * unit, top + 2 * unit);
            n05.SetPosition(left + 4 * unit, top + 2 * unit);
            n06.SetPosition(left + 4 * unit, top + 0 * unit);
            n07.SetPosition(left + 6 * unit, top + 0 * unit);
            n08.SetPosition(left + 6 * unit, top + 2 * unit);

            scene.UpdateEdges();
        }

        public override string ToString()
        {
            return "SCC Test Graph";
        }
    }

    // Copied from Wikipedia article image in order to test Boruvka algorithm
    // Article: http://en.wikipedia.org/wiki/Bor%C5%AFvka%27s_algorithm
    public class MinSpanTreeTestGraphCreator : AGraphCreator
    {
        public override float DefaultNodeRad
        {
            get { return 20; }
        }

        public MinSpanTreeTestGraphCreator() { }

        public MinSpanTreeTestGraphCreator(float cx, float cy) 
            : base(cx, cy) 
        { 
        }

        public override void CreateGraph(CircleNodeScene scene, 
            float rad, float ang)
        {
            CircleNode nA = new CircleNode(scene, rad, "nA");
            CircleNode nB = new CircleNode(scene, rad, "nB");
            CircleNode nC = new CircleNode(scene, rad, "nC");
            CircleNode nD = new CircleNode(scene, rad, "nD");
            CircleNode nE = new CircleNode(scene, rad, "nE");
            CircleNode nF = new CircleNode(scene, rad, "nF");
            CircleNode nG = new CircleNode(scene, rad, "nG");

            ArrowEdge eAB = new ArrowEdge(nA, nB, scene,  7, ang, "eAB");
            ArrowEdge eAD = new ArrowEdge(nA, nD, scene,  4, ang, "eAD");
            ArrowEdge eBC = new ArrowEdge(nB, nC, scene, 11, ang, "eBC");
            ArrowEdge eBD = new ArrowEdge(nB, nD, scene,  9, ang, "eBD");
            ArrowEdge eBE = new ArrowEdge(nB, nE, scene, 10, ang, "eBE");
            ArrowEdge eCE = new ArrowEdge(nC, nE, scene,  5, ang, "eCE");
            ArrowEdge eDE = new ArrowEdge(nD, nE, scene, 15, ang, "eDE");
            ArrowEdge eDF = new ArrowEdge(nD, nF, scene,  6, ang, "eDF");
            ArrowEdge eEF = new ArrowEdge(nE, nF, scene, 12, ang, "eEF");
            ArrowEdge eEG = new ArrowEdge(nE, nG, scene,  8, ang, "eEG");
            ArrowEdge eFG = new ArrowEdge(nF, nG, scene, 13, ang, "eFG");

            //ArrowEdge eDA = new ArrowEdge(nD, nA, scene,  4, ang, "eDA");
            //ArrowEdge eDB = new ArrowEdge(nD, nB, scene,  9, ang, "eDB");

            float unit = 50;
            float left = this.mCenterX - 2 * unit;
            float top  = this.mCenterY - 2 * unit;

            nA.SetPosition(left + 0 * unit, top + 0 * unit);
            nB.SetPosition(left + 2 * unit, top + 1 * unit);
            nC.SetPosition(left + 4 * unit, top + 0 * unit);
            nD.SetPosition(left + 1 * unit, top + 2 * unit);
            nE.SetPosition(left + 3 * unit, top + 2 * unit);
            nF.SetPosition(left + 2 * unit, top + 3 * unit);
            nG.SetPosition(left + 4 * unit, top + 4 * unit);

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
    public class SPQRTestGraphCreator : AGraphCreator
    {
        public override float DefaultNodeRad
        {
            get { return 10; }
        }

        public override float DefaultEdgeAng
        {
            get { return 10; }//degrees
        }

        public SPQRTestGraphCreator() { }

        public SPQRTestGraphCreator(float cx, float cy) : base(cx, cy) { }

        public override void CreateGraph(CircleNodeScene scene, 
            float rad, float ang)
        {
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
            float left = this.mCenterX - 1.5f * unit;
            float top  = this.mCenterY - 4.5f * unit;

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

            float temp = left + 1.5f * unit;
            nC1.SetPosition(left, top + 6 * unit);
            nC2.SetPosition(temp, (float)(top + (6 + 0.75 * sqrt3) * unit));
            nC3.SetPosition(temp, (float)(top + (6 + 1.50 * sqrt3) * unit));

            scene.UpdateEdges();
        }

        public override string ToString()
        {
            return "SPQR Test Graph";
        }
    }

    public class WagonWheelGraphCreator : AGraphCreator
    {
        public override float DefaultNodeRad
        {
            get { return 10; }
        }

        public override float DefaultEdgeAng
        {
            get { return 10; }//degrees
        }

        public WagonWheelGraphCreator() { }

        public WagonWheelGraphCreator(float cx, float cy) : base(cx, cy) { }

        private void GenerateWagonWheelGraph(CircleNodeScene scene, 
            float rad, float ang, int spokeCount, 
            float freeArc, float minRadius, bool clockwise,
            bool rimUndirected, bool spokesUndirected, 
            bool rimReversed, bool spokesOutward)
        {
            int i;
            ArrowEdge edge;
            CircleNode[] nodes = new CircleNode[spokeCount + 1];
            CircleNode center = new CircleNode(scene, rad, "n00");
            CircleNode curr = new CircleNode(scene, rad, "n01");
            if (!spokesOutward || spokesUndirected)
                edge = new ArrowEdge(curr, center, scene, 1, ang);
            if (spokesOutward || spokesUndirected)
                edge = new ArrowEdge(center, curr, scene, 1, ang);
            nodes[0] = center;
            nodes[1] = curr;
            CircleNode prev = curr;
            for (i = 1; i < spokeCount; i++)
            {
                curr = new CircleNode(scene, rad, (i + 1).ToString("n00"));
                if (!rimReversed || rimUndirected)
                    edge = new ArrowEdge(prev, curr, scene, 1, ang);
                if (rimReversed || rimUndirected)
                    edge = new ArrowEdge(curr, prev, scene, 1, ang);
                if (!spokesOutward || spokesUndirected)
                    edge = new ArrowEdge(curr, center, scene, 1, ang);
                if (spokesOutward || spokesUndirected)
                    edge = new ArrowEdge(center, curr, scene, 1, ang);
                prev = curr;
                nodes[i + 1] = curr;
            }
            if (!rimReversed || rimUndirected)
                edge = new ArrowEdge(prev, nodes[1], scene, 1, ang);
            if (rimReversed || rimUndirected)
                edge = new ArrowEdge(nodes[1], prev, scene, 1, ang);

            center.SetPosition(this.mCenterX, this.mCenterY);
            double radius = Math.Max(minRadius, 
                spokeCount * (rad + freeArc / 2.0) / Math.PI);
            double a = 2 * Math.PI / spokeCount;
            double angle = 0;
            for (i = 1; i <= spokeCount; i++)
            {
                nodes[i].SetPosition(
                    (float)(this.mCenterX + radius * Math.Cos(angle)),
                    (float)(this.mCenterY + radius * Math.Sin(angle)));
                angle += clockwise ? -a : a;
            }
            scene.UpdateEdges();
        }

        public override void CreateGraph(CircleNodeScene scene, 
            float rad, float ang)
        {
            this.GenerateWagonWheelGraph(scene, rad, ang, 15, 
                10f, 50f, false,
                false, false, false, false);
        }

        public override string ToString()
        {
            return "Wagon Wheel Graph";
        }
    }

    public class BalloonCirclesTestGraph01 : AGraphCreator
    {
        public BalloonCirclesTestGraph01() { }

        public BalloonCirclesTestGraph01(float cx, float cy) 
            : base(cx, cy) 
        { 
        }

        public override void CreateGraph(CircleNodeScene scene, 
            float rad, float ang)
        {
            CircleNode n01 = new CircleNode(scene, rad, "n01");
            CircleNode n02 = new CircleNode(scene, rad, "n02");
            CircleNode n03 = new CircleNode(scene, rad, "n03");
            CircleNode n04 = new CircleNode(scene, rad, "n04");
            CircleNode n05 = new CircleNode(scene, rad, "n05");
            CircleNode n06 = new CircleNode(scene, rad, "n06");

            ArrowEdge e01 = new ArrowEdge(n01, n02, scene, 1, ang);
            ArrowEdge e02 = new ArrowEdge(n02, n03, scene, 1, ang);
            ArrowEdge e03 = new ArrowEdge(n03, n04, scene, 1, ang);
            ArrowEdge e04 = new ArrowEdge(n04, n05, scene, 1, ang);
            ArrowEdge e05 = new ArrowEdge(n05, n06, scene, 1, ang);
            ArrowEdge e06 = new ArrowEdge(n06, n01, scene, 1, ang);

            CircleNode n07 = new CircleNode(scene, rad, "n07");
            CircleNode n08 = new CircleNode(scene, rad, "n08");
            CircleNode n09 = new CircleNode(scene, rad, "n09");
            CircleNode n10 = new CircleNode(scene, rad, "n10");
            CircleNode n11 = new CircleNode(scene, rad, "n11");
            CircleNode n12 = new CircleNode(scene, rad, "n12");

            ArrowEdge e07 = new ArrowEdge(n01, n07, scene, 1, ang);
            ArrowEdge e08 = new ArrowEdge(n02, n08, scene, 1, ang);
            ArrowEdge e09 = new ArrowEdge(n03, n09, scene, 1, ang);
            ArrowEdge e10 = new ArrowEdge(n04, n10, scene, 1, ang);
            ArrowEdge e11 = new ArrowEdge(n05, n11, scene, 1, ang);
            ArrowEdge e12 = new ArrowEdge(n06, n12, scene, 1, ang);

            ArrowEdge e13 = new ArrowEdge(n01, n08, scene, 1, ang);
            ArrowEdge e14 = new ArrowEdge(n02, n09, scene, 1, ang);
            ArrowEdge e15 = new ArrowEdge(n03, n10, scene, 1, ang);
            ArrowEdge e16 = new ArrowEdge(n04, n11, scene, 1, ang);
            ArrowEdge e17 = new ArrowEdge(n05, n12, scene, 1, ang);
            ArrowEdge e18 = new ArrowEdge(n06, n07, scene, 1, ang);

            CircleNode n13 = new CircleNode(scene, rad, "n13");
            CircleNode n14 = new CircleNode(scene, rad, "n14");
            CircleNode n15 = new CircleNode(scene, rad, "n15");
            CircleNode n16 = new CircleNode(scene, rad, "n16");
            CircleNode n17 = new CircleNode(scene, rad, "n17");
            CircleNode n18 = new CircleNode(scene, rad, "n18");

            ArrowEdge e19 = new ArrowEdge(n07, n13, scene, 1, ang);
            ArrowEdge e20 = new ArrowEdge(n08, n14, scene, 1, ang);
            ArrowEdge e21 = new ArrowEdge(n09, n15, scene, 1, ang);
            ArrowEdge e22 = new ArrowEdge(n10, n16, scene, 1, ang);
            ArrowEdge e23 = new ArrowEdge(n11, n17, scene, 1, ang);
            ArrowEdge e24 = new ArrowEdge(n12, n18, scene, 1, ang);

            float u = 25;

            n01.SetPosition(this.mCenterX - 2 * u, this.mCenterY);
            n02.SetPosition(this.mCenterX - 1 * u, this.mCenterY + 2 * u);
            n03.SetPosition(this.mCenterX + 1 * u, this.mCenterY + 2 * u);
            n04.SetPosition(this.mCenterX + 2 * u, this.mCenterY);
            n05.SetPosition(this.mCenterX + 1 * u, this.mCenterY - 2 * u);
            n06.SetPosition(this.mCenterX - 1 * u, this.mCenterY - 2 * u);

            n07.SetPosition(this.mCenterX - 3 * u, this.mCenterY - 2 * u);
            n08.SetPosition(this.mCenterX - 3 * u, this.mCenterY + 2 * u);
            n09.SetPosition(this.mCenterX + 0 * u, this.mCenterY + 4 * u);
            n10.SetPosition(this.mCenterX + 3 * u, this.mCenterY + 2 * u);
            n11.SetPosition(this.mCenterX + 3 * u, this.mCenterY - 2 * u);
            n12.SetPosition(this.mCenterX + 0 * u, this.mCenterY - 4 * u);

            n13.SetPosition(this.mCenterX - 5 * u, this.mCenterY - 3 * u);
            n14.SetPosition(this.mCenterX - 5 * u, this.mCenterY + 3 * u);
            n15.SetPosition(this.mCenterX + 0 * u, this.mCenterY + 6 * u);
            n16.SetPosition(this.mCenterX + 5 * u, this.mCenterY + 3 * u);
            n17.SetPosition(this.mCenterX + 5 * u, this.mCenterY - 3 * u);
            n18.SetPosition(this.mCenterX + 0 * u, this.mCenterY - 6 * u);

            scene.UpdateEdges();
        }

        public override string ToString()
        {
            return "Balloon Circles Test Graph 1";
        }
    }

    public class BalloonCirclesTestGraph02 : AGraphCreator
    {
        public override float DefaultNodeRad
        {
            get { return 10; }
        }

        public override float DefaultEdgeAng
        {
            get { return 10; }//degrees
        }

        public BalloonCirclesTestGraph02() { }

        public BalloonCirclesTestGraph02(float cx, float cy)
            : base(cx, cy)
        {
        }

        public override void CreateGraph(CircleNodeScene scene, 
            float rad, float ang)
        {
            CircleNode n01 = new CircleNode(scene, rad, "n01");
            CircleNode n02 = new CircleNode(scene, rad, "n02");
            CircleNode n03 = new CircleNode(scene, rad, "n03");
            CircleNode n04 = new CircleNode(scene, rad, "n04");
            CircleNode n05 = new CircleNode(scene, rad, "n05");
            CircleNode n06 = new CircleNode(scene, rad, "n06");

            ArrowEdge e01 = new ArrowEdge(n01, n02, scene, 1, ang);
            ArrowEdge e02 = new ArrowEdge(n02, n03, scene, 1, ang);
            ArrowEdge e03 = new ArrowEdge(n03, n04, scene, 1, ang);
            ArrowEdge e04 = new ArrowEdge(n04, n05, scene, 1, ang);
            ArrowEdge e05 = new ArrowEdge(n05, n06, scene, 1, ang);
            ArrowEdge e06 = new ArrowEdge(n06, n01, scene, 1, ang);

            CircleNode n07 = new CircleNode(scene, rad, "n07");
            CircleNode n08 = new CircleNode(scene, rad, "n08");
            CircleNode n09 = new CircleNode(scene, rad, "n09");

            ArrowEdge e07 = new ArrowEdge(n01, n07, scene, 1, ang);
            ArrowEdge e08 = new ArrowEdge(n01, n08, scene, 1, ang);
            ArrowEdge e09 = new ArrowEdge(n01, n09, scene, 1, ang);

            CircleNode n10 = new CircleNode(scene, rad, "n10");
            CircleNode n11 = new CircleNode(scene, rad, "n11");
            CircleNode n12 = new CircleNode(scene, rad, "n12");
            CircleNode n13 = new CircleNode(scene, rad, "n13");
            CircleNode n14 = new CircleNode(scene, rad, "n14");

            ArrowEdge e28 = new ArrowEdge(n02, n10, scene, 1, ang);

            ArrowEdge e10 = new ArrowEdge(n10, n11, scene, 1, ang);
            ArrowEdge e11 = new ArrowEdge(n11, n12, scene, 1, ang);
            ArrowEdge e12 = new ArrowEdge(n12, n13, scene, 1, ang);
            ArrowEdge e13 = new ArrowEdge(n13, n14, scene, 1, ang);
            ArrowEdge e14 = new ArrowEdge(n14, n10, scene, 1, ang);

            CircleNode n15 = new CircleNode(scene, rad, "n15");
            CircleNode n16 = new CircleNode(scene, rad, "n16");
            CircleNode n17 = new CircleNode(scene, rad, "n17");

            ArrowEdge e15 = new ArrowEdge(n12, n15, scene, 1, ang);
            ArrowEdge e16 = new ArrowEdge(n12, n16, scene, 1, ang);
            ArrowEdge e17 = new ArrowEdge(n12, n17, scene, 1, ang);

            /*CircleNode n18 = new CircleNode(scene, rad, "n18");

            ArrowEdge e18 = new ArrowEdge(n04, n18, scene, 1, ang);

            CircleNode n19 = new CircleNode(scene, rad, "n19");
            CircleNode n20 = new CircleNode(scene, rad, "n20");
            CircleNode n21 = new CircleNode(scene, rad, "n21");
            CircleNode n22 = new CircleNode(scene, rad, "n22");
            CircleNode n23 = new CircleNode(scene, rad, "n23");
            CircleNode n24 = new CircleNode(scene, rad, "n24");
            CircleNode n25 = new CircleNode(scene, rad, "n25");
            CircleNode n26 = new CircleNode(scene, rad, "n26");
            CircleNode n27 = new CircleNode(scene, rad, "n27");

            ArrowEdge e19 = new ArrowEdge(n18, n19, scene, 1, ang);
            ArrowEdge e20 = new ArrowEdge(n18, n20, scene, 1, ang);
            ArrowEdge e21 = new ArrowEdge(n18, n21, scene, 1, ang);
            ArrowEdge e22 = new ArrowEdge(n18, n22, scene, 1, ang);
            ArrowEdge e23 = new ArrowEdge(n18, n23, scene, 1, ang);
            ArrowEdge e24 = new ArrowEdge(n18, n24, scene, 1, ang);
            ArrowEdge e25 = new ArrowEdge(n18, n25, scene, 1, ang);
            ArrowEdge e26 = new ArrowEdge(n18, n26, scene, 1, ang);
            ArrowEdge e27 = new ArrowEdge(n18, n27, scene, 1, ang);/* */

            float u = 25;

            n01.SetPosition(this.mCenterX - 4 * u, this.mCenterY - 2 * u);
            n02.SetPosition(this.mCenterX - 2 * u, this.mCenterY - 1 * u);
            n03.SetPosition(this.mCenterX - 2 * u, this.mCenterY + 1 * u);
            n04.SetPosition(this.mCenterX - 4 * u, this.mCenterY + 2 * u);
            n05.SetPosition(this.mCenterX - 6 * u, this.mCenterY + 1 * u);
            n06.SetPosition(this.mCenterX - 6 * u, this.mCenterY - 1 * u);

            n07.SetPosition(this.mCenterX - 6 * u, this.mCenterY - 3 * u);
            n08.SetPosition(this.mCenterX - 4 * u, this.mCenterY - 4 * u);
            n09.SetPosition(this.mCenterX - 2 * u, this.mCenterY - 3 * u);

            n10.SetPosition(this.mCenterX + 0 * u, this.mCenterY + 0 * u);
            n11.SetPosition(this.mCenterX + 2 * u, this.mCenterY + 1 * u);
            n12.SetPosition(this.mCenterX + 4 * u, this.mCenterY + 0 * u);
            n13.SetPosition(this.mCenterX + 3 * u, this.mCenterY - 2 * u);
            n14.SetPosition(this.mCenterX + 1 * u, this.mCenterY - 2 * u);

            n15.SetPosition(this.mCenterX + 6 * u, this.mCenterY + 1 * u);
            n16.SetPosition(this.mCenterX + 6 * u, this.mCenterY + 0 * u);
            n17.SetPosition(this.mCenterX + 6 * u, this.mCenterY - 1 * u);

            /*n18.SetPosition(this.mCenterX - 4 * u, this.mCenterY + 4 * u);

            n19.SetPosition(this.mCenterX - 6 * u, this.mCenterY + 3 * u);
            n20.SetPosition(this.mCenterX - 6 * u, this.mCenterY + 4 * u);
            n21.SetPosition(this.mCenterX - 6 * u, this.mCenterY + 5 * u);
            n22.SetPosition(this.mCenterX - 5 * u, this.mCenterY + 6 * u);
            n23.SetPosition(this.mCenterX - 4 * u, this.mCenterY + 6 * u);
            n24.SetPosition(this.mCenterX - 3 * u, this.mCenterY + 6 * u);
            n25.SetPosition(this.mCenterX - 2 * u, this.mCenterY + 5 * u);
            n26.SetPosition(this.mCenterX - 2 * u, this.mCenterY + 4 * u);
            n27.SetPosition(this.mCenterX - 2 * u, this.mCenterY + 3 * u);/* */

            scene.UpdateEdges();
        }

        public override string ToString()
        {
            return "Balloon Circles Test Graph 2";
        }
    }

}
