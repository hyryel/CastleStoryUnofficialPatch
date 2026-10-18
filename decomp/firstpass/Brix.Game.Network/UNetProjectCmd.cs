using System.Collections.Generic;
using Brix.Audio;
using Brix.Engine;
using Brix.Engine.Abstract;
using Brix.Engine.Nature;
using Brix.External.Factories;
using Brix.Game.AI;
using Brix.Game.AI.InstructionType;
using Brix.Game.Beacons;
using Brix.Game.Components;
using Brix.Game.Components.Tunnel;
using Brix.Game.Semantique;
using Brix.Lifecycle.Pooling;
using Brix.Transactions;
using Brix.Utils;
using Brix.Utils.Factories;
using Smooth.Algebraics;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Network;

public class UNetProjectCmd : NetworkBehaviour
{
	private static int kCmdCmdChangeProject;

	private static int kCmdCmdAccepterPosition;

	private static int kCmdCmdAccepterRayon;

	private static int kCmdCmdTransformLog;

	private static int kCmdCmdAddMoveToGoal;

	private static int kCmdCmdSetAutonomy;

	private static int kCmdCmdAddMoveToVolumeGoal;

	private static int kCmdCmdAddEmergencyRecallGoal;

	private static int kCmdCmdAddCallToArmsGoal;

	private static int kCmdCmdAddBackToWorkGoal;

	private static int kCmdCmdGoDetonate;

	private static int kCmdCmdAddCleanGoal;

	private static int kCmdCmdAddTreeGoal;

	private static int kCmdCmdAddNatureHarvestGoal;

	private static int kCmdCmdGoDropAt;

	private static int kCmdCmdGoStoreInto;

	private static int kCmdCmdAddSmartTransferGoal;

	private static int kCmdCmdRemoveAllLabor;

	private static int kCmdCmdAddBreakVoxelGoal;

	private static int kCmdCmdRepair;

	private static int kCmdCmdBreakObject;

	private static int kCmdCmdAttack;

	private static int kCmdCmdMoveAttack;

	private static int kCmdCmdArmBearTrap;

	private static int kCmdCmdPickup;

	private static int kRpcRpcAddHighlightOnPosition;

	private static int kRpcRpcRemoveHighlightOnPosition;

	private static int kCmdCmdSpawnFireflyNest;

	private static int kCmdCmdSpawn;

	private static int kCmdCmdSpawnWithLevel;

	private static int kCmdCmdSpawnCrossbowTron;

	private static int kCmdCmdSpawnAlchemist;

	private static int kCmdCmdSpawnGeomancer;

	private static int kCmdCmdSpawnHalbardier;

	private static int kRpcRpcPlaySound;

	private static int kCmdCmdInstantiateTunnelUnit;

	private static int kCmdCmdRemoveTunnelUnit;

	private static int kCmdCmdCraftingAssignWorker;

	private static int kCmdCmdHarvestToggleResource;

	private static int kCmdCmdHarvestSetResource;

	private static int kCmdCmdQuarryToggleResource;

	private static int kCmdCmdTunnelToggleResource;

	private static int kCmdCmdCraftingRemoveResources;

	private static int kCmdCmdCraftingAddResources;

	private static int kCmdCmdCraftingRemoveProduction;

	private static int kCmdCmdCraftingWorkAt;

	private static int kCmdCmdcraftingQueueRecipe;

	private static int kCmdCmdcraftingProduce;

	private static int kCmdCmdcraftingDoNothing;

	private static int kCmdCmdcraftingLoop;

	private static int kCmdCmdcraftingRemoveAt;

	private static int kCmdCmdStockpileKeepEmpty;

	private static int kCmdCmdStockpileAcceptAll;

	private static int kCmdCmdStockpileAcceptOnly;

	private static int kRpcRpcMakeExclusive;

	[Command]
	public void CmdChangeProject(GameObject laborGO, GameObject projectGO)
	{
		Labor labor = ((!laborGO.IsNullOrReleased()) ? laborGO.GetComponent<Labor>() : null);
		if (labor.IsNullOrReleased())
		{
			return;
		}
		Project project = ((!projectGO.IsNullOrReleased()) ? projectGO.GetComponent<Project>() : null);
		if (!(labor.faction == GetComponent<User>().faction) && !HandleRaidProject(projectGO))
		{
			return;
		}
		if (project == null || project is IdleProject)
		{
			labor.AssignToIdle();
			labor.Autonomy = AutonomyStatus.FreeAgent;
			return;
		}
		if (project.IsFull())
		{
			project.MaximumCrewSize++;
		}
		if (project.Hire(labor))
		{
			labor.Autonomy = AutonomyStatus.LockedToOperable;
		}
		else
		{
			labor.Autonomy = AutonomyStatus.InstinctOnly;
		}
	}

	private bool HandleRaidProject(GameObject projectGO)
	{
		if (projectGO.IsNullOrReleased())
		{
			return false;
		}
		RaidProject component = projectGO.GetComponent<RaidProject>();
		return (bool)component && component.IsCorruptrons && GetComponent<User>().isServer;
	}

	[Command]
	public void CmdAccepterPosition(GameObject beaconGO, int x, int y, int z)
	{
		if (Affiliation.IsOwnedBy(beaconGO, base.gameObject))
		{
			XYZ position = new XYZ(x, y, z);
			BeaconDriver component = beaconGO.GetComponent<BeaconDriver>();
			component.Beacon.position = position;
			component.NetworkNetworkPosition = position.ToVector3();
		}
	}

	[Command]
	public void CmdAccepterRayon(GameObject beaconGO, int radius)
	{
		if (Affiliation.IsOwnedBy(beaconGO, base.gameObject))
		{
			BeaconDriver component = beaconGO.GetComponent<BeaconDriver>();
			component.Beacon.radius = radius;
			component.NetworkNetworkRadius = radius;
		}
	}

	[Command]
	public void CmdTransformLog(GameObject instance, GameObject objectToTransform)
	{
		if (Affiliation.IsOwnedBy(instance, base.gameObject))
		{
			Labor component = instance.GetComponent<Labor>();
			Order.ForLabor.TransformLog.Send(component, objectToTransform, Labor.DecisionSource.User);
		}
	}

	[Command]
	public void CmdAddMoveToGoal(GameObject laborGO, Vector3 position, bool drop, bool queued)
	{
		if (Affiliation.IsOwnedBy(laborGO, base.gameObject))
		{
			Labor component = laborGO.GetComponent<Labor>();
			drop |= DirectOrderProject.CheckLeftShift();
			LaborInstruction<Location> laborInstruction = ((!drop) ? Order.Move.RunToward : Order.Transfer.GoDropAt);
			Location location = ((!drop) ? Location.Of(position) : Location.LeadingTo(position));
			if (queued)
			{
				Order.Package.OfLocation(laborInstruction, location, Labor.DecisionSource.User).Queue(component);
			}
			else
			{
				laborInstruction.Send(component, location, Labor.DecisionSource.User);
			}
		}
	}

	[Command]
	public void CmdSetAutonomy(GameObject laborGO, int autonomyId)
	{
		if (Affiliation.IsOwnedBy(laborGO, base.gameObject))
		{
			Labor component = laborGO.GetComponent<Labor>();
			component.Autonomy = AutonomyStatus.FromId((AutonomyStatus.Ids)autonomyId);
			if (component.Occupation.CurrentOccupation == Occupation.Type.Builder)
			{
				Order.Move.HaltWhatever.Send(component);
			}
		}
	}

	[Command]
	public void CmdAddMoveToVolumeGoal(GameObject laborGO, GameObject toObjectGO, bool queued)
	{
		if (Affiliation.IsOwnedBy(laborGO, base.gameObject))
		{
			Labor component = laborGO.GetComponent<Labor>();
			SoundEngine.Play(component.Sounds.Go, component.gameObject);
			if (queued)
			{
				Order.Package.OfGameObject(Order.Move.MoveToVolume, toObjectGO, Labor.DecisionSource.User).Queue(component);
			}
			else
			{
				Order.Move.MoveToVolume.Send(component, toObjectGO, Labor.DecisionSource.User);
			}
		}
	}

	[Command]
	public void CmdAddEmergencyRecallGoal(GameObject laborGO, GameObject toObjectGO)
	{
		if (Affiliation.IsOwnedBy(laborGO, base.gameObject))
		{
			Labor component = laborGO.GetComponent<Labor>();
			Order.Move.MoveToVolumeAndHold.Send(component, toObjectGO, Labor.DecisionSource.User);
		}
	}

