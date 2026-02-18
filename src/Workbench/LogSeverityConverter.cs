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

using Microsoft.Extensions.Logging;

using Scaleout.Modules.Abstractions;

namespace Scaleout.DigitalTwin.Workbench
{
    internal static class LogSeverityConverter
    {
        /// <summary>
        /// Maps a ScaleOut Digital Twin APIs <see cref="AlertSeverity"/> 
        /// to a .NET <see cref="LogLevel"/>.
        /// </summary>
        /// <param name="severity"><see cref="AlertSeverity"/> value.</param>
        /// <returns>The corresponding <see cref="LogLevel"/> value.</returns>
        public static LogLevel ToLogLevel(this AlertSeverity severity)
        {
            switch (severity)
            {
                case AlertSeverity.None:
                    return LogLevel.None;
                case AlertSeverity.Info:
                    return LogLevel.Information;
                case AlertSeverity.Warning:
                    return LogLevel.Warning;
                case AlertSeverity.Error:
                    return LogLevel.Error;
                default:
                    return LogLevel.Information;
            }
        }
    }
}
