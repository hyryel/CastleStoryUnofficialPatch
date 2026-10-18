using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Factories;
using Brix.Lifecycle.Pooling;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.NewUI.OverheadDisplay;

public class ItemCounts : Overhead, IPoolableComponent, IInitializable, IRequestable, IResetable
{
	[SerializeField]
	private GameObject CloneableResource;

	[SerializeField]
	private GameObject Divider;

	[SerializeField]
	private int elementsBeforeItems = 2;

	[SerializeField]
	private int elementsAfterItems = 1;

	public bool HideIfZero = true;

	private bool enableButtons;

	private bool initialized;

	private int removedThisFrame;

	private List<GameObject> _toRemove = new List<GameObject>();

	public override int PreferedIndex => 2;

	public void Init()
	{
		if (!initialized)
		{
			SibblingsBeforeItems = elementsBeforeItems;
			SibblingsAfterItems = elementsAfterItems;
			enableButtons = true;
			if (HideIfZero && base.ItemCount == 0)
			{
				base.Show = false;
			}
			initialized = true;
		}
	}

	private void Start()
	{
		Init();
	}

	private void SetResource(GameObject resourcesGO, Sprite img, int qty, string name)
	{
		resourcesGO.transform.Find("Image").GetComponent<Image>().sprite = img;
		resourcesGO.transform.Find("Text").GetComponent<Text>().text = qty.ToString();
		resourcesGO.name = name;
	}

	private GameObject GetExistingResource(string name)
	{
		GameObject result = null;
		Transform transform = base.transform.Find(name);
		if (transform != null)
		{
			result = transform.gameObject;
		}
		return result;
	}

	public void EnableButtons(bool enable)
	{
		for (int i = SibblingsBeforeItems; i < base.ItemCount + SibblingsBeforeItems; i++)
		{
		}
		enableButtons = enable;
	}

	public void SetOrAddResource(Sprite img, int qty, string name)
	{
		if (qty == 0 && HideIfZero)
		{
			removedThisFrame += RemoveResource(name);
			if (removedThisFrame != 0 && base.ItemCount <= removedThisFrame)
			{
				base.Show = false;
			}
			return;
		}
		base.Show = true;
		Neo.Relais(delegate
		{
			GameObject gameObject = GetExistingResource(name);
			if (gameObject == null)
			{
				if (base.ItemCount > 0)
				{
					GameObject gameObject2 = CloneItemAsSibbling(Divider, SibblingsBeforeItems);
					gameObject2.name = "divider";
				}
				gameObject = CloneItemAsSibbling(CloneableResource, SibblingsBeforeItems);
			}
			SetResource(gameObject, img, qty, name);
		}, "ItemCount setting ressources");
	}

	public int RemoveResource(string name)
	{
		int num = 0;
		_toRemove.Clear();
		for (int i = 0; i < base.transform.childCount; i++)
		{
			GameObject gameObject = base.transform.GetChild(i).gameObject;
			if (!(gameObject.name == name))
			{
				continue;
			}
			_toRemove.Add(gameObject);
			num++;
			if (i < base.transform.childCount - 1)
			{
				GameObject gameObject2 = base.transform.GetChild(i + 1).gameObject;
				if (gameObject2.name == "divider")
				{
					_toRemove.Add(gameObject2);
				}
			}
			else
			{
				GameObject gameObject3 = base.transform.GetChild(i - 1).gameObject;
				if (gameObject3.name == "divider")
				{
					_toRemove.Add(gameObject3);
				}
			}
		}
		_toRemove.ForEach(delegate(GameObject o)
		{
			ObjectPoolSingleton.Release(o);
		});
		return num;
	}

	public void ClearAllResources()
	{
		for (int i = SibblingsBeforeItems; i < base.ItemCount + SibblingsBeforeItems; i++)
		{
			ObjectPoolSingleton.Release(base.transform.GetChild(i).gameObject);
		}
		base.Show = false;
	}

	private void Update()
	{
		removedThisFrame = 0;
	}

	public override void OnRequested()
	{
		base.OnRequested();
		ResetComponent();
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		Init();
		ClearAllResources();
		overhead = null;
	}
}
