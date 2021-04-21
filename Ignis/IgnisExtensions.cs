using System;
using Ignis;

/// <summary>
/// Root namespace.
/// </summary>
namespace Ignis
{
/// <summary>
/// Defines various extensions to the library types.
/// </summary>
public static class IgnisExtensions
{
	/// <summary>
	/// Adds a component to an existing entity.
	/// </summary>
	/// <param name="container">The ECS container</param>
	/// <param name="entityId">Entity ID for which the component should be attached</param>
	/// <typeparam name="T">Type of the component</typeparam>
	/// <typeparam name="TState">Type of the state parameter passed to container</typeparam>
	/// <returns>The ECS container</returns>
	public static IContainer<TState> AddComponent<T, TState>(
		this IContainer<TState> container, int entityId)
		where T : new()
	{
		container.EntityManager.AddComponent<T>(entityId);
		return container;
	}

	/// <summary>
	/// Adds a component to an existing entity and sets it's value.
	/// </summary>
	/// <param name="container">The ECS container</param>
	/// <param name="entityId">Entity ID for which the component should be attached</param>
	/// <param name="value">Component data</param>
	/// <typeparam name="T">Type of the component</typeparam>
	/// <typeparam name="TState">Type of the state parameter passed to container</typeparam>
	/// <returns>The ECS container</returns>
	public static IContainer<TState> AddComponent<T, TState>(
		this IContainer<TState> container, int entityId, T value)
		where T : new()
	{
		var em = container.EntityManager;
		em.AddComponent<T>(entityId);
		container.GetStorageFor<T>().Update(entityId, value);
		return container;
	}

	/// <summary>
	/// Gets an entity view which contains entities with the specified components.
	/// </summary>
	/// <param name="em">Entity manager</param>
	/// <param name="filter">Required component types</param>
	/// <returns>Entity view</returns>
	public static IEntityView GetView(this IEntityManager em, params Type[] filter)
	{
		return em.GetView(filter);
	}

#pragma warning disable HAA0101 // we're aware of the params allocation thing
	/// <summary>
	/// Gets an entity view which contains entities with the specified components.
	/// </summary>
	/// <param name="em">Entity manager</param>
	/// <typeparam name="T1">First component type</typeparam>
	/// <returns>Entity view</returns>
	public static IEntityView GetView<T1>(this IEntityManager em)
	{
		return GetView(em, typeof(T1));
	}

	/// <summary>
	/// Gets an entity view which contains entities with the specified components.
	/// </summary>
	/// <param name="em">Entity manager</param>
	/// <typeparam name="T1">First component type</typeparam>
	/// <typeparam name="T2">Second component type</typeparam>
	/// <returns>Entity view</returns>
	public static IEntityView GetView<T1, T2>(this IEntityManager em)
	{
		return GetView(em, typeof(T1), typeof(T2));
	}

	/// <summary>
	/// Gets an entity view which contains entities with the specified components.
	/// </summary>
	/// <param name="em">Entity manager</param>
	/// <typeparam name="T1">First component type</typeparam>
	/// <typeparam name="T2">Second component type</typeparam>
	/// <typeparam name="T3">Third component type</typeparam>
	/// <returns>Entity view</returns>
	public static IEntityView GetView<T1, T2, T3>(this IEntityManager em)
	{
		return GetView(em, typeof(T1), typeof(T2), typeof(T3));
	}

	/// <summary>
	/// Gets an entity view which contains entities with the specified components.
	/// </summary>
	/// <param name="em">Entity manager</param>
	/// <typeparam name="T1">First component type</typeparam>
	/// <typeparam name="T2">Second component type</typeparam>
	/// <typeparam name="T3">Third component type</typeparam>
	/// <typeparam name="T4">Fourth component type</typeparam>
	/// <returns>Entity view</returns>
	public static IEntityView GetView<T1, T2, T3, T4>(this IEntityManager em)
	{
		return GetView(em, typeof(T1), typeof(T2), typeof(T3), typeof(T4));
	}
}
}