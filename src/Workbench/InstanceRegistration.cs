#region Copyright notice and license

// Copyright 2023-2025 ScaleOut Software, Inc.
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

using Scaleout.Streaming.DigitalTwin.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Scaleout.DigitalTwin.Workbench
{
    internal class InstanceRegistration
    {
        public InstanceRegistration(DigitalTwinBase digitalTwinInstance, ModelRegistration modelRegistration, InstanceRegistration? dataSource)
        {
            DigitalTwinInstance = digitalTwinInstance;
            ModelRegistration = modelRegistration;
            DataSource = dataSource;
        }

        public DigitalTwinBase DigitalTwinInstance { get; }

        public ModelRegistration ModelRegistration { get; }

        public bool IsDeleted { get; set; } = false;
		
        public InstanceRegistration? DataSource { get; set; }		

        public bool IsFirstSimStep { get; set; } = false;
    }
}
