using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Ignis;
using Ignis.Containers;
using Ignis.Storage;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
public class ContainerTest : IDisposable
{
	private readonly IContainer<object> container;

	public ContainerTest()
	{
		container = new MicroResolverContainer<object>();
	}

	public void Dispose()
	{
		container.Dispose();
	}

	[Fact]
	public void ShouldRegisterAndExecuteComponentsAndSystems()
	{
		container.IsBuilt().Should().BeFalse();
		container
			.AddComponent<SampleComponent>()
			.AddSystem<SampleSystem>()
			.Build();
		container.IsBuilt().Should().BeTrue();

		container.GetSystemTypes().Should().BeEquivalentTo(typeof(SampleSystem));
		container.GetComponentTypes().Should().BeEquivalentTo(typeof(SampleComponent));

		var storage = container.GetStorageFor<SampleComponent>();
		storage.Should().NotBeNull();
		Assert.NotNull(container.GetStorageFor(typeof(SampleComponent)));
		container.Resolve<IComponentCollection<SampleComponent>>().Should().BeSameAs(storage);

		var sampleSystem = container.GetSystem<SampleSystem>();
		sampleSystem.Should().NotBeNull();
		sampleSystem.Container.Should().Be(container);
		sampleSystem.EntityManager.Should().Be(container.EntityManager);
		Assert.NotNull(container.GetSystem(typeof(SampleSystem)));

		var entityManager = container.EntityManager;
		var entityCount = 5;
		var entityIds = Enumerable.Range(0, entityCount)
		                          .Select(i => entityManager.Create())
		                          .ToList();

		entityIds.ForEach(id => container.AddComponent<SampleComponent, object>(id));

		// Initial state
		storage.GetCount().Should().Be(entityCount);
		storage.GetValues().Should().OnlyContain(c => c.Foo == 0.0f && c.Bar == false);

		// Execute the system once
		var pairs = new Dictionary<int, SampleComponent>();
		container.InitializeSystems();
		container.ExecuteSystems();
		storage.ForEach((id, val) => pairs.Add(id, val));

		pairs.AsEnumerable()
		     .Should()
		     .OnlyContain(kvp =>
			                  kvp.Value.Foo == kvp.Key && kvp.Value.Bar == true);

		// Execute the system again and observe changes
		pairs.Clear();
		container.ExecuteSystems();
		storage.ForEach((id, val) => pairs.Add(id, val));
		pairs.AsEnumerable()
		     .Should()
		     .OnlyContain(kvp =>
			                  kvp.Value.Foo == kvp.Key + 1 && kvp.Value.Bar == false);
	}

	[Fact]
	public void ShouldSetComponentValueIfSupplied()
	{
		container
			.AddComponent<SampleComponent>()
			.Build();
		var em = container.EntityManager;
		var id = em.Create();
		var storage = container.GetStorageFor<SampleComponent>();
		em.HasComponent<SampleComponent>(id).Should().BeFalse();

		var expected = new SampleComponent()
		{
			Foo = -1f,
			Bar = !default(bool)
		};
		container.AddComponent(id, expected);
		em.HasComponent<SampleComponent>(id).Should().BeTrue();
		storage.Get(id).Should().Be(expected);
	}

	[Fact]
	public void ShouldRegisterUsingNonGenericOverloads()
	{
		container
			.Register(typeof(DoubleListStorage<SampleComponent>))
			.Register(typeof(SampleSystem))
			.Build();

		container.GetSystemTypes().Should().BeEquivalentTo(typeof(SampleSystem));
		container.GetComponentTypes().Should().BeEquivalentTo(typeof(SampleComponent));
		container.Resolve(typeof(SampleSystem)).Should().BeOfType<SampleSystem>();
	}
}

public class SampleSystem : SystemBase<object>
{
	private IComponentCollection<SampleComponent> sampleComponent;

	public SampleSystem(ContainerProvider<object> provider) : base(provider)
	{
		sampleComponent = _container.GetStorageFor<SampleComponent>();
	}

	public override void Execute(object state)
	{
		sampleComponent.ForEach((id, val) =>
		{
			var newValue = new SampleComponent()
			{
				Foo = val.Bar ? id + 1 : id,
				Bar = !val.Bar
			};
			sampleComponent.UpdateCurrent(newValue);
		});
	}
}
}