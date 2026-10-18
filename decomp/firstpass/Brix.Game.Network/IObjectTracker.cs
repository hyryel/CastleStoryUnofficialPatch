using UnityEngine;

namespace Brix.Game.Network;

public interface IObjectTracker
{
	GameObject gameObject { get; }

	void ObjectUntracked(GameObject item);

	void OnObjectTracked(GameObject item);

	Transform ContainerFor(GameObject item);
}
