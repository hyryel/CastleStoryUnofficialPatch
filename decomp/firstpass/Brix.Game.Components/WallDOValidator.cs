using System.Collections.Generic;
using Brix.Engine;
using Brix.Game.Components.Volumes;
using Brix.Utils;

namespace Brix.Game.Components;

public class WallDOValidator : DefaultDOValidator
{
	protected override VPropertyMask SupportSettings => VPropertyMask.stoneSupport | VPropertyMask.logSupport;

	protected override Dictionary<XYZ, SupportState> BuildSupportStates(ref ValidationData vData, IEnumerable<ValidationData> validationVolumes)
	{
		Dictionary<XYZ, SupportState> dictionary = CollectionPool.RequestDictionary<XYZ, SupportState>();
		foreach (XYZ @case in vData.Volume.GetCases(VProperty.mainSupportReq))
		{
			if (VolumeRegistry.HasVolumesWithPropertyAt(@case, VProperty.notWallSupport))
			{
				dictionary.Add(@case, SupportState.Unsupported);
			}
			else
			{
				dictionary.Add(@case, VUtil.SupportStateAt(@case, SupportSettings, vData.Faction, validationVolumes));
			}
		}
		return dictionary;
	}

	public override AbstractValidator GetBPValidatorCounterpart()
	{
		return SingletonSpawner.Get<WallObjectBPValidator>();
	}
}
