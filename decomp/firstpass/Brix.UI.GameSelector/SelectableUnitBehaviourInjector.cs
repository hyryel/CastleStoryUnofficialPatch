using UnityEngine;

namespace Brix.UI.GameSelector;

public class SelectableUnitBehaviourInjector : SelectableBehaviourInjector
{
	[SerializeField]
	private SelectableUnitsBehaviour unitBehaviour;

	protected override SelectableBehaviour behaviour => unitBehaviour;
}
