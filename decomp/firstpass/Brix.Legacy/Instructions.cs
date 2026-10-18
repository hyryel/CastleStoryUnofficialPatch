using System;
using System.Collections.Generic;
using System.Linq;
using Brix.Audio;
using Brix.Common.Collections;
using Brix.Engine;
using Brix.Engine.Abstract;
using Brix.Engine.Nature;
using Brix.External.Factories;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Game.Components.CatapultSpace;
using Brix.Game.Components.Volumes;
using Brix.Game.Compounders;
using Brix.Game.Network;
using Brix.Game.Rules;
using Brix.Game.Semantique;
using Brix.Input;
using Brix.Input.ContextualOrders;
using Brix.Lifecycle.Pooling;
using Brix.Pathfinding;
using Brix.UI;
using Brix.UI.Builder.Menu;
using Brix.UI.GameSelector;
using Brix.Utils;
using Brix.Utils.Collections;
using Brix.Utils.Factories;
using Brix.Utils.Logging;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Legacy;

public class Instructions
{
	private static BrixLogger logger = new BrixLogger(typeof(Instructions).FullName);

	public static Dictionary<string, Action> commendes = new Dictionary<string, Action>();

	public static Dictionary<string, Func<object>> conditions = new Dictionary<string, Func<object>>();

	public static MonoBehaviour[] sujets;

	public static MonoBehaviour acteur;

	public static object objet;

	public static Ray ray;

	public static object resiliantObject;

	public static float lastTime;

	public static int _buildSubmenu = 0;

	private static UNetCatapult catapultHost => User.LocalUser.GetComponent<UNetCatapult>();

	private static UNetProjectCmd localUserProject => User.LocalUser.GetComponent<UNetProjectCmd>();

	private static UNetContextualCmd localUserContext => User.LocalUser.GetComponent<UNetContextualCmd>();

