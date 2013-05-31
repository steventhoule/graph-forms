using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GraphForms.Algorithms.Layout.ForceDirected;

namespace GraphAlgorithmDemo
{
    public partial class DemoForm : Form
    {
        private CircleNodeScene mScene;

        private IForceDirectedLayoutAlgorithm[] mLayouts;
        private int mPrevLayoutIndex;

        private StyleAlgorithm[] mStyleAlgs;

        public DemoForm()
        {
            InitializeComponent();
            this.mScene = new CircleNodeScene();
            this.mScene.BoundingBox = new RectangleF(0, 0, 400, 400);
            this.mScene.UpdateBounds();
            this.mScene.AddView(this.graphPanel);

            this.mLayouts = new IForceDirectedLayoutAlgorithm[]
            {
                new ElasticLayoutForCircles(this.mScene),
                new FRFreeLayoutForCircles(this.mScene),
                new FRBoundedLayoutForCircles(this.mScene),
                new ISOMLayoutForCircles(this.mScene),
                new KKLayoutForCircles(this.mScene),
                new LinLogLayoutForCircles(this.mScene),
                new FDSingleCircleLayoutForCircles(this.mScene)
            };
            this.layoutAlgCMB.Items.AddRange(this.mLayouts);
            this.mPrevLayoutIndex = this.layoutAlgCMB.SelectedIndex;

            this.mStyleAlgs = new StyleAlgorithm[]
            {
                new BCCStyleAlgorithm(this.mScene),
                new SCCStyleAlgorithm(this.mScene),
                new BFSpanTreeStyleAlgorithm(this.mScene),
                new DFSpanTreeStyleAlgorithm(this.mScene),
                new KruskalSpanTreeStyleAlgorithm(this.mScene),
                new BoruvkaSpanTreeStyleAlgorithm(this.mScene),
                new DFLongestPathStyleAlgorithm(this.mScene),
                new ClearAllStyleAlgorithm(this.mScene)
            };
            this.styleAlgCMB.Items.AddRange(this.mStyleAlgs);

            //this.GenerateRandomGraph();
            this.GenerateGraph3();
            //this.GenerateWagonWheelGraph(15);
            //this.mScene.Layout.ShuffleNodePositions();

            this.mScene.NodeMoved += new Action<CircleNode>(sceneNodeMoved);
            this.mScene.LayoutStopped += new EventHandler(sceneLayoutStopped);
        }

        private void GenerateRandomGraph()
        {
            Random rnd = new Random(DateTime.Now.Millisecond);
            int i, count = rnd.Next() % 10 + 5;
            CircleNode[] nodes = new CircleNode[count];
            CircleNode node;
            for (i = 0; i < count; i++)
            {
                node = new CircleNode(this.mScene, 
                    (i + 1).ToString("n00"));
                nodes[i] = node;
                //this.mScene.AddItem(node);
            }
            int j, k, eCount;
            ArrowEdge edge;
            for (i = 0; i < count; i++)
            {
                node = nodes[i];
                eCount = rnd.Next() % (count - 4) + 2;
                for (j = 0; j < eCount; j++)
                {
                    k = rnd.Next() % count;
                    while (k == i || //Comment out for self-referencing edges
                        this.mScene.Graph.IndexOfEdge(node, nodes[k]) >= 0)
                    {
                        k = rnd.Next() % count;
                    }
                    edge = new ArrowEdge(node, nodes[k], this.mScene);
                    //this.mScene.AddItem(edge);
                    //this.mScene.Graph.AddEdge(edge, false);
                }
            }
        }

