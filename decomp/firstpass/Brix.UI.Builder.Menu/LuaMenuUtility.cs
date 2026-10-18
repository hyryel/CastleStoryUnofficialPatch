using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.Game.AI;
using Brix.Input;
using Brix.Lua;
using Brix.UI.Builder.Lui;
using Brix.UI.Builder.Widget;
using Brix.Utils;
using Brix.Utils.Logging;
using Brix.Utils.UI;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Menu;

public class LuaMenuUtility
{
	public class BuilderWrapper
	{
		public object wrap;

		public T As<T>()
		{
			return (T)wrap;
		}
	}

	private static BrixLogger logger = new BrixLogger(typeof(LuaMenuUtility).FullName);

	public static bool _drawUnits;

	public static int _unitSize;

	public static int _natureSize;

	public static Texture2D _mapTex;

	public static int _unitMapSize = 256;

	public static Texture2D _unitTex;

	public static Color32[] _unitInitPixels;

	public static Color32[] _unitPixels;

	public static int _natureMapSize = 256;

	public static Texture2D _natureTex;

	public static Color32[] _natureInitPixels;

	public static Color32[] _naturePixels;

	public static Sprite _mapSprite;

	public static Rect _mapUvRect;

	public static Vector2 _mapUvRectPosOffset;

	public static Vector2 _mapUvRectSizeFactor;

	public static Vector2 _mapSpritePivot;

	public static Color _treeColor;

	public static Color _grassColor;

	public static Color _earthColor;

	public static Color _bricktronColor;

	public static Color _enemyBricktronColor;

	public static Color _enemyObjectColor;

	public static Color _alliedBricktronColor;

	public static Color _crystalColor;

	private static Table lua_colors = new Table(null, new Dictionary<string, DynValue>
	{
		{
			"CastleYellow",
			LuaColor.castleYellow
		},
		{
			"Yellow",
			LuaColor.yellow
		},
		{
			"Gray",
			LuaColor.gray
		},
		{
			"DarkGray",
			LuaColor.darkGray
		},
		{
			"White",
			LuaColor.white
		}
	});

	private static Table lua_enums = new Table(null, new Dictionary<string, DynValue>
	{
		{
			"Alignment",
			new LuaEnumDescriptor<TextAnchor>().CreateDynValue()
		},
		{
			"FrameHighlight",
			new LuaEnumDescriptor<UniversalButtonInterface.HiglightsModes>().CreateDynValue()
		},
		{
			"FitMode",
			new LuaEnumDescriptor<ContentSizeFitter.FitMode>().CreateDynValue()
		},
		{
			"FillMethod",
			new LuaEnumDescriptor<Image.FillMethod>().CreateDynValue()
		},
		{
			"MenuFlag",
			new LuaEnumDescriptor<BaseMenu.BuilderFlag>().CreateDynValue()
		},
		{
			"MenuState",
			new LuaEnumDescriptor<BaseMenu.MenuState>().CreateDynValue()
		},
		{
			"OccupationType",
			new LuaEnumDescriptor<Occupation.Type>().CreateDynValue()
		},
		{
			"TrimWhen",
			new LuaEnumDescriptor<LayoutElementWidgetComponent.TrimWhen>().CreateDynValue()
		},
		{
			"ClampWhen",
			new LuaEnumDescriptor<LayoutElementWidgetComponent.ClampWhen>().CreateDynValue()
		},
		{
			"ClampTo",
			new LuaEnumDescriptor<LayoutElementWidgetComponent.ClampTo>().CreateDynValue()
		},
		{
			"Brush",
			new LuaEnumDescriptor<WeTerrain.Brush>().CreateDynValue()
		},
		{
			"SubBrush",
			new LuaEnumDescriptor<WeTerrain.SubBrush>().CreateDynValue()
		},
		{
			"Tool",
			new LuaEnumDescriptor<WeMarkers.Tool>().CreateDynValue()
		},
		{
			"SubTool",
			new LuaEnumDescriptor<WeMarkers.SubTool>().CreateDynValue()
		},
		{
			"MarkerType",
			new LuaEnumDescriptor<UIMarkersInterface.SpawnerObject>().CreateDynValue()
		},
		{
			"ContentType",
			new LuaEnumDescriptor<InputField.ContentType>().CreateDynValue()
		},
		{
			"LineType",
			new LuaEnumDescriptor<InputField.LineType>().CreateDynValue()
		},
		{
			"UndersideLevelSource",
			new LuaEnumDescriptor<TerrainUndersideMenu.LevelSource>().CreateDynValue()
		},
		{
			"UndersideAlignment",
			new LuaEnumDescriptor<TerrainUndersideMenu.Alignment>().CreateDynValue()
		},
		{
			"SteamItemVisibility",
			new LuaEnumDescriptor<ERemoteStoragePublishedFileVisibility>().CreateDynValue()
		},
		{
			"KeyBindingsCategory",
			new LuaEnumDescriptor<KeyBindingsUtility.Category>().CreateDynValue()
		},
		{
			"Font",
			new LuaEnumDescriptor<LabelWidgetComponent.FontEnum>().CreateDynValue()
		}
	});