	public static void Batire()
	{
		Batire_Stockpile();
		Batire_Crafting();
		Batire_Catapult();
		SetBlueprint("StoneStandard", Blueprints.Stone_DoubleBrick);
		SetBlueprint("StoneHalf", Blueprints.Stone_Brick);
		SetBlueprint("StoneStair", Blueprints.Stone_Staircase);
		SetBlueprint("StoneArch", Blueprints.Stone_Arch);
		SetBlueprint("StoneColumnBase", Blueprints.Stone_Column_Base);
		SetBlueprint("StoneColumnMiddle", Blueprints.Stone_Column_Middle);
		SetBlueprint("StoneColumnTop", Blueprints.Stone_Column_Top);
		SetBlueprint("StoneDoubleBrickRoundLR", Blueprints.Stone_DoubleBrick_RoundLR);
		SetBlueprint("StoneDoubleBrickRoundL", Blueprints.Stone_DoubleBrick_RoundL);
		SetBlueprint("StoneDoubleBrickRoundR", Blueprints.Stone_DoubleBrick_RoundR);
		SetBlueprint("StoneBrickRound", Blueprints.Stone_Brick_Round);
		SetBlueprint("StoneStock", Blueprints.Pallet);
		SetBlueprint("Barrel", Blueprints.Barrel);
		SetBlueprint("Door", Blueprints.Door);
		SetLegacyBeacon("SelectMine", Groupes.Mine);
		SetBlueprint("Tool_Brick", Blueprints.Stone_DoubleBrick);
		SetBlueprint("Tool_HalfBrick", Blueprints.Stone_Brick);
		SetBlueprint("Tool_Stair", Blueprints.Stone_Staircase);
		SetBlueprint("Tool_Arch", Blueprints.Stone_Arch);
		SetBlueprint("Tool_StoneColumnBase", Blueprints.Stone_Column_Base);
		SetBlueprint("Tool_StoneColumnMiddle", Blueprints.Stone_Column_Middle);
		SetBlueprint("Tool_StoneColumnTop", Blueprints.Stone_Column_Top);
		SetBlueprint("Tool_FlutedColumnBase", Blueprints.Stone_Column2_Base);
		SetBlueprint("Tool_StonePillarBase", Blueprints.Stone_Column3_Base);
		SetBlueprint("Tool_RoundColumnBase", Blueprints.Stone_Column4_Base);
		SetBlueprint("Tool_SquareColumnBase", Blueprints.Stone_Column5_Base);
		SetBlueprint("Tool_StoneDoubleBrickRoundLR", Blueprints.Stone_DoubleBrick_RoundLR);
		SetBlueprint("Tool_StoneDoubleBrickRoundL", Blueprints.Stone_DoubleBrick_RoundL);
		SetBlueprint("Tool_StoneDoubleBrickRoundR", Blueprints.Stone_DoubleBrick_RoundR);
		SetBlueprint("Tool_StoneBrickRound", Blueprints.Stone_Brick_Round);
		SetBlueprint("Tool_StoneBrace1", Blueprints.Stone_Brace1);
		SetBlueprint("Tool_StoneBrace2", Blueprints.Stone_Brace2);
		SetBlueprint("Tool_Merlon", Blueprints.Stone_Merlon);
		SetBlueprint("Tool_MerlonCorner", Blueprints.Stone_MerlonCorner);
		SetBlueprint("Tool_MerlonDouble", Blueprints.Stone_MerlonDouble);
		SetBlueprint("Tool_MerlonConcave", Blueprints.Stone_MerlonInverseCorner);
		SetBlueprint("Tool_StairSingle", Blueprints.Stone_StairSingle);
		SetBlueprint("Tool_StairSpiralLeft", Blueprints.Stone_StairSpiralLeft);
		SetBlueprint("Tool_StairSpiralRight", Blueprints.Stone_StairSpiralRight);
		SetBlueprint("Tool_StairCornerLeft", Blueprints.Stone_StairCornerLeft);
		SetBlueprint("Tool_StairCornerRight", Blueprints.Stone_StairCornerRight);
		SetBlueprint("Tool_StairCornerSingle", Blueprints.Stone_StairCornerSingle);
		SetBlueprint("Tool_StoneParapet1", Blueprints.Stone_Parapet1);
		SetBlueprint("Tool_StoneParapet2", Blueprints.Stone_Parapet2);
		SetBlueprint("Tool_StoneParapet3", Blueprints.Stone_Parapet3);
		SetBlueprint("Tool_BearTrap", Blueprints.BearTrap);
		SetBlueprint("Tool_SingleLog", Blueprints.Wood_SingleLog);
		SetBlueprint("Tool_VerticalBeam", Blueprints.Wood_VerticalBeam);
		SetBlueprint("Tool_SinglePlank", Blueprints.Wood_SinglePlank);
		SetBlueprint("Tool_Beam", Blueprints.Wood_Beam);
		SetBlueprint("Tool_Ramp", Blueprints.Wood_Ramp_TopA);
		SetBlueprint("Tool_RampJunction", Blueprints.Wood_Ramp_JunctionA);
		SetBlueprint("Tool_BrickPlug4Way", Blueprints.Stone_BeamPlug_AllSides);
		SetBlueprint("Tool_BrickPlug2Way", Blueprints.Stone_BeamPlug_TwoSides);
		SetBlueprint("Tool_BrickPlug1Way", Blueprints.Stone_BeamPlug_OneSide);
		SetBlueprint("Tool_BrickPlugCorner", Blueprints.Stone_BeamPlug_Corner);
		SetBlueprint("Tool_BrickPlug3way", Blueprints.Stone_BeamPlug_ThreeSides);
		SetBlueprint("Tool_Barrel", Blueprints.Barrel);
		SetBlueprint("Tool_Chest", Blueprints.Chest);
		SetBlueprint("Tool_Stockpile", Blueprints.Pallet);
		SetBlueprint("Tool_ShakyDoor", Blueprints.ShakyDoor);
		SetBlueprint("Tool_Door", Blueprints.Door);
		SetBlueprint("Tool_ReinforcedDoor", Blueprints.HeavyDoor);
		SetBlueprint("Tool_RopeBridge", Blueprints.RopeBridgePlacement);
		SetBlueprint("Tool_RopeBridgePlank", Blueprints.RopeBridgePlank);
		SetBlueprint("Tool_WoodBrace", Blueprints.Wood_Brace);
		SetBlueprint("Tool_ProjectileWard", Blueprints.SentinelWard);
		SetBlueprint("Tool_RegenWard", Blueprints.HealingWard);
		SetBlueprint("Tool_VisionWard", Blueprints.LanternWard);
		SetBlueprint("Tool_PylonWard", Blueprints.PylonGround);
		SetBlueprint("Tool_CaptureWard", Blueprints.CaptureWard);
		SetBlueprint("Tool_PaintingSmall", Blueprints.PaintingSmall);
		SetBlueprint("Tool_PaintingBig", Blueprints.PaintingBig);
		SetBlueprint("Tool_PaintingTall", Blueprints.PaintingTall);
		SetBlueprint("Tool_PaintingWide", Blueprints.PaintingWide);
		SetBlueprint("Tool_KnightStatue", Blueprints.KnightStatue);
		SetBlueprint("Tool_Window", Blueprints.Window);
		SetBlueprint("Tool_DoubleWindow", Blueprints.DoubleWindow);
		SetLegacyBeacon("Tool_Quarry", Groupes.Mine);
		SetLegacyBeacon("Tool_Tunnel", Groupes.Tunnel);
		SetLegacyBeacon("Tool_Clean", Groupes.Cleanup);
		SetLegacyBeacon("Tool_Repair", Groupes.Repair);
		SetLegacyBeacon("Tool_Landscape", Groupes.Landscape);
		SetLegacyBeacon("Tool_CustomBeacon", Balises.DefaultBalise);
		SetLegacyBeacon("Tool_HarvestEverything", Groupes.Harvest, HarvestProjectPlacementOptions.Everything);
		SetLegacyBeacon("Tool_HarvestTrees", Groupes.Harvest, HarvestProjectPlacementOptions.TreesOnly);
		SetLegacyBeacon("Tool_HarvestPlants", Groupes.Harvest, HarvestProjectPlacementOptions.PlantsOnly);
		SetLegacyBeacon("Tool_HarvestBoulders", Groupes.Harvest, HarvestProjectPlacementOptions.BouldersOnly);
		SetLegacyBeacon("Tool_HarvestIron", Groupes.Harvest, HarvestProjectPlacementOptions.IronOnly);
		SetLegacyBeacon("Tool_HarvestBrimstone", Groupes.Harvest, HarvestProjectPlacementOptions.BrimstoneOnly);
		SetPicker("Tool_Terrain_Platform", Pickers.IslandBrush);
		SetPicker("Tool_Terrain_Add", Pickers.ElevationBrush);
		SetPicker("Tool_Terrain_Remove", Pickers.ElevationBrush);
		SetPicker("Tool_Terrain_Smooth", Pickers.SmoothBrush);
		SetPicker("Tool_Terrain_Equalize", Pickers.PlateauBrush);
		SetPicker("Tool_Grass", Pickers.GrassBrush);
		SetPicker("Tool_Dirt", Pickers.GrassBrush);
		SetPicker("Tool_Plant", Pickers.PlantBrush);
		SetPicker("Tool_Tree", Pickers.TreeBrush);
		SetUIMarker("Tool_Markers_Crystal", UIMarkersInterface.SpawnerObject.Crystal);
		SetUIMarker("Tool_Markers_Player", UIMarkersInterface.SpawnerObject.PlayerSpawn);
		SetUIMarker("Tool_Markers_Enemy", UIMarkersInterface.SpawnerObject.CorruptronSpawn);
		SetUIMarker("Tool_Markers_Tutorial", UIMarkersInterface.SpawnerObject.Tutorial);
		SetGrabber("Tool_DrawBridge", ObjetsDynamiques.DrawBridge);
		SetGrabber("Tool_MagicBridge", ObjetsDynamiques.MagicBridge);
		commendes["Tool_BuildDebug"] = delegate
		{
			_buildSubmenu = -1;
		};
		commendes["Tool_Construction_Deselect"] = delegate
		{
			_buildSubmenu = 0;
		};
		commendes["Tool_Construction"] = delegate
		{
			_buildSubmenu = 0;
			User.LocalUser.GetComponent<UNetGroupe>().CallCmdCreate();
		};
		commendes["Tool_Construction_SpecialBlocks"] = delegate
		{
			_buildSubmenu = 1;
			Picking.Instance.SendPicker(Pickers.BlueprintPlacement, Blueprints.Stone_Staircase);
		};
		commendes["Tool_Construction_Items"] = delegate
		{
			_buildSubmenu = 2;
			Picking.Instance.SendPicker(Pickers.BlueprintPlacement, Blueprints.PylonWard);
		};
		commendes["Tool_Construction_Structure"] = delegate
		{
			_buildSubmenu = 3;
			Picking.Instance.SendPicker(Pickers.BlueprintPlacement, Blueprints.Catapult);
		};
		commendes["Tool_WorldEditor"] = delegate
		{
			_buildSubmenu = 5;
		};
		conditions["Condition_InputModeStorage"] = delegate
		{
			bool flag = InputModeController.IsSelectingStorage();
			return flag;
		};
		conditions["Condition_InputModeBlueprintSelection"] = delegate
		{
			bool flag = InputModeController.IsSelectingBlueprints();
			return flag;
		};
		conditions["Condition_IsSelf"] = () => OneSubjectOnTargetIsTrue((Labor a, Labor t) => a == t);
		conditions["Condition_BlueprintIsStructure"] = () => false;
		conditions["Condition_MixedBricktrons"] = delegate
		{
			Labor reference = UIGameObserver.bricktrons.CheckSelection().Slinq().FirstOrDefault();
			if (reference == null)
			{
				return false;
			}
			bool flag = UIGameObserver.bricktrons.CheckSelection().Slinq().Any((Labor b) => reference.Profession.InferedArchetype() != b.Profession.InferedArchetype());
			return flag;
		};
		conditions["Condition_EstUnEnnemi"] = () => TargetIsTrue<GameObject>(Affiliation.BelongsToEnnemy);
		conditions["Condition_ActorIsNotMine"] = () => OneSubjectIsTrue((GameObject go) => !Affiliation.BelongsToMe(go));
		conditions["Condition_ActorIsMine"] = () => OneSubjectIsTrue<GameObject>(Affiliation.BelongsToMe);
		conditions["Condition_TargetIsMine"] = () => TargetIsTrue<GameObject>(Affiliation.BelongsToMe);
		conditions["Condition_ActorIsBuilder"] = () => OneSubjectIsTrue((Occupation o) => o.CurrentOccupation == Occupation.Type.Builder);
		conditions["Condition_ActorIsKnight"] = () => OneSubjectIsTrue((Occupation o) => o.CurrentOccupation == Occupation.Type.Knight);
		conditions["Condition_ActorIsArcher"] = () => OneSubjectIsTrue((Occupation o) => o.CurrentOccupation == Occupation.Type.Archer);
		conditions["Condition_ActorIsArtificer"] = () => OneSubjectIsTrue((Toolbag t) => t.HasTool(Accessories.Staff));
		conditions["Condition_ActorIsMinitron"] = () => OneSubjectIsTrue((Occupation o) => o.CurrentOccupation == Occupation.Type.Minitron);
		conditions["Condition_ActorIsCorruptron"] = () => OneSubjectIsTrue((Occupation o) => o.CurrentOccupation == Occupation.Type.Corruptron);
		conditions["Condition_ActorIsBiftron"] = () => OneSubjectIsTrue((Occupation o) => o.CurrentOccupation == Occupation.Type.Biftron);
		conditions["Condition_IsBuildDebug"] = () => _buildSubmenu == -1;
		conditions["Condition_IsWorldEditor"] = () => InputModeController.DefaultMode == InputMode.worldEditor;
		conditions["Condition_IsTerrainBrushMenu"] = () => We.CurrentTab == We.Tab.Terrain;
		conditions["Condition_IsMarkersMenu"] = () => We.CurrentTab == We.Tab.Markers;
		conditions["Condition_RepairTask"] = () => TaskRules.Main.AllowedTasks.HasFlag(TaskType.Repair);
		conditions["Condition_LandscapeTask"] = () => TaskRules.Main.AllowedTasks.HasFlag(TaskType.Landscape);
		conditions["Condition_DoorLocked"] = delegate
		{
			Porte[] batchFromSelection = GetBatchFromSelection<Porte>();
			return batchFromSelection.Slinq().Any() && batchFromSelection.Slinq().All((Porte p) => p.locked);
		};
		conditions["Condition_ArmBearTrap"] = () => true;
		conditions["Condition_CanJumpOff"] = () => true;
		conditions["Condition_OutilConstruction"] = () => UIGameObserver.GetCurrentProjectInputContext() != null && UIGameObserver.GetCurrentProjectInputContext().defaultInputMode == InputMode.blueprint;
		conditions["Condition_CanDrop"] = () => OneSubjectIsTrue((Labor l) => !l.recepteur.IsEmpty());
		conditions["Condition_IsPlant"] = () => objet is int && PlantRegistry.IsPlant(XYZ.FromScalaire((int)objet));
		conditions["Condition_DigVoxel"] = () => objet is int && TerrainRules.Main.CanDigAt(Brix.Engine.Abstract.Voxel.GetVecteur((int)objet)) && OneSubjectIsTrue((Labor labor) => !labor.recepteur.RequiresBothHands());
		conditions["Condition_CutTree"] = () => objet is int && TerrainRules.Main.CanCutTreesAt(Brix.Engine.Abstract.Voxel.GetVecteur((int)objet)) && OneSubjectIsTrue((Labor labor) => !labor.recepteur.RequiresBothHands());
		conditions["Condition_TakeFromStorage"] = () => OneSubjectOnTargetIsTrue((Labor labor, Recepteur palette) => labor.recepteur.HasRoomForSomeOf(palette));
		conditions["Condition_StoreInContainer"] = () => OneSubjectOnTargetIsTrue((Labor labor, Recepteur palette) => palette.HasRoomForSomeOf(labor.recepteur));
		conditions["Condition_CanTakeFromGround"] = () => OneSubjectIsTrue((Labor l) => !l.recepteur.RequiresBothHands() && l.Profession.OccupationType == Occupation.Type.Builder);
		conditions["Condition_HasHandsFree"] = () => OneSubjectIsTrue((Labor l) => !l.recepteur.RequiresBothHands());
		conditions["Condition_HasHeavyLuggage"] = () => OneSubjectIsTrue((Labor l) => l.recepteur.RequiresBothHands());
		conditions["Condition_NewTaskConstruction"] = () => TaskRules.Main.AllowedTasks.HasFlag(TaskType.Construction) && Project.ProjectUnderCap();
		conditions["Condition_NewTaskStructure"] = () => TaskRules.Main.AllowedTasks.HasFlag(TaskType.Construction);
		conditions["Condition_NewTaskMine"] = () => TaskRules.Main.AllowedTasks.HasFlag(TaskType.Quarry) && Project.ProjectUnderCap();
		conditions["Condition_NewTaskTunnel"] = () => TaskRules.Main.AllowedTasks.HasFlag(TaskType.Tunnel) && Project.ProjectUnderCap();
		conditions["Condition_NewTaskHarvest"] = () => TaskRules.Main.AllowedTasks.HasFlag(TaskType.Harvest) && Project.ProjectUnderCap();
		conditions["Condition_NewTaskRepair"] = () => TaskRules.Main.AllowedTasks.HasFlag(TaskType.Repair) && Project.ProjectUnderCap();
		conditions["Condition_NewTaskLandscape"] = () => TaskRules.Main.AllowedTasks.HasFlag(TaskType.Landscape) && Project.ProjectUnderCap();
		conditions["Condition_NewTaskCleaning"] = () => TaskRules.Main.AllowedTasks.HasFlag(TaskType.Cleaning) && Project.ProjectUnderCap();
		conditions["Condition_ShowTaskConstruction"] = () => TaskRules.Main.AllowedTasks.HasFlag(TaskType.Construction);
		conditions["Condition_ShowTaskStructure"] = () => TaskRules.Main.AllowedTasks.HasFlag(TaskType.Construction);
		conditions["Condition_ShowTaskMine"] = () => TaskRules.Main.AllowedTasks.HasFlag(TaskType.Quarry);
		conditions["Condition_ShowTaskTunnel"] = () => TaskRules.Main.AllowedTasks.HasFlag(TaskType.Tunnel);
		conditions["Condition_ShowTaskHarvest"] = () => TaskRules.Main.AllowedTasks.HasFlag(TaskType.Harvest);
		conditions["Condition_ShowTaskCleaning"] = () => TaskRules.Main.AllowedTasks.HasFlag(TaskType.Cleaning);
		conditions["Condition_CanChangeSettings"] = () => true;
		conditions["Condition_CanJoinProject"] = delegate
		{
			Project project = GetProject();
			if (project == null)
			{
				return false;
			}
			bool flag = UIGameObserver.bricktrons.CheckSelection().Slinq().Any((Labor b) => project.CanHire(b));
			return flag;
		};
		conditions["Condition_CanDeleteTask"] = delegate
		{
			Project project = GetProject();
			if (project == null)
			{
				return false;
			}
			if (!project.CanBeDeleted)
			{
				return false;
			}
			return (!TaskRules.Main.CanDeleteTasks) ? ((object)false) : ((object)true);
		};
		conditions["Condition_ProjectHasWorkers"] = delegate
		{
			Project project = GetProject();
			if (project == null)
			{
				return false;
			}
			bool flag = project.CrewCount() > 0;
			return flag;
		};
		conditions["Condition_BuildDebug"] = () => false;
		conditions["Condition_ReadyToSpawn"] = () => Or((FireflyNest n) => n.CanSpawnFor(Faction.DefaultAllied), OneSubjectIsTrue, TargetIsTrue);
		conditions["Condition_ReadyToSpawnMilitary"] = () => Or((FireflyNest n) => n.CanSpawnMilitaryFor(Faction.DefaultAllied), OneSubjectIsTrue, TargetIsTrue);
		commendes["Event_DeselectTool"] = delegate
		{
			Picking.Instance.ClearPick();
			UIGameActor.SelectIdleProject();
		};
		commendes["Event_DeselectGroup"] = delegate
		{
			Picking.Instance.ClearPick();
			UIGameActor.SelectIdleProject();
		};
		commendes["Event_ManualConvert"] = delegate
		{
			ResourceConverter[] batchFromSelection = GetBatchFromSelection<ResourceConverter>();
			batchFromSelection.Slinq().ForEach(delegate(ResourceConverter p)
			{
				p.ManualConversion();
			});
		};
		commendes["Event_AddWorker"] = delegate
		{
			Try(delegate(FireflyNest f)
			{
				f.AssignJobRequest(Occupation.Type.Builder, null);
			}, ForSubjects, ForTarget);
		};
		commendes["Event_AddArcher"] = delegate
		{
			Try(delegate(FireflyNest f)
			{
				f.AssignJobRequest(Occupation.Type.Archer, null);
			}, ForSubjects, ForTarget);
		};
		commendes["Event_AddWarrior"] = delegate
		{
			Try(delegate(FireflyNest f)
			{
				f.AssignJobRequest(Occupation.Type.Knight, null);
			}, ForSubjects, ForTarget);
		};
		commendes["Event_AddSoulStone"] = delegate
		{
			SpawnerCristal spawnerCristal = GetBatchFromSelection<SpawnerCristal>().Slinq().FirstOrDefault();
			if (!(spawnerCristal == null))
			{
				XYZ xYZ = Brix.Engine.Architecte.PigerUneCase(spawnerCristal.transform.position, 5f, 20);
			}
		};
		commendes["Event_AddCorruptron"] = delegate
		{
			AIGlobal.Spawn(Test.Corruptron, CameraScript.Target, Quaternion.identity, Faction.DefaultEnemy, Occupation.Type.Corruptron, equipGear: true);
		};
		commendes["Event_AddEnemyWarrior"] = delegate
		{
			AIGlobal.Spawn(Test.Bricktron, CameraScript.Target, Quaternion.identity, Faction.DefaultEnemy, Occupation.Type.Knight, equipGear: true);
		};
		commendes["Event_DeleteProject"] = delegate
		{
			Picking.Instance.ClearPick();
			Project project = GetProject();
			if (!(project == null) && !BlueprintCompounder.IsInstantiatingCompound())
			{
				project.RequestDelete();
			}
		};
		commendes["Event_Explode"] = delegate
		{
			ForTarget(delegate(GameObject b)
			{
				User.LocalUser.GetComponent<UNetContextualCmd>().CallCmdStartBarrelFuse(b);
			});
		};
		conditions["Condition_HasLuggage"] = () => OneSubjectIsTrue((Labor l) => !l.recepteur.IsEmpty());
		commendes["Event_PlaceLuggage"] = delegate
		{
			ForSubjectsOnTarget(delegate(Labor l, GameObject go)
			{
				DirectOrderProject.AddGoDropInto(l, go);
			});
		};
		commendes["Event_GoDetonate"] = delegate
		{
			ForSubjectsOnTarget(delegate(Labor l, GameObject go)
			{
				DirectOrderProject.GoDetonate(l, go);
			});
		};
		commendes["Event_MoveAttack"] = delegate
		{
			ContextualActionGroupData contextualActionGroupData = ObjectPool<ContextualActionGroupData>.Reserve().Init(sujets);
			Vector3 destination = ((!(objet is int)) ? ((GameObject)objet).transform.position : Brix.Engine.Abstract.Voxel.GetVecteur((int)objet));
			foreach (Labor item in (from m in sujets.Slinq()
				select m.GetComponent<Labor>()).NoLeak())
			{
				Location destination2 = Location.Of(contextualActionGroupData.GetFormationDestination(destination, item.gameObject));
				DirectOrderProject.AddMoveAttackGoal(item, destination2);
			}
			ObjectPool<ContextualActionGroupData>.Release(contextualActionGroupData);
		};
		commendes["Event_RecallWorkers"] = delegate
		{
			SoundEngine.Play("Recall_Workers");
			Try(delegate(SpawnerCristal v)
			{
				ForUserBuilders(delegate(Labor l)
				{
					DirectOrderProject.AddEmergencyRecallGoal(l, v.gameObject);
				});
			}, ForSubjects, ForTarget);
		};
		commendes["Event_CallToArms"] = delegate
		{
			SoundEngine.Play("Call_To_Arm");
			Try(delegate(SpawnerCristal v)
			{
				ForUserBuilders(delegate(Labor l)
				{
					DirectOrderProject.AddCallToArmsGoal(l, v.gameObject);
				});
			}, ForSubjects, ForTarget);
		};
		commendes["Event_BackToWork"] = delegate
		{
			SoundEngine.Play("Back_To_Work");
			Try(delegate
			{
				ForUserNotBuilders(delegate(Labor l)
				{
					DirectOrderProject.AddBackToworkGoal(l);
				});
			}, new Func<Action<GameObject>, bool>[2] { ForSubjects, ForTarget });
		};
		commendes["Event_Underside_Generate"] = delegate
		{
			Brix.Engine.Voxel.Engine.GenerateDefaultBottom();
		};
		commendes["Event_SelectWorkers"] = delegate
		{
			GetProject().Try(delegate(Project p)
			{
				UIGameActor.SelectProjectMember(p);
			});
		};
		commendes["Event_RemoveWorkers"] = delegate
		{
			GetProject().Try(delegate(Project p)
			{
				p.FireAll();
			});
		};
		commendes["Action_CleanupNow"] = delegate
		{
			Labor component = acteur.GetComponent<Labor>();
			if (component != null)
			{
				GameObject gameObject = null;
				gameObject = objet as GameObject;
				if ((bool)gameObject)
				{
					DirectOrderProject.AddCleanGoal(component, gameObject.transform.position, null);
				}
				else
				{
					int num = (int)objet;
					if (!Brix.Engine.Voxel.IsOpenAndWalkable(XYZ.FromScalaire(num)))
					{
						num += 1048576;
					}
					DirectOrderProject.AddCleanGoal(component, Brix.Engine.Abstract.Voxel.GetVecteur(num), null);
				}
			}
		};
		commendes["Action_DropLuggage"] = delegate
		{
			ForSubjects(delegate(Labor l)
			{
				DirectOrderProject.AddGoDropAt(l, Brix.Engine.Abstract.Voxel.GetVecteur((int)objet));
			});
		};
		commendes["Action_Transform_Log"] = delegate
		{
			ForSubjectsOnTarget(delegate(Labor l, GameObject go)
			{
				DirectOrderProject.TransformLog(l, go);
			});
		};
		commendes["Action_PickUp"] = delegate
		{
			ForSubjectsOnTarget(delegate(Labor l, GameObject go)
			{
				DirectOrderProject.PickUp(l, go);
			});
		};
		commendes["Event_CutTree"] = delegate
		{
			ForSubjects(delegate(Labor l)
			{
				localUserProject.CallCmdAddTreeGoal(l.gameObject, Brix.Engine.Abstract.Voxel.GetVecteur((int)objet), queued: false);
			});
		};
		commendes["Action_HarvestMinerals"] = delegate
		{
			ForSubjects(delegate(Labor s)
			{
				DirectOrderProject.AddNatureHarvestGoal(s, ((BoulderData)objet).RootPos.ToScalaire());
			});
		};
		commendes["Action_HarvestCrystals"] = delegate
		{
			ForSubjects(delegate(Labor s)
			{
				DirectOrderProject.AddNatureHarvestGoal(s, ((MineralData)objet).RootPos.ToScalaire());
			});
		};
		commendes["Action_JoinProject"] = delegate
		{
			ForSubjectsOnTarget(delegate(Labor l, Project p)
			{
				UIGameActor.AssignCharacterToProject(p, l);
			}, GetProject);
		};
		commendes["Action_CollectPlant"] = delegate
		{
			ForSubjects(delegate(Labor l)
			{
				DirectOrderProject.AddNatureHarvestGoal(l, (int)objet);
			});
		};
		commendes["Action_BreakObject"] = delegate
		{
			ForSubjectsOnTarget(delegate(Labor l, GameObject go)
			{
				DirectOrderProject.AddBreakObjectGoal(l, go);
			});
		};
		commendes["Action_CleanupNow"] = delegate
		{
			Labor component = acteur.GetComponent<Labor>();
			if (component != null)
			{
				GameObject gameObject = null;
				gameObject = objet as GameObject;
				if ((bool)gameObject)
				{
					DirectOrderProject.AddCleanGoal(component, gameObject.transform.position, null);
				}
				else
				{
					int num = (int)objet;
					if (!Brix.Engine.Voxel.IsOpenAndWalkable(XYZ.FromScalaire(num)))
					{
						num += 1048576;
					}
					DirectOrderProject.AddCleanGoal(component, Brix.Engine.Abstract.Voxel.GetVecteur(num), null);
				}
			}
		};
		commendes["Action_CleanupSimilars"] = delegate
		{
			Labor component = acteur.GetComponent<Labor>();
			if (component != null)
			{
				GameObject gameObject = (GameObject)objet;
				DirectOrderProject.AddCleanGoal(component, gameObject.transform.position, gameObject);
			}
		};
		commendes["Action_Pathfinding"] = delegate
		{
			Labor component = acteur.GetComponent<Labor>();
			if (component != null)
			{
				if (component.GetComponent<Navigation>().PathDrawer != null)
				{
					component.GetComponent<Navigation>().PathDrawer = null;
				}
				else
				{
					PathDrawer pathDrawer = UnityEngine.Object.FindObjectOfType<Brix.Pathfinding.Pathfinding>().GetComponentsInChildren<PathDrawer>(includeInactive: true).Slinq()
						.FirstOrDefault();
					if (pathDrawer != null)
					{
						pathDrawer.gameObject.SetActive(value: true);
					}
					component.GetComponent<Navigation>().PathDrawer = pathDrawer;
					int num = (int)objet;
					if (!Brix.Engine.Voxel.IsOpenAndWalkable(XYZ.FromScalaire(num)))
					{
						num += 1048576;
					}
					DirectOrderProject.AddMoveToGoal(component, Brix.Engine.Abstract.Voxel.GetVecteur(num));
				}
			}
		};
		commendes["Event_JumpOff"] = delegate
		{
			List<Labor> list = Subjects<Labor>();
			Labor labor = list.Slinq().FirstOrDefault();
			if (labor == null)
			{
				return;
			}
			Vector3 targetPos = ((!(objet is int)) ? PickingUtility.SubjectiveRaycast(labor.gameObject, ray) : Brix.Engine.Abstract.Voxel.GetVecteur((int)objet));
			Formation formation = new Formation();
			formation.Init(targetPos, (from l in list.Slinq()
				select l.gameObject).ToList());
			int num = 0;
			foreach (Labor item2 in list)
			{
				DirectOrderProject.GoAndJumpTo(item2, formation.Spot(num++));
			}
		};
		commendes["Action_MoveTo"] = delegate
		{
			Labor labor = acteur.Get<Labor>();
			if (labor != null)
			{
				if (objet is int)
				{
					int num = (int)objet;
					if (Brix.Engine.Voxel.IsFull(XYZ.FromScalaire(num)) && !Brix.Engine.Voxel.IsFull(XYZ.FromScalaire(num + 1048576)))
					{
						num += 1048576;
					}
					DirectOrderProject.AddMoveToGoal(labor, Brix.Engine.Abstract.Voxel.GetVecteur(num));
				}
				else if (objet is GameObject && (objet as GameObject).GetComponentInParent<IVolume>() != null)
				{
					DirectOrderProject.AddMoveToVolumeGoal(labor, (objet as GameObject).GetComponentInParent<IVolume>());
				}
			}
		};
		commendes["Action_TurnTo"] = delegate
		{
			Labor component = acteur.GetComponent<Labor>();
			if (component != null)
			{
				Location location = ((objet is int) ? new SpaceLocation(Brix.Engine.Abstract.Voxel.GetVecteur((int)objet)) : ((objet is GameObject) ? new ObjectLocation(objet as GameObject) : ((!(objet is Component)) ? ((Location)new SpaceLocation(PickingUtility.SubjectiveRaycast(component.gameObject, ray))) : ((Location)new ObjectLocation((objet as Component).gameObject)))));
				ObjectPoolSingleton.Request_Unsafe(Gameplay.ClickConfirm, location.Position, Quaternion.identity);
				DirectOrderProject.AddTurnToGoal(component, location);
			}
		};
		commendes["Action_DigVoxel"] = delegate
		{
			Labor component = acteur.GetComponent<Labor>();
			if (component != null)
			{
				int index = (int)objet;
				DirectOrderProject.AddBreakVoxelGoal(component, index);
			}
		};
		commendes["Action_RepairBlock"] = delegate
		{
			Labor component = acteur.GetComponent<Labor>();
			if (component != null)
			{
				int index = (int)objet;
				DirectOrderProject.RepairBlock(component, index);
			}
		};
		commendes["Action_DetachBlock"] = delegate
		{
			Labor component = acteur.GetComponent<Labor>();
			if (component != null)
			{
				Location location = ((objet is int) ? ((Location)new SpaceLocation(Brix.Engine.Architecte.SurfaceRaycast(Brix.Engine.Abstract.Voxel.GetVecteur((int)objet)), Quaternion.identity)) : ((Location)((!(objet is GameObject)) ? new ObjectLocation((objet as Component).gameObject) : new ObjectLocation(objet as GameObject))));
				DirectOrderProject.ShootLocation(component, location);
				ObjectPoolSingleton.Request_Unsafe(Gameplay.ClickConfirm, location.Position, Quaternion.identity);
			}
		};
		commendes["Action_ShootAt"] = delegate
		{
			Labor component = acteur.GetComponent<Labor>();
			if (component != null)
			{
				Location location = ((objet is int) ? ((Location)new SpaceLocation(Brix.Engine.Architecte.SurfaceRaycast(Brix.Engine.Abstract.Voxel.GetVecteur((int)objet)), Quaternion.identity)) : ((Location)((!(objet is GameObject)) ? new ObjectLocation((objet as Component).gameObject) : new ObjectLocation(objet as GameObject))));
				DirectOrderProject.ShootLocation(component, location);
				ObjectPoolSingleton.Request_Unsafe(Gameplay.ClickConfirm, location.Position, Quaternion.identity);
			}
		};
		commendes["Toggle_DoorLock"] = delegate
		{
			Porte[] batchFromSelection = GetBatchFromSelection<Porte>();
			if (batchFromSelection.Slinq().Any())
			{
				bool anyUnlocked = batchFromSelection.Slinq().Any((Porte p) => !p.locked);
				batchFromSelection.Slinq().ForEach(delegate(Porte p)
				{
					p.locked = anyUnlocked;
				});
			}
		};
		commendes["Event_ArmBearTrap"] = delegate
		{
			ForSubjectsOnTarget(delegate(GameObject l, GameObject t)
			{
				localUserProject.CallCmdArmBearTrap(l, t);
			});
		};
		conditions["Condition_HoldingPosition"] = () => OneSubjectIsTrue((Labor l) => l.Autonomy.IsDefensive) ? 1 : 0;
		commendes["Toggle_Hold_Position"] = delegate
		{
			AutonomyStatus autonomyStatus = null;
			foreach (Labor item3 in Subjects<Labor>())
			{
				if (autonomyStatus == null)
				{
					autonomyStatus = item3.Autonomy.OnPlayerToggle;
				}
				localUserProject.CallCmdSetAutonomy(item3.gameObject, (int)autonomyStatus.Id);
			}
		};
		commendes["Hover_DoorLock"] = delegate
		{
			PieContenu.SetCurrentTooltip("Locks or unlocks the door");
		};
		commendes["Hover_AutoConvert"] = delegate
		{
			ResourceConverter[] batchFromSelection = GetBatchFromSelection<ResourceConverter>();
			if (batchFromSelection.Slinq().Any())
			{
				batchFromSelection.Slinq().ForEach(delegate(ResourceConverter p)
				{
					PieContenu.SetCurrentTooltip((!p.AutoConvert) ? "Stockpile won't convert rock into stone blocks." : "Stockpile converts rock into stone blocks.");
				});
			}
		};
		commendes["Hover_ManualConvert"] = delegate
		{
			PieContenu.SetCurrentTooltip("Manually convert the stockpile.");
		};
	}

