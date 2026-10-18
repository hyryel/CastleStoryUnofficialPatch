using Brix.External.Factories;
using Brix.Lua;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Brix.Input;

public class InputContextComponent : MonoBehaviour
{
	public InputMode defaultInputMode;

	public Factory.AssetKey defaultPicker;

	public static Factory.AssetKey defaultHeldObject = Blueprints.Stone_DoubleBrick;

	public bool SaveContextChanges;

	private InputContextData _inputContextData;

	public InputContextData InputContextData
	{
		get
		{
			if (_inputContextData == null)
			{
				_inputContextData = new InputContextData();
				_inputContextData.mode = defaultInputMode;
				_inputContextData.pickerKey = defaultPicker;
				_inputContextData.objectKey = defaultHeldObject;
			}
			return _inputContextData;
		}
		set
		{
			_inputContextData = value;
		}
	}

	public void SaveCurrentContext()
	{
		if (SaveContextChanges && !InputModeController.IsPaused())
		{
			InputContextData = InputContextData.GetCurrentActiveContext();
		}
	}

	public void LoadContext()
	{
		InputContextData.SetContextActive();
	}

	[LuaDrivable("ChangeDefaultBlueprint")]
	public static DynValue lua_ChangeDefaultBlueprint(ScriptExecutionContext executionContext, CallbackArguments args)
	{
		string text = args.AsString(0, "ChangeDefaultBlueprint");
		if (text == "Pallet")
		{
			defaultHeldObject = Blueprints.Pallet;
		}
		else if (text == "Stone_DoubleBrick")
		{
			defaultHeldObject = Blueprints.Stone_DoubleBrick;
		}
		return DynValue.Void;
	}
}
