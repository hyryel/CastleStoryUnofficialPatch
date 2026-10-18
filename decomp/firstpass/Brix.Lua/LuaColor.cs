using System.Globalization;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using UnityEngine;

namespace Brix.Lua;

public abstract class LuaColor
{
	public static LuaObjectDescriptor<Color> desc = new LuaObjectDescriptor<Color>(typeof(LuaColor));

	private static DynValue _black = UserData.CreateWithDescriptor(Color.black, desc);

	private static DynValue _blue = UserData.CreateWithDescriptor(Color.blue, desc);

	private static DynValue _clear = UserData.CreateWithDescriptor(Color.clear, desc);

	private static DynValue _cyan = UserData.CreateWithDescriptor(Color.cyan, desc);

	private static DynValue _gray = UserData.CreateWithDescriptor(Color.gray, desc);

	private static DynValue _darkGrey = UserData.CreateWithDescriptor(new Color(0.35f, 0.35f, 0.35f, 1f), desc);

	private static DynValue _darkGray = UserData.CreateWithDescriptor(new Color(0.35f, 0.35f, 0.35f, 1f), desc);

	private static DynValue _grey = UserData.CreateWithDescriptor(Color.grey, desc);

	private static DynValue _green = UserData.CreateWithDescriptor(Color.green, desc);

	private static DynValue _magenta = UserData.CreateWithDescriptor(Color.magenta, desc);

	private static DynValue _red = UserData.CreateWithDescriptor(Color.red, desc);

	private static DynValue _white = UserData.CreateWithDescriptor(Color.white, desc);

	private static DynValue _yellow = UserData.CreateWithDescriptor(Color.yellow, desc);

	private static DynValue _csYellow = UserData.CreateWithDescriptor(new Color(1f, 0.77254903f, 2f / 51f, 1f), desc);

	[LuaDrivable("r", UseExtern = true)]
	public float r;

	[LuaDrivable("g", UseExtern = true)]
	public float g;

	[LuaDrivable("b", UseExtern = true)]
	public float b;

	[LuaDrivable("a", UseExtern = true)]
	public float a;

	[LuaDrivable("black")]
	public static DynValue black => _black;

	[LuaDrivable("blue")]
	public static DynValue blue => _blue;

	[LuaDrivable("clear")]
	public static DynValue clear => _clear;

	[LuaDrivable("cyan")]
	public static DynValue cyan => _cyan;

	[LuaDrivable("gray")]
	public static DynValue gray => _gray;

	[LuaDrivable("darkGrey")]
	public static DynValue darkGrey => _darkGrey;

	[LuaDrivable("darkGray")]
	public static DynValue darkGray => _darkGray;

	[LuaDrivable("grey")]
	public static DynValue grey => _grey;

	[LuaDrivable("green")]
	public static DynValue green => _green;

	[LuaDrivable("magenta")]
	public static DynValue magenta => _magenta;

	[LuaDrivable("red")]
	public static DynValue red => _red;

	[LuaDrivable("white")]
	public static DynValue white => _white;

	[LuaDrivable("yellow")]
	public static DynValue yellow => _yellow;

	[LuaDrivable("castleYellow")]
	public static DynValue castleYellow => _csYellow;

	[LuaDrivable("gamma", UseExtern = true)]
	public extern Color gamma { get; set; }

	[LuaDrivable("linear", UseExtern = true)]
	public extern Color linear { get; set; }

	[LuaDrivable("grayscale", UseExtern = true)]
	public extern float grayscale { get; set; }

	public static void RegisterClass(Script script)
	{
		script.Globals.Set("Color", desc.GetStaticProxy());
	}

	[LuaDrivable("New")]
	public static DynValue New(ScriptExecutionContext context, CallbackArguments args)
	{
		float num = args.AsFloat(0, "Color.New");
		float num2 = args.AsFloat(1, "Color.New");
		float num3 = args.AsFloat(2, "Color.New");
		float num4 = args.AsFloat(3, "Color.New");
		return UserData.CreateWithDescriptor(new Color(num, num2, num3, num4), desc);
	}

	[LuaDrivable("NewHex")]
	public static DynValue NewHex(ScriptExecutionContext context, CallbackArguments args)
	{
		string text = args.AsString(0, "Color.NewHex");
		text = text.Replace("0x", string.Empty);
		text = text.Replace("#", string.Empty);
		byte b = byte.MaxValue;
		byte b2 = byte.Parse(text.Substring(0, 2), NumberStyles.HexNumber);
		byte b3 = byte.Parse(text.Substring(2, 2), NumberStyles.HexNumber);
		byte b4 = byte.Parse(text.Substring(4, 2), NumberStyles.HexNumber);
		if (text.Length == 8)
		{
			b = byte.Parse(text.Substring(4, 2), NumberStyles.HexNumber);
		}
		return UserData.CreateWithDescriptor((Color)new Color32(b2, b3, b4, b), (IUserDataDescriptor)desc);
	}

	[LuaDrivable("Lerp")]
	public static DynValue Lerp(ScriptExecutionContext context, CallbackArguments args)
	{
		Color color = args.AsUserData<Color>(0, "Color.Lerp");
		Color color2 = args.AsUserData<Color>(1, "Color.Lerp");
		float t = args.AsFloat(2, "Color.Lerp");
		return UserData.CreateWithDescriptor(Color.Lerp(color, color2, t), desc);
	}

	[LuaDrivable("ToHex")]
	public static DynValue ToHex(ScriptExecutionContext context, CallbackArguments args)
	{
		Color32 color = args.AsUserData<Color>(0, "Color:ToHex");
		return DynValue.NewNumber((color.r << 0) | (color.g << 16) | (color.b << 8) | color.a);
	}

	[LuaDrivable("ToHexString")]
	public static DynValue ToHexString(ScriptExecutionContext context, CallbackArguments args)
	{
		Color32 color = args.AsUserData<Color>(0, "Color:ToHexString");
		return DynValue.NewString("#" + color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2") + color.a.ToString("X2"));
	}

	[LuaDrivable("ColorString")]
	public static DynValue ColorString(ScriptExecutionContext context, CallbackArguments args)
	{
		Color32 color = args.AsUserData<Color>(0, "Color:ColorString");
		string text = args.AsString(1, "Color:ColorString");
		return DynValue.NewString("<color=#" + color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2") + color.a.ToString("X2") + ">" + text + "</color>");
	}

	[LuaDrivable("__add")]
	public static DynValue __add(ScriptExecutionContext context, CallbackArguments args)
	{
		Color color = args.AsUserData<Color>(0, "Color.__add");
		Color color2 = args.AsUserData<Color>(1, "Color.__add");
		return UserData.CreateWithDescriptor(color + color2, desc);
	}

	[LuaDrivable("__sub")]
	public static DynValue __sub(ScriptExecutionContext context, CallbackArguments args)
	{
		Color color = args.AsUserData<Color>(0, "Color.__sub");
		Color color2 = args.AsUserData<Color>(1, "Color.__sub");
		return UserData.CreateWithDescriptor(color - color2, desc);
	}

	[LuaDrivable("__eq")]
	public static DynValue __eq(ScriptExecutionContext context, CallbackArguments args)
	{
		Color color = args.AsUserData<Color>(0, "Color.__eq", allowNil: true);
		Color color2 = args.AsUserData<Color>(1, "Color.__eq", allowNil: true);
		return DynValue.NewBoolean(color == color2);
	}
}
