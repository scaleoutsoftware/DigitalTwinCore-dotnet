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

using System;
using System.Collections.Generic;
using System.Text;

namespace Scaleout.DigitalTwin.Workbench
{
    /// <summary>
    /// Execution result of a simulation time step.
    /// </summary>
    public readonly struct StepResult
    {
        internal StepResult(SimulationStatus status, DateTimeOffset nextSimulationTime)
        {
            SimulationStatus = status;
            NextSimulationTime = nextSimulationTime;
        }

        /// <summary>
        /// The status of the simulation after the time step completes.
        /// </summary>
        public SimulationStatus SimulationStatus { get; }

        /// <summary>
        /// The time of the next step in the simulation. (Equivalent to <see cref="SimulationWorkbench.PeekNextTimeStep"/>).
        /// </summary>
        /// <remarks>
        /// <para>
        /// At simulation completion, this property's value will vary depending on the final
        /// <see cref="SimulationStatus"/>.
        /// </para>
        /// <list type="table">
        ///   <listheader>
        ///     <term>Final SimulationStatus</term>
        ///     <description>NextSimulationTime value</description>
        ///   </listheader>
        ///   <item>
        ///     <term>StopRequested</term>
        ///     <description>The next time step in the simulation that would have occurred if the simulation had not ended.</description>
        ///   </item>
        ///   <item>
        ///     <term>NoRemainingWork</term>
        ///     <description><see cref="DateTimeOffset.MaxValue"/></description>
        ///   </item>
        ///   <item>
        ///     <term>EndTimeReached</term>
        ///     <description>
        ///     The next time step in the simulation that would have occurred if the simulation had not ended.
        ///     </description>
        ///   </item>
        /// </list>
        /// </remarks>
        public DateTimeOffset NextSimulationTime { get; }
    }
}
