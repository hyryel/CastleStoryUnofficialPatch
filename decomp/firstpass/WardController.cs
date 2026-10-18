using System.Collections.Generic;
using Brix.Audio;
using Brix.Engine;
using Brix.Engine.Lifecycle;
using Brix.External.Signals;
using Brix.Game.Components;
using Brix.Game.Components.Tracking;
using Brix.UI.GameSelector;
using Brix.Utils;
using Smooth.Slinq;
using UnityEngine;

public class WardController : TrackingController, IHoverObserver, IUnhoverObserver, ISoulevable
{
	public class WardSingletonData
	{
		public Signal<WardController> WardAdded = new Signal<WardController>();

		public Signal<WardController> WardRemoved = new Signal<WardController>();
	}

	public enum WardHoverGroup
	{
		None,
		Pylons,
		Lanterns,
		Sentinel,
		Healing,
		Capture
	}

	[SerializeField]
	private bool _animatorAlwaysActive;

	private WardSingletonData WardSingleton;

	public GameObject Visual;

	public Color HoverDisplayColor = new Color(1f, 0f, 0.25f, 1f);

	public WardHoverGroup HoverGroup;

	public List<MonoBehaviour> TogglableComponents = new List<MonoBehaviour>();

	public string SoundEventEnergyOn;

	public string SoundEventEnergyOff;

	private Animator animator;

	private bool initialStateTriggered;

	public override void Initialize()
	{
		base.Initialize();
		animator = GetComponent<Animator>();
	}

	public override void OnRequested()
	{
		base.OnRequested();
		WardSingleton = SingletonProvider.Get<WardController, WardSingletonData>();
		WardSingleton.WardAdded.Invoke(this);
		SetVisual(value: false);
		SetTogglableComponent(value: false);
	}

	void IHoverObserver.OnHover()
	{
		SetVisual(value: true);
	}

	void IUnhoverObserver.OnUnhover()
	{
		SetVisual(value: false);
	}

	public void ForceInitialActivationEvent(bool active)
	{
		if (!initialStateTriggered)
		{
			if (active)
			{
				OnEnable();
			}
			else
			{
				OnDisable();
			}
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		initialStateTriggered = true;
		SetTogglableComponent(value: true);
		if ((bool)animator)
		{
			animator.SetBool("Active", value: true);
		}
		if (!string.IsNullOrEmpty(SoundEventEnergyOn))
		{
			SoundEngine.Play(SoundEventEnergyOn, base.GameObject);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		initialStateTriggered = true;
		SetTogglableComponent(value: false);
		if ((bool)animator)
		{
			animator.SetBool("Active", _animatorAlwaysActive);
		}
		if (!string.IsNullOrEmpty(SoundEventEnergyOff))
		{
			SoundEngine.Play(SoundEventEnergyOff, base.GameObject);
		}
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		WardSingleton.WardRemoved.Invoke(this);
	}

	protected void SetTogglableComponent(bool value)
	{
		TogglableComponents.Slinq().ForEach(delegate(MonoBehaviour c, bool val)
		{
			c.enabled = val;
		}, value);
	}

	protected void SetVisual(bool value)
	{
		if ((bool)Visual)
		{
			Visual.SetActive(value);
		}
	}

	public void Soulever()
	{
	}

	public void Deposer()
	{
		base.voxel.position = XYZ.FromVector3(base.transform.position);
		if (!Voxel.IsOpenAndWalkable(base.voxel.position))
		{
			if (Voxel.IsOpenAndWalkable(base.voxel.position - XYZ.up))
			{
				base.voxel.position -= XYZ.up;
			}
			else if (Voxel.IsOpenAndWalkable(base.voxel.position - XYZ.up * 2))
			{
				base.voxel.position -= XYZ.up * 2;
			}
		}
		base.transform.position = Architecte.SurfaceRaycast(base.voxel.position.ToVector3()) + Vector3.up / 2f;
		ClasseMoi.Move(base.gameObject);
		SurfaceSpawnFitter component = GetComponent<SurfaceSpawnFitter>();
		if ((bool)component)
		{
			component.Fit();
		}
	}
}
