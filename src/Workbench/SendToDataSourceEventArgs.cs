using System;
using System.Collections.Generic;
using System.Text;

namespace Scaleout.DigitalTwin.Workbench
{
    /// <summary>
    /// 
    /// </summary>
    public class SendToDataSourceEventArgs : EventArgs
    {
        internal SendToDataSourceEventArgs(string digitalTwinId, string modelName, object message)
        {
            DigitalTwinId = digitalTwinId;
            Message = message;
            ModelName = modelName;
        }
        /// <summary>
        /// Message sent to the digital twin's data source.
        /// </summary>
        public object Message { get; set; }

        /// <summary>
        /// ID of the digital twin instance.
        /// </summary>
        public string DigitalTwinId { get; set; }


        /// <summary>
        /// Name of the model.
        /// </summary>
        public string ModelName { get; set; }
    }
}
