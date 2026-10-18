using System.Collections.Generic;
using Brix.Engine;
using Brix.Game.AI;
using Brix.Game.Components.Volumes;
using Brix.Game.Network;
using Brix.Transactions;
using Brix.Utils.Signals;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components;

public class PlaceBlueprintsOnBrokenBlocks : GameComponent
{
	[SerializeField]
	private BuildGoalProvider provider;

	[SerializeField]
	private RepairProject project;

	private IBlueprintTracker _blueprintTracker;

	private static readonly HashSet<XYZ> LockedBlocks = new HashSet<XYZ>();

	protected override void Awake()
	{
		base.Awake();
		_blueprintTracker = provider;
		GameSignals.Connect(GameSignals.AfterBlockRemoved, OnBlockRemoved).DisconnectWhen(base.OnReleaseOrDestroySignal);
	}

	private void OnBlockRemoved(XYZ position, Quaternion rotation, BlockInfo blockInfo)
	{
		if (!this.IsNullOrReleased() && project.IsInRange(position) && !ThereIsAForeignBlueprintZAt(position))
		{
			AddBlueprintForBrokenBlock(position, rotation, blockInfo);
		}
	}

	private bool ThereIsAForeignBlueprintZAt(XYZ pos)
	{
		return VolumeRegistry.VolumesWithPropertyAt(pos, VProperty.fantome).Slinq().Select(IVolumeExtender.GetComponent<Blueprint>)
			.Any((Blueprint bp) => bp.Project != project);
	}

	private static void BlueprintPostProcess(GameObject g, XYZ p)
	{
		LockedBlocks.Remove(p);
	}

	private void AddBlueprintForBrokenBlock(XYZ position, Quaternion rotation, BlockInfo blockInfo)
	{
		if (NetworkServer.active)
		{
			GameObject item = Transactor.SpawnUNet(blockInfo.BlueprintKey, base.faction, blockInfo.OffsetedPosition(position, rotation), rotation);
			_blueprintTracker.AddObject(item);
		}
	}
}
