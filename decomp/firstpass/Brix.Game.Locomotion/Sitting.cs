using System;

namespace Brix.Game.Locomotion;

[Serializable]
public class Sitting : LocomotionCommonState
{
	public void DoSit()
	{
		base.controller.IsSitting.Set(b: true);
	}

	public void EndSit()
	{
		base.controller.IsSitting.Set(b: false);
	}

	public override void Exit()
	{
		base.controller.IsSitting.Set(b: false);
	}
}
