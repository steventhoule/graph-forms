using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms.Layout.ForceDirected
{
    /// <summary>
    /// A simple base class for algorithms which calculate the layout of a given
    /// <see cref="T:DirectionalGraph`2{Node,Edge}"/> instance by using physics
    /// calculations to set the positions of its <typeparamref name="Node"/> 
    /// instances, based on balancing forces carried along the 
    /// <typeparamref name="Edge"/> instances connecting them and influenced by 
    /// the parameter values within the <typeparamref name="Params"/> instance.
    /// </summary>
    /// <typeparam name="Node">The class of <see cref="GraphElement"/> nodes
    /// which are rearranged by this force-directed layout algorithm.</typeparam>
    /// <typeparam name="Edge">The class of <see cref="T:IGraphEdge`1{Node}"/>
    /// edges that connect and transmit forces between the 
    /// <typeparamref name="Node"/> instances that this force-directed layout 
    /// algorithm rearranges.</typeparam>
    /// <typeparam name="Params">The <see cref="ForceDirectedLayoutParameters"/> 
    /// class with properties that influence how this force-directed layout 
    /// algorithm arranges its graph's <typeparamref name="Node"/> instances.
    /// </typeparam>
    /// <remarks>
    /// Force-Directed Layout Algorithms tend to be much more dynamic than
    /// other varieties of layout algorithms, which makes them ideal for
    /// user interactive graphs which can give real-time feedback.
    /// For example, whenever the user moves one of the 
    /// <typeparamref name="Node"/> instances out of place, a force-directed
    /// layout algorithm can be used to pull neighboring nodes along with it,
    /// and then let those nodes ease back into place once the user releases
    /// it from being dragged by their mouse.</remarks>
    public abstract class ForceDirectedLayoutAlgorithm<Node, Edge, Params>
        : LayoutAlgorithm<Node, Edge, Params>, IForceDirectedLayoutAlgorithm
        where Node : GraphElement, ILayoutNode
        where Edge : class, IGraphEdge<Node>, IUpdateable
        where Params : ForceDirectedLayoutParameters
    {
        /// <summary>
        /// Whether the algorithm should reset back to its starting point
        /// and re-initialize before beginning the next iteration.
        /// </summary>
        private bool bResetting = true;
        /// <summary>
        /// Whether any nodes changed position during the last iteration.
        /// If nodes aren't moving, the algorithm has obviously reached an
        /// equilibrium and finished.
        /// </summary>
        private bool bItemMoved = true;

        private int mMaxIterations;

        /// <summary>Initializes a new 
        /// <see cref="T:ForceDirectedLayoutAlgorithm`3{Node,Edge,Params}"/>
        /// instance that arranges the <typeparamref name="Node"/> instances
        /// within the given <paramref name="graph"/>, influenced by its
        /// <see cref="P:LayoutAlgorithm`3.DefaultParameters"/>.</summary>
        /// <param name="graph">The graph that will be rearranged by this
        /// force-directed layout algorithm.</param>
        public ForceDirectedLayoutAlgorithm(DirectionalGraph<Node, Edge> graph)
            : base(graph, null)
        {
        }

        /// <summary>Initializes a new 
        /// <see cref="T:ForceDirectedLayoutAlgorithm`3{Node,Edge,Params}"/>
        /// instance that arranges the <typeparamref name="Node"/> instances
        /// within the given <paramref name="graph"/>, influenced by parameter
        /// values within the given <paramref name="oldParameters"/>.
        /// </summary>
        /// <param name="graph">The graph that will be rearranged by this
        /// force-directed layout algorithm.</param>
        /// <param name="oldParameters">The parameters that will influence how
        /// this layout algorithm rearranges the <paramref name="graph"/>.
        /// If null, the <see cref="P:LayoutAlgorithm`3.DefaultParameters"/> 
        /// are used instead.</param>
        public ForceDirectedLayoutAlgorithm(DirectionalGraph<Node, Edge> graph,
            Params oldParameters)
            : base(graph, oldParameters)
        {
        }

        /// <summary>
        /// The <see cref="ForceDirectedLayoutParameters"/> instance used to 
        /// influence the behavior of this force-directed layout algorithm 
        /// and how it arranges the nodes in its graph.
        /// </summary>
        ForceDirectedLayoutParameters IForceDirectedLayoutAlgorithm.Parameters
        {
            get { return this.Parameters as ForceDirectedLayoutParameters; }
        }

        /// <summary>
        /// Resets this algorithm back to its starting point, which causes it
        /// to re-initialize before beginning its next iteration.
        /// </summary><remarks>
        /// This function is useful for when the positions of one or more
        /// nodes are changed by external code (such as a user dragging them
        /// with their mouse) before this algorithm finishes or aborts its
        /// layout computation.</remarks>
        public void ResetAlgorithm()
        {
            this.bResetting = true;
        }

        /// <summary>
        /// Reimplement this function to initialize any data needed before
        /// the algorithm begins its layout computation. This function is
        /// called by <see cref="OnStarted()"/> and whenever the algorithm
        /// is reset using <see cref="ResetAlgorithm()"/>.
        /// </summary>
        protected virtual void InitializeAlgorithm()
        {
        }

        /// <summary>
        /// Initializes the underlying algorithm by calling
        /// <see cref="InitializeAlgorithm()"/> before performing the
        /// <c>OnStarted()</c> code of the base class.
        /// </summary><seealso 
        /// cref="M:LayoutAlgorithm`3{Node,Edge,Params}.OnStarted()"/>
        protected override void OnStarted()
        {
            this.mMaxIterations = this.Parameters.MaxIterations;
            this.bResetting = true;
            this.bItemMoved = true;
            this.mIter = 0;
            this.InitializeAlgorithm();
            base.OnStarted();
        }

        /*/// <summary>
        /// Initializes the internal iteration limit from the 
        /// <see cref="ForceDirectedLayoutParameters.MaxIterations"/> of the 
        /// <see cref="P:LayoutAlgorithm`3.Parameters"/>, and also performs 
        /// parameter initialization of its base classes.
        /// </summary><seealso 
        /// cref="M:LayoutAlgorithm`3{Node,Edge,Params}.InitParameters()"/>
        protected override void InitParameters()
        {
            base.InitParameters();
            this.mMaxIterations = this.Parameters.MaxIterations;
        }/* */

        protected override bool OnBeginIteration(bool paramsDirty, int lastNodeCount, int lastEdgeCount)
        {
            if (paramsDirty)
            {
                this.mMaxIterations = this.Parameters.MaxIterations;
            }
            return base.OnBeginIteration(paramsDirty, lastNodeCount, lastEdgeCount);
        }

        /// <summary>
        /// Whether the algorithm should perform the next iteration in its
        /// layout computation, or stop and finish.
        /// </summary>
        /// <returns>true if the algorithm should run its next iteration,
        /// or false if the algorithm should stop and finish.</returns>
        protected virtual bool CanIterate()
        {
            return true;
        }

        /// <summary>
        /// This function should contain the core code which is responsible for
        /// calculating the new positions of every <typeparamref name="Node"/>
        /// in the <see cref="P:LayoutAlgorithm`3.Graph"/> on each iteration.
        /// </summary>
        /// <param name="iteration">The total number of iterations completed by
        /// this algorithm (not counting the one calling this function).</param>
        /// <param name="maxIterations">The maximum number of iterations that
        /// can be completed before the algorithm finishes, as determined by
        /// the algorithm's <see cref="P:LayoutAlgorithm`3.Parameters"/>.</param>
        protected abstract void PerformIteration(int iteration, int maxIterations);

        /// <summary>
        /// Iterates the code at the core of this force-directed layout algorithm,
        /// calling <see cref="M:LayoutAlgorithm`3.BeginIteration()"/>, 
        /// <see cref="PerformIteration(int,int)"/>, and 
        /// <see cref="M:LayoutAlgorithm`3.EndIteration(int,double,string)"/>
        /// repeatedly until one or more of the conditions for completion are
        /// met, or until this algorithm is aborted.
        /// </summary>
        protected override void InternalCompute()
        {
            for (int i = 0; i < this.mMaxIterations &&
                this.State != ComputeState.Aborting &&
                this.bItemMoved && this.CanIterate(); i++)
            {
                if (this.bResetting)
                {
                    this.InitializeAlgorithm();
                    i = 0;
                    this.bResetting = false;
                }
                this.BeginIteration();
                if (this.mGraph.NodeCount > 0 && this.mGraph.EdgeCount > 0)
                {
                    this.PerformIteration(i, this.mMaxIterations);
                }
                this.EndIteration(i, i / (double)this.mMaxIterations, null);
            }
        }

        private ComputeState mAsyncState = ComputeState.None;
        private int mIter;

        /// <summary>
        /// Runs a single iteration of this force-directed layout algorithm
        /// asynchronously, keeping track of a separate asynchronous compute
        /// state to trigger implementations of the <see cref="OnStarted()"/>,
        /// <see cref="AAlgorithm.OnAborted()"/>, and 
        /// <see cref="AAlgorithm.OnFinished()"/> functions.
        /// </summary>
        /// <param name="forceRestart">Whether to force this algorithm to
        /// start running again if it has finished or has been aborted.</param>
        /// <returns>true if the iteration is successfully run, or false if
        /// the algorithm has finished or has been aborted.</returns>
        public bool AsyncIterate(bool forceRestart)
        {
            switch (this.State)
            {
                case ComputeState.Running:
                case ComputeState.Aborting:
                    // Can't run asynchronously and synchronously 
                    // simultaneously
                    throw new InvalidOperationException();
            }
            switch (this.mAsyncState)
            {
                case ComputeState.None:
                    this.mAsyncState = ComputeState.Running;
                    this.OnStarted();
                    break;
                case ComputeState.Finished:
                case ComputeState.Aborted:
                    if (forceRestart)
                    {
                        this.mAsyncState = ComputeState.Running;
                        this.OnStarted();
                    }
                    break;
            }
            if (this.mIter < this.mMaxIterations &&
                this.mAsyncState == ComputeState.Running &&
                this.bItemMoved && this.CanIterate())
            {
                if (this.bResetting)
                {
                    this.InitializeAlgorithm();
                    this.mIter = 0;
                    this.bResetting = false;
                }
                this.BeginIteration();
                if (this.mGraph.NodeCount > 0 && this.mGraph.EdgeCount > 0)
                {
                    this.PerformIteration(this.mIter, this.mMaxIterations);
                }
                this.EndIteration(this.mIter,
                    this.mIter / (double)this.mMaxIterations, null);
                this.mIter++;
                return true;
            }
            else
            {
                switch (this.mAsyncState)
                {
                    case ComputeState.Running:
                        this.mAsyncState = ComputeState.Finished;
                        this.OnFinished();
                        break;
                    case ComputeState.Aborting:
                        this.mAsyncState = ComputeState.Aborted;
                        this.OnAborted();
                        break;
                }
                return false;
            }
        }

        /// <summary>
        /// Stop running the algorithm and abort its computation,
        /// both synchronously and asychronously.
        /// </summary>
        public override void Abort()
        {
            if (this.mAsyncState == ComputeState.Running)
            {
                this.mAsyncState = ComputeState.Aborting;
            }
            base.Abort();
        }

        /// <summary><para>
        /// Reimplement this function to trigger events and other actions
        /// that occur after an iteration has ended and all the nodes and
        /// edges in the graph have been updated with their new positions.
        /// </para><para>
        /// If this function returns false, the entire algorithm is aborted
        /// as soon as possible.</para></summary>
        /// <param name="iteration">The total number of iterations that have
        /// occurred since this algorithm started its computation.</param>
        /// <param name="statusInPercent">The status of this algorithm in
        /// percent, which is usually <paramref name="iteration"/> divided by
        /// the maximum number of iterations specified in the parameters.</param>
        /// <param name="distanceChange">The total change in the positions
        /// of every <typeparamref name="Node"/> instance since the last 
        /// iteration; sum of squared distances between every old and new 
        /// position.</param>
        /// <param name="maxDistanceChange">The maximum possible total change
        /// in the positions of every <typeparamref name="Node"/> instance;
        /// squared length of bounding box diagonal times number of nodes.
        /// </param>
        /// <param name="message">The textual representation of the status of
        /// this algorithm, which usually describes its current computation 
        /// step or phase or iteration.</param>
        /// <returns>true to continue the computation after the iteration has 
        /// ended, or false to abort the entire algorithm as soon as possible.
        /// </returns>
        protected override bool OnIterationEnded(int iteration, double statusInPercent,
            double distanceChange, double maxDistanceChange, string message)
        {
            this.bItemMoved = distanceChange > 0;
            return base.OnIterationEnded(iteration, statusInPercent,
                distanceChange, maxDistanceChange, message);
        }
    }
}
