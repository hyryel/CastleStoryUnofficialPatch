using System;
using System.Collections.Generic;
using Brix.External.Factories;
using Brix.Lifecycle.Pooling;
using Brix.Utils.Factories;
using UnityEngine;

public class CircleHighlighter : MonoBehaviour
{
	[SerializeField]
	private Material defaultMaterial;

	private static CircleHighlighter _circleHighlighter;

	private Dictionary<int, Dictionary<Vector3, CircleHighlight>> highlightByPosition = new Dictionary<int, Dictionary<Vector3, CircleHighlight>>();

	private Dictionary<int, Dictionary<GameObject, CircleHighlight>> highlightByGameObject = new Dictionary<int, Dictionary<GameObject, CircleHighlight>>();

	private int id;

	private static Material DefaultMaterial => GetCircleHighlighter.defaultMaterial;

	private static CircleHighlighter GetCircleHighlighter
	{
		get
		{
			if (_circleHighlighter == null)
			{
			}
			return _circleHighlighter;
		}
	}

	private void Awake()
	{
		_circleHighlighter = this;
	}

	public static CircleHighlight HighlightGameObject(int id, GameObject obj, Material material, float radius)
	{
		CircleHighlight circleHighlight = GetCircleHighlighter.AddOrGet(id, obj);
		circleHighlight.Material = new Material(material);
		circleHighlight.Radius = radius;
		return circleHighlight;
	}

	public static CircleHighlight HighlightGameObject(int id, GameObject obj, Color color, float radius)
	{
		CircleHighlight circleHighlight = HighlightGameObject(id, obj, DefaultMaterial, radius);
		circleHighlight.Color = color;
		return circleHighlight;
	}

	public static CircleHighlight HighlightPosition(int id, Vector3 pos, Material material, float radius)
	{
		CircleHighlight circleHighlight = GetCircleHighlighter.AddOrGet(id, pos);
		circleHighlight.Radius = radius;
		circleHighlight.Material = new Material(material);
		return circleHighlight;
	}

	public static CircleHighlight HighlightPosition(int id, Vector3 position, Color color, float radius)
	{
		CircleHighlight circleHighlight = HighlightPosition(id, position, DefaultMaterial, radius);
		circleHighlight.Color = color;
		return circleHighlight;
	}

	public static CircleHighlight GetHighlight(int id, Vector3 pos)
	{
		return GetCircleHighlighter._GetPositionHighligth(id, pos);
	}

	public static CircleHighlight GetHighlight(int id, GameObject obj)
	{
		return GetCircleHighlighter._GetGameObjectHighligth(id, obj);
	}

	public static void Remove(int id, Vector3 position)
	{
		GetCircleHighlighter._Remove(id, position);
	}

	public static void Remove(int id, GameObject go)
	{
		GetCircleHighlighter._Remove(id, go);
	}

	public static void Clear(int id)
	{
		GetCircleHighlighter._Clear(id);
	}

	public static int GetId()
	{
		return GetCircleHighlighter.GetHighlighId();
	}

	private int GetHighlighId()
	{
		id++;
		highlightByPosition.Add(id, new Dictionary<Vector3, CircleHighlight>());
		highlightByGameObject.Add(id, new Dictionary<GameObject, CircleHighlight>());
		return id;
	}

	private CircleHighlight _GetPositionHighligth(int id, Vector3 pos)
	{
		if (!highlightByPosition.ContainsKey(id))
		{
			throw new Exception("Invalid Id, get a id using the appropriate function");
		}
		if (!highlightByPosition[id].ContainsKey(pos))
		{
			return null;
		}
		return highlightByPosition[id][pos];
	}

	private CircleHighlight _GetGameObjectHighligth(int id, GameObject obj)
	{
		if (!highlightByPosition.ContainsKey(id))
		{
			throw new Exception("Invalid Id, get a id using the appropriate function");
		}
		if (!highlightByGameObject[id].ContainsKey(obj))
		{
			return null;
		}
		return highlightByGameObject[id][obj];
	}

