using UnityEngine;

namespace Brix.UI.GameSelector;

public class SelectableSiegeBehaviourInjector : SelectableBehaviourInjector
{
	[SerializeField]
	private SelectableSiegeBehaviour siegeBehaviour;

	protected override SelectableBehaviour behaviour => siegeBehaviour;
}
