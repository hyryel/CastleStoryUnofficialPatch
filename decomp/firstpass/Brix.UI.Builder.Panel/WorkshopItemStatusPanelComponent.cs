using System;
using Brix.External.Factories;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI.Builder.Panel;

public class WorkshopItemStatusPanelComponent : BasePanelComponent
{
	public class BasePanel : BasePanel<WorkshopItemStatusPanelComponent>
	{
		protected override Factory.AssetKey AssetKey => Panels.WorkshopItemStatus;

		public BasePanel(Action<WorkshopItemStatusPanelComponent> onReset, Action<WorkshopItemStatusPanelComponent> onRefresh)
			: base(onReset, onRefresh, luaDriven: false)
		{
		}
	}

	public GameObject _contentGo;

	public Image _previewImage;

	public Text _titleLabel;

	public Text _infoLabel;

	public Image _progressImage;

	public static BasePanel Add(Action<WorkshopItemStatusPanelComponent> onReset = null, Action<WorkshopItemStatusPanelComponent> onRefresh = null)
	{
		return new BasePanel(onReset, onRefresh);
	}
}
