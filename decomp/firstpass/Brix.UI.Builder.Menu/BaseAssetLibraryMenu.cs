using System;
using System.Collections.Generic;
using Brix.Assets;
using Brix.Engine;
using Brix.Steam;
using Brix.UI.Builder.Panel;
using Brix.UI.Builder.Widget;
using Brix.UI.Icons;
using Smooth.Slinq;
using Steamworks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Brix.UI.Builder.Menu;

public abstract class BaseAssetLibraryMenu<TAsset> : BaseAssetMenu where TAsset : Asset_Base<TAsset>
{
	public class Section
	{
		public Func<string> GetLabel;

		public bool HasHeader;

		public Func<List<TAsset>, List<TAsset>> ToFiltered;

		public Func<List<TAsset>, List<TAsset>> ToSorted;

		public Section(Func<string> getLabel, Func<List<TAsset>, List<TAsset>> toFiltered, Func<List<TAsset>, List<TAsset>> toSorted)
		{
			GetLabel = getLabel;
			HasHeader = true;
			ToFiltered = toFiltered;
			ToSorted = toSorted;
		}

		public Section(Func<string> getLabel, Func<List<TAsset>, List<TAsset>> toFiltered)
		{
			GetLabel = getLabel;
			HasHeader = true;
			ToFiltered = toFiltered;
			ToSorted = (List<TAsset> assets) => assets;
		}

		public Section(Func<List<TAsset>, List<TAsset>> toFiltered, Func<List<TAsset>, List<TAsset>> toSorted)
		{
			GetLabel = () => string.Empty;
			HasHeader = false;
			ToFiltered = toFiltered;
			ToSorted = toSorted;
		}

		public Section(Func<List<TAsset>, List<TAsset>> toFiltered)
		{
			GetLabel = () => string.Empty;
			HasHeader = false;
			ToFiltered = toFiltered;
			ToSorted = (List<TAsset> assets) => assets;
		}
	}

	protected class SectionHandle
	{
		private readonly BaseAssetLibraryMenu<TAsset> _menu;

		public SectionHandle(BaseAssetLibraryMenu<TAsset> menu)
		{
			_menu = menu;
			_menu._sections.Clear();
		}

		public SectionHandle AddSection(Section section)
		{
			_menu._sections.Add(section);
			return this;
		}

		public SectionHandle ClearSections()
		{
			_menu._sections.Clear();
			return this;
		}
	}

	protected enum ItemVariant
	{
		Thumbnail,
		Universal
	}

	protected enum LayoutVariant
	{
		Grid,
		List
	}

	private class SectionHeaderWidget : BaseWidget, IGetCollapsable, IGetText, IWidgetHandle
	{
		private Section _section;

		public SectionHeaderWidget(Section section)
		{
			_section = section;
		}

		~SectionHeaderWidget()
		{
			_section = null;
		}

		public bool GetCollapsable()
		{
			return false;
		}

		public string GetText()
		{
			return _section.GetLabel();
		}
	}

	private WorkshopItemStatusMenu _workshopItemStatusMenu;

	private readonly List<Section> _sections = new List<Section>();

	protected static Func<List<TAsset>, List<TAsset>> ToSortedByDateThenTime = (List<TAsset> assets) => (from asset in assets.Slinq()
		orderby asset.dateTimeCreated descending
		select asset).ToList();

	private ItemVariant _itemVariant;

	private LayoutVariant _selectedLayoutVariant;

	private bool _interactable;

	private bool _dragging;

	private static readonly NotImplementedException TmpNotImplementedException = new NotImplementedException();

	private static readonly Action<BaseAssetLibraryMenu<TAsset>> OnLoadingScreenStart = delegate(BaseAssetLibraryMenu<TAsset> menu)
	{
		menu._interactable = false;
	};

	private static readonly Action<BaseAssetLibraryMenu<TAsset>> OnLoadingScreenEnd = delegate(BaseAssetLibraryMenu<TAsset> menu)
	{
		menu._interactable = true;
	};

