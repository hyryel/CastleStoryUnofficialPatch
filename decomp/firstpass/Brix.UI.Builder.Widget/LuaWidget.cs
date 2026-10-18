using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Brix.Lua;
using Brix.UI.Builder.Menu;
using Brix.Utils.Logging;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Widget;

public class LuaWidget : BaseWidget, IGetVisible, IGetEnabled, IGetPreserveAspect, IGetCanHover, IGetCanPress, IGetDiscreet, IGetInteractable, IGetSprite, IGetColor, IGetControlColor, IGetControlSprite, IGetRaycasts, IGetText, IGetFont, IGetFontSize, IGetAlignment, IGetLineSpacing, IGetIgnoreLayout, IGetAnchorMin, IGetAnchorMax, IGetOffsetMin, IGetOffsetMax, IGetAnchoredWidth, IGetAnchoredHeight, IGetPivot, IGetAnchoredPosition, IGetAnchoredPositionTracking, IGetAnchoredPositionTrimming, IGetAnchoredPositionClamping, IGetHpTracking, IGetStaminaTracking, IGetCaptureTracking, IGetOrientationTracking, IGetTrimsChildren, IGetClampsChildren, IGetAutoFitPlus, IGetAutoFit, IGetAutoFitHorizontal, IGetAutoFitVertical, IGetAutoFitOnce, IGetAutoFitForcesLayoutUpdate, IGetAutoMinWidth, IGetAutoMinHeight, IGetAutoPreferredWidth, IGetAutoPreferredHeight, IGetMinWidth, IGetMinHeight, IGetPreferredWidth, IGetPreferredHeight, IGetFlexibleWidth, IGetFlexibleHeight, IGetCellSize, IGetSpacing, IGetSpacingXy, IGetPadding, IGetFill, IGetFillMethod, IGetChildForceExpandWidth, IGetChildForceExpandHeight, IOnAction, IOnDoubleAction, IOnAltAction, IOnAltDoubleAction, IGetCount, IGetIconAsCount, IOnPointerEnter, IOnPointerExit, IOnPointerUp, IOnPointerDown, IGetHighlight, IGetOnEndShowing, IGetOnEndHiding, IGetLocalRotation, IGetWholeNumbers, IGetMinNumberValue, IGetMaxNumberValue, IGetNumberValue, ISetNumberValue, IGetValidatedNumberValue, IGetTextValueFromNumberValue, IGetNumberValueFromTextValue, IOnHide, IOnShow, IGetSelection, ISetSelection, IGetOptions, IGetTextValue, ISetTextValue, IGetDoSelect, IOnSelect, IOnDeselect, IGetCharacterLimit, IGetContentType, IGetUvRect, IGetTexture, IGetLineType, IGetOutlines, IGetCompassTarget, IGetSiblingIndex, IGetSpriteSize, IOnCtrlAction, IOnShiftAction, IOnCtrlDoubleAction, IOnShiftDoubleAction, IGetHideFirstFrame, IWidgetHandle
{
	private static class Utility<TReturn>
	{
		public static Func<TReturn> Default()
		{
			return () => default(TReturn);
		}
	}

	private static readonly Dictionary<string, Type> HandleNameTypeMap;

	private readonly Dictionary<Type, Closure> _h = new Dictionary<Type, Closure>();

	private readonly Dictionary<Type, DynValue> _hOnce = new Dictionary<Type, DynValue>();

	private BrixLogger logger = new BrixLogger(typeof(LuaWidget).FullName);

	private Func<Vector3> LocalRotationGetter;

	private Func<bool> CanHoverGetter;

	private Func<bool> PreserveAspectGetter;

	private Func<bool> CanPressGetter;

	private Func<bool> DiscreetGetter;

	private Func<bool> VisibleGetter;

	private Func<bool> HideFirstFrameGetter;

	private Func<bool> EnabledGetter;

	private Func<bool> InteractableGetter;

	private Func<string> HighlightGetter;

	private Func<Closure> OnEndShowingGetter;

	private Func<Closure> OnEndHidingGetter;

	private Func<Color> ColorGetter;

	private Func<string> TextGetter;

	private Func<Sprite> SpriteGetter;

	private Func<Color> ControlColorGetter;

	private Func<Sprite> ControlSpriteGetter;

	private Func<LabelWidgetComponent.FontEnum> FontGetter;

	private Func<int> FontSizeGetter;

	private Func<TextAnchor> AlignmentGetter;