	[Command]
	public void CmdAddCallToArmsGoal(GameObject laborGO, GameObject toObjectGO)
	{
		if (Affiliation.IsOwnedBy(laborGO, base.gameObject))
		{
			Labor component = laborGO.GetComponent<Labor>();
			Order.AboutTools.FindWeponAndRegroup.Send(component, toObjectGO, Labor.DecisionSource.User);
		}
	}

	[Command]
	public void CmdAddBackToWorkGoal(GameObject laborGO)
	{
		if (Affiliation.IsOwnedBy(laborGO, base.gameObject))
		{
			Labor component = laborGO.GetComponent<Labor>();
			Order.AboutTools.StoreWeapons.Send(component, Labor.DecisionSource.User);
		}
	}

	[Command]
	internal void CmdGoDetonate(GameObject laborGO, GameObject barelGO)
	{
		if (Affiliation.IsOwnedBy(laborGO, base.gameObject))
		{
			Labor component = laborGO.GetComponent<Labor>();
			GameComponent component2 = barelGO.GetComponent<GameComponent>();
			Order.ForLabor.Detonate.Send(component, component2, Labor.DecisionSource.User);
		}
	}

	[Command]
	public void CmdAddCleanGoal(GameObject laborGO, Vector3 position, GameObject onlyObjectsLikeThisOne, bool queued)
	{
		if (!Affiliation.IsOwnedBy(laborGO, base.gameObject))
		{
			return;
		}
		Labor component = laborGO.GetComponent<Labor>();
		if ((bool)onlyObjectsLikeThisOne)
		{
			if (queued)
			{
				Order.Package.OfGameObject(Order.ForLabor.CleanUpSimilarObjectsAround, onlyObjectsLikeThisOne, Labor.DecisionSource.User).Queue(component);
			}
			else
			{
				Order.ForLabor.CleanUpSimilarObjectsAround.Send(component, onlyObjectsLikeThisOne, Labor.DecisionSource.User);
			}
		}
		else if (queued)
		{
			Order.Package.OfLocation(Order.ForLabor.CleanUpAnythingAround, Location.Of(position), Labor.DecisionSource.User).Queue(component);
		}
		else
		{
			Order.ForLabor.CleanUpAnythingAround.Send(component, Location.Of(position), Labor.DecisionSource.User);
		}
	}

	[Command]
	public void CmdAddTreeGoal(GameObject laborGO, Vector3 position, bool queued)
	{
		if (!Affiliation.IsOwnedBy(laborGO, base.gameObject))
		{
			return;
		}
		Labor component = laborGO.GetComponent<Labor>();
		if (!component.Occupation.Profession.CanChopTrees())
		{
			DirectOrderProject.AddMoveToGoal(component, position);
			return;
		}
		SoundEngine.Play(component.Sounds.ActionLumber, component.gameObject);
		if (queued)
		{
			Order.Package.OfLocation(Order.ForLabor.GoCutNewTree, Location.Of(position), Labor.DecisionSource.User).Queue(component);
		}
		else
		{
			Order.ForLabor.GoCutNewTree.Send(component, Location.Of(position), Labor.DecisionSource.User);
		}
	}

	[Command]
	public void CmdAddNatureHarvestGoal(GameObject laborGO, int index)
	{
		if (Affiliation.IsOwnedBy(laborGO, base.gameObject))
		{
			Labor component = laborGO.GetComponent<Labor>();
			NatureData natureData = NatureRegistry.NatureAt(XYZ.FromScalaire(index));
			if (natureData.IsValid)
			{
				Order.ForLabor.HarvestNatureAt.Send(component, natureData.GoalPosition, Labor.DecisionSource.User);
			}
		}
	}

	[Command]
	public void CmdGoDropAt(GameObject laborGO, Vector3 position)
	{
		if (Affiliation.IsOwnedBy(laborGO, base.gameObject))
		{
			Labor component = laborGO.GetComponent<Labor>();
			Order.Transfer.GoDropAt.Send(component, Location.LeadingTo(position), Labor.DecisionSource.User);
		}
	}

	[Command]
	public void CmdGoStoreInto(GameObject laborGO, GameObject recepteurGO)
	{
		if (Affiliation.IsOwnedBy(laborGO, base.gameObject))
		{
			Labor component = laborGO.GetComponent<Labor>();
			Order.Transfer.GoStoreInto.Send(component, recepteurGO.GetComponent<GameComponent>(), Labor.DecisionSource.User);
		}
	}

	[Command]
	public void CmdAddSmartTransferGoal(GameObject laborGO, GameObject target, bool queued)
	{
		if (Affiliation.IsOwnedBy(laborGO, base.gameObject))
		{
			Labor component = laborGO.GetComponent<Labor>();
			if (queued)
			{
				Order.Package.OfGameObject(Order.Transfer.SmartTransfer, target, Labor.DecisionSource.User).Queue(component);
			}
			else
			{
				Order.Transfer.SmartTransfer.Send(component, target, Labor.DecisionSource.User);
			}
		}
	}

	[Command]
	public void CmdRemoveAllLabor(GameObject GroupeGO)
	{
		if (GroupeGO.IsNullOrReleased() || !Affiliation.IsOwnedBy(GroupeGO, base.gameObject))
		{
			return;
		}
		List<Labor> list = CollectionPool.RequestList<Labor>();
		GroupeGO.GetComponent<Project>().Crew().Slinq()
			.AddTo(list);
		foreach (Labor item in list)
		{
			item.AbortWorkSignal.Invoke(TaskResult.ProjectCancelledTask);
			item.AssignToIdle();
		}
		list.Dispose();
	}

	[Command]
	public void CmdAddBreakVoxelGoal(GameObject laborGO, int index)
	{
		if (Affiliation.IsOwnedBy(laborGO, base.gameObject))
		{
			Labor component = laborGO.GetComponent<Labor>();
			XYZ position = XYZ.FromScalaire(index);
			if (Brix.Engine.Voxel.IsEmpty(position))
			{
				position -= XYZ.up;
			}
			Order.ForLabor.GoDig.Send(component, new SpaceLocation(position.ToVector3(), Quaternion.identity), Labor.DecisionSource.User);
		}
	}

	[Command]
	public void CmdRepair(GameObject instance, int index)
	{
		if (Affiliation.IsOwnedBy(instance, base.gameObject))
		{
			Labor component = instance.GetComponent<Labor>();
			Order.ForLabor.Repair.Send(component, new SpaceLocation(Brix.Engine.Abstract.Voxel.GetVecteur(index), Quaternion.identity), Labor.DecisionSource.User);
		}
	}

	[Command]
	public void CmdBreakObject(GameObject laborGO, GameObject go)
	{
		if (Affiliation.IsOwnedBy(laborGO, base.gameObject))
		{
			Labor component = laborGO.GetComponent<Labor>();
			Order.Combat.BreakObject.Send(component, go.GetComponent<GameComponent>(), Labor.DecisionSource.User);
		}
	}

	[Command]
	public void CmdAttack(GameObject laborGO, GameObject go)
	{
		if (Affiliation.IsOwnedBy(laborGO, base.gameObject))
		{
			Labor component = laborGO.GetComponent<Labor>();
			Order.Combat.GoAttackDirectly.Send(component, go, Labor.DecisionSource.User);
		}
	}

	[Command]
	public void CmdMoveAttack(GameObject laborGO, Vector3 destination)
	{
		if (Affiliation.IsOwnedBy(laborGO, base.gameObject))
		{
			Labor component = laborGO.GetComponent<Labor>();
			if (component.toolbag.HasTool(Accessories.Staff))
			{
				component.Autonomy = AutonomyStatus.InstinctOnly;
			}
			Order.Combat.MoveAttack.Send(component, new SpaceLocation(destination), Labor.DecisionSource.User);
		}
	}

	[Command]
	public void CmdArmBearTrap(GameObject laborGO, GameObject bearTrapGO)
	{
		if (Affiliation.IsOwnedBy(laborGO, base.gameObject))
		{
			Labor component = laborGO.GetComponent<Labor>();
			Var<OrderPackage<Labor>> var = new Var<OrderPackage<Labor>>();
			bearTrapGO.GetComponent<ObjectActivator>().GetInstruction(component, var);
			var.Get().Send(component);
		}
	}

