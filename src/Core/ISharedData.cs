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
        /// <remarks>
        /// <para>
        /// The <see cref="ICacheResult.Status"/> property of the returned result 
        /// will contain one of the following <see cref="CacheOperationStatus"/> outcomes:
        /// </para>
        /// <list type="table">
        ///   <listheader>
        ///     <term>Status</term>
        ///     <description>Description</description>
        ///   </listheader>
        ///   <item>
        ///     <term>ObjectRetrieved</term>
        ///     <description>The object was successfully retrieved.</description>
        ///   </item>
        ///   <item>
        ///     <term>ObjectDoesNotExist</term>
        ///     <description>
        ///     The requested object was not found.
        ///     </description>
        ///   </item>
        /// </list>
        /// </remarks>
        ICacheResult Get(string key);

        /// <summary>
        /// Adds or updates an object in the cache.
        /// </summary>
        /// <param name="key">Identifier of the object in the cache.</param>
        /// <param name="value">Value to be stored in the cache.</param>
        /// <returns><see cref="ICacheResult"/> containing the outcome of the operation.</returns>
        /// <remarks>
        /// <para>
        /// The <see cref="ICacheResult.Status"/> property of the returned result 
        /// will contain the following <see cref="CacheOperationStatus"/>:
        /// </para>
        /// <list type="table">
        ///   <listheader>
        ///     <term>Status</term>
        ///     <description>Description</description>
        ///   </listheader>
        ///   <item>
        ///     <term>ObjectPut</term>
        ///     <description>The object was successfully put into the shared data repository.</description>
        ///   </item>
        /// </list>
        /// </remarks>
        ICacheResult Put(string key, byte[] value);

        /// <summary>
        /// Removes an object from the cache.
        /// </summary>
        /// <param name="key">Identifier of the object in the cache.</param>
        /// <returns><see cref="ICacheResult"/> containing the outcome of the operation.</returns>
        /// <remarks>
        /// <para>
        /// The <see cref="ICacheResult.Status"/> property of the returned result 
        /// will contain the following <see cref="CacheOperationStatus"/>:
        /// </para>
        /// <list type="table">
        ///   <listheader>
        ///     <term>Status</term>
        ///     <description>Description</description>
        ///   </listheader>
        ///   <item>
        ///     <term>ObjectRemoved</term>
        ///     <description>The object was successfully removed from the shared data repository.</description>
        ///   </item>
        /// </list>
        /// </remarks>
        ICacheResult Remove(string key);

        /// <summary>
        /// Clears all objects from the cache.
        /// </summary>
        /// <returns><see cref="ICacheResult"/> containing the outcome of the operation.</returns>
        /// <remarks>
        /// <para>
        /// The <see cref="ICacheResult.Status"/> property of the returned result 
        /// will contain the following <see cref="CacheOperationStatus"/>:
        /// </para>
        /// <list type="table">
        ///   <listheader>
        ///     <term>Status</term>
        ///     <description>Description</description>
        ///   </listheader>
        ///   <item>
        ///     <term>CacheCleared</term>
        ///     <description>The shared data repository was successfully cleared.</description>
        ///   </item>
        /// </list>
        /// </remarks>
        ICacheResult Clear();
    }
}
