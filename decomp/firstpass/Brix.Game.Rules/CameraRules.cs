using System;
using Brix.Input;
using Brix.Lua;
using Brix.Network.Util;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Brix.Game.Rules;

[Serializable]
[SerializerTarget(new string[] { "save" })]
[RequireComponent(typeof(RuleMaster))]
public class CameraRules : MonoBehaviour
{
	private CompCache<RuleMaster> _ruleMaster;

	private static CameraRules _main;

	public static CameraRules Main => _main;

	private void Awake()
	{
		_ruleMaster = new CompCache<RuleMaster>(base.gameObject);
		if (_ruleMaster.Get.Priority == -1)
		{
			_main = this;
		}
	}

	public void UseCameraBounds(bool value)
	{
		Camera.main.GetComponent<CameraScript>().UseCameraBounds = value;
	}

	public void SetCameraBounds(Vector3 center, Vector3 extents)
	{
		CameraScript component = Camera.main.GetComponent<CameraScript>();
		component.BoundsLimit = new Bounds(center, extents);
	}

	public void SetWorldEditorProfile()
	{
		CameraScript component = Camera.main.GetComponent<CameraScript>();
		component.EnableWorldEditorCamera();
	}

	public void SetCanMoveCamera(bool move)
	{
	}

	[LuaDrivable("UseCameraBounds")]
	private static DynValue lua_UseCameraBounds(ScriptExecutionContext context, CallbackArguments args)
	{
		CameraRules cameraRules = args.AsComponent<CameraRules>(0, "UseCameraBounds");
		bool value = args.AsBool(1, "UseCameraBounds");
		cameraRules.UseCameraBounds(value);
		return DynValue.Void;
	}

	[LuaDrivable("SetCameraBounds")]
	private static DynValue lua_SetCameraBounds(ScriptExecutionContext context, CallbackArguments args)
	{
		CameraRules cameraRules = args.AsComponent<CameraRules>(0, "SetCameraBounds");
		Vector3 center = args.AsUserData<Vector3>(1, "SetCameraBounds");
		Vector3 extents = args.AsUserData<Vector3>(2, "SetCameraBounds");
		cameraRules.SetCameraBounds(center, extents);
		return DynValue.Void;
	}

	[LuaDrivable("EnableWorldEditorCameraProfile")]
	private static DynValue lua_EnableWorldEditorCameraProfile(ScriptExecutionContext context, CallbackArguments args)
	{
		CameraRules cameraRules = args.AsComponent<CameraRules>(0, "EnableWorldEditorCameraProfile");
		cameraRules.SetWorldEditorProfile();
		return DynValue.Void;
	}
}
