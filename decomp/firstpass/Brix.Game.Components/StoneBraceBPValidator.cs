using System.Collections.Generic;
using Brix.Engine;
using Brix.Game.Components.Volumes;
using Brix.Utils;
using Brix.Utils.Collections;

namespace Brix.Game.Components;

internal class StoneBraceBPValidator : BlueprintValidator
{
	protected override VPropertyMask SupportSettings => VPropertyMask.stoneSupport;

	protected override Dictionary<XYZ, SupportState> BuildSupportStates(ref ValidationData vData, IEnumerable<ValidationData> validationVolumes)
	{
		Dictionary<XYZ, SupportState> dictionary = CollectionPool.RequestDictionary<XYZ, SupportState>();
		List<XYZ> positions = vData.Volume.GetCases(VPropertyMask.occupy | VPropertyMask.fantome).Slinq().ToPooledList();
		foreach (XYZ @case in vData.Volume.GetCases(VProperty.mainSupportReq))
		{
			bool flag = false;
			foreach (IVolume item in VolumeRegistry.VolumesWithPropertyAt(@case, SupportSettings))
			{
				if (item.HasPropertyAt(VProperty.canHookBeam, positions))
				{
					dictionary.Add(@case, VUtil.SupportStateAt(@case, SupportSettings, vData.Faction, validationVolumes));
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				dictionary.Add(@case, SupportState.Unsupported);
			}
		}
		return dictionary;
	}

	protected override bool HasGroundSupportAt(ref ValidationData vData, XYZ pos)
	{
		return false;
	}

	public override AbstractValidator GetBlockValidatorCounterpart()
	{
		return SingletonSpawner.Get<StoneBraceValidator>();
	}
}
