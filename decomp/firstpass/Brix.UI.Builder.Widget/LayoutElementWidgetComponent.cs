using Brix.External.Signals;
using Brix.Lua;
using Brix.UI.Builder.Lui;
using Brix.UI.Builder.Menu;
using Brix.Utils;
using Brix.Utils.Signals;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Widget;

public class LayoutElementWidgetComponent : BaseWidgetComponent
{
	public enum TrimWhen
	{
		Never,
		Inside,
		Outside
	}

	public enum ClampWhen
	{
		Never,
		Inside,
		Outside
	}

	public enum ClampTo
	{
		Nothing,
		Border,
		Middle
	}

	public LuiLayoutElementWidget LuiLayoutElementWidget = new LuiLayoutElementWidget();

	[SerializeField]
	protected RectTransform _rectTransform;

	[SerializeField]
	protected LayoutElement _layoutElement;

	[SerializeField]
	protected ContentSizeFitter _fitter;

	[SerializeField]
	protected ContentSizeFitterPlus _fitterPlus;

	private float _screenWidth;

	private float _screenHeight;

	private bool _hidingForOneFrame;

	private CanvasGroup _canvasGroup;

	private bool _wantsAutoFitOnce;

	private bool _isAutoFittingOnce;

	private Transform _posTrackTargetTrs;

	private bool _isPosTracking;

	private Vector2 _posOffset2D = Vector2.zero;

	private Vector3 _posOffset3D = Vector3.zero;

	private bool _posUseParentSpace;

	private bool _posUse3D;

	private bool _posTrack_yPosInversedBehindCamera2D = true;

	private bool _posTrackOnce;

	private int _hasPosTracked = 3;

	private bool _hasPrevPos;

	private Vector3 _prevPos = Vector3.zero;

	private Transform _rotTrackTargetTrs;

	private bool _isRotTracking;

	private Vector2 _rotPosOffset2D = Vector2.zero;

	private Vector2 _rotTargetPosOffset2D = Vector2.zero;

	private Vector3 _rotTargetPosOffset3D = Vector3.zero;

	private float _rotAngleOffset;

	private bool _rotTrack_yPosInversedBehindCamera2D = true;

	private bool _rotTrackOnce;

	private bool _hasRotTracked;

	private bool _isTrimmed;

	private bool _isTrimming;

	private TrimWhen _trimWhen;

	private Vector4 _trimWhenOffset;

	private bool _trimOnce;

	private bool _hasTrimmed;

	private bool _isClamping;

	private ClampWhen _clampWhen;

	private ClampTo _clampTo;

	private Vector4 _clampWhenOffset;

	private Vector4 _clampToOffset;

	private bool _autoClampWhenOffset;

	private bool _autoClampToOffset;

	private bool _clampOnce;

	private bool _hasClamped;

	private float _w;

	private float _h;

	private RectTransform _clampingParentRectTrs;

	private static bool _trimpAndClamp_Begin = true;

	private static bool _trimpAndClamp_UpdateSignal = true;

	private static bool _trimpAndClamp_LateUpdateSignal = true;

	private static bool _trimpAndClamp_OnEndOfFrameSignal = true;

	private static bool _trimpAndClamp_OnEnable = true;

	private static bool _trimpAndClamp_Update = true;

	private static bool _trimpAndClamp_LateUpdate = true;

	private readonly Signal _onDisconnect = new Signal();

	private bool _connected;

	private bool HasPosTracked
	{
		get
		{
			return _hasPosTracked == 0;
		}
		set
		{
			if (value)
			{
				_hasPosTracked--;
			}
			else
			{
				_hasPosTracked = 3;
			}
		}
	}

