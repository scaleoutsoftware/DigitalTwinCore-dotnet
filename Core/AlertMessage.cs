/*
  AlertMessage.cs
  
  Copyright (C), 2018-2021 by ScaleOut Software, Inc.

  PROPRIETARY TRADE SECRET INFORMATION OF SCALEOUT SOFTWARE, INC.

  The information contained in this file is a trade secret of ScaleOut Software, Inc.
  and is not to be disclosed or copied in any form without written permission.
*/
using System;
using System.Collections.Generic;
using System.Text;

namespace Scaleout.Streaming.DigitalTwin.Core
{
    /// <summary>
    /// Holds the different types of Alerting providers supported in the system.
    /// </summary>
    public static class AlertProviderType
    {
        /// <summary>Send alerts to Slack.</summary>
        public const string Slack = "Slack";
        /// <summary>Send alerts to Splunk On-Call.</summary>
        public const string Splunk = "Splunk";
        /// <summary>Send alerts to Pager Duty.</summary>
        public const string PagerDuty = "PagerDuty";
    }

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
