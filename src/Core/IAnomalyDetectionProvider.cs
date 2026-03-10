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

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Scaleout.Modules.DigitalTwin.Abstractions
{
    /// <summary>
    /// Encapsulates the capabilities of a ScaleOut real-time digital twin 
    /// anomaly detection provider.
    /// </summary>
    public interface IAnomalyDetectionProvider
    {
        /// <summary>
        /// Detects anomalies by using the trained algorithm and the provided property values
        /// </summary>
        /// <param name="properties">A dictionary of the properties to use for the prediction</param>
        /// <returns>True if an anomaly is detected, False otherwise</returns>
        bool DetectAnomaly(Dictionary<string, float> properties);

        /// <summary>
        /// Obtain a prediction score (between 0 and 1) by using the trained algorithm and the
        /// provided property values.
        /// </summary>
        /// <param name="properties">A dictionary of the properties to use for the prediction</param>
        /// <returns>A number between 0 and 1: the closer to 1, the higher the chance an anomaly is detected.</returns>
        float GetPredictionScore(Dictionary<string, float> properties);

        /// <summary>
        /// Add anomaly data. Used for retraining ML algorithms, anomaly data consists in
        /// a collection of properties and their associated values, along with a label
        /// to indicate whether these properties constitute an anomaly.
        /// </summary>
        /// <param name="properties">A collection of properties and their values</param>
        /// <param name="isAnomaly">True if the values constitute an anomaly</param>
        Task ReportAnomalyDataAsync(Dictionary<string, float> properties, bool isAnomaly);
    }
}
