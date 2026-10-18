using Brix.External.Factories;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Lifecycle.Pooling;

[SerializeField]
public class DummyGameObjectPool : AbstractGameObjectPool
{
	private static BrixLogger logger = new BrixLogger();

	private Transform _garbageZone;

	protected override Transform PoolContainer => base.DefaultOutputPath;

	public override int PoolSize => 0;

	public override int InitialPoolSize => 0;

	public override int PoolSizeLimit => 0;

	public override int InstanceLimit => 0;

	public override bool KeepActive => false;

	public DummyGameObjectPool(Factory.AssetKey key, Transform poolContainer)
		: base(key)
	{
		_garbageZone = poolContainer;
	}

	public override void ReleaseToPool(GameObject go)
	{
		if (go.transform.parent == _garbageZone)
		{
			logger.Warn($"GameObject '{go.name}', was attempted to be released more than once. GameObject Release canceled (AssetKey='{AssetKey}')");
			return;
		}
		_instancesRequested--;
		go.SetActive(value: false);
		go.transform.SetParent(_garbageZone, worldPositionStays: false);
		Object.Destroy(go);
	}

	public override bool RequestFromPool(out GameObject returnedGO, Vector3 p, Quaternion r, bool active)
	{
		returnedGO = CreatePoolObject(p, r);
		_instancesRequested++;
		OnGameObjectRequestedFromPool(returnedGO);
		returnedGO.SetActive(active);
		return true;
	}
}
