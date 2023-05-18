/// <summary>
/// Contains container-related functionality.
/// </summary>

namespace Ignis.Containers
{
/// <summary>
/// Allows for creation of ECS containers.
/// </summary>
public static class ContainerFactory
{
	/// <summary>
	/// Creates an ECS container.
	/// </summary>
	/// <typeparam name="TState">Type of state parameter that gets passed to systems.</typeparam>
	public static IContainer<TState> CreateContainer<TState>()
	{
		return new MEDIContainer<TState>();
	}
}
}