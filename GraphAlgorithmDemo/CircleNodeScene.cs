using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GraphForms;
using GraphForms.Algorithms;
using GraphForms.Algorithms.Layout;
using GraphForms.Algorithms.Layout.ForceDirected;

namespace GraphAlgorithmDemo
{
    public class CircleNodeScene : GraphScene, IClusterNode
    {
        private Digraph<CircleNode, ArrowEdge> mGraph;
        private Timer mLayoutTimer;

        //private IForceDirectedLayoutAlgorithm mLayout;
        private LayoutAlgorithm<CircleNode, ArrowEdge> mLayout;

        public CircleNodeScene()
        {
            this.mGraph = new Digraph<CircleNode, ArrowEdge>();

            this.mLayoutTimer = new Timer();
            this.mLayoutTimer.Interval = 1000 / 25;
            this.mLayoutTimer.Tick += new EventHandler(OnLayoutTimerTick);

            this.mLayout = new ElasticLayoutForCircles(this, this.BoundingBox);

            this.mMouseUpHistory = new int[3];
            for (int i = 0; i < this.mMouseUpHistory.Length; i++)
            {
                this.mMouseUpHistory[i] = -1;
            }
        }

        public void ClearGraph()
        {
            int i;
            Digraph<CircleNode, ArrowEdge>.GEdge[] edges
                = this.mGraph.InternalEdges;
            for (i = 0; i < edges.Length; i++)
            {
                this.RemoveItem(edges[i].Data);
            }
            this.mGraph.ClearEdges();
            Digraph<CircleNode, ArrowEdge>.GNode[] nodes
                = this.mGraph.InternalNodes;
            for (i = 0; i < nodes.Length; i++)
            {
                this.RemoveItem(nodes[i].Data);
            }
            this.mGraph.ClearNodes();

            for (i = 0; i < this.mMouseUpHistory.Length; i++)
            {
                this.mMouseUpHistory[i] = -1;
            }
        }

        public void UpdateEdges()
        {
            Digraph<CircleNode, ArrowEdge>.GEdge[] edges
                = this.mGraph.InternalEdges;
            for (int i = 0; i < edges.Length; i++)
            {
                edges[i].Data.Update();
            }
        }

        /// <summary>
        /// The graph that contains the nodes that are arranged by the
        /// <see cref="Layout"/> algorithm.
        /// </summary>
        public Digraph<CircleNode, ArrowEdge> Graph
        {
            get { return this.mGraph; }
        }

        /// <summary>
        /// The force-directed layout algorithm used to arrange the nodes 
        /// in the <see cref="Graph"/>. Setting this to a different value
        /// aborts the current algorithm.
        /// </summary>
        public LayoutAlgorithm<CircleNode, ArrowEdge> Layout
        //public IForceDirectedLayoutAlgorithm Layout
        {
            get { return this.mLayout; }
            set
            {
                if (this.mLayout != value && value != null)
                {
                    this.mLayout.Abort();
                    this.mLayout = value;
                    this.UpdateBounds();
                }
            }
        }

        /// <summary>
        /// Restarts the <see cref="Layout"/> algorithm if it is not already
        /// running.
        /// </summary>
        public void StartLayout()
        {
            if (!this.mLayoutTimer.Enabled)
                this.mLayoutTimer.Start();
        }

        /// <summary>
        /// Whether the <see cref="Layout"/> algorithm is currently running
        /// and arranging the nodes in the <see cref="Graph"/>.
        /// Setting this to false is the same as 
        /// </summary>
        public bool IsLayoutRunning
        {
            get { return this.mLayoutTimer.Enabled; }
            set
            {
                if (value)
                {
                    this.StartLayout();
                }
                else
                {
                    this.mLayout.Abort();
                }
            }
        }

        /// <summary>
        /// Whether moving a node in the <see cref="Graph"/> restarts the 
        /// <see cref="Layout"/> algorithm, and resets it if it is not paused.
        /// </summary>
        public bool LayoutOnNodeMoved = false;
        /// <summary>
        /// If true, the <see cref="Layout"/> algorithm will not iterate and 
        /// will not be reset by moving a node in the <see cref="Graph"/>.
        /// </summary>
        public bool LayoutPaused = false;

        private int[] mMouseUpHistory;

        public int[] MouseUpHistory
        {
            get { return this.mMouseUpHistory; }
        }

        public void OnNodeMouseUp(CircleNode node)
        {
            //this.mMouseUpHistory[2] = this.mMouseUpHistory[1];
            //this.mMouseUpHistory[1] = this.mMouseUpHistory[0];
            Array.Copy(this.mMouseUpHistory, 0, this.mMouseUpHistory, 1, 
                this.mMouseUpHistory.Length - 1);
            this.mMouseUpHistory[0] = this.mGraph.IndexOfNode(node);
            if (this.NodeMouseUp != null)
                this.NodeMouseUp(node);
        }

        public event Action<CircleNode> NodeMouseUp;

        public void OnNodeMovedByMouse(CircleNode node)
        {
            if (this.LayoutOnNodeMoved)
            {
                this.StartLayout();
                if (!this.LayoutPaused)
                    this.mLayout.ResetAlgorithm();
            }
            if (this.NodeMoved != null)
                this.NodeMoved(node);
        }

        public event Action<CircleNode> NodeMoved;

        private float mMinX;
        private float mMinY;
        private float mCenX;
        private float mCenY;
        private float mMaxX;
        private float mMaxY;

