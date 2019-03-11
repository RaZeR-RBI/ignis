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

        void AddComponent<T>(T component) where T : struct;
        void RemoveComponent<T>() where T : struct;
        EventHandler<EntityComponentEventArgs> OnEntityComponentAdded { get; set; }
        EventHandler<EntityComponentEventArgs> OnEntityComponentRemoved { get; set; }
        bool HasComponent<T>(long entityId);
    }
}