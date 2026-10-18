using Brix.External.Factories;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Brix.Lua;

public abstract class LuaMaterial
{
	public static LuaObjectDescriptor<Material> desc = new LuaObjectDescriptor<Material>(typeof(LuaMaterial));

	public static void RegisterClass(Script script)
	{
		script.Globals["Material"] = desc.GetStaticProxy();
	}

	[LuaDrivable("New")]
	public static DynValue New(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		if (args.Count > 0)
		{
			string name = args.AsString(0, "Material.New");
			return UserData.CreateWithDescriptor(new Material(Shader.Find(name)), desc);
		}
		return UserData.CreateWithDescriptor(new Material(Shader.Find("Diffuse")), desc);
	}

	[LuaDrivable("SetOnAssetKey")]
	public static DynValue SetMatrialOnKey(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		Factory.AssetKey key = args.AsUserData<Factory.AssetKey>(0, "SetOnAssetKey");
		Material sharedMaterial = args.AsUserData<Material>(1, "SetOnAssetKey");
		string text = args.AsString(2, "SetOnAssetKey", allowNil: true);
		GameObject gameObject = Factory.Peek(key);
		bool v = false;
		if (gameObject != null)
		{
			Transform transform = ((!string.IsNullOrEmpty(text)) ? gameObject.transform.Find(text) : gameObject.transform);
			if (transform != null)
			{
				MeshRenderer componentInChildren = transform.GetComponentInChildren<MeshRenderer>();
				if (componentInChildren != null)
				{
					componentInChildren.sharedMaterial = sharedMaterial;
					v = true;
				}
			}
		}
		return DynValue.NewBoolean(v);
	}

	[LuaDrivable("SetOnDriver")]
	public static DynValue SetMatrialOnDriver(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		GameObject gameObject = args.AsGameObject(0, "SetOnDriver");
		Material sharedMaterial = args.AsUserData<Material>(1, "SetOnDriver");
		string text = args.AsString(2, "SetOnDriver", allowNil: true);
		bool v = false;
		Transform transform = ((!string.IsNullOrEmpty(text)) ? gameObject.transform.Find(text) : gameObject.transform);
		if (transform != null)
		{
			MeshRenderer componentInChildren = transform.GetComponentInChildren<MeshRenderer>();
			if (componentInChildren != null)
			{
				componentInChildren.sharedMaterial = sharedMaterial;
				v = true;
			}
		}
		return DynValue.NewBoolean(v);
	}

	[LuaDrivable("SetFloat")]
	private static DynValue SetFloat(ScriptExecutionContext context, CallbackArguments args)
	{
		Material material = args.AsUserData<Material>(0, "SetFloat");
		string name = args.AsString(1, "SetFloat");
		float value = args.AsFloat(2, "SetFloat");
		material.SetFloat(name, value);
		return DynValue.Void;
	}

	[LuaDrivable("SetTexture")]
	private static DynValue SetTexture(ScriptExecutionContext context, CallbackArguments args)
	{
		Material material = args.AsUserData<Material>(0, "SetTexture");
		string name = args.AsString(1, "SetTexture");
		Texture2D value = args.AsUserData<Texture2D>(2, "SetTexture");
		material.SetTexture(name, value);
		return DynValue.Void;
	}

	[LuaDrivable("SetColor")]
	private static DynValue SetColor(ScriptExecutionContext context, CallbackArguments args)
	{
		Material material = args.AsUserData<Material>(0, "SetColor");
		string name = args.AsString(1, "SetColor");
		Color value = args.AsUserData<Color>(2, "SetColor");
		material.SetColor(name, value);
		return DynValue.Void;
	}
}
