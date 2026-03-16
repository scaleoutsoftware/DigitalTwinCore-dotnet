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

namespace Scaleout.Modules.DigitalTwin.Abstractions
{
    /// <summary>
    /// The methods of this interface allow user to control all aspects of 
    /// digital twin's model simulation process.
    /// </summary>
    public interface ISimulationController
    {
        /// <summary>
        /// Get a simulation time increment.
        /// </summary>
        /// <returns><see cref="TimeSpan"/> for the simulation time increment.</returns>
        /// <exception cref="Scaleout.Modules.DigitalTwin.Abstractions.Exceptions.ModelSimulationException">
        /// The exception is thrown if the current digital twin model does not support simulation.
        /// </exception>
        TimeSpan GetSimulationTimeIncrement();

        /// <summary>
        /// Delays the wake-up for the current instance for the specified <paramref name="delay"/>.
        /// </summary>
        /// <param name="delay"><see cref="TimeSpan"/> for simulation time delay.</param>
        /// <exception cref="Scaleout.Modules.DigitalTwin.Abstractions.Exceptions.ModelSimulationException">
        /// The exception is thrown if the current digital twin model does not support simulation.
        /// </exception>
        void Delay(TimeSpan delay);

        /// <summary>
        /// Delays calling the <see cref="SimulationProcessor{TDigitalTwin}.ProcessModelAsync(ProcessingContext{TDigitalTwin}, TDigitalTwin, DateTimeOffset)"/>
        /// method for this instance forever. Users can interrupt this infinite delay later
        /// by calling <see cref="ISimulationController.RunThisTwin"/> for this instance within the 
        /// <see cref="MessageProcessor{TDigitalTwin}.ProcessMessageAsync(ProcessingContext{TDigitalTwin}, TDigitalTwin, byte[])"/> method call.
        /// </summary>
        /// <exception cref="Scaleout.Modules.DigitalTwin.Abstractions.Exceptions.ModelSimulationException">
        /// The exception is thrown if the current digital twin model does not support simulation.
        /// </exception>
        void DelayIndefinitely();

        /// <summary>
        /// Sends a telemetry message to the corresponding real-time digital twin instance. 
        /// The twin ids for both, sending digital twin in a simulation model and the receiving twin 
        /// in the real-time model are the same.
        /// </summary>
        /// <param name="modelName">Real-time digital twin model name.</param>
        /// <param name="message">The JSON-serialized message to send.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="Scaleout.Modules.DigitalTwin.Abstractions.Exceptions.ModelSimulationException">
        /// The exception is thrown if the current digital twin model does not support simulation.
        /// </exception>
        /// <exception cref="Scaleout.Modules.DigitalTwin.Abstractions.Exceptions.DigitalTwinProcessingException">
        /// An error occurred while processing the message by digital twin.
        /// </exception>
        Task EmitTelemetryAsync(string modelName, byte[] message);


        /// <summary>
        /// Create a new digital twin instance of the specified simulation <paramref name="modelName"/>.
        /// This method forces to use the specified object instance over the one that could be found
        /// in the persistence store if it is enabled.
        /// </summary>
        /// <param name="modelName">Digital twin model name.</param>
        /// <param name="twinId">Digital twin identifier.</param>
        /// <param name="newInstance">Digital twin instance to create. It could be an object of a real
        /// digital twin model type or simply an anonymous object with a set of digital twin model's 
        /// properties and their initial values.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="Scaleout.Modules.DigitalTwin.Abstractions.Exceptions.ModelSimulationException">
        /// The exception is thrown if the current digital twin model does not support simulation.
        /// </exception>
        /// <exception cref="Scaleout.Modules.DigitalTwin.Abstractions.Exceptions.DigitalTwinInstantiationException">
        /// An error occurred while creating a new digital twin instance.
        /// </exception>
        Task CreateTwinAsync(string modelName, string twinId, object newInstance);

        /// <summary>
        /// Create a new digital twin instance of the specified simulation <paramref name="modelName"/>.
        /// This method first tries to create a new twin instance from a persistence store if it is enabled.
        /// If a persistence store is not enabled or if the twin instance is not found there, then the property values 
        /// of the specified fallback <paramref name="defaultInstance"/> are used to create and initialize a new twin instance.
        /// </summary>
        /// <param name="modelName">Digital twin model name.</param>
        /// <param name="twinId">Digital twin identifier.</param>
        /// <param name="defaultInstance">Digital twin instance to create. It could an object of a real
        /// digital twin model type or simply an anonymous object with a set of digital twin model's 
        /// properties and their initial values.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="Scaleout.Modules.DigitalTwin.Abstractions.Exceptions.ModelSimulationException">
        /// The exception is thrown if the current digital twin model does not support simulation.
        /// </exception>
        /// <exception cref="Scaleout.Modules.DigitalTwin.Abstractions.Exceptions.DigitalTwinInstantiationException">
        /// An error occurred while creating a new digital twin instance.
        /// </exception>
        Task CreateTwinFromPersistenceStoreAsync(string modelName, string twinId, object defaultInstance);

        /// <summary>
        /// Create a new digital twin instance of the specified simulation <paramref name="modelName"/>.
        /// This method assumes that persistence store is enabled for the <paramref name="modelName"/> and 
        /// twin instance with the specified <paramref name="twinId"/> exists there. In this case,
        /// a new twin instance is created and initialized from the persistence store.
        /// </summary>
        /// <param name="modelName">Digital twin model name.</param>
        /// <param name="twinId">Digital twin identifier.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="Scaleout.Modules.DigitalTwin.Abstractions.Exceptions.ModelSimulationException">
        /// The exception is thrown if the current digital twin model does not support simulation.
        /// </exception>
        /// <exception cref="Scaleout.Modules.DigitalTwin.Abstractions.Exceptions.DigitalTwinInstantiationException">
        /// An error occurred while creating a new digital twin instance.
        /// </exception>
        Task CreateTwinFromPersistenceStoreAsync(string modelName, string twinId);

        /// <summary>
        /// Delete a digital twin instance of the specified simulation <paramref name="modelName"/>.
        /// </summary>
		/// <param name="modelName">Digital twin model name.</param>
		/// <param name="twinId">Digital twin identifier.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="Scaleout.Modules.DigitalTwin.Abstractions.Exceptions.ModelSimulationException">
        /// The exception is thrown if the current digital twin model does not support simulation.
        /// </exception>
        Task DeleteTwinAsync(string modelName, string twinId);

        /// <summary>
        /// Delete this simulation twin instance (itself).
        /// </summary>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="Scaleout.Modules.DigitalTwin.Abstractions.Exceptions.ModelSimulationException">
        /// The exception is thrown if the current digital twin model does not support simulation.
        /// </exception>
        Task DeleteThisTwinAsync();

        /// <summary>
        /// Adds this simulation twin instance (itself) to the end of the priority queue for
        /// running the <see cref="SimulationProcessor{TDigitalTwin}.ProcessModelAsync(ProcessingContext{TDigitalTwin}, TDigitalTwin, DateTimeOffset)"/> 
        /// method for it at the current simulation time.
        /// </summary>
        void RunThisTwin();

        /// <summary>
        /// Stop the currently running simulation.
        /// </summary>
        void StopSimulation();

        /// <summary>
        /// Returns the simulation start time in UTC.
        /// </summary>
        /// <returns>The simulation start time.</returns>
        DateTimeOffset SimulationStartTime { get; }
    }
}
