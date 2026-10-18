using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Brix.Audio;
using Brix.Engine.Abstract;
using Brix.Engine.Loop;
using Brix.Engine.Nature;
using Brix.External.Factories;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.AI.Damage;
using Brix.Game.AI.Pathfinding;
using Brix.Game.Bricktron;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Input;
using Brix.Legacy;
using Brix.Lifecycle.Pooling;
using Brix.Lua;
using Brix.Network;
using Brix.NewUI.UserSettings_;
using Brix.Pathfinding.Test;
using Brix.Transactions;
using Brix.Util;
using Brix.Utils;
using Brix.Utils.Factories;
using Brix.Utils.Locomotion;
using Brix.Utils.Logging;
using Brix.Utils.Signals;
using MoonSharp.Interpreter;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Engine;

public class Architecte : Brix.Engine.Abstract.Architecte
{
	[Flags]
	private enum ChangeMegaVoxel
	{
		Surface = 1,
		Dommage = 2,
		Trees = 4,
		Plants = 8,
		Minerals = 0x10,
		Crystals = 0x20,
		Nature = 0x3C
	}

	private static BrixLogger logger = new BrixLogger(typeof(Architecte).FullName);

	public static bool allowEditeurShortcut = false;

	public static Architecte instance;

	public static readonly Dictionary<int, MegaVoxel> _megaVoxels = new Dictionary<int, MegaVoxel>();

	public Jardinier jardinier;

	public static bool editorMode = false;

	private static bool _allowDebugMenu = false;

	public static bool heightmapMode = false;

	public float LastVoxelChangedTime;

	private bool _isPathfindingEnabled = true;

	private static bool UIHidden = false;

	private static GameObject currentCanvas;

	private static bool _staticInitialized = false;

	private Dictionary<int, ChangeMegaVoxel> changedMegaVoxels = new Dictionary<int, ChangeMegaVoxel>();

	private HashSet<XYZ> freedVoxels = new HashSet<XYZ>();

	private HashSet<XYZ> _waitingForDebloc = new HashSet<XYZ>();

	private Dictionary<int, ChangeMegaVoxel> changedMegaVoxelsSwap = new Dictionary<int, ChangeMegaVoxel>();

	private HashSet<XYZ> freedVoxelsSwap = new HashSet<XYZ>();

	private static int GarnottesGeneratedThisFrame = 0;

	private static readonly VPropertyMask PreventSpawningProperties = VPropertyMask.obstacle | VPropertyMask.character;

	public static bool allowDebugMenu
	{
		get
		{
			return _allowDebugMenu;
		}
		set
		{
			if (value && GameParam.Map != null)
			{
				GameParam.Map.isDebug = true;
			}
			if (_allowDebugMenu != value)
			{
				_allowDebugMenu = value;
				LuaHookBase.InvokeFromCLR(UIGameObserver.lua_OnSetAllowDebugMode);
			}
		}
	}

	public static bool commence => GameSignals.Received(GameSignals.LevelLoaded);

	public static bool termine => GameSignals.Received(GameSignals.LeaveGame);

	public static bool physicsReady => GameSignals.Received(GameSignals.PhysicsReady);

	public static Transactor Transactor => GameObject.Find("/Jeu/NeutralTransactor").GetComponent<Transactor>();

	public static bool isPathfindingEnabled
	{
		get
		{
			return instance._isPathfindingEnabled;
		}
		set
		{
			instance._isPathfindingEnabled = value;
		}
	}

	public void ToggleHideUI()
	{
		if (currentCanvas == null)
		{
			currentCanvas = GameObject.Find("Canvas");
		}
		if (!UIHidden)
		{
			currentCanvas.GetComponent<Canvas>().enabled = false;
			UIHidden = true;
		}
		else
		{
			currentCanvas.GetComponent<Canvas>().enabled = true;
			UIHidden = false;
		}
	}

