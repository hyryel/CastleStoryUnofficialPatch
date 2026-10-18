using MoonSharp.Interpreter;
using UnityEngine;

namespace Brix.Lua;

public class LuaVector2
{
	public static LuaObjectDescriptor<Vector2> desc = new LuaObjectDescriptor<Vector2>(typeof(LuaVector2));

	private static DynValue _up = UserData.CreateWithDescriptor(Vector2.up, desc);

	private static DynValue _down = UserData.CreateWithDescriptor(Vector2.down, desc);

	private static DynValue _left = UserData.CreateWithDescriptor(Vector2.left, desc);

	private static DynValue _right = UserData.CreateWithDescriptor(Vector2.right, desc);

	private static DynValue _zero = UserData.CreateWithDescriptor(Vector2.zero, desc);

	private static DynValue _one = UserData.CreateWithDescriptor(Vector2.one, desc);

	[LuaDrivable("x", UseExtern = true)]
	public float x;

	[LuaDrivable("y", UseExtern = true)]
	public float y;

	[LuaDrivable("up")]
	public static DynValue up => _up;

	[LuaDrivable("down")]
	public static DynValue down => _down;

	[LuaDrivable("left")]
	public static DynValue left => _left;

	[LuaDrivable("right")]
	public static DynValue right => _right;

	[LuaDrivable("zero")]
	public static DynValue zero => _zero;

	[LuaDrivable("one")]
	public static DynValue one => _one;

	[LuaDrivable("kEpsilon")]
	public float kEpsilon => 1E-05f;

	public static void RegisterClass(Script script)
	{
		script.Globals.Set("Vector2", desc.GetStaticProxy());
	}

	[LuaDrivable("New")]
	public static DynValue New(ScriptExecutionContext context, CallbackArguments args)
	{
		float num = args.AsFloat(0, "Vector2.New");
		float num2 = args.AsFloat(1, "Vector2.New");
		return UserData.CreateWithDescriptor(new Vector2(num, num2), desc);
	}

	[LuaDrivable("Angle")]
	public static DynValue Angle(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector2 vector = args.AsUserData<Vector2>(0, "Vector2.Angle");
		Vector2 to = args.AsUserData<Vector2>(1, "Vector2.Angle");
		return DynValue.NewNumber(Vector2.Angle(vector, to));
	}

	[LuaDrivable("ClampMagnitude")]
	public static DynValue ClampMagnitude(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector2 vector = args.AsUserData<Vector2>(0, "Vector2.ClampMagnitude");
		float maxLength = args.AsFloat(1, "Vector2.ClampMagnitude");
		return UserData.CreateWithDescriptor(Vector2.ClampMagnitude(vector, maxLength), desc);
	}

	[LuaDrivable("Dot")]
	public static DynValue Dot(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector2 lhs = args.AsUserData<Vector2>(0, "Vector2.Dot");
		Vector2 rhs = args.AsUserData<Vector2>(1, "Vector2.Dot");
		return DynValue.NewNumber(Vector2.Dot(lhs, rhs));
	}

	[LuaDrivable("Distance")]
	public static DynValue Distance(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector2 a = args.AsUserData<Vector2>(0, "Vector2.Distance");
		Vector2 b = args.AsUserData<Vector2>(1, "Vector2.Distance");
		return DynValue.NewNumber(Vector2.Distance(a, b));
	}

	[LuaDrivable("Lerp")]
	public static DynValue Lerp(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector2 a = args.AsUserData<Vector2>(0, "Vector2.Lerp");
		Vector2 b = args.AsUserData<Vector2>(1, "Vector2.Lerp");
		float t = args.AsFloat(2, "Vector2.Lerp");
		return UserData.CreateWithDescriptor(Vector2.Lerp(a, b, t), desc);
	}

	[LuaDrivable("Max")]
	public static DynValue Max(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector2 lhs = args.AsUserData<Vector2>(0, "Vector2.Max");
		Vector2 rhs = args.AsUserData<Vector2>(1, "Vector2.Max");
		return UserData.CreateWithDescriptor(Vector2.Max(lhs, rhs), desc);
	}

	[LuaDrivable("Min")]
	public static DynValue Min(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector2 lhs = args.AsUserData<Vector2>(0, "Vector2.Min");
		Vector2 rhs = args.AsUserData<Vector2>(1, "Vector2.Min");
		return UserData.CreateWithDescriptor(Vector2.Min(lhs, rhs), desc);
	}

	[LuaDrivable("MoveTowards")]
	public static DynValue MoveTowards(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector2 current = args.AsUserData<Vector2>(0, "Vector2.MoveTowards");
		Vector2 target = args.AsUserData<Vector2>(1, "Vector2.MoveTowards");
		float maxDistanceDelta = args.AsFloat(2, "Vector2.MoveTowards");
		return UserData.CreateWithDescriptor(Vector2.MoveTowards(current, target, maxDistanceDelta), desc);
	}

	[LuaDrivable("Reflect")]
	public static DynValue Reflect(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector2 inDirection = args.AsUserData<Vector2>(0, "Vector2.Reflect");
		Vector2 inNormal = args.AsUserData<Vector2>(1, "Vector2.Reflect");
		return UserData.CreateWithDescriptor(Vector2.Reflect(inDirection, inNormal), desc);
	}

	[LuaDrivable("Scale")]
	public static DynValue Scale(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector2 a = args.AsUserData<Vector2>(0, "Vector2.Scale");
		Vector2 b = args.AsUserData<Vector2>(1, "Vector2.Scale");
		return UserData.CreateWithDescriptor(Vector2.Scale(a, b), desc);
	}

	[LuaDrivable("SqrMagnitude")]
	public static DynValue SqrMagnitude(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector2 a = args.AsUserData<Vector2>(0, "Vector2.SqrMagnitude");
		return DynValue.NewNumber(Vector2.SqrMagnitude(a));
	}

	[LuaDrivable("__add")]
	private static DynValue __add(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector2 vector = args.AsUserData<Vector2>(0, "Vector2:__add");
		Vector2 vector2 = args.AsUserData<Vector2>(1, "Vector2:__add");
		return UserData.CreateWithDescriptor(vector + vector2, desc);
	}

	[LuaDrivable("__sub")]
	private static DynValue __sub(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector2 vector = args.AsUserData<Vector2>(0, "Vector2:__sub");
		Vector2 vector2 = args.AsUserData<Vector2>(1, "Vector2:__sub");
		return UserData.CreateWithDescriptor(vector - vector2, desc);
	}

	[LuaDrivable("__eq")]
	private static DynValue __eq(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector2 vector = args.AsUserData<Vector2>(0, "Vector2:__eq", allowNil: true);
		Vector2 vector2 = args.AsUserData<Vector2>(1, "Vector2:__eq", allowNil: true);
		return DynValue.NewBoolean(vector == vector2);
	}
}
