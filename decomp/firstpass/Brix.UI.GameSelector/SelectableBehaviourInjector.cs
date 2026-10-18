using UnityEngine;

namespace Brix.UI.GameSelector;

public abstract class SelectableBehaviourInjector : MonoBehaviour
{
	protected abstract SelectableBehaviour behaviour { get; }

	public void Inject(ref SelectableBehaviour destination, UISelectable selectableComponent, bool selectable, bool hoverable, SelectionType type)
	{
		destination = behaviour;
		behaviour.Setup(selectableComponent, selectable, hoverable, type);
	}
}
