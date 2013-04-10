using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms
{
    /// <summary>
    /// Simple thread-safe implementation of the <see cref="IAlgorithm"/> 
    /// interface, with handlers for computation state changes.
    /// </summary>
    public abstract class AAlgorithm : IAlgorithm
    {
        private volatile object mSyncRoot = new object();
        private volatile ComputeState mState = ComputeState.None;

        /// <summary>
        /// Object used to lock the algorithm for thread safety.
        /// </summary>
        public object SyncRoot
        {
            get { return this.mSyncRoot; }
        }

        /// <summary>
        /// The current state of the computation of the algorithm.
        /// </summary>
        public ComputeState State
        {
            get { return this.mState; }
        }

        /// <summary>
        /// Reimplement this function to trigger events and other reactions
        /// to any change the <see cref="State"/> of this 
        /// <see cref="AAlgorithm"/> instance which occur after more
        /// specific handlers have been called.</summary>
        /// <param name="oldState">The previous <see cref="State"/> of this
        /// <see cref="AAlgorithm"/> instance before the state change
        /// occurred.</param>
        protected virtual void OnStateChanged(ComputeState oldState) { }

        /// <summary>
        /// Reimplement this function to trigger events and other actions
        /// which occur before <see cref="InternalCompute()"/> is called
        /// and this <see cref="AAlgorithm"/> instance's computation begins.
        /// </summary>
        protected virtual void OnStarted() { }

        /// <summary>
        /// Reimplement this function to trigger events and other actions
        /// which occur after <see cref="InternalCompute()"/> is called
        /// and this <see cref="AAlgorithm"/> instance's computation 
        /// successfully ends.</summary>
        protected virtual void OnFinished() { }

        /// <summary>
        /// Reimplement this function to trigger events and other actions
        /// which occur after <see cref="InternalCompute()"/> is called
        /// and this <see cref="AAlgorithm"/> instance's computation
        /// has been aborted before completion.</summary>
        protected virtual void OnAborted() { }

        private void BeginComputation()
        {
            lock (this.mSyncRoot)
            {
                if (this.mState != ComputeState.None)
                    throw new InvalidOperationException();

                this.mState = ComputeState.Running;
                this.OnStarted();
                this.OnStateChanged(ComputeState.None);
            }
        }

        /// <summary>
        /// This implementation of this function should contain the core code 
        /// that this <see cref="AAlgorithm"/> instance executes during its
        /// computation. This code should be designed to stop as soon as 
        /// possible whenever an abortion is pending.
        /// </summary>
        protected abstract void InternalCompute();

        private void EndComputation()
        {
            lock (this.mSyncRoot)
            {
                ComputeState oldState = this.mState;
                switch (this.mState)
                {
                    case ComputeState.Running:
                        this.mState = ComputeState.Finished;
                        this.OnFinished();
                        break;
                    case ComputeState.Aborting:
                        this.mState = ComputeState.Aborted;
                        this.OnAborted();
                        break;
                    default:
                        throw new InvalidOperationException();
                }
                this.OnStateChanged(oldState);
            }
        }

        /// <summary>
        /// Run the algorithm and start its computation.
        /// </summary>
        public void Compute()
        {
            this.BeginComputation();
            this.InternalCompute();
            this.EndComputation();
        }

        /// <summary>
        /// Stop the running algorithm and abort its computation.
        /// </summary>
        public virtual void Abort()
        {
            lock (this.mSyncRoot)
            {
                if (this.mState == ComputeState.Running)
                {
                    this.mState = ComputeState.Aborting;
                    this.OnStateChanged(ComputeState.Running);
                }
            }
        }
    }
}
