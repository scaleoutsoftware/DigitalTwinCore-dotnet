using Scaleout.Modules.DigitalTwin.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Scaleout.DigitalTwin.Workbench
{
    internal class SimProcessingResult
    {
        public ProcessingResult ProcessingResult { get; set; }

        public bool StopRequested { get; set; }

        public bool DeleteRequested { get; set; }

        public TimeSpan RequestedSimulationCycleDelay { get; set; }
    }
}
