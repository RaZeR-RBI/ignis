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
	private readonly IEntityManager em;

	private readonly IComponentCollection<SampleComponent>
		storage = new DoubleListStorage<SampleComponent>();

	private static readonly Type[] componentTypes = new Type[] {typeof(SampleComponent)};

	public EntityManagerTest()
	{
		em = new EntityManager(StorageResolver);
	}

	private IComponentCollectionStorage StorageResolver(Type type)
	{
		if (type == typeof(SampleComponent))
			return storage as IComponentCollectionStorage;
		return (IComponentCollectionStorage) Activator.CreateInstance(
		typeof(NullStorage<>).MakeGenericType(type));
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
		em.GetEntityIds().Should().BeEquivalentTo(new int[] {firstEntity});
		lastChangedEntity.Should().Be(firstEntity);

		var secondEntity = em.Create();
		secondEntity.Should().NotBe(IgnisConstants.NonExistingEntityId);
		secondEntity.Should().NotBe(firstEntity, "two separate entities");
		em.Exists(secondEntity).Should().BeTrue("second entity was created");
		em.EntityCount.Should().Be(2, "two entities were created");
		em.GetEntityIds().Should().BeEquivalentTo(new int[] {firstEntity, secondEntity});
		lastChangedEntity.Should().Be(secondEntity);

		em.Destroy(firstEntity);
		em.Exists(firstEntity).Should().BeFalse("first entity was destroyed");
		em.EntityCount.Should().Be(1, "one of two entities was destroyed");
		em.GetEntityIds().Should().BeEquivalentTo(new int[] {secondEntity});
		lastChangedEntity.Should().Be(firstEntity);

		em.Destroy(secondEntity);
		em.Exists(secondEntity).Should().BeFalse("second entity was destroyed");
		em.EntityCount.Should().Be(0, "no entities left");
		em.GetEntityIds().Should().BeEquivalentTo(Enumerable.Empty<int>());
		lastChangedEntity.Should().Be(secondEntity);

		em.Destroy(IgnisConstants.NonExistingEntityId);
	}

	[Fact]
	public void ShouldQuerySubsets()
	{
		var entityOne = em.Create();
		var entityTwo = em.Create();
		var entityThree = em.Create();
		Span<int> subset = stackalloc int[2];
		subset[0] = entityTwo;
		subset[1] = entityThree;

		em.AddComponent<SampleComponent>(entityOne);
		em.AddComponent<SampleComponent>(entityTwo);

		Span<int> storage = stackalloc int[3];
		var result = em.QuerySubset(subset, storage, componentTypes);
		result.Length.Should().Be(1);
		result[0].Should().Be(entityTwo);
	}

	[Fact]
	public void ShouldAddCheckAndRemoveComponents()
	{
		var lastEvent = "";
		Span<int> storage = stackalloc int[1];
		em.OnEntityComponentAdded += (s, e) => lastEvent = "added";
		em.OnEntityComponentRemoved += (s, e) => lastEvent = "removed";

		var entity = em.Create();
		var oneEntity = new int[] {entity};
		lastEvent.Should().BeNullOrEmpty();

		em.HasComponent<SampleComponent>(entity).Should().BeFalse("created a new entity");
		em.GetEntityIds().Should().BeEquivalentTo(oneEntity);
		em.Query(typeof(SampleComponent)).Should().BeEmpty();
		var result = em.Query(storage, typeof(SampleComponent));
		result.Length.Should().Be(0);

		em.AddComponent<SampleComponent>(entity);

		em.HasComponent<SampleComponent>(entity).Should().BeTrue("added a component");
		em.Query(typeof(SampleComponent)).Should().BeEquivalentTo(oneEntity);
		result = em.Query(storage, componentTypes);
		result.Length.Should().Be(1);
		result[0].Should().Be(entity);
		lastEvent.Should().Be("added");

		em.RemoveComponent<SampleComponent>(entity);
		em.HasComponent<SampleComponent>(entity).Should().BeFalse("removed a component");
		em.Query(typeof(SampleComponent)).Should().BeEmpty();
		result = em.Query(storage, componentTypes);
		result.Length.Should().Be(0);
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

	public struct Component1
	{
	}

	public struct Component2
	{
	}

	public struct Component3
	{
	}

	public struct Component4
	{
	}

	private void TestQuery(IEntityManager em, IReadOnlyCollection<int> expected,
	                       params Type[] types)
	{
		em.Query(types).Should().BeEquivalentTo(expected);

		Span<int> span = stackalloc int[expected.Count];
		var items = new List<int>();
		em.Query(span, types).ToArray().Should().BeEquivalentTo(expected);
		em.QueryTo(items, types);
		items.Should().BeEquivalentTo(expected);
		items.Clear();

		switch (types.Length)
		{
		case 1:
			em.Query(span, types[0]).ToArray().Should().BeEquivalentTo(expected);
			em.QueryTo(items, types[0]);
			items.Should().BeEquivalentTo(expected);
			break;
		case 2:
			em.Query(span, types[0], types[1]).ToArray().Should().BeEquivalentTo(expected);
			em.QueryTo(items, types[0], types[1]);
			items.Should().BeEquivalentTo(expected);
			break;
		case 3:
			em.Query(span, types[0], types[1], types[2])
			  .ToArray()
			  .Should()
			  .BeEquivalentTo(expected);
			em.QueryTo(items, types[0], types[1], types[2]);
			items.Should().BeEquivalentTo(expected);
			break;
		case 4:
			em.Query(span, types[0], types[1], types[2], types[3])
			  .ToArray()
			  .Should()
			  .BeEquivalentTo(expected);
			em.QueryTo(items, types[0], types[1], types[2], types[3]);
			items.Should().BeEquivalentTo(expected);
			break;
		default:
			em.Query(span, types).ToArray().Should().BeEquivalentTo(expected);
			em.QueryTo(items, types);
			items.Should().BeEquivalentTo(expected);
			break;
		}
	}

	[Fact]
	public void ShouldQueryUsingOverloads()
	{
		Span<int> entities = stackalloc int[4];
		for (var i = 0; i < entities.Length; i++)
		{
			entities[i] = em.Create();
			em.AddComponent<Component1>(entities[i]);
			if (i > 0) em.AddComponent<Component2>(entities[i]);
			if (i > 1) em.AddComponent<Component3>(entities[i]);
			if (i > 2) em.AddComponent<Component4>(entities[i]);
		}

		Span<int> storage = stackalloc int[entities.Length];

		// query & queryTo
		var ids = entities.ToArray();
		TestQuery(em, ids, typeof(Component1));
		TestQuery(em, ids.Skip(1).ToList(), typeof(Component1), typeof(Component2));
		TestQuery(em, ids.Skip(2).ToList(), typeof(Component1), typeof(Component2),
		          typeof(Component3));
		TestQuery(em, ids.Skip(3).ToList(), typeof(Component1), typeof(Component2),
		          typeof(Component3), typeof(Component4));

		// query subset
		var subset = entities.ToArray().Skip(1).ToArray();
		var result = em.QuerySubset(subset, storage, typeof(Component1));
		result.Length.Should().Be(3);
		result.ToArray().Should().BeEquivalentTo(subset);

		result = em.QuerySubset(subset, storage, typeof(Component1), typeof(Component2));
		result.Length.Should().Be(3);
		result.ToArray().Should().BeEquivalentTo(subset);

		result = em.QuerySubset(subset, storage, typeof(Component1), typeof(Component2),
		                        typeof(Component3));
		result.Length.Should().Be(2);
		result.ToArray().Should().BeEquivalentTo(subset.Skip(1));

		result = em.QuerySubset(subset, storage, typeof(Component1), typeof(Component2),
		                        typeof(Component3), typeof(Component4));
		result.Length.Should().Be(1);
		result.ToArray().Should().BeEquivalentTo(subset.Skip(2));
	}
}
}