using Brix.Audio;
using Brix.Lua;
using Brix.Utils;
using I2.Loc;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.NewUI.Tooltips;

public class Tooltip : MonoBehaviour
{
	public enum Width
	{
		narrow = 128,
		medium = 196,
		large = 256
	}

	[SerializeField]
	private LuaDriver Ld;

	[SerializeField]
	private RectTransform content;

	[SerializeField]
	private Text title;

	[SerializeField]
	private GameObject header;

	[SerializeField]
	private Image titleImage;

	[SerializeField]
	private Text mainText;

	[SerializeField]
	private Image subImage;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private Button nextButton;

	[SerializeField]
	private Text turnOffText;

	[SerializeField]
	private Text nextText;

	[SerializeField]
	private Image nextImage;

	[SerializeField]
	private TooltipPositioner positioner;

	[SerializeField]
	private Tooltipositioner3D positioner3D;

	[SerializeField]
	private TooltipArrows arrows;

	[SerializeField]
	private Width width;

	private RectTransform target;

	[LuaDrivable("hk_OnPressNext")]
	private static LuaHook<LuaDriver> lua_OnPressNext = new LuaHook<LuaDriver>();

	private void Start()
	{
		closeButton.onClick.AddListener(Clear);
		nextButton.onClick.AddListener(Clear);
		NoNextButton();
		NoTurnOffText();
		nextText.text = I2Helper.TryGet("##gamemenu_help_popup_next");
		turnOffText.text = I2Helper.TryGet("##gamemenu_help_popup_todisable");
	}

	public void SetTarget3D(GameObject target)
	{
		RectTransform target2D = positioner3D.SetTarget(target);
		SetTarget2D(target2D);
	}

	public void SetPositionOffset3D(Vector3 offset)
	{
		positioner3D.SetOffsetFromTarget(offset);
	}

	public void SetPositionOffset2D(Vector3 offset)
	{
		positioner.SetOffsetFromTarget(offset);
	}

	public void SetFollowCursor(bool value)
	{
		positioner.FollowCursor = value;
	}

	public void SetTarget2D(RectTransform target)
	{
		this.target = target;
		positioner.SetPositioningDonwCallback(AfterPositioning);
		positioner.RequestPositioning(content, target);
		Hide();
	}

	private void AfterPositioning()
	{
		Show();
		arrows.Follow(MainRect(), target);
	}

	public void SetPositionPriority(TooltipPositioner.Direction direction)
	{
		positioner.RequestPositioning(content, target);
		positioner.SetDirectionPriority(direction);
		Hide();
	}

