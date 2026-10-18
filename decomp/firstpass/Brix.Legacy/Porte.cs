using System.Collections;
using System.Runtime.InteropServices;
using Brix.Audio;
using Brix.Engine;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Game.Network;
using Brix.Lua;
using Brix.Network;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Legacy;

public class Porte : NetworkGameComponent, IPassThrough
{
	public enum Etat
	{
		Ouverte,
		Fermee
	}

	[SyncVar(hook = "OnDoorStateChanged")]
	public Etat _doorState = Etat.Fermee;

	public Signal<IGameComponent> PassageEngage = new Signal<IGameComponent>();

	private bool _manuelMode;

	private int cross;

	private IntClasse passages;

	private float timeToClose;

	private Animation _childAnimation;

	[SyncVar]
	private bool _locked;

	[LuaDrivable("locked")]
	public bool locked
	{
		get
		{
			return _locked;
		}
		set
		{
			if (!base.isServer)
			{
				User.LocalUser.GetComponent<UNetContextualCmd>().CallCmdLockDoor(base.gameObject, value);
				return;
			}
			if (locked && _doorState == Etat.Ouverte)
			{
				CloseDoor();
			}
			Network_locked = value;
		}
	}

	public Etat Network_doorState
	{
		get
		{
			return _doorState;
		}
		[param: In]
		set
		{
			ref Etat doorState = ref _doorState;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				OnDoorStateChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref doorState, 1u);
		}
	}

	public bool Network_locked
	{
		get
		{
			return _locked;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref _locked, 2u);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_childAnimation = base.transform.GetChild(0).GetComponent<Animation>();
	}

	protected override void Start()
	{
		base.Start();
		PassageEngage.Connect(OnDoorUseRequest);
	}

	private void OnDoorStateChanged(Etat value)
	{
		if (!base.isServer)
		{
			switch (value)
			{
			case Etat.Fermee:
				ForceClose();
				break;
			case Etat.Ouverte:
				ForceOpen();
				break;
			}
		}
	}

	public bool IsOpen()
	{
		return _doorState == Etat.Ouverte;
	}

	public bool IsClosed()
	{
		return _doorState == Etat.Fermee;
	}

	public bool IsMoving()
	{
		return _childAnimation.isPlaying;
	}

	public bool IsWalkable()
	{
		if (_manuelMode)
		{
			return _doorState == Etat.Ouverte;
		}
		return true;
	}

	public bool CanSearchThrough(GameObject who)
	{
		return CanPassThrough(who) || who.GetComponent<Labor>().Profession.CanMakeWay();
	}

	public bool CanPassThrough(GameObject who)
	{
		if (_manuelMode)
		{
			return _doorState == Etat.Ouverte;
		}
		return _doorState == Etat.Ouverte || (!_locked && Affiliation.IsAlliedOrNeutral(base.gameObject, who));
	}

	public int GetNiveauxAt(XYZ voxel)
	{
		return 0;
	}

	public float GetWalkHeight(XYZ voxel, float dx, float dz)
	{
		return -0.5f;
	}

	public void AutomateDoor()
	{
		if (_doorState == Etat.Ouverte && _manuelMode)
		{
			CloseDoor();
		}
		_manuelMode = false;
	}

	public void ForceOpen()
	{
		_manuelMode = true;
		if (_doorState != Etat.Ouverte)
		{
			OpenDoor();
		}
	}

	public void ForceClose()
	{
		_manuelMode = true;
		if (_doorState != Etat.Fermee)
		{
			CloseDoor();
		}
	}

	private void OpenDoor()
	{
		if (!_locked)
		{
			Network_doorState = Etat.Ouverte;
			_childAnimation.Play("Ouvrir");
			SoundEngine.Play("Door_Open", base.gameObject);
		}
	}

	private void CloseDoor()
	{
		Network_doorState = Etat.Fermee;
		_childAnimation.Play("Fermer");
		SoundEngine.Play("Door_Close", base.gameObject);
	}

	private void OnDoorUseRequest(IGameComponent b)
	{
		if (_doorState == Etat.Fermee && !_locked && Affiliation.IsAlliedOrNeutral(base.gameObject, b.faction.gameObject))
		{
			OpenDoor();
			timeToClose = Time.time + 1f;
			StartCoroutine(CloseWhenFree());
		}
		else if (_doorState == Etat.Ouverte)
		{
			timeToClose = Time.time + 1f;
		}
	}

	private IEnumerator CloseWhenFree()
	{
		while (Time.time < timeToClose || IsSomeoneUsingTheDoor())
		{
			yield return new WaitForSeconds(0.2f);
		}
		if (_doorState == Etat.Ouverte)
		{
			CloseDoor();
		}
	}

	private bool IsSomeoneUsingTheDoor()
	{
		return GetComponent<IVolume>().obstacles.Slinq().Any((XYZ i) => VoxelPresence.GetPresenceAt(i).Any());
	}

	private void OnDommage()
	{
		GameObject gameObject = base.gameObject;
		_childAnimation.Play("Vibration2");
		GameObject gameObject2 = Particules.Emettre(EffetsParticules.BreakStones, base.transform.position, base.transform.rotation, 3f);
		gameObject2.transform.Rotate(90f, 0f, 0f);
		if (base.transform.InverseTransformPoint(gameObject.transform.position).y < 0f)
		{
			gameObject2.transform.Rotate(0f, 180f, 0f);
		}
		gameObject2.transform.position += new Vector3(0f, 1.25f, -0.35f);
		Object.Destroy(gameObject2, 3f);
	}

	[LuaDrivable("Open")]
	private static DynValue lua_ForceOpen(ScriptExecutionContext context, CallbackArguments args)
	{
		Porte porte = args.AsComponent<Porte>(0, "Open");
		porte.ForceOpen();
		return DynValue.Void;
	}

	[LuaDrivable("Close")]
	private static DynValue lua_ForceClose(ScriptExecutionContext context, CallbackArguments args)
	{
		Porte porte = args.AsComponent<Porte>(0, "Close");
		porte.ForceClose();
		return DynValue.Void;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write((int)_doorState);
			writer.Write(_locked);
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
			writer.Write((int)_doorState);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(_locked);
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
			_doorState = (Etat)reader.ReadInt32();
			_locked = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			OnDoorStateChanged((Etat)reader.ReadInt32());
		}
		if ((num & 2) != 0)
		{
			_locked = reader.ReadBoolean();
		}
	}
}