	private static void ForUserBuilders(Action<Labor> action)
	{
		foreach (Labor item in UIGameObserver.bricktrons.CheckCreation())
		{
			if (item.GetComponent<Occupation>().CurrentOccupation == Occupation.Type.Builder && !(item.GetComponent<AffiliationComponent>().faction != User.LocalUser.faction))
			{
				action(item);
			}
		}
	}

	private static void ForUserNotBuilders(Action<Labor> action)
	{
		foreach (Labor item in UIGameObserver.bricktrons.CheckCreation())
		{
			if (item.GetComponent<Occupation>().CurrentOccupation != Occupation.Type.Builder && !(item.GetComponent<AffiliationComponent>().faction != User.LocalUser.faction))
			{
				action(item);
			}
		}
	}

	public static Project GetProject()
	{
		if ((bool)UIGameObserver.projects.CurrentHovered)
		{
			return UIGameObserver.projects.CurrentHovered;
		}
		if ((bool)UIGameObserver.projects.LastHovered)
		{
			return UIGameObserver.projects.LastHovered;
		}
		return null;
	}

	public static void Execute(string s)
	{
		if (commendes.TryGetValue(s, out var value))
		{
			value();
		}
		else
		{
			logger.Error("Instructions.Execute() failed : " + s);
		}
	}

