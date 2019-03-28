using System;

namespace Ignis
{
    public interface IEntityManager
    {
        int Create();
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
    }
}