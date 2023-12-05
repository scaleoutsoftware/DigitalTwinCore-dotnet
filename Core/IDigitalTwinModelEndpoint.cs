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

using System.Collections.Generic;

namespace Scaleout.Streaming.DigitalTwin.Core
{
    /// <summary>
    /// Interface for use by client applications that send messages to digital twin instances.
    /// </summary>
    public interface IDigitalTwinModelEndpoint
    {
        /// <summary>
        /// Sends JSON serialized messages to a digital twin instance.
        /// </summary>
        /// <param name="digitalTwinId">ID of the digital twin instance.</param>
        /// <param name="messages">Enumerable collection of serialized messages.</param>
        /// <returns><see cref="SendingResult.Handled"/> if the message was successfully sent,
		/// <see cref="SendingResult.NotHandled"/> otherwise.</returns>
        SendingResult Send(string digitalTwinId, IEnumerable<byte[]> messages);

        /// <summary>
        /// Sends a JSON serialized message to a digital twin instance.
        /// </summary>
        /// <param name="digitalTwinId">ID of the digital twin instance.</param>
        /// <param name="message">Serialized message.</param>
        /// <returns><see cref="SendingResult.Handled"/> if the message was successfully sent,
		/// <see cref="SendingResult.NotHandled"/> otherwise.</returns>
        SendingResult Send(string digitalTwinId, byte[] message);

        /// <summary>
        /// Sends a JSON serialized message to a digital twin instance.
        /// </summary>
        /// <param name="digitalTwinId">ID of the digital twin instance.</param>
        /// <param name="message">Serialized message.</param>
        /// <returns><see cref="SendingResult.Handled"/> if the message was successfully sent,
		/// <see cref="SendingResult.NotHandled"/> otherwise.</returns>
        SendingResult Send(string digitalTwinId, string message);

        /// <summary>
        /// Sends a message to a digital twin instance.
        /// </summary>
        /// <param name="digitalTwinId">ID of the digital twin instance.</param>
        /// <param name="message">Message instance.</param>
        /// <returns><see cref="SendingResult.Handled"/> if the message was successfully sent,
		/// <see cref="SendingResult.NotHandled"/> otherwise.</returns>
        SendingResult Send(string digitalTwinId, object message);

        /// <summary>
        /// Sends messages to a digital twin instance.
        /// </summary>
        /// <param name="digitalTwinId">ID of the digital twin instance.</param>
        /// <param name="message">Messages to send to the instance.</param>
        /// <returns><see cref="SendingResult.Handled"/> if the message was successfully sent,
		/// <see cref="SendingResult.NotHandled"/> otherwise.</returns>
        SendingResult Send(string digitalTwinId, IEnumerable<object> message);

        /// <summary>
        /// Creates a new digital twin instance.
        /// </summary>
        /// <param name="digitalTwinId">ID of the digital twin instance.</param>
        /// <param name="digitalTwin">Digital twin instance to create. It could an object of a real
        /// digital twin model type or simply an anonymous object with a set of digital twin model's 
        /// properties and their initial values.</param>
        /// <returns><see cref="SendingResult.Handled"/> if the message was successfully sent,
		/// <see cref="SendingResult.NotHandled"/> otherwise.</returns>
        SendingResult CreateTwin(string digitalTwinId, object digitalTwin);

        /// <summary>
        /// Create a new digital twin instance . This method first tries to create 
        /// a new twin instance from a persistence store if it is enabled. If a persistence store is not enabled or
        /// if the twin instance is not found there, the property values of the specified fallback 
        /// <paramref name="defaultValue"/> are used to create and initialize a new twin instance.
        /// </summary>
        /// <param name="digitalTwinId">Digital twin identifier.</param>
        /// <param name="defaultValue">Digital twin instance to create. It could an object of a real
        /// digital twin model type or simply an anonymous object with a set of digital twin model's 
        /// properties and their initial values.</param>
        /// <returns><see cref="SendingResult.Handled"/> in case of success, otherwise 
        /// the method throws DigitalTwinInstantiationException with the error details.</returns>
        SendingResult CreateTwinFromPersistenceStore(string digitalTwinId, object defaultValue);

        /// <summary>
        /// Create a new digital twin instance in the ScaleOut data grid. This method assumes that persistence store is 
        /// enabled for the target model and the twin instance with the specified <paramref name="digitalTwinId"/> 
        /// exists there. In this case, a new twin instance is created and initialized from the persistence store.
        /// Otherwise, the <see cref="Scaleout.Streaming.DigitalTwin.Core.Exceptions.DigitalTwinInstantiationException"/>
        /// is thrown.
        /// </summary>
        /// <param name="digitalTwinId">Digital twin identifier.</param>
        /// <returns><see cref="SendingResult.Handled"/> in case of success, otherwise 
        /// the method throws DigitalTwinInstantiationException with the error details.</returns>
        SendingResult CreateTwinFromPersistenceStore(string digitalTwinId);

        /// <summary>
        /// Delete a digital twin instance from the ScaleOut data grid.
        /// </summary>
		/// <param name="digitalTwinId">Digital twin identifier.</param>
        /// <returns><see cref="SendingResult.Handled"/> in case of success, otherwise 
        /// the method throws DigitalTwinProcessingException with the error details.</returns>
        SendingResult DeleteTwin(string digitalTwinId);
    }
}