	private static Table lua_loc = new Table(null, new Dictionary<string, DynValue> { 
	{
		"GetLocalized",
		DynValue.NewCallback((ScriptExecutionContext con, CallbackArguments args) => DynValue.NewString(I2Helper.TryGet(args.AsString(0, "GetLocalized"))))
	} });

	public static LuaObjectDescriptor<BuilderWrapper> builderWrapperDescriptor = new LuaObjectDescriptor<BuilderWrapper>();

	private static LuaObjectDescriptor<LuaMenuUtility> _desc = new LuaObjectDescriptor<LuaMenuUtility>();

	private static readonly We.KeyBindingCategoryUtil CameraKeyBindings = new We.KeyBindingCategoryUtil("Camera");

	private static readonly We.KeyBindingCategoryUtil GeneralKeyBindings = new We.KeyBindingCategoryUtil("General");

	private static readonly We.KeyBindingCategoryUtil WeMenuKeyBindings = new We.KeyBindingCategoryUtil("we_menu");

	private static readonly We.KeyBindingCategoryUtil WeTerrainMenuKeyBindings = new We.KeyBindingCategoryUtil("we_terrain_menu");

	private static readonly We.KeyBindingCategoryUtil WeMarkersMenuKeyBindings = new We.KeyBindingCategoryUtil("we_markers_menu");

	private static readonly We.KeyBindingCategoryUtil DebugBindings = new We.KeyBindingCategoryUtil("debug");

	private static readonly We.KeyBindingCategoryUtil TaskMenuBindings = new We.KeyBindingCategoryUtil("TaskMenu");

	private static readonly We.KeyBindingCategoryUtil BlocksMenuBindings = new We.KeyBindingCategoryUtil("BlocksMenu");

	private static readonly We.KeyBindingCategoryUtil StructuresMenuBindings = new We.KeyBindingCategoryUtil("StructuresMenu");

	private static readonly We.KeyBindingCategoryUtil BlueprintsBindings = new We.KeyBindingCategoryUtil("Blueprints");

	private static readonly We.KeyBindingCategoryUtil CraftingBindings = new We.KeyBindingCategoryUtil("Crafting");

	public const string StrName = "Name";

	public const string StrBin = "Bin";

	public const string StrMenu = "Menu";

	public const string StrPanel = "Panel";

	public const string StrPanels = "Panels";

	public const string StrLayoutPanel = "LayoutPanel";

	public const string StrLayoutPanels = "LayoutPanels";

	public const string StrSlidables = "Slidables";

	public const string StrButtons = "Buttons";

	public const string StrWidgets = "Widgets";

	public const string StrHandles = "Handles";

	public const string StrClosure = "Closure";

	public const string StrInstance = "Instance";

	private LuaMenuUtility()
	{
	}

