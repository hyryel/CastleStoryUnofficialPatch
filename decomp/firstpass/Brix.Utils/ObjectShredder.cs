using System.Collections.Generic;
using Brix.Lifecycle.Pooling;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Utils;

[ExecuteInEditMode]
public class ObjectShredder : MonoBehaviour
{
	private static List<GameObject> _shreds = new List<GameObject>();

	private void Update()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		lock (_shreds)
		{
			(from go in _shreds.Slinq()
				where go != null
				select go).ForEach(delegate(GameObject g)
			{
				ObjectPoolSingleton.Release(g);
			});
			_shreds.Clear();
		}
	}

	public static void Shred(GameObject obj)
	{
		lock (_shreds)
		{
			_shreds.Add(obj);
		}
	}
}
