using System.Collections.Generic;
using Brix.Engine;
using UnityEngine;

namespace Brix.Game.Components.Volumes.Generators;

public class VoxelBoundsComponent : VoxelVolumeComponent
{
	public bool overrideTransform;

	public XYZ min;

	public XYZ max;

	private bool exOverrideTransform;

	private XYZ exMin;

	private XYZ exMax;

	private IVoxelVolume volume;

	public override IVoxelVolume Volume
	{
		get
		{
			UpdateVolume();
			return volume;
		}
	}

	protected void UpdateVolume()
	{
		if (!overrideTransform)
		{
			CopyFromTransform();
		}
		else
		{
			CopyToTransform();
		}
		if (exMin != min || exMax != max || volume == null)
		{
			exMin = min;
			exMax = max;
			volume = CreateVolume();
			VolumeChanged.Invoke();
		}
	}

	[ContextMenu("Copy From Transform")]
	public void CopyFromTransform()
	{
		min = XYZ.FromVector3(base.transform.position - base.transform.localScale / 2f);
		max = XYZ.FromVector3(base.transform.position + base.transform.localScale / 2f);
	}

	[ContextMenu("Copy To Transform")]
	public void CopyToTransform()
	{
		base.transform.position = (max + min).ToVector3() / 2f;
		base.transform.localScale = (max - min).ToVector3();
	}

	public void UpdateFromConstraints(IEnumerable<XYZ> constraints, int heightPad)
	{
		min = XYZ.Min(constraints);
		max = XYZ.Max(constraints);
		min.y += -heightPad;
		max.y += heightPad;
		UpdateVolume();
	}

	public IVoxelVolume CreateVolume()
	{
		return new VoxelBounds
		{
			min = min,
			max = max
		};
	}

	private void OnDrawGizmosSelected()
	{
		UpdateVolume();
		Gizmos.color = Color.white;
		Gizmos.DrawWireCube((max + min).ToVector3() / 2f, (max - min).ToVector3());
	}
}
