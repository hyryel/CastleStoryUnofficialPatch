using System;
using Brix.External.Factories;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Brix.UI.Builder.Panel;

public class ThumbnailPanelComponent : BasePanelComponent
{
	public enum Variant
	{
		Default,
		ListItem,
		ServerListItem,
		PlayerListItem
	}

	public class BasePanel : BasePanel<ThumbnailPanelComponent>
	{
		private Factory.AssetKey _assetKey = Buttons.Thumbnail;

		protected override Factory.AssetKey AssetKey => _assetKey;

		public BasePanel(Action<ThumbnailPanelComponent> onReset, Action<ThumbnailPanelComponent> onRefresh)
			: base(onReset, onRefresh, luaDriven: false)
		{
		}

		public void As(Variant variant)
		{
			switch (variant)
			{
			case Variant.Default:
				_assetKey = Buttons.Thumbnail;
				break;
			case Variant.ListItem:
				_assetKey = Buttons.ThumbnailListItem;
				break;
			case Variant.ServerListItem:
				_assetKey = Buttons.ServerListItem;
				break;
			case Variant.PlayerListItem:
				_assetKey = Buttons.PlayerListItem;
				break;
			}
		}
	}

	public Button _button;

	public EventTrigger _eventTrigger;

	public Image _image;

	public RawImage _rawImage;

	public Text _label;

	public Text _subLabel;

	public Text _infoLabel;

	public Animator _animator;

	public Image _badgeImage;

	public Image _defaultImage;

	public Text _defaultLabel;

	public Text _playerNameText;

	public Text _playerIsReadyText;

	public Image _bgImage;

	public Image _pingImage;

	public Image _pingBgImage;

	public Image _factionImage;

	public GameObject _downloadGo;

	public Image _downloadImage;

	public GameObject _avatarGo;

	public RawImage _avatarRawImage;

	public Image _avatarBgImage;

	public Button _avatarButton;

	public EventTrigger _avatarEventTrigger;

	public static BasePanel Build(Action<ThumbnailPanelComponent> onReset = null, Action<ThumbnailPanelComponent> onRefresh = null)
	{
		return new BasePanel(onReset, onRefresh);
	}
}
