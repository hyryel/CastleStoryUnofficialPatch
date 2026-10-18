using UnityEngine;

namespace Brix.UI.GameSelector;

public class SelectableWardsBehaviourInjector : SelectableBehaviourInjector
{
	[SerializeField]
	private SelectableWardsBehaviour wardBehaviour;

	protected override SelectableBehaviour behaviour => wardBehaviour;
}
