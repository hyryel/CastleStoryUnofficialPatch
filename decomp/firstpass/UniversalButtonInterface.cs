using System.Collections.Generic;
using System.Linq;
using Brix.Components;
using Brix.External.Signals;
using Brix.Lua;
using Brix.Utils;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;

public class UniversalButtonInterface : BrixComponent
{
	public enum ContentModes
	{
		PrimaryIcon,
		SecondaryIcon,
		TerciaryIcon,
		Counter,
		Invalid
	}

	public enum Colors
	{
		Yellow,
		Red,
		Blue,
		Green,
		White,
		Grey,
		Invalid
	}

	public enum HiglightsModes
	{
		Bottom,
		Top,
		Left,
		Right,
		All,
		None,
		Invalid
	}

	private class ColorCache
	{
		private static Dictionary<string, Dictionary<Colors, int>> hashes = new Dictionary<string, Dictionary<Colors, int>>();

		private static Dictionary<Color, Colors> colorEquivalents = new Dictionary<Color, Colors>
		{
			{
				Color.yellow,
				Colors.Yellow
			},
			{
				Color.red,
				Colors.Red
			},
			{
				Color.blue,
				Colors.Blue
			},
			{
				Color.green,
				Colors.Green
			},
			{
				Color.white,
				Colors.White
			},
			{
				Color.grey,
				Colors.Grey
			}
		};

		private static Dictionary<Colors, string> ColorStrings()
		{
			Dictionary<Colors, string> dictionary = new Dictionary<Colors, string>();
			dictionary.Add(Colors.Yellow, "Yellow");
			dictionary.Add(Colors.Red, "Red");
			dictionary.Add(Colors.Blue, "Blue");
			dictionary.Add(Colors.Green, "Green");
			dictionary.Add(Colors.White, "White");
			dictionary.Add(Colors.Grey, "Grey");
			return dictionary;
		}

		private static Dictionary<Colors, int> ColorStringToAnimatorParams(Dictionary<Colors, string> colors, string suffix)
		{
			return colors.ToDictionary((KeyValuePair<Colors, string> kvp) => kvp.Key, (KeyValuePair<Colors, string> kvp) => Animator.StringToHash(kvp.Value + suffix));
		}

		public static Dictionary<Color, Colors> ColorEquivalents()
		{
			return colorEquivalents;
		}

		public static Dictionary<Colors, int> ColorParams(string suffix)
		{
			return hashes.GetValueOrCreate(suffix, ColorStringToAnimatorParams(ColorStrings(), suffix));
		}
	}

	private ContentModes initialContentMode = ContentModes.Invalid;

	private HiglightsModes initialHighlight = HiglightsModes.Invalid;

	private Colors initialTextColor = Colors.Invalid;

	private Colors initialIconColor = Colors.Invalid;

	private Colors currentTextColor = Colors.Invalid;

	private Colors currentIconColor = Colors.Invalid;

	private bool? initialShowBackgroundbyDefault;

	private bool? initialDisabled;

	private bool? initialEnableHover;

	private bool? initialEnableClick;

	private bool? initialFlashOnHighlight;

	private UniversalButtonItem _menuItem;

	private Dictionary<Colors, int> textColors;

	private Dictionary<Colors, int> iconColors;

	private Dictionary<Color, Colors> colorEquivalents;

	public Signal EnabledSignal = new Signal();

	public Signal DisabledSignal = new Signal();

	[SerializeField]
	private Animator InputAnimator;

	[SerializeField]
	private Animator HighlightAnimator;

	[SerializeField]
	private Animator ColorAnimator;

	[SerializeField]
	private Animator ContentChangeAnimator;

	[SerializeField]
	private Image uiIconPrimary;

	[SerializeField]
	private Image uiIconSecondary;

	[SerializeField]
	private Image uiIconTerciary;

	[SerializeField]
	private Image uiIconUncolored;

	[SerializeField]
	private Image uiIconCounter;

	[SerializeField]
	private Text uiCountText;

	[SerializeField]
	private Text uiText;

	[SerializeField]
	private Text uiKey;

	private int iconCyclingIndex;

	[LuaDrivable("name")]
	private string lua_Name
	{
		get
		{
			return _menuItem.name;
		}
		set
		{
			_menuItem.name = value;
			_menuItem.GenerateOrUpdateButton();
		}
	}

