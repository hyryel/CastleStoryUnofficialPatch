using System.Collections.Generic;
using System.Linq;
using Brix.Audio;
using Brix.Components;
using Brix.Engine;
using Brix.Engine.Blocks;
using Brix.External.Factories;
using Brix.Game.AI.Damage;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Lifecycle.Pooling;
using Brix.Network;
using Brix.Utils.Logging;
using UnityEngine.Networking;

namespace Brix.Game.Factories;

public class VoxelBlockRequestComponent : NetworkGameComponent, IPoolableComponent, IInitializable, IRequestable, IResetable
{
	public string SoundSwitch;

	public float dmg;

	public BlockInfo _bi;

	protected static BrixLogger logger = new BrixLogger(typeof(VoxelBlockRequestComponent).FullName);

	protected override void Start()
	{
		base.Start();
		Factory.AssetKey assetKey = GetComponent<FactoryImprint>().AssetKey;
		_bi = Factory.Peek(assetKey).GetComponent<VoxelBlockRequestComponent>()._bi;
		BaseVoxelTransform component = GetComponent<BaseVoxelTransform>();
		BrixSingleton<BlockEngine>.Instance.RegisterBlock(component.position, component.RotationIndex, _bi, base.gameObject, dmg);
		StatsTracker.ObjectBuilt(faction, assetKey);
		IDamageReceiver component2 = GetComponent<IDamageReceiver>();
		if (component2 is PlacedBlockDamageReceiver)
		{
			(component2 as PlacedBlockDamageReceiver).SetBlockData(Voxel.GetBlockData(component.position).RootOrSelf);
		}
		else if (component2 == null)
		{
			logger.ErrorFormat("Missing IDamageReceiver on placed block {0}", base.name);
		}
		RelocateMoveableObstacles();
		Spawn();
		if (Architecte.commence)
		{
			SoundEngine.SetSwitch("Material", _bi.SurfaceString, base.gameObject);
			SoundEngine.Play("Bricktron_Places_Block", base.gameObject);
		}
	}

	private void RelocateMoveableObstacles()
	{
		IVolume component = GetComponent<IVolume>();
		IEnumerable<MovableVolume> enumerable = VolumeRegistry.VolumesWithPropertyAt(component.GetCases(VProperty.occupy), VProperty.obstacle).OfType<MovableVolume>();
		foreach (MovableVolume item in enumerable)
		{
			item.voxel.position = item.voxel.position + new XYZ(0, 1, 0);
		}
	}

	private void Spawn()
	{
		if (NetworkServer.active)
		{
			NetworkServer.Spawn(base.gameObject, GetComponent<FactoryImprint>().AssetKey.AssetId);
		}
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		IBlockData blockData = Voxel.GetBlockData(GetComponent<BaseVoxelTransform>().position);
		if (blockData.MatchesGameObject(base.gameObject))
		{
			blockData.DeBlockClient(-1);
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		bool flag2 = default(bool);
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
	}
}
