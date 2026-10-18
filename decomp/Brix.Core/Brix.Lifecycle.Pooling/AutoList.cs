using System.Collections.Generic;
using Brix.Components;
using Brix.External.Factories;
using Brix.Utils;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Lifecycle.Pooling;

public class AutoList : BrixSingleton<AutoList>
{
	private static BrixLogger logger = new BrixLogger();

	private Dictionary<Factory.AssetKey, HashSet<GameObject>> AutoLists = new Dictionary<Factory.AssetKey, HashSet<GameObject>>();

	protected override void Start()
	{
		base.Start();
		ObjectPoolSingleton.Instance.GameObjectRequestedSignal.Connect(AddToAutoList);
		ObjectPoolSingleton.Instance.GameObjectReleasedSignal.Connect(Remove);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		foreach (KeyValuePair<Factory.AssetKey, HashSet<GameObject>> autoList in AutoLists)
		{
			autoList.Value.Clear();
		}
		AutoLists.Clear();
	}

	public void AddToAutoList(Factory.AssetKey key, GameObject go)
	{
		if (!AutoLists.GetValueOrCreate(key).Add(go))
		{
			logger.WarnFormat("Autolisting the GameObject '{0}' with the key '{1}' failed because it was already added to the autolist system.", go.name, key);
		}
	}

	public void RemoveFromAutoList(GameObject go)
	{
		FactoryImprint component = go.GetComponent<FactoryImprint>();
		if (component != null)
		{
			RemoveFromAutoList(component.AssetKey, go);
		}
	}

	private void Remove(Factory.AssetKey key, GameObject go)
	{
		RemoveFromAutoList(key, go);
	}

	public bool RemoveFromAutoList(Factory.AssetKey key, GameObject go)
	{
		return AutoLists.GetValueOrCreate(key).Remove(go);
	}

	public HashSet<GameObject> GetInstances(Factory.AssetKey key)
	{
		return AutoLists.GetValueOrCreate(key);
	}
}