        // Copied from Wikipedia article image in order to test BCC algorithm
        // Article: http://en.wikipedia.org/wiki/Biconnected_component
        private void GenerateGraph1()
        {
            CircleNode n01 = new CircleNode(this.mScene, "n01");
            CircleNode n02 = new CircleNode(this.mScene, "n02");
            CircleNode n03 = new CircleNode(this.mScene, "n03");
            CircleNode n04 = new CircleNode(this.mScene, "n04");

            ArrowEdge e01 = new ArrowEdge(n01, n02, this.mScene);
            ArrowEdge e02 = new ArrowEdge(n01, n03, this.mScene);
            ArrowEdge e03 = new ArrowEdge(n02, n04, this.mScene);
            ArrowEdge e04 = new ArrowEdge(n03, n04, this.mScene);

            CircleNode n05 = new CircleNode(this.mScene, "n05");
            ArrowEdge e05 = new ArrowEdge(n04, n05, this.mScene);

            CircleNode n06 = new CircleNode(this.mScene, "n06");
            ArrowEdge e06 = new ArrowEdge(n05, n06, this.mScene);

            CircleNode n07 = new CircleNode(this.mScene, "n07");
            CircleNode n08 = new CircleNode(this.mScene, "n08");
            CircleNode n09 = new CircleNode(this.mScene, "n09");
            CircleNode n10 = new CircleNode(this.mScene, "n10");
            CircleNode n11 = new CircleNode(this.mScene, "n11");
            CircleNode n12 = new CircleNode(this.mScene, "n12");

            ArrowEdge e07 = new ArrowEdge(n07, n08, this.mScene);
            ArrowEdge e08 = new ArrowEdge(n07, n09, this.mScene);
            ArrowEdge e09 = new ArrowEdge(n08, n09, this.mScene);
            ArrowEdge e10 = new ArrowEdge(n08, n10, this.mScene);
            ArrowEdge e11 = new ArrowEdge(n10, n11, this.mScene);
            ArrowEdge e12 = new ArrowEdge(n09, n12, this.mScene);
            ArrowEdge e13 = new ArrowEdge(n11, n12, this.mScene);

            ArrowEdge e14 = new ArrowEdge(n06, n10, this.mScene);

            CircleNode n13 = new CircleNode(this.mScene, "n13");
            ArrowEdge e15 = new ArrowEdge(n10, n13, this.mScene);

            CircleNode n14 = new CircleNode(this.mScene, "n14");
            ArrowEdge e16 = new ArrowEdge(n11, n14, this.mScene);

            /*ArrowEdge e17 = new ArrowEdge(n02, n01, this.mScene);
            ArrowEdge e18 = new ArrowEdge(n03, n01, this.mScene);
            ArrowEdge e19 = new ArrowEdge(n04, n02, this.mScene);
            ArrowEdge e20 = new ArrowEdge(n04, n03, this.mScene);

            ArrowEdge e21 = new ArrowEdge(n05, n04, this.mScene);

            ArrowEdge e22 = new ArrowEdge(n06, n05, this.mScene);

            ArrowEdge e23 = new ArrowEdge(n08, n07, this.mScene);
            ArrowEdge e24 = new ArrowEdge(n09, n07, this.mScene);
            ArrowEdge e25 = new ArrowEdge(n09, n08, this.mScene);
            ArrowEdge e26 = new ArrowEdge(n10, n08, this.mScene);
            ArrowEdge e27 = new ArrowEdge(n11, n10, this.mScene);
            ArrowEdge e28 = new ArrowEdge(n12, n09, this.mScene);
            ArrowEdge e29 = new ArrowEdge(n12, n11, this.mScene);

            ArrowEdge e30 = new ArrowEdge(n10, n06, this.mScene);

            ArrowEdge e31 = new ArrowEdge(n13, n10, this.mScene);

            ArrowEdge e32 = new ArrowEdge(n14, n11, this.mScene);/* */

            ArrowEdge[] edges = { e01, e02, e03, e04, e05, e06, e07, e08, 
                                  e09, e10, e11, e12, e13, e14, e15, e16,
                                  //e17, e18, e19, e20, e21, e22, e23, e24, 
                                  //e25, e26, e27, e28, e29, e30, e31, e32 
                                };

        }

