using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;

namespace Ignis.Storage
{
    public class ListOfPairsStorage<T> : IComponentCollection<T>, IComponentCollectionStorage
        where T : new()
    {
        private int _curIndex = 0;
        private List<EntityValuePair<T>> _pairs = new List<EntityValuePair<T>>();

        public void ForEach(Action<int, T> action)
        {
            Reset();
            while (HasNext())
            {
                var value = _pairs[_curIndex];
                _curIndex++;
                action(value.EntityID, value.ComponentValue);
            }
        }

        public T Get(int entityId) =>
            _pairs.First(p => p.EntityID == entityId).ComponentValue;

        public IEnumerator<T> GetEnumerator()
        {
            Reset();
            while (HasNext())
            {
                var value = _pairs[_curIndex];
                _curIndex++;
                yield return value.ComponentValue;
            }
        }

        public bool RemoveComponentFromStorage(int entityId)
        {
            for (int i = 0; i < _pairs.Count; i++)
                if (_pairs[i].EntityID == entityId)
                {
                    _pairs.RemoveAt(i);
                    if (i <= _curIndex)
                        _curIndex--;
                    return true;
                }
            return false;
        }

        public bool StoreComponentForEntity(int entityId)
        {
            if (_pairs.Any(p => p.EntityID == entityId))
                return false;
            _pairs.Add(new EntityValuePair<T>(entityId));
            return true;
        }

        public void Update(int entityId, T value)
        {
            var entityIndex = _pairs.FindIndex(p => p.EntityID == entityId);
            if (entityIndex == -1) return;
            var pair = _pairs[entityIndex];
            _pairs[entityIndex] = new EntityValuePair<T>(entityId, value);
        }

        public void UpdateCurrent(T value)
        {
            if (_curIndex - 1 >= _pairs.Count || _curIndex < 1) return;
            var curPair = _pairs[_curIndex - 1];
            curPair.ComponentValue = value;
            _pairs[_curIndex - 1] = curPair;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Reset() => _curIndex = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasNext() => _curIndex < _pairs.Count;
    }
}