	private CircleHighlight AddNew(int id, Vector3 position)
	{
		if (!highlightByPosition.ContainsKey(id))
		{
			throw new Exception("Invalid Id, get a id using the appropriate function");
		}
		ObjectPoolSingleton.RequestComponent<CircleHighlight>(out var t, UI.CircleHighlight);
		t.transform.position = position;
		t.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
		if (!highlightByPosition[id].ContainsKey(position))
		{
			highlightByPosition[id].Add(position, t);
		}
		return t;
	}

	private CircleHighlight AddOrGet(int id, Vector3 position)
	{
		if (!highlightByPosition.ContainsKey(id))
		{
			throw new Exception("Invalid Id, get a id using the appropriate function");
		}
		if (highlightByPosition[id].ContainsKey(position))
		{
			return highlightByPosition[id][position];
		}
		return AddNew(id, position);
	}

	private void _Remove(int id, Vector3 position)
	{
		if (!highlightByPosition.ContainsKey(id))
		{
			throw new Exception("Invalid Id, get a id using the appropriate function");
		}
		if (highlightByPosition[id].ContainsKey(position))
		{
			CircleHighlight circleHighlight = highlightByPosition[id][position];
			highlightByPosition[id].Remove(position);
			ObjectPoolSingleton.Release(circleHighlight.gameObject);
		}
	}

	private CircleHighlight AddNew(int id, GameObject go)
	{
		if (!highlightByPosition.ContainsKey(id))
		{
			throw new Exception("Invalid Id, get a id using the appropriate function");
		}
		ObjectPoolSingleton.RequestComponent<CircleHighlight>(out var t, UI.CircleHighlight);
		t.transform.parent = go.transform;
		t.transform.localPosition = new Vector3(0f, ComputeYOffset(go), 0f);
		t.transform.localEulerAngles = Vector3.zero;
		if (!highlightByGameObject[id].ContainsKey(go))
		{
			highlightByGameObject[id].Add(go, t);
		}
		return t;
	}

	private float ComputeYOffset(GameObject go)
	{
		return go.WithComponent((MeshFilter m) => m.mesh.bounds.center.y - m.mesh.bounds.extents.y, 0f) + 0.3f;
	}

	private CircleHighlight AddOrGet(int id, GameObject go)
	{
		if (!highlightByPosition.ContainsKey(id))
		{
			throw new Exception("Invalid Id, get a id using the appropriate function");
		}
		if (highlightByGameObject[id].ContainsKey(go))
		{
			return highlightByGameObject[id][go];
		}
		return AddNew(id, go);
	}

	private void _Remove(int id, GameObject go)
	{
		if (!highlightByPosition.ContainsKey(id))
		{
			throw new Exception("Invalid Id, get a id using the appropriate function");
		}
		if (highlightByGameObject[id].ContainsKey(go))
		{
			CircleHighlight circleHighlight = highlightByGameObject[id][go];
			highlightByGameObject[id].Remove(go);
			ObjectPoolSingleton.Release(circleHighlight.gameObject);
		}
	}

	private void _Clear(int id)
	{
		if (!highlightByPosition.ContainsKey(id))
		{
			throw new Exception("Invalid Id, get a id using the appropriate function");
		}
		foreach (Vector3 key in highlightByPosition[id].Keys)
		{
			if (highlightByPosition[id][key] != null)
			{
				ObjectPoolSingleton.Release(highlightByPosition[id][key].gameObject);
			}
		}
		highlightByPosition[id].Clear();
		foreach (GameObject key2 in highlightByGameObject[id].Keys)
		{
			if (highlightByGameObject[id][key2] != null)
			{
				ObjectPoolSingleton.Release(highlightByGameObject[id][key2].gameObject);
			}
		}
		highlightByGameObject[id].Clear();
	}
}
