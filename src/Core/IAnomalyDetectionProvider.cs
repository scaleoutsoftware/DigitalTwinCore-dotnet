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
