using Brix.External.Signals;

namespace Brix.Game.Names;

public interface INameDispenser
{
	string GetName();

	Signal<string> GetNewNameSignal();

	void Destroy();
}
