using System;
using System.Collections.Generic;
using Brix.Audio;
using Brix.Engine;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Game.Semantique;
using Smooth.Algebraics;
using Smooth.Slinq;
using UnityEngine;

public class ConflictResolver
{
	public enum ResolutionModel
	{
		Mining,
		Construction,
		Warrior,
		Operation
	}

	private enum CarryStatus
	{
		bothEmpty,
		onlyMe,
		onlyYou,
		bothCarry
	}

	public static HashSet<Tuple<Labor, Labor>> SwitchTaskCooldowns = new HashSet<Tuple<Labor, Labor>>();

	public IOperable<Labor> project;

	public Func<ResolutionModel> GetResolutionModel;

	public ResolutionModel ResolutionMode
	{
		get
		{
			return GetResolutionModel();
		}
		set
		{
			GetResolutionModel = () => value;
		}
	}

	public ConflictResolver(Func<ResolutionModel> getResolutionModel)
	{
		GetResolutionModel = getResolutionModel;
	}

	public ConflictResolver(IOperable<Labor> project, ResolutionModel resolutionModel = ResolutionModel.Mining)
	{
		ResolutionMode = resolutionModel;
		this.project = project;
	}

	public bool HasValidTransfer(Labor a, Labor b)
	{
		if (a.faction != b.faction)
		{
			return false;
		}
		if (!a.Autonomy.CanSwitchTask || !b.Autonomy.CanSwitchTask)
		{
			return false;
		}
		if (!a.Profession.IsWorker() || !b.Profession.IsWorker())
		{
			return false;
		}
		if (IsOnCooldown(a, b))
		{
			return false;
		}
		if (ResolutionMode == ResolutionModel.Construction && !ConstructionSwitchPermission(a, b))
		{
			return false;
		}
		return true;
	}

	public void OnLaborRequestTransfer(Labor a, Labor b)
	{
		SolveItNOW(a, b);
	}

	private bool IsOnCooldown(Labor a, Labor b)
	{
		return SwitchTaskCooldowns.Contains(new Tuple<Labor, Labor>(a, b)) || SwitchTaskCooldowns.Contains(new Tuple<Labor, Labor>(b, a));
	}

	private void PutOnCooldown(Labor a, Labor b, float time = 1f)
	{
		Tuple<Labor, Labor> ab = new Tuple<Labor, Labor>(a, b);
		Tuple<Labor, Labor> ba = new Tuple<Labor, Labor>(b, a);
		SwitchTaskCooldowns.Add(ab);
		SwitchTaskCooldowns.Add(ba);
		Neo.Relais(delegate
		{
			SwitchTaskCooldowns.Remove(ab);
		}, time, "switchtask cooldown");
		Neo.Relais(delegate
		{
			SwitchTaskCooldowns.Remove(ba);
		}, time, "switchtask cooldown");
	}

	private void SolveItNOW(Labor a, Labor b)
	{
		if (!HasValidTransfer(a, b))
		{
			a.navigation.InvalidTransfer.Invoke();
			return;
		}
		if (a.CurrentTask != null)
		{
			a.CurrentTask.result = TaskResult.Switched;
		}
		if (b.CurrentTask != null)
		{
			b.CurrentTask.result = TaskResult.Switched;
		}
		OperationSwitchTask(a, b);
	}

	private void OperationSwitchTask(Labor a, Labor b)
	{
		a.navigation.Locomotion.PathBlockedAt(a.transform.position);
		b.navigation.Locomotion.PathBlockedAt(b.transform.position);
		SwitchInstructions(a, b);
		IDescriptor descriptor = a.recepteur.StoredItems.Slinq().FirstOrDefault();
		if (descriptor != null)
		{
			SoundEngine.SetSwitch("Material", SoundController.GetObjectTypeSwitch(descriptor.Description));
			SoundEngine.Play(a.Sounds.PassObject, a.gameObject);
		}
		a.recepteur.SwitchContentWith(b.recepteur);
		PutOnCooldown(a, b);
	}

	private void SwitchOperable(Labor a, Labor b)
	{
		AutonomyStatus autonomy = a.Autonomy;
		AutonomyStatus autonomy2 = b.Autonomy;
		if (a.Operable != null)
		{
			a.Operable.Hire(b);
		}
		if (b.Operable != null)
		{
			b.Operable.Hire(a);
		}
		a.Autonomy = autonomy2;
		b.Autonomy = autonomy;
	}

	private void SwitchInstructions(Labor a, Labor b)
	{
		OrderPackage<Labor> currentPackage = GetCurrentPackage(a);
		OrderPackage<Labor> currentPackage2 = GetCurrentPackage(b);
		if (currentPackage2 != null)
		{
			currentPackage2.Send(a);
		}
		else
		{
			Order.Move.Halt.Send(a);
		}
		if (currentPackage != null)
		{
			currentPackage.Send(b);
		}
		else
		{
			Order.Move.Halt.Send(b);
		}
	}

	private OrderPackage<Labor> GetCurrentPackage(Labor labor)
	{
		if (labor.CurrentTask != null && labor.CurrentTask.ValidateFunc())
		{
			return labor.CurrentTask.InitialPackage as OrderPackage<Labor>;
		}
		return null;
	}

	private bool ConstructionSwitchPermission(Labor a, Labor b)
	{
		CarryStatus carryStatus = (a.recepteur.IsEmpty() ? ((!b.recepteur.IsEmpty()) ? CarryStatus.onlyYou : CarryStatus.bothEmpty) : (b.recepteur.IsEmpty() ? CarryStatus.onlyMe : CarryStatus.bothCarry));
		GameObject currentTaskGiver = a.GetCurrentTaskGiver();
		GameObject currentTaskGiver2 = b.GetCurrentTaskGiver();
		int num = ((!currentTaskGiver.IsNullOrReleased()) ? currentTaskGiver.GetComponent<Goal>().BasePriority() : 0);
		int num2 = ((!currentTaskGiver2.IsNullOrReleased()) ? currentTaskGiver2.GetComponent<Goal>().BasePriority() : 0);
		switch (carryStatus)
		{
		case CarryStatus.onlyMe:
			return true;
		case CarryStatus.bothCarry:
			if (currentTaskGiver != null && currentTaskGiver == currentTaskGiver2)
			{
				return false;
			}
			break;
		}
		if (carryStatus == CarryStatus.bothCarry && num > num2)
		{
			return true;
		}
		if (carryStatus == CarryStatus.bothCarry && num == num2)
		{
			return true;
		}
		if (carryStatus == CarryStatus.bothCarry && IsBlockingMutual(a, b))
		{
			return true;
		}
		if (carryStatus == CarryStatus.bothEmpty && !NeedsSameResource(currentTaskGiver, currentTaskGiver2))
		{
			return true;
		}
		return false;
	}

	private bool IsBlockingMutual(Labor a, Labor b)
	{
		return b.presence.PositionEntering == a.voxel.position;
	}

	private bool NeedsSameResource(GameObject a, GameObject b)
	{
		if (a.IsNullOrReleased() || b.IsNullOrReleased())
		{
			return false;
		}
		BuildGoal component = a.GetComponent<BuildGoal>();
		BuildGoal component2 = b.GetComponent<BuildGoal>();
		if (component == null || component2 == null)
		{
			return false;
		}
		return component.recepteur.CurrentCapacity.PeutPrendre(component2.recepteur.CurrentCapacity) || component2.recepteur.CurrentCapacity.PeutPrendre(component.recepteur.CurrentCapacity);
	}
}
