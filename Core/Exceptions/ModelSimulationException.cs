/*
  ModelSimulationException.cs
  
  Copyright (C), 2018-2023 by ScaleOut Software, Inc.

  PROPRIETARY TRADE SECRET INFORMATION OF SCALEOUT SOFTWARE, INC.

  The information contained in this file is a trade secret of ScaleOut Software, Inc.
  and is not to be disclosed or copied in any form without written permission.
*/
using System;

namespace Scaleout.Streaming.DigitalTwin.Core.Exceptions
{
    /// <summary>
    /// The ModelSimulationException is raised when an error condition 
    /// encountered while running the model simulation.
    /// </summary>
    public class ModelSimulationException : Exception
    {
        /// <summary>
        /// Public constructor.
        /// </summary>
        public ModelSimulationException() { }

        /// <summary>
        /// Creates a ModelSimulationException with a message.
        /// </summary>
        /// <param name="message">The message associated with this exception.</param>
        public ModelSimulationException(string message) : base(message) { }

        /// <summary>
        /// Creates a ModelSimulationException with a message and an inner exception.
        /// </summary>
        /// <param name="message">The message associated with this exception.</param>
        /// <param name="innerException">The inner exception.</param>
        public ModelSimulationException(String message, Exception innerException) : base(message, innerException) { }
    }
}
