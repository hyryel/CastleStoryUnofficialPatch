using System;
using Brix.External.Factories;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Lifecycle.Pooling;

[Serializable]
public class GameObjectPool : AbstractGameObjectPool
{
	private static BrixLogger logger = new BrixLogger();

	[SerializeField]
	private int _initialPoolSize;

	[SerializeField]
	private int _poolSizeLimit;

	[SerializeField]
	private int _instanceLimit;

	[SerializeField]
	private Transform _poolContainer;

	[SerializeField]
	private bool _keepActive;

	public override int PoolSize => PoolContainer.childCount;

	public override int InitialPoolSize => _initialPoolSize;

	public override int PoolSizeLimit => _poolSizeLimit;

	public override int InstanceLimit => _instanceLimit;

	protected override Transform PoolContainer => _poolContainer;

	public override bool KeepActive => _keepActive;

	public GameObjectPool(Factory.AssetKey key, Transform poolContainer, PoolConfiguration poolConfiguration)
		: base(key)
	{
		_initialPoolSize = poolConfiguration.InitialPoolSize;
		_poolSizeLimit = poolConfiguration.MaximumPoolSize;
		_instanceLimit = poolConfiguration.InstanceLimit;
		_poolContainer = poolContainer;
		_keepActive = poolConfiguration.KeepActive;
	}

	public override void ReleaseToPool(GameObject go)
	{
		if (go.transform.parent == PoolContainer)
		{
			logger.WarnFormat("GameObject '{0}', was attempted to be released more than once. GameObject Release canceled (AssetKey='{1}')", go.name, AssetKey);
			return;
		}
		_instancesRequested--;
		go.SetActive(KeepActive);
		if (PoolSize < PoolSizeLimit)
		{
			OnGameObjectAddedToPool();
			go.transform.SetParent(PoolContainer, worldPositionStays: false);
		}
		else
		{
			logger.InfoFormat("GameObject pool is full for key '{0}', destroying object '{1}' instead of keeping it", AssetKey, go.name);
			go.transform.SetParent(PoolContainer, worldPositionStays: false);
			UnityEngine.Object.Destroy(go);
		}
	}

	public override bool RequestFromPool(out GameObject returnedGO, Vector3 p, Quaternion r, bool active)
	{
		if (PoolContainer.childCount == 0)
		{
			if (base.InstancesRequested >= InstanceLimit)
			{
				returnedGO = null;
				logger.ErrorFormat("Pool request for key '{0}' could not be completed. Instance limit ({1}) has been encoutered and can no longer spawn additionnal objects", AssetKey, InstanceLimit);
				return false;
			}
			returnedGO = CreatePoolObject(p, r);
		}
		else
		{
			returnedGO = PoolContainer.GetChild(PoolContainer.childCount - 1).gameObject;
		}
		_instancesRequested++;
		Transform transform = returnedGO.transform;
		transform.position = p;
		transform.rotation = r;
		OnGameObjectRequestedFromPool(returnedGO);
		transform.SetParent(base.DefaultOutputPath, worldPositionStays: false);
		returnedGO.SetActive(active);
		return true;
	}
}
