using System.Collections.Generic;
using Brix.Assets;
using Brix.Engine;
using Brix.External.Factories;
using Brix.Game.Components;
using Brix.Game.Components.Validators.Graphs;
using Brix.Game.Components.Volumes;
using Brix.Input;
using Brix.Lifecycle.Pooling;
using Brix.Lua;
using Brix.UI;
using Brix.UI.GameSelector;
using Brix.Utils;
using Brix.Utils.Collections;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Smooth.Slinq;
using Steamworks;
using UnityEngine;

namespace Brix.Game.Compounders;

public class BlueprintCompounder
{
	public enum PlacementMode
	{
		none,
		move,
		duplicate
	}

	private static Dictionary<string, Sprite> thumbnailDictionary = new Dictionary<string, Sprite>();

	private static Dictionary<string, Asset_Blueprints> blueprintsDictionary = new Dictionary<string, Asset_Blueprints>();

	private static Sprite currentThumbnail = null;

	private static string currentName = "BlueprintName";

	private static string currentSelected = string.Empty;

	private static bool instantiatingCompound = false;

	private static Asset_Blueprints _current;

	public static PlacementMode CompoundPlacementMode;

	private static LuaObjectDescriptor<BlueprintCompounder> _desc = new LuaObjectDescriptor<BlueprintCompounder>();

	private static LuaEnumDescriptor<PlacementMode> _enumPlacementMode = new LuaEnumDescriptor<PlacementMode>();

	[LuaDrivable("PlacementMode")]
	private static DynValue lua_PlacementMode = _enumPlacementMode.CreateDynValue();

	[LuaDrivable("hk_OnDoneInstantiating")]
	public static LuaHook lua_OnDoneInstantiating = new LuaHook();

	[LuaDrivable("hk_CompoundSelectionChanged")]
	private static LuaHook lua_CompoundSelectionChanged = new LuaHook();

	[LuaDrivable("hk_OnCompoundDeleted")]
	private static LuaHook lua_OnCompoundDeleted = new LuaHook();

	public static Asset_Blueprints current
	{
		get
		{
			return _current;
		}
		set
		{
			_current = value;
			if (_current == null)
			{
				Picking.Instance.ClearPick();
			}
		}
	}

	private BlueprintCompounder()
	{
		InitializeCompoundDictionary();
	}

	public static void Move()
	{
		BeginCompoundBlueprintAction(PlacementMode.move);
	}

	public static void Duplicate()
	{
		BeginCompoundBlueprintAction(PlacementMode.duplicate);
	}

	public static void Expand()
	{
		UIGameSelectorUtilities.SelectAllConnectedVolumes(SelectionType.Blueprint, VProperty.fantome);
	}

	public static void Cancel()
	{
		InputModeController.SelectBlueprints();
		current = null;
		UIGameSelector.UnselectAll(SelectionType.Blueprint);
	}

	public static void CompoundBlueprints()
	{
		CollectionPool.Request<GameObject>(out List<GameObject> list);
		UIGameSelector.GetSelectedInTrack(ref list, SelectionTrackID.Blueprints);
		BlueprintGraph blueprintGraph = GenerateBlueprintGraph(list);
		list.Dispose();
		if (blueprintGraph.Nodes.Count == 0)
		{
			current = null;
			return;
		}
		Asset_Blueprints asset_Blueprints = Asset_Blueprints.CreateShell();
		asset_Blueprints.customName = "MyBlueprintName";
		asset_Blueprints.rawDescription = "My Description";
		asset_Blueprints.Graph = blueprintGraph;
		current = asset_Blueprints;
	}

	public static BlueprintGraph GenerateBlueprintGraph(List<GameObject> goList)
	{
		HashSet<GameObject> hashSet = goList.Slinq().ToPooledHashSet();
		BlueprintGraph blueprintGraph = new BlueprintGraph();
		blueprintGraph.AddNodes(hashSet);
		hashSet.Dispose();
		return blueprintGraph;
	}