	public static void TryInitUnitTex()
	{
		if (!(_unitTex != null) || _unitMapSize != _unitTex.width)
		{
			Color32 color = new Color32(0, 0, 0, 0);
			Texture2D texture2D = new Texture2D(_unitMapSize, _unitMapSize, TextureFormat.RGBA32, mipmap: false);
			texture2D.filterMode = FilterMode.Point;
			texture2D.wrapMode = TextureWrapMode.Clamp;
			_unitTex = texture2D;
			_unitInitPixels = _unitTex.GetPixels32();
			for (int i = 0; i < _unitInitPixels.Length; i++)
			{
				_unitInitPixels[i] = color;
			}
			_unitPixels = _unitTex.GetPixels32();
		}
	}

	public static void TryInitNatureTex()
	{
		if (!(_natureTex != null) || _unitMapSize != _natureTex.width)
		{
			Color32 color = new Color32(0, 0, 0, 0);
			Texture2D texture2D = new Texture2D(_unitMapSize, _unitMapSize, TextureFormat.RGBA32, mipmap: false);
			texture2D.filterMode = FilterMode.Point;
			texture2D.wrapMode = TextureWrapMode.Clamp;
			_natureTex = texture2D;
			_natureInitPixels = _natureTex.GetPixels32();
			for (int i = 0; i < _natureInitPixels.Length; i++)
			{
				_natureInitPixels[i] = color;
			}
			_naturePixels = _natureTex.GetPixels32();
		}
	}

	public static void RegisterClass(Script script)
	{
		Table callbackTable = _desc.GetCallbackTable(script);
		callbackTable.Set("Colors", DynValue.NewTable(lua_colors));
		callbackTable.Set("Enums", DynValue.NewTable(lua_enums));
		callbackTable.Set("Funcs", DynValue.NewTable(lua_loc));
		callbackTable.Set("Builder", DynValue.NewTable(GetTBuilder(script)));
		script.Globals.Set("LUI2", DynValue.NewTable(callbackTable));
	}

