using Brix.Engine;
using Brix.External.Factories;
using Brix.Game.AI;
using Brix.Game.Beacons;
using Brix.Game.Components;
using Brix.Input;
using Brix.Lifecycle.Pooling;
using Brix.UI.GameSelector;
using Smooth.Slinq;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Network;

public class UNetGroupe : NetworkBehaviour
{
	private static int kCmdCmdSpawnGoupe;

	private static int kCmdCmdSpawnHarvestProject;

	private static int kCmdCmdSpawnMineProject;

	private static int kCmdCmdSpawnLandscapeProject;

	private static int kCmdCmdSpawnRaidGoupeOccupy;

	private static int kCmdCmdCreate;

	private static int kCmdCmdCommit;

	private static int kCmdCmdCancelCommit;

	private static int kCmdCmdSetMine;

	private static int kCmdCmdSetLandscape;

	private static int kCmdCmdDelete;

	private static int kCmdCmdAddProjectCapacity;

	private static int kCmdCmdRemoveProjectCapacity;

	private static int kCmdCmdRaisePriority;

	private static int kCmdCmdLowerPriority;

	public void CmdSpawnGoupe(string _factory, string _name, bool spawnSelected)
	{
		CallCmdSpawnGoupe(_factory, _name, default(Vector3), default(Quaternion), spawnSelected);
	}

	[Command]
	public void CmdSpawnGoupe(string _factory, string _name, Vector3 position, Quaternion rotation, bool spawnSelected)
	{
		GameObject gameObject = CreateGroup(new Factory.AssetKey(_factory, _name), position, spawnSelected);
		if ((bool)gameObject.GetComponent<NetworkIdentity>())
		{
			NetworkServer.Spawn(gameObject, gameObject.GetComponent<AssetKeyHolder>().AssetKey.AssetId);
		}
	}

	[Command]
	public void CmdSpawnHarvestProject(Vector3 position, Quaternion rotation, bool spawnSelected, int acceptedTypes)
	{
		GameObject gameObject = CreateGroup(Groupes.Harvest, position, spawnSelected);
		gameObject.GetComponent<HarvestData>().Set(acceptedTypes);
		if ((bool)gameObject.GetComponent<NetworkIdentity>())
		{
			NetworkServer.Spawn(gameObject, gameObject.GetComponent<AssetKeyHolder>().AssetKey.AssetId);
		}
	}

	[Command]
	public void CmdSpawnMineProject(Vector3 position, Quaternion rotation, bool spawnSelected)
	{
		GameObject gameObject = CreateGroup(Groupes.Mine, position, spawnSelected);
		gameObject.GetComponent<QuarryGoalProvider2>().SetDefault(position);
		if ((bool)gameObject.GetComponent<NetworkIdentity>())
		{
			NetworkServer.Spawn(gameObject, gameObject.GetComponent<AssetKeyHolder>().AssetKey.AssetId);
		}
	}

	[Command]
	public void CmdSpawnLandscapeProject(Vector3 position, Quaternion rotation, bool spawnSelected)
	{
		GameObject gameObject = CreateGroup(Groupes.Landscape, position, spawnSelected);
		gameObject.GetComponent<LandscapeGoalProvider>().SetDefault(position);
		if ((bool)gameObject.GetComponent<NetworkIdentity>())
		{
			NetworkServer.Spawn(gameObject, gameObject.GetComponent<AssetKeyHolder>().AssetKey.AssetId);
		}
	}

	private GameObject CreateGroup(Factory.AssetKey key, Vector3 position, bool spawnSelected)
	{
		ObjectPoolSingleton.RequestComponent<BeaconDriver>(out var t, key, Vector3.zero, Quaternion.identity);
		t.faction = base.gameObject.GetComponent<User>().faction;
		t.affiliation.NetworkUserCreator = base.gameObject;
		t.NetworkNetworkPosition = position;
		UISelectable component = t.GetComponent<UISelectable>();
		if ((bool)component)
		{
			component.NetworkSelectOnSpawn = spawnSelected;
		}
		return t.GameObject;
	}

	[Command]
	public void CmdSpawnRaidGoupeOccupy(string _factory, string _name, Vector3 position, Quaternion rotation, GameObject[] laborToAdd)
	{
		ObjectPoolSingleton.RequestComponent<RaidProject2>(out var project, new Factory.AssetKey(_factory, _name), position, rotation);
		project.GetComponent<AffiliationComponent>().NetworkUserCreator = base.gameObject;
		project.faction = base.gameObject.GetComponent<User>().faction;
		NetworkServer.Spawn(project.gameObject, project.GetComponent<AssetKeyHolder>().AssetKey.AssetId);
		(from o in laborToAdd.Slinq()
			where o != null
			select o).ForEach(delegate(GameObject o)
		{
			project.Hire(o.GetComponent<Labor>());
		});
	}

