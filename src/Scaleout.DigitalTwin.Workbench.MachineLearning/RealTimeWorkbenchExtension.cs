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

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Scaleout.Streaming.DigitalTwin.Common;
using Scaleout.Modules.DigitalTwin.Abstractions.Exceptions;
using System;
using System.IO.Compression;
using System.IO;
using System.Linq;
using Scaleout.Streaming.DigitalTwin.MachineLearning;
using Scaleout.Modules.DigitalTwin.Abstractions;
using System.Collections.Generic;

namespace Scaleout.DigitalTwin.Workbench.MachineLearning
{
    /// <summary>
    /// This is an extension of the RealTimeWorkbench specific to Machine Learning Algorithms.
    /// It adds support for Anomaly Detection Providers built using the ScaleOut Machine Learning Training Tool
    /// </summary>
    public static class RealTimeWorkbenchExtension
    {
        /// <summary>
        /// This reads the Zip file produced by the ScaleOut Machine Learning Training Tool to create a new instance
        /// of IAnomalyDetectionProvider so that the RealTimeProcessingContext has access to it. It allows digital
        /// twin instances to call the DetectAnomaly API through the processing context.
        /// </summary>
        /// <param name="wb">The workbench we are adding this provider to</param>
        /// <param name="dtModelName">The name of the Digital Twin model that this provider is used by</param>
        /// <param name="referenceName">The reference name of the Anomaly Detection Provider</param>
        /// <param name="pathToZip">The path to the zip file produced by the ScaleOut Machine Learning Training Tool</param>
        /// <param name="logger">A logger for the application</param>
        /// <exception cref="DigitalTwinInstantiationException"></exception>
        public static void AddAnomalyDetectionProvider(this RealTimeWorkbench wb, string dtModelName, string referenceName, string pathToZip, ILogger logger)
        {
            if (logger != null)
            {
                logger.LogInformation($"Ready to add Anomaly Detection Provider for model {dtModelName}, named '{referenceName}', using the zip file: {pathToZip}");
            }

            try
            {
                using Stream stream = File.OpenRead(pathToZip);
                var anomalyDetectionManager = new AnomalyDetectionManager(dtModelName, referenceName, stream, logger);

                if (!wb.AnomalyDetectionProviders.ContainsKey(dtModelName))
                {
                    wb.AnomalyDetectionProviders.Add(dtModelName, new Dictionary<string, IAnomalyDetectionProvider>());
                }

                if (wb.AnomalyDetectionProviders[dtModelName] == null)
                {
                    wb.AnomalyDetectionProviders[dtModelName] = new Dictionary<string, IAnomalyDetectionProvider>();
                }

                wb.AnomalyDetectionProviders[dtModelName][referenceName] = (IAnomalyDetectionProvider)anomalyDetectionManager;
                if (logger != null)
                    logger.LogInformation($"Anomaly Detection Provider {referenceName} has been successfully added to the workbench for the digital twin model: {dtModelName}.");
            }
            catch (Exception ex)
            {
                if (logger != null)
                    logger.LogError(ex.Message);
            }
            
        }
    }
}
