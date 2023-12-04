/*
  ExecutionEnvironmentException.cs
  
  Copyright (C), 2018-2023 by ScaleOut Software, Inc.

  PROPRIETARY TRADE SECRET INFORMATION OF SCALEOUT SOFTWARE, INC.

  The information contained in this file is a trade secret of ScaleOut Software, Inc.
  and is not to be disclosed or copied in any form without written permission.
*/
using System;

namespace Scaleout.Streaming.DigitalTwin.Core.Exceptions
{
    /// <summary>
    /// Exception indicating that an error occurred when establishing the execution environment.
    /// </summary>
    public class ExecutionEnvironmentException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ExecutionEnvironmentException()
        {
        }

        /// <summary>
        /// Creates a ExecutionEnvironmentException with a message.
        /// </summary>
        /// <param name="message">The message associated with this exception.</param>
        public ExecutionEnvironmentException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a ExecutionEnvironmentException with a message and an inner exception.
        /// </summary>
        /// <param name="message">The message associated with this exception.</param>
        /// <param name="innerException">The inner exception.</param>
        public ExecutionEnvironmentException(String message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