	[Command]
	public void CmdCreate()
	{
		CmdSpawnGoupe("Groupes", "Construction", spawnSelected: true);
	}

	[Command]
	public void CmdCommit(GameObject project)
	{
		if (!project.IsNullOrReleased())
		{
			project.GetComponent<Project>().Commit();
		}
	}

	[Command]
	public void CmdCancelCommit(GameObject project)
	{
		if (!project.IsNullOrReleased())
		{
			project.GetComponent<Project>().CancelCommit();
		}
	}

	[Command]
	public void CmdSetMine(GameObject instance, Vector3 boundsMin, Vector3 boundsMax, Vector3 stairCorner, Vector3 stairDirection, bool commited)
	{
		if (!instance.IsNullOrReleased())
		{
			QuarryData component = instance.GetComponent<QuarryData>();
			QuarryGoalProvider2 component2 = instance.GetComponent<QuarryGoalProvider2>();
			VoxelBounds voxelBounds = new VoxelBounds(XYZ.FromVector3(boundsMin), XYZ.FromVector3(boundsMax));
			Vector3 min = boundsMin;
			min.y = Mathf.Max(Architecte.LowestPointIn(voxelBounds).y, min.y);
			StairOrientation orientation = StairOrientation.From(XYZ.FromVector3(stairCorner), XYZ.FromVector3(stairDirection));
			int y;
			StairOrientation stairOrientation = component2.ValidStairFrom(voxelBounds, orientation, out y);
			component.Set(min, boundsMax, stairOrientation.corner.ToVector3(), stairOrientation.direction.ToVector3(), y, commited);
		}
	}

	[Command]
	public void CmdSetLandscape(GameObject instance, Vector3 min, Vector3 max, int threshold)
	{
		if (!instance.IsNullOrReleased())
		{
			LandscapeData component = instance.GetComponent<LandscapeData>();
			component.Set(min, max, threshold);
		}
	}

	[Command]
	public void CmdDelete(GameObject instance)
	{
		if (!instance.IsNullOrReleased() && Affiliation.IsOwnedBy(instance, base.gameObject))
		{
			NetworkServer.UnSpawn(instance);
			ObjectPoolSingleton.Release(instance);
		}
	}

	[Command]
	public void CmdAddProjectCapacity(GameObject instance)
	{
		if (!instance.IsNullOrReleased())
		{
			instance.GetComponent<Project>().MaximumCrewSize++;
		}
	}

	[Command]
	public void CmdRemoveProjectCapacity(GameObject instance)
	{
		if (!instance.IsNullOrReleased())
		{
			instance.GetComponent<Project>().MaximumCrewSize--;
		}
	}

	[Command]
	public void CmdRaisePriority(GameObject instance)
	{
		if (!instance.IsNullOrReleased())
		{
			instance.GetComponent<Project>().RaisePriority();
		}
	}

