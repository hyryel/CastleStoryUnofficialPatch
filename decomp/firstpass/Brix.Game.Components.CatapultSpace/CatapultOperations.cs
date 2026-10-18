using Brix.External.Signals;
using Brix.Game.AI;
using Brix.Game.AI.InstructionType;
using Brix.Game.AI.KnowledgeSpace;
using Brix.Game.AI.Nodes;
using Brix.Game.Components.Volumes;
using Brix.Game.Locomotion;
using Brix.Game.Semantique;
using Brix.UI.Icons;
using Motus.Behavior;
using UnityEngine;

namespace Brix.Game.Components.CatapultSpace;

public class CatapultOperations : GameComponent, IOperable<Labor>
{
	[SerializeField]
	private Catapult catapult;

	private OperableLocation _location;

	private int _operationAccessIndices;

	private Profile AmmoProfile = Adjectif.explosiveBarrel.Profile;

	private CatapultShootStatus ShootingStatus = CatapultShootStatus.HoldFire;

	public LaborInstruction UnloadAmmoInstuction;

	public LaborInstruction GoFetchAmmoInstruction;

	public LaborInstruction CrankAndAimInstruction;

	public LaborInstruction ShootInstruction;

	public IconKey Icon => (!FiringAtWill) ? IconKeys._Catapult : IconKeys._Catapult_AutoFire;

	public bool FiringAtWill => ShootingStatus == CatapultShootStatus.FireAtWill;

	public bool IsStillAlive => !catapult.IsNullOrReleased();

	public ProjectPriority PriorityWeights => ProjectPriority.Catapult;

	public ProjectPriority.Bias PriorityBias
	{
		get
		{
			return ProjectPriority.Bias.High;
		}
		set
		{
		}
	}

	public int MaximumCrewSize => 1;

	public Labor Operator
	{
		get
		{
			if (!catapult.control.ouvrier)
			{
				return null;
			}
			return catapult.control.ouvrier.GetComponent<Labor>();
		}
		set
		{
			if (value == null)
			{
				catapult.CesserDePiloter();
			}
			else
			{
				catapult.Piloter(value.gameObject);
			}
		}
	}

	public ConflictResolver ConflictResolver => null;

	protected override void Start()
	{
		base.Start();
		_location = new OperableLocation(this);
		_operationAccessIndices = base.volume.GetPropertyBitMaskSharedByOtherProperty(VProperty.access, VProperty.operation);
		UnloadAmmoInstuction = new LaborInstruction("UnloadCatapult", IconKeys.Missing, UnloadNode);
		GoFetchAmmoInstruction = new LaborInstruction("PickUpAndLoadCatapult", IconKeys.Missing, FetchAndLoadAmmo);
		CrankAndAimInstruction = new LaborInstruction("CrankAndAimCatapult", IconKeys.Missing, CrankAndAimAutonomously);
		ShootInstruction = new LaborInstruction("ShootFromCatapult", IconKeys.Missing, ShootAutonomously);
		ProjectDatabase.For(base.faction).Register(this);
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		Fire(Operator);
	}

	private void Update()
	{
		if (CatapultWantsToFire() && CatapultReadyToShoot())
		{
			catapult.control.Declancher();
		}
	}

	public void RequestFireOnce()
	{
		ShootingStatus = CatapultShootStatus.FireOnce;
	}

	public void RequestFireAtWill()
	{
		ShootingStatus = CatapultShootStatus.FireAtWill;
	}

	public void RequestHoldFire()
	{
		ShootingStatus = CatapultShootStatus.HoldFire;
	}

	public void ClearTarget()
	{
		catapult.Deprogrammer();
	}

	public bool CatapultHasAmmo()
	{
		return catapult.control.boulet;
	}

	public void OnShoot()
	{
		ShootingStatus = ShootingStatus.OnFire;
	}

	public bool CatapultHasTarget()
	{
		return catapult.cible != Vector3.zero;
	}

	private bool CatapultNeedsWork()
	{
		return CatapultHasAmmo() && CatapultHasTarget() && !CatapultIsOnTarget();
	}

	public bool CatapultWantsToFire()
	{
		return ShootingStatus.WantsToFire;
	}

	private bool CatapultReadyToShoot()
	{
		return CatapultHasAmmo() && CatapultIsOnTarget();
	}

