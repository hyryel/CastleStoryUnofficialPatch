using System.Collections;
using Brix.Engine;
using Brix.Pathfinding.Map;
using UnityEngine;

namespace Brix.Pathfinding;

public class Pathfinding : MonoBehaviour
{
	public Brix.Pathfinding.Map.Map map;

	public int lodCount = 4;

	public PathLod[] pathLods;

	public bool initTerrainLod = true;

	public bool initHigherLods = true;

	public int initStepsPerFrame = 10000;

	private bool initialized = false;

	public bool Initialized => initialized;

	private void Start()
	{
		CreateLods();
	}

	private void Update()
	{
		if (initTerrainLod)
		{
			initTerrainLod = false;
			InitTerrainLod();
		}
		if (initHigherLods)
		{
			initHigherLods = false;
			InitHigherLods();
		}
	}

	[ContextMenu("Clear all LOD data")]
	public void CreateLods()
	{
		pathLods = new PathLod[lodCount];
		if (lodCount > 0)
		{
			pathLods[0] = new PathTerrainLod();
			for (int i = 1; i < lodCount; i++)
			{
				pathLods[i] = new PathHigherLod
				{
					lod = i
				};
			}
			initialized = false;
		}
	}

	[ContextMenu("Init Terrain Nodes")]
	public void InitTerrainLod()
	{
		((PathTerrainLod)pathLods[0]).Init(map, GetPathLod(1));
	}

	[ContextMenu("Init Higher LOD Nodes")]
	public void InitHigherLods()
	{
		for (int i = 1; i < lodCount; i++)
		{
			((PathHigherLod)pathLods[i]).Init(GetPathLod(i - 1), GetPathLod(i + 1));
		}
		initialized = true;
	}

	public IEnumerator InitHigherLods_Coroutine()
	{
		for (int i = 1; i < lodCount; i++)
		{
			yield return ((PathHigherLod)pathLods[i]).Init_Coroutine(GetPathLod(i - 1), GetPathLod(i + 1), initStepsPerFrame);
		}
		initialized = true;
	}

	public PathLod GetPathLod(int lod)
	{
		return (lod >= 0 && lod < pathLods.Length) ? pathLods[lod] : PathLod.Empty;
	}

	public PathLowerLod GetPathLod0()
	{
		return pathLods[0] as PathLowerLod;
	}

	public PathNode GetWalkable(XYZ p, int lod)
	{
		PathNode pathNode = GetWalkableLod0(p);
		for (int i = 0; i < lod; i++)
		{
			PathNode parentNode = pathNode.ParentNode;
			if (parentNode.IsEmpty)
			{
				break;
			}
			pathNode = parentNode;
		}
		return pathNode;
	}

	private PathNode GetWalkableLod0(XYZ p)
	{
		PathLod pathLod = pathLods[0];
		XYZ lowResPosition = p;
		XYZ lowResPosition2 = p;
		while (lowResPosition2.y >= 0 || lowResPosition.y < 256)
		{
			PathNode pathNode = pathLod.GetPathNode(lowResPosition);
			if (!pathNode.IsEmpty)
			{
				return pathNode;
			}
			lowResPosition.y++;
			PathNode pathNode2 = pathLod.GetPathNode(lowResPosition2);
			if (!pathNode2.IsEmpty)
			{
				return pathNode2;
			}
			lowResPosition2.y--;
		}
		return PathNode.Empty;
	}
}