	private void Awake()
	{
		instance = this;
		jardinier = new Jardinier();
		this.GetOrAddComponent<GameSignals>();
		this.GetOrAddComponent<MusicLoop>();
		this.GetOrAddComponent<TerrainLoop>();
		this.GetOrAddComponent<PhysicsLoop>();
		GameSignals.Connect(GameSignals.GameOver, delegate(Architecte self)
		{
			self.LeaveGame();
		}, this);
	}

	private IEnumerator Start()
	{
		logger.Info("Architecte started");
		IEnumerator waiting = FactorySignalInvoker.OnFactoriesReadySig.WaitForSignal();
		while (waiting.MoveNext())
		{
			while (waiting.MoveNext())
			{
				yield return GameClock.WaitForEndOfFrame;
			}
		}
		if (!GameParam.instance._isInitialized)
		{
			GameParam.instance.LoadDebug();
		}
		UserSettingManager.SetSettingIntoGame(UserSettingsReader.Read());
		allowDebugMenu = GameParam.Map != null && GameParam.Map.isDebug;
		Physics.IgnoreLayerCollision(12, 13, ignore: true);
		StaticInit();
		MegaVoxel.StaticInit();
		ObjectPoolSingleton.Request_Unsafe(Brix.External.Factories.Environment.TerrainData);
		yield return GameClock.WaitForEndOfFrame;
		InitLoader();
		logger.Info("Architecte.Start() done");
		TerrainDamageReceiver.OnTerrainDamaged.Connect(OnTerrainDamaged).DisconnectWhen(GameSignals.LevelUnloaded);
		PlacedBlockDamageReceiver.OnBlockDamaged.Connect(OnPlacedBlockDamaged).DisconnectWhen(GameSignals.LevelUnloaded);
		PlacedBlockDamageReceiver.OnBlockHealed.Connect(OnPlacedBlockHealed).DisconnectWhen(GameSignals.LevelUnloaded);
		GameSignals.Connect(GameSignals.VoxelDestroyed, ProcessVoxelDestruction);
		GameSignals.Connect(GameSignals.VoxelChanged, Changement);
		GameSignals.Connect(GameSignals.BeforeBlockRemoved, WaitForDebloc);
		GameSignals.Connect(GameSignals.BlockTypeChanged, Changement);
		GameSignals.Connect(GameSignals.TreeChanged, delegate(XYZ p)
		{
			DirtyNature(p, ChangeMegaVoxel.Trees);
		});
		GameSignals.Connect(GameSignals.PlantChanged, delegate(XYZ p)
		{
			DirtyNature(p, ChangeMegaVoxel.Plants);
		});
		GameSignals.Connect(GameSignals.BouldersChanged, delegate(XYZ p)
		{
			DirtyNature(p, ChangeMegaVoxel.Minerals);
		});
		GameSignals.Connect(GameSignals.MineralsChanged, delegate(XYZ p)
		{
			DirtyNature(p, ChangeMegaVoxel.Crystals);
		});
	}

	public static void StaticInit()
	{
		if (!_staticInitialized)
		{
			_staticInitialized = true;
			Instructions.Batire();
			Livre.Lire("PieMenu_Buttons");
			Livre.Compile("PieMenu_Buttons");
			Livre.Lire("PieMenu_Menus");
			Livre.Compile("PieMenu_Menus");
		}
	}

	private void InitLoader()
	{
		StartCoroutine(GameLoader.LoadNewGameFromGameParams(OnLoadError));
	}

	private void LeaveGame()
	{
		GameSignals.Invoke(GameSignals.LeaveGame);
		ProjectDatabase.ClearAll();
		UNetManager.instance.Disconnect();
		editorMode = false;
	}

	private void StartErrorDialog(string title, string desc)
	{
		GameSignals.Invoke(GameSignals.StartError, desc);
	}

