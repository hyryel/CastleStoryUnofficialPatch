using System.Collections.Generic;
using Brix.Engine;
using Brix.Lifecycle.Pooling;
using Brix.Utils;
using Smooth.Slinq;
using UnityEngine;

public class SelectionCircleDispenser : MonoBehaviour
{
	public static SelectionCircleDispenser Instance;

	private Dictionary<Vector3, SelectionCircle> highlightByPosition = new Dictionary<Vector3, SelectionCircle>();

	private Dictionary<GameObject, SelectionCircle> highlightByGameObject = new Dictionary<GameObject, SelectionCircle>();

	private void Awake()
	{
		Instance = this;
	}

	public SelectionCircle AddOrGet(Vector3 position)
	{
		return highlightByPosition.GetValueOrCreate(position, SelectionCircle.New);
	}

	public SelectionCircle AddOrGet(GameObject go)
	{
		return highlightByGameObject.GetValueOrCreate(go, (GameObject p) => SelectionCircle.New(go));
	}

	public void Remove(GameObject go, float delay = 0f)
	{
		SelectionCircle valueOrDefaultAndRemove = highlightByGameObject.GetValueOrDefaultAndRemove(go, null);
		if ((bool)valueOrDefaultAndRemove)
		{
			_Remove(valueOrDefaultAndRemove.gameObject, delay);
		}
	}

	private void _Remove(GameObject go, float delay)
	{
		if (delay != 0f)
		{
			Neo.Relais(delegate
			{
				ObjectPoolSingleton.Release(go);
			}, delay, "RemoveCircle");
		}
		else
		{
			ObjectPoolSingleton.Release(go);
		}
	}

	public void Remove(Vector3 position, float delay = 0f)
	{
		SelectionCircle valueOrDefaultAndRemove = highlightByPosition.GetValueOrDefaultAndRemove(position, null);
		if ((bool)valueOrDefaultAndRemove)
		{
			_Remove(valueOrDefaultAndRemove.gameObject, delay);
		}
	}

	public void Clear()
	{
		highlightByPosition.Values.Slinq().NotNull().ForEach(delegate(SelectionCircle c)
		{
			ObjectPoolSingleton.Release(c.gameObject);
		});
		highlightByPosition.Clear();
		highlightByGameObject.Values.Slinq().NotNull().ForEach(delegate(SelectionCircle c)
		{
			ObjectPoolSingleton.Release(c.gameObject);
		});
		highlightByGameObject.Clear();
	}
}
