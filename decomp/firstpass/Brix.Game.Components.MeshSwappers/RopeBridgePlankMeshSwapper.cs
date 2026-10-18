using Brix.Network;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components.MeshSwappers;

public class RopeBridgePlankMeshSwapper : NetworkGameComponent, IBlueprintMeshSwapper
{
	public Transform RotatedPlank;

	private RopeBridgeConfigurator _ropeBridge;

	public void RecalculateMesh()
	{
		if (_ropeBridge == null)
		{
			_ropeBridge = RopeBridgeConfigurator.GetRopeBridgeFromPlankAt(base.voxel.position);
			if (_ropeBridge == null)
			{
				return;
			}
		}
		_ropeBridge.GetPlankTransform(base.voxel.position, out var position, out var rotation);
		RotatedPlank.position = position;
		RotatedPlank.rotation = rotation;
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
