using System;
using Brix.Engine;
using Brix.Engine.Volumes;

namespace Brix.Game.Components.Volumes.Generators;

public class VoxelVolumePredicateComponent : VoxelVolumeComponent
{
	public VoxelVolumeComponent volume;

	public VoxelPredicateComponent predicate;

	private readonly VoxelVolumeWithProperty _volume = new VoxelVolumeWithProperty();

	public override IVoxelVolume Volume
	{
		get
		{
			UpdateVolume();
			return _volume;
		}
	}

	private static bool AlwaysTrue(XYZ p)
	{
		return true;
	}

	protected override void Awake()
	{
		volume.VolumeChanged.Connect(VolumeChanged.Invoke);
	}

	private void UpdateVolume()
	{
		IVoxelVolume voxelVolume = ((!(volume != null)) ? VoxelVolume.Empty : volume.Volume);
		Func<XYZ, bool> func = ((!(predicate != null)) ? new Func<XYZ, bool>(AlwaysTrue) : new Func<XYZ, bool>(predicate.HasProperty));
		if (!object.ReferenceEquals(_volume.v, voxelVolume) || _volume.HasProperty != func)
		{
			_volume.v = voxelVolume;
			_volume.HasProperty = func;
			VolumeChanged.Invoke();
		}
	}
}