        // Copied from Wikipedia article image in order to test SCC algorithm
        // Article: http://en.wikipedia.org/wiki/Tarjan's_strongly_connected_components_algorithm
        private void GenerateGraph2()
        {
            CircleNode n01 = new CircleNode(this.mScene, "n01");
            CircleNode n02 = new CircleNode(this.mScene, "n02");
            CircleNode n03 = new CircleNode(this.mScene, "n03");
            ArrowEdge e01 = new ArrowEdge(n01, n02, this.mScene);
            ArrowEdge e02 = new ArrowEdge(n02, n03, this.mScene);
            ArrowEdge e03 = new ArrowEdge(n03, n01, this.mScene);

            CircleNode n04 = new CircleNode(this.mScene, "n04");
            CircleNode n05 = new CircleNode(this.mScene, "n05");
            ArrowEdge e04 = new ArrowEdge(n04, n02, this.mScene);
            ArrowEdge e05 = new ArrowEdge(n04, n03, this.mScene);
            ArrowEdge e06 = new ArrowEdge(n04, n05, this.mScene);
            ArrowEdge e07 = new ArrowEdge(n05, n04, this.mScene);

            CircleNode n06 = new CircleNode(this.mScene, "n06");
            CircleNode n07 = new CircleNode(this.mScene, "n07");
            ArrowEdge e08 = new ArrowEdge(n05, n06, this.mScene);
            ArrowEdge e09 = new ArrowEdge(n06, n07, this.mScene);
            ArrowEdge e10 = new ArrowEdge(n07, n06, this.mScene);
            ArrowEdge e11 = new ArrowEdge(n06, n03, this.mScene);

            CircleNode n08 = new CircleNode(this.mScene, "n08");
            ArrowEdge e12 = new ArrowEdge(n08, n05, this.mScene);
            ArrowEdge e13 = new ArrowEdge(n08, n07, this.mScene);
            ArrowEdge e14 = new ArrowEdge(n08, n08, this.mScene);

            n01.SetPosition( 50, 150);
            n02.SetPosition( 50, 250);
            n03.SetPosition(150, 150);
            n04.SetPosition(150, 250);
            n05.SetPosition(250, 250);
            n06.SetPosition(250, 150);
            n07.SetPosition(350, 150);
            n08.SetPosition(350, 250);
            ArrowEdge[] edges = { e01, e02, e03, e04, e05, e06, e07, e08, 
                                  e09, e10, e11, e12, e13, e14 };
            for (int i = 0; i < edges.Length; i++)
            {
                edges[i].Update();
            }
        }

        // Copied from Wikipedia article image in order to test Boruvka algorithm
        // Article: http://en.wikipedia.org/wiki/Bor%C5%AFvka%27s_algorithm
        private void GenerateGraph3()
        {
            float rad = 20;
            CircleNode nA = new CircleNode(this.mScene, rad, "nA");
            CircleNode nB = new CircleNode(this.mScene, rad, "nB");
            CircleNode nC = new CircleNode(this.mScene, rad, "nC");
            CircleNode nD = new CircleNode(this.mScene, rad, "nD");
            CircleNode nE = new CircleNode(this.mScene, rad, "nE");
            CircleNode nF = new CircleNode(this.mScene, rad, "nF");
            CircleNode nG = new CircleNode(this.mScene, rad, "nG");

            ArrowEdge eAB = new ArrowEdge(nA, nB, this.mScene,  7, "eAB");
            ArrowEdge eAD = new ArrowEdge(nA, nD, this.mScene,  4, "eAD");
            ArrowEdge eBC = new ArrowEdge(nB, nC, this.mScene, 11, "eBC");
            ArrowEdge eBD = new ArrowEdge(nB, nD, this.mScene,  9, "eBD");
            ArrowEdge eBE = new ArrowEdge(nB, nE, this.mScene, 10, "eBE");
            ArrowEdge eCE = new ArrowEdge(nC, nE, this.mScene,  5, "eCE");
            ArrowEdge eDE = new ArrowEdge(nD, nE, this.mScene, 15, "eDE");
            ArrowEdge eDF = new ArrowEdge(nD, nF, this.mScene,  6, "eDF");
            ArrowEdge eEF = new ArrowEdge(nE, nF, this.mScene, 12, "eEF");
            ArrowEdge eEG = new ArrowEdge(nE, nG, this.mScene,  8, "eEG");
            ArrowEdge eFG = new ArrowEdge(nF, nG, this.mScene, 13, "eFG");

            //ArrowEdge eAD = new ArrowEdge(nD, nA, this.mScene,  4, "eDA");
            //ArrowEdge eBD = new ArrowEdge(nD, nB, this.mScene,  9, "eDB");

            nA.SetPosition(100, 100);
            nB.SetPosition(200, 150);
            nC.SetPosition(300, 100);
            nD.SetPosition(125, 200);
            nE.SetPosition(275, 200);
            nF.SetPosition(200, 250);
            nG.SetPosition(300, 300);
            ArrowEdge[] edges = { eAB, eAD, eBC, eBD, eBE, eCE, eDE, eDF, eEF, eEG, eFG };
            for (int i = 0; i < edges.Length; i++)
            {
                edges[i].Update();
            }
        }

