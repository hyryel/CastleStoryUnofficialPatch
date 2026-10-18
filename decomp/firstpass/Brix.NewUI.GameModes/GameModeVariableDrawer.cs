using System;
using System.Collections.Generic;
using Brix.Utils.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.NewUI.GameModes;

public class GameModeVariableDrawer
{
	private static BrixLogger logger = new BrixLogger(typeof(GameModeVariableDrawer).FullName);

	public readonly GameObject container;

	private List<GameModeVariable> settings;

	private List<GameObject> layout;

	public GameModeVariableDrawer(GameObject container)
	{
		this.container = container;
	}

	private List<GameObject> MakeLayout(List<GameModeVariable> settings)
	{
		int height;
		List<GameObject> result = VerticalLayout(out height, container, settings.Count);
		container.GetComponent<RectTransform>().sizeDelta = new Vector2(container.GetComponent<RectTransform>().sizeDelta.x, height);
		return result;
	}

	private List<GameObject> VerticalLayout(out int height, GameObject container, int divs)
	{
		List<GameObject> list = new List<GameObject>();
		height = 0;
		for (int i = 0; i < divs; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("UI/NewGame/HorizontalSlice")) as GameObject;
			gameObject.transform.SetParent(this.container.transform);
			RectTransform component = gameObject.GetComponent<RectTransform>();
			component.localPosition = new Vector3(0f, 0f, 0f);
			component.offsetMin = new Vector2(0f, component.offsetMin.y);
			component.offsetMax = new Vector2(0f, component.offsetMax.y);
			component.anchoredPosition = new Vector2(0f, 0f - component.rect.height / 2f - (float)i * component.rect.height);
			list.Add(gameObject.gameObject);
			height += (int)component.rect.height;
		}
		return list;
	}

	public void EraseAll()
	{
	}

	private List<GameModeSettingDrawing> DrawAll(List<GameModeVariable> settings, List<GameObject> layout)
	{
		if (settings.Count != layout.Count)
		{
			throw new Exception("incompatible layout");
		}
		List<GameModeSettingDrawing> list = new List<GameModeSettingDrawing>();
		for (int i = 0; i < settings.Count; i++)
		{
			list.Add(DrawOne(settings[i], layout[i]));
		}
		return list;
	}

	private GameModeSettingDrawing DrawOne(GameModeVariable setting, GameObject layout)
	{
		GameModeSettingDrawing gameModeSettingDrawing = new GameModeSettingDrawing(Create(setting), setting);
		gameModeSettingDrawing.drawing.transform.SetParent(layout.transform);
		RectTransform component = gameModeSettingDrawing.drawing.GetComponent<RectTransform>();
		component.offsetMax = Vector2.zero;
		component.offsetMin = Vector2.zero;
		component.localPosition = Vector2.zero;
		return gameModeSettingDrawing;
	}

	public void PopulateWith(Dictionary<string, object> preset)
	{
	}

	private void PopulateWith(List<GameModeSettingDrawing> settingDrawings, Dictionary<string, object> preset)
	{
		foreach (GameModeSettingDrawing settingDrawing in settingDrawings)
		{
			SetValue(settingDrawing, preset[settingDrawing.setting.luaKey]);
		}
	}

	private void SetValue(GameModeSettingDrawing drawingSetting, object value)
	{
		try
		{
			switch (drawingSetting.setting.type)
			{
			case GameModeVariable.dataType.Bool:
				drawingSetting.drawing.GetComponent<Toggle>().isOn = (bool)value;
				break;
			case GameModeVariable.dataType.Enum:
				break;
			case GameModeVariable.dataType.Int:
				drawingSetting.drawing.transform.Find("Input").GetComponent<InputField>().text = ((long)value/*cast due to .constrained prefix*/).ToString();
				break;
			case GameModeVariable.dataType.Float:
				try
				{
					drawingSetting.drawing.transform.Find("Input").GetComponent<InputField>().text = ((double)value/*cast due to .constrained prefix*/).ToString();
					break;
				}
				catch (InvalidCastException)
				{
					drawingSetting.drawing.transform.Find("Input").GetComponent<InputField>().text = ((long)value/*cast due to .constrained prefix*/).ToString();
					break;
				}
			case GameModeVariable.dataType.String:
				drawingSetting.drawing.transform.Find("Input").GetComponent<InputField>().text = (string)value;
				break;
			default:
				throw new Exception("Invalid GameSetting type");
			}
		}
		catch (InvalidCastException)
		{
			logger.Error("Problem drawing : " + drawingSetting.setting.luaKey + " with value " + value.ToString() + " of type " + drawingSetting.setting.type);
			throw;
		}
	}

	public Dictionary<string, object> Extract()
	{
		return null;
	}

	private Dictionary<string, object> Extract(List<GameModeSettingDrawing> settingDrawings)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		foreach (GameModeSettingDrawing settingDrawing in settingDrawings)
		{
			dictionary.Add(settingDrawing.setting.luaKey, GetValue(settingDrawing));
		}
		return dictionary;
	}

	private object GetValue(GameModeSettingDrawing drawingSetting)
	{
		object result = null;
		switch (drawingSetting.setting.type)
		{
		case GameModeVariable.dataType.Bool:
			result = drawingSetting.drawing.GetComponent<Toggle>().isOn;
			break;
		case GameModeVariable.dataType.Int:
			result = int.Parse(drawingSetting.drawing.transform.Find("Input").GetComponent<InputField>().text);
			break;
		case GameModeVariable.dataType.Float:
			result = float.Parse(drawingSetting.drawing.transform.Find("Input").GetComponent<InputField>().text);
			break;
		case GameModeVariable.dataType.String:
			result = drawingSetting.drawing.transform.Find("Input").GetComponent<InputField>().text;
			break;
		default:
			throw new Exception("Invalid GameSetting type");
		case GameModeVariable.dataType.Enum:
			break;
		}
		return result;
	}

	private GameObject Create(GameModeVariable setting)
	{
		return setting.type switch
		{
			GameModeVariable.dataType.Bool => CreateBool(setting), 
			GameModeVariable.dataType.Enum => CreateEnum(setting), 
			GameModeVariable.dataType.Int => CreateInt(setting), 
			GameModeVariable.dataType.Float => CreateFloat(setting), 
			GameModeVariable.dataType.String => CreateString(setting), 
			_ => throw new Exception("Invalid GameSetting type"), 
		};
	}

	private GameObject CreateString(GameModeVariable setting)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("UI/NewGame/TextBox")) as GameObject;
		gameObject.transform.Find("Label").GetComponent<Text>().text = setting.luaKey;
		gameObject.transform.Find("ToolTip").GetComponent<Text>().text = setting.toolTip;
		return gameObject;
	}

	private GameObject CreateInt(GameModeVariable setting)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("UI/NewGame/TextBox")) as GameObject;
		gameObject.transform.Find("Label").GetComponent<Text>().text = setting.luaKey;
		gameObject.transform.Find("Input").GetComponent<InputField>().contentType = InputField.ContentType.IntegerNumber;
		gameObject.transform.Find("ToolTip").GetComponent<Text>().text = setting.toolTip;
		return gameObject;
	}

	private GameObject CreateFloat(GameModeVariable setting)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("UI/NewGame/TextBox")) as GameObject;
		gameObject.transform.Find("Label").GetComponent<Text>().text = setting.luaKey;
		gameObject.transform.Find("Input").GetComponent<InputField>().contentType = InputField.ContentType.DecimalNumber;
		gameObject.transform.Find("ToolTip").GetComponent<Text>().text = setting.toolTip;
		return gameObject;
	}

	private GameObject CreateBool(GameModeVariable setting)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("UI/NewGame/CheckBox")) as GameObject;
		gameObject.transform.Find("Label").GetComponent<Text>().text = setting.luaKey;
		gameObject.transform.Find("ToolTip").GetComponent<Text>().text = setting.toolTip;
		return gameObject;
	}

	private GameObject CreateEnum(GameModeVariable setting)
	{
		return null;
	}
}
