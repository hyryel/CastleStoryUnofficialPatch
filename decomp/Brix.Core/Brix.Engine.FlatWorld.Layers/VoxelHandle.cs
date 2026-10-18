using System.Collections.Generic;

namespace Brix.Engine.FlatWorld.Layers;

internal class VoxelHandle : Brix.Engine.VoxelHandle
{
	public LayerVoxelEngine VoxelEngine;

	public XYZ p;

	public FlatWorldHandle h;

	internal override bool _IsValid()
	{
		return VoxelEngine.IsValidVoxelHandle(this);
	}

	internal override XYZ _GetVoxelPosition()
	{
		return VoxelEngine.GetVoxelPosition(this);
	}

	internal override void _SetVoxelData(int t, int v, int d)
	{
		VoxelEngine.SetVoxelData(this, t, v, d);
	}

	internal override void _SetVoxelType(int t)
	{
		VoxelEngine.SetVoxelType(this, t);
	}

	internal override void _SetVoxelVolume(int v)
	{
		VoxelEngine.SetVoxelVolume(this, v);
	}

	internal override void _RemoveVoxel()
	{
		VoxelEngine.RemoveVoxel(this);
	}

	internal override void _RemoveGrass()
	{
		VoxelEngine.RemoveGrass(this);
	}

	internal override int _GetVoxelType()
	{
		return VoxelEngine.GetVoxelType(this);
	}

	internal override int _GetVoxelVolume()
	{
		return VoxelEngine.GetVoxelVolume(this);
	}

	internal override void _AddTerrainResources(VoxelTerrainResources r)
	{
		VoxelEngine.AddTerrainResources(this, r);
	}

	internal override void _GetTerrainResources(ICollection<int> resultTypes, ICollection<int> resultAmounts)
	{
		VoxelEngine.GetTerrainResources(this, resultTypes, resultAmounts);
	}

	internal override void _CompactCrust()
	{
		VoxelEngine.CompactCrust(this);
	}

	internal override bool _IsVoxelEmpty()
	{
		return VoxelEngine.IsVoxelEmpty(this);
	}

	internal override bool _IsVoxelNonEmpty()
	{
		return VoxelEngine.IsVoxelNonEmpty(this);
	}

	internal override bool _IsVoxelVisible()
	{
		return VoxelEngine.IsVoxelVisible(this);
	}

	internal override bool _IsVoxelInternal()
	{
		return VoxelEngine.IsVoxelInternal(this);
	}

	internal override bool _IsVoxelSmooth()
	{
		return VoxelEngine.IsVoxelSmooth(this);
	}

	internal override int _EstimateVoxelLegacyLevels()
	{
		return VoxelEngine.EstimateVoxelLegacyLevels(this);
	}

	internal override int _EstimateVoxelLevels()
	{
		return VoxelEngine.EstimateVoxelLevels(this);
	}

	internal override int _EstimateVoxelConfig()
	{
		return VoxelEngine.EstimateVoxelConfig(this);
	}

	internal override int _EstimateVoxelDeepType()
	{
		return VoxelEngine.EstimateVoxelDeepType(this);
	}

	internal override int _ComputeVoxelLegacyLevels()
	{
		return VoxelEngine.ComputeVoxelLegacyLevels(this);
	}

	internal override int _ComputeVoxelLevels()
	{
		return VoxelEngine.ComputeVoxelLevels(this);
	}

	internal override int _ComputeVoxelConfig()
	{
		return VoxelEngine.ComputeVoxelConfig(this);
	}

	internal override int _ComputeVoxelDeepType()
	{
		return VoxelEngine.ComputeVoxelDeepType(this);
	}

	internal override void _AssignVoxelHandle(XYZ p)
	{
		VoxelEngine.AssignVoxelHandle(this, p);
	}
}
