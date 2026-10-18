using System.Collections.Generic;
using Brix.External.Signals;
using Brix.Game.Components;

namespace Brix.Game.Network;

public interface IBlueprintTracker : IObjectTracker
{
	IReadOnlySignal<Blueprint> BlueprintAddedSignal { get; }

	IReadOnlySignal<Blueprint> BlueprintRemovedSignal { get; }

	HashSet<Blueprint> TrackedBlueprints { get; }
}
