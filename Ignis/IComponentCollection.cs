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

/// <summary>
/// Represents a component storage.
/// </summary>
/// <typeparam name="T">Component type</typeparam>
public interface IComponentCollection<T>
	where T : new()
{
	/// <summary>
	/// Updates current entity's component value.
	/// </summary>
	/// <param name="value">New component value</param>
	/// <seealso cref="ForEach(Action<int, T>)" />
	void UpdateCurrent(T value);
	/// <summary>
	/// Updates the specified entity's component value.
	/// </summary>
	/// <param name="entityId">Entity ID</param>
	/// <param name="value">New component value</param>
	void Update(int entityId, T value);
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
	/// </summary>
	/// <param name="entityId">Entity ID</param>
	T Get(int entityId);
	/// <summary>
	/// Iterates over each 'entity ID'-'component' pair using the supplied
	/// callback.
	/// If your callback is a closure (which means it uses local variables or 'this'),
	/// consider using <see cref="ForEach<TState>(Action<int, T, TState>, TState)" /> instead
	/// to reduce heap memory allocations.
	/// </summary>
	/// <param name="action">Callback to be called on each pair</param>
	void ForEach(Action<int, T> action);
	/// <summary>
	/// Iterates over each 'entity ID'-'component' pair using the supplied
	/// callback. Passes an additional parameter to the callback which can
	/// be used to avoid heap allocations by passing the required variables
	/// through it.
	/// </summary>
	/// <param name="action">Callback to be called on each pair</param>
	/// <param name="state">Additional parameter for the callback</param>
	void ForEach<TState>(Action<int, T, TState> action, TState state);
	/// <summary>
	/// Returns a view which contains entity IDs that have this component.
	/// </summary>
	IEntityView GetView();
	/// <summary>
	/// Return the count of entities with this component.
	/// </summary>
	int GetCount();
	/// <summary>
	/// Returns the values of all components.
	/// </summary>
	IEnumerable<T> GetValues();
}
}