	private static Table GetTBuilder(Script script)
	{
		Table table = new Table(script);
		table["Menu"] = new LuiBuilder.MenuLuiBuilder(script, "Menu").T;
		string[] array = new string[5] { "Vertical", "Horizontal", "Grid", "Scrollable", "SquareButton" };
		Table table2 = (Table)(table["LayoutPanels"] = new Table(script));
		string[] array2 = array;
		foreach (string text in array2)
		{
			string typeName = "LayoutPanels." + text;
			table2[text] = new LuiBuilder.LayoutPanelLuiBuilder(script, typeName).T;
		}
		Table table4 = (Table)(table2["Slidables"] = new Table(script));
		string[] array3 = new string[4] { "Top", "Bottom", "Left", "Right" };
		string[] array4 = array3;
		foreach (string text2 in array4)
		{
			string typeName2 = "LayoutPanels.Slidables." + text2;
			table4[text2] = new LuiBuilder.LayoutPanelLuiBuilder(script, typeName2).T;
		}
		string[] array5 = new string[9] { "Label", "Image", "RawImage", "Slider", "Dropdown", "InputField", "Compass", "Media", "Minimap" };
		Table table6 = (Table)(table["Panels"] = new Table(script));
		string[] array6 = array5;
		foreach (string text3 in array6)
		{
			string typeName3 = "Panels." + text3;
			table6[text3] = new LuiBuilder.PanelLuiBuilder(script, typeName3).T;
		}
		Table table8 = (Table)(table6["Buttons"] = new Table(script));
		string[] array7 = new string[1] { "Standard" };
		string[] array8 = array7;
		foreach (string text4 in array8)
		{
			string typeName4 = "Panels.Buttons." + text4;
			table8[text4] = new LuiBuilder.PanelLuiBuilder(script, typeName4).T;
		}
		string[] array9 = new string[32]
		{
			"LayoutGroup", "LayoutElement", "LabelLayoutElement", "ValueLayoutElement", "SlidableLayoutElement", "SlidableImage", "Label", "Button", "Toggle", "StateAnimator",
			"FrameAnimator", "Slider", "Slidable", "Image", "ImageLabel", "ArrowImage", "ValueCheckImage", "ValueImage", "ValueLabel", "ValueTemplateImage",
			"ValueTemplateLabel", "Dropdown", "ValueTemplateBgImage", "ListBgImage", "InputField", "PlaceholderLabel", "RawImage", "Hotkey", "CompassLayoutElement", "HoverBgImage",
			"HoverImage", "HoverLabel"
		};
		Table table10 = (Table)(table["Widgets"] = new Table(script));
		string[] array10 = array9;
		foreach (string text5 in array10)
		{
			table10[text5] = new LuiBuilder.WidgetLuiBuilder(script, text5).T;
		}
		string[] array11 = new string[104]
		{
			"CellSize", "Spacing", "SpacingXy", "Padding", "Sprite", "Color", "ControlColor", "ControlSprite", "LocalRotation", "WholeNumbers",
			"NumberValue", "ValidatedNumberValue", "SetNumberValue", "MinNumberValue", "MaxNumberValue", "NumberValueFromTextValue", "TextValueFromNumberValue", "PreserveAspect", "Raycasts", "MinWidth",
			"MinHeight", "PreferredWidth", "PreferredHeight", "FlexibleWidth", "FlexibleHeight", "AutoMinWidth", "AutoMinHeight", "AutoPreferredWidth", "AutoPreferredHeight", "Alignment",
			"LineSpacing", "IgnoreLayout", "AnchorMin", "AnchorMax", "AnchoredWidth", "AnchoredHeight", "OffsetMin", "OffsetMax", "AnchoredPosition", "AnchoredPositionTracking",
			"AnchoredPositionClamping", "AnchoredPositionTrimming", "HpTracking", "StaminaTracking", "CaptureTracking", "OrientationTracking", "AutoFitPlus", "AutoFit", "AutoFitHorizontal", "AutoFitVertical",
			"AutoFitOnce", "AutoFitForcesLayoutUpdate", "Pivot", "OnAction", "OnDoubleAction", "OnAltAction", "OnAltDoubleAction", "Count", "IconAsCount", "Text",
			"Enabled", "Discreet", "CanHover", "CanPress", "Highlight", "Visible", "ChildForceExpandWidth", "ChildForceExpandHeight", "OnPointerEnter", "OnPointerExit",
			"OnPointerUp", "OnPointerDown", "Fill", "FillMethod", "Font", "FontSize", "TrimsChildren", "OnEndShowing", "OnEndHiding", "OnShow",
			"OnHide", "Selection", "SetSelection", "Options", "TextValue", "SetTextValue", "OnSelect", "OnDeselect", "DoSelect", "CharacterLimit",
			"ContentType", "Interactable", "UvRect", "Texture", "LineType", "Outlines", "CompassTarget", "SiblingIndex", "SpriteSize", "OnCtrlAction",
			"OnShiftAction", "OnCtrlDoubleAction", "OnShiftDoubleAction", "HideFirstFrame"
		};
		Table table12 = (Table)(table["Handles"] = new Table(script));
		string[] array12 = array11;
		foreach (string text6 in array12)
		{
			table12[text6] = new LuiBuilder.HandleLuiBuilder(script, text6).T;
		}
		return table;
	}