	private static readonly Action<PublishedFileId_t, BaseAssetLibraryMenu<TAsset>> OnSubscription = delegate(PublishedFileId_t fileId, BaseAssetLibraryMenu<TAsset> menu)
	{
		BaseMenu.logger.InfoFormat("OnSubscription: {0}\n", fileId);
		menu.Reopen();
	};

	private static readonly Action<PublishedFileId_t, BaseAssetLibraryMenu<TAsset>> OnUnsubscription = delegate(PublishedFileId_t fileId, BaseAssetLibraryMenu<TAsset> menu)
	{
		BaseMenu.logger.InfoFormat("OnUnsubscription: {0}\n", fileId);
		menu.Reopen();
	};

	private static readonly Action<PublishedFileId_t> OnNewItemCreated = delegate(PublishedFileId_t fileId)
	{
		BaseMenu.logger.InfoFormat("OnNewItemCreated: {0}\n", fileId);
	};

	private static readonly Action<PublishedFileId_t, BaseAssetLibraryMenu<TAsset>> OnItemUpdated = delegate(PublishedFileId_t fileId, BaseAssetLibraryMenu<TAsset> menu)
	{
		BaseMenu.logger.InfoFormat("OnItemDownloaded: {0}\n", fileId);
		menu.Reopen();
	};

	private static readonly Action<PublishedFileId_t, EResult> OnItemDownloaded = delegate(PublishedFileId_t fileId, EResult re)
	{
		BaseMenu.logger.InfoFormat("OnItemDownloaded: {0}\n{1}", fileId, re);
	};

	private static readonly Action<PublishedFileId_t, EItemState, EItemState> OnStateChanged = delegate(PublishedFileId_t fileId, EItemState ea, EItemState eb)
	{
		BaseMenu.logger.InfoFormat("OnStateChanged: {0}\n{1}:{2}", fileId, ea, eb);
	};

	private IPanel Library
	{
		get
		{
			Action<ScrollableLayoutPanelComponent> onReset = delegate(ScrollableLayoutPanelComponent panel)
			{
				panel._layoutElement.enabled = true;
				panel._layoutElement.flexibleHeight = 1f;
				panel._layoutElement.flexibleWidth = 1f;
			};
			return ScrollableLayoutPanel.New(onReset).AddPanel(VerticalLayoutPanel.New().As(VerticalLayoutPanel.Variant.Library).AddPanels(LibraryContent));
		}
	}

	private IPanel WorkshopStatus
	{
		get
		{
			Action<StandardLayoutPanelComponent> onReset = delegate(StandardLayoutPanelComponent panel)
			{
				panel._layoutElement.enabled = true;
				panel._layoutElement.ignoreLayout = true;
			};
			return StandardLayoutPanel.New(onReset).SetMenu(out _workshopItemStatusMenu);
		}
	}

	protected List<Section> Sections => _sections;

	protected virtual bool AutoSelectFirstAsset => false;

	protected abstract TAsset SelectedAsset { get; set; }

	protected abstract List<TAsset> Assets { get; }

	protected virtual LayoutVariant SelectedLayoutVariant => _selectedLayoutVariant;

	private PanelList LibraryContent
	{
		get
		{
			PanelList panelList = PanelList.Request();
			for (int i = 0; i < Sections.Count; i++)
			{
				int index = i;
				Section section = Sections[index];
				List<TAsset> assets = Assets;
				assets = section.ToFiltered(assets);
				assets = section.ToSorted(assets);
				assets = ToSpecialFiltered(assets);
				if (assets.Count != 0)
				{
					if (section.HasHeader)
					{
						SectionHeaderWidget widget = new SectionHeaderWidget(section);
						panelList.Add(HeaderPanel.New().SetWidget(HeaderPanel.WidgetId.Text, widget).SetWidget(HeaderPanel.WidgetId.Collapsable, widget)
							.SetWidget(HeaderPanel.WidgetId.LayoutElement, widget));
					}
					panelList.Add(ItemsLayout(assets));
				}
			}
			return panelList;
		}
	}

	protected virtual bool ShowBadges => false;

	protected BaseAssetLibraryMenu(Func<RectTransform> getParent)
		: base(getParent)
	{
	}

	protected override void OpenSelf()
	{
		_interactable = true;
		base.OpenSelf();
	}

