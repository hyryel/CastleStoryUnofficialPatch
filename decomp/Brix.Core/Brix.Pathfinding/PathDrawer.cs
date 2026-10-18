using System.Collections.Generic;
using System.Linq;
using Brix.Engine;
using UnityEngine;

namespace Brix.Pathfinding;

public class PathDrawer : MonoBehaviour
{
	public enum Algorithm
	{
		Breadth,
		AStar,
		DoubleAStar,
		BreadthLod,
		AStarLod,
		DoubleAStarLod
	}

	public delegate IEnumerable<PathLink> AlgorithmDelegate(PathRequestParams p);

	public Pathfinding pathfinding;

	public GameObject cubePrefab;

	public GameObject from;

	public GameObject to;

	public Algorithm algorithm;

	public int lodMin;

	public int lodMax = -1;

	public bool unrollLod;

	public int unrollLimit;

	public int searchRate = 100;

	public bool autoUpdate;

	public bool autoRedraw;

	public bool autoSearch;

	public bool verbose;

	public bool showSearchedArea;

	public bool flipDirection;

	public bool invalidInput;

	public int pathLength = 0;

	public int searchedArea = 0;

	public float searchedTime = 0f;

	public bool forceDirty;

	public bool testMemory;

	public bool allowRequeueing = true;

	public PathSelectableBranchEvaluator branchEvaluator;

	public PathBranchEvaluator alternateBranchEvaluator;

	public PathLodUnroller lodUnroller;

	public List<PathLink> path = new List<PathLink>();

	public List<PathLink> searchedLinks = new List<PathLink>();

	private PathNode source;

	private PathNode goal;

	private IEnumerator<PathLink> algo;

	private List<PathLink> tmpPath = new List<PathLink>();

	private PathNode exSource;

	private PathNode exGoal;

	private int exLodMin;

	private int exLodMax;

	private Algorithm exAlgorithm;

	private PathSelectableBranchEvaluator.ObstacleAccessibility exAccessibility;

	private PathBranch.BranchType exBranchType;

	private bool exUnrollLod;

	private int exUnrollLimit;

	private Dictionary<PathNode, PathLink> _visited = new Dictionary<PathNode, PathLink>();

	private PathBuilder _builder;

	private PathAccepter _accepter;

	private PathSearch _search;

	private PathLink _sourceLink;

	private void Update()
	{
		if (autoUpdate)
		{
			RecalculateIfNeeded();
		}
		if (autoSearch)
		{
			Search();
		}
		if (testMemory)
		{
			TestMemory();
		}
	}

	private void RecalculateIfNeeded()
	{
		invalidInput = true;
		if (!pathfinding.Initialized)
		{
			return;
		}
		source = GetWalkable(from.transform.position);
		bool flag = !source.IsEmpty;
		ShowWalkable(from, flag);
		goal = GetWalkable(to.transform.position);
		bool flag2 = !goal.IsEmpty;
		ShowWalkable(to, flag2);
		if (flag && flag2)
		{
			invalidInput = false;
			if (flipDirection)
			{
				PathNode pathNode = source;
				source = goal;
				goal = pathNode;
			}
			if (forceDirty || exSource != source || exGoal != goal || exLodMin != lodMin || exLodMax != lodMax || exAlgorithm != algorithm || exAccessibility != branchEvaluator.AcceptObstacle || exBranchType != branchEvaluator.AcceptedType || exUnrollLod != unrollLod || exUnrollLimit != unrollLimit)
			{
				forceDirty = false;
				exSource = source;
				exGoal = goal;
				exLodMin = lodMin;
				exLodMax = lodMax;
				exAlgorithm = algorithm;
				exAccessibility = branchEvaluator.AcceptObstacle;
				exBranchType = branchEvaluator.AcceptedType;
				exUnrollLod = unrollLod;
				exUnrollLimit = unrollLimit;
				Recalculate();
			}
		}
	}

	private PathNode GetWalkable(Vector3 position)
	{
		return pathfinding.GetWalkable(XYZ.FromVector3(position), lodMin);
	}

	public void ShowWalkable(GameObject marker, bool walkable)
	{
		marker.GetComponent<MeshRenderer>().material.color = (walkable ? new Color(0.5f, 0f, 1f) : Color.white);
	}

	private AlgorithmDelegate SelectAlgorithm(Algorithm algorithm)
	{
		return algorithm switch
		{
			Algorithm.Breadth => PathAlgorithms.Breadth, 
			Algorithm.AStar => PathAlgorithms.AStar, 
			Algorithm.DoubleAStar => PathAlgorithms.DoubleAStar, 
			Algorithm.BreadthLod => PathAlgorithms.BreadthLod, 
			Algorithm.AStarLod => PathAlgorithms.AStarLod, 
			Algorithm.DoubleAStarLod => PathAlgorithms.DoubleAStarLod, 
			_ => null, 
		};
	}

	[ContextMenu("Recalculate")]
	public void Recalculate()
	{
		int lodCount = ((lodMax < lodMin) ? (-1) : (lodMax - lodMin + 1));
		PathLod pathLod = pathfinding.GetPathLod(lodMin);
		PathRequestParams p = new PathRequestParams
		{
			Origin = source,
			Destination = goal,
			OutPath = tmpPath,
			PathLod = pathLod,
			LodCount = lodCount,
			BranchEvaluator = branchEvaluator,
			AllowRequeueing = allowRequeueing
		};
		algo = SelectAlgorithm(algorithm)(p).GetEnumerator();
		searchedLinks.Clear();
		pathLength = 0;
		searchedArea = 0;
		searchedTime = 0f;
	}

