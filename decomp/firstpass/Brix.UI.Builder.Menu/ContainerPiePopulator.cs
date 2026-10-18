using System;
using System.Collections;
using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Legacy;
using Brix.Utils;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.UI.Builder.Menu;

public class ContainerPiePopulator : LegacyBaseMenu
{
	private static BrixLogger logger = new BrixLogger(typeof(ContainerPiePopulator).FullName);

	public static bool isDebug = false;

	public static bool isUseOldPieMenu = false;

	public CanvasGroup canvasGroup;

	public GameObject panelGo;

	public Container container;

	public ContaineePie menuItemPrefab;

	public ContaineePie menuItemSubPrefab;

	public ContaineePie menuItemNumPrefab;

	public ContaineePie menuItemTogglePrefab;

	public ContaineePie menuItemToggleNumPrefab;

	public ContaineePie menuItemDividerPrefab;

	public ContaineePie menuItemCorePrefab;

	public Material pieIconMaterial;

	public RadialLayoutGroup radialLayoutGroup;

	private Hashtable coreGameItem;

	public ContainerPiePopulator superMenu;

	public bool isSubMenu;

	private List<Hashtable> subGameItems = new List<Hashtable>();

	private List<Hashtable> gameItems = new List<Hashtable>();

	public bool spawnsSubMenu;

	private int coreMenuItemIdx = -1;

	private List<Container.Item> menuItems = new List<Container.Item>();

	public int testEditorSelectItemIdx = -1;

	private readonly List<SignalConnection> _sgs = new List<SignalConnection>();

	private static readonly Action<int, ContainerPiePopulator> OnItemClick = delegate(int idx, ContainerPiePopulator menu)
	{
		Container.Item item = menu.container.items[idx];
		Hashtable hashtable = menu.gameItems[idx];
		if (hashtable != menu.coreGameItem)
		{
			if (hashtable.ContainsKey("accept") && hashtable["accept"] != null && hashtable["accept"] is Action action && !menu.skipAcceptActionIdxList.Contains(idx))
			{
				try
				{
					action();
				}
				catch (Exception)
				{
					logger.Warn("Pie game action failed.");
					throw;
				}
				Refresh(menu);
				Neo.Relais(delegate
				{
					Refresh(menu);
				}, 0.1f, "ContainerPiePopulator refresh");
			}
			if (hashtable.ContainsKey("submenu") && hashtable["submenu"] != null)
			{
				List<Hashtable> list = (List<Hashtable>)hashtable["submenu"];
				if (list != null)
				{
					menu.subMenuGameItemIdx = idx;
					menu.GenerateSubMenu(list);
				}
			}
		}
	};

	private List<int> skipAcceptActionIdxList = new List<int>();

	private static readonly Action<int, ContainerPiePopulator> OnItemAltClick = delegate(int idx, ContainerPiePopulator menu)
	{
		Hashtable hashtable = menu.gameItems[idx];
		if (hashtable != menu.coreGameItem)
		{
			if (hashtable.ContainsKey("onToggle") && hashtable["onToggle"] != null && hashtable["onToggle"] is Action action)
			{
				if (!menu.skipAcceptActionIdxList.Contains(idx))
				{
					menu.skipAcceptActionIdxList.Add(idx);
				}
				try
				{
					action();
				}
				catch (Exception)
				{
					logger.Warn("Pie game action failed.");
					throw;
				}
				Refresh(menu);
				Neo.Relais(delegate
				{
					Refresh(menu);
				}, 0.1f, "ContainerPiePopulator refresh");
			}
			if (hashtable.ContainsKey("submenu") && hashtable["submenu"] != null)
			{
				List<Hashtable> list = (List<Hashtable>)hashtable["submenu"];
				if (list != null)
				{
					menu.subMenuGameItemIdx = idx;
					menu.GenerateSubMenu(list);
				}
			}
		}
	};

	private static readonly Action<ContainerPiePopulator> Refresh = delegate(ContainerPiePopulator menu)
	{
		menu.Build(clearExisting: false);
		menu.Apply();
	};

	[NonSerialized]
	public int subMenuGameItemIdx = -1;

	public ContainerPiePopulator subMenu;

	public Container.Item coreMenuItem => (coreMenuItemIdx != -1) ? menuItems[coreMenuItemIdx] : null;

