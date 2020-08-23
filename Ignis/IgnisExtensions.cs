using Ignis;

namespace Ignis
{
	public static class IgnisExtensions
	{
		public static IContainer<TState> AddComponent<T, TState>(this IContainer<TState> container, int entityId)
			where T : new()
		{
			container.EntityManager.AddComponent<T>(entityId);
			return container;
		}

		public static IContainer<TState> AddComponent<T, TState>(this IContainer<TState> container, int entityId, T value)
			where T : new()
		{
			var em = container.EntityManager;
			em.AddComponent<T>(entityId);
			container.GetStorageFor<T>().Update(entityId, value);
			return container;
		}
	}
}