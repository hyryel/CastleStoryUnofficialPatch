using Brix.UI.Icons;
using Motus.Behavior;

namespace Brix.Game.AI;

public interface IOrderPackage
{
	IconKey IconKey { get; }

	Labor.DecisionSource DecisionSource { get; }

	StaticNode Prepare(BaseLabor orderReceiver);

	void Send(BaseLabor orderReceiver);

	void Queue(BaseLabor orderReceiver);
}
