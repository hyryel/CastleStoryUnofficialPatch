using System;
using System.Collections.Generic;
using System.Linq;
using Brix.Engine;
using Brix.Engine.Nature;
using Brix.Input;
using Brix.Legacy;
using Brix.Pathfinding.Test;
using Brix.UI.Builder.Panel;
using Brix.UI.Builder.Widget;
using Brix.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Menu;

public class TerrainUndersideMenu : BaseMenu
{
	public enum Alignment
	{
		None,
		Top,
		Bottom,
		Middle
	}

	public enum LevelSource
	{
		Custom,
		Camera,
		Grid
	}

	private abstract class LevelSliderWidget : BaseWidget, IGetSuffixLabelText, IGetSuffixLabelColor, IGetSuffixLabelWidth, IGetMinValue<float>, IGetMaxValue<float>, IToValidValue<float>, IToTextValue<float>, IGetContentType, IGetWholeNumbers, IOnValidateInput, IGetCharacterLimit, IOnSelect, IOnDeselect, IGetMinWidth, IGetPreferredWidth, IWidgetHandle
	{
		private readonly BaseMenu _m;

		protected LevelSliderWidget(BaseMenu m)
		{
			_m = m;
		}

		public string GetSuffixLabelText()
		{
			return "vx";
		}

		public Color GetSuffixLabelColor()
		{
			return Colors.Gray;
		}

		public float GetSuffixLabelWidth(Text text)
		{
			return (text.preferredWidth + 1f) * -1f;
		}

		public float GetMinValue()
		{
			return _yMin;
		}

		public float GetMaxValue()
		{
			return _yMax;
		}

		public float ToValidValue(float value)
		{
			return Mathf.Clamp(value, GetMinValue(), GetMaxValue());
		}

		public string ToTextValue(float value)
		{
			return Mathf.Round(value).ToString();
		}

		public InputField.ContentType GetContentType()
		{
			return InputField.ContentType.IntegerNumber;
		}

		public bool GetWholeNumbers()
		{
			return true;
		}

		public char OnValidateInput(string value, int index, char addedChar)
		{
			return char.IsNumber(addedChar) ? addedChar : '\0';
		}

		public int GetCharacterLimit()
		{
			return 3;
		}

		public void OnSelect()
		{
			_m.SetKeyBindingCategories(flag: false);
		}

		public void OnDeselect()
		{
			_m.ResetKeyBindingCategories();
		}

		public float GetMinWidth()
		{
			return 256f;
		}

		public float GetPreferredWidth()
		{
			return 256f;
		}

		public abstract float GetValue();

		public float FromTextValue(string textValue)
		{
			return (!string.IsNullOrEmpty(textValue)) ? float.Parse(textValue) : GetValue();
		}
	}

	private class CustomLevelSliderWidget : LevelSliderWidget, IGetKeyLabelText, IGetKeyLabelColor, IGetControlColor, IGetValue<float>, ISetValue<float>, IFromTextValue<float>, IGetInteractable, IWidgetHandle
	{
		private readonly BaseMenu _m;

		public CustomLevelSliderWidget(BaseMenu m)
			: base(m)
		{
			_m = m;
		}

		public string GetKeyLabelText()
		{
			return "Level (Custom)";
		}

		public Color GetKeyLabelColor()
		{
			return (!GetInteractable()) ? Colors.Gray : Colors.Yellow;
		}

		public Color GetControlColor()
		{
			return (!GetInteractable()) ? Colors.Gray : Colors.Yellow;
		}

		public override float GetValue()
		{
			return CustomLevel;
		}

		public void SetValue(float value)
		{
			CustomLevel = Mathf.RoundToInt(value);
			_m.Refresh();
		}

		public bool GetInteractable()
		{
			return true;
		}
	}

	private class CameraLevelSliderWidget : LevelSliderWidget, IGetKeyLabelText, IGetKeyLabelColor, IGetControlColor, IGetValue<float>, IFromTextValue<float>, IGetInteractable, IWidgetHandle
	{
		public CameraLevelSliderWidget(BaseMenu m)
			: base(m)
		{
		}

		public string GetKeyLabelText()
		{
			return "Level (Camera)";
		}

		public Color GetKeyLabelColor()
		{
			return (!GetInteractable()) ? Colors.Gray : Colors.Yellow;
		}

