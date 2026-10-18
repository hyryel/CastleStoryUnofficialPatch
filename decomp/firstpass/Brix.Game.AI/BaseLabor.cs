using System.Collections.Generic;
using Brix.External.Signals;
using Brix.Game.Network;
using Brix.Transactions;
using Brix.UI.Icons;
using Brix.Utils;
using Brix.Utils.Logging;
using Motus.Behavior;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.AI;

public class BaseLabor : MultiBehavior
{
	private BrixLogger logger = new BrixLogger(typeof(BaseLabor).FullName);

	public Task CurrentTask;

	public Task PendingTask;

	public Signal<Node> HasStartedWork = new Signal<Node>();

	public Signal<Node> HasFinishedWork = new Signal<Node>();

	public Signal<Node> HasReceivedOrder = new Signal<Node>();

	public float LastWorkEndTime;

	protected Queue<IOrderPackage> InstructionQueue = new Queue<IOrderPackage>();

	protected Queue<IconKey> IconQueue = new Queue<IconKey>();

	public Signal TakeOnTaskInInstructionQueue = new Signal();

	public Signal<IEnumerable<IconKey>> InstructionQueueChanged = new Signal<IEnumerable<IconKey>>();

	private float _delayBeforeAvailable;

	protected Dictionary<Labor.DecisionSource, float> timeAtLastOrder = new Dictionary<Labor.DecisionSource, float>();

	private IconKey _currentIconKey;

	protected Node CurrentWork => (CurrentTask == null) ? null : CurrentTask.WorkNode;

	public float DelayBeforeAvailable
	{
		get
		{
			return Mathf.Max(0f, _delayBeforeAvailable - Time.time);
		}
		set
		{
			_delayBeforeAvailable = Time.time + value;
		}
	}

	public IconKey CurrentIconKey
	{
		get
		{
			return _currentIconKey;
		}
		set
		{
			_currentIconKey = value;
			InstructionQueueChanged.Invoke(GetInstructionIcons());
		}
	}

	public NetworkSignal<TaskResult> AbortWorkSignal { get; private set; }

	public TaskResult WorkResult { get; set; }

	protected override string CurrentTaskName => (CurrentTask == null) ? "Labor doing nothing" : CurrentTask.Description;

	public virtual BaseLabor Target => this;

	public override void Initialize()
	{
		base.Initialize();
		Transactor orAddComponent = base.gameObject.GetOrAddComponent<Transactor>();
		AbortWorkSignal = new NetworkSignal<TaskResult>(orAddComponent);
	}

	protected override Node CreateTree()
	{
		Node node = base.CreateTree();
		node.Label = base.name;
		return node;
	}

	protected StaticNode ProcessCurrentWorkSequence()
	{
		return Node.Selector.When((BaseLabor self) => self.CurrentWork != null, this).Label("When --> Work is available (CurrentWork != null)").Do((BaseLabor self) => self.WorkResult = TaskResult.Succes, this)
			.Label("Set TaskResult to default value")
			.Do(delegate(BaseLabor self)
			{
				self.HasStartedWork.Invoke(self.CurrentWork);
			}, this)
			.Label("Invoke HasStartedWork")
			.Do((BaseLabor self) => self.CurrentWork, this)
			.Label("Run Work Node")
			.Do(delegate(BaseLabor self)
			{
				self.HasFinishedWork.Invoke(self.CurrentWork);
			}, this)
			.Label("Invoke HasFinishedWork")
			.Do((BaseLabor self) => self.CurrentTask = null, this)
			.Label("Set CurrentTask to null")
			.Do(Node.WaitForTicks(1))
			.When((BaseLabor self) => self.IsAvailable() && self.InstructionQueue.Count > 0, this)
			.Label("When --> Available & has Instructions in queue")
			.Do((BaseLabor self) => self.IconQueue.Dequeue(), this)
			.Label("IconQueue.Dequeue()")
			.Do(delegate(BaseLabor self)
			{
				self.TakeOnTaskInInstructionQueue.Invoke();
			}, this)
			.Label("TakeOnTaskInInstructionQueue.Invoke()")
			.Do(delegate(BaseLabor self)
			{
				self.InstructionQueue.Dequeue().Send(self);
			}, this)
			.Label("Send queued instruction")
			.Do(Node.WaitForTicks(1))
			.Default()
			.Label("Default (No work & InstructionQueue empty)")
			.Do(Node.Forever)
			.ToNode;
	}

