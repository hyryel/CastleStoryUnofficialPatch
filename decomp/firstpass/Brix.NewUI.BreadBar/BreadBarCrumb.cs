using System;
using System.Collections.Generic;
using Brix.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.NewUI.BreadBar;

public class BreadBarCrumb : MonoBehaviour, IPlug
{
	public static readonly Color castleGrayColor = new Color(0.5019608f, 0.5019608f, 0.5019608f, 1f);

	public static readonly Color castleYellowColor = new Color(1f, 0.77254903f, 2f / 51f, 1f);

	public static readonly Color castleRedColor = new Color(1f, 0f, 0.2509804f, 1f);

	[SerializeField]
	private List<Plug> m_Plugs = new List<Plug>();

	private Dictionary<Type, Dictionary<string, Plug>> m_PlugMap = new Dictionary<Type, Dictionary<string, Plug>>();

	protected string _title;

	private Color _titleColor;

	protected bool _titleColorOverride;

	protected bool _titleColorDirty;

	private Sprite _icon;

	private Color _iconColor;

	protected bool _iconColorOverride;

	protected bool _iconColorDirty;

	public List<Plug> Plugs => m_Plugs;

	public Dictionary<Type, Dictionary<string, Plug>> PlugMap => m_PlugMap;

	protected virtual void OnEnable()
	{
		this.ResetPlugCache();
	}

	public void SetLabel(string title, Color titleColor)
	{
		_title = I2Helper.TryGet(title);
		_titleColorOverride = _titleColor != titleColor;
		_titleColor = titleColor;
		_titleColorOverride = true;
		RefreshTitleTextUI();
	}

	public void SetLabel(string title)
	{
		_title = I2Helper.TryGet(title);
		_titleColorOverride = false;
		RefreshTitleTextUI();
	}

	public void SetIcon(Sprite icon, Color iconColor)
	{
		_icon = icon;
		_iconColorDirty = _iconColor != iconColor;
		_iconColor = iconColor;
		_iconColorOverride = true;
		RefreshIconImageUI();
	}

	public void SetIcon(Sprite icon)
	{
		_icon = icon;
		_iconColorOverride = false;
		RefreshIconImageUI();
	}

	public virtual void Clear()
	{
		SetLabel(string.Empty, castleYellowColor);
		SetIcon(null, castleYellowColor);
	}

	protected virtual void RefreshUI()
	{
		RefreshTitleTextUI();
		RefreshIconImageUI();
	}

	protected void RefreshTitleTextUI()
	{
		this.ResetPlugCache();
		Animator plugComponent = this.GetPlugComponent<Animator>("Animator");
		Text plugComponent2 = this.GetPlugComponent<Text>("Title");
		if (!(plugComponent2 != null))
		{
			return;
		}
		plugComponent2.text = _title;
		if (_titleColorOverride)
		{
			if (_titleColorDirty)
			{
				bool activeSelf = plugComponent.gameObject.activeSelf;
				plugComponent.gameObject.SetActive(value: false);
				plugComponent2.color = _titleColor;
				plugComponent.gameObject.SetActive(activeSelf);
				_titleColorDirty = false;
			}
			else
			{
				plugComponent2.color = _titleColor;
			}
		}
	}

	protected void RefreshIconImageUI()
	{
		this.ResetPlugCache();
		Animator plugComponent = this.GetPlugComponent<Animator>("Animator");
		Image plugComponent2 = this.GetPlugComponent<Image>("Icon");
		if (!(plugComponent2 != null))
		{
			return;
		}
		plugComponent2.sprite = _icon;
		if (_iconColorOverride)
		{
			if (_iconColorDirty)
			{
				bool activeSelf = plugComponent.gameObject.activeSelf;
				plugComponent.gameObject.SetActive(value: false);
				plugComponent2.color = _iconColor;
				plugComponent.gameObject.SetActive(activeSelf);
				_iconColorDirty = false;
			}
			else
			{
				plugComponent2.color = _iconColor;
			}
		}
	}

	public void Toggle(bool flag)
	{
		if (this != null)
		{
			base.gameObject.SetActive(flag);
		}
	}

	public void Show()
	{
		Toggle(flag: true);
	}

	public void Hide()
	{
		Toggle(flag: false);
	}
}
