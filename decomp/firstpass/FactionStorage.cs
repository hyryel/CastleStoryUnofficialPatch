using System;
using Brix.Components;
using Brix.Engine;
using Brix.External.Factories;
using Brix.Game;
using Brix.Game.Components;
using Brix.Game.Semantique;
using Brix.Game.Storage;
using Brix.Lifecycle.Pooling;
using Brix.Lua;
using Brix.Utils;
using Brix.Utils.Factories;
using Brix.Utils.Signals;
using MoonSharp.Interpreter;
using Smooth.Slinq;
using UnityEngine;

public class FactionStorage : GameComponent
{
	[SerializeField]
	private CompoundRecepteur storage;

	private static readonly LuaHook lua_OnSetContent = new LuaHook();

	public static LuaHookCollapsed lua_OnSetContentCollapsed;

	private static readonly Func<ScriptExecutionContext, CallbackArguments, DynValue> lua_GetContent = delegate(ScriptExecutionContext sec, CallbackArguments arg)
	{
		Faction faction = User.LocalUser.faction;
		return faction.Storage.storage.ContentDescription.ToDynTable(sec.OwnerScript, faction.Storage.storage.RecepteursEmptyCount());
	};

	private static bool lua_hasInit = false;

	public override void Initialize()
	{
		base.Initialize();
		GameSignals.LateConnectOnce(UIGameObserver.IsReady, delegate(FactionStorage aFactionStorage)
		{
			aFactionStorage.Init();
		}, this);
	}

	private void Init()
	{
		ConnectToSignals();
		AddStorages();
		storage.Changed.Connect(delegate
		{
			LuaHookBase.InvokeFromCLR(lua_OnSetContent);
		}).DisconnectWhen(base.OnReleaseOrDestroySignal);
	}

	private void ConnectToSignals()
	{
		UIGameObserver.storage.Added.Connect(delegate(Transform aTransform, FactionStorage aFactionStorage)
		{
			aFactionStorage.OnAddObject(aTransform);
		}, this).DisconnectWhen(base.OnReleaseOrDestroySignal);
		UIGameObserver.storage.Removed.Connect(delegate(Transform aTransform, FactionStorage aFactionStorage)
		{
			aFactionStorage.OnRemoveObject(aTransform);
		}, this).DisconnectWhen(base.OnReleaseOrDestroySignal);
	}

	public void AddStorages()
	{
		(from r in BrixSingleton<AutoList>.Instance.GetInstances(ObjetsDynamiques.Palette).Slinq().Where(Affiliation.BelongsToMe)
			select r.GetComponent<Recepteur>()).ForEach(delegate(Recepteur r, CompoundRecepteur s)
		{
			s.Add(r);
		}, storage);
	}

	public void OnAddObject(Transform t)
	{
		t.WithComponent(delegate(Recepteur r, CompoundRecepteur s)
		{
			s.Add(r);
		}, storage);
	}

	public void OnRemoveObject(Transform t)
	{
		t.WithComponent(delegate(Recepteur r, CompoundRecepteur s)
		{
			s.Remove(r);
		}, storage);
	}

	public static void RegisterClass(Script script)
	{
		if (!lua_hasInit)
		{
			lua_hasInit = true;
			lua_OnSetContentCollapsed = new LuaHookCollapsed(PersistentCoroutineRunner.UpdateSignal, new LuaHook[1] { lua_OnSetContent });
		}
		DynValue value = DynValue.NewTable(script);
		value.Table.Set("hk_OnSetContent", UserData.CreateWithDescriptor(lua_OnSetContentCollapsed, LuaHookBase.desc));
		value.Table.Set("GetContent", DynValue.NewCallback(lua_GetContent));
		script.Globals.Set("FactionStorage", value);
	}
}