	public static void Act(string s)
	{
		if (commendes.TryGetValue(s, out var value))
		{
			if (sujets.Length == 0)
			{
				SetObjetAsSujet();
			}
			for (int i = 0; i < sujets.Length; i++)
			{
				acteur = sujets[i];
				if (acteur != null)
				{
					value();
				}
			}
		}
		else
		{
			logger.Error("Instructions.Act() failed : " + s);
		}
	}

	public static object Evaluate(string s)
	{
		object result = true;
		if (conditions.TryGetValue(s, out var value))
		{
			result = value();
		}
		else
		{
			logger.Error("Instructions.Evaluate() failed : " + s);
		}
		return result;
	}

	public static void SetObjetAsSujet()
	{
		if (objet is GameObject)
		{
			sujets = new MonoBehaviour[1] { (objet as GameObject).GetComponent<UISelectable>() };
		}
	}

	public static void SetPicker(string nom, Factory.AssetKey key)
	{
		commendes[nom] = delegate
		{
			Picking.Instance.SendPicker(key);
		};
	}

	public static void SetGrabber(string nom, Factory.AssetKey key)
	{
		commendes[nom] = delegate
		{
			InputModeController.Grab(key);
		};
	}

	public static void SetLegacyBeacon(string nom, Factory.AssetKey key)
	{
		commendes[nom] = delegate
		{
			InputModeController.PlaceBeacon(key);
		};
	}

