using System.Numerics;

namespace ConsoleSample.Components
{
public struct PhysicsObject
{
	public Vector2 Position, Velocity;

	public PhysicsObject(Vector2 position, Vector2 velocity)
	{
		Position = position;
		Velocity = velocity;
	}
}
}