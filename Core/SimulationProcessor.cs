/*
  SimulationProcessor.cs
  
  Copyright (C), 2018-2023 by ScaleOut Software, Inc.

  PROPRIETARY TRADE SECRET INFORMATION OF SCALEOUT SOFTWARE, INC.

  The information contained in this file is a trade secret of ScaleOut Software, Inc.
  and is not to be disclosed or copied in any form without written permission.
*/
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
