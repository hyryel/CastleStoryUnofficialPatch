using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using MoonSharp.Interpreter;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.Lua;

[Serializable]
[JsonObject(MemberSerialization.OptIn)]
public class LuaTimer
{
	private float _startTime;

	[JsonProperty]
	private float _elapsedTime;

	[JsonProperty]
	private bool _running;

	[JsonProperty]
	private bool _initialized;

	[JsonProperty]
	private float _delay;

	[JsonProperty]
	private bool _useUnscaledTime;

	public static LuaObjectDescriptor<LuaTimer> desc = new LuaObjectDescriptor<LuaTimer>();

	private static int charZero = 48;

	private string evenCachedString = new string('\0', 10);

	private string oddCachedString = new string('\0', 10);

	[LuaDrivable("running")]
	public DynValue running
	{
		get
		{
			return (!_running) ? DynValue.False : DynValue.True;
		}
		private set
		{
			bool boolean = value.Boolean;
			if (_running && !boolean)
			{
				_elapsedTime = clr_ElapsedSeconds(this);
			}
			else if (!_running && boolean)
			{
				if (_useUnscaledTime)
				{
					_startTime = Time.unscaledTime - _elapsedTime;
				}
				else
				{
					_startTime = Time.time - _elapsedTime;
				}
			}
			_running = boolean;
		}
	}

	[LuaDrivable("delay")]
	public DynValue delay => DynValue.NewNumber(_delay);

	[LuaDrivable("useUnscaledTime")]
	public DynValue useUnscaledTime
	{
		get
		{
			return (!_useUnscaledTime) ? DynValue.False : DynValue.True;
		}
		set
		{
			_useUnscaledTime = value.Boolean;
		}
	}

	private LuaTimer()
	{
	}

	public LuaTimer(float time, bool unscaledTime = false)
	{
		_useUnscaledTime = unscaledTime;
		_delay = time;
		_startTime = 0f;
	}

	public static void RegisterClass(Script script)
	{
		script.Globals.Set("Timer", desc.GetStaticProxy());
	}

	[OnSerializing]
	internal void OnSerializing(StreamingContext context)
	{
		if (_running)
		{
			_elapsedTime = clr_ElapsedSeconds(this);
		}
	}

	[OnDeserialized]
	internal void OnDeserialized(StreamingContext context)
	{
		if (_useUnscaledTime)
		{
			_startTime = Time.unscaledTime - _elapsedTime;
		}
		else
		{
			_startTime = Time.time - _elapsedTime;
		}
	}

	[LuaDrivable("New")]
	public static DynValue New(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		float time = args.AsFloat(0, "Timer.New");
		bool unscaledTime = args.AsBool(1, "Timer.New");
		return UserData.CreateWithDescriptor(new LuaTimer(time, unscaledTime), desc);
	}

	[LuaDrivable("Set")]
	public static DynValue Set(ScriptExecutionContext context, CallbackArguments args)
	{
		LuaTimer luaTimer = args.AsUserData<LuaTimer>(0, "Timer:Set");
		luaTimer._delay = args.AsFloat(1, "Timer:Set");
		luaTimer._startTime = 0f;
		return DynValue.Void;
	}

	[LuaDrivable("Start")]
	public static DynValue Start(ScriptExecutionContext context, CallbackArguments args)
	{
		LuaTimer luaTimer = args.AsUserData<LuaTimer>(0, "Timer:Start");
		if (args.Count > 1)
		{
			luaTimer._delay = args.AsFloat(1, "Timer:Start");
		}
		if (luaTimer._useUnscaledTime)
		{
			luaTimer._startTime = Time.unscaledTime;
		}
		else
		{
			luaTimer._startTime = Time.time;
		}
		luaTimer._initialized = true;
		luaTimer.running = DynValue.True;
		return DynValue.Void;
	}

	[LuaDrivable("Stop")]
	public static DynValue Stop(ScriptExecutionContext context, CallbackArguments args)
	{
		LuaTimer luaTimer = args.AsUserData<LuaTimer>(0, "Timer:Stop");
		luaTimer.running = DynValue.False;
		return DynValue.Void;
	}

	[LuaDrivable("Check")]
	public static DynValue Check(ScriptExecutionContext context, CallbackArguments args)
	{
		LuaTimer luaTimer = args.AsUserData<LuaTimer>(0, "Timer:Check");
		if (!luaTimer._initialized)
		{
			return DynValue.False;
		}
		if (luaTimer._useUnscaledTime)
		{
			if (Time.unscaledTime >= luaTimer._startTime + luaTimer._delay)
			{
				return DynValue.True;
			}
			return DynValue.False;
		}
		if (Time.time >= luaTimer._startTime + luaTimer._delay)
		{
			return DynValue.True;
		}
		return DynValue.False;
	}

	public static float clr_ElapsedSeconds(LuaTimer timer)
	{
		if (timer._running)
		{
			if (timer._useUnscaledTime)
			{
				return Time.unscaledTime - timer._startTime;
			}
			return Time.time - timer._startTime;
		}
		return timer._elapsedTime;
	}

