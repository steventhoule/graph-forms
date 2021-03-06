﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GraphForms.Algorithms;
using GraphForms.Algorithms.Layout;
using GraphForms.Algorithms.Layout.ForceDirected;

namespace GraphAlgorithmDemo
{
    public partial class DemoForm : Form
    {
        private CircleNodeScene mScene;

        //private IForceDirectedLayoutAlgorithm[] mLayouts;
        private LayoutAlgorithm<CircleNode, ArrowEdge>[] mLayouts;
        private int mPrevLayoutIndex;

        private StyleAlgorithm[] mStyleAlgs;

        private IGraphCreator[] mGraphCreators;
        private bool bPendingGraphCreation;

        private ShortPathColorAlg[] mShortPathAlgs;
        private bool bShortPathOn;
        private bool bShortPathDirected;
        private bool bShortPathReversed;
        private int mShortPathAlgIndex;

        public DemoForm()
        {
            InitializeComponent();
            this.mScene = new CircleNodeScene();
            this.mScene.BoundingBox = new RectangleF(0, 0, 400, 400);
            this.mScene.UpdateBounds();
            this.mScene.AddView(this.graphPanel);

            /*this.mLayouts = new IForceDirectedLayoutAlgorithm[]
            {
                new ElasticLayoutForCircles(this.mScene),
                new FRFreeLayoutForCircles(this.mScene),
                new FRBoundedLayoutForCircles(this.mScene),
                new ISOMLayoutForCircles(this.mScene),
                new KKLayoutForCircles(this.mScene),
                new LinLogLayoutForCircles(this.mScene),
                new FDSCircleLayoutForCircles(this.mScene)
            };/* */
            this.mLayouts = new LayoutAlgorithm<CircleNode, ArrowEdge>[]
            {
                new ElasticLayoutForCircles(mScene, mScene.LayoutBBox),
                new FRFreeLayoutForCircles(mScene, mScene.LayoutBBox),
                new FRBoundedLayoutForCircles(mScene, mScene.LayoutBBox),
                new ISOMLayoutForCircles(mScene, mScene.LayoutBBox),
                new KKLayoutForCircles(mScene, mScene.LayoutBBox),
                new LinLogLayoutForCircles(mScene, mScene.LayoutBBox),
                new SCircleLayoutForCircles(mScene, mScene.LayoutBBox),
                new BalloonCirclesLayoutForCircles(mScene, mScene.LayoutBBox),
                new BalloonTreeLayoutForCircles(mScene, mScene.LayoutBBox),
                new SimpleTreeLayoutForCircles(mScene, mScene.LayoutBBox)
            };/* */
            /*this.mLayouts = new NewLayoutAlgorithm<CircleNode, ArrowEdge>[]
            {
                new ElasticLayoutForCircles(this.mScene, this.mScene),
                new FRFreeLayoutForCircles(this.mScene, this.mScene),
                new FRBoundedLayoutForCircles(this.mScene, this.mScene),
                new ISOMLayoutForCircles(this.mScene, this.mScene),
                new KKLayoutForCircles(this.mScene, this.mScene),
                new LinLogLayoutForCircles(this.mScene, this.mScene),
                new FDSCircleLayoutForCircles(this.mScene, this.mScene),
                new BalloonCirclesLayoutForCircles(this.mScene, this.mScene),
                new BalloonTreeLayoutForCircles(this.mScene, this.mScene),
                new SimpleTreeLayoutForCircles(this.mScene, this.mScene)
            };/* */
            this.layoutAlgCMB.Items.AddRange(this.mLayouts);
            this.mPrevLayoutIndex = this.layoutAlgCMB.SelectedIndex;

            this.layoutOnNodeMovedCHK.Checked = this.mScene.LayoutOnNodeMoved;
            this.layoutPausedCHK.Checked = this.mScene.LayoutPaused;

            this.mStyleAlgs = new StyleAlgorithm[]
            {
                new BCCStyleAlgorithm(),
                new CCStyleAlgorithm(),
                new SCCStyleAlgorithm(),
                new WCCStyleAlgorithm(),
                new BFSpanTreeStyleAlgorithm(),
                new DFSpanTreeStyleAlgorithm(),
                new KruskalSpanTreeStyleAlgorithm(),
                new BoruvkaSpanTreeStyleAlgorithm(),
                new PrimSpanTreeStyleAlgorithm(),
                new DFLongestPathStyleAlgorithm(),
                new AddColorToDashedStyleAlgorithm(),
#if DEBUG
                new DrawConvexHullStyleAlgorithm(),
#endif
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
                new WagonWheelGraphCreator(),
                new BalloonCirclesTestGraph01(),
                new BalloonCirclesTestGraph02()
            };
            this.graphCreatorCMB.Items.AddRange(this.mGraphCreators);

            this.mShortPathAlgs = new ShortPathColorAlg[]
            {
                new FloydWarshallShortPathColorAlg(),
                new BellManFordShortPathColorAlg(),
                new DijkstraShortPathColorAlg()
            };
            this.shortPathAlgCMB.Items.AddRange(this.mShortPathAlgs);

            this.bShortPathOn = false;
            this.shortPathOnOffBTN.Text = "On";
            this.bShortPathDirected = this.shortPathDirectedCHK.Checked;
            this.bShortPathReversed = this.shortPathReversedCHK.Checked;
            this.mShortPathAlgIndex = this.shortPathAlgCMB.SelectedIndex;

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
            this.RefreshShortPathAlgs();

            this.mScene.NodeMouseUp += new Action<CircleNode>(sceneNodeMouseUp);
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
                LayoutAlgorithm<CircleNode, ArrowEdge> alg
                    = this.mLayouts[this.mPrevLayoutIndex];
                /*if (alg.Spring == null)
                {
                    this.layoutParamsGrid.SelectedObject = alg;//.Parameters;
                }
                else
                {
                    this.layoutParamsGrid.SelectedObjects
                        = new object[] { alg, alg.Spring };
                }/* */
                this.layoutParamsGrid.SelectedObject = alg;//.Parameters;
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
            if (this.mScene.Layout != null)
            {
                bool flag = !this.mScene.IsLayoutRunning;
                this.mScene.IsLayoutRunning = flag;
                this.layoutStartStopBTN.Text = flag ? "Stop" : "Start";
            }
        }

