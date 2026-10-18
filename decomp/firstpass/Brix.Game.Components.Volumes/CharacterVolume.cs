using Brix.Engine;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components.Volumes;

[RequireComponent(typeof(VoxelTransform))]
public class CharacterVolume : MovableVolume
{
	public void Disappear()
	{
		Unregister();
	}

	public void Reappear()
	{
		Register();
	}

	public override void OnRequested()
	{
		base.RegistryMode &= ~VolumeRegistryMode.OnStart;
		base.OnRequested();
	}

	protected override void Start()
	{
		GameSignals.LateConnectOnce(GameSignals.LevelReady, delegate(CharacterVolume self)
		{
			self.Register();
		}, this);
		base.Start();
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
