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

using System.Collections.Generic;

namespace Scaleout.Streaming.DigitalTwin.Core
{
	/// <summary>
	/// Interface that must be implemented by a message sender class for sending
	/// messages/events to a digital twin.
	/// </summary>
	public interface IMessageSender
	{
		/// <summary>
		/// Application namespace used to identify the message source (i.e., a data source connector or a digital twin object).
		/// </summary>
		uint SourceAppId { get; }

        /// <summary>
        /// Sends serialized messages to a digital twin object located in the data grid or
		/// IoT device that a digital twin represents.
        /// </summary>
        /// <param name="digitalTwinId">Digital twin identifier.</param>
        /// <param name="messageInfo">JSON-encoded message info specifying the data source Id, target, and source digital twin model types.</param>
        /// <param name="messages">The list of serialized JSON-encoded event messages to send.</param>
        /// <returns><see cref="SendingResult.Handled"/> when message was successfully sent,
        /// <see cref="SendingResult.NotHandled"/> otherwise.</returns>
        SendingResult Send(string digitalTwinId, string messageInfo, IEnumerable<byte[]> messages);

        /// <summary>
        /// Sends a serialized message to a digital twin object located in the data grid or
		/// IoT device that a digital twin represents.
        /// </summary>
        /// <param name="digitalTwinId">Digital twin identifier.</param>
        /// <param name="messageInfo">JSON-encoded message info specifying the data source Id, target, and source digital twin model types.</param>
        /// <param name="message">The serialized JSON-encoded event message to send.</param>
        /// <returns><see cref="SendingResult.Handled"/> when message was successfully sent,
        /// <see cref="SendingResult.NotHandled"/> otherwise.</returns>
        SendingResult Send(string digitalTwinId, string messageInfo, byte[] message);
    }
}