using System;
using System.Collections.Generic;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Input.WorldEditor;
using Brix.Utils.Signals;
using Rewired;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Input;

[Serializable]
public class Picker : MonoBehaviour
{
	private class Inout<T>
	{
		private Signal<T> _first = new Signal<T>();

		private Signal<T> _continued = new Signal<T>();

		private Signal<T> _last = new Signal<T>();

		private T _lastHit;

		private EqualityComparer<T> comparer;

		private bool _hadTarget;

		public T Target => _lastHit;

		public Inout()
		{
			if (typeof(T) == typeof(Vector3?))
			{
				comparer = new NullableVectorComparer() as EqualityComparer<T>;
			}
			else
			{
				comparer = EqualityComparer<T>.Default;
			}
		}

		public void Connect(PickerComponent pc, Action<T> first, Action<T> continued, Action<T> last, IReadOnlySignal disconnected)
		{
			_first.Connect(first).DisconnectWhen(disconnected);
			_continued.Connect(continued).DisconnectWhen(disconnected);
			_last.Connect(last).DisconnectWhen(disconnected);
		}

		public bool IsDefault()
		{
			return IsDefault(_lastHit);
		}

		private bool IsDefault(T test)
		{
			return IsSame(test, default(T));
		}

		private bool IsSame(T A, T B)
		{
			return comparer.Equals(A, B);
		}

		public void SetTo(T hit)
		{
			if (!IsSame(hit, _lastHit) || (_hadTarget && IsDefault(hit)))
			{
				if (_hadTarget)
				{
					_last.Invoke(_lastHit);
					_lastHit = default(T);
					_hadTarget = false;
				}
				if (!IsDefault(hit))
				{
					_first.Invoke(hit);
					_continued.Invoke(hit);
					_lastHit = hit;
					_hadTarget = true;
				}
			}
			else if (!IsDefault(hit))
			{
				_continued.Invoke(hit);
				_hadTarget = true;
			}
		}
	}

	public class NullableVectorComparer : EqualityComparer<Vector3?>
	{
		public override bool Equals(Vector3? x, Vector3? y)
		{
			if (!x.HasValue && !y.HasValue)
			{
				return true;
			}
			if (x.HasValue && y.HasValue)
			{
				return x.Value == y.Value;
			}
			return false;
		}

		public override int GetHashCode(Vector3? obj)
		{
			throw new NotImplementedException();
		}
	}

	private class InoutClick<T>
	{
		private Signal<T, ClickInfo> _first = new Signal<T, ClickInfo>();

		private Signal<T, ClickInfo> _continued = new Signal<T, ClickInfo>();

		private Signal<T, ClickInfo> _last = new Signal<T, ClickInfo>();

		private Dictionary<string, T> _lastHits = new Dictionary<string, T>
		{
			{
				"cam_action1",
				default(T)
			},
			{
				"cam_action2",
				default(T)
			}
		};

		private float _lastTime;

		private bool _onSlowUpdate;

		private EqualityComparer<T> comparer;

		public InoutClick(bool onSlowUpdate)
		{
			if (typeof(T) == typeof(Vector3?))
			{
				comparer = new NullableVectorComparer() as EqualityComparer<T>;
			}
			else
			{
				comparer = EqualityComparer<T>.Default;
			}
			_onSlowUpdate = onSlowUpdate;
		}

		public void Connect(PickerComponent pc, Action<T, ClickInfo> first, Action<T, ClickInfo> continued, Action<T, ClickInfo> last, IReadOnlySignal disconnected)
		{
			_first.Connect(first).DisconnectWhen(disconnected);
			_continued.Connect(continued).DisconnectWhen(disconnected);
			_last.Connect(last).DisconnectWhen(disconnected);
		}

		private bool IsDefault(T test)
		{
			return IsSame(test, default(T));
		}

		private bool IsSame(T A, T B)
		{
			return comparer.Equals(A, B);
		}

		public void SetTo(T hit, ClickInfo clickInfo)
		{
			if (!IsSame(hit, _lastHits[clickInfo.actionName]))
			{
				if (!IsDefault(_lastHits[clickInfo.actionName]))
				{
					if (clickInfo.clickState == ClickState.up || (_onSlowUpdate && IsDefault(hit)))
					{
						_last.Invoke(_lastHits[clickInfo.actionName], clickInfo);
					}
					_lastHits[clickInfo.actionName] = default(T);
				}
				if (!IsDefault(hit))
				{
					if (clickInfo.clickState == ClickState.down || (_onSlowUpdate && IsDefault(_lastHits[clickInfo.actionName])))
					{
						_first.Invoke(hit, clickInfo);
					}
					_continued.Invoke(hit, clickInfo);
					_lastHits[clickInfo.actionName] = hit;
					_lastTime = ReInput.time.unscaledTime - 0.25f;
				}
			}
			else if (!IsDefault(hit) && ReInput.time.unscaledTime >= _lastTime + 0.1f)
			{
				if (clickInfo.clickState == ClickState.down)
				{
					_first.Invoke(hit, clickInfo);
				}
				_continued.Invoke(hit, clickInfo);
			}
		}
	}