	private Func<bool> IgnoreLayoutGetter;

	private Func<Vector2> AnchorMinGetter;

	private Func<Vector2> AnchorMaxGetter;

	private Func<Vector2> OffsetMinGetter;

	private Func<Vector2> OffsetMaxGetter;

	private Func<float> AnchoredWidthGetter;

	private Func<float> LineSpacingGetter;

	private Func<float> AnchoredHeightGetter;

	private Func<Vector2> PivotGetter;

	private Func<Vector2> AnchoredPositionGetter;

	private Func<float> MinWidthGetter;

	private Func<float> MinHeightGetter;

	private Func<float> PreferredWidthGetter;

	private Func<float> PreferredHeightGetter;

	private Func<float> FlexibleWidthGetter;

	private Func<float> FlexibleHeightGetter;

	private Func<bool> ChildForceExpandWidthGetter;

	private Func<bool> ChildForceExpandHeightGetter;

	private Func<ContentSizeFitter.FitMode> AutoFitHorizontalGetter;

	private Func<ContentSizeFitter.FitMode> AutoFitVerticalGetter;

	static LuaWidget()
	{
		HandleNameTypeMap = new Dictionary<string, Type>();
		InitHandleTypeNameMap();
	}

	private static void InitHandleTypeNameMap()
	{
		Type[] array = (from p in AppDomain.CurrentDomain.GetAssemblies().SelectMany((Assembly s) => s.GetTypes())
			where typeof(IWidgetHandle).IsAssignableFrom(p)
			select p).ToArray();
		foreach (Type type in array)
		{
			string name = type.Name;
			name = name.TrimStart('I');
			name = name.Replace("Get", string.Empty);
			if (!HandleNameTypeMap.ContainsKey(name))
			{
				HandleNameTypeMap.Add(name, type);
			}
		}
	}

	public override bool Is<TIOverride>()
	{
		return base.Is<TIOverride>() && _h.ContainsKey(typeof(TIOverride));
	}

	public override void Lua_SetHandle(string handleKey, Closure closure, DynValue dOnce)
	{
		if (!HandleNameTypeMap.TryGetValue(handleKey, out var value))
		{
			throw new Exception("Unidentified handle type name \"" + handleKey + "\"\n");
		}
		if (!_h.ContainsKey(value))
		{
			_h.Add(value, closure);
		}
		else
		{
			_h[value] = closure;
		}
		if (dOnce.IsNotNil() && dOnce.Type == DataType.Boolean && dOnce.Boolean)
		{
			if (!_hOnce.ContainsKey(value))
			{
				try
				{
					_hOnce.Add(value, closure.Call());
				}
				catch (Exception e)
				{
					LuaLibs.LuaExceptionCatcher(e);
				}
			}
		}
		else if (_hOnce.ContainsKey(value))
		{
			_hOnce.Remove(value);
		}
	}

	public override void Lua_ClearHandle(string handleKey)
	{
	}

	public override void Lua_ClearHandles(string[] handleKeys)
	{
	}

	public override void Lua_ClearAllHandles()
	{
	}

	private DynValue TryCall<THandle>() where THandle : IWidgetHandle
	{
		Type typeFromHandle = typeof(THandle);
		try
		{
			if (_hOnce.ContainsKey(typeFromHandle))
			{
				return _hOnce[typeFromHandle];
			}
			try
			{
				return _h[typeFromHandle].Call();
			}
			catch (Exception e)
			{
				LuaLibs.LuaExceptionCatcher(e);
			}
		}
		catch (Exception e2)
		{
			logger.ErrorFormat(e2, "Error at handle:\n", typeof(THandle));
		}
		return DynValue.Nil;
	}

	private Func<TReturn> GetCaller<THandle, TReturn>() where THandle : IWidgetHandle
	{
		Type typeFromHandle = typeof(THandle);
		if (_hOnce.ContainsKey(typeFromHandle))
		{
			try
			{
				TReturn value = _hOnce[typeFromHandle].ToObject<TReturn>();
				return () => value;
			}
			catch (Exception e)
			{
				LuaLibs.LuaExceptionCatcher(e);
			}
		}
		if (_h.ContainsKey(typeFromHandle))
		{
			Closure closure = _h[typeFromHandle];
			string typeName = typeFromHandle.Name;
			return () => TryCall(closure, typeName).ToObject<TReturn>();
		}
		return Utility<TReturn>.Default();
	}

