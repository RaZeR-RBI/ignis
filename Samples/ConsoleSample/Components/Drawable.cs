using ANSITerm;

namespace ConsoleSample.Components
{
public struct Drawable
{
	public char Symbol;
	public ColorValue Color;

	public Drawable(char symbol, Color16 color)
	{
		Symbol = symbol;
		Color = new ColorValue(color);
	}
}
}