	private void OnLoadError(Exception e)
	{
		if (e is InterpreterException)
		{
			LuaLibs.LuaExceptionCatcher(e);
		}
		else
		{
			logger.Error(e, string.Empty);
		}
		GameSignals.Invoke(GameSignals.LeaveGame);
		if (e.Message == "UserCancel")
		{
			GameSignals.Invoke(GameSignals.StartError, string.Empty);
			LeaveGame();
		}
		else
		{
			GameSignals.Invoke(GameSignals.LoadError);
			logger.Error(e, "Architecte.OnLoadError()");
			StartErrorDialog("Error", "Your game could not be started properly. If this problem occurs again, please contact <color=#ffc000ff>support@castlestory.net</color>.");
		}
	}

	private void OnDestroy()
	{
		logger.Info("Architecte.Terminer() - debut");
		editorMode = false;
		instance = null;
		StopAllCoroutines();
		ClockConfig.Paused = false;
		ControlContext.SetContext("Menu", focus: false);
		GestionnaireDeNom.Terminer();
		ClearMegaVoxels();
		MegaVoxel.StaticDestroy();
		VoxelEngine.Terminer();
		SoundEngine.Terminer();
		Resources.UnloadUnusedAssets();
		logger.Info("Architecte.Terminer() - fin");
	}

	public static void QuitToMainMenu()
	{
		instance.LeaveGame();
	}

	private void Update()
	{
		GarnottesGeneratedThisFrame = 0;
		if (PickingUtility.Player.GetButtonDown("dbg_toggle_editormode") && allowEditeurShortcut)
		{
			editorMode = !editorMode;
			GameSignals.Invoke(GameSignals.OnEditorModeChanged, editorMode);
		}
		if (!editorMode || !PickingUtility.Player.GetButtonDown("dbg_forceCleanup"))
		{
			return;
		}
		if (allowEditeurShortcut)
		{
			List<Labor> list = UIGameObserver.bricktrons.CheckSelection().ToList();
			list.Slinq().ForEach(delegate(Labor b)
			{
				ObjectPoolSingleton.Release(b.gameObject);
			});
		}
		Neo.Vidanges();
	}

	private static MegaVoxel GetOrCreateMegaVoxel(int gScalaire)
	{
		return _megaVoxels.GetValueOrCreate(gScalaire, delegate(int gIndex)
		{
			int scalaire = Brix.Engine.Abstract.Voxel.GetScalaire(gIndex, 0);
			Vector3 v = Brix.Engine.Abstract.Voxel.GetGVecteur(scalaire) * 16f;
			return new MegaVoxel(v);
		});
	}

	public static MegaVoxel TryGetMegaVoxel(int gScalaire)
	{
		return _megaVoxels.GetValueOrDefault(gScalaire);
	}

	private static void ClearMegaVoxels()
	{
		_megaVoxels.Clear();
	}

	public static void DirtyVoxel(XYZ p)
	{
		MegaVoxel orCreateMegaVoxel = GetOrCreateMegaVoxel(Brix.Engine.Abstract.Voxel.GetGScalaire(p.x, p.y, p.z));
		orCreateMegaVoxel.AddVoxel(p);
	}

	public static TransactionMonitor NetworkAsymmetricDestroy(GameObject obj)
	{
		TransactionMonitor result = TransactionMonitor.Dummy;
		if (obj == null)
		{
			logger.Warn("Network object " + obj.name + " is already destroyed. Ignoring.");
			return result;
		}
		if (obj.GetComponent<IdentityHolder>().Registered)
		{
			result = Transactor.ExecuteSynchedMethod(UnityEngine.Object.Destroy, obj);
		}
		if (obj.IsMine())
		{
			ObjectPoolSingleton.Release(obj);
		}
		return result;
	}

	public static void AddVoxelToFreedList(XYZ position)
	{
		instance.freedVoxels.Add(position);
	}

	private static void ChangeMVSurface(int gIndex)
	{
		instance.changedMegaVoxels.ModifyValue(gIndex, (ChangeMegaVoxel b) => b | ChangeMegaVoxel.Surface);
	}

