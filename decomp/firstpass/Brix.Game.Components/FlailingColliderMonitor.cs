using Brix.Components;
using Brix.External.Factories;
using Brix.Lifecycle.Pooling;
using UnityEngine;

namespace Brix.Game.Components;

public class FlailingColliderMonitor : BrixComponent
{
	public Transform characterTransform;

	public void ReparentToCharacterParent()
	{
		if (characterTransform != null)
		{
			base.transform.SetParent(characterTransform.parent, worldPositionStays: true);
			characterTransform.SetParent(base.transform, worldPositionStays: false);
			characterTransform.localRotation = Quaternion.identity;
			characterTransform.localPosition = Vector3.zero;
		}
	}

	public override void OnRequested()
	{
		characterTransform = null;
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		if (characterTransform != null && base.transform.parent != characterTransform && !characterTransform.IsNullOrReleased())
		{
			ObjectPoolSingleton.Release(characterTransform.GetComponent<FactoryImprint>());
		}
	}
}
