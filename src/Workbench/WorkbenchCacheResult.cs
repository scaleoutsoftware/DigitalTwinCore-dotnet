using Scaleout.Streaming.DigitalTwin.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Scaleout.DigitalTwin.Workbench
{
    internal class WorkbenchCacheResult : ICacheResult
    {
        byte[]? _value;

        public WorkbenchCacheResult(string? key, CacheOperationStatus status, byte[]? value)
        {
            Key = key;
            Status = status;
            _value = value;
        }
        public string? Key { get; }

        public CacheOperationStatus Status { get; }

        public byte[]? Value
        {
            get
            {
                switch (Status)
                {
                    case CacheOperationStatus.ObjectRetrieved:
                        return _value;
                    case CacheOperationStatus.ObjectDoesNotExist:
                        throw new KeyNotFoundException("The object was not found.");
                    default:
                        throw new InvalidOperationException("Value is only returned from Get() operations.");
                }
            }
        }

    }
}
