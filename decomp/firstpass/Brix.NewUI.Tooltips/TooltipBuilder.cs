using System.Collections.Generic;
using Brix.Lua;
using Brix.Utils;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Brix.NewUI.Tooltips;

public class TooltipBuilder : MonoBehaviour
{
	public enum TooltipTag
	{
		ContextualHelp,
		Tutorial,
		Dialog,
		OtherUnique,
		OtherMultiple
	}

	[SerializeField]
	private GameObject cloneableTooltip;

	private Tooltip tooltip;

	private Dictionary<TooltipTag, bool> typeIsUnique = new Dictionary<TooltipTag, bool>(FastEnumIntEqualityComparer<TooltipTag>.Instance)
	{
		{
			TooltipTag.ContextualHelp,
			true
		},
		{
			TooltipTag.Tutorial,
			false
		},
		{
			TooltipTag.Dialog,
			false
		},
		{
			TooltipTag.OtherUnique,
			true
		},
		{
			TooltipTag.OtherMultiple,
			false
		}
	};

	private Dictionary<TooltipTag, GameObject> tooltips = new Dictionary<TooltipTag, GameObject>(FastEnumIntEqualityComparer<TooltipTag>.Instance);

	private static TooltipBuilder _instance;

	public static TooltipBuilder Instance => _instance;

	private void Start()
	{
		_instance = this;
	}

	public TooltipBuilder SetParent(Transform parent)
	{
		tooltip.transform.SetParent(parent);
		return this;
	}

	public TooltipBuilder NewTooltip(TooltipTag type = TooltipTag.ContextualHelp)
	{
		if (ShouldDestroy(type))
		{
			Object.Destroy(tooltips[type].gameObject);
		}
		tooltip = Object.Instantiate(cloneableTooltip).GetComponent<Tooltip>();
		tooltip.transform.SetParent(base.gameObject.transform);
		tooltip.name = "Tooltip";
		tooltip.gameObject.SetActive(value: true);
		tooltip.Clear();
		tooltips[type] = tooltip.gameObject;
		return this;
	}

	private bool ShouldDestroy(TooltipTag type)
	{
		bool flag = tooltips.ContainsKey(type) && tooltips[type] != null;
		bool flag2 = !typeIsUnique.ContainsKey(type) || typeIsUnique[type];
		return flag && flag2;
	}

	public TooltipBuilder WithMainImage(Sprite img)
	{
		tooltip.SetTitleImage(img);
		return this;
	}

	public TooltipBuilder WithTitle(string text)
	{
		tooltip.SetTitle(text);
		return this;
	}

	public TooltipBuilder WithSmallImage(Sprite img)
	{
		tooltip.SetSubImage(img);
		return this;
	}

	public TooltipBuilder Pointing2D(RectTransform rect)
	{
		tooltip.SetTarget2D(rect);
		tooltip.WithArrow();
		return this;
	}

	public TooltipBuilder Pointing3D(GameObject target)
	{
		tooltip.SetTarget3D(target);
		return this;
	}

	public TooltipBuilder From(TooltipPositioner.Direction direction)
	{
		tooltip.SetPositionPriority(direction);
		return this;
	}

	public TooltipBuilder SetFollowCursor(bool value)
	{
		tooltip.SetFollowCursor(value);
		return this;
	}

	public TooltipBuilder OfSize(Tooltip.Width width)
	{
		tooltip.SetWidth(width);
		return this;
	}

	public TooltipBuilder Dynamically()
	{
		tooltip.SetFollowDynamic(value: true);
		return this;
	}

	public TooltipBuilder WithOffset2D(Vector3 offset)
	{
		tooltip.SetPositionOffset2D(offset);
		return this;
	}

	public TooltipBuilder WithOffset3D(Vector3 offset)
	{
		tooltip.SetPositionOffset3D(offset);
		return this;
	}

	public Tooltip Make()
	{
		return tooltip;
	}

	public void DestroyTooltip()
	{
		Object.Destroy(tooltip.gameObject);
	}

	[LuaDrivable("NewBuilder")]
	private static DynValue lua_NewTooltip(ScriptExecutionContext context, CallbackArguments args)
	{
		TooltipBuilder tooltipBuilder = args.AsComponent<TooltipBuilder>(0, "NewBuilder");
		return DynValue.FromObject(context.OwnerScript, tooltipBuilder.NewTooltip(TooltipTag.Tutorial));
	}

	[LuaDrivable("OfSize")]
	private static DynValue lua_OfSize(ScriptExecutionContext context, CallbackArguments args)
	{
		TooltipBuilder tooltipBuilder = args.AsComponent<TooltipBuilder>(0, "OfSize");
		switch (args.AsInt(1, "OfSize"))
		{
		case 0:
			tooltipBuilder.OfSize(Tooltip.Width.large);
			break;
		case 1:
			tooltipBuilder.OfSize(Tooltip.Width.medium);
			break;
		case 2:
			tooltipBuilder.OfSize(Tooltip.Width.narrow);
			break;
		default:
			tooltipBuilder.OfSize(Tooltip.Width.large);
			break;
		}
		return DynValue.FromObject(context.OwnerScript, tooltipBuilder);
	}

	[LuaDrivable("TitleImage")]
	private static DynValue lua_WithMainImage(ScriptExecutionContext context, CallbackArguments args)
	{
		TooltipBuilder tooltipBuilder = args.AsComponent<TooltipBuilder>(0, "TitleImage");
		Sprite img = args.AsUserData<Sprite>(1, "TitleImage");
		tooltipBuilder.WithMainImage(img);
		return DynValue.FromObject(context.OwnerScript, tooltipBuilder);
	}