	private static void ChangeMVDommage(int gIndex)
	{
		instance.changedMegaVoxels.ModifyValue(gIndex, (ChangeMegaVoxel b) => b | ChangeMegaVoxel.Dommage);
	}

	private static void ChangeMVNature(int gIndex, ChangeMegaVoxel natureFlags)
	{
		instance.changedMegaVoxels.ModifyValue(gIndex, (ChangeMegaVoxel b) => b | natureFlags);
	}

	private void DirtyNature(XYZ p, ChangeMegaVoxel natureFlags)
	{
		DirtyVoxel(p);
		ChangeMVNature(Brix.Engine.Abstract.Voxel.GetGScalaire(p.ToScalaire()), natureFlags);
	}

	private static void AppliquerMVSurface(int gIndex)
	{
		MegaVoxel orCreateMegaVoxel = GetOrCreateMegaVoxel(gIndex);
		orCreateMegaVoxel.List();
		orCreateMegaVoxel.AfficherSpecial();
	}

	private static void AppliquerNature(int gIndex, ChangeMegaVoxel natureFlags)
	{
		MegaVoxel orCreateMegaVoxel = GetOrCreateMegaVoxel(gIndex);
		orCreateMegaVoxel.UpdateNatureLists();
	}

	public void AjouterAuJardinier(MegaVoxel m)
	{
		jardinier.AjouterMegaVoxel(m);
	}

	public void EnleverDuJardinier(MegaVoxel m)
	{
		jardinier.EnleverMegaVoxel(m);
	}

	public void UpdateJardinier()
	{
		jardinier.Travailler();
	}

	public static void AppliquerChangements()
	{
		if (instance.changedMegaVoxels.Count > 0)
		{
			Donnees.Swap(ref instance.changedMegaVoxels, ref instance.changedMegaVoxelsSwap);
			foreach (KeyValuePair<int, ChangeMegaVoxel> item in instance.changedMegaVoxelsSwap)
			{
				if ((item.Value & (ChangeMegaVoxel.Surface | ChangeMegaVoxel.Dommage)) != 0)
				{
					AppliquerMVSurface(item.Key);
				}
				if ((item.Value & ChangeMegaVoxel.Nature) != 0)
				{
					AppliquerNature(item.Key, item.Value);
				}
			}
			instance.changedMegaVoxelsSwap.Clear();
			instance.LastVoxelChangedTime = Time.time;
		}
		if (instance.freedVoxels.Count <= 0)
		{
			return;
		}
		Donnees.Swap(ref instance.freedVoxels, ref instance.freedVoxelsSwap);
		foreach (XYZ item2 in instance.freedVoxelsSwap)
		{
			using IBlockData blockData = Voxel.GetBlockData(item2);
			if (!blockData.IsNull)
			{
				blockData.Physicalize();
			}
			else
			{
				logger.WarnFormat("A requested Block Physicalization could not be completed. Voxel is empty at {0}: ", item2);
			}
		}
		instance.freedVoxelsSwap.Clear();
		instance.LastVoxelChangedTime = Time.time;
	}

	private static void WaitForDebloc(IBlockData bd)
	{
		instance._waitingForDebloc.Remove(bd.Position);
	}

	public static bool AuRepos()
	{
		return instance.changedMegaVoxels.Count == 0 && !VoxelEngine.Working;
	}

	private static void CheckArbre(XYZ position)
	{
		if (!TreeRegistry.Instance.HasTree(position))
		{
			return;
		}
		using IVoxel voxel = Voxel.GetTerrainBlockData(position);
		using IVoxel voxel2 = Voxel.GetTerrainBlockData(position - XYZ.up);
		if ((voxel.IsNull || !voxel.IsCrust) && (!voxel.IsNull || voxel2.IsNull || voxel2.IsCrust))
		{
			logger.InfoFormat("Deleting Tree: {0}", position);
			DamagedTree.GetAtOrCreate(position).Blast();
			TreeRegistry.Instance.Remove(position);
			ChangeMVNature(Brix.Engine.Abstract.Voxel.GetGScalaire(position.ToScalaire()), ChangeMegaVoxel.Trees);
		}
	}

