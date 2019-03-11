using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Ignis.Storage
{
    public class DictionaryStorage<T> : IComponentCollection<T>, IComponentCollectionStorage
        where T : struct
    {
        private int _curIndex = 0;
        private ConcurrentDictionary<long, T> _values = new ConcurrentDictionary<long, T>();

        public long GetCurrentEntityId()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        public bool HasNextValue()
        {
            throw new System.NotImplementedException();
        }

        public ref T NextValue()
        {
            throw new System.NotImplementedException();
        }

        public bool RemoveComponentFromStorage(long entityId)
        {
            throw new System.NotImplementedException();
        }

        public void ResetIterator()
        {
            throw new System.NotImplementedException();
        }

        public bool StoreComponentForEntity(long entityId)
        {
            throw new System.NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new System.NotImplementedException();
        }
    }
}