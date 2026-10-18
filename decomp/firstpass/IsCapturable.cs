using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Game;
using Brix.Game.Components;
using Brix.Game.Components.Tracking;
using Brix.Game.Rules;
using Brix.Utils;
using Brix.Utils.Collections;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(AffiliationComponent))]
public class IsCapturable : UpdatableGameComponent<IsCapturable>
{
	[SerializeField]
	private CaptureClock CaptureClock;

	public int CaptureRange;

	public static GameSignals.GameSignal<IsCapturable> CapturableAdded = new GameSignals.GameSignal<IsCapturable>();

	public static GameSignals.GameSignal<IsCapturable> CapturableRemoved = new GameSignals.GameSignal<IsCapturable>();

	private HashSet<GameObject> collisionsSnapshot = new HashSet<GameObject>();

	public IReadOnlySignal<GameObject> SelfCapturableRemoved => base.OnReleaseOrDestroySignal;

	public Signal<Faction> CaptureStarted => CaptureClock.CaptureStarted;

	public Signal CaptureEnded => CaptureClock.CaptureEnded;

	public Signal<Faction> CapturersChanged => CaptureClock.CapturerChanged;

	public Signal<float> RatioChanged => CaptureClock.CaptureRatioChanged;

	public float IncrementTime => CaptureClock.IncrementTime;

	public float CaptureRatio => CaptureClock.CaptureRatio;

	public Faction CurrentCapturers => CaptureClock.CurrentCapturers;

	public bool DoneCapturing { get; private set; }

	public override void OnRequested()
	{
		base.OnRequested();
		GameSignals.LateConnectOnce(GameSignals.LevelReady, delegate(IsCapturable self)
		{
			self.Init();
		}, this);
		GameSignals.Invoke(CapturableAdded, this);
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		GameSignals.Invoke(CapturableRemoved, this);
	}

	private void Init()
	{
		base.enabled = NetworkServer.active;
	}

	protected override void BrixSlowUpdate()
	{
		if (CanBeCaptured())
		{
			base.BrixSlowUpdate();
			RefreshCaptureArea();
		}
	}

	protected void RefreshCaptureArea()
	{
		List<Faction> list = CollectionPool.RequestList<Faction>();
		(from f in Faction.GetAllFactions().Slinq()
			where f != Faction.DefaultNeutral
			select f).AddTo(list);
		HashSet<GameObject> unitsInCaptureZone = (from t in TrackingRegistry.GetCollisions(base.transform.position, CaptureRange, TrackingLayer.Character, list).Slinq()
			select t.GameObject).ToPooledHashSet();
		list.Dispose();
		CaptureClock.SetUnitsInCaptureZone(unitsInCaptureZone);
		CollectionPool.Release(ref collisionsSnapshot);
		collisionsSnapshot = unitsInCaptureZone;
	}

	public bool CanBeCaptured()
	{
		return GeneralRules.Main.CanCaptureCapturables;
	}
}