	[Command]
	public void CmdPickup(GameObject laborGO, GameObject objectToPickupFrom)
	{
		if (Affiliation.IsOwnedBy(laborGO, base.gameObject))
		{
			Labor component = laborGO.GetComponent<Labor>();
			Order.Transfer.GoPickUpFrom.Send(component, objectToPickupFrom, Labor.DecisionSource.User);
		}
	}

	[ClientRpc]
	public void RpcAddHighlightOnPosition(int x, int y, int z)
	{
		XYZ pos = new XYZ(x, y, z);
		Highlighter.highlight.At(pos, Color.yellow);
	}

	[ClientRpc]
	public void RpcRemoveHighlightOnPosition(int x, int y, int z)
	{
		XYZ pos = new XYZ(x, y, z);
		Highlighter.remove.At(pos);
	}

	[Command]
	public void CmdSpawnFireflyNest(GameObject spawner, Occupation.Type occupation)
	{
		spawner.GetComponentInChildren<FireflyNest>().Hack_AssignJobToNamedFirefly(occupation);
	}

	[Command]
	public void CmdSpawn(string factory, string name, Vector3 position, Quaternion rotation, GameObject factionGO, Occupation.Type occupation, bool equipGear)
	{
		SpawnUnit(new Factory.AssetKey(factory, name), position, rotation, factionGO, occupation, equipGear);
	}

	[Command]
	public void CmdSpawnWithLevel(string factory, string name, Vector3 position, Quaternion rotation, GameObject factionGO, Occupation.Type occupation, bool equipGear, int level)
	{
		GameObject gameObject = SpawnUnit(new Factory.AssetKey(factory, name), position, rotation, factionGO, occupation, equipGear);
		Profession component = gameObject.GetComponent<Profession>();
		if (!component.IsNullOrReleased())
		{
			component.CharacterLevel = level;
		}
	}

	[Command]
	public void CmdSpawnCrossbowTron(string factory, string name, Vector3 position, Quaternion rotation, GameObject factionGO)
	{
		GameObject gameObject = SpawnUnit(new Factory.AssetKey(factory, name), position, rotation, factionGO, Occupation.Type.Archer, equipGear: false);
		Toolbag component = gameObject.GetComponent<Toolbag>();
		component.EquipTool(Accessories.Crossbow);
		component.EquipTool(Accessories.Pouch);
		component.EquipTool(Accessories.Kepi);
	}

	[Command]
	public void CmdSpawnAlchemist(string factory, string name, Vector3 position, Quaternion rotation, GameObject factionGO)
	{
		GameObject gameObject = SpawnUnit(new Factory.AssetKey(factory, name), position, rotation, factionGO, Occupation.Type.Archer, equipGear: false);
		Toolbag component = gameObject.GetComponent<Toolbag>();
		component.EquipTool(Accessories.Backpack);
		component.EquipTool(Accessories.Bonnet);
	}

	[Command]
	public void CmdSpawnGeomancer(string factory, string name, Vector3 position, Quaternion rotation, GameObject factionGO)
	{
		GameObject gameObject = SpawnUnit(new Factory.AssetKey(factory, name), position, rotation, factionGO, Occupation.Type.Archer, equipGear: false);
		Toolbag component = gameObject.GetComponent<Toolbag>();
		component.EquipTool(Accessories.Staff);
		component.EquipTool(Accessories.Sachel);
		component.EquipTool(Accessories.ArtificerHat);
	}

	[Command]
	public void CmdSpawnHalbardier(string factory, string name, Vector3 position, Quaternion rotation, GameObject factionGO)
	{
		GameObject gameObject = SpawnUnit(new Factory.AssetKey(factory, name), position, rotation, factionGO, Occupation.Type.Archer, equipGear: false);
		Toolbag component = gameObject.GetComponent<Toolbag>();
		component.EquipTool(Accessories.Poleaxe);
		component.EquipTool(Accessories.KettleHat);
	}

	private GameObject SpawnUnit(Factory.AssetKey assetkey, Vector3 position, Quaternion rotation, GameObject factionGO, Occupation.Type occupation, bool equipGear)
	{
		GameObject gameObject = Transactor.SpawnUNet(assetkey, factionGO.GetComponent<Faction>(), position, rotation);
		gameObject.GetComponent<Occupation>().SetOccupation(occupation, equipGear);
		return gameObject;
	}

	[ClientRpc]
	public void RpcPlaySound(GameObject instance, string Sound)
	{
		if (instance != null)
		{
			SoundEngine.Play(Sound, instance);
		}
	}

	[Command]
	public void CmdInstantiateTunnelUnit(GameObject TunnelProject, Vector3 vpos, Vector3 forward, TunnelType tunnelType)
	{
		if (TunnelProject != null)
		{
			TunnelVolume tunnelVolume = TunnelProject.GetComponentsInChildren<TunnelVolume>(includeInactive: true).Slinq().First();
			ObjectPoolSingleton.RequestComponent<TunnelUnit>(out var t, VolumesUI.TunnelUnit);
			t.tunnel = tunnelVolume;
			TunnelUI component = TunnelProject.GetComponent<TunnelUI>();
			t.GetComponent<TunnelUnitUI>().SetUI(component);
			t.transform.parent = tunnelVolume.unitsParent;
			t.GetComponent<TunnelUnitUI>().tunnelUI = TunnelProject.GetComponent<TunnelUI>();
			t.Position = tunnelVolume.PositionKey(XYZ.FromVector3(vpos));
			t.gameObject.SetActive(value: true);
			t.InitDirections();
			if (forward == -base.transform.forward)
			{
				t.reversed = -1;
			}
			tunnelVolume.Register(t);
			TunnelProject.GetComponent<TunnelNetworkDeserialization>().UnitsPositionsNetwork.Add(vpos);
		}
	}

	[Command]
	public void CmdRemoveTunnelUnit(GameObject instance, Vector3 position)
	{
		if (Affiliation.IsOwnedBy(instance, base.gameObject))
		{
			XYZ pos = XYZ.FromVector3(position);
			TunnelVolume tunnelVolume = instance.GetComponentsInChildren<TunnelVolume>(includeInactive: true).Slinq().First();
			if (tunnelVolume.IsOccupied(pos))
			{
				tunnelVolume.Remove(tunnelVolume.UnitAt(pos));
			}
		}
	}

	[Command]
	public void CmdCraftingAssignWorker(GameObject craftingGO, GameObject laborGo)
	{
		if (Affiliation.IsOwnedBy(craftingGO, base.gameObject))
		{
			CraftingStation component = craftingGO.GetComponent<CraftingStation>();
			Labor component2 = laborGo.GetComponent<Labor>();
			component.Hire(component2);
			component2.Autonomy = AutonomyStatus.LockedToOperable;
		}
	}

	[Command]
	public void CmdHarvestToggleResource(GameObject projectGo, int type)
	{
		if (Affiliation.IsOwnedBy(projectGo, base.gameObject))
		{
			projectGo.WithComponent(delegate(HarvestProject p)
			{
				p.ToggleResource((NatureType)type);
			});
		}
	}

	[Command]
	public void CmdHarvestSetResource(GameObject projectGo, int type)
	{
		if (Affiliation.IsOwnedBy(projectGo, base.gameObject))
		{
			projectGo.WithComponent(delegate(HarvestProject p)
			{
				p.SetResource((NatureType)type);
			});
		}
	}

	[Command]
	public void CmdQuarryToggleResource(GameObject projectGo, int type)
	{
		if (Affiliation.IsOwnedBy(projectGo, base.gameObject))
		{
			projectGo.WithComponent(delegate(MineProject p)
			{
				p.ToggleResource((GroundResource)type);
			});
			projectGo.WithComponent(delegate(MineProject p)
			{
				p.RebuildDigGoalNodes();
			});
		}
	}

	[Command]
	public void CmdTunnelToggleResource(GameObject projectGo, int type)
	{
		if (Affiliation.IsOwnedBy(projectGo, base.gameObject))
		{
			projectGo.WithComponent(delegate(TunnelProject p)
			{
				p.ToggleResource((GroundResource)type);
			});
			projectGo.WithComponent(delegate(TunnelProject p)
			{
				p.RebuildDigGoalNodes();
			});
		}
	}

	[Command]
	public void CmdCraftingRemoveResources(GameObject craftingGO, GameObject laborGo)
	{
		if (Affiliation.IsOwnedBy(craftingGO, base.gameObject))
		{
			CraftingStation component = craftingGO.GetComponent<CraftingStation>();
			Labor component2 = laborGo.GetComponent<Labor>();
			if (component.ResourceToPickup(out var go))
			{
				Order.Transfer.GoPickUp.Send(component2, Tuple.Create(go, -1), Labor.DecisionSource.User);
			}
		}
	}

