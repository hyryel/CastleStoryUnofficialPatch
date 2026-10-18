using System;
using System.Collections.Generic;
using Brix.Engine.Nature;
using Brix.External.Factories;
using Brix.Game.AI;
using Brix.Lua;
using Brix.Utils;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

public static class LuaProjectStorage
{
	[Flags]
	private enum lua_ResourceType
	{
		None = 0,
		Stockpile = 1,
		Boulder = 2,
		Iron = 4,
		Brimstone = 8,
		Plant = 0x10,
		Wood = 0x20,
		Plank = 0x40,
		Block = 0x80,
		Minerals = 0xE,
		Raw = 0x1E,
		Build = 0xE0,
		All = 0x7F
	}

	public static LuaHook<Project> lua_StorageOptionsChanged = new LuaHook<Project>();

	public static LuaHookCollapsed<Project> lua_CollapsedStorageOptionsChanged = new LuaHookCollapsed<Project>(PersistentCoroutineRunner.UpdateSignal, new LuaHook<Project>[1] { lua_StorageOptionsChanged }, noDoubles: true);

	public static void AddToLuaTable(Script script, DynValue t)
	{
		t.Table.Set("resourceType", new LuaEnumDescriptor<lua_ResourceType>().CreateDynValue());
		t.Table.Set("IsStorageManagementReady", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_IsStorageManagementReady(exec, args)));
		t.Table.Set("IsBlueprintManagementReady", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_IsBlueprintManagementReady(exec, args)));
		t.Table.Set("GetResourceCount", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_GetResourceCount(exec, args)));
		t.Table.Set("GetRequiredResourceCount", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_GetRequiredResourceCount(exec, args)));
		t.Table.Set("GetStockpiledResourceTypes", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_GetStockpiledResourceTypes(exec, args)));
		t.Table.Set("GetStockpiledResourceFindability", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_GetStockpiledResourceFindability(exec, args)));
		t.Table.Set("GetStockpiledResourceCount", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_GetStockpiledResourceCount(exec, args)));
		t.Table.Set("GetStockpiledRequiredResourceCount", DynValue.NewCallback((ScriptExecutionContext exec, CallbackArguments args) => lua_GetStockpiledRequiredResourceCount(exec, args)));
		t.Table.Set("hk_OnStorageOptionsChanged", UserData.CreateWithDescriptor(lua_CollapsedStorageOptionsChanged, LuaHookBase.desc));
	}

	public static void CallStorageSignals(Project p)
	{
		LuaHookBase.InvokeFromCLR(lua_StorageOptionsChanged, p);
	}

	private static Ressource lua_TypeToAdj(lua_ResourceType type)
	{
		return type switch
		{
			lua_ResourceType.Iron => Adjectif.rawIron, 
			lua_ResourceType.Boulder => Adjectif.stones, 
			lua_ResourceType.Brimstone => Adjectif.orangeCrystal, 
			lua_ResourceType.Plant => Adjectif.plant, 
			lua_ResourceType.Wood => Adjectif.woodBlock, 
			lua_ResourceType.Plank => Adjectif.plankBlock, 
			lua_ResourceType.Block => Adjectif.stoneBlock, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private static lua_ResourceType FromNatureType(NatureType type)
	{
		lua_ResourceType lua_ResourceType = lua_ResourceType.None;
		lua_ResourceType = (lua_ResourceType)((int)lua_ResourceType | (type.HasFlag(NatureType.boulder) ? 2 : 0));
		lua_ResourceType = (lua_ResourceType)((int)lua_ResourceType | (type.HasFlag(NatureType.iron) ? 4 : 0));
		lua_ResourceType = (lua_ResourceType)((int)lua_ResourceType | (type.HasFlag(NatureType.pyrite) ? 8 : 0));
		return (lua_ResourceType)((int)lua_ResourceType | (type.HasFlag(NatureType.plant) ? 16 : 0));
	}

	private static lua_ResourceType lua_InterestingResources(Project p)
	{
		if (p is HarvestProject)
		{
			return FromNatureType((p as HarvestProject).AcceptedTypes);
		}
		if (p is MineProject || p is TunnelProject || p is LandscapeProject)
		{
			return lua_ResourceType.Minerals;
		}
		if (p is BuildProject || p is RepairProject || p is CleanupProject)
		{
			return lua_ResourceType.All;
		}
		if (p is IdleProject)
		{
			return lua_ResourceType.All;
		}
		throw new ArgumentOutOfRangeException();
	}

	private static lua_ResourceType lua_TypeFromStorable(Project p)
	{
		lua_ResourceType lua_ResourceType = lua_ResourceType.None;
		HashSet<Factory.AssetKey> keys = CollectionPool.RequestHashSet<Factory.AssetKey>();
		p.UniqueItemsGiven(ref keys);
		lua_ResourceType = (lua_ResourceType)((int)lua_ResourceType | (keys.Contains(RawResources.RawStone) ? 2 : 0));
		lua_ResourceType = (lua_ResourceType)((int)lua_ResourceType | (keys.Contains(RawResources.RawIron) ? 4 : 0));
		lua_ResourceType = (lua_ResourceType)((int)lua_ResourceType | (keys.Contains(RawResources.RawOrangeCrystal) ? 8 : 0));
		lua_ResourceType = (lua_ResourceType)((int)lua_ResourceType | (keys.Contains(RawResources.RawFiber) ? 16 : 0));
		lua_ResourceType = (lua_ResourceType)((int)lua_ResourceType | (keys.Contains(Blocks.Wood_DoublePlank_AA) ? 32 : 0));
		lua_ResourceType = (lua_ResourceType)((int)lua_ResourceType | (keys.Contains(Blocks.Stone_DoubleBrick) ? 128 : 0));
		keys.Dispose();
		return lua_ResourceType;
	}

	public static bool IsStorageManagementReady(Project project)
	{
		if (project is BuildProject)
		{
			return project.GoalCounts() != 0;
		}
		return !project.IsBeingPlaced;
	}

	public static DynValue lua_IsStorageManagementReady(ScriptExecutionContext sec, CallbackArguments args)
	{
		Project project = args.AsComponent<Project>(0, "IsStorageManagementReady");
		return (!IsStorageManagementReady(project)) ? DynValue.False : DynValue.True;
	}

	private static DynValue lua_IsBlueprintManagementReady(ScriptExecutionContext sec, CallbackArguments args)
	{
		return lua_IsStorageManagementReady(sec, args);
	}

	private static DynValue lua_GetResourceCount(ScriptExecutionContext sec, CallbackArguments args)
	{
		Project project = args.AsComponent<Project>(0, "GetResourceCount");
		lua_ResourceType lua_ResourceType = args.AsEnum<lua_ResourceType>(1, "GetResourceCount");
		int num;
		if (lua_ResourceType == lua_ResourceType.Stockpile)
		{
			num = -1;
		}
		else
		{
			Adjectif adjectif = lua_TypeToAdj(lua_ResourceType);
			num = ((!(project.ProjectData == null)) ? project.ProjectData.Storage.ContentDescription.Value(adjectif) : (-1));
		}
		return DynValue.NewNumber(num);
	}

	private static DynValue lua_GetRequiredResourceCount(ScriptExecutionContext sec, CallbackArguments ca)
	{
		Project project = ca.AsComponent<Project>(0, "GetRequiredResourceCount");
		lua_ResourceType lua_ResourceType = ca.AsEnum<lua_ResourceType>(1, "GetResourceCount");
		int num;
		if (lua_ResourceType == lua_ResourceType.Stockpile)
		{
			num = -1;
		}
		else
		{
			Adjectif adj = lua_TypeToAdj(lua_ResourceType);
			num = ((!(project.ProjectData == null)) ? project.ProjectData.BlueprinRecepteurCompound.QtyNeeded(adj) : (-1));
		}
		return DynValue.NewNumber(num);
	}

	private static DynValue lua_GetStockpiledResourceTypes(ScriptExecutionContext sec, CallbackArguments ca)
	{
		Project p = ca.AsComponent<Project>(0, "GetStockpiledResourceTypes");
		lua_ResourceType value = lua_InterestingResources(p);
		return DynValue.FromObject(sec.OwnerScript, value);
	}

	private static DynValue lua_GetStockpiledResourceFindability(ScriptExecutionContext sec, CallbackArguments ca)
	{
		Project p = ca.AsComponent<Project>(0, "GetStockpiledResourceFindability");
		lua_ResourceType flag = ca.AsEnum<lua_ResourceType>(1, "GetStockpiledResourceFindability");
		lua_ResourceType value = lua_TypeFromStorable(p) | lua_ResourceType.Stockpile;
		bool v = value.HasFlag(flag);
		return DynValue.NewBoolean(v);
	}

	private static DynValue lua_GetStockpiledResourceCount(ScriptExecutionContext sec, CallbackArguments ca)
	{
		Project project = ca.AsComponent<Project>(0, "GetStockpiledResourceCount");
		lua_ResourceType lua_ResourceType = ca.AsEnum<lua_ResourceType>(1, "GetStockpiledResourceCount");
		int num = ((project.ProjectData == null) ? (-1) : ((lua_ResourceType != lua_ResourceType.Stockpile) ? project.ProjectData.Storage.RecepteursFullOfCount(lua_TypeToAdj(lua_ResourceType)) : project.ProjectData.Storage.RecepteursEmptyCount()));
		return DynValue.NewNumber(num);
	}

	private static DynValue lua_GetStockpiledRequiredResourceCount(ScriptExecutionContext sec, CallbackArguments ca)
	{
		Project project = ca.AsComponent<Project>(0, "GetStockpiledRequiredResourceCount");
		lua_ResourceType lua_ResourceType = ca.AsEnum<lua_ResourceType>(1, "GetStockpiledRequiredResourceCount");
		int num = ((project.ProjectData == null) ? (-1) : ((lua_ResourceType != lua_ResourceType.Stockpile) ? project.ProjectData.Storage.RecepteursWithCount(lua_TypeToAdj(lua_ResourceType)) : project.ProjectData.Storage.StorageUnitCount));
		return DynValue.NewNumber(num);
	}
}
