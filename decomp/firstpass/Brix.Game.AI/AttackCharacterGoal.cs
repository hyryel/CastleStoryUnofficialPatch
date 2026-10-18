using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Game.AI.KnowledgeSpace;
using Brix.Game.Components;
using Brix.Legacy;
using Brix.UI.Icons;
using Brix.Utils.Logging;
using Motus.Behavior;
using UnityEngine;

namespace Brix.Game.AI;

public class AttackCharacterGoal : Goal
{
	private static BrixLogger logger = new BrixLogger(typeof(AttackCharacterGoal).FullName);

	[SerializeField]
	private VoxelTransform _target;

	public int qteAssigned;

	public List<Labor> Assignees = new List<Labor>();

	private float _timeWitoutAssignation;

	public float RemoveAfterDelay;

	protected SignalConnection TargetMovedConnection = SignalConnection.Empty;

	public GameObject Target
	{
		get
		{
			return (!(_target != null)) ? null : _target.gameObject;
		}
		set
		{
			DisconnectSignalMoved();
			if (value == null)
			{
				Architecte.NetworkAsymmetricDestroy(base.gameObject);
				return;
			}
			_target = value.GetComponent<VoxelTransform>();
			if (_target == null)
			{
				Architecte.NetworkAsymmetricDestroy(base.gameObject);
				return;
			}
			Labor component = _target.GetComponent<Labor>();
			if (component != null)
			{
				component.DestroyedSignal.Connect(delegate
				{
					if (!(this == null))
					{
						Architecte.NetworkAsymmetricDestroy(base.gameObject);
					}
				});
			}
			ConnectSignalMoved();
			MoveToTarget(_target.position);
		}
	}

	public float TargetAggro
	{
		get
		{
			if (Target == null)
			{
				return 0f;
			}
			if ((bool)Target.GetComponent<SpawnerCristal>())
			{
				return 0.2f;
			}
			Occupation component = Target.GetComponent<Occupation>();
			if (component == null)
			{
				return 0f;
			}
			return component.GetProfession().CombatAggro;
		}
	}

	public override IconKey IconKey => IconKeys.Missing;

	private void ConnectSignalMoved()
	{
		TargetMovedConnection = _target.AddMoveListener(TargetMoved);
	}

	private void DisconnectSignalMoved()
	{
		TargetMovedConnection.Disconnect();
	}

	protected override void OnDestroy()
	{
		DisconnectSignalMoved();
		base.OnDestroy();
	}

	public override void EndWork(Task work)
	{
		if (work.Worker != null && work.Worker is Labor && Assignees.Contains(work.Worker as Labor))
		{
			Assignees.Remove(work.Worker as Labor);
		}
		base.EndWork(work);
	}

	protected override Task WrapTask(Labor labor, GameObject target)
	{
		if (!Assignees.Contains(labor))
		{
			Assignees.Add(labor);
		}
		return base.WrapTask(labor, target);
	}

	public override Node GetWorkNode(Labor Worker)
	{
		if (this == null || Worker == null)
		{
			return (StaticNode)Node.Sequence.Do(delegate
			{
			});
		}
		return (StaticNode)Node.Selector.When(() => Target == null).Do(delegate
		{
			DisconnectSignalMoved();
		}).When(() => Target != null)
			.Do(() => Worker.GoAttack(Target));
	}

	public override bool IsMatchFor(Labor labor, AIErrorsStore errorDestination, ref float time, ref bool unmappedFlag)
	{
		if (NullDetected(labor, out var error))
		{
			errorDestination.AddDecisionError(error, labor);
			return false;
		}
		if (!Affiliation.IsEnemy(labor.gameObject, Target))
		{
			errorDestination.AddDecisionError(SharedWorkException<Exceptions.CombatIsNotEnnemy>.Get.Problem, labor);
			return false;
		}
		time += 0.05f;
		if (!labor.Profession.CanSack() && !labor.Profession.ProjectileAttackAvailable() && !Knowledge.Instance.CanReach(labor, _target))
		{
			return false;
		}
		return true;
	}

	public override int PriorityFor(Labor labor, ref float time)
	{
		float num = TargetRemainingAggro() * 50f;
		float num2 = (Knowledge.Instance.CanReach(labor, _target) ? 30 : 0);
		float num3 = Vector3.Distance(Target.transform.position, labor.transform.position);
		float num4 = ((labor.Goal == this) ? 50 : 0);
		float num5 = num + num4 - num3 - num2;
		time += 0.1f;
		return (int)num5;
	}

	private bool NullDetected(Labor labor, out AIError error)
	{
		if (this.IsNullOrReleased())
		{
			logger.Info("Null goal in Attack character goal");
			error = SharedWorkException<Exceptions.NullTarget>.Get.Problem;
			return true;
		}
		if (labor.IsNullOrReleased())
		{
			logger.Info("Null labor in Attack character goal");
			error = SharedWorkException<Exceptions.NullLabor>.Get.Problem;
			return true;
		}
		if (Target == null)
		{
			logger.Info("Null Target in Attack character goal");
			error = SharedWorkException<Exceptions.NullTarget>.Get.Problem;
			return true;
		}
		error = null;
		return false;
	}

	private float AssigneesAggro()
	{
		Assignees.RemoveAll((Labor l) => l == null);
		float num = 0f;
		foreach (Labor assignee in Assignees)
		{
			num += assignee.Profession.CombatAggro;
		}
		return num;
	}

	public float TargetRemainingAggro()
	{
		return TargetAggro - AssigneesAggro();
	}

	private void TargetMoved()
	{
		MoveToTarget(_target.voxel.position);
	}

	private void MoveToTarget(XYZ targetPosition)
	{
		base.voxel.position = targetPosition;
	}

	public void Update()
	{
		if (Target == null)
		{
			Architecte.NetworkAsymmetricDestroy(base.gameObject);
		}
		if (qteAssigned > 0)
		{
			_timeWitoutAssignation = Time.time;
		}
	}

	public override bool IsValid()
	{
		if (Target == null)
		{
			return false;
		}
		if (RemoveAfterDelay > 0f && _timeWitoutAssignation < Time.time - RemoveAfterDelay)
		{
			return false;
		}
		return true;
	}
}
