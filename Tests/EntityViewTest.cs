using System;
using System.Linq;
using FluentAssertions;
using Ignis;
using Ignis.Storage;
using Xunit;

namespace Tests
{
public class EntityViewTest
{
	private readonly IEntityManager em;

	public EntityViewTest()
	{
		em = new EntityManager(StorageResolver);
	}

	private IComponentCollectionStorage StorageResolver(Type type)
	{
		return (IComponentCollectionStorage) Activator.CreateInstance(
		typeof(NullStorage<>).MakeGenericType(type));
	}

	[Fact]
	public void ShouldPrepopulateEntityIds()
	{
		for (var i = 1; i < 5; i++)
		{
			em.Create(i);
			em.AddComponent<Component1>(i);
		}

		var view = em.GetView<Component1>();
		view.AsEnumerable().Should().BeEquivalentTo(Enumerable.Range(1, 4));
	}

	[Fact]
	public void ShouldUpdateEntityList()
	{
		var view1 = em.GetView<Component1>();
		var view24 = em.GetView<Component2, Component4>();
		var _view1 = view1.AsEnumerable();
		var _view24 = view24.AsEnumerable();

		view1.Filter.Should().BeEquivalentTo(new[] {typeof(Component1)});
		view24.Filter.Should().BeEquivalentTo(new[] {typeof(Component2), typeof(Component4)});

		var entities2 = Enumerable.Range(0, 3).Select(_ => em.Create()).ToList();
		entities2.ForEach(em.AddComponent<Component2>);

		view1.EntityCount.Should().Be(0);
		_view1.Should().BeEmpty();
		view24.EntityCount.Should().Be(0);
		_view24.Should().BeEmpty();

		const int count1 = 5;
		var entities1 = Enumerable.Range(0, count1).Select(_ => em.Create()).ToList();
		entities1.ForEach(em.AddComponent<Component1>);
		view1.EntityCount.Should().Be(count1);
		_view1.Should().BeEquivalentTo(entities1);
		_view1.Should().NotContain(entities2);
		_view1.Should().OnlyContain(id => view1.Contains(id));
		view1.Contains(-1).Should().BeFalse();

		Span<int> copyTarget = stackalloc int[10];
		var copied = view1.CopyTo(copyTarget);
		copied.Length.Should().Be(count1);
		copied.ToArray().Should().BeEquivalentTo(entities1);

		view24.EntityCount.Should().Be(0);
		_view24.Should().BeEmpty();

		const int count24 = 4;
		var entities24 = Enumerable.Range(0, count24).Select(_ => em.Create()).ToList();
		entities24.ForEach(id =>
		{
			em.AddComponent<Component2>(id);
			em.AddComponent<Component4>(id);
		});

		view1.EntityCount.Should().Be(count1);
		_view1.Should().BeEquivalentTo(entities1);
		_view1.Should().NotContain(entities2);

		view24.EntityCount.Should().Be(count24);
		_view24.Should().BeEquivalentTo(entities24);
		_view24.Should().NotContain(entities2);

		var view2 = em.GetView<Component2>();
		view2.EntityCount.Should().Be(entities2.Count + entities24.Count);
		view2.AsEnumerable().Should().BeEquivalentTo(entities2.Concat(entities24));

		entities2.ForEach(em.Destroy);
		view2.EntityCount.Should().Be(count24);
		view2.AsEnumerable().Should().BeEquivalentTo(entities24);
		view24.EntityCount.Should().Be(count24);
		view24.AsEnumerable().Should().BeEquivalentTo(entities24);
	}

	[Fact]
	public void ShouldCacheViews()
	{
		var view1 = em.GetView<Component1>();
		var sameView1 = em.GetView<Component1>();
		var view2 = em.GetView<Component1, Component2>();
		var sameView2 = em.GetView<Component1, Component2>();
		var view3 = em.GetView<Component1, Component2, Component3>();
		var sameView3 = em.GetView<Component1, Component2, Component3>();
		var view4 = em.GetView<Component1, Component2, Component3, Component4>();
		var sameView4 = em.GetView<Component1, Component2, Component3, Component4>();

		Assert.Same(view1, sameView1);
		Assert.Same(view2, sameView2);
		Assert.Same(view3, sameView3);
		Assert.Same(view4, sameView4);

		em.DestroyView(typeof(Component4)).Should().BeFalse();
		em.DestroyView(typeof(Component1)).Should().BeTrue();
		var newView1 = em.GetView<Component1>();
		newView1.Should().NotBeSameAs(view1);
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
}
}