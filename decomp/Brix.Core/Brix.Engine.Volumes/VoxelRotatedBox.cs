using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Brix.Engine.Volumes;

public class VoxelRotatedBox : IVoxelVolume, IEnumerable<XYZ>, IEnumerable
{
	private class LineIterator
	{
		public int n = 0;

		public float w = 0f;

		public IEnumerable<int> GetSteps()
		{
			float e = 0f;
			int i = 0;
			while (i < n)
			{
				int j = 0;
				while (i < n && e < 0.5f)
				{
					e += w;
					i++;
					j++;
				}
				e -= 1f;
				yield return j;
			}
		}
	}

	private struct XY
	{
		public int x;

		public int y;
	}

	private class PlaneIterator
	{
		public IEnumerable<XY> GetRects()
		{
			yield break;
		}
	}

	public XYZ center;

	public XYZ extents;

	public Quaternion rotation;

	public IEnumerable<XYZ> Voxels
	{
		get
		{
			XYZ xYZ = XYZ.FromVector3(extents.ToVector3() * 1.732051f);
			Quaternion invRot = Quaternion.Inverse(rotation);
			return from p in new VoxelBounds(-xYZ, xYZ)
				where CheckPoint(Rotate(p, invRot))
				select center + p;
		}
	}

	public IEnumerable<VoxelSurfaceElement> Surface => this.ComputeSurface();

	public VoxelBounds Bounds
	{
		get
		{
			XYZ xYZ = XYZ.FromVector3(extents.ToVector3() * 1.732051f);
			return new VoxelBounds(center - xYZ, center + xYZ);
		}
	}

	public VoxelBounds MinimalBounds => new VoxelBounds(Voxels);

	public Bounds VectorBounds => Bounds.ToVectorBounds();

	IEnumerator<XYZ> IEnumerable<XYZ>.GetEnumerator()
	{
		return Voxels.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return Voxels.GetEnumerator();
	}

	public bool Contains(XYZ p)
	{
		return CheckPoint(Rotate(p - center, Quaternion.Inverse(rotation)));
	}

	private bool CheckPoint(XYZ d)
	{
		return Sq(d.x) <= SqE(extents.x) && Sq(d.y) <= SqE(extents.y) && Sq(d.z) <= SqE(extents.z);
	}

	public static XYZ Rotate(XYZ p, Quaternion rotation)
	{
		return XYZ.FromVector3(rotation * p.ToVector3());
	}

	private static int Sq(int n)
	{
		return n * n;
	}

	private static int SqE(int n)
	{
		return n * (n - 1);
	}

	private static IEnumerable<XYZ> Volume(XYZ[] Step, int[] Weight, int max)
	{
		int e = 0;
		if (e <= max)
		{
		}
		yield break;
	}
}
