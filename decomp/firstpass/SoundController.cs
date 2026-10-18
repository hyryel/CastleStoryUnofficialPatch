using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Brix.Audio;
using Brix.Engine;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Game.Semantique;
using Brix.Network;
using Brix.Utils.Logging;
using Brix.Utils.Signals;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Networking;

public class SoundController : NetworkGameComponent
{
	private struct SoundEventData : IEquatable<SoundEventData>
	{
		public GameObject emitter;

		public string soundEvent;

		public string switchName;

		public string switchValue;

		public bool workIntensityEvent;

		public SoundEventData(string soundEvent, GameObject emitter, string switchName, string switchValue, bool workIntensityEvent)
		{
			this.emitter = emitter;
			this.soundEvent = soundEvent;
			this.switchName = switchName;
			this.switchValue = switchValue;
			this.workIntensityEvent = workIntensityEvent;
		}

		public bool Equals(SoundEventData other)
		{
			return object.Equals(emitter, other.emitter) && string.Equals(soundEvent, other.soundEvent) && string.Equals(switchName, other.switchName) && string.Equals(switchValue, other.switchValue);
		}

		public override bool Equals(object obj)
		{
			if (object.ReferenceEquals(null, obj))
			{
				return false;
			}
			return obj is SoundEventData && Equals((SoundEventData)obj);
		}

		public override int GetHashCode()
		{
			int num = ((emitter != null) ? emitter.GetHashCode() : 0);
			num = (num * 397) ^ ((soundEvent != null) ? soundEvent.GetHashCode() : 0);
			num = (num * 397) ^ ((switchName != null) ? switchName.GetHashCode() : 0);
			return (num * 397) ^ ((switchValue != null) ? switchValue.GetHashCode() : 0);
		}
	}

	private delegate void SyncVoxelHitsoundDelegate(Vector3 positionVector3, VoxelSurface surface, bool destroyed);

	private static BrixLogger logger;

	public string OnCreatedEvent;

	public string OnDropEvent;

	public string OnPickupEvent;

	public string OnDestroyEvent;

	public bool PlayRecepteurSounds;

	public bool PlayToolbagSounds;

	[SyncVar(hook = "OnCreated_Hook")]
	private bool created;

	private HashSet<SoundEventData> soundEventPool = new HashSet<SoundEventData>();

	private static int kEventEventSyncVoxelHitSound;

	public bool Networkcreated
	{
		get
		{
			return created;
		}
		[param: In]
		set
		{
			ref bool fieldValue = ref created;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				OnCreated_Hook(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 1u);
		}
	}

	[SyncEvent]
	private event SyncVoxelHitsoundDelegate EventSyncVoxelHitSound;

	public override void OnStartClient()
	{
		base.OnStartClient();
		Networkcreated = true;
		if (PlayRecepteurSounds)
		{
			Recepteur component = GetComponent<Recepteur>();
			if ((bool)component)
			{
				component.OnResourceAdded.Connect(OnRecepteurResourceAdded).DisconnectWhen(base.OnReleaseOrDestroySignal);
				component.OnResourceRemoved.Connect(OnRecepteurResourceRemoved).DisconnectWhen(base.OnReleaseOrDestroySignal);
			}
			else
			{
				logger.ErrorFormat("Missing Recepteur on {0}, could not initialize the sound controller.", base.gameObject.name);
			}
		}
		if (PlayToolbagSounds)
		{
			Toolbag component2 = GetComponent<Toolbag>();
			if ((bool)component2)
			{
				component2.OnToolEquiped.Connect(OnToolbagToolEquiped).DisconnectWhen(base.OnReleaseOrDestroySignal);
				component2.OnToolUnequiped.Connect(OnToolbagToolUnequiped).DisconnectWhen(base.OnReleaseOrDestroySignal);
			}
			else
			{
				logger.ErrorFormat("Missing toolbag on {0}, could not initialize the sound controller.", base.gameObject.name);
			}
		}
		EventSyncVoxelHitSound += PlayVoxelHitSound;
	}