	private Node UnloadNode(Labor labor)
	{
		return Node.Selector.If((CatapultOperations self, Labor l) => self.CanBeUnloadedBy(l), this, labor).Do((CatapultOperations self, Labor l) => l.RunToExactly(self._location), this, labor).Do(delegate(CatapultOperations self, Labor l)
		{
			self.Desarmer(l);
		}, this, labor)
			.Else()
			.Do(Node.WaitForTicks(100))
			.FailWhen(SharedWorkException<Exceptions.NullTarget>.Get)
			.Condition(GameComponentExtender.IsNullOrReleased, this)
			.ToNode;
	}

	private void Desarmer(Labor labor)
	{
		if (labor.IsOwnedByMe)
		{
			labor.recepteur.AddObject(catapult.control.boulet);
			catapult.Desarmer();
		}
	}

	private Node FetchAndLoadAmmo(Labor labor)
	{
		return Node.Sequence.Do((CatapultOperations self, Labor l) => l.FindAndPickUp(self.AmmoProfile), this, labor).Do((CatapultOperations self, Labor l) => l.RunToExactly(self._location), this, labor).Do((CatapultOperations self, Labor l) => self.Armer(l), this, labor)
			.FailWhen(SharedWorkException<Exceptions.NullTarget>.Get)
			.Condition(GameComponentExtender.IsNullOrReleased, this)
			.ToNode;
	}

	private Node Armer(Labor labor)
	{
		if (labor.recepteur.FirstObjectOf(Adjectif.explosiveBarrel, out var go))
		{
			catapult.Armer(go);
			return Node.Empty;
		}
		return Node.Fail(SharedWorkException<Exceptions.ObjectDoesNotFitInDestination>.Get);
	}

	private Node CrankAndAim(Labor labor)
	{
		return Node.Sequence.Do((CatapultOperations self, Labor l) => l.RunToExactly(self._location), this, labor).Do((CatapultOperations self, Labor l) => self.Hire(l), this, labor).Do((CatapultOperations self, Labor l) => AnimationNodes.Loop(Node.Empty, Working.WorkStates.Catapult, self.transform, l), this, labor)
			.Do(Node.WaitForCondition((CatapultOperations self) => self.CatapultIsOnTarget(), this))
			.ToNode.FailWhen(SharedWorkException<Exceptions.NullTarget>.Get).Condition(GameComponentExtender.IsNullOrReleased, this).ToNode;
	}

	private Node Shoot(Labor labor)
	{
		return Node.Sequence.Do((CatapultOperations self, Labor l) => l.RunToExactly(self._location), this, labor).Do((CatapultOperations self, Labor l) => self.Hire(l), this, labor).Do(delegate(Catapult c)
		{
			c.control.Declancher();
		}, catapult)
			.Do(Node.WaitForCondition((CatapultOperations self) => !self.CatapultHasAmmo(), this))
			.ToNode.FailWhen(SharedWorkException<Exceptions.NullTarget>.Get).Condition((CatapultOperations self) => self.IsNullOrReleased() || !self.catapult.PeutDeclancher(), this).ToNode;
	}

	private Node CrankAndAimAutonomously(Labor labor)
	{
		return Node.Empty;
	}

	private Node ShootAutonomously(Labor labor)
	{
		return Node.Empty;
	}

	private OperatorActions DecideWhatToDo(Labor labor)
	{
		if (catapult.IsNullOrReleased() || !catapult.PeutOperer(labor) || !Knowledge.Instance.CanReach(labor, this, _operationAccessIndices))
		{
			return OperatorActions.Nothing;
		}
		if (!CatapultHasAmmo() && labor.recepteur.HasRoomFor(labor.Find(AmmoProfile)))
		{
			return OperatorActions.Load;
		}
		if (CanBeWorkedBy(labor))
		{
			return OperatorActions.CrankAndAim;
		}
		if (CanBeShotBy(labor) && ShootingStatus.WantsToFire)
		{
			return OperatorActions.Shoooot;
		}
		return OperatorActions.Nothing;
	}

