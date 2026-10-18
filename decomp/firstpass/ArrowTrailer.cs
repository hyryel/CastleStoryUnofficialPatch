using Brix.Components;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.AI;
using Brix.Game.Rules;
using Brix.Lifecycle.Pooling;
using Brix.Rendering;
using UnityEngine;

public class ArrowTrailer : BrixComponent
{
	[SerializeField]
	private MagicProjectile magic;

	[SerializeField]
	private KinematicProjectile arrow;

	[SerializeField]
	private Factory.AssetKey trailAssetKey;

	[SerializeField]
	private Transform trailHolder;

	private TrailRendererDriver trailRendererDriver;

	private TrailRenderer trail;

	private SignalConnection con1;

	private SignalConnection con2;

	public override void OnRequested()
	{
		base.OnRequested();
		ObjectPoolSingleton.Request(out var go, trailAssetKey, trailHolder.position, trailHolder.rotation);
		trailRendererDriver = go.GetComponentInChildren<TrailRendererDriver>();
		trail = go.GetComponentInChildren<TrailRenderer>();
		trail.transform.SetParent(trailHolder, worldPositionStays: true);
		trail.time = 1f / CombatRules.ArrowSpeedMultiplier;
		con1 = magic.ReachedDestination.ConnectOnce(delegate
		{
			UnparentTrail();
		});
		con2 = arrow.ImpactSignal.ConnectOnce(delegate(Vector3 v)
		{
			UnparentTrail();
			trail.transform.position = v;
		});
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		if (trailRendererDriver != null)
		{
			UnparentTrail();
		}
	}

	private void DisconnectSignals()
	{
		con1.Disconnect();
		con2.Disconnect();
	}

	private void UnparentTrail()
	{
		DisconnectSignals();
		trailRendererDriver.UnparentTrail();
		trailRendererDriver = null;
	}
}
