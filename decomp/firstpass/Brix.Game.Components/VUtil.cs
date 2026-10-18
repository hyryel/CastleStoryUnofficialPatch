using System;
using System.Collections.Generic;
using Brix.Common.Collections;
using Brix.Engine;
using Brix.Game.Components.Volumes;
using Brix.Legacy;
using Brix.Util;
using Brix.Utils;
using Brix.Utils.Collections;
using Smooth.Slinq;

namespace Brix.Game.Components;

public static class VUtil
{
	public const VPropertyMask SupportingProperties = VPropertyMask.stoneSupport | VPropertyMask.logSupport | VPropertyMask.braceSupport | VPropertyMask.beamSupport | VPropertyMask.plankSupport | VPropertyMask.notWallSupport | VPropertyMask.canHookBeam | VPropertyMask.ropeSupport | VPropertyMask.stickySupport;

	public const VPropertyMask SupportedProperties = VPropertyMask.mainSupportReq | VPropertyMask.auxiliarySupportReq;

	public const VPropertyMask OccupyingProperties = VPropertyMask.occupy | VPropertyMask.fantome;

	public const VPropertyMask SupportProperties = VPropertyMask.stoneSupport | VPropertyMask.logSupport | VPropertyMask.braceSupport | VPropertyMask.beamSupport | VPropertyMask.plankSupport | VPropertyMask.mainSupportReq | VPropertyMask.auxiliarySupportReq | VPropertyMask.notWallSupport | VPropertyMask.canHookBeam | VPropertyMask.ropeSupport | VPropertyMask.stickySupport;

	public const VPropertyMask AllVProperties = (VPropertyMask)(-9223372036854775808L);

	private static Guid _currentVID = Guid.NewGuid();

	private static List<XYZ> ImpactedTerrainPositions = new List<XYZ>
	{
		XYZ.up,
		XYZ.forward,
		-XYZ.forward,
		XYZ.right,
		-XYZ.right
	};

	public static Guid CurrentVID => _currentVID;

	public static Guid NewVID()
	{
		_currentVID = Guid.NewGuid();
		return _currentVID;
	}

	public static List<IVolume> ImpactedVolumes(IVolume volume)
	{
		if (volume == null)
		{
			return CollectionPool.RequestList<IVolume>();
		}
		VolumeMultiPropertyRequest cases = volume.GetCases(VPropertyMask.stoneSupport | VPropertyMask.logSupport | VPropertyMask.braceSupport | VPropertyMask.beamSupport | VPropertyMask.plankSupport | VPropertyMask.notWallSupport | VPropertyMask.canHookBeam | VPropertyMask.ropeSupport | VPropertyMask.stickySupport);
		return (from v in VolumeRegistry.VolumesWithPropertyAt(cases, VPropertyMask.mainSupportReq | VPropertyMask.auxiliarySupportReq).Slinq()
			where v != volume
			select v).ToPooledList();
	}

	public static List<T> ImpactedVolumes<T>(IVolume volume) where T : class, IVolume
	{
		if (volume == null)
		{
			return CollectionPool.RequestList<T>();
		}
		VolumeMultiPropertyRequest cases = volume.GetCases(VPropertyMask.stoneSupport | VPropertyMask.logSupport | VPropertyMask.braceSupport | VPropertyMask.beamSupport | VPropertyMask.plankSupport | VPropertyMask.notWallSupport | VPropertyMask.canHookBeam | VPropertyMask.ropeSupport | VPropertyMask.stickySupport);
		return (from v in VolumeRegistry.VolumesWithPropertyAt(cases, VPropertyMask.mainSupportReq | VPropertyMask.auxiliarySupportReq).Slinq()
			select v as T).NotNull().Where((T v, IVolume excluded) => v != excluded, volume).ToPooledList();
	}

	public static List<IVolume> ImpactedTerrainVolume(IVolume baseVolume)
	{
		XYZ position = baseVolume.Position;
		return ImpactedTerrainPositions.Slinq().Select((XYZ p, XYZ baseP) => Voxel.GetTerrainBlockData(p + baseP).Volume, position).NotNull()
			.ToPooledList();
	}

