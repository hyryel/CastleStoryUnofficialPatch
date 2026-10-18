using MoonSharp.Interpreter;
using UnityEngine;

namespace Brix.Lua;

public abstract class LuaMatrix4x4
{
	private static LuaObjectDescriptor<Matrix4x4> desc = new LuaObjectDescriptor<Matrix4x4>(typeof(LuaMatrix4x4));

	[LuaDrivable("m00", UseExtern = true)]
	public float m00;

	[LuaDrivable("m01", UseExtern = true)]
	public float m01;

	[LuaDrivable("m02", UseExtern = true)]
	public float m02;

	[LuaDrivable("m03", UseExtern = true)]
	public float m03;

	[LuaDrivable("m10", UseExtern = true)]
	public float m10;

	[LuaDrivable("m11", UseExtern = true)]
	public float m11;

	[LuaDrivable("m12", UseExtern = true)]
	public float m12;

	[LuaDrivable("m13", UseExtern = true)]
	public float m13;

	[LuaDrivable("m20", UseExtern = true)]
	public float m20;

	[LuaDrivable("m21", UseExtern = true)]
	public float m21;

	[LuaDrivable("m22", UseExtern = true)]
	public float m22;

	[LuaDrivable("m23", UseExtern = true)]
	public float m23;

	[LuaDrivable("m30", UseExtern = true)]
	public float m30;

	[LuaDrivable("m31", UseExtern = true)]
	public float m31;

	[LuaDrivable("m32", UseExtern = true)]
	public float m32;

	[LuaDrivable("m33", UseExtern = true)]
	public float m33;

	[LuaDrivable("identity", UseExtern = true)]
	public static extern Matrix4x4 identity { get; }

	public static void RegisterClass(Script script)
	{
		script.Globals.Set("Matrix4x4", desc.GetStaticProxy());
	}

	[LuaDrivable("New")]
	public static DynValue New(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		if (args.Count > 0 && args[0].Type == DataType.Table)
		{
			Table table = args.AsTable(0, "Matrix4x4.New");
			return UserData.CreateWithDescriptor(new Matrix4x4
			{
				m00 = (float)table.Get(1).Number,
				m01 = (float)table.Get(2).Number,
				m02 = (float)table.Get(3).Number,
				m03 = (float)table.Get(4).Number,
				m10 = (float)table.Get(5).Number,
				m11 = (float)table.Get(6).Number,
				m12 = (float)table.Get(7).Number,
				m13 = (float)table.Get(8).Number,
				m20 = (float)table.Get(9).Number,
				m21 = (float)table.Get(10).Number,
				m22 = (float)table.Get(11).Number,
				m23 = (float)table.Get(12).Number,
				m30 = (float)table.Get(13).Number,
				m31 = (float)table.Get(14).Number,
				m32 = (float)table.Get(15).Number,
				m33 = (float)table.Get(16).Number
			}, desc);
		}
		return UserData.CreateWithDescriptor(default(Matrix4x4), desc);
	}

	[LuaDrivable("TRS")]
	public static DynValue TRS(ScriptExecutionContext context, CallbackArguments args)
	{
		Vector3 pos = args.AsUserData<Vector3>(0, "Matrix4x4.TRS");
		Quaternion q = args.AsUserData<Quaternion>(1, "Matrix4x4.TRS");
		Vector3 s = args.AsUserData<Vector3>(2, "Matrix4x4.TRS");
		return UserData.CreateWithDescriptor(Matrix4x4.TRS(pos, q, s), desc);
	}
}
