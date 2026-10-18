using MoonSharp.Interpreter;
using UnityEngine;

namespace Brix.Lua;

public class LuaPhysics
{
	private static LuaObjectDescriptor<LuaPhysics> _desc = new LuaObjectDescriptor<LuaPhysics>();

	public static void RegisterClass(Script script)
	{
		script.Globals["Physics"] = _desc.GetStaticProxy();
	}

	[LuaDrivable("CheckTwoPoints")]
	private static DynValue CheckTwoPoints(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector3 a = args.AsUserData<Vector3>(0, "CheckTwoPoints");
		Vector3 b = args.AsUserData<Vector3>(1, "CheckTwoPoints");
		float num = args.AsFloat(2, "CheckTwoPoints");
		return DynValue.NewBoolean(Vector3.Distance(a, b) <= num);
	}

	[LuaDrivable("ColliderContainsPoint")]
	private static DynValue ColliderContainsPoint(ScriptExecutionContext context, CallbackArguments args)
	{
		LuaDriver luaDriver = args.AsUserData<LuaDriver>(0, "ColliderContainsPoint");
		Vector3 point = args.AsUserData<Vector3>(1, "ColliderContainsPoint");
		Collider componentInChildren = luaDriver.GetComponentInChildren<Collider>();
		if (componentInChildren == null)
		{
			throw new ScriptRuntimeException("error in arg #2, driver doesn't have a collider");
		}
		return DynValue.NewBoolean(componentInChildren.bounds.Contains(point));
	}

	[LuaDrivable("PointIsWithinRadius")]
	private static DynValue PointIsWithinRadius(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector3 a = args.AsUserData<Vector3>(0, "PointIsWithinRadius");
		Vector3 b = args.AsUserData<Vector3>(1, "PointIsWithinRadius");
		float num = args.AsFloat(2, "PointIsWithinRadius");
		return DynValue.NewBoolean(Vector3.Distance(a, b) <= num);
	}

	[LuaDrivable("SphereCastFor")]
	private static DynValue SphereCastFor(ScriptExecutionContext context, CallbackArguments args)
	{
		GameObject gameObject = args.AsGameObject(0, "SphereCastFor");
		Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
		Physics.SphereCast(ray, 1.5f, out var hitInfo, 300f, 71681);
		if (hitInfo.collider != null)
		{
			return DynValue.NewBoolean(hitInfo.collider.gameObject == gameObject);
		}
		return DynValue.NewBoolean(v: false);
	}
}
