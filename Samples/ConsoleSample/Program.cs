using System;
using System.Numerics;
using System.Threading;
using ANSITerm;
using ConsoleSample.Components;
using ConsoleSample.Systems;
using Ignis;
using Ignis.Containers;

namespace ConsoleSample
{
internal class Program
{
	private static Random _rnd = new Random();

	private static void Main(string[] args)
	{
		// prepare terminal
		var term = ANSIConsole.GetInstance();
		term.SetFullscreen(true);
		term.CursorVisible = false;
		term.Clear();

		// create an ECS and run the loop
		var ecs = ConfigureECS();
		SpawnEntities(ecs, term);
		RunLoop(term, ecs);

		// roll back our changes
		term.SetFullscreen(false);
		term.CursorVisible = true;
	}

	private static IContainer<GameState> ConfigureECS()
	{
		return ContainerFactory.CreateMicroResolverContainer<GameState>()
		                       .AddComponent<PhysicsObject>()
		                       .AddComponent<Drawable>()
		                       .AddSystem<PhysicsSystem>()
		                       .AddSystem<RenderingSystem>()
		                       .Build();
	}

	private static char[] s_symbols = new char[] {'O', 'X'};

	private static void SpawnEntities(IContainer<GameState> ecs, IConsoleBackend term)
	{
		for (var i = 0; i < 50; i++)
			SpawnRandomEntity(ecs, term);
	}

	private static int SpawnRandomEntity(IContainer<GameState> ecs, IConsoleBackend term)
	{
		var em = ecs.EntityManager;
		var id = em.Create(); // create an entity ID
		var screenSize = new Vector2(term.WindowWidth, term.WindowHeight);
		var maxSpeed = new Vector2(10f);

		// add physics component
		var position = RandomVector(Vector2.Zero, screenSize);
		var velocity = RandomVector(-maxSpeed, maxSpeed);
		ecs.AddComponent(id, new PhysicsObject(position, velocity));

		// add drawable component
		var symbol = RandomElement(s_symbols);
		var color = RandomEnumValue<Color16>();
		ecs.AddComponent(id, new Drawable(symbol, color));

		return id;
	}

	private static void RunLoop(IConsoleBackend term, IContainer<GameState> ecs)
	{
		const int msPerFrame = 20;
		var width = term.WindowWidth;
		var height = term.WindowHeight;
		var state = new GameState(msPerFrame / 1000f, width, height, term);
		// initialize
		ecs.InitializeSystems(state);

		// run
		while (true)
		{
			// Note - in a real game update and rendering logic
			// is often separated - there is no requirement
			// for putting the rendering logic inside ECS -
			// since it's a DI container, you can freely
			// resolve needed components and systems from
			// external code and use them in any other way
			ecs.ExecuteSystems(state);
			Thread.Sleep(20);
			if (term.KeyAvailable)
				break; // exit on any key press
		}
	}

	// helper methods

	private static float RandomFloat(float min, float max)
	{
		var normalized = (float) _rnd.NextDouble();
		return normalized * (max - min) + min;
	}

	private static Vector2 RandomVector(Vector2 min, Vector2 max)
	{
		return new Vector2(RandomFloat(min.X, max.X), RandomFloat(min.Y, max.Y));
	}

	private static T RandomElement<T>(T[] elements)
	{
		return elements[_rnd.Next(0, elements.Length)];
	}

	private static T RandomEnumValue<T>()
		where T : Enum
	{
		var elements = Enum.GetValues(typeof(T));
		return (T) elements.GetValue(_rnd.Next(0, elements.Length));
	}
}
}