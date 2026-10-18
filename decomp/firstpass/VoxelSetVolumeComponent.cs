using Brix.Engine;
using Brix.Engine.Volumes;
using Brix.Game.Components.Volumes;

public class VoxelSetVolumeComponent : VoxelVolumeComponent
{
	private VoxelSet voxels = new VoxelSet();

	public override IVoxelVolume Volume => voxels;

	public void Add(XYZ voxel, bool signalChange)
	{
		voxels.Add(voxel);
		if (signalChange)
		{
			VolumeChanged.Invoke();
		}
	}

	public void Remove(XYZ voxel, bool signalChange)
	{
		voxels.Remove(voxel);
		if (signalChange)
		{
			VolumeChanged.Invoke();
		}
	}

	public void Refresh()
	{
		VolumeChanged.Invoke();
	}

	internal void RemoveAll()
	{
		voxels.Clear();
	}
}