	private static void CheckNature(XYZ position)
	{
		if (!NatureRegistry.HasNatureAt(position))
		{
			return;
		}
		using IVoxel voxel = Voxel.GetTerrainBlockData(position);
		if (!voxel.IsCrust)
		{
			logger.InfoFormat("Deleting Nature: {0}", position);
			if (NetworkServer.active)
			{
				NatureRegistry.BreakAt(position);
			}
			ChangeMVNature(Brix.Engine.Abstract.Voxel.GetGScalaire(position.ToScalaire()), ChangeMegaVoxel.Nature);
		}
	}

	public static void Changement(XYZ position)
	{
		DirtyVoxel(position);
		ChangeMVSurface(Brix.Engine.Abstract.Voxel.GetGScalaire(position.ToScalaire()));
		foreach (int gScalairesVoisin in GetGScalairesVoisins(position.ToScalaire()))
		{
			ChangeMVSurface(gScalairesVoisin);
		}
		CheckArbre(position);
		CheckArbre(position + XYZ.up);
		CheckNature(position);
	}

	private static void ProcessVoxelDestruction(IVoxel ibd, bool withParticles, bool withGarnotte)
	{
		if (ibd.BlockInfo.Groups.HasFlag(BlockGroups.Wood))
		{
			return;
		}
		if (withParticles)
		{
			Particules.Emettre(Factory.Peek(ibd.BlockInfo.ResourceKey).GetComponent<Garnotte>().particlesAsset, ibd.Position.ToVector3(), Quaternion.identity, 3f);
		}
		if (!withGarnotte)
		{
			return;
		}
		List<Factory.AssetKey> list = CollectionPool.RequestList<Factory.AssetKey>();
		List<int> list2 = CollectionPool.RequestList<int>();
		using (IVolume volume = ibd.Volume)
		{
			foreach (XYZ @case in volume.GetCases(VProperty.occupy))
			{
				Voxel.GetGarnotteValue(@case, list, list2);
				for (int i = 0; i < list.Count; i++)
				{
					if (list2[i] != 0)
					{
						if (GarnottesGeneratedThisFrame >= 20)
						{
							break;
						}
						Garnotte.Nouvelle(@case.ToVector3(), list2[i], list[i]);
						GarnottesGeneratedThisFrame += list2[i];
					}
				}
				list.Clear();
				list2.Clear();
			}
		}
		list.Dispose();
		list2.Dispose();
	}

	private static IEnumerable<int> GetGScalairesVoisins(int index)
	{
		if (Brix.Engine.Abstract.Voxel.GetPX(index) == 0 && Brix.Engine.Abstract.Voxel.GetGX(index) > 0)
		{
			yield return Brix.Engine.Abstract.Voxel.GetGScalaire(index) - 1;
		}
		else if (Brix.Engine.Abstract.Voxel.GetPX(index) == 15 && Brix.Engine.Abstract.Voxel.GetGX(index) < 64)
		{
			yield return Brix.Engine.Abstract.Voxel.GetGScalaire(index) + 1;
		}
		if (Brix.Engine.Abstract.Voxel.GetPY(index) == 0 && Brix.Engine.Abstract.Voxel.GetGY(index) > 0)
		{
			yield return Brix.Engine.Abstract.Voxel.GetGScalaire(index) - 4096;
		}
		else if (Brix.Engine.Abstract.Voxel.GetPY(index) == 15 && Brix.Engine.Abstract.Voxel.GetGY(index) < 64)
		{
			yield return Brix.Engine.Abstract.Voxel.GetGScalaire(index) + 4096;
		}
		if (Brix.Engine.Abstract.Voxel.GetPZ(index) == 0 && Brix.Engine.Abstract.Voxel.GetGZ(index) > 0)
		{
			yield return Brix.Engine.Abstract.Voxel.GetGScalaire(index) - 64;
		}
		else if (Brix.Engine.Abstract.Voxel.GetPZ(index) == 15 && Brix.Engine.Abstract.Voxel.GetGZ(index) < 64)
		{
			yield return Brix.Engine.Abstract.Voxel.GetGScalaire(index) + 64;
		}
	}