	[SerializeField]
	public VoxelRaycastHit RaycastHit;

	[SerializeField]
	public PickerType type;

	public int actionPriority;

	public LayerBundle ActiveLayers;

	public InputModeBundle RegisteredModes;

	public bool RaycastOnDefaultPlane;

	public bool OnSlowUpdate;

	public bool PersistsInProjectContext;

	private List<PickerComponent> _pickComponents = new List<PickerComponent>();

	private PickerComponent _activeComponent;

	public PickerComponent mainComponent;

	public PickerComponent shiftComponent;

	public PickerComponent ctrlComponent;

	public bool locked;

	private bool _initialised;

	private BrushSettings _sharedSettings;

	private bool _active;

	private bool _onStandBy;

	private List<ClickInfo> clickInfos = new List<ClickInfo>
	{
		new ClickInfo("cam_action1"),
		new ClickInfo("cam_action2")
	};

	private Factory.AssetKey _heldKey;

	private Factory.AssetKey _lastHeldKey;

	private IPickerComponentOptions _lastOptions;

	private Inout<bool> _onHoverAny = new Inout<bool>();

	private Inout<Vector3?> _onHoverTerrain = new Inout<Vector3?>();

	private Inout<GameObject> _onHoverGameObject = new Inout<GameObject>();

	private Inout<Blueprint> _onHoverBlueprint = new Inout<Blueprint>();

	private Inout<Labor> _onHoverCharacter = new Inout<Labor>();

	private InoutClick<bool> _onActionAny;

	private InoutClick<Vector3?> _onActionTerrain;

	private InoutClick<GameObject> _onActionGameObject;

	private InoutClick<Blueprint> _onActionBlueprint;

	private InoutClick<Labor> _onActionCharacter;

	public PickerComponent CurrentPickerComponent => _activeComponent;

	public Factory.AssetKey AssetKey => GetComponent<FactoryImprint>().AssetKey;

	public Factory.AssetKey HeldKey
	{
		get
		{
			return _heldKey;
		}
		set
		{
			_heldKey = value;
		}
	}

	public Factory.AssetKey LastHeldKey
	{
		get
		{
			return _lastHeldKey;
		}
		set
		{
			_lastHeldKey = value;
		}
	}

	public IPickerComponentOptions LastOptions
	{
		get
		{
			return _lastOptions;
		}
		set
		{
			_lastOptions = value;
		}
	}

	public BrushSettings SharedSettings => _sharedSettings;

	public object PieMenuTarget
	{
		get
		{
			if (_activeComponent != null)
			{
				return _activeComponent.GetPieMenuTarget();
			}
			return null;
		}
	}

	public void SetOptions(IPickerComponentOptions options)
	{
		foreach (PickerComponent pickComponent in _pickComponents)
		{
			pickComponent.SetOptions(options);
		}
	}

	private void OnAwake()
	{
		Init();
	}

	public void Init()
	{
		if (!_initialised)
		{
			_onActionAny = new InoutClick<bool>(OnSlowUpdate);
			_onActionTerrain = new InoutClick<Vector3?>(OnSlowUpdate);
			_onActionGameObject = new InoutClick<GameObject>(OnSlowUpdate);
			_onActionBlueprint = new InoutClick<Blueprint>(OnSlowUpdate);
			_onActionCharacter = new InoutClick<Labor>(OnSlowUpdate);
			AddComponent(mainComponent);
			AddComponent(shiftComponent);
			AddComponent(ctrlComponent);
			mainComponent.SetPickingParent(this);
			if (shiftComponent != null)
			{
				shiftComponent.SetPickingParent(this);
			}
			if (shiftComponent != null)
			{
				ctrlComponent.SetPickingParent(this);
			}
			_initialised = true;
		}
	}

	public bool PlaceAtRaycast(Ray ray, float distance, LayerBundle layerOverride = null)
	{
		RaycastHit.PlaceAtRaycast(ray, distance, (layerOverride != null) ? layerOverride : ActiveLayers, RaycastOnDefaultPlane);
		if (HeldKey != null)
		{
			foreach (PickerComponent pickComponent in _pickComponents)
			{
				pickComponent.HoldKey(HeldKey);
			}
			HeldKey = null;
		}
		return type == PickerType.targetless || RaycastHit.HasHit;
	}

