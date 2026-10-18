using Brix.Engine;
using Brix.Game.Components.Volumes;
using Brix.Network;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components;

public class SurfaceSpawnFitter : NetworkGameComponent
{
	public override void OnRequested()
	{
		base.OnRequested();
		if (NetworkServer.active)
		{
			if (Architecte.commence)
			{
				Fit();
			}
			else
			{
				GameSignals.Once(GameSignals.LevelLoaded, Fit);
			}
		}
	}

	public void Fit()
	{
		VoxelTransform component = GetComponent<VoxelTransform>();
		IVolume component2 = GetComponent<IVolume>();
		XYZ position = component2.GetCases(VProperty.mainSupportReq).Slinq().FirstOrDefault();
		if ((bool)component && Voxel.IsCrust(position) && (!(GetComponent<Blueprint>() != null) || !VolumeRegistry.HasVolumesWithPropertyAt(position, VProperty.fantome)) && Architecte.SurfaceRaycast(position.ToVector3()).y >= (float)position.y)
		{
			component.PlacementOffset = new Vector3(0f, -0.6f, 0f);
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
