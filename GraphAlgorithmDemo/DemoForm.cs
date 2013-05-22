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
                new BFSpanTreeStyleAlgorithm(this.mScene),
                new DFSpanTreeStyleAlgorithm(this.mScene),
                new ClearAllStyleAlgorithm(this.mScene)
            };
            this.styleAlgCMB.Items.AddRange(this.mStyleAlgs);

            //this.GenerateRandomGraph();
            this.GenerateGraph1();
            //this.GenerateWagonWheelGraph(15);
            this.mScene.Layout.ShuffleNodePositions();

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
                node = new CircleNode(this.mScene);
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
            CircleNode n01 = new CircleNode(this.mScene);
            CircleNode n02 = new CircleNode(this.mScene);
            CircleNode n03 = new CircleNode(this.mScene);
            CircleNode n04 = new CircleNode(this.mScene);

            ArrowEdge e01 = new ArrowEdge(n01, n02, this.mScene);
            ArrowEdge e02 = new ArrowEdge(n01, n03, this.mScene);
            ArrowEdge e03 = new ArrowEdge(n02, n04, this.mScene);
            ArrowEdge e04 = new ArrowEdge(n03, n04, this.mScene);

            CircleNode n05 = new CircleNode(this.mScene);
            ArrowEdge e05 = new ArrowEdge(n04, n05, this.mScene);

            CircleNode n06 = new CircleNode(this.mScene);
            ArrowEdge e06 = new ArrowEdge(n05, n06, this.mScene);

            CircleNode n07 = new CircleNode(this.mScene);
            CircleNode n08 = new CircleNode(this.mScene);
            CircleNode n09 = new CircleNode(this.mScene);
            CircleNode n10 = new CircleNode(this.mScene);
            CircleNode n11 = new CircleNode(this.mScene);
            CircleNode n12 = new CircleNode(this.mScene);

            ArrowEdge e07 = new ArrowEdge(n07, n08, this.mScene);
            ArrowEdge e08 = new ArrowEdge(n07, n09, this.mScene);
            ArrowEdge e09 = new ArrowEdge(n08, n09, this.mScene);
            ArrowEdge e10 = new ArrowEdge(n08, n10, this.mScene);
            ArrowEdge e11 = new ArrowEdge(n10, n11, this.mScene);
            ArrowEdge e12 = new ArrowEdge(n09, n12, this.mScene);
            ArrowEdge e13 = new ArrowEdge(n11, n12, this.mScene);

            ArrowEdge e14 = new ArrowEdge(n06, n10, this.mScene);

            CircleNode n13 = new CircleNode(this.mScene);
            ArrowEdge e15 = new ArrowEdge(n10, n13, this.mScene);

            CircleNode n14 = new CircleNode(this.mScene);
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
        }

        private void GenerateWagonWheelGraph(int spokeCount)
        {
            ArrowEdge edge;
            CircleNode center = new CircleNode(this.mScene);
            CircleNode first = new CircleNode(this.mScene);
            edge = new ArrowEdge(first, center, this.mScene);
            //edge = new ArrowEdge(center, first, this.mScene);
            CircleNode curr, prev = first;
            for (int i = 1; i < spokeCount; i++)
            {
                curr = new CircleNode(this.mScene);
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

        private void colorAlgTestClick(object sender, EventArgs e)
        {
            StyleAlgorithm alg = this.styleAlgCMB.SelectedItem as StyleAlgorithm;
            if (alg != null)
            {
                alg.Compute();
            }
        }
    }
}
