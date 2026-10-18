using System.Collections.Generic;
using Brix.Components;
using Brix.Game.AI.Damage;
using Brix.Utils.Logging;
using Brix.Utils.Signals;

namespace Brix.Engine.Blocks;

public class DamagedBlockTracker : BrixComponent
{
	private static BrixLogger logger = new BrixLogger(typeof(DamagedBlockTracker).FullName);

	public HashSet<BlockData> DamagedBlocks = new HashSet<BlockData>();

	protected override void Start()
	{
		base.Start();
		PlacedBlockDamageReceiver.OnBlockDamaged.Connect(OnBlockDamaged).DisconnectWhen(base.OnReleaseOrDestroySignal);
		PlacedBlockDamageReceiver.OnBlockHealed.Connect(BlockCompletelyHealed).DisconnectWhen(base.OnReleaseOrDestroySignal);
		GameSignals.Connect(GameSignals.BeforeBlockRemoved, OnBlockDead).DisconnectWhen(base.OnReleaseOrDestroySignal);
		RootBlockData.RootBlockCreated.Connect(delegate(RootBlockData rbd)
		{
			AddBlockIfDamaged(rbd);
		}).DisconnectWhen(base.OnReleaseOrDestroySignal);
	}

	private void AddBlockIfDamaged(RootBlockData rbd)
	{
		if (rbd.Damage > 0f)
		{
			DamagedBlocks.Add(rbd);
		}
	}

	private void OnBlockDead(BlockData bd)
	{
		DamagedBlocks.Remove(bd);
	}

	private void BlockCompletelyHealed(BlockData bd, bool withParticles)
	{
		if (!DamagedBlocks.Remove(bd))
		{
			logger.WarnFormat("A previously damaged BlockData was not tracked by the DamagedBlockTracker. The position of the block is {0}.", bd.Position);
		}
	}

	private void OnBlockDamaged(BlockData bd, bool withParticles)
	{
		DamagedBlocks.Add(bd.RootOrSelf);
	}
}
