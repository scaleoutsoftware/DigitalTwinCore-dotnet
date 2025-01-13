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
using System.Collections.Generic;
using System.Text;

namespace Scaleout.Streaming.DigitalTwin.Core
{
    /// <summary>
    /// Encapsulates the capabilities of a ScaleOut real-time digital twin 
    /// anomaly detection provider (based on ML.NET libraries).
    /// </summary>
    public interface IAnomalyDetectionProvider
    {
        /// <summary>
        /// Detects anomalies by using the trained algorithm and the provided property values
        /// </summary>
        /// <param name="properties">A dictionary of the properties to use for the prediction</param>
        /// <returns>True if an anomaly is detected, False otherwise</returns>
        bool DetectAnomaly(Dictionary<string, float> properties);
    }
}
