using System.Collections.Generic;
using Brix.External.Signals;
using Brix.Utils;
using UnityEngine;
using UnityEngine.UI;

public class ContentSizeFitterPlus : MonoBehaviour
{
	public enum Operation
	{
		Nothing,
		MinSize,
		PreferredSize
	}

	public enum Target
	{
		LayoutElement,
		RectTransform
	}

	public bool _isConstrainOnEnable;

	public bool _isConstrainOnLateEnable;

	public bool _isConstrainOnEofEnable;

	public bool _isConstrainOnRectChange;

	public bool _isConstrainOnUpdate;

	public bool _isConstrainOnLateUpdate;

	public bool _isConstrainOnEof;

	public bool _ignoreZeroes;

	private SignalConnection _onEofOnceSignalConnection;

	private SignalConnection _onEofSignalConnection;

	private int _hierarchyCount = -1;

	private readonly List<RectTransform> _childRectTransforms = new List<RectTransform>();

	private readonly List<SignalConnection> _signalConnections = new List<SignalConnection>();

	public Operation _horizontalFit;

	public Operation _verticalFit;

	public Target _target;

	public Vector2 _sizeOffset = Vector2.zero;

	public bool _useBounds;

	public bool _useAltBounds;

	public bool _useAltBounds2;

	public bool _useAllDescendants = true;

	public float PaddingX;

	public float PaddingY;

	private const float _failSafe = 5000f;

	private RectTransform _rectTrs;

	private LayoutElement _layoutElement;

	public List<RectTransform> _slaveRectTransforms = new List<RectTransform>();

	public List<LayoutElement> _slaveLayoutElements = new List<LayoutElement>();

	private void CacheChildRectTransforms(bool force = false)
	{
		if (force || _hierarchyCount != base.transform.hierarchyCount)
		{
			_hierarchyCount = base.transform.hierarchyCount;
			_childRectTransforms.Clear();
			for (int i = 0; i < base.transform.childCount; i++)
			{
				Transform child = base.transform.GetChild(i);
				RectTransform item = child as RectTransform;
				_childRectTransforms.Add(item);
			}
		}
	}

	private void OnEnable()
	{
		CacheChildRectTransforms(force: true);
		if (_isConstrainOnEnable)
		{
			Constrain();
		}
		if (_isConstrainOnLateEnable)
		{
			PersistentCoroutineRunner.Relais(delegate(ContentSizeFitterPlus aContentSizeFitterPlus)
			{
				TryConstrain(aContentSizeFitterPlus);
			}, this);
		}
		ConnectAll();
	}

	private void OnDisable()
	{
		DisconnectAll();
	}

	private void ConnectAll()
	{
		if (_isConstrainOnEof)
		{
			_signalConnections.Add(PersistentCoroutineRunner.OnEndOfFrameSignal.Connect(delegate(ContentSizeFitterPlus aContentSizeFitterPlus)
			{
				TryConstrain(aContentSizeFitterPlus);
			}, this));
		}
		else if (_isConstrainOnEofEnable)
		{
			_signalConnections.Add(PersistentCoroutineRunner.OnEndOfFrameSignal.ConnectOnce(delegate(ContentSizeFitterPlus aContentSizeFitterPlus)
			{
				TryConstrain(aContentSizeFitterPlus);
			}, this));
		}
		if (_isConstrainOnUpdate)
		{
			_signalConnections.Add(PersistentCoroutineRunner.UpdateSignal.Connect(delegate(ContentSizeFitterPlus aContentSizeFitterPlus)
			{
				TryConstrain(aContentSizeFitterPlus);
			}, this));
		}
		if (_isConstrainOnLateUpdate)
		{
			_signalConnections.Add(PersistentCoroutineRunner.LateUpdateSignal.Connect(delegate(ContentSizeFitterPlus aContentSizeFitterPlus)
			{
				TryConstrain(aContentSizeFitterPlus);
			}, this));
		}
	}

	private void DisconnectAll()
	{
		foreach (SignalConnection signalConnection in _signalConnections)
		{
			signalConnection.Disconnect();
		}
		_signalConnections.Clear();
	}

	private static void TryConstrain(ContentSizeFitterPlus csfp)
	{
		if (!(csfp == null))
		{
			csfp.Constrain();
		}
	}

