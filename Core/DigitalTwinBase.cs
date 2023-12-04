/*
  DigitalTwinBase.cs
  
  Copyright (C), 2018-2023 by ScaleOut Software, Inc.

  PROPRIETARY TRADE SECRET INFORMATION OF SCALEOUT SOFTWARE, INC.

  The information contained in this file is a trade secret of ScaleOut Software, Inc.
  and is not to be disclosed or copied in any form without written permission.
*/
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Scaleout.Streaming.DigitalTwin.Core
{
    /// <summary>
    /// Digital twin timer delegate. Must be assigned to either a public static method or a 
	/// class instance method.
    /// </summary>
    /// <remarks>
    /// Each digital twin instance can have up to 5 timers that can be started via the 
	/// <see cref="ProcessingContext.StartTimer(string, TimeSpan, TimerType, TimerHandler)"/> method call.
    /// </remarks>
    /// <param name="timerName">The timer name.</param>
    /// <param name="context">The digital twin message processing context.</param>
    /// <param name="instance">The target digital twin object.</param>
    /// <returns>Return <see cref="ProcessingResult.DoUpdate"/> to indicate that the digital twin
    /// object data was modified so the digital twin needs to be saved. Return 
    /// <see cref="ProcessingResult.NoUpdate"/> if the twin object was not modified and does not need to be saved.</returns>
    public delegate ProcessingResult TimerHandler(string timerName, DigitalTwinBase instance, ProcessingContext context);

	/// <summary>
	/// All digital twin objects must be subclassed from this <see cref="DigitalTwinBase"/> 
	/// abstract base class to be integrated into the ScaleOut StreamServer message processing pipeline.
	/// </summary>
	public abstract class DigitalTwinBase
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
        public virtual void Init(string id, string model, InitContext initContext)
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
        public virtual Task InitAsync(string id, string model, InitContext initContext)
		{
			return InitAsync(id, model); 
		}

        /// <summary>
        /// Collection of digital twin timer handlers.
        /// </summary>
        [JsonProperty]
		[JsonConverter(typeof(DelegateSerializationConverter))]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		internal Dictionary<string, TimerMetadata> TimerHandlers { get; set; } = new Dictionary<string, TimerMetadata>();

        /// <summary>
        /// The next simulation time.
        /// </summary>
        [JsonProperty("NextSimulationTime")]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal ulong NextSimulationTimeUnixMsec { get; set; } = 0;

        /// <summary>
        /// Initializes <see cref="DigitalTwinBase.Id"/> and <see cref="DigitalTwinBase.Model"/>
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
		public void InitInternal(string id, string model, InitContext initContext)
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
				// Make sure their InitAsync method is finished before starting processing the 1st messages for this twin
				InitAsync(id, model, initContext).GetAwaiter().GetResult();
			}
			else
			{
				if ( (string.CompareOrdinal(id, this.Id) != 0 || string.CompareOrdinal(model, this.Model) != 0) &&
					Initialized == true)
				{
					throw new InvalidOperationException("Cannot re-initialize existing digital twin object with different identifier or model type.");
				}
			}
		}
	}
}
