/*
  DigitalTwinInstantiationException.cs
  
  Copyright (C), 2018-2023 by ScaleOut Software, Inc.

  PROPRIETARY TRADE SECRET INFORMATION OF SCALEOUT SOFTWARE, INC.

  The information contained in this file is a trade secret of ScaleOut Software, Inc.
  and is not to be disclosed or copied in any form without written permission.
*/
using System;

namespace Scaleout.Streaming.DigitalTwin.Core.Exceptions
{
    /// <summary>
    /// Exception indicating that a digital twin is unable to be instantiated by the 
    /// DigitalTwin service.
    /// </summary>
    public class DigitalTwinInstantiationException : DigitalTwinConfigurationException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public DigitalTwinInstantiationException()
        {
        }

        /// <summary>
        /// Creates a DigitalTwinInstantiationException with a message.
        /// </summary>
        /// <param name="message">The message associated with this exception</param>
        public DigitalTwinInstantiationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a DigitalTwinInstantiationException with a message an an inner exception.
        /// </summary>
        /// <param name="message">The message associated with this exception</param>
        /// <param name="innerException">The inner exception</param>
        public DigitalTwinInstantiationException(String message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
