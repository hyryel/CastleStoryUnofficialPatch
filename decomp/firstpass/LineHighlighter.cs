using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Lifecycle.Pooling;
using Brix.Utils;
using Brix.Utils.Collections;
using Smooth.Algebraics;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;

public class LineHighlighter : MonoBehaviour
{
	[SerializeField]
	private Transform linesParent;

	private static LineHighlighter _lineHighlighter;

	private Dictionary<Tuple<GameObject, GameObject>, LineRenderer> highlights = new Dictionary<Tuple<GameObject, GameObject>, LineRenderer>();

	private static LineHighlighter GetlineHighlighter => _lineHighlighter;

	private void Awake()
	{
		_lineHighlighter = this;
	}

	public static LineRenderer Add(GameObject obj1, GameObject obj2, Color color, float duration = -1f)
	{
		return GetlineHighlighter._Add(obj1, obj2, color, duration);
	}

	public static void Remove(GameObject obj1, GameObject obj2)
	{
		GetlineHighlighter._Remove(obj1, obj2);
	}

	public static void Remove(LineRenderer line)
	{
		GetlineHighlighter._Remove(line);
	}

	public static void RemoveAllFor(GameObject go)
	{
		GetlineHighlighter._RemoveAllFor(go);
	}

	public static void AddAtPos(Vector3 pos, Vector3 pos2, Color color, Signal removeWhen)
	{
		GetlineHighlighter._AddAtPos(pos, pos2, color, removeWhen);
	}

	private LineRenderer _AddAtPos(Vector3 pos, Vector3 pos2, Color color, Signal removeWhen)
	{
		ObjectPoolSingleton.RequestComponent<LineRenderer>(out var line, UI.LineHighlight);
		line.gameObject.transform.SetParent(linesParent);
		line.SetPosition(0, pos);
		line.SetPosition(1, pos2);
		line.startColor = color;
		line.endColor = color;
		line.startWidth = 0.1f;
		line.endWidth = 0.1f;
		removeWhen.Connect(delegate
		{
			if (line != null)
			{
				ObjectPoolSingleton.Release(line.gameObject);
			}
		});
		return line;
	}

	private LineRenderer _Add(GameObject obj1, GameObject obj2, Color color, float duration = -1f)
	{
		Tuple<GameObject, GameObject> key = new Tuple<GameObject, GameObject>(obj1, obj2);
		LineRenderer t;
		if (highlights.ContainsKey(key))
		{
			t = highlights[key];
		}
		else
		{
			ObjectPoolSingleton.RequestComponent<LineRenderer>(out t, UI.LineHighlight);
			highlights.Add(key, t);
			t.gameObject.transform.SetParent(linesParent);
		}
		t.SetPosition(0, obj1.transform.position);
		t.SetPosition(1, obj2.transform.position);
		t.startColor = color;
		t.endColor = color;
		t.startWidth = 0.1f;
		t.endWidth = 0.1f;
		if (duration != -1f)
		{
			Neo.Relais(delegate
			{
				Remove(obj1, obj2);
			}, duration, "remove line highlight");
		}
		return t;
	}

	private void _Remove(GameObject obj1, GameObject obj2)
	{
		Tuple<GameObject, GameObject> key = new Tuple<GameObject, GameObject>(obj1, obj2);
		if (highlights.ContainsKey(key))
		{
			ObjectPoolSingleton.Release(highlights[key].gameObject);
			highlights.Remove(key);
		}
	}

	private void _Remove(LineRenderer line)
	{
		KeyValuePair<Tuple<GameObject, GameObject>, LineRenderer> keyValuePair = highlights.Slinq().FirstOrDefault((KeyValuePair<Tuple<GameObject, GameObject>, LineRenderer> kvp) => kvp.Value == line);
		if (!keyValuePair.Equals(default(KeyValuePair<Tuple<GameObject, GameObject>, LineRenderer>)) && highlights.ContainsKey(keyValuePair.Key))
		{
			highlights.Remove(keyValuePair.Key);
		}
	}

	private void _RemoveAllFor(GameObject go)
	{
		Slinq<KeyValuePair<Tuple<GameObject, GameObject>, LineRenderer>, PredicateContext<KeyValuePair<Tuple<GameObject, GameObject>, LineRenderer>, DictionaryContext<Tuple<GameObject, GameObject>, LineRenderer>>> slinq = from kvp in highlights.Slinq()
			where kvp.Key._1 == go
			select kvp;
		if (!slinq.Any())
		{
			return;
		}
		List<Tuple<GameObject, GameObject>> list = slinq.Select((KeyValuePair<Tuple<GameObject, GameObject>, LineRenderer> kvp) => kvp.Key).ToList();
		foreach (Tuple<GameObject, GameObject> item in list)
		{
			_Remove(item._1, item._2);
		}
	}

	private void DeleteNulls()
	{
		Slinq<KeyValuePair<Tuple<GameObject, GameObject>, LineRenderer>, PredicateContext<KeyValuePair<Tuple<GameObject, GameObject>, LineRenderer>, DictionaryContext<Tuple<GameObject, GameObject>, LineRenderer>>> slinq = from kvp in highlights.Slinq()
			where kvp.Key._1 == null || kvp.Key._2 == null
			select kvp;
		List<Tuple<GameObject, GameObject>> list = slinq.Select((KeyValuePair<Tuple<GameObject, GameObject>, LineRenderer> kvp) => kvp.Key).ToPooledList();
		foreach (Tuple<GameObject, GameObject> item in list)
		{
			_Remove(item._1, item._2);
		}
		list.Dispose();
	}
}
