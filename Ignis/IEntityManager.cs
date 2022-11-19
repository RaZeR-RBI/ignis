using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Ignis
{
/// <summary>
/// Represents an entity manager that's responsible for entity and component
/// creation, query and deletion.
/// </summary>
public interface IEntityManager
{
	/// <summary>
	/// Creates an entity.
	/// </summary>
	/// <returns>Entity ID (handle)</returns>
	int Create();

	/// <summary>
	/// Tries to create an entity with the specified ID.
	/// </summary>
	/// <param name="requestedEntityId">Entity ID to use</param>
	/// <returns>True if it was created, false if it already exists</returns>
	bool Create(int requestedEntityId);

	/// <summary>
	/// Deletes the specified entity.
	/// </summary>
	/// <param name="entityId">Entity ID</param>
	void Destroy(int entityId);

	/// <summary>
	/// Fires when an entity gets created.
	/// </summary>
	EventHandler<EntityIdEventArgs> OnEntityCreated { get; set; }

	/// <summary>
	/// Fires when an entity is being destroyed (before components removal).
	/// </summary>
	EventHandler<EntityIdEventArgs> OnEntityDestroying { get; set; }

	/// <summary>
	/// Fires when an entity gets destroyed.
	/// </summary>
	EventHandler<EntityIdEventArgs> OnEntityDestroyed { get; set; }

	/// <summary>
	/// Checks if the entity with the specified ID exists.
	/// </summary>
	/// <param name="entityId">Entity ID to check</param>
	/// <returns>True if exists, false otherwise</returns>
	bool Exists(int entityId);

	/// <summary>
	/// Gets the active entity count.
	/// </summary>
	int EntityCount { get; }

	/// <summary>
	/// Gets the active entity count as long.
	/// </summary>
	long EntityCountLong { get; }

	/// <summary>
	/// Adds a component with the specified type to an entity.
	/// </summary>
	/// <param name="entityId">Entity ID</param>
	/// <param name="type">Component type</param>
	void AddComponent(int entityId, Type type);

	/// <summary>
	/// Adds a component with the specified type to an entity.
	/// </summary>
	/// <param name="entityId">Entity ID</param>
	/// <typeparam name="T">Component type</typeparam>
	void AddComponent<T>(int entityId) where T : new();

	/// <summary>
	/// Removes a component from an entity.
	/// </summary>
	/// <param name="entityId">Entity ID</param>
	/// <param name="type">Component type</param>
	void RemoveComponent(int entityId, Type type);

	/// <summary>
	/// Removes a component from an entity.
	/// </summary>
	/// <param name="entityId">Entity ID</param>
	/// <typeparam name="T">Component type</typeparam>
	void RemoveComponent<T>(int entityId) where T : new();

	/// <summary>
	/// Fires after a component was added to an entity.
	/// </summary>
	EventHandler<EntityComponentEventArgs> OnEntityComponentAdded { get; set; }

	/// <summary>
	/// Fires before removing a component from an entity.
	/// </summary>
	EventHandler<EntityComponentEventArgs> OnEntityComponentRemoving { get; set; }

	/// <summary>
	/// Fires after removing a component from an entity.
	/// </summary>
	EventHandler<EntityComponentEventArgs> OnEntityComponentRemoved { get; set; }

	/// <summary>
	/// Checks if the specified entity has the specified component.
	/// </summary>
	/// <param name="entityId">Entity ID</param>
	/// <param name="type">Component type</param>
	/// <returns>True if it has this component, false otherwise</returns>
	bool HasComponent(int entityId, Type type);

	/// <summary>
	/// Checks if the specified entity has the specified component.
	/// </summary>
	/// <param name="entityId">Entity ID</param>
	/// <typeparam name="T">Component type</typeparam>
	/// <returns>True if it has this component, false otherwise</returns>
	bool HasComponent<T>(int entityId) where T : new();

	/// <summary>
	/// Gets the currently existing entity IDs.
	/// </summary>
	CollectionEnumerable<int> GetEntityIds();

	[ExcludeFromCodeCoverage]
	/// <summary>
	/// Returns an enumerator over existing entity IDs.
	/// </summary>
	/// <returns></returns>
	CollectionEnumerator<int> GetEnumerator()
	{
		return GetEntityIds().GetEnumerator();
	}

	/// <summary>
	/// Queries entity IDs that have the specified components.
	/// If you care about performance and memory allocation, consider using
	/// <see cref="Query(Span<int>, ReadOnlySpan<Type>)" /> and other overloads
	/// with Span, or <see cref="GetView(Type[])" />.
	/// </summary>
	/// <param name="componentTypes">Component types to check</param>
	IEnumerable<int> Query(params Type[] componentTypes)
	{
		return QuerySubset(GetEntityIds().AsEnumerable(), componentTypes, false);
	}

	/// <summary>
	/// Queries entity IDs that have the specified components and adds them
	/// to the specified collection.
	/// If you care about performance and memory allocation, consider using
	/// <see cref="Query(Span<int>, ReadOnlySpan<Type>)" /> and other overloads
	/// with Span, or <see cref="GetView(Type[])" />.
	/// </summary>
	/// <param name="storage">Collection to populate</param>
	/// <param name="componentTypes">Component types</param>
	void QueryTo(ICollection<int> storage, params Type[] componentTypes);

	/// <summary>
	/// Queries entity IDs that have the specified components and adds them
	/// to the specified collection.
	/// If you care about performance and memory allocation, consider using
	/// <see cref="Query(Span<int>, ReadOnlySpan<Type>)" /> and other overloads
	/// with Span, or <see cref="GetView(Type[])" />.
	/// </summary>
	void QueryTo(ICollection<int> storage, Type component1);

	/// <summary>
	/// Queries entity IDs that have the specified components and adds them
	/// to the specified collection.
	/// If you care about performance and memory allocation, consider using
	/// <see cref="Query(Span<int>, ReadOnlySpan<Type>)" /> and other overloads
	/// with Span, or <see cref="GetView(Type[])" />.
	/// </summary>
	void QueryTo(ICollection<int> storage, Type component1, Type component2);

	/// <summary>
	/// Queries entity IDs that have the specified components and adds them
	/// to the specified collection.
	/// If you care about performance and memory allocation, consider using
	/// <see cref="Query(Span<int>, ReadOnlySpan<Type>)" /> and other overloads
	/// with Span, or <see cref="GetView(Type[])" />.
	/// </summary>
	void QueryTo(ICollection<int> storage, Type component1, Type component2, Type component3);

	/// <summary>
	/// Queries entity IDs that have the specified components and adds them
	/// to the specified collection.
	/// If you care about performance and memory allocation, consider using
	/// <see cref="Query(Span<int>, ReadOnlySpan<Type>)" /> and other overloads
	/// with Span, or <see cref="GetView(Type[])" />.
	/// </summary>
	void QueryTo(ICollection<int> storage, Type component1, Type component2, Type component3,
	             Type component4);

	/// <summary>
	/// Queries entity IDs that have the specified components and populates the
	/// resulting Span.
	/// </summary>
	/// <returns>
	/// Slice of the original span that contain the query results.
	/// IDs that don't fit in the Span are skipped.
	/// </returns>
	ReadOnlySpan<int> Query(Span<int> storage, ReadOnlySpan<Type> componentTypes);

	/// <summary>
	/// Queries entity IDs that have the specified components and populates the
	/// resulting Span.
	/// </summary>
	/// <returns>
	/// Slice of the original span that contain the query results.
	/// IDs that don't fit in the Span are skipped.
	/// </returns>
	ReadOnlySpan<int> Query(Span<int> storage, Type component1);

	/// <summary>
	/// Queries entity IDs that have the specified components and populates the
	/// resulting Span.
	/// </summary>
	/// <returns>
	/// Slice of the original span that contain the query results.
	/// IDs that don't fit in the Span are skipped.
	/// </returns>
	ReadOnlySpan<int> Query(Span<int> storage, Type component1, Type component2);

	/// <summary>
	/// Queries entity IDs that have the specified components and populates the
	/// resulting Span.
	/// </summary>
	/// <returns>
	/// Slice of the original span that contain the query results.
	/// IDs that don't fit in the Span are skipped.
	/// </returns>
	ReadOnlySpan<int> Query(Span<int> storage, Type component1, Type component2, Type component3);

	/// <summary>
	/// Queries entity IDs that have the specified components and populates the
	/// resulting Span.
	/// </summary>
	/// <returns>
	/// Slice of the original span that contain the query results.
	/// IDs that don't fit in the Span are skipped.
	/// </returns>
	ReadOnlySpan<int> Query(Span<int> storage, Type component1, Type component2, Type component3,
	                        Type component4);

	/// <summary>
	/// Queries entity IDs from the specified enumerable that have the specified components.
	/// If you care about performance and memory allocation, consider using
	/// <see cref="Query(Span<int>, ReadOnlySpan<Type>)" /> and other overloads
	/// with Span, or <see cref="GetView(Type[])" />.
	/// </summary>
	/// <param name="ids">Entity ID subset to check</param>
	/// <param name="checkExistence">Should the ID existence be checked</param>
	IEnumerable<int> QuerySubset<T, C>(T ids, C componentTypes, bool checkExistence = true)
		where T : IEnumerable<int>
		where C : IEnumerable<Type>;

	/// <summary>
	/// Queries an entity ID subset that have the specified components and populates the
	/// resulting Span.
	/// </summary>
	/// <param name="ids">Entity ID subset to check</param>
	/// <param name="checkExistence">Should the ID existence be checked</param>
	/// <returns>
	/// Slice of the original span that contain the query results.
	/// IDs that don't fit in the Span are skipped.
	/// </returns>
	ReadOnlySpan<int> QuerySubset(ReadOnlySpan<int> ids, Span<int> storage,
	                              ReadOnlySpan<Type> componentTypes, bool checkExistence = true);

	/// <summary>
	/// Queries an entity ID subset that have the specified components and populates the
	/// resulting Span.
	/// </summary>
	/// <param name="ids">Entity ID subset to check</param>
	/// <param name="checkExistence">Should the ID existence be checked</param>
	/// <returns>
	/// Slice of the original span that contain the query results.
	/// IDs that don't fit in the Span are skipped.
	/// </returns>
	ReadOnlySpan<int> QuerySubset(ReadOnlySpan<int> ids, Span<int> storage, Type component1,
	                              bool checkExistence = true);

	/// <summary>
	/// Queries an entity ID subset that have the specified components and populates the
	/// resulting Span.
	/// </summary>
	/// <param name="ids">Entity ID subset to check</param>
	/// <param name="checkExistence">Should the ID existence be checked</param>
	/// <returns>
	/// Slice of the original span that contain the query results.
	/// IDs that don't fit in the Span are skipped.
	/// </returns>
	ReadOnlySpan<int> QuerySubset(ReadOnlySpan<int> ids, Span<int> storage, Type component1,
	                              Type component2, bool checkExistence = true);

	/// <summary>
	/// Queries an entity ID subset that have the specified components and populates the
	/// resulting Span.
	/// </summary>
	/// <param name="ids">Entity ID subset to check</param>
	/// <param name="checkExistence">Should the ID existence be checked</param>
	/// <returns>
	/// Slice of the original span that contain the query results.
	/// IDs that don't fit in the Span are skipped.
	/// </returns>
	ReadOnlySpan<int> QuerySubset(ReadOnlySpan<int> ids, Span<int> storage, Type component1,
	                              Type component2, Type component3, bool checkExistence = true);

	/// <summary>
	/// Queries an entity ID subset that have the specified components and populates the
	/// resulting Span.
	/// </summary>
	/// <param name="ids">Entity ID subset to check</param>
	/// <param name="checkExistence">Should the ID existence be checked</param>
	/// <returns>
	/// Slice of the original span that contain the query results.
	/// IDs that don't fit in the Span are skipped.
	/// </returns>
	ReadOnlySpan<int> QuerySubset(ReadOnlySpan<int> ids, Span<int> storage, Type component1,
	                              Type component2, Type component3, Type component4,
	                              bool checkExistence = true);

	/// <summary>
	/// Gets an entity view which contains entities with the specified components.
	/// </summary>
	/// <param name="filter">Required component types</param>
	IEntityView GetView(IEnumerable<Type> filter);

	/// <summary>
	/// Gets an entity view which contains entities with the specified components.
	/// </summary>
	/// <param name="filter">Required component types</param>
	IEntityView GetView(params Type[] filter);

	/// <summary>
	/// Destroys a view with the specified component filter.
	/// </summary>
	bool DestroyView(IEnumerable<Type> filter);

	/// <summary>
	/// Destroys a view with the specified component filter.
	/// </summary>
	bool DestroyView(params Type[] filter);
}
}