	public void OnAction()
	{
		LeaveStandBy();
		if (RaycastHit.HitGameObject != null)
		{
			if (RaycastHit.HasHitTerrain())
			{
				_onHoverAny.SetTo(hit: true);
				_onHoverTerrain.SetTo(RaycastHit.HitVector);
				_onHoverGameObject.SetTo(null);
				_onHoverBlueprint.SetTo(null);
				_onHoverCharacter.SetTo(null);
			}
			else
			{
				_onHoverTerrain.SetTo(null);
				_onHoverBlueprint.SetTo(RaycastHit.HitGameObject.GetComponentInParent<Blueprint>());
				_onHoverCharacter.SetTo(RaycastHit.HitGameObject.GetComponentInParent<Labor>());
				_onHoverGameObject.SetTo(RaycastHit.HitRelevantGameObject);
				if (!_onHoverBlueprint.IsDefault() || !_onHoverGameObject.IsDefault() || !_onHoverCharacter.IsDefault())
				{
					_onHoverAny.SetTo(hit: true);
				}
				else
				{
					_onHoverAny.SetTo(type == PickerType.targetless || (type == PickerType.placement && PickingUtility.Player.GetButton("cam_action1")));
				}
			}
		}
		else if (RaycastHit.OnDefaultPlane && RaycastHit.HasHit)
		{
			_onHoverAny.SetTo(hit: true);
			_onHoverTerrain.SetTo(null);
			_onHoverGameObject.SetTo(null);
			_onHoverBlueprint.SetTo(null);
			_onHoverCharacter.SetTo(null);
		}
		else
		{
			CloseAllInouts();
		}
		bool flag = false;
		foreach (ClickInfo clickInfo2 in clickInfos)
		{
			ClickInfo clickInfo = Picking.Instance.ReferenceClicks.Slinq().Where((ClickInfo c, string n) => c.actionName == n, clickInfo2.actionName).First();
			if (OnSlowUpdate && clickInfo2.clickState == ClickState.holdDown && clickInfo2.clickDownTime < clickInfo.clickDownTime)
			{
				clickInfo2.clickState = ClickState.down;
				clickInfo2.clickDownTime = clickInfo.clickDownTime;
			}
			if (clickInfo2.clickState == ClickState.down && clickInfo2.actionName == "cam_action1")
			{
				locked = true;
			}
			if (clickInfo2.clickState == ClickState.down || clickInfo2.clickState == ClickState.holdDown)
			{
				if (Picking.Instance.ReferenceClicks.Slinq().Any((ClickInfo c, string n) => c.pickingActiveOnLastMouseDown && c.actionName == n, clickInfo2.actionName))
				{
					_onActionAny.SetTo(hit: true, clickInfo2);
					if (clickInfo2.pickingActive)
					{
						_onActionTerrain.SetTo(_onHoverTerrain.Target, clickInfo2);
						_onActionBlueprint.SetTo(_onHoverBlueprint.Target, clickInfo2);
						_onActionCharacter.SetTo(_onHoverCharacter.Target, clickInfo2);
						_onActionGameObject.SetTo(_onHoverGameObject.Target, clickInfo2);
					}
				}
				flag = true;
			}
			else if (Picking.Instance.ReferenceClicks.Slinq().Any((ClickInfo c, string n) => c.pickingActiveOnLastMouseDown && c.actionName == n, clickInfo2.actionName))
			{
				_onActionAny.SetTo(hit: false, clickInfo2);
				if (clickInfo2.pickingActive)
				{
					_onActionTerrain.SetTo(null, clickInfo2);
					_onActionBlueprint.SetTo(null, clickInfo2);
					_onActionGameObject.SetTo(null, clickInfo2);
					_onActionCharacter.SetTo(null, clickInfo2);
				}
			}
		}
		if (!flag)
		{
			locked = false;
		}
	}

	public void UpdateClickInfo()
	{
		foreach (ClickInfo clickInfo in clickInfos)
		{
			clickInfo.SetClickStatus();
		}
	}

	public void AddComponent(PickerComponent pick)
	{
		if (pick != null)
		{
			_pickComponents.Add(pick);
			pick.SetVoxelRaycastHit(RaycastHit);
		}
	}

	public T GetPick<T>()
	{
		return (T)(object)_pickComponents.Find((PickerComponent x) => x.GetType() == typeof(T));
	}

	public void LoadBrushSettings()
	{
		BrushComponent brushComponent = mainComponent as BrushComponent;
		BrushComponent brushComponent2 = shiftComponent as BrushComponent;
		BrushComponent brushComponent3 = ctrlComponent as BrushComponent;
		if (brushComponent != null)
		{
			_sharedSettings = brushComponent.brushSettings;
			if (brushComponent2 != null)
			{
				brushComponent2.brushSettings = _sharedSettings;
			}
			if (brushComponent3 != null)
			{
				brushComponent3.brushSettings = _sharedSettings;
			}
		}
	}

