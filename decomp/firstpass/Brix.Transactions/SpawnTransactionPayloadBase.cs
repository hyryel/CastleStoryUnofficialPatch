using System;
using Brix.External.Factories;
using Brix.Game;
using Brix.Game.Components;
using Brix.Lifecycle.Pooling;
using UnityEngine;

namespace Brix.Transactions;

public class SpawnTransactionPayloadBase<TMonitor> : TransactionPayload<SpawnTransactionReturnValue, TMonitor> where TMonitor : SpawnTransactionMonitor, new()
{
	protected Factory.AssetKey _assetKey;

	private Faction _faction;

	private SpawnPostProcessor[] _postProcessors;

	private string _wantedId;

	public SpawnTransactionPayloadBase(Factory.AssetKey assetKey, Faction faction, params SpawnPostProcessor[] postProcessors)
	{
		_assetKey = assetKey;
		_postProcessors = postProcessors;
		_faction = faction;
		_wantedId = GenerateId();
	}

	public static string GenerateId()
	{
		return Guid.NewGuid().ToString();
	}

	public virtual GameObject Spawn()
	{
		ObjectPoolSingleton.Request(out var go, _assetKey, active: false);
		InstantiatePostProcess(go);
		return go;
	}

	protected void InstantiatePostProcess(GameObject instance)
	{
		AffiliationComponent component = instance.GetComponent<AffiliationComponent>();
		if (component != null)
		{
			component.faction = _faction;
		}
	}

	public override void Invoke(SpawnTransactionReturnValue returnValue)
	{
		try
		{
			GameObject gameObject = Spawn();
			gameObject.SetId(_wantedId);
			gameObject.SetActive(value: true);
			returnValue.SpawnedInstance = gameObject;
		}
		catch (Exception e)
		{
			throw new SpawnTransactionInstantiateException(_assetKey, e);
		}
		SpawnPostProcessor[] postProcessors = _postProcessors;
		foreach (SpawnPostProcessor spawnPostProcessor in postProcessors)
		{
			try
			{
				spawnPostProcessor.OnSpawnPostProcess(returnValue.SpawnedInstance);
			}
			catch (Exception e2)
			{
				throw new SpawnTransactionPostProcessException(spawnPostProcessor, e2);
			}
		}
	}
}