        private void layoutShuffleClick(object sender, EventArgs e)
        {
            if (this.mScene.Layout != null)
            {
                this.mScene.Layout.ShuffleNodes(true);
            }
        }

        private void sceneNodeMouseUp(CircleNode obj)
        {
            if (this.bShortPathOn)
            {
                // Force the selected shortest path algorithm
                // to color the shortest path between last two nodes
                // in the scene's MouseUpHistory
                ShortPathColorAlg spca = this.shortPathAlgCMB.SelectedItem as ShortPathColorAlg;
                if (spca != null)
                {
                    spca.ColorShortestPath(this.mScene);
                }
            }
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
                    this.TurnOffShortPath();
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
                    this.RefreshShortPathAlgs();
                    this.graphCreatorCMB.Enabled = true;
                    this.shortPathAlgCMB.Enabled = true;
                    this.shortPathDirectedCHK.Enabled = true;
                    this.shortPathReversedCHK.Enabled = true;
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
                this.TurnOffShortPath();
                if (this.mScene.IsLayoutRunning)
                {
                    this.mScene.IsLayoutRunning = false;

                    this.mScene.LayoutPaused = false;
                    this.layoutPausedCHK.Checked = false;
                    
                    this.graphCreatorCMB.Enabled = false;

                    this.shortPathAlgCMB.Enabled = false;
                    this.shortPathDirectedCHK.Enabled = false;
                    this.shortPathReversedCHK.Enabled = false;

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
                    this.RefreshShortPathAlgs();
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

        private void shortPathAlgSelectedValueChanged(object sender, EventArgs e)
        {
            ShortPathColorAlg spca 
                = this.shortPathAlgCMB.SelectedItem as ShortPathColorAlg;
            if (spca != null)
            {
                this.shortPathDirectedCHK.Checked = spca.Directed;
                this.shortPathReversedCHK.Checked = spca.Reversed;
                spca.RefreshAlgorithm(this.mScene);
            }
        }

        private void shortPathDirectedCheckedChanged(object sender, EventArgs e)
        {
            ShortPathColorAlg spca 
                = this.shortPathAlgCMB.SelectedItem as ShortPathColorAlg;
            if (spca != null && spca.Directed != this.shortPathDirectedCHK.Checked)
            {
                spca.Directed = this.shortPathDirectedCHK.Checked;
                spca.IsDirty = true;
                spca.RefreshAlgorithm(this.mScene);
            }
        }

        private void shortPathReversedCheckedChanged(object sender, EventArgs e)
        {
            ShortPathColorAlg spca 
                = this.shortPathAlgCMB.SelectedItem as ShortPathColorAlg;
            if (spca != null && spca.Reversed != this.shortPathReversedCHK.Checked)
            {
                spca.Reversed = this.shortPathReversedCHK.Checked;
                spca.IsDirty = true;
                spca.RefreshAlgorithm(this.mScene);
            }
        }

        private void TurnOffShortPath()
        {
            if (this.bShortPathOn)
            {
                this.bShortPathOn = false;
                this.shortPathOnOffBTN.Text = "On";
                //this.shortPathAlgCMB.Enabled = true;
                //this.shortPathDirectedCHK.Enabled = true;
                //this.shortPathReversedCHK.Enabled = true;
            }
        }

        private void RefreshShortPathAlgs()
        {
            for (int i = 0; i < this.mShortPathAlgs.Length; i++)
            {
                this.mShortPathAlgs[i].IsDirty = true;
            }
        }

        private void shortPathOnOffClick(object sender, EventArgs e)
        {
            this.bShortPathOn = !this.bShortPathOn;
            this.shortPathOnOffBTN.Text = this.bShortPathOn ? "Off" : "On";
            if (this.bShortPathOn)
            {
                ShortPathColorAlg spca 
                    = this.shortPathAlgCMB.SelectedItem as ShortPathColorAlg;
                if (spca != null)
                {
                    spca.RefreshAlgorithm(this.mScene);
                }
                //this.shortPathAlgCMB.Enabled = false;
                //this.shortPathDirectedCHK.Enabled = false;
                //this.shortPathReversedCHK.Enabled = false;
            }
            else
            {
                //this.shortPathAlgCMB.Enabled = true;
                //this.shortPathDirectedCHK.Enabled = true;
                //this.shortPathReversedCHK.Enabled = true;
            }
        }
    }
}
