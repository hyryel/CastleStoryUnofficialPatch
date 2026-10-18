using System.Collections.Generic;
using Brix.External.Signals;

namespace Brix.Game.Semantique;

public interface IRecepteur
{
	IReadOnlySignal Changed { get; }

	HashSet<IDescriptor> StoredItems { get; }

	Description SelfDescription { get; }

	Description ContentDescription { get; }

	Description BaseCapacity { get; }

	Description BaseCapacityWithExclusions { get; }

	Description CurrentCapacity { get; }

	bool AcceptMultipleResourceType { get; }
}
