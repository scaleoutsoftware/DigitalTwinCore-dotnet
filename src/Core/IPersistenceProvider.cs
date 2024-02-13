#region Copyright notice and license

// Copyright 2023 ScaleOut Software, Inc.
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
    /// List of supported persistence providers.
    /// </summary>
    public enum PersistenceProviderType
    {
        /// <summary>Unspecified (when model does not use any persistence provider).</summary>
        [Display(Name = "Unspecified")]
        Unspecified,

        /// <summary>Azure Digital Twins.</summary>
        [Display(Name = "Azure Digital Twins Service")]
        AzureDigitalTwinsService,

        /// <summary>Azure Blob Storage service (future release).</summary>
        [Display(Name = "Azure Blob Storage")]
        AzureBlobStorage,

        /// <summary>SQL Server</summary>
        [Display(Name = "SQLServer")]
        SQLServer,

        /// <summary>SQLite</summary>
        [Display(Name = "SQLite")]
        SQLite,

        /// <summary>DynamoDB</summary>
        [Display(Name = "DynamoDB")]
        DynamoDb
    }

    /// <summary>
    /// Encapsulates the capabilities of a ScaleOut real-time digital twin 
    /// persistence provider.
    /// </summary>
    public interface IPersistenceProvider
    {
        /// <summary>Returns <see cref="PersistenceProviderType"/> identifier.</summary>
        PersistenceProviderType ProviderType { get; }

        /// <summary>
        /// Indicates whether the persistence provider is active and can be used.
        /// </summary>
        bool IsActive { get; }

        // <summary>Returns a list of persistence containers. For the Azure Digital Twins service,
        // this is a list of model Ids (interface IDs). For Azure Blob storage, this is a list of BLOB containers,
        // for relational database providers it is a list of tables.</summary>
        // <exception cref="PersistenceProviderException">Failed to get the list of containers
        // from the persistence provider.</exception>
        //Task<IReadOnlyList<string>> GetContainerListAsync();

        // <summary>Returns a list of persistence containers. For the Azure Digital Twins service
        // it is a list of models (interfaces), for Azure Blob Storage this is a list of BLOB containers,
        // for relational database providers this is a list of tables.</summary>
        // <exception cref="PersistenceProviderException">Failed to get the list of containers
        // from the persistence provider.</exception>
        //IReadOnlyList<string> GetContainerList();

        // <summary>Returns the schema of the specified persistence <paramref name="containerName"/>. 
        // For the Azure Digital Twins service, this is a JSON-formatted model definition.</summary>
        // <param name="containerName">The name of a persistence container.</param>
        // <returns>The schema that describes this persistence container's properties and their
        // types.</returns>
        // <exception cref="ArgumentNullException"><paramref name="containerName"/> is empty or null.</exception>
        // <exception cref="PersistenceProviderException">Failed to get the schema for the specified
        // container from the persistence provider.</exception>
        //Task<string> GetContainerSchemaAsync(string containerName);

        // <summary>Returns the schema of the specified persistence <paramref name="containerName"/>. 
        // For the Azure Digital Twins service, this is a JSON-formatted model definition.</summary>
        // <param name="containerName">The name of the persistence container.</param>
        // <returns>The schema that describes the persistence container's properties and their
        // types.</returns>
        // <exception cref="ArgumentNullException"><paramref name="containerName"/> is empty or null.</exception>
        // <exception cref="PersistenceProviderException">Failed to get the schema for the specified
        // container from the persistence provider.</exception>
        //string GetContainerSchema(string containerName);

        /// <summary>Returns the list of object identifiers that are part of the specified <paramref name="containerName"/>. 
        /// For the Azure Digital Twins service it is a list of digital twin instance identifiers, for the Azure Blob storage it could be 
        /// a list of Blob names. For the relational database providers it is a list of table's primary keys.</summary>
        /// <param name="containerName">The name of the persistence container.</param>
        /// <returns>The list of object identifiers that are located in the specified <paramref name="containerName"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="containerName"/> is empty or null.</exception>
        /// <exception cref="PersistenceProviderException">Failed to obtain the list of object instance identifiers
        /// for the specified <paramref name="containerName"/>.</exception>
        Task<List<string>> GetInstanceIdsAsync(string containerName);

        /// <summary>Returns the list of object identifiers that are part of the specified <paramref name="containerName"/>. 
        /// For the Azure Digital Twins service it is a list of digital twin instance identifiers, for the Azure Blob storage it could be 
        /// a list of Blob names. For the relational database providers it is a list of table's primary keys.</summary>
        /// <param name="containerName">The name of the persistence container.</param>
        /// <returns>The list of object identifiers that are located in the specified <paramref name="containerName"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="containerName"/> is empty or null.</exception>
        /// <exception cref="PersistenceProviderException">Failed to obtain the list of object instance identifiers
        /// for the specified <paramref name="containerName"/>.</exception>
        List<string> GetInstanceIds(string containerName);

        /// <summary>Returns a JSON-serialized object's <paramref name="instanceId"/> 
        /// that is part of the specified <paramref name="containerName"/></summary>
        /// <param name="containerName">The name of the persistence container.</param>
        /// <param name="instanceId">Persistence object's instance Id.</param>
        /// <returns>The JSON-formatted content of the object's object's <paramref name="instanceId"/> 
        /// that is located in the specified <paramref name="containerName"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="containerName"/> or <paramref name="instanceId"/> is empty or null.</exception>
        /// <exception cref="PersistenceProviderException">Failed to obtain the object instance
        /// for the specified <paramref name="containerName"/>.</exception>
        string GetInstance(string containerName, string instanceId);

        /// <summary>Returns a JSON-serialized object's <paramref name="instanceId"/> 
        /// that is part of the specified <paramref name="containerName"/></summary>
        /// <param name="containerName">The name of the persistence container.</param>
        /// <param name="instanceId">Persistence object's instance Id.</param>
        /// <returns>The JSON-formatted content of the object's object's <paramref name="instanceId"/> 
        /// that is located in the specified <paramref name="containerName"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="containerName"/> or <paramref name="instanceId"/> is empty or null.</exception>
        /// <exception cref="PersistenceProviderException">Failed to obtain the object instance
        /// for the specified <paramref name="containerName"/>.</exception>       
        Task<string> GetInstanceAsync(string containerName, string instanceId);

        /// <summary>Returns the properties on a persistence object. For the Azure Digital Twins service
        /// it is a list of digital twin's properties, for the Azure Blob storage it is a list of Blob's properties,
        /// and for the relational database providers it is a list of table's columns.</summary>
        /// <param name="containerName">The name of the persistence container.</param>
        /// <returns>List of object's properties.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="containerName"/> is empty or null.</exception>
        /// <exception cref="PersistenceProviderException">Failed to build the list of properties
        /// for the specified <paramref name="containerName"/>.</exception>
        Task<IEnumerable<(string propertyName, string propertyType)>> GetPropertyListAsync(string containerName);

        /// <summary>Returns the properties on a persistence object. For the Azure Digital Twins service
        /// it is a list of digital twin's properties, for the Azure Blob storage it is a list of Blob's properties,
        /// and for the relational database providers it is a list of table's columns.</summary>
        /// <param name="containerName">The name of the persistence container.</param>
        /// <returns>List of object's properties.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="containerName"/> is empty or null.</exception>
        /// <exception cref="PersistenceProviderException">Failed to build the list of properties
        /// for the specified <paramref name="containerName"/>.</exception>
        IEnumerable<(string propertyName, string propertyType)> GetPropertyList(string containerName);

        /// <summary>
        /// Updates a property value on a specified persistence object, 
        /// given an <paramref name="instanceId"/> and <paramref name="containerName"/>.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="containerName">Name of the persistence container.</param>
        /// <param name="instanceId">Persistence object's instance Id.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyValue">Property's new value.</param>
        Task UpdatePropertyAsync<T>(string containerName, string instanceId, string propertyName, T propertyValue);

        /// <summary>
        /// Updates a property value on a specified persistence object, 
        /// given an <paramref name="instanceId"/> and <paramref name="containerName"/>.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="containerName">Name of the persistence container.</param>
        /// <param name="instanceId">Persistence object's instance Id.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyValue">Property's new value.</param>
        void UpdateProperty<T>(string containerName, string instanceId, string propertyName, T propertyValue);

        /// <summary>
        /// Updates a property value of a persistence object that is part of 
        /// a ScaleOut definition in the context of current real-time digital twin (RTDT) model.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="instanceId">Persistence object's instance Id.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyValue">Property's new value.</param>
        Task UpdateRTDTPropertyAsync<T>(string instanceId, string propertyName, T propertyValue);

        /// <summary>
        /// Updates a property value of a persistence object that is part of 
        /// a ScaleOut definition in the context of current real-time digital twin (RTDT) model.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="instanceId">Persistence object's instance Id.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyValue">Property's new value.</param>
        void UpdateRTDTProperty<T>(string instanceId, string propertyName, T propertyValue);

        /// <summary>
        /// Gets a property value on the specified persistence object located
        /// in the <paramref name="containerName"/> container.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="containerName">Name of the persistence container.</param>
        /// <param name="instanceId">Persistence object's instance Id.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The current property's value.</returns>
        Task<T> GetPropertyAsync<T>(string containerName, string instanceId, string propertyName);

        /// <summary>
        /// Gets a property value on the specified persistence object located
        /// in the <paramref name="containerName"/> container.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="containerName">Name of the persistence container.</param>
        /// <param name="instanceId">Persistence object's instance Id.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The current property's value.</returns>
        T GetProperty<T>(string containerName, string instanceId, string propertyName);

        /// <summary>
        /// Gets a property value on the specified persistence object <paramref name="instanceId"/> that is part of 
        /// a ScaleOut component definition in the context of current real-time digital twin (RTDT) model.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="instanceId">Persistence object's instance Id.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The current property's value.</returns>
        Task<T> GetRTDTPropertyAsync<T>(string instanceId, string propertyName);

        /// <summary>
        /// Gets a property value on the specified persistence object <paramref name="instanceId"/> that is part of 
        /// a ScaleOut component definition in the context of current real-time digital twin (RTDT) model.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="instanceId">Persistence object's instance Id.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The current property's value.</returns>
        T GetRTDTProperty<T>(string instanceId, string propertyName);
    }
}
