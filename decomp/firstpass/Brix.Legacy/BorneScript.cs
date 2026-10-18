using Brix.Components;
using Brix.Lifecycle.Pooling;
using Brix.Utils.Unity;
using UnityEngine;

namespace Brix.Legacy;

public class BorneScript : BrixComponent
{
	[DisplayRules(DisplayFlags.ReadOnly | DisplayFlags.HideInEditMode)]
	public float DeleteAt;

	public float TTL;

	public override void OnRequested()
	{
		base.OnRequested();
		DeleteAt = Time.time + TTL;
	}

	private void Update()
	{
		if (DeleteAt < Time.time)
		{
			ObjectPoolSingleton.Release(base.gameObject);
		}
	}
}
