using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Brix.NewUI.Tooltips;

public class TooltipPositioner : MonoBehaviour
{
	public enum Direction
	{
		up,
		right,
		down,
		left,
		all,
		none
	}

	public enum Points
	{
		up,
		upright,
		right,
		downright,
		down,
		downleft,
		left,
		upleft,
		none
	}

	private class uiHit
	{
		public Points point;

		public Direction direction;

		public RectTransform rect;

		public uiHit(Points point, RectTransform rect)
		{
			this.point = point;
			this.rect = rect;
			direction = Direction.none;
		}
	}

	public const int FRAME_DELAY_BEFORE_SET_POSITION = 2;

	public const int MAX_ATTEMPT_AT_SETTING_POSITION = 1;

	[SerializeField]
	public List<GameObject> DontCollideWithThese;

	[SerializeField]
	private Vector2 offsetFromTarget;

	[SerializeField]
	private Vector2 stayAwayFromScreenEdgeByThisMuch;

	public RectTransform Target;

	public RectTransform TooltipContent;

	[SerializeField]
	private bool dynamicPosition;

	[SerializeField]
	private Direction positionPriority = Direction.none;

	private bool positionRequest;

	private Action positioningDoneCallback;

	private List<Direction> DirectionPriorities = new List<Direction>
	{
		Direction.up,
		Direction.left,
		Direction.down,
		Direction.right
	};

	public bool FollowCursor { get; set; }

	private void Start()
	{
	}

	public void RequestPositioning(RectTransform rect, RectTransform target, Direction positionPriority = Direction.none)
	{
		Target = target;
		TooltipContent = rect;
		this.positionPriority = positionPriority;
		positionRequest = true;
	}

	public void SetDirectionPriority(Direction positionPriority)
	{
		this.positionPriority = positionPriority;
	}

	public void SetPositioningDonwCallback(Action callback)
	{
		positioningDoneCallback = callback;
	}

	public void SetOffsetFromTarget(Vector3 offset)
	{
		offsetFromTarget = offset;
	}

	private void PositionTooltip(RectTransform rect, RectTransform target, Direction priority = Direction.none)
	{
		if (priority == Direction.none || !TrySetPositionInScreen(rect, target, priority) || !TrySetPositionAroundUI(rect, target, priority))
		{
			for (int i = 0; i < 1 && !TrySetPositionInScreen(rect, target, DirectionPriorities[i]); i++)
			{
			}
		}
	}

	public void PositionIsDynamic(bool value)
	{
		dynamicPosition = value;
	}

	private bool TrySetPositionInScreen(RectTransform rect, RectTransform target, Direction direction)
	{
		if (NeedsRelocationBecauseOfSceenLimits(rect, target, direction))
		{
			return false;
		}
		Vector2 position = DeterminePosition(rect, target, direction);
		position = ClampInScreen(position, rect);
		rect.anchoredPosition = position;
		return true;
	}

	private bool TrySetPositionAroundUI(RectTransform rect, RectTransform target, Direction direction)
	{
		List<uiHit> list = new List<uiHit>();
		Vector2 vector = Vector2.one * float.MaxValue;
		Vector2 vector2 = Vector2.zero;
		for (int i = 0; i < 1; i++)
		{
			list = ClashesWithOtherUI(rect);
			list = DetermineHitDirection(list, rect);
			vector = ComputeMaxPosition(rect, list, stayAwayFromScreenEdgeByThisMuch);
			if (vector == vector2)
			{
				break;
			}
			vector2 = vector;
			if (list.Any() && NeedsRelocationBecauseOfHit(rect, vector, direction, list))
			{
				return false;
			}
			rect.anchoredPosition = ClampPositionAccordingToDirection(rect.anchoredPosition, list, vector);
		}
		return true;
	}

	private bool NeedsRelocationBecauseOfSceenLimits(RectTransform rect, RectTransform target, Direction direction)
	{
		Vector2 vector = new Vector2(Screen.width * 2, Screen.height * 2);
		Vector2 min = new Vector2(target.transform.position.x, target.transform.position.y) - vector;
		return direction switch
		{
			Direction.up => !IsEnoughSpace(vector, min, target.sizeDelta / 2f, rect.sizeDelta, 1, horizontal: false), 
			Direction.right => !IsEnoughSpace(vector, min, target.sizeDelta / 2f, rect.sizeDelta, 1, horizontal: true), 
			Direction.down => !IsEnoughSpace(vector, min, target.sizeDelta / 2f, rect.sizeDelta, -1, horizontal: false), 
			Direction.left => !IsEnoughSpace(vector, min, target.sizeDelta / 2f, rect.sizeDelta, -1, horizontal: true), 
			_ => false, 
		};
	}

