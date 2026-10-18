using Brix.External.Signals;
using Brix.Game.AI.Damage;
using Brix.Lua;
using Brix.UI.Builder.Menu;
using Brix.Utils;
using Brix.Utils.Signals;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Widget;

public class ImageWidgetComponent : BaseWidgetComponent
{
	[SerializeField]
	protected Image _image;

	private bool _isHpTracking;

	private LuaDriver _hpTrackTarget;

	private DamageReceiver _targetDamageReceiver;

	private bool _isStaminaTracking;

	private LuaDriver _staminaTrackTarget;

	private BricktronDamageReceiver _targetStaminaDamageReceiver;

	private bool _isCaptureTracking;

	private LuaDriver _captureTrackTarget;

	private CaptureClock _targetCaptureClock;

	private readonly Signal _onDisconnect = new Signal();

	private bool _connected;

	public override bool Refresh(IWidget w)
	{
		base.Refresh(w);
		if (w.Is<IGetPreserveAspect>())
		{
			bool preserveAspect = (w as IGetPreserveAspect).GetPreserveAspect();
			if (_image.preserveAspect != preserveAspect)
			{
				_image.preserveAspect = preserveAspect;
			}
		}
		if (w.Is<IGetEnabled>())
		{
			bool flag = (w as IGetEnabled).GetEnabled();
			if (_image.enabled != flag)
			{
				_image.enabled = flag;
			}
		}
		if (w.Is<IGetSprite>())
		{
			Sprite sprite = (w as IGetSprite).GetSprite();
			if (_image.sprite != sprite)
			{
				_image.sprite = sprite;
			}
		}
		if (w.Is<IGetColor>())
		{
			Color color = (w as IGetColor).GetColor();
			if (_image.color != color)
			{
				_image.color = color;
			}
		}
		if (w.Is<IGetFill>() || w.Is<IGetHpTracking>() || w.Is<IGetStaminaTracking>() || w.Is<IGetCaptureTracking>())
		{
			Image.Type type = Image.Type.Filled;
			if (_image.type != type)
			{
				_image.type = type;
			}
			if (w.Is<IGetFillMethod>())
			{
				Image.FillMethod fillMethod = (w as IGetFillMethod).GetFillMethod();
				if (_image.fillMethod != fillMethod)
				{
					_image.fillMethod = fillMethod;
				}
			}
			else
			{
				Image.FillMethod fillMethod2 = Image.FillMethod.Horizontal;
				if (_image.fillMethod != fillMethod2)
				{
					_image.fillMethod = fillMethod2;
				}
			}
			bool flag2 = false;
			if (_image.preserveAspect != flag2)
			{
				_image.preserveAspect = flag2;
			}
			if (w.Is<IGetFill>())
			{
				float fill = (w as IGetFill).GetFill();
				if (_image.fillAmount != fill)
				{
					_image.fillAmount = fill;
				}
			}
		}
		if (w.Is<IGetRaycasts>())
		{
			bool raycasts = (w as IGetRaycasts).GetRaycasts();
			if (_image.raycastTarget != raycasts)
			{
				_image.raycastTarget = raycasts;
			}
		}
		bool flag3 = w.Is<IGetHpTracking>();
		SetHpTrackTarget((!flag3) ? null : (w as IGetHpTracking).GetHpTracking());
		bool flag4 = w.Is<IGetStaminaTracking>();
		SetStaminaTrackTarget((!flag4) ? null : (w as IGetStaminaTracking).GetStaminaTracking());
		bool flag5 = w.Is<IGetCaptureTracking>();
		SetCaptureTrackTarget((!flag5) ? null : (w as IGetCaptureTracking).GetCaptureTracking());
		return true;
	}

	private void SetHpTrackTarget(Table t)
	{
		if (t == null)
		{
			return;
		}
		DynValue dynValue = t.Get("driver");
		if (!dynValue.IsNil())
		{
			LuaDriver luaDriver = dynValue.ToObject<LuaDriver>();
			DamageReceiver component = luaDriver.GetComponent<DamageReceiver>();
			if (!(_hpTrackTarget == luaDriver) && !(component == null))
			{
				EndHpTracking();
				_targetDamageReceiver = component;
				_hpTrackTarget = luaDriver;
				BeginHpTracking();
			}
		}
	}

