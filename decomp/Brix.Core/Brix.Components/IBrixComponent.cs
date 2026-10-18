using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Lifecycle.Pooling;
using UnityEngine;

namespace Brix.Components;

public interface IBrixComponent : IPoolableComponent, IInitializable, IRequestable, IResetable
{
	string Name { get; set; }

	IReadOnlySignal<GameObject> OnReleaseOrDestroySignal { get; }

	IReadOnlySignal OnReleaseSignal { get; }

	IReadOnlySignal OnDestroySignal { get; }

	bool Initialized { get; }

	bool Released { get; }

	bool Destroyed { get; }

	BrixComponentPoco BrixComponentPoco { get; }
}