        // Copied from Wikipedia article image in order to test 
        // SPQR Tree (Triconnected Components) algorithm
        // Article: http://en.wikipedia.org/wiki/SPQR_tree
        private void GenerateGraph4()
        {
            float rad = 10;
            float ang = 10;

            CircleNode nA1 = new CircleNode(this.mScene, rad, "A1");
            CircleNode nA2 = new CircleNode(this.mScene, rad, "A2");
            CircleNode nA3 = new CircleNode(this.mScene, rad, "A3");
            CircleNode nA4 = new CircleNode(this.mScene, rad, "A4");
            CircleNode nA5 = new CircleNode(this.mScene, rad, "A5");
            CircleNode nA6 = new CircleNode(this.mScene, rad, "A6");
            CircleNode nA7 = new CircleNode(this.mScene, rad, "A7");
            CircleNode nA8 = new CircleNode(this.mScene, rad, "A8");

            ArrowEdge eA1A2 = new ArrowEdge(nA1, nA2, mScene, 1, ang, "A1->A2");
            ArrowEdge eA7A1 = new ArrowEdge(nA7, nA1, mScene, 1, ang, "A7->A1");
            ArrowEdge eA2A8 = new ArrowEdge(nA2, nA8, mScene, 1, ang, "A2->A8");
            ArrowEdge eA1A3 = new ArrowEdge(nA1, nA3, mScene, 1, ang, "A1->A3");
            ArrowEdge eA2A4 = new ArrowEdge(nA2, nA4, mScene, 1, ang, "A2->A4");
            ArrowEdge eA7A5 = new ArrowEdge(nA7, nA5, mScene, 1, ang, "A7->A5");
            ArrowEdge eA8A6 = new ArrowEdge(nA8, nA6, mScene, 1, ang, "A8->A6");
            ArrowEdge eA3A4 = new ArrowEdge(nA3, nA4, mScene, 1, ang, "A3->A4");
            ArrowEdge eA4A6 = new ArrowEdge(nA4, nA6, mScene, 1, ang, "A4->A6");
            ArrowEdge eA6A5 = new ArrowEdge(nA6, nA5, mScene, 1, ang, "A6->A5");
            ArrowEdge eA5A3 = new ArrowEdge(nA5, nA3, mScene, 1, ang, "A5->A3");

            CircleNode nB1 = new CircleNode(this.mScene, rad, "B1");
            CircleNode nB2 = new CircleNode(this.mScene, rad, "B2");
            CircleNode nB3 = new CircleNode(this.mScene, rad, "B3");
            CircleNode nB4 = new CircleNode(this.mScene, rad, "B4");
            CircleNode nB5 = new CircleNode(this.mScene, rad, "B5");

            ArrowEdge eA8B1 = new ArrowEdge(nA8, nB1, mScene, 1, ang, "A8->B1");
            ArrowEdge eB1B5 = new ArrowEdge(nB1, nB5, mScene, 1, ang, "B1->B5");
            ArrowEdge eB5B4 = new ArrowEdge(nB5, nB4, mScene, 1, ang, "B5->B4");
            ArrowEdge eA8B2 = new ArrowEdge(nA8, nB2, mScene, 1, ang, "A8->B2");
            ArrowEdge eB1B2 = new ArrowEdge(nB1, nB2, mScene, 1, ang, "B1->B2");
            ArrowEdge eB5B3 = new ArrowEdge(nB5, nB3, mScene, 1, ang, "B5->B3");
            ArrowEdge eB4B3 = new ArrowEdge(nB4, nB3, mScene, 1, ang, "B4->B3");
            ArrowEdge eB2B3 = new ArrowEdge(nB2, nB3, mScene, 1, ang, "B2->B3");

            CircleNode nC1 = new CircleNode(this.mScene, rad, "C1");
            CircleNode nC2 = new CircleNode(this.mScene, rad, "C2");
            CircleNode nC3 = new CircleNode(this.mScene, rad, "C3");

            ArrowEdge eB4C3 = new ArrowEdge(nB4, nC3, mScene, 1, ang, "B4->C3");
            ArrowEdge eC3C1 = new ArrowEdge(nC3, nC1, mScene, 1, ang, "C3->C1");
            ArrowEdge eC1C2 = new ArrowEdge(nC1, nC2, mScene, 1, ang, "C1->C2");
            ArrowEdge eB4C2 = new ArrowEdge(nB4, nC2, mScene, 1, ang, "B4->C2");
            ArrowEdge eC3C2 = new ArrowEdge(nC3, nC2, mScene, 1, ang, "C3->C2");

            ArrowEdge eB4C1 = new ArrowEdge(nB4, nC1, mScene, 1, ang, "B4->C1");

            ArrowEdge eC1A7 = new ArrowEdge(nC1, nA7, mScene, 1, ang, "C1->A7");

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

            ArrowEdge[] edges = { eA1A2, eA7A1, eA2A8, eA1A3, eA2A4, eA7A5, 
                                  eA8A6, eA3A4, eA4A6, eA6A5, eA5A3,
                                  eA8B1, eB1B5, eB5B4, eA8B2, eB1B2, eB5B3,
                                  eB4B3, eB2B3,
                                  eB4C3, eC3C1, eC1C2, eB4C2, eC3C2, 
                                  eB4C1, eC1A7 };
            for (int i = 0; i < edges.Length; i++)
            {
                edges[i].Update();
            }
        }