	private unsafe static string AssignTimeToString(LuaTimer self, int time)
	{
		int num = time % 60;
		int num2 = num % 10;
		int num3 = (num - num2) / 10;
		char c = (char)(num2 + charZero);
		char c2 = (char)(num3 + charZero);
		if (self.evenCachedString == null)
		{
			self.evenCachedString = new string('\0', 10);
			self.oddCachedString = new string('\0', 10);
		}
		string text = ((num2 % 2 != 0) ? self.oddCachedString : self.evenCachedString);
		if (time < 3600)
		{
			int num4 = time / 60;
			num2 = num4 % 10;
			num3 = (num4 - num2) / 10;
			char c3 = (char)(num2 + charZero);
			char c4 = (char)(num3 + charZero);
			fixed (string text2 = text)
			{
				char* ptr = (char*)((nint)text2 + RuntimeHelpers.OffsetToStringData);
				*ptr = c4;
				ptr[1] = c3;
				ptr[2] = ':';
				ptr[3] = c2;
				ptr[4] = c;
				ptr[5] = '\0';
			}
		}
		else
		{
			int num4 = time / 60 % 60;
			num2 = num4 % 10;
			num3 = (num4 - num2) / 10;
			char c5 = (char)(num2 + charZero);
			char c6 = (char)(num3 + charZero);
			int num5 = time / 3600;
			num2 = num5 % 10;
			num3 = (num5 - num2) / 10;
			char c7 = (char)(num2 + charZero);
			char c8 = (char)(num3 + charZero);
			fixed (string text3 = text)
			{
				char* ptr2 = (char*)((nint)text3 + RuntimeHelpers.OffsetToStringData);
				*ptr2 = c8;
				ptr2[1] = c7;
				ptr2[2] = ':';
				ptr2[3] = c6;
				ptr2[4] = c5;
				ptr2[5] = ':';
				ptr2[6] = c2;
				ptr2[7] = c;
				ptr2[8] = '\0';
			}
		}
		return text;
	}

	public static float clr_RemainingSeconds(LuaTimer timer)
	{
		return Mathf.Max(0f, timer._delay - clr_ElapsedSeconds(timer));
	}

	[LuaDrivable("ElapsedSeconds")]
	public static DynValue ElapsedSeconds(ScriptExecutionContext context, CallbackArguments args)
	{
		LuaTimer timer = args.AsUserData<LuaTimer>(0, "Timer:ElapsedSeconds");
		return DynValue.NewNumber(clr_ElapsedSeconds(timer));
	}

	[LuaDrivable("RemainingSeconds")]
	public static DynValue RemainingSeconds(ScriptExecutionContext context, CallbackArguments args)
	{
		LuaTimer timer = args.AsUserData<LuaTimer>(0, "Timer:RemainingSeconds");
		return DynValue.NewNumber(clr_RemainingSeconds(timer));
	}

	[LuaDrivable("FormatedElapsedSeconds")]
	public static DynValue FormatedElapsedSeconds(ScriptExecutionContext context, CallbackArguments args)
	{
		LuaTimer luaTimer = args.AsUserData<LuaTimer>(0, "Timer:FormatedElapsedSeconds");
		float num = ((args.Count < 2) ? 0f : args.AsFloat(1, "Timer:FormatedElapsedSeconds"));
		int time = Mathf.FloorToInt(clr_ElapsedSeconds(luaTimer) + num);
		return DynValue.NewString(AssignTimeToString(luaTimer, time));
	}

	[LuaDrivable("FormatedRemainingSeconds")]
	public static DynValue FormatedRemainingSeconds(ScriptExecutionContext context, CallbackArguments args)
	{
		LuaTimer luaTimer = args.AsUserData<LuaTimer>(0, "Timer:FormatedRemainingSeconds");
		int time = Mathf.FloorToInt(clr_RemainingSeconds(luaTimer));
		return DynValue.NewString(AssignTimeToString(luaTimer, time));
	}

	[LuaDrivable("PercentDone")]
	public static DynValue PercentDone(ScriptExecutionContext context, CallbackArguments args)
	{
		LuaTimer luaTimer = args.AsUserData<LuaTimer>(0, "Timer:PercentDone");
		float num = clr_ElapsedSeconds(luaTimer) / luaTimer._delay;
		return DynValue.NewNumber((!(num > 1f)) ? num : 1f);
	}

	[LuaDrivable("Reset")]
	public static DynValue Reset(ScriptExecutionContext context, CallbackArguments args)
	{
		LuaTimer luaTimer = args.AsUserData<LuaTimer>(0, "Timer:Reset");
		if (luaTimer._useUnscaledTime)
		{
			luaTimer._startTime = Time.unscaledTime;
		}
		else
		{
			luaTimer._startTime = Time.time;
		}
		return DynValue.Void;
	}
}
