using System;
using System.Collections.Generic;
using Brix.External.Signals;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.AI.Damage;
using Brix.Game.Components;
using Brix.Lua;
using Brix.UI.Builder.Menu;
using Brix.UI.GameSelector;
using Brix.Utils.Signals;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Widget;

public class CompassLayoutElementWidgetComponent : LayoutElementWidgetComponent
{
	[SerializeField]
	protected RawImage _rawImage;

	public Material SourceMaterial;

	private Material _material;

	private LuaDriver _ld;

	private GameObject _go;

	private UISelectable _selectable;

	private BricktronDamageReceiver _bricktronDamageReceiver;

	private Labor _labor;

	private LocalFactionVision _localFactionVision;

	private CaptureClock _captureClock;

	protected static readonly Dictionary<UISelectable, CompassLayoutElementWidgetComponent> SelectableDico = new Dictionary<UISelectable, CompassLayoutElementWidgetComponent>();

	private Func<CompassLayoutElementWidgetComponent, bool> _hasHoveredGetter;

	private Func<CompassLayoutElementWidgetComponent, bool> _hoveredGetter;

	private Func<CompassLayoutElementWidgetComponent, bool> _hasSelectedGetter;

	private Func<CompassLayoutElementWidgetComponent, bool> _selectedGetter;

	private Func<CompassLayoutElementWidgetComponent, bool> _hasHpGetter;

	private Func<CompassLayoutElementWidgetComponent, float> _hpRatioGetter;

	private Func<CompassLayoutElementWidgetComponent, bool> _hasStaminaGetter;

	private Func<CompassLayoutElementWidgetComponent, float> _staminaRatioGetter;

	private Func<CompassLayoutElementWidgetComponent, bool> _hasCaptureGetter;

	private Func<CompassLayoutElementWidgetComponent, float> _captureRatioGetter;

	private Func<CompassLayoutElementWidgetComponent, bool> _outFogGetter;

	private Func<CompassLayoutElementWidgetComponent, bool> _inDangerGetter;

	private readonly Signal _disconnectCompassSignal = new Signal();

	private bool _compassConnected;

	private bool _hasHovered;

	private bool _hovered;

	private bool _hasSelected;

	private bool _selected;

	private bool _hasHp;

	private float _hpRatio;

	private bool _hasStamina;

	private float _staminaRatio;

	private bool _inDanger;

	private bool _outFog;

	private bool _hasCapture;

	private float _captureRatio;

	public override bool Refresh(IWidget w)
	{
		if (!base.Refresh(w))
		{
			return false;
		}
		if (w.Is<IGetCompassTarget>())
		{
			SetCompassTarget((w as IGetCompassTarget).GetCompassTarget());
		}
		else
		{
			SetCompassTarget(null);
		}
		return true;
	}

	public static void InitSignals()
	{
		SelectableDico.Clear();
		UIGameSelector.AddOnSelectedListener((SelectionType)5, OnSelected);
		UIGameSelector.AddOnUnselectedListener((SelectionType)5, OnSelected);
		UIGameSelector.AddOnHoveredListener((SelectionType)5, OnHovered);
		UIGameSelector.AddOnUnhoveredListener((SelectionType)5, OnHovered);
	}

	private static void OnSelected(UISelectable selectable)
	{
		if (SelectableDico.TryGetValue(selectable, out var value))
		{
			value.OnSelected();
		}
	}

	private static void OnHovered(UISelectable selectable)
	{
		if (SelectableDico.TryGetValue(selectable, out var value))
		{
			value.OnHovered();
		}
	}

	private void ResetCompass()
	{
		_go = null;
		_selectable = null;
		_bricktronDamageReceiver = null;
		_labor = null;
		_localFactionVision = null;
		_captureClock = null;
		_hasHoveredGetter = (CompassLayoutElementWidgetComponent me) => false;
		_hoveredGetter = (CompassLayoutElementWidgetComponent me) => false;
		_hasSelectedGetter = (CompassLayoutElementWidgetComponent me) => false;
		_selectedGetter = (CompassLayoutElementWidgetComponent me) => false;
		_hasHpGetter = (CompassLayoutElementWidgetComponent me) => false;
		_hpRatioGetter = (CompassLayoutElementWidgetComponent me) => 0f;
		_hasStaminaGetter = (CompassLayoutElementWidgetComponent me) => false;
		_staminaRatioGetter = (CompassLayoutElementWidgetComponent me) => 0f;
		_hasCaptureGetter = (CompassLayoutElementWidgetComponent me) => false;
		_captureRatioGetter = (CompassLayoutElementWidgetComponent me) => 0f;
	}

