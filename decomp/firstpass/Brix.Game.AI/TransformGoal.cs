using Brix.External.Factories;
using Brix.Game.AI.InstructionType;
using Brix.Game.AI.KnowledgeSpace;
using Brix.Game.AI.Nodes;
using Brix.Game.Components;
using Brix.Game.Locomotion;
using Brix.Lifecycle.Pooling;
using Brix.UI.Icons;
using Motus.Behavior;
using UnityEngine;

namespace Brix.Game.AI;

public class TransformGoal : Goal
{
	public bool DestroyIfAbandonned;

	public bool EatGarnotteWhileWorking = true;

	public bool DigOnlyWhatFitsInBag = true;

	public bool Highlight;

	private GameObject ObjectToTransform;

	private Factory.AssetKey TransformInto;

	private int TransformQty;

	private WorkNodeGenerator<TransformGoal> workNodeGenerator;

	public LaborInstruction<GameObject> GoTransform;

	public override IconKey IconKey => IconKeys.Missing;

	public override Accessoire PreferedTool => Adjectif.blockBreaking;

	protected override void Awake()
	{
		base.Awake();
		GoTransform = new LaborInstruction<GameObject>("DigGoal", IconKey, (Labor labor, GameObject value) => GetWorkNode(labor));
	}

	protected override void Start()
	{
		SetupBuilder();
		base.Start();
	}

	private void SetupBuilder()
	{
		workNodeGenerator = new WorkNodeGenerator<TransformGoal>();
		workNodeGenerator.WorkAction = TransformAction;
		workNodeGenerator.WorkDoneCondition = TransformationComplete;
		workNodeGenerator.Animation = Working.WorkStates.Pick;
		workNodeGenerator.ReleaseWhen = (ReleaseNodeWhen)(4 | (DestroyIfAbandonned ? 2 : 0));
		if (EatGarnotteWhileWorking)
		{
			workNodeGenerator.ActionWrappers.Add(WorkNodesWrappers.EatGarnotteWhileWorking);
		}
		if (Highlight)
		{
			workNodeGenerator.NodeWrappers.Add(WorkNodesWrappers.HighlightTargetYellow);
		}
		if (DestroyIfAbandonned)
		{
			workNodeGenerator.Finally = delegate(TransformGoal g, Labor l)
			{
				ObjectPoolSingleton.Release(g.gameObject);
			};
		}
	}

	public void RebuildNode(GameObject objectToTransform, Factory.AssetKey transformInto, int Qty)
	{
		ObjectToTransform = objectToTransform;
		TransformInto = transformInto;
		TransformQty = Qty;
		SetupBuilder();
	}

	public override Node GetWorkNode(Labor Worker)
	{
		return workNodeGenerator.BuildNode(this, Worker);
	}

	private void SpawnEm(Factory.AssetKey key, int qty, Vector3 atPosition, float inRadius)
	{
		for (int i = 0; i < qty; i++)
		{
			Vector2 vector = Random.insideUnitCircle * inRadius;
			Vector3 position = new Vector3(atPosition.x + vector.x, atPosition.y, atPosition.z + vector.y);
			ObjectPoolSingleton.Request(out var _, key, position, Quaternion.identity);
		}
	}

	public override bool IsMatchFor(Labor labor, AIErrorsStore errorDestination, ref float time, ref bool unmappedFlag)
	{
		if (HasOtherWorkerAssigned(labor))
		{
			errorDestination.AddDecisionError(SharedWorkException<Exceptions.GoalTooCrowded>.Get.Problem, labor);
			return false;
		}
		time += 0.05f;
		if (!Knowledge.Instance.CanReach(labor, this))
		{
			errorDestination.AddDecisionError(SharedWorkException<Exceptions.CantReach>.Get.Problem, labor);
			return false;
		}
		return true;
	}

	public override int PriorityFor(Labor labor, ref float time)
	{
		return -(int)Vector3.Distance(labor.transform.position, base.transform.position);
	}

	private Node TransformAction(TransformGoal goal, Labor labor)
	{
		return Node.Sequence.Do(delegate(TransformGoal self)
		{
			self.SpawnEm(self.TransformInto, self.TransformQty, self.ObjectToTransform.transform.position, 0.5f);
		}, this).Do(delegate(GameObject goToTransform)
		{
			goToTransform.SafeNetworkDestroy();
		}, ObjectToTransform).ToNode;
	}

	private bool TransformationComplete(TransformGoal goal, Labor ignored)
	{
		return ObjectToTransform.IsNullOrReleased();
	}
}
