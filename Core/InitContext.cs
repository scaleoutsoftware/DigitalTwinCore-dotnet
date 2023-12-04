using System;
using System.Collections.Generic;
using System.Text;

namespace Scaleout.Streaming.DigitalTwin.Core
{
    /// <summary>
    /// Context object that provides operations that are available
	/// when a digital twin instance is being created.
    /// </summary>
    public abstract class InitContext
    {
        /// <summary>
		/// Starts a new timer for the digital twin being created.
		/// </summary>
		/// <param name="timerName">The timer name.</param>
		/// <param name="interval">The timer interval.</param>
		/// <param name="type">The type of the timer.</param>
		/// <param name="timerCallback">A delegate representing a user-defined timer callback static method to be executed.</param>
		/// <returns><see cref="TimerActionResult.Success"/> if the timer was started successfully, otherwise one of the following 
		/// error codes is returned: <see cref="TimerActionResult.FailedTooManyTimers"/> when the maximum number of timers is reached or 
		/// <see cref="TimerActionResult.FailedInternalError"/> if an error occurred during the method call.</returns>
        public abstract TimerActionResult StartTimer(string timerName, TimeSpan interval, TimerType type, TimerHandler timerCallback);
    }
}
