using System;
using Ignis;
using Ignis.Storage;
using FluentAssertions;
using Xunit;
using System.Collections.Generic;

namespace Tests
{
    public class EntityManagerTest
    {
        private IEntityManager em;
        private IComponentCollection<SampleComponent> storage = new LockedListStorage<SampleComponent>();

        public EntityManagerTest() => em = new EntityManager(StorageResolver);

        private IComponentCollectionStorage StorageResolver(Type type)
        {
            if (type == typeof(SampleComponent))
                return storage as IComponentCollectionStorage;
            throw new Exception("This component type is not registered");
        }

        [Fact]
        public void ShouldCreateCheckAndDestroyEntities()
        {
            var lastChangedEntity = InternalConstants.NonExistingEntityId;
            em.OnEntityCreated += (s, e) => lastChangedEntity = e.EntityID;
            em.OnEntityDestroyed += (s, e) => lastChangedEntity = e.EntityID;

            em.EntityCount.Should().Be(0);

            var firstEntity = em.Create();
            firstEntity.Should().NotBe(InternalConstants.NonExistingEntityId);
            em.Exists(firstEntity).Should().BeTrue("first entity was created");
            em.EntityCount.Should().Be(1, "one entity was created");
            lastChangedEntity.Should().Be(firstEntity);

            var secondEntity = em.Create();
            secondEntity.Should().NotBe(InternalConstants.NonExistingEntityId);
            secondEntity.Should().NotBe(firstEntity, "two separate entities");
            em.Exists(secondEntity).Should().BeTrue("second entity was created");
            em.EntityCount.Should().Be(2, "two entities were created");
            lastChangedEntity.Should().Be(secondEntity);

            em.Destroy(firstEntity);
            em.Exists(firstEntity).Should().BeFalse("first entity was destroyed");
            em.EntityCount.Should().Be(1, "one of two entities was destroyed");
            lastChangedEntity.Should().Be(firstEntity);

            em.Destroy(secondEntity);
            em.Exists(secondEntity).Should().BeFalse("second entity was destroyed");
            em.EntityCount.Should().Be(0, "no entities left");
            lastChangedEntity.Should().Be(secondEntity);
        }

        [Fact]
        public void ShouldAddCheckAndRemoveComponents()
        {
            var lastEvent = "";
            em.OnEntityComponentAdded += (s, e) => lastEvent = "added";
            em.OnEntityComponentRemoved += (s, e) => lastEvent = "removed";

            var entity = em.Create();
            lastEvent.Should().BeNullOrEmpty();

            em.HasComponent<SampleComponent>(entity).Should().BeFalse("created a new entity");
            em.AddComponent<SampleComponent>(entity);
            em.HasComponent<SampleComponent>(entity).Should().BeTrue("added a component");
            lastEvent.Should().Be("added");

            em.RemoveComponent<SampleComponent>(entity);
            em.HasComponent<SampleComponent>(entity).Should().BeFalse("removed a component");
            lastEvent.Should().Be("removed");
        }
    }
}