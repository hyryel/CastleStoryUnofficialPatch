using Brix.Game.Components;
using Brix.Lifecycle.Pooling;
using UnityEngine;
using UnityEngine.Networking;

public class TimedDestruction : UpdatableGameComponent<TimedDestruction>
{
	public float TimeBeforeDestruction;

	private float _destructionTime;

	public override void ResetComponent()
	{
		base.ResetComponent();
		_destructionTime = 0f;
	}

	protected override void BrixSlowUpdate()
	{
		base.BrixSlowUpdate();
		if (_destructionTime <= 0f)
		{
			_destructionTime = Time.time + TimeBeforeDestruction;
		}
		if (Time.time > _destructionTime && !base.gameObject.IsNullOrReleased())
		{
			NetworkIdentity component = base.gameObject.GetComponent<NetworkIdentity>();
			if ((bool)component && !component.netId.IsEmpty())
			{
				NetworkServer.UnSpawn(base.gameObject);
			}
			ObjectPoolSingleton.Release(base.gameObject);
			base.enabled = false;
			_destructionTime = 0f;
		}
	}
}