	private List<Hashtable> GetGameItems()
	{
		List<Hashtable> list = new List<Hashtable>();
		if (isSubMenu)
		{
			list = new List<Hashtable>(subGameItems);
		}
		else
		{
			string mode = "Start";
			Vector3 mouse = radialLayoutGroup.mouseDownPos;
			list = PieContenu.GetGeneratedButtonData(mode, mouse);
		}
		coreGameItem = list[0];
		list.RemoveAt(0);
		list.Reverse();
		list.Insert(0, coreGameItem);
		spawnsSubMenu = false;
		foreach (Hashtable item in list)
		{
			if (item.ContainsKey("submenu") && item["submenu"] != null)
			{
				spawnsSubMenu = true;
				break;
			}
		}
		return list;
	}

	private List<Container.Item> GetMenuItems(List<Hashtable> gameItems)
	{
		return GetMenuItems(gameItems, clearExisting: true);
	}

	private List<Container.Item> GetMenuItems(List<Hashtable> gameItems, bool clearExisting)
	{
		if (!clearExisting)
		{
			for (int i = 0; i < gameItems.Count; i++)
			{
				if (i >= menuItems.Count)
				{
					clearExisting = true;
					break;
				}
			}
		}
		if (clearExisting)
		{
			menuItems.Clear();
		}
		for (int j = 0; j < gameItems.Count; j++)
		{
			Hashtable gameItem = gameItems[j];
			bool flag = gameItem == coreGameItem;
			bool flag2 = !flag && gameItem["icone"] == null && gameItem["sprite"] == null;
			bool flag3 = gameItem.ContainsKey("submenu") && gameItem["submenu"] != null;
			if (!flag && !flag2)
			{
				Func<bool> func = gameItem["check"] as Func<bool>;
				try
				{
					bool flag4 = func();
				}
				catch (Exception)
				{
					logger.Warn("Pie check action failed.");
					throw;
				}
			}
			int num = -1;
			if (!flag && !flag2)
			{
				Func<int> func2 = gameItem["checkToggled"] as Func<int>;
				try
				{
					num = func2();
				}
				catch (Exception)
				{
					logger.Warn("Pie check toggled action failed.");
					throw;
				}
			}
			bool flag5 = num != -1;
			bool flag6 = !string.IsNullOrEmpty((string)gameItem["checkLabel"]) && (string)gameItem["checkLabel"] != "!!";
			bool flag7 = flag5 && flag6;
			if (gameItem["accept"] == null)
			{
				Action onAction = (Action)gameItem["onToggle"];
				Action onNoneAction = delegate
				{
				};
				Action value = delegate
				{
					if (onAction != null)
					{
						try
						{
							onAction();
						}
						catch (Exception)
						{
							logger.Warn("Pie action failed.");
							throw;
						}
					}
				};
				Action value2 = delegate
				{
					gameItem["accept"] = onNoneAction;
					gameItem["onToggle"] = onAction;
					if (onAction != null)
					{
						try
						{
							onAction();
						}
						catch (Exception)
						{
							logger.Warn("Pie action failed.");
							throw;
						}
					}
				};
				gameItem["accept"] = value;
				gameItem["onToggle"] = value2;
			}
			Texture2D imageTexture = null;
			Sprite sprite = null;
			int size = 0;
			Color color = Color.white;
			string empty = string.Empty;
			string text = string.Empty;
			string term = string.Empty;
			if (!flag)
			{
				imageTexture = (Texture2D)gameItem["icone"];
				sprite = (Sprite)gameItem["sprite"];
				size = (int)gameItem["size"];
				color = (Color)gameItem["color"];
				empty = (string)gameItem["titre"];
				empty = ((empty != null) ? empty : string.Empty);
				text = (string)gameItem["checkLabel"];
				text = ((text != null) ? text : string.Empty);
				term = (string)gameItem["tooltip"];
				term = ((term != null) ? term : string.Empty);
			}
			else
			{
				empty = (string)gameItem["titre"];
				empty = ((empty != null) ? empty : string.Empty);
			}
			Sprite toggleImageTexture = null;
			if (flag5)
			{
				Sprite[] array = (Sprite[])gameItem["iconesToggle"];
				toggleImageTexture = array[num];
			}
			Containee containee = menuItemPrefab;
			containee = ((!flag3) ? containee : menuItemSubPrefab);
			containee = ((!flag6) ? containee : menuItemNumPrefab);
			containee = ((!flag5) ? containee : menuItemTogglePrefab);
			containee = ((!flag7) ? containee : menuItemToggleNumPrefab);
			containee = ((!flag) ? containee : menuItemCorePrefab);
			containee = ((!flag2) ? containee : menuItemDividerPrefab);
			Container.Item item = (clearExisting ? new Container.Item() : menuItems[j]);
			item.original = (clearExisting ? containee : menuItems[j].original);
			item.instance = (clearExisting ? null : menuItems[j].instance);
			item.parentTrs = null;
			item.currentState = (clearExisting ? Containee.State.Opened : menuItems[j].currentState);
			item.imageTexture = imageTexture;
			item.sprite = sprite;
			item.size = size;
			item.color = color;
			item.toggleImageTexture = toggleImageTexture;
			item.titleText = I2Helper.TryGet(empty);
			item.subtitleText = I2Helper.TryGet(text);
			item.subsubtitleText = I2Helper.TryGet(term);
			item.localizeTitle = empty;
			item.localizeSubtitle = text;
			item.usePreferredSpreadOffset = empty.Contains("Deselect");
			item.preferredSpreadOffset = 0.75f;
			item.bgColor = new Color(0f, 0f, 0f, 0f);
			item.idxOffset = 0;
			item.group = Container.Item.Group.None;
			item.isDynamic = true;
			if (clearExisting)
			{
				if (flag)
				{
					coreMenuItemIdx = j;
				}
				menuItems.Add(item);
			}
		}
		return menuItems;
	}

