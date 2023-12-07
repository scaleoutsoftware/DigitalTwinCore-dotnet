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
	/// Abstract base class that must be subclassed to implement the message processor which
	/// processes incoming messages for a digital twin object.
	/// </summary>
	/// <typeparam name="TDigitalTwin">User-defined type for a digital twin object.</typeparam>
	/// <typeparam name="TMessage">User-defined type for incoming messages.</typeparam>
	public abstract class MessageProcessor<TDigitalTwin, TMessage> : MessageProcessor where TDigitalTwin: class
	{
		/// <summary>
		/// This method is called by ScaleOut StreamServer to pass new messages
		/// to the specified digital twin object.
		/// </summary>
		/// <param name="context">The digital twin message processing context.</param>
		/// <param name="digitalTwin">The target digital twin object.</param>
		/// <param name="newMessages">New messages to process.</param>
		/// <returns><see cref="ProcessingResult.DoUpdate"/> when the digital twin
		/// object and the list of processed messages need to be updated and <see cref="ProcessingResult.NoUpdate"/> when
		/// no updates are needed.</returns>
		public abstract ProcessingResult ProcessMessages(ProcessingContext context, TDigitalTwin digitalTwin, IEnumerable<TMessage> newMessages);

		/// <summary>
		/// Internal method called by ScaleOut Digital Twin library for passing 
		/// the digital twin (state) object together with lists of processed and new messages
		/// (type of <typeref name="TMessage"/>) into a user-defined message processor class.
		/// </summary>
		/// <param name="context">The digital twin message processing context.</param>
		/// <param name="digitalTwin">The target digital twin object.</param>
		/// <param name="messageListFactory">The message list factory which generates message lists of a user-defined type.
		/// <typeref name="TMessage"/>.</param>
		/// <returns><see cref="ProcessingResult.DoUpdate"/> when digital twin
		/// object and the list of processed messages need to be updated and <see cref="ProcessingResult.NoUpdate"/> when
		/// no updates are needed.</returns>
		internal override ProcessingResult ProcessMessages(ProcessingContext context, DigitalTwinBase digitalTwin, IMessageListFactory messageListFactory)
		{
			IEnumerable<TMessage> newMessages	= messageListFactory.GetIncomingMessageList<TMessage>();

			return ProcessMessages(context, digitalTwin as TDigitalTwin, newMessages);
		}

		/// <inheritdoc/>
		internal override Type DigitalTwinModelType { get => typeof(TDigitalTwin); }

		/// <inheritdoc/>
		internal override Type MessageType { get => typeof(TMessage); }
	}

	/// <summary>
	/// The top level abstract base class used by ScaleOut Digital Twin Library.
	/// </summary>
	public abstract class MessageProcessor
	{
		internal abstract ProcessingResult ProcessMessages(ProcessingContext context, DigitalTwinBase digitalTwin, IMessageListFactory messageListFactory);

		/// <summary>
		/// Gets the user-defined type for a digital twin model associated with this
		/// MessageProcessor.
		/// </summary>
		internal abstract Type DigitalTwinModelType { get; }

		/// <summary>
		/// Gets the user-defined type for a messages that are handled by this
		/// MessageProcessor.
		/// </summary>
		internal abstract Type MessageType { get; }
	}
}
