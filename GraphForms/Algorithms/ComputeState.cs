

namespace GraphForms.Algorithms
{
    /// <summary>
    /// Describes the state of the computation of an algorithm, primarily
    /// <see cref="IAlgorithm"/> and classes that implement it.
    /// </summary>
    public enum ComputeState
    {
        /// <summary>
        /// Algorithm has not yet started its computation.
        /// </summary>
        None,
        /// <summary>
        /// Algorithm has been started and is currently computing.
        /// </summary>
        Running,
        /// <summary>
        /// Algorithm is about to abort its computation.
        /// </summary>
        Aborting,
        /// <summary>
        /// Algorithm has successfully finished its computation.
        /// </summary>
        Finished,
        /// <summary>
        /// Algorithm was aborted before finishing its computation.
        /// </summary>
        Aborted
    }
}
