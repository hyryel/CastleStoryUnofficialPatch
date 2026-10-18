using System;
using System.Collections.Generic;
using Brix.Game.AI.Nodes;
using Brix.Game.Components;
using Brix.Game.Locomotion;
using Brix.Lifecycle.Pooling;
using Brix.Utils.Logging;
using Motus.Behavior;
using UnityEngine;

namespace Brix.Game.AI;

public class WorkNodeGenerator<T> where T : MonoBehaviour
{
	public delegate Node NodeFunc(T goal, Labor worker);

	public delegate bool NodePredicate(T goal, Labor worker);

	private static BrixLogger logger = new BrixLogger(typeof(WorkNodeGenerator<T>).FullName);

	public bool FailOnTargetReleased = true;

	public NodePredicate WorkDoneCondition = (T goal, Labor l) => false;

	public NodePredicate WorkInvalidCondition = (T goal, Labor l) => false;

	public NodeFunc PrepareAction = (T goal, Labor labor) => Node.Empty;

	public NodeFunc OnWorkEnded = (T goal, Labor labor) => Node.Empty;

	public NodeFunc OnWorkInvalid = (T goal, Labor labor) => Node.Empty;

	public NodeFunc WorkAction = (T goal, Labor labor) => (StaticNode)Node.Sequence.Do(delegate
	{
		logger.Info("Doing Imaginary Work!");
	});

	public bool UseCustomAnimation;

	public Func<Transform, Labor, Node> CustomAnimation = (Transform goal, Labor labor) => (StaticNode)Node.Sequence.Do(delegate
	{
		logger.Info("Doing Imaginary Animation!");
	});

	public Working.WorkStates Animation = Working.WorkStates.Hammer;

	public Action<T, Labor> Finally = delegate
	{
	};

	public Func<T, Location> GetLocation = (T t) => Location.Of(t);

	public bool LoopAction = true;

	public ReleaseNodeWhen ReleaseWhen = ReleaseNodeWhen.Ended;

	public List<WorkNodesWrappers.NodeWrappingFunc<T>> ActionWrappers = new List<WorkNodesWrappers.NodeWrappingFunc<T>>();

	public List<WorkNodesWrappers.NodeWrappingFunc<T>> NodeWrappers = new List<WorkNodesWrappers.NodeWrappingFunc<T>>();

	public Node BuildNode(T goal, Labor worker)
	{
		return GetWorkNode(goal, worker);
	}

	private Node GetWorkNode(T goal, Labor worker)
	{
		Node node = ApplyNodeWrappers(FinalizedNode(goal, worker), goal, worker);
		if (!FailOnTargetReleased)
		{
			return node;
		}
		return node.FailWhen(SharedWorkException<Exceptions.NullTarget>.Get).Condition((T g) => g == null || g.gameObject.IsNullOrReleased(), goal);
	}

	private Node ApplyActionWrappers(Node node, T goal, Labor labor)
	{
		return _ApplyWrappers(node, goal, labor, ActionWrappers);
	}

	private Node ApplyNodeWrappers(Node node, T goal, Labor labor)
	{
		return _ApplyWrappers(node, goal, labor, NodeWrappers);
	}

	private Node _ApplyWrappers(Node node, T goal, Labor labor, List<WorkNodesWrappers.NodeWrappingFunc<T>> wrappers)
	{
		Node node2 = node;
		foreach (WorkNodesWrappers.NodeWrappingFunc<T> wrapper in wrappers)
		{
			node2 = wrapper(node2, goal, labor);
		}
		return node2;
	}

	private StaticNode FinalizedNode(T component, Labor worker)
	{
		return Node.Sequence.Do(WorkNode(component, worker)).Finally(delegate(WorkNodeGenerator<T> self, T c, Labor w)
		{
			self.Finalization(c, w);
		}, this, component, worker).ToNode;
	}

	private void Finalization(T component, Labor worker)
	{
		if (!(component == null))
		{
			Finally(component, worker);
			ReleaseIfNeeded(ReleaseNodeWhen.Abandoned, component);
		}
	}

