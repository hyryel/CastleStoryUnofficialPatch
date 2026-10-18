using System.Collections.Generic;
using Brix.Engine;
using Brix.Game.Components.Volumes;
using Brix.Utils;
using Smooth.Slinq;
using Smooth.Slinq.Context;

namespace Brix.Game.Components;

public class RopeBridgePlankBPValidator : BlueprintValidator
{
	protected override VPropertyMask SupportSettings => VPropertyMask.ropeSupport;

	protected VPropertyMask AuxSupportSettings => VPropertyMask.plankSupport;

	public override AbstractValidator GetBlockValidatorCounterpart()
	{
		return SingletonSpawner.Get<RopeBridgePlankBlockValidator>();
	}

	protected override Dictionary<XYZ, SupportState> BuildSupportStates(ref ValidationData vData, IEnumerable<ValidationData> validationVolumes)
	{
		Dictionary<XYZ, SupportState> dictionary = CollectionPool.RequestDictionary<XYZ, SupportState>();
		foreach (XYZ @case in vData.Volume.GetCases(VProperty.mainSupportReq))
		{
			dictionary.Add(@case, VUtil.SupportStateAt(@case, SupportSettings, vData.Faction, validationVolumes));
		}
		foreach (XYZ case2 in vData.Volume.GetCases(VProperty.auxiliarySupportReq))
		{
			IVolume volume = VolumeRegistry.VolumeWithPropertyAt(case2, AuxSupportSettings);
			if (volume != null && (volume.VData.Validator.GetValidatorType == typeof(RopeBridgePlankBPValidator) || volume.VData.Validator.GetValidatorType == typeof(RopeBridgePlankBlockValidator)))
			{
				dictionary.Add(case2, VUtil.SupportStateAt(case2, AuxSupportSettings, vData.Faction, validationVolumes));
			}
		}
		return dictionary;
	}

	protected override bool IsValid(ref ValidationData vData, Dictionary<XYZ, SupportState> supportStates)
	{
		Slinq<KeyValuePair<XYZ, SupportState>, PredicateContext<KeyValuePair<XYZ, SupportState>, DictionaryContext<XYZ, SupportState>, IVolume>> slinq = supportStates.Slinq().Where((KeyValuePair<XYZ, SupportState> ss, IVolume v) => v.HasPropertyAt(VProperty.mainSupportReq, ss.Key), vData.Volume);
		return slinq.Where((KeyValuePair<XYZ, SupportState> ss, SupportState support) => (ss.Value & support) != 0 && ss.Value.HasFlag(SupportState.Valid), AcceptedSupportState).Count() >= slinq.Count() - vData.MaxMissingSupport;
	}

	protected override bool IsBuildable(ref ValidationData vData, Dictionary<XYZ, SupportState> supportStates)
	{
		Slinq<KeyValuePair<XYZ, SupportState>, PredicateContext<KeyValuePair<XYZ, SupportState>, DictionaryContext<XYZ, SupportState>, IVolume>> slinq = supportStates.Slinq().Where((KeyValuePair<XYZ, SupportState> ss, IVolume v) => v.HasPropertyAt(VProperty.mainSupportReq, ss.Key), vData.Volume);
		Slinq<KeyValuePair<XYZ, SupportState>, PredicateContext<KeyValuePair<XYZ, SupportState>, DictionaryContext<XYZ, SupportState>, IVolume>> slinq2 = supportStates.Slinq().Where((KeyValuePair<XYZ, SupportState> ss, IVolume v) => v.HasPropertyAt(VProperty.auxiliarySupportReq, ss.Key), vData.Volume);
		int num = slinq.Where((KeyValuePair<XYZ, SupportState> ss, SupportState support) => (ss.Value & support) == support && ss.Value.HasFlag(SupportState.Valid), base.BuildableSupportState).Count();
		int num2 = slinq2.Where((KeyValuePair<XYZ, SupportState> ss, SupportState support) => (ss.Value & support) == support && ss.Value.HasFlag(SupportState.Valid), base.BuildableSupportState).Count();
		return num >= slinq.Count() - vData.MaxMissingSupport && num2 >= slinq2.Count() - 2 && !slinq.Any((KeyValuePair<XYZ, SupportState> ss) => (ss.Value & SupportState.BPSupport) > (SupportState)0);
	}
}
