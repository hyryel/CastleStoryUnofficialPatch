using Brix.Engine;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.NewUI.Tooltips;

public class TooltipArrows : MonoBehaviour
{
	public enum Direction
	{
		up,
		right,
		down,
		left,
		none
	}

	[SerializeField]
	private RectTransform tooltipContent;

	[SerializeField]
	private RectTransform target;

	[SerializeField]
	private GameObject arrowUp;

	[SerializeField]
	private GameObject arrowRight;

	[SerializeField]
	private GameObject arrowDown;

	[SerializeField]
	private GameObject arrowLeft;

	private Vector2 halfArrowsWidth;

	private void Awake()
	{
		Show(show: false);
	}

	private void Start()
	{
		halfArrowsWidth = ArrowWidth();
	}

	public void Follow(RectTransform tooltipContent, RectTransform target)
	{
		this.tooltipContent = tooltipContent;
		this.target = target;
	}

	public void NoFollow()
	{
		target = null;
	}

	private Vector2 ArrowWidth()
	{
		RectTransform component = arrowUp.GetComponent<RectTransform>();
		RectTransform component2 = arrowRight.GetComponent<RectTransform>();
		Vector2 result = new Vector2(component.sizeDelta.y / 2f, component2.sizeDelta.y / 2f);
		return result;
	}

	private void ShowArrow(Direction direction)
	{
		switch (direction)
		{
		case Direction.up:
			WithArrows(show: false);
			arrowUp.SetActive(value: true);
			break;
		case Direction.right:
			WithArrows(show: false);
			arrowRight.SetActive(value: true);
			break;
		case Direction.down:
			WithArrows(show: false);
			arrowDown.SetActive(value: true);
			break;
		case Direction.left:
			WithArrows(show: false);
			arrowLeft.SetActive(value: true);
			break;
		}
	}

	private void WithArrows(bool show = true)
	{
		arrowUp.SetActive(show);
		arrowRight.SetActive(show);
		arrowDown.SetActive(show);
		arrowLeft.SetActive(show);
	}

	public Direction DetermineArrowToKeep(Vector2 tooltipPosition, Vector2 targetPosition)
	{
		Vector2 vector = targetPosition - tooltipPosition;
		vector = Quaternion.Euler(0f, 0f, 45f) * vector;
		if (vector.x > 0f && vector.y > 0f)
		{
			return Direction.right;
		}
		if (vector.x > 0f && vector.y < 0f)
		{
			return Direction.down;
		}
		if (vector.x < 0f && vector.y > 0f)
		{
			return Direction.up;
		}
		if (vector.x < 0f && vector.y < 0f)
		{
			return Direction.left;
		}
		return Direction.none;
	}

	private void AdjustArrowAlongSide(RectTransform tooltipRect, Vector2 target, Direction direction)
	{
		Vector2 delta = target - tooltipRect.anchoredPosition;
		float position = KeepRightAxis(delta, tooltipRect, direction);
		SetArrowPosition(position, direction);
	}

	private float KeepRightAxis(Vector2 delta, RectTransform tooltipRect, Direction direction)
	{
		switch (direction)
		{
		case Direction.up:
		case Direction.down:
		{
			float num2 = tooltipRect.sizeDelta.x / 2f - halfArrowsWidth.x;
			return Mathf.Clamp(delta.x, 0f - num2, num2);
		}
		case Direction.right:
		case Direction.left:
		{
			float num = tooltipRect.sizeDelta.y / 2f - halfArrowsWidth.y;
			return Mathf.Clamp(delta.y, 0f - num, num);
		}
		default:
			return 0f;
		}
	}

	private void Show(bool show)
	{
		float a = (show ? 1 : 0);
		arrowUp.GetComponent<RawImage>().color = new Color(0f, 0f, 0f, a);
		arrowRight.GetComponent<RawImage>().color = new Color(0f, 0f, 0f, a);
		arrowDown.GetComponent<RawImage>().color = new Color(0f, 0f, 0f, a);
		arrowLeft.GetComponent<RawImage>().color = new Color(0f, 0f, 0f, a);
	}

	private void SetArrowPosition(float position, Direction direction)
	{
		switch (direction)
		{
		case Direction.up:
			arrowUp.GetComponent<RectTransform>().anchoredPosition = new Vector2(position, 0f);
			break;
		case Direction.right:
			arrowRight.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, position);
			break;
		case Direction.down:
			arrowDown.GetComponent<RectTransform>().anchoredPosition = new Vector2(position, 0f);
			break;
		case Direction.left:
			arrowLeft.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, position);
			break;
		}
	}

	private void SetArrow(RectTransform tooltipRect, Vector2 target)
	{
		Direction direction = DetermineArrowToKeep(tooltipRect.anchoredPosition, target);
		ShowArrow(direction);
		AdjustArrowAlongSide(tooltipRect, target, direction);
		Neo.Relais(delegate
		{
			Show(show: true);
		}, "TooltipArrow show");
	}

	private void Update()
	{
		if (!(tooltipContent == null) && !(target == null))
		{
			Vector2 vector = new Vector2(target.transform.position.x, target.transform.position.y);
			Vector2 vector2 = new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f);
			Vector2 vector3 = vector - vector2;
			SetArrow(tooltipContent, vector3);
		}
	}
}
