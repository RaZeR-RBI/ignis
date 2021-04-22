using ANSITerm;

namespace ConsoleSample
{
public class GameState
{
	public float DeltaSeconds { get; set; }
	public int ScreenWidth { get; set; }
	public int ScreenHeight { get; set; }
	public IConsoleBackend Backend { get; set; }

	public GameState(float deltaSeconds, int screenWidth, int screenHeight, IConsoleBackend backend)
	{
		DeltaSeconds = deltaSeconds;
		ScreenWidth = screenWidth;
		ScreenHeight = screenHeight;
		Backend = backend;
	}
}
}