	private void SetCompass()
	{
		switch (_ld.Handle)
		{
		case "Project":
		case "fy_BuildBlocksProject":
		case "fy_QuarryProject":
		case "fy_LumberProject":
		case "fy_CleanupProject":
		case "fy_RepairProject":
		case "fy_LandscapeProject":
		case "fy_TunnelProject":
		case "fy_RaidProjectPlayer":
			_go = _ld.gameObject;
			_selectable = UIGameSelector.FindSelectable(_go);
			_hasHoveredGetter = (CompassLayoutElementWidgetComponent me) => IsSelf(me._go);
			_hoveredGetter = (CompassLayoutElementWidgetComponent me) => me._selectable.hover;
			_hasSelectedGetter = (CompassLayoutElementWidgetComponent me) => IsSelf(me._go);
			_selectedGetter = (CompassLayoutElementWidgetComponent me) => me._selectable.selection;
			break;
		case "fy_Bricktron":
			_go = _ld.gameObject;
			_selectable = UIGameSelector.FindSelectable(_go);
			_bricktronDamageReceiver = _go.GetComponent<BricktronDamageReceiver>();
			_labor = _go.GetComponent<Labor>();
			_localFactionVision = _go.GetComponent<LocalFactionVision>();
			_hasHoveredGetter = (CompassLayoutElementWidgetComponent me) => IsSelf(me._go);
			_hoveredGetter = (CompassLayoutElementWidgetComponent me) => me._selectable.hover;
			_hasSelectedGetter = (CompassLayoutElementWidgetComponent me) => IsSelf(me._go);
			_selectedGetter = (CompassLayoutElementWidgetComponent me) => me._selectable.selection;
			_hasHpGetter = (CompassLayoutElementWidgetComponent me) => IsSelfOrAllied(me._go);
			_hpRatioGetter = (CompassLayoutElementWidgetComponent me) => me._bricktronDamageReceiver.CurrentHP / me._bricktronDamageReceiver.MaxHP;
			_bricktronDamageReceiver.HPChangedSignal.Connect(delegate(float a, float b, CompassLayoutElementWidgetComponent self)
			{
				self.OnHpRatioChanged();
			}, this).DisconnectWhen(_disconnectCompassSignal);
			_hasStaminaGetter = (CompassLayoutElementWidgetComponent me) => IsSelfOrAllied(me._go) && me._labor.Occupation.CurrentOccupation == Occupation.Type.Knight;
			_staminaRatioGetter = (CompassLayoutElementWidgetComponent me) => me._bricktronDamageReceiver.CurrentStamina / me._bricktronDamageReceiver.MaxStamina;
			_bricktronDamageReceiver.StaminaChangedSignal.Connect(delegate(float a, float b, CompassLayoutElementWidgetComponent self)
			{
				self.OnStaminaRatioChanged();
			}, this).DisconnectWhen(_disconnectCompassSignal);
			_outFogGetter = (CompassLayoutElementWidgetComponent me) => LocalFactionVision.ObjectIsVisibleForFaction(me._go, User.LocalUser.faction);
			_localFactionVision.OnEnterFogSignal.Connect(delegate(CompassLayoutElementWidgetComponent self)
			{
				self.OnFogChanged();
			}, this).DisconnectWhen(_disconnectCompassSignal);
			_localFactionVision.OnExitFogSignal.Connect(delegate(CompassLayoutElementWidgetComponent self)
			{
				self.OnFogChanged();
			}, this).DisconnectWhen(_disconnectCompassSignal);
			_inDangerGetter = (CompassLayoutElementWidgetComponent me) => IsSelfOrAllied(me._go) && me._labor.IsInDanger;
			_labor.OnDangerStartedSignal.Connect(delegate(CompassLayoutElementWidgetComponent self)
			{
				self.OnDangerChanged();
			}, this).DisconnectWhen(_disconnectCompassSignal);
			_labor.OnDangerEndedSignal.Connect(delegate(CompassLayoutElementWidgetComponent self)
			{
				self.OnDangerChanged();
			}, this).DisconnectWhen(_disconnectCompassSignal);
			break;
		case "fy_Corruptron":
		case "fy_Minitron":
		case "fy_Biftron":
			_go = _ld.gameObject;
			_localFactionVision = _go.GetComponent<LocalFactionVision>();
			_outFogGetter = (CompassLayoutElementWidgetComponent me) => LocalFactionVision.ObjectIsVisibleForFaction(me._go, User.LocalUser.faction);
			_localFactionVision.OnEnterFogSignal.Connect(delegate(CompassLayoutElementWidgetComponent self)
			{
				self.OnFogChanged();
			}, this).DisconnectWhen(_disconnectCompassSignal);
			_localFactionVision.OnExitFogSignal.Connect(delegate(CompassLayoutElementWidgetComponent self)
			{
				self.OnFogChanged();
			}, this).DisconnectWhen(_disconnectCompassSignal);
			break;
		case "fy_Crystal":
			_go = _ld.gameObject;
			_localFactionVision = _go.GetComponent<LocalFactionVision>();
			_captureClock = _go.GetComponent<CaptureClock>();
			_outFogGetter = (CompassLayoutElementWidgetComponent me) => IsSelf(me._go) && LocalFactionVision.ObjectIsVisibleForFaction(me._go, User.LocalUser.faction);
			_localFactionVision.OnEnterFogSignal.Connect(delegate(CompassLayoutElementWidgetComponent self)
			{
				self.OnFogChanged();
			}, this).DisconnectWhen(_disconnectCompassSignal);
			_localFactionVision.OnExitFogSignal.Connect(delegate(CompassLayoutElementWidgetComponent self)
			{
				self.OnFogChanged();
			}, this).DisconnectWhen(_disconnectCompassSignal);
			_hasCaptureGetter = (CompassLayoutElementWidgetComponent me) => true;
			_captureRatioGetter = (CompassLayoutElementWidgetComponent me) => me._captureClock.CaptureRatio;
			_captureClock.CaptureRatioChanged.Connect(delegate(float value, CompassLayoutElementWidgetComponent self)
			{
				self.OnCaptureRatioChanged();
			}, this).DisconnectWhen(_disconnectCompassSignal);
			break;
		case "fy_CrystalFirefly":
			_go = _ld.gameObject;
			_localFactionVision = _go.GetComponent<LocalFactionVision>();
			_outFogGetter = (CompassLayoutElementWidgetComponent me) => IsSelf(me._go) && LocalFactionVision.ObjectIsVisibleForFaction(me._go, User.LocalUser.faction);
			_localFactionVision.OnEnterFogSignal.Connect(delegate(CompassLayoutElementWidgetComponent self)
			{
				self.OnFogChanged();
			}, this).DisconnectWhen(_disconnectCompassSignal);
			_localFactionVision.OnExitFogSignal.Connect(delegate(CompassLayoutElementWidgetComponent self)
			{
				self.OnFogChanged();
			}, this).DisconnectWhen(_disconnectCompassSignal);
			break;
		}
	}

