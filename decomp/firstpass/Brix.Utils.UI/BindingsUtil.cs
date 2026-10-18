using System;
using System.Collections.Generic;
using System.Linq;
using Brix.Input;
using Brix.Utils.Logging;
using Rewired;
using Smooth.Collections;

namespace Brix.Utils.UI;

public static class BindingsUtil
{
	private static BrixLogger logger = new BrixLogger(typeof(BindingsUtil).FullName);

	public static string GetKeyBindingCodeName<TEnum>(this IKeyBindingsCallbackReceiver<TEnum> iReceiver, TEnum enumValue) where TEnum : struct
	{
		return DoGetKeyBindingCodeName(iReceiver.GetKeyBindingActionName(enumValue), iReceiver.GetKeyBindingCategoryName(enumValue));
	}

	private static string DoGetKeyBindingCodeName(string actionName, string categoryName)
	{
		if (actionName == null)
		{
			logger.Error("Invalid actionName.");
			return string.Empty;
		}
		if (categoryName == null)
		{
			logger.Error("Invalid categoryName.");
			return string.Empty;
		}
		KeyboardMap keyboardMap = null;
		try
		{
			keyboardMap = PickingUtility.Player.controllers.maps.GetMap<KeyboardMap>(0, categoryName, "QWERTY");
		}
		catch (Exception message)
		{
			logger.Info(message);
		}
		if (keyboardMap == null)
		{
			logger.Error("Invalid actionName/categoryName.\nactionName = " + actionName + ", categoryName = " + categoryName);
			return string.Empty;
		}
		ActionElementMap firstElementMapWithAction = keyboardMap.GetFirstElementMapWithAction(actionName);
		if (firstElementMapWithAction == null)
		{
			logger.Error("Missing action in category. actionName = \"" + actionName + "\", categoryName = \"" + categoryName + "\"");
			return string.Empty;
		}
		string text = firstElementMapWithAction.keyCode.ToString();
		text = ((!text.Contains("Alpha")) ? text : text.Replace("Alpha", string.Empty));
		if (firstElementMapWithAction.hasModifiers)
		{
			AppendModifierKeyName(ref text, firstElementMapWithAction.modifierKey3);
			AppendModifierKeyName(ref text, firstElementMapWithAction.modifierKey2);
			AppendModifierKeyName(ref text, firstElementMapWithAction.modifierKey1);
		}
		return text;
	}

	private static void AppendModifierKeyName(ref string keyCode, ModifierKey key)
	{
		switch (key)
		{
		case ModifierKey.Control:
			keyCode = "Ctrl+" + keyCode;
			break;
		case ModifierKey.Alt:
			keyCode = "Alt+" + keyCode;
			break;
		case ModifierKey.Shift:
			keyCode = "Shift+" + keyCode;
			break;
		case ModifierKey.Command:
			keyCode = "Cmd+" + keyCode;
			break;
		}
	}

	public static string GetKeyBindingActionName<TEnum>(this IKeyBindingsCallbackReceiver<TEnum> iReceiver, TEnum enumValue) where TEnum : struct
	{
		if (iReceiver.BindingActionNameMap.ContainsKey(enumValue))
		{
			return iReceiver.BindingActionNameMap[enumValue];
		}
		logger.Error(string.Concat("Missing action name for enum value. enumValue = \"", enumValue, "\""));
		return null;
	}

	public static string GetKeyBindingCategoryName<TEnum>(this IKeyBindingsCallbackReceiver<TEnum> iReceiver, TEnum enumValue) where TEnum : struct
	{
		if (iReceiver.BindingCategoryNameMap.ContainsKey(enumValue))
		{
			return iReceiver.BindingCategoryNameMap[enumValue];
		}
		logger.Error(string.Concat("Missing category name for enum value. enumValue = \"", enumValue, "\""));
		return null;
	}

