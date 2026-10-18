using System.Collections.Generic;
using Brix.Engine;
using Brix.Game.Components.Volumes;
using Brix.Utils;
using Brix.Utils.Collections;

namespace Brix.Game.Components;

public class BraceBlockValidator : WoodBlockValidator
{
	private const VPropertyMask HookSetting = VPropertyMask.canHookBeam;

	protected override VPropertyMask SupportSettings => VPropertyMask.logSupport;

	protected override bool HasGroundSupportAt(ref ValidationData vData, XYZ i)
	{
		return false;
	}

	protected override Dictionary<XYZ, SupportState> BuildSupportStates(ref ValidationData vData, IEnumerable<ValidationData> validationVolumes)
	{
		Dictionary<XYZ, SupportState> dictionary = CollectionPool.RequestDictionary<XYZ, SupportState>();
		List<XYZ> list = vData.Volume.GetCases(VPropertyMask.occupy | VPropertyMask.fantome).Slinq().ToPooledList();
		foreach (XYZ @case in vData.Volume.GetCases(VProperty.mainSupportReq))
		{
			SupportState value = VUtil.SupportStateAt(@case, SupportSettings, VPropertyMask.canHookBeam, list, vData.Faction, validationVolumes);
			dictionary.Add(@case, value);
		}
		list.Dispose();
		return dictionary;
	}

	public override AbstractValidator GetBPValidatorCounterpart()
	{
		return SingletonSpawner.Get<BraceBPValidator>();
	}
}