	[LuaDrivable("GetMapSprite")]
	private static DynValue lua_GetMapSprite(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector2 mapUvRectSizeFactor = args.AsUserData<Vector2>(0, "GetMapSprite");
		Vector2 mapUvRectPosOffset = args.AsUserData<Vector2>(1, "GetMapSprite");
		try
		{
			_mapTex = Minimap.GetTerrainTypeMinimap(_mapTex);
		}
		catch (Exception ex)
		{
			_mapTex = null;
			_mapSprite = null;
			logger.Info("Non-initialized world:\n" + ex);
			return DynValue.FromObject(context.OwnerScript, _mapSprite);
		}
		_mapTex.filterMode = FilterMode.Point;
		_mapTex.Apply();
		Rect rect = new Rect(0f, 0f, 1f, 1f);
		Bounds bounds = default(Bounds);
		try
		{
			bounds = Voxel.Engine.ComputeOccupiedBounds();
		}
		catch (Exception ex2)
		{
			_mapSprite = null;
			logger.Info("Non-initialized world:\n" + ex2);
			return DynValue.FromObject(context.OwnerScript, _mapSprite);
		}
		float num = 1f;
		float num2 = 0f;
		float num3 = 0f;
		float x = bounds.size.x;
		float z = bounds.size.z;
		if (x != 0f || z != 0f)
		{
			float num4 = ((!(x > z)) ? z : x);
			num4 += 4f;
			num = num4 / 1024f;
			bounds.size = new Vector3(num4, bounds.size.y, num4);
			num2 = bounds.min.x;
			num3 = bounds.min.z;
			num2 /= 1024f;
			num3 /= 1024f;
			num2 *= 1024f;
			num3 *= 1024f;
			num *= 1024f;
			rect = (_mapUvRect = new Rect(num2, num3, num, num));
		}
		Vector2 vector = new Vector2(0.5f, 0.5f);
		_mapUvRectPosOffset = mapUvRectPosOffset;
		_mapUvRectSizeFactor = mapUvRectSizeFactor;
		_mapSpritePivot = vector;
		rect.width *= mapUvRectSizeFactor.x;
		rect.height *= mapUvRectSizeFactor.y;
		rect.x += mapUvRectPosOffset.x * rect.width;
		rect.y += mapUvRectPosOffset.y * rect.height;
		_mapSprite = Sprite.Create(_mapTex, rect, vector);
		return DynValue.FromObject(context.OwnerScript, _mapSprite);
	}

	[LuaDrivable("SetMapColors")]
	private static DynValue lua_SetMapColors(ScriptExecutionContext context, CallbackArguments args)
	{
		Table table = args.AsTable(0, "SetMapColors");
		_grassColor = table.Get("grass").ToObject<Color>();
		_earthColor = table.Get("earth").ToObject<Color>();
		_treeColor = table.Get("tree").ToObject<Color>();
		_bricktronColor = table.Get("bricktron").ToObject<Color>();
		_crystalColor = table.Get("crystal").ToObject<Color>();
		_alliedBricktronColor = table.Get("alliedBricktron").ToObject<Color>();
		_enemyBricktronColor = table.Get("enemyBricktron").ToObject<Color>();
		_enemyObjectColor = table.Get("enemyObject").ToObject<Color>();
		return DynValue.Void;
	}

	[LuaDrivable("GetMapTexture")]
	private static DynValue lua_GetMapTexture(ScriptExecutionContext context, CallbackArguments args)
	{
		bool flag = false;
		try
		{
			Color grassColor = Minimap.grassColor;
			Color earthColor = Minimap.earthColor;
			Minimap.grassColor = _grassColor;
			Minimap.earthColor = _earthColor;
			_mapTex = Minimap.GetTerrainTypeMinimap(_mapTex);
			Minimap.grassColor = grassColor;
			Minimap.earthColor = earthColor;
			flag = true;
		}
		catch (Exception ex)
		{
			_mapTex = null;
			logger.Info("Non-initialized world:\n" + ex);
		}
		if (flag && _mapTex != null)
		{
			_mapTex.filterMode = FilterMode.Point;
		}
		return DynValue.FromObject(context.OwnerScript, _mapTex);
	}

	[LuaDrivable("GetMapTextureUvs")]
	private static DynValue lua_GetMapTextureUvs(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector2 vector = args.AsUserData<Vector2>(0, "GetMapTextureUvs");
		Vector2 vector2 = args.AsUserData<Vector2>(1, "GetMapTextureUvs");
		Rect rect = new Rect(0f, 0f, 1f, 1f);
		Bounds bounds = default(Bounds);
		try
		{
			bounds = Voxel.Engine.ComputeOccupiedBounds();
		}
		catch (Exception ex)
		{
			logger.Info("Non-initialized world:\n" + ex);
		}
		bounds.size = new Vector3(bounds.size.x + 2f, bounds.size.y + 2f, bounds.size.z + 2f);
		float x = bounds.size.x;
		float z = bounds.size.z;
		if (x != 0f || z != 0f)
		{
			float num = ((!(x > z)) ? z : x);
			bounds.size = new Vector3(num, bounds.size.y, num);
			float x2 = bounds.min.x;
			float z2 = bounds.min.z;
			float x3 = x2 / 1024f;
			float y = z2 / 1024f;
			float num2 = num / 1024f;
			rect = new Rect(x3, y, num2, num2);
			rect.width *= vector2.x;
			rect.height *= vector2.y;
			rect.x += vector.x;
			rect.y += vector.y;
		}
		return DynValue.FromObject(context.OwnerScript, new Vector4(rect.x, rect.y, rect.width, rect.height));
	}

