using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Factories;
using Brix.Game.Components;
using UnityEngine;

namespace Brix.Utils;

public class ClasseMoi : GameComponent
{
	public string ou;

	private static void MoveTo(string path, GameObject go)
	{
		Neo.ClasseMoi(path, go);
	}

	public override void OnRequested()
	{
		base.OnRequested();
		if (Factory.GetFactory(AssetKey.Factory).InstantiatedObjectDefaultPath == base.gameObject.Path())
		{
			Move();
		}
	}

	private void Move()
	{
		Neo.ClasseMoi(ou, base.gameObject);
	}

	public static void Move(List<GameObject> gos)
	{
		foreach (GameObject go in gos)
		{
			Move(go);
		}
	}

	public static void Move(GameObject go)
	{
		ClasseMoi component = go.GetComponent<ClasseMoi>();
		if (component != null)
		{
			component.Move();
			return;
		}
		FactoryImprint component2 = go.GetComponent<FactoryImprint>();
		if (component2 != null)
		{
			MoveTo(Factory.GetFactory(component2.AssetKey.Factory).InstantiatedObjectDefaultPath, go);
		}
		else
		{
			MoveTo("UnclassedObjects", go);
		}
	}
}
