using System;
using Brix.External.Factories;
using Brix.Game.AI;
using Brix.Game.AI.Damage;
using Brix.Game.Locomotion;
using Brix.Game.Semantique;
using UnityEngine;

namespace Brix.Engine.Nature;

public abstract class NatureData
{
	public static readonly NullNatureData NullNature = new NullNatureData();

	public const float MinGrowth = 0f;

	public const float MaxGrowth = 1f;

	protected float _growth;

	public virtual XYZ RootPos { get; set; }

	public virtual Vector3 GoalPosition => RootPos.ToVector3();

	public virtual Quaternion Orientation => Quaternion.identity;

	public virtual float Growth
	{
		get
		{
			return _growth;
		}
		set
		{
			_growth = Mathf.Clamp(value, 0f, 1f);
		}
	}

	public virtual Accessoire ToolToHarvest => Adjectif.mineralHarvesting;

	public abstract NatureType Type { get; }

	public bool IsValid => Type != NatureType.none;

	public virtual int MaxRessources => 1;

	public virtual int CurrentRessources
	{
		get
		{
			return 1;
		}
		set
		{
		}
	}

	public virtual IDamageReceiver DamageReceiver => NatureDamageReceiver.Make(RootPos);

	public virtual Factory.AssetKey GetGoalAsset()
	{
		return Test.SmallHarvestGoal;
	}

	public bool Registered()
	{
		return NatureRegistry.HasNatureAt(RootPos);
	}

	public Working.WorkStates WorkAnim()
	{
		if (Type == NatureType.plant)
		{
			return Working.WorkStates.Harvest;
		}
		return Working.WorkStates.Pick;
	}

	public Factory.AssetKey RepresentativeKey()
	{
		return Type switch
		{
			NatureType.boulder => RawResources.RawStone, 
			NatureType.iron => RawResources.RawIron, 
			NatureType.pyrite => RawResources.RawOrangeCrystal, 
			NatureType.plant => RawResources.RawFiber, 
			NatureType.none => null, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	public int ExpectedQte()
	{
		switch (Type)
		{
		case NatureType.boulder:
			return 4;
		case NatureType.iron:
		case NatureType.pyrite:
			return 1;
		case NatureType.plant:
			return 2;
		case NatureType.none:
			return 0;
		default:
			return 10;
		}
	}

	public Ressource RepresentativeRessource()
	{
		return Apparence.PrincipaleOf(Factory.Peek(RepresentativeKey()));
	}

	public override int GetHashCode()
	{
		return RootPos.GetHashCode() ^ ((int)Type/*cast due to .constrained prefix*/).GetHashCode();
	}
}