	protected StaticNode ProcessPendingWorkSequence()
	{
		return Node.Selector.When((BaseLabor self) => self.PendingTask != null, this).Label("When --> If there is pending work").Do(Node.Selector.When((BaseLabor self) => self.CurrentWork == null, this).Label("When --> Work isn't available (CurrentWork == null)").Do((BaseLabor self) => self.CurrentTask = self.PendingTask, this)
			.Label("Switch work to pendingtask")
			.Do((BaseLabor self) => self.PendingTask = null, this)
			.Label("Set PendingTask to NULL")
			.When((BaseLabor self) => self.CurrentTask.CanInterrupt, this)
			.Label("When --> Current Task can be interrupted")
			.Do(delegate(BaseLabor self)
			{
				self.CurrentTask.TaskCancelled.Invoke(TaskResult.ReceivedNewInstruction);
			}, this)
			.Label("Interrupt it (Invoke TaskCancelled)")
			.Do(Node.WaitForTicks(1))
			.Else()
			.Label("Else --> Wait for CurrentWork to run it's course")
			.Do(Node.Forever)
			.ToNode)
				.Else()
				.Label("Else --> No pending work")
				.Do(Node.Forever)
				.ToNode;
		}

		public void Do(Task work)
		{
			PendingTask = work;
		}

		public bool HasCurrentOrPendingTask(Task task)
		{
			return CurrentTask == task || PendingTask == task;
		}

		public Task GetCurrentOrPendingTask()
		{
			Task task = CurrentTask ?? PendingTask;
			if (task == null)
			{
				return null;
			}
			if (task.WorkNode == null && PendingTask != null)
			{
				task = PendingTask;
			}
			return task;
		}

		public GameObject GetCurrentTaskGiver()
		{
			return GetCurrentOrPendingTask()?.Giver;
		}

		public bool IsAvailable()
		{
			bool flag = CurrentWork == null;
			bool flag2 = PendingTask == null;
			bool flag3 = DelayBeforeAvailable == 0f;
			return flag && flag2 && flag3;
		}

		public float TimeSinceLastOrder()
		{
			return TimeSinceLastOrder(Labor.DecisionSource.Unspecified);
		}

		public float TimeSinceLastOrder(Labor.DecisionSource source)
		{
			float a = ((source == Labor.DecisionSource.Unspecified && timeAtLastOrder.Count != 0) ? (Time.time - (from kvp in timeAtLastOrder.Slinq()
				select kvp.Value).Min()) : (Time.time - timeAtLastOrder.GetValueOrCreate(source, 0f)));
			float b = Time.time - LastWorkEndTime;
			return (!IsAvailable()) ? 0f : Mathf.Min(a, b);
		}

		public IEnumerable<IconKey> GetInstructionIcons()
		{
			yield return CurrentIconKey;
			Queue<IconKey>.Enumerator iconEnumerator = IconQueue.GetEnumerator();
			while (iconEnumerator.MoveNext())
			{
				yield return iconEnumerator.Current;
			}
		}

		public void DefaultEndWork(Task task)
		{
			LastWorkEndTime = Time.time;
			base.signal.WorkEnded.Invoke();
		}

		public virtual Node GetReadyForNextTask()
		{
			return Node.Empty;
		}

		public virtual Node HandleWorkException(Exceptions.WorkFailedException exception)
		{
			logger.InfoFormat("{0} failed with error: {1}", base.name, exception);
			return HandleAIError(exception.Problem);
		}

		public virtual Node HandlePathFail(Exceptions.PathException exception)
		{
			return Node.Empty;
		}

		public virtual Node HandleAIError(AIError error)
		{
			return Node.Empty;
		}

		public virtual void ClearOrderQueue()
		{
			InstructionQueue.Clear();
			CurrentTask = null;
		}

		public virtual void SendOrder(Task task, Labor.DecisionSource source)
		{
			if (source != Labor.DecisionSource.Unspecified)
			{
				timeAtLastOrder[source] = Time.time;
			}
			task.TaskEnded.ConnectOnce(delegate
			{
				LastWorkEndTime = Time.time;
			});
			HasReceivedOrder.Invoke(task.WorkNode);
			Do(task);
		}

		public void QueueOrder(IOrderPackage package)
		{
			InstructionQueue.Enqueue(package);
			if (package.IconKey != null && package.IconKey != IconKeys.Missing)
			{
				IconQueue.Enqueue(package.IconKey);
			}
		}
	}
