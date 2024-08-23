namespace Scaleout.Streaming.DigitalTwin.Core
{
    /// <summary>
    /// Context object that provides operations that are available
	/// when a new simulation starts
    /// </summary>
    public abstract class InitSimulationContext
    {
        /// <summary>
        /// Gets an <see cref="ISharedData"/> instance for accessing shared objects
        /// that are associated with the model being processed.
        /// </summary>
        public abstract ISharedData SharedModelData { get; }

        /// <summary>
        /// Gets an <see cref="ISharedData"/> instance for accessing objects
        /// that are shared globally between all models.
        /// </summary>
        public abstract ISharedData SharedGlobalData { get; }
    }
}
