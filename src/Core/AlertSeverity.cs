#region Copyright notice and license

// Copyright 2023-2026 ScaleOut Software, Inc.
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

namespace Scaleout.Modules.DigitalTwin.Abstractions
{
    /// <summary>
	/// Defines the severity levels for alert messages.
	/// </summary>
	public enum AlertSeverity
    {
        /// <summary>
        /// Indicates that a UI alert has an informational purpose.
        /// </summary>
        Info,

        /// <summary>
        /// Indicates that a UI alert is a module warning.
        /// </summary>
        Warning,

        /// <summary>
        /// Indicates that a UI alert is a module error.
        /// </summary>
        Error,

        /// <summary>
        /// Indicates that the alert should not be displayed in the UI.
        /// </summary>
        None
    }
}
