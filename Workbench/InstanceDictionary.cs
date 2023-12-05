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

using Scaleout.Streaming.DigitalTwin.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Scaleout.DigitalTwin.Workbench
{
    internal class InstanceDictionary<TDigitalTwin> : IReadOnlyDictionary<string, TDigitalTwin>
        where TDigitalTwin : DigitalTwinBase
    {
        IDictionary<string, InstanceRegistration> _instanceRegistrations;

        public InstanceDictionary(IDictionary<string, InstanceRegistration> instanceRegistrations)
        {
            _instanceRegistrations = instanceRegistrations;
        }
        public TDigitalTwin this[string key] => (TDigitalTwin)_instanceRegistrations[key].DigitalTwinInstance;

        public IEnumerable<string> Keys => _instanceRegistrations.Keys;

        public IEnumerable<TDigitalTwin> Values => 
            _instanceRegistrations.Values
                    .Select(ir => ir.DigitalTwinInstance)
                    .Cast<TDigitalTwin>();

        public int Count => _instanceRegistrations.Count;

        public bool ContainsKey(string key)
        {
            return _instanceRegistrations.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<string, TDigitalTwin>> GetEnumerator()
        {
            foreach (var item in _instanceRegistrations)
            {
                yield return new KeyValuePair<string, TDigitalTwin>(item.Key, (TDigitalTwin)item.Value.DigitalTwinInstance);
            }
        }

        public bool TryGetValue(string key, out TDigitalTwin value)
        {
            if (_instanceRegistrations.TryGetValue(key, out InstanceRegistration ir))
            {
                value = (TDigitalTwin)ir.DigitalTwinInstance;
                return true;
            }
            else
            {
                // As of C# 8, the null-forgiving operator is mandatory here on "default".
                // This may be fixed in a future version of the compiler.
                // https://github.com/dotnet/roslyn/issues/30953
                value = default!;

                return false;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
