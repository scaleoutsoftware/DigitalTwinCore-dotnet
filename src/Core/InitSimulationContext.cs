#region Copyright notice and license

// Copyright 2024 ScaleOut Software, Inc.
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

namespace Scaleout.Streaming.DigitalTwin.Core
{
    /// <summary>
    /// Context object that provides operations that are available
	/// when a new simulation starts
    /// </summary>
    public abstract class InitSimulationContext
    {
        /// <summary>
        /// Gets an <see cref="ISharedData"/> instance for accessing shared objects
        /// that are associated with the model being processed.
        /// </summary>
        public abstract ISharedData SharedModelData { get; }

        /// <summary>
        /// Gets an <see cref="ISharedData"/> instance for accessing objects
        /// that are shared globally between all models.
        /// </summary>
        public abstract ISharedData SharedGlobalData { get; }
    }
}
