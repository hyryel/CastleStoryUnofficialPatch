using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LayoutConstraintB : MonoBehaviour
{
	public enum ConstraintType
	{
		Unconstrained,
		MinSize,
		PreferredSize
	}

	public enum BoundsType
	{
		Default,
		Rect,
		Lowest,
		Highest
	}

	public List<RectTransform> _rectTransforms;

	public List<LayoutElement> _layoutElements;

	public ConstraintType _horizontalFit;

	public ConstraintType _verticalFit;

	public Vector2 _boundsOffset = Vector2.zero;

	public Vector2 _maxBounds = new Vector2(5000f, 5000f);

	public int _minDescendantDepth = 1;

	public int _maxDescendantDepth = -1;

	private RectTransform _self;

	private readonly List<List<RectTransform>> _children = new List<List<RectTransform>>();

	public BoundsType _boundsType;

	[ContextMenu("ResetCache")]
	public void ResetCache()
	{
		ResetChildrenCache();
	}

	private void ResetChildrenCache()
	{
		_self = base.transform as RectTransform;
		_children.Clear();
		ResetChildrenCacheRecursive(_self, 0);
	}

	private void ResetChildrenCacheRecursive(RectTransform parent, int depth)
	{
		if (_children.Count < depth + 1)
		{
			_children.Add(new List<RectTransform>());
		}
		_children[depth].Add(parent);
		depth++;
		foreach (RectTransform item in parent)
		{
			ResetChildrenCacheRecursive(item, depth);
		}
	}

	private Vector2 GetBounds()
	{
		if (_maxDescendantDepth != -1 && _minDescendantDepth > _maxDescendantDepth)
		{
			return default(Vector2);
		}
		if (_maxDescendantDepth == 0)
		{
			return default(Vector2);
		}
		Vector2 vector = default(Vector2);
		Vector2 vector2 = default(Vector2);
		int num = ((_minDescendantDepth != -1) ? _minDescendantDepth : 0);
		int num2 = ((_maxDescendantDepth != -1) ? Mathf.Min(_maxDescendantDepth, _children.Count) : (_children.Count - 1));
		for (int i = num; i <= num2; i++)
		{
			foreach (RectTransform item in _children[i])
			{
				if (item.gameObject.activeSelf)
				{
					float x = item.localPosition.x;
					float xMin = item.rect.xMin;
					float y = item.localPosition.y;
					float yMin = item.rect.yMin;
					float num3 = item.localPosition.x + item.sizeDelta.x;
					float xMax = item.rect.xMax;
					float num4 = item.localPosition.y + item.sizeDelta.y;
					float yMax = item.rect.yMax;
					float num5 = x;
					float num6 = y;
					float num7 = num3;
					float num8 = num4;
					switch (_boundsType)
					{
					case BoundsType.Rect:
						num5 = xMin;
						num6 = yMin;
						num7 = xMax;
						num8 = yMax;
						break;
					case BoundsType.Highest:
						num5 = ((!(x < xMin)) ? xMin : x);
						num6 = ((!(y < yMin)) ? yMin : y);
						num7 = ((!(num3 > xMax)) ? xMax : num3);
						num8 = ((!(num4 > yMax)) ? yMax : num4);
						break;
					case BoundsType.Lowest:
						num5 = ((!(x > xMin)) ? xMin : x);
						num6 = ((!(y > yMin)) ? yMin : y);
						num7 = ((!(num3 < xMax)) ? xMax : num3);
						num8 = ((!(num4 < yMax)) ? yMax : num4);
						break;
					}
					vector.x = ((!(vector.x > num5)) ? vector.x : num5);
					vector.y = ((!(vector.y < num7)) ? vector.y : num7);
					vector2.x = ((!(vector2.x > num6)) ? vector2.x : num6);
					vector2.y = ((!(vector2.y < num8)) ? vector2.y : num8);
				}
			}
		}
		return new Vector2(Mathf.Clamp(vector.y - vector.x, 0f, _maxBounds.x), Mathf.Clamp(vector2.y - vector2.x, 0f, _maxBounds.y));
	}

	[ContextMenu("Constrain")]
	public void Constrain()
	{
		Vector2 bounds = GetBounds();
		bounds += _boundsOffset;
		foreach (LayoutElement layoutElement in _layoutElements)
		{
			switch (_horizontalFit)
			{
			case ConstraintType.MinSize:
				layoutElement.minWidth = bounds.x;
				break;
			case ConstraintType.PreferredSize:
				layoutElement.preferredWidth = bounds.x;
				break;
			}
			switch (_verticalFit)
			{
			case ConstraintType.MinSize:
				layoutElement.minHeight = bounds.y;
				break;
			case ConstraintType.PreferredSize:
				layoutElement.preferredHeight = bounds.y;
				break;
			}
		}
		foreach (RectTransform rectTransform in _rectTransforms)
		{
			Vector2 sizeDelta = rectTransform.sizeDelta;
			switch (_horizontalFit)
			{
			case ConstraintType.MinSize:
				sizeDelta.x = bounds.x;
				break;
			case ConstraintType.PreferredSize:
				sizeDelta.x = bounds.x;
				break;
			}
			switch (_verticalFit)
			{
			case ConstraintType.MinSize:
				sizeDelta.y = bounds.y;
				break;
			case ConstraintType.PreferredSize:
				sizeDelta.y = bounds.y;
				break;
			}
			rectTransform.sizeDelta = sizeDelta;
		}
	}
}
