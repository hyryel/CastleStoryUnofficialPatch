using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Brix.Engine.Volumes;
using Brix.Util;
using UnityEngine;

namespace Brix.Engine;

[Serializable]
public struct VoxelBounds : IVoxelVolume, IEnumerable<XYZ>, IEnumerable, IComparable<VoxelBounds>, IEquatable<VoxelBounds>
{
	public XYZ min;

	public XYZ max;

	public XYZ Min => min;

	public XYZ Max => max;

	public XYZ Center => max + min + XYZ.one >> 1;

	public XYZ Size => max - min + XYZ.one;

	public Vector3 VectorMin => min.ToVector3() - Vector3.one * 0.5f;

	public Vector3 VectorMax => max.ToVector3() + Vector3.one * 0.5f;

	public Vector3 VectorCenter => (max + min).ToVector3() * 0.5f;

	public IEnumerable<int> Scalaires => from XYZ p in this
		select p.ToScalaire();

	public VoxelBounds Bounds => this;

	public VoxelBounds MinimalBounds => this;

	public Bounds VectorBounds => ToVectorBounds();

	public IEnumerable<VoxelSurfaceElement> Surface
	{
		get
		{
			int x0 = min.x;
			int y0 = min.y;
			int z0 = min.z;
			int x1 = max.x;
			int y1 = max.y;
			int z1 = max.z;
			VoxelSurfaceElement e = new VoxelSurfaceElement
			{
				faceIndex = 0,
				p = 
				{
					y = max.y,
					x = x0
				}
			};
			while (e.p.x <= x1)
			{
				e.p.z = z0;
				while (e.p.z <= z1)
				{
					yield return e;
					e.p.z++;
				}
				e.p.x++;
			}
			e.faceIndex = 1;
			e.p.z = max.z;
			e.p.x = x0;
			while (e.p.x <= x1)
			{
				e.p.y = y0;
				while (e.p.y <= y1)
				{
					yield return e;
					e.p.y++;
				}
				e.p.x++;
			}
			e.faceIndex = 2;
			e.p.z = min.z;
			e.p.x = x0;
			while (e.p.x <= x1)
			{
				e.p.y = y0;
				while (e.p.y <= y1)
				{
					yield return e;
					e.p.y++;
				}
				e.p.x++;
			}
			e.faceIndex = 3;
			e.p.x = max.x;
			e.p.y = y0;
			while (e.p.y <= y1)
			{
				e.p.z = z0;
				while (e.p.z <= z1)
				{
					yield return e;
					e.p.z++;
				}
				e.p.y++;
			}
			e.faceIndex = 4;
			e.p.x = min.x;
			e.p.y = y0;
			while (e.p.y <= y1)
			{
				e.p.z = z0;
				while (e.p.z <= z1)
				{
					yield return e;
					e.p.z++;
				}
				e.p.y++;
			}
			e.faceIndex = 5;
			e.p.y = min.y;
			e.p.x = x0;
			while (e.p.x <= x1)
			{
				e.p.z = z0;
				while (e.p.z <= z1)
				{
					yield return e;
					e.p.z++;
				}
				e.p.x++;
			}
		}
	}

	public Calcule.VolumeEnumerable Voxels => Calcule.VolumeBetween(min, max);

	IEnumerable<XYZ> IVoxelVolume.Voxels => Voxels;

	public Calcule.VolumeEnumerator GetEnumerator()
	{
		return Voxels.GetEnumerator();
	}

	IEnumerator<XYZ> IEnumerable<XYZ>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public VoxelBounds(XYZ a, XYZ b)
	{
		min = XYZ.Min(a, b);
		max = XYZ.Max(a, b);
	}

	public VoxelBounds(int scalaireA, int scalaireB)
		: this(XYZ.FromScalaire(scalaireA), XYZ.FromScalaire(scalaireB))
	{
	}

	public VoxelBounds(IEnumerable<XYZ> points)
	{
		this = default(VoxelBounds);
		IEnumerator<XYZ> enumerator = points.GetEnumerator();
		if (enumerator.MoveNext())
		{
			min = (max = enumerator.Current);
			while (enumerator.MoveNext())
			{
				Encapsulate(enumerator.Current);
			}
		}
	}

	public VoxelBounds(IEnumerable<int> points)
		: this(points.Select(XYZ.FromScalaire))
	{
	}

