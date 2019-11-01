using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Ignis.Storage
{
    public class NullStorage<T> : IComponentCollection<T>, IComponentCollectionStorage
        where T : new()
    {
        public void ForEach(Action<int, T> action)
        {
            throw new InvalidOperationException();
        }

        public T Get(int entityId)
        {
            throw new InvalidOperationException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new InvalidOperationException();
        }

        public bool RemoveComponentFromStorage(int entityId) => true;

        public bool StoreComponentForEntity(int entityId) => true;

        public void Update(int entityId, T value)
        {
            throw new InvalidOperationException();
        }

        public void Update(int entityId, object value)
        {
            throw new InvalidOperationException();
        }

        public void UpdateCurrent(T value)
        {
            throw new InvalidOperationException();
        }

        [ExcludeFromCodeCoverage]
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}