	[ContextMenu("Search")]
	public void Search()
	{
		if (algo == null)
		{
			return;
		}
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		bool flag = false;
		for (int i = 0; i < searchRate; i++)
		{
			if (!algo.MoveNext())
			{
				flag = true;
				break;
			}
			searchedLinks.Add(algo.Current);
			searchedArea++;
		}
		searchedTime += Time.realtimeSinceStartup - realtimeSinceStartup;
		if (flag)
		{
			ConcludeSearch();
		}
	}

	private void ConcludeSearch()
	{
		algo = null;
		int count = tmpPath.Count;
		if (count > 0)
		{
			if (unrollLod || unrollLimit > 0)
			{
				float realtimeSinceStartup = Time.realtimeSinceStartup;
				PathLink successLink = tmpPath[tmpPath.Count - 1];
				lodUnroller = new PathLodUnroller(successLink, pathfinding.GetPathLod(lodMin), branchEvaluator, new PathLightFirstQueue());
				if (unrollLod)
				{
					path = lodUnroller.Nodes.Select((PathNode node) => new PathLink(node)).ToList();
				}
				else
				{
					IEnumerator<PathNode> enumerator = lodUnroller.Nodes.GetEnumerator();
					for (int num = 0; num < unrollLimit; num++)
					{
						if (!enumerator.MoveNext())
						{
							break;
						}
					}
					path = lodUnroller.ReverseSuccessLink.Nodes.Select((PathNode node) => new PathLink(node)).ToList();
				}
				searchedTime += Time.realtimeSinceStartup - realtimeSinceStartup;
			}
			else
			{
				path = tmpPath;
			}
		}
		pathLength = count;
		if (verbose)
		{
			UnityEngine.Debug.Log(string.Concat("ComputePath([", source, "], [", goal, "]) searched area=", searchedArea, " with result length=", pathLength));
		}
		if (autoRedraw)
		{
			Redraw();
		}
	}

	[ContextMenu("Redraw")]
	public void Redraw()
	{
		Transform parent = CreateChild("Path");
		int num = 0;
		foreach (PathLink item in path)
		{
			float num2 = ((path.Count > 1) ? ((float)num++ / (float)(path.Count - 1)) : 1f);
			CreateCube(item.head.Node.LowResPosition, item.head.Node.Lod, parent, new Color(0f, num2, 1f - num2, 0.5f));
		}
	}

	[ContextMenu("Debug")]
	public void Debug()
	{
		Transform parent = CreateChild("Debug");
		for (int i = lodMin; i <= lodMax; i++)
		{
			PathLod pathLod = pathfinding.GetPathLod(i);
			float t = ((lodMax > lodMin) ? ((float)(i - lodMin) / (float)(lodMax - lodMin)) : 1f);
			Color color = Color.Lerp(new Color(1f, 0f, 0f, 0.5f), new Color(1f, 1f, 0f, 0.5f), t);
			foreach (PathNode pathNode in pathLod.PathNodes)
			{
				if ((i <= lodMin || pathNode.IsFull) && (i >= lodMax || !pathNode.ParentNode.IsFull))
				{
					CreateCube(pathNode.LowResPosition, i, parent, color);
				}
			}
		}
	}

	[ContextMenu("Draw Searched Area")]
	public void DrawSearched()
	{
		Transform parent = CreateChild("Searched");
		int num = 0;
		foreach (PathLink searchedLink in searchedLinks)
		{
			float num2 = ((searchedLinks.Count > 1) ? ((float)num++ / (float)(searchedLinks.Count - 1)) : 1f);
			CreateCube(searchedLink.head.Node.LowResPosition, searchedLink.head.Node.Lod, parent, new Color(1f, num2, 1f - num2, 0.5f));
		}
	}

	private Transform CreateChild(string name)
	{
		Transform transform = base.transform.Find(name);
		if (transform != null)
		{
			Object.Destroy(transform.gameObject);
		}
		transform = new GameObject(name).transform;
		transform.SetParent(base.transform);
		return transform;
	}

	private GameObject CreateCube(XYZ p, int lod, Transform parent, Color color)
	{
		GameObject gameObject = Object.Instantiate(cubePrefab);
		gameObject.transform.position = (p.ToVector3() + Vector3.one / 2f) * (1 << lod) - Vector3.one / 2f + Vector3.up / 2f;
		gameObject.transform.localScale = Vector3.one * (1 << lod);
		gameObject.transform.SetParent(parent);
		gameObject.GetComponent<MeshRenderer>().material.color = color;
		return gameObject;
	}

	[ContextMenu("Test Memory")]
	public void TestMemory()
	{
		if (_search == null)
		{
			_builder = new PathLodBuilder(pathfinding.GetPathLod(lodMin), branchEvaluator);
			_accepter = new PathGoalAccepter(goal);
			_search = new PathSearch(new PathAStarQueue(goal.RepresentativePosition), _builder, _accepter, _visited);
			_sourceLink = new PathLink(source);
		}
		_search.Start(_sourceLink);
		_search.ForceImmediate();
		_search.Clear();
	}
}
