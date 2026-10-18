using System;
using System.Collections.Generic;
using Brix.External.Signals;
using Brix.Input;
using UnityEngine;

public class HandleGroup2<T>
{
	public struct OnOffAction
	{
		public Action<T> on;

		public Action<T> off;

		public OnOffAction(Action<T> _on, Action<T> _off)
		{
			on = _on;
			off = _off;
		}
	}

	public float stickiness = 0.4f;

	public float maxRange = 8f;

	private OnOffAction actions;

	private Func<T, Vector3> getPosition;

	private List<T> elements = new List<T>();

	private Plane[] planes = new Plane[0];

	private int currentClosest;

	private float currentDist;

	private Signal<GameObject> disconnect = new Signal<GameObject>();

	private bool wasTooFar;

	public HandleGroup2(Action<T> on, Action<T> off, Func<T, Vector3> _getPosition)
	{
		actions = new OnOffAction((on != null) ? on : ((Action<T>)delegate
		{
		}), (off != null) ? off : ((Action<T>)delegate
		{
		}));
		getPosition = _getPosition;
	}

	public static HandleGroup2<DragHandle> Default()
	{
		return new HandleGroup2<DragHandle>(delegate(DragHandle h)
		{
			h.WakeUp();
		}, delegate(DragHandle h)
		{
			h.Sleep();
		}, (DragHandle h) => h.Origin);
	}

	public void Connect(Func<T, IReadOnlyEdgeSignal<T>> signalGet, Action<T> on, Action<T> off)
	{
		for (int i = 0; i < elements.Count; i++)
		{
			int _i = i;
			signalGet(elements[i]).Connect(delegate
			{
				ToAllExcept(_i, on);
			}, null, delegate
			{
				ToAllExcept(_i, off);
			}, disconnect);
		}
	}

	private void ToAllExcept(int index, Action<T> action)
	{
		for (int i = 0; i < elements.Count; i++)
		{
			if (i != index)
			{
				action(elements[i]);
			}
		}
	}

	public void AddHandle(T h)
	{
		elements.Add(h);
		if (planes.Length < elements.Count)
		{
			Array.Resize(ref planes, elements.Count);
		}
	}

	public void AddRange(List<T> add)
	{
		elements.AddRange(add);
		if (planes.Length < elements.Count)
		{
			Array.Resize(ref planes, elements.Count);
		}
	}

	public void Clear()
	{
		elements.Clear();
		currentClosest = -1;
		currentDist = float.MaxValue;
		disconnect.Invoke(null);
	}

	public void AddInto(HandleGroup2<T> group)
	{
		for (int i = 0; i < elements.Count; i++)
		{
			group.AddHandle(elements[i]);
		}
	}

	private void SetupPlanes()
	{
		for (int i = 0; i < elements.Count; i++)
		{
			Vector3 vector = getPosition(elements[i]);
			Vector3 vector2 = Camera.main.transform.position - vector;
			planes[i].SetNormalAndPosition(vector2.normalized, vector);
		}
	}

	private float DistFromCursor(int index, Ray ray)
	{
		if (planes[index].Raycast(ray, out var enter))
		{
			return Vector3.SqrMagnitude(ray.GetPoint(enter) - getPosition(elements[index]));
		}
		return float.MaxValue;
	}

	private void ClosestRaycast(out int index, out float minDist)
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		index = 0;
		minDist = float.MaxValue;
		for (int i = 0; i < elements.Count; i++)
		{
			float num = DistFromCursor(i, ray);
			if (i == currentClosest)
			{
				currentDist = num;
			}
			else if (num < minDist)
			{
				minDist = num;
				index = i;
			}
		}
	}

	private void ToggleAll(int bigOne)
	{
		for (int i = 0; i < elements.Count; i++)
		{
			if (i == bigOne)
			{
				actions.on(elements[i]);
			}
			else
			{
				actions.off(elements[i]);
			}
		}
	}

	public void Refresh()
	{
		SetupPlanes();
		ClosestRaycast(out var index, out var minDist);
		if (minDist > maxRange && currentDist > maxRange)
		{
			ToggleAll(-1);
			wasTooFar = true;
		}
		else if (minDist < (1f - stickiness) * currentDist || wasTooFar)
		{
			currentClosest = index;
			ToggleAll(currentClosest);
			wasTooFar = false;
		}
	}
}
