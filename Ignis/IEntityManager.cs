using System;
using System.Collections.Generic;

namespace Ignis
{
	public interface IEntityManager
	{
		int Create();
		bool Create(int requestedEntityId);
		void Destroy(int entityId);
		EventHandler<EntityIdEventArgs> OnEntityCreated { get; set; }
		EventHandler<EntityIdEventArgs> OnEntityDestroyed { get; set; }
		bool Exists(int entityId);
		int EntityCount { get; }
		long EntityCountLong { get; }

		void AddComponent(int entityId, Type type);
		void AddComponent<T>(int entityId) where T : new();
		void RemoveComponent(int entityId, Type type);
		void RemoveComponent<T>(int entityId) where T : new();
		EventHandler<EntityComponentEventArgs> OnEntityComponentAdded { get; set; }
		EventHandler<EntityComponentEventArgs> OnEntityComponentRemoving { get; set; }
		EventHandler<EntityComponentEventArgs> OnEntityComponentRemoved { get; set; }
		bool HasComponent(int entityId, Type type);
		bool HasComponent<T>(int entityId) where T : new();
		IEnumerable<int> GetEntityIds();
		IEnumerable<int> Query(params Type[] componentTypes);
		void QueryTo(IList<int> storage, params Type[] componentTypes);
		void QueryTo(IList<int> storage, Type component1);
		void QueryTo(IList<int> storage, Type component1, Type component2);
		void QueryTo(IList<int> storage, Type component1, Type component2, Type component3);
		void QueryTo(IList<int> storage, Type component1, Type component2, Type component3, Type component4);
		ReadOnlySpan<int> Query(Span<int> storage, ReadOnlySpan<Type> componentTypes);
		ReadOnlySpan<int> Query(Span<int> storage, Type component1);
		ReadOnlySpan<int> Query(Span<int> storage, Type component1, Type component2);
		ReadOnlySpan<int> Query(Span<int> storage, Type component1, Type component2, Type component3);
		ReadOnlySpan<int> Query(Span<int> storage, Type component1, Type component2, Type component3, Type component4);
		IEnumerable<int> QuerySubset(IEnumerable<int> ids, bool checkExistence = true, params Type[] componentTypes);
		ReadOnlySpan<int> QuerySubset(ReadOnlySpan<int> ids, Span<int> storage, ReadOnlySpan<Type> componentTypes, bool checkExistence = true);
		ReadOnlySpan<int> QuerySubset(ReadOnlySpan<int> ids, Span<int> storage, Type component1, bool checkExistence = true);
		ReadOnlySpan<int> QuerySubset(ReadOnlySpan<int> ids, Span<int> storage, Type component1, Type component2, bool checkExistence = true);
		ReadOnlySpan<int> QuerySubset(ReadOnlySpan<int> ids, Span<int> storage, Type component1, Type component2, Type component3, bool checkExistence = true);
		ReadOnlySpan<int> QuerySubset(ReadOnlySpan<int> ids, Span<int> storage, Type component1, Type component2, Type component3, Type component4, bool checkExistence = true);
	}
}