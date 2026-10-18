using Brix.Engine;
using Brix.Engine.Volumes;
using Brix.Game.Components.Volumes.Drawers;
using UnityEngine;

namespace Brix.Game.AI.UI;

public class LandscapeGoalsDrawer : MonoBehaviour
{
	private VoxelSet fillSet = new VoxelSet();

	private VoxelSet digSet = new VoxelSet();

	[SerializeField]
	private BoxDrawer.Spawner fillDrawerSpawner;

	[SerializeField]
	private BoxDrawer.Spawner digDrawerSpawner;

	private BoxDrawer fillDrawer => fillDrawerSpawner.Instance;

	private BoxDrawer digDrawer => digDrawerSpawner.Instance;

	public bool Showing
	{
		get
		{
			return fillDrawer.Showing && digDrawer.Showing;
		}
		set
		{
			fillDrawer.Showing = value;
			digDrawer.Showing = value;
		}
	}

	private void Awake()
	{
		fillDrawerSpawner.Spawn();
		digDrawerSpawner.Spawn();
	}

	public void Refresh()
	{
		fillDrawer.Redraw(fillSet);
		digDrawer.Redraw(digSet);
	}

	public void AddorSetVoxel(XYZ voxel, LandscapeGoal.Landscape type)
	{
		switch (type)
		{
		case LandscapeGoal.Landscape.Dig:
			fillSet.Remove(voxel);
			digSet.Add(voxel);
			break;
		case LandscapeGoal.Landscape.Fill:
			digSet.Remove(voxel);
			fillSet.Add(voxel);
			break;
		}
	}

	public void RemoveVoxel(XYZ voxel, bool autoUpdate = true)
	{
		fillSet.Remove(voxel);
		digSet.Remove(voxel);
	}

	public void Clear()
	{
		fillSet.Clear();
		digSet.Clear();
	}
}
