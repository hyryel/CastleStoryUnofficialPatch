using UnityEngine;
using UnityEngine.UI;

namespace Brix.NewUI.OverheadDisplay;

public class BigIcon : Overhead
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private Image background;

	[SerializeField]
	private Image foreground;

	private Color defaultColor;

	public override int PreferedIndex => 4;

	public float Size
	{
		set
		{
			icon.gameObject.transform.localScale = Vector3.one * value;
		}
	}

	public Color Color
	{
		get
		{
			return background.color;
		}
		set
		{
			background.color = value;
		}
	}

	public void SetIcon(Sprite sprite)
	{
		icon.sprite = sprite;
		base.Show = true;
	}

	public void NoIcon()
	{
		icon.sprite = null;
		base.Show = false;
	}

	public void Init()
	{
	}

	public override void Initialize()
	{
		base.Initialize();
		defaultColor = Color;
	}

	public override void OnRequested()
	{
		base.OnRequested();
		ResetComponent();
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		NoIcon();
		Color = defaultColor;
		overhead = null;
	}
}
