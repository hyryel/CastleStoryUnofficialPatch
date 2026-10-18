using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Signals;
using Brix.Game.AI.InstructionType;
using Brix.Game.AI.KnowledgeSpace;
using Brix.Game.Components;
using Brix.UI.Icons;
using Brix.Utils.Logging;
using Brix.Utils.Signals;
using Motus.Behavior;
using UnityEngine;

namespace Brix.Game.AI;

public class Task : IComparable<Task>
{
	private static BrixLogger logger = new BrixLogger(typeof(Task).FullName);

	public BaseLabor Worker;

	public TaskResult result;

	public IOrderPackage InitialPackage;

	private Queue<string> Logs = new Queue<string>();

	private float timeStarted;

	public bool CanInterrupt = true;

	public int DelayOnFinished = 10;

	public int DelayOnFail = 30;

	public string Description = string.Empty;

	public GameObject Target;

	public TaskProgression Status;

	public IconKey IconKey = IconKeys.Missing;

	public FailException Exception;

	public float TaskStartedTimestamp;

	private Signal finalized = new Signal();

	private TaskResult cancelledResult = TaskResult.WorkerFailed;

	private TaskResult abortResult = TaskResult.WorkerFailed;

	private HashSet<GameObject> refusedByWorker;

	public Signal<TaskResult> TaskEnded;

	public Signal<TaskResult> TaskCancelled;

	public Signal<string> LogProgress;

	public Func<bool> ValidateFunc = () => true;

	public static GameSignals.GameSignal<Task, TaskResult> AnyTaskFinalized = new GameSignals.GameSignal<Task, TaskResult>();

	private Node _workNode;

	public GameObject Giver { get; set; }

	public Action<Task> EndAction { get; set; }

	public Node WorkNode
	{
		get
		{
			return _workNode;
		}
		set
		{
			_workNode = WrapNode(value);
		}
	}

	public Node UnshieldedWorkNode
	{
		set
		{
			_workNode = value;
		}
	}

	public Task()
	{
		TaskEnded = new Signal<TaskResult>();
		TaskCancelled = new Signal<TaskResult>();
		LogProgress = new Signal<string>();
	}

	public void RefuseOnlyThese(IEnumerable<GameObject> gos)
	{
		refusedByWorker = new HashSet<GameObject>(gos);
	}

	public IEnumerable<GameObject> ObjectsRefusedByWorker()
	{
		return refusedByWorker;
	}

	private void ConnectSignals()
	{
		TaskCancelled.Connect(delegate(TaskResult workResult, Task self)
		{
			self.cancelledResult = workResult;
		}, this).DisconnectWhen(finalized);
		Worker.AbortWorkSignal.Connect(delegate(TaskResult workResult, Task self)
		{
			self.abortResult = workResult;
		}, this).DisconnectWhen(finalized);
	}

	private void CleanUp()
	{
		finalized.Invoke();
		GameSignals.Invoke(AnyTaskFinalized, this, result);
	}

	private Node WrapWithSignals(Node node)
	{
		return Node.Sequence.Do(delegate(Task self)
		{
			self.ConnectSignals();
		}, this).Do((StaticNode)node).Finally(delegate(Task self)
		{
			self.CleanUp();
		}, this)
			.ToNode;
	}

	public Node WrapNode(Node workNode)
	{
		Brancher.Builder builder = Node.Selector.When(TaskCancelled, (TaskResult result, Task self) => self.CanInterruptTask(result), this).Label("When --> TaskCancelled signal Fired").Do((Task self) => self.result = self.cancelledResult, this)
			.Label("Set Result to cancelled result")
			.When(Worker.AbortWorkSignal)
			.Label("When --> AbortWorkSignal signal Fired")
			.Do((Task self) => self.result = self.abortResult, this)
			.Label("Set Result to abort result")
			.Default()
			.Label("Default --> No TaskAborts or TaskCancelled")
			.Do(delegate(Task self)
			{
				IconKey iconKey = self.IconKey;
				self.Worker.CurrentIconKey = iconKey;
				return iconKey;
			}, this)
			.Label("Set CurrentIconKey")
			.Do((Task self) => self.TaskStartedTimestamp = Time.time, this)
			.Label("Set TaskStartedTimestamp")
			.Do((Task self) => self.Worker.GetReadyForNextTask(), this)
			.Label("Worker.GetReadyForNextTask()")
			.Do((StaticNode)workNode)
			.Label(Description, Giver);
		return Catches(WrapWithSignals(CoreNode((StaticNode)builder)));
	}

