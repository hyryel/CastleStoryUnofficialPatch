using System.Collections.Generic;
using System.Runtime.InteropServices;
using Brix.Components;
using Brix.Engine;
using Brix.Engine.Nature;
using Brix.External.Factories;
using Brix.Game.AI.Damage;
using Brix.Game.AI.KnowledgeSpace;
using Brix.Game.AI.Nodes;
using Brix.Game.AI.Pathfinding;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Game.Semantique;
using Brix.Pathfinding;
using Brix.UI.Icons;
using Brix.Utils;
using Brix.Utils.Logging;
using Brix.Utils.Signals;
using Motus.Behavior;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.AI;

public class BuildGoal : Goal
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct PriorityComparer : IComparer<BuildGoal>
	{
		public int Compare(BuildGoal x, BuildGoal y)
		{
			return ComparePriority(x, y);
		}
	}

	private static BrixLogger logger = new BrixLogger(typeof(BuildGoal).FullName);

	private CompCache<Blueprint> _blueprint;

	private int _referenceY;

	[SerializeField]
	private bool IgnoreHeightPriority;

	[SerializeField]
	private bool AcceptMultiWorkers;

	private IRecepteur _compoundRecepteur;

	public BuildGoalProvider Provider;

	public bool IsAnti => _blueprint.Get.IsAnti;

	public override bool Commited
	{
		get
		{
			return Blueprint.commited;
		}
		set
		{
			Blueprint.Networkcommited = value;
		}
	}

	public override IconKey IconKey => IconKeys.Missing;

	public void Commit()
	{
		Commited = true;
	}

	public override void Initialize()
	{
		base.Initialize();
		_blueprint = new CompCache<Blueprint>(base.gameObject);
	}

	protected override void Start()
	{
		if (IsAnti)
		{
			base.recepteur.DoExcludeAll();
		}
		_blueprint.Get.OnStateChangedSignal.Connect(delegate(Blueprint aBlueprint, PlacementFlags aPreviousMode, PlacementFlags aNewMode, BuildGoal aBuildGoal)
		{
			aBuildGoal.OnBlueprintStateChanged(aBlueprint, aPreviousMode, aNewMode);
		}, this).DisconnectWhen(base.OnReleaseSignal);
		base.Start();
		base.enabled = _blueprint.Get.State.HasFlag(PlacementFlags.placed);
	}

	private void OnBlueprintStateChanged(Blueprint blueprint, PlacementFlags previousMode, PlacementFlags newMode)
	{
		if (((previousMode ^ newMode) & PlacementFlags.placed) != PlacementFlags.none)
		{
			base.enabled = newMode.HasFlag(PlacementFlags.placed);
		}
		if (((previousMode ^ newMode) & (PlacementFlags.placed | PlacementFlags.buildable)) != PlacementFlags.none)
		{
			_referenceY = (from position in base.volume.blueprint.Slinq()
				select position.y).Min();
		}
	}

	public override Node GetWorkNode(Labor labor)
	{
		if (this.IsNullOrReleased())
		{
			return Node.FailWork<Exceptions.NullTarget>();
		}
		if (labor.IsNullOrReleased())
		{
			return Node.FailWork<Exceptions.NullLabor>();
		}
		if (IsAnti)
		{
			return AntiBlueprintNode(labor);
		}
		if (labor.recepteur.FirstThatFitsIn(base.recepteur) != null)
		{
			return Node.Do((BuildGoal aBuildGoal, Labor aLabor) => aLabor.GoStoreIn(aBuildGoal), this, labor);
		}
		return (StaticNode)Node.Sequence.Label("BuildGoal.GetWorkNode Sequence").Do((BuildGoal aBuildGoal, Labor aLabor) => aLabor.FindAndPickUp(aBuildGoal.recepteur), this, labor).Label("FindAndPickUp", labor)
			.Do((BuildGoal aBuildGoal, Labor aLabor) => aLabor.GoStoreIn(aBuildGoal), this, labor);
	}

	public override bool IsValid()
	{
		return !base.recepteur.IsFull();
	}

	private Node AntiBlueprintNode(Labor labor)
	{
		XYZ position = base.voxel.position;
		if (GetComponent<RopeBridgeConfigurator>() != null)
		{
			RopeBridgeConfigurator component = GetComponent<RopeBridgeConfigurator>();
			position = component.StartPoint - component.SideDirection;
			if (!VolumeRegistry.HasVolumesWithPropertyAt(position, VProperty.obstacle))
			{
				position = component.StartPoint + component.SideDirection;
			}
			IVolume v = VolumeRegistry.VolumeWithPropertyAt(position, VProperty.obstacle);
			component.GetComponent<AdaptativeDamageReceiver>().SetProxiedDamageReceiver(v.GetComponent<DamageReceiver>());
			return labor.BreakObject(component.GetComponent<GameComponent>());
		}
		foreach (IVolume item in VolumeRegistry.VolumesWithPropertyAt(position, VProperty.occupy))
		{
			if (item is MovableVolume)
			{
				return labor.BreakObject(item.GameObject.GetComponent<GameComponent>());
			}
		}
		List<XYZ> list = CollectionPool.RequestList<XYZ>();
		foreach (XYZ @case in base.volume.GetCases(VProperty.fantome))
		{
			list.Add(@case);
		}
		if (list.Count <= 1)
		{
			list.Dispose();
			return labor.Dig(Location.Of(this));
		}
		XYZ p = list[0];
		XYZ p2 = list[1];
		list.Dispose();
		return (StaticNode)Node.Trier.Do((Labor l, XYZ position2) => l.Dig(Location.Of(position2)), labor, p).Do((Labor l, XYZ position2) => l.Dig(Location.Of(position2)), labor, p2).Do((Labor l, BuildGoal self) => l.Dig(Location.Of(self)), labor, this);
	}

	public override void ConnectProject(Project project)
	{
		base.ConnectProject(project);
		if (!(project == null) && !(project.ProjectData == null))
		{
			if (project.ProjectData.BlueprinRecepteurCompound != null && project.ProjectData.BlueprinRecepteurCompound.Recepteur != null)
			{
				_compoundRecepteur = project.ProjectData.BlueprinRecepteurCompound.Recepteur;
			}
			else
			{
				_compoundRecepteur = base.recepteur;
			}
		}
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
		error = null;
		return false;
	}

	public override int PriorityFor(Labor labor, ref float time)
	{
		int num = (HasOtherWorkerAssigned(labor) ? (-10) : 0);
		int num2 = HeightAdjustmentToPriority() * 10;
		int num3 = (AdjacentLower() ? (-100) : 0);
		return num2 + num3 + num - (int)Vector3.Distance(labor.transform.position, base.transform.position);
	}

	private bool ValuesAreValid<T, T2, T3>(T t, string tName, T2 t2, string t2Name, T3 t3, string t3Name) where T : BrixComponent where T2 : BrixComponent where T3 : BrixComponent
	{
		if (t.IsNullOrReleased())
		{
			logger.WarnFormat("{0} was null in buildGoal", tName);
			return false;
		}
		if (t2.IsNullOrReleased())
		{
			logger.WarnFormat("{0} was null in buildGoal", t2Name);
			return false;
		}
		if (t3.IsNullOrReleased())
		{
			logger.WarnFormat("{0} was null in buildGoal", t3Name);
			return false;
		}
		return true;
	}

	public override bool IsMatchFor(Labor labor, AIErrorsStore errorDestination, ref float time, ref bool unmappedFlag)
	{
		if (NatureRegistry.HasNatureBlockerAt(XYZ.FromVector3(base.transform.position)))
		{
			return false;
		}
		if (NullDetected(labor, out var error))
		{
			errorDestination.AddDecisionError(error, labor);
			return false;
		}
		if (IsAnti && HasOtherWorkerAssigned(labor))
		{
			errorDestination.AddDecisionError(SharedWorkException<Exceptions.GoalTooCrowded>.Get.Problem, labor);
			return false;
		}
		time += 0.01f;
		if (IsCaseBlueprint(labor))
		{
			errorDestination.AddDecisionError(Exceptions.PathExecutionFailed.Problem_, labor);
			return false;
		}
		time += 0.01f;
		if (!Blueprint.State.HasFlag(PlacementFlags.valid) || !Blueprint.State.HasFlag(PlacementFlags.buildable))
		{
			errorDestination.AddDecisionError(SharedWorkException<Exceptions.GoalUnavailable>.Get.Problem, labor);
			return false;
		}
		time += 0.01f;
		if (!CanReachWithKnowledge(labor))
		{
			if (labor.recepteur.IsEmpty() || IsAnti)
			{
				errorDestination.AddDecisionError(SharedWorkException<Exceptions.CantReach>.Get.Problem, labor);
			}
			return false;
		}
		if (IsAnti)
		{
			if (!labor.Occupation.Profession.CanBreakVoxels())
			{
				errorDestination.AddDecisionError(SharedWorkException<Exceptions.CharacterCantDig>.Get.Problem, labor);
				return false;
			}
			return true;
		}
		time += 0.01f;
		GameObject o = labor.Find(base.recepteur);
		if (labor.recepteur.HasRoomFor(o))
		{
			return true;
		}
		errorDestination.AddDecisionError(SharedWorkException<Exceptions.CannotFindWhatINeedToPickUp>.Get.Problem, labor);
		return false;
	}

	private bool CanReachWithKnowledge(Labor labor)
	{
		return Knowledge.Instance.CanReach(labor, this) || base.gameObject.GetComponent<FactoryImprint>().AssetKey.Name == Blocks.RopeBridgePlank.Name;
	}

	private bool IsCaseBlueprint(Labor labor)
	{
		return base.volume.HasPropertyAt(VProperty.fantome, labor.presence.CurrentVoxel) && PathNodeRegistry.GetPositionInReach(labor.presence.CurrentVoxel, (!labor.navigation.Locomotion.CanClimb) ? PathBranch.BranchType.StepDown : PathBranch.BranchType.Climbing).Slinq().All((XYZ i, IVolume v) => v.HasPropertyAt(VProperty.fantome, i), base.volume);
	}

	private int HeightAdjustmentToPriority()
	{
		int num = 128;
		if (!IgnoreHeightPriority)
		{
			num -= base.voxel.position.y;
		}
		return num;
	}

	private bool AdjacentLower()
	{
		return base.volume.access.Slinq().Where((XYZ aPosition, BuildGoal aBuildGoal) => aPosition.y < aBuildGoal._referenceY, this).Any((XYZ acces) => VolumeRegistry.HasVolumesWithPropertyAt(acces, VProperty.fantome));
	}

	public static int ComparePriority(BuildGoal goalA, BuildGoal goalB)
	{
		int value = goalA.HeightAdjustmentToPriority();
		int num = goalB.HeightAdjustmentToPriority().CompareTo(value);
		return (num == 0) ? goalA.CompareTo(goalB) : num;
	}
}
