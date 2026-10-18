using MoonSharp.Interpreter;
using UnityEngine;

namespace Brix.Lua;

public abstract class LuaQuaternion
{
	private static LuaObjectDescriptor<Quaternion> desc = new LuaObjectDescriptor<Quaternion>(typeof(LuaQuaternion));

	private static DynValue _identity = UserData.CreateWithDescriptor(Quaternion.identity, desc);

	[LuaDrivable("x", UseExtern = true)]
	public float x;

	[LuaDrivable("y", UseExtern = true)]
	public float y;

	[LuaDrivable("z", UseExtern = true)]
	public float z;

	[LuaDrivable("w", UseExtern = true)]
	public float w;

	[LuaDrivable("identity")]
	public static DynValue identity => _identity;

	[LuaDrivable("kEpsilon")]
	public float kEpsilon => 1E-06f;

	public static void RegisterClass(Script script)
	{
		script.Globals.Set("Quaternion", desc.GetStaticProxy());
	}

	[LuaDrivable("NewXYZW")]
	public static DynValue NewXYZW(ScriptExecutionContext context, CallbackArguments args)
	{
		float num = args.AsFloat(0, "Quaternion.NewXYZW");
		float num2 = args.AsFloat(1, "Quaternion.NewXYZW");
		float num3 = args.AsFloat(2, "Quaternion.NewXYZW");
		float num4 = args.AsFloat(4, "Quaternion.NewXYZW");
		return UserData.CreateWithDescriptor(new Quaternion(num, num2, num3, num4), desc);
	}

	[LuaDrivable("New")]
	public static DynValue New(ScriptExecutionContext context, CallbackArguments args)
	{
		float num = args.AsFloat(0, "Quaternion.New");
		float num2 = args.AsFloat(1, "Quaternion.New");
		float num3 = args.AsFloat(2, "Quaternion.New");
		return UserData.CreateWithDescriptor(Quaternion.Euler(num, num2, num3), desc);
	}

	[LuaDrivable("EulerXYZ")]
	public static DynValue EulerXYZ(ScriptExecutionContext context, CallbackArguments args)
	{
		float num = args.AsFloat(0, "Quaternion.EulerXYZ");
		float num2 = args.AsFloat(1, "Quaternion.EulerXYZ");
		float num3 = args.AsFloat(2, "Quaternion.EulerXYZ");
		return UserData.CreateWithDescriptor(Quaternion.Euler(num, num2, num3), desc);
	}

	[LuaDrivable("Euler")]
	public static DynValue Euler(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector3 euler = args.AsUserData<Vector3>(0, "Quaternion.Euler");
		return UserData.CreateWithDescriptor(Quaternion.Euler(euler), desc);
	}

	[LuaDrivable("Dot")]
	public static DynValue Dot(ScriptExecutionContext context, CallbackArguments args)
	{
		Quaternion a = args.AsUserData<Quaternion>(0, "Quaternion.Dot");
		Quaternion b = args.AsUserData<Quaternion>(1, "Quaternion.Dot");
		return DynValue.NewNumber(Quaternion.Dot(a, b));
	}

	[LuaDrivable("Angle")]
	public static DynValue Angle(ScriptExecutionContext context, CallbackArguments args)
	{
		Quaternion a = args.AsUserData<Quaternion>(0, "Quaternion.Angle");
		Quaternion b = args.AsUserData<Quaternion>(1, "Quaternion.Angle");
		return DynValue.NewNumber(Quaternion.Angle(a, b));
	}

	[LuaDrivable("AngleAxis")]
	public static DynValue AngleAxis(ScriptExecutionContext context, CallbackArguments args)
	{
		float angle = args.AsFloat(0, "Quaternion.AngleAxis");
		Vector3 axis = args.AsUserData<Vector3>(1, "Quaternion.AngleAxis");
		return UserData.CreateWithDescriptor(Quaternion.AngleAxis(angle, axis), desc);
	}

	[LuaDrivable("FromToRotation")]
	public static DynValue FromToRotation(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector3 fromDirection = args.AsUserData<Vector3>(0, "Quaternion.FromToRotation");
		Vector3 toDirection = args.AsUserData<Vector3>(1, "Quaternion.FromToRotation");
		return UserData.CreateWithDescriptor(Quaternion.FromToRotation(fromDirection, toDirection), desc);
	}

	[LuaDrivable("Inverse")]
	public static DynValue Inverse(ScriptExecutionContext context, CallbackArguments args)
	{
		Quaternion rotation = args.AsUserData<Quaternion>(0, "Quaternion.rotation");
		return UserData.CreateWithDescriptor(Quaternion.Inverse(rotation), desc);
	}

	[LuaDrivable("Lerp")]
	public static DynValue Lerp(ScriptExecutionContext context, CallbackArguments args)
	{
		Quaternion a = args.AsUserData<Quaternion>(0, "Quaternion.Lerp");
		Quaternion b = args.AsUserData<Quaternion>(1, "Quaternion.Lerp");
		float t = args.AsFloat(2, "Quaternion.Lerp");
		return UserData.CreateWithDescriptor(Quaternion.Lerp(a, b, t), desc);
	}

	[LuaDrivable("Slerp")]
	public static DynValue Slerp(ScriptExecutionContext context, CallbackArguments args)
	{
		Quaternion a = args.AsUserData<Quaternion>(0, "Quaternion.Slerp");
		Quaternion b = args.AsUserData<Quaternion>(1, "Quaternion.Slerp");
		float t = args.AsFloat(2, "Quaternion.Slerp");
		return UserData.CreateWithDescriptor(Quaternion.Slerp(a, b, t), desc);
	}

	[LuaDrivable("LookRotation")]
	public static DynValue LookRotation(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector3 forward = args.AsUserData<Vector3>(0, "Quaternion.LookRotation");
		Vector3 upwards = ((args.Count < 2) ? Vector3.up : args.AsUserData<Vector3>(1, "Quaternion.LookRotation"));
		return UserData.CreateWithDescriptor(Quaternion.LookRotation(forward, upwards), desc);
	}

	[LuaDrivable("RotateTowards")]
	public static DynValue RotateTowards(ScriptExecutionContext context, CallbackArguments args)
	{
		Quaternion quaternion = args.AsUserData<Quaternion>(0, "Quaternion.RotateTowards");
		Quaternion to = args.AsUserData<Quaternion>(1, "Quaternion.RotateTowards");
		float maxDegreesDelta = args.AsFloat(2, "Quaternion.RotateTowards");
		return UserData.CreateWithDescriptor(Quaternion.RotateTowards(quaternion, to, maxDegreesDelta), desc);
	}

	[LuaDrivable("__mul")]
	private static DynValue __add(ScriptExecutionContext context, CallbackArguments args)
	{
		Quaternion quaternion = args.AsUserData<Quaternion>(0, "Quaternion.__mul");
		Quaternion quaternion2 = args.AsUserData<Quaternion>(1, "Quaternion.__mul");
		return UserData.CreateWithDescriptor(quaternion * quaternion2, desc);
	}

	[LuaDrivable("__eq")]
	public static DynValue __eq(ScriptExecutionContext context, CallbackArguments args)
	{
		Quaternion quaternion = args.AsUserData<Quaternion>(0, "Quaternion.__eq", allowNil: true);
		Quaternion quaternion2 = args.AsUserData<Quaternion>(1, "Quaternion.__eq", allowNil: true);
		return DynValue.NewBoolean(quaternion == quaternion2);
	}
}
