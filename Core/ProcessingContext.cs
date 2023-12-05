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

namespace Scaleout.Streaming.DigitalTwin.Core
{
	/// <summary>
	/// Message processing context for currently processing messages.
	/// It allows sending messages back to a data source where the messages 
	/// are originated from.
	/// </summary>
	public abstract class ProcessingContext
	{
		/// <summary>
		/// Data source unique identifier, e.g. for IoT use case it is typically device Id,
		/// which is also used as a digital twin Id.
		/// </summary>
		public abstract string DataSourceId { get; }

		/// <summary>
		/// Digital twin model type.
		/// </summary>
		public abstract string DigitalTwinModel { get; }

		/// <summary>
		/// Sends a message back to a data source origination point (e.g. IoT device). When sending 
		/// a message to the ScaleOut Messaging REST service as a data source, the message content
		/// must be JSON encoded.
		/// </summary>
		/// <param name="message">JSON encoded message as <see cref="T:byte[]" />.</param>
		/// <returns><see cref="SendingResult.Enqueued"/> when message was successfully enqueued,
		/// <see cref="SendingResult.NotHandled"/> otherwise.</returns>
		public abstract SendingResult SendToDataSource(byte[] message);

        /// <summary>
        /// Sends a message back to a data source origination point (e.g. IoT device). When sending 
        /// a message to the ScaleOut Messaging REST service as a data source, the message content
        /// must be JSON encoded.
        /// </summary>
        /// <param name="message">Message object to be encoded as JSON.</param>
        /// <returns><see cref="SendingResult.Enqueued"/> when message was successfully enqueued,
        /// <see cref="SendingResult.NotHandled"/> otherwise.</returns>
        public abstract SendingResult SendToDataSource(object message);

        /// <summary>
        /// Sends a list of messages back to a data source origination point (e.g. IoT device).
        /// When sending messages to the ScaleOut Messaging REST service as a data source, each message
        /// must be JSON encoded.
        /// </summary>
        /// <param name="messages">JSON encoded messages as a list of <see cref="T:byte[]" />.</param>
        /// <returns><see cref="SendingResult.Enqueued"/> when messages were successfully enqueued,
        /// <see cref="SendingResult.NotHandled"/> otherwise.</returns>
        public abstract SendingResult SendToDataSource(IEnumerable<byte[]> messages);

        /// <summary>
        /// Sends a list of messages back to a data source origination point (e.g. IoT device).
        /// When sending messages to the ScaleOut Messaging REST service as a data source, each message
        /// must be JSON encoded.
        /// </summary>
        /// <param name="messages">Message object to be encoded as JSON.</param>
        /// <returns><see cref="SendingResult.Enqueued"/> when messages were successfully enqueued,
        /// <see cref="SendingResult.NotHandled"/> otherwise.</returns>
        public abstract SendingResult SendToDataSource(IEnumerable<object> messages);

        /// <summary>
        /// Sends message to a different digital twin in twin's hierarchy.
        /// </summary>
        /// <param name="targetTwinModel">Digital twin model type.</param>
        /// <param name="targetTwinId">Digital twin identifier.</param>
        /// <param name="message">JSON encoded message as <see cref="T:byte[]" />.</param>
        /// <returns><see cref="SendingResult.Enqueued"/> when message was successfully enqueued,
        /// <see cref="SendingResult.NotHandled"/> otherwise.</returns>
        public abstract SendingResult SendToTwin(string targetTwinModel, string targetTwinId, byte[] message);

        /// <summary>
        /// Sends message to a different digital twin in twin's hierarchy.
        /// </summary>
        /// <param name="targetTwinModel">Digital twin model type.</param>
        /// <param name="targetTwinId">Digital twin identifier.</param>
        /// <param name="message">Message object to be encoded as JSON.</param>
        /// <returns><see cref="SendingResult.Enqueued"/> when message was successfully enqueued,
        /// <see cref="SendingResult.NotHandled"/> otherwise.</returns>
        public abstract SendingResult SendToTwin(string targetTwinModel, string targetTwinId, object message);

        /// <summary>
        /// Sends a list of messages to a different digital twin in twin's hierarchy.
        /// </summary>
        /// <param name="targetTwinModel">Digital twin model type.</param>
        /// <param name="targetTwinId">Digital twin identifier.</param>
        /// <param name="messages">JSON encoded messages as a list of <see cref="T:byte[]" />.</param>
        /// <returns><see cref="SendingResult.Enqueued"/> when messages were successfully enqueued,
        /// <see cref="SendingResult.NotHandled"/> otherwise.</returns>
        public abstract SendingResult SendToTwin(string targetTwinModel, string targetTwinId, IEnumerable<byte[]> messages);

        /// <summary>
        /// Sends a list of messages to a different digital twin in twin's hierarchy.
        /// </summary>
        /// <param name="targetTwinModel">Digital twin model type.</param>
        /// <param name="targetTwinId">Digital twin identifier.</param>
        /// <param name="messages">Messages to be encoded as JSON.</param>
        /// <returns><see cref="SendingResult.Enqueued"/> when messages were successfully enqueued,
        /// <see cref="SendingResult.NotHandled"/> otherwise.</returns>
        public abstract SendingResult SendToTwin(string targetTwinModel, string targetTwinId, IEnumerable<object> messages);


        /// <summary>
        /// Logs user message with the specified severity level.
        /// </summary>
        /// <param name="severity">The severity level for the specified message.</param>
        /// <param name="message">The user message to log.</param>
        public abstract void LogMessage(LogSeverity severity, string message);

		/// <summary>
		/// Sends an alert to a given Alerting provider. 
		/// </summary>
		/// <param name="providerName">The name of the configuration of an Alerting Provider to send the alert to.</param>
		/// <param name="alertMessage">The provided object contains information about the alert data, as well as the provider to target.</param>
		/// <returns></returns>
		public abstract SendingResult SendAlert(string providerName, AlertMessage alertMessage);

        /// <summary>
        /// Returns the reference to registered persistence provider the model is using.
        /// The name of the registered persistence provider should be specified in the model's
        /// appsettings.json file via the PersistenceProvider key. Only Azure Digital Twins Service 
		/// provider is supported.
        /// </summary>
        /// <returns>The registered <see cref="IPersistenceProvider"/> used by the model.</returns>
        public abstract IPersistenceProvider PersistenceProvider { get; }

		/// <summary>
		/// Starts a new timer for the digital twin <see cref="DataSourceId"/>.
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
		/// Stops the specified timer.
		/// </summary>
		/// <param name="timerName">The timer name.</param>
		/// <returns><see cref="TimerActionResult.Success"/> if the timer was stopped successfully, otherwise one of the following 
		/// error codes is returned: <see cref="TimerActionResult.FailedNoSuchTimer"/> when the specified timer was not found or 
		/// <see cref="TimerActionResult.FailedInternalError"/> if an error occurred during the method call.</returns>
		public abstract TimerActionResult StopTimer(string timerName);

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
    }
}