	private void SetCompassTarget(LuaDriver ld)
	{
		if (!(_ld == ld))
		{
			if (_ld != null)
			{
				DisconnectCompass();
			}
			_ld = ld;
			if (_ld != null)
			{
				ConnectCompass();
			}
		}
	}

	private void ConnectCompass()
	{
		if (!_compassConnected)
		{
			_compassConnected = true;
			ResetCompass();
			SetCompass();
			InitCompass();
			if ((bool)_selectable)
			{
				SelectableDico.Add(_selectable, this);
			}
		}
	}

	private void DisconnectCompass()
	{
		if (_compassConnected)
		{
			_compassConnected = false;
			_disconnectCompassSignal.Invoke();
			if ((bool)_selectable)
			{
				SelectableDico.Remove(_selectable);
			}
			ResetCompass();
		}
	}

	public override void OnDestroy()
	{
		if (_ld != null)
		{
			DisconnectCompass();
		}
		UnityEngine.Object.Destroy(_material);
		base.OnDestroy();
	}

	private void InitCompass()
	{
		TryInitCompassMaterial();
		OnHovered(init: true);
		OnSelected(init: true);
		OnHpRatioChanged(init: true);
		OnStaminaRatioChanged(init: true);
		OnDangerChanged(init: true);
		OnFogChanged(init: true);
		OnCaptureRatioChanged(init: true);
	}

