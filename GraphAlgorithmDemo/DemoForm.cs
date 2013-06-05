using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GraphForms.Algorithms;
using GraphForms.Algorithms.Layout.ForceDirected;

namespace GraphAlgorithmDemo
{
    public partial class DemoForm : Form
    {
        private CircleNodeScene mScene;

        private IForceDirectedLayoutAlgorithm[] mLayouts;
        private int mPrevLayoutIndex;

        private StyleAlgorithm[] mStyleAlgs;

        private IGraphCreator[] mGraphCreators;
        private bool bPendingGraphCreation;

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

            this.layoutOnNodeMovedCHK.Checked = this.mScene.LayoutOnNodeMoved;
            this.layoutPausedCHK.Checked = this.mScene.LayoutPaused;

            this.mStyleAlgs = new StyleAlgorithm[]
            {
                new BCCStyleAlgorithm(),
                new SCCStyleAlgorithm(),
                new WCCStyleAlgorithm(),
                new BFSpanTreeStyleAlgorithm(),
                new DFSpanTreeStyleAlgorithm(),
                new KruskalSpanTreeStyleAlgorithm(),
                new BoruvkaSpanTreeStyleAlgorithm(),
                new DFLongestPathStyleAlgorithm(),
                new ClearAllStyleAlgorithm()
            };
            this.styleAlgCMB.Items.AddRange(this.mStyleAlgs);

            this.mGraphCreators = new IGraphCreator[]
            {
                new RandomGraphCreator(),
                new BCCTestGraphCreator(),
                new SCCTestGraphCreator(),
                new MinSpanTreeTestGraphCreator(),
                new SPQRTestGraphCreator(),
                new WagonWheelGraphCreator()
            };
            this.graphCreatorCMB.Items.AddRange(this.mGraphCreators);

            IGraphCreator gc = new BCCTestGraphCreator();
            if (this.graphStyleResetOnCreateCHK.Checked)
            {
                this.nodeRadNUM.Value = (decimal)gc.DefaultNodeRad;
                this.edgeAngNUM.Value = (decimal)gc.DefaultEdgeAng;
                gc.CreateGraph(this.mScene,
                    gc.DefaultNodeRad, gc.DefaultEdgeAng);
            }
            else
            {
                gc.CreateGraph(this.mScene,
                    (float)this.nodeRadNUM.Value,
                    (float)this.edgeAngNUM.Value);
            }

            this.mScene.NodeMoved += new Action<CircleNode>(sceneNodeMoved);
            this.mScene.LayoutStopped += new EventHandler(sceneLayoutStopped);

            this.bPendingGraphCreation = false;
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
            if (this.bPendingGraphCreation)
            {
                IGraphCreator gc = this.graphCreatorCMB.SelectedItem as IGraphCreator;
                if (gc != null)
                {
                    this.mScene.ClearGraph();
                    if (this.graphStyleResetOnCreateCHK.Checked)
                    {
                        this.nodeRadNUM.Value = (decimal)gc.DefaultNodeRad;
                        this.edgeAngNUM.Value = (decimal)gc.DefaultEdgeAng;
                        gc.CreateGraph(this.mScene,
                            gc.DefaultNodeRad, gc.DefaultEdgeAng);
                    }
                    else
                    {
                        gc.CreateGraph(this.mScene,
                            (float)this.nodeRadNUM.Value,
                            (float)this.edgeAngNUM.Value);
                    }
                    this.graphCreatorCMB.Enabled = true;
                }
                this.bPendingGraphCreation = false;
            }
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
                alg.Compute(this.mScene);
            }
        }

        private void createGraphClick(object sender, EventArgs e)
        {
            IGraphCreator gc = this.graphCreatorCMB.SelectedItem as IGraphCreator;
            if (gc != null)
            {
                if (this.mScene.IsLayoutRunning)
                {
                    this.mScene.IsLayoutRunning = false;

                    this.mScene.LayoutPaused = false;
                    this.layoutPausedCHK.Checked = false;
                    
                    this.graphCreatorCMB.Enabled = false;
                    this.bPendingGraphCreation = true;
                }
                else
                {
                    this.mScene.ClearGraph();
                    if (this.graphStyleResetOnCreateCHK.Checked)
                    {
                        this.nodeRadNUM.Value = (decimal)gc.DefaultNodeRad;
                        this.edgeAngNUM.Value = (decimal)gc.DefaultEdgeAng;
                        gc.CreateGraph(this.mScene,
                            gc.DefaultNodeRad, gc.DefaultEdgeAng);
                    }
                    else
                    {
                        gc.CreateGraph(this.mScene,
                            (float)this.nodeRadNUM.Value,
                            (float)this.edgeAngNUM.Value);
                    }
                }
            }
        }

        private void nodeRadValueChanged(object sender, EventArgs e)
        {
            float rad = (float)this.nodeRadNUM.Value;
            Digraph<CircleNode, ArrowEdge>.GNode[] nodes
                = this.mScene.Graph.InternalNodes;
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i].Data.Radius = rad;
            }
            this.mScene.UpdateEdges();
        }

        private void edgeAngValueChanged(object sender, EventArgs e)
        {
            float ang = (float)this.edgeAngNUM.Value;
            Digraph<CircleNode, ArrowEdge>.GEdge[] edges
                = this.mScene.Graph.InternalEdges;
            for (int i = 0; i < edges.Length; i++)
            {
                edges[i].Data.Angle = ang;
            }
        }
    }
}