	[Command]
	public void CmdLowerPriority(GameObject instance)
	{
		if (!instance.IsNullOrReleased())
		{
			instance.GetComponent<Project>().LowerPriority();
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdSpawnGoupe(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSpawnGoupe called on client.");
		}
		else
		{
			((UNetGroupe)obj).CmdSpawnGoupe(reader.ReadString(), reader.ReadString(), reader.ReadVector3(), reader.ReadQuaternion(), reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdSpawnHarvestProject(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSpawnHarvestProject called on client.");
		}
		else
		{
			((UNetGroupe)obj).CmdSpawnHarvestProject(reader.ReadVector3(), reader.ReadQuaternion(), reader.ReadBoolean(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeCmdCmdSpawnMineProject(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSpawnMineProject called on client.");
		}
		else
		{
			((UNetGroupe)obj).CmdSpawnMineProject(reader.ReadVector3(), reader.ReadQuaternion(), reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdSpawnLandscapeProject(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSpawnLandscapeProject called on client.");
		}
		else
		{
			((UNetGroupe)obj).CmdSpawnLandscapeProject(reader.ReadVector3(), reader.ReadQuaternion(), reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdSpawnRaidGoupeOccupy(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSpawnRaidGoupeOccupy called on client.");
		}
		else
		{
			((UNetGroupe)obj).CmdSpawnRaidGoupeOccupy(reader.ReadString(), reader.ReadString(), reader.ReadVector3(), reader.ReadQuaternion(), GeneratedNetworkCode._ReadArrayGameObject_None(reader));
		}
	}

	protected static void InvokeCmdCmdCreate(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCreate called on client.");
		}
		else
		{
			((UNetGroupe)obj).CmdCreate();
		}
	}

	protected static void InvokeCmdCmdCommit(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCommit called on client.");
		}
		else
		{
			((UNetGroupe)obj).CmdCommit(reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdCancelCommit(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdCancelCommit called on client.");
		}
		else
		{
			((UNetGroupe)obj).CmdCancelCommit(reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdSetMine(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetMine called on client.");
		}
		else
		{
			((UNetGroupe)obj).CmdSetMine(reader.ReadGameObject(), reader.ReadVector3(), reader.ReadVector3(), reader.ReadVector3(), reader.ReadVector3(), reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdSetLandscape(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetLandscape called on client.");
		}
		else
		{
			((UNetGroupe)obj).CmdSetLandscape(reader.ReadGameObject(), reader.ReadVector3(), reader.ReadVector3(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeCmdCmdDelete(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDelete called on client.");
		}
		else
		{
			((UNetGroupe)obj).CmdDelete(reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdAddProjectCapacity(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdAddProjectCapacity called on client.");
		}
		else
		{
			((UNetGroupe)obj).CmdAddProjectCapacity(reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdRemoveProjectCapacity(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRemoveProjectCapacity called on client.");
		}
		else
		{
			((UNetGroupe)obj).CmdRemoveProjectCapacity(reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdRaisePriority(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRaisePriority called on client.");
		}
		else
		{
			((UNetGroupe)obj).CmdRaisePriority(reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdLowerPriority(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdLowerPriority called on client.");
		}
		else
		{
			((UNetGroupe)obj).CmdLowerPriority(reader.ReadGameObject());
		}
	}

	public void CallCmdSpawnGoupe(string _factory, string _name, Vector3 position, Quaternion rotation, bool spawnSelected)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSpawnGoupe called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSpawnGoupe(_factory, _name, position, rotation, spawnSelected);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSpawnGoupe);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(_factory);
		networkWriter.Write(_name);
		networkWriter.Write(position);
		networkWriter.Write(rotation);
		networkWriter.Write(spawnSelected);
		SendCommandInternal(networkWriter, 0, "CmdSpawnGoupe");
	}

	public void CallCmdSpawnHarvestProject(Vector3 position, Quaternion rotation, bool spawnSelected, int acceptedTypes)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSpawnHarvestProject called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSpawnHarvestProject(position, rotation, spawnSelected, acceptedTypes);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSpawnHarvestProject);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(position);
		networkWriter.Write(rotation);
		networkWriter.Write(spawnSelected);
		networkWriter.WritePackedUInt32((uint)acceptedTypes);
		SendCommandInternal(networkWriter, 0, "CmdSpawnHarvestProject");
	}

	public void CallCmdSpawnMineProject(Vector3 position, Quaternion rotation, bool spawnSelected)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSpawnMineProject called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSpawnMineProject(position, rotation, spawnSelected);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSpawnMineProject);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(position);
		networkWriter.Write(rotation);
		networkWriter.Write(spawnSelected);
		SendCommandInternal(networkWriter, 0, "CmdSpawnMineProject");
	}

	public void CallCmdSpawnLandscapeProject(Vector3 position, Quaternion rotation, bool spawnSelected)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSpawnLandscapeProject called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSpawnLandscapeProject(position, rotation, spawnSelected);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSpawnLandscapeProject);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(position);
		networkWriter.Write(rotation);
		networkWriter.Write(spawnSelected);
		SendCommandInternal(networkWriter, 0, "CmdSpawnLandscapeProject");
	}

	public void CallCmdSpawnRaidGoupeOccupy(string _factory, string _name, Vector3 position, Quaternion rotation, GameObject[] laborToAdd)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSpawnRaidGoupeOccupy called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSpawnRaidGoupeOccupy(_factory, _name, position, rotation, laborToAdd);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSpawnRaidGoupeOccupy);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(_factory);
		networkWriter.Write(_name);
		networkWriter.Write(position);
		networkWriter.Write(rotation);
		GeneratedNetworkCode._WriteArrayGameObject_None(networkWriter, laborToAdd);
		SendCommandInternal(networkWriter, 0, "CmdSpawnRaidGoupeOccupy");
	}

	public void CallCmdCreate()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdCreate called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdCreate();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdCreate);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdCreate");
	}

	public void CallCmdCommit(GameObject project)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdCommit called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdCommit(project);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdCommit);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(project);
		SendCommandInternal(networkWriter, 0, "CmdCommit");
	}

	public void CallCmdCancelCommit(GameObject project)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdCancelCommit called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdCancelCommit(project);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdCancelCommit);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(project);
		SendCommandInternal(networkWriter, 0, "CmdCancelCommit");
	}

	public void CallCmdSetMine(GameObject instance, Vector3 boundsMin, Vector3 boundsMax, Vector3 stairCorner, Vector3 stairDirection, bool commited)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetMine called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetMine(instance, boundsMin, boundsMax, stairCorner, stairDirection, commited);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetMine);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(instance);
		networkWriter.Write(boundsMin);
		networkWriter.Write(boundsMax);
		networkWriter.Write(stairCorner);
		networkWriter.Write(stairDirection);
		networkWriter.Write(commited);
		SendCommandInternal(networkWriter, 0, "CmdSetMine");
	}

	public void CallCmdSetLandscape(GameObject instance, Vector3 min, Vector3 max, int threshold)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetLandscape called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetLandscape(instance, min, max, threshold);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetLandscape);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(instance);
		networkWriter.Write(min);
		networkWriter.Write(max);
		networkWriter.WritePackedUInt32((uint)threshold);
		SendCommandInternal(networkWriter, 0, "CmdSetLandscape");
	}

	public void CallCmdDelete(GameObject instance)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdDelete called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdDelete(instance);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdDelete);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(instance);
		SendCommandInternal(networkWriter, 0, "CmdDelete");
	}

	public void CallCmdAddProjectCapacity(GameObject instance)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdAddProjectCapacity called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdAddProjectCapacity(instance);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdAddProjectCapacity);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(instance);
		SendCommandInternal(networkWriter, 0, "CmdAddProjectCapacity");
	}

	public void CallCmdRemoveProjectCapacity(GameObject instance)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdRemoveProjectCapacity called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdRemoveProjectCapacity(instance);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdRemoveProjectCapacity);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(instance);
		SendCommandInternal(networkWriter, 0, "CmdRemoveProjectCapacity");
	}

	public void CallCmdRaisePriority(GameObject instance)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdRaisePriority called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdRaisePriority(instance);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdRaisePriority);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(instance);
		SendCommandInternal(networkWriter, 0, "CmdRaisePriority");
	}

	public void CallCmdLowerPriority(GameObject instance)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdLowerPriority called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdLowerPriority(instance);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdLowerPriority);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(instance);
		SendCommandInternal(networkWriter, 0, "CmdLowerPriority");
	}

	static UNetGroupe()
	{
		kCmdCmdSpawnGoupe = 602611541;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetGroupe), kCmdCmdSpawnGoupe, InvokeCmdCmdSpawnGoupe);
		kCmdCmdSpawnHarvestProject = 1343493309;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetGroupe), kCmdCmdSpawnHarvestProject, InvokeCmdCmdSpawnHarvestProject);
		kCmdCmdSpawnMineProject = 1783931161;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetGroupe), kCmdCmdSpawnMineProject, InvokeCmdCmdSpawnMineProject);
		kCmdCmdSpawnLandscapeProject = -1184346645;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetGroupe), kCmdCmdSpawnLandscapeProject, InvokeCmdCmdSpawnLandscapeProject);
		kCmdCmdSpawnRaidGoupeOccupy = -1380990438;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetGroupe), kCmdCmdSpawnRaidGoupeOccupy, InvokeCmdCmdSpawnRaidGoupeOccupy);
		kCmdCmdCreate = -1402693942;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetGroupe), kCmdCmdCreate, InvokeCmdCmdCreate);
		kCmdCmdCommit = -1405214971;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetGroupe), kCmdCmdCommit, InvokeCmdCmdCommit);
		kCmdCmdCancelCommit = 645063583;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetGroupe), kCmdCmdCancelCommit, InvokeCmdCmdCancelCommit);
		kCmdCmdSetMine = 422385607;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetGroupe), kCmdCmdSetMine, InvokeCmdCmdSetMine);
		kCmdCmdSetLandscape = 1602095047;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetGroupe), kCmdCmdSetLandscape, InvokeCmdCmdSetLandscape);
		kCmdCmdDelete = -1385858183;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetGroupe), kCmdCmdDelete, InvokeCmdCmdDelete);
		kCmdCmdAddProjectCapacity = -2039933728;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetGroupe), kCmdCmdAddProjectCapacity, InvokeCmdCmdAddProjectCapacity);
		kCmdCmdRemoveProjectCapacity = 1706800705;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetGroupe), kCmdCmdRemoveProjectCapacity, InvokeCmdCmdRemoveProjectCapacity);
		kCmdCmdRaisePriority = 1295769634;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetGroupe), kCmdCmdRaisePriority, InvokeCmdCmdRaisePriority);
		kCmdCmdLowerPriority = 1909980727;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetGroupe), kCmdCmdLowerPriority, InvokeCmdCmdLowerPriority);
		NetworkCRC.RegisterBehaviour("UNetGroupe", 0);
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
