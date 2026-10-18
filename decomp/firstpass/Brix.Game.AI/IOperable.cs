using Brix.External.Signals;
using Brix.UI.Icons;
using Motus.Behavior;
using UnityEngine;

namespace Brix.Game.AI;

public interface IOperable<TLabor> where TLabor : BaseLabor
{
	IconKey Icon { get; }

	IReadOnlySignal<GameObject> OnReleaseOrDestroySignal { get; }

	GameObject gameObject { get; }

	bool IsStillAlive { get; }

	ProjectPriority PriorityWeights { get; }

	ProjectPriority.Bias PriorityBias { get; set; }

	int MaximumCrewSize { get; }

	Labor Operator { get; set; }

	ConflictResolver ConflictResolver { get; }

	Node GetInstruction(TLabor labor, Var<OrderPackage<TLabor>> result);

	Node VerifyWorkFor(Labor labor);

	bool HasWorkFor(Labor labor);

	ProjectPriority PriorityInput(Labor labor);

	int CrewCount();

	int GlobalCrewCount();

	bool Hire(TLabor labor);

	void Fire(TLabor labor);
}
