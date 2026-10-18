using Brix.Audio;
using Brix.Game.Components;
using UnityEngine;

namespace Brix.Game.AI;

public class Thinker : UpdatableGameComponent<Thinker>
{
	[SerializeField]
	private CharacterState state;

	private float TimeAtLastTempThought;

	private bool thoughtIsTemporary;

	private float secondsToShowThought;

	private const float TIME_BEFORE_ERASE_PREVIOUS = 1f;

	private float timeWhenStoppedThinking;

	private Thought previousThought;

	public Thought PreviousThought => previousThought;

	public virtual Thought CurrentThought
	{
		get
		{
			return state.Thought;
		}
		set
		{
			state.Thought = value;
		}
	}

	public virtual void StopThinking()
	{
		if (!thoughtIsTemporary)
		{
			_StopThinking();
		}
	}

	private void _StopThinking()
	{
		timeWhenStoppedThinking = Time.time;
		SetThought(Thought.None);
	}

	public virtual void SetThoughtFor(float seconds, Thought newThought)
	{
		TimeAtLastTempThought = Time.time;
		secondsToShowThought = seconds;
		SetThought(newThought, isTemporary: true);
	}

	public virtual void SetThought(Thought newThought, bool isTemporary = false)
	{
		thoughtIsTemporary = isTemporary;
		if (CurrentThought != newThought)
		{
			previousThought = CurrentThought;
			CurrentThought = newThought;
			PlaySound();
		}
	}

	public virtual void ShowResolutionThought()
	{
		if (previousThought != null && previousThought.onResolution != null)
		{
			SetThoughtFor(1f, previousThought.onResolution);
		}
	}

	private void PlaySound()
	{
		string soundLabel = CurrentThought.soundLabel;
		if (soundLabel != string.Empty)
		{
			SoundEngine.Play(soundLabel, base.gameObject);
		}
	}

	public virtual bool HasThought()
	{
		return CurrentThought != Thought.None;
	}

	protected override void BrixUpdate()
	{
		bool flag = Time.time - TimeAtLastTempThought > secondsToShowThought;
		if (thoughtIsTemporary && flag)
		{
			_StopThinking();
		}
		bool flag2 = Time.time - timeWhenStoppedThinking > 1f;
		if (CurrentThought == Thought.None && flag2)
		{
			previousThought = Thought.None;
		}
	}
}
