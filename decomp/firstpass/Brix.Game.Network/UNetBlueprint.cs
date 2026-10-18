using System.Collections.Generic;
using Brix.Engine;
using Brix.External.Factories;
using Brix.Game.AI;
using Brix.Game.Beacons;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Input;
using Brix.Lifecycle.Pooling;
using Brix.Transactions;
using Brix.UI.GameSelector;
using Brix.Utils;
using Brix.Utils.Locomotion;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Network;

public class UNetBlueprint : NetworkBehaviour
{
	private static int kCmdCmdPlaceBlueprint;

	private static int kCmdCmdSpawnPlacedBlueprint;

	private static int kCmdCmdSpawnPlacedRopeBridge;

	private static int kCmdCmdDestroyBluePrint;

	private static int kCmdCmdPlaceAnti;

	private static int kCmdCmdSendBlueprintsToNewProject;

	[Command]
	public void CmdPlaceBlueprint(string _Factory, string _name, Vector3 _position, Quaternion _rotation)
	{
		ObjectPoolSingleton.RequestComponent<Blueprint>(out var t, new Factory.AssetKey(_Factory, base.name), _position, _rotation);
		t.State |= PlacementFlags.placed;
		NetworkServer.Spawn(t.gameObject, t.AssetKey.AssetId);
	}

	[Command]
	public void CmdSpawnPlacedBlueprint(string factory, string name, Vector3 position, Quaternion rotation, bool instant, GameObject BuildProject, bool spawnSelected)
	{
		Factory.AssetKey assetKey = new Factory.AssetKey(factory, name);
		_SpawnBlueprint(assetKey, position, rotation, instant, BuildProject, spawnSelected);
	}

	private void _SpawnBlueprint(Factory.AssetKey assetKey, Vector3 position, Quaternion rotation, bool instant, GameObject buildProject, bool spawnSelected)
	{
		Blueprint blueprint = _PrepareSpawnBlueprint(assetKey, position, rotation, spawnSelected);
		if (blueprint != null)
		{
			_FinishSpawnBlueprint(blueprint, instant, buildProject);
		}
	}

	private Blueprint _PrepareSpawnBlueprint(Factory.AssetKey assetKey, Vector3 position, Quaternion rotation, bool spawnSelected)
	{
		ObjectPoolSingleton.RequestComponent<Blueprint>(out var t, assetKey, position, rotation);
		t.affiliation.NetworkUserCreator = base.gameObject;
		t.faction = GetComponent<User>().faction;
		ValidatorComponent component = t.GetComponent<ValidatorComponent>();
		if ((bool)component)
		{
			ValidationResult value = component.Validate();
			if (value.HasFlag(ValidationResult.Obstructed))
			{
				ObjectPoolSingleton.Release(t.gameObject);
				return null;
			}
		}
		UISelectable component2 = t.GetComponent<UISelectable>();
		if ((bool)component2)
		{
			component2.NetworkSelectOnSpawn = spawnSelected;
		}
		return t;
	}

	private GameObject _FinishSpawnBlueprint(Blueprint blueprint, bool instant, GameObject buildProject)
	{
		if (!instant)
		{
			if ((bool)buildProject)
			{
				buildProject.GetComponent<BuildGoalProvider>().AddObject(blueprint.gameObject);
			}
			NetworkServer.Spawn(blueprint.gameObject, blueprint.AssetKey.AssetId);
			return blueprint.gameObject;
		}
		return blueprint.MorphToObject();
	}

	[Command]
	public void CmdSpawnPlacedRopeBridge(Vector3 start, Vector3 end, bool instant, GameObject buildProject, Vector3 placementOffset, bool spawnSelected)
	{
		Quaternion rotation = Quaternion.LookRotation((end - start).Flatten());
		Blueprint blueprint = _PrepareSpawnBlueprint(Blueprints.RopeBridgeFrame, start, rotation, spawnSelected);
		if (!(blueprint != null))
		{
			return;
		}
		XYZ position = blueprint.voxel.position;
		RopeBridgeConfigurator component = blueprint.GetComponent<RopeBridgeConfigurator>();
		component.SetEndPoints(position, position + XYZ.FromVector3(end - start));
		component.CreateVolume(isBlueprint: true);
		component.InitRopes();
		component.CreatePlankColliders();
		component = _FinishSpawnBlueprint(blueprint, instant, buildProject).GetComponent<RopeBridgeConfigurator>();
		if (component == null)
		{
			return;
		}
		List<XYZ> list = CollectionPool.RequestList<XYZ>();
		component.GetPlankPositions(list);
		foreach (XYZ item in list)
		{
			if (!VolumeRegistry.HasVolumesWithPropertyAt(item, VPropertyMask.occupy | VPropertyMask.fantome))
			{
				_SpawnBlueprint(Blueprints.RopeBridgePlank, component.voxel.GetOffsettedPosition(item), rotation, instant, buildProject, spawnSelected);
			}
		}
		list.Dispose();
	}

