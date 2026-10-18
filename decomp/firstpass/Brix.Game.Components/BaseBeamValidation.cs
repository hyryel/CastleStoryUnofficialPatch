using System.Collections.Generic;
using Brix.Common.Collections;
using Brix.Engine;
using Brix.Game.Components.Validators.Graphs;
using Brix.Game.Components.Volumes;
using Brix.Utils;
using Brix.Utils.Collections;
using Smooth.Slinq;

namespace Brix.Game.Components;

public class BaseBeamValidation : BaseRecursiveValidation
{
	private static VPropertyMask _supportSettings = VPropertyMask.logSupport;

	private static VPropertyMask _recursiveSupportSettings = VPropertyMask.beamSupport;

	private static VPropertyMask _solidifierSettings = VPropertyMask.braceSupport;

	private static VPropertyMask _hookSetting = VPropertyMask.canHookBeam;

	public const int MaxSupportDistance = 4;

	public const int HalfSupportDistance = 2;

	public const int BraceDistanceExtension = 2;

	public static Dictionary<XYZ, SupportState> BuildSupportStates(ref ValidationData VData, IEnumerable<ValidationData> validationVolumes = null)
	{
		Dictionary<XYZ, SupportState> dictionary = CollectionPool.RequestDictionary<XYZ, SupportState>();
		foreach (XYZ @case in VData.Volume.GetCases(VProperty.mainSupportReq))
		{
			dictionary.Add(@case, SupportStateFromOrigin(VData, validationVolumes, @case));
		}
		return dictionary;
	}

	private static SupportState SupportStateFromOrigin(ValidationData vData, IEnumerable<ValidationData> validationVolumes, XYZ origin)
	{
		SupportState currentState = SupportState.Unsupported | SupportState.ValidPath;
		List<XYZ> list = vData.Volume.GetCases(VPropertyMask.occupy | VPropertyMask.fantome).Slinq().ToPooledList();
		if (!EndSupportStateAt(ref currentState, ref vData, origin, list, validationVolumes))
		{
			ValidationData regVData = BaseRecursiveValidation.VDataWithPropertyAt(_recursiveSupportSettings, origin);
			ValidationData volVData = BaseRecursiveValidation.ValidationVolumeVDataWithPropertyAt(validationVolumes, _recursiveSupportSettings, origin);
			ValidationData nextBeamVData = BaseRecursiveValidation.PickBestVData(ref regVData, ref volVData);
			if (nextBeamVData.IsNotDefault)
			{
				currentState = RecursiveSupportStateAt(vData, nextBeamVData, validationVolumes);
			}
		}
		else
		{
			currentState &= ~SupportState.Unsupported;
		}
		list.Dispose();
		return currentState;
	}

	private static bool EndSupportStateAt(ref SupportState currentState, ref ValidationData queryMaker, XYZ pos, List<XYZ> hookLocations, IEnumerable<ValidationData> validationVolumes = null, int jumpsMade = 0, bool braceInPath = false)
	{
		SupportState supportState = VUtil.SupportStateAt(pos, _supportSettings, _hookSetting, hookLocations, queryMaker.Faction, validationVolumes);
		if (!queryMaker.Validator.IsEnoughSupport(supportState))
		{
			return false;
		}
		BaseRecursiveValidation.UpdatePathEndDetails(ref currentState, supportState, jumpsMade, 2, 4, braceInPath);
		return true;
	}

	private static SupportState RecursiveSupportStateAt(ValidationData queryMaker, ValidationData nextBeamVData, IEnumerable<ValidationData> validationVolumes)
	{
		List<XYZ> list = queryMaker.Volume.GetCases(VPropertyMask.occupy | VPropertyMask.fantome).Slinq().ToPooledList();
		SupportState supportState = RecursiveStepUpkeep(ref queryMaker, ref nextBeamVData, list, SupportState.Unsupported | SupportState.ValidPath, 0, 4, validationVolumes);
		list.Dispose();
		if ((supportState & (SupportState.NearSupport | SupportState.FarSupport)) != 0)
		{
			supportState &= ~SupportState.Unsupported;
			supportState |= nextBeamVData.SupportStateType;
		}
		return supportState;
	}

	private static SupportState RecursiveStepUpkeep(ref ValidationData queryMaker, ref ValidationData currentObj, List<XYZ> avoidVoxels, SupportState currentState, int jumpsMade, int jumpsLeft, IEnumerable<ValidationData> validationVolumes)
	{
		if (!currentObj.Orientation.IsAlignedTo(queryMaker.Orientation))
		{
			return currentState;
		}
		jumpsMade++;
		jumpsLeft--;
		if (currentObj.GetCurrentState(validationVolumes) == ValidationResult.Invalid)
		{
			currentState &= ~SupportState.ValidPath;
		}
		if (jumpsMade <= 2 && !currentObj.IsBlueprint)
		{
			currentState |= SupportState.ConcreteEnd;
		}
		bool braceInPath = false;
		SupportState solidity = currentObj.GetSolidity(validationVolumes);
		if (queryMaker.Validator.IsEnoughSolidity(solidity))
		{
			braceInPath = true;
			currentState |= solidity;
			if ((solidity & SupportState.CCSolid) == 0 && jumpsMade > 1)
			{
				currentState &= ~SupportState.ConcreteEnd;
			}
		}
		else if (jumpsLeft <= 0)
		{
			return currentState;
		}
		currentState = NextRecursiveStep(ref queryMaker, ref currentObj, avoidVoxels, currentState, jumpsMade, jumpsLeft, validationVolumes, braceInPath);
		return currentState;
	}