	public static void SetLegacyBeacon(string nom, Factory.AssetKey key, IPickerComponentOptions options)
	{
		commendes[nom] = delegate
		{
			InputModeController.PlaceBeacon(key, options);
		};
	}

	public static void SetBlueprint(string nom, Factory.AssetKey blueprint)
	{
		commendes[nom] = delegate
		{
			InputModeController.PlaceBlueprint(blueprint);
		};
	}

	public static void SetUIMarker(string nom, UIMarkersInterface.SpawnerObject marker)
	{
		commendes[nom] = delegate
		{
			UIMarkersInterface.SendToPicker(marker);
		};
	}

	private static void Batire_Catapult()
	{
		SetBlueprint("Catapult", Blueprints.Catapult);
		SetBlueprint("Tool_Catapult", Blueprints.Catapult);
		OnCatapult();
		BricktronCatapult();
		Catapult_Conditions();
	}

	private static void Catapult_Conditions()
	{
		conditions["Condition_Catapult_Load"] = () => OneSubjectOnTargetIsTrue((Labor l, CatapultOperations c) => c.CanBeLoadedBy(l));
		conditions["Condition_Catapult_Unload"] = () => OneSubjectOnTargetIsTrue((Labor l, CatapultOperations c) => c.CanBeUnloadedBy(l));
		conditions["Condition_Catapult_AssignTo"] = () => true;
		conditions["Condition_Catapult_Work"] = () => OneSubjectOnTargetIsTrue((Labor l, CatapultOperations c) => c.CanBeWorkedBy(l));
		conditions["Condition_Catapult_Shoot"] = () => OneSubjectOnTargetIsTrue((Labor l, CatapultOperations c) => c.CanBeShotBy(l));
	}