	public void Constrain()
	{
		CacheChildRectTransforms();
		Vector2 zero = Vector2.zero;
		if (_useBounds)
		{
			Vector2 vector = default(Vector2);
			Vector2 vector2 = default(Vector2);
			int num = 0;
			foreach (RectTransform childRectTransform in _childRectTransforms)
			{
				if (!(childRectTransform == null) && childRectTransform.gameObject.activeSelf)
				{
					float num2 = ((!_useAltBounds) ? childRectTransform.rect.xMin : childRectTransform.localPosition.x);
					float num3 = ((!_useAltBounds) ? childRectTransform.rect.xMax : (childRectTransform.localPosition.x + childRectTransform.sizeDelta.x));
					float num4 = ((!_useAltBounds) ? childRectTransform.rect.yMin : childRectTransform.localPosition.y);
					float num5 = ((!_useAltBounds) ? childRectTransform.rect.yMax : (childRectTransform.localPosition.y + childRectTransform.sizeDelta.y));
					if (_useAltBounds2)
					{
						float num6 = num3 - num2;
						float num7 = num5 - num4;
						vector.y += num6;
						vector2.y += num7;
					}
					else
					{
						vector.x = ((!(vector.x > num2)) ? vector.x : num2);
						vector.y = ((!(vector.y < num3)) ? vector.y : num3);
						vector2.x = ((!(vector2.x > num4)) ? vector2.x : num4);
						vector2.y = ((!(vector2.y < num5)) ? vector2.y : num5);
					}
					num++;
				}
			}
			zero.x = vector.y - vector.x;
			zero.y = vector2.y - vector2.x;
			if (num > 1)
			{
				int num8 = num - 1;
				if (!_ignoreZeroes || zero.x > 0f)
				{
					zero.x += (float)num8 * PaddingX;
				}
				if (!_ignoreZeroes || zero.y > 0f)
				{
					zero.y += (float)num8 * PaddingY;
				}
			}
			zero.x = Mathf.Clamp(zero.x, -5000f, 5000f);
			zero.y = Mathf.Clamp(zero.y, -5000f, 5000f);
		}
		else
		{
			foreach (RectTransform childRectTransform2 in _childRectTransforms)
			{
				if (!(childRectTransform2 == null) && childRectTransform2.gameObject.activeSelf)
				{
					zero.x += childRectTransform2.sizeDelta.x;
					zero.y += childRectTransform2.sizeDelta.y;
				}
			}
		}
		if (!_ignoreZeroes || zero.x > 0f)
		{
			zero.x += _sizeOffset.x;
		}
		if (!_ignoreZeroes || zero.y > 0f)
		{
			zero.y += _sizeOffset.y;
		}
		if (_target == Target.LayoutElement)
		{
			if (_layoutElement == null)
			{
				_layoutElement = base.gameObject.GetComponent<LayoutElement>();
			}
			AffectLayoutElement(_layoutElement, zero);
		}
		else if (_target == Target.RectTransform)
		{
			if (_rectTrs == null)
			{
				_rectTrs = base.gameObject.transform as RectTransform;
			}
			AffectRectTransform(_rectTrs, zero);
		}
		AffectSlaveRectTransforms(zero);
		AffectSlaveLayoutElements(zero);
	}

	private void AffectSlaveRectTransforms(Vector2 childrenSize)
	{
		foreach (RectTransform slaveRectTransform in _slaveRectTransforms)
		{
			AffectRectTransform(slaveRectTransform, childrenSize);
		}
	}

	private void AffectRectTransform(RectTransform rectTrs, Vector2 childrenSize)
	{
		if (!(rectTrs == null))
		{
			Vector2 sizeDelta = rectTrs.sizeDelta;
			Vector2 sizeDelta2 = sizeDelta;
			if (_horizontalFit == Operation.MinSize)
			{
				sizeDelta2.x = childrenSize.x;
			}
			else if (_horizontalFit == Operation.PreferredSize)
			{
				sizeDelta2.x = childrenSize.x;
			}
			if (_verticalFit == Operation.MinSize)
			{
				sizeDelta2.y = childrenSize.y;
			}
			else if (_verticalFit == Operation.PreferredSize)
			{
				sizeDelta2.y = childrenSize.y;
			}
			if (_ignoreZeroes)
			{
				sizeDelta2.x = ((!(sizeDelta2.x >= 1f)) ? sizeDelta.x : sizeDelta2.x);
				sizeDelta2.y = ((!(sizeDelta2.y >= 1f)) ? sizeDelta.y : sizeDelta2.y);
			}
			rectTrs.sizeDelta = sizeDelta2;
		}
	}

	private void AffectLayoutElement(LayoutElement layoutElement, Vector2 childrenSize)
	{
		if (!(layoutElement == null))
		{
			if (_horizontalFit == Operation.MinSize)
			{
				layoutElement.minWidth = childrenSize.x;
			}
			else if (_horizontalFit == Operation.PreferredSize)
			{
				layoutElement.preferredWidth = childrenSize.x;
			}
			if (_verticalFit == Operation.MinSize)
			{
				layoutElement.minHeight = childrenSize.y;
			}
			else if (_verticalFit == Operation.PreferredSize)
			{
				layoutElement.preferredHeight = childrenSize.y;
			}
		}
	}

	private void AffectSlaveLayoutElements(Vector2 childrenSize)
	{
		foreach (LayoutElement slaveLayoutElement in _slaveLayoutElements)
		{
			AffectLayoutElement(slaveLayoutElement, childrenSize);
		}
	}
}
