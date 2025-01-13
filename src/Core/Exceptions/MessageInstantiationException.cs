#region Copyright notice and license

// Copyright 2023-2025 ScaleOut Software, Inc.
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
