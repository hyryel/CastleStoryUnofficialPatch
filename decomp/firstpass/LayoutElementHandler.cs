using Brix.Lua;
using MoonSharp.Interpreter;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class LayoutElementHandler : MonoBehaviour
{
	private RectTransform rectTrans;

	private void Awake()
	{
		rectTrans = GetComponent<RectTransform>();
	}

	[LuaDrivable("AddElement")]
	private static DynValue Add(ScriptExecutionContext context, CallbackArguments args)
	{
		LayoutElementHandler layoutElementHandler = args.AsComponent<LayoutElementHandler>(0, "AddElement");
		RectTransform rectTransform = args.AsComponent<RectTransform>(1, "AddElement");
		rectTransform.SetParent(layoutElementHandler.rectTrans);
		return DynValue.Void;
	}

	[LuaDrivable("RemoveElement")]
	private static DynValue Remove(ScriptExecutionContext context, CallbackArguments args)
	{
		RectTransform rectTransform = args.AsComponent<RectTransform>(1, "AddElement");
		rectTransform.SetParent(null);
		return DynValue.Void;
	}
}