	public override bool Refresh(IWidget w)
	{
		base.Refresh(w);
		bool flag = false;
		bool flag2 = true;
		if (w.Is<IGetVisible>())
		{
			flag2 = (w as IGetVisible).GetVisible();
		}
		bool activeSelf = _rectTransform.gameObject.activeSelf;
		bool flag3 = flag2;
		if (!activeSelf && flag3)
		{
			flag = true;
		}
		if (activeSelf != flag3)
		{
			_rectTransform.gameObject.SetActive(flag3);
		}
		if (!flag2)
		{
			SetPosTrackTarget(null);
			SetRotTrackTarget(null);
			SetTrimming(null);
			SetClamping(null);
			return false;
		}
		if (flag && w.Is<IGetHideFirstFrame>())
		{
			HideForOneFrame();
		}
		if (w.Is<IGetPivot>())
		{
			Vector2 pivot = (w as IGetPivot).GetPivot();
			if (_rectTransform.pivot != pivot)
			{
				_rectTransform.pivot = pivot;
			}
		}
		if (w.Is<IGetSiblingIndex>())
		{
			int siblingIndex = (w as IGetSiblingIndex).GetSiblingIndex();
			if (siblingIndex != -1)
			{
				int num = siblingIndex;
				if (_rectTransform.GetSiblingIndex() != num)
				{
					_rectTransform.SetSiblingIndex(num);
				}
			}
		}
		if (w.Is<IGetAutoFitPlus>())
		{
			AutoFitPlus((w as IGetAutoFitPlus).GetAutoFitPlus());
		}
		bool flag4 = w.Is<IGetIgnoreLayout>() && (w as IGetIgnoreLayout).GetIgnoreLayout();
		bool flag5 = flag4;
		if (_layoutElement.ignoreLayout != flag5)
		{
			_layoutElement.ignoreLayout = flag5;
		}
		if (w.Is<IGetAutoFit>())
		{
			bool flag6 = _fitter.enabled;
			bool autoFit = (w as IGetAutoFit).GetAutoFit();
			bool flag7 = autoFit;
			if (_fitter.enabled != flag7)
			{
				_fitter.enabled = flag7;
			}
			if (autoFit)
			{
				if (w.Is<IGetAutoFitHorizontal>())
				{
					ContentSizeFitter.FitMode autoFitHorizontal = (w as IGetAutoFitHorizontal).GetAutoFitHorizontal();
					if (_fitter.horizontalFit != autoFitHorizontal)
					{
						_fitter.horizontalFit = autoFitHorizontal;
					}
					_fitter.SetLayoutHorizontal();
				}
				if (w.Is<IGetAutoFitVertical>())
				{
					ContentSizeFitter.FitMode autoFitVertical = (w as IGetAutoFitVertical).GetAutoFitVertical();
					if (_fitter.verticalFit != autoFitVertical)
					{
						_fitter.verticalFit = autoFitVertical;
					}
					_fitter.SetLayoutVertical();
				}
				if (w.Is<IGetAutoFitOnce>())
				{
					AutoFitOnce((w as IGetAutoFitOnce).GetAutoFitOnce());
				}
			}
			if (w.Is<IGetAutoFitForcesLayoutUpdate>() && (w as IGetAutoFitForcesLayoutUpdate).GetAutoFitForcesLayoutUpdate() && autoFit && !flag6)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
			}
		}
		if (flag4)
		{
			if (w.Is<IGetAnchorMin>())
			{
				Vector2 anchorMin = (w as IGetAnchorMin).GetAnchorMin();
				if (_rectTransform.anchorMin != anchorMin)
				{
					_rectTransform.anchorMin = anchorMin;
				}
			}
			if (w.Is<IGetAnchorMax>())
			{
				Vector2 anchorMax = (w as IGetAnchorMax).GetAnchorMax();
				if (_rectTransform.anchorMax != anchorMax)
				{
					_rectTransform.anchorMax = anchorMax;
				}
			}
			if (w.Is<IGetOffsetMin>())
			{
				Vector2 offsetMin = (w as IGetOffsetMin).GetOffsetMin();
				if (_rectTransform.offsetMin != offsetMin)
				{
					_rectTransform.offsetMin = offsetMin;
				}
			}
			if (w.Is<IGetOffsetMax>())
			{
				Vector2 offsetMax = (w as IGetOffsetMax).GetOffsetMax();
				if (_rectTransform.offsetMax != offsetMax)
				{
					_rectTransform.offsetMax = offsetMax;
				}
			}
			bool flag8 = w.Is<IGetAnchoredPosition>();
			bool flag9 = w.Is<IGetAnchoredPositionTracking>();
			if (flag8 || flag9)
			{
				if (flag8)
				{
					Vector2 anchoredPosition = (w as IGetAnchoredPosition).GetAnchoredPosition();
					if (_rectTransform.anchoredPosition != anchoredPosition)
					{
						_rectTransform.anchoredPosition = anchoredPosition;
					}
					SetPosTrackTarget(null);
				}
				else if (flag9)
				{
					SetPosTrackTarget((w as IGetAnchoredPositionTracking).GetAnchoredPositionTracking());
				}
				bool flag10 = w.Is<IGetAnchoredPositionTrimming>();
				bool flag11 = w.Is<IGetAnchoredPositionClamping>();
				bool flag12 = w.Is<IGetOrientationTracking>();
				SetTrimming((!flag10) ? null : (w as IGetAnchoredPositionTrimming).GetAnchoredPositionTrimming());
				SetClamping((!flag11) ? null : (w as IGetAnchoredPositionClamping).GetAnchoredPositionClamping());
				SetRotTrackTarget((!flag12) ? null : (w as IGetOrientationTracking).GetOrientationTracking());
			}
			else
			{
				SetPosTrackTarget(null);
				SetRotTrackTarget(null);
				SetTrimming(null);
				SetClamping(null);
			}
			bool flag13 = w.Is<IGetAnchoredWidth>();
			bool flag14 = w.Is<IGetAnchoredHeight>();
			if (flag13 || flag14)
			{
				Vector2 sizeDelta = _rectTransform.sizeDelta;
				Vector2 vector = sizeDelta;
				Rect rect = (_rectTransform.parent as RectTransform).rect;
				if (flag13)
				{
					float anchoredWidth = (w as IGetAnchoredWidth).GetAnchoredWidth();
					vector.x = anchoredWidth - rect.width * (_rectTransform.anchorMax.x - _rectTransform.anchorMin.x);
				}
				if (flag14)
				{
					float anchoredHeight = (w as IGetAnchoredHeight).GetAnchoredHeight();
					vector.y = anchoredHeight - rect.height * (_rectTransform.anchorMax.y - _rectTransform.anchorMin.y);
				}
				if (sizeDelta != vector)
				{
					_rectTransform.sizeDelta = vector;
				}
			}
		}
		else
		{
			if (w.Is<IGetMinWidth>())
			{
				float minWidth = (w as IGetMinWidth).GetMinWidth();
				if (_layoutElement.minWidth != minWidth)
				{
					_layoutElement.minWidth = minWidth;
				}
			}
			if (w.Is<IGetMinHeight>())
			{
				float minHeight = (w as IGetMinHeight).GetMinHeight();
				if (_layoutElement.minHeight != minHeight)
				{
					_layoutElement.minHeight = minHeight;
				}
			}
			if (w.Is<IGetPreferredWidth>())
			{
				float preferredWidth = (w as IGetPreferredWidth).GetPreferredWidth();
				if (_layoutElement.preferredWidth != preferredWidth)
				{
					_layoutElement.preferredWidth = preferredWidth;
				}
			}
			if (w.Is<IGetPreferredHeight>())
			{
				float preferredHeight = (w as IGetPreferredHeight).GetPreferredHeight();
				if (_layoutElement.preferredHeight != preferredHeight)
				{
					_layoutElement.preferredHeight = preferredHeight;
				}
			}
			if (w.Is<IGetFlexibleWidth>())
			{
				float flexibleWidth = (w as IGetFlexibleWidth).GetFlexibleWidth();
				if (_layoutElement.flexibleWidth != flexibleWidth)
				{
					_layoutElement.flexibleWidth = flexibleWidth;
				}
			}
			if (w.Is<IGetFlexibleHeight>())
			{
				float flexibleHeight = (w as IGetFlexibleHeight).GetFlexibleHeight();
				if (_layoutElement.flexibleHeight != flexibleHeight)
				{
					_layoutElement.flexibleHeight = flexibleHeight;
				}
			}
			SetPosTrackTarget(null);
			SetRotTrackTarget(null);
			SetTrimming(null);
			SetClamping(null);
		}
		if (w.Is<IGetLocalRotation>())
		{
			Vector3 localRotation = (w as IGetLocalRotation).GetLocalRotation();
			if (_rectTransform.localEulerAngles != localRotation)
			{
				_rectTransform.localEulerAngles = localRotation;
			}
		}
		return true;
	}

	private void HideForOneFrame()
	{
		if (_canvasGroup == null)
		{
			_canvasGroup = base.gameObject.AddComponent<CanvasGroup>();
			_canvasGroup.blocksRaycasts = true;
			_canvasGroup.interactable = true;
			_canvasGroup.ignoreParentGroups = false;
		}
		if (_hidingForOneFrame)
		{
			return;
		}
		_hidingForOneFrame = true;
		_canvasGroup.alpha = 0f;
		PersistentCoroutineRunner.Relais(delegate(LayoutElementWidgetComponent aLayoutElementWidgetComponent)
		{
			if (!(aLayoutElementWidgetComponent == null))
			{
				aLayoutElementWidgetComponent._hidingForOneFrame = false;
				if (!(aLayoutElementWidgetComponent._canvasGroup == null))
				{
					aLayoutElementWidgetComponent._canvasGroup.alpha = 1f;
				}
			}
		}, this);
	}

	private void AutoFitOnce(bool wantsAutoFitOnce)
	{
		_wantsAutoFitOnce = wantsAutoFitOnce;
		if (!_wantsAutoFitOnce || _isAutoFittingOnce)
		{
			return;
		}
		_isAutoFittingOnce = true;
		PersistentCoroutineRunner.Relais(delegate(LayoutElementWidgetComponent self)
		{
			PersistentCoroutineRunner.Relais(delegate(LayoutElementWidgetComponent self2)
			{
				OnAfterAutoFitOnce(self2);
			}, self);
		}, this);
	}

	private static void OnAfterAutoFitOnce(LayoutElementWidgetComponent self)
	{
		if (!(self == null))
		{
			self._isAutoFittingOnce = false;
			if (self._wantsAutoFitOnce)
			{
				self._wantsAutoFitOnce = false;
				self._fitter.enabled = false;
			}
		}
	}

	private void AutoFitPlus(Table t)
	{
		if (t != null)
		{
			DynValue dynValue = t.Get("mode");
			switch ((!dynValue.IsNil()) ? ((int)dynValue.Number) : 0)
			{
			case 0:
				_fitterPlus._useBounds = true;
				_fitterPlus._useAltBounds = true;
				_fitterPlus._useAltBounds2 = true;
				break;
			case 1:
				_fitterPlus._useBounds = true;
				_fitterPlus._useAltBounds = true;
				_fitterPlus._useAltBounds2 = false;
				break;
			case 2:
				_fitterPlus._useBounds = true;
				_fitterPlus._useAltBounds = false;
				_fitterPlus._useAltBounds2 = false;
				break;
			case 3:
				_fitterPlus._useBounds = false;
				_fitterPlus._useAltBounds = false;
				_fitterPlus._useAltBounds2 = false;
				break;
			}
			DynValue dynValue2 = t.Get("sizeOffset");
			_fitterPlus._sizeOffset = ((!dynValue2.IsNil()) ? dynValue2.ToObject<Vector2>() : Vector2.zero);
			DynValue dynValue3 = t.Get("padding");
			Vector2 vector = ((!dynValue3.IsNil()) ? dynValue3.ToObject<Vector2>() : Vector2.zero);
			_fitterPlus.PaddingX = vector.x;
			_fitterPlus.PaddingY = vector.y;
			DynValue dv = t.Get("horizontalFit");
			ContentSizeFitter.FitMode horizontalFit = ((!dv.IsNil()) ? dv.ToEnum<ContentSizeFitter.FitMode>() : ContentSizeFitter.FitMode.Unconstrained);
			_fitterPlus._horizontalFit = (ContentSizeFitterPlus.Operation)horizontalFit;
			DynValue dv2 = t.Get("verticalFit");
			ContentSizeFitter.FitMode verticalFit = ((!dv2.IsNil()) ? dv2.ToEnum<ContentSizeFitter.FitMode>() : ContentSizeFitter.FitMode.Unconstrained);
			_fitterPlus._verticalFit = (ContentSizeFitterPlus.Operation)verticalFit;
			_fitterPlus._target = ContentSizeFitterPlus.Target.RectTransform;
			_fitterPlus._isConstrainOnUpdate = true;
			_fitterPlus._isConstrainOnLateUpdate = true;
			_fitterPlus._isConstrainOnEof = true;
			_fitterPlus._useAllDescendants = false;
			_fitterPlus.enabled = true;
			_fitterPlus.Constrain();
		}
	}

	private void SetPosTrackTarget(Table t)
	{
		if (t == null)
		{
			return;
		}
		_screenWidth = Screen.width;
		_screenHeight = Screen.height;
		DynValue dynValue = t.Get("posOffset2D");
		_posOffset2D = ((!dynValue.IsNil()) ? dynValue.ToObject<Vector2>() : Vector2.zero);
		DynValue dynValue2 = t.Get("posOffset3D");
		_posOffset3D = ((!dynValue2.IsNil()) ? dynValue2.ToObject<Vector3>() : Vector3.zero);
		DynValue dynValue3 = t.Get("useParentSpace");
		if (dynValue3.IsNotNil())
		{
			_posUseParentSpace = dynValue3.Boolean;
		}
		DynValue dynValue4 = t.Get("posUse3D");
		if (dynValue4.IsNotNil())
		{
			_posUse3D = dynValue4.Boolean;
		}
		DynValue dynValue5 = t.Get("yPosInversedBehindCamera2D");
		if (dynValue5.IsNotNil())
		{
			_posTrack_yPosInversedBehindCamera2D = dynValue5.Boolean;
		}
		DynValue dynValue6 = t.Get("once");
		if (dynValue6.IsNotNil())
		{
			_posTrackOnce = dynValue6.Boolean;
		}
		Transform transform = _posTrackTargetTrs;
		DynValue dynValue7 = t.Get("targetDriver");
		if (dynValue7.IsNotNil())
		{
			LuaDriver luaDriver = dynValue7.ToObject<LuaDriver>();
			if (luaDriver != null)
			{
				transform = luaDriver.transform;
			}
		}
		DynValue dynValue8 = t.Get("targetPanel");
		if (dynValue8.IsNotNil())
		{
			Table table = dynValue8.Table;
			if (table != null && LuiBuilder.MapHelper.HasPanel(table))
			{
				transform = LuiBuilder.MapHelper.GetPanel(table).GetTrs();
			}
		}
		if (!(transform == _posTrackTargetTrs))
		{
			EndPosTracking();
			_posTrackTargetTrs = transform;
			BeginPosTracking();
		}
	}

	private void BeginPosTracking()
	{
		if (!_isPosTracking)
		{
			_isPosTracking = true;
			HasPosTracked = false;
			_hasPrevPos = false;
			Connect();
		}
	}

	private void EndPosTracking()
	{
		if (_isPosTracking)
		{
			_isPosTracking = false;
			_posTrackTargetTrs = null;
			HasPosTracked = false;
			_hasPrevPos = false;
			Disconnect();
		}
	}

	public void OnPosTrack()
	{
		if (!_isPosTracking || _posTrackTargetTrs == null)
		{
			return;
		}
		_hasPrevPos = false;
		_posTrackOnce = false;
		if (_posTrackOnce && HasPosTracked)
		{
			return;
		}
		Vector3 position = _posTrackTargetTrs.transform.position;
		if (!(position == Vector3.zero))
		{
			position += _posOffset3D;
			Vector2 vector = new Vector2(position.x, position.y);
			if (!_posUse3D)
			{
				vector = RectTransformUtility.WorldToScreenPoint(Camera.main, position);
			}
			if (_posTrack_yPosInversedBehindCamera2D && !_posUse3D && IsBehindCamera(position))
			{
				vector.y = _screenHeight - vector.y;
			}
			if (_posUseParentSpace)
			{
				RectTransform rectTransform = base.transform.parent as RectTransform;
				Rect rect = rectTransform.rect;
				Vector2 sizeDelta = rectTransform.sizeDelta;
				Vector2 vector2 = ((!_posUse3D) ? new Vector2(_screenWidth, _screenHeight) : new Vector2(1024f, 1024f));
				vector.x /= vector2.x;
				vector.y /= vector2.y;
				vector.x *= ((rect.width != 0f) ? rect.width : sizeDelta.x);
				vector.y *= ((rect.height != 0f) ? rect.height : sizeDelta.y);
				Vector3 position2 = base.transform.parent.position;
				vector.x += position2.x;
				vector.y += position2.y;
			}
			vector.x += _posOffset2D.x;
			vector.y += _posOffset2D.y;
			Vector3 position3 = _rectTransform.position;
			Vector3 vector3 = position3;
			vector3.x = vector.x;
			vector3.y = vector.y;
			vector3.x = Mathf.Round(vector3.x);
			vector3.y = Mathf.Round(vector3.y);
			vector3.z = Mathf.Round(vector3.z);
			if (!_hasPrevPos || _prevPos != vector3)
			{
				_rectTransform.position = vector3;
				_prevPos = vector3;
				_hasPrevPos = true;
			}
			else
			{
				HasPosTracked = true;
			}
		}
	}

	public static bool IsBehindCamera(Vector3 targetPos)
	{
		Transform transform = Camera.main.transform;
		Vector3 lhs = targetPos - transform.position;
		return Vector3.Dot(lhs, transform.forward) < 0f;
	}

	private void SetRotTrackTarget(Table t)
	{
		if (t == null)
		{
			return;
		}
		DynValue dynValue = t.Get("posOffset2D");
		_rotPosOffset2D = ((!dynValue.IsNil()) ? dynValue.ToObject<Vector2>() : Vector2.zero);
		DynValue dynValue2 = t.Get("targetPosOffset2D");
		_rotTargetPosOffset2D = ((!dynValue2.IsNil()) ? dynValue2.ToObject<Vector2>() : Vector2.zero);
		DynValue dynValue3 = t.Get("targetPosOffset3D");
		_rotTargetPosOffset3D = ((!dynValue3.IsNil()) ? dynValue3.ToObject<Vector3>() : Vector3.zero);
		DynValue dynValue4 = t.Get("angleOffset");
		_rotAngleOffset = ((!dynValue4.IsNil()) ? ((float)dynValue4.Number) : 0f);
		DynValue dynValue5 = t.Get("yPosInversedBehindCamera2D");
		if (dynValue5.IsNotNil())
		{
			_rotTrack_yPosInversedBehindCamera2D = dynValue5.Boolean;
		}
		DynValue dynValue6 = t.Get("once");
		if (dynValue6.IsNotNil())
		{
			_rotTrackOnce = dynValue6.Boolean;
		}
		Transform transform = _rotTrackTargetTrs;
		DynValue dynValue7 = t.Get("targetDriver");
		if (dynValue7.IsNotNil())
		{
			LuaDriver luaDriver = dynValue7.ToObject<LuaDriver>();
			if (luaDriver != null)
			{
				transform = luaDriver.transform;
			}
		}
		DynValue dynValue8 = t.Get("targetPanel");
		if (dynValue8.IsNotNil())
		{
			Table table = dynValue8.Table;
			if (table != null && LuiBuilder.MapHelper.HasPanel(table))
			{
				transform = LuiBuilder.MapHelper.GetPanel(table).GetTrs();
			}
		}
		if (!(transform == _rotTrackTargetTrs))
		{
			EndRotTracking();
			_rotTrackTargetTrs = transform;
			BeginRotTracking();
		}
	}

	private void BeginRotTracking()
	{
		if (!_isRotTracking)
		{
			_isRotTracking = true;
			_hasRotTracked = false;
			Connect();
		}
	}

	private void EndRotTracking()
	{
		if (_isRotTracking)
		{
			_isRotTracking = false;
			_rotTrackTargetTrs = null;
			_hasRotTracked = false;
			Disconnect();
		}
	}

	public void OnRotTrack()
	{
		if (!_isRotTracking || _rotTrackTargetTrs == null)
		{
			Vector3 zero = Vector3.zero;
			if (_rectTransform.localEulerAngles != zero)
			{
				_rectTransform.localEulerAngles = zero;
			}
		}
		else
		{
			if (_rotTrackOnce && _hasRotTracked)
			{
				return;
			}
			_hasRotTracked = true;
			Vector3 position = _rotTrackTargetTrs.position;
			if (!(position == Vector3.zero))
			{
				position += _rotTargetPosOffset3D;
				Vector2 vector = RectTransformUtility.WorldToScreenPoint(Camera.main, position);
				vector.x += _rotTargetPosOffset2D.x;
				vector.y += _rotTargetPosOffset2D.y;
				if (_rotTrack_yPosInversedBehindCamera2D && IsBehindCamera(position))
				{
					_screenWidth = Screen.width;
					_screenHeight = Screen.height;
					vector.y = _screenHeight - vector.y;
				}
				Vector3 position2 = base.transform.position;
				position2.x += _rotPosOffset2D.x;
				position2.y += _rotPosOffset2D.y;
				Vector2 vector2 = vector;
				vector2.x -= position2.x;
				vector2.y -= position2.y;
				Vector3 localEulerAngles = _rotTrackTargetTrs.localEulerAngles;
				localEulerAngles.z = Mathf.Atan2(vector2.y, vector2.x) * 57.29578f;
				localEulerAngles.z += _rotAngleOffset;
				localEulerAngles.z -= 90f;
				localEulerAngles.x = 0f;
				localEulerAngles.y = 0f;
				Vector3 vector3 = localEulerAngles;
				if (_rectTransform.eulerAngles != vector3)
				{
					_rectTransform.eulerAngles = vector3;
				}
			}
		}
	}

	private void SetTrimming(Table t)
	{
		if (t == null)
		{
			EndTrimming();
			return;
		}
		_trimWhen = TrimWhen.Never;
		_trimWhenOffset = Vector4.zero;
		DynValue dv = t.Get("trimWhen");
		if (dv.IsNotNil())
		{
			_trimWhen = dv.ToEnum<TrimWhen>();
		}
		DynValue dynValue = t.Get("trimWhenOffset");
		if (dynValue.IsNotNil())
		{
			_trimWhenOffset = dynValue.ToObject<Vector4>();
		}
		DynValue dynValue2 = t.Get("once");
		if (dynValue2.IsNotNil())
		{
			_trimOnce = dynValue2.Boolean;
		}
		BeginTrimming();
	}

	private void BeginTrimming()
	{
		if (!_isTrimming)
		{
			_isTrimming = true;
			_hasTrimmed = false;
			Connect();
			OnTrim();
		}
	}

	private void EndTrimming()
	{
		if (_isTrimming)
		{
			_isTrimming = false;
			_hasTrimmed = false;
			Disconnect();
		}
	}

	public void OnTrim()
	{
		_isTrimmed = false;
		if (_isTrimming && (!_trimOnce || !_hasTrimmed))
		{
			_hasTrimmed = true;
			Vector3 localPosition = base.transform.localPosition;
			Rect rect = (base.transform.parent as RectTransform).rect;
			Rect rect2 = new Rect(rect);
			rect2.xMin -= _trimWhenOffset.x;
			rect2.yMin -= _trimWhenOffset.y;
			rect2.xMax += _trimWhenOffset.z;
			rect2.yMax += _trimWhenOffset.w;
			bool flag = base.gameObject.activeSelf;
			if (_trimWhen.HasFlag(TrimWhen.Inside))
			{
				flag = !rect2.Contains(localPosition);
				_isTrimmed = !flag;
			}
			else if (_trimWhen.HasFlag(TrimWhen.Outside))
			{
				flag = rect2.Contains(localPosition);
				_isTrimmed = !flag;
			}
			bool activeSelf = base.gameObject.activeSelf;
			if (activeSelf != flag)
			{
				base.gameObject.SetActive(flag);
			}
		}
	}

	private void SetClamping(Table t)
	{
		if (t == null)
		{
			EndClamping();
			return;
		}
		_clampWhen = ClampWhen.Never;
		_clampWhenOffset = Vector4.zero;
		_clampTo = ClampTo.Nothing;
		_clampToOffset = Vector4.zero;
		DynValue dv = t.Get("clampWhen");
		if (dv.IsNotNil())
		{
			_clampWhen = dv.ToEnum<ClampWhen>();
		}
		DynValue dynValue = t.Get("clampWhenOffset");
		if (dynValue.IsNotNil())
		{
			_clampWhenOffset = dynValue.ToObject<Vector4>();
		}
		DynValue dynValue2 = t.Get("autoClampWhenOffset");
		if (dynValue2.IsNotNil())
		{
			_autoClampWhenOffset = dynValue2.Boolean;
		}
		DynValue dv2 = t.Get("clampTo");
		if (dv2.IsNotNil())
		{
			_clampTo = dv2.ToEnum<ClampTo>();
		}
		DynValue dynValue3 = t.Get("clampToOffset");
		if (dynValue3.IsNotNil())
		{
			_clampToOffset = dynValue3.ToObject<Vector4>();
		}
		DynValue dynValue4 = t.Get("autoClampToOffset");
		if (dynValue4.IsNotNil())
		{
			_autoClampToOffset = dynValue4.Boolean;
		}
		DynValue dynValue5 = t.Get("once");
		if (dynValue5.IsNotNil())
		{
			_clampOnce = dynValue5.Boolean;
		}
		Table table = t.Get("parentPanel").Table;
		if (table != null)
		{
			_clampingParentRectTrs = ((!LuiBuilder.MapHelper.HasPanel(table)) ? null : LuiBuilder.MapHelper.GetPanel(table).GetTrs());
		}
		else
		{
			_clampingParentRectTrs = null;
		}
		BeginClamping();
	}

	private void BeginClamping()
	{
		if (!_isClamping)
		{
			_isClamping = true;
			_hasClamped = false;
			Connect();
			if (_trimpAndClamp_Begin)
			{
				TrimAndClamp("Begin");
			}
		}
	}

	private void EndClamping()
	{
		if (_isClamping)
		{
			_isClamping = false;
			_hasClamped = false;
			Disconnect();
		}
	}

	private void OnClamp(string info = "")
	{
		if (!_isClamping || _isTrimmed || (_clampOnce && _hasClamped))
		{
			return;
		}
		if (_autoClampToOffset || _autoClampWhenOffset)
		{
			RectTransform rectTransform = base.transform as RectTransform;
			if (rectTransform != null)
			{
				float x = rectTransform.sizeDelta.x;
				float y = rectTransform.sizeDelta.y;
				Vector2 pivot = rectTransform.pivot;
				float x2 = (0f - x) * pivot.x;
				float y2 = (0f - y) * pivot.y;
				float z = (0f - x) * (1f - pivot.x);
				float w = (0f - y) * (1f - pivot.y);
				if (_autoClampToOffset)
				{
					_clampToOffset = new Vector4(x2, y2, z, w);
				}
				if (_autoClampWhenOffset)
				{
					_clampWhenOffset = new Vector4(x2, y2, z, w);
				}
			}
		}
		Vector3 vector;
		Rect rect;
		if (_clampingParentRectTrs == null)
		{
			vector = base.transform.localPosition;
			rect = (base.transform.parent as RectTransform).rect;
		}
		else
		{
			vector = _clampingParentRectTrs.InverseTransformPoint(base.transform.position);
			rect = _clampingParentRectTrs.rect;
		}
		Rect rect2 = new Rect(rect);
		rect2.xMin -= _clampWhenOffset.x;
		rect2.yMin -= _clampWhenOffset.y;
		rect2.xMax += _clampWhenOffset.z;
		rect2.yMax += _clampWhenOffset.w;
		Rect rect3 = new Rect(rect);
		rect3.xMin -= _clampToOffset.x;
		rect3.yMin -= _clampToOffset.y;
		rect3.xMax += _clampToOffset.z;
		rect3.yMax += _clampToOffset.w;
		bool flag = rect2.Contains(vector);
		if ((!flag || !_clampWhen.HasFlag(ClampWhen.Inside)) && (flag || !_clampWhen.HasFlag(ClampWhen.Outside)))
		{
			return;
		}
		if (_clampTo.HasFlag(ClampTo.Border))
		{
			vector.x = Mathf.Clamp(vector.x, rect3.xMin, rect3.xMax);
			vector.y = Mathf.Clamp(vector.y, rect3.yMin, rect3.yMax);
		}
		else if (_clampTo.HasFlag(ClampTo.Middle))
		{
			if (vector.x < rect3.xMin)
			{
				vector.x = 0f - _clampToOffset.x;
			}
			if (vector.x > rect3.xMax)
			{
				vector.x = _clampToOffset.z;
			}
			if (vector.y < rect3.yMin)
			{
				vector.y = 0f - _clampToOffset.x;
			}
			if (vector.y > rect3.yMax)
			{
				vector.y = _clampToOffset.z;
			}
		}
		vector.x = Mathf.Round(vector.x);
		vector.y = Mathf.Round(vector.y);
		vector.z = Mathf.Round(vector.z);
		if (base.transform.localPosition != vector)
		{
			base.transform.localPosition = vector;
		}
		else
		{
			_hasClamped = true;
		}
	}

	[ContextMenu("Toggle_Begin")]
	private void Toggle_Begin()
	{
		_trimpAndClamp_Begin = !_trimpAndClamp_Begin;
		Debug.Log("_trimpAndClamp_Begin == " + _trimpAndClamp_Begin);
	}

	[ContextMenu("Toggle_UpdateSignal")]
	private void Toggle_UpdateSignal()
	{
		_trimpAndClamp_UpdateSignal = !_trimpAndClamp_UpdateSignal;
		Debug.Log("_trimpAndClamp_UpdateSignal == " + _trimpAndClamp_UpdateSignal);
	}

	[ContextMenu("Toggle_LateUpdateSignal")]
	private void Toggle_LateUpdateSignal()
	{
		_trimpAndClamp_LateUpdateSignal = !_trimpAndClamp_LateUpdateSignal;
		Debug.Log("_trimpAndClamp_LateUpdateSignal == " + _trimpAndClamp_LateUpdateSignal);
	}

	[ContextMenu("Toggle_OnEndOfFrameSignal")]
	private void Toggle_OnEndOfFrameSignal()
	{
		_trimpAndClamp_OnEndOfFrameSignal = !_trimpAndClamp_OnEndOfFrameSignal;
		Debug.Log("_trimpAndClamp_OnEndOfFrameSignal == " + _trimpAndClamp_OnEndOfFrameSignal);
	}

	[ContextMenu("Toggle_OnEnable")]
	private void Toggle_OnEnable()
	{
		_trimpAndClamp_OnEnable = !_trimpAndClamp_OnEnable;
		Debug.Log("_trimpAndClamp_OnEnable == " + _trimpAndClamp_OnEnable);
	}

	[ContextMenu("Toggle_Update")]
	private void Toggle_Update()
	{
		_trimpAndClamp_Update = !_trimpAndClamp_Update;
		Debug.Log("_trimpAndClamp_Update == " + _trimpAndClamp_Update);
	}

	[ContextMenu("Toggle_LateUpdate")]
	private void Toggle_LateUpdate()
	{
		_trimpAndClamp_LateUpdate = !_trimpAndClamp_LateUpdate;
		Debug.Log("_trimpAndClamp_LateUpdate == " + _trimpAndClamp_LateUpdate);
	}

	private void Connect()
	{
		if (_connected)
		{
			return;
		}
		_connected = true;
		PersistentCoroutineRunner.UpdateSignal.Connect(delegate(LayoutElementWidgetComponent self)
		{
			if (_trimpAndClamp_UpdateSignal)
			{
				self.TrimAndClamp("UpdateSignal");
			}
		}, this).DisconnectWhen(_onDisconnect);
		PersistentCoroutineRunner.LateUpdateSignal.Connect(delegate(LayoutElementWidgetComponent self)
		{
			if (_trimpAndClamp_LateUpdateSignal)
			{
				self.TrimAndClamp("LateUpdateSignal");
			}
		}, this).DisconnectWhen(_onDisconnect);
		PersistentCoroutineRunner.OnEndOfFrameSignal.Connect(delegate(LayoutElementWidgetComponent self)
		{
			if (_trimpAndClamp_OnEndOfFrameSignal)
			{
				self.TrimAndClamp("OnEndOfFrameSignal");
			}
		}, this).DisconnectWhen(_onDisconnect);
	}

	private void Disconnect()
	{
		if (_connected && !_isPosTracking && !_isRotTracking && !_isTrimming && !_isClamping)
		{
			_connected = false;
			_onDisconnect.Invoke();
		}
	}

	private void TrimAndClamp(string info = "")
	{
		if (_isTrimming || _isClamping)
		{
			OnPosTrack();
			OnTrim();
			OnClamp(info);
			OnRotTrack();
		}
	}

	public virtual void OnDestroy()
	{
		EndPosTracking();
		EndRotTracking();
		EndTrimming();
		EndClamping();
	}

	private void OnEnable()
	{
		if (_trimpAndClamp_OnEnable && _isClamping)
		{
			TrimAndClamp("OnEnable");
		}
	}

	private void Update()
	{
		if (_trimpAndClamp_Update && _isClamping)
		{
			TrimAndClamp("Update");
		}
	}

	private void LateUpdate()
	{
		if (_trimpAndClamp_LateUpdate && (_isTrimming || _isClamping))
		{
			TrimAndClamp("LateUpdate");
		}
	}
}