	private bool IsEnoughSpace(Vector2 max, Vector2 min, Vector2 targetRadius, Vector2 neededSpace, int reverse, bool horizontal)
	{
		if (horizontal)
		{
			float num = max.x - (float)reverse * min.x;
			float num2 = targetRadius.x + stayAwayFromScreenEdgeByThisMuch.x + offsetFromTarget.x;
			float num3 = num - num2;
			return num3 > neededSpace.x;
		}
		float num4 = max.y - (float)reverse * min.y;
		float num5 = targetRadius.y + stayAwayFromScreenEdgeByThisMuch.x + offsetFromTarget.y;
		float num6 = num4 - num5;
		return num6 > neededSpace.y;
	}

	private Vector2 ClampInScreen(Vector2 position, RectTransform rect)
	{
		Vector2 vector = new Vector2(Screen.width * 2, Screen.height * 2);
		Vector2 vector2 = vector - rect.sizeDelta / 2f - stayAwayFromScreenEdgeByThisMuch;
		position.x = Mathf.Clamp(position.x, 0f - vector2.x, vector2.x);
		position.y = Mathf.Clamp(position.y, 0f - vector2.y, vector2.y);
		return position;
	}

	private Vector2 DeterminePosition(RectTransform rect, RectTransform target, Direction direction)
	{
		int num = 0;
		Vector2 vector = Vector2.zero;
		Vector2 vector2 = Vector2.zero;
		if (direction == Direction.up || direction == Direction.right)
		{
			num = 1;
		}
		if (direction == Direction.left || direction == Direction.down)
		{
			num = -1;
		}
		Vector2 vector3 = new Vector2(target.transform.position.x, target.transform.position.y);
		if (direction == Direction.left || direction == Direction.right)
		{
			vector = new Vector2((float)num * (rect.sizeDelta.x / 2f + target.sizeDelta.x / 2f), 0f);
			vector2 = new Vector2((float)num * offsetFromTarget.x, 0f);
		}
		if (direction == Direction.up || direction == Direction.down)
		{
			vector = new Vector2(0f, (float)num * (rect.sizeDelta.y / 2f + target.sizeDelta.y / 2f));
			vector2 = new Vector2(0f, (float)num * offsetFromTarget.y);
		}
		Vector2 vector4 = new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f);
		return vector3 - vector4 + vector + vector2;
	}

	private bool NeedsRelocationBecauseOfHit(RectTransform rect, Vector2 max, Direction position, List<uiHit> hitList)
	{
		List<uiHit> source = hitList.Where((uiHit h) => h.direction == Direction.right || h.direction == Direction.left).ToList();
		List<uiHit> source2 = hitList.Where((uiHit h) => h.direction == Direction.down || h.direction == Direction.up).ToList();
		Vector2 vector = new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f);
		RectTransform component = Target.GetComponent<RectTransform>();
		Vector2 vector2 = new Vector2(component.transform.position.x, component.transform.position.y) - vector;
		Vector2 vector3 = component.sizeDelta / 2f;
		int num = 1;
		if (source.Any())
		{
			if (source.Any((uiHit h) => h.direction == Direction.left))
			{
				num = -1;
			}
			float num2 = max.x + rect.sizeDelta.x / 2f - (float)num * vector2.x;
			float num3 = vector3.x + 2f * stayAwayFromScreenEdgeByThisMuch.x + offsetFromTarget.x;
			float num4 = num2 - num3;
			bool flag = source.Any((uiHit h) => h.direction == position);
			bool flag2 = num4 < rect.sizeDelta.x;
			if (flag && flag2)
			{
				return true;
			}
		}
		if (source2.Any())
		{
			if (source.Any((uiHit h) => h.direction == Direction.down))
			{
				num = -1;
			}
			float num5 = max.y + rect.sizeDelta.y / 2f - vector2.y;
			float num6 = vector3.y + 2f * stayAwayFromScreenEdgeByThisMuch.y + offsetFromTarget.y;
			float num7 = num5 - num6;
			bool flag3 = source2.Any((uiHit h) => h.direction == position);
			bool flag4 = num7 < rect.sizeDelta.y;
			if (flag3 && flag4)
			{
				return true;
			}
		}
		return false;
	}

	private Vector2 ComputeMaxPosition(RectTransform rect, List<uiHit> hitList, Vector2 buffferOffset)
	{
		Vector2 vector = new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f);
		Vector2 result = Vector2.one * float.MaxValue;
		Vector2 vector2 = rect.sizeDelta / 2f;
		Vector2 vector3 = new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f);
		if (!hitList.Any())
		{
			result.x = vector3.x - vector2.x - buffferOffset.x;
			result.y = vector3.y - vector2.y - buffferOffset.y;
			return result;
		}
		List<uiHit> source = hitList.Where((uiHit h) => h.direction == Direction.right || h.direction == Direction.left).ToList();
		List<uiHit> source2 = hitList.Where((uiHit h) => h.direction == Direction.down || h.direction == Direction.up).ToList();
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		if (source.Any())
		{
			RectTransform rect2 = source.First().rect;
			int num4 = 1;
			if (hitList.Any((uiHit h) => h.direction == Direction.left))
			{
				num4 = -1;
			}
			if (hitList.Any((uiHit h) => h.direction == Direction.right))
			{
				num4 = 1;
			}
			if (rect2 == null)
			{
				result.x = vector3.x;
			}
			else
			{
				num3 = rect2.transform.position.x - vector.x;
				num2 = (float)num4 * rect2.sizeDelta.x / 2f;
				num = num3 - num2;
				result.x = Mathf.Abs(num);
			}
			result.x = result.x - vector2.x - buffferOffset.x;
		}
		if (source2.Any())
		{
			RectTransform rect3 = source2.First().rect;
			int num5 = 1;
			if (hitList.Any((uiHit h) => h.direction == Direction.down))
			{
				num5 = -1;
			}
			if (hitList.Any((uiHit h) => h.direction == Direction.up))
			{
				num5 = 1;
			}
			if (rect3 == null)
			{
				result.y = vector3.y;
			}
			else
			{
				num3 = rect3.transform.position.y - vector.y;
				num2 = (float)num5 * rect3.sizeDelta.y / 2f;
				num = num3 - num2;
				result.y = Mathf.Abs(num);
			}
			result.y = result.y - vector2.y - buffferOffset.y;
		}
		return result;
	}

	private float MaxAccordingToAnchors(RectTransform conflict, float screenLimit, int reverse, float conflictBound)
	{
		float result = 0f;
		if (conflict.anchorMax == new Vector2(0.5f, 0.5f) && conflict.anchorMin == new Vector2(0.5f, 0.5f))
		{
			result = Mathf.Min(screenLimit, (float)reverse * conflictBound);
		}
		if (conflict.anchorMin.x == 1f && conflict.anchorMax.x == 1f)
		{
			result = Mathf.Min(screenLimit, screenLimit + conflictBound);
		}
		if (conflict.anchorMin.x == 0f && conflict.anchorMax.x == 0f)
		{
			result = Mathf.Min(screenLimit, screenLimit - conflictBound);
		}
		if (conflict.anchorMin.y == 0f && conflict.anchorMax.y == 0f)
		{
			result = Mathf.Min(screenLimit, screenLimit - conflictBound);
		}
		if (conflict.anchorMin.y == 1f && conflict.anchorMax.y == 1f)
		{
			result = Mathf.Min(screenLimit, screenLimit + conflictBound);
		}
		return result;
	}

	private List<uiHit> DetermineHitDirection(List<uiHit> hits, RectTransform rect)
	{
		Direction direction = Direction.none;
		foreach (uiHit hit in hits)
		{
			if (hit.point == Points.up)
			{
				hit.direction = Direction.up;
				direction = Direction.up;
			}
			if (hit.point == Points.right)
			{
				hit.direction = Direction.right;
				direction = Direction.right;
			}
			if (hit.point == Points.down)
			{
				hit.direction = Direction.down;
				direction = Direction.down;
			}
			if (hit.point == Points.left)
			{
				hit.direction = Direction.left;
				direction = Direction.left;
			}
			if (direction != Direction.none)
			{
				hit.direction = direction;
				continue;
			}
			float num = Mathf.Abs(hit.rect.transform.position.y - rect.transform.position.y);
			float num2 = Mathf.Abs(hit.rect.transform.position.x - rect.transform.position.x);
			if (hit.point == Points.upleft)
			{
				if (num > num2)
				{
					hit.direction = Direction.up;
				}
				else
				{
					hit.direction = Direction.left;
				}
			}
			if (hit.point == Points.upright)
			{
				if (num > num2)
				{
					hit.direction = Direction.up;
				}
				else
				{
					hit.direction = Direction.right;
				}
			}
			if (hit.point == Points.downright)
			{
				if (num > num2)
				{
					hit.direction = Direction.down;
				}
				else
				{
					hit.direction = Direction.right;
				}
			}
			if (hit.point == Points.downleft)
			{
				if (num > num2)
				{
					hit.direction = Direction.down;
				}
				else
				{
					hit.direction = Direction.left;
				}
			}
		}
		return hits;
	}

	private Vector2 ClampPositionAccordingToDirection(Vector2 position, List<uiHit> hits, Vector2 max)
	{
		Vector2 vector = new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f);
		if (hits.Any((uiHit d) => d.direction == Direction.right))
		{
			position.x = Mathf.Clamp(position.x, 0f - vector.x, max.x);
		}
		if (hits.Any((uiHit d) => d.direction == Direction.left))
		{
			position.x = Mathf.Clamp(position.x, 0f - max.x, vector.x);
		}
		if (hits.Any((uiHit d) => d.direction == Direction.up))
		{
			position.y = Mathf.Clamp(position.y, 0f - vector.y, max.y);
		}
		if (hits.Any((uiHit d) => d.direction == Direction.down))
		{
			position.y = Mathf.Clamp(position.y, 0f - max.y, vector.y);
		}
		if (!hits.Any())
		{
			position = new Vector2(Mathf.Clamp(position.x, 0f - vector.x, vector.x), Mathf.Clamp(position.y, 0f - vector.y, vector.y));
		}
		return position;
	}

	private bool RayCastPoint(Vector2 point, out RectTransform conflict, List<GameObject> exclude = null)
	{
		List<RaycastResult> list = new List<RaycastResult>();
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.position = point + new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f);
		EventSystem.current.RaycastAll(pointerEventData, list);
		if (list.Any())
		{
			List<RectTransform> rects = GetRects(list);
			conflict = GetHighestParent(rects);
			return true;
		}
		conflict = null;
		return false;
	}

	private List<RectTransform> GetRects(List<RaycastResult> hits)
	{
		List<RectTransform> list = new List<RectTransform>();
		foreach (RaycastResult hit in hits)
		{
			list.Add(hit.gameObject.GetComponent<RectTransform>());
		}
		return list;
	}

	private RectTransform GetHighestParent(List<RectTransform> rects)
	{
		RectTransform rectTransform = rects.First();
		foreach (RectTransform rect in rects)
		{
			if (!rect.transform.IsChildOf(rectTransform.transform))
			{
				rectTransform = rect;
			}
		}
		return rectTransform;
	}

	private Dictionary<Points, Vector2> GetPointsCoord(RectTransform rect)
	{
		Vector2 anchoredPosition = rect.anchoredPosition;
		Vector2 value = anchoredPosition + rect.sizeDelta / 2f;
		Vector2 value2 = anchoredPosition - rect.sizeDelta / 2f;
		Dictionary<Points, Vector2> dictionary = new Dictionary<Points, Vector2>();
		dictionary.Add(Points.up, new Vector2(anchoredPosition.x, value.y));
		dictionary.Add(Points.upright, value);
		dictionary.Add(Points.right, new Vector2(value.x, anchoredPosition.y));
		dictionary.Add(Points.downright, new Vector2(value.x, value2.y));
		dictionary.Add(Points.down, new Vector2(anchoredPosition.x, value2.y));
		dictionary.Add(Points.downleft, value2);
		dictionary.Add(Points.left, new Vector2(value2.x, anchoredPosition.y));
		dictionary.Add(Points.upleft, new Vector2(value2.x, value.y));
		return dictionary;
	}

	private List<uiHit> ClashesWithOtherUI(RectTransform rect)
	{
		List<uiHit> list = new List<uiHit>();
		Dictionary<Points, Vector2> pointsCoord = GetPointsCoord(rect);
		foreach (Points key in pointsCoord.Keys)
		{
			Vector2 point = pointsCoord[key];
			if (pointsCoord[key].x > (float)Screen.width / 2f)
			{
				point = new Vector2((float)Screen.width / 2f - stayAwayFromScreenEdgeByThisMuch.x, pointsCoord[key].y);
			}
			if (pointsCoord[key].x < (float)(-Screen.width) / 2f)
			{
				point = new Vector2((float)(-Screen.width) / 2f + stayAwayFromScreenEdgeByThisMuch.x, pointsCoord[key].y);
			}
			if (pointsCoord[key].y > (float)Screen.height / 2f)
			{
				point = new Vector2(pointsCoord[key].x, (float)Screen.height / 2f - stayAwayFromScreenEdgeByThisMuch.y);
			}
			if (pointsCoord[key].y < (float)(-Screen.height) / 2f)
			{
				point = new Vector2(pointsCoord[key].x, (float)(-Screen.width) / 2f + stayAwayFromScreenEdgeByThisMuch.y);
			}
			if (RayCastPoint(point, out var conflict, GetExceptions(rect)))
			{
				list.Add(new uiHit(key, conflict));
			}
		}
		return list;
	}

	public List<GameObject> GetExceptions(RectTransform rect)
	{
		List<GameObject> list = new List<GameObject>();
		list.Add(rect.gameObject);
		list.Add(Target.gameObject);
		List<GameObject> list2 = list;
		list2.AddRange(DontCollideWithThese);
		return list2;
	}

	private void Update()
	{
		if (dynamicPosition || positionRequest)
		{
			if (FollowCursor)
			{
				TooltipContent.position = new Vector2(UnityEngine.Input.mousePosition.x + 100f, UnityEngine.Input.mousePosition.y);
			}
			else
			{
				PositionTooltip(TooltipContent, Target, positionPriority);
			}
			positioningDoneCallback();
			positionRequest = false;
		}
	}
}
