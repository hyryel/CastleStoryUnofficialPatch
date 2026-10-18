using Brix.Engine;
using Brix.External.Factories;
using Brix.Game.Components;
using Brix.Lifecycle.Pooling;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.NewUI.OverheadDisplay;

public class NameTag : Overhead, IPoolableComponent, IInitializable, IRequestable, IResetable
{
	[SerializeField]
	private Image FillImage;

	[SerializeField]
	private Image Icon;

	[SerializeField]
	private Image Badge;

	[SerializeField]
	private Text Name;

	[SerializeField]
	private Text Context;

	[SerializeField]
	private RectTransform content;

	[SerializeField]
	private AnimatorController animator;

	[SerializeField]
	private TogglableHorizontalLayoutGroup layoutGroup;

	public override int PreferedIndex => 0;

	public bool HighlightedForOverhead()
	{
		return animator.GetBool("Highlight");
	}

	public void HighlightForOverhead()
	{
		animator.SetBool("Highlight", value: true);
	}

	public void RemoveHighlightForOverhead()
	{
		animator.SetBool("Highlight", value: false);
	}

	public void WithIcon(Sprite img, bool forceRefresh = false)
	{
		if (forceRefresh)
		{
			Icon.sprite = null;
			Icon.sprite = img;
			Badge.gameObject.SetActive(value: true);
			RefreshUI();
		}
		else if (Icon.sprite != img || !Badge.gameObject.activeSelf)
		{
			Icon.sprite = img;
			Badge.gameObject.SetActive(value: true);
			RefreshUI();
		}
	}

	public void TextColor(Color color)
	{
		Name.color = color;
	}

	public void NoIcon()
	{
		Badge.gameObject.SetActive(value: false);
		RefreshUI();
	}

	public void SetName(string name, bool forceRefresh = false)
	{
		if (forceRefresh)
		{
			Name.text = string.Empty;
			Name.text = name;
			Neo.Relais(RefreshUI, "NameTag refresh");
		}
		else if (Name.text != name)
		{
			Name.text = name;
			Neo.Relais(RefreshUI, "NameTag refresh");
		}
	}

	public string GetName()
	{
		return Name.text;
	}

	public void SetContext(string text)
	{
		Context.text = "(" + text + ")";
		Context.gameObject.SetActive(value: true);
		RefreshUI();
	}

	public void NoContext()
	{
		if ((bool)Context)
		{
			Context.gameObject.SetActive(value: false);
		}
		RefreshUI();
	}

	public void NoName()
	{
		Name.text = string.Empty;
		base.Show = false;
		RefreshUI();
	}

	public void JustIcon(bool value)
	{
		Name.gameObject.SetActive(!value);
		Badge.enabled = !value;
	}

	public void RefreshUI()
	{
		if (layoutGroup != null)
		{
			layoutGroup.Refresh();
		}
	}

	public override void OnRequested()
	{
		base.OnRequested();
		JustIcon(value: false);
		NoName();
		NoIcon();
		NoContext();
		overhead = null;
		RefreshUI();
	}
}
