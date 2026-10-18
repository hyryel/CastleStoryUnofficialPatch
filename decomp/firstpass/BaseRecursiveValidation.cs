using System.Collections.Generic;
using Brix.Engine;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Smooth.Slinq;

public class BaseRecursiveValidation
{
	protected const SupportState StartingState = SupportState.Unsupported | SupportState.ValidPath;

	protected static void UpdatePathEndDetails(ref SupportState currentState, SupportState stepState, int jumpsMade, int supportDistance, int braceSupportDistance = 0, bool braceSupport = false)
	{
		if (jumpsMade == 0)
		{
			currentState = stepState;
		}
		else
		{
			currentState |= stepState & SupportState.Valid;
		}
		if ((stepState & SupportState.CCSupport) == 0)
		{
			currentState &= ~SupportState.ConcreteEnd;
		}
		else
		{
			currentState |= SupportState.ConcreteEnd;
		}
		if (jumpsMade < supportDistance || (braceSupport && jumpsMade < braceSupportDistance))
		{
			currentState |= SupportState.NearSupport;
		}
		else
		{
			currentState |= SupportState.FarSupport;
		}
	}

	protected static ValidationData VDataWithPropertyAt(VPropertyMask properties, XYZ pos)
	{
		return VolumeWithPropertyAt(properties, pos)?.VData ?? ValidationData.None;
	}

	protected static IVolume VolumeWithPropertyAt(VPropertyMask properties, XYZ pos)
	{
		return VolumeRegistry.VolumesWithPropertyAt(pos, properties).Slinq().FirstOrDefault();
	}

	protected static IVolume VolumeWithPropertyAt(VProperty property, XYZ pos)
	{
		return VolumeRegistry.VolumesWithPropertyAt(pos, property).Slinq().FirstOrDefault();
	}

	protected static ValidationData ValidationVolumeVDataWithPropertyAt(IEnumerable<ValidationData> validationVolumes, VPropertyMask properties, XYZ pos)
	{
		return validationVolumes?.Slinq().Where((ValidationData v, XYZ _i) => v.Volume.HasAnyPropertiesAt(properties, _i), pos).FirstOrDefault() ?? ValidationData.None;
	}

	protected static ValidationData PickBestVData(ref ValidationData regVData, ref ValidationData volVData)
	{
		if (!volVData.IsNotDefault)
		{
			return regVData;
		}
		if (!regVData.IsNotDefault)
		{
			return volVData;
		}
		int num = 0;
		int num2 = 0;
		if (volVData.GetCurrentState() != ValidationResult.Invalid)
		{
			num += 2;
		}
		if (regVData.GetCurrentState() != ValidationResult.Invalid)
		{
			num2 += 2;
		}
		if (!volVData.IsBlueprint)
		{
			num++;
		}
		if (!regVData.IsBlueprint)
		{
			num2++;
		}
		return (num <= num2) ? regVData : volVData;
	}

	public static bool IsFarSupportedCC(Dictionary<XYZ, SupportState> supports)
	{
		return (from ss in supports.Slinq()
			where (ss.Value & (SupportState.Valid | SupportState.ConcreteEnd)) == (SupportState.Valid | SupportState.ConcreteEnd)
			where (ss.Value & (SupportState.NearSupport | SupportState.FarSupport)) > (SupportState)0
			select ss).Count() >= 2 && (from ss in supports.Slinq()
			where (ss.Value & (SupportState.CCSupport | SupportState.ValidPath)) == (SupportState.CCSupport | SupportState.ValidPath)
			select ss).Count() >= 1;
	}

	public static bool IsFarSupportedBP(Dictionary<XYZ, SupportState> supports)
	{
		return (from ss in supports.Slinq()
			where (ss.Value & SupportState.Valid) > (SupportState)0
			where (ss.Value & (SupportState.NearSupport | SupportState.FarSupport)) > (SupportState)0
			select ss).Count() >= 2 && (from ss in supports.Slinq()
			where (ss.Value & (SupportState.BPSupport | SupportState.ValidPath)) == (SupportState.BPSupport | SupportState.ValidPath)
			select ss).Count() >= 1;
	}
}
