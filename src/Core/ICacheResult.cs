#region Copyright notice and license

// Copyright 2023 ScaleOut Software, Inc.
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
using System.Collections.Generic;
using System.Text;

namespace Scaleout.Streaming.DigitalTwin.Core
{

    /// <summary>
    /// Represents a response from an <see cref="ISharedData"/> operation.
    /// </summary>
    public interface ICacheResult
    {
        /// <summary>
        /// Gets the key to the object associated with the result.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Get the object returned from a Get operation.
        /// </summary>
        byte[] Value { get; }

        /// <summary>
        /// Gets the outcome of the cache operation.
        /// </summary>
        CacheOperationStatus Status { get; }
    }
}