	public static void ConnectKeyBindings<TEnum>(this IKeyBindingsCallbackReceiver<TEnum> iReceiver) where TEnum : struct
	{
		if (!ReInput.isReady)
		{
			return;
		}
		iReceiver.KeyBindingsConnected = true;
		IEnumerable<KeyValuePair<TEnum, Action<InputActionEventData>>> values = iReceiver.BindingActionMap.Select((KeyValuePair<TEnum, Action> kv) => new KeyValuePair<TEnum, Action<InputActionEventData>>(kv.Key, delegate
		{
			kv.Value();
		}));
		if (iReceiver.BindingInputActionCache == null)
		{
			iReceiver.BindingInputActionCache = new Dictionary<TEnum, Action<InputActionEventData>>();
		}
		iReceiver.BindingInputActionCache.AddAll(values);
		foreach (KeyValuePair<TEnum, Action<InputActionEventData>> item in iReceiver.BindingInputActionCache)
		{
			TEnum key = item.Key;
			Action<InputActionEventData> value = item.Value;
			string keyBindingActionName = iReceiver.GetKeyBindingActionName(key);
			if (keyBindingActionName == null)
			{
				break;
			}
			PickingUtility.Player.AddInputEventDelegate(value, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, keyBindingActionName);
		}
	}

	public static void DisconnectKeyBindings<TEnum>(this IKeyBindingsCallbackReceiver<TEnum> iReceiver) where TEnum : struct
	{
		if (!ReInput.isReady || !iReceiver.KeyBindingsConnected)
		{
			return;
		}
		iReceiver.KeyBindingsConnected = false;
		foreach (KeyValuePair<TEnum, Action<InputActionEventData>> item in iReceiver.BindingInputActionCache)
		{
			TEnum key = item.Key;
			Action<InputActionEventData> value = item.Value;
			string keyBindingActionName = iReceiver.GetKeyBindingActionName(key);
			if (keyBindingActionName == null)
			{
				return;
			}
			PickingUtility.Player.RemoveInputEventDelegate(value, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, keyBindingActionName);
		}
		iReceiver.BindingInputActionCache.Clear();
	}

	public static void ActivateKeyBindingCategories<TEnum>(this IKeyBindingsCallbackReceiver<TEnum> iReceiver) where TEnum : struct
	{
		if (!ReInput.isReady)
		{
			return;
		}
		iReceiver.CategoriesActive = true;
		if (iReceiver.BindingControllerEnabledCache == null)
		{
			iReceiver.BindingControllerEnabledCache = new Dictionary<int, bool>();
		}
		else
		{
			iReceiver.BindingControllerEnabledCache.Clear();
		}
		string[] categoryNameExclusions = iReceiver.CategoryNameExclusions;
		foreach (string categoryName in categoryNameExclusions)
		{
			IEnumerable<KeyboardMap> allMapsInCategory = PickingUtility.Player.controllers.maps.GetAllMapsInCategory<KeyboardMap>(categoryName);
			foreach (KeyboardMap item in allMapsInCategory)
			{
				iReceiver.BindingControllerEnabledCache.Add(item.id, item.enabled);
				item.enabled = false;
			}
		}
		string[] categoryNameInclusions = iReceiver.CategoryNameInclusions;
		foreach (string categoryName2 in categoryNameInclusions)
		{
			IEnumerable<KeyboardMap> allMapsInCategory2 = PickingUtility.Player.controllers.maps.GetAllMapsInCategory<KeyboardMap>(categoryName2);
			foreach (KeyboardMap item2 in allMapsInCategory2)
			{
				iReceiver.BindingControllerEnabledCache.Add(item2.id, item2.enabled);
				item2.enabled = true;
			}
		}
	}

	public static void DeactivateKeyBindingCategories<TEnum>(this IKeyBindingsCallbackReceiver<TEnum> iReceiver) where TEnum : struct
	{
		if (!ReInput.isReady || !iReceiver.CategoriesActive)
		{
			return;
		}
		iReceiver.CategoriesActive = false;
		string[] categoryNameExclusions = iReceiver.CategoryNameExclusions;
		foreach (string categoryName in categoryNameExclusions)
		{
			IEnumerable<KeyboardMap> allMapsInCategory = PickingUtility.Player.controllers.maps.GetAllMapsInCategory<KeyboardMap>(categoryName);
			foreach (KeyboardMap item in allMapsInCategory)
			{
				item.enabled = iReceiver.BindingControllerEnabledCache[item.id];
			}
		}
		string[] categoryNameInclusions = iReceiver.CategoryNameInclusions;
		foreach (string categoryName2 in categoryNameInclusions)
		{
			IEnumerable<KeyboardMap> allMapsInCategory2 = PickingUtility.Player.controllers.maps.GetAllMapsInCategory<KeyboardMap>(categoryName2);
			foreach (KeyboardMap item2 in allMapsInCategory2)
			{
				item2.enabled = iReceiver.BindingControllerEnabledCache[item2.id];
			}
		}
	}
}
