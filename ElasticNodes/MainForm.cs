using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using GraphForms;

namespace ElasticNodes
{
    public partial class MainForm : Form
    {
        
        private NodeScene scene;
        private Node centerNode;

        public MainForm()
        {
            InitializeComponent();
            this.scene = new NodeScene();
            this.scene.BoundingBox = new RectangleF(-200, -200, 400, 400);
            this.scene.Position = new PointF(200, 200);
            this.scene.AddView(this.graphPanel);
            //this.graphPanel.Scale(0.8f, 0.8f);

            Node node1 = new Node(this.scene);
            Node node2 = new Node(this.scene);
            Node node3 = new Node(this.scene);
            Node node4 = new Node(this.scene);
            centerNode = new Node(this.scene);
            Node node6 = new Node(this.scene);
            Node node7 = new Node(this.scene);
            Node node8 = new Node(this.scene);
            Node node9 = new Node(this.scene);
            scene.AddItem(node1);
            scene.AddItem(node2);
            scene.AddItem(node3);
            scene.AddItem(node4);
            scene.AddItem(centerNode);
            scene.AddItem(node6);
            scene.AddItem(node7);
            scene.AddItem(node8);
            scene.AddItem(node9);

            scene.AddItem(new Edge(node1, node2));
            scene.AddItem(new Edge(node2, node3));
            scene.AddItem(new Edge(node2, centerNode));
            scene.AddItem(new Edge(node3, node6));
            scene.AddItem(new Edge(node4, node1));
            scene.AddItem(new Edge(node4, centerNode));
            scene.AddItem(new Edge(centerNode, node6));
            scene.AddItem(new Edge(centerNode, node8));
            scene.AddItem(new Edge(node6, node9));
            scene.AddItem(new Edge(node7, node4));
            scene.AddItem(new Edge(node8, node7));
            scene.AddItem(new Edge(node9, node8));/* */

            /*scene.AddItem(new Edge2(node1, node2));
            scene.AddItem(new Edge2(node2, node3));
            scene.AddItem(new Edge2(node2, centerNode));
            scene.AddItem(new Edge2(node3, node6));
            scene.AddItem(new Edge2(node4, node1));
            scene.AddItem(new Edge2(node4, centerNode));
            scene.AddItem(new Edge2(centerNode, node6));
            scene.AddItem(new Edge2(centerNode, node8));
            scene.AddItem(new Edge2(node6, node9));
            scene.AddItem(new Edge2(node7, node4));
            scene.AddItem(new Edge2(node8, node7));
            scene.AddItem(new Edge2(node9, node8));/* */

            node1.Position = new PointF(-50, -50);
            node2.Position = new PointF(0, -50);
            node3.Position = new PointF(50, -50);
            node4.Position = new PointF(-50, 0);
            centerNode.Position = new PointF(0, 0);
            node6.Position = new PointF(50, 0);
            node7.Position = new PointF(-50, 50);
            node8.Position = new PointF(0, 50);
            node9.Position = new PointF(50, 50);
        }

        protected void ScaleView(float scaleFactor)
        {
            //float factor = GraphHelpers.MapRect(this.graphPanel.Transform,
            //    new RectangleF(0, 0, 1, 1)).Width;
            //if (0.07 <= factor && factor <= 100)
            //    this.graphPanel.Scale(scaleFactor, scaleFactor);
        }

        public void ZoomIn()
        {
            this.ScaleView(1.2f);
        }

        public void ZoomOut()
        {
            this.ScaleView(1 / 1.2f);
        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                    this.centerNode.MoveBy(0, -20);
                    break;
                case Keys.Down:
                    this.centerNode.MoveBy(0, 20);
                    break;
                case Keys.Left:
                    this.centerNode.MoveBy(-20, 0);
                    break;
                case Keys.Right:
                    this.centerNode.MoveBy(20, 0);
                    break;
                case Keys.PageDown:
                    //this.ZoomIn();
                    break;
                case Keys.PageUp:
                    //this.ZoomOut();
                    break;
                case Keys.Space:
                case Keys.Enter:
                    this.scene.Shuffle();
                    break;
            }
        }
    }
}
