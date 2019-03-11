using System;

namespace Ignis
{
    public interface IEntityManager
    {
        long Create();
        void Destroy(long entityId);
        EventHandler<EntityIdEventArgs> OnEntityCreated { get; set; }
        EventHandler<EntityIdEventArgs> OnEntityDestroyed { get; set; }
        bool Exists(long entityId);
        int EntityCount { get; }
        long EntityCountLong { get; }

        void AddComponent(long entityId, Type type);
        void AddComponent<T>(long entityId) where T : struct;
        void RemoveComponent(long entityId, Type type);
        void RemoveComponent<T>(long entityId) where T : struct;
        EventHandler<EntityComponentEventArgs> OnEntityComponentAdded { get; set; }
        EventHandler<EntityComponentEventArgs> OnEntityComponentRemoved { get; set; }
        bool HasComponent(long entityId, Type type);
        bool HasComponent<T>(long entityId);
    }
}