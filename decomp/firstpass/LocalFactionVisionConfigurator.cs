using System;
using Brix.Assets;
using Brix.Game.Components;
using Brix.Game.Rules;
using Brix.Lua;
using Brix.Network.Util;
using Brix.Utils.Logging;
using MoonSharp.Interpreter;
using Newtonsoft.Json;
using UnityEngine;

[SerializerTarget(new string[] { "save" })]
public class LocalFactionVisionConfigurator : MonoBehaviour
{
	[JsonProperty]
	[SerializeField]
	private int _fogDistance = -1;

	private int FogDistance
	{
		get
		{
			if (_fogDistance == -1)
			{
				try
				{
					_fogDistance = DistanceForSize(GameParam.Map.mapSize);
				}
				catch (Exception ex)
				{
					_fogDistance = 60;
					new BrixLogger(typeof(LocalFactionVisionConfigurator).FullName).Error("Could not read map size for fog of war distance :" + ex.Message);
				}
			}
			return _fogDistance;
		}
	}

	private void Start()
	{
		SetFogOfWar();
		GeneralRules.Main.FogOfWarDistanceChanged.Connect(SetFogOfWar);
	}

	private void SetFogOfWar()
	{
		LocalFactionVision.AllVisible = false;
		GeneralRules.Main.FogOfWarDistance = FogDistance;
	}

	public static void ToggleFog(bool value)
	{
		LocalFactionVision.AllVisible = !value;
	}

	private int DistanceForSize(Asset_Map.MapSize size)
	{
		return size switch
		{
			Asset_Map.MapSize.Large => 80, 
			Asset_Map.MapSize.Medium => 60, 
			Asset_Map.MapSize.Small => 40, 
			_ => 60, 
		};
	}

	[LuaDrivable("ToggleFog")]
	private static DynValue lua_ToggleFog(ScriptExecutionContext context, CallbackArguments args)
	{
		bool value = args.AsBool(0, "ToggleFog");
		ToggleFog(value);
		return DynValue.Void;
	}
}
