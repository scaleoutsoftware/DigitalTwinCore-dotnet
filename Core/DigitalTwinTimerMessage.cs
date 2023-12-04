/*
  DigitalTwinTimerMessage.cs
  
  Copyright (C), 2018-2023 by ScaleOut Software, Inc.

  PROPRIETARY TRADE SECRET INFORMATION OF SCALEOUT SOFTWARE, INC.

  The information contained in this file is a trade secret of ScaleOut Software, Inc.
  and is not to be disclosed or copied in any form without written permission.
*/

namespace Scaleout.Streaming.DigitalTwin.Core
{
    /// <summary>
    /// Defines a timer message for data exchange between the timer's object 
    /// expiration handler and the corresponding digital twin instance itself.
    /// </summary>
    public class DigitalTwinTimerMessage
    {
        /// <summary>
        /// Target digital twin model name.
        /// </summary>
        public string ModelName { get; set; }

        /// <summary>
        /// Target digital twin Id.
        /// </summary>
        public string TwinId { get; set; }

        /// <summary>
        /// Timer identifier (from 0 to 4).
        /// </summary>
        public int TimerId { get; set; }

        /// <summary>
        /// Timer name.
        /// </summary>
        public string TimerName { get; set; }

        /// <summary>
        /// Timer type.
        /// </summary>
        public TimerType TimerType { get; set; }

        /// <summary>
        /// The string representation of the <see cref="DigitalTwinTimerMessage"/>.
        /// </summary>
        /// <returns>String-formated object representation.</returns>
        public override string ToString()
        {
            return $"ModelName: {ModelName}, TwinId: {TwinId}, TimerId: {TimerId}, TimerType: {TimerType}";
        }
    }
}
