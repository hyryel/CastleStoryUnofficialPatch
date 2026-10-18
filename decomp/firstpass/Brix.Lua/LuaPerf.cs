using System.Diagnostics;
using MoonSharp.Interpreter;

namespace Brix.Lua;

public class LuaPerf
{
	private static Stopwatch _sw = new Stopwatch();

	private static int _compensation = 0;

	private static LuaObjectDescriptor<LuaPerf> _desc = new LuaObjectDescriptor<LuaPerf>();

	public static void RegisterClass(Script script)
	{
		script.Globals["Perf"] = _desc.GetStaticProxy();
	}

	[LuaDrivable("AdjustCompensation")]
	public static DynValue AdjustCompensation(ScriptExecutionContext context, CallbackArguments args)
	{
		_compensation = args.AsInt(0, "AdjustCompensation");
		return DynValue.Void;
	}

	[LuaDrivable("Start")]
	public static DynValue Start(ScriptExecutionContext context, CallbackArguments args)
	{
		_sw.Start();
		return DynValue.Void;
	}

	[LuaDrivable("Reset")]
	public static DynValue Reset(ScriptExecutionContext context, CallbackArguments args)
	{
		_sw.Reset();
		return DynValue.Void;
	}

	[LuaDrivable("Stop")]
	public static DynValue Stop(ScriptExecutionContext context, CallbackArguments args)
	{
		_sw.Stop();
		return DynValue.Void;
	}

	[LuaDrivable("ElapsedSeconds")]
	public static DynValue ElapsedSeconds(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewNumber(((double)_sw.ElapsedTicks - (double)_compensation) / (double)Stopwatch.Frequency);
	}

	[LuaDrivable("ElapsedMilliseconds")]
	public static DynValue ElapsedMilliseconds(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewNumber(((double)_sw.ElapsedTicks - (double)_compensation) / (double)Stopwatch.Frequency * 1000.0);
	}

	[LuaDrivable("ElapsedNanoseconds")]
	public static DynValue ElapsedNanoseconds(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewNumber(((double)_sw.ElapsedTicks - (double)_compensation) / (double)Stopwatch.Frequency * 1000000000.0);
	}

	[LuaDrivable("ElapsedTicks")]
	public static DynValue ElapsedTicks(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewNumber(_sw.ElapsedTicks);
	}

	[LuaDrivable("Frequency")]
	public static DynValue Frequency(ScriptExecutionContext context, CallbackArguments args)
	{
		return DynValue.NewNumber(Stopwatch.Frequency);
	}
}
