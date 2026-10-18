using System.Collections.Generic;
using Brix.Common.Collections;
using Brix.Engine;
using Brix.External.Factories;
using Brix.Game.Components.Validators.Graphs;
using Brix.Game.Components.Volumes;
using Brix.Utils;
using Brix.Utils.Collections;
using Smooth.Slinq;

namespace Brix.Game.Components;

public class BasePlankValidation : BaseRecursiveValidation
{
	private static VPropertyMask _supportSettings = VPropertyMask.logSupport;

	private static VPropertyMask _sameVoxelSupportSettings = VPropertyMask.beamSupport;

	private static VPropertyMask _recursiveSupportSettings = VPropertyMask.plankSupport;

	private const int MaxSupportDistance = 4;

	private const int HalfSupportDistance = 2;

	public static Dictionary<XYZ, SupportState> BuildSupportStates(ValidationData startingPlankVData, IEnumerable<ValidationData> validationVolumes = null)
	{
		Dictionary<XYZ, SupportState> dictionary = CollectionPool.RequestDictionary<XYZ, SupportState>();
		foreach (XYZ @case in startingPlankVData.Volume.GetCases(VProperty.auxiliarySupportReq))
		{
			SupportState currentState = SupportState.Unsupported | SupportState.ValidPath;
			AuxiliaryEndSupportStateAt(ref currentState, startingPlankVData, @case, validationVolumes);
			dictionary.Add(@case, currentState);
		}
		foreach (XYZ case2 in startingPlankVData.Volume.GetCases(VProperty.mainSupportReq))
		{
			dictionary.Add(case2, SupportFromOrigin(startingPlankVData, validationVolumes, case2));
		}
		return dictionary;
	}

	private static bool AuxiliaryEndSupportStateAt(ref SupportState currentState, ValidationData queryMaker, XYZ pos, IEnumerable<ValidationData> validationVolumes, int jumpsMade = 0)
	{
		SupportState supportState = VUtil.SupportStateAt(pos, _sameVoxelSupportSettings, queryMaker.Faction, validationVolumes);
		if (!queryMaker.Validator.IsEnoughSupport(supportState))
		{
			return false;
		}
		BaseRecursiveValidation.UpdatePathEndDetails(ref currentState, supportState, jumpsMade, 2, 4);
		return true;
	}

	private static bool MainEndSupportStateAt(ref SupportState currentState, ValidationData queryMaker, XYZ pos, IEnumerable<ValidationData> validationVolumes, int jumpsMade = 0)
	{
		SupportState supportState = VUtil.SupportStateAt(pos, _supportSettings, queryMaker.Faction, validationVolumes);
		if (!queryMaker.Validator.IsEnoughSupport(supportState))
		{
			return false;
		}
		BaseRecursiveValidation.UpdatePathEndDetails(ref currentState, supportState, jumpsMade, 2, 4);
		return true;
	}

	private static SupportState SupportFromOrigin(ValidationData vData, IEnumerable<ValidationData> validationVolumes, XYZ pos)
	{
		SupportState currentState = (SupportState)0;
		if (!MainEndSupportStateAt(ref currentState, vData, pos, validationVolumes))
		{
			ValidationData regVData = BaseRecursiveValidation.VDataWithPropertyAt(_recursiveSupportSettings, pos);
			ValidationData volVData = BaseRecursiveValidation.ValidationVolumeVDataWithPropertyAt(validationVolumes, _recursiveSupportSettings, pos);
			ValidationData nextPlankVData = BaseRecursiveValidation.PickBestVData(ref regVData, ref volVData);
			if (nextPlankVData.IsNotDefault)
			{
				currentState = RecursiveSupportStateAt(vData, nextPlankVData, validationVolumes, currentState);
			}
		}
		return currentState;
	}