	[Command]
	public void CmdDestroyBluePrint(GameObject instance)
	{
		_UnspawnBlueprint(instance);
		if (!(instance.GetComponent<FactoryImprint>().AssetKey == Blueprints.RopeBridgeFrame))
		{
			return;
		}
		List<XYZ> list = CollectionPool.RequestList<XYZ>();
		instance.GetComponent<RopeBridgeConfigurator>().GetPlankPositions(list);
		foreach (XYZ item in list)
		{
			IVolume volume = VolumeRegistry.VolumeWithPropertyAt(item, VProperty.fantome, (IVolume v) => v.GetComponent<FactoryImprint>().AssetKey == Blueprints.RopeBridgePlank);
			if (volume != null)
			{
				_UnspawnBlueprint(volume.GameObject);
			}
		}
		list.Dispose();
	}

	private void _UnspawnBlueprint(GameObject instance)
	{
		if (Affiliation.IsOwnedBy(instance, base.gameObject) && instance != null)
		{
			AffiliationComponent component = instance.GetComponent<AffiliationComponent>();
			if (!component || InputModeController.DefaultMode == InputMode.worldEditor || ((bool)component && component.faction == GetComponent<User>().faction))
			{
				NetworkServer.UnSpawn(instance);
				ObjectPoolSingleton.Release(instance);
			}
		}
	}

	[Command]
	public void CmdPlaceAnti(string factory, string name, Vector3 position, Quaternion rotation, bool instant, GameObject BuildProject)
	{
		GameObject gameObject = Transactor.SpawnUNet(new Factory.AssetKey(factory, name), GetComponent<User>().faction, position, rotation);
		Blueprint component = gameObject.GetComponent<Blueprint>();
		component.Network_isAnti = true;
		if ((bool)gameObject.GetComponent<RopeBridgeConfigurator>())
		{
			RopeBridgeConfigurator component2 = gameObject.GetComponent<RopeBridgeConfigurator>();
			RopeBridgeConfigurator ropeBridgeFromFrameAt = RopeBridgeConfigurator.GetRopeBridgeFromFrameAt(XYZ.FromVector3(position + rotation * Vector3.right * 1.5f));
			if (ropeBridgeFromFrameAt != null)
			{
				component2.SetEndPoints(ropeBridgeFromFrameAt.StartPoint, ropeBridgeFromFrameAt.EndPoint);
				component2.CreateVolume(isBlueprint: true);
				component2.InitRopes();
			}
		}
		if (!instant)
		{
			BuildProject.GetComponent<BuildGoalProvider>().AddObject(gameObject);
		}
		else
		{
			component.MorphToObject();
		}
	}

