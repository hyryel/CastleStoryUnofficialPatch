using System;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Brix.Lua;

public abstract class LuaMath
{
	private static LuaObjectDescriptor<LuaMath> _desc = new LuaObjectDescriptor<LuaMath>();

	[LuaDrivable("deg2Rad")]
	public static float deg2Rad => (float)Math.PI / 180f;

	[LuaDrivable("epsilon")]
	public static float epsilon => Mathf.Epsilon;

	[LuaDrivable("infinity")]
	public static float infinity => float.PositiveInfinity;

	[LuaDrivable("negativeInfinity")]
	public static float negativeInfinity => float.NegativeInfinity;

	[LuaDrivable("pi")]
	public static float pi => (float)Math.PI;

	[LuaDrivable("rad2Deg")]
	public static float rad2Deg => 57.29578f;

	public static void RegisterClass(Script script)
	{
		script.Globals.Set("Math", _desc.GetStaticProxy());
	}

	[LuaDrivable("Floor")]
	public static DynValue Floor(ScriptExecutionContext context, CallbackArguments args)
	{
		float f = args.AsFloat(0, "Math.Floor");
		return DynValue.NewNumber(Mathf.Floor(f));
	}

	[LuaDrivable("FloorToInt")]
	public static DynValue FloorToInt(ScriptExecutionContext context, CallbackArguments args)
	{
		float f = args.AsFloat(0, "Math.FloorToInt");
		return DynValue.NewNumber(Mathf.FloorToInt(f));
	}

	[LuaDrivable("Ceil")]
	public static DynValue Ceil(ScriptExecutionContext context, CallbackArguments args)
	{
		float f = args.AsFloat(0, "Math.Ceil");
		return DynValue.NewNumber(Mathf.Ceil(f));
	}

	[LuaDrivable("CeilToInt")]
	public static DynValue CeilToInt(ScriptExecutionContext context, CallbackArguments args)
	{
		float f = args.AsFloat(0, "Math.CeilToInt");
		return DynValue.NewNumber(Mathf.CeilToInt(f));
	}

	[LuaDrivable("Round")]
	public static DynValue Round(ScriptExecutionContext context, CallbackArguments args)
	{
		float f = args.AsFloat(0, "Math.Round");
		return DynValue.NewNumber(Mathf.Round(f));
	}

	[LuaDrivable("RoundToInt")]
	public static DynValue RoundToInt(ScriptExecutionContext context, CallbackArguments args)
	{
		float f = args.AsFloat(0, "Math.RoundToInt");
		return DynValue.NewNumber(Mathf.RoundToInt(f));
	}

	[LuaDrivable("Abs")]
	public static DynValue Abs(ScriptExecutionContext context, CallbackArguments args)
	{
		float f = args.AsFloat(0, "Math.Abs");
		return DynValue.NewNumber(Mathf.Abs(f));
	}

	[LuaDrivable("Acos")]
	public static DynValue Acos(ScriptExecutionContext context, CallbackArguments args)
	{
		float f = args.AsFloat(0, "Math.Acos");
		return DynValue.NewNumber(Mathf.Acos(f));
	}

	[LuaDrivable("Approximately")]
	public static DynValue Approximately(ScriptExecutionContext context, CallbackArguments args)
	{
		float a = args.AsFloat(0, "Math.Approximately");
		float b = args.AsFloat(1, "Math.Approximately");
		return DynValue.NewBoolean(Mathf.Approximately(a, b));
	}

	[LuaDrivable("Asin")]
	public static DynValue Asin(ScriptExecutionContext context, CallbackArguments args)
	{
		float f = args.AsFloat(0, "Math.Asin");
		return DynValue.NewNumber(Mathf.Asin(f));
	}

	[LuaDrivable("Atan")]
	public static DynValue Atan(ScriptExecutionContext context, CallbackArguments args)
	{
		float f = args.AsFloat(0, "Math.Atan");
		return DynValue.NewNumber(Mathf.Atan(f));
	}

	[LuaDrivable("Atan2")]
	public static DynValue Atan2(ScriptExecutionContext context, CallbackArguments args)
	{
		float y = args.AsFloat(0, "Math.Atan2");
		float x = args.AsFloat(1, "Math.Atan2");
		return DynValue.NewNumber(Mathf.Atan2(y, x));
	}

	[LuaDrivable("Clamp")]
	public static DynValue Clamp(ScriptExecutionContext context, CallbackArguments args)
	{
		float value = args.AsFloat(0, "Math.Clamp");
		float min = args.AsFloat(1, "Math.Clamp");
		float max = args.AsFloat(2, "Math.Clamp");
		return DynValue.NewNumber(Mathf.Clamp(value, min, max));
	}

	[LuaDrivable("Clamp01")]
	public static DynValue Clamp01(ScriptExecutionContext context, CallbackArguments args)
	{
		float value = args.AsFloat(0, "Math.Clamp01");
		return DynValue.NewNumber(Mathf.Clamp01(value));
	}

