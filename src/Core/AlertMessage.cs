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
using System.Collections.Generic;
using System.Text;

namespace Scaleout.Streaming.DigitalTwin.Core
{
    /// <summary>
    /// The AlertMessage objects contain all the data to send alerts to external services.
    /// This includes properties such as title, message and severity. Finally, alerts can include snapshots of instance
    /// properties along with the message.
    /// </summary>
    public class AlertMessage
    {
        /// <summary>
        /// Title of the alert.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Severity of the alert. Stored as a string since different providers use different severity names.
        /// </summary>
        public string Severity { get; set; }

        /// <summary>
        /// A more descriptive message about the alert.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Just like events, alerts can include snapshots of instance properties.
        /// </summary>
        public Dictionary<string, string> OptionalTwinInstanceProperties { get; set; } = new Dictionary<string, string>();
    }
}
