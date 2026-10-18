using Brix.Engine;
using UnityEngine;

namespace Brix.Game.Components.Volumes;

public static class IVolumeExtender
{
	public static T GetComponent<T>(this IVolume v) where T : Component
	{
		if (v == null)
		{
			return (T)null;
		}
		GameObject gameObject = v.GameObject;
		if (gameObject.IsNullOrReleased())
		{
			return (T)null;
		}
		return gameObject.GetComponent<T>();
	}

	public static VoxelBounds ComputeBounds(this IVolume v)
	{
		XYZ position = v.Position;
		VoxelBounds voxelBounds = v.BaseVolume.ComputeBounds();
		return new VoxelBounds(voxelBounds.Min + position, voxelBounds.Max + position);
	}

	public static int GetPropertyBitMaskSharedByOtherProperty(this IVolume v, VProperty property, VProperty other)
	{
		if (property == other)
		{
			return -1;
		}
		int num = 0;
		VolumePropertyRequest cases = v.GetCases(property);
		foreach (XYZ @case in v.GetCases(other))
		{
			int num2 = cases.IndexOf(@case);
			if (num2 >= 0)
			{
				num |= 1 << num2;
			}
		}
		return num;
	}

	public static IGameComponent AsComponent(this IVolume v)
	{
		return v?.GameComponent;
	}
}