	private Func<TReturn> GetEnumCaller<THandle, TReturn>() where THandle : IWidgetHandle where TReturn : struct, IComparable, IFormattable, IConvertible
	{
		Type typeFromHandle = typeof(THandle);
		if (_hOnce.ContainsKey(typeFromHandle))
		{
			try
			{
				TReturn value = _hOnce[typeFromHandle].ToEnum<TReturn>();
				return () => value;
			}
			catch (Exception e)
			{
				LuaLibs.LuaExceptionCatcher(e);
			}
		}
		if (_h.ContainsKey(typeFromHandle))
		{
			Closure closure = _h[typeFromHandle];
			string typeName = typeFromHandle.Name;
			return () => TryCall(closure, typeName).ToEnum<TReturn>();
		}
		return Utility<TReturn>.Default();
	}

	private Func<bool> GetBoolCaller<THandle>() where THandle : IWidgetHandle
	{
		Type typeFromHandle = typeof(THandle);
		if (_hOnce.ContainsKey(typeFromHandle))
		{
			try
			{
				bool value = _hOnce[typeFromHandle].Boolean;
				return () => value;
			}
			catch (Exception e)
			{
				LuaLibs.LuaExceptionCatcher(e);
			}
		}
		if (_h.ContainsKey(typeFromHandle))
		{
			Closure closure = _h[typeFromHandle];
			string typeName = typeFromHandle.Name;
			return () => TryCall(closure, typeName).Boolean;
		}
		return Utility<bool>.Default();
	}

	private Func<float> GetFloatCaller<THandle>() where THandle : IWidgetHandle
	{
		Type typeFromHandle = typeof(THandle);
		if (_hOnce.ContainsKey(typeFromHandle))
		{
			try
			{
				float value = (float)_hOnce[typeFromHandle].Number;
				return () => value;
			}
			catch (Exception e)
			{
				LuaLibs.LuaExceptionCatcher(e);
			}
		}
		if (_h.ContainsKey(typeFromHandle))
		{
			Closure closure = _h[typeFromHandle];
			string typeName = typeFromHandle.Name;
			return () => (float)TryCall(closure, typeName).Number;
		}
		return Utility<float>.Default();
	}

	private Func<int> GetIntCaller<THandle>() where THandle : IWidgetHandle
	{
		Type typeFromHandle = typeof(THandle);
		if (_hOnce.ContainsKey(typeFromHandle))
		{
			try
			{
				int value = (int)_hOnce[typeFromHandle].Number;
				return () => value;
			}
			catch (Exception e)
			{
				LuaLibs.LuaExceptionCatcher(e);
			}
		}
		if (_h.ContainsKey(typeFromHandle))
		{
			Closure closure = _h[typeFromHandle];
			string typeName = typeFromHandle.Name;
			return () => (int)TryCall(closure, typeName).Number;
		}
		return Utility<int>.Default();
	}

	private DynValue TryCall(Closure closure, string typeName)
	{
		DynValue result = DynValue.Nil;
		try
		{
			result = closure.Call();
		}
		catch (Exception e)
		{
			LuaLibs.LuaExceptionCatcher(e);
		}
		return result;
	}

