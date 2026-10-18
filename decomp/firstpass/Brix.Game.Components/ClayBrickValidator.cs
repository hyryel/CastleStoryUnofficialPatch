using System.Collections.Generic;
using Brix.Engine;
using Brix.Game.Components.Volumes;
using Brix.Utils;

namespace Brix.Game.Components;

internal class ClayBrickValidator : CobbleBrickValidator
{
	protected override Dictionary<XYZ, SupportState> BuildSupportStates(ref ValidationData vData, IEnumerable<ValidationData> validationVolumes)
	{
		Dictionary<XYZ, SupportState> dictionary = CollectionPool.RequestDictionary<XYZ, SupportState>();
		foreach (XYZ @case in vData.Volume.GetCases(VProperty.mainSupportReq))
		{
			dictionary.Add(@case, VUtil.SupportStateAt(@case, SupportSettings, vData.Faction, validationVolumes));
		}
		foreach (XYZ case2 in vData.Volume.GetCases(VProperty.auxiliarySupportReq))
		{
			bool flag = false;
			foreach (IVolume item in VolumeRegistry.VolumesWithPropertyAt(case2, VProperty.stickySupport))
			{
				if (item.HasPropertyAt(VProperty.stickySupport, case2))
				{
					dictionary.Add(case2, VUtil.WeakestSupportOf(VUtil.SupportStateAt(case2, SupportSettings, vData.Faction, validationVolumes), VUtil.SupportStateAt(case2 - XYZ.up, SupportSettings, vData.Faction, validationVolumes)));
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				dictionary.Add(case2, SupportState.Unsupported);
			}
		}
		return dictionary;
	}

	public override AbstractValidator GetBPValidatorCounterpart()
	{
		return SingletonSpawner.Get<ClayBrickBPValidator>();
	}
}
