using Assets.Standard_Assets.Brix.Game.Components;
using Brix.External.Signals;
using Brix.Game.AI;
using Brix.Game.Components.Tracking;
using Brix.Game.Network;
using Brix.Utils.Factories;
using Brix.Utils.Signals;
using Brix.Utils.Unity;
using UnityEngine;

namespace Brix.Game.Components;

public class ArtificerStaff : GameComponent
{
	[DisplayRules(DisplayFlags.ReadOnlyAtRuntime)]
	[SerializeField]
	private ArtificerStaffEffects effects;

	[DisplayRules(DisplayFlags.ReadOnlyAtRuntime)]
	[SerializeField]
	private ProjectileWeapon weapon;

	[DisplayRules(DisplayFlags.ReadOnlyAtRuntime)]
	[SerializeField]
	private RegenAura aura;

	[DisplayRules(DisplayFlags.ReadOnlyAtRuntime)]
	[SerializeField]
	private ToolParentTracker gripTracker;

	[DisplayRules(DisplayFlags.ReadOnlyAtRuntime)]
	[SerializeField]
	private TrackingController tracking;

	[DisplayRules(DisplayFlags.ReadOnlyAtRuntime)]
	[SerializeField]
	private int RegenRadius;

	private CharacterState wielder;

	private SignalConnection wielderConnection;

	public override void OnRequested()
	{
		base.OnRequested();
		gripTracker.ObjectNewlyTrackedSignal.Connect(RefreshWielder).DisconnectWhen(base.OnReleaseOrDestroySignal);
		gripTracker.ObjectTrackedByNewParentSignal.Connect(RefreshWielder).DisconnectWhen(base.OnReleaseOrDestroySignal);
		gripTracker.ObjectNewlyUntrackedSignal.Connect(RefreshWielder).DisconnectWhen(base.OnReleaseOrDestroySignal);
		RefreshWielder(null);
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		wielder = null;
		wielderConnection.Disconnect();
	}

	private void RefreshWielder(GameObject ignored)
	{
		CharacterState characterState = gripTracker.TrackingParent().WithComponent((CharacterState s) => s, null);
		if (characterState != wielder)
		{
			wielderConnection.Disconnect();
			if (characterState != null)
			{
				wielderConnection = characterState.NetworkUpdate.Connect(OnWielderChangeState);
			}
			else
			{
				effects.HideAura();
			}
		}
		tracking.SignificantGO = ((!(characterState != null)) ? null : characterState.gameObject);
		wielder = characterState;
	}

	private void OnWielderChangeState()
	{
		if (wielder.Autonomy.IsDefensive)
		{
			ToHealing();
		}
		else
		{
			ToFire();
		}
	}

	private void ToHealing()
	{
		effects.ToHealStone();
		effects.ShowAura(RegenRadius);
		tracking.EffectRange = RegenRadius;
		aura.StarHealing();
		weapon.AmmoCountGetter = 0;
	}

	private void ToFire()
	{
		effects.ToSentinelStone();
		effects.HideAura();
		aura.StopHealing();
		weapon.AmmoCountGetter = int.MaxValue;
	}
}
