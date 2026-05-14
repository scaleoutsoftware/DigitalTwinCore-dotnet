#region Copyright notice and license

// Copyright 2023-2026 ScaleOut Software, Inc.
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

using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Scaleout.Modules.DigitalTwin.Abstractions
{
    /// <summary>
    /// Digital twin timer delegate. Must be assigned to either a public static method or a 
    /// class instance method.
    /// </summary>
    /// <remarks>
    /// Each digital twin instance can have up to 5 timers that can be started via the 
    /// <see cref="ProcessingContext{TDigitalTwin}.StartTimerAsync(string, TimeSpan, TimerType, TimerAsyncHandler{TDigitalTwin})"/> 
    /// method call.
    /// </remarks>
    /// <param name="timerName">The timer name.</param>
    /// <param name="context">The digital twin message processing context.</param>
    /// <param name="instance">The target digital twin object.</param>
    /// <returns>Return <see cref="ProcessingResult.DoUpdate"/> to indicate that the digital twin
    /// object data was modified so the digital twin needs to be saved. Return 
    /// <see cref="ProcessingResult.NoUpdate"/> if the twin object was not modified and does not need to be saved.</returns>
    public delegate Task<ProcessingResult> TimerAsyncHandler<TDigitalTwin>(string timerName, TDigitalTwin instance, ProcessingContext<TDigitalTwin> context) 
        where TDigitalTwin : DigitalTwinBase<TDigitalTwin>;

    /// <summary>
    /// All digital twin objects must be subclassed from this 
    /// abstract base class to be integrated into the ScaleOut Digital Twins message processing pipeline.
    /// </summary>
    public abstract class DigitalTwinBase<TDigitalTwin> where TDigitalTwin : DigitalTwinBase<TDigitalTwin>
    {
        // Indicates whether this DT instance was already initialized (e.g. the Init method was called once).
        private bool Initialized = false;

        /// <summary>
        /// Unique digital twin identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Digital twin model type (e.g. "WindTurbine", "TemperatureSensor").
        /// </summary>
        public string Model { get; set; }

        /// <summary>
		/// For internal use only. ScaleOut AppId of instance's datasource, or zero if not available. Used
        /// for <see cref="ProcessingContext{TDigitalTwin}.SendToDataSourceAsync(byte[])"/> operations.
		/// </summary>
		public uint SourceNamespaceAppId { get; set; }

        /// <summary>
        /// The next simulation time. Used when a digital twin instance is being used
        /// in a simulation and the next simulation time needs to be tracked on the instance.
        /// </summary>
        public ulong NextSimulationTimeUnixMsec { get; set; } = 0;

        /// <summary>
        /// Override to implement initialization logic for a digital twin instance
        /// at creation time.
        /// </summary>
        /// <param name="id">Digital twin identifier.</param>
        /// <param name="model">Digital twin model type.</param>
        public virtual void Init(string id, string model) 
        {
        }

        /// <summary>
        /// Override to implement initialization logic for a digital twin instance
        /// at creation time.
        /// </summary>
        /// <param name="id">Digital twin identifier.</param>
        /// <param name="model">Digital twin model type.</param>
        /// <param name="initContext">
        /// Context object providing operations that are available
        /// when a digital twin instance is being created.
        /// </param>
        public virtual void Init(string id, string model, InitContext<TDigitalTwin> initContext)
        {
            Init(id, model);
        }

        /// <summary>
        /// Override to implement initialization logic for a digital twin instance
        /// at creation time (async version).
        /// </summary>
        /// <param name="id">Digital twin identifier.</param>
        /// <param name="model">Digital twin model type.</param>
        public virtual Task InitAsync(string id, string model) { return Task.CompletedTask; }

        /// <summary>
        /// Override to implement initialization logic for a digital twin instance
        /// at creation time (async version).
        /// </summary>
        /// <param name="id">Digital twin identifier.</param>
        /// <param name="model">Digital twin model type.</param>
        /// <param name="initContext">
        /// Context object providing operations that are available
        /// when a digital twin instance is being created.
        /// </param>
        public virtual Task InitAsync(string id, string model, InitContext<TDigitalTwin> initContext)
        {
            return InitAsync(id, model); 
		}

        /// <summary>
        /// Initializes <see cref="DigitalTwinBase{TDerived}.Id"/> 
        /// and <see cref="DigitalTwinBase{TDerived}.Model"/>
        /// properties at the time of object creation.
        /// </summary>
        /// <param name="id">Digital twin identifier.</param>
        /// <param name="model">Digital twin model type.</param>
        /// <param name="initContext">
        /// Context object providing operations that are available
        /// when a digital twin instance is being created.
        /// </param>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Task InitInternalAsync(string id, string model, InitContext<TDigitalTwin> initContext)
        {
            if (!Initialized)
            {
                this.Id		= id;
                this.Model	= model;
                Initialized	= true;

                //
                // Call virtual Init() and InitAsync() methods in case they were overridden by customer
                //
                Init(id, model, initContext);
                // Kick off InitAsync before starting processing the 1st messages for this twin. Caller should await.
                return InitAsync(id, model, initContext);
            }
            else
            {
                if ( (string.CompareOrdinal(id, this.Id) != 0 || string.CompareOrdinal(model, this.Model) != 0) &&
                    Initialized == true)
                {
                    throw new InvalidOperationException("Cannot re-initialize existing digital twin object with different identifier or model type.");
                }
                return Task.CompletedTask;
            }
        }
    }
}
