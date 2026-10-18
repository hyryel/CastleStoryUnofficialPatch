using Brix.External.Factories;
using Brix.Lua;
using Brix.UI.Builder.Widget;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Panel;

public class VerticalLayoutPanelComponent : BaseLayoutPanelComponent
{
	public RectTransform _bin;

	public VerticalLayoutGroup _layoutGroup;

	public Image _image;

	public LayoutElement _layoutElement;

	public ContentSizeFitter _fitter;

	public ContentSizeFitterPlus _fitterPlus;

	public VerticalLayoutGroupWidgetComponent _layoutGroupWidget;

	public LayoutElementWidgetComponent _layoutElementWidget;

	public ImageWidgetComponent _imageWidget;

	[LuaDrivable("layoutGroup")]
	private LuiLayoutGroupWidget LuiLayoutGroupWidget => _layoutGroupWidget.LuiLayoutGroupWidget;

	[LuaDrivable("layoutElement")]
	private LuiLayoutElementWidget LuiLayoutElementWidget => _layoutElementWidget.LuiLayoutElementWidget;

	[LuaDrivable("CreateB")]
	public static DynValue lua_Create(ScriptExecutionContext sec, CallbackArguments ca)
	{
		GameObject gameObject = Factory.Instantiate(Layouts.Vertical);
		if (gameObject == null)
		{
			return DynValue.Nil;
		}
		LuaDriver component = gameObject.GetComponent<LuaDriver>();
		if (component == null)
		{
			return DynValue.Nil;
		}
		return DynValue.FromObject(sec.OwnerScript, component);
	}

	[LuaDrivable("DestroyB")]
	public static DynValue lua_Destroy(ScriptExecutionContext sec, CallbackArguments ca)
	{
		GameObject obj = ca.AsGameObject(0, "DestroyB");
		Object.Destroy(obj);
		return DynValue.Void;
	}

	[LuaDrivable("SetParentB")]
	public static DynValue lua_SetParent(ScriptExecutionContext sec, CallbackArguments ca)
	{
		Transform transform = ca.AsComponent<Transform>(0, "SetParentB");
		Transform parent = ca.AsComponent<Transform>(1, "SetParentB", allowNil: true);
		transform.SetParent(parent);
		return DynValue.Void;
	}
}
