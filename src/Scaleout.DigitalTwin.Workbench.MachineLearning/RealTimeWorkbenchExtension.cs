#region Copyright notice and license

// Copyright 2023-2024 ScaleOut Software, Inc.
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
using Scaleout.Streaming.DigitalTwin.Core.Exceptions;
using System;
using System.IO.Compression;
using System.IO;
using System.Linq;
using Scaleout.Streaming.DigitalTwin.MachineLearning;
using Scaleout.Streaming.DigitalTwin.Core;
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
            using ZipArchive archive = ZipFile.Open(pathToZip, ZipArchiveMode.Read);

            // In the provided Zip file, there should be a JSON file for the metadata
            // and a Zip file for the trained algorithm
            var metadataFile = archive.Entries.FirstOrDefault(e => String.Compare(Path.GetExtension(e.Name), ".json", StringComparison.InvariantCultureIgnoreCase) == 0);
            if (metadataFile == null)
            {
                if (logger != null)
                    logger.LogError("The Digital Twin model includes a machine learning algorithm but it is missing metadata.");
                return;
            }

            // Read the appsettings file to find potential trained ML algorithms
            string metadata;
            using Stream zipStream = metadataFile.Open();
            using (var sr = new StreamReader(zipStream))
            {
                metadata = sr.ReadToEnd();
            }
            if (String.IsNullOrEmpty(metadata))
            {
                if (logger != null)
                    logger.LogError("The Digital Twin model's metadata file is empty. The Machine Learning algorithm will be deployed.");
                return;
            }

            MachineLearningTrainedAlgorithmInfo? algo = JsonConvert.DeserializeObject<MachineLearningTrainedAlgorithmInfo>(metadata);
            if (algo == null)
            {
                if (logger != null)
                    logger.LogError("The Digital Twin model's metadata file does not contain Machine Learning algorithm info. No Machine Learning algorithm will be deployed.");
                return;
            }

            // The DT model name is decided by the user at the time of deployment. The metadata from the zip is what is created
            // by the ML Training app and is based on the class name. It might not be the same, so we update the metadata before
            // adding it into the cache with the value we received from the Middle-Tier (passed as a param to this function).
            // It matters because we use the model name as part of the key for metadata and trained algorithms.
            algo.DTModelName = dtModelName;

            string zipFileName = $"{Path.GetFileNameWithoutExtension(metadataFile.FullName)}.zip";
            var zipFile = archive.Entries.FirstOrDefault(e => e.FullName == zipFileName);
            if (zipFile == null)
            {
                if (logger != null)
                    logger.LogError($"The model refers to a machine learning algorithm exported as {zipFileName}, but the zip file does not contain such file.");
                return;
            }

            // Open a stream so we can initialize the provider
            using Stream mlZipStream = zipFile.Open();
            using var ms = new MemoryStream();
            mlZipStream.CopyTo(ms);

            AnomalyDetectionManager anomalyDetectionManager = new AnomalyDetectionManager();
            anomalyDetectionManager.Initialize(ms, algo.ColumnMappings);

            if (!wb.AnomalyDetectionProviders.ContainsKey(dtModelName))
            {
                wb.AnomalyDetectionProviders.Add(dtModelName, new Dictionary<string, IAnomalyDetectionProvider>());
            }

            if (wb.AnomalyDetectionProviders[dtModelName] == null)
            {
                wb.AnomalyDetectionProviders[dtModelName] = new Dictionary<string, IAnomalyDetectionProvider>();
            }

            wb.AnomalyDetectionProviders[dtModelName][referenceName] = anomalyDetectionManager;
            if (logger != null)
                logger.LogInformation($"Anomaly Detection Provider {referenceName} has been successfully added to the workbench for the digital twin model: {dtModelName}.");
        }
    }
}