	[LuaDrivable("ClosestPowerOfTwo")]
	public static DynValue ClosestPowerOfTwo(ScriptExecutionContext context, CallbackArguments args)
	{
		int value = args.AsInt(0, "Math.ClosestPowerOfTwo");
		return DynValue.NewNumber(Mathf.ClosestPowerOfTwo(value));
	}

	[LuaDrivable("Cos")]
	public static DynValue Cos(ScriptExecutionContext context, CallbackArguments args)
	{
		float f = args.AsFloat(0, "Math.Cos");
		return DynValue.NewNumber(Mathf.Cos(f));
	}

	[LuaDrivable("DeltaAngle")]
	public static DynValue DeltaAngle(ScriptExecutionContext context, CallbackArguments args)
	{
		float current = args.AsFloat(0, "Math.DeltaAngle");
		float target = args.AsFloat(1, "Math.DeltaAngle");
		return DynValue.NewNumber(Mathf.DeltaAngle(current, target));
	}

	[LuaDrivable("Exp")]
	public static DynValue Exp(ScriptExecutionContext context, CallbackArguments args)
	{
		float power = args.AsFloat(0, "Math.Exp");
		return DynValue.NewNumber(Mathf.Exp(power));
	}

	[LuaDrivable("Gamma")]
	public static DynValue Gamma(ScriptExecutionContext context, CallbackArguments args)
	{
		float value = args.AsFloat(0, "Math.Gamma");
		float absmax = args.AsFloat(1, "Math.Gamma");
		float gamma = args.AsFloat(2, "Math.Gamma");
		return DynValue.NewNumber(Mathf.Gamma(value, absmax, gamma));
	}

	[LuaDrivable("GammaToLinearSpace")]
	public static DynValue GammaToLinearSpace(ScriptExecutionContext context, CallbackArguments args)
	{
		float value = args.AsFloat(0, "Math.GammaToLinearSpace");
		return DynValue.NewNumber(Mathf.GammaToLinearSpace(value));
	}

	[LuaDrivable("InverseLerp")]
	public static DynValue InverseLerp(ScriptExecutionContext context, CallbackArguments args)
	{
		float a = args.AsFloat(0, "Math.InverseLerp");
		float b = args.AsFloat(1, "Math.InverseLerp");
		float value = args.AsFloat(2, "Math.InverseLerp");
		return DynValue.NewNumber(Mathf.InverseLerp(a, b, value));
	}

	[LuaDrivable("IsPowerOfTwo")]
	public static DynValue IsPowerOfTwo(ScriptExecutionContext context, CallbackArguments args)
	{
		int value = args.AsInt(0, "Math.IsPowerOfTwo");
		return DynValue.NewBoolean(Mathf.IsPowerOfTwo(value));
	}

	[LuaDrivable("Lerp")]
	public static DynValue Lerp(ScriptExecutionContext context, CallbackArguments args)
	{
		float a = args.AsFloat(0, "Math.Lerp");
		float b = args.AsFloat(1, "Math.Lerp");
		float t = args.AsFloat(2, "Math.Lerp");
		return DynValue.NewNumber(Mathf.Lerp(a, b, t));
	}

	[LuaDrivable("LerpAngle")]
	public static DynValue LerpAngle(ScriptExecutionContext context, CallbackArguments args)
	{
		float a = args.AsFloat(0, "Math.LerpAngle");
		float b = args.AsFloat(1, "Math.LerpAngle");
		float t = args.AsFloat(2, "Math.LerpAngle");
		return DynValue.NewNumber(Mathf.LerpAngle(a, b, t));
	}

	[LuaDrivable("LinearToGammaSpace")]
	public static DynValue LinearToGammaSpace(ScriptExecutionContext context, CallbackArguments args)
	{
		float value = args.AsFloat(0, "Math.LinearToGammaSpace");
		return DynValue.NewNumber(Mathf.LinearToGammaSpace(value));
	}

	[LuaDrivable("Log")]
	public static DynValue Log(ScriptExecutionContext context, CallbackArguments args)
	{
		float f = args.AsFloat(0, "Math.Log");
		if (args.Count == 2)
		{
			float p = args.AsFloat(1, "Math.Log");
			return DynValue.NewNumber(Mathf.Log(f, p));
		}
		return DynValue.NewNumber(Mathf.Log(f));
	}

	[LuaDrivable("Log10")]
	public static DynValue Log10(ScriptExecutionContext context, CallbackArguments args)
	{
		float f = args.AsFloat(0, "Math.Log10");
		return DynValue.NewNumber(Mathf.Log10(f));
	}

	[LuaDrivable("Max")]
	public static DynValue Max(ScriptExecutionContext context, CallbackArguments args)
	{
		if (args.Count == 1)
		{
			Table table = args.AsTable(0, "Math.Max");
			float[] array = new float[table.Length];
			int num = 0;
			foreach (DynValue value in table.Values)
			{
				array[num] = (float)value.Number;
				num++;
			}
			return DynValue.NewNumber(Mathf.Max(array));
		}
		float a = args.AsFloat(0, "Math.Max");
		float b = args.AsFloat(1, "Math.Max");
		return DynValue.NewNumber(Mathf.Max(a, b));
	}

