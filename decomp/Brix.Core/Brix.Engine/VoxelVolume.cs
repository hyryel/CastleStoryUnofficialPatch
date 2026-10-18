using System.Collections.Generic;
using System.Linq;
using Brix.Engine.Geometrie;
using Brix.Engine.Volumes;

namespace Brix.Engine;

public static class VoxelVolume
{
	public static VoxelEmptyVolume Empty = new VoxelEmptyVolume();

	public static bool Contains(this IVoxelVolume v, int scalaire)
	{
		return v.Contains(XYZ.FromScalaire(scalaire));
	}

	public static IEnumerable<int> Scalaires(this IVoxelVolume v)
	{
		return v.Voxels.Select((XYZ p) => p.ToScalaire());
	}

	public static bool IsOnSurface(this IVoxelVolume v, XYZ p)
	{
		return !v.Contains(p + XYZ.right) || !v.Contains(p + XYZ.up) || !v.Contains(p + XYZ.forward) || !v.Contains(p - XYZ.right) || !v.Contains(p - XYZ.up) || !v.Contains(p - XYZ.forward);
	}

	public static IEnumerable<VoxelSurfaceElement> ComputeSurface(this IVoxelVolume v)
	{
		foreach (XYZ p in v.Voxels)
		{
			if (!v.Contains(p + XYZ.up))
			{
				yield return new VoxelSurfaceElement
				{
					faceIndex = 0,
					p = p
				};
			}
			if (!v.Contains(p + XYZ.forward))
			{
				yield return new VoxelSurfaceElement
				{
					faceIndex = 1,
					p = p
				};
			}
			if (!v.Contains(p - XYZ.forward))
			{
				yield return new VoxelSurfaceElement
				{
					faceIndex = 2,
					p = p
				};
			}
			if (!v.Contains(p + XYZ.right))
			{
				yield return new VoxelSurfaceElement
				{
					faceIndex = 3,
					p = p
				};
			}
			if (!v.Contains(p - XYZ.right))
			{
				yield return new VoxelSurfaceElement
				{
					faceIndex = 4,
					p = p
				};
			}
			if (!v.Contains(p - XYZ.up))
			{
				yield return new VoxelSurfaceElement
				{
					faceIndex = 5,
					p = p
				};
			}
		}
	}

	public static IEnumerable<VoxelSurfaceConfig> ComputeSurfaceConfigs(this IVoxelVolume v)
	{
		return from kv in v.Surface.ToLookup((VoxelSurfaceElement e) => e.p, (VoxelSurfaceElement e) => e.faceIndex)
			select new VoxelSurfaceConfig
			{
				p = kv.Key,
				config = Config.FromFaceIndices(kv)
			};
	}

	public static void ComputeSurfaceConfigs(this IVoxelVolume v, Dictionary<XYZ, VoxelSurfaceConfig> result)
	{
		foreach (VoxelSurfaceElement item in v.Surface)
		{
			VoxelSurfaceConfig valueOrDefault = result.GetValueOrDefault(item.p);
			valueOrDefault.p = item.p;
			valueOrDefault.config |= Config.FromFaceIndex(item.faceIndex);
			result[item.p] = valueOrDefault;
		}
	}
}
