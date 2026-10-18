using Brix.Engine;
using Brix.Engine.Volumes;

namespace Brix.Game.Components.Volumes.Generators;

public class VoxelVolumeBooleanComponent : VoxelVolumeComponent
{
	public enum Mode
	{
		Union,
		Intersection,
		Subtraction
	}

	public Mode mode;

	public VoxelVolumeComponent volumeA;

	public VoxelVolumeComponent volumeB;

	private IVoxelVolume v1;

	private IVoxelVolume v2;

	private IVoxelVolume exV1;

	private IVoxelVolume exV2;

	private Mode exMode;

	private IVoxelVolume volume;

	public override IVoxelVolume Volume
	{
		get
		{
			UpdateVolume();
			return volume;
		}
	}

	private void UpdateVolume()
	{
		v1 = volumeA.Volume;
		v2 = volumeB.Volume;
		if (exV1 != v1 || exV2 != v2 || exMode != mode || volume == null)
		{
			exV1 = v1;
			exV2 = v2;
			exMode = mode;
			volume = CreateVolume();
			VolumeChanged.Invoke();
		}
	}

	public IVoxelVolume CreateVolume()
	{
		switch (mode)
		{
		case Mode.Union:
		{
			VoxelVolumeUnion voxelVolumeUnion = new VoxelVolumeUnion();
			voxelVolumeUnion.v1 = v1;
			voxelVolumeUnion.v2 = v2;
			return voxelVolumeUnion;
		}
		case Mode.Intersection:
		{
			VoxelVolumeIntersection voxelVolumeIntersection = new VoxelVolumeIntersection();
			voxelVolumeIntersection.v1 = v1;
			voxelVolumeIntersection.v2 = v2;
			return voxelVolumeIntersection;
		}
		case Mode.Subtraction:
		{
			VoxelVolumeSubtraction voxelVolumeSubtraction = new VoxelVolumeSubtraction();
			voxelVolumeSubtraction.v1 = v1;
			voxelVolumeSubtraction.v2 = v2;
			return voxelVolumeSubtraction;
		}
		default:
			return null;
		}
	}
}
