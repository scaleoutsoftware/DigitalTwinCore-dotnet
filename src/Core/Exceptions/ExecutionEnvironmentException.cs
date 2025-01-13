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