	protected override PanelHandle AddPanels()
	{
		AddSections();
		return base.AddPanels().AddPanel(Library).AddPanel(WorkshopStatus);
	}

	protected override MenuHandle AddMenus()
	{
		return base.AddMenus().AddMenu(_workshopItemStatusMenu);
	}

	private void AddSections()
	{
		OnAddSections();
	}

	protected virtual SectionHandle OnAddSections()
	{
		return new SectionHandle(this);
	}

	protected abstract void ExecuteMenuOperation(AssetQueryParameters assetQueryParameters);

	public static bool IsSameMap(Asset_Map first, Asset_Map second)
	{
		if (first == null || second == null)
		{
			return first == null && second == null;
		}
		if (first.codename == second.codename)
		{
			return true;
		}
		return false;
	}

	protected virtual List<TAsset> ToSpecialFiltered(List<TAsset> assets)
	{
		return assets;
	}

	private IPanel ItemsLayout(List<TAsset> sectionAssets)
	{
		return SelectedLayoutVariant switch
		{
			LayoutVariant.Grid => GridItemsLayout(sectionAssets), 
			LayoutVariant.List => ListItemsLayout(sectionAssets), 
			_ => throw new NotImplementedException(), 
		};
	}

	private IPanel GridItemsLayout(List<TAsset> sectionAssets)
	{
		Action<GridLayoutPanelComponent> onRefresh = delegate
		{
		};
		Action<GridLayoutPanelComponent> onReset = delegate(GridLayoutPanelComponent panel)
		{
			panel._fitter.enabled = false;
			panel._layoutElement.enabled = true;
			panel._layoutElement.flexibleWidth = 1f;
			panel._image.enabled = false;
			panel._fitterPlus.enabled = false;
			panel._layoutGroup.cellSize = new Vector2(184f, 310f);
			panel._layoutGroup.spacing = new Vector2(4f, 8f);
			panel._layoutGroup.childAlignment = TextAnchor.UpperCenter;
		};
		return GridLayoutPanel.New(onReset, onRefresh).AddPanels(Items(sectionAssets));
	}

	private IPanel ListItemsLayout(List<TAsset> sectionAssets)
	{
		Action<VerticalLayoutPanelComponent> onReset = delegate(VerticalLayoutPanelComponent panel)
		{
			panel._fitter.enabled = false;
			panel._layoutElement.enabled = true;
		};
		return VerticalLayoutPanel.New(onReset).AddPanels(Items(sectionAssets));
	}

	private PanelList Items(List<TAsset> sectionAssets)
	{
		return _itemVariant switch
		{
			ItemVariant.Universal => ItemsAsUniversalButton(sectionAssets), 
			ItemVariant.Thumbnail => ItemsAsThumbnailButton(sectionAssets), 
			_ => throw TmpNotImplementedException, 
		};
	}

	protected abstract string GetAssetLabel(TAsset asset);

	protected abstract string GetAssetSubLabel(TAsset asset);

	protected abstract string GetAssetInfoLabel(TAsset asset);

	protected virtual Sprite GetAssetDefaultImage(TAsset asset)
	{
		return IconKeys._UI_MissingImg.Get32();
	}

	protected virtual string GetAssetDefaultText(TAsset asset)
	{
		return "No thumbnail";
	}

