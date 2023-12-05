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
using System.Reflection;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Scaleout.Streaming.DigitalTwin.Core
{
    /// <summary>
    /// Serializes a dictionary of user defined timer handlers/delegates.
    /// </summary>
    public class DelegateSerializationConverter : JsonConverter<Dictionary<string, TimerMetadata>> //where T : DigitalTwinBase
    {
        const char delimiterSymbol = '^';

        /// <summary>
        /// Reads serialized JSON stream that represents a dictionary of user defined timer delegates and deserializes it to
        /// a live dictionary with deserialized timer delegates.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="hasExistingValue">The existing value has a value.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>Deserialized dictionary of the user defined timer delegates.</returns>
        public override Dictionary<string, TimerMetadata> ReadJson(JsonReader reader,
                                                               Type objectType,
                                                               Dictionary<string, TimerMetadata> existingValue,
                                                               bool hasExistingValue,
                                                               JsonSerializer serializer)
        {
            var delegateDictionary = new Dictionary<string, TimerMetadata>();
            int timerId = -1;
            string timerName = string.Empty;

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                {
                    return delegateDictionary;
                }

                if (reader.TokenType == JsonToken.PropertyName)
                {
                    timerName = reader.Value.ToString();
                }
                else if (reader.TokenType == JsonToken.String)
                {
                    var timerInfo = reader.Value.ToString();
                    var timerInfoParts = timerInfo.Split(delimiterSymbol);
                    if (timerInfoParts.Length < 5)
                        throw new JsonException("The serialized timer runtime details format is not recognized.");
                    else
                    {
                        if (int.TryParse(timerInfoParts[2], out int index))
                        {
                            timerId = index;
                            TimerHandler timerHandler = null;

                            Type type = Type.GetType(timerInfoParts[0]);
                            MethodInfo delegateInfo = type.GetMethod(timerInfoParts[1], BindingFlags.Public | BindingFlags.Static);
                            //if (delegateInfo != null)
                            //{
                            //    timerHandler = (TimerHandler)Delegate.CreateDelegate(typeof(TimerHandler), delegateInfo, throwOnBindFailure: false);
                            //    if (Enum.TryParse<TimerType>(timerInfoParts[3], out TimerType timerType))
                            //    {
                            //        if (TimeSpan.TryParse(timerInfoParts[4], out TimeSpan timerInterval))
                            //        {
                            //            if (!string.IsNullOrEmpty(timerName))
                            //            {
                            //                var timerRuntimeDetails = new TimerMetadata() { Id = timerId, TimerHandler = timerHandler, Interval = timerInterval, Type = timerType };
                            //                delegateDictionary.Add(timerName, timerRuntimeDetails);
                            //            }
                            //        }
                            //        else
                            //            throw new JsonException("Failed to parse the timer interval.");
                            //    }
                            //    else
                            //        throw new JsonException("Failed to parse the timer type.");
                            //}
                            //else
                            //    throw new JsonException("The twin deserialization failed since one of the user timer's event handler was not defined as public static method.");

                            if (delegateInfo != null)
                                timerHandler = (TimerHandler)Delegate.CreateDelegate(typeof(TimerHandler), delegateInfo, throwOnBindFailure: false);
                            else
                            {
                                delegateInfo = type.GetMethod(timerInfoParts[1], BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
                                if (delegateInfo != null)
                                {
                                    var instance = Activator.CreateInstance(type);
                                    timerHandler = (TimerHandler)Delegate.CreateDelegate(typeof(TimerHandler), instance, delegateInfo.Name);

                                    if (Enum.TryParse<TimerType>(timerInfoParts[3], out TimerType timerType))
                                    {
                                        if (TimeSpan.TryParse(timerInfoParts[4], out TimeSpan timerInterval))
                                        {
                                            if (!string.IsNullOrEmpty(timerName))
                                            {
                                                var timerRuntimeDetails = new TimerMetadata() { Id = timerId, TimerHandler = timerHandler, Interval = timerInterval, Type = timerType };
                                                delegateDictionary.Add(timerName, timerRuntimeDetails);
                                            }
                                        }
                                        else
                                            throw new JsonException("Failed to parse the timer interval while deserializing the digital twin.");
                                    }
                                    else
                                        throw new JsonException("Failed to parse the timer type while deserializing the digital twin.");
                                }
                                else
                                    throw new JsonException("The twin deserialization failed since one of the user timer's event handler type was not detected.");
                            }
                        }
                        else
                            throw new JsonException("Failed to parse the timer Id while deserializing the digital twin.");
                    }
                }
            }

            // Should not reach here
            throw new JsonException();
        }

        /// <summary>
        /// Writes the JSON representation of the digital twin internal dictionary that might contain
        /// user defined timer delegates.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The timer handler dictionary.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, Dictionary<string, TimerMetadata> value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            foreach(var element in value)
            {
                writer.WritePropertyName(element.Key);

                // The following value consists of 5 parts: "part1^part2^part3^part4^part5".
                // - part1 is the fully qualified name of the type where the user delegate is defined;
                // - part2 is the name of the user defined timer delegate;
                // - part3 is the timer Id (the value from 0 to 4);
                // - part4 is the timer type (Recurring, OneTime);
                // - part5 is the timer interval.
                string jsonValue = 
                    $"{element.Value.TimerHandler.Method.DeclaringType.AssemblyQualifiedName}{delimiterSymbol}" +
                    $"{element.Value.TimerHandler.Method.Name}{delimiterSymbol}" +
                    $"{element.Value.Id}{delimiterSymbol}" +
                    $"{element.Value.Type}{delimiterSymbol}" +
                    $"{element.Value.Interval.ToString()}";

                writer.WriteValue(jsonValue);
            }
            writer.WriteEndObject();
        }
    }
}
