using System;
using System.Collections.Generic;
using System.Text;

namespace Scaleout.Modules.DigitalTwin.Abstractions
{
    /// <summary>
    /// Severity levels for logging messages to the ScaleOut UI.
    /// </summary>
    public enum LogSeverity
    {
        /// <summary>
        /// Used for logging of lengthy messages.
        /// </summary>
        Verbose,

        /// <summary>
        /// Indicates that log messages with that level have informational purpose.
        /// </summary>
        Informational,

        /// <summary>
        /// Indicates that log messages with that level are application warnings.
        /// </summary>
        Warning,

        /// <summary>
        /// Indicates that log messages with that level should be treated as an application error.
        /// </summary>
        Error,

        /// <summary>
        /// The message corresponds to a critical error that has caused a major failure that 
        /// requires immediate attention.
        /// </summary>
        Critical,

        /// <summary>
        /// Indicates that log messages with that level should not be stored (e.g. temporarily 
        /// based on some digital twin configuration setting).
        /// </summary>
        None
    }
}