	public static BlueprintCompoundComponent LoadCompoundBlueprints(Asset_Blueprints compound, bool fromFile)
	{
		BlueprintGraph graph = compound.Graph;
		GameObject gameObject = ObjectPoolSingleton.Request_Unsafe(Brix.External.Factories.Compounders.BlueprintCompound, active: false);
		BlueprintCompoundComponent component = gameObject.GetComponent<BlueprintCompoundComponent>();
		component.Populate(graph, User.LocalUser.faction);
		component.DestinationProject = UIGameObserver.projects.CurrentSelected;
		component.NeedsGraphRebuild = fromFile;
		Picking.Instance.SendPicker(Pickers.CompoundPlacement, null, new CompoundPlacementOptions(gameObject));
		return component;
	}

	public static void BeginCompoundBlueprintAction(PlacementMode mode)
	{
		CollectionPool.Request<GameObject>(out List<GameObject> list);
		UIGameSelector.GetSelectedInTrack(ref list, SelectionTrackID.Blueprints);
		bool flag = list.Slinq().Any();
		list.Dispose();
		if (!flag)
		{
			return;
		}
		CompoundPlacementMode = mode;
		CompoundBlueprints();
		if (current != null)
		{
			LoadCompoundBlueprints(current, fromFile: false);
			if (mode == PlacementMode.move)
			{
				UIGameObserver.projects.CurrentSelected.AutoDeleteDisabled = true;
				UIGameActor.DeleteSelectedBlueprints();
			}
		}
	}

	public static void DoneInstantiating()
	{
		instantiatingCompound = false;
		LuaHookBase.InvokeFromCLR(lua_OnDoneInstantiating);
	}

	public static bool IsInstantiatingCompound()
	{
		return instantiatingCompound;
	}

	public static void PlaceCompoundBlueprint(BlueprintCompoundComponent bcc)
	{
		UIGameSelector.UnselectAllInTrack(SelectionTrackID.Blueprints);
		GameSignals.Invoke(PickingObserver.lua_BlueprintSelectionActionTaken);
		bcc.PlaceBlueprintsSignal.Invoke();
		bcc.DoneInstantiatingSignal.ConnectOnce(DoneInstantiating);
		instantiatingCompound = true;
	}

	public static void CancelCompoundBlueprintPlacement(BlueprintCompoundComponent bcc)
	{
		if (current != null && CompoundPlacementMode == PlacementMode.move)
		{
			bcc.PlaceBackOriginalBlueprints();
		}
		current = null;
		if (bcc != null)
		{
			bcc.Clear();
		}
	}

	public static void SaveSelected()
	{
		if (current != null && currentName != string.Empty)
		{
			Texture2D src = UIGameObserver.TmpBlueprintThumbnail ?? GameParam.Map.Disk_ReadThumbnail();
			current.customName = currentName;
			src.CopyTo(ref current.thumbnail);
			if (blueprintsDictionary.ContainsKey(currentName))
			{
				blueprintsDictionary.Remove(currentName);
			}
			if (thumbnailDictionary.ContainsKey(currentName))
			{
				thumbnailDictionary.Remove(currentName);
			}
			thumbnailDictionary.Add(currentName, Sprite.Create(current.thumbnail, new Rect(0f, 0f, current.thumbnail.width, current.thumbnail.height), new Vector2(0.5f, 0.5f)));
			blueprintsDictionary.Add(currentName, current);
			current.Disk_WriteAsNew(AssetAuthorType.UGC);
			Picking.Instance.CancelPickOrder();
			Picking.Instance.ClearPick();
		}
	}

	public static Asset_Blueprints GetCurrentSelected()
	{
		if (blueprintsDictionary[currentSelected] != null)
		{
			return blueprintsDictionary[currentSelected];
		}
		return null;
	}

	public static string GetCurrentSelectedName()
	{
		return currentSelected;
	}

	public static List<Asset_Blueprints> GetAllSavedCompounds()
	{
		return AssetDispensary.blueprints.SendQuery(AssetQueryParameters.AllSources, AssetPlayableContext.Any);
	}

