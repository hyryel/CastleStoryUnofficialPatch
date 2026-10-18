using System;
using UnityEngine;

namespace Brix.Lifecycle.Pooling;

public class PoolMonitor : MonoBehaviour
{
	public int InstancePeak = 0;

	public int PoolSizePeak = 0;

	public int TotalRequestAmount = 0;

	public IGameObjectPool GameObjectPool;

	private void Start()
	{
		GameObjectPool.GameObjectAddedToPool += OnGameObjectAddedToPool;
		GameObjectPool.GameObjectRequestedFromPool += OnGameObjectRequested;
	}

	private void Update()
	{
		int num = GameObjectPool.PoolSize + GameObjectPool.InstancesRequested;
		if (num < GameObjectPool.InitialPoolSize)
		{
			GameObjectPool.CreatePoolObject();
		}
		else
		{
			base.enabled = false;
		}
	}

	private void OnGameObjectAddedToPool()
	{
		PoolSizePeak = Math.Max(PoolSizePeak, GameObjectPool.PoolSize);
	}

	private void OnGameObjectRequested(GameObject go)
	{
		TotalRequestAmount++;
		InstancePeak = Math.Max(InstancePeak, GameObjectPool.InstancesRequested);
	}
}
