using System;
using System.IO;
using Brix.Assets;
using Brix.Audio.Environment;
using Brix.Engine;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.AI.Nodes;
using Brix.Game.Components;
using Brix.Input;
using Brix.Lua;
using Brix.ThirdParty.Steamworks;
using Brix.Utils;
using Brix.Utils.Logging;
using MoonSharp.Interpreter;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.NewUI.CornerMenus;

public class MenuCalls
{
	private static BrixLogger logger = new BrixLogger(typeof(MenuCalls).FullName);

	private const string ScreenShotDirectory = "Screenshots";

	private const string QuickSavePath = "Info/Saves/_QuickSave";

	private static LuaObjectDescriptor<MenuCalls> _desc = new LuaObjectDescriptor<MenuCalls>();

	private static bool state = false;

	private MenuCalls()
	{
	}

	public static void RegisterClass(Script script)
	{
		script.Globals["MenuCalls"] = _desc.GetStaticProxy();
	}

	[LuaDrivable("TakeScreenShot")]
	private static DynValue TakeScreenShot(ScriptExecutionContext context, CallbackArguments args)
	{
		bool menusOff = false;
		string fileName = "Screenshot " + DateTime.Now.ToString().Replace("/", "-").Replace("\\", "-")
			.Replace(":", string.Empty);
		if (!Directory.Exists("Screenshots"))
		{
			Directory.CreateDirectory("Screenshots");
		}
		Neo.menusOff = menusOff;
		if (SteamworksPlugin.Initialized)
		{
			SteamworksPlugin.TriggerScreenshot();
		}
		ScreenShotTaker.CaptureScreenshot(hideMenus: true, createThumbnail: false, compoundMode: false, 1f, delegate(Texture2D screenshot, Texture2D thumbnail)
		{
			string text = "Screenshots/" + fileName + ".png";
			File.WriteAllBytes(text, screenshot.EncodeToPNG());
			logger.InfoFormat("PrintScreen! {0}", text);
			UnityEngine.Object.Destroy(screenshot);
		});
		Neo.menusOff = false;
		return DynValue.Void;
	}

	[LuaDrivable("TogglePause")]
	private static DynValue TogglePause(ScriptExecutionContext context, CallbackArguments args)
	{
		InputModeController.TogglePause();
		return DynValue.Void;
	}

	[LuaDrivable("ToggleMainMenu")]
	private static DynValue ToggleMainMenu(ScriptExecutionContext context, CallbackArguments args)
	{
		InputModeController.ToggleMainMenu();
		return DynValue.Void;
	}

	[LuaDrivable("QuickSave")]
	private static DynValue QuickSave(ScriptExecutionContext context, CallbackArguments args)
	{
		StatsTracker.SaveStats();
		Asset_Map asset_Map = AssetDispensary.saves.SendSaveQuery(Asset_Map.SaveType.Quicksave).Slinq().FirstOrDefault();
		if (asset_Map == null)
		{
			asset_Map = Asset_Map.CreateShellFrom(GameParam.Map, Asset_Map.MapType.Save);
			asset_Map.codename = "quicksave";
			asset_Map.rawDisplayName = "--Quicksave--";
			asset_Map.customName = "--Quicksave--";
			asset_Map.saveType = Asset_Map.SaveType.Quicksave;
			asset_Map.Disk_WriteAsNew(AssetAuthorType.Official);
		}
		else
		{
			asset_Map.isDebug = GameParam.Map.isDebug;
			asset_Map.Disk_Overwrite();
		}
		return DynValue.Void;
	}

	[LuaDrivable("ToggleXRay")]
	private static DynValue ToggleXRay(ScriptExecutionContext context, CallbackArguments args)
	{
		XRayImpl.instance.enabled = !XRayImpl.instance.enabled;
		return DynValue.Void;
	}

	[LuaDrivable("ToggleFogOfWar")]
	private static DynValue ToggleFogOfWar(ScriptExecutionContext context, CallbackArguments args)
	{
		LocalFactionVision.ToggleFogSystem(!LocalFactionVision.IsEnabled());
		return DynValue.Void;
	}

	[LuaDrivable("SetFogOfWar")]
	private static DynValue SetFogOfWar(ScriptExecutionContext context, CallbackArguments args)
	{
		bool value = args.AsBool(0, "SetFogOfWar");
		LocalFactionVision.ToggleFogSystem(value);
		return DynValue.Void;
	}

	[LuaDrivable("ToggleDebugs")]
	private static DynValue ToggleDebugs(ScriptExecutionContext context, CallbackArguments args)
	{
		state = !state;
		Navigation.DrawPathIfSelected = state;
		ProjectilePhysics.showDebug = state;
		Labor.showCombatDebug = state;
		NavigationNodes.DebugEveryoneCanDoEverything = state;
		return DynValue.Void;
	}

	[LuaDrivable("ToggleMinimap")]
	private static DynValue ToggleMinimap(ScriptExecutionContext context, CallbackArguments args)
	{
		UIGameObserver.ShowsMinimap = !UIGameObserver.ShowsMinimap;
		LuaHookBase.InvokeFromCLR(UIGameObserver.lua_OnSetShowsMinimap);
		return DynValue.Void;
	}

	[LuaDrivable("ToggleSpeedX2")]
	private static DynValue ToggleSpeedX2(ScriptExecutionContext context, CallbackArguments args)
	{
		ClockConfig.ToggleClockSpeed(ClockConfig.ClockSpeed.Double);
		return DynValue.Void;
	}

	[LuaDrivable("SetAudioDebug")]
	private static DynValue lua_SetAudioDebug(ScriptExecutionContext context, CallbackArguments args)
	{
		bool flag = args.AsBool(0, "SetAudioDebug");
		GameSignals.Invoke(GameSignals.SetAudioDebug, flag);
		EnvironmentSoundCluster.DebugEnabled = flag;
		return DynValue.Void;
	}

	[LuaDrivable("GetAudioDebug")]
	private static DynValue lua_GetAudioDebug(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewBoolean(EnvironmentSoundCluster.DebugEnabled);
	}
}
