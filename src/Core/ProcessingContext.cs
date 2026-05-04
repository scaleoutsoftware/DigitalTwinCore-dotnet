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

using Scaleout.Modules.DigitalTwin.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Scaleout.Modules.DigitalTwin.Abstractions
{
    /// <summary>
    /// Message processing context for currently processing messages.
    /// It allows sending messages back to a data source where the messages 
    /// are originated from.
    /// </summary>
    public abstract class ProcessingContext<TDigitalTwin> where TDigitalTwin : DigitalTwinBase<TDigitalTwin>
    {
        /// <summary>
        /// Digital twin model name.
        /// </summary>
        public abstract string DigitalTwinModel { get; }

        /// <summary>
        /// Sends a message back to a data source origination point (e.g. IoT device). When sending 
        /// a message to the ScaleOut Messaging REST service as a data source, the message content
        /// must be JSON encoded.
        /// </summary>
        /// <param name="message">JSON encoded message as <see cref="T:byte[]" />.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public abstract Task SendToDataSourceAsync(byte[] message);

        /// <summary>
        /// Sends a list of messages back to a data source origination point (e.g. IoT device).
        /// When sending messages to the ScaleOut Messaging REST service as a data source, each message
        /// must be JSON encoded.
        /// </summary>
        /// <param name="messages">JSON encoded messages as a list of <see cref="T:byte[]" />.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public abstract Task SendToDataSourceAsync(IEnumerable<byte[]> messages);



        /// <summary>
        /// Sends message to a different digital twin in twin's hierarchy.
        /// </summary>
        /// <param name="targetTwinModel">Digital twin model name.</param>
        /// <param name="targetTwinId">Digital twin identifier.</param>
        /// <param name="message">JSON encoded message as <see cref="T:byte[]" />.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public abstract Task SendToTwinAsync(string targetTwinModel, string targetTwinId, byte[] message);



        /// <summary>
        /// Sends a list of messages to a different digital twin in twin's hierarchy.
        /// </summary>
        /// <param name="targetTwinModel">Digital twin model name.</param>
        /// <param name="targetTwinId">Digital twin identifier.</param>
        /// <param name="messages">JSON encoded messages as a list of <see cref="T:byte[]" />.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public abstract Task SendToTwinAsync(string targetTwinModel, string targetTwinId, IEnumerable<byte[]> messages);



        /// <summary>
        /// Logs a message that is visible in the ScaleOut Digital Twins UI.
        /// </summary>
        /// <param name="severity">The severity level for the specified message.</param>
        /// <param name="message">The user message to log.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public abstract Task LogMessageAsync(LogSeverity severity, string message);

        /// <summary>
		/// Sends an alert using the alerting provider that is associated with the 
        /// digital twin model.
		/// </summary>
		/// <param name="alertMessage">The provided object contains information about the alert data, as well as the provider to target.</param>
		/// <returns>The task object representing the asynchronous operation.</returns>
		public abstract Task SendAlertAsync(AlertMessage alertMessage);

        /// <summary>
        /// Returns a reference to registered AzureDigitalTwins provider the model is using.
        /// Azure Digital Twins must be configured as a persistence provider in the ScaleOut
        /// Digital Twins service, and the model must be configured to use it in appsettings.json.
        /// </summary>
        /// <returns>The AzureDigitalTwins provider used by the model.</returns>
        public abstract IAzureDigitalTwinsProvider AzureDigitalTwinsProvider { get; }

        /// <summary>
        /// Returns the collection of registered anomaly detection providers the model has access to.
        /// </summary>
        public abstract Dictionary<string, IAnomalyDetectionProvider> AnomalyDetectionProviders { get; }

        /// <summary>
        /// Starts a new timer for the digital twin whose message is currently being processed.
        /// </summary>
        /// <param name="timerName">The timer name.</param>
        /// <param name="interval">The timer interval.</param>
        /// <param name="type">The type of the timer.</param>
        /// <param name="timerCallback">A function representing a user-defined timer callback method to be executed.</param>
        /// <returns><see cref="TimerActionResult.Success"/> if the timer was started successfully, otherwise one of the following 
        /// error codes is returned: <see cref="TimerActionResult.FailedTooManyTimers"/> when the maximum number of timers is reached or 
        /// <see cref="TimerActionResult.FailedInternalError"/> if an error occurred during the method call.</returns>
        public abstract Task<TimerActionResult> StartTimerAsync(string timerName, TimeSpan interval, TimerType type, 
                                                                TimerAsyncHandler<TDigitalTwin> timerCallback);

        /// <summary>
        /// Stops the specified timer.
        /// </summary>
        /// <param name="timerName">The timer name.</param>
        /// <returns><see cref="TimerActionResult.Success"/> if the timer was stopped successfully, otherwise one of the following 
        /// error codes is returned: <see cref="TimerActionResult.FailedNoSuchTimer"/> when the specified timer was not found or 
        /// <see cref="TimerActionResult.FailedInternalError"/> if an error occurred during the method call.</returns>
        public abstract Task<TimerActionResult> StopTimerAsync(string timerName);

        /// <summary>
        /// Returns the <see cref="ISimulationController"/> interface to control
        /// all aspects of model simulation.
        /// </summary>
        /// <returns>Get the <see cref="ISimulationController"/> interface.</returns>
        public abstract ISimulationController SimulationController { get; }

        /// <summary>
        /// Returns the current time in UTC. If model simulation is active the method
		/// returns the current simulation time, otherwise it returns the current UTC
		/// time.
        /// </summary>
        /// <returns>The current time - either the current system time or the current
		/// simulation time if simulation process is active.</returns>
        public abstract DateTimeOffset GetCurrentTime();

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

        /// <summary>
        /// Deletes a real-time twin instance.
        /// </summary>
        /// <param name="targetTwinModel">Digital twin model name.</param>
        /// <param name="targetTwinId">Digital twin identifier.</param>
        /// <returns><see cref="DeleteResult"/> indicating the result of the delete operation.</returns>
        public abstract Task<DeleteResult> RemoveRealTimeTwinAsync(string targetTwinModel, string targetTwinId);
    }
}
