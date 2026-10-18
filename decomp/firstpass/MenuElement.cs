using System;
using System.Collections.Generic;
using Brix.Components;
using Brix.External.Factories;
using Brix.Lifecycle.Pooling;
using UnityEngine;
using UnityEngine.Events;

public class MenuElement : BrixComponent, IPlug
{
	[Serializable]
	public class MenuElementReferenceItem
	{
		public MenuElementReference _menuElementReference;

		public bool _overrideSource;

		public Factory.AssetKey _source;
	}

	public UnityEvent _onAfterEnable;

	public UnityEvent _onBeforeDisable;

	private MenuElement _parentMenuElement;

	public MenuElementReferenceItem[] _menuElementReferenceItems;

	[SerializeField]
	private List<Plug> _plugs = new List<Plug>();

	private Dictionary<Type, Dictionary<string, Plug>> _plugMap = new Dictionary<Type, Dictionary<string, Plug>>();

	public List<Plug> Plugs => _plugs;

	public Dictionary<Type, Dictionary<string, Plug>> PlugMap => _plugMap;

	public void Enable()
	{
		this.ResetPlugCache();
		OnAfterEnable();
	}

	public void OnAfterEnable()
	{
		EnableReferences();
		_onAfterEnable.Invoke();
	}

	private void Disable()
	{
		ObjectPoolSingleton.Release(base.gameObject);
	}

	private void OnBeforeDisable(GameObject g)
	{
		DisableReferences();
		_onBeforeDisable.Invoke();
	}

	public override void OnRequested()
	{
		base.OnRequested();
	}

	public override void ResetComponent()
	{
		OnBeforeDisable(null);
		base.ResetComponent();
	}

	private void OnTransformParentChanged()
	{
		if (base.Initialized && base.Requested && !base.Destroyed)
		{
			OnAfterEnable();
			_parentMenuElement = this.GetComponentInParentExcludingSelf<MenuElement>();
			if (!(_parentMenuElement == null))
			{
				_parentMenuElement.OnReleaseOrDestroySignal.ConnectOnce(OnBeforeDisable);
			}
		}
	}

	private void EnableReferences()
	{
		MenuElementReferenceItem[] menuElementReferenceItems = _menuElementReferenceItems;
		foreach (MenuElementReferenceItem menuElementReferenceItem in menuElementReferenceItems)
		{
			MenuElementReference menuElementReference = menuElementReferenceItem._menuElementReference;
			if (!(menuElementReference._destination != null))
			{
				menuElementReference._source = ((!menuElementReferenceItem._overrideSource || !(menuElementReferenceItem._source != null)) ? menuElementReference._source : menuElementReferenceItem._source);
				ObjectPoolSingleton.Request(out var go, menuElementReference._source);
				go.transform.SetParent(menuElementReference.transform.parent, worldPositionStays: false);
				go.transform.SetSiblingIndex(menuElementReference.transform.GetSiblingIndex());
				menuElementReference._destination = go;
				menuElementReference.gameObject.SetActive(value: false);
			}
		}
	}

	private void DisableReferences()
	{
		MenuElementReferenceItem[] menuElementReferenceItems = _menuElementReferenceItems;
		foreach (MenuElementReferenceItem menuElementReferenceItem in menuElementReferenceItems)
		{
			MenuElementReference menuElementReference = menuElementReferenceItem._menuElementReference;
			if (!(menuElementReference._destination == null))
			{
				ObjectPoolSingleton.Release(menuElementReference._destination);
				menuElementReference.gameObject.SetActive(value: true);
			}
		}
	}

	[ContextMenu("TestSpawn")]
	public void TestSpawn()
	{
	}

	[ContextMenu("TestKill")]
	public void TestKill()
	{
		Disable();
	}
}