	private bool CanInterruptTask(TaskResult t)
	{
		if (CanInterrupt)
		{
			CanInterrupt = false;
			return true;
		}
		return false;
	}

	private Node Catches(Node node)
	{
		return (StaticNode)Node.Catcher.Do(() => node).Catch(delegate(FailException e, Task self)
		{
			self.OnFailException(e);
		}, this).Catch((Exceptions.WorkFailedException e, Task self) => self.OnWorkFailed(e), this)
			.Catch((Exceptions.PathException e, Task self, Labor labor) => self.OnPathFailed(labor, e), this, Worker as Labor)
			.WaitAfterFailure(DelayOnFail)
			.Finally(OnFinally);
	}

	private bool ResetExclusions()
	{
		return (InitialPackage != null && InitialPackage.DecisionSource == Labor.DecisionSource.User) || (result == TaskResult.Succes && Worker is Labor);
	}

	private void OnFinally()
	{
		if (ResetExclusions())
		{
			Knowledge.Instance.ResetExclusionsFor(Worker.gameObject, 5f);
			logger.InfoFormat("{0} finished task#{1} with description : '{2}'", Worker.gameObject.name, GetHashCode(), Description);
		}
		else
		{
			logger.InfoFormat("{0} failed task#{1} with description : '{2}'", Worker.gameObject.name, GetHashCode(), Description);
		}
		EndAction(this);
		TaskEnded.Invoke(result);
		Logs.Enqueue("Ended on " + result);
	}

	private Node CoreNode(Node node)
	{
		return Node.Sequence.Do((Task self) => self.Status = TaskProgression.Running, this).Label("SetProgression to Running").Do((StaticNode)node)
			.Do((Task self) => self.Status = TaskProgression.Finished, this)
			.Label("SetProgression to Finished")
			.Do(Node.WaitForTicks(DelayOnFinished))
			.Label("Delay on Finished")
			.ToNode;
	}

	private void OnFailException(FailException e)
	{
		Status = TaskProgression.Failed;
		result = TaskResult.WorkerFailed;
		Exception = e;
	}

	private Node OnWorkFailed(Exceptions.WorkFailedException e)
	{
		Status = TaskProgression.Failed;
		result = TaskResult.WorkerFailed;
		return Worker.HandleWorkException(e);
	}

	private Node OnPathFailed(Labor worker, Exceptions.PathException e)
	{
		Status = TaskProgression.Failed;
		result = TaskResult.PathFailed;
		if (InitialPackage.DecisionSource != Labor.DecisionSource.User)
		{
			return worker.HandlePathFail(e);
		}
		return Node.Empty;
	}

	public static Task CreateFromGoal(Goal goal, Labor worker, GameObject target)
	{
		Task task = new Task
		{
			Giver = goal.gameObject,
			IconKey = goal.IconKey,
			Description = $"{worker.name} doing {goal.name}",
			Target = target,
			Worker = worker,
			WorkNode = goal.GetWorkNode(worker)
		};
		task.EndAction = goal.EndWork;
		task.ValidateFunc = () => !task.Giver.IsNullOrReleased();
		return task;
	}

	public static Task GetTaskFromInstruction<TLabor>(WorkInstruction<TLabor> instruction, BaseLabor labor, Node node) where TLabor : BaseLabor
	{
		Task task = new Task();
		task.Description = instruction.Description;
		task.IconKey = instruction.IconKey;
		task.CanInterrupt = instruction.CanInterrupt;
		task.Worker = labor;
		task.EndAction = labor.DefaultEndWork;
		task.WorkNode = node;
		return task;
	}

	public string GetLog()
	{
		if (Logs == null || Logs.Count == 0)
		{
			return "No message yet";
		}
		return Logs.Peek();
	}

	public int CompareTo(Task other)
	{
		return other.GetHashCode().CompareTo(GetHashCode());
	}
}
