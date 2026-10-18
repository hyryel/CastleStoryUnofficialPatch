namespace Brix.Input.WorldEditor;

public interface ICreateIsland
{
	bool DontCreate { get; set; }

	bool DontLower { get; set; }

	bool DontRaise { get; set; }
}