	[LuaDrivable("SetKeyBindings")]
	private static DynValue lua_SetAllKeyBindingsTo(ScriptExecutionContext context, CallbackArguments args)
	{
		bool allKeyBindingsTo = args.AsBool(0, "SetKeyBindings");
		SetAllKeyBindingsTo(allKeyBindingsTo);
		return DynValue.Void;
	}

	public static void SetAllKeyBindingsTo(bool flag)
	{
		CameraKeyBindings.Set(flag);
		GeneralKeyBindings.Set(flag);
		WeMenuKeyBindings.Set(flag);
		WeTerrainMenuKeyBindings.Set(flag);
		WeMarkersMenuKeyBindings.Set(flag);
		DebugBindings.Set(flag);
	}

	[LuaDrivable("ResetKeyBindingsTo")]
	private static DynValue lua_ResetAllKeyBindingsTo(ScriptExecutionContext context, CallbackArguments args)
	{
		bool flag = args.AsBool(0, "ResetKeyBindingsTo");
		ResetAllKeyBindingsTo(flag);
		return DynValue.Void;
	}

	public static void ResetAllKeyBindingsTo(bool flag)
	{
		CameraKeyBindings.Reset(flag);
		GeneralKeyBindings.Reset(flag);
		WeMenuKeyBindings.Reset(flag);
		WeTerrainMenuKeyBindings.Reset(flag);
		WeMarkersMenuKeyBindings.Reset(flag);
		DebugBindings.Reset(flag);
	}

	[LuaDrivable("ResetKeyBindings")]
	private static DynValue lua_ResetAllKeyBindings(ScriptExecutionContext context, CallbackArguments args)
	{
		ResetAllKeyBindings();
		return DynValue.Void;
	}

	public static void ResetAllKeyBindings()
	{
		CameraKeyBindings.Reset();
		GeneralKeyBindings.Reset();
		WeMenuKeyBindings.Reset();
		WeTerrainMenuKeyBindings.Reset();
		WeMarkersMenuKeyBindings.Reset();
		DebugBindings.Reset();
	}

	public static void SetKeyBindingsTo(KeyBindingsUtility.Category categoryName, bool flag)
	{
		switch (categoryName)
		{
		case KeyBindingsUtility.Category.Camera:
			CameraKeyBindings.Set(flag);
			break;
		case KeyBindingsUtility.Category.General:
			GeneralKeyBindings.Set(flag);
			break;
		case KeyBindingsUtility.Category.WorldEditorMenu:
			WeMenuKeyBindings.Set(flag);
			break;
		case KeyBindingsUtility.Category.WorldEditorTerrainMenu:
			WeTerrainMenuKeyBindings.Set(flag);
			break;
		case KeyBindingsUtility.Category.WorldEditorMarkerMenu:
			WeMarkersMenuKeyBindings.Set(flag);
			break;
		case KeyBindingsUtility.Category.Debug:
			DebugBindings.Set(flag);
			break;
		case KeyBindingsUtility.Category.TaskMenu:
			TaskMenuBindings.Set(flag);
			break;
		case KeyBindingsUtility.Category.BlocksMenu:
			BlocksMenuBindings.Set(flag);
			break;
		case KeyBindingsUtility.Category.StructuresMenu:
			StructuresMenuBindings.Set(flag);
			break;
		case KeyBindingsUtility.Category.Blueprints:
			BlueprintsBindings.Set(flag);
			break;
		case KeyBindingsUtility.Category.Crafting:
			CraftingBindings.Set(flag);
			break;
		case KeyBindingsUtility.Category.Construction:
		case KeyBindingsUtility.Category.Projects:
			break;
		}
	}

