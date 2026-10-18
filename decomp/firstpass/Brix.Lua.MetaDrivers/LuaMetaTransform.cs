using MoonSharp.Interpreter;
using UnityEngine;

namespace Brix.Lua.MetaDrivers;

[LuaMetaDrivable(typeof(Transform))]
public abstract class LuaMetaTransform
{
	[LuaDrivable("position", UseExtern = true)]
	public abstract Vector3 position { get; set; }

	[LuaDrivable("localPosition", UseExtern = true)]
	public abstract Vector3 localPosition { get; set; }

	[LuaDrivable("rotation", UseExtern = true)]
	public abstract Quaternion rotation { get; set; }

	[LuaDrivable("localRotation", UseExtern = true)]
	public abstract Quaternion localRotation { get; set; }

	[LuaDrivable("scale", UseExtern = true)]
	public abstract Vector3 localScale { get; set; }

	[LuaDrivable("Translate")]
	private static DynValue lua_Translate(ScriptExecutionContext context, CallbackArguments args)
	{
		Transform transform = args.AsComponent<Transform>(0, "Translate");
		Vector3 translation = args.AsUserData<Vector3>(1, "Translate");
		transform.Translate(translation);
		return DynValue.Void;
	}
}
