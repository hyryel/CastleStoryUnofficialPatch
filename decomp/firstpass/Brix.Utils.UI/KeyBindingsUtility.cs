using System;
using System.Collections.Generic;
using Brix.Input;
using Brix.Utils.Logging;
using Rewired;
using UnityEngine;

namespace Brix.Utils.UI;

public static class KeyBindingsUtility
{
	public enum Category
	{
		Camera,
		General,
		WorldEditorMenu,
		WorldEditorTerrainMenu,
		WorldEditorMarkerMenu,
		Debug,
		Blueprints,
		Construction,
		Projects,
		TaskMenu,
		BlocksMenu,
		StructuresMenu,
		Crafting,
		Bricktron
	}

	private static BrixLogger logger = new BrixLogger(typeof(KeyBindingsUtility).FullName);

	private static readonly Dictionary<string, Action<InputActionEventData>> ActionMap = new Dictionary<string, Action<InputActionEventData>>();

	public static string GetKeyBindingCodename(string categoryName, string actionName)
	{
		if (categoryName == null)
		{
			Debug.Log("Invalid category name lol");
			logger.Error("Invalid categoryName.");
			return string.Empty;
		}
		if (actionName == null)
		{
			Debug.Log("Invalid key name!");
			logger.Error("Invalid actionName.");
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
			return string.Empty;
		}
		string text = firstElementMapWithAction.keyCode.ToString();
		text = ((!text.Contains("Minus")) ? text : text.Replace("Minus", "-"));
		text = ((!text.Contains("Equals")) ? text : text.Replace("Equals", "+"));
		text = ((!text.Contains("Backspace")) ? text : text.Replace("Backspace", "Back"));
		text = ((!text.Contains("Delete")) ? text : text.Replace("Delete", "Del"));
		text = ((!text.Contains("Alpha")) ? text : text.Replace("Alpha", string.Empty));
		text = ((!text.Contains("None")) ? text : text.Replace("None", string.Empty));
		if (firstElementMapWithAction.hasModifiers)
		{
			AppendModifierKeyCodename(ref text, firstElementMapWithAction.modifierKey3);
			AppendModifierKeyCodename(ref text, firstElementMapWithAction.modifierKey2);
			AppendModifierKeyCodename(ref text, firstElementMapWithAction.modifierKey1);
		}
		return text;
	}

	private static void AppendModifierKeyCodename(ref string keyCodename, ModifierKey modifierKey)
	{
		switch (modifierKey)
		{
		case ModifierKey.Control:
			keyCodename = "Ctrl+" + keyCodename;
			break;
		case ModifierKey.Alt:
			keyCodename = "Alt+" + keyCodename;
			break;
		case ModifierKey.Shift:
			keyCodename = "Shift+" + keyCodename;
			break;
		case ModifierKey.Command:
			keyCodename = "Cmd+" + keyCodename;
			break;
		}
	}

	public static string GetCategoryName(Category category)
	{
		return category switch
		{
			Category.Camera => "Camera", 
			Category.Bricktron => "Bricktron", 
			Category.General => "General", 
			Category.WorldEditorMenu => "we_menu", 
			Category.WorldEditorTerrainMenu => "we_terrain_menu", 
			Category.WorldEditorMarkerMenu => "we_markers_menu", 
			Category.Debug => "debug", 
			Category.Blueprints => "Blueprints", 
			Category.Construction => "tb_constr_menu", 
			Category.Projects => "Projects", 
			Category.TaskMenu => "TaskMenu", 
			Category.BlocksMenu => "BlocksMenu", 
			Category.StructuresMenu => "StructuresMenu", 
			Category.Crafting => "Crafting", 
			_ => string.Empty, 
		};
	}

	public static void RegisterAction(string actionName, Action<InputActionEventData> action)
	{
		UnregisterAction(actionName);
		ActionMap.Add(actionName, action);
		ActivateAction(actionName, ActionMap[actionName]);
	}

	public static void UnregisterAction(string actionName)
	{
		if (ActionMap.ContainsKey(actionName))
		{
			DeactivateAction(actionName, ActionMap[actionName]);
			ActionMap.Remove(actionName);
		}
	}

	public static void UnregisterAll()
	{
		foreach (KeyValuePair<string, Action<InputActionEventData>> item in ActionMap)
		{
			DeactivateAction(item.Key, item.Value);
		}
		ActionMap.Clear();
	}

	private static void ActivateAction(string actionName, Action<InputActionEventData> action)
	{
		PickingUtility.Player.AddInputEventDelegate(action, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, actionName);
	}

	private static void DeactivateAction(string actionName, Action<InputActionEventData> action)
	{
		PickingUtility.Player.RemoveInputEventDelegate(action, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, actionName);
	}
}
