using Brix.Components;
using Brix.Game;
using Brix.Game.AI.Damage;
using Brix.Game.Components;
using Brix.Rendering;
using Brix.Utils.Signals;
using Smooth.Slinq;
using UnityEngine;

public class CrystalDriver : BrixComponent
{
	[SerializeField]
	private CrystalVisuals crystal;

	[SerializeField]
	private AffiliationComponent affiliation;

	[SerializeField]
	private DamageReceiver damageReceiver;

	[SerializeField]
	private IsCapturable capturable;

	private Journee clock;

	protected override void Awake()
	{
		base.Awake();
		ConnectToSignals();
		GetComponents<ShareMaterialPropertyBlock>().Slinq().ForEach(delegate(ShareMaterialPropertyBlock c)
		{
			c.sourceRenderer.material.SetVector("_TransitionPoint", base.transform.position - base.transform.parent.up * crystal.TransitionPointOffset);
		});
	}

	protected override void Start()
	{
		base.Start();
		crystal.DriverGameObject = base.gameObject;
		crystal.CrystalColor = affiliation.faction.CrystalColor;
		crystal.ProgressSpeed = capturable.IncrementTime;
	}

	private void ConnectToSignals()
	{
		affiliation.OnFactionChanged.Connect(delegate(Faction oldFaction, Faction newFaction)
		{
			crystal.FactionChanged(oldFaction, newFaction);
		}).DisconnectWhen(base.OnReleaseSignal);
		damageReceiver.HasReceivedDamage.Connect(delegate
		{
			crystal.ShakeOnce();
		}).DisconnectWhen(base.OnReleaseSignal);
		capturable.RatioChanged.Connect(delegate(float ratio)
		{
			crystal.CaptureProgress = ratio;
		}).DisconnectWhen(base.OnReleaseSignal);
		capturable.CaptureStarted.Connect(delegate(Faction faction)
		{
			crystal.StartTransition(faction, affiliation.faction);
		}).DisconnectWhen(base.OnReleaseSignal);
		capturable.CaptureEnded.Connect(delegate
		{
			crystal.EndTransition();
		}).DisconnectWhen(base.OnReleaseSignal);
	}
}