	private static void BricktronCatapult()
	{
		commendes["Action_Catapult_AssignTo"] = delegate
		{
			ForSubjectsOnTarget(delegate(GameObject l, GameObject c)
			{
				catapultHost.CallCmdAssignWorker(c, l);
			});
		};
		commendes["Action_Catapult_Load"] = delegate
		{
			ForSubjectsOnTarget(delegate(GameObject l, GameObject c)
			{
				catapultHost.CallCmdLoadIntoCatapult(c, l);
			});
		};
		commendes["Action_Catapult_Unload"] = delegate
		{
			ForSubjectsOnTarget(delegate(GameObject l, GameObject c)
			{
				catapultHost.CallCmdUnloadFromCatapult(c, l);
			});
		};
		commendes["Action_Catapult_Work"] = delegate
		{
			ForSubjectsOnTarget(delegate(GameObject l, GameObject c)
			{
				catapultHost.CallCmdWorkOnCatapult(c, l);
			});
		};
		commendes["Action_Catapult_Shoot"] = delegate
		{
			ForSubjectsOnTarget(delegate(GameObject l, GameObject c)
			{
				catapultHost.CallCmdShootCatapult(c, l);
			});
		};
	}

	private static void OnCatapult()
	{
		commendes["Event_CatapultClearTarget"] = delegate
		{
			Try(ClearCatapulTarget, new Func<Action<GameObject>, bool>[2] { ForSubjects, ForTarget });
		};
		commendes["Event_CatapultHoldFire"] = delegate
		{
			Try(delegate(GameObject c)
			{
				catapultHost.CallCmdRequestHoldFire(c);
			}, ForSubjects, ForTarget);
		};
		commendes["Event_CatapultSetTarget"] = delegate
		{
			WithList(SetCatapultPicker, () => TryGet<GameObject>(Subjects<GameObject>, Target<GameObject>));
		};
		commendes["Event_CatapultFireOnce"] = delegate
		{
			Try(delegate(GameObject c)
			{
				catapultHost.CallCmdRequestFireOnce(c);
			}, ForSubjects, ForTarget);
		};
		commendes["Event_CatapultFireAtWill"] = delegate
		{
			Try(delegate(GameObject c)
			{
				catapultHost.CallCmdRequestFireAtWill(c);
			}, ForSubjects, ForTarget);
		};
	}

	private static void ClearCatapulTarget(GameObject go)
	{
		Picking.Instance.ClearPick();
		User.LocalUser.GetComponent<UNetCatapult>().CallCmdClearTarget(go);
	}

	private static void SetCatapultPicker(List<GameObject> gos)
	{
		CatapultTargetPicker.CatapultTargetPlacementOptions options = new CatapultTargetPicker.CatapultTargetPlacementOptions(gos);
		Picking.Instance.SendPicker(Pickers.CatapultTargetPlacement, null, options);
	}

	private static void Batire_Crafting()
	{
		SetBlueprint("Tool_Toolrack", Blueprints.Toolrack);
		SetBlueprint("Tool_Altar", Blueprints.Altar);
		SetBlueprint("Tool_ChoppingBlock", Blueprints.ChoppingBlock);
		SetBlueprint("Tool_Forge", Blueprints.Forge);
		SetBlueprint("Tool_Furnace", Blueprints.Furnace);
		SetBlueprint("Tool_Lab", Blueprints.Lab);
		SetBlueprint("Tool_Loom", Blueprints.Loom);
		SetBlueprint("Tool_MachineShop", Blueprints.MachineShop);
		SetBlueprint("Tool_Workbench", Blueprints.Workbench);
		Recipes();
		BrictronStation();
		Conditions();
		commendes["Action_EquipToolRackContent"] = delegate
		{
			ForSubjectsOnTarget(delegate(Labor labor, ToolRack t)
			{
				localUserContext.CallCmdEquipToolRackContent(labor.gameObject, t.gameObject);
			});
		};
		commendes["Action_UnequipToolIntoRackContent"] = delegate
		{
			ForSubjectsOnTarget(delegate(Labor labor, ToolRack t)
			{
				localUserContext.CallCmdUnequipIntoRack(labor.gameObject, t.gameObject);
			});
		};
		commendes["Action_EquipTool"] = delegate
		{
			ForSubjectsOnTarget(delegate(Labor labor, ToolRack t)
			{
				localUserContext.CallCmdEquipTool(labor.gameObject, t.gameObject);
			});
		};
		commendes["Action_UnequipTools"] = delegate
		{
			ForSubjects(delegate(Labor labor)
			{
				localUserContext.CallCmdUnequipAllTools(labor.gameObject);
			});
		};
	}

	private static void Conditions()
	{
		conditions["Condition_CanEquipTool"] = () => GeneralRules.AllowPartialToolsets && OneSubjectOnTargetIsTrue((Labor labor, GameObject go) => labor.Profession.CanEquip(go));
		conditions["Condition_CanUnequipTools"] = () => GeneralRules.AllowToDropTools;
		conditions["Condition_CraftingEnabled"] = () => GeneralRules.AllowCrafting;
		conditions["Condition_EquipRackContent"] = () => GeneralRules.AllowPartialToolsets || TargetIsTrue((ToolRack t) => t.HasFullKit());
		conditions["Condition_ToolRack_IsForKnight"] = () => TargetIsTrue((ToolRack rack) => rack.IsForKnight);
		conditions["Condition_ToolRack_IsForArcher"] = () => TargetIsTrue((ToolRack rack) => rack.IsForArcher);
		conditions["Condition_ToolRack_IsForHalberdier"] = () => TargetIsTrue((ToolRack rack) => rack.IsForHalberdier);
		conditions["Condition_ToolRack_IsForArbalist"] = () => TargetIsTrue((ToolRack rack) => rack.IsForArbalist);
		conditions["Condition_ToolRack_IsForAlchemist"] = () => TargetIsTrue((ToolRack rack) => rack.IsForAlchemist);
		conditions["Condition_ToolRack_IsForArtificer"] = () => TargetIsTrue((ToolRack rack) => rack.IsForArtificer);
		conditions["Condition_ToolRack_CharacterCanUnequip"] = () => OneSubjectOnTargetIsTrue((Labor l, ToolRack t) => t.HasRoomForEquipmentOf(l));
		conditions["Condition_Crafting_CanTakeResources"] = () => OneSubjectOnTargetIsTrue((Labor labor, CraftingStation station) => DefaultIfClient(station.CanPickUpRessource(labor), _ifClient: true));
		conditions["Condition_Crafting_CanAddResources"] = () => OneSubjectOnTargetIsTrue((Labor labor, CraftingStation station) => DefaultIfClient(station.recepteur.HasRoomForSomeOf(labor.recepteur), _ifClient: true));
		conditions["Condition_Crafting_CanTakeProduction"] = () => OneSubjectOnTargetIsTrue((Labor labor, CraftingStation station) => DefaultIfClient(station.CanPickUpProduction(labor), _ifClient: true));
		conditions["Condition_Crafting_CanWork"] = () => OneSubjectOnTargetIsTrue((Labor labor, CraftingStation station) => DefaultIfClient(station.CanWork(labor), _ifClient: true));
		conditions["Condition_Crafting_CanQueueKit"] = () => Or((CraftingStation station) => station.QueueCount <= 3, OneSubjectIsTrue, TargetIsTrue);
	}

