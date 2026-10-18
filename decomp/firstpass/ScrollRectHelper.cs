using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollRectHelper : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler, IEventSystemHandler
{
	private ScrollRect _scrollRect;

	private void OnEnable()
	{
		ScrollRect[] componentsInParent = GetComponentsInParent<ScrollRect>(includeInactive: true);
		_scrollRect = ((componentsInParent.Length <= 0) ? null : componentsInParent[0]);
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		ScrollRect[] componentsInParent = GetComponentsInParent<ScrollRect>(includeInactive: true);
		_scrollRect = ((componentsInParent.Length <= 0) ? null : componentsInParent[0]);
		if (!(_scrollRect == null))
		{
			_scrollRect.OnBeginDrag(eventData);
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		ScrollRect[] componentsInParent = GetComponentsInParent<ScrollRect>(includeInactive: true);
		_scrollRect = ((componentsInParent.Length <= 0) ? null : componentsInParent[0]);
		if (!(_scrollRect == null))
		{
			_scrollRect.OnDrag(eventData);
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		ScrollRect[] componentsInParent = GetComponentsInParent<ScrollRect>(includeInactive: true);
		_scrollRect = ((componentsInParent.Length <= 0) ? null : componentsInParent[0]);
		if (!(_scrollRect == null))
		{
			_scrollRect.OnEndDrag(eventData);
		}
	}

	public void OnScroll(PointerEventData data)
	{
		ScrollRect[] componentsInParent = GetComponentsInParent<ScrollRect>(includeInactive: true);
		_scrollRect = ((componentsInParent.Length <= 0) ? null : componentsInParent[0]);
		if (!(_scrollRect == null))
		{
			_scrollRect.OnScroll(data);
		}
	}
}
