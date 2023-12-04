using System;

namespace Scaleout.Streaming.DigitalTwin.Core
{
    /// <summary>
    /// Used by the <see cref="DigitalTwinBase"/> class to store
    /// information about a timer associated with a Digital Twin instance.
    /// </summary>
    [Serializable]
    sealed public class TimerMetadata
    {
        /// <summary>The timer Id.</summary>
        public int Id { get; set; }

        /// <summary>The timer interval.</summary>
        public TimeSpan Interval { get; set; }

        /// <summary>The timer type.</summary>
        public TimerType Type { get; set; }

        /// <summary>The timer handler. Only assign a public static method or 
        /// a class instance method to this property.</summary>
        public TimerHandler TimerHandler { get; set; }
    }
}
