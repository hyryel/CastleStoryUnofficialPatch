using System.Collections.Generic;
using Brix.Engine;
using Brix.Engine.Trackers;
using Brix.Game.Components.Volumes;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Legacy;

[RequireComponent(typeof(IVolume))]
public class VoxelInvincibles : MonoBehaviour
{
	private List<XYZ> configuredGeostaticVoxels = new List<XYZ>();

	private List<XYZ> configuredInvincibleVoxels = new List<XYZ>();

	private void Start()
	{
		IVolume component = GetComponent<IVolume>();
		configuredGeostaticVoxels = (from p in component.GetCases(VProperty.geostatic).Slinq()
			select (p) into p
			where !GeostaticityTracker.IsGeostatic(p) && Voxel.IsTerrain(p)
			select p).ToList();
		configuredInvincibleVoxels = (from p in component.GetCases(VProperty.invincible).Slinq()
			select (p) into p
			where !InvincibilityTracker.IsInvincible(p) && Voxel.IsTerrain(p)
			select p).ToList();
		MarkGeostaticVoxels();
		MarkInvicibleVoxels();
	}

	public void UnmarkGeostaticVoxels()
	{
		configuredGeostaticVoxels.Slinq().ForEach(GeostaticityTracker.ClearGeostatique);
	}

	public void MarkGeostaticVoxels()
	{
		UnmarkGeostaticVoxels();
		(from p in configuredGeostaticVoxels.Slinq()
			where !GeostaticityTracker.IsGeostatic(p)
			select p).ForEach(delegate(XYZ xyz, VoxelInvincibles self)
		{
			self.MarkGeostaticVoxel(xyz);
		}, this);
	}

	public void MarkGeostaticVoxel(XYZ p)
	{
		GeostaticityTracker.SetGeostatique(p);
		configuredGeostaticVoxels.Add(p);
	}

	public void UnmarkInvincibleVoxels()
	{
		configuredGeostaticVoxels.Slinq().ForEach(InvincibilityTracker.ClearInvincible);
	}

	public void MarkInvicibleVoxels()
	{
		UnmarkGeostaticVoxels();
		(from p in configuredInvincibleVoxels.Slinq()
			where !InvincibilityTracker.IsInvincible(p)
			select p).ForEach(delegate(XYZ xyz, VoxelInvincibles self)
		{
			self.MarkInvicibleVoxel(xyz);
		}, this);
	}

	public void MarkInvicibleVoxel(XYZ p)
	{
		InvincibilityTracker.SetInvincible(p);
		configuredInvincibleVoxels.Slinq().ForEach(InvincibilityTracker.SetInvincible);
	}

	private void OnDestroy()
	{
		UnmarkGeostaticVoxels();
		UnmarkInvincibleVoxels();
	}
}
