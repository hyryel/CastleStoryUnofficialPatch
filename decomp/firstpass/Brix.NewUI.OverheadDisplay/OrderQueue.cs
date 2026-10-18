using Brix.External.Factories;
using Brix.Lifecycle.Pooling;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.NewUI.OverheadDisplay;

public class OrderQueue : Overhead, IPoolableComponent, IInitializable, IRequestable, IResetable
{
	public int ElementsCountBeforeActions = 2;

	public int ElementsCountAfterActions = 1;

	public GameObject CloneableAction;

	private bool buttonsEnabled;

	private bool noQueue;

	private bool initialized;

	public override int PreferedIndex => 3;

	public void Init()
	{
		if (!initialized)
		{
			noQueue = true;
			buttonsEnabled = true;
			SibblingsBeforeItems = ElementsCountBeforeActions;
			SibblingsAfterItems = ElementsCountAfterActions;
			initialized = true;
		}
	}

	private void Start()
	{
		Init();
	}

	private void HideIfNeeded()
	{
		if (noQueue)
		{
			base.Show = false;
		}
	}

	public void AddToQueue(Sprite img)
	{
		if (base.ItemCount == 0)
		{
			AddOrderFirst(img);
		}
		else
		{
			AddOrderLast(img);
		}
	}

	public void ClearQueue()
	{
		for (int i = 0; i < base.ItemCount; i++)
		{
			RemoveOrderAt(i);
		}
		noQueue = true;
		HideIfNeeded();
	}

	public void EnableButtons(bool enable)
	{
		buttonsEnabled = enable;
	}

	public void AddOrderFirst(Sprite image)
	{
		AddOrderAt(0, image, 1.25f);
	}

	public void AddOrderLast(Sprite image)
	{
		AddOrderAt(base.ItemCount, image);
	}

	public void AddOrderAt(int index, Sprite image, float scale = 1f)
	{
		base.Show = true;
		noQueue = false;
		int index2 = index + SibblingsBeforeItems;
		GameObject gameObject = CloneItemAsSibbling(CloneableAction, index2);
		if (index == 0)
		{
			LayoutElement component = gameObject.GetComponent<LayoutElement>();
			component.preferredWidth = 32f;
			component.preferredHeight = 32f;
		}
		gameObject.transform.GetComponent<Image>().sprite = image;
	}

	public void RemoveLastOrder()
	{
		RemoveOrderAt(base.ItemCount - 1);
	}

	public void RemoveFirstOrder()
	{
		RemoveOrderAt(0);
	}

	public void RemoveOrderAt(int index)
	{
		if (base.ItemCount >= 1)
		{
			int index2 = index + SibblingsBeforeItems;
			Object.Destroy(base.transform.GetChild(index2).gameObject);
			if (base.ItemCount == 1)
			{
				noQueue = true;
				HideIfNeeded();
			}
		}
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
		ClearQueue();
		overhead = null;
	}
}
