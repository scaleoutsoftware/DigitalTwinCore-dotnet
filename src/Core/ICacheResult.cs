using System;
using System.Collections.Generic;
using System.Text;

namespace Scaleout.Streaming.DigitalTwin.Core
{

    /// <summary>
    /// Represents a response from an <see cref="ISharedData"/> operation.
    /// </summary>
    public interface ICacheResult
    {
        /// <summary>
        /// Gets the key to the object associated with the result.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Get the object returned from a Get operation.
        /// </summary>
        byte[] Value { get; }

        /// <summary>
        /// Gets the outcome of the cache operation.
        /// </summary>
        CacheOperationStatus Status { get; }
    }
}
