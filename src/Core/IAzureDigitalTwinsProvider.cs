#region Copyright notice and license

// Copyright 2023-2026 ScaleOut Software, Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Scaleout.Modules.DigitalTwin.Abstractions.Exceptions;

namespace Scaleout.Modules.Abstractions
{
    /// <summary>
    /// Defines the methods available to ScaleOut Digital Twins for interacting with
    /// corresponding Azure Digital Twins (ADT) instances.
    /// </summary>
    public interface IAzureDigitalTwinsProvider
    {
        /// <summary>Indicates whether the ADT provider is active and can be used.</summary>
        bool IsActive { get; }

        /// <summary>
        /// Returns a list of digital twin instance identifiers associated with the specified <paramref name="modelName"/>. 
        /// </summary>
        /// <param name="modelName">The name of the digital twin model.</param>
        /// <returns>
        /// The list of digital twin instance identifiers that are associated with the specified <paramref name="modelName"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="modelName"/> is empty or null.</exception>
        /// <exception cref="PersistenceProviderException">Failed to obtain the list of object instance identifiers
        /// for the specified <paramref name="modelName"/>.</exception>
        Task<List<string>> GetInstanceIdsAsync(string modelName);

        /// <summary>Returns a JSON-serialized digital twin instance with the Id <paramref name="instanceId"/>, of
        /// type <paramref name="modelName"/>.</summary>
        /// <param name="modelName">The name of the digital twins model.</param>
        /// <param name="instanceId">The digital twin instance Id.</param>
        /// <returns>The JSON-serialized digital twin instance with the Id <paramref name="instanceId"/>, of
        /// type <paramref name="modelName"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="modelName"/> or <paramref name="instanceId"/> is empty or null.</exception>
        /// <exception cref="PersistenceProviderException">Failed to obtain the twin instance
        /// for the specified <paramref name="modelName"/>.</exception>       
        Task<string> GetInstanceAsync(string modelName, string instanceId);

        /// <summary>Returns the list of digital twin's properties defined by the specified <paramref name="modelName"/>.</summary>
        /// <param name="modelName">The name of the digital twins model.</param>
        /// <returns>List of digital twin's properties.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="modelName"/> is empty or null.</exception>
        /// <exception cref="PersistenceProviderException">Failed to build the list of properties
        /// for the specified <paramref name="modelName"/>.</exception>
        Task<IEnumerable<(string propertyName, string propertyType)>> GetPropertyListAsync(string modelName);

        /// <summary>
        /// Updates the value of a property on a digital twin identified by 
        /// <paramref name="instanceId"/> and <paramref name="modelName"/>.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="modelName">The name of the digital twins model.</param>
        /// <param name="instanceId">Digital twin instance Id.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyValue">Property's new value.</param>
        Task UpdatePropertyAsync<T>(string modelName, string instanceId, string propertyName, T propertyValue);

        /// <summary>
        /// Gets the value of a property from a digital twin identified by 
        /// <paramref name="instanceId"/> and <paramref name="modelName"/>.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="modelName">The name of the digital twins model.</param>
        /// <param name="instanceId">Digital twin instance Id.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The current property's value.</returns>
        Task<T> GetPropertyAsync<T>(string modelName, string instanceId, string propertyName);
    }
}
