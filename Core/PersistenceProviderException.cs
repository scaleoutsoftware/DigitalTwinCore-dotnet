/*
  PersistenceProviderException.cs
  
  Copyright (C), 2018-2021 by ScaleOut Software, Inc.

  PROPRIETARY TRADE SECRET INFORMATION OF SCALEOUT SOFTWARE, INC.

  The information contained in this file is a trade secret of ScaleOut Software, Inc.
  and is not to be disclosed or copied in any form without written permission.
*/
using System;

namespace Scaleout.Streaming.DigitalTwin.Core.Exceptions
{
    /// <summary>
    /// An exception thrown by persistence providers.
    /// </summary>
    public class PersistenceProviderException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public PersistenceProviderException()
        {
        }

        /// <summary>
        /// Creates a <see cref="PersistenceProviderException"/> with a message.
        /// </summary>
        /// <param name="message">The message associated with this exception.</param>
        public PersistenceProviderException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a <see cref="PersistenceProviderException"/> with a message and 
        /// an inner exception.
        /// </summary>
        /// <param name="message">The message associated with this exception.</param>
        /// <param name="innerException">The inner exception.</param>
        public PersistenceProviderException(String message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
