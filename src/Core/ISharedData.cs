using System;
using System.Collections.Generic;
using System.Text;

namespace Scaleout.Streaming.DigitalTwin.Core
{
    /// <summary>
    /// Provides access to objects that are shared between model instances.
    /// </summary>
    public interface ISharedData
    {
        /// <summary>
        /// Retrieves an existing object from the cache.
        /// </summary>
        /// <param name="key">Identifier of the object in the cache.</param>
        /// <returns><see cref="ICacheResult"/> containing the outcome of the operation and the retrieved object (if successful).</returns>
        ICacheResult Get(string key);

        /// <summary>
        /// Adds or updates an object in the cache.
        /// </summary>
        /// <param name="key">Identifier of the object in the cache.</param>
        /// <param name="value">Value to be stored in the cache.</param>
        /// <returns><see cref="ICacheResult"/> containing the outcome of the operation.</returns>
        ICacheResult Put(string key, byte[] value);

        /// <summary>
        /// Removes an object from the cache.
        /// </summary>
        /// <param name="key">Identifier of the object in the cache.</param>
        /// <returns><see cref="ICacheResult"/> containing the outcome of the operation.</returns>
        ICacheResult Remove(string key);

        /// <summary>
        /// Clears all objects from the cache.
        /// </summary>
        /// <returns><see cref="ICacheResult"/> containing the outcome of the operation.</returns>
        ICacheResult Clear();
    }
}
