using UnityEngine;

namespace Brix.NewUI.OverheadDisplay;

public class TestOrderQueue : MonoBehaviour
{
	public OrderQueue orderQueue;

	public Sprite imgToAdd;

	public int index;

	public bool SetIcon;

	public bool NoIcon;

	public bool SlideLeft;

	public bool AddToQueue;

	public bool ClearQueue;

	public bool AddFirst;

	public bool AddLast;

	public bool AddAt;

	public bool RemoveFirst;

	public bool RemoveLast;

	public bool RemoveAt;

	public bool ActivateButtons;

	public bool DesactivateButtons;

	public bool Show;

	public bool Hide;

	private void Update()
	{
		if (SetIcon)
		{
			SetIcon = false;
		}
		if (NoIcon)
		{
			NoIcon = false;
		}
		if (SlideLeft)
		{
			SlideLeft = false;
		}
		if (AddToQueue)
		{
			orderQueue.AddToQueue(imgToAdd);
			AddToQueue = false;
		}
		if (ClearQueue)
		{
			orderQueue.ClearQueue();
			ClearQueue = false;
		}
		if (AddFirst)
		{
			orderQueue.AddOrderFirst(imgToAdd);
			AddFirst = false;
		}
		if (AddLast)
		{
			orderQueue.AddOrderLast(imgToAdd);
			AddLast = false;
		}
		if (AddAt)
		{
			orderQueue.AddOrderAt(index, imgToAdd);
			AddAt = false;
		}
		if (RemoveFirst)
		{
			orderQueue.RemoveFirstOrder();
			RemoveFirst = false;
		}
		if (RemoveLast)
		{
			orderQueue.RemoveLastOrder();
			RemoveLast = false;
		}
		if (RemoveAt)
		{
			orderQueue.RemoveOrderAt(index);
			RemoveAt = false;
		}
		if (ActivateButtons)
		{
			orderQueue.EnableButtons(enable: true);
			ActivateButtons = false;
		}
		if (DesactivateButtons)
		{
			orderQueue.EnableButtons(enable: false);
			DesactivateButtons = false;
		}
		if (Show)
		{
			orderQueue.Show = true;
			Show = false;
		}
		if (Hide)
		{
			Hide = false;
			orderQueue.Show = false;
		}
	}
}
