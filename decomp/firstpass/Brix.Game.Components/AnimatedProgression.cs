using Brix.Game.Semantique;
using Brix.Utils.Signals;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Components;

[RequireComponent(typeof(Recepteur), typeof(Animator))]
public class AnimatedProgression : GameComponent
{
	private Animator animator;

	private float _lastContainerCount;

	private float _waitForTransition;

	private bool _dirty;

	protected override void Awake()
	{
		base.Awake();
		animator = GetComponent<Animator>();
	}

	protected override void Start()
	{
		base.Start();
		base.recepteur.InventoryChanged.Connect(InventoryChanged).DisconnectWhen(base.OnReleaseOrDestroySignal);
		base.enabled = true;
		_dirty = true;
		animator.enabled = false;
	}

	private void Update()
	{
		if (_dirty)
		{
			_dirty = false;
			_lastContainerCount = animator.GetFloat("Container");
			float proportionUsed = base.recepteur.GetProportionUsed();
			string text = base.recepteur.ContentString();
			if (text == "Empty")
			{
				PlayTransition("Empty");
			}
			else if (proportionUsed != _lastContainerCount)
			{
				PlayTransition(text);
			}
			animator.SetFloat("Container", proportionUsed);
			if (proportionUsed != _lastContainerCount && proportionUsed != 0f)
			{
				GetComponentsInChildren<ParticleSystem>().Slinq().ForEach(delegate(ParticleSystem x)
				{
					x.Play();
				});
			}
		}
		if (_waitForTransition > 0f || animator.IsInTransition(0))
		{
			animator.Update(Time.deltaTime);
			_waitForTransition -= Time.deltaTime;
		}
		else
		{
			base.enabled = false;
		}
	}

	private void PlayTransition(string trigger)
	{
		animator.SetTrigger(trigger);
		_waitForTransition = 1f;
	}

	private void InventoryChanged()
	{
		base.enabled = true;
		_dirty = true;
	}
}