	[Command]
	public void CmdCraftingAddResources(GameObject craftingGO, GameObject laborGo)
	{
		if (Affiliation.IsOwnedBy(craftingGO, base.gameObject))
		{
			GameComponent component = craftingGO.GetComponent<GameComponent>();
			Labor component2 = laborGo.GetComponent<Labor>();
			Order.Transfer.GoStoreInto.Send(component2, component, Labor.DecisionSource.User);
		}
	}

	[Command]
	public void CmdCraftingRemoveProduction(GameObject craftingGO, GameObject laborGo)
	{
		if (Affiliation.IsOwnedBy(craftingGO, base.gameObject))
		{
			CraftingStation component = craftingGO.GetComponent<CraftingStation>();
			Labor component2 = laborGo.GetComponent<Labor>();
			if (component.ProductionToPickup(out var go))
			{
				Order.Transfer.GoPickUp.Send(component2, Tuple.Create(go, -1), Labor.DecisionSource.User);
			}
		}
	}

	[Command]
	public void CmdCraftingWorkAt(GameObject craftingGO, GameObject laborGo)
	{
		if (Affiliation.IsOwnedBy(craftingGO, base.gameObject))
		{
			CraftingStation component = craftingGO.GetComponent<CraftingStation>();
			Labor component2 = laborGo.GetComponent<Labor>();
			component.GetPackage(CraftingStation.Actions.Work).Send(component2);
		}
	}

	[Command]
	public void CmdcraftingQueueRecipe(GameObject craftingGO, int recipeId, int qty)
	{
		if (Affiliation.IsOwnedBy(craftingGO, base.gameObject))
		{
			RecipeInfo byId = CookBook.GetById(recipeId);
			craftingGO.WithComponent(delegate(CraftingLabor f, RecipeInfo _r)
			{
				f.QueueRecipe(_r);
			}, byId);
		}
	}

	[Command]
	public void CmdcraftingProduce(GameObject craftingGO, int recipeId)
	{
		if (Affiliation.IsOwnedBy(craftingGO, base.gameObject))
		{
			RecipeInfo byId = CookBook.GetById(recipeId);
			craftingGO.WithComponent(delegate(CraftingLabor f, RecipeInfo _r)
			{
				f.ProduceRecipe(_r);
			}, byId);
		}
	}

	[Command]
	public void CmdcraftingDoNothing(GameObject craftingGO)
	{
		if (Affiliation.IsOwnedBy(craftingGO, base.gameObject))
		{
			craftingGO.WithComponent(delegate(CraftingLabor f)
			{
				f.ClearOrderQueue();
			});
		}
	}

	[Command]
	public void CmdcraftingLoop(GameObject craftingGO, bool value)
	{
		if (Affiliation.IsOwnedBy(craftingGO, base.gameObject))
		{
			craftingGO.WithComponent(delegate(CraftingLabor f)
			{
				f.LoopRecipe = value;
			});
		}
	}

	[Command]
	public void CmdcraftingRemoveAt(GameObject craftingGO, int index)
	{
		if (Affiliation.IsOwnedBy(craftingGO, base.gameObject))
		{
			craftingGO.WithComponent(delegate(CraftingLabor f)
			{
				f.RemoveRecipeAt(index);
			});
		}
	}

	[Command]
	public void CmdStockpileKeepEmpty(GameObject craftingGO)
	{
		if (Affiliation.IsOwnedBy(craftingGO, base.gameObject))
		{
			craftingGO.WithComponent(delegate(Recepteur r)
			{
				r.DoExcludeAll();
			});
		}
	}

	[Command]
	public void CmdStockpileAcceptAll(GameObject craftingGO)
	{
		if (Affiliation.IsOwnedBy(craftingGO, base.gameObject))
		{
			craftingGO.WithComponent(delegate(Recepteur r)
			{
				r.ClearExclusions();
			});
		}
	}

	[Command]
	public void CmdStockpileAcceptOnly(GameObject recepteurGO, string ressourceString)
	{
		if (Affiliation.IsOwnedBy(recepteurGO, base.gameObject))
		{
			Recepteur component = recepteurGO.GetComponent<Recepteur>();
			Adjectif adjectif = Adjectif.StringToAdjectif(ressourceString);
			component.MakeExclusive(new List<Ressource> { adjectif as Ressource });
			CallRpcMakeExclusive(recepteurGO, ressourceString);
		}
	}

