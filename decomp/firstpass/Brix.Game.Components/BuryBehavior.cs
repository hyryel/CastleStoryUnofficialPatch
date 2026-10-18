using Brix.Engine;
using Brix.Utils.Logging;
using UnityEngine;

namespace Brix.Game.Components;

public class BuryBehavior : MonoBehaviour
{
	private static BrixLogger logger = new BrixLogger(typeof(BuryBehavior).FullName);

	private float startTime;

	private XYZ Top => XYZ.FromVector3(base.transform.position) + XYZ.up;

	private bool IsBuried => Voxel.IsNonEmpty(Top);

	private void Start()
	{
		startTime = Time.time;
	}

	private void Update()
	{
		if (IsBuried)
		{
			PopOutOfTerrain();
			Object.Destroy(this);
		}
		else if (Time.time >= startTime + 2f)
		{
			Object.Destroy(this);
		}
	}

	private void PopOutOfTerrain()
	{
		Vector3 position = base.transform.position;
		Architecte.VoxelDessus(XZ.FromXYZ(Top), out var result);
		base.transform.position = Architecte.SurfaceRaycast(result.ToVector3()) + Vector3.up;
		logger.Info(string.Concat("Popped ", base.name, " out in extremis, from ", position, " to ", base.transform.position));
	}
}
