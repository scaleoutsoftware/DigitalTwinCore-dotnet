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

namespace Scaleout.Streaming.DigitalTwin.Core
{
    /// <summary>
    /// Defines a timer message for data exchange between the timer's object 
    /// expiration handler and the corresponding digital twin instance itself.
    /// </summary>
    public class DigitalTwinTimerMessage
    {
        /// <summary>
        /// Target digital twin model name.
        /// </summary>
        public string ModelName { get; set; }

        /// <summary>
        /// Target digital twin Id.
        /// </summary>
        public string TwinId { get; set; }

        /// <summary>
        /// Timer identifier (from 0 to 4).
        /// </summary>
        public int TimerId { get; set; }

        /// <summary>
        /// Timer name.
        /// </summary>
        public string TimerName { get; set; }

        /// <summary>
        /// Timer type.
        /// </summary>
        public TimerType TimerType { get; set; }

        /// <summary>
        /// The string representation of the <see cref="DigitalTwinTimerMessage"/>.
        /// </summary>
        /// <returns>String-formated object representation.</returns>
        public override string ToString()
        {
            return $"ModelName: {ModelName}, TwinId: {TwinId}, TimerId: {TimerId}, TimerType: {TimerType}";
        }
    }
}
