using Brix.Game.Components.Volumes;
using UnityEngine;

namespace Brix.Game.Components;

public class LogBlockValidator : WoodBlockValidator
{
	private static VPropertyMask _supportSettings = VPropertyMask.stoneSupport | VPropertyMask.logSupport;

	protected override VPropertyMask SupportSettings => _supportSettings;

	protected override void SpawnSplinters(Vector3 position, Quaternion rotation)
	{
		base.SpawnSplinters(position, rotation * Quaternion.Euler(90f, 0f, 0f));
	}

	public override AbstractValidator GetBPValidatorCounterpart()
	{
		return SingletonSpawner.Get<LogBPValidator>();
	}
}