		public Color GetControlColor()
		{
			return (!GetInteractable()) ? Colors.Gray : Colors.Yellow;
		}

		public override float GetValue()
		{
			return _cameraLevel;
		}

		public bool GetInteractable()
		{
			return false;
		}
	}

	private class GridLevelSliderWidget : LevelSliderWidget, IGetKeyLabelText, IGetKeyLabelColor, IGetControlColor, IGetValue<float>, IFromTextValue<float>, IGetInteractable, IWidgetHandle
	{
		public GridLevelSliderWidget(BaseMenu m)
			: base(m)
		{
		}

		public string GetKeyLabelText()
		{
			return "Level (Grid)";
		}

		public Color GetKeyLabelColor()
		{
			return (!GetInteractable()) ? Colors.Gray : Colors.Yellow;
		}

		public Color GetControlColor()
		{
			return (!GetInteractable()) ? Colors.Gray : Colors.Yellow;
		}

		public override float GetValue()
		{
			return _gridLevel;
		}

		public bool GetInteractable()
		{
			return false;
		}
	}

	private class PercentSliderWidget : BaseWidget, IGetKeyLabelColor, IGetControlColor, IGetSuffixLabelText, IGetSuffixLabelColor, IGetSuffixLabelWidth, IGetMinValue<float>, IGetMaxValue<float>, IToValidValue<float>, IToTextValue<float>, IGetContentType, IGetWholeNumbers, IOnValidateInput, IGetCharacterLimit, IGetInteractable, IOnSelect, IOnDeselect, IGetMinWidth, IGetPreferredWidth, IWidgetHandle
	{
		private readonly BaseMenu _m;

		protected PercentSliderWidget(BaseMenu m)
		{
			_m = m;
		}

		public Color GetKeyLabelColor()
		{
			return (!GetInteractable()) ? Colors.Gray : Colors.Yellow;
		}

		public Color GetControlColor()
		{
			return (!GetInteractable()) ? Colors.Gray : Colors.Yellow;
		}

		public string GetSuffixLabelText()
		{
			return "%";
		}

		public Color GetSuffixLabelColor()
		{
			return Colors.Gray;
		}

		public float GetSuffixLabelWidth(Text text)
		{
			return (text.preferredWidth + 1f) * -1f;
		}

		public float GetMinValue()
		{
			return 0f;
		}

		public float GetMaxValue()
		{
			return 100f;
		}

		public float ToValidValue(float value)
		{
			return Mathf.Clamp(value, GetMinValue(), GetMaxValue());
		}

		public string ToTextValue(float value)
		{
			return Mathf.Round(value).ToString();
		}

		public InputField.ContentType GetContentType()
		{
			return InputField.ContentType.IntegerNumber;
		}

		public bool GetWholeNumbers()
		{
			return true;
		}

		public char OnValidateInput(string value, int index, char addedChar)
		{
			return char.IsNumber(addedChar) ? addedChar : '\0';
		}

		public int GetCharacterLimit()
		{
			return 3;
		}

		public bool GetInteractable()
		{
			return true;
		}

		public void OnSelect()
		{
			_m.SetKeyBindingCategories(flag: false);
		}

		public void OnDeselect()
		{
			_m.ResetKeyBindingCategories();
		}

		public float GetMinWidth()
		{
			return 256f;
		}

		public float GetPreferredWidth()
		{
			return 256f;
		}
	}

	private class ThicknessSliderWidget : PercentSliderWidget, IGetKeyLabelText, IGetValue<float>, ISetValue<float>, IFromTextValue<float>, IWidgetHandle
	{
		private readonly BaseMenu _m;

		public ThicknessSliderWidget(BaseMenu m)
			: base(m)
		{
			_m = m;
		}

		public string GetKeyLabelText()
		{
			return "Thickness";
		}

		public float GetValue()
		{
			return Thickness;
		}

		public void SetValue(float value)
		{
			Thickness = value;
			NeedsCalculate = true;
			_m.Refresh();
		}

		public float FromTextValue(string textValue)
		{
			return (!string.IsNullOrEmpty(textValue)) ? float.Parse(textValue) : GetValue();
		}
	}

