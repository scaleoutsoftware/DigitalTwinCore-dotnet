/*
  MessageInstantiationException.cs
  
  Copyright (C), 2018-2023 by ScaleOut Software, Inc.

  PROPRIETARY TRADE SECRET INFORMATION OF SCALEOUT SOFTWARE, INC.

  The information contained in this file is a trade secret of ScaleOut Software, Inc.
  and is not to be disclosed or copied in any form without written permission.
*/
using System;

namespace Scaleout.Streaming.DigitalTwin.Core.Exceptions
{
    /// <summary>
    /// The MessageInstantiationException is used to detect if a Message is unable to be instantiated by the 
    /// DigitalTwin service.
    /// </summary>
    public class MessageInstantiationException : DigitalTwinConfigurationException
    {
        /// <summary>
        /// Creates a MessageInstantiationException
        /// </summary>
        public MessageInstantiationException()
        {
        }

        /// <summary>
        /// Creates a MessageInstantiationException with a message.
        /// </summary>
        /// <param name="message">the message associated with this exception</param>
        public MessageInstantiationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a MessageInstantiationException with a message and inner exception.
        /// </summary>
        /// <param name="message">the message associate with this exception</param>
        /// <param name="innerException">the inner exception</param>
        public MessageInstantiationException(String message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
