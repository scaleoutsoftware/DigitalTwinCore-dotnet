#region Copyright notice and license

// Copyright 2023-2024 ScaleOut Software, Inc.
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
using System.ComponentModel.DataAnnotations;

using Scaleout.Streaming.DigitalTwin.Core.Exceptions;

namespace Scaleout.Streaming.DigitalTwin.Core
{


    /// <summary>
    /// Encapsulates the capabilities of a ScaleOut real-time digital twin 
    /// persistence provider.
    /// </summary>
    public interface IPersistenceProvider
    {
        /// <summary>Returns type of persistence provider ("SQLServer", "SQLite", etc).</summary>
        string ProviderType { get; }

        /// <summary>
        /// Indicates whether the persistence provider is active and can be used.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Returns the list of instance identifiers that are held in the specified <paramref name="containerName"/>. 
        /// </summary>
        /// <remarks><para>
        /// For the relational database providers, a list containing primary key values is returned.
        /// </para><para>
        /// For the Azure Digital Twins service, a list of digital twin instance identifiers is returned.
        /// </para><para>
        /// For Azure Blob storage, a list of Blob names would be returned.
        /// </para></remarks>
        /// <param name="containerName">The name of the persistence container.</param>
        /// <returns>The list of object identifiers that are located in the specified <paramref name="containerName"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="containerName"/> is empty or null.</exception>
        /// <exception cref="PersistenceProviderException">Failed to obtain the list of object instance identifiers
        /// for the specified <paramref name="containerName"/>.</exception>
        Task<List<string>> GetInstanceIdsAsync(string containerName);

        /// <summary>
        /// Returns the list of instance identifiers that are held in the specified <paramref name="containerName"/>. 
        /// </summary>
        /// <remarks><para>
        /// For the relational database providers, a list containing primary key values is returned.
        /// </para><para>
        /// For the Azure Digital Twins service, a list of digital twin instance identifiers is returned.
        /// </para><para>
        /// For Azure Blob storage, a list of Blob names would be returned.
        /// </para></remarks>
        /// <returns>The list of object identifiers that are located in the specified <paramref name="containerName"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="containerName"/> is empty or null.</exception>
        /// <exception cref="PersistenceProviderException">Failed to obtain the list of object instance identifiers
        /// for the specified <paramref name="containerName"/>.</exception>
        List<string> GetInstanceIds(string containerName);

        /// <summary>
        /// Returns the JSON-serialized object associated with an <paramref name="instanceId"/>.
        /// </summary>
        /// <param name="containerName">Name of the persistence container holding the object.</param>
        /// <param name="instanceId">ID of the persisted object.</param>
        /// <returns>JSON-serialized object.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="containerName"/> or <paramref name="instanceId"/> is empty or null.</exception>
        /// <exception cref="PersistenceProviderException">Failed to obtain the object instance
        /// for the specified <paramref name="containerName"/>.</exception>
        string GetInstance(string containerName, string instanceId);

        /// <summary>
        /// Returns a JSON-serialized object associated with an <paramref name="instanceId"/>.
        /// </summary>
        /// <param name="containerName">Name of the persistence container holding the object.</param>
        /// <param name="instanceId">ID of the persisted object.</param>
        /// <returns>JSON-serialized object.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="containerName"/> or <paramref name="instanceId"/> is empty or null.</exception>
        /// <exception cref="PersistenceProviderException">Failed to obtain the object instance
        /// for the specified <paramref name="containerName"/>.</exception>      
        Task<string> GetInstanceAsync(string containerName, string instanceId);

        /// <summary>Returns the properties on a persisted object. For the Azure Digital Twins service,
        /// this is a list of digital twin's properties, for the Azure Blob storage it is a list of Blob's properties,
        /// and for the relational database providers it is a list of table's columns.</summary>
        /// <param name="containerName">The name of the persistence container.</param>
        /// <returns>List of object's properties.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="containerName"/> is empty or null.</exception>
        /// <exception cref="PersistenceProviderException">Failed to build the list of properties
        /// for the specified <paramref name="containerName"/>.</exception>
        Task<IEnumerable<(string propertyName, string propertyType)>> GetPropertyListAsync(string containerName);

        /// <summary>Returns the properties on a persisted object. For the Azure Digital Twins service
        /// it is a list of digital twin's properties, for the Azure Blob storage it is a list of Blob's properties,
        /// and for the relational database providers it is a list of table's columns.</summary>
        /// <param name="containerName">The name of the persistence container.</param>
        /// <returns>List of object's properties.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="containerName"/> is empty or null.</exception>
        /// <exception cref="PersistenceProviderException">Failed to build the list of properties
        /// for the specified <paramref name="containerName"/>.</exception>
        IEnumerable<(string propertyName, string propertyType)> GetPropertyList(string containerName);

        /// <summary>
        /// Updates a property value on a specified persisted object, 
        /// given an <paramref name="instanceId"/> and <paramref name="containerName"/>.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="containerName">Name of the persistence container.</param>
        /// <param name="instanceId">Persisted object's instance Id.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyValue">Property's new value.</param>
        Task UpdatePropertyAsync<T>(string containerName, string instanceId, string propertyName, T propertyValue);

        /// <summary>
        /// Updates a property value on a specified persisted object, 
        /// given an <paramref name="instanceId"/> and <paramref name="containerName"/>.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="containerName">Name of the persistence container.</param>
        /// <param name="instanceId">Persisted object's instance Id.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyValue">Property's new value.</param>
        void UpdateProperty<T>(string containerName, string instanceId, string propertyName, T propertyValue);

        /// <summary>
        /// Updates a property value of a persisted object that is part of 
        /// a ScaleOut definition in the context of current real-time digital twin (RTDT) model.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="instanceId">Persisted object's instance Id.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyValue">Property's new value.</param>
        Task UpdateRTDTPropertyAsync<T>(string instanceId, string propertyName, T propertyValue);

        /// <summary>
        /// Updates a property value of a persisted object that is part of 
        /// a ScaleOut definition in the context of current real-time digital twin (RTDT) model.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="instanceId">Persisted object's instance Id.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyValue">Property's new value.</param>
        void UpdateRTDTProperty<T>(string instanceId, string propertyName, T propertyValue);

        /// <summary>
        /// Gets a property value on the specified persisted object located
        /// in the <paramref name="containerName"/> container.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="containerName">Name of the persistence container.</param>
        /// <param name="instanceId">Persisted object's instance Id.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The current property's value.</returns>
        Task<T> GetPropertyAsync<T>(string containerName, string instanceId, string propertyName);

        /// <summary>
        /// Gets a property value on the specified persisted object located
        /// in the <paramref name="containerName"/> container.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="containerName">Name of the persistence container.</param>
        /// <param name="instanceId">Persisted object's instance Id.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The current property's value.</returns>
        T GetProperty<T>(string containerName, string instanceId, string propertyName);

        /// <summary>
        /// Gets a property value on the specified persisted object <paramref name="instanceId"/> that is part of 
        /// a ScaleOut component definition in the context of current real-time digital twin (RTDT) model.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="instanceId">Persisted object's instance Id.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The current property's value.</returns>
        Task<T> GetRTDTPropertyAsync<T>(string instanceId, string propertyName);

        /// <summary>
        /// Gets a property value on the specified persisted object <paramref name="instanceId"/> that is part of 
        /// a ScaleOut component definition in the context of current real-time digital twin (RTDT) model.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="instanceId">Persisted object's instance Id.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The current property's value.</returns>
        T GetRTDTProperty<T>(string instanceId, string propertyName);
    }
}
