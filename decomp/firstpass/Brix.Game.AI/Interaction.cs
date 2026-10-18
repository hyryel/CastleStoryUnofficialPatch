using Brix.External.Factories;
using Brix.Game.AI.Behaviors;
using Brix.Game.Components;
using Brix.Utils.Signals;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.AI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Toolbag))]
[RequireComponent(typeof(Occupation))]
public class Interaction : GameComponent
{
	public Brix.Game.AI.Behaviors.AnimationEvent ToolHitEvent;

	private Animator animator;

	private Occupation occupation;

	private Toolbag _toolbag;

	private FloatParameter CarryingMode;

	private Factory.AssetKey CarriedProp;

	private bool firstUpdate = true;

	public TakeType CarriedType
	{
		get
		{
			if (CarriedProp == null)
			{
				return TakeType.None;
			}
			return Factory.Peek(CarriedProp).GetComponent<Grip>().TakeType;
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		animator = GetComponent<Animator>();
		occupation = GetComponent<Occupation>();
		_toolbag = GetComponent<Toolbag>();
		CarryingMode = new FloatParameter(animator, "CarryMode", sendOnNetwork: true);
	}

	public override void OnRequested()
	{
		base.OnRequested();
		if (NetworkServer.active)
		{
			base.recepteur.Changed.Connect(UpdateCarrying).DisconnectWhen(base.OnReleaseOrDestroySignal);
		}
	}

	public bool CanRun()
	{
		TakeType carriedType = CarriedType;
		return carriedType == TakeType.None || carriedType == TakeType.Bag;
	}

	public bool CanClimb()
	{
		TakeType carriedType = CarriedType;
		return carriedType == TakeType.None || carriedType == TakeType.Bag || occupation.Profession.CanClimbWithLoad();
	}

	public bool CanStrike()
	{
		TakeType carriedType = CarriedType;
		return carriedType == TakeType.None || carriedType == TakeType.Bag;
	}

	public void UpdateCarrying()
	{
		Factory.AssetKey assetKey = base.recepteur.CarryType();
		if (NeedsUpdate(assetKey))
		{
			firstUpdate = false;
			ChangeProp(assetKey);
			CarryingMode.Set((float)CarriedType);
		}
	}

	private bool NeedsUpdate(Factory.AssetKey wanted)
	{
		if (firstUpdate)
		{
			firstUpdate = false;
			return true;
		}
		return (wanted == null && CarriedProp != null) || (wanted != null && !_toolbag.HasTool(wanted));
	}

	private void ChangeProp(Factory.AssetKey wantedProp)
	{
		if (wantedProp == null && CarriedProp != null)
		{
			_toolbag.UnequipTool(CarriedProp);
			CarriedProp = null;
		}
		if (wantedProp != null)
		{
			_toolbag.UnequipTool(CarriedProp);
			_toolbag.EquipMagicalTool(wantedProp);
			CarriedProp = wantedProp;
			if (CarriesRequiresRightHand(CarriedType))
			{
				_toolbag.UnequipTool(Slot.RightHand, Tool.OnDropBehaviours.Release);
			}
		}
	}

	private bool CarriesRequiresRightHand(TakeType mode)
	{
		return mode == TakeType.TwoHands || mode == TakeType.Back;
	}
}