	private void FixedUpdate()
	{
		PlayQueuedEvents();
	}

	private void PlayQueuedEvents()
	{
		if (soundEventPool.Count > 0)
		{
			soundEventPool.Slinq().ForEach(delegate(SoundEventData s)
			{
				if (!string.IsNullOrEmpty(s.switchName))
				{
					SoundEngine.SetSwitch(s.switchName, s.switchValue, s.emitter);
				}
				SoundEngine.Play(s.soundEvent, s.emitter);
				if (s.workIntensityEvent)
				{
					GameplayAudio.AddWorkIntensity(s.emitter.transform.position, 1f);
				}
			});
			soundEventPool.Clear();
		}
		base.enabled = false;
	}

	public void QueueSound(string soundEvent, GameObject emitter, string switchName = "", string switchValue = "", bool workIntensityEvent = false)
	{
		soundEventPool.Add(new SoundEventData(soundEvent, emitter, switchName, switchValue, workIntensityEvent));
		base.enabled = true;
	}

	private void PlayObjectPickupSound(GameObject obj)
	{
		SoundController component = obj.GetComponent<SoundController>();
		if ((bool)component && !string.IsNullOrEmpty(component.OnPickupEvent))
		{
			QueueSound(component.OnPickupEvent, obj, string.Empty, string.Empty);
		}
	}

	private void PlayObjectDropingSound(GameObject obj)
	{
		SoundController component = obj.GetComponent<SoundController>();
		if ((bool)component && !string.IsNullOrEmpty(component.OnDropEvent))
		{
			QueueSound(component.OnDropEvent, obj, string.Empty, string.Empty);
		}
	}

	public void BroadcastVoxelHitSound(Vector3 voxelPosition, VoxelSurface surface, bool destroyed)
	{
		CallEventSyncVoxelHitSound(voxelPosition, surface, destroyed);
	}

	public void PlayVoxelHitSound(Vector3 voxelPosition, VoxelSurface surface, bool destroyed)
	{
		Labor component = GetComponent<Labor>();
		if ((bool)component)
		{
			QueueSound((!destroyed) ? component.Sounds.HitsSurface : component.Sounds.DestroysSurface, base.gameObject, "BrickHitting_Type", Voxel.GetSurfaceName(surface));
		}
	}

	private void OnToolbagToolUnequiped(GameObject obj)
	{
		PlayObjectDropingSound(obj);
	}

	private void OnToolbagToolEquiped(GameObject obj)
	{
		PlayObjectPickupSound(obj);
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		this.EventSyncVoxelHitSound = null;
		if (NetworkClient.active && !string.IsNullOrEmpty(OnDestroyEvent))
		{
			QueueSound(OnDestroyEvent, base.gameObject, string.Empty, string.Empty);
			PlayQueuedEvents();
		}
	}

	private void OnCreated_Hook(bool value)
	{
		Networkcreated = value;
		if (created && !string.IsNullOrEmpty(OnCreatedEvent))
		{
			QueueSound(OnCreatedEvent, base.gameObject, string.Empty, string.Empty);
			PlayQueuedEvents();
		}
	}

	private void OnRecepteurResourceRemoved(IDescriptor obj)
	{
		Labor component = GetComponent<Labor>();
		if ((bool)component)
		{
			PlayAdjectifStoringSound(obj.GameObject, component);
		}
	}

	private void OnRecepteurResourceAdded(IDescriptor obj)
	{
		Labor component = GetComponent<Labor>();
		if ((bool)component)
		{
			PlayAdjectifPickingSound(obj.GameObject, component);
		}
	}

	private void PlayAdjectifStoringSound(GameObject resource, Labor labor)
	{
		string objectTypeSwitch = GetObjectTypeSwitch(resource.GetComponent<GameComponent>());
		QueueSound(labor.Sounds.DropObject, base.gameObject, "Material", objectTypeSwitch, workIntensityEvent: true);
	}

