#region Copyright notice and license

// Copyright 2023-2025 ScaleOut Software, Inc.
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

using Scaleout.Streaming.DigitalTwin.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Scaleout.DigitalTwin.Workbench
{
    /// <summary>
    /// Represents the status of a simulation running under a <see cref="SimulationWorkbench"/> instance.
    /// </summary>
    public enum SimulationStatus
    {
        /// <summary>
        /// The simulation is running, and additional time steps remain.
        /// </summary>
        Running,

        /// <summary>
        /// The simulation has stopped because a ProcessModel implementation
        /// called <see cref="ISimulationController.StopSimulation"/>.
        /// </summary>
        StopRequested,

        /// <summary>
        /// The simulation has stopped because there are no digital twins
        /// participating in the simulation.
        /// </summary>
        NoRemainingWork,

        /// <summary>
        /// The simulation has stopped because the simulated time has reached
        /// the endTime that was specified at simulation initialization.
        /// </summary>
        EndTimeReached
    }
}