        private void GenerateWagonWheelGraph(int spokeCount)
        {
            ArrowEdge edge;
            CircleNode center = new CircleNode(this.mScene, "n00");
            CircleNode first = new CircleNode(this.mScene, "n01");
            edge = new ArrowEdge(first, center, this.mScene);
            //edge = new ArrowEdge(center, first, this.mScene);
            CircleNode curr, prev = first;
            for (int i = 1; i < spokeCount; i++)
            {
                curr = new CircleNode(this.mScene, (i + 1).ToString("n00"));
                edge = new ArrowEdge(prev, curr, this.mScene);
                //edge = new ArrowEdge(curr, prev, this.mScene);
                edge = new ArrowEdge(curr, center, this.mScene);
                //edge = new ArrowEdge(center, curr, this.mScene);
                prev = curr;
            }
            edge = new ArrowEdge(prev, first, this.mScene);
            //edge = new ArrowEdge(first, prev, this.mScene);
        }

        private void DemoForm_Load(object sender, EventArgs e)
        {

        }

        private void layoutAlgSelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.layoutAlgCMB.SelectedIndex != this.mPrevLayoutIndex)
            {
                this.mPrevLayoutIndex = this.layoutAlgCMB.SelectedIndex;
                this.mScene.Layout = this.mLayouts[this.mPrevLayoutIndex];
                this.layoutStartStopBTN.Text = "Start";
                this.layoutParamsGrid.SelectedObject 
                    = this.mLayouts[this.mPrevLayoutIndex].Parameters;
            }
        }

        private void layoutOnNodeMovedCheckedChanged(object sender, EventArgs e)
        {
            this.mScene.LayoutOnNodeMoved = this.layoutOnNodeMovedCHK.Checked;
        }

        private void layoutPausedCheckedChanged(object sender, EventArgs e)
        {
            this.mScene.LayoutPaused = this.layoutPausedCHK.Checked;
        }

        private void layoutStartStopClick(object sender, EventArgs e)
        {
            bool flag = !this.mScene.IsLayoutRunning;
            this.mScene.IsLayoutRunning = flag;
            this.layoutStartStopBTN.Text = flag ? "Stop" : "Start";
        }

        private void layoutShuffleClick(object sender, EventArgs e)
        {
            this.mScene.Layout.ShuffleNodePositions();
        }

        private void sceneNodeMoved(CircleNode obj)
        {
            if (this.mScene.LayoutOnNodeMoved)
                this.layoutStartStopBTN.Text = "Stop";
        }

        private void sceneLayoutStopped(object sender, EventArgs e)
        {
            this.layoutStartStopBTN.Text = "Start";
        }

        private void styleAlgSelectedValueChanged(object sender, EventArgs e)
        {
            StyleAlgorithm alg = this.styleAlgCMB.SelectedItem as StyleAlgorithm;
            if (alg != null)
            {
                this.styleAlgDirectedCHK.Enabled = alg.EnableDirected;
                this.styleAlgReversedCHK.Enabled = alg.EnableReversed;
                this.styleAlgDirectedCHK.Checked = alg.Directed;
                this.styleAlgReversedCHK.Checked = alg.Reversed;
            }
        }

        private void styleAlgUndirectedCheckedChanged(object sender, EventArgs e)
        {
            StyleAlgorithm alg = this.styleAlgCMB.SelectedItem as StyleAlgorithm;
            if (alg != null)
            {
                alg.Directed = this.styleAlgDirectedCHK.Checked;
            }
        }

        private void styleAlgReversedCheckedChanged(object sender, EventArgs e)
        {
            StyleAlgorithm alg = this.styleAlgCMB.SelectedItem as StyleAlgorithm;
            if (alg != null)
            {
                alg.Reversed = this.styleAlgReversedCHK.Checked;
            }
        }

        private void styleAlgTestClick(object sender, EventArgs e)
        {
            StyleAlgorithm alg = this.styleAlgCMB.SelectedItem as StyleAlgorithm;
            if (alg != null)
            {
                alg.Compute();
            }
        }
    }
}