	public OrderPackage<Labor> ContextualActionFor(Labor labor)
	{
		if (CanBeShotBy(labor))
		{
			return Order.Package.Of(ShootInstruction, Labor.DecisionSource.User);
		}
		if (CanBeWorkedBy(labor))
		{
			return Order.Package.Of(CrankAndAimInstruction, Labor.DecisionSource.User);
		}
		if (CanBeLoadedBy(labor))
		{
			return Order.Package.Of(GoFetchAmmoInstruction, Labor.DecisionSource.User);
		}
		if (CanBeUnloadedBy(labor))
		{
			return Order.Package.Of(UnloadAmmoInstuction, Labor.DecisionSource.User);
		}
		return OrderPackage<Labor>.Empty;
	}

	public bool CanBeLoadedBy(Labor labor)
	{
		return (bool)labor.recepteur.FirstObjectOf(Adjectif.explosiveBarrel) && !CatapultHasAmmo();
	}

	public bool CanBeUnloadedBy(Labor labor)
	{
		return CatapultHasAmmo() && labor.recepteur.HasRoomFor(catapult.boulet);
	}

	public bool CanBeWorkedBy(Labor labor)
	{
		return CatapultNeedsWork() && !labor.recepteur.RequiresBothHands();
	}

	public bool CanBeShotBy(Labor labor)
	{
		return CatapultReadyToShoot() && !labor.recepteur.RequiresBothHands();
	}

	private bool CatapultIsOnTarget()
	{
		return GetComponent<CatapultBody>().OnTarget();
	}

	private OrderPackage<Labor> PackageInstruction(OperatorActions action)
	{
		OrderPackage<Labor> orderPackage = OrderPackage<Labor>.Empty;
		switch (action)
		{
		case OperatorActions.Load:
			orderPackage = Order.Package.Of(GoFetchAmmoInstruction, Labor.DecisionSource.Project);
			orderPackage.SetPrepare((BaseLabor l) => Hire(l as Labor));
			break;
		case OperatorActions.CrankAndAim:
			orderPackage = Order.Package.Of(CrankAndAimInstruction, Labor.DecisionSource.Project);
			orderPackage.SetPrepare((BaseLabor l) => Hire(l as Labor));
			break;
		case OperatorActions.Shoooot:
			orderPackage = Order.Package.Of(ShootInstruction, Labor.DecisionSource.Project);
			orderPackage.SetPrepare((BaseLabor l) => Hire(l as Labor));
			break;
		}
		return orderPackage;
	}

	public Node GetInstruction(Labor labor, Var<OrderPackage<Labor>> result)
	{
		return (StaticNode)Node.Sequence.Do(delegate(CatapultOperations self, Labor _labor, Var<OrderPackage<Labor>> _result)
		{
			self._GetInstruction(_labor, _result);
		}, this, labor, result);
	}

	private void _GetInstruction(Labor labor, Var<OrderPackage<Labor>> result)
	{
		OperatorActions action = DecideWhatToDo(labor);
		OrderPackage<Labor> x = PackageInstruction(action);
		result.Set(x);
	}

	public Node VerifyWorkFor(Labor labor)
	{
		return (DecideWhatToDo(labor) == OperatorActions.Nothing) ? Node.FailWork<Exceptions.ProjectWithoutGoal>() : Node.Empty;
	}

	public bool HasWorkFor(Labor labor)
	{
		if (Operator != null && Operator != labor)
		{
			return false;
		}
		return true;
	}

	public ProjectPriority PriorityInput(Labor labor)
	{
		return new ProjectPriority
		{
			bias = 1f,
			crewSize = CrewCount(),
			globalCrewSize = GlobalCrewCount(),
			remainingCrewSpace = MaximumCrewSize - CrewCount(),
			distance = Project.DistanceIncrement(this, labor),
			goalCount = 1f,
			isCurrent = ((Operator == labor) ? 1 : 0),
			deathCount = 0f,
			manualBias = (float)PriorityBias
		};
	}

	public int CrewCount()
	{
		return (Operator != null) ? 1 : 0;
	}

	public int GlobalCrewCount()
	{
		return ProjectDatabase.For(base.faction).WorkerInTaskSimilarTasksTo(this);
	}

	public bool Hire(Labor labor)
	{
		return IOperableExtender.Hire(this, labor);
	}

	public void Fire(Labor labor)
	{
		IOperableExtender.Fire(this, labor);
	}

	IReadOnlySignal<GameObject> IOperable<Labor>.get_OnReleaseOrDestroySignal()
	{
		return base.OnReleaseOrDestroySignal;
	}

	GameObject IOperable<Labor>.get_gameObject()
	{
		return base.gameObject;
	}
}
