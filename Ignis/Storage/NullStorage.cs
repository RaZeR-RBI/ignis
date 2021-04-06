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

	public void ForEach<TState>(Action<int, T, TState> action, TState state)
	{
		throw new InvalidOperationException();
	}

	public T Get(int entityId)
	{
		throw new InvalidOperationException();
	}

	public int GetCount()
	{
		throw new InvalidOperationException();
	}

	public IEnumerable<T> GetValues()
	{
		throw new InvalidOperationException();
	}

	public IEntityView GetView()
	{
		throw new InvalidOperationException();
	}

	public bool RemoveComponentFromStorage(int entityId)
	{
		return true;
	}

	public bool StoreComponentForEntity(int entityId)
	{
		return true;
	}

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
}
}