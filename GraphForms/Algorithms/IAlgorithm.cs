using System;
using System.Collections.Generic;
using System.Text;

namespace GraphForms.Algorithms
{
    /// <summary>
    /// Simple algorithm interface which is not connected to any graph.
    /// </summary>
    public interface IAlgorithm
    {
        /// <summary>
        /// Object used to lock the algorithm for thread safety.
        /// </summary>
        object SyncRoot { get; }
        /// <summary>
        /// The current state of the computation of the algorithm.
        /// </summary>
        ComputeState State { get; }
        /// <summary>
        /// Run the algorithm and start its computation.
        /// </summary>
        void Compute();
        /// <summary>
        /// Stop the running algorithm and abort its computation.
        /// </summary>
        void Abort();
    }
}
