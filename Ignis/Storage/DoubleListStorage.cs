using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Ignis.Storage
{
    public class DoubleListStorage<T> : IComponentCollection<T>, IComponentCollectionStorage
        where T : new()
    {
        private int _curIndex = 0;
        private List<int> _ids = new List<int>();
        private List<T> _values = new List<T>();


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasNext() => _curIndex < _ids.Count;

        public IEnumerator<T> GetEnumerator()
        {
            Reset();
            while (HasNext())
            {
                var value = _values[_curIndex];
                _curIndex++;
                yield return value;
            }
        }

        public bool RemoveComponentFromStorage(int entityId)
        {
            var entityIndex = _ids.IndexOf(entityId);
            if (entityIndex == -1) return false;
            _ids.RemoveAt(entityIndex);
            _values.RemoveAt(entityIndex);
            if (entityIndex <= _curIndex)
                _curIndex--;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Reset() => _curIndex = 0;

        public bool StoreComponentForEntity(int entityId)
        {
            if (_ids.Contains(entityId)) return false;
            _ids.Add(entityId);
            _values.Add(new T());
            return true;
        }

        public void Update(int entityId, T value)
        {
            var entityIndex = _ids.IndexOf(entityId);
            if (entityIndex == -1) return;
            _values[entityIndex] = value;
        }

        public void UpdateCurrent(T value)
        {
            if (_curIndex - 1 >= _ids.Count || _curIndex < 1) return;
            _values[_curIndex - 1] = value;
        }

        public void ForEach(Action<int, T> action)
        {
            Reset();
            while (HasNext())
            {
                int entityId = _ids[_curIndex];
                T componentValue = _values[_curIndex];
                _curIndex++;
                action(entityId, componentValue);
            }
        }

        public T Get(int entityId) => _values[_ids.IndexOf(entityId)];

        [ExcludeFromCodeCoverage]
        public void Update(int entityId, object value) => Update(entityId, (T)value);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}