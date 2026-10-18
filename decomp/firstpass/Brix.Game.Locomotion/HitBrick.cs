using Brix.Engine;
using Brix.External.Signals;
using Brix.Game.AI.Behaviors;
using Brix.Game.Components.Volumes;
using UnityEngine;

namespace Brix.Game.Locomotion;

public class HitBrick : LocomotionMovementState
{
	private XYZ brick;

	private XYZ current;

	private SignalConnection onEventConnection = SignalConnection.Empty;

	public void DoHitBrick(XYZ destination)
	{
		brick = destination;
		current = brick;
		base.controller.HittingBrick.Set(b: true);
		float f = Mathf.Round((float)destination.y - (base.trf.position.y + 1f));
		base.controller.WorkHeight.Set(f);
	}

	public void StopHittingBrick()
	{
		brick = XYZ.zero;
		base.controller.HittingBrick.Set(b: false);
	}

	public override void Enter()
	{
		onEventConnection.Disconnect();
		onEventConnection = Locomotion.Labor.animationEventBoard.OnEvent.Connect(delegate(Brix.Game.AI.Behaviors.AnimationEvent e)
		{
			OnEvent(e);
		});
	}

	public override void Exit()
	{
		onEventConnection.Disconnect();
		onEventConnection = SignalConnection.Empty;
	}

	public override void Update()
	{
		if (!base.PathIterator.Active)
		{
			StopHittingBrick();
		}
		else
		{
			if (IsBreakable(current))
			{
				return;
			}
			for (int i = brick.y; i <= brick.y + 1 && i <= base.voxel.position.y + 2; i++)
			{
				current.y = i;
				if (IsBreakable(current))
				{
					return;
				}
			}
			StopHittingBrick();
		}
	}

	public static bool IsBreakable(XYZ v)
	{
		return Voxel.IsBlock(v) || VolumeRegistry.VolumeWithPropertyAt(v, VProperty.obstacle) != null;
	}

	private void OnEvent(Brix.Game.AI.Behaviors.AnimationEvent e)
	{
		if (e == Locomotion.Interaction.ToolHitEvent)
		{
			int blockBreaking = Locomotion.Labor.Profession.BlockBreaking;
			Voxel.GetBlockData(current).DamageReceiver.ApplyDamage(blockBreaking, base.gameObject);
			VolumeRegistry.VolumeWithPropertyAt(current, VProperty.obstacle)?.DamageReceiver.ApplyDamage(blockBreaking, base.gameObject);
		}
	}
}