	public static void ExplosionSimple(Faction faction, Vector3 pos, float force, string effetSpecial = "Explosion")
	{
		_ExplosionSimple(pos, force, effetSpecial, faction);
	}

	public static void ExplosionSimple(GameObject source, Vector3 pos, float force, string effetSpecial = "Explosion")
	{
		_ExplosionSimple(pos, force, effetSpecial, Faction.GetFaction(source));
	}

	private static void _ExplosionSimple(Vector3 pos, float force, string effetSpecial, Faction faction)
	{
		logger.InfoFormat("Architecte.ExplosionSimple() pos: {0}, force: {1}, effectSpecial: {2}, faction: {3}", pos, force, effetSpecial, faction);
		Factory.AssetKey key = EffetsParticules.Explosion;
		string text;
		switch (effetSpecial)
		{
		case "Explosion":
			text = "explosion";
			break;
		case "ExplosionAlchemistPotion":
			key = EffetsParticules.ExplosionAlchimistPotion;
			text = "Bomb_Explode";
			break;
		case "Petite":
			key = EffetsParticules.ExplosionBarilVide;
			text = "explosion";
			break;
		case "Safe":
			key = EffetsParticules.ExplosionSafe;
			text = "explosion";
			break;
		default:
			key = EffetsParticules.ExplosionBarilVide;
			text = "explosion";
			break;
		}
		GameObject emitter = Transactor.SpawnUNet(key, faction, pos, Quaternion.identity);
		SoundEngine.Play(text, emitter);
	}

	public static void OnTerrainDamaged(XYZ position, bool withParticles = true)
	{
		if (withParticles)
		{
			Particules.Emettre(EffetsParticules.StoneParticles, position.ToVector3(), Quaternion.identity, 3f);
		}
		ChangeMVDommage(Brix.Engine.Abstract.Voxel.GetGScalaire(position.ToScalaire()));
	}

	public static void OnPlacedBlockDamaged(BlockData bd, bool withParticles = true)
	{
		if (withParticles)
		{
			Particules.Emettre(EffetsParticules.StoneParticles, bd.OffsetedPosition, Quaternion.identity, 3f);
		}
		ChangeMVDommage(Brix.Engine.Abstract.Voxel.GetGScalaire(bd.RootOrSelf.Position.ToScalaire()));
	}

	public static void OnPlacedBlockHealed(BlockData bd, bool withParticles = true)
	{
		if (withParticles)
		{
			Particules.Emettre(EffetsParticules.StoneParticles, bd.OffsetedPosition, Quaternion.identity, 3f);
		}
		ChangeMVDommage(Brix.Engine.Abstract.Voxel.GetGScalaire(bd.RootOrSelf.Position.ToScalaire()));
	}

	public static XYZ PigerUneCase(Vector3 position, float rayon, int maxAttempts = 100)
	{
		for (int i = 0; i < maxAttempts; i++)
		{
			Vector2 insideUnitCircle = UnityEngine.Random.insideUnitCircle;
			Vector3 vector = new Vector3(insideUnitCircle.x, 0f, insideUnitCircle.y);
			XYZ voxel = XYZ.FromVector3(position + vector * rayon);
			WalkablePathNode walkablePathNode = PathNodeRegistry.WalkableAt(voxel, 5);
			if (walkablePathNode != null)
			{
				voxel = walkablePathNode.RepresentativePosition;
				if (!VolumeRegistry.HasVolumesWithPropertyAt(voxel, PreventSpawningProperties))
				{
					return voxel;
				}
			}
		}
		logger.Warn(string.Concat("Architecte.PigerUneCase: mauvaise destination. ", position, ", ", rayon));
		return XYZ.zero;
	}

