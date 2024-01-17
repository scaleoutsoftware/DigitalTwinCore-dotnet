namespace Scaleout.Streaming.DigitalTwin.Core
{
    /// <summary>
    /// An enumeration that indicates the outcome of a cache operation.
    /// </summary>
    public enum CacheOperationStatus
    {
        /// <summary>
        /// The object was successfully retrieved.
        /// </summary>
        ObjectRetrieved,

        /// <summary>
        /// The object was successfully added/updated.
        /// </summary>
        ObjectPut,

        /// <summary>
        /// The object could not be retrieved because it was not found.
        /// </summary>
        ObjectDoesNotExist,

        /// <summary>
        /// The object was removed successfully.
        /// </summary>
        ObjectRemoved,

        /// <summary>
        /// The cache was cleared successfully.
        /// </summary>
        CacheCleared


    }
}
