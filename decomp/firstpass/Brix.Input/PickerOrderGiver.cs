using System;
using System.Collections.Generic;
using System.Linq;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.Network;
using Brix.Input.ContextualOrders;
using UnityEngine;

namespace Brix.Input;

public class PickerOrderGiver
{
	public const float ExpirationDelay = 2f;

	private readonly List<GameObject> _actors = new List<GameObject>();

	private Vector3 _targetPosition;

	private GameObject _targetObject;

	private ActionType _actionType;

	private float _timestamp;

	private SignalConnection _taskEndedConnection = SignalConnection.Empty;

	private readonly Action<Task, TaskResult> _taskEndedAction;

	public PickerOrderGiver()
	{
		_taskEndedAction = OnTaskEnded;
	}

	public bool AcceptNewOrder(List<GameObject> actors, Vector3 targetPosition, GameObject targetObject, ActionType actionType)
	{
		if (actors.Count != _actors.Count || Enumerable.Range(0, actors.Count).Any((int i) => actors[i] != _actors[i]) || targetPosition != _targetPosition || targetObject != _targetObject || actionType != _actionType || Time.time >= _timestamp + 2f)
		{
			_actors.Clear();
			_actors.AddRange(actors);
			_targetPosition = targetPosition;
			_targetObject = targetObject;
			_actionType = actionType;
			_timestamp = Time.time;
			_taskEndedConnection.Disconnect();
			_taskEndedConnection = GameSignals.Once(Task.AnyTaskFinalized, _taskEndedAction);
			return true;
		}
		return false;
	}

	public void OnTaskEnded(Task task, TaskResult result)
	{
		if (!(task.TaskStartedTimestamp <= _timestamp))
		{
			_actors.Clear();
			_targetPosition = Vector3.zero;
			_targetObject = null;
			_actionType = ActionType.Primary;
			_timestamp = 0f;
		}
	}

	public void DirectOrder(List<GameObject> listGO, Vector3 target)
	{
		ActionType actionType = PickingUtility.GetActionType();
		if (AcceptNewOrder(listGO, target, null, actionType))
		{
			UNetContextualCmd component = User.LocalUser.GetComponent<UNetContextualCmd>();
			component.CallCmdBatchedContextualActionVector(listGO.ToArray(), target, actionType);
		}
	}

	public void DirectOrder(List<GameObject> listGO, GameObject target)
	{
		ActionType actionType = PickingUtility.GetActionType();
		if (AcceptNewOrder(listGO, Vector3.zero, target, actionType))
		{
			UNetContextualCmd component = User.LocalUser.GetComponent<UNetContextualCmd>();
			component.CallCmdBatchedContextualActionGameObject(listGO.ToArray(), target, actionType);
		}
	}
}
