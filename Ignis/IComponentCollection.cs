using System;
using System.Collections;
using System.Collections.Generic;

namespace Ignis
{
internal interface IComponentCollectionStorage
{
	bool RemoveComponentFromStorage(int entityId);
	bool StoreComponentForEntity(int entityId);
}

public interface IComponentCollection
{
	/// <summary>
	/// Updates the specified entity's component value.
	/// It's strongly recommended to use the generic version
	/// <see cref="Update(int, T)" /> instead.
	/// </summary>
	/// <param name="entityId">Entity ID</param>
	/// <param name="value">New value (must be of type T)</param>
	void Update(int entityId, object value);

	/// <summary>
	/// Retrieves a component value for the specified entity.
	/// It's strongly recommended to use the generic version
	/// <see cref="Get(int)" /> instead.
	/// </summary>
	/// <param name="entityId">Entity ID</param>
	object GetValue(int entityId);
}

/// <summary>
/// Represents a component storage.
/// </summary>
/// <typeparam name="T">Component type</typeparam>
public interface IComponentCollection<T> : IComponentCollection
	where T : new()
{
	/// <summary>
	/// Updates current entity's component value.
	/// This method should be called ONLY when executing inside
	/// <see cref="ForEach{TState}(Action{int, T, TState}, TState)" />.
	/// </summary>
	/// <param name="value">New component value</param>
	/// <seealso cref="ForEach{TState}(Action{int, T, TState}, TState)" />
	void UpdateCurrent(T value);

	/// <summary>
	/// Updates the specified entity's component value.
	/// </summary>
	/// <param name="entityId">Entity ID</param>
	/// <param name="value">New component value</param>
	void Update(int entityId, T value);

	/// <summary>
	/// Retrieves a component value for the specified entity.
	/// </summary>
	/// <param name="entityId">Entity ID</param>
	T Get(int entityId);

	/// <summary>
	/// Iterates over each 'entity ID'-'component' pair using the supplied
	/// callback. The return value will be saved as the new component value.
	/// If your callback is a closure (which means it uses local variables or 'this'),
	/// consider using <see cref="ForEach<TState>(Action<int, T, TState>, TState)" /> instead
	/// to reduce heap memory allocations.
	/// </summary>
	/// <param name="action">Callback to be called on each pair</param>
	void Process(Func<int, T, T> action);

	/// <summary>
	/// Iterates over each 'entity ID'-'component' pair using the supplied
	/// callback. Passes an additional parameter to the callback which can
	/// be used to avoid heap allocations by passing the required variables
	/// through it.
	/// </summary>
	/// <param name="action">Callback to be called on each pair</param>
	/// <param name="state">Additional parameter for the callback</param>
	/// <seealso cref="UpdateCurrent(T)" />
	void ForEach<TState>(Action<int, T, TState> action, TState state);

	/// <summary>
	/// Return the count of entities with this component.
	/// </summary>
	int GetCount();

	/// <summary>
	/// Returns a view which contains entity IDs that have this component.
	/// </summary>
	IEntityView GetView();

	/// <summary>
	/// Check if the specified entity ID has a component stored in this storage.
	/// </summary>
	bool Contains(int entityId)
	{
		return GetView().Contains(entityId);
	}

	/// <summary>
	/// Returns an enumerator over entity IDs that have this component.
	/// </summary>
	/// <returns></returns>
	CollectionEnumerator<int> GetEnumerator()
	{
		return GetEntityIds().GetEnumerator();
	}

	/// <summary>
	/// Returns the entity IDs that have this component.
	/// </summary>
	CollectionEnumerable<int> GetEntityIds();

	/// <summary>
	/// Returns the values of all components.
	/// </summary>
	CollectionEnumerable<T> GetValues();
}
}