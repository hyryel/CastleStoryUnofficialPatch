using System;
using System.Collections.Generic;
using Brix.UI.Builder.Panel;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public abstract class BaseInfoMenu : BaseMenu
{
	private string _message = string.Empty;

	protected virtual string Message => _message;

	protected BaseInfoMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	public List<BasePanel> GetBuilders()
	{
		List<BasePanel> list = new List<BasePanel>();
		Action<TitleTextPanelComponent> action = delegate(TitleTextPanelComponent panel)
		{
			panel._label.text = Message;
			panel._label.alignment = TextAnchor.UpperLeft;
			panel._layoutElement.minHeight = panel._label.preferredHeight;
			panel._fitter.enabled = false;
			panel._layoutElement.preferredHeight = -1f;
		};
		TitleTextPanelComponent.BasePanel item = TitleTextPanelComponent.Build(action, action).As(TitleTextPanelComponent.BasePanel.Variant.SidebarHelpText);
		list.Add(item);
		return list;
	}
}