	public static VoxelBounds FromVectorBounds(Bounds bounds)
	{
		XYZ a = XYZ.FromVector3(bounds.min + Vector3.one * 0.5f);
		XYZ b = XYZ.FromVector3(bounds.max - Vector3.one * 0.5f);
		return new VoxelBounds(a, b);
	}

	public static VoxelBounds FromVectorBoundsAdjustedToMin(Bounds bounds)
	{
		XYZ xYZ = XYZ.FromVector3(bounds.min + Vector3.one * 0.5f);
		XYZ xYZ2 = XYZ.FromVector3(bounds.size);
		return new VoxelBounds(xYZ, xYZ + xYZ2 - XYZ.one);
	}

	public static VoxelBounds FromVectorBoundsAdjustedToMax(Bounds bounds)
	{
		XYZ xYZ = XYZ.FromVector3(bounds.max - Vector3.one * 0.5f);
		XYZ xYZ2 = XYZ.FromVector3(bounds.size);
		return new VoxelBounds(xYZ - xYZ2 + XYZ.one, xYZ);
	}

	public Bounds ToVectorBounds()
	{
		return new Bounds(VectorCenter, Size.ToVector3());
	}

	public bool Contains(XYZ p)
	{
		return p >= min && p <= max;
	}

	public void Encapsulate(XYZ p)
	{
		min = XYZ.Min(min, p);
		max = XYZ.Max(max, p);
	}

	public void Encapsulate(VoxelBounds other)
	{
		if (min == XYZ.zero && max == XYZ.zero)
		{
			min = other.min;
			max = other.max;
		}
		else
		{
			min = XYZ.Min(min, other.min);
			max = XYZ.Max(max, other.max);
		}
	}

	public void Resize(XYZ newSize)
	{
		SetSize(ref min.x, ref max.x, newSize.x);
		SetSize(ref min.y, ref max.y, newSize.y);
		SetSize(ref min.z, ref max.z, newSize.z);
	}

	public void IncreaseSize(XYZ minSize)
	{
		XYZ size = Size;
		if (size.x < minSize.x)
		{
			SetSize(ref min.x, ref max.x, minSize.x);
		}
		if (size.y < minSize.y)
		{
			SetSize(ref min.y, ref max.y, minSize.y);
		}
		if (size.z < minSize.z)
		{
			SetSize(ref min.z, ref max.z, minSize.z);
		}
	}

	public void DecreaseSize(XYZ maxSize)
	{
		XYZ size = Size;
		if (size.x > maxSize.x)
		{
			SetSize(ref min.x, ref max.x, maxSize.x);
		}
		if (size.y > maxSize.y)
		{
			SetSize(ref min.y, ref max.y, maxSize.y);
		}
		if (size.z > maxSize.z)
		{
			SetSize(ref min.z, ref max.z, maxSize.z);
		}
	}

	public void ClampSize(XYZ minSize, XYZ maxSize)
	{
		IncreaseSize(minSize);
		DecreaseSize(maxSize);
	}

	public static VoxelBounds Div(VoxelBounds bounds, XYZ factor)
	{
		return new VoxelBounds
		{
			min = XYZ.Div(bounds.min, factor),
			max = XYZ.DivCeil(bounds.max, factor)
		};
	}

	private static void SetSize(ref int min, ref int max, int size)
	{
		min = (min + max - size) / 2;
		max = min + size;
	}

	public int CompareTo(VoxelBounds other)
	{
		int num = min.CompareTo(other.min);
		return (num != 0) ? num : max.CompareTo(other.max);
	}

	public bool Equals(VoxelBounds other)
	{
		return min.Equals(other.min) && max.Equals(other.max);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		return obj is VoxelBounds && Equals((VoxelBounds)obj);
	}

	public override int GetHashCode()
	{
		int hashCode = min.GetHashCode();
		return (hashCode * 397) ^ max.GetHashCode();
	}

	public override string ToString()
	{
		return string.Concat("[", min, ", ", max, "]");
	}

	public static bool operator ==(VoxelBounds a, VoxelBounds b)
	{
		return a.min == b.min && a.max == b.max;
	}

	public static bool operator !=(VoxelBounds a, VoxelBounds b)
	{
		return a.min != b.min || a.max != b.max;
	}
}
