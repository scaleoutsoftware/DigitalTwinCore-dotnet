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
using Scaleout.Streaming.DigitalTwin.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Scaleout.DigitalTwin.Workbench
{
    internal static class LogSeverityConverter
    {
        /// <summary>
        /// Maps a ScaleOut Digital Twin APIs <see cref="LogSeverity"/> 
        /// to a .NET <see cref="LogLevel"/>.
        /// </summary>
        /// <param name="severity"><see cref="LogSeverity"/> value.</param>
        /// <returns>The corresponding <see cref="LogLevel"/> value.</returns>
        public static LogLevel ToLogLevel(this LogSeverity severity)
        {
            switch (severity)
            {
                case LogSeverity.None:
                    return LogLevel.None;
                case LogSeverity.Verbose:
                    return LogLevel.Debug;
                case LogSeverity.Informational:
                    return LogLevel.Information;
                case LogSeverity.Warning:
                    return LogLevel.Warning;
                case LogSeverity.Error:
                    return LogLevel.Error;
                case LogSeverity.Critical:
                    return LogLevel.Critical;
                default:
                    return LogLevel.Information;
            }
        }
    }
}