	protected PanelList ItemsAsUniversalButton(List<TAsset> assets)
	{
		PanelList panelList = PanelList.Request();
		for (int i = 0; i < assets.Count; i++)
		{
			int index = i;
			TAsset assetN = assets[index];
			Action<MiniButtonPanelComponent> onRefresh = delegate(MiniButtonPanelComponent panel)
			{
				bool flag = IsSameMap(assetN as Asset_Map, SelectedAsset as Asset_Map);
				panel._frameAnimator.SetTrigger((!flag) ? "None" : "All");
			};
			Action<MiniButtonPanelComponent> onReset = delegate(MiniButtonPanelComponent panel)
			{
				panel._rawImage.texture = assetN.Disk_ReadThumbnail();
				panel._rawImage.gameObject.SetActive(value: true);
				panel._rawImage.gameObject.name = "Image 1";
				panel._image.gameObject.SetActive(value: false);
				panel._image.gameObject.name = "Image 1 sprite";
				panel._label.text = assetN.displayName;
				panel._iconAnimator.SetTrigger("ShowPrimary");
				panel._colorAnimator.SetTrigger("YellowText");
				panel._colorAnimator.SetTrigger("YellowIcon");
				panel._stateAnimator.SetBool("CanHover", value: true);
				panel._stateAnimator.SetBool("CanPress", value: true);
				panel._stateAnimator.SetBool("Discreet", value: true);
				Action onClick = delegate
				{
					if (_interactable)
					{
						SelectedAsset = assetN;
						ExecuteMenuOperation(AssetQueryParameters.AllSources);
						Refresh();
					}
				};
				panel._button.onClick.RemoveAllListeners();
				panel._button.onClick.AddListener(delegate
				{
					onClick();
				});
				bool flag = (assetN.authorType == AssetAuthorType.Official || assetN.authorType == AssetAuthorType.Workshop) && ShowBadges;
				panel._badgeImage.gameObject.SetActive(flag);
				if (flag)
				{
					Sprite sprite = ((assetN.authorType != AssetAuthorType.Official) ? IconKeys._UI_Steam.Get64() : IconKeys._UI_SmallBadge.Get64());
					panel._badgeImage.sprite = sprite;
				}
			};
			panelList.Add(MiniButtonPanelComponent.Build(onReset, onRefresh));
		}
		return panelList;
	}