	public void SetWidth(Width width)
	{
		content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (float)width);
	}

	public void SetFollowDynamic(bool value)
	{
		positioner.PositionIsDynamic(value);
	}

	public RectTransform MainRect()
	{
		return content;
	}

	public void Show()
	{
		content.GetComponent<Mask>().enabled = false;
		content.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.9f);
		if (!content.gameObject.activeInHierarchy)
		{
			SoundEngine.Play("Dialog_Appear");
		}
		content.gameObject.SetActive(value: true);
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		content.GetComponent<Mask>().enabled = true;
		content.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
		content.gameObject.SetActive(value: true);
		base.gameObject.SetActive(value: false);
	}

	public void Clear()
	{
		GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
		NoArrow();
		NoImage();
		NoMainText();
		NoSubImage();
		NoTitle();
		Hide();
	}

	public void SetTitleImage(Sprite img)
	{
		if (img != null)
		{
			header.SetActive(value: true);
			titleImage.sprite = img;
		}
		else
		{
			NoImage();
		}
	}

	public void NoImage()
	{
		header.gameObject.SetActive(value: false);
	}

	public void NoCloseButton()
	{
		closeButton.gameObject.SetActive(value: false);
	}

	public void NoTitle()
	{
		title.gameObject.SetActive(value: false);
	}

	public void NoTurnOffText()
	{
		turnOffText.gameObject.SetActive(value: false);
	}

	public void NoNextButton()
	{
		nextButton.gameObject.SetActive(value: false);
	}

	public void DisableNextButton()
	{
		nextButton.interactable = false;
		nextText.color = new Color(0.6f, 0.6f, 0.6f, 1f);
		nextImage.color = new Color(0.6f, 0.6f, 0.6f, 1f);
	}

	public void EnableNextButton()
	{
		nextButton.interactable = true;
		nextText.color = new Color(1f, 0.7725f, 0.0392f, 1f);
		nextImage.color = new Color(1f, 0.7725f, 0.0392f, 1f);
	}

	public void SetTitle(string text)
	{
		if (!string.IsNullOrEmpty(text))
		{
			title.gameObject.SetActive(value: true);
			title.text = text;
			title.gameObject.GetComponent<Localize>().SetTerm(text);
		}
		else
		{
			NoTitle();
		}
	}

	public void TextAlignement(TextAnchor alignement)
	{
		mainText.alignment = alignement;
	}

	public void SetSubImage(Sprite img)
	{
		if (img != null)
		{
			subImage.gameObject.SetActive(img);
			subImage.sprite = img;
		}
		else
		{
			NoSubImage();
		}
	}

	public void NoSubImage()
	{
		subImage.gameObject.SetActive(value: false);
	}

	public void SetMainText(string text)
	{
		if (!string.IsNullOrEmpty(text))
		{
			mainText.gameObject.SetActive(value: true);
			mainText.text = text;
			mainText.gameObject.GetComponent<Localize>().SetTerm(text);
			mainText.text = mainText.text.Replace("\\n", "\n");
		}
		else
		{
			NoMainText();
		}
	}

	public void NoMainText()
	{
		mainText.gameObject.SetActive(value: false);
	}

	public void WithArrow()
	{
		arrows.Follow(MainRect(), target);
	}

	public void NoArrow()
	{
		arrows.NoFollow();
	}

	public void SetPosition(Vector2 pos)
	{
		content.anchoredPosition = pos;
	}

	public Vector2 Size()
	{
		return content.sizeDelta;
	}

	public void SelfDestruct()
	{
		Object.Destroy(base.gameObject);
	}

	[LuaDrivable("Target")]
	private static DynValue lua_SetTarget3D(ScriptExecutionContext context, CallbackArguments args)
	{
		Tooltip tooltip = args.AsComponent<Tooltip>(0, "Target");
		GameObject target3D = args.AsGameObject(1, "Target");
		tooltip.SetTarget3D(target3D);
		return DynValue.Void;
	}

	[LuaDrivable("Offset")]
	private static DynValue lua_SetPositionOffset3D(ScriptExecutionContext context, CallbackArguments args)
	{
		Tooltip tooltip = args.AsComponent<Tooltip>(0, "Offset");
		Vector3 offsetFromTarget = args.AsUserData<Vector3>(1, "Offset");
		tooltip.positioner3D.SetOffsetFromTarget(offsetFromTarget);
		return DynValue.Void;
	}

	[LuaDrivable("Show")]
	private static DynValue lua_Show(ScriptExecutionContext context, CallbackArguments args)
	{
		Tooltip tooltip = args.AsComponent<Tooltip>(0, "Show");
		tooltip.Show();
		return DynValue.Void;
	}

	[LuaDrivable("Hide")]
	private static DynValue lua_Hide(ScriptExecutionContext context, CallbackArguments args)
	{
		Tooltip tooltip = args.AsComponent<Tooltip>(0, "Hide");
		tooltip.Hide();
		return DynValue.Void;
	}

	[LuaDrivable("TitleImage")]
	private static DynValue lua_TitleImage(ScriptExecutionContext context, CallbackArguments args)
	{
		Tooltip tooltip = args.AsComponent<Tooltip>(0, "TitleImage");
		Sprite sprite = args.AsUserData<Sprite>(1, "TitleImage", allowNil: true);
		tooltip.SetTitleImage(sprite);
		return DynValue.Void;
	}

	[LuaDrivable("Title")]
	private static DynValue lua_Title(ScriptExecutionContext context, CallbackArguments args)
	{
		Tooltip tooltip = args.AsComponent<Tooltip>(0, "Title");
		string text = args.AsString(1, "Title", allowNil: true);
		tooltip.SetTitle(text);
		return DynValue.Void;
	}

	[LuaDrivable("Image")]
	private static DynValue lua_SetSubImage(ScriptExecutionContext context, CallbackArguments args)
	{
		Tooltip tooltip = args.AsComponent<Tooltip>(0, "Image");
		Sprite sprite = args.AsUserData<Sprite>(1, "Image", allowNil: true);
		tooltip.SetSubImage(sprite);
		return DynValue.Void;
	}

	[LuaDrivable("Text")]
	private static DynValue lua_SetMainText(ScriptExecutionContext context, CallbackArguments args)
	{
		Tooltip tooltip = args.AsComponent<Tooltip>(0, "Text");
		string text = args.AsString(1, "Text", allowNil: true);
		tooltip.SetMainText(text);
		return DynValue.Void;
	}
}
