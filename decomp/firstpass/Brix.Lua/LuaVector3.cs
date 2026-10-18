using Brix.Engine.Abstract;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Brix.Lua;

public abstract class LuaVector3
{
	public static LuaObjectDescriptor<Vector3> desc = new LuaObjectDescriptor<Vector3>(typeof(LuaVector3));

	private static DynValue _up = UserData.CreateWithDescriptor(Vector3.up, desc);

	private static DynValue _down = UserData.CreateWithDescriptor(Vector3.down, desc);

	private static DynValue _left = UserData.CreateWithDescriptor(Vector3.left, desc);

	private static DynValue _right = UserData.CreateWithDescriptor(Vector3.right, desc);

	private static DynValue _forward = UserData.CreateWithDescriptor(Vector3.forward, desc);

	private static DynValue _back = UserData.CreateWithDescriptor(Vector3.back, desc);

	private static DynValue _zero = UserData.CreateWithDescriptor(Vector3.zero, desc);

	private static DynValue _one = UserData.CreateWithDescriptor(Vector3.one, desc);

	[LuaDrivable("x", UseExtern = true)]
	public float x;

	[LuaDrivable("y", UseExtern = true)]
	public float y;

	[LuaDrivable("z", UseExtern = true)]
	public float z;

	[LuaDrivable("up")]
	public static DynValue up => _up;

	[LuaDrivable("down")]
	public static DynValue down => _down;

	[LuaDrivable("left")]
	public static DynValue left => _left;

	[LuaDrivable("right")]
	public static DynValue right => _right;

	[LuaDrivable("forward")]
	public static DynValue forward => _forward;

	[LuaDrivable("back")]
	public static DynValue back => _back;

	[LuaDrivable("zero")]
	public static DynValue zero => _zero;

	[LuaDrivable("one")]
	public static DynValue one => _one;

	[LuaDrivable("kEpsilon")]
	public float kEpsilon => 1E-05f;

	public static void RegisterClass(Script script)
	{
		script.Globals.Set("Vector3", desc.GetStaticProxy());
	}

	[LuaDrivable("New")]
	public static DynValue New(ScriptExecutionContext context, CallbackArguments args)
	{
		float num = args.AsFloat(0, "Vector3.New");
		float num2 = args.AsFloat(1, "Vector3.New");
		float num3 = args.AsFloat(2, "Vector3.New");
		return UserData.CreateWithDescriptor(new Vector3(num, num2, num3), desc);
	}

	[LuaDrivable("Angle")]
	public static DynValue Angle(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector3 vector = args.AsUserData<Vector3>(0, "Vector3.Angle");
		Vector3 to = args.AsUserData<Vector3>(1, "Vector3.Angle");
		return DynValue.NewNumber(Vector3.Angle(vector, to));
	}

	[LuaDrivable("ClampMagnitude")]
	public static DynValue ClampMagnitude(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector3 vector = args.AsUserData<Vector3>(0, "Vector3.ClampMagnitude");
		float maxLength = args.AsFloat(1, "Vector3.ClampMagnitude");
		return UserData.CreateWithDescriptor(Vector3.ClampMagnitude(vector, maxLength), desc);
	}

	[LuaDrivable("Cross")]
	public static DynValue Cross(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector3 lhs = args.AsUserData<Vector3>(0, "Vector3.Cross");
		Vector3 rhs = args.AsUserData<Vector3>(1, "Vector3.Cross");
		return UserData.CreateWithDescriptor(Vector3.Cross(lhs, rhs), desc);
	}

	[LuaDrivable("Dot")]
	public static DynValue Dot(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector3 lhs = args.AsUserData<Vector3>(0, "Vector3.Dot");
		Vector3 rhs = args.AsUserData<Vector3>(1, "Vector3.Dot");
		return DynValue.NewNumber(Vector3.Dot(lhs, rhs));
	}

	[LuaDrivable("Distance")]
	public static DynValue Distance(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector3 a = args.AsUserData<Vector3>(0, "Vector3.Distance");
		Vector3 b = args.AsUserData<Vector3>(1, "Vector3.Distance");
		return DynValue.NewNumber(Vector3.Distance(a, b));
	}

	[LuaDrivable("ProjectOnPlane")]
	public static DynValue ProjectOnPlane(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector3 vector = args.AsUserData<Vector3>(0, "Vector3.ProjectOnPlane");
		Vector3 planeNormal = args.AsUserData<Vector3>(1, "Vector3.ProjectOnPlane");
		return UserData.CreateWithDescriptor(Vector3.ProjectOnPlane(vector, planeNormal), desc);
	}

	[LuaDrivable("Lerp")]
	public static DynValue Lerp(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector3 a = args.AsUserData<Vector3>(0, "Vector3.Lerp");
		Vector3 b = args.AsUserData<Vector3>(1, "Vector3.Lerp");
		float t = args.AsFloat(2, "Vector3.Lerp");
		return UserData.CreateWithDescriptor(Vector3.Lerp(a, b, t), desc);
	}

