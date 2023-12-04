/*
  MessageProcessorInstantiationException.cs
  
  Copyright (C), 2018-2023 by ScaleOut Software, Inc.

  PROPRIETARY TRADE SECRET INFORMATION OF SCALEOUT SOFTWARE, INC.

  The information contained in this file is a trade secret of ScaleOut Software, Inc.
  and is not to be disclosed or copied in any form without written permission.
*/
using System;

namespace Scaleout.Streaming.DigitalTwin.Core.Exceptions
{
    /// <summary>
    /// The MessageProcessorInstantiationException is used to detect if a messages processor is unable to be instantiated by the 
    /// DigitalTwin service.
    /// </summary>
    public class MessageProcessorInstantiationException : DigitalTwinConfigurationException
    {
        /// <summary>
        /// Creates a MessageProcessorInstantiationException
        /// </summary>
        public MessageProcessorInstantiationException()
        {
        }

        /// <summary>
        /// Creates a MessageProcessorInstantiationException with a message.
        /// </summary>
        /// <param name="message">The message associated with this exception</param>
        public MessageProcessorInstantiationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a MessageProcessorInstantiationException with a message and an inner exception
        /// </summary>
        /// <param name="message">the messages associated with this exception </param>
        /// <param name="innerException">the inner exception</param>
        public MessageProcessorInstantiationException(String message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
