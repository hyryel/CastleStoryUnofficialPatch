using System.Collections.Generic;
using Brix.Engine;
using Brix.Game.Components.Volumes;
using Brix.Utils;
using Brix.Utils.Collections;

namespace Brix.Game.Components;

internal class WallObjectBPValidator : BlueprintValidator
{
	protected override VPropertyMask SupportSettings => VPropertyMask.stoneSupport | VPropertyMask.logSupport;

	protected override Dictionary<XYZ, SupportState> BuildSupportStates(ref ValidationData vData, IEnumerable<ValidationData> validationVolumes)
	{
		Dictionary<XYZ, SupportState> dictionary = CollectionPool.RequestDictionary<XYZ, SupportState>();
		List<XYZ> list = vData.Volume.GetCases(VPropertyMask.occupy | VPropertyMask.fantome).Slinq().ToPooledList();
		foreach (XYZ @case in vData.Volume.GetCases(VProperty.mainSupportReq))
		{
			if (VolumeRegistry.HasVolumesWithPropertyAt(@case, VProperty.notWallSupport))
			{
				dictionary.Add(@case, SupportState.Unsupported);
				continue;
			}
			bool flag = false;
			foreach (IVolume item in VolumeRegistry.VolumesWithPropertyAt(@case, SupportSettings))
			{
				if (item.HasPropertyAt(VProperty.canHookBeam, list))
				{
					dictionary.Add(@case, SupportState.Unsupported);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				dictionary.Add(@case, VUtil.SupportStateAt(@case, SupportSettings, vData.Faction, validationVolumes));
			}
		}
		list.Dispose();
		return dictionary;
	}

	public override AbstractValidator GetBlockValidatorCounterpart()
	{
		return SingletonSpawner.Get<WallDOValidator>();
	}
}