	private static SupportState NextRecursiveStep(ref ValidationData queryMaker, ref ValidationData currentObj, List<XYZ> avoidVoxels, SupportState currentState, int jumpsMade, int jumpsLeft, IEnumerable<ValidationData> validationVolumes, bool braceInPath)
	{
		foreach (XYZ item in currentObj.Volume.GetCases(VProperty.mainSupportReq).Slinq().Except(avoidVoxels.Slinq())
			.NoLeak())
		{
			List<XYZ> list = currentObj.Volume.GetCases(VPropertyMask.occupy | VPropertyMask.fantome).Slinq().ToPooledList();
			bool flag = EndSupportStateAt(ref currentState, ref queryMaker, item, list, validationVolumes, jumpsMade, braceInPath);
			list.Dispose();
			if (flag)
			{
				break;
			}
			ValidationData currentObj2 = BaseRecursiveValidation.VDataWithPropertyAt(_recursiveSupportSettings, item);
			if (!currentObj2.IsNotDefault)
			{
				currentObj2 = BaseRecursiveValidation.ValidationVolumeVDataWithPropertyAt(validationVolumes, _recursiveSupportSettings, item);
			}
			if (!currentObj2.IsNotDefault)
			{
				continue;
			}
			currentObj.Volume.GetCases(VPropertyMask.occupy | VPropertyMask.fantome).Slinq().AddTo(avoidVoxels);
			return RecursiveStepUpkeep(ref queryMaker, ref currentObj2, avoidVoxels, currentState, jumpsMade, (!braceInPath || jumpsLeft > 2) ? jumpsLeft : 3, validationVolumes);
		}
		return currentState;
	}

	public static SupportState GetSolidity(ValidationData vData, IEnumerable<ValidationData> validationVolumes)
	{
		foreach (XYZ @case in vData.Volume.GetCases(VProperty.auxiliarySupportReq))
		{
			SupportState supportState = VUtil.SupportStateAt(@case, _solidifierSettings, vData.Faction, validationVolumes);
			if ((supportState & SupportState.Valid) > (SupportState)0 && (supportState & (SupportState.BPSolid | SupportState.CCSolid)) > (SupportState)0)
			{
				return supportState & (SupportState.BPSolid | SupportState.CCSolid);
			}
		}
		return (SupportState)0;
	}

	public static void PropagateRevalidation(ValidationData vData)
	{
		List<IVolume> list = VUtil.ImpactedVolumes(vData.Volume);
		XYZ position = vData.Position;
		list.Slinq().Where((IVolume v, XYZ _current) => v.Position == _current, position).ForEach(delegate(IVolume v)
		{
			v.Validate();
		});
		list.Slinq().Where((IVolume v, XYZ _current) => v.Position != _current, position).ForEach(delegate(IVolume v)
		{
			v.Validate();
		});
		list.Dispose();
	}

	public static List<IVolume> GetOrderedSupportVolumes(ref ValidationData vData)
	{
		List<IVolume> list = CollectionPool.RequestList<IVolume>();
		GraphSupportState graphSupportState = (GraphSupportState)(-2147483648);
		foreach (XYZ @case in vData.Volume.GetCases(VProperty.mainSupportReq))
		{
			IVolume resultVolume;
			GraphSupportState graphSupportState2 = GraphSupportStateAt(ref vData, ref vData, @case, out resultVolume);
			if (resultVolume != null)
			{
				int index = ((graphSupportState > graphSupportState2) ? 1 : 0);
				list.Insert(index, resultVolume);
				graphSupportState = graphSupportState2;
			}
		}
		return list;
	}

	private static GraphSupportState GraphSupportStateAt(ref ValidationData queryMaker, ref ValidationData excludedSide, XYZ at, out IVolume resultVolume)
	{
		IVolume volume = BaseRecursiveValidation.VolumeWithPropertyAt(VProperty.logSupport, at);
		if (volume != null)
		{
			IVolume braceSupport = GetBraceSupport(excludedSide);
			resultVolume = volume;
			return (braceSupport == null) ? GraphSupportState.Near : GraphSupportState.BracedNear;
		}
		IVolume volume2 = BaseRecursiveValidation.VolumeWithPropertyAt(VProperty.beamSupport, at);
		if (volume2 != null)
		{
			foreach (XYZ @case in volume2.GetCases(VProperty.mainSupportReq))
			{
				if (@case != excludedSide.Position)
				{
					ValidationData excludedSide2 = volume2.VData;
					GraphSupportState result = GraphSupportStateAt(ref queryMaker, ref excludedSide2, @case, out resultVolume) - 1;
					resultVolume = volume2;
					return result;
				}
			}
		}
		resultVolume = null;
		return GraphSupportState.Unsupported;
	}

	public static IVolume GetBraceSupport(ValidationData vData)
	{
		foreach (XYZ @case in vData.Volume.GetCases(VProperty.auxiliarySupportReq))
		{
			List<ValidationData> supports = VUtil.GetSupports(@case, VProperty.braceSupport, vData.Faction);
			ValidationData validationData = (from b in supports.Slinq()
				where b.GetCurrentState().HasFlag(ValidationResult.Valid)
				select b).FirstOrDefault();
			if (validationData.IsNotDefault)
			{
				return validationData.Volume;
			}
			if (supports.Count > 0)
			{
				return supports[0].Volume;
			}
		}
		return null;
	}
}
