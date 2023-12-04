/*
  DigitalTwinProcessingException.cs
  
  Copyright (C), 2018-2023 by ScaleOut Software, Inc.

  PROPRIETARY TRADE SECRET INFORMATION OF SCALEOUT SOFTWARE, INC.

  The information contained in this file is a trade secret of ScaleOut Software, Inc.
  and is not to be disclosed or copied in any form without written permission.
*/

using System;

namespace Scaleout.Streaming.DigitalTwin.Core.Exceptions
{
    /// <summary>
    /// Exception that contains an error occured while processing messages by a digital twin.
    /// </summary>
    public class DigitalTwinProcessingException : ExecutionEnvironmentException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public DigitalTwinProcessingException()
        {
        }

        /// <summary>
        /// Creates a DigitalTwinProcessingException with a message.
        /// </summary>
        /// <param name="message">The message associated with this exception.</param>
        public DigitalTwinProcessingException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a DigitalTwinProcessingException with a message an an inner exception.
        /// </summary>
        /// <param name="message">The message associated with this exception.</param>
        /// <param name="innerException">The inner exception.</param>
        public DigitalTwinProcessingException(String message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
