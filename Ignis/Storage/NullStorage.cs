using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Ignis.Storage
{
/// <summary>
/// A storage that doesn't store any values and throws exceptions on almost all
/// actions. Useful for testing purposes or when a component is just an empty 'tag'
/// (has no value).
/// </summary>
/// <typeparam name="T">Component type</typeparam>
public class NullStorage<T> : IComponentCollection<T>, IComponentCollectionStorage
	where T : new()
{
	public void Process(Func<int, T, T> action)
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