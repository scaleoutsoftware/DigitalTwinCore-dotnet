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

namespace Scaleout.Streaming.DigitalTwin.Core
{
    /// <summary>
    /// Specifies whether or not a Digital Twin should be updated after a call to a
	/// <see cref="MessageProcessor{TDigitalTwin, TMessage}.ProcessMessages(ProcessingContext, TDigitalTwin, System.Collections.Generic.IEnumerable{TMessage})"/>
	/// implementation has returned.
    /// </summary>
    public enum ProcessingResult
	{
		/// <summary>
		/// The digital twin object has been modified and must be updated in the ScaleOut service.
		/// </summary>
		DoUpdate,

        /// <summary>
        /// The digital twin object was not modified and does not need to be updated in the ScaleOut service.
        /// </summary>
        NoUpdate
    }

	/// <summary>
	/// Indicates the status of a <see cref="IMessageSender.Send(string, string, System.Collections.Generic.IEnumerable{byte[]})"/> 
	/// operation when sending messages to or from a digital twin object.
	/// </summary>
	public enum SendingResult
	{
		/// <summary>
		/// The messages were sent and processed successfully.
		/// </summary>
		Handled,

		/// <summary>
		/// An error occurred while sending or processing messages by a digital twin object or data source.
		/// </summary>
		NotHandled,

		/// <summary>
		/// The messages were successfully enqueued for delivery.
		/// </summary>
		Enqueued
	}

	/// <summary>
	/// Defines the severity levels for logging messages.
	/// </summary>
	public enum LogSeverity
	{
		/// <summary>
		/// Used for logging of lengthy messages.
		/// </summary>
		Verbose,

		/// <summary>
		/// Indicates that log messages with that level have informational purpose.
		/// </summary>
		Informational,

		/// <summary>
		/// Indicates that log messages with that level are application warnings.
		/// </summary>
		Warning,

		/// <summary>
		/// Indicates that log messages with that level should be treated as an application error.
		/// </summary>
		Error,

		/// <summary>
		/// The message corresponds to a critical error that has caused a major failure that 
		/// requires immediate attention.
		/// </summary>
		Critical,

		/// <summary>
		/// Indicates that log messages with that level should not be stored (e.g. temporarily 
		/// based on some digital twin configuration setting).
		/// </summary>
		None
	}

	/// <summary>
	/// Defines the type of timer that can be created by <see cref="ProcessingContext.StartTimer(string, System.TimeSpan, TimerType, TimerHandler)"/>.
	/// </summary>
	public enum TimerType
	{
		/// <summary>The timer is fired periodically with the specified interval 
		/// until the <see cref="ProcessingContext.StopTimer(string)"/> is called.</summary>
		Recurring,

		/// <summary>The timer is fired once after the specified time interval is elapsed.</summary>
		OneTime
	}

	/// <summary>
	/// Defines result codes for timer methods.
	/// </summary>
	public enum TimerActionResult
	{
		/// <summary>The operation completed successfully.</summary>
		Success = 0,

		/// <summary>Failed to start a new timer due to reaching the limit for a number of active timers.</summary>
		FailedTooManyTimers = 1,

		/// <summary>Failed to stop the existing timer: the timer is no longer active.</summary>
		FailedNoSuchTimer = 2,

		/// <summary>Failed to start the timer: the timer with the specified name already exists.</summary>
		FailedTimerAlreadyExists = 3,

		/// <summary>Failed to start/stop timer due to an internal error.</summary>
		FailedInternalError = 4
	}
}
