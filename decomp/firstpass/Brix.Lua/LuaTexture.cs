using System.IO;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using UnityEngine;

namespace Brix.Lua;

public abstract class LuaTexture
{
	private static LuaEnumDescriptor<TextureFormat> _enumTextureFormat = new LuaEnumDescriptor<TextureFormat>();

	public static LuaObjectDescriptor<Texture2D> desc = new LuaObjectDescriptor<Texture2D>(typeof(LuaTexture));

	[LuaDrivable("blackTexture", UseExtern = true)]
	public static extern LuaTexture blackTexture { get; }

	[LuaDrivable("whiteTexture", UseExtern = true)]
	public static extern LuaTexture whiteTexture { get; }

	[LuaDrivable("format", UseExtern = true)]
	public extern TextureFormat format { get; }

	[LuaDrivable("mipmapCount", UseExtern = true)]
	public extern int mipmapCount { get; }

	[LuaDrivable("anisoLevel", UseExtern = true)]
	public extern int anisoLevel { get; set; }

	[LuaDrivable("filterMode", UseExtern = true)]
	public extern FilterMode filterMode { get; set; }

	[LuaDrivable("height", UseExtern = true)]
	public extern int height { get; set; }

	[LuaDrivable("mipMapBias", UseExtern = true)]
	public extern float mipMapBias { get; set; }

	[LuaDrivable("texelSize", UseExtern = true)]
	public extern Vector2 texelSize { get; }

	[LuaDrivable("width", UseExtern = true)]
	public extern int width { get; set; }

	public static void RegisterClass(Script script)
	{
		script.Globals["TextureFormat"] = _enumTextureFormat.CreateDynValue();
		script.Globals["Texture"] = desc.GetStaticProxy();
	}

	[LuaDrivable("New")]
	public static DynValue New(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		string text = args.AsString(0, "Texture.New");
		if (!string.IsNullOrEmpty(text) && !text.Contains("Info/Lua/"))
		{
			text = "Info/Lua/" + text;
		}
		Texture2D texture2D = null;
		if (args.Count > 1 && args[1].Type == DataType.Boolean)
		{
			if (args.AsBool(1, "Texture.New"))
			{
				Texture2D texture2D2 = new Texture2D(2, 2, TextureFormat.RGBA32, mipmap: false, linear: true);
				texture2D2.LoadImage(File.ReadAllBytes(text));
				texture2D = new Texture2D(texture2D2.width, texture2D2.height, TextureFormat.RGBA32, mipmap: false, linear: true);
				Object.Destroy(texture2D2);
				Color[] pixels = texture2D2.GetPixels();
				for (int i = 0; i != pixels.Length; i++)
				{
					pixels[i].a = pixels[i].r;
					pixels[i].r = pixels[i].g;
					pixels[i].b = pixels[i].g;
				}
				texture2D.SetPixels(pixels);
				texture2D.Apply();
			}
		}
		else if (args.Count > 1 && args[1].Type == DataType.Number)
		{
			TextureFormat textureFormat = (TextureFormat)args.AsInt(1, "Texture.New");
			texture2D = new Texture2D(2, 2, textureFormat, mipmap: false);
			texture2D.LoadRawTextureData(File.ReadAllBytes(text));
		}
		if (texture2D == null)
		{
			texture2D = new Texture2D(2, 2);
			texture2D.LoadImage(File.ReadAllBytes(text));
		}
		return UserData.CreateWithDescriptor(texture2D, desc);
	}
}
