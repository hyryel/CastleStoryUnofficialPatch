using Brix.Engine;
using Brix.Game.Components;
using UnityEngine;

public class WardActivationModifier : GameComponent
{
	public int VisionActive;

	public int VisionInactive;

	public float LightRangeActive;

	public float LightRangeInactive;

	public float GlowActive;

	public float GlowInactive;

	public float SpinActive;

	public float SpinInactive;

	[SerializeField]
	private Light lightComponent;

	private LocalFactionVision visionComponent;

	private Animator animatorComponent;

	public override void Initialize()
	{
		base.Initialize();
		visionComponent = GetComponent<LocalFactionVision>();
		animatorComponent = GetComponent<Animator>();
	}

	public override void OnRequested()
	{
		base.OnRequested();
		InitHack();
	}

	private void OnTransformParentChanged()
	{
		InitHack();
	}

	private void InitHack()
	{
		if (this.IsNullOrReleased())
		{
			return;
		}
		Neo.Relais(delegate
		{
			if (!this.IsNullOrReleased())
			{
				ToggleModifiers(base.enabled);
			}
		}, "ward stat init");
	}

	private void OnEnable()
	{
		ToggleModifiers(active: true);
	}

	private void OnDisable()
	{
		ToggleModifiers(active: false);
	}

	public void ToggleModifiers(bool active)
	{
		if ((bool)visionComponent)
		{
			visionComponent.LineOfSightRange = ((!active) ? VisionInactive : VisionActive);
		}
		if ((bool)lightComponent)
		{
			lightComponent.range = ((!active) ? LightRangeInactive : LightRangeActive);
		}
		if ((bool)animatorComponent)
		{
			float num = ((!active) ? GlowInactive : GlowActive);
			animatorComponent.SetBool("Glow", num > 0f);
			animatorComponent.SetFloat("Glow Amount", num);
			animatorComponent.SetFloat("Speed", (!active) ? SpinInactive : SpinActive);
		}
	}
}
