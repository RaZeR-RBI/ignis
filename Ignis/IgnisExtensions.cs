using System;
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

		public static IEntityView CreateView(this IEntityManager em, params Type[] filter)
		{
			return new EntityView(em, filter);
		}

		public static IEntityView CreateView<T1>(this IEntityManager em) =>
			CreateView(em, typeof(T1));

		public static IEntityView CreateView<T1, T2>(this IEntityManager em) =>
			CreateView(em, typeof(T1), typeof(T2));

		public static IEntityView CreateView<T1, T2, T3>(this IEntityManager em) =>
			CreateView(em, typeof(T1), typeof(T2), typeof(T3));

		public static IEntityView CreateView<T1, T2, T3, T4>(this IEntityManager em) =>
			CreateView(em, typeof(T1), typeof(T2), typeof(T3), typeof(T4));
	}
}