	private void BeginHpTracking()
	{
		if (!_isHpTracking)
		{
			_isHpTracking = true;
			Connect();
		}
	}

	private void EndHpTracking()
	{
		if (_isHpTracking)
		{
			_isHpTracking = false;
			_hpTrackTarget = null;
			Disconnect();
		}
	}

	public void OnHpTrack()
	{
		if (_isHpTracking && !(_hpTrackTarget == null))
		{
			float num = _targetDamageReceiver.CurrentHP / _targetDamageReceiver.MaxHP;
			if (_image.fillAmount != num)
			{
				_image.fillAmount = num;
			}
		}
	}

	private void SetStaminaTrackTarget(Table t)
	{
		if (t == null)
		{
			return;
		}
		DynValue dynValue = t.Get("driver");
		if (!dynValue.IsNil())
		{
			LuaDriver luaDriver = dynValue.ToObject<LuaDriver>();
			BricktronDamageReceiver component = luaDriver.GetComponent<BricktronDamageReceiver>();
			if (!(_staminaTrackTarget == luaDriver) && !(component == null))
			{
				EndStaminaTracking();
				_targetStaminaDamageReceiver = component;
				_staminaTrackTarget = luaDriver;
				BeginStaminaTracking();
			}
		}
	}

	private void BeginStaminaTracking()
	{
		if (!_isStaminaTracking)
		{
			_isStaminaTracking = true;
			Connect();
		}
	}

	private void EndStaminaTracking()
	{
		if (_isStaminaTracking)
		{
			_isStaminaTracking = false;
			_staminaTrackTarget = null;
			Disconnect();
		}
	}

	public void OnStaminaTrack()
	{
		if (_isStaminaTracking && !(_staminaTrackTarget == null))
		{
			float num = _targetStaminaDamageReceiver.CurrentStamina / _targetStaminaDamageReceiver.MaxStamina;
			if (_image.fillAmount != num)
			{
				_image.fillAmount = num;
			}
		}
	}

	private void SetCaptureTrackTarget(Table t)
	{
		if (t == null)
		{
			return;
		}
		DynValue dynValue = t.Get("driver");
		if (!dynValue.IsNil())
		{
			LuaDriver luaDriver = dynValue.ToObject<LuaDriver>();
			CaptureClock component = luaDriver.GetComponent<CaptureClock>();
			if (_isCaptureTracking != (bool)luaDriver && !(component == null))
			{
				EndCaptureTracking();
				_targetCaptureClock = component;
				_captureTrackTarget = luaDriver;
				BeginCaptureTracking();
			}
		}
	}

	private void BeginCaptureTracking()
	{
		if (!_isCaptureTracking)
		{
			_isCaptureTracking = true;
			Connect();
		}
	}

	private void EndCaptureTracking()
	{
		if (_isCaptureTracking)
		{
			_isCaptureTracking = false;
			_captureTrackTarget = null;
			Disconnect();
		}
	}

	public void OnCaptureTrack()
	{
		if (_isCaptureTracking && !(_captureTrackTarget == null))
		{
			float captureRatio = _targetCaptureClock.CaptureRatio;
			if (_image.fillAmount != captureRatio)
			{
				_image.fillAmount = captureRatio;
			}
		}
	}

	private void Connect()
	{
		if (!_connected)
		{
			_connected = true;
			PersistentCoroutineRunner.LateUpdateSignal.Connect(OnLateUpdate).DisconnectWhen(_onDisconnect);
		}
	}

	private void Disconnect()
	{
		if (_connected && !_isHpTracking && !_isStaminaTracking && !_isCaptureTracking)
		{
			_connected = false;
			_onDisconnect.Invoke();
		}
	}

	private void OnLateUpdate()
	{
		OnHpTrack();
		OnStaminaTrack();
		OnCaptureTrack();
	}

	private void OnDestroy()
	{
		EndCaptureTracking();
		EndStaminaTracking();
		EndHpTracking();
	}
}
