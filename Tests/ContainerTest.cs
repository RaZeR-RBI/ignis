using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Ignis;
using Ignis.Containers;
using Xunit;

namespace Tests
{
    public class ContainerTest
    {
        [Theory]
        [ClassData(typeof(ContainerTestData))]
        public void ShouldRegisterAndExecuteComponentsAndSystems(IContainer container)
        {
            container
                .AddComponent<SampleComponent>()
                .AddSystem<SampleSystem>()
                .Build();

            var storage = container.GetStorageFor<SampleComponent>();
            storage.Should().NotBeNull();
            container.GetSystem<SampleSystem>().Should().NotBeNull();

            var entityManager = container.EntityManager;
            var entityCount = 5;
            var entityIds = Enumerable.Range(0, entityCount)
                .Select(i => entityManager.Create())
                .ToList();

            // Initial state
            storage.Count().Should().Be(entityCount);
            storage.Should().OnlyContain(c => c.Foo == 0.0f && c.Bar == false);

            // Execute the system once
            var pairs = new Dictionary<int, SampleComponent>();
            container.ExecuteSystems();
            storage.ForEach((id, val) => pairs.Add(id, val));

            pairs.AsEnumerable().Should().OnlyContain(kvp =>
                kvp.Value.Foo == kvp.Key && kvp.Value.Bar == true);

            // Execute the system again and observe changes
            pairs.Clear();
            container.ExecuteSystems();
            storage.ForEach((id, val) => pairs.Add(id, val));
            pairs.AsEnumerable().Should().OnlyContain(kvp =>
                kvp.Value.Foo == kvp.Key + 1 && kvp.Value.Bar == false);
        }
    }

    public class SampleSystem : SystemBase
    {
        private IComponentCollection<SampleComponent> sampleComponent;

        public SampleSystem(IContainer owner) : base(owner)
        {
            sampleComponent = owner.GetStorageFor<SampleComponent>();
        }

        public override void Execute()
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

    public class ContainerTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { new MicroResolverContainer() };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}