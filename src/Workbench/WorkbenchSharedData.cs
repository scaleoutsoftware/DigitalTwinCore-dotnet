using Scaleout.Modules.DigitalTwin.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Scaleout.DigitalTwin.Workbench
{
    internal class WorkbenchSharedData : ISharedData
    {
        ConcurrentDictionary<string, byte[]> _data = new ConcurrentDictionary<string, byte[]>();

        public Task<ICacheResult> ClearAsync()
        {
            _data.Clear();
            return Task.FromResult<ICacheResult>(new WorkbenchCacheResult(null, CacheOperationStatus.CacheCleared, null));
        }

        public Task<ICacheResult> GetAsync(string key)
        {
            if (_data.TryGetValue(key, out var result))
                return Task.FromResult<ICacheResult>(new WorkbenchCacheResult(key, CacheOperationStatus.ObjectRetrieved, result));
            else
                return Task.FromResult<ICacheResult>(new WorkbenchCacheResult(key, CacheOperationStatus.ObjectDoesNotExist, null));
        }

        public Task<ICacheResult> PutAsync(string key, byte[] value)
        {
            _data[key] = value;
            return Task.FromResult<ICacheResult>(new WorkbenchCacheResult(key, CacheOperationStatus.ObjectPut, null));
        }

        public Task<ICacheResult> RemoveAsync(string key)
        {
            if (_data.TryRemove(key, out var result))
                return Task.FromResult<ICacheResult>(new WorkbenchCacheResult(key, CacheOperationStatus.ObjectRemoved, null));
            else
                return Task.FromResult<ICacheResult>(new WorkbenchCacheResult(key, CacheOperationStatus.ObjectDoesNotExist, null));
        }
    }
}
