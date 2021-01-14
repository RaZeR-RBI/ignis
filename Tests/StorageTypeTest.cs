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
			em.AddComponent<SampleComponent>(entityWithComponent);
			withComponents.Add(entityWithComponent);
		}

		// Check if components were added where necessary
		withComponents.Should().OnlyContain(id => em.HasComponent<SampleComponent>(id));
		withoutComponents.Should().OnlyContain(id => !em.HasComponent<SampleComponent>(id));

		// Try to iterate using foreach
		var count = 0;
		foreach (var component in storage)
			count++;
		count.Should().Be(withComponents.Count);

		// Try to iterate and modify
		count = 0;
		foreach (var component in storage)
		{
			var value = new SampleComponent()
			{
				Foo = 1337,
				Bar = true
			};
			storage.UpdateCurrent(value);
			count++;
		}

		count.Should().Be(withComponents.Count);

		// Check if they were modified
		storage.Should().OnlyContain(c => c.Foo == 1337);
		storage.Should().OnlyContain(c => c.Bar == true);

		// Modify again and delete component from every second entity
		var iterations = 0;
		count = 0;
		storage.ForEach((id, componentValue) =>
		{
			componentValue.Foo = id;
			storage.UpdateCurrent(componentValue);
			if (iterations % 2 == 1)
				em.RemoveComponent<SampleComponent>(id);
			else
				count++;
			iterations++;
		});
		iterations.Should().Be(withComponents.Count);
		var oddsAndEvens = withComponents
		                   .Select((item, i) => (item: item, isEven: i % 2 == 0))
		                   .ToLookup(x => x.isEven, x => x.item);

		// Check if the components were removed
		oddsAndEvens[true].Should().OnlyContain(id => em.HasComponent<SampleComponent>(id));
		oddsAndEvens[false].Should().OnlyContain(id => !em.HasComponent<SampleComponent>(id));

		// Check if the alive ones was updated
		storage.ForEach((id, val) =>
		{
			val.Foo.Should().Be(id);
			val.Bar.Should().BeTrue();
		});

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
		foreach (var component in storage)
			count++;
		count.Should().Be(0);

		count = 0;
		storage.ForEach((id, val) => count++);
		count.Should().Be(0);
	}

	[Fact]
	public void ShouldThrowExceptionOnNullStorageAccess()
	{
		var storage = new NullStorage<SampleComponent>();
		var actions = new List<Action>
		{
			() => storage.ForEach((id, val) => { }),
			() => storage.Get(-1),
			() => storage.GetEnumerator(),
			() => storage.Update(-1, new SampleComponent()),
			() => storage.Update(-1, new object()),
			() => storage.UpdateCurrent(new SampleComponent())
		};
		foreach (var action in actions)
			Assert.ThrowsAny<InvalidOperationException>(action);
		storage.StoreComponentForEntity(1).Should().BeTrue();
		storage.RemoveComponentFromStorage(1).Should().BeTrue();
	}
}

public class StorageTypesTestData : IEnumerable<object[]>
{
	public IEnumerator<object[]> GetEnumerator()
	{
		yield return new object[] {new DoubleListStorage<SampleComponent>()};
		yield return new object[] {new ListOfPairsStorage<SampleComponent>()};
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
}