	private void UpdateMenuItemIconDisables()
	{
		for (int i = 0; i < menuItems.Count; i++)
		{
			Container.Item item = menuItems[i];
			if (item == null || item.instance == null)
			{
				continue;
			}
			ContaineePie containeePie = item.instance as ContaineePie;
			if (containeePie == null)
			{
				continue;
			}
			Hashtable hashtable = gameItems[i];
			if (hashtable == null)
			{
				continue;
			}
			bool flag = hashtable == coreGameItem;
			bool flag2 = !flag && hashtable["icone"] == null && hashtable["sprite"] == null;
			bool flag3 = flag || flag2;
			if (!flag3)
			{
				Func<bool> func = hashtable["check"] as Func<bool>;
				try
				{
					flag3 = func();
				}
				catch (Exception)
				{
					logger.Warn("Pie check action failed.");
					throw;
				}
			}
			containeePie.isIconEnabled = flag3;
			if (hashtable.ContainsKey("onToggle") && hashtable["onToggle"] != null)
			{
				containeePie.isActivatedOnClick = true;
			}
			if (Convert.ToBoolean(hashtable["sonNegatif"]))
			{
				containeePie.isActionNegative = true;
			}
			string text = "Pie Icon";
			if (containeePie.pieRawImage != null)
			{
				Material material = containeePie.pieRawImage.material;
				if (material == null || material.name != text)
				{
					containeePie.pieRawImage.material = new Material(pieIconMaterial);
					material = containeePie.pieRawImage.material;
					material.name = text;
					material.mainTexture = item.imageTexture;
				}
				material.SetFloat("_AlphaFactor", (!radialLayoutGroup.isFrozen || !radialLayoutGroup.isUsingFrozen) ? 1f : 0.5f);
				material.SetFloat("_GrayscaleFactor", (!flag3) ? 1f : 0f);
			}
			if (containeePie.pieImage != null)
			{
				Material material2 = containeePie.pieImage.material;
				containeePie.pieImage.sprite = item.sprite;
				if (material2 == null || material2.name != text)
				{
					containeePie.pieImage.material = new Material(pieIconMaterial);
					material2 = containeePie.pieImage.material;
					material2.name = text;
				}
				material2.SetFloat("_AlphaFactor", (!radialLayoutGroup.isFrozen || !radialLayoutGroup.isUsingFrozen) ? 1f : 0.5f);
				material2.SetFloat("_GrayscaleFactor", (!flag3) ? 1f : 0f);
			}
		}
	}

	public void UpdateTooltipText(RadialLayoutElement targetRadialLayoutElement)
	{
		if (coreMenuItem == null || coreMenuItem.instance == null)
		{
			return;
		}
		if (targetRadialLayoutElement == null)
		{
			if (coreMenuItem.instance.subtitle != null)
			{
				coreMenuItem.instance.subtitle.text = string.Empty;
			}
			if (coreMenuItem.instance.subsubtitle != null)
			{
				coreMenuItem.instance.subsubtitle.text = string.Empty;
			}
			if (coreMenuItem.instance.subsubsubtitle != null)
			{
				coreMenuItem.instance.subsubsubtitle.text = string.Empty;
			}
			return;
		}
		Containee containee = targetRadialLayoutElement.containee;
		int itemIdx = containee.container.GetItemIdx(containee);
		if (itemIdx < 0 || itemIdx >= menuItems.Count)
		{
			return;
		}
		Container.Item item = menuItems[itemIdx];
		if (item != null)
		{
			string titleText = item.titleText;
			string subsubtitleText = item.subsubtitleText;
			if (coreMenuItem.instance.subtitle != null)
			{
				coreMenuItem.instance.subtitle.text = titleText;
			}
			if (coreMenuItem.instance.subsubtitle != null)
			{
				coreMenuItem.instance.subsubtitle.text = subsubtitleText;
			}
		}
	}

