using Brix.Engine;
using Brix.Engine.Nature;
using Brix.Legacy;
using Brix.Util;
using Brix.Utils.Logging;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Brix.Lua;

public class LuaRandom
{
	private static BrixLogger logger = new BrixLogger(typeof(LuaRandom).FullName);

	private static LuaObjectDescriptor<Random> _desc = new LuaObjectDescriptor<Random>(typeof(LuaRandom));

	[LuaDrivable("insideUnitCircle", UseExtern = true)]
	public static extern Vector2 insideUnitCircle { get; }

	[LuaDrivable("insideUnitSphere", UseExtern = true)]
	public static extern Vector3 insideUnitSphere { get; }

	[LuaDrivable("onUnitSphere", UseExtern = true)]
	public static extern Vector3 onUnitSphere { get; }

	[LuaDrivable("rotation", UseExtern = true)]
	public static extern Quaternion rotation { get; }

	[LuaDrivable("rotationUniform", UseExtern = true)]
	public static extern Quaternion rotationUniform { get; }

	[LuaDrivable("state", UseExtern = true)]
	public static extern Random.State state { get; }

	[LuaDrivable("value", UseExtern = true)]
	public static extern float value { get; }

	[LuaDrivable("seed")]
	public static int seed
	{
		set
		{
			Random.InitState(value);
		}
	}

	public static void RegisterClass(Script script)
	{
		script.Globals["Random"] = _desc.GetStaticProxy();
	}

	[LuaDrivable("Range")]
	public static DynValue Range(ScriptExecutionContext context, CallbackArguments args)
	{
		float min = args.AsFloat(0, "Random.Range");
		float max = args.AsFloat(1, "Random.Range");
		return DynValue.NewNumber(Random.Range(min, max));
	}

	[LuaDrivable("RangeInt")]
	public static DynValue RangeInt(ScriptExecutionContext context, CallbackArguments args)
	{
		int min = args.AsInt(0, "Random.RangeInt");
		int max = args.AsInt(1, "Random.RangeInt");
		return DynValue.NewNumber(Random.Range(min, max));
	}

	[LuaDrivable("VoxelAround")]
	public static DynValue VoxelAround(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector3 vector = args.AsUserData<Vector3>(0, "Random.VoxelAround");
		float num = ((args.Count < 2) ? (Random.value * 4f + 2f) : args.AsFloat(1, "Random.VoxelAround"));
		int num2 = 0;
		Vector3 vector2 = vector;
		do
		{
			vector2 = Random.insideUnitSphere;
			vector2.y = 0f;
			vector2 = Calcule.RoundVecteur(vector + vector2.normalized * num);
			vector2 = Architecte.SurfaceRaycast(XYZ.FromVector3(vector2).ToVector3());
			num2++;
		}
		while (num2 <= 1000 && (TreeRegistry.Instance.IsNearATree(XYZ.FromVector3(vector2)) || NatureRegistry.HasNatureBlockerAt(XYZ.FromVector3(vector2)) || Voxel.IsEmpty(XYZ.FromVector3(vector2) - XYZ.up)));
		if (num2 > 1000)
		{
			logger.WarnFormat("LuaRandom.VoxelAround failed to find a valid location for point='{0}', radius='{1}'. Proceeding anyway.", vector, num);
		}
		return UserData.CreateWithDescriptor(vector2, LuaVector3.desc);
	}
}