	private void PlayAdjectifPickingSound(GameObject resource, Labor labor)
	{
		string objectTypeSwitch = GetObjectTypeSwitch(resource.GetComponent<GameComponent>());
		QueueSound(labor.Sounds.GrabsObject, base.gameObject, "Material", objectTypeSwitch, workIntensityEvent: true);
	}

	public static string GetObjectTypeSwitch(GameComponent obj)
	{
		if ((bool)obj && obj.recepteur != null)
		{
			return GetObjectTypeSwitch(obj.recepteur.ContentDescription);
		}
		if ((bool)obj && obj.Descriptor != null)
		{
			return GetObjectTypeSwitch(obj.Descriptor.Description);
		}
		return string.Empty;
	}

	public static string GetObjectTypeSwitch(Description d)
	{
		if (d.Has<Stones>())
		{
			return "Garnotte";
		}
		if (d.Has<WoodBlock>())
		{
			return "Wood";
		}
		if (d.Has<Plant>())
		{
			return "Plant";
		}
		if (d.Has<PlankBlock>())
		{
			return "Plank";
		}
		if (d.Has<StoneBlock>())
		{
			return "Stone";
		}
		if (d.Has<OrangeCrystal>())
		{
			return "OrangeCrystal";
		}
		if (d.Has<BlueCrystal>())
		{
			return "BlueCrystal";
		}
		if (d.Has<RawIron>())
		{
			return "Garnotte_Iron";
		}
		if (d.Has<Iron>())
		{
			return "Iron";
		}
		if (d.Has<Glass>())
		{
			return "Glass";
		}
		if (d.Has<Bow>() || d.Has<Cap>() || d.Has<Quiver>())
		{
			return "Kit_Archer";
		}
		if (d.Has<Sword>() || d.Has<Shield>() || d.Has<Helmet>())
		{
			return "Kit_Knight";
		}
		if (d.Has<Rope>())
		{
			return "Rope";
		}
		if (d.Has<Fabric>())
		{
			return "Fabric";
		}
		if (d.Has<ExplosiveBarrel>())
		{
			return "Barrel";
		}
		if (d.Has<Ward>())
		{
			return "Ward";
		}
		if (d.Has<Cog>())
		{
			return "Cog";
		}
		if (d.Has<Bomb>())
		{
			return "Bomb";
		}
		if (d.Has<BearTrap>())
		{
			return "BearTrap";
		}
		if (d.Has<CrystalObjective>())
		{
			return "Gem";
		}
		return string.Empty;
	}

	static SoundController()
	{
		logger = new BrixLogger(typeof(SoundController).FullName);
		kEventEventSyncVoxelHitSound = 2073956124;
		NetworkBehaviour.RegisterEventDelegate(typeof(SoundController), kEventEventSyncVoxelHitSound, InvokeSyncEventEventSyncVoxelHitSound);
		NetworkCRC.RegisterBehaviour("SoundController", 0);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeSyncEventEventSyncVoxelHitSound(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Event EventSyncVoxelHitSound called on server.");
		}
		else if (((SoundController)obj).EventSyncVoxelHitSound != null)
		{
			((SoundController)obj).EventSyncVoxelHitSound(reader.ReadVector3(), (VoxelSurface)reader.ReadInt32(), reader.ReadBoolean());
		}
	}

	public void CallEventSyncVoxelHitSound(Vector3 positionVector3, VoxelSurface surface, bool destroyed)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Event EventSyncVoxelHitSound called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)7);
		networkWriter.WritePackedUInt32((uint)kEventEventSyncVoxelHitSound);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(positionVector3);
		networkWriter.Write((int)surface);
		networkWriter.Write(destroyed);
		SendEventInternal(networkWriter, 0, "EventSyncVoxelHitSound");
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write(created);
			return true;
		}
		bool flag2 = false;
		if ((base.syncVarDirtyBits & 1) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(created);
		}
		if (!flag2)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
		if (initialState)
		{
			created = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			OnCreated_Hook(reader.ReadBoolean());
		}
	}
}