	[ClientRpc]
	public void RpcMakeExclusive(GameObject recepteurGO, string ressourceString)
	{
		if (!NetworkServer.active)
		{
			Recepteur component = recepteurGO.GetComponent<Recepteur>();
			Adjectif adjectif = Adjectif.StringToAdjectif(ressourceString);
			component.MakeExclusive(new List<Ressource> { adjectif as Ressource });
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdChangeProject(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChangeProject called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdChangeProject(reader.ReadGameObject(), reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdAccepterPosition(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdAccepterPosition called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdAccepterPosition(reader.ReadGameObject(), (int)reader.ReadPackedUInt32(), (int)reader.ReadPackedUInt32(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeCmdCmdAccepterRayon(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdAccepterRayon called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdAccepterRayon(reader.ReadGameObject(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeCmdCmdTransformLog(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTransformLog called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdTransformLog(reader.ReadGameObject(), reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdAddMoveToGoal(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdAddMoveToGoal called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdAddMoveToGoal(reader.ReadGameObject(), reader.ReadVector3(), reader.ReadBoolean(), reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdSetAutonomy(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetAutonomy called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdSetAutonomy(reader.ReadGameObject(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeCmdCmdAddMoveToVolumeGoal(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdAddMoveToVolumeGoal called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdAddMoveToVolumeGoal(reader.ReadGameObject(), reader.ReadGameObject(), reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdAddEmergencyRecallGoal(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdAddEmergencyRecallGoal called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdAddEmergencyRecallGoal(reader.ReadGameObject(), reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdAddCallToArmsGoal(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdAddCallToArmsGoal called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdAddCallToArmsGoal(reader.ReadGameObject(), reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdAddBackToWorkGoal(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdAddBackToWorkGoal called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdAddBackToWorkGoal(reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdGoDetonate(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdGoDetonate called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdGoDetonate(reader.ReadGameObject(), reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdAddCleanGoal(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdAddCleanGoal called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdAddCleanGoal(reader.ReadGameObject(), reader.ReadVector3(), reader.ReadGameObject(), reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdAddTreeGoal(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdAddTreeGoal called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdAddTreeGoal(reader.ReadGameObject(), reader.ReadVector3(), reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdAddNatureHarvestGoal(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdAddNatureHarvestGoal called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdAddNatureHarvestGoal(reader.ReadGameObject(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeCmdCmdGoDropAt(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdGoDropAt called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdGoDropAt(reader.ReadGameObject(), reader.ReadVector3());
		}
	}

	protected static void InvokeCmdCmdGoStoreInto(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdGoStoreInto called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdGoStoreInto(reader.ReadGameObject(), reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdAddSmartTransferGoal(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdAddSmartTransferGoal called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdAddSmartTransferGoal(reader.ReadGameObject(), reader.ReadGameObject(), reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdRemoveAllLabor(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRemoveAllLabor called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdRemoveAllLabor(reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdAddBreakVoxelGoal(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdAddBreakVoxelGoal called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdAddBreakVoxelGoal(reader.ReadGameObject(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeCmdCmdRepair(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRepair called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdRepair(reader.ReadGameObject(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeCmdCmdBreakObject(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdBreakObject called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdBreakObject(reader.ReadGameObject(), reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdAttack(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdAttack called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdAttack(reader.ReadGameObject(), reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdMoveAttack(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdMoveAttack called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdMoveAttack(reader.ReadGameObject(), reader.ReadVector3());
		}
	}

	protected static void InvokeCmdCmdArmBearTrap(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdArmBearTrap called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdArmBearTrap(reader.ReadGameObject(), reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdPickup(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPickup called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdPickup(reader.ReadGameObject(), reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdSpawnFireflyNest(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSpawnFireflyNest called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdSpawnFireflyNest(reader.ReadGameObject(), (Occupation.Type)reader.ReadInt32());
		}
	}

	protected static void InvokeCmdCmdSpawn(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSpawn called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdSpawn(reader.ReadString(), reader.ReadString(), reader.ReadVector3(), reader.ReadQuaternion(), reader.ReadGameObject(), (Occupation.Type)reader.ReadInt32(), reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdSpawnWithLevel(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSpawnWithLevel called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdSpawnWithLevel(reader.ReadString(), reader.ReadString(), reader.ReadVector3(), reader.ReadQuaternion(), reader.ReadGameObject(), (Occupation.Type)reader.ReadInt32(), reader.ReadBoolean(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeCmdCmdSpawnCrossbowTron(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSpawnCrossbowTron called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdSpawnCrossbowTron(reader.ReadString(), reader.ReadString(), reader.ReadVector3(), reader.ReadQuaternion(), reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdSpawnAlchemist(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSpawnAlchemist called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdSpawnAlchemist(reader.ReadString(), reader.ReadString(), reader.ReadVector3(), reader.ReadQuaternion(), reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdSpawnGeomancer(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSpawnGeomancer called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdSpawnGeomancer(reader.ReadString(), reader.ReadString(), reader.ReadVector3(), reader.ReadQuaternion(), reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdSpawnHalbardier(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSpawnHalbardier called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdSpawnHalbardier(reader.ReadString(), reader.ReadString(), reader.ReadVector3(), reader.ReadQuaternion(), reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdInstantiateTunnelUnit(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdInstantiateTunnelUnit called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdInstantiateTunnelUnit(reader.ReadGameObject(), reader.ReadVector3(), reader.ReadVector3(), (TunnelType)reader.ReadInt32());
		}
	}

	protected static void InvokeCmdCmdRemoveTunnelUnit(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRemoveTunnelUnit called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdRemoveTunnelUnit(reader.ReadGameObject(), reader.ReadVector3());
		}
	}

	protected static void InvokeCmdCmdCraftingAssignWorker(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCraftingAssignWorker called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdCraftingAssignWorker(reader.ReadGameObject(), reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdHarvestToggleResource(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdHarvestToggleResource called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdHarvestToggleResource(reader.ReadGameObject(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeCmdCmdHarvestSetResource(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdHarvestSetResource called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdHarvestSetResource(reader.ReadGameObject(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeCmdCmdQuarryToggleResource(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdQuarryToggleResource called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdQuarryToggleResource(reader.ReadGameObject(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeCmdCmdTunnelToggleResource(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTunnelToggleResource called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdTunnelToggleResource(reader.ReadGameObject(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeCmdCmdCraftingRemoveResources(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCraftingRemoveResources called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdCraftingRemoveResources(reader.ReadGameObject(), reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdCraftingAddResources(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCraftingAddResources called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdCraftingAddResources(reader.ReadGameObject(), reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdCraftingRemoveProduction(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCraftingRemoveProduction called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdCraftingRemoveProduction(reader.ReadGameObject(), reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdCraftingWorkAt(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCraftingWorkAt called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdCraftingWorkAt(reader.ReadGameObject(), reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdcraftingQueueRecipe(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdcraftingQueueRecipe called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdcraftingQueueRecipe(reader.ReadGameObject(), (int)reader.ReadPackedUInt32(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeCmdCmdcraftingProduce(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdcraftingProduce called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdcraftingProduce(reader.ReadGameObject(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeCmdCmdcraftingDoNothing(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdcraftingDoNothing called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdcraftingDoNothing(reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdcraftingLoop(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdcraftingLoop called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdcraftingLoop(reader.ReadGameObject(), reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdcraftingRemoveAt(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdcraftingRemoveAt called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdcraftingRemoveAt(reader.ReadGameObject(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeCmdCmdStockpileKeepEmpty(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdStockpileKeepEmpty called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdStockpileKeepEmpty(reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdStockpileAcceptAll(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdStockpileAcceptAll called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdStockpileAcceptAll(reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdStockpileAcceptOnly(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdStockpileAcceptOnly called on client.");
		}
		else
		{
			((UNetProjectCmd)obj).CmdStockpileAcceptOnly(reader.ReadGameObject(), reader.ReadString());
		}
	}

	public void CallCmdChangeProject(GameObject laborGO, GameObject projectGO)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdChangeProject called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdChangeProject(laborGO, projectGO);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdChangeProject);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(laborGO);
		networkWriter.Write(projectGO);
		SendCommandInternal(networkWriter, 0, "CmdChangeProject");
	}

	public void CallCmdAccepterPosition(GameObject beaconGO, int x, int y, int z)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdAccepterPosition called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdAccepterPosition(beaconGO, x, y, z);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdAccepterPosition);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(beaconGO);
		networkWriter.WritePackedUInt32((uint)x);
		networkWriter.WritePackedUInt32((uint)y);
		networkWriter.WritePackedUInt32((uint)z);
		SendCommandInternal(networkWriter, 0, "CmdAccepterPosition");
	}

	public void CallCmdAccepterRayon(GameObject beaconGO, int radius)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdAccepterRayon called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdAccepterRayon(beaconGO, radius);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdAccepterRayon);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(beaconGO);
		networkWriter.WritePackedUInt32((uint)radius);
		SendCommandInternal(networkWriter, 0, "CmdAccepterRayon");
	}

	public void CallCmdTransformLog(GameObject instance, GameObject objectToTransform)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdTransformLog called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdTransformLog(instance, objectToTransform);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdTransformLog);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(instance);
		networkWriter.Write(objectToTransform);
		SendCommandInternal(networkWriter, 0, "CmdTransformLog");
	}

	public void CallCmdAddMoveToGoal(GameObject laborGO, Vector3 position, bool drop, bool queued)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdAddMoveToGoal called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdAddMoveToGoal(laborGO, position, drop, queued);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdAddMoveToGoal);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(laborGO);
		networkWriter.Write(position);
		networkWriter.Write(drop);
		networkWriter.Write(queued);
		SendCommandInternal(networkWriter, 0, "CmdAddMoveToGoal");
	}

	public void CallCmdSetAutonomy(GameObject laborGO, int autonomyId)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetAutonomy called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetAutonomy(laborGO, autonomyId);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetAutonomy);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(laborGO);
		networkWriter.WritePackedUInt32((uint)autonomyId);
		SendCommandInternal(networkWriter, 0, "CmdSetAutonomy");
	}

	public void CallCmdAddMoveToVolumeGoal(GameObject laborGO, GameObject toObjectGO, bool queued)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdAddMoveToVolumeGoal called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdAddMoveToVolumeGoal(laborGO, toObjectGO, queued);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdAddMoveToVolumeGoal);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(laborGO);
		networkWriter.Write(toObjectGO);
		networkWriter.Write(queued);
		SendCommandInternal(networkWriter, 0, "CmdAddMoveToVolumeGoal");
	}

	public void CallCmdAddEmergencyRecallGoal(GameObject laborGO, GameObject toObjectGO)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdAddEmergencyRecallGoal called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdAddEmergencyRecallGoal(laborGO, toObjectGO);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdAddEmergencyRecallGoal);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(laborGO);
		networkWriter.Write(toObjectGO);
		SendCommandInternal(networkWriter, 0, "CmdAddEmergencyRecallGoal");
	}

	public void CallCmdAddCallToArmsGoal(GameObject laborGO, GameObject toObjectGO)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdAddCallToArmsGoal called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdAddCallToArmsGoal(laborGO, toObjectGO);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdAddCallToArmsGoal);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(laborGO);
		networkWriter.Write(toObjectGO);
		SendCommandInternal(networkWriter, 0, "CmdAddCallToArmsGoal");
	}

	public void CallCmdAddBackToWorkGoal(GameObject laborGO)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdAddBackToWorkGoal called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdAddBackToWorkGoal(laborGO);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdAddBackToWorkGoal);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(laborGO);
		SendCommandInternal(networkWriter, 0, "CmdAddBackToWorkGoal");
	}

	public void CallCmdGoDetonate(GameObject laborGO, GameObject barelGO)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdGoDetonate called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdGoDetonate(laborGO, barelGO);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdGoDetonate);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(laborGO);
		networkWriter.Write(barelGO);
		SendCommandInternal(networkWriter, 0, "CmdGoDetonate");
	}

	public void CallCmdAddCleanGoal(GameObject laborGO, Vector3 position, GameObject onlyObjectsLikeThisOne, bool queued)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdAddCleanGoal called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdAddCleanGoal(laborGO, position, onlyObjectsLikeThisOne, queued);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdAddCleanGoal);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(laborGO);
		networkWriter.Write(position);
		networkWriter.Write(onlyObjectsLikeThisOne);
		networkWriter.Write(queued);
		SendCommandInternal(networkWriter, 0, "CmdAddCleanGoal");
	}

	public void CallCmdAddTreeGoal(GameObject laborGO, Vector3 position, bool queued)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdAddTreeGoal called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdAddTreeGoal(laborGO, position, queued);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdAddTreeGoal);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(laborGO);
		networkWriter.Write(position);
		networkWriter.Write(queued);
		SendCommandInternal(networkWriter, 0, "CmdAddTreeGoal");
	}

	public void CallCmdAddNatureHarvestGoal(GameObject laborGO, int index)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdAddNatureHarvestGoal called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdAddNatureHarvestGoal(laborGO, index);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdAddNatureHarvestGoal);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(laborGO);
		networkWriter.WritePackedUInt32((uint)index);
		SendCommandInternal(networkWriter, 0, "CmdAddNatureHarvestGoal");
	}

	public void CallCmdGoDropAt(GameObject laborGO, Vector3 position)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdGoDropAt called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdGoDropAt(laborGO, position);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdGoDropAt);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(laborGO);
		networkWriter.Write(position);
		SendCommandInternal(networkWriter, 0, "CmdGoDropAt");
	}

	public void CallCmdGoStoreInto(GameObject laborGO, GameObject recepteurGO)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdGoStoreInto called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdGoStoreInto(laborGO, recepteurGO);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdGoStoreInto);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(laborGO);
		networkWriter.Write(recepteurGO);
		SendCommandInternal(networkWriter, 0, "CmdGoStoreInto");
	}

	public void CallCmdAddSmartTransferGoal(GameObject laborGO, GameObject target, bool queued)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdAddSmartTransferGoal called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdAddSmartTransferGoal(laborGO, target, queued);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdAddSmartTransferGoal);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(laborGO);
		networkWriter.Write(target);
		networkWriter.Write(queued);
		SendCommandInternal(networkWriter, 0, "CmdAddSmartTransferGoal");
	}

	public void CallCmdRemoveAllLabor(GameObject GroupeGO)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdRemoveAllLabor called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdRemoveAllLabor(GroupeGO);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdRemoveAllLabor);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(GroupeGO);
		SendCommandInternal(networkWriter, 0, "CmdRemoveAllLabor");
	}

	public void CallCmdAddBreakVoxelGoal(GameObject laborGO, int index)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdAddBreakVoxelGoal called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdAddBreakVoxelGoal(laborGO, index);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdAddBreakVoxelGoal);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(laborGO);
		networkWriter.WritePackedUInt32((uint)index);
		SendCommandInternal(networkWriter, 0, "CmdAddBreakVoxelGoal");
	}

	public void CallCmdRepair(GameObject instance, int index)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdRepair called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdRepair(instance, index);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdRepair);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(instance);
		networkWriter.WritePackedUInt32((uint)index);
		SendCommandInternal(networkWriter, 0, "CmdRepair");
	}

	public void CallCmdBreakObject(GameObject laborGO, GameObject go)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdBreakObject called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdBreakObject(laborGO, go);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdBreakObject);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(laborGO);
		networkWriter.Write(go);
		SendCommandInternal(networkWriter, 0, "CmdBreakObject");
	}

	public void CallCmdAttack(GameObject laborGO, GameObject go)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdAttack called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdAttack(laborGO, go);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdAttack);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(laborGO);
		networkWriter.Write(go);
		SendCommandInternal(networkWriter, 0, "CmdAttack");
	}

	public void CallCmdMoveAttack(GameObject laborGO, Vector3 destination)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdMoveAttack called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdMoveAttack(laborGO, destination);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdMoveAttack);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(laborGO);
		networkWriter.Write(destination);
		SendCommandInternal(networkWriter, 0, "CmdMoveAttack");
	}

	public void CallCmdArmBearTrap(GameObject laborGO, GameObject bearTrapGO)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdArmBearTrap called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdArmBearTrap(laborGO, bearTrapGO);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdArmBearTrap);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(laborGO);
		networkWriter.Write(bearTrapGO);
		SendCommandInternal(networkWriter, 0, "CmdArmBearTrap");
	}

	public void CallCmdPickup(GameObject laborGO, GameObject objectToPickupFrom)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdPickup called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdPickup(laborGO, objectToPickupFrom);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdPickup);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(laborGO);
		networkWriter.Write(objectToPickupFrom);
		SendCommandInternal(networkWriter, 0, "CmdPickup");
	}

	public void CallCmdSpawnFireflyNest(GameObject spawner, Occupation.Type occupation)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSpawnFireflyNest called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSpawnFireflyNest(spawner, occupation);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSpawnFireflyNest);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(spawner);
		networkWriter.Write((int)occupation);
		SendCommandInternal(networkWriter, 0, "CmdSpawnFireflyNest");
	}

	public void CallCmdSpawn(string factory, string name, Vector3 position, Quaternion rotation, GameObject factionGO, Occupation.Type occupation, bool equipGear)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSpawn called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSpawn(factory, name, position, rotation, factionGO, occupation, equipGear);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSpawn);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(factory);
		networkWriter.Write(name);
		networkWriter.Write(position);
		networkWriter.Write(rotation);
		networkWriter.Write(factionGO);
		networkWriter.Write((int)occupation);
		networkWriter.Write(equipGear);
		SendCommandInternal(networkWriter, 0, "CmdSpawn");
	}

	public void CallCmdSpawnWithLevel(string factory, string name, Vector3 position, Quaternion rotation, GameObject factionGO, Occupation.Type occupation, bool equipGear, int level)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSpawnWithLevel called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSpawnWithLevel(factory, name, position, rotation, factionGO, occupation, equipGear, level);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSpawnWithLevel);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(factory);
		networkWriter.Write(name);
		networkWriter.Write(position);
		networkWriter.Write(rotation);
		networkWriter.Write(factionGO);
		networkWriter.Write((int)occupation);
		networkWriter.Write(equipGear);
		networkWriter.WritePackedUInt32((uint)level);
		SendCommandInternal(networkWriter, 0, "CmdSpawnWithLevel");
	}

	public void CallCmdSpawnCrossbowTron(string factory, string name, Vector3 position, Quaternion rotation, GameObject factionGO)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSpawnCrossbowTron called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSpawnCrossbowTron(factory, name, position, rotation, factionGO);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSpawnCrossbowTron);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(factory);
		networkWriter.Write(name);
		networkWriter.Write(position);
		networkWriter.Write(rotation);
		networkWriter.Write(factionGO);
		SendCommandInternal(networkWriter, 0, "CmdSpawnCrossbowTron");
	}

	public void CallCmdSpawnAlchemist(string factory, string name, Vector3 position, Quaternion rotation, GameObject factionGO)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSpawnAlchemist called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSpawnAlchemist(factory, name, position, rotation, factionGO);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSpawnAlchemist);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(factory);
		networkWriter.Write(name);
		networkWriter.Write(position);
		networkWriter.Write(rotation);
		networkWriter.Write(factionGO);
		SendCommandInternal(networkWriter, 0, "CmdSpawnAlchemist");
	}

	public void CallCmdSpawnGeomancer(string factory, string name, Vector3 position, Quaternion rotation, GameObject factionGO)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSpawnGeomancer called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSpawnGeomancer(factory, name, position, rotation, factionGO);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSpawnGeomancer);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(factory);
		networkWriter.Write(name);
		networkWriter.Write(position);
		networkWriter.Write(rotation);
		networkWriter.Write(factionGO);
		SendCommandInternal(networkWriter, 0, "CmdSpawnGeomancer");
	}

	public void CallCmdSpawnHalbardier(string factory, string name, Vector3 position, Quaternion rotation, GameObject factionGO)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSpawnHalbardier called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSpawnHalbardier(factory, name, position, rotation, factionGO);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSpawnHalbardier);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(factory);
		networkWriter.Write(name);
		networkWriter.Write(position);
		networkWriter.Write(rotation);
		networkWriter.Write(factionGO);
		SendCommandInternal(networkWriter, 0, "CmdSpawnHalbardier");
	}

	public void CallCmdInstantiateTunnelUnit(GameObject TunnelProject, Vector3 vpos, Vector3 forward, TunnelType tunnelType)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdInstantiateTunnelUnit called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdInstantiateTunnelUnit(TunnelProject, vpos, forward, tunnelType);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdInstantiateTunnelUnit);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(TunnelProject);
		networkWriter.Write(vpos);
		networkWriter.Write(forward);
		networkWriter.Write((int)tunnelType);
		SendCommandInternal(networkWriter, 0, "CmdInstantiateTunnelUnit");
	}

	public void CallCmdRemoveTunnelUnit(GameObject instance, Vector3 position)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdRemoveTunnelUnit called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdRemoveTunnelUnit(instance, position);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdRemoveTunnelUnit);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(instance);
		networkWriter.Write(position);
		SendCommandInternal(networkWriter, 0, "CmdRemoveTunnelUnit");
	}

	public void CallCmdCraftingAssignWorker(GameObject craftingGO, GameObject laborGo)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdCraftingAssignWorker called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdCraftingAssignWorker(craftingGO, laborGo);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdCraftingAssignWorker);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(craftingGO);
		networkWriter.Write(laborGo);
		SendCommandInternal(networkWriter, 0, "CmdCraftingAssignWorker");
	}

	public void CallCmdHarvestToggleResource(GameObject projectGo, int type)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdHarvestToggleResource called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdHarvestToggleResource(projectGo, type);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdHarvestToggleResource);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(projectGo);
		networkWriter.WritePackedUInt32((uint)type);
		SendCommandInternal(networkWriter, 0, "CmdHarvestToggleResource");
	}

	public void CallCmdHarvestSetResource(GameObject projectGo, int type)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdHarvestSetResource called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdHarvestSetResource(projectGo, type);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdHarvestSetResource);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(projectGo);
		networkWriter.WritePackedUInt32((uint)type);
		SendCommandInternal(networkWriter, 0, "CmdHarvestSetResource");
	}

	public void CallCmdQuarryToggleResource(GameObject projectGo, int type)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdQuarryToggleResource called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdQuarryToggleResource(projectGo, type);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdQuarryToggleResource);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(projectGo);
		networkWriter.WritePackedUInt32((uint)type);
		SendCommandInternal(networkWriter, 0, "CmdQuarryToggleResource");
	}

	public void CallCmdTunnelToggleResource(GameObject projectGo, int type)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdTunnelToggleResource called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdTunnelToggleResource(projectGo, type);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdTunnelToggleResource);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(projectGo);
		networkWriter.WritePackedUInt32((uint)type);
		SendCommandInternal(networkWriter, 0, "CmdTunnelToggleResource");
	}

	public void CallCmdCraftingRemoveResources(GameObject craftingGO, GameObject laborGo)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdCraftingRemoveResources called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdCraftingRemoveResources(craftingGO, laborGo);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdCraftingRemoveResources);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(craftingGO);
		networkWriter.Write(laborGo);
		SendCommandInternal(networkWriter, 0, "CmdCraftingRemoveResources");
	}

	public void CallCmdCraftingAddResources(GameObject craftingGO, GameObject laborGo)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdCraftingAddResources called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdCraftingAddResources(craftingGO, laborGo);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdCraftingAddResources);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(craftingGO);
		networkWriter.Write(laborGo);
		SendCommandInternal(networkWriter, 0, "CmdCraftingAddResources");
	}

	public void CallCmdCraftingRemoveProduction(GameObject craftingGO, GameObject laborGo)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdCraftingRemoveProduction called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdCraftingRemoveProduction(craftingGO, laborGo);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdCraftingRemoveProduction);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(craftingGO);
		networkWriter.Write(laborGo);
		SendCommandInternal(networkWriter, 0, "CmdCraftingRemoveProduction");
	}

	public void CallCmdCraftingWorkAt(GameObject craftingGO, GameObject laborGo)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdCraftingWorkAt called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdCraftingWorkAt(craftingGO, laborGo);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdCraftingWorkAt);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(craftingGO);
		networkWriter.Write(laborGo);
		SendCommandInternal(networkWriter, 0, "CmdCraftingWorkAt");
	}

	public void CallCmdcraftingQueueRecipe(GameObject craftingGO, int recipeId, int qty)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdcraftingQueueRecipe called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdcraftingQueueRecipe(craftingGO, recipeId, qty);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdcraftingQueueRecipe);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(craftingGO);
		networkWriter.WritePackedUInt32((uint)recipeId);
		networkWriter.WritePackedUInt32((uint)qty);
		SendCommandInternal(networkWriter, 0, "CmdcraftingQueueRecipe");
	}

	public void CallCmdcraftingProduce(GameObject craftingGO, int recipeId)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdcraftingProduce called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdcraftingProduce(craftingGO, recipeId);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdcraftingProduce);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(craftingGO);
		networkWriter.WritePackedUInt32((uint)recipeId);
		SendCommandInternal(networkWriter, 0, "CmdcraftingProduce");
	}

	public void CallCmdcraftingDoNothing(GameObject craftingGO)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdcraftingDoNothing called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdcraftingDoNothing(craftingGO);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdcraftingDoNothing);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(craftingGO);
		SendCommandInternal(networkWriter, 0, "CmdcraftingDoNothing");
	}

	public void CallCmdcraftingLoop(GameObject craftingGO, bool value)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdcraftingLoop called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdcraftingLoop(craftingGO, value);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdcraftingLoop);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(craftingGO);
		networkWriter.Write(value);
		SendCommandInternal(networkWriter, 0, "CmdcraftingLoop");
	}

	public void CallCmdcraftingRemoveAt(GameObject craftingGO, int index)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdcraftingRemoveAt called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdcraftingRemoveAt(craftingGO, index);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdcraftingRemoveAt);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(craftingGO);
		networkWriter.WritePackedUInt32((uint)index);
		SendCommandInternal(networkWriter, 0, "CmdcraftingRemoveAt");
	}

	public void CallCmdStockpileKeepEmpty(GameObject craftingGO)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdStockpileKeepEmpty called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdStockpileKeepEmpty(craftingGO);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdStockpileKeepEmpty);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(craftingGO);
		SendCommandInternal(networkWriter, 0, "CmdStockpileKeepEmpty");
	}

	public void CallCmdStockpileAcceptAll(GameObject craftingGO)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdStockpileAcceptAll called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdStockpileAcceptAll(craftingGO);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdStockpileAcceptAll);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(craftingGO);
		SendCommandInternal(networkWriter, 0, "CmdStockpileAcceptAll");
	}

