using Brix.Engine;
using Brix.Engine.Volumes;
using UnityEngine;

namespace Brix.Game.Components.Volumes.Generators;

public class VoxelVolumePrimitiveComponent : VoxelVolumeComponent
{
	public enum Mode
	{
		Box,
		Sphere,
		Cylinder
	}

	public Mode mode;

	public bool overrideTransform;

	public XYZ center;

	public XYZ extents;

	public XYZ resCenter;

	public XYZ resExtents;

	private Mode exMode;

	private bool exOverrideTransform;

	private XYZ exCenter;

	private XYZ exExtents;

	private IVoxelVolume volume;

	public XYZ Center => resCenter;

	public XYZ Extents => resExtents;

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
		if (overrideTransform)
		{
			resCenter = center;
			resExtents = extents;
		}
		else
		{
			resCenter = XYZ.FromVector3(base.transform.position);
			resExtents = XYZ.FromVector3(base.transform.localScale);
		}
		if (exMode != mode || exCenter != resCenter || exExtents != resExtents || volume == null)
		{
			exMode = mode;
			exCenter = resCenter;
			exExtents = resExtents;
			volume = CreateVolume();
			VolumeChanged.Invoke();
		}
	}

	[ContextMenu("Copy From Transform")]
	public void CopyFromTransform()
	{
		UpdateVolume();
		center = resCenter;
		extents = resExtents;
	}

	[ContextMenu("Copy To Transform")]
	public void CopyToTransform()
	{
		UpdateVolume();
		base.transform.position = resCenter.ToVector3();
		base.transform.localScale = resExtents.ToVector3();
	}

	public IVoxelVolume CreateVolume()
	{
		switch (mode)
		{
		case Mode.Box:
		{
			VoxelBox voxelBox = new VoxelBox();
			voxelBox.center = Center;
			voxelBox.extents = Extents;
			return voxelBox;
		}
		case Mode.Sphere:
		{
			VoxelSphere voxelSphere = new VoxelSphere();
			voxelSphere.center = Center;
			voxelSphere.extents = Extents;
			return voxelSphere;
		}
		case Mode.Cylinder:
		{
			VoxelCylinder voxelCylinder = new VoxelCylinder();
			voxelCylinder.center = Center;
			voxelCylinder.extents = Extents;
			voxelCylinder.orientation = VoxelCylinder.Orientation.Up;
			return voxelCylinder;
		}
		default:
			return null;
		}
	}

	private void OnDrawGizmosSelected()
	{
		UpdateVolume();
		Gizmos.color = Color.white;
		Gizmos.DrawWireCube(Center.ToVector3(), Extents.ToVector3() * 2f + Vector3.one);
	}
}
