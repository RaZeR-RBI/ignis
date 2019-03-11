using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Ignis.Storage
{
    public class LockedListStorage<T> : IComponentCollection<T>, IComponentCollectionStorage
        where T : struct
    {
        private int _curIndex = 0;
        private object sync = new object();
        private List<int> _ids = new List<int>();
        private List<T> _values = new List<T>();

        public object Current => GetEnumerator().Current;

        private bool HasNext() => _curIndex < _ids.Count;

        private R Locked<R>(Func<R> func)
        {
            R value;
            lock (sync)
                value = func();
            return value;
        }

        private void Locked(Action act)
        {
            lock (_ids)
                lock (_values)
                    act();
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (!HasNext()) yield break;
            do
            {
                var value = Locked(() => _values[_curIndex]);
                yield return value;
            } while (HasNext());
        }

        public bool MoveNext() => GetEnumerator().MoveNext();

        public bool RemoveComponentFromStorage(int entityId) =>
            Locked(() =>
            {
                var entityIndex = _ids.IndexOf(entityId);
                if (entityIndex == -1) return false;
                _ids.RemoveAt(entityIndex);
                _values.RemoveAt(entityIndex);
                if (entityIndex <= _curIndex)
                    _curIndex--;
                return true;
            });

        public void Reset() => _curIndex = 0;

        public bool StoreComponentForEntity(int entityId) =>
            Locked(() =>
            {
                if (_ids.Contains(entityId)) return false;
                _ids.Add(entityId);
                _values.Add(new T());
                return true;
            });

        public void Update(int entityId, T value)
        {
            Locked(() =>
            {
                var entityIndex = _ids.IndexOf(entityId);
                if (entityIndex == -1) return;
                _values[entityIndex] = value;
            });
        }

        public void UpdateCurrent(T value)
        {
            Locked(() =>
            {
                if (_curIndex >= _ids.Count) return;
                _values[_curIndex] = value;
            });
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void ForEach(Action<int, T> action)
        {
            Reset();
            while (HasNext())
            {
                int entityId;
                T componentValue;
                lock (sync)
                {
                    entityId = _ids[_curIndex];
                    componentValue = _values[_curIndex];
                }
                action(entityId, componentValue);
                _curIndex++;
            }
        }
    }
}