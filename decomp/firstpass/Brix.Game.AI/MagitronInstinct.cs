using Brix.Game.AI.InstructionType;
using Brix.Game.Components.Wards;
using Brix.Game.Locomotion;
using Brix.UI.Icons;
using Motus.Behavior;
using UnityEngine;

namespace Brix.Game.AI;

internal class MagitronInstinct : LaborInstinct
{
	private MagitronRez rez;

	private WorkNodeGenerator<MagitronRez> generator;

	protected override void Awake()
	{
		base.Awake();
		rez = GetComponent<MagitronRez>();
		SetupGenerator();
	}

	protected override bool IsOutnumbered()
	{
		return rez.ReadyToRez && !rez.IsOnCooldown;
	}

	protected override OrderPackage<Labor, GameObject> OnOutnumbered()
	{
		OrderPackage<Labor, GameObject> orderPackage = new OrderPackage<Labor, GameObject>(Resurect(), null);
		orderPackage.DecisionSource = Labor.DecisionSource.Character;
		return orderPackage;
	}

	private LaborInstruction<GameObject> Resurect()
	{
		return new LaborInstruction<GameObject>("Resurect", IconKeys.Corruptron, (Labor l, GameObject go) => ResurectNode(l));
	}

	private void SetupGenerator()
	{
		generator = new WorkNodeGenerator<MagitronRez>();
		generator.Animation = Working.WorkStates.Resurect;
		generator.WorkAction = Spawn;
		generator.WorkDoneCondition = (MagitronRez m, Labor l) => m.IsOnCooldown;
		generator.ReleaseWhen = ReleaseNodeWhen.Dont;
		generator.LoopAction = false;
		generator.GetLocation = (MagitronRez t) => Location.Everywhere;
	}

	private Node Spawn(MagitronRez self, Labor labor)
	{
		return Node.Do(delegate
		{
			rez.SpawnCorruptrons();
		});
	}

	private Node ResurectNode(Labor labor)
	{
		return (StaticNode)Node.Sequence.Do(() => labor.Activity = Activity.Fighting).Do(() => generator.BuildNode(rez, labor));
	}
}
