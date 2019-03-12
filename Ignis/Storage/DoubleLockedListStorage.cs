using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Ignis.Storage
{
    public class DoubleLockedListStorage<T> : IComponentCollection<T>, IComponentCollectionStorage
        where T : struct
    {
        private int _curIndex = 0;
        private object sync = new object();
        private List<int> _ids = new List<int>();
        private List<T> _values = new List<T>();


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasNext() => _curIndex < _ids.Count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private R Locked<R>(Func<R> func)
        {
            R value;
            lock (sync)
                value = func();
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Locked(Action act)
        {
            lock (sync)
                act();
        }

        public IEnumerator<T> GetEnumerator()
        {
            Reset();
            while (HasNext())
            {
                var value = Locked(() => _values[_curIndex]);
                _curIndex++;
                yield return value;
            }
        }

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Reset() => _curIndex = 0;

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
                if (_curIndex - 1 >= _ids.Count || _curIndex < 1) return;
                _values[_curIndex - 1] = value;
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
                _curIndex++;
                action(entityId, componentValue);
            }
        }

        public T Get(int entityId) => Locked(() => _values[_ids.IndexOf(entityId)]);
    }
}