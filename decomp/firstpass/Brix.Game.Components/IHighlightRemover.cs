using Brix.Engine;

namespace Brix.Game.Components;

public interface IHighlightRemover
{
	void At(XYZ pos);

	void Request(int i);
}