	public static List<ValidationData> GetSupports(XYZ i, VPropertyMask properties, Faction faction)
	{
		List<ValidationData> list = CollectionPool.RequestList<ValidationData>();
		foreach (ValidationData item in (from v in VolumeRegistry.VolumesWithPropertyAt(i, properties).Slinq()
			select v.VData).NoLeak())
		{
			if (item.IsNotDefault && (!item.IsBlueprint || item.Faction == faction))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static List<ValidationData> GetSupports(XYZ i, VPropertyMask properties, List<ValidationData> toConsider)
	{
		List<ValidationData> list = CollectionPool.RequestList<ValidationData>();
		foreach (ValidationData item in toConsider)
		{
			if (item.IsNotDefault && item.Volume.HasAnyPropertiesAt(properties, i))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static List<ValidationData> GetSupports(XYZ i, VProperty property, Faction faction)
	{
		List<ValidationData> list = CollectionPool.RequestList<ValidationData>();
		foreach (ValidationData item in (from v in VolumeRegistry.VolumesWithPropertyAt(i, property).Slinq()
			select v.VData).NoLeak())
		{
			if (item.IsNotDefault && (!item.IsBlueprint || item.Faction == faction))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static SupportState SupportStateAt(XYZ voxel, VPropertyMask supports, Faction faction, IEnumerable<ValidationData> validationVolumes = null)
	{
		return SupportCheck(voxel, supports, faction, validationVolumes);
	}

	public static SupportState SupportStateAt(XYZ voxel, VPropertyMask hook, List<XYZ> hookLocations, Faction faction, IEnumerable<ValidationData> validationVolumes = null)
	{
		return HookCheck(voxel, hook, hookLocations, faction, validationVolumes);
	}

	public static SupportState SupportStateAt(XYZ voxel, VPropertyMask supports, VPropertyMask hook, List<XYZ> hookLocations, Faction faction, IEnumerable<ValidationData> validationVolumes = null)
	{
		SupportState a = SupportCheck(voxel, supports, faction, validationVolumes);
		SupportState b = HookCheck(voxel, hook, hookLocations, faction, validationVolumes);
		return StrongestSupportOf(a, b);
	}

	private static SupportState SupportCheck(XYZ voxel, VPropertyMask supports, Faction faction, IEnumerable<ValidationData> validationVolumes = null)
	{
		SupportState state = SupportState.Unsupported;
		SupportState state2 = SupportState.Unsupported;
		foreach (IVolume item in VolumeRegistry.VolumesWithPropertyAt(voxel, supports))
		{
			ValidatorComponent component = item.GetComponent<ValidatorComponent>();
			if (component != null)
			{
				SetStateFromVComp(ref state, component, faction, validationVolumes);
				break;
			}
		}
		if (validationVolumes != null)
		{
			foreach (ValidationData validationVolume in validationVolumes)
			{
				using (LongBitsEnumerator longBitsEnumerator = supports.Bits().GetEnumerator())
				{
					while (longBitsEnumerator.MoveNext())
					{
						if (validationVolume.Volume.HasPropertyAt((VProperty)longBitsEnumerator.Current, voxel))
						{
							SetStateFromVData(ref state2, validationVolume, validationVolumes);
							break;
						}
					}
				}
				if (state2 != SupportState.Unsupported)
				{
					break;
				}
			}
		}
		return StrongestSupportOf(state2, state);
	}

	public static SupportState StrongestSupportOf(SupportState a, SupportState b)
	{
		if ((a & (SupportState.BPSupport | SupportState.CCSupport | SupportState.Valid)) > (b & (SupportState.BPSupport | SupportState.CCSupport | SupportState.Valid)))
		{
			return a;
		}
		return b;
	}

	public static SupportState WeakestSupportOf(SupportState a, SupportState b)
	{
		if ((a & (SupportState.BPSupport | SupportState.CCSupport | SupportState.Valid)) > (b & (SupportState.BPSupport | SupportState.CCSupport | SupportState.Valid)))
		{
			return b;
		}
		return a;
	}

	private static SupportState HookCheck(XYZ voxel, VPropertyMask hook, List<XYZ> hookLocations, Faction faction, IEnumerable<ValidationData> validationVolumes = null)
	{
		SupportState state = SupportState.Unsupported;
		SupportState state2 = SupportState.Unsupported;
		bool flag = false;
		foreach (IVolume item in VolumeRegistry.VolumesWithPropertyAt(voxel, VPropertyMask.occupy | VPropertyMask.fantome))
		{
			foreach (XYZ @case in item.GetCases(hook))
			{
				if (hookLocations.Contains(@case))
				{
					ValidatorComponent component = item.GetComponent<ValidatorComponent>();
					if (component != null)
					{
						SetStateFromVComp(ref state, component, faction, validationVolumes);
						flag = true;
					}
				}
				if (flag)
				{
					break;
				}
			}
			if (flag)
			{
				break;
			}
		}
		flag = false;
		if (validationVolumes != null)
		{
			foreach (ValidationData validationVolume in validationVolumes)
			{
				if (validationVolume.Volume.HasAnyPropertiesAt(VPropertyMask.occupy | VPropertyMask.fantome, voxel))
				{
					foreach (XYZ case2 in validationVolume.Volume.GetCases(hook))
					{
						if (hookLocations.Contains(case2))
						{
							SetStateFromVData(ref state2, validationVolume, validationVolumes);
							flag = true;
						}
						if (flag)
						{
							break;
						}
					}
				}
				if (flag)
				{
					break;
				}
			}
		}
		if ((state2 & (SupportState.BPSupport | SupportState.CCSupport | SupportState.Valid)) > (state & (SupportState.BPSupport | SupportState.CCSupport | SupportState.Valid)))
		{
			return state2;
		}
		return state;
	}

	private static void SetStateFromVComp(ref SupportState state, ValidatorComponent vComp, Faction faction, IEnumerable<ValidationData> validationVolumes)
	{
		state &= ~SupportState.Unsupported;
		ValidationResult currentState = vComp.GetCurrentState(validationVolumes);
		if (currentState >= ValidationResult.Valid)
		{
			state |= SupportState.Valid;
		}
		if (!vComp.IsBlueprint)
		{
			state |= SupportState.CCSupport;
		}
		else if (vComp.volume.Faction == faction)
		{
			state |= SupportState.BPSupport;
		}
		state |= vComp.GetSolidity(validationVolumes);
	}

	private static void SetStateFromVData(ref SupportState state, ValidationData vData, IEnumerable<ValidationData> validationVolumes)
	{
		state &= ~SupportState.Unsupported;
		state |= SupportState.Valid;
		if (vData.IsBlueprint)
		{
			state |= SupportState.BPSupport;
		}
		else
		{
			state |= SupportState.CCSupport;
		}
		state |= vData.GetSolidity(validationVolumes);
	}

	public static Dictionary<XYZ, SupportState> GetSupportStates(IEnumerable<XYZ> voxels, VPropertyMask supports, Faction faction, IEnumerable<ValidationData> validationVolumes = null)
	{
		Dictionary<XYZ, SupportState> dictionary = CollectionPool.RequestDictionary<XYZ, SupportState>();
		foreach (XYZ voxel in voxels)
		{
			dictionary.Add(voxel, SupportStateAt(voxel, supports, faction, validationVolumes));
		}
		return dictionary;
	}

	public static bool IsObstructed(XYZ i, VPropertyMask obstructions, Faction faction, IVolume excludedVolume = null)
	{
		using IVoxel voxel = Voxel.GetVoxel(i);
		return (!voxel.IsNull && !voxel.IsCrust && voxel.IsTerrain) || TreeRegistry.Instance.IsNearATree(i) || VolumeRegistry.VolumesWithPropertyAtExcluding(i, obstructions, excludedVolume).Slinq().Where((IVolume v, Faction _faction) => !(v is BlueprintVolume) || v.Faction == _faction, faction)
			.Any();
	}

	public static bool IsObstructed(XYZ i, VPropertyMask obstructions, Faction faction, List<IVolume> excludedVolumes = null)
	{
		using IVoxel voxel = Voxel.GetVoxel(i);
		return (!voxel.IsNull && !voxel.IsCrust && voxel.IsTerrain) || TreeRegistry.Instance.IsNearATree(i) || VolumeRegistry.VolumesWithPropertyAt(i, obstructions).Slinq().Where((IVolume v, List<IVolume> volumes) => !volumes.Contains(v), excludedVolumes)
			.Where((IVolume v, Faction _faction) => !(v is BlueprintVolume) || v.Faction == _faction, faction)
			.Any();
	}
}