	public static void InitializeCompoundDictionary()
	{
		List<Asset_Blueprints> allSavedCompounds = GetAllSavedCompounds();
		foreach (Asset_Blueprints item in allSavedCompounds)
		{
			if (!blueprintsDictionary.ContainsKey(item.customName) && item.Disk_Exists())
			{
				if (item.thumbnail == null)
				{
					item.Disk_ReadThumbnail();
				}
				if (!(item.thumbnail == null))
				{
					blueprintsDictionary.Add(item.customName, item);
					item.InitGraph();
				}
			}
		}
	}

	public static void SendToWorkshop()
	{
		if (currentSelected != null)
		{
			blueprintsDictionary[currentSelected].SendToWorkshop(currentSelected, string.Empty, string.Empty, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic);
		}
	}

	public static void ClearStaticLists()
	{
		blueprintsDictionary.Clear();
		thumbnailDictionary.Clear();
	}

	public static void RegisterClass(Script script)
	{
		script.Globals["BlueprintSelection"] = _desc.GetStaticProxy();
	}

	[LuaDrivable("Move")]
	private static DynValue lua_Move(ScriptExecutionContext context, CallbackArguments args)
	{
		Move();
		return DynValue.Void;
	}

	[LuaDrivable("Duplicate")]
	private static DynValue lua_Duplicate(ScriptExecutionContext context, CallbackArguments args)
	{
		Duplicate();
		return DynValue.Void;
	}

	[LuaDrivable("Split")]
	private static DynValue lua_Split(ScriptExecutionContext context, CallbackArguments args)
	{
		UIGameActor.SplitSelectedBlueprints();
		return DynValue.Void;
	}

	[LuaDrivable("Delete")]
	private static DynValue lua_Delete(ScriptExecutionContext context, CallbackArguments args)
	{
		UIGameActor.DeleteSelectedBlueprints();
		return DynValue.Void;
	}

	[LuaDrivable("Expand")]
	private static DynValue lua_Expand(ScriptExecutionContext context, CallbackArguments args)
	{
		Expand();
		return DynValue.Void;
	}

	[LuaDrivable("Cancel")]
	private static DynValue lua_Cancel(ScriptExecutionContext context, CallbackArguments args)
	{
		Cancel();
		return DynValue.Void;
	}

	[LuaDrivable("GetCurrentName")]
	private static DynValue lua_GetCurrentName(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewString(currentName);
	}

	[LuaDrivable("SetCurrentName")]
	private static DynValue lua_SetCurrentName(ScriptExecutionContext context, CallbackArguments args)
	{
		string text = args.AsString(0, "SetCurrentName");
		currentName = text;
		return DynValue.Void;
	}

	[LuaDrivable("GetPlacementMode")]
	private static DynValue lua_GetPlacementMode(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.FromObject(context.OwnerScript, CompoundPlacementMode);
	}

	[LuaDrivable("SaveSelected")]
	private static DynValue lua_SaveSelected(ScriptExecutionContext context, CallbackArguments args)
	{
		SaveSelected();
		return DynValue.Void;
	}

	[LuaDrivable("SendToWorkshop")]
	private static DynValue lua_SendToWorkshop(ScriptExecutionContext context, CallbackArguments args)
	{
		SendToWorkshop();
		return DynValue.Void;
	}

	[LuaDrivable("DuplicateAndSave")]
	private static DynValue lua_DuplicateAndSave(ScriptExecutionContext context, CallbackArguments args)
	{
		Duplicate();
		SaveSelected();
		return DynValue.Void;
	}

	[LuaDrivable("SelectCompound")]
	private static DynValue lua_SelectCompound(ScriptExecutionContext context, CallbackArguments args)
	{
		string text = args.AsString(0, "LoadSelected");
		currentSelected = text;
		lua_CompoundSelectionChanged._signal.Invoke();
		return DynValue.Void;
	}

	[LuaDrivable("LoadSelected")]
	private static DynValue lua_LoadSelected(ScriptExecutionContext context, CallbackArguments args)
	{
		LoadCompoundBlueprints(blueprintsDictionary[currentSelected], fromFile: true);
		return DynValue.Void;
	}