	public static void ResetKeyBindingsTo(KeyBindingsUtility.Category categoryName, bool flag)
	{
		switch (categoryName)
		{
		case KeyBindingsUtility.Category.Camera:
			CameraKeyBindings.Reset(flag);
			break;
		case KeyBindingsUtility.Category.General:
			GeneralKeyBindings.Reset(flag);
			break;
		case KeyBindingsUtility.Category.WorldEditorMenu:
			WeMenuKeyBindings.Reset(flag);
			break;
		case KeyBindingsUtility.Category.WorldEditorTerrainMenu:
			WeTerrainMenuKeyBindings.Reset(flag);
			break;
		case KeyBindingsUtility.Category.WorldEditorMarkerMenu:
			WeMarkersMenuKeyBindings.Reset(flag);
			break;
		case KeyBindingsUtility.Category.Debug:
			DebugBindings.Reset(flag);
			break;
		case KeyBindingsUtility.Category.TaskMenu:
			TaskMenuBindings.Reset(flag);
			break;
		case KeyBindingsUtility.Category.BlocksMenu:
			BlocksMenuBindings.Reset(flag);
			break;
		case KeyBindingsUtility.Category.StructuresMenu:
			StructuresMenuBindings.Reset(flag);
			break;
		case KeyBindingsUtility.Category.Blueprints:
			BlueprintsBindings.Reset(flag);
			break;
		case KeyBindingsUtility.Category.Crafting:
			CraftingBindings.Reset(flag);
			break;
		case KeyBindingsUtility.Category.Construction:
		case KeyBindingsUtility.Category.Projects:
			break;
		}
	}

	public static void ResetKeyBindings(KeyBindingsUtility.Category categoryName)
	{
		switch (categoryName)
		{
		case KeyBindingsUtility.Category.Camera:
			CameraKeyBindings.Reset();
			break;
		case KeyBindingsUtility.Category.General:
			GeneralKeyBindings.Reset();
			break;
		case KeyBindingsUtility.Category.WorldEditorMenu:
			WeMenuKeyBindings.Reset();
			break;
		case KeyBindingsUtility.Category.WorldEditorTerrainMenu:
			WeTerrainMenuKeyBindings.Reset();
			break;
		case KeyBindingsUtility.Category.WorldEditorMarkerMenu:
			WeMarkersMenuKeyBindings.Reset();
			break;
		case KeyBindingsUtility.Category.Debug:
			DebugBindings.Reset();
			break;
		case KeyBindingsUtility.Category.TaskMenu:
			TaskMenuBindings.Reset();
			break;
		case KeyBindingsUtility.Category.BlocksMenu:
			BlocksMenuBindings.Reset();
			break;
		case KeyBindingsUtility.Category.StructuresMenu:
			StructuresMenuBindings.Reset();
			break;
		case KeyBindingsUtility.Category.Blueprints:
			BlueprintsBindings.Reset();
			break;
		case KeyBindingsUtility.Category.Crafting:
			CraftingBindings.Reset();
			break;
		case KeyBindingsUtility.Category.Construction:
		case KeyBindingsUtility.Category.Projects:
			break;
		}
	}

	[LuaDrivable("SetHandle")]
	public static DynValue SetHandle(ScriptExecutionContext context, CallbackArguments args)
	{
		long entryId = args.AsLong(0, "SetHandle");
		Table table = args.AsTable(1, "SetHandle");
		Closure function = args.AsType(2, "SetHandle", DataType.Function).Function;
		if (!LuaIdUtility.TryGetEntryFromEntryId<LuaIdUtility.WidgetEntry>(entryId, out var typedEntry))
		{
			throw new Exception($"Matching widget id could not be found. \"{entryId.ToString()}\"");
		}
		typedEntry.Widget.Lua_SetHandle(table.Get("Name").ToString(), function, DynValue.False);
		return DynValue.Void;
	}

