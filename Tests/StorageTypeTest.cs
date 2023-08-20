using Ignis;
using Ignis.Storage;
using Xunit;
using FluentAssertions;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

namespace Tests
{
public class StorageTypeTest
{
	[Theory]
	[ClassData(typeof(StorageTypesTestData))]
	public void ShouldIterateAndModify(IComponentCollection<SampleComponent> storage)
	{
		var em = new EntityManager(t => storage as IComponentCollectionStorage);
		var withComponents = new List<int>();
		var withoutComponents = new List<int>();
		for (var i = 0; i < 5; i++)
		{
			withoutComponents.Add(em.Create());
			var entityWithComponent = em.Create();
			storage.Contains(entityWithComponent).Should().BeFalse();
			em.AddComponent<SampleComponent>(entityWithComponent);
			storage.Contains(entityWithComponent).Should().BeTrue();
			withComponents.Add(entityWithComponent);
		}

		// Check if components were added where necessary
		withComponents.Should().OnlyContain(id => em.HasComponent<SampleComponent>(id));
		withoutComponents.Should().OnlyContain(id => !em.HasComponent<SampleComponent>(id));

		// Check IEntityView implementation
		var view = storage.GetView();
		view.AsEnumerable().Should().BeEquivalentTo(withComponents);
		view.EntityCount.Should().Be(withComponents.Count);
		view.Filter.Should().BeEquivalentTo(new[] {typeof(SampleComponent)});
		Span<int> viewData = stackalloc int[withComponents.Count];
		var actualViewData = view.CopyTo(viewData).ToArray();
		actualViewData.Should().BeEquivalentTo(withComponents);

		// Try to iterate using foreach
		var count = 0;
		foreach (var component in storage.GetValues())
			count++;
		count.Should().Be(withComponents.Count);

		// Try to iterate and modify
		count = 0;
		var ids = storage.GetView();
		foreach (var id in ids)
		{
			var value = new SampleComponent()
			{
				Foo = 1337,
				Bar = true
			};
			storage.Update(id, value);
			count++;
		}

		count.Should().Be(withComponents.Count);

		// Check if they were modified
		var values = storage.GetValues().AsEnumerable();
		values.Should().OnlyContain(c => c.Foo == 1337);
		values.Should().OnlyContain(c => c.Bar == true);

		// Modify again and delete component from every second entity
		var iterations = 0;
		count = 0;
		storage.ForEach((id, componentValue, _) =>
		{
			componentValue.Foo = id;
			storage.UpdateCurrent(componentValue);
			if (iterations % 2 == 1)
				em.RemoveComponent<SampleComponent>(id);
			else
				count++;
			iterations++;
		}, default(object));
		iterations.Should().Be(withComponents.Count);
		var oddsAndEvens = withComponents
		                   .Select((item, i) => (item, isEven: i % 2 == 0))
		                   .ToLookup(x => x.isEven, x => x.item);

		// Check if the components were removed
		oddsAndEvens[true].Should().OnlyContain(id => em.HasComponent<SampleComponent>(id));
		oddsAndEvens[false].Should().OnlyContain(id => !em.HasComponent<SampleComponent>(id));

		// Check if the alive ones was updated
		storage.ForEach((id, val, _) =>
		{
			val.Foo.Should().Be(id);
			val.Bar.Should().BeTrue();
		}, default(object));

		// Modify and get in different order
		// Note: this is slower than the ForEach due to additional lookups
		var alive = oddsAndEvens[true].Reverse();
		foreach (var id in alive)
		{
			var newValue = new SampleComponent()
			{
				Foo = id + 1
			};
			storage.Update(id, newValue);
		}

		foreach (var id in alive.Reverse())
			storage.Get(id).Foo.Should().Be(id + 1);
	}

	[Theory]
	[ClassData(typeof(StorageTypesTestData))]
	public void ShouldIterateWhenEmpty(IComponentCollection<SampleComponent> storage)
	{
		var count = 0;
		foreach (var component in storage.GetValues())
			count++;
		count.Should().Be(0);

		var items = new List<int>();
		storage.ForEach((id, val, list) => list.Add(id), items);
		items.Should().BeEmpty();
	}

	[Fact]
	public void ShouldThrowExceptionOnNullStorageAccess()
	{
		var storage = new NullStorage<SampleComponent>();
		var actions = new List<Action>
		{
			() => storage.Process((id, val) => val),
			() => storage.ForEach((id, val, _) => { }, default(object)),
			() => storage.Get(-1),
			() => storage.GetValues(),
			() => storage.Update(-1, new SampleComponent()),
			() => storage.Update(-1, new object()),
			() => storage.UpdateCurrent(new SampleComponent())
		};
		foreach (var action in actions)
			Assert.ThrowsAny<InvalidOperationException>(action);
		storage.StoreComponentForEntity(1).Should().BeTrue();
		storage.GetView().Contains(1).Should().BeTrue();
		storage.GetCount().Should().Be(1);
		storage.RemoveComponentFromStorage(1).Should().BeTrue();
		storage.GetView().Contains(1).Should().BeFalse();
		storage.GetCount().Should().Be(0);
	}
}

public class StorageTypesTestData : IEnumerable<object[]>
{
	public IEnumerator<object[]> GetEnumerator()
	{
		yield return new object[] {new DoubleListStorage<SampleComponent>()};
		yield return new object[] {new SparseLinearDictionaryStorage<SampleComponent>()};
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
}