        public void UpdateBounds()
        {
            const float padding = 10;
            RectangleF bbox = this.SceneBoundingBox;
            //int x = (int)Math.Floor(bbox.X) + padding;
            //int y = (int)Math.Floor(bbox.Y) + padding;
            //int w = (int)Math.Ceiling(bbox.X + bbox.Width) - x - padding;
            //int h = (int)Math.Ceiling(bbox.Y + bbox.Height) - y - padding;
            //this.mLayout.Parameters.BoundingBox
            //    = new RectangleF(bbox.X + padding, bbox.Y + padding,
            //        bbox.Width - 2 * padding, bbox.Height - 2 * padding);
            this.mMinX = bbox.X + padding;
            this.mMinY = bbox.Y + padding;
            this.mMaxX = this.mMinX + bbox.Width - 2 * padding;
            this.mMaxY = this.mMinY + bbox.Height - 2 * padding;
            this.mCenX = this.mMaxX / 2;
            this.mCenY = this.mMaxY / 2;
        }/* */

        //private bool bTimerTicked = false;

        private void OnLayoutTimerTick(object sender, EventArgs e)
        {
            if (!this.LayoutPaused && !this.mLayout.AsyncIterate(true))
            {
                this.mLayoutTimer.Stop();

                if (this.LayoutStopped != null)
                    this.LayoutStopped(this, e);
            }

            /*this.bTimerTicked = true;
            switch (this.mLayout.State)
            {
                case ComputeState.Finished:
                case ComputeState.Aborted:
                    this.mTimer.Stop();
            }/* */

            if (this.LayoutTimerTick != null)
                this.LayoutTimerTick(this, e);
        }

        public event EventHandler LayoutStopped;

        public event EventHandler LayoutTimerTick;

        public class LayoutIterEndedEventArgs : EventArgs
        {
            private uint mIteration;
            //private double mStatusInPercent;
            private double mDistChange;
            //private double mMaxDistChange;
            //private string mMessage;

            public uint Iteration
            {
                get { return this.mIteration; }
            }

            /*public double StatusInPercent
            {
                get { return this.mStatusInPercent; }
            }/* */

            public double DistanceChange
            {
                get { return this.mDistChange; }
            }

            /*public double MaxDistanceChange
            {
                get { return this.mMaxDistChange; }
            }

            public string Message
            {
                get { return this.mMessage; }
            }/* */

            public LayoutIterEndedEventArgs(uint iteration, double distChange)//statusInPercent,
                //double distanceChange, double maxDistanceChange, string message)
            {
                this.mIteration = iteration;
                //this.mStatusInPercent = statusInPercent;
                this.mDistChange = distChange;//distanceChange;
                //this.mMaxDistChange = maxDistanceChange;
                //this.mMessage = message;
            }
        }

        public bool OnLayoutIterEnded(uint iteration, double distChange)//statusInPercent,
            //double distanceChange, double maxDistanceChange, string message)
        {
            // Wait for timer to tick before ending layout iteration
            /*while (!this.bTimerTicked)
            {
                System.Threading.Thread.Sleep(1000 / 125);
            }
            this.bTimerTicked = false;/* */

            if (this.LayoutIterEnded != null)
            {
                LayoutIterEndedEventArgs e = new LayoutIterEndedEventArgs(
                    iteration, distChange);//statusInPercent, distanceChange,
                    //maxDistanceChange, message);
                this.LayoutIterEnded(this, e);
            }

            return true;
        }

        public event EventHandler<LayoutIterEndedEventArgs> LayoutIterEnded;

        private float mLastMouseX, mLastMouseY;

        protected override void OnMouseDown(GraphMouseEventArgs e)
        {
            this.mLastMouseX = e.SceneX;
            this.mLastMouseY = e.SceneY;
            e.Handled = true;
        }

        protected override void OnMouseMove(GraphMouseEventArgs e)
        {
            CircleNode[] nodes = this.mGraph.Nodes;
            CircleNode node;
            PointF pos;
            for (int i = 0; i < nodes.Length; i++)
            {
                node = nodes[i];
                if (node.MouseGrabbed)
                {
                    pos = node.Position;
                    node.Position = new PointF(
                        pos.X + e.SceneX - this.mLastMouseX,
                        pos.Y + e.SceneY - this.mLastMouseY);
                }
            }
            this.mLastMouseX = e.SceneX;
            this.mLastMouseY = e.SceneY;
            e.Handled = true;
        }

        protected override void OnMouseUp(GraphMouseEventArgs e)
        {
            CircleNode[] nodes = this.mGraph.Nodes;
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i].MouseGrabbed = false;
            }
            e.Handled = true;
        }

        #region IClusterNode Members

        public PointF GetPortNodePos(double angle)
        {
            double dx = Math.Cos(angle);
            double dy = Math.Sin(angle);
            double t = Math.Min(
                Math.Abs(this.mCenX / dx), Math.Abs(this.mCenY / dy));
            dx = dx * t + this.mCenX;
            dy = dy * t + this.mCenY;
            return new PointF((float)dx, (float)dy);
        }

        public PointF AugmentNodePos(float x, float y)
        {
            float dx = Math.Min(Math.Max(x, this.mMinX), this.mMaxX);
            float dy = Math.Min(Math.Max(y, this.mMinY), this.mMaxY);
            return new PointF(dx, dy);
        }

        #endregion

        #region ILayoutNode Members

        private float mNewX;
        private float mNewY;

        public bool PositionFixed
        {
            get { return true; }
        }

        public float NewX
        {
            get { return this.mNewX; }
        }

        public float NewY
        {
            get { return this.mNewY; }
        }

        public void SetNewPosition(float newX, float newY)
        {
            this.mNewX = newX;
            this.mNewY = newY;
        }

        #endregion
    }
}