	public static Vector3 SurfaceRaycast(Vector3 position, int maxDist = 1, bool downOnly = false)
	{
		if (position.y < 1f)
		{
			return position;
		}
		Vector3 vector = Calcule.RoundVecteur(position);
		XYZ xYZ = XYZ.FromVector3(vector);
		int num = 0;
		while (!Voxel.IsOpenAndWalkable(xYZ) && num < maxDist)
		{
			num++;
			XYZ xYZ2 = xYZ + XYZ.up * num;
			if (!downOnly && Voxel.IsOpenAndWalkable(xYZ2))
			{
				xYZ = xYZ2;
				break;
			}
			XYZ xYZ3 = xYZ - XYZ.up * num;
			if (Voxel.IsOpenAndWalkable(xYZ3))
			{
				xYZ = xYZ3;
				break;
			}
		}
		int niveaux = Voxel.GetNiveaux(xYZ);
		Vector3 vector2 = position - vector;
		float num2 = Voxel.ComputeSurfaceHeightFromNiveaux(niveaux, vector2.x, vector2.z);
		position.y = (float)xYZ.y + num2;
		return position;
	}

	private static Vector3 ExactSurfaceAt(XYZ surface, Vector3 exactPosition)
	{
		Vector3 result = exactPosition.Flatten(surface.ToVector3());
		int niveaux = Voxel.GetNiveaux(surface);
		Vector3 vector = exactPosition - surface.ToVector3();
		float num = Voxel.ComputeSurfaceHeightFromNiveaux(niveaux, vector.x, vector.z);
		result.y += num;
		return result;
	}

	public static bool ExactTerrainSurfaceAt(Vector3 position, out Vector3 result, Vector3 roundToward, int min = 0, int max = 256)
	{
		XYZ xyz = Calcule.RoundVecteurToward(position, roundToward);
		if (TerrainDessus(XZ.FromXYZ(xyz), out var result2, min, max))
		{
			result = ExactSurfaceAt(result2, position);
			return true;
		}
		result = result2.ToVector3();
		return false;
	}

	public static bool VoxelDessus(XZ positionDepart, out XYZ result, VoxelBounds bounds)
	{
		return VoxelDessus(positionDepart, out result, Mathf.FloorToInt(bounds.min.y), Mathf.CeilToInt(bounds.max.y));
	}

	public static bool VoxelDessus(XZ positionDepart, out XYZ result, XYZ min, XYZ max)
	{
		return VoxelDessus(positionDepart, out result, min.y, max.y);
	}

	public static XYZ VoxelDessous(XZ positionDepart)
	{
		return new XYZ(positionDepart.x, VoxelEngine.GetColumnBottom(positionDepart.x, positionDepart.z), positionDepart.z);
	}

	public static bool IsUnderIsland(XYZ pos)
	{
		return VoxelDessous(XZ.FromXYZ(pos)).y > pos.y;
	}

	public static XYZ LowestPointIn(VoxelBounds bounds)
	{
		XYZ result = bounds.Center;
		for (int i = bounds.min.x; i <= bounds.max.x; i++)
		{
			for (int j = bounds.min.z; j <= bounds.max.z; j++)
			{
				int columnBottom = VoxelEngine.GetColumnBottom(i, j);
				if (columnBottom < result.y)
				{
					result = new XYZ(i, columnBottom, j);
				}
			}
		}
		return result;
	}

	public static bool VoxelDessus(XZ from, out XYZ result, int minY = 0, int maxY = 256)
	{
		return SearchDessus(from, out result, terrainOnly: false, minY, maxY);
	}

