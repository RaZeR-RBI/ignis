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
		public EntityViewTest() => em = new EntityManager(StorageResolver);

		private IComponentCollectionStorage StorageResolver(Type type)
		{
			return (IComponentCollectionStorage)Activator.CreateInstance(typeof(NullStorage<>).MakeGenericType(type));
		}

		[Fact]
		public void ShouldUpdateEntityList()
		{
			var view1 = em.CreateView<Component1>();
			var view24 = em.CreateView<Component2, Component4>();
			view1.Filter.Should().BeEquivalentTo(new[] { typeof(Component1) });
			view24.Filter.Should().BeEquivalentTo(new[] { typeof(Component2), typeof(Component4) });

			var entities2 = Enumerable.Range(0, 3).Select(_ => em.Create()).ToList();
			entities2.ForEach(em.AddComponent<Component2>);

			view1.EntityCount.Should().Be(0);
			view1.Should().BeEmpty();
			view24.EntityCount.Should().Be(0);
			view24.Should().BeEmpty();

			const int count1 = 5;
			var entities1 = Enumerable.Range(0, count1).Select(_ => em.Create()).ToList();
			entities1.ForEach(em.AddComponent<Component1>);
			view1.EntityCount.Should().Be(count1);
			view1.Should().BeEquivalentTo(entities1);
			view1.Should().NotContain(entities2);

			view24.EntityCount.Should().Be(0);
			view24.Should().BeEmpty();

			const int count24 = 4;
			var entities24 = Enumerable.Range(0, count24).Select(_ => em.Create()).ToList();
			entities24.ForEach(id =>
			{
				em.AddComponent<Component2>(id);
				em.AddComponent<Component4>(id);
			});

			view1.EntityCount.Should().Be(count1);
			view1.Should().BeEquivalentTo(entities1);
			view1.Should().NotContain(entities2);

			view24.EntityCount.Should().Be(count24);
			view24.Should().BeEquivalentTo(entities24);
			view24.Should().NotContain(entities2);

			var view2 = em.CreateView<Component2>();
			view2.EntityCount.Should().Be(entities2.Count + entities24.Count);
			view2.Should().BeEquivalentTo(entities2.Concat(entities24));

			entities2.ForEach(em.Destroy);
			view2.EntityCount.Should().Be(count24);
			view2.Should().BeEquivalentTo(entities24);
			view24.EntityCount.Should().Be(count24);
			view24.Should().BeEquivalentTo(entities24);
		}

		public struct Component1 { }
		public struct Component2 { }
		public struct Component3 { }
		public struct Component4 { }

	}
}