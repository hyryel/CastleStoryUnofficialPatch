using System;

namespace Brix.Game.Locomotion;

[Serializable]
public class LocomotionCommonState : LocomotionStateUpdater
{
	public Grounded Grounded => base.Common.Grounded;

	public Grabbing Grabbing => base.Common.Grabbing;

	public Flailing Flailing => base.Common.Flailing;

	public Airborne Airborne => base.Common.Airborne;

	public Trapped Trapped => base.Common.Trapped;

	public Hurting Hurting => base.Common.Hurting;

	public LookAt LookAt => base.Common.LookAt;

	public Sitting Sitting => base.Common.Sitting;

	public Working Working => base.Common.Working;
}