	public override void Lua_Init()
	{
		LocalRotationGetter = GetCaller<IGetLocalRotation, Vector3>();
		CanHoverGetter = GetBoolCaller<IGetCanHover>();
		PreserveAspectGetter = GetBoolCaller<IGetPreserveAspect>();
		CanPressGetter = GetBoolCaller<IGetCanPress>();
		DiscreetGetter = GetBoolCaller<IGetDiscreet>();
		VisibleGetter = GetBoolCaller<IGetVisible>();
		HideFirstFrameGetter = GetBoolCaller<IGetHideFirstFrame>();
		EnabledGetter = GetBoolCaller<IGetEnabled>();
		InteractableGetter = GetBoolCaller<IGetInteractable>();
		HighlightGetter = GetCaller<IGetHighlight, string>();
		OnEndShowingGetter = GetCaller<IGetOnEndShowing, Closure>();
		OnEndHidingGetter = GetCaller<IGetOnEndHiding, Closure>();
		ColorGetter = GetCaller<IGetColor, Color>();
		TextGetter = GetCaller<IGetText, string>();
		SpriteGetter = GetCaller<IGetSprite, Sprite>();
		ControlColorGetter = GetCaller<IGetControlColor, Color>();
		ControlSpriteGetter = GetCaller<IGetControlSprite, Sprite>();
		FontGetter = GetEnumCaller<IGetFont, LabelWidgetComponent.FontEnum>();
		FontSizeGetter = GetIntCaller<IGetFontSize>();
		AlignmentGetter = GetEnumCaller<IGetAlignment, TextAnchor>();
		IgnoreLayoutGetter = GetBoolCaller<IGetIgnoreLayout>();
		AnchorMinGetter = GetCaller<IGetAnchorMin, Vector2>();
		AnchorMaxGetter = GetCaller<IGetAnchorMax, Vector2>();
		OffsetMinGetter = GetCaller<IGetOffsetMin, Vector2>();
		OffsetMaxGetter = GetCaller<IGetOffsetMax, Vector2>();
		AnchoredWidthGetter = GetFloatCaller<IGetAnchoredWidth>();
		AnchoredHeightGetter = GetFloatCaller<IGetAnchoredHeight>();
		LineSpacingGetter = GetFloatCaller<IGetLineSpacing>();
		PivotGetter = GetCaller<IGetPivot, Vector2>();
		AnchoredPositionGetter = GetCaller<IGetAnchoredPosition, Vector2>();
		MinWidthGetter = GetFloatCaller<IGetMinWidth>();
		MinHeightGetter = GetFloatCaller<IGetMinHeight>();
		PreferredWidthGetter = GetFloatCaller<IGetPreferredWidth>();
		PreferredHeightGetter = GetFloatCaller<IGetPreferredHeight>();
		FlexibleWidthGetter = GetFloatCaller<IGetFlexibleWidth>();
		FlexibleHeightGetter = GetFloatCaller<IGetFlexibleHeight>();
		ChildForceExpandWidthGetter = GetBoolCaller<IGetChildForceExpandWidth>();
		ChildForceExpandHeightGetter = GetBoolCaller<IGetChildForceExpandHeight>();
		AutoFitHorizontalGetter = GetEnumCaller<IGetAutoFitHorizontal, ContentSizeFitter.FitMode>();
		AutoFitVerticalGetter = GetEnumCaller<IGetAutoFitVertical, ContentSizeFitter.FitMode>();
	}

	public string GetText()
	{
		return TextGetter();
	}

	public Sprite GetSprite()
	{
		return SpriteGetter();
	}

	public Color GetControlColor()
	{
		return ControlColorGetter();
	}

	public Sprite GetControlSprite()
	{
		return ControlSpriteGetter();
	}

	public LabelWidgetComponent.FontEnum GetFont()
	{
		return FontGetter();
	}

	public int GetFontSize()
	{
		return FontSizeGetter();
	}

	public TextAnchor GetAlignment()
	{
		return AlignmentGetter();
	}

	public float GetLineSpacing()
	{
		return LineSpacingGetter();
	}

	public bool GetIgnoreLayout()
	{
		return IgnoreLayoutGetter();
	}

	public Vector2 GetAnchorMin()
	{
		return AnchorMinGetter();
	}

	public Vector2 GetAnchorMax()
	{
		return AnchorMaxGetter();
	}

	public Vector2 GetOffsetMin()
	{
		return OffsetMinGetter();
	}

	public Vector2 GetOffsetMax()
	{
		return OffsetMaxGetter();
	}

	public float GetAnchoredWidth()
	{
		return AnchoredWidthGetter();
	}

	public float GetAnchoredHeight()
	{
		return AnchoredHeightGetter();
	}

	public Vector2 GetPivot()
	{
		return PivotGetter();
	}

	public Vector2 GetAnchoredPosition()
	{
		return AnchoredPositionGetter();
	}

	public float GetMinWidth()
	{
		return MinWidthGetter();
	}

	public float GetMinHeight()
	{
		return MinHeightGetter();
	}

	public float GetPreferredWidth()
	{
		return PreferredWidthGetter();
	}

	public float GetPreferredHeight()
	{
		return PreferredHeightGetter();
	}

	public float GetFlexibleWidth()
	{
		return FlexibleWidthGetter();
	}

	public float GetFlexibleHeight()
	{
		return FlexibleHeightGetter();
	}

	public bool GetChildForceExpandWidth()
	{
		return ChildForceExpandWidthGetter();
	}

	public bool GetChildForceExpandHeight()
	{
		return ChildForceExpandHeightGetter();
	}

	public ContentSizeFitter.FitMode GetAutoFitHorizontal()
	{
		return AutoFitHorizontalGetter();
	}

	public ContentSizeFitter.FitMode GetAutoFitVertical()
	{
		return AutoFitVerticalGetter();
	}

