using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Factories;
using Brix.External.Factories.Templates;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Game.Network;
using Brix.Game.Semantique;
using Brix.Lifecycle.Pooling;
using MoonSharp.Interpreter;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Lua;

public class LuaFactory
{
	public static void RegisterClass(Script script)
	{
		DynValue value = DynValue.NewTable(script);
		value.Table.Set("CreateItemFrom", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => CreateItemFrom(exec, args), "CreateItemFrom"));
		value.Table.Set("Instantiate", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => Instantiate(exec, args), "Instantiate"));
		value.Table.Set("NetworkInstantiate", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => NetworkInstantiate(exec, args), "NetworkInstantiate"));
		value.Table.Set("PlaceObject", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => PlaceObject(exec, args), "PlaceObject"));
		value.Table.Set("PlaceBlock", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => PlaceBlock(exec, args), "PlaceBlock"));
		value.Table.Set("PlaceBlueprint", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => PlaceBlueprint(exec, args), "PlaceBlueprint"));
		value.Table.Set("Explosion", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => Explosion(exec, args), "Explosion"));
		script.Globals.Set("Factory", value);
	}

	public static DynValue CreateItemFrom(ScriptExecutionContext context, CallbackArguments args)
	{
		LuaStaticDriver luaStaticDriver = args.AsUserData<LuaStaticDriver>(0, "Factory.CreateItemFrom");
		string text = args.AsString(1, "Factory.CreateItemFrom");
		GameObject gameObject = Factory.Instantiate(luaStaticDriver.AssetKey, active: false);
		gameObject.name = text;
		gameObject.GetComponent<LuaDriver>().Handle = text;
		Apparence component = gameObject.GetComponent<Apparence>();
		if (component != null)
		{
			List<Adjectif> list = component.description.Adjectifs.Where((Adjectif x) => x.GetType() != typeof(WorkerTool) && x.GetType() != typeof(KnightTool) && x.GetType() != typeof(ArcherTool) && x.GetType() != typeof(Protection)).ToList();
			Object.Destroy(component);
			component = gameObject.AddComponent<Apparence>();
			foreach (Adjectif item in list)
			{
				component.description.Add(item);
			}
		}
		Object.Destroy((Component)gameObject.GetInterface<IVolume>());
		Factory orCreateFactory = Factory.GetOrCreateFactory("ModsFactory");
		Factory.AssetKey key = orCreateFactory.AddAsset<CloneTemplate>(gameObject);
		LuaDriverHQ.AddNewDriver(Factory.Peek(key).GetComponent<LuaDriver>(), fromFactory: true);
		return DynValue.Void;
	}

	public static DynValue Instantiate(ScriptExecutionContext context, CallbackArguments args)
	{
		Factory.AssetKey key = args.AsUserData<Factory.AssetKey>(0, "Factory.Instantiate");
		Faction faction = args.AsComponent<Faction>(1, "Factory.Instantiate");
		Vector3 position = args.AsUserData<Vector3>(2, "Factory.Instantiate");
		Quaternion rotation = ((args.Count < 4) ? Quaternion.identity : args.AsUserData<Quaternion>(3, "Factory.Instantiate"));
		return DynValue.FromObject(context.OwnerScript, InstantiateFromCLR(key, faction, position, rotation));
	}

	public static LuaDriver InstantiateFromCLR(Factory.AssetKey key, Faction faction, Vector3 position, Quaternion rotation)
	{
		ObjectPoolSingleton.Request(out var go, key, position, rotation);
		AffiliationComponent component = go.GetComponent<AffiliationComponent>();
		if (component != null)
		{
			component.faction = faction;
		}
		if (go.GetComponent<NetworkIdentity>() != null)
		{
			NetworkServer.Spawn(go, go.GetComponent<AssetKeyHolder>().AssetKey.AssetId);
		}
		return go.GetComponent<LuaDriver>();
	}

	public static DynValue NetworkInstantiate(ScriptExecutionContext context, CallbackArguments args)
	{
		Factory.AssetKey asset = args.AsUserData<Factory.AssetKey>(0, "Factory.NetworkInstantiate");
		Faction faction = args.AsComponent<Faction>(1, "Factory.NetworkInstantiate");
		Closure function = args.AsType(2, "Factory.NetworkInstantiate", DataType.Function).Function;
		Vector3 position = args.AsUserData<Vector3>(3, "Factory.NetworkInstantiate");
		Quaternion rotation = ((args.Count < 5) ? Quaternion.identity : args.AsUserData<Quaternion>(4, "Factory.NetworkInstantiate"));
		GameObject gameObject = Factory.Instantiate(asset, position, rotation);
		AffiliationComponent component = gameObject.GetComponent<AffiliationComponent>();
		if (component != null && faction != null)
		{
			component.faction = faction;
		}
		NetworkServer.Spawn(gameObject, gameObject.GetComponent<AssetKeyHolder>().AssetKey.AssetId);
		function.Call(DynValue.FromObject(context.OwnerScript, gameObject));
		return DynValue.FromObject(context.OwnerScript, gameObject.GetComponent<LuaDriver>());
	}

	public static DynValue PlaceObject(ScriptExecutionContext context, CallbackArguments args)
	{
		Factory.AssetKey key = args.AsUserData<Factory.AssetKey>(0, "Factory.PlaceObject");
		Vector3 position = args.AsUserData<Vector3>(1, "Factory.PlaceObject");
		Quaternion rotation = ((args.Count < 3) ? Quaternion.identity : args.AsUserData<Quaternion>(2, "Factory.PlaceObject"));
		ObjectPoolSingleton.Request_Unsafe(key, position, rotation);
		return DynValue.Void;
	}

	public static DynValue PlaceBlock(ScriptExecutionContext context, CallbackArguments args)
	{
		Factory.AssetKey key = args.AsUserData<Factory.AssetKey>(0, "Factory.PlaceBlock");
		Vector3 position = args.AsUserData<Vector3>(1, "Factory.PlaceBlock");
		Quaternion rotation = ((args.Count < 3) ? Quaternion.identity : args.AsUserData<Quaternion>(2, "Factory.PlaceBlock"));
		ObjectPoolSingleton.RequestComponent<Blueprint>(out var t, BlockInfo.GetFromKey(key).BlueprintKey, position, rotation);
		t.MorphToObject();
		return DynValue.Void;
	}

	public static DynValue PlaceBlueprint(ScriptExecutionContext context, CallbackArguments args)
	{
		Factory.AssetKey assetKey = args.AsUserData<Factory.AssetKey>(0, "Factory.PlaceBlueprint");
		Vector3 position = args.AsUserData<Vector3>(1, "Factory.PlaceBlueprint");
		Quaternion rotation = ((args.Count < 3) ? Quaternion.identity : args.AsUserData<Quaternion>(2, "Factory.PlaceBlueprint"));
		User.LocalUser.GetComponent<UNetBlueprint>().CallCmdPlaceBlueprint(assetKey.Factory, assetKey.Name, position, rotation);
		return DynValue.Void;
	}

	public static DynValue Explosion(ScriptExecutionContext context, CallbackArguments args)
	{
		Faction faction = args.AsComponent<Faction>(0, "Factory.Explosion");
		Vector3 pos = args.AsUserData<Vector3>(1, "Factory.Explosion");
		float force = ((args.Count < 3) ? 4f : args.AsFloat(2, "Factory.Explosion"));
		Architecte.ExplosionSimple(faction, pos, force);
		return DynValue.Void;
	}
}
