using System;
using System.Collections;
using System.Collections.Generic;

namespace Ignis.Storage
{
    public class ConcurrentDictionaryStorage<T> : IComponentCollection<T>, IComponentCollectionStorage
        where T : struct
    {

        public object Current => throw new NotImplementedException();

        public void ForEach(Action<int, T> action)
        {
            throw new NotImplementedException();
        }

        public T Get(int entityId)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool MoveNext()
        {
            throw new NotImplementedException();
        }

        public bool RemoveComponentFromStorage(int entityId)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public bool StoreComponentForEntity(int entityId)
        {
            throw new NotImplementedException();
        }

        public void Update(int entityId, T value)
        {
            throw new NotImplementedException();
        }

        public void UpdateCurrent(T value)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}