	public void OnPointerEnter()
	{
		TryCall<IOnPointerEnter>();
	}

	public void OnPointerExit()
	{
		TryCall<IOnPointerExit>();
	}

	public void OnPointerUp()
	{
		TryCall<IOnPointerUp>();
	}

	public void OnPointerDown()
	{
		TryCall<IOnPointerDown>();
	}

	public Vector3 GetLocalRotation()
	{
		return LocalRotationGetter();
	}

	public bool GetCanHover()
	{
		return CanHoverGetter();
	}

	public bool GetPreserveAspect()
	{
		return PreserveAspectGetter();
	}

	public bool GetCanPress()
	{
		return CanPressGetter();
	}

	public bool GetDiscreet()
	{
		return DiscreetGetter();
	}

	public bool GetVisible()
	{
		return VisibleGetter();
	}

	public bool GetHideFirstFrame()
	{
		return HideFirstFrameGetter();
	}

	public bool GetEnabled()
	{
		return EnabledGetter();
	}

	public bool GetInteractable()
	{
		return InteractableGetter();
	}

	public string GetHighlight()
	{
		return HighlightGetter();
	}

	public Closure GetOnEndShowing()
	{
		return OnEndShowingGetter();
	}

	public Closure GetOnEndHiding()
	{
		return OnEndHidingGetter();
	}

	public Color GetColor()
	{
		return ColorGetter();
	}

	public bool GetRaycasts()
	{
		return TryCall<IGetRaycasts>().Boolean;
	}

	public Table GetAnchoredPositionClamping()
	{
		return TryCall<IGetAnchoredPositionClamping>().Table;
	}

	public Table GetAnchoredPositionTrimming()
	{
		return TryCall<IGetAnchoredPositionTrimming>().Table;
	}

	public Table GetOrientationTracking()
	{
		return TryCall<IGetOrientationTracking>().Table;
	}

	public Table GetAnchoredPositionTracking()
	{
		return TryCall<IGetAnchoredPositionTracking>().Table;
	}

	public Table GetHpTracking()
	{
		return TryCall<IGetHpTracking>().Table;
	}

	public Table GetStaminaTracking()
	{
		return TryCall<IGetStaminaTracking>().Table;
	}

	public Table GetCaptureTracking()
	{
		return TryCall<IGetCaptureTracking>().Table;
	}

	public Table GetTrimsChildren()
	{
		return TryCall<IGetTrimsChildren>().Table;
	}

	public Table GetClampsChildren()
	{
		return TryCall<IGetClampsChildren>().Table;
	}

	public Table GetAutoFitPlus()
	{
		return TryCall<IGetAutoFitPlus>().Table;
	}

	public bool GetAutoFit()
	{
		return TryCall<IGetAutoFit>().Boolean;
	}

	public bool GetAutoFitOnce()
	{
		return TryCall<IGetAutoFitOnce>().Boolean;
	}

	public bool GetAutoFitForcesLayoutUpdate()
	{
		return TryCall<IGetAutoFitForcesLayoutUpdate>().Boolean;
	}

	public bool GetAutoMinWidth()
	{
		return TryCall<IGetAutoMinWidth>().Boolean;
	}

	public bool GetAutoMinHeight()
	{
		return TryCall<IGetAutoMinHeight>().Boolean;
	}

	public bool GetAutoPreferredWidth()
	{
		return TryCall<IGetAutoPreferredWidth>().Boolean;
	}

	public bool GetAutoPreferredHeight()
	{
		return TryCall<IGetAutoPreferredHeight>().Boolean;
	}

	public Vector2 GetCellSize()
	{
		return TryCall<IGetCellSize>().ToObject<Vector2>();
	}

	public float GetSpacing()
	{
		return (float)TryCall<IGetSpacing>().Number;
	}

	public Vector2 GetSpacingXy()
	{
		return TryCall<IGetSpacingXy>().ToObject<Vector2>();
	}

