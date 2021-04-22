using System;
using System.Diagnostics.CodeAnalysis;

namespace Ignis
{
/// <summary>
/// Describes an entity-related event with supplied EntityID and component type.
/// </summary>
public struct EntityComponentEventArgs
{
	/// <summary>
	/// Entity ID that corresponds to this event.
	/// </summary>
	public int EntityID { get; }

	/// <summary>
	/// Component type that corresponds to this event.
	/// </summary>
	public Type ComponentType { get; }

	/// <summary>
	/// Public constructor.
	/// </summary>
	public EntityComponentEventArgs(int id, Type component)
	{
		(EntityID, ComponentType) = (id, component);
	}
}
}