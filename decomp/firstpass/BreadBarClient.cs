using System;
using System.Collections.Generic;
using System.Linq;
using Brix.Engine;
using Brix.NewUI.BreadBar;
using Brix.Utils.Logging;
using UnityEngine;
using UnityEngine.Serialization;

public class BreadBarClient : MonoBehaviour
{
	[Serializable]
	public class Btn
	{
		public string title;

		public Sprite icon;

		public List<Container.Item.ActionItem> actionItems = new List<Container.Item.ActionItem>();
	}

	private static BrixLogger logger = new BrixLogger(typeof(BreadBarClient).FullName);

	public bool setOnEnable;

	[FormerlySerializedAs("path")]
	public string _path = string.Empty;

	[FormerlySerializedAs("closingClient")]
	public BreadBarClient navParent;

	public List<BreadBarClient> navChildren = new List<BreadBarClient>();

	public bool isNavRoot;

	public static BreadBarClient navRoot;

	public BreadBarClient closingClient;

	public BreadBarDriver breadDriver;

	public List<Btn> buttons = new List<Btn>();

	[ContextMenu("Reset Nav Cache [Self]")]
	public void ResetNavCacheSelf()
	{
		ResetNavCache(cascade: false);
	}

	[ContextMenu("Reset Nav Cache [All]")]
	public void ResetNavCacheAll()
	{
		ResetNavCache(cascade: true);
	}

	private void ResetNavCache(bool cascade)
	{
		if (navParent == null)
		{
			if (!isNavRoot)
			{
				logger.Warn("Missing Nav Parent for non-root Nav Element:\n\t" + base.name);
			}
			else
			{
				navRoot = this;
			}
			return;
		}
		if (!navParent.navChildren.Contains(this))
		{
			navParent.navChildren.Add(this);
		}
		for (int num = navParent.navChildren.Count - 1; num >= 0; num--)
		{
			if (!(navParent.navChildren[num] != null))
			{
				navParent.navChildren.RemoveAt(num);
			}
		}
		if (cascade)
		{
			navParent.ResetNavCache(cascade: true);
		}
	}

	public static string GetNavFullPath(BreadBarClient navElem)
	{
		if (navElem == null)
		{
			return string.Empty;
		}
		List<BreadBarClient> navParents = GetNavParents(navElem);
		navParents.Reverse();
		return (navParents.Count != 1) ? string.Join("/", (from i in navParents
			where i._path != string.Empty
			select i._path).ToArray()) : navParents[0]._path;
	}

	public static List<BreadBarClient> GetNavParents(BreadBarClient navElem)
	{
		List<BreadBarClient> navParents = new List<BreadBarClient>();
		if (navElem != null)
		{
			GetNavParentsRecursive(navElem, ref navParents);
		}
		return navParents;
	}

	public static void GetNavParentsRecursive(BreadBarClient navElem, ref List<BreadBarClient> navParents)
	{
		navParents.Add(navElem);
		if (!(navElem.navParent == null))
		{
			GetNavParentsRecursive(navElem.navParent, ref navParents);
		}
	}

	private void Start()
	{
		SetOnEnable();
	}

	private void OnEnable()
	{
		SetOnEnable();
	}

	private void OnDisable()
	{
		if (!(closingClient == null))
		{
			closingClient.SetDefaultBar();
		}
	}

	private void SetOnEnable()
	{
		if (setOnEnable)
		{
			SetDefaultBar();
			if (Neo.Exists)
			{
				Neo.Relais(SetDefaultBar, 2f, "BreadBar set default");
			}
		}
	}

	public void SetDefaultBar()
	{
		if (breadDriver != null)
		{
			breadDriver.Awake();
		}
		GameSignals.Invoke(GameSignals.OnMenuBreadChange, new SectionBread(GetNavFullPath(this)));
		GameSignals.Invoke(GameSignals.OnMenuBreadAddBtn, buttons.Select((Btn btn) => new ButtonBread(btn.title, btn.icon, delegate
		{
			Container.RunActions(btn.actionItems);
		}).SetColor(BreadBarCrumb.castleYellowColor)).ToArray());
	}

	public void SetOverrideBar(string dstShortPath, ButtonBread[] buttonBreads)
	{
		if (breadDriver != null)
		{
			breadDriver.Awake();
		}
		string path = _path;
		_path = dstShortPath;
		GameSignals.Invoke(GameSignals.OnMenuBreadChange, new SectionBread(GetNavFullPath(this)));
		GameSignals.Invoke(GameSignals.OnMenuBreadAddBtn, buttonBreads);
		_path = path;
	}
}
