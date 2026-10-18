using System.Collections.Generic;

namespace Brix.Engine;

public abstract class VoxelHandle
{
	internal abstract bool _IsValid();

	internal abstract void _SetVoxelData(int t, int v, int d);

	internal abstract void _SetVoxelType(int t);

	internal abstract void _SetVoxelVolume(int t);

	internal abstract void _RemoveVoxel();

	internal abstract void _RemoveGrass();

	internal abstract void _GetTerrainResources(ICollection<int> resultTypes, ICollection<int> resultAmounts);

	internal abstract void _AddTerrainResources(VoxelTerrainResources r);

	internal abstract void _CompactCrust();

	internal abstract XYZ _GetVoxelPosition();

	internal abstract int _GetVoxelType();

	internal abstract int _GetVoxelVolume();

	internal abstract bool _IsVoxelEmpty();

	internal abstract bool _IsVoxelNonEmpty();

	internal abstract bool _IsVoxelVisible();

	internal abstract bool _IsVoxelInternal();

	internal abstract bool _IsVoxelSmooth();

	internal abstract int _EstimateVoxelLegacyLevels();

	internal abstract int _EstimateVoxelLevels();

	internal abstract int _EstimateVoxelConfig();

	internal abstract int _EstimateVoxelDeepType();

	internal abstract int _ComputeVoxelLegacyLevels();

	internal abstract int _ComputeVoxelLevels();

	internal abstract int _ComputeVoxelConfig();

	internal abstract int _ComputeVoxelDeepType();

	internal abstract void _AssignVoxelHandle(XYZ p);
}
