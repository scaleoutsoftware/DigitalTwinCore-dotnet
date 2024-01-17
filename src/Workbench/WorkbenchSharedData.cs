using Scaleout.Streaming.DigitalTwin.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Scaleout.DigitalTwin.Workbench
{
    internal class WorkbenchSharedData : ISharedData
    {
        ConcurrentDictionary<string, byte[]> _data = new ConcurrentDictionary<string, byte[]>();

        public ICacheResult Clear()
        {
            _data.Clear();
            return new WorkbenchCacheResult(null, CacheOperationStatus.CacheCleared, null);
        }

        public ICacheResult Get(string key)
        {
            if (_data.TryGetValue(key, out var result))
                return new WorkbenchCacheResult(key, CacheOperationStatus.ObjectRetrieved, result);
            else
                return new WorkbenchCacheResult(key, CacheOperationStatus.ObjectDoesNotExist, null);
        }

        public ICacheResult Put(string key, byte[] value)
        {
            _data[key] = value;
            return new WorkbenchCacheResult(key, CacheOperationStatus.ObjectPut, null);
        }

        public ICacheResult Remove(string key)
        {
            if (_data.TryRemove(key, out var result))
                return new WorkbenchCacheResult(key, CacheOperationStatus.ObjectRemoved, null);
            else
                return new WorkbenchCacheResult(key, CacheOperationStatus.ObjectDoesNotExist, null);
        }
    }
}
