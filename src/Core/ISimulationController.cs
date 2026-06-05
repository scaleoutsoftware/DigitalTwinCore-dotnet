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
    /// Represents the controller of a digital twin model simulation process.
    /// </summary>
    public interface ISimulationController
    {
        /// <summary>
        /// Gets the simulation's time increment.
        /// </summary>
        /// <returns><see cref="TimeSpan"/> for the simulation time increment.</returns>
        /// <exception cref="Scaleout.Modules.DigitalTwin.Abstractions.Exceptions.ModelSimulationException">
        /// The exception is thrown if the current digital twin model does not support simulation.
        /// </exception>
        TimeSpan GetSimulationTimeIncrement();

        /// <summary>
        /// Delays the scheduling of the current instance for the specified <paramref name="delay"/>.
        /// </summary>
        /// <param name="delay"><see cref="TimeSpan"/> for simulation time delay.</param>
        /// <exception cref="Scaleout.Modules.DigitalTwin.Abstractions.Exceptions.ModelSimulationException">
        /// The exception is thrown if the current digital twin model does not support simulation.
        /// </exception>
        void Delay(TimeSpan delay);

        /// <summary>
        /// Delays calling the <see cref="SimulationProcessor{TDigitalTwin}.ProcessModelAsync(ProcessingContext{TDigitalTwin}, TDigitalTwin, DateTimeOffset)"/>
        /// method for this instance forever. Users can interrupt this infinite delay later
        /// by calling <see cref="ISimulationController.RunThisInstance"/> for this instance within the 
        /// <see cref="MessageProcessor{TDigitalTwin}.ProcessMessageAsync(ProcessingContext{TDigitalTwin}, TDigitalTwin, byte[])"/> method call.
        /// </summary>
        /// <exception cref="Scaleout.Modules.DigitalTwin.Abstractions.Exceptions.ModelSimulationException">
        /// The exception is thrown if the current digital twin model does not support simulation.
        /// </exception>
        void DelayIndefinitely();

        /// <summary>
        /// Sends a telemetry message to the corresponding real-time digital twin instance. 
        /// The instance ID of the corresponding real-time digital twin is expected to be 
        /// the same as the instance ID of the simulated digital twin instance that
        /// emits the telemetry message. 
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
        /// </summary>
        /// <param name="modelName">Digital twin model name.</param>
        /// <param name="twinId">Digital twin identifier.</param>
        /// <param name="newInstance">Digital twin instance to create. This can an object of the actual class 
        /// representing the digital twin model type or simply an anonymous object with a set of digital twin model's
        /// digital twin model or simply an anonymous object with a set of the digital twin model's 
        /// properties and their initial values.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="Scaleout.Modules.DigitalTwin.Abstractions.Exceptions.ModelSimulationException">
        /// The exception is thrown if the current digital twin model does not support simulation.
        /// </exception>
        /// <exception cref="Scaleout.Modules.DigitalTwin.Abstractions.Exceptions.DigitalTwinInstantiationException">
        /// An error occurred while creating a new digital twin instance.
        /// </exception>
        Task CreateInstanceAsync(string modelName, string twinId, object newInstance);

        /// <summary>
        /// Delete a digital twin instance of the specified simulation <paramref name="modelName"/>.
        /// </summary>
		/// <param name="modelName">Digital twin model name.</param>
		/// <param name="twinId">Digital twin identifier.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="Scaleout.Modules.DigitalTwin.Abstractions.Exceptions.ModelSimulationException">
        /// The exception is thrown if the current digital twin model does not support simulation.
        /// </exception>
        Task DeleteInstanceAsync(string modelName, string twinId);

        /// <summary>
        /// Delete this simulation twin instance.
        /// </summary>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="Scaleout.Modules.DigitalTwin.Abstractions.Exceptions.ModelSimulationException">
        /// The exception is thrown if the current digital twin model does not support simulation.
        /// </exception>
        Task DeleteThisInstanceAsync();

        /// <summary>
        /// Adds this simulation twin instance to the end of the priority queue for
        /// running the <see cref="SimulationProcessor{TDigitalTwin}.ProcessModelAsync(ProcessingContext{TDigitalTwin}, TDigitalTwin, DateTimeOffset)"/> 
        /// method for it at the current simulation time.
        /// </summary>
        void RunThisInstance();

        /// <summary>
        /// Stop the currently running simulation after completing the current timestep.
        /// </summary>
        void StopSimulation();

        /// <summary>
        /// Returns the simulation start time in UTC.
        /// </summary>
        /// <returns>The simulation start time.</returns>
        DateTimeOffset SimulationStartTime { get; }
    }
}
