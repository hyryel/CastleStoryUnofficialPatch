using UnityEngine;

namespace Brix.Game.AI.Behaviors;

public class AnimationEvent : ScriptableObject
{
	public float SleepDelay;

	public string SoundEvent;

	public string SoundSwitchName;

	public string SoundSwitchValue;

	public bool IsSwitchingOnSurface;

	public bool IsBricktronSoundEffect;
}
