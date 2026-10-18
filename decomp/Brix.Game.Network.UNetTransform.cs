using System.Runtime.InteropServices;
using Brix.Game.Components;
using Brix.Utils.Signals;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Network;

[NetworkSettings(channel = 1)]
public class UNetTransform : UpdatableNetworkGameComponent
{
	private static readonly UpdatableNetworkProfiler<UNetTransform> UpdatableProfiler = new UpdatableNetworkProfiler<UNetTransform>();

	[SyncVar]
	[SerializeField]
	private Vector3 LastServerPosition;

	[SyncVar]
	private Quaternion LastServerRotation = Quaternion.identity;

	public float SendRate;

	private float lastSend;

	public bool UpdateOnlyOnSleep;

	private RigidbodyTracker rigidBodyTracker;

	private Rigidbody rigidBody;

	private bool _updateToggle;

	private bool reached;

	protected override UpdatableProfiler Updatable => UpdatableProfiler;

	public float InterpolationRate { get; set; }

	private bool UpdateToggle
	{
		get
		{
			return _updateToggle;
		}
		set
		{
			_updateToggle = value;
			if (NetworkServer.active)
			{
				base.enabled = value;
			}
		}
	}

	protected override bool HasUpdate => !NetworkServer.active;

	protected override bool HasFixedUpdate => NetworkServer.active;

	public Vector3 NetworkLastServerPosition
	{
		get
		{
			return LastServerPosition;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref LastServerPosition, 1u);
		}
	}

	public Quaternion NetworkLastServerRotation
	{
		get
		{
			return LastServerRotation;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref LastServerRotation, 2u);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		NetworkLastServerPosition = base.transform.position;
		SendRate = 1f / 6f;
		lastSend = Time.fixedTime;
		InterpolationRate = 9f;
	}

	protected override void Start()
	{
		base.Start();
		if (UpdateOnlyOnSleep)
		{
			UpdateToggle = false;
			rigidBodyTracker = GetComponent<RigidbodyTracker>();
			rigidBody = GetComponent<Rigidbody>();
			rigidBodyTracker.RigidBodyPseudoSleepingSignal.Connect(delegate(UNetTransform self)
			{
				self.OnSleep();
			}, this).DisconnectWhen(base.OnReleaseOrDestroySignal);
			rigidBodyTracker.RigidBodyAwokenSignal.Connect(delegate(UNetTransform self)
			{
				self.OnAwoken();
			}, this).DisconnectWhen(base.OnReleaseOrDestroySignal);
		}
		else
		{
			UpdateToggle = true;
		}
	}

	[ServerCallback]
	private void OnSleep()
	{
		if (NetworkServer.active)
		{
			UpdateToggle = true;
		}
	}

	[ServerCallback]
	private void OnAwoken()
	{
		if (NetworkServer.active)
		{
			UpdateToggle = false;
		}
	}

	protected override void BrixUpdate()
	{
		float num = Vector3.Distance(base.transform.position, LastServerPosition);
		if (num > 10f)
		{
			base.transform.position = LastServerPosition;
			base.transform.rotation = LastServerRotation;
		}
		else if (reached && UpdateOnlyOnSleep && !UpdateToggle)
		{
			if (UpdateOnlyOnSleep)
			{
				rigidBody.isKinematic = false;
			}
			if (num > 1f)
			{
				reached = false;
			}
		}
		else if (UpdateToggle || (!UpdateToggle && UpdateOnlyOnSleep))
		{
			if (UpdateOnlyOnSleep)
			{
				rigidBody.isKinematic = true;
			}
			base.transform.position = Vector3.Lerp(base.transform.position, LastServerPosition, InterpolationRate * Time.deltaTime);
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, LastServerRotation, InterpolationRate * Time.deltaTime);
			if (num < 1f)
			{
				reached = true;
			}
		}
	}

	protected override void BrixFixedUpdate()
	{
		UpdatePosition();
	}

	private void UpdatePosition()
	{
		if (!this.IsNullOrReleased() && Time.fixedTime > lastSend + SendRate)
		{
			if (LastServerRotation != base.transform.rotation)
			{
				NetworkLastServerRotation = base.transform.rotation;
			}
			if (LastServerPosition != base.transform.position)
			{
				NetworkLastServerPosition = base.transform.position;
			}
			lastSend = Time.fixedTime;
		}
	}

	private void UNetVersion()
	{
	}

	public override int GetNetworkChannel()
	{
		return 1;
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write(LastServerPosition);
			writer.Write(LastServerRotation);
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
			writer.Write(LastServerPosition);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(LastServerRotation);
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
			LastServerPosition = reader.ReadVector3();
			LastServerRotation = reader.ReadQuaternion();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			LastServerPosition = reader.ReadVector3();
		}
		if ((num & 2) != 0)
		{
			LastServerRotation = reader.ReadQuaternion();
		}
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.1.0.9782' (yours is '9.1.0.7988')
