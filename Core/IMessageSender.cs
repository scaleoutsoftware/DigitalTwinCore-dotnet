/*
  IMessageSender.cs
  
  Copyright (C), 2018-2021 by ScaleOut Software, Inc.

  PROPRIETARY TRADE SECRET INFORMATION OF SCALEOUT SOFTWARE, INC.

  The information contained in this file is a trade secret of ScaleOut Software, Inc.
  and is not to be disclosed or copied in any form without written permission.
*/
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