	[LuaDrivable("ClearHandles")]
	public static DynValue ClearHandles(ScriptExecutionContext context, CallbackArguments args)
	{
		long entryId = args.AsLong(0, "ClearHandles");
		Table table = args.AsTable(1, "ClearHandles");
		string[] array = new string[table.Length];
		int num = 0;
		foreach (TablePair pair in table.Pairs)
		{
			array[num] = pair.Value.String;
			num++;
		}
		if (!LuaIdUtility.TryGetEntryFromEntryId<LuaIdUtility.WidgetEntry>(entryId, out var typedEntry))
		{
			throw new Exception($"Matching widget id could not be found. \"{entryId.ToString()}\"");
		}
		typedEntry.Widget.Lua_ClearHandles(array);
		return DynValue.Void;
	}

	[LuaDrivable("ClearAllHandles")]
	public static DynValue ClearAllHandles(ScriptExecutionContext context, CallbackArguments args)
	{
		long entryId = args.AsLong(0, "ClearAllHandles");
		if (!LuaIdUtility.TryGetEntryFromEntryId<LuaIdUtility.WidgetEntry>(entryId, out var typedEntry))
		{
			throw new Exception($"Matching widget id could not be found. \"{entryId.ToString()}\"");
		}
		typedEntry.Widget.Lua_ClearAllHandles();
		return DynValue.Void;
	}

	[LuaDrivable("Close")]
	public static DynValue Close(ScriptExecutionContext context, CallbackArguments args)
	{
		long entryId = args.AsLong(0, "Close");
		if (!LuaIdUtility.TryGetEntryFromEntryId<LuaIdUtility.MenuEntry>(entryId, out var typedEntry))
		{
			throw new Exception($"Matching menu id could not be found. \"{entryId.ToString()}\"");
		}
		typedEntry.Menu.Close();
		return DynValue.Void;
	}

	[LuaDrivable("Open")]
	public static DynValue Open(ScriptExecutionContext context, CallbackArguments args)
	{
		long entryId = args.AsLong(0, "Close");
		if (!LuaIdUtility.TryGetEntryFromEntryId<LuaIdUtility.MenuEntry>(entryId, out var typedEntry))
		{
			throw new Exception($"Matching menu id could not be found. \"{entryId.ToString()}\"");
		}
		typedEntry.Menu.Open();
		return DynValue.Void;
	}

	[LuaDrivable("ReOpen")]
	public static DynValue ReOpen(ScriptExecutionContext context, CallbackArguments args)
	{
		long entryId = args.AsLong(0, "ReOpen");
		if (!LuaIdUtility.TryGetEntryFromEntryId<LuaIdUtility.MenuEntry>(entryId, out var typedEntry))
		{
			throw new Exception($"Matching menu id could not be found. \"{entryId.ToString()}\"");
		}
		typedEntry.Menu.Reopen();
		return DynValue.Void;
	}

	[LuaDrivable("Refresh")]
	public static DynValue Refresh(ScriptExecutionContext context, CallbackArguments args)
	{
		long entryId = args.AsLong(0, "Refresh");
		if (!LuaIdUtility.TryGetEntryFromEntryId<LuaIdUtility.MenuEntry>(entryId, out var typedEntry))
		{
			throw new Exception($"Matching menu id could not be found. \"{entryId.ToString()}\"");
		}
		typedEntry.Menu.Refresh();
		return DynValue.Void;
	}

	[LuaDrivable("Build")]
	public static DynValue Build(ScriptExecutionContext context, CallbackArguments args)
	{
		long entryId = args.AsLong(0, "Build");
		if (!LuaIdUtility.TryGetEntryFromEntryId<LuaIdUtility.MenuEntry>(entryId, out var typedEntry))
		{
			throw new Exception($"Matching menu id could not be found. \"{entryId.ToString()}\"");
		}
		typedEntry.Menu.Load();
		return DynValue.Void;
	}
}
