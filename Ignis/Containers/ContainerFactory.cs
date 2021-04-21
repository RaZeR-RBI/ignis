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
	/// Creates a MicroResolver-based ECS container.
	/// </summary>
	/// <typeparam name="TState">Type of state parameter that gets passed to systems.</typeparam>
	public static IContainer<TState> CreateMicroResolverContainer<TState>()
	{
		return new MicroResolverContainer<TState>();
	}
}
}