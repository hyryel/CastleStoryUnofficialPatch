using Brix.External.Signals;
using Brix.Utils.Logging;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Brix.Game.Components.Tunnel;

public class TunnelHandle : MonoBehaviour
{
	private static BrixLogger logger = new BrixLogger(typeof(TunnelHandle).FullName);

	public Signal Clicked = new Signal();

	[SerializeField]
	private TunnelUnit unit;

	private Vector3? dragStart;

	public void Drag(BaseEventData eventData)
	{
		Vector3? vector = dragStart;
		if (!vector.HasValue)
		{
			dragStart = unit.transform.position;
		}
		PointerEventData pointerEventData = eventData as PointerEventData;
		logger.Info(string.Concat("Dragging - ", pointerEventData.pointerDrag.name, " : ", pointerEventData.pressPosition, " : ", pointerEventData.position));
		Vector3 vector2 = Camera.main.ScreenToWorldPoint(pointerEventData.position);
		Vector3 vector3 = dragStart.Value - vector2;
		logger.Info(string.Concat("drag started at:", dragStart.Value, " and is now at: ", vector2, " for a total of: ", vector3));
	}

	public void DragEnd(BaseEventData eventData)
	{
		dragStart = null;
		PointerEventData pointerEventData = eventData as PointerEventData;
		logger.Info(string.Concat("DragEnded - ", pointerEventData.pressPosition, " : ", pointerEventData.position));
	}

	public void Click(BaseEventData eventData)
	{
		Clicked.Invoke();
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
