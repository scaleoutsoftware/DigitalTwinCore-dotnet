#region Copyright notice and license

// Copyright 2023 ScaleOut Software, Inc.
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

namespace Scaleout.Streaming.DigitalTwin.Core
{
    /// <summary>
    /// Used by the <see cref="DigitalTwinBase"/> class to store
    /// information about a timer associated with a Digital Twin instance.
    /// </summary>
    [Serializable]
    sealed public class TimerMetadata
    {
        /// <summary>The timer Id.</summary>
        public int Id { get; set; }

        /// <summary>The timer interval.</summary>
        public TimeSpan Interval { get; set; }

        /// <summary>The timer type.</summary>
        public TimerType Type { get; set; }

        /// <summary>The timer handler. Only assign a public static method or 
        /// a class instance method to this property.</summary>
        public TimerHandler TimerHandler { get; set; }
    }
}