	protected PanelList ItemsAsThumbnailButton(List<TAsset> assets)
	{
		PanelList panelList = PanelList.Request();
		for (int i = 0; i < assets.Count; i++)
		{
			int index = i;
			TAsset assetN = assets[index];
			string assetNameN = ((assetN != null) ? assetN.customName : string.Empty);
			Action<ThumbnailPanelComponent> onRefreshN = delegate(ThumbnailPanelComponent panel)
			{
				bool flag = IsSameMap(assetN as Asset_Map, SelectedAsset as Asset_Map);
				panel._image.color = ((!flag) ? Colors.Black : Colors.White);
				ColorBlock colors = panel._button.colors;
				if (flag)
				{
					colors.disabledColor = Colors.White;
					colors.normalColor = Colors.White;
					colors.highlightedColor = Colors.White;
					colors.pressedColor = Colors.White;
				}
				else if (SelectedLayoutVariant == LayoutVariant.List)
				{
					colors.disabledColor = Colors.Yellow;
					colors.normalColor = Colors.Yellow;
					colors.highlightedColor = Colors.LightYellow;
					colors.pressedColor = Colors.White;
				}
				else
				{
					colors.disabledColor = Colors.LightYellow;
					colors.normalColor = Colors.LightYellow;
					colors.highlightedColor = Colors.LighterYellow;
					colors.pressedColor = Colors.White;
				}
				panel._button.colors = colors;
			};
			Action<ThumbnailPanelComponent> onReset = delegate(ThumbnailPanelComponent panel)
			{
				if (assetN == null)
				{
					BaseMenu.logger.InfoFormat("Asset is null?!?!?!?!?!\nassetNameN == {0}", assetNameN);
				}
				panel._label.text = GetAssetLabel(assetN).ToUpper();
				panel._subLabel.text = GetAssetSubLabel(assetN).ToUpper();
				panel._infoLabel.text = GetAssetInfoLabel(assetN);
				Action onClick = delegate
				{
					if (_interactable && !_dragging)
					{
						SelectedAsset = assetN;
						ExecuteMenuOperation(AssetQueryParameters.AllSources);
						MenuSound.PlaySound(SoundPrefix, MenuSound.Suffix.Select);
					}
				};
				panel._button.onClick.RemoveAllListeners();
				panel._button.onClick.AddListener(delegate
				{
					onClick();
				});
				panel._defaultImage.sprite = GetAssetDefaultImage(assetN);
				panel._defaultLabel.text = GetAssetDefaultText(assetN);
				panel._rawImage.gameObject.SetActive(value: false);
				ref TAsset reference = ref assetN;
				Action<Texture2D> callback = delegate(Texture2D t2d)
				{
					if (t2d != null && panel._rawImage != null)
					{
						panel._rawImage.texture = t2d;
						panel._rawImage.gameObject.SetActive(value: true);
					}
				};
				reference.Disk_ReadThumbnailAsync(callback);
				bool flag = (assetN.authorType == AssetAuthorType.Official || assetN.authorType == AssetAuthorType.Workshop) && ShowBadges && !assetN.IsShell();
				panel._badgeImage.gameObject.SetActive(flag);
				if (flag)
				{
					Sprite sprite = ((assetN.authorType != AssetAuthorType.Official) ? IconKeys._UI_Steam.Get64() : IconKeys._UI_SmallBadge.Get64());
					panel._badgeImage.sprite = sprite;
				}
				panel._eventTrigger.triggers.Clear();
				EventTrigger.Entry entry = new EventTrigger.Entry();
				entry.eventID = EventTriggerType.PointerEnter;
				entry.callback = new EventTrigger.TriggerEvent();
				entry.callback.AddListener(delegate
				{
					if (_interactable)
					{
						MenuSound.PlaySound(SoundPrefix, MenuSound.Suffix.RolloverOn);
					}
				});
				panel._eventTrigger.triggers.Add(entry);
				EventTrigger.Entry entry2 = new EventTrigger.Entry();
				entry2.eventID = EventTriggerType.PointerExit;
				entry2.callback = new EventTrigger.TriggerEvent();
				entry2.callback.AddListener(delegate
				{
					if (_interactable)
					{
						MenuSound.PlaySound(SoundPrefix, MenuSound.Suffix.RolloverOff);
					}
				});
				panel._eventTrigger.triggers.Add(entry2);
				EventTrigger.Entry entry3 = new EventTrigger.Entry();
				entry3.eventID = EventTriggerType.BeginDrag;
				entry3.callback = new EventTrigger.TriggerEvent();
				entry3.callback.AddListener(delegate
				{
					_dragging = true;
				});
				panel._eventTrigger.triggers.Add(entry3);
				EventTrigger.Entry entry4 = new EventTrigger.Entry();
				entry4.eventID = EventTriggerType.EndDrag;
				entry4.callback = new EventTrigger.TriggerEvent();
				entry4.callback.AddListener(delegate
				{
					_dragging = false;
				});
				panel._eventTrigger.triggers.Add(entry4);
				onRefreshN(panel);
			};
			ThumbnailPanelComponent.BasePanel basePanel = ThumbnailPanelComponent.Build(onReset, onRefreshN);
			if (SelectedLayoutVariant == LayoutVariant.List)
			{
				basePanel.As(ThumbnailPanelComponent.Variant.ListItem);
			}
			panelList.Add(basePanel);
		}
		return panelList;
	}

	protected virtual void OnUnloadAssetsSignal()
	{
	}

	protected override SignalHandle<MenuState> AddSignals()
	{
		return base.AddSignals().AddSignalConnection(MenuState.Opened, GameSignals.LoadingScreenAnimStart, OnLoadingScreenStart, this).AddSignalConnection(MenuState.Opened, GameSignals.LoadingScreenAnimEnd, OnLoadingScreenEnd, this)
			.AddSignalConnection(MenuState.Opened, SteamWorkshopAgent.OnSubscription, OnSubscription, this)
			.AddSignalConnection(MenuState.Opened, SteamWorkshopAgent.OnUnsubscription, OnUnsubscription, this)
			.AddSignalConnection(MenuState.Opened, SteamWorkshopAgent.OnNewItemCreated, OnNewItemCreated)
			.AddSignalConnection(MenuState.Opened, SteamWorkshopAgent.OnItemUpdated, OnItemUpdated, this)
			.AddSignalConnection(MenuState.Opened, SteamWorkshopAgent.OnItemDownloaded, OnItemDownloaded)
			.AddSignalConnection(MenuState.Opened, SteamWorkshopAgent.OnStateChanged, OnStateChanged)
			.AddSignalConnection(MenuState.Opened, GameParam.OnSetSelectedMapSignal, BaseMenu.OnRefresh, this);
	}
}
