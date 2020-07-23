using System;
using Ignis;
using Ignis.Storage;
using FluentAssertions;
using Xunit;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
	public class EntityManagerTest
	{
		private IEntityManager em;
		private IComponentCollection<SampleComponent> storage = new DoubleListStorage<SampleComponent>();

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
			var lastChangedEntity = IgnisConstants.NonExistingEntityId;
			em.OnEntityCreated += (s, e) => lastChangedEntity = e.EntityID;
			em.OnEntityDestroyed += (s, e) => lastChangedEntity = e.EntityID;

			em.EntityCount.Should().Be(0);
			em.GetEntityIds().Should().BeEquivalentTo(Enumerable.Empty<int>());

			var firstEntity = em.Create();
			firstEntity.Should().NotBe(IgnisConstants.NonExistingEntityId);
			em.Exists(firstEntity).Should().BeTrue("first entity was created");
			em.EntityCount.Should().Be(1, "one entity was created");
			em.GetEntityIds().Should().BeEquivalentTo(new int[] { firstEntity });
			lastChangedEntity.Should().Be(firstEntity);

			var secondEntity = em.Create();
			secondEntity.Should().NotBe(IgnisConstants.NonExistingEntityId);
			secondEntity.Should().NotBe(firstEntity, "two separate entities");
			em.Exists(secondEntity).Should().BeTrue("second entity was created");
			em.EntityCount.Should().Be(2, "two entities were created");
			em.GetEntityIds().Should().BeEquivalentTo(new int[] { firstEntity, secondEntity });
			lastChangedEntity.Should().Be(secondEntity);

			em.Destroy(firstEntity);
			em.Exists(firstEntity).Should().BeFalse("first entity was destroyed");
			em.EntityCount.Should().Be(1, "one of two entities was destroyed");
			em.GetEntityIds().Should().BeEquivalentTo(new int[] { secondEntity });
			lastChangedEntity.Should().Be(firstEntity);

			em.Destroy(secondEntity);
			em.Exists(secondEntity).Should().BeFalse("second entity was destroyed");
			em.EntityCount.Should().Be(0, "no entities left");
			em.GetEntityIds().Should().BeEquivalentTo(Enumerable.Empty<int>());
			lastChangedEntity.Should().Be(secondEntity);
		}

		[Fact]
		public void ShouldAddCheckAndRemoveComponents()
		{
			var lastEvent = "";
			em.OnEntityComponentAdded += (s, e) => lastEvent = "added";
			em.OnEntityComponentRemoved += (s, e) => lastEvent = "removed";

			var entity = em.Create();
			var oneEntity = new int[] { entity };
			lastEvent.Should().BeNullOrEmpty();

			em.HasComponent<SampleComponent>(entity).Should().BeFalse("created a new entity");
			em.GetEntityIds().Should().BeEquivalentTo(oneEntity);
			em.Query(typeof(SampleComponent)).Should().BeEmpty();

			em.AddComponent<SampleComponent>(entity);

			em.HasComponent<SampleComponent>(entity).Should().BeTrue("added a component");
			em.Query(typeof(SampleComponent)).Should().BeEquivalentTo(oneEntity);
			lastEvent.Should().Be("added");

			em.RemoveComponent<SampleComponent>(entity);
			em.HasComponent<SampleComponent>(entity).Should().BeFalse("removed a component");
			em.Query(typeof(SampleComponent)).Should().BeEmpty();
			lastEvent.Should().Be("removed");
		}

		[Fact]
		public void ShouldCreateEntityWhenIdIsSpecified()
		{
			em.Create(1337).Should().BeTrue();
			em.Exists(1337).Should().BeTrue();

			em.Create(1337).Should().BeFalse();
			em.Create(IgnisConstants.NonExistingEntityId).Should().BeFalse();
		}
	}
}