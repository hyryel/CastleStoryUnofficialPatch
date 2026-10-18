using System;
using UnityEngine.UI;

namespace Brix.NewUI.BreadBar;

public class BreadBarCrumbBtn : BreadBarCrumb
{
	private Action _clickAction;

	private bool _clickActionEnabled;

	public void SetClickAction(Action clickAction, bool clickActionEnabled = true)
	{
		_clickAction = clickAction;
		_clickActionEnabled = clickActionEnabled;
		RefreshButtonUI();
	}

	public override void Clear()
	{
		base.Clear();
		SetClickAction(null, clickActionEnabled: false);
	}

	protected override void RefreshUI()
	{
		base.RefreshUI();
		RefreshButtonUI();
	}

	private void RefreshButtonUI()
	{
		this.ResetPlugCache();
		Button plugComponent = this.GetPlugComponent<Button>("Button");
		if (!(plugComponent != null))
		{
			return;
		}
		plugComponent.interactable = _clickActionEnabled;
		plugComponent.onClick.RemoveAllListeners();
		plugComponent.onClick.AddListener(delegate
		{
			if (_clickAction != null)
			{
				_clickAction();
			}
		});
		plugComponent.onClick.AddListener(delegate
		{
			if (_clickAction != null)
			{
				MenuSound.PlaySound(MenuSound.Prefix.Title, MenuSound.Suffix.Back);
			}
		});
	}
}