	public void Build()
	{
		Build(clearExisting: true);
	}

	public void Build(bool clearExisting)
	{
		gameItems = GetGameItems();
		menuItems = GetMenuItems(gameItems, clearExisting);
	}

	[ContextMenu("Build And Populate")]
	public bool BuildAndPopulate()
	{
		Build();
		if (menuItems.Count <= 1)
		{
			return false;
		}
		if (container != null)
		{
			container.Populate(isChain: false, purgeOnly: true);
			container.items.Clear();
			container.items.AddRange(menuItems);
			container.Populate(isChain: false);
			UpdateMenuItemIconDisables();
			return true;
		}
		return false;
	}

	[ContextMenu("Apply")]
	public void Apply()
	{
		container.Apply();
	}

	public void ApplyInterpolators()
	{
		if (!(radialLayoutGroup == null))
		{
			radialLayoutGroup.UpdateSettings();
			radialLayoutGroup.UpdateTransforms();
		}
	}

	[ContextMenu("Build And Populate And Show")]
	public void BuildAndPopulateAndShow()
	{
		BuildAndPopulate();
		ShowPanel();
		InitLayout();
	}

	[ContextMenu("ShowPanel")]
	public void ShowPanel()
	{
		TogglePanel(flag: true);
	}

	[ContextMenu("HidePanel")]
	public void HidePanel()
	{
		TogglePanel(flag: false);
	}

	public void TogglePanel(bool flag)
	{
		if (!(panelGo == null))
		{
			panelGo.SetActive(flag);
		}
	}

	[ContextMenu("InitLayout")]
	public void InitLayout()
	{
		if (radialLayoutGroup != null)
		{
			radialLayoutGroup.Init();
			radialLayoutGroup.ApplyInterpolators();
			radialLayoutGroup.UpdateAll();
		}
	}

	private void OnEnable()
	{
		Connect();
		Init();
	}

	private void OnDisable()
	{
		Disconnect();
	}

	private void Connect()
	{
		if (container != null)
		{
			_sgs.Add(container.OnItemClickSignal.Connect(OnItemClick, this));
		}
		if (container != null)
		{
			_sgs.Add(container.OnItemAltClickSignal.Connect(OnItemAltClick, this));
		}
	}

	private void Disconnect()
	{
		foreach (SignalConnection sg in _sgs)
		{
			sg.Disconnect();
		}
		_sgs.Clear();
	}

	public void ResetToggleCache()
	{
		skipAcceptActionIdxList.Clear();
	}

	[ContextMenu("Init")]
	protected override void Init()
	{
		container = ((!(container != null)) ? base.gameObject.GetComponent<Container>() : container);
		subMenuGameItemIdx = -1;
	}

	[ContextMenu("DebugPrintMenu")]
	public void DebugPrintMenu()
	{
		List<Hashtable> list = GetGameItems();
		foreach (Hashtable item in list)
		{
			logger.Info("gameItem: " + item["titre"]);
		}
	}

	private void GenerateSubMenu(List<Hashtable> subGameItems)
	{
		ContainerPiePopulator containerPiePopulator = subMenu;
		if (!(containerPiePopulator == null))
		{
			RadialLayoutGroup radialLayoutGroup = this.radialLayoutGroup;
			RadialLayoutGroup radialLayoutGroup2 = containerPiePopulator.radialLayoutGroup;
			radialLayoutGroup2.baseOffsetPos = radialLayoutGroup.baseOffsetPos;
			radialLayoutGroup2.mousePos = radialLayoutGroup.mousePos;
			radialLayoutGroup2.mouseDownPos = radialLayoutGroup.mouseDownPos;
			radialLayoutGroup2.mouseDragPos = radialLayoutGroup.mouseDragPos;
			radialLayoutGroup2.rawBaseAngleOffset = radialLayoutGroup.rawBaseAngleOffset;
			HideCore();
			containerPiePopulator.subGameItems = subGameItems;
			containerPiePopulator.BuildAndPopulateAndShow();
			radialLayoutGroup2.ShowSelf();
		}
	}

	public void ShowCore()
	{
		ToggleCore(flag: true);
	}

	public void HideCore()
	{
		ToggleCore(flag: false);
	}

	public void ToggleCore(bool flag)
	{
		if (coreMenuItem != null && !(coreMenuItem.instance == null))
		{
			coreMenuItem.instance.interpolatorTargetable.WarpTo("Closed");
			if (flag)
			{
				coreMenuItem.instance.interpolatorTargetable.PlayTo("Opened");
			}
			coreMenuItem.instance.gameObject.SetActive(flag);
		}
	}
}
