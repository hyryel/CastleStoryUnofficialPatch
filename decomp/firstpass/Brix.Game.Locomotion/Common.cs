using System;

namespace Brix.Game.Locomotion;

[Serializable]
public class Common : LocomotionStateUpdater
{
	public Grounded Grounded = new Grounded();

	public Grabbing Grabbing = new Grabbing();

	public Flailing Flailing = new Flailing();

	public Airborne Airborne = new Airborne();

	public Trapped Trapped = new Trapped();

	public Hurting Hurting = new Hurting();

	public LookAt LookAt = new LookAt();

	public Sitting Sitting = new Sitting();

	public Working Working = new Working();
}
