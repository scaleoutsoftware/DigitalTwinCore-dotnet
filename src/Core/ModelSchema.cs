/*
  ModelSchema.cs
  
  Copyright (C), 2018-2021 by ScaleOut Software, Inc.

  PROPRIETARY TRADE SECRET INFORMATION OF SCALEOUT SOFTWARE, INC.

  The information contained in this file is a trade secret of ScaleOut Software, Inc.
  and is not to be disclosed or copied in any form without written permission.
*/
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

namespace Scaleout.Streaming.DigitalTwin.Core
{
    /// <summary>
    /// This class defines the structure of the model.json file that
    /// needs to be included into a zip file before uploading the model to
    /// the Digital Twin Builder cloud service.
    /// </summary>
    public class ModelSchema
    {
        /// <summary>
        /// The name of the file assembly that contains digital twin model, message processor and message classes defined.
        /// </summary>
        [Required]
        [DataMember(Name = "assemblyName")]
        public string AssemblyName { get; set; }

        /// <summary>
        /// The .NET type as the name of the digital twin model class. The value must include the namespace.
        /// </summary>
        [DataMember(Name = "modelType")]
        public string ModelType { get; set; }

        /// <summary>
        /// The .NET type as the name for the message processor class. The value must include the namespace.
        /// </summary>
        [DataMember(Name = "messageProcessorType")]
        public string MessageProcessorType { get; set; }

        /// <summary>
        /// The .NET type as the name for the message class. The value must include the namespace.
        /// </summary>
        [DataMember(Name = "messageType")]
        public string messageType { get; set; }

    }
}