	[Command]
	public void CmdSendBlueprintsToNewProject(GameObject[] blueprints)
	{
		ObjectPoolSingleton.RequestComponent<BeaconDriver>(out var t, Groupes.Construction);
		t.faction = base.gameObject.GetComponent<User>().faction;
		t.affiliation.NetworkUserCreator = base.gameObject;
		if ((bool)t.GetComponent<NetworkIdentity>())
		{
			NetworkServer.Spawn(t.gameObject, Groupes.Construction.AssetId);
		}
		BuildGoalProvider component = t.GetComponent<BuildGoalProvider>();
		foreach (GameObject gameObject in blueprints)
		{
			if (gameObject != null)
			{
				component.AddObject(gameObject);
			}
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdPlaceBlueprint(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPlaceBlueprint called on client.");
		}
		else
		{
			((UNetBlueprint)obj).CmdPlaceBlueprint(reader.ReadString(), reader.ReadString(), reader.ReadVector3(), reader.ReadQuaternion());
		}
	}

	protected static void InvokeCmdCmdSpawnPlacedBlueprint(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSpawnPlacedBlueprint called on client.");
		}
		else
		{
			((UNetBlueprint)obj).CmdSpawnPlacedBlueprint(reader.ReadString(), reader.ReadString(), reader.ReadVector3(), reader.ReadQuaternion(), reader.ReadBoolean(), reader.ReadGameObject(), reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdSpawnPlacedRopeBridge(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSpawnPlacedRopeBridge called on client.");
		}
		else
		{
			((UNetBlueprint)obj).CmdSpawnPlacedRopeBridge(reader.ReadVector3(), reader.ReadVector3(), reader.ReadBoolean(), reader.ReadGameObject(), reader.ReadVector3(), reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdDestroyBluePrint(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDestroyBluePrint called on client.");
		}
		else
		{
			((UNetBlueprint)obj).CmdDestroyBluePrint(reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdPlaceAnti(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPlaceAnti called on client.");
		}
		else
		{
			((UNetBlueprint)obj).CmdPlaceAnti(reader.ReadString(), reader.ReadString(), reader.ReadVector3(), reader.ReadQuaternion(), reader.ReadBoolean(), reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdSendBlueprintsToNewProject(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSendBlueprintsToNewProject called on client.");
		}
		else
		{
			((UNetBlueprint)obj).CmdSendBlueprintsToNewProject(GeneratedNetworkCode._ReadArrayGameObject_None(reader));
		}
	}

	public void CallCmdPlaceBlueprint(string _Factory, string _name, Vector3 _position, Quaternion _rotation)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdPlaceBlueprint called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdPlaceBlueprint(_Factory, _name, _position, _rotation);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdPlaceBlueprint);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(_Factory);
		networkWriter.Write(_name);
		networkWriter.Write(_position);
		networkWriter.Write(_rotation);
		SendCommandInternal(networkWriter, 0, "CmdPlaceBlueprint");
	}

	public void CallCmdSpawnPlacedBlueprint(string factory, string name, Vector3 position, Quaternion rotation, bool instant, GameObject BuildProject, bool spawnSelected)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSpawnPlacedBlueprint called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSpawnPlacedBlueprint(factory, name, position, rotation, instant, BuildProject, spawnSelected);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSpawnPlacedBlueprint);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(factory);
		networkWriter.Write(name);
		networkWriter.Write(position);
		networkWriter.Write(rotation);
		networkWriter.Write(instant);
		networkWriter.Write(BuildProject);
		networkWriter.Write(spawnSelected);
		SendCommandInternal(networkWriter, 0, "CmdSpawnPlacedBlueprint");
	}

	public void CallCmdSpawnPlacedRopeBridge(Vector3 start, Vector3 end, bool instant, GameObject buildProject, Vector3 placementOffset, bool spawnSelected)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSpawnPlacedRopeBridge called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSpawnPlacedRopeBridge(start, end, instant, buildProject, placementOffset, spawnSelected);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSpawnPlacedRopeBridge);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(start);
		networkWriter.Write(end);
		networkWriter.Write(instant);
		networkWriter.Write(buildProject);
		networkWriter.Write(placementOffset);
		networkWriter.Write(spawnSelected);
		SendCommandInternal(networkWriter, 0, "CmdSpawnPlacedRopeBridge");
	}

	public void CallCmdDestroyBluePrint(GameObject instance)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdDestroyBluePrint called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdDestroyBluePrint(instance);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdDestroyBluePrint);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(instance);
		SendCommandInternal(networkWriter, 0, "CmdDestroyBluePrint");
	}

	public void CallCmdPlaceAnti(string factory, string name, Vector3 position, Quaternion rotation, bool instant, GameObject BuildProject)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdPlaceAnti called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdPlaceAnti(factory, name, position, rotation, instant, BuildProject);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdPlaceAnti);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(factory);
		networkWriter.Write(name);
		networkWriter.Write(position);
		networkWriter.Write(rotation);
		networkWriter.Write(instant);
		networkWriter.Write(BuildProject);
		SendCommandInternal(networkWriter, 0, "CmdPlaceAnti");
	}

	public void CallCmdSendBlueprintsToNewProject(GameObject[] blueprints)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSendBlueprintsToNewProject called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSendBlueprintsToNewProject(blueprints);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSendBlueprintsToNewProject);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		GeneratedNetworkCode._WriteArrayGameObject_None(networkWriter, blueprints);
		SendCommandInternal(networkWriter, 0, "CmdSendBlueprintsToNewProject");
	}

	static UNetBlueprint()
	{
		kCmdCmdPlaceBlueprint = 1068363479;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetBlueprint), kCmdCmdPlaceBlueprint, InvokeCmdCmdPlaceBlueprint);
		kCmdCmdSpawnPlacedBlueprint = -1555257562;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetBlueprint), kCmdCmdSpawnPlacedBlueprint, InvokeCmdCmdSpawnPlacedBlueprint);
		kCmdCmdSpawnPlacedRopeBridge = 1908910312;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetBlueprint), kCmdCmdSpawnPlacedRopeBridge, InvokeCmdCmdSpawnPlacedRopeBridge);
		kCmdCmdDestroyBluePrint = -221371804;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetBlueprint), kCmdCmdDestroyBluePrint, InvokeCmdCmdDestroyBluePrint);
		kCmdCmdPlaceAnti = -216865634;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetBlueprint), kCmdCmdPlaceAnti, InvokeCmdCmdPlaceAnti);
		kCmdCmdSendBlueprintsToNewProject = -1705606841;
		NetworkBehaviour.RegisterCommandDelegate(typeof(UNetBlueprint), kCmdCmdSendBlueprintsToNewProject, InvokeCmdCmdSendBlueprintsToNewProject);
		NetworkCRC.RegisterBehaviour("UNetBlueprint", 0);
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
