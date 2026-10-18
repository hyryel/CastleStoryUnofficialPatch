using System.Collections.Generic;
using Brix.Engine;
using Brix.Engine.Trackers;
using Brix.Game.Components.Volumes;

namespace Brix.Game.Components;

public class DefaultBlockValidator : ConcreteValidator
{
	protected override VPropertyMask SupportSettings => VPropertyMask.stoneSupport;

	public override ValidationResult Validate(ref ValidationData vData, IEnumerable<ValidationData> validationVolumes = null)
	{
		return (!IsGeostatic(vData.Volume)) ? base.Validate(ref vData, validationVolumes) : ValidationResult.Valid;
	}

	private static bool IsGeostatic(IVolume volume)
	{
		return volume.GetCases(VProperty.occupy).Slinq().Any(GeostaticityTracker.IsGeostatic);
	}

	public override void SetUnstable(ref ValidationData vData)
	{
		base.SetUnstable(ref vData);
		Architecte.AddVoxelToFreedList(vData.Volume.Position);
	}

	public override AbstractValidator GetBPValidatorCounterpart()
	{
		return SingletonSpawner.Get<DefaultBPValidator>();
	}

	protected bool DestroyBlock(ValidationData vData)
	{
		vData.Volume.Unregister();
		IBlockData blockData = Voxel.GetBlockData(vData.Position);
		if (!blockData.IsNull)
		{
			blockData.DestroyBlock(withParticles: true, withGarnotte: true);
			return true;
		}
		return false;
	}
}
