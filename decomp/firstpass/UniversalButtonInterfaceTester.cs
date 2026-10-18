using UnityEngine;

public class UniversalButtonInterfaceTester : MonoBehaviour
{
	public UniversalButtonInterface Button;

	public Sprite sprite;

	public string str;

	public UniversalButtonInterface.ContentModes buttonMode;

	public bool boolValue;

	public UniversalButtonInterface.Colors color;

	public UniversalButtonInterface.HiglightsModes highlightMode;

	public bool SetIconForContent;

	public bool SetText;

	public bool SetKey;

	public bool SwitchContent;

	public bool CycleToNextIcon;

	public bool EnableClick;

	public bool ShowBackgroundByDefault;

	public bool SetHighlight;

	public bool SetColor;

	private void Update()
	{
		if (SetIconForContent)
		{
			Button.SetIconForContent(sprite, buttonMode);
			SetIconForContent = false;
		}
		if (SetText)
		{
			Button.SetText(str);
			SetText = false;
		}
		if (SetKey)
		{
			Button.SetKey(str);
			SetKey = false;
		}
		if (SwitchContent)
		{
			Button.SwitchContent(buttonMode);
			SwitchContent = false;
		}
		if (CycleToNextIcon)
		{
			Button.CycleToNextIcon();
			CycleToNextIcon = false;
		}
		if (EnableClick)
		{
			Button.EnableClick(boolValue);
			EnableClick = false;
		}
		if (ShowBackgroundByDefault)
		{
			Button.ShowBackgroundByDefault(boolValue);
			ShowBackgroundByDefault = false;
		}
		if (SetHighlight)
		{
			Button.SetHighlight(highlightMode);
			SetHighlight = false;
		}
		if (SetColor)
		{
			Button.SetTextColor(color);
			SetColor = false;
		}
	}
}