	private class RandomnessSliderWidget : PercentSliderWidget, IGetKeyLabelText, IGetValue<float>, ISetValue<float>, IFromTextValue<float>, IWidgetHandle
	{
		private readonly BaseMenu _m;

		public RandomnessSliderWidget(BaseMenu m)
			: base(m)
		{
			_m = m;
		}

		public string GetKeyLabelText()
		{
			return "Randomness";
		}

		public float GetValue()
		{
			return Randomness;
		}

		public void SetValue(float value)
		{
			Randomness = value;
			NeedsCalculate = true;
			_m.Refresh();
		}

		public float FromTextValue(string textValue)
		{
			return (!string.IsNullOrEmpty(textValue)) ? float.Parse(textValue) : GetValue();
		}
	}

	private const int _botPadding = 8;

	private const int _topPadding = 40;

	public static bool NeedsCalculate = true;

	private static Alignment _undersideAlignment = Alignment.None;

	private static float _thickness = 0.5f;

	public static int _randomnessSeed = 0;

	private static float _randomness = 0.5f;

	public static bool _translateCustomLevelOnAlign = true;

	private static LevelSource _currentLevelSource = LevelSource.Camera;

	private static int _customLevel = 127;

	private static Bounds _bounds = default(Bounds);

	private static int _yMin => 8;

	private static int _yMax => 215;

	public static bool _canReallyRegenerate => CanRegenerate(Thickness, _cameraLevel);

	public static Alignment UndersideAlignment
	{
		get
		{
			return _undersideAlignment;
		}
		set
		{
			_undersideAlignment = value;
			NeedsCalculate = true;
		}
	}

	public static float Thickness
	{
		get
		{
			return _thickness;
		}
		set
		{
			_thickness = value;
			NeedsCalculate = true;
		}
	}

	public static float Randomness
	{
		get
		{
			return _randomness;
		}
		set
		{
			_randomness = value;
			NeedsCalculate = true;
		}
	}

	public static LevelSource CurrentLevelSource
	{
		get
		{
			return _currentLevelSource;
		}
		set
		{
			_currentLevelSource = value;
			NeedsCalculate = true;
		}
	}

	public static int CustomLevel
	{
		get
		{
			return _customLevel;
		}
		set
		{
			_customLevel = value;
			NeedsCalculate = true;
		}
	}

	public static int _cameraLevel => Mathf.RoundToInt(Camera.main.GetComponent<CameraScript>().GizmoTarget.transform.position.y);

	public static int _gridLevel => Picking.Instance.WorldEditorGrid.Height;

	public static int Level => CurrentLevelSource switch
	{
		LevelSource.Custom => CustomLevel, 
		LevelSource.Camera => _cameraLevel, 
		LevelSource.Grid => _gridLevel, 
		_ => throw new NotImplementedException("LevelSource not supported: " + CurrentLevelSource), 
	};

	public TerrainUndersideMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public static TerrainUndersideMenu Build(Func<RectTransform> binGetter)
	{
		return new TerrainUndersideMenu(binGetter);
	}

	private static bool IsAlignementRequired(Bounds bounds, Alignment aAlignment)
	{
		return aAlignment switch
		{
			Alignment.Bottom => bounds.min.y < (float)_yMin, 
			Alignment.Top => bounds.min.y > (float)_yMax, 
			_ => false, 
		};
	}

	private static bool IsAlignmentPossible(Bounds bounds)
	{
		return !IsAlignementRequired(bounds, Alignment.Top) || !IsAlignementRequired(bounds, Alignment.Bottom);
	}

	private static XYZ GetTranslation(Bounds bounds, Alignment alignment)
	{
		XYZ zero = XYZ.zero;
		switch (alignment)
		{
		case Alignment.Bottom:
			zero.y = GetDistance(bounds, alignment).y * -1;
			break;
		case Alignment.Top:
			zero.y = GetDistance(bounds, alignment).y;
			break;
		case Alignment.Middle:
		{
			int num = GetDistance(bounds, Alignment.Bottom).y * -1;
			int y = GetDistance(bounds, Alignment.Top).y;
			int y2 = Mathf.RoundToInt((float)(num + y) / 2f);
			zero.y = y2;
			break;
		}
		}
		return zero;
	}

