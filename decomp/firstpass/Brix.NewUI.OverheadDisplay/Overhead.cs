using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Lifecycle.Pooling;
using Brix.Utils.Factories;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Brix.NewUI.OverheadDisplay;

public class Overhead : MonoBehaviour, IPoolableComponent, IInitializable, IRequestable, IResetable
{
	[SerializeField]
	public OverheadDisplay overhead;

	protected int SibblingsBeforeItems;

	protected int SibblingsAfterItems;

	public Signal RightClick = new Signal();

	public Signal LeftClick = new Signal();

	public Signal Hover = new Signal();

	public Signal EndHover = new Signal();

	public virtual int PreferedIndex => 0;

	public bool ForceShow { get; set; }

	public bool Show { get; set; }

	public bool Interactible
	{
		get
		{
			return this.WithComponent((CanvasGroup c) => c.interactable && c.blocksRaycasts, _default: false);
		}
		set
		{
			this.WithComponent(delegate(CanvasGroup c)
			{
				bool interactable = (c.blocksRaycasts = value);
				c.interactable = interactable;
			});
		}
	}

	public int ItemCount => base.transform.childCount - (SibblingsBeforeItems + SibblingsAfterItems);

	public void MakeVisible(bool value)
	{
		CanvasGroup component = GetComponent<CanvasGroup>();
		component.alpha = (value ? 1 : 0);
		component.interactable = value;
	}

	protected GameObject CloneItemAsSibbling(GameObject cloneable, int index)
	{
		GameObject gameObject = Object.Instantiate(cloneable);
		gameObject.transform.SetParent(base.transform);
		gameObject.transform.SetSiblingIndex(index);
		gameObject.SetActive(value: true);
		return gameObject;
	}

	public void OnClick(BaseEventData baseData)
	{
		PointerEventData pointerEventData = baseData as PointerEventData;
		ISignal signal = ((pointerEventData.button == PointerEventData.InputButton.Left) ? LeftClick : ((pointerEventData.button != PointerEventData.InputButton.Right) ? ((ISignal)NullSignal.Get) : ((ISignal)RightClick)));
		signal.Invoke();
	}

	public void OnHover()
	{
		Hover.Invoke();
	}

	public void OnEndHover()
	{
		EndHover.Invoke();
	}

	public virtual void Initialize()
	{
	}

	public virtual void OnRequested()
	{
	}

	public virtual void ResetComponent()
	{
		ForceShow = false;
	}
}