	private static SupportState RecursiveSupportStateAt(ValidationData queryMaker, ValidationData nextPlankVData, IEnumerable<ValidationData> validationVolumes, SupportState state)
	{
		List<XYZ> list = queryMaker.Volume.GetCases(VPropertyMask.occupy | VPropertyMask.fantome).Slinq().ToPooledList();
		state = RecursiveStepUpkeep(queryMaker, nextPlankVData, list, SupportState.Unsupported | SupportState.ValidPath, 0, 4, validationVolumes);
		list.Dispose();
		if ((state & (SupportState.NearSupport | SupportState.FarSupport)) != 0)
		{
			state &= ~SupportState.Unsupported;
			state |= nextPlankVData.SupportStateType;
		}
		return state;
	}

	private static SupportState RecursiveStepUpkeep(ValidationData queryMaker, ValidationData currentObj, List<XYZ> avoidVoxels, SupportState currentState, int jumpsMade, int jumpsLeft, IEnumerable<ValidationData> validationVolumes)
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
		if (jumpsLeft <= 0)
		{
			return currentState;
		}
		currentState = NextRecursiveStep(queryMaker, currentObj, avoidVoxels, currentState, jumpsMade, jumpsLeft, validationVolumes);
		return currentState;
	}

	private static SupportState NextRecursiveStep(ValidationData queryMaker, ValidationData currentObj, List<XYZ> avoidVoxels, SupportState currentState, int jumpsMade, int jumpsLeft, IEnumerable<ValidationData> validationVolumes)
	{
		IVolume volume = currentObj.Volume;
		foreach (XYZ item in volume.GetCases(VProperty.auxiliarySupportReq).Slinq().Except(avoidVoxels.Slinq())
			.NoLeak())
		{
			if (AuxiliaryEndSupportStateAt(ref currentState, currentObj, item, validationVolumes, jumpsMade))
			{
				return currentState;
			}
		}
		foreach (XYZ item2 in volume.GetCases(VProperty.mainSupportReq).Slinq().Where((XYZ i, List<XYZ> v) => !v.Contains(i), avoidVoxels)
			.NoLeak())
		{
			if (MainEndSupportStateAt(ref currentState, currentObj, item2, validationVolumes, jumpsMade))
			{
				return currentState;
			}
			ValidationData currentObj2 = BaseRecursiveValidation.VDataWithPropertyAt(_recursiveSupportSettings, item2);
			if (!currentObj2.IsNotDefault)
			{
				currentObj2 = BaseRecursiveValidation.ValidationVolumeVDataWithPropertyAt(validationVolumes, _recursiveSupportSettings, item2);
			}
			if (!currentObj2.IsNotDefault)
			{
				continue;
			}
			volume.GetCases(VPropertyMask.occupy | VPropertyMask.fantome).Slinq().AddTo(avoidVoxels);
			return RecursiveStepUpkeep(queryMaker, currentObj2, avoidVoxels, currentState, jumpsMade, jumpsLeft, validationVolumes);
		}
		return currentState;
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
		resultVolume = BaseRecursiveValidation.VolumeWithPropertyAt(VProperty.logSupport, at);
		if (resultVolume != null)
		{
			return GraphSupportState.FullSupport;
		}
		IVolume volume = BaseRecursiveValidation.VolumeWithPropertyAt(VProperty.plankSupport, at);
		if (volume != null && volume.AssetKey != Blocks.RopeBridgePlank && volume.AssetKey != Blueprints.RopeBridgePlank)
		{
			IVolume volume2 = BaseRecursiveValidation.VolumeWithPropertyAt(VProperty.beamSupport, at);
			if (volume2 != null)
			{
				resultVolume = volume;
				return GraphSupportState.Near;
			}
			foreach (XYZ @case in volume.GetCases(VProperty.mainSupportReq))
			{
				if (@case != excludedSide.Position)
				{
					ValidationData excludedSide2 = volume.VData;
					GraphSupportState graphSupportState = GraphSupportStateAt(ref queryMaker, ref excludedSide2, @case, out resultVolume);
					resultVolume = volume;
					return (graphSupportState != GraphSupportState.FullSupport) ? (graphSupportState - 1) : GraphSupportState.Near;
				}
			}
		}
		return GraphSupportState.Unsupported;
	}
}