	private StaticNode WorkNode(T component, Labor worker)
	{
		bool cantReach = false;
		return Node.Selector.When((WorkNodeGenerator<T> self, T comp) => self.WorkInvalidCondition(comp, worker), this, component).Do((WorkNodeGenerator<T> self, Labor _worker, T _component) => self.WorkInvalid(_component, _worker), this, worker, component).Else()
			.Do(Node.Sequence.Do((WorkNodeGenerator<T> self, Labor _worker, T _component) => self.PrepareAction(_component, _worker), this, worker, component).Label("Prepare Action").Do((Labor _worker, T _component) => GetMoveNode(_component, _worker), worker, component)
				.Label("RunToExactly")
				.Do((Labor _worker, T _component) => cantReach = !_worker.CanInteract(_component), worker, component)
				.Label("Validate CanInteract")
				.Do(Node.Selector.When((WorkNodeGenerator<T> self, T comp) => self.WorkDoneCondition(comp, worker), this, component).Label("When WorkDoneCondition is true").Do(Node.Empty)
					.If((bool _reach) => _reach, cantReach)
					.Label("If cantReach")
					.Do(Node.Fail(SharedWorkException<Exceptions.CantReach>.Get))
					.Else()
					.Do((WorkNodeGenerator<T> self, Labor _worker, T _component) => self.ApplyActionWrappers(self.WorkUntilDone(_component, _worker), _component, _worker), this, worker, component)
					.ToNode)
					.Do((WorkNodeGenerator<T> self, Labor _worker, T _component) => self.EndWork(_component, _worker), this, worker, component)
					.ToNode)
					.ToNode;
			}

			private Node GetMoveNode(T component, Labor worker)
			{
				Location location = GetLocation(component);
				if (location == null)
				{
					return Node.Fail(SharedWorkException<Exceptions.NullTarget>.Get);
				}
				if (!location.TargetReached(worker.gameObject, worker.transform.position))
				{
					return worker.RunToExactly(location);
				}
				if (location.HasSpecificPosition && !worker.navigation.IsTurnedTowards(location.Position))
				{
					return worker.TurnToward(location);
				}
				return Node.Empty;
			}

			private StaticNode WorkInvalid(T component, Labor labor)
			{
				return Node.Sequence.Label("WorkInvalid Sequence").Do((WorkNodeGenerator<T> self, T _component, Labor _labor) => self.OnWorkInvalid(_component, _labor), this, component, labor).Do(delegate(WorkNodeGenerator<T> self, T _component)
				{
					self.ReleaseIfNeeded(ReleaseNodeWhen.Invalid, _component);
				}, this, component);
			}

			private StaticNode EndWork(T component, Labor labor)
			{
				return Node.Sequence.Label("EndWork Sequence").Do((WorkNodeGenerator<T> self, T _component, Labor _labor) => self.OnWorkEnded(_component, _labor), this, component, labor).Label("OnWorkEnded")
					.Do(delegate(WorkNodeGenerator<T> self, T _component)
					{
						self.ReleaseIfNeeded(ReleaseNodeWhen.Ended, _component);
					}, this, component)
					.Label("ReleaseIfNeeded");
			}

			private void ReleaseIfNeeded(ReleaseNodeWhen requirement, T component)
			{
				if (ReleaseWhen.HasFlag(requirement) && !(component == null) && !component.gameObject.IsNullOrReleased())
				{
					ObjectPoolSingleton.Release(component.gameObject);
				}
			}

			private StaticNode WorkUntilDoneCustom(T component, Labor worker)
			{
				StaticNode staticNode = Node.Sequence.Label("WorkUntilDone Sequence").Do((WorkNodeGenerator<T> self, Labor _worker, T _component) => self.CustomAnimation(_component.transform, _worker), this, worker, component).Do((WorkNodeGenerator<T> self, Labor _worker, T _component) => self.WorkAction(_component, _worker), this, worker, component)
					.ToNode;
				if (LoopAction)
				{
					staticNode = Node.Loop(staticNode);
				}
				return staticNode;
			}

			private StaticNode WorkUntilDone(Working.WorkStates work, T component, Labor worker)
			{
				if (LoopAction)
				{
					return Node.Do((WorkNodeGenerator<T> self, Working.WorkStates _work, Labor _worker, T _component) => AnimationNodes.Loop(WorkAction(_component, _worker), work, _component.transform, _worker), this, work, worker, component);
				}
				return Node.Do((WorkNodeGenerator<T> self, Working.WorkStates _work, Labor _worker, T _component) => AnimationNodes.Once(WorkAction(_component, _worker), work, _component.transform, _worker), this, work, worker, component);
			}

			private StaticNode WorkUntilDone(T component, Labor worker)
			{
				if (UseCustomAnimation)
				{
					return WorkUntilDoneCustom(component, worker);
				}
				return WorkUntilDone(Animation, component, worker);
			}
		}
