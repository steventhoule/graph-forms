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
                new FRLayoutForCircles(this.mScene),
                new ISOMLayoutForCircles(this.mScene),
                new KKLayoutForCircles(this.mScene),
                new LinLogLayoutForCircles(this.mScene)
            };
            this.layoutAlgCMB.Items.AddRange(this.mLayouts);
            this.mPrevLayoutIndex = this.layoutAlgCMB.SelectedIndex;

            this.GenerateRandomGraph();
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
                this.mScene.AddItem(node);
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
                    edge = new ArrowEdge(node, nodes[k]);
                    this.mScene.AddItem(edge);
                    this.mScene.Graph.AddEdge(edge, false);
                }
            }
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
    }
}
