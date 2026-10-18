using System;
using System.Collections.Generic;
using System.IO;
using Brix.External.Signals;
using Brix.UI.Builder.Panel;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public abstract class BaseSaveActionsMenu : BaseMenu
{
	private static readonly Signal ReopenSignal = new Signal();

	protected BaseSaveActionsMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	protected static BaseSaveActionsMenu BaseBuild(BaseSaveActionsMenu menuN, Func<bool> getAllowPublishValue, Action onExecuteAction)
	{
		BuilderUtility.AssignTree(menuN);
		return menuN;
	}

	public static List<BasePanel> GetBuilders(Func<bool> getAllowPublishValue, Action onExecuteAction)
	{
		List<BasePanel> list = new List<BasePanel>();
		List<BasePanel> list2 = new List<BasePanel>();
		list2.AddRange(GetPublishBuilders(getAllowPublishValue, onExecuteAction));
		BaseLayoutPanel<VerticalLayoutPanelComponent> item = VerticalLayoutPanel.New().AddPanels_2(list2.ToArray());
		list.Add(item);
		return list;
	}

	private static List<BasePanel> GetPublishBuilders(Func<bool> getAllowPublishValue, Action onExecuteAction)
	{
		List<BasePanel> list = new List<BasePanel>();
		Action<LegacyButtonPanelComponent> onReset = delegate(LegacyButtonPanelComponent panel)
		{
			panel._label.text = "Save";
			bool flag = getAllowPublishValue();
			panel._button.interactable = flag;
			panel._button.onClick.RemoveAllListeners();
			if (flag)
			{
				panel._button.onClick.AddListener(delegate
				{
					onExecuteAction();
					ReopenSignal.Invoke();
				});
			}
		};
		LegacyButtonPanelComponent.BasePanel item = LegacyButtonPanelComponent.Build(onReset);
		list.Add(item);
		return list;
	}

	protected static string GetValidFileName(string srcValue)
	{
		return Path.GetInvalidFileNameChars().Slinq().Aggregate(srcValue, (string current, char c) => current.Replace(char.ToString(c), string.Empty))
			.Replace(" ", string.Empty);
	}

	protected override SignalHandle<MenuState> AddSignals()
	{
		return base.AddSignals().AddSignalConnection(MenuState.Opened, ReopenSignal, BaseMenu.OnReopen, this);
	}
}
