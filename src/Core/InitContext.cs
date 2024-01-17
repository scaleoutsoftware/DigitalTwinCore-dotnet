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
using System.Collections.Generic;
using System.Text;

namespace Scaleout.Streaming.DigitalTwin.Core
{
    /// <summary>
    /// Context object that provides operations that are available
	/// when a digital twin instance is being created.
    /// </summary>
    public abstract class InitContext
    {
        /// <summary>
		/// Starts a new timer for the digital twin being created.
		/// </summary>
		/// <param name="timerName">The timer name.</param>
		/// <param name="interval">The timer interval.</param>
		/// <param name="type">The type of the timer.</param>
		/// <param name="timerCallback">A delegate representing a user-defined timer callback static method to be executed.</param>
		/// <returns><see cref="TimerActionResult.Success"/> if the timer was started successfully, otherwise one of the following 
		/// error codes is returned: <see cref="TimerActionResult.FailedTooManyTimers"/> when the maximum number of timers is reached or 
		/// <see cref="TimerActionResult.FailedInternalError"/> if an error occurred during the method call.</returns>
        public abstract TimerActionResult StartTimer(string timerName, TimeSpan interval, TimerType type, TimerHandler timerCallback);

        /// <summary>
        /// Gets an <see cref="ISharedData"/> instance for accessing shared objects
        /// that are associated with the model being processed.
        /// </summary>
        public abstract ISharedData SharedModelData { get; }

        /// <summary>
        /// Gets an <see cref="ISharedData"/> instance for accessing objects
        /// that are shared globally between all models.
        /// </summary>
        public abstract ISharedData SharedGlobalData { get; }


    }
}
