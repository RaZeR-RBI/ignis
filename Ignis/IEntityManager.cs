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
		EventHandler<EntityComponentEventArgs> OnEntityComponentRemoved { get; set; }
		bool HasComponent(int entityId, Type type);
		bool HasComponent<T>(int entityId) where T : new();
		IEnumerable<int> GetEntityIds();
		IEnumerable<int> Query(params Type[] componentTypes);
		IEnumerable<int> QuerySubset(IEnumerable<int> ids, bool checkExistence = true, params Type[] componentTypes);
	}
}