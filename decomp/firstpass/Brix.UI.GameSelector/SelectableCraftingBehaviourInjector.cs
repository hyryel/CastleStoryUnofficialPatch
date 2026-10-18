using UnityEngine;

namespace Brix.UI.GameSelector;

public class SelectableCraftingBehaviourInjector : SelectableBehaviourInjector
{
	[SerializeField]
	private SelectableCraftingStationBehaviour unitBehaviour;

	protected override SelectableBehaviour behaviour => unitBehaviour;
}
