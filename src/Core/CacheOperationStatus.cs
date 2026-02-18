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

namespace Scaleout.Modules.DigitalTwin.Abstractions
{
    /// <summary>
    /// An enumeration that indicates the outcome of a cache operation.
    /// </summary>
    public enum CacheOperationStatus
    {
        /// <summary>
        /// The object was successfully retrieved.
        /// </summary>
        ObjectRetrieved,

        /// <summary>
        /// The object was successfully added/updated.
        /// </summary>
        ObjectPut,

        /// <summary>
        /// The object could not be retrieved because it was not found.
        /// </summary>
        ObjectDoesNotExist,

        /// <summary>
        /// The object was removed successfully.
        /// </summary>
        ObjectRemoved,

        /// <summary>
        /// The cache was cleared successfully.
        /// </summary>
        CacheCleared


    }
}