	private static XYZ GetDistance(Bounds bounds, Alignment alignment, bool safe = true)
	{
		safe = true;
		XYZ zero = XYZ.zero;
		switch (alignment)
		{
		case Alignment.Bottom:
			zero.y = ((!safe) ? ((int)bounds.min.y) : ((int)bounds.min.y - _yMin));
			break;
		case Alignment.Top:
			zero.y = ((!safe) ? (255 - (int)bounds.max.y) : (_yMax - (int)bounds.max.y));
			break;
		}
		return zero;
	}

	private static Bounds GetBounds(float thickness, int level)
	{
		UnityEngine.Random.InitState(_randomnessSeed);
		return Voxel.Engine.GetGeneratedBottomBounds(thickness, Randomness, level);
	}

	private static void Generate(XYZ translation, float thickness, int level)
	{
		UnityEngine.Random.InitState(_randomnessSeed);
		Voxel.Engine.GenerateDefaultBottom(translation, thickness, Randomness, level);
	}

	private static bool CanRegenerate(float thickness, int level)
	{
		return GetTranslation(GetBounds(thickness, level), UndersideAlignment) == XYZ.zero;
	}

	private static void TranslateNature(XYZ translation)
	{
		if (translation == XYZ.zero)
		{
			return;
		}
		try
		{
			TreeRegistry.Instance.TranslateAllTrees(translation);
			NatureRegistry.TranslateNature(translation);
		}
		catch (Exception ex)
		{
			throw ex;
		}
	}

	private static void TranslateSpawners(XYZ translation)
	{
		if (translation == XYZ.zero)
		{
			return;
		}
		Vector3 vector = new Vector3(translation.x, translation.y, translation.z);
		List<Transform> list = UIGameObserver.worldEditorObjects.All.ToList();
		foreach (Transform item in list)
		{
			item.position += vector;
		}
	}

	private static void TranslateCamera(XYZ translation)
	{
		if (!(translation == XYZ.zero))
		{
			Vector3 vector = new Vector3(translation.x, translation.y, translation.z);
			Camera main = Camera.main;
			Vector3 p = main.transform.position + vector;
			Quaternion rotation = main.transform.rotation;
			main.GetComponent<CameraScript>().Positionner(p, rotation);
		}
	}

	private static void RebuildPathfinding()
	{
		TestPathfinding.Instance.Read();
	}

	private static void DoTranslate()
	{
		XYZ xYZ = XYZ.zero;
		if (UndersideAlignment != Alignment.None)
		{
			xYZ = GetTranslation(_bounds, UndersideAlignment);
		}
		Generate(xYZ, Thickness, Level);
		if (xYZ != XYZ.zero && CurrentLevelSource == LevelSource.Custom && _translateCustomLevelOnAlign)
		{
			CustomLevel += Mathf.RoundToInt(xYZ.y);
		}
		TranslateNature(xYZ);
		TranslateSpawners(xYZ);
		TranslateCamera(xYZ);
		RebuildPathfinding();
	}

	public static void CalculateBounds()
	{
		_randomnessSeed++;
		NeedsCalculate = false;
		_bounds = GetBounds(Thickness, Level);
	}

	public static Vector2 GetBounds()
	{
		int y = GetDistance(_bounds, Alignment.Top, safe: false).y;
		int y2 = GetDistance(_bounds, Alignment.Bottom, safe: false).y;
		return new Vector2(y, y2);
	}

	public static void Regenerate()
	{
		DoTranslate();
		NeedsCalculate = true;
	}

	public static void SetLevelSource(LevelSource levelSource)
	{
		CurrentLevelSource = levelSource;
		NeedsCalculate = true;
	}

	public static void SetAlignment(Alignment alignment)
	{
		UndersideAlignment = alignment;
		NeedsCalculate = true;
	}

	private static void AddLevelSlider(ref List<BasePanel> builders, BaseWidget slider, LevelSource levelSource)
	{
		SliderInputPanel item = SliderInputPanel.New();
		builders.Add(item);
	}

	private static void AddPercentSlider(ref List<BasePanel> builders, BaseWidget slider)
	{
	}

