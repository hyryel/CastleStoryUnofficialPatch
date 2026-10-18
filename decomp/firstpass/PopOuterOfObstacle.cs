using Brix.Engine;
using Brix.Engine.Nature;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Utils.Logging;
using UnityEngine;
using UnityEngine.Networking;

public class PopOuterOfObstacle : UpdatableGameComponent<PopOuterOfObstacle>
{
	private static BrixLogger logger = new BrixLogger(typeof(PopOuterOfObstacle).FullName);

	[SerializeField]
	private Locomotion4 locomotion;

	public override void OnRequested()
	{
		base.OnRequested();
		ConnectToSignals();
		base.enabled = NetworkServer.active;
	}

	public void ConnectToSignals()
	{
	}

	public bool CheckObstacle()
	{
		if (locomotion.IsAirborne)
		{
			return false;
		}
		return FloorPicker.IsOccupiedOrInsideGround(base.gameObject, locomotion.transform.position, avoidCharacterCollisions: false) || NatureRegistry.HasNatureBlockerAt(locomotion.PositionOccupied - XYZ.up);
	}

	protected override void BrixSlowUpdate()
	{
		base.BrixSlowUpdate();
		CheckAndPop();
	}

	public void CheckAndPop()
	{
		if (CheckObstacle())
		{
			PopOut();
		}
	}

	public void PopOut()
	{
		Vector3 position = locomotion.transform.position;
		Vector3 nearestFreePosition = locomotion.GetNearestFreePosition(position);
		logger.InfoFormat("PopOut '{0}' from '{1}' to '{2}'.", base.gameObject, position, nearestFreePosition);
		locomotion.SetPosition(nearestFreePosition, forced: true);
		locomotion.PathBlockedAt(nearestFreePosition);
	}
}