	public void Activate()
	{
		if (!_active)
		{
			_active = true;
			UpdateActiveComponent();
			Picking.Instance.PickingObserver.PickerActivated.Invoke(this);
		}
	}

	public void Deactivate()
	{
		if (_active)
		{
			_active = false;
			UpdateActiveComponent();
			Picking.Instance.PickingObserver.PickerDeactivated.Invoke(this);
		}
	}

	public void UpdateActiveComponent()
	{
		if (_active)
		{
			PickerComponent pickerComponent = ((shiftComponent != null && PickingUtility.Player.GetButton("cam_mouse_alt")) ? shiftComponent : ((!(ctrlComponent != null) || !PickingUtility.Player.GetButton("cam_mouse_sec")) ? mainComponent : ctrlComponent));
			if (!(_activeComponent != pickerComponent))
			{
				return;
			}
			_activeComponent = pickerComponent;
			{
				foreach (PickerComponent pickComponent in _pickComponents)
				{
					if (pickComponent == pickerComponent)
					{
						ActivateComponent(pickComponent);
					}
					else
					{
						DeactivateComponent(pickComponent);
					}
				}
				return;
			}
		}
		_activeComponent = null;
		foreach (PickerComponent pickComponent2 in _pickComponents)
		{
			DeactivateComponent(pickComponent2);
		}
	}

	private void ActivateComponent(PickerComponent pc)
	{
		_activeComponent = pc;
		if (!pc.Active)
		{
			_onHoverAny.Connect(pc, pc.OnEnterAnyAction, pc.OnHoverAnyAction, pc.OnLeaveAnyAction, pc.Deactivated);
			_onHoverTerrain.Connect(pc, pc.OnEnterTerrainAction, pc.OnHoverTerrainAction, pc.OnLeaveTerrainAction, pc.Deactivated);
			_onHoverBlueprint.Connect(pc, pc.OnEnterBlueprintAction, pc.OnHoverBlueprintAction, pc.OnLeaveBlueprintAction, pc.Deactivated);
			_onHoverCharacter.Connect(pc, pc.OnEnterCharacterAction, pc.OnHoverCharacterAction, pc.OnLeaveCharacterAction, pc.Deactivated);
			_onHoverGameObject.Connect(pc, pc.OnEnterGameObjectAction, pc.OnHoverGameObjectAction, pc.OnLeaveGameObjectAction, pc.Deactivated);
			_onActionAny.Connect(pc, pc.OnFirstActionAnyAction, pc.OnActionAnyAction, pc.OnLastActionAnyAction, pc.Deactivated);
			_onActionTerrain.Connect(pc, pc.OnFirstActionTerrainAction, pc.OnActionTerrainAction, pc.OnLastActionTerrainAction, pc.Deactivated);
			_onActionBlueprint.Connect(pc, pc.OnFirstActionBlueprintAction, pc.OnActionBlueprintAction, pc.OnLastActionBlueprintAction, pc.Deactivated);
			_onActionCharacter.Connect(pc, pc.OnFirstActionCharacterAction, pc.OnActionCharacterAction, pc.OnLastActionCharacterAction, pc.Deactivated);
			_onActionGameObject.Connect(pc, pc.OnFirstActionGameObjectAction, pc.OnActionGameObjectAction, pc.OnLastActionGameObjectAction, pc.Deactivated);
			CloseAllInouts();
			pc.Active = true;
			pc.StandBy = _onStandBy;
		}
	}

	private void DeactivateComponent(PickerComponent pc)
	{
		if (_activeComponent == pc)
		{
			_activeComponent = null;
		}
		if (pc.Active)
		{
			CloseAllInouts();
			pc.StandBy = false;
			pc.Active = false;
		}
	}

	public void EnterStandBy()
	{
		if (!_onStandBy)
		{
			CloseAllInouts();
			_onStandBy = true;
			_activeComponent.StandBy = _onStandBy;
		}
	}

	public void LeaveStandBy()
	{
		if (_onStandBy)
		{
			CloseAllInouts();
			_onStandBy = false;
			_activeComponent.StandBy = _onStandBy;
		}
	}

	private void CloseAllInouts()
	{
		_onHoverAny.SetTo(type == PickerType.targetless || (type == PickerType.placement && PickingUtility.Player.GetButton("cam_action1")));
		_onHoverTerrain.SetTo(null);
		_onHoverGameObject.SetTo(null);
		_onHoverBlueprint.SetTo(null);
		_onHoverCharacter.SetTo(null);
	}
}
