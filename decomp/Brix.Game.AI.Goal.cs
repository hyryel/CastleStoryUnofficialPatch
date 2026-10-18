using System;
using System.Collections.Generic;
using Brix.Audio;
using Brix.External.Signals;
using Brix.Game.AI.InstructionType;
using Brix.Game.Components;
using Brix.UI.Icons;
using Brix.Utils.Logging;
using Motus.Behavior;
using UnityEngine;

namespace Brix.Game.AI;

public abstract class Goal : GameComponent
{
	private static BrixLogger logger = new BrixLogger(typeof(Goal).FullName);

	public Signal GoalStarted = new Signal();

	public LaborInstruction<GameObject> Instruction;

	[SerializeField]
	private List<Labor> workers = new List<Labor>();

	protected Project project;

	public virtual bool Commited
	{
		get
		{
			return true;
		}
		set
		{
		}
	}

	public virtual Accessoire PreferedTool => null;

	public virtual int RequiredLabors => 1;

	public virtual IconKey IconKey => null;

	public OrderPackage<Labor> Package(Labor.DecisionSource source)
	{
		return Order.Package.OfGameObject(Instruction, base.gameObject, source);
	}

	protected override void Awake()
	{
		base.Awake();
		Instruction = new LaborInstruction<GameObject>
		{
			GetTask = WrapTask
		};
	}

	protected override void Start()
	{
		base.Start();
		GoalStarted.Invoke();
	}

	public virtual void ConnectProject(Project project)
	{
		this.project = project;
	}

	public virtual bool IsValid()
	{
		return true;
	}

	public virtual Node GetWorkNode(Labor Worker)
	{
		throw new NotImplementedException();
	}

	public virtual bool IsMatchFor(Labor labor, AIErrorsStore errorDestination, ref float time, ref bool unmappedFlag)
	{
		return false;
	}

	public virtual int PriorityFor(Labor labor, ref float time)
	{
		return 0;
	}

	public int PriorityFor(Labor labor)
	{
		float time = 0f;
		return PriorityFor(labor, ref time);
	}

	public virtual int BasePriority()
	{
		return 0;
	}

	protected virtual Task WrapTask(Labor labor, GameObject target)
	{
		Task task = Task.CreateFromGoal(this, labor, target);
		if (!workers.Contains(labor))
		{
			workers.Add(labor);
		}
		logger.InfoFormat("{0} started task#{1} with description : '{2}'", labor.name, task.GetHashCode(), task.Description);
		return task;
	}

	public virtual void EndWork(Task work)
	{
		BaseLabor worker = work.Worker;
		string endSoundSignal = GetEndSoundSignal(worker as Labor, work.result);
		if (endSoundSignal != string.Empty)
		{
			SoundEngine.Play(GetEndSoundSignal(worker as Labor, work.result), worker.transform.gameObject);
		}
		worker.LastWorkEndTime = Time.time;
		UnassignWorker(work);
	}

	public void UnassignWorker(Task task)
	{
		BaseLabor worker = task.Worker;
		worker.DelayBeforeAvailable = 0.05f;
		worker.signal.WorkEnded.Invoke();
		if (workers.Contains(task.Worker as Labor))
		{
			workers.Remove(task.Worker as Labor);
		}
		task.Worker = null;
	}

	private string GetEndSoundSignal(Labor worker, TaskResult result)
	{
		if (result == TaskResult.Succes)
		{
			return worker.Sounds.TaskCompleted;
		}
		return string.Empty;
	}

	public bool HasOtherWorkerAssigned(Labor labor)
	{
		return workers.Count > (workers.Contains(labor) ? 1 : 0);
	}

	public int AssignedWorkerCount()
	{
		return workers.Count;
	}

	public int CompareTo(Goal other)
	{
		return other.GetInstanceID().CompareTo(GetInstanceID());
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.1.0.9782' (yours is '9.1.0.7988')
