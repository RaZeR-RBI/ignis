using System;
using ANSITerm;
using ConsoleSample.Components;
using Ignis;
using Ignis.Containers;
using MicroResolver;

namespace ConsoleSample.Systems
{
public class RenderingSystem : SystemBase<GameState>
{
	[Inject] private IComponentCollection<PhysicsObject> _physObjects = null;

	[Inject] private IComponentCollection<Drawable> _drawables = null;

	private readonly IEntityView _drawableIds;

	public RenderingSystem(ContainerProvider<GameState> ownerProvider) : base(ownerProvider)
	{
		// filter out entities that have both PhysicsObject and Drawable
		_drawableIds = EntityManager.GetView<PhysicsObject, Drawable>();
	}

	public override void Execute(GameState state)
	{
		var term = state.Backend;
		term.BackgroundColor = new ColorValue(Color16.Black);
		term.Clear();
		// enumerate through entity view
		foreach (var id in _drawableIds)
		{
			var position = _physObjects.Get(id).Position;
			position.X = MathF.Round(position.X);
			position.Y = MathF.Round(position.Y);
			var drawable = _drawables.Get(id);

			// draw it
			term.SetCursorPosition((int) position.X, (int) position.Y);
			term.ForegroundColor = drawable.Color;
			term.Write(drawable.Symbol.ToString());
		}
	}
}
}