	private static void BrictronStation()
	{
		commendes["Action_Crafting_AssignTo"] = delegate
		{
			ForSubjectsOnTarget(delegate(Labor l, GameObject f)
			{
				localUserProject.CallCmdCraftingAssignWorker(f.gameObject, l.gameObject);
			});
		};
		commendes["Action_Crafting_TakeResources"] = delegate
		{
			ForSubjectsOnTarget(delegate(Labor l, GameObject f)
			{
				localUserProject.CallCmdCraftingRemoveResources(f.gameObject, l.gameObject);
			});
		};
		commendes["Action_Crafting_AddResources"] = delegate
		{
			ForSubjectsOnTarget(delegate(Labor l, GameObject f)
			{
				localUserProject.CallCmdCraftingAddResources(f.gameObject, l.gameObject);
			});
		};
		commendes["Action_Crafting_TakeProduction"] = delegate
		{
			ForSubjectsOnTarget(delegate(Labor l, GameObject f)
			{
				localUserProject.CallCmdCraftingRemoveProduction(f.gameObject, l.gameObject);
			});
		};
		commendes["Action_Crafting_Work"] = delegate
		{
			ForSubjectsOnTarget(delegate(Labor l, GameObject f)
			{
				localUserProject.CallCmdCraftingWorkAt(f.gameObject, l.gameObject);
			});
		};
	}

	private static void Recipes()
	{
		commendes["Event_Workbench_OrderSword"] = delegate
		{
			OrderRecipe(1, CookBook.Sword);
		};
		commendes["Event_Workbench_OrderHelmet"] = delegate
		{
			OrderRecipe(1, CookBook.Helmet);
		};
		commendes["Event_Workbench_OrderShield"] = delegate
		{
			OrderRecipe(1, CookBook.Shield);
		};
		commendes["Event_Workbench_OrderKnightKit"] = delegate
		{
			OrderRecipe(1, CookBook.Sword, CookBook.Shield, CookBook.Helmet);
			SoundEngine.Play("Workbench_Knight_Select", localUserProject.gameObject);
		};
		commendes["Event_Workbench_OrderBow"] = delegate
		{
			OrderRecipe(1, CookBook.Bow);
		};
		commendes["Event_Workbench_OrderQuiver"] = delegate
		{
			OrderRecipe(1, CookBook.Quiver);
		};
		commendes["Event_Workbench_OrderCap"] = delegate
		{
			OrderRecipe(1, CookBook.Cap);
		};
		commendes["Event_Workbench_OrderArcherKit"] = delegate
		{
			OrderRecipe(1, CookBook.Bow, CookBook.Quiver, CookBook.Cap);
			SoundEngine.Play("Workbench_Archer_Select", localUserProject.gameObject);
		};
		commendes["Event_Crafting_DoNothing"] = CraftingDoNothing;
		commendes["Event_Crafting_ProducePlanks"] = delegate
		{
			ProduceRecipe(CookBook.Planks);
		};
		commendes["Event_Crafting_ProduceIron"] = delegate
		{
			ProduceRecipe(CookBook.IronIngot);
		};
		commendes["Event_Crafting_ProduceGlass"] = delegate
		{
			ProduceRecipe(CookBook.GlassIngot);
		};
		commendes["Event_Crafting_ProduceRope"] = delegate
		{
			ProduceRecipe(CookBook.RopeBallot);
		};
		commendes["Event_Crafting_ProduceFabric"] = delegate
		{
			ProduceRecipe(CookBook.FabricPile);
		};
		commendes["Event_Workbench_OrderBearTrap"] = delegate
		{
			OrderRecipe(1, CookBook.BearTrap);
		};
		commendes["Event_Workbench_OrderSentinelWard"] = delegate
		{
			OrderRecipe(1, CookBook.SentinelWard);
		};
		commendes["Event_Workbench_OrderHealingWard"] = delegate
		{
			OrderRecipe(1, CookBook.HealingWard);
		};
		commendes["Event_Workbench_OrderCaptureWard"] = delegate
		{
			OrderRecipe(1, CookBook.CaptureWard);
		};
		commendes["Event_Workbench_OrderLanternWard"] = delegate
		{
			OrderRecipe(1, CookBook.LanternWard);
		};
		commendes["Event_Workbench_OrderPyloneWard"] = delegate
		{
			OrderRecipe(1, CookBook.PylonWard);
		};
		commendes["Event_Workbench_OrderExplosiveBarrel"] = delegate
		{
			OrderRecipe(1, CookBook.ExplosiveBarrel);
		};
		commendes["Event_Workbench_OrderHalbard"] = delegate
		{
			OrderRecipe(1, CookBook.Poleaxe, CookBook.KettleHat);
		};
		commendes["Event_Workbench_OrderAlchemist"] = delegate
		{
			OrderRecipe(1, CookBook.AlchemistPouch, CookBook.AlchemistHat);
		};
		commendes["Event_Workbench_OrderCrossbow"] = delegate
		{
			OrderRecipe(1, CookBook.CrossbowWeapon, CookBook.CrossbowPouch, CookBook.CrossbowMorion);
		};
		commendes["Event_Workbench_OrderGeomancer"] = delegate
		{
			OrderRecipe(1, CookBook.Staff, CookBook.Sachel, CookBook.ArtificerHat);
		};
		commendes["Event_Crafting_ProduceBomb"] = delegate
		{
			ProduceRecipe(CookBook.Bomb);
		};
		commendes["Event_Crafting_ProduceCog"] = delegate
		{
			ProduceRecipe(CookBook.Cog);
		};
	}

	private static void OrderRecipe(int qty, params RecipeInfo[] recipes)
	{
		Try(delegate(CraftingStation f)
		{
			RecipeInfo[] array = recipes;
			foreach (RecipeInfo recipeInfo in array)
			{
				localUserProject.CallCmdcraftingQueueRecipe(f.gameObject, recipeInfo.id, qty);
			}
		}, ForSubjects, ForTarget);
	}

	private static void ProduceRecipe(RecipeInfo recipe)
	{
		Try(delegate(CraftingStation f)
		{
			localUserProject.CallCmdcraftingProduce(f.gameObject, recipe.id);
		}, ForSubjects, ForTarget);
	}

	private static void CraftingDoNothing()
	{
		Try(delegate(CraftingStation f)
		{
			localUserProject.CallCmdcraftingDoNothing(f.gameObject);
		}, ForSubjects, ForTarget);
	}

	private static void Batire_Stockpile()
	{
		StockpileConditions();
		StockpileEvents();
	}

	private static void StockpileEvents()
	{
		commendes["Event_Stockpile_AcceptNone"] = delegate
		{
			Try(delegate(Recepteur r)
			{
				localUserProject.CallCmdStockpileKeepEmpty(r.gameObject);
			}, ForSubjects, ForTarget);
		};
		commendes["Event_Stockpile_AcceptAll"] = delegate
		{
			Try(delegate(Recepteur r)
			{
				localUserProject.CallCmdStockpileAcceptAll(r.gameObject);
			}, ForSubjects, ForTarget);
		};
		ExclusionEvents();
		commendes["Stockpile_Toggle_AutoConvert"] = delegate
		{
			ResourceConverter[] batchFromSelection = GetBatchFromSelection<ResourceConverter>();
			if (batchFromSelection.Any())
			{
				bool parameter = !batchFromSelection.First().AutoConvert;
				batchFromSelection.Slinq().ForEach(delegate(ResourceConverter p, bool value)
				{
					p.ManualConversion();
					p.AutoConvert = value;
				}, parameter);
			}
		};
	}

