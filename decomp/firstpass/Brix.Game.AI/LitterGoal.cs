using System.Collections.Generic;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.AI.KnowledgeSpace;
using Brix.Game.AI.Nodes;
using Brix.Game.Components;
using Brix.UI.Icons;
using Motus.Behavior;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.Game.AI;

public class LitterGoal : StorableGoal
{
	[JsonProperty]
	public HashSet<Transform> Garnottes = new HashSet<Transform>();

	private Transform _transform;

	public Signal<Transform> RejectGarnotteSignal = new Signal<Transform>();

	public Adjectif EatThis;

	private Node.Enumerator _createTreeEnumerator;

	public override IconKey IconKey => IconKeys.Missing;

	protected override void Awake()
	{
		base.Awake();
		_transform = base.transform;
		_createTreeEnumerator = CreateTree().ToEnumerator();
	}

	public override Node GetWorkNode(Labor Worker)
	{
		Node n = (StaticNode)Node.Sequence.Do(delegate
		{
			Worker.eater.EatOnly(EatThis);
		}).Do(() => Worker.RunToExactly(Location.Of(this))).Label("Navigate to ")
			.Finally(delegate
			{
				Worker.eater.Starve();
			});
		return n.FailWhen(SharedWorkException<Exceptions.NullTarget>.Get).Condition(GameComponentExtender.IsNullOrReleased, this);
	}

	public override void UniqueItems(ref HashSet<Factory.AssetKey> keys)
	{
		foreach (Transform garnotte in Garnottes)
		{
			if (!(garnotte == null) && !garnotte.gameObject.IsNullOrReleased())
			{
				keys.Add(garnotte.GetComponent<AssetKeyHolder>().AssetKey);
			}
		}
	}

	protected Node CreateTree()
	{
		return (StaticNode)Node.Selector.When(() => this == null).Do(Node.Empty).Else()
			.Do(Node.Sequence.Do(delegate
			{
				SnapToFloor();
			}).Do(delegate
			{
				CheckForGarnottes();
			}).Do(Node.WaitForTicks(30))
				.Loop());
	}

	private void FixedUpdate()
	{
		_createTreeEnumerator.MoveNext();
	}

	protected override void OnDestroy()
	{
		_createTreeEnumerator.Dispose();
		base.OnDestroy();
	}

	private void SnapToFloor()
	{
		base.voxel.MovePivot(Navigation.SnapToWalkableSurface(base.voxel.position));
	}

	public bool IsNearby(Transform t, float maxDistance = 2f)
	{
		return (t.position - _transform.position).sqrMagnitude <= maxDistance * maxDistance;
	}

	private bool _RejectGarnotte(Transform t)
	{
		RejectGarnotteSignal.Invoke(t);
		return true;
	}

	private void RemoveInvalidGarnottes()
	{
		Garnottes.RemoveWhere((Transform t) => (t == null || !t.gameObject.activeSelf || !IsNearby(t)) && _RejectGarnotte(t));
	}

	private void CheckForGarnottes()
	{
		RemoveInvalidGarnottes();
		if (Garnottes.Count == 0)
		{
			base.gameObject.SafeNetworkDestroy();
		}
	}

	public override bool IsValid()
	{
		RemoveInvalidGarnottes();
		return Garnottes.Count > 0;
	}

	public override bool IsMatchFor(Labor labor, AIErrorsStore errorDestination, ref float time, ref bool unmappedFlag)
	{
		if (HasOtherWorkerAssigned(labor))
		{
			errorDestination.AddDecisionError(SharedWorkException<Exceptions.GoalTooCrowded>.Get.Problem, labor);
			return false;
		}
		if (labor.recepteur.IsFull())
		{
			errorDestination.AddDecisionError(SharedWorkException<Exceptions.InventoryIsFull>.Get.Problem, labor);
			return false;
		}
		time += 0.05f;
		if (!Knowledge.Instance.CanReach(labor, this))
		{
			errorDestination.AddDecisionError(SharedWorkException<Exceptions.CantReach>.Get.Problem, labor);
			return false;
		}
		time += 0.1f;
		if (!CouldStore(labor))
		{
			errorDestination.AddDecisionError(SharedWorkException<Exceptions.BagTooFullToDig>.Get.Problem, labor);
			return false;
		}
		return true;
	}

	public override int PriorityFor(Labor labor, ref float time)
	{
		return -Mathf.RoundToInt(Vector3.Distance(labor.transform.position, base.gameObject.transform.position));
	}
}
