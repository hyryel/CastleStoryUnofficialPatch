using Brix.Engine;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Brix.Lua;

public class LuaMetaIVoxel
{
	public static LuaObjectDescriptor<IVoxel> desc = new LuaObjectDescriptor<IVoxel>(typeof(LuaMetaIVoxel));

	[LuaDrivable("hk_OnVoxelDestroyed")]
	private static LuaHook<IVoxel, bool, bool> _voxelDestroyed = new LuaHook<IVoxel, bool, bool>(GameSignals.VoxelDestroyed);

	[LuaDrivable("hk_OnTerrainRemoved")]
	private static LuaHook<Vector3> _terrainRemoved = new LuaHook<Vector3>(GameSignals.TerrainRemovedLua);

	public static void RegisterClass(Script script)
	{
		script.Globals.Set("Voxel", desc.GetStaticProxy());
	}

	[LuaDrivable("GetPosition")]
	private static DynValue _GetPosition(ScriptExecutionContext context, CallbackArguments args)
	{
		IVoxel voxel = args.AsUserData<IVoxel>(0, "GetPosition");
		return UserData.CreateWithDescriptor(voxel.Position.ToVector3(), LuaVector3.desc);
	}

	[LuaDrivable("GetID")]
	private static DynValue _GetID(ScriptExecutionContext context, CallbackArguments args)
	{
		IVoxel voxel = args.AsUserData<IVoxel>(0, "GetID");
		return DynValue.NewNumber(voxel.BlockInfo.ID);
	}
}