	public RectOffset GetPadding()
	{
		Vector4 vector = TryCall<IGetPadding>().ToObject<Vector4>();
		return new RectOffset(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y), Mathf.RoundToInt(vector.z), Mathf.RoundToInt(vector.w));
	}

	public float GetFill()
	{
		return (float)TryCall<IGetFill>().Number;
	}

	public Image.FillMethod GetFillMethod()
	{
		return TryCall<IGetFillMethod>().ToEnum<Image.FillMethod>();
	}

	public void OnCtrlAction()
	{
		TryCall<IOnCtrlAction>();
	}

	public void OnShiftAction()
	{
		TryCall<IOnShiftAction>();
	}

	public void OnCtrlDoubleAction()
	{
		TryCall<IOnCtrlDoubleAction>();
	}

	public void OnShiftDoubleAction()
	{
		TryCall<IOnShiftDoubleAction>();
	}

	public void OnAction()
	{
		TryCall<IOnAction>();
	}

	public void OnDoubleAction()
	{
		TryCall<IOnDoubleAction>();
	}

	public void OnAltAction()
	{
		TryCall<IOnAltAction>();
	}

	public void OnAltDoubleAction()
	{
		TryCall<IOnAltDoubleAction>();
	}

	public string GetCount()
	{
		return TryCall<IGetCount>().String;
	}

	public bool GetIconAsCount()
	{
		return TryCall<IGetIconAsCount>().Boolean;
	}

	public void OnRectChange(Vector2 pos, Vector4 lclRect)
	{
		_h[typeof(IOnRectChange)].Call(pos, lclRect);
	}

	public bool GetWholeNumbers()
	{
		return TryCall<IGetWholeNumbers>().Boolean;
	}

	public float GetMinNumberValue()
	{
		return (float)TryCall<IGetMinNumberValue>().Number;
	}

	public float GetMaxNumberValue()
	{
		return (float)TryCall<IGetMaxNumberValue>().Number;
	}

	public float GetNumberValue()
	{
		return (float)TryCall<IGetNumberValue>().Number;
	}

	public void SetNumberValue(float value)
	{
		_h[typeof(ISetNumberValue)].Call(DynValue.NewNumber(value));
	}

	public float GetValidatedNumberValue(float value)
	{
		return (float)_h[typeof(IGetValidatedNumberValue)].Call(DynValue.NewNumber(value)).Number;
	}

	public string GetTextValueFromNumberValue(float value)
	{
		return _h[typeof(IGetTextValueFromNumberValue)].Call(DynValue.NewNumber(value)).String;
	}

	public float GetNumberValueFromTextValue(string textValue)
	{
		return (float)_h[typeof(IGetNumberValueFromTextValue)].Call(DynValue.NewString(textValue)).Number;
	}

	public void OnShow()
	{
		TryCall<IOnShow>();
	}

	public void OnHide()
	{
		TryCall<IOnHide>();
	}

	public int GetSelection()
	{
		return Mathf.RoundToInt((float)TryCall<IGetSelection>().Number);
	}

	public void SetSelection(int i)
	{
		_h[typeof(ISetSelection)].Call(DynValue.NewNumber(i));
	}

	public DynValue GetOptions()
	{
		return TryCall<IGetOptions>();
	}

	public string GetTextValue()
	{
		return TryCall<IGetTextValue>().String;
	}

	public void SetTextValue(string textValue)
	{
		_h[typeof(ISetTextValue)].Call(DynValue.NewString(textValue));
	}

	public bool GetDoSelect()
	{
		return TryCall<IGetDoSelect>().Boolean;
	}

	public void OnSelect()
	{
		TryCall<IOnSelect>();
	}

	public void OnDeselect()
	{
		TryCall<IOnDeselect>();
	}

	public int GetCharacterLimit()
	{
		return (int)TryCall<IGetCharacterLimit>().Number;
	}

	public InputField.ContentType GetContentType()
	{
		return TryCall<IGetContentType>().ToEnum<InputField.ContentType>();
	}

	public Vector4 GetUvRect()
	{
		return TryCall<IGetUvRect>().ToObject<Vector4>();
	}

	public Texture2D GetTexture()
	{
		return TryCall<IGetTexture>().ToObject<Texture2D>();
	}

	public InputField.LineType GetLineType()
	{
		return TryCall<IGetLineType>().ToEnum<InputField.LineType>();
	}

	public DynValue GetOutlines()
	{
		return TryCall<IGetOutlines>();
	}

	public LuaDriver GetCompassTarget()
	{
		return TryCall<IGetCompassTarget>().ToObject<LuaDriver>();
	}

	public int GetSiblingIndex()
	{
		DynValue dynValue = TryCall<IGetSiblingIndex>();
		return (int)((!object.Equals(dynValue, DynValue.Nil)) ? dynValue.Number : (-1.0));
	}

	public float GetSpriteSize()
	{
		return (float)TryCall<IGetSpriteSize>().Number;
	}
}