	[LuaDrivable("Slerp")]
	public static DynValue Slerp(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector3 a = args.AsUserData<Vector3>(0, "Vector3.Slerp");
		Vector3 b = args.AsUserData<Vector3>(1, "Vector3.Slerp");
		float t = args.AsFloat(2, "Vector3.Slerp");
		return UserData.CreateWithDescriptor(Vector3.Slerp(a, b, t), desc);
	}

	[LuaDrivable("Magnitude")]
	public static DynValue Magnitude(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector3 a = args.AsUserData<Vector3>(0, "Vector3:Magnitude");
		return DynValue.NewNumber(Vector3.Magnitude(a));
	}

	[LuaDrivable("Max")]
	public static DynValue Max(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector3 lhs = args.AsUserData<Vector3>(0, "Vector3.Max");
		Vector3 rhs = args.AsUserData<Vector3>(1, "Vector3.Max");
		return UserData.CreateWithDescriptor(Vector3.Max(lhs, rhs), desc);
	}

	[LuaDrivable("Min")]
	public static DynValue Min(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector3 lhs = args.AsUserData<Vector3>(0, "Vector3.Min");
		Vector3 rhs = args.AsUserData<Vector3>(1, "Vector3.Min");
		return UserData.CreateWithDescriptor(Vector3.Min(lhs, rhs), desc);
	}

	[LuaDrivable("MoveTowards")]
	public static DynValue MoveTowards(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector3 current = args.AsUserData<Vector3>(0, "Vector3.MoveTowards");
		Vector3 target = args.AsUserData<Vector3>(1, "Vector3.MoveTowards");
		float maxDistanceDelta = args.AsFloat(2, "Vector3.MoveTowards");
		return UserData.CreateWithDescriptor(Vector3.MoveTowards(current, target, maxDistanceDelta), desc);
	}

	[LuaDrivable("Normalize")]
	public static DynValue Normalize(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector3 value = args.AsUserData<Vector3>(0, "Vector3:Normalize");
		return UserData.CreateWithDescriptor(Vector3.Normalize(value), desc);
	}

	[LuaDrivable("RotateTowards")]
	public static DynValue RotateTowards(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector3 current = args.AsUserData<Vector3>(0, "Vector3.RotateTowards");
		Vector3 target = args.AsUserData<Vector3>(1, "Vector3.RotateTowards");
		float maxRadiansDelta = args.AsFloat(2, "Vector3.RotateTowards");
		float maxMagnitudeDelta = args.AsFloat(3, "Vector3.RotateTowards");
		return UserData.CreateWithDescriptor(Vector3.RotateTowards(current, target, maxRadiansDelta, maxMagnitudeDelta), desc);
	}

	[LuaDrivable("Project")]
	public static DynValue Project(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector3 vector = args.AsUserData<Vector3>(0, "Vector3.Project");
		Vector3 onNormal = args.AsUserData<Vector3>(1, "Vector3.Project");
		return UserData.CreateWithDescriptor(Vector3.Project(vector, onNormal), desc);
	}

	[LuaDrivable("Reflect")]
	public static DynValue Reflect(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector3 inDirection = args.AsUserData<Vector3>(0, "Vector3.Reflect");
		Vector3 inNormal = args.AsUserData<Vector3>(1, "Vector3.Reflect");
		return UserData.CreateWithDescriptor(Vector3.Reflect(inDirection, inNormal), desc);
	}

	[LuaDrivable("Scale")]
	public static DynValue Scale(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector3 a = args.AsUserData<Vector3>(0, "Vector3.Scale");
		Vector3 b = args.AsUserData<Vector3>(1, "Vector3.Scale");
		return UserData.CreateWithDescriptor(Vector3.Scale(a, b), desc);
	}

	[LuaDrivable("SqrMagnitude")]
	public static DynValue SqrMagnitude(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector3 a = args.AsUserData<Vector3>(0, "Vector3.SqrMagnitude");
		return DynValue.NewNumber(Vector3.SqrMagnitude(a));
	}

	[LuaDrivable("ToScalaire")]
	private static DynValue GetScalaire(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector3 vecteur = args.AsUserData<Vector3>(0, "Vector3.ToScalaire");
		return DynValue.NewNumber(Voxel.GetScalaire(vecteur));
	}

	[LuaDrivable("__add")]
	private static DynValue __add(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector3 vector = args.AsUserData<Vector3>(0, "Vector3.__add");
		Vector3 vector2 = args.AsUserData<Vector3>(1, "Vector3.__add");
		return UserData.CreateWithDescriptor(vector + vector2, desc);
	}

	[LuaDrivable("__sub")]
	private static DynValue __sub(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector3 vector = args.AsUserData<Vector3>(0, "Vector3.__sub");
		Vector3 vector2 = args.AsUserData<Vector3>(1, "Vector3.__sub");
		return UserData.CreateWithDescriptor(vector - vector2, desc);
	}

	[LuaDrivable("__eq")]
	private static DynValue __eq(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector3 vector = args.AsUserData<Vector3>(0, "Vector3.__eq", allowNil: true);
		Vector3 vector2 = args.AsUserData<Vector3>(1, "Vector3.__eq", allowNil: true);
		return DynValue.NewBoolean(vector == vector2);
	}
}