	[LuaDrivable("Title")]
	private static DynValue lua_WithTitle(ScriptExecutionContext context, CallbackArguments args)
	{
		TooltipBuilder tooltipBuilder = args.AsComponent<TooltipBuilder>(0, "Title");
		string text = args.AsString(1, "Title");
		tooltipBuilder.WithTitle(text);
		return DynValue.FromObject(context.OwnerScript, tooltipBuilder);
	}

	[LuaDrivable("Image")]
	private static DynValue lua_SetSubImage(ScriptExecutionContext context, CallbackArguments args)
	{
		TooltipBuilder tooltipBuilder = args.AsComponent<TooltipBuilder>(0, "Image");
		Sprite img = args.AsUserData<Sprite>(1, "Image");
		tooltipBuilder.WithSmallImage(img);
		return DynValue.FromObject(context.OwnerScript, tooltipBuilder);
	}

	[LuaDrivable("Text")]
	private static DynValue lua_WithText(ScriptExecutionContext context, CallbackArguments args)
	{
		TooltipBuilder tooltipBuilder = args.AsComponent<TooltipBuilder>(0, "Text");
		string mainText = args.AsString(1, "Text");
		TextAnchor alignement = ((args.Count < 3) ? TextAnchor.MiddleLeft : args.AsUserData<TextAnchor>(2, "Text"));
		tooltipBuilder.tooltip.SetMainText(mainText);
		tooltipBuilder.tooltip.TextAlignement(alignement);
		return DynValue.FromObject(context.OwnerScript, tooltipBuilder);
	}

	[LuaDrivable("Target")]
	private static DynValue lua_Target(ScriptExecutionContext context, CallbackArguments args)
	{
		TooltipBuilder tooltipBuilder = args.AsComponent<TooltipBuilder>(0, "Target");
		GameObject target3D = args.AsGameObject(1, "Target");
		tooltipBuilder.tooltip.SetTarget3D(target3D);
		return DynValue.FromObject(context.OwnerScript, tooltipBuilder);
	}

	[LuaDrivable("AsDynamic")]
	private static DynValue lua_Dynamically(ScriptExecutionContext context, CallbackArguments args)
	{
		TooltipBuilder tooltipBuilder = args.AsComponent<TooltipBuilder>(0, "AsDynamic");
		tooltipBuilder.Dynamically();
		return DynValue.FromObject(context.OwnerScript, tooltipBuilder);
	}

	[LuaDrivable("Offset")]
	private static DynValue lua_WithOffset3D(ScriptExecutionContext context, CallbackArguments args)
	{
		TooltipBuilder tooltipBuilder = args.AsComponent<TooltipBuilder>(0, "Offset");
		Vector3 offset = args.AsUserData<Vector3>(1, "Offset");
		tooltipBuilder.WithOffset3D(offset);
		return DynValue.FromObject(context.OwnerScript, tooltipBuilder);
	}

	[LuaDrivable("Make")]
	private static DynValue lua_Make(ScriptExecutionContext context, CallbackArguments args)
	{
		TooltipBuilder tooltipBuilder = args.AsComponent<TooltipBuilder>(0, "Make");
		return DynValue.FromObject(context.OwnerScript, tooltipBuilder.Make());
	}

	[LuaDrivable("DestroyTooltip")]
	private static DynValue lua_DestroyTooltip(ScriptExecutionContext context, CallbackArguments args)
	{
		TooltipBuilder tooltipBuilder = args.AsComponent<TooltipBuilder>(0, "DestroyTooltip");
		tooltipBuilder.tooltip.SelfDestruct();
		return DynValue.Void;
	}

	[LuaDrivable("ShowTooltip")]
	private static DynValue lua_ShowTooltip(ScriptExecutionContext context, CallbackArguments args)
	{
		TooltipBuilder tooltipBuilder = args.AsComponent<TooltipBuilder>(0, "ShowTooltip");
		tooltipBuilder.tooltip.Show();
		return DynValue.Void;
	}

	[LuaDrivable("HideTooltip")]
	private static DynValue lua_HideTooltip(ScriptExecutionContext context, CallbackArguments args)
	{
		TooltipBuilder tooltipBuilder = args.AsComponent<TooltipBuilder>(0, "HideTooltip");
		tooltipBuilder.tooltip.Hide();
		return DynValue.Void;
	}

	[LuaDrivable("ClearTooltip")]
	private static DynValue lua_ClearTooltip(ScriptExecutionContext context, CallbackArguments args)
	{
		TooltipBuilder tooltipBuilder = args.AsComponent<TooltipBuilder>(0, "ClearTooltip");
		tooltipBuilder.tooltip.Clear();
		return DynValue.Void;
	}

	[LuaDrivable("EnableNext")]
	private static DynValue lua_EnableNext(ScriptExecutionContext context, CallbackArguments args)
	{
		TooltipBuilder tooltipBuilder = args.AsComponent<TooltipBuilder>(0, "EnableNext");
		tooltipBuilder.tooltip.EnableNextButton();
		return DynValue.Void;
	}

	[LuaDrivable("DisableNext")]
	private static DynValue lua_DisableNext(ScriptExecutionContext context, CallbackArguments args)
	{
		TooltipBuilder tooltipBuilder = args.AsComponent<TooltipBuilder>(0, "DisableNext");
		tooltipBuilder.tooltip.DisableNextButton();
		return DynValue.Void;
	}

	[LuaDrivable("GetTooltip")]
	private static DynValue lua_GetTooltip(ScriptExecutionContext context, CallbackArguments args)
	{
		TooltipBuilder tooltipBuilder = args.AsComponent<TooltipBuilder>(0, "GetTooltip");
		return DynValue.FromObject(context.OwnerScript, tooltipBuilder.tooltip);
	}
}
