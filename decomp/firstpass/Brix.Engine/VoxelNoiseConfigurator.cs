using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Brix.External.Factories;
using Brix.Noise;
using Newtonsoft.Json;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Engine;

public class VoxelNoiseConfigurator : NetworkBehaviour
{
	[Serializable]
	public class NoiseConfig
	{
		public bool Enabled = true;

		[VoxelTerrainType]
		public int Type;

		public string NoiseAssetKey;

		public float Threshold;

		public bool UseRange;

		public int Start;

		public int End;

		[HideInInspector]
		[JsonIgnore]
		public NoiseBaseComponent NoiseComponent;
	}

	private bool initialized;

	[SyncVar(hook = "globalSeedChanged")]
	private int globalSeed;

	public List<NoiseConfig> NoiseConfigList = new List<NoiseConfig>();

	private static VoxelNoiseConfigurator instance;

	private Dictionary<VoxelTerrainType, NoiseConfig> _voxelTypeNoiseMap = new Dictionary<VoxelTerrainType, NoiseConfig>();

	public static VoxelNoiseConfigurator Instance => instance;

	public int NetworkglobalSeed
	{
		get
		{
			return globalSeed;
		}
		[param: In]
		set
		{
			ref int fieldValue = ref globalSeed;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				globalSeedChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 1u);
		}
	}

	public static bool IsInitialized()
	{
		return (bool)instance && instance.initialized;
	}

	public static int GetGlobalSeed()
	{
		return instance.globalSeed;
	}

	public static void SetGlobalSeed(int seed)
	{
		instance.NetworkglobalSeed = seed;
	}

	public void Awake()
	{
		instance = this;
	}

	public override void OnStartClient()
	{
		Init();
	}

	private void globalSeedChanged(int value)
	{
		NetworkglobalSeed = value;
		if (initialized)
		{
			instance.NoiseConfigList.Slinq().ForEach(delegate(NoiseConfig nc)
			{
				nc.NoiseComponent.Refresh();
			});
		}
	}

	public static void Init()
	{
		if (!instance.initialized)
		{
			if (instance.globalSeed == 0)
			{
				instance.NetworkglobalSeed = UnityEngine.Random.Range(1, 1000);
			}
			instance.NoiseConfigList.Slinq().ForEach(instance.InstantiateNoiseComponent);
			instance.initialized = true;
		}
	}

	public static Slinq<KeyValuePair<VoxelTerrainType, NoiseConfig>, PredicateContext<KeyValuePair<VoxelTerrainType, NoiseConfig>, DictionaryContext<VoxelTerrainType, NoiseConfig>>> GetTerrainTypesWithNoise()
	{
		return from vt in instance._voxelTypeNoiseMap.Slinq()
			where vt.Value.Enabled
			select vt;
	}

	public static NoiseConfig GetNoiseConfigFromVoxelTerrainType(VoxelTerrainType type)
	{
		instance._voxelTypeNoiseMap.TryGetValue(type, out var value);
		return value;
	}

	private void InstantiateNoiseComponent(NoiseConfig noiseConfig)
	{
		Factory.AssetKey assetKey = new Factory.AssetKey(noiseConfig.NoiseAssetKey);
		if (assetKey != null)
		{
			noiseConfig.NoiseComponent = Factory.Instantiate(assetKey).GetComponent<NoiseBaseComponent>();
			_voxelTypeNoiseMap[(VoxelTerrainType)noiseConfig.Type] = noiseConfig;
		}
	}

	public static List<NoiseConfig> GetAllNoiseTypes()
	{
		return instance.NoiseConfigList;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.WritePackedUInt32((uint)globalSeed);
			return true;
		}
		bool flag = false;
		if ((base.syncVarDirtyBits & 1) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)globalSeed);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			globalSeed = (int)reader.ReadPackedUInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			globalSeedChanged((int)reader.ReadPackedUInt32());
		}
	}
}
