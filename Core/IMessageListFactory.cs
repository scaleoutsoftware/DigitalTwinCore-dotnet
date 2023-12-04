/*
  IMessageListFactory.cs
  
  Copyright (C), 2018-2021 by ScaleOut Software, Inc.

  PROPRIETARY TRADE SECRET INFORMATION OF SCALEOUT SOFTWARE, INC.

  The information contained in this file is a trade secret of ScaleOut Software, Inc.
  and is not to be disclosed or copied in any form without written permission.
*/
using System.Collections.Generic;

namespace Scaleout.Streaming.DigitalTwin.Core
{
	/// <summary>
	/// Creates collection of new messages received by a digital twin object.
	/// </summary>
	public interface IMessageListFactory
	{
		/// <summary>
		/// Returns an enumerable collection of new (incoming) messages as
		/// <see cref="IEnumerable{TMessage}"/>.
		/// </summary>
		/// <typeparam name="TMessage">User defined message/event class type.</typeparam>
		/// <returns>Collection of new messages to process.</returns>
		IEnumerable<TMessage> GetIncomingMessageList<TMessage>();
	}
}