	[LuaDrivable("Min")]
	public static DynValue Min(ScriptExecutionContext context, CallbackArguments args)
	{
		if (args.Count == 1)
		{
			Table table = args.AsTable(0, "Math.Min");
			float[] array = new float[table.Length];
			int num = 0;
			foreach (DynValue value in table.Values)
			{
				array[num] = (float)value.Number;
				num++;
			}
			return DynValue.NewNumber(Mathf.Min(array));
		}
		float a = args.AsFloat(0, "Math.Min");
		float b = args.AsFloat(1, "Math.Min");
		return DynValue.NewNumber(Mathf.Min(a, b));
	}

	[LuaDrivable("MoveTowards")]
	public static DynValue MoveTowards(ScriptExecutionContext context, CallbackArguments args)
	{
		float current = args.AsFloat(0, "Math.MoveTowards");
		float target = args.AsFloat(1, "Math.MoveTowards");
		float maxDelta = args.AsFloat(2, "Math.MoveTowards");
		return DynValue.NewNumber(Mathf.MoveTowards(current, target, maxDelta));
	}

	[LuaDrivable("MoveTowardsAngle")]
	public static DynValue MoveTowardsAngle(ScriptExecutionContext context, CallbackArguments args)
	{
		float current = args.AsFloat(0, "Math.MoveTowardsAngle");
		float target = args.AsFloat(1, "Math.MoveTowardsAngle");
		float maxDelta = args.AsFloat(2, "Math.MoveTowardsAngle");
		return DynValue.NewNumber(Mathf.MoveTowardsAngle(current, target, maxDelta));
	}

	[LuaDrivable("NextPowerOfTwo")]
	public static DynValue NextPowerOfTwo(ScriptExecutionContext context, CallbackArguments args)
	{
		int value = args.AsInt(0, "Math.NextPowerOfTwo");
		return DynValue.NewNumber(Mathf.NextPowerOfTwo(value));
	}

	[LuaDrivable("PerlinNoise")]
	public static DynValue PerlinNoise(ScriptExecutionContext context, CallbackArguments args)
	{
		float x = args.AsFloat(0, "Math.PerlinNoise");
		float y = args.AsFloat(1, "Math.PerlinNoise");
		return DynValue.NewNumber(Mathf.PerlinNoise(x, y));
	}

	[LuaDrivable("PingPong")]
	public static DynValue PingPong(ScriptExecutionContext context, CallbackArguments args)
	{
		float t = args.AsFloat(0, "Math.PingPong");
		float length = args.AsFloat(1, "Math.PingPong");
		return DynValue.NewNumber(Mathf.PingPong(t, length));
	}

	[LuaDrivable("Pow")]
	public static DynValue Pow(ScriptExecutionContext context, CallbackArguments args)
	{
		float f = args.AsFloat(0, "Math.Pow");
		float p = args.AsFloat(1, "Math.Pow");
		return DynValue.NewNumber(Mathf.Pow(f, p));
	}

	[LuaDrivable("Repeat")]
	public static DynValue Repeat(ScriptExecutionContext context, CallbackArguments args)
	{
		float t = args.AsFloat(0, "Math.Repeat");
		float length = args.AsFloat(1, "Math.Repeat");
		return DynValue.NewNumber(Mathf.Repeat(t, length));
	}

	[LuaDrivable("Sign")]
	public static DynValue Sign(ScriptExecutionContext context, CallbackArguments args)
	{
		float f = args.AsFloat(0, "Math.Sign");
		return DynValue.NewNumber(Mathf.Sign(f));
	}

	[LuaDrivable("Sin")]
	public static DynValue Sin(ScriptExecutionContext context, CallbackArguments args)
	{
		float f = args.AsFloat(0, "Math.Sin");
		return DynValue.NewNumber(Mathf.Sin(f));
	}

	[LuaDrivable("SmoothStep")]
	public static DynValue SmoothStep(ScriptExecutionContext context, CallbackArguments args)
	{
		float num = args.AsFloat(0, "Math.SmoothStep");
		float to = args.AsFloat(1, "Math.SmoothStep");
		float t = args.AsFloat(2, "Math.SmoothStep");
		return DynValue.NewNumber(Mathf.SmoothStep(num, to, t));
	}

	[LuaDrivable("Sqrt")]
	public static DynValue Sqrt(ScriptExecutionContext context, CallbackArguments args)
	{
		float f = args.AsFloat(0, "Math.Sqrt");
		return DynValue.NewNumber(Mathf.Sqrt(f));
	}

	[LuaDrivable("Tan")]
	public static DynValue Tan(ScriptExecutionContext context, CallbackArguments args)
	{
		float f = args.AsFloat(0, "Math.Tan");
		return DynValue.NewNumber(Mathf.Tan(f));
	}
}