	private void TryInitCompassMaterial()
	{
		if (_material == null)
		{
			_material = new Material(SourceMaterial);
			_rawImage.material = _material;
		}
	}

	private void OnHovered(bool init = false)
	{
		if (init)
		{
			_hasHovered = _hasHoveredGetter(this);
			_material.SetFloat("HasHovered", (!_hasHovered) ? 0f : 1f);
		}
		if (_hasHovered)
		{
			bool hovered = _hoveredGetter(this);
			if (!hovered.Equals(_hovered))
			{
				_hovered = hovered;
				_material.SetFloat("Hovered", (!_hovered) ? 0f : 1f);
			}
		}
	}

	private void OnSelected(bool init = false)
	{
		if (init)
		{
			_hasSelected = _hasSelectedGetter(this);
			_material.SetFloat("HasSelected", (!_hasSelected) ? 0f : 1f);
		}
		if (_hasSelected)
		{
			bool selected = _selectedGetter(this);
			if (!selected.Equals(_selected))
			{
				_selected = selected;
				_material.SetFloat("Selected", (!_selected) ? 0f : 1f);
			}
		}
	}

	private void OnHpRatioChanged(bool init = false)
	{
		if (init)
		{
			_hasHp = _hasHpGetter(this);
			_material.SetFloat("HasHP", (!_hasHp) ? 0f : 1f);
		}
		if (_hasHp)
		{
			float hpRatio = _hpRatioGetter(this);
			if (!hpRatio.Equals(_hpRatio))
			{
				_hpRatio = hpRatio;
				_material.SetFloat("HP Amount", _hpRatio);
			}
		}
	}

	private void OnStaminaRatioChanged(bool init = false)
	{
		if (init)
		{
			_hasStamina = _hasStaminaGetter(this);
			_material.SetFloat("HasStam", (!_hasStamina) ? 0f : 1f);
		}
		if (_hasStamina)
		{
			float staminaRatio = _staminaRatioGetter(this);
			if (!staminaRatio.Equals(_staminaRatio))
			{
				_staminaRatio = staminaRatio;
				_material.SetFloat("Stam Amount", _staminaRatio);
			}
		}
	}

	private void OnDangerChanged(bool init = false)
	{
		bool inDanger = _inDangerGetter(this);
		if (!inDanger.Equals(_inDanger))
		{
			_inDanger = inDanger;
			_material.SetColor("Tint", (!_inDanger) ? Color.white : Color.red);
		}
	}

	private void OnFogChanged(bool init = false)
	{
		bool outFog = _outFogGetter(this);
		if (!outFog.Equals(_outFog))
		{
			_outFog = outFog;
			_material.SetFloat("OutFog", (!_outFog) ? 0f : 1f);
		}
	}

	private void OnCaptureRatioChanged(bool init = false)
	{
		if (init)
		{
			_hasCapture = _hasCaptureGetter(this);
			_material.SetFloat("HasFrame", (!_hasCapture) ? 0f : 1f);
		}
		if (_hasCapture)
		{
			float captureRatio = _captureRatioGetter(this);
			if (!captureRatio.Equals(_captureRatio))
			{
				_captureRatio = captureRatio;
				_material.SetFloat("Frame Progress", _captureRatio);
			}
		}
	}

	private static bool IsSelf(GameObject go)
	{
		if (Affiliation.BelongsToMe(go))
		{
			return true;
		}
		return false;
	}

	private static bool IsSelfOrAllied(GameObject go)
	{
		if (Affiliation.BelongsToMe(go))
		{
			return true;
		}
		if (Affiliation.BelongsToAllied(go))
		{
			return true;
		}
		return false;
	}
}
