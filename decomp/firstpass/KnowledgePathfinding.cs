using Brix.Engine;
using Brix.Game.AI;
using Brix.Pathfinding.Test;
using UnityEngine;

public class KnowledgePathfinding : MonoBehaviour
{
	public SearchPathRequest pathRequest;

	protected void Start()
	{
		GameSignals.LateConnect(TestPathfinding.InitializedSignal, OnPathfindingReady);
	}

	private void OnPathfindingReady()
	{
		pathRequest = SearchPathRequest.Make();
	}

	private void OnDestroy()
	{
		if (pathRequest != null)
		{
			pathRequest.Dispose();
			pathRequest = null;
		}
	}
}
