using System;

namespace Ignis
{
/// <summary>
/// Describes an entity-related event with a supplied entity ID.
/// </summary>
public struct EntityIdEventArgs
{
	/// <summary>
	/// Entity ID that corresponds to this event.
	/// </summary>
	/// <value></value>
	public int EntityID { get; }

	/// <summary>
	/// Public constructor.
	/// </summary>
	public EntityIdEventArgs(int id)
	{
		EntityID = id;
	}
}
}