	public void CallCmdStockpileAcceptOnly(GameObject recepteurGO, string ressourceString)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdStockpileAcceptOnly called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdStockpileAcceptOnly(recepteurGO, ressourceString);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdStockpileAcceptOnly);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(recepteurGO);
		networkWriter.Write(ressourceString);
		SendCommandInternal(networkWriter, 0, "CmdStockpileAcceptOnly");
	}

	protected static void InvokeRpcRpcAddHighlightOnPosition(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcAddHighlightOnPosition called on server.");
		}
		else
		{
			((UNetProjectCmd)obj).RpcAddHighlightOnPosition((int)reader.ReadPackedUInt32(), (int)reader.ReadPackedUInt32(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcRpcRemoveHighlightOnPosition(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRemoveHighlightOnPosition called on server.");
		}
		else
		{
			((UNetProjectCmd)obj).RpcRemoveHighlightOnPosition((int)reader.ReadPackedUInt32(), (int)reader.ReadPackedUInt32(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcRpcPlaySound(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlaySound called on server.");
		}
		else
		{
			((UNetProjectCmd)obj).RpcPlaySound(reader.ReadGameObject(), reader.ReadString());
		}
	}

	protected static void InvokeRpcRpcMakeExclusive(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcMakeExclusive called on server.");
		}
		else
		{
			((UNetProjectCmd)obj).RpcMakeExclusive(reader.ReadGameObject(), reader.ReadString());
		}
	}

	public void CallRpcAddHighlightOnPosition(int x, int y, int z)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcAddHighlightOnPosition called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcAddHighlightOnPosition);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)x);
		networkWriter.WritePackedUInt32((uint)y);
		networkWriter.WritePackedUInt32((uint)z);
		SendRPCInternal(networkWriter, 0, "RpcAddHighlightOnPosition");
	}

	public void CallRpcRemoveHighlightOnPosition(int x, int y, int z)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcRemoveHighlightOnPosition called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcRemoveHighlightOnPosition);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)x);
		networkWriter.WritePackedUInt32((uint)y);
		networkWriter.WritePackedUInt32((uint)z);
		SendRPCInternal(networkWriter, 0, "RpcRemoveHighlightOnPosition");
	}

	public void CallRpcPlaySound(GameObject instance, string Sound)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcPlaySound called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcPlaySound);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(instance);
		networkWriter.Write(Sound);
		SendRPCInternal(networkWriter, 0, "RpcPlaySound");
	}

	public void CallRpcMakeExclusive(GameObject recepteurGO, string ressourceString)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcMakeExclusive called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcMakeExclusive);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(recepteurGO);
		networkWriter.Write(ressourceString);
		SendRPCInternal(networkWriter, 0, "RpcMakeExclusive");
	}

	static UNetProjectCmd()
	{
		kCmdCmdChangeProject = 1832411392;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdChangeProject, InvokeCmdCmdChangeProject);
		kCmdCmdAccepterPosition = -2051813145;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdAccepterPosition, InvokeCmdCmdAccepterPosition);
		kCmdCmdAccepterRayon = -1630636917;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdAccepterRayon, InvokeCmdCmdAccepterRayon);
		kCmdCmdTransformLog = 1430752673;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdTransformLog, InvokeCmdCmdTransformLog);
		kCmdCmdAddMoveToGoal = 1583049367;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdAddMoveToGoal, InvokeCmdCmdAddMoveToGoal);
		kCmdCmdSetAutonomy = -1729931947;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdSetAutonomy, InvokeCmdCmdSetAutonomy);
		kCmdCmdAddMoveToVolumeGoal = -650840623;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdAddMoveToVolumeGoal, InvokeCmdCmdAddMoveToVolumeGoal);
		kCmdCmdAddEmergencyRecallGoal = -1480351235;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdAddEmergencyRecallGoal, InvokeCmdCmdAddEmergencyRecallGoal);
		kCmdCmdAddCallToArmsGoal = -1026761445;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdAddCallToArmsGoal, InvokeCmdCmdAddCallToArmsGoal);
		kCmdCmdAddBackToWorkGoal = 1569339614;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdAddBackToWorkGoal, InvokeCmdCmdAddBackToWorkGoal);
		kCmdCmdGoDetonate = 1486332113;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdGoDetonate, InvokeCmdCmdGoDetonate);
		kCmdCmdAddCleanGoal = 1874378436;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdAddCleanGoal, InvokeCmdCmdAddCleanGoal);
		kCmdCmdAddTreeGoal = -452285175;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdAddTreeGoal, InvokeCmdCmdAddTreeGoal);
		kCmdCmdAddNatureHarvestGoal = 750356669;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdAddNatureHarvestGoal, InvokeCmdCmdAddNatureHarvestGoal);
		kCmdCmdGoDropAt = -53635981;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdGoDropAt, InvokeCmdCmdGoDropAt);
		kCmdCmdGoStoreInto = 2091265104;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdGoStoreInto, InvokeCmdCmdGoStoreInto);
		kCmdCmdAddSmartTransferGoal = 1575967695;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdAddSmartTransferGoal, InvokeCmdCmdAddSmartTransferGoal);
		kCmdCmdRemoveAllLabor = -1854838052;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdRemoveAllLabor, InvokeCmdCmdRemoveAllLabor);
		kCmdCmdAddBreakVoxelGoal = 1840229106;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdAddBreakVoxelGoal, InvokeCmdCmdAddBreakVoxelGoal);
		kCmdCmdRepair = 1196308406;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdRepair, InvokeCmdCmdRepair);
		kCmdCmdBreakObject = 414615189;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdBreakObject, InvokeCmdCmdBreakObject);
		kCmdCmdAttack = 723584625;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdAttack, InvokeCmdCmdAttack);
		kCmdCmdMoveAttack = 93131042;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdMoveAttack, InvokeCmdCmdMoveAttack);
		kCmdCmdArmBearTrap = 1943690484;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdArmBearTrap, InvokeCmdCmdArmBearTrap);
		kCmdCmdPickup = 1142366885;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdPickup, InvokeCmdCmdPickup);
		kCmdCmdSpawnFireflyNest = -1837245213;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdSpawnFireflyNest, InvokeCmdCmdSpawnFireflyNest);
		kCmdCmdSpawn = 2118038066;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdSpawn, InvokeCmdCmdSpawn);
		kCmdCmdSpawnWithLevel = 457722956;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdSpawnWithLevel, InvokeCmdCmdSpawnWithLevel);
		kCmdCmdSpawnCrossbowTron = 1202674905;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdSpawnCrossbowTron, InvokeCmdCmdSpawnCrossbowTron);
		kCmdCmdSpawnAlchemist = 321132800;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdSpawnAlchemist, InvokeCmdCmdSpawnAlchemist);
		kCmdCmdSpawnGeomancer = 951154997;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdSpawnGeomancer, InvokeCmdCmdSpawnGeomancer);
		kCmdCmdSpawnHalbardier = 57028708;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdSpawnHalbardier, InvokeCmdCmdSpawnHalbardier);
		kCmdCmdInstantiateTunnelUnit = -1071506099;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdInstantiateTunnelUnit, InvokeCmdCmdInstantiateTunnelUnit);
		kCmdCmdRemoveTunnelUnit = 810033817;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdRemoveTunnelUnit, InvokeCmdCmdRemoveTunnelUnit);
		kCmdCmdCraftingAssignWorker = 1817604248;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdCraftingAssignWorker, InvokeCmdCmdCraftingAssignWorker);
		kCmdCmdHarvestToggleResource = 570928130;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdHarvestToggleResource, InvokeCmdCmdHarvestToggleResource);
		kCmdCmdHarvestSetResource = 1423332880;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdHarvestSetResource, InvokeCmdCmdHarvestSetResource);
		kCmdCmdQuarryToggleResource = -1626008217;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdQuarryToggleResource, InvokeCmdCmdQuarryToggleResource);
		kCmdCmdTunnelToggleResource = 170848883;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdTunnelToggleResource, InvokeCmdCmdTunnelToggleResource);
		kCmdCmdCraftingRemoveResources = -17984810;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdCraftingRemoveResources, InvokeCmdCmdCraftingRemoveResources);
		kCmdCmdCraftingAddResources = 1921116623;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdCraftingAddResources, InvokeCmdCmdCraftingAddResources);
		kCmdCmdCraftingRemoveProduction = -1753828824;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdCraftingRemoveProduction, InvokeCmdCmdCraftingRemoveProduction);
		kCmdCmdCraftingWorkAt = 1517273775;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdCraftingWorkAt, InvokeCmdCmdCraftingWorkAt);
		kCmdCmdcraftingQueueRecipe = -1093808780;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdcraftingQueueRecipe, InvokeCmdCmdcraftingQueueRecipe);
		kCmdCmdcraftingProduce = -1046736171;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdcraftingProduce, InvokeCmdCmdcraftingProduce);
		kCmdCmdcraftingDoNothing = -518656649;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdcraftingDoNothing, InvokeCmdCmdcraftingDoNothing);
		kCmdCmdcraftingLoop = 717376719;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdcraftingLoop, InvokeCmdCmdcraftingLoop);
		kCmdCmdcraftingRemoveAt = -1893111678;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdcraftingRemoveAt, InvokeCmdCmdcraftingRemoveAt);
		kCmdCmdStockpileKeepEmpty = 409176841;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdStockpileKeepEmpty, InvokeCmdCmdStockpileKeepEmpty);
		kCmdCmdStockpileAcceptAll = 63145402;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdStockpileAcceptAll, InvokeCmdCmdStockpileAcceptAll);
		kCmdCmdStockpileAcceptOnly = 1957926579;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetProjectCmd), kCmdCmdStockpileAcceptOnly, InvokeCmdCmdStockpileAcceptOnly);
		kRpcRpcAddHighlightOnPosition = -754410726;
		NetworkBehaviour.RegisterRpcDelegate(typeof(UNetProjectCmd), kRpcRpcAddHighlightOnPosition, InvokeRpcRpcAddHighlightOnPosition);
		kRpcRpcRemoveHighlightOnPosition = 1803035385;
		NetworkBehaviour.RegisterRpcDelegate(typeof(UNetProjectCmd), kRpcRpcRemoveHighlightOnPosition, InvokeRpcRpcRemoveHighlightOnPosition);
		kRpcRpcPlaySound = 1161004636;
		NetworkBehaviour.RegisterRpcDelegate(typeof(UNetProjectCmd), kRpcRpcPlaySound, InvokeRpcRpcPlaySound);
		kRpcRpcMakeExclusive = 1154483185;
		NetworkBehaviour.RegisterRpcDelegate(typeof(UNetProjectCmd), kRpcRpcMakeExclusive, InvokeRpcRpcMakeExclusive);
		NetworkCRC.RegisterBehaviour("UNetProjectCmd", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result = default(bool);
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}
}
