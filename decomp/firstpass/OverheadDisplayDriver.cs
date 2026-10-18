using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.Components;
using Brix.Input;
using Brix.Lifecycle.Pooling;
using Brix.NewUI.OverheadDisplay;
using Brix.UI.GameSelector;
using Brix.Utils.Signals;
using Brix.Utils.Unity;
using Rewired;
using UnityEngine;

public abstract class OverheadDisplayDriver : UpdatableGameComponent<OverheadDisplayDriver>, ISelectObserver, IUnselectObserver, IHoverObserver, IUnhoverObserver
{
	public class OverheadAudio
	{
		public string OnHover;

		public string OnEndHover;

		public string OnClick;
	}

	[SerializeField]
	protected Color BackgroundColor;

	public static Signal<OverheadDisplayDriver, GameObject> ParentMe = new Signal<OverheadDisplayDriver, GameObject>();

	public Transform MasterTransform;

	[DisplayRules(DisplayFlags.ReadOnly)]
	public OverheadDisplay overheadDisplay;

	protected bool selected;

	public bool IgnoreHover;

	protected Signal Disconnected = new Signal();

	protected bool initialized;

	protected abstract void ConnectToSignals();

	public void HighlightTarget(Sprite icon, bool value)
	{
		if (overheadDisplay == null)
		{
			return;
		}
		if (overheadDisplay.BigIcon.IsSpawned)
		{
			if (value)
			{
				overheadDisplay.BigIcon.Get.SetIcon(icon);
			}
			else
			{
				overheadDisplay.BigIcon.Get.NoIcon();
			}
			overheadDisplay.BigIcon.Get.ForceShow = true;
		}
		overheadDisplay.ShowOrHide();
	}

	protected virtual void InitValues()
	{
	}

	protected override void Start()
	{
		base.Start();
		if (!initialized)
		{
			Init();
		}
	}

	protected override void BrixUpdate()
	{
		if (!(overheadDisplay == null))
		{
			overheadDisplay.CheckCloseEnough();
		}
	}

	private void TemplateConnectToSignals()
	{
		PickingUtility.Player.AddInputEventDelegate(ShowOverHeadsButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonPressed, "cam_show_overheads");
		PickingUtility.Player.AddInputEventDelegate(ShowOverHeadsButtonUp, UpdateLoopType.Update, InputActionEventType.ButtonJustReleased, "cam_show_overheads");
		LocalFactionVision component = GetComponent<LocalFactionVision>();
		if ((bool)component)
		{
			component.EnteredFog.Connect(OnVisionChange).DisconnectWhen(base.OnReleaseOrDestroySignal);
			component.LeftFog.Connect(OnVisionChange).DisconnectWhen(base.OnReleaseOrDestroySignal);
		}
		ConnectToSignals();
	}

	private void TemplateDisconnectFromSignals()
	{
		Disconnected.Invoke();
		if (ReInput.isReady)
		{
			PickingUtility.Player.RemoveInputEventDelegate(ShowOverHeadsButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonPressed, "cam_show_overheads");
			PickingUtility.Player.RemoveInputEventDelegate(ShowOverHeadsButtonUp, UpdateLoopType.Update, InputActionEventType.ButtonJustReleased, "cam_show_overheads");
		}
	}

	protected void Init()
	{
		overheadDisplay = CreateOverheadDisplay();
		if (!(overheadDisplay == null))
		{
			overheadDisplay.Init();
			initialized = true;
			InitValues();
			TemplateConnectToSignals();
			overheadDisplay.ShowOrHide();
		}
	}

	protected OverheadDisplay CreateOverheadDisplay()
	{
		ObjectPoolSingleton.Request(out var go, UI.Overhead);
		ParentMe.Invoke(this, go);
		go.GetComponent<RectTransform>().position = Vector3.zero;
		TransformConstraint component = go.GetComponent<TransformConstraint>();
		component.masterTransform = MasterTransform;
		OverheadDisplay component2 = go.GetComponent<OverheadDisplay>();
		component2.pivot = MasterTransform;
		go.name = base.gameObject.name + " OverHead";
		return component2;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		TemplateDisconnectFromSignals();
		if (overheadDisplay != null)
		{
			ObjectPoolSingleton.Release(overheadDisplay.gameObject);
		}
	}

	private void OnVisionChange()
	{
		overheadDisplay.InFog = !base.vision.IsVisibleForMe();
		overheadDisplay.ShowOrHide();
	}

	private void ShowOverHeadsButtonDown(InputActionEventData data)
	{
		overheadDisplay.ShowAllButton = true;
		overheadDisplay.ShowOrHide();
	}

	private void ShowOverHeadsButtonUp(InputActionEventData data)
	{
		overheadDisplay.ShowAllButton = false;
		overheadDisplay.ShowOrHide();
	}

	void ISelectObserver.OnSelect()
	{
		OnSelect();
	}

	void IUnselectObserver.OnUnselect()
	{
		OnUnselect();
	}

	void IHoverObserver.OnHover()
	{
		OnHover();
	}

	void IUnhoverObserver.OnUnhover()
	{
		OnUnhover();
	}

	protected virtual void OnHover()
	{
		if ((bool)overheadDisplay && !selected && !IgnoreHover)
		{
			overheadDisplay.Hovered = true;
			overheadDisplay.ShowOrHide();
		}
	}

	protected virtual void OnUnhover()
	{
		if ((bool)overheadDisplay && !selected && !IgnoreHover)
		{
			overheadDisplay.Hovered = false;
			overheadDisplay.ShowOrHide();
		}
	}

	protected virtual void OnSelect()
	{
		if ((bool)overheadDisplay)
		{
			selected = true;
			overheadDisplay.Hovered = false;
			overheadDisplay.Selected = true;
			overheadDisplay.ShowOrHide();
		}
	}

	protected virtual void OnUnselect()
	{
		if ((bool)overheadDisplay)
		{
			selected = false;
			overheadDisplay.Selected = false;
			overheadDisplay.ShowOrHide();
		}
	}
}
