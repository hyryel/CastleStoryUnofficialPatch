using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Signals;
using UnityEngine;

namespace Brix.NewUI.BreadBar;

public class BreadBarDriver : MonoBehaviour, IPlug
{
	[SerializeField]
	private List<Plug> m_Plugs = new List<Plug>();

	private Dictionary<Type, Dictionary<string, Plug>> m_PlugMap = new Dictionary<Type, Dictionary<string, Plug>>();

	public bool _isAwake;

	private SignalConnection _menuBreadChangeConnection = SignalConnection.Empty;

	private SignalConnection _menuBreadAddContextBtnConnection = SignalConnection.Empty;

	private const char _pathToken = '/';

	public BreadBarCrumb sectionCloneable;

	public BreadBarCrumb contextCloneable;

	public BreadBarCrumbBtn buttonCloneable;

	public Transform sectionCloneBin;

	public int sectionCloneSiblingIdxOffset;

	public Transform contextCloneBin;

	public int contextCloneSiblingIdxOffset;

	public Transform btnBackCloneBin;

	public Transform btnCloneBin;

	public int btnCloneSiblingIdxOffset;

	public List<BreadBarCrumb> sections;

	public BreadBarCrumb context;

	public List<BreadBarCrumbBtn> buttons;

	private const int sectionsMaxCount = 5;

	private const int btnsMaxCount = 5;

	public List<Plug> Plugs => m_Plugs;

	public Dictionary<Type, Dictionary<string, Plug>> PlugMap => m_PlugMap;

	protected virtual void OnEnable()
	{
		this.ResetPlugCache();
	}

	public void Awake()
	{
		DisconnectFromSignals();
		ConnectToSignals();
		if (!_isAwake)
		{
			_isAwake = true;
			ResetCache();
		}
	}

	protected void ConnectToSignals()
	{
		_menuBreadChangeConnection = GameSignals.Connect(GameSignals.OnMenuBreadChange, OnMenuBreadChange);
		_menuBreadAddContextBtnConnection = GameSignals.Connect(GameSignals.OnMenuBreadAddBtn, OnMenuBreadAddContextBtn);
	}

	protected void DisconnectFromSignals()
	{
		_menuBreadChangeConnection.Disconnect();
		_menuBreadAddContextBtnConnection.Disconnect();
	}

	private void OnMenuBreadChange(SectionBread sectionBread)
	{
		string[] array = sectionBread._path.Split('/');
		if (sectionBread._path == string.Empty || array.Length == 0)
		{
			HideAllSections();
			if (context != null)
			{
				context.gameObject.SetActive(value: false);
			}
		}
		else
		{
			SetSections(array);
			if (context != null)
			{
				context.gameObject.SetActive(value: true);
			}
		}
	}

	private void OnMenuBreadAddContextBtn(ButtonBread[] buttonBreads)
	{
		HideAllButtons(buttonBreads.Length);
		for (int i = 0; i < buttonBreads.Length; i++)
		{
			ButtonBread buttonBread = buttonBreads[i];
			BreadBarCrumbBtn breadBarCrumbBtn = buttons[i];
			breadBarCrumbBtn.SetLabel(buttonBread._label, buttonBread._labelColor);
			breadBarCrumbBtn.SetIcon(buttonBread._icon, buttonBread._iconColor);
			breadBarCrumbBtn.SetClickAction(buttonBread._clickAction, buttonBread._clickActionEnabled);
			breadBarCrumbBtn.Show();
		}
	}

	private void SetSections(string[] crumNames)
	{
		HideAllSections();
		for (int i = 0; i < crumNames.Length; i++)
		{
			BreadBarCrumb breadBarCrumb = ((i != crumNames.Length - 1) ? sections[i] : context);
			breadBarCrumb.SetLabel(crumNames[i]);
			breadBarCrumb.SetIcon(null);
			breadBarCrumb.Show();
		}
	}

	private void HideAllSections()
	{
		context.Hide();
		foreach (BreadBarCrumb section in sections)
		{
			section.Hide();
		}
	}

	private void HideAllButtons(int newLength = -1)
	{
		bool active = false;
		bool flag = false;
		for (int i = 0; i < buttons.Count; i++)
		{
			BreadBarCrumbBtn breadBarCrumbBtn = buttons[i];
			if (newLength != -1 && newLength != 0 && i < newLength)
			{
				active = i != 0;
				flag = i == 0 || flag;
			}
			else
			{
				breadBarCrumbBtn.Hide();
			}
		}
		if (btnCloneBin != null)
		{
			btnCloneBin.gameObject.SetActive(active);
		}
		if (btnBackCloneBin != null)
		{
			btnBackCloneBin.gameObject.SetActive(flag);
		}
	}

	private void ResetSectionsCache()
	{
		BreadBarCrumb breadBarCrumb = sectionCloneable;
		sections.Clear();
		for (int i = 0; i < 5; i++)
		{
			BreadBarCrumb breadBarCrumb2 = UnityEngine.Object.Instantiate(breadBarCrumb);
			breadBarCrumb2.name = breadBarCrumb.name.Replace("Cloneable", "Clone #" + i);
			breadBarCrumb2.transform.SetParent(sectionCloneBin);
			breadBarCrumb2.transform.SetSiblingIndex(breadBarCrumb2.transform.GetSiblingIndex() + sectionCloneSiblingIdxOffset);
			breadBarCrumb2.transform.localScale = Vector3.one;
			sections.Add(breadBarCrumb2.GetComponent<BreadBarCrumb>());
		}
	}

	private void ResetContextCache()
	{
		BreadBarCrumb breadBarCrumb = contextCloneable;
		BreadBarCrumb breadBarCrumb2 = UnityEngine.Object.Instantiate(breadBarCrumb);
		breadBarCrumb2.name = breadBarCrumb.name.Replace("Cloneable", "Clone");
		breadBarCrumb2.transform.SetParent(contextCloneBin);
		breadBarCrumb2.transform.SetSiblingIndex(breadBarCrumb2.transform.GetSiblingIndex() + contextCloneSiblingIdxOffset);
		breadBarCrumb2.transform.localScale = Vector3.one;
		context = breadBarCrumb2.GetComponent<BreadBarCrumb>();
	}

	private void ResetButtonsCache()
	{
		BreadBarCrumbBtn breadBarCrumbBtn = buttonCloneable;
		buttons.Clear();
		for (int i = 0; i < 5; i++)
		{
			BreadBarCrumbBtn breadBarCrumbBtn2 = UnityEngine.Object.Instantiate(breadBarCrumbBtn);
			BreadBarCrumbBtn component = breadBarCrumbBtn2.GetComponent<BreadBarCrumbBtn>();
			breadBarCrumbBtn2.name = breadBarCrumbBtn.name.Replace("Cloneable", "Clone #" + i);
			breadBarCrumbBtn2.transform.SetParent((i != 0) ? btnCloneBin : btnBackCloneBin);
			breadBarCrumbBtn2.transform.SetSiblingIndex(breadBarCrumbBtn2.transform.GetSiblingIndex() + btnCloneSiblingIdxOffset);
			breadBarCrumbBtn2.transform.localScale = Vector3.one;
			buttons.Add(component);
		}
	}

	private void ResetCache()
	{
		ResetSectionsCache();
		ResetContextCache();
		ResetButtonsCache();
	}

	private void OnDestroy()
	{
		DisconnectFromSignals();
	}
}