	public static List<BasePanel> GetBuilders(BaseMenu menuN)
	{
		List<BasePanel> builders = new List<BasePanel>();
		List<LevelSource> levelSources = new List<LevelSource>
		{
			LevelSource.Custom,
			LevelSource.Grid,
			LevelSource.Camera
		};
		Action<DropdownPanelComponent> onRefreshN = delegate(DropdownPanelComponent panel)
		{
			LevelSource currentLevelSource = CurrentLevelSource;
			int value = levelSources.IndexOf(currentLevelSource);
			string text = CurrentLevelSource.ToString();
			panel._valueLabel.text = text;
			panel._dropdown.interactable = true;
			Dropdown.DropdownEvent onValueChanged = panel._dropdown.onValueChanged;
			panel._dropdown.onValueChanged = new Dropdown.DropdownEvent();
			panel._dropdown.value = value;
			panel._dropdown.onValueChanged = onValueChanged;
		};
		Action<DropdownPanelComponent> onReset = delegate(DropdownPanelComponent panel)
		{
			panel._arrowImage.color = Colors.Yellow;
			panel._keyLabel.text = "Level";
			panel._dropdown.options = new List<Dropdown.OptionData>();
			foreach (LevelSource item7 in levelSources)
			{
				panel._dropdown.options.Add(new Dropdown.OptionData(item7.ToString()));
			}
			panel._dropdown.onValueChanged.RemoveAllListeners();
			panel._dropdown.value = levelSources.IndexOf(CurrentLevelSource);
			panel._dropdown.onValueChanged.AddListener(delegate(int dstIndex)
			{
				SetLevelSource(levelSources[dstIndex]);
				onRefreshN(panel);
			});
			onRefreshN(panel);
		};
		DropdownPanel item = DropdownPanel.New(onReset, onRefreshN);
		builders.Add(item);
		AddLevelSlider(ref builders, new CustomLevelSliderWidget(menuN), LevelSource.Custom);
		AddLevelSlider(ref builders, new GridLevelSliderWidget(menuN), LevelSource.Grid);
		AddLevelSlider(ref builders, new CameraLevelSliderWidget(menuN), LevelSource.Camera);
		AddPercentSlider(ref builders, new ThicknessSliderWidget(menuN));
		AddPercentSlider(ref builders, new RandomnessSliderWidget(menuN));
		List<Alignment> alignments = new List<Alignment>
		{
			Alignment.None,
			Alignment.Bottom,
			Alignment.Middle,
			Alignment.Top
		};
		Action<DropdownPanelComponent> onRefreshN2 = delegate(DropdownPanelComponent panel)
		{
			Alignment undersideAlignment = UndersideAlignment;
			int value = alignments.IndexOf(undersideAlignment);
			string text = I2Helper.TryGet(UndersideAlignment.ToString());
			panel._valueLabel.text = text;
			panel._dropdown.interactable = true;
			Dropdown.DropdownEvent onValueChanged = panel._dropdown.onValueChanged;
			panel._dropdown.onValueChanged = new Dropdown.DropdownEvent();
			panel._dropdown.value = value;
			panel._dropdown.onValueChanged = onValueChanged;
		};
		Action<DropdownPanelComponent> onReset2 = delegate(DropdownPanelComponent panel)
		{
			panel._arrowImage.color = Colors.Yellow;
			panel._keyLabel.text = "Alignment";
			panel._dropdown.options = new List<Dropdown.OptionData>();
			foreach (Alignment item8 in alignments)
			{
				panel._dropdown.options.Add(new Dropdown.OptionData(item8.ToString()));
			}
			panel._dropdown.onValueChanged.RemoveAllListeners();
			panel._dropdown.onValueChanged.AddListener(delegate(int dstIndex)
			{
				SetAlignment(alignments[dstIndex]);
				onRefreshN2(panel);
			});
		};
		DropdownPanel item2 = DropdownPanel.New(onReset2, onRefreshN2);
		builders.Add(item2);
		Action<CompoundEntryPanelComponent> onRefreshN3 = delegate(CompoundEntryPanelComponent panel)
		{
			bool active = UndersideAlignment != Alignment.None && CurrentLevelSource == LevelSource.Custom;
			bool translateCustomLevelOnAlign = _translateCustomLevelOnAlign;
			panel.gameObject.SetActive(active);
			panel._selectImage.gameObject.SetActive(translateCustomLevelOnAlign);
		};
		Action<CompoundEntryPanelComponent> onReset3 = delegate(CompoundEntryPanelComponent panel)
		{
			panel._label.text = "Alignment translates Level";
			panel._selectImage.sprite = BaseMenu.IconCheck;
			panel._button.onClick.RemoveAllListeners();
			panel._button.onClick.AddListener(delegate
			{
				_translateCustomLevelOnAlign = !_translateCustomLevelOnAlign;
			});
			panel._selectButton.onClick.RemoveAllListeners();
			panel._selectButton.onClick.AddListener(delegate
			{
				_translateCustomLevelOnAlign = !_translateCustomLevelOnAlign;
			});
			onRefreshN3(panel);
		};
		CompoundEntryPanelComponent.BasePanel item3 = CompoundEntryPanelComponent.Build(onReset3, onRefreshN3);
		builders.Add(item3);
		Action<LegacyButtonPanelComponent> onRefresh = delegate(LegacyButtonPanelComponent panel)
		{
			panel._button.interactable = NeedsCalculate;
		};
		Action<LegacyButtonPanelComponent> onReset4 = delegate(LegacyButtonPanelComponent panel)
		{
			panel._label.text = "Calculate bounds";
			panel._button.onClick.RemoveAllListeners();
			panel._button.onClick.AddListener(delegate
			{
				CalculateBounds();
			});
		};
		LegacyButtonPanelComponent.BasePanel item4 = LegacyButtonPanelComponent.Build(onReset4, onRefresh);
		builders.Add(item4);
		Action<LegacyButtonPanelComponent> onRefreshN4 = delegate(LegacyButtonPanelComponent panel)
		{
			panel._button.interactable = !NeedsCalculate;
		};
		Action<LegacyButtonPanelComponent> onReset5 = delegate(LegacyButtonPanelComponent panel)
		{
			panel._label.text = "Regenerate";
			panel._button.onClick.RemoveAllListeners();
			panel._button.onClick.AddListener(delegate
			{
				Regenerate();
			});
			onRefreshN4(panel);
		};
		LegacyButtonPanelComponent.BasePanel item5 = LegacyButtonPanelComponent.Build(onReset5, onRefreshN4);
		builders.Add(item5);
		Action<TitleTextPanelComponent> action = delegate(TitleTextPanelComponent panel)
		{
			panel._layoutElement.gameObject.SetActive(!NeedsCalculate);
			if (!NeedsCalculate)
			{
				string text = string.Empty;
				int y = GetDistance(_bounds, Alignment.Top, safe: false).y;
				int y2 = GetDistance(_bounds, Alignment.Bottom, safe: false).y;
				if (y != _yMax && y2 != -135)
				{
					bool flag = y < 40;
					bool flag2 = y2 < 8;
					flag = y < 0;
					flag2 = y2 < 0;
					Color color = ((!flag) ? Colors.Gray : Colors.Red);
					Color color2 = ((!flag2) ? Colors.Gray : Colors.Red);
					string text2 = $"#{(int)(color.r * 255f):X2}{(int)(color.g * 255f):X2}{(int)(color.b * 255f):X2}";
					string text3 = $"#{(int)(color2.r * 255f):X2}{(int)(color2.g * 255f):X2}{(int)(color2.b * 255f):X2}";
					text += "\n";
					string text4 = text;
					text = text4 + "Distance to top: <color=" + text2 + ">" + y + "</color>\n";
					text4 = text;
					text = text4 + "Distance to bottom: <color=" + text3 + ">" + y2 + "</color>\n";
					text += "\n";
					string text5 = "Expected translation: " + GetTranslation(_bounds, UndersideAlignment);
					text = text + text5 + "\n";
				}
				panel._label.text = text;
			}
			else
			{
				string empty = string.Empty;
				empty += "\n";
				empty += "No changes expected.\n";
				panel._label.text = empty;
			}
			panel._layoutElement.minHeight = panel._label.preferredHeight;
			panel._layoutElement.preferredHeight = panel._label.preferredHeight;
		};
		TitleTextPanelComponent.BasePanel item6 = TitleTextPanelComponent.Build(action, action).As(TitleTextPanelComponent.BasePanel.Variant.SidebarHelpText);
		builders.Add(item6);
		return builders;
	}
}
