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

namespace Scaleout.Streaming.DigitalTwin.Core
{
    /// <summary>
    /// Abstract base class for logic that gets triggered for every time interval in a simulation.
    /// </summary>
    /// <typeparam name="TDigitalTwin">User-defined type for a digital twin object.</typeparam>
    public abstract class SimulationProcessor<TDigitalTwin> : SimulationProcessor where TDigitalTwin: class
    {
        /// <summary>
        /// The method called by the ScaleOut service every time the simulation time 
        /// interval has elapsed.
        /// </summary>
        /// <param name="context">Digital twin model processing context.</param>
        /// <param name="digitalTwin">Targeted digital twin instance.</param>
        /// <param name="currentTime">The current simulation time.</param>
        /// <returns><see cref="ProcessingResult.DoUpdate"/> if the digital twin
        /// object needs to be updated, or <see cref="ProcessingResult.NoUpdate"/> if
        /// no updates are needed.</returns>
        public abstract ProcessingResult ProcessModel(ProcessingContext context, TDigitalTwin digitalTwin, DateTimeOffset currentTime);

        /// <summary>
        /// This method called by the ScaleOut service when the simulation starts.
        /// </summary>
        /// <param name="context">Initial simulation processing context that allows to access shared data.</param>
        /// <param name="digitalTwin">Targeted digital twin instance.</param>
        /// <param name="startTime">The simulation start time.</param>
        /// <returns><see cref="ProcessingResult.DoUpdate"/> if the digital twin
        /// object needs to be updated, or <see cref="ProcessingResult.NoUpdate"/> if
        /// no updates are needed.</returns>
        public abstract ProcessingResult InitSimulation(InitSimulationContext context, TDigitalTwin digitalTwin, DateTimeOffset startTime);

        internal override ProcessingResult InitSimulation(InitSimulationContext context, DigitalTwinBase digitalTwin, DateTimeOffset startTime)
        {
            return InitSimulation(context, digitalTwin as TDigitalTwin, startTime);
        }

        internal override ProcessingResult ProcessModel(ProcessingContext context, DigitalTwinBase digitalTwin, DateTimeOffset currentTime)
        {
            return ProcessModel(context, digitalTwin as TDigitalTwin, currentTime);
        }

        /// <inheritdoc/>
        internal override TimeSpan SimulationInterval { get; }
    }

    /// <summary>
    /// Abstract base class used by ScaleOut Digital Twin Library infrastructure.
    /// </summary>
    public abstract class SimulationProcessor
    {
        /// <summary>
        /// This method called by the ScaleOut service when the simulation starts.
        /// </summary>
        /// <param name="context">Initial simulation processing context that allows to access shared data.</param>
        /// <param name="digitalTwin">Targeted digital twin instance.</param>
        /// <param name="startTime">The simulation start time.</param>
        /// <returns><see cref="ProcessingResult.DoUpdate"/> if the digital twin
        /// object needs to be updated, or <see cref="ProcessingResult.NoUpdate"/> if
        /// no updates are needed.</returns>
        internal abstract ProcessingResult InitSimulation(InitSimulationContext context, DigitalTwinBase digitalTwin, DateTimeOffset startTime);

        /// <summary>
        /// The method called by the ScaleOut service every time the simulation time 
        /// interval has elapsed.
        /// </summary>
        /// <param name="context">Digital twin model processing context.</param>
        /// <param name="digitalTwin">Targeted digital twin instance.</param>
        /// <param name="currentTime">The current simulation time.</param>
        /// <returns><see cref="ProcessingResult.DoUpdate"/> if the digital twin
        /// object needs to be updated, or <see cref="ProcessingResult.NoUpdate"/> if
        /// no updates are needed.</returns>
        internal abstract ProcessingResult ProcessModel(ProcessingContext context, DigitalTwinBase digitalTwin, DateTimeOffset currentTime);

        /// <summary>
        /// The simulated time interval.
        /// </summary>
        internal abstract TimeSpan SimulationInterval { get; }
    }


}
