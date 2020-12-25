using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using ConcurrentCollections;

namespace Ignis
{
	public interface IEntityView : IEnumerable<int>
	{
		int EntityCount { get; }
		IReadOnlyCollection<Type> Filter { get; }
	}

	public class EntityView : IEnumerable<int>, IEntityView
	{
		private readonly IReadOnlyCollection<Type> _filter;
		private readonly ConcurrentHashSet<int> _ids;
		private readonly IEntityManager _em;
		private volatile int _entityCount = 0;

		public int EntityCount => _entityCount;

		public IReadOnlyCollection<Type> Filter => _filter;

		public EntityView(IEntityManager em, params Type[] components)
			: this(em, (IEnumerable<Type>)components)
		{
		}

		public EntityView(IEntityManager em, IEnumerable<Type> components)
		{
			_em = em;
			_filter = new List<Type>(components);
			_ids = new ConcurrentHashSet<int>();
			_em.OnEntityComponentAdded += (s, e) => TryAdd(e.EntityID);
			_em.OnEntityComponentRemoved += (s, e) => TryRemove(e.EntityID);
			_em.OnEntityDestroyed += (s, e) => TryRemove(e.EntityID);
			FillSet();
		}

		private void TryAdd(int id)
		{
			if (!Belongs(id))
				return;
			if (_ids.Add(id))
				Interlocked.Increment(ref _entityCount);
		}

		private void TryRemove(int id)
		{
			if (_em.Exists(id) && Belongs(id))
				return;
			if (_ids.TryRemove(id))
				Interlocked.Decrement(ref _entityCount);
		}

		public IEnumerator<int> GetEnumerator()
		{
			return ((IEnumerable<int>)_ids).GetEnumerator();
		}

		private bool Belongs(int id)
		{
			foreach (var type in _filter)
				if (!_em.HasComponent(id, type))
					return false;
			return true;
		}

		private void FillSet()
		{
			var lockTaken = false;
			try
			{
				Monitor.Enter(_em, ref lockTaken);
				foreach (var id in _em.GetEntityIds())
					if (Belongs(id))
					{
						_ids.Add(id);
						_entityCount++;
					}
			}
			finally
			{
				if (lockTaken)
					Monitor.Exit(_em);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_ids).GetEnumerator();
		}
	}
}