	private static void ExclusionEvents()
	{
		foreach (AdjectiveInfo adj in AdjectiveInfo.GetInfosFromType(AdjectiveInfo.AdjectiveType.Ressource))
		{
			commendes["Event_Stockpile_Exclude_" + adj.codeName] = delegate
			{
				Try(delegate(Recepteur r)
				{
					localUserProject.CallCmdStockpileAcceptOnly(r.gameObject, adj.codeName);
				}, ForSubjects, ForTarget);
			};
		}
	}

	private static void StockpileConditions()
	{
		conditions["Condition_Stockpile_CanChangeSettings"] = () => true;
		conditions["Condition_Stockpile_ShowWoodButton"] = delegate
		{
			Recepteur[] batchFromSelection = GetBatchFromSelection<Recepteur>();
			bool flag = batchFromSelection.Slinq().All((Recepteur p) => p.ContentDescription.Has<WoodBlock>());
			return flag;
		};
		conditions["Condition_Stockpile_IsFull"] = delegate
		{
			Recepteur[] batchFromSelection = GetBatchFromSelection<Recepteur>();
			return batchFromSelection.Slinq().All((Recepteur p) => p.IsFull());
		};
		conditions["Stockpile_Toggled_AutoConvert"] = delegate
		{
			ResourceConverter[] batchFromSelection = GetBatchFromSelection<ResourceConverter>();
			int num = (from p in batchFromSelection.Slinq()
				where p.AutoConvert
				select p).Count();
			int num2 = batchFromSelection.Count();
			if (num == 0)
			{
				return 0;
			}
			return (num != num2) ? ((object)1) : ((object)2);
		};
	}

	private static SelectionType SelectionTypeFor<T>(GameObject go)
	{
		if (typeof(T) == typeof(Recepteur) || typeof(T) == typeof(ResourceConverter))
		{
			return SelectionType.Storage;
		}
		return go.GetComponent<UISelectable>().SelectionType;
	}

	private static T[] CastToArray<T>(List<GameObject> gos)
	{
		List<T> list = (from obj in gos.Slinq()
			select obj.GetComponent<T>() into t
			where t != null
			select t).ToPooledList();
		T[] result = list.ToArray();
		list.Dispose();
		return result;
	}

	protected static T[] GetBatchFromSelection<T>()
	{
		GameObject gameObject = objet as GameObject;
		SelectionType type = SelectionTypeFor<T>(gameObject);
		CollectionPool.Request<GameObject>(out List<GameObject> list);
		UIGameSelector.GetSelectedOfType(ref list, type);
		if (list.Any())
		{
			return CastToArray<T>(list);
		}
		list.Dispose();
		CollectionPool.Request<GameObject>(out List<GameObject> list2);
		UIGameSelector.GetHoveredOfType(type, ref list2);
		if (list2.Any())
		{
			return CastToArray<T>(list2);
		}
		list2.Dispose();
		T[] array = new T[1];
		T component = gameObject.GetComponent<T>();
		if (component != null)
		{
			array[0] = component;
		}
		return array;
	}

	private static bool DefaultIfClient(bool real, bool _ifClient)
	{
		if (NetworkServer.active)
		{
			return real;
		}
		return _ifClient;
	}

	private static void WithList<T>(Action<List<T>> action, Func<List<T>> generator) where T : UnityEngine.Object
	{
		List<T> list = generator();
		if (list.Count > 0)
		{
			action(list);
		}
	}

	private static List<T> TryGet<T>(params Func<List<T>>[] funcs) where T : UnityEngine.Object
	{
		foreach (Func<List<T>> func in funcs)
		{
			List<T> list = func();
			if (list.Count > 0)
			{
				return list;
			}
		}
		return Empty.List<T>();
	}

	private static List<T> Subjects<T>() where T : UnityEngine.Object
	{
		List<T> list = new List<T>();
		MonoBehaviour[] array = sujets;
		foreach (MonoBehaviour monoBehaviour in array)
		{
			if (!(monoBehaviour == null))
			{
				T val = monoBehaviour.Get<T>();
				if (!(val == null))
				{
					list.Add(val);
				}
			}
		}
		return list;
	}

	private static List<T> Target<T>() where T : UnityEngine.Object
	{
		List<T> list = new List<T>();
		if (objet is GameObject)
		{
			T val = (objet as GameObject).Get<T>();
			if (val != null)
			{
				list.Add(val);
			}
		}
		return list;
	}

	private static void Try<T>(Action<T> action, params Func<Action<T>, bool>[] funcs) where T : UnityEngine.Object
	{
		foreach (Func<Action<T>, bool> func in funcs)
		{
			if (func(action))
			{
				break;
			}
		}
	}

	private static bool ForTarget<T>(Action<T> action) where T : UnityEngine.Object
	{
		if (objet is GameObject)
		{
			T val = (objet as GameObject).Get<T>();
			if (val != null)
			{
				action(val);
				return true;
			}
		}
		return false;
	}

	private static bool ForSubjects<T>(Action<T> action) where T : UnityEngine.Object
	{
		bool result = false;
		MonoBehaviour[] array = sujets;
		foreach (MonoBehaviour monoBehaviour in array)
		{
			if (!(monoBehaviour == null))
			{
				T val = monoBehaviour.Get<T>();
				if (!(val == null))
				{
					action(val);
					result = true;
				}
			}
		}
		return result;
	}

	private static bool ForSubjectsOnTarget<Tsubject, Ttarget>(Action<Tsubject, Ttarget> action) where Tsubject : UnityEngine.Object where Ttarget : UnityEngine.Object
	{
		return ForSubjectsOnTarget(action, () => (objet as GameObject).Get<Ttarget>());
	}

	private static bool ForSubjectsOnTarget<Tsubject, Ttarget>(Action<Tsubject, Ttarget> action, Func<Ttarget> targetGetter) where Tsubject : UnityEngine.Object where Ttarget : UnityEngine.Object
	{
		bool result = false;
		if (objet is GameObject)
		{
			Ttarget val = targetGetter();
			if (val == null)
			{
				return false;
			}
			MonoBehaviour[] array = sujets;
			foreach (MonoBehaviour monoBehaviour in array)
			{
				if (!(monoBehaviour == null))
				{
					Tsubject val2 = monoBehaviour.Get<Tsubject>();
					if (!(val2 == null))
					{
						action(val2, val);
						result = true;
					}
				}
			}
		}
		return result;
	}

	private static bool And<T>(Func<T, bool> action, params Func<Func<T, bool>, bool>[] funcs) where T : UnityEngine.Object
	{
		foreach (Func<Func<T, bool>, bool> func in funcs)
		{
			if (!func(action))
			{
				return false;
			}
		}
		return true;
	}

	private static bool Or<T>(Func<T, bool> action, params Func<Func<T, bool>, bool>[] funcs) where T : UnityEngine.Object
	{
		foreach (Func<Func<T, bool>, bool> func in funcs)
		{
			if (func(action))
			{
				return true;
			}
		}
		return false;
	}

	private static bool TargetIsTrue<T>(Func<T, bool> test) where T : UnityEngine.Object
	{
		if (objet is GameObject)
		{
			T val = (objet as GameObject).Get<T>();
			return val != null && test(val);
		}
		return false;
	}

	private static bool OneSubjectIsTrue<Tsubject>(Func<Tsubject, bool> test) where Tsubject : UnityEngine.Object
	{
		MonoBehaviour[] array = sujets;
		foreach (MonoBehaviour monoBehaviour in array)
		{
			if (!(monoBehaviour == null))
			{
				Tsubject val = monoBehaviour.Get<Tsubject>();
				if (!(val == null) && test(val))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool OneSubjectOnTargetIsTrue<Tsubject, Ttarget>(Func<Tsubject, Ttarget, bool> test) where Tsubject : UnityEngine.Object where Ttarget : UnityEngine.Object
	{
		if (objet is GameObject)
		{
			Ttarget val = (objet as GameObject).Get<Ttarget>();
			if (val == null)
			{
				return false;
			}
			MonoBehaviour[] array = sujets;
			foreach (MonoBehaviour monoBehaviour in array)
			{
				if (!(monoBehaviour == null))
				{
					Tsubject val2 = monoBehaviour.Get<Tsubject>();
					if (!(val2 == null) && test(val2, val))
					{
						return true;
					}
				}
			}
		}
		return false;
	}
}