	public static bool TerrainDessus(XZ from, out XYZ result, int minY = 0, int maxY = 256)
	{
		return SearchDessus(from, out result, terrainOnly: true, minY, maxY);
	}

	private static bool SearchDessus(XZ from, out XYZ result, bool terrainOnly, int minY = 0, int maxY = 256)
	{
		XYZ xYZ = new XYZ(from.x, VoxelEngine.GetColumnTop(from.x, from.z), from.z);
		result = ((!Voxel.IsTerrain(xYZ)) ? SearchUnder(xYZ, minY, terrainOnly) : SearchAbove(xYZ, maxY, terrainOnly));
		return result != XYZ.zero;
	}

	private static XYZ SearchAbove(XYZ start, int maxY = 256, bool terrainOnly = false)
	{
		Func<XYZ, bool> func = ((!terrainOnly) ? ((Func<XYZ, bool>)((XYZ v) => !Voxel.IsNonEmpty(v))) : ((Func<XYZ, bool>)((XYZ v) => !Voxel.IsTerrain(v))));
		for (int num = start.y; num < maxY; num++)
		{
			XYZ xYZ = new XYZ(start.x, num, start.z);
			if (func(xYZ))
			{
				return xYZ - XYZ.up;
			}
		}
		return XYZ.zero;
	}

	private static XYZ SearchUnder(XYZ start, int minY = 0, bool terrainOnly = false)
	{
		Func<XYZ, bool> func = ((!terrainOnly) ? new Func<XYZ, bool>(Voxel.IsNonEmpty) : new Func<XYZ, bool>(Voxel.IsTerrain));
		for (int num = start.y; num > minY; num--)
		{
			XYZ xYZ = new XYZ(start.x, num, start.z);
			if (func(xYZ))
			{
				return xYZ;
			}
		}
		return XYZ.zero;
	}

	public static void SurfaceInBounds(VoxelBounds bounds, ref List<XYZ> positions, ref List<int> niveaux)
	{
		XYZ xYZ = bounds.Center;
		for (int i = 0; i < bounds.Size.x; i++)
		{
			for (int j = 0; j < bounds.Size.z; j++)
			{
				int num = ((i % 2 != 0) ? (bounds.Size.z - j) : j);
				XYZ xyz = new XYZ(bounds.min.x + i, xYZ.y, bounds.min.z + num);
				if (VoxelDessus(XZ.FromXYZ(xyz), out var result, bounds))
				{
					positions.Add(result);
					niveaux.Add(Voxel.GetNiveaux(result));
					xYZ = result;
				}
			}
		}
	}

	public static void TopAndBottomInBounds(VoxelBounds bounds, out XYZ top, out XYZ bottom)
	{
		XYZ xYZ = bounds.Center;
		top = bounds.min;
		bottom = bounds.max;
		for (int i = 0; i < bounds.Size.x; i++)
		{
			for (int j = 0; j < bounds.Size.z; j++)
			{
				int num = ((i % 2 != 0) ? (bounds.Size.z - j) : j);
				Vector3 v = new Vector3(bounds.min.x + i, xYZ.y, bounds.min.z + num);
				if (VoxelDessus(XZ.FromVector3(v), out var result, bounds))
				{
					if (result.y < bottom.y)
					{
						bottom = result;
					}
					else if (result.y > top.y)
					{
						top = result;
					}
					xYZ = result;
				}
			}
		}
	}

	public static XYZ VoxelMarchable(XYZ positionDepart, int maxDist = 20)
	{
		for (int num = 0; num < maxDist; num = -num + ((num >= 0) ? (-1) : 0))
		{
			XYZ xYZ = positionDepart + XYZ.up * num;
			if (Voxel.IsOpenAndWalkable(xYZ))
			{
				return xYZ;
			}
		}
		return XYZ.zero;
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.1.0.9782' (yours is '9.1.0.7988')