	[LuaDrivable("GetCurrentThumbnail")]
	private static DynValue lua_GetCurrentThumbnail(ScriptExecutionContext context, CallbackArguments args)
	{
		Texture2D texture2D = UIGameObserver.TmpBlueprintThumbnail ?? GameParam.Map.Disk_ReadThumbnail();
		currentThumbnail = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
		return UserData.CreateWithDescriptor(currentThumbnail, LuaTexture.desc);
	}

	[LuaDrivable("GetThumbnailFromFile")]
	private static DynValue lua_GetThumbnailFromFile(ScriptExecutionContext context, CallbackArguments args)
	{
		string key = args.AsString(0, "LoadSelected");
		Asset_Blueprints asset_Blueprints = blueprintsDictionary[key];
		Texture2D thumbnail = asset_Blueprints.Thumbnail;
		if (!thumbnailDictionary.ContainsKey(key))
		{
			thumbnailDictionary.Add(key, Sprite.Create(thumbnail, new Rect(0f, 0f, thumbnail.width, thumbnail.height), new Vector2(0.5f, 0.5f)));
		}
		else if (thumbnailDictionary[key].texture == null)
		{
			thumbnailDictionary.Remove(key);
			thumbnailDictionary.Add(key, Sprite.Create(thumbnail, new Rect(0f, 0f, thumbnail.width, thumbnail.height), new Vector2(0.5f, 0.5f)));
		}
		return UserData.CreateWithDescriptor(thumbnailDictionary[key], LuaTexture.desc);
	}

	[LuaDrivable("DeleteSelected")]
	private static DynValue lua_DeleteSelected(ScriptExecutionContext context, CallbackArguments args)
	{
		if (currentSelected != string.Empty)
		{
			Asset_Blueprints asset_Blueprints = blueprintsDictionary[currentSelected];
			blueprintsDictionary.Remove(currentSelected);
			thumbnailDictionary.Remove(currentSelected);
			currentSelected = string.Empty;
			asset_Blueprints.Disk_Delete();
			LuaHookBase.InvokeFromCLR(lua_OnCompoundDeleted);
		}
		return DynValue.Void;
	}

	[LuaDrivable("GetCurrentSelected")]
	private static DynValue lua_GetCurrentSelected(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewString(currentSelected);
	}

	[LuaDrivable("HasSelectedAny")]
	private static DynValue lua_HasSelectedAny(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewBoolean(currentSelected != string.Empty);
	}

	[LuaDrivable("HasBlueprintSelection")]
	private static DynValue lua_HasBlueprintSelection(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewBoolean(UIGameSelector.HasSelectionInTrack(SelectionTrackID.Blueprints) || UIGameSelector.HasSelectionInTrack(SelectionTrackID.Blocks));
	}

	[LuaDrivable("SelectionIsFromWorkshop")]
	private static DynValue lua_SelectionIsFromWorkshop(ScriptExecutionContext context, CallbackArguments args)
	{
		Asset_Blueprints asset_Blueprints = blueprintsDictionary[currentSelected];
		return DynValue.NewBoolean(asset_Blueprints.authorType == AssetAuthorType.Workshop);
	}

	[LuaDrivable("IsInstantiating")]
	private static DynValue lua_IsInstantiating(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewBoolean(instantiatingCompound);
	}

	[LuaDrivable("GetCompoundList")]
	private static DynValue lua_GetCompoundList(ScriptExecutionContext context, CallbackArguments args)
	{
		Table table = new Table(context.OwnerScript);
		foreach (KeyValuePair<string, Asset_Blueprints> item in blueprintsDictionary)
		{
			table.Append(DynValue.NewString(item.Key));
		}
		return DynValue.NewTable(table);
	}

	[LuaDrivable("InitializeCompounds")]
	private static DynValue lua_InitializeCompounds(ScriptExecutionContext context, CallbackArguments args)
	{
		InitializeCompoundDictionary();
		GameSignals.Connect(GameSignals.LeaveGame, ClearStaticLists);
		return DynValue.Void;
	}
}
