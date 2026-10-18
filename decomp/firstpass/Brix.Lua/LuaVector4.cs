using MoonSharp.Interpreter;
using UnityEngine;

namespace Brix.Lua;

public class LuaVector4
{
	public static LuaObjectDescriptor<Vector4> desc = new LuaObjectDescriptor<Vector4>(typeof(LuaVector4));

	private static DynValue _zero = UserData.CreateWithDescriptor(Vector4.zero, desc);

	private static DynValue _one = UserData.CreateWithDescriptor(Vector4.one, desc);

	[LuaDrivable("x", UseExtern = true)]
	public float x;

	[LuaDrivable("y", UseExtern = true)]
	public float y;

	[LuaDrivable("z", UseExtern = true)]
	public float z;

	[LuaDrivable("w", UseExtern = true)]
	public float w;

	[LuaDrivable("zero")]
	public static DynValue zero => _zero;

	[LuaDrivable("one")]
	public static DynValue one => _one;

	[LuaDrivable("kEpsilon")]
	public float kEpsilon => 1E-05f;

	public static void RegisterClass(Script script)
	{
		script.Globals.Set("Vector4", desc.GetStaticProxy());
	}

	[LuaDrivable("New")]
	public static DynValue New(ScriptExecutionContext context, CallbackArguments args)
	{
		float num = args.AsFloat(0, "Vector4.New");
		float num2 = args.AsFloat(1, "Vector4.New");
		float num3 = args.AsFloat(2, "Vector4.New");
		float num4 = args.AsFloat(3, "Vector4.New");
		return UserData.CreateWithDescriptor(new Vector4(num, num2, num3, num4), desc);
	}

	[LuaDrivable("Dot")]
	public static DynValue Dot(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector4 a = args.AsUserData<Vector4>(0, "Vector4.Dot");
		Vector4 b = args.AsUserData<Vector4>(1, "Vector4.Dot");
		return DynValue.NewNumber(Vector4.Dot(a, b));
	}

	[LuaDrivable("Distance")]
	public static DynValue Distance(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector4 a = args.AsUserData<Vector4>(0, "Vector4.Distance");
		Vector4 b = args.AsUserData<Vector4>(1, "Vector4.Distance");
		return DynValue.NewNumber(Vector4.Distance(a, b));
	}

	[LuaDrivable("Lerp")]
	public static DynValue Lerp(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector4 a = args.AsUserData<Vector4>(0, "Vector4.Lerp");
		Vector4 b = args.AsUserData<Vector4>(1, "Vector4.Lerp");
		float t = args.AsFloat(2, "Vector4.Lerp");
		return UserData.CreateWithDescriptor(Vector4.Lerp(a, b, t), desc);
	}

	[LuaDrivable("LerpUnclamped")]
	public static DynValue LerpUnclamped(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector4 a = args.AsUserData<Vector4>(0, "Vector4.Lerp");
		Vector4 b = args.AsUserData<Vector4>(1, "Vector4.Lerp");
		float t = args.AsFloat(2, "Vector4.Lerp");
		return UserData.CreateWithDescriptor(Vector4.LerpUnclamped(a, b, t), desc);
	}

	[LuaDrivable("Magnitude")]
	public static DynValue Magnitude(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector4 a = args.AsUserData<Vector4>(0, "Vector4:Magnitude");
		return DynValue.NewNumber(Vector4.Magnitude(a));
	}

	[LuaDrivable("Max")]
	public static DynValue Max(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector4 lhs = args.AsUserData<Vector4>(0, "Vector4.Max");
		Vector4 rhs = args.AsUserData<Vector4>(1, "Vector4.Max");
		return UserData.CreateWithDescriptor(Vector4.Max(lhs, rhs), desc);
	}

	[LuaDrivable("Min")]
	public static DynValue Min(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector4 lhs = args.AsUserData<Vector4>(0, "Vector4.Min");
		Vector4 rhs = args.AsUserData<Vector4>(1, "Vector4.Min");
		return UserData.CreateWithDescriptor(Vector4.Min(lhs, rhs), desc);
	}

	[LuaDrivable("MoveTowards")]
	public static DynValue MoveTowards(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector4 current = args.AsUserData<Vector4>(0, "Vector4.MoveTowards");
		Vector4 target = args.AsUserData<Vector4>(1, "Vector4.MoveTowards");
		float maxDistanceDelta = args.AsFloat(2, "Vector4.MoveTowards");
		return UserData.CreateWithDescriptor(Vector4.MoveTowards(current, target, maxDistanceDelta), desc);
	}

	[LuaDrivable("Normalize")]
	public static DynValue Normalize(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector4 a = args.AsUserData<Vector4>(0, "Vector4:Normalize");
		return UserData.CreateWithDescriptor(Vector4.Normalize(a), desc);
	}

	[LuaDrivable("Project")]
	public static DynValue Project(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector4 a = args.AsUserData<Vector4>(0, "Vector4.Project");
		Vector4 b = args.AsUserData<Vector4>(1, "Vector4.Project");
		return UserData.CreateWithDescriptor(Vector4.Project(a, b), desc);
	}

	[LuaDrivable("Scale")]
	public static DynValue Scale(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector4 a = args.AsUserData<Vector4>(0, "Vector4.Scale");
		Vector4 b = args.AsUserData<Vector4>(1, "Vector4.Scale");
		return UserData.CreateWithDescriptor(Vector4.Scale(a, b), desc);
	}

	[LuaDrivable("SqrMagnitude")]
	public static DynValue SqrMagnitude(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector4 a = args.AsUserData<Vector4>(0, "Vector4.SqrMagnitude");
		return DynValue.NewNumber(Vector4.SqrMagnitude(a));
	}

	[LuaDrivable("__add")]
	private static DynValue __add(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector4 vector = args.AsUserData<Vector4>(0, "Vector4.__add");
		Vector4 vector2 = args.AsUserData<Vector4>(1, "Vector4.__add");
		return UserData.CreateWithDescriptor(vector + vector2, desc);
	}

	[LuaDrivable("__sub")]
	private static DynValue __sub(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector4 vector = args.AsUserData<Vector4>(0, "Vector4.__sub");
		Vector4 vector2 = args.AsUserData<Vector4>(1, "Vector4.__sub");
		return UserData.CreateWithDescriptor(vector - vector2, desc);
	}

	[LuaDrivable("__eq")]
	private static DynValue __eq(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector4 vector = args.AsUserData<Vector4>(0, "Vector4.__eq", allowNil: true);
		Vector4 vector2 = args.AsUserData<Vector4>(1, "Vector4.__eq", allowNil: true);
		return DynValue.NewBoolean(vector == vector2);
	}
}
