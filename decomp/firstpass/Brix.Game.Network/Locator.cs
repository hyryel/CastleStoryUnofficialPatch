namespace Brix.Game.Network;

public abstract class Locator<T>
{
	public interface Handle
	{
		T Value { get; }
	}
}