	[LuaDrivable("showBackgroundByDefault")]
	private bool lua_showBackgroundByDefault
	{
		get
		{
			return _menuItem.showBackgroundByDefault;
		}
		set
		{
			_menuItem.showBackgroundByDefault = value;
			_menuItem.GenerateOrUpdateButton();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		textColors = ColorCache.ColorParams("Text");
		iconColors = ColorCache.ColorParams("Icon");
		colorEquivalents = ColorCache.ColorEquivalents();
	}

	private void InitAnimatorValues()
	{
		if (initialContentMode != ContentModes.Invalid)
		{
			SwitchContent(initialContentMode);
		}
		if (initialTextColor != Colors.Invalid)
		{
			SetTextColor(currentTextColor);
		}
		if (initialIconColor != Colors.Invalid)
		{
			SetIconColor(currentIconColor);
		}
		if (initialHighlight != HiglightsModes.Invalid)
		{
			SetHighlight(initialHighlight);
		}
		if (initialShowBackgroundbyDefault.HasValue)
		{
			ShowBackgroundByDefault(initialShowBackgroundbyDefault.Value);
		}
		if (initialDisabled.HasValue)
		{
			Disable(initialDisabled.Value);
		}
		if (initialEnableClick.HasValue)
		{
			EnableClick(initialEnableClick.Value);
		}
		if (initialEnableHover.HasValue)
		{
			EnableHover(initialEnableHover.Value);
		}
		if (initialFlashOnHighlight.HasValue)
		{
			EnableFlashOnHighlight(initialFlashOnHighlight.Value);
		}
	}

	private void OnEnable()
	{
		InitAnimatorValues();
		EnabledSignal.Invoke();
	}

	private void OnDisable()
	{
		DisabledSignal.Invoke();
	}

	public void SetIconForContent(Sprite icon, ContentModes mode)
	{
		Image image = mode switch
		{
			ContentModes.PrimaryIcon => uiIconPrimary, 
			ContentModes.SecondaryIcon => uiIconSecondary, 
			ContentModes.TerciaryIcon => uiIconTerciary, 
			ContentModes.Counter => uiIconCounter, 
			_ => null, 
		};
		if ((bool)image)
		{
			image.sprite = icon;
		}
	}

	public void SetText(string value)
	{
		if ((bool)uiText)
		{
			uiText.text = I2Helper.TryGet(value);
		}
	}

	public void StandardHighlight(bool value)
	{
		ShowTextAlways(value);
		if (value)
		{
			SetHighlight(HiglightsModes.Bottom);
		}
		else
		{
			SetHighlight(HiglightsModes.None);
		}
	}

	public void SetCountText(string value)
	{
		if ((bool)uiCountText)
		{
			uiCountText.text = value;
		}
	}

	public void SetKey(string value)
	{
		if ((bool)uiKey)
		{
			uiKey.text = value;
		}
	}

	private string IconCycleTrigger(int i)
	{
		return i switch
		{
			0 => "ShowPrimary", 
			1 => "ShowSecondary", 
			2 => "ShowSecondary", 
			_ => "ShowPrimary", 
		};
	}

	private int ModeIndex(ContentModes mode)
	{
		return iconCyclingIndex = mode switch
		{
			ContentModes.PrimaryIcon => 0, 
			ContentModes.SecondaryIcon => 1, 
			ContentModes.TerciaryIcon => 2, 
			_ => 0, 
		};
	}

	public void SwitchContent(ContentModes mode)
	{
		if ((bool)ContentChangeAnimator && ContentChangeAnimator.isInitialized && mode != ContentModes.Invalid)
		{
			string trigger = ((mode != ContentModes.Counter) ? IconCycleTrigger(ModeIndex(mode)) : "ShowCounter");
			ContentChangeAnimator.SetTrigger(trigger);
		}
		else
		{
			initialContentMode = mode;
		}
	}

	public void CycleToNextIcon()
	{
		if ((bool)ContentChangeAnimator && ContentChangeAnimator.isInitialized)
		{
			iconCyclingIndex = (iconCyclingIndex + 1) % 3;
			string trigger = IconCycleTrigger(iconCyclingIndex);
			ContentChangeAnimator.SetTrigger(trigger);
		}
		else
		{
			initialContentMode = ContentModes.PrimaryIcon;
		}
	}

	public void Disable(bool value)
	{
		if ((bool)InputAnimator && InputAnimator.isInitialized)
		{
			InputAnimator.SetBool("Disabled", value);
		}
		else
		{
			initialDisabled = value;
		}
	}

	public void EnableClick(bool value)
	{
		if ((bool)InputAnimator && InputAnimator.isInitialized)
		{
			InputAnimator.SetBool("CanPress", value);
		}
		else
		{
			initialEnableClick = value;
		}
	}

	public void EnableHover(bool value)
	{
		if ((bool)InputAnimator && InputAnimator.isInitialized)
		{
			InputAnimator.SetBool("CanHover", value);
		}
		else
		{
			initialEnableHover = value;
		}
	}

	public void EnableFlashOnHighlight(bool value)
	{
		if ((bool)InputAnimator && InputAnimator.isInitialized)
		{
			InputAnimator.SetFloat("FlashOnHighlight", (!value) ? 0f : 1f);
		}
		else
		{
			initialFlashOnHighlight = value;
		}
	}

	public void MakeFlash()
	{
		if ((bool)InputAnimator && InputAnimator.isInitialized)
		{
			InputAnimator.SetTrigger("GoBackToNormal");
			InputAnimator.SetTrigger("Highlighted");
		}
	}

	public void ShowBackgroundByDefault(bool value)
	{
		if ((bool)InputAnimator && InputAnimator.isInitialized)
		{
			InputAnimator.SetBool("Discreet", !value);
		}
		else
		{
			initialShowBackgroundbyDefault = value;
		}
	}

	public void ShowTextAlways(bool value)
	{
		InputAnimator.SetFloat("ShowTextAlways", (!value) ? 0f : 1f);
	}

	public void SetHighlight(HiglightsModes mode)
	{
		if ((bool)HighlightAnimator && HighlightAnimator.isInitialized && mode != HiglightsModes.Invalid)
		{
			HighlightAnimator.SetTrigger(mode switch
			{
				HiglightsModes.Bottom => "Bottom", 
				HiglightsModes.Top => "Top", 
				HiglightsModes.Left => "Left", 
				HiglightsModes.Right => "Right", 
				HiglightsModes.All => "All", 
				_ => "None", 
			});
		}
		else
		{
			initialHighlight = mode;
		}
	}

	public void SetTextColor(Color color)
	{
		SetTextColor(colorEquivalents[color]);
	}

	public void SetIconColor(Color color)
	{
		SetIconColor(colorEquivalents[color]);
	}

	public void SetTextColor(Colors color)
	{
		currentTextColor = color;
		if ((bool)ColorAnimator && ColorAnimator.isInitialized && color != Colors.Invalid)
		{
			ColorAnimator.SetTrigger(textColors[color]);
		}
		else
		{
			initialTextColor = color;
		}
	}

	public void SetIconColor(Colors color)
	{
		currentIconColor = color;
		if ((bool)ColorAnimator && ColorAnimator.isInitialized && color != Colors.Invalid)
		{
			ColorAnimator.SetTrigger(iconColors[color]);
		}
		else
		{
			initialIconColor = color;
		}
	}

	[LuaDrivable("NewButton")]
	private static DynValue lua_NewButton(ScriptExecutionContext context, CallbackArguments args)
	{
		string text = args.AsType(0, "NewButton", DataType.String).String;
		Sprite icon = args.AsUserData<Sprite>(1, "NewButton", allowNil: true);
		Closure onClick = args.AsType(2, "NewButton", DataType.Function).Function;
		Color color = args.AsUserData<Color>(3, "NewButton");
		string tooltip = args.AsType(4, "NewButton", DataType.String, allowNil: true).String;
		string key = args.AsType(5, "NewButton", DataType.String, allowNil: true).String;
		UniversalButtonItem universalButtonItem = UniversalButtonItem.NewButton(text, icon, delegate
		{
			onClick.Call();
		}, color, colorIcon: true, tooltip, key);
		UniversalButtonInterface universalButtonInterface = universalButtonItem.GenerateOrUpdateButton();
		universalButtonInterface._menuItem = universalButtonItem;
		return DynValue.FromObject(context.OwnerScript, universalButtonInterface.GetComponent<LuaDriver>());
	}
}
