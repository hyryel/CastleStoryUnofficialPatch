using System;
using System.Runtime.InteropServices;
using Brix.Components;
using Brix.External.Signals;
using Brix.Game.AI;
using Brix.Game.Components;
using Brix.Utils.Locomotion;
using UnityEngine;
using UnityEngine.Networking;

public class MagicProjectile : NetworkBrixComponent
{
	private struct Trajectory
	{
		public Vector3 origin;

		public Location target;

		public Vector3 initialDelta;

		public Vector3 orthogonalUp;

		public Vector3 targetInitialposition;

		private bool valid;

		private Vector3 lastDestination;

		public Vector3 Destination
		{
			get
			{
				if (target != null && !target.Object.IsNullOrReleased())
				{
					lastDestination = target.RangeHitPosition;
				}
				return lastDestination;
			}
		}

		public float Distance => Vector3.Distance(origin, Destination);

		public Vector3 Direction => (Destination - origin).normalized;

		public static Trajectory From(Vector3 origin, Vector3 targetInitialPosition, Location target)
		{
			Trajectory result = default(Trajectory);
			result.origin = origin;
			result.target = target;
			result.targetInitialposition = targetInitialPosition;
			result.ComputeInitialDelta();
			result.orthogonalUp = Vector3.ProjectOnPlane(Vector3.up, result.initialDelta).normalized;
			return result;
		}

		private void ComputeInitialDelta()
		{
			if (target != null && !target.Object.IsNullOrReleased())
			{
				initialDelta = origin - target.RangeHitPosition;
			}
			else
			{
				initialDelta = origin - targetInitialposition;
			}
		}
	}

	public enum Mode
	{
		Default,
		WardAttack
	}

	public Signal ReachedDestination = new Signal();

	private const float reachedThreshold = 0.98f;

	[SyncVar]
	private Vector3 origin;

	[SyncVar]
	private GameObject targetGO;

	[SyncVar]
	private Vector3 targetInitialposition;

	[SyncVar]
	private float mps;

	[SyncVar]
	private float timeToHit;

	private float timeSinceLaunch;

	[SyncVar]
	private Vector3 startSpeed;

	[SyncVar]
	private Vector3 endSpeed;

	private Trajectory trajectory;

	[SerializeField]
	private Mode mode;

	private NetworkInstanceId ___targetGONetId;

	public Vector3 Networkorigin
	{
		get
		{
			return origin;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref origin, 1u);
		}
	}

	public GameObject NetworktargetGO
	{
		get
		{
			return targetGO;
		}
		[param: In]
		set
		{
			SetSyncVarGameObject(value, ref targetGO, 2u, ref ___targetGONetId);
		}
	}

	public Vector3 NetworktargetInitialposition
	{
		get
		{
			return targetInitialposition;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref targetInitialposition, 4u);
		}
	}

	public float Networkmps
	{
		get
		{
			return mps;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref mps, 8u);
		}
	}

	public float NetworktimeToHit
	{
		get
		{
			return timeToHit;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref timeToHit, 16u);
		}
	}

	public Vector3 NetworkstartSpeed
	{
		get
		{
			return startSpeed;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref startSpeed, 32u);
		}
	}

	public Vector3 NetworkendSpeed
	{
		get
		{
			return endSpeed;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref endSpeed, 64u);
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		if (!NetworkServer.active)
		{
			trajectory = Trajectory.From(origin, targetInitialposition, Location.Of(targetGO));
			NetworktimeToHit = trajectory.Distance / mps;
		}
	}

	public void Init(Vector3 origin, Location target, float spd)
	{
		Networkorigin = origin;
		NetworktargetGO = target.Object;
		if (!target.Object.IsNullOrReleased())
		{
			NetworktargetInitialposition = target.RangeHitPosition;
		}
		trajectory = Trajectory.From(origin, targetInitialposition, target);
		NetworktimeToHit = trajectory.Distance / spd;
		timeSinceLaunch = 0f;
		Networkmps = spd;
		switch (mode)
		{
		case Mode.Default:
			SetOrthogonalDistance(timeToHit * 2.5f);
			break;
		case Mode.WardAttack:
		{
			Vector3 normalized = (targetInitialposition - origin).normalized;
			Vector3 normalized2 = normalized.Flatten().normalized;
			float t = Mathf.Clamp(normalized.y, -0.7f, 0.7f) / 0.7f * 0.5f + 0.5f;
			Vector3 vector = Vector3.Lerp(normalized2, Vector3.up, t).normalized * trajectory.Distance / 2f;
			Vector3 vector2 = Vector3.Lerp(-Vector3.up, normalized2, t).normalized * trajectory.Distance / 2f;
			SetStartVelocity(vector, vector2);
			break;
		}
		}
	}

	public void SetOrthogonalDistance(float maxDistance)
	{
		float num = Mathf.Atan(4f * maxDistance / trajectory.Distance) * 57.29578f;
		SetStartAngle(num, 0f - num);
		float num2 = Vector3.Distance(Evaluate(0.5f), Vector3.Lerp(trajectory.origin, trajectory.Destination, 0.5f));
		float power = ((!(num2 > 0f)) ? 0f : (Mathf.Abs(maxDistance) / num2));
		SetStartAngle(num, 0f - num, power);
	}

	public void SetStartAngle(float startAngle)
	{
		SetStartAngle(startAngle, 0f - startAngle);
	}

	public void SetStartAngle(float startAngle, float endAngle, float power = 1f)
	{
		float f = startAngle * ((float)Math.PI / 180f);
		float f2 = endAngle * ((float)Math.PI / 180f);
		Vector3 vector = trajectory.Direction * power * trajectory.Distance;
		Vector3 vector2 = trajectory.orthogonalUp * power * trajectory.Distance;
		SetStartVelocity(Mathf.Cos(f) * vector + Mathf.Sin(f) * vector2, Mathf.Cos(f2) * vector + Mathf.Sin(f2) * vector2);
	}

	public void SetStartVelocity(Vector3 startSpeed, Vector3 endSpeed)
	{
		NetworkstartSpeed = startSpeed;
		NetworkendSpeed = endSpeed;
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		ReachedDestination.Clear();
		trajectory = default(Trajectory);
		Networkmps = 0f;
		NetworktimeToHit = 0f;
		timeSinceLaunch = 0f;
	}

	private void Magic(float deltaTime)
	{
		timeSinceLaunch += deltaTime;
		float t = timeSinceLaunch / timeToHit;
		Vector3 position = base.transform.position;
		Vector3 vector = Evaluate(t);
		if (HasReachedDestination())
		{
			ReachedDestination.Invoke();
		}
		if (!float.IsNaN(vector.x))
		{
			base.transform.position = vector;
			base.transform.rotation = Quaternion.LookRotation(vector - position);
		}
	}

	private Vector3 Evaluate(float t)
	{
		return (2f * t * t * t - 3f * t * t + 1f) * trajectory.origin + (t * t * t - 2f * t * t + t) * startSpeed + (-2f * t * t * t + 3f * t * t) * trajectory.Destination + (t * t * t - t * t) * endSpeed;
	}

	public void Move(float deltaTime)
	{
		Magic(deltaTime);
	}

	public bool HasReachedDestination()
	{
		return timeSinceLaunch > timeToHit * 0.98f;
	}

	public Vector3 GetVelocity()
	{
		float num = timeSinceLaunch / timeToHit;
		return ((Evaluate(num + 0.01f) - Evaluate(num)) / 0.01f).normalized * mps;
	}

	public void Stick()
	{
		ReachedDestination.Invoke();
		base.transform.rotation = Quaternion.LookRotation(trajectory.initialDelta);
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write(origin);
			writer.Write(targetGO);
			writer.Write(targetInitialposition);
			writer.Write(mps);
			writer.Write(timeToHit);
			writer.Write(startSpeed);
			writer.Write(endSpeed);
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
			writer.Write(origin);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(targetGO);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(targetInitialposition);
		}
		if ((base.syncVarDirtyBits & 8) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(mps);
		}
		if ((base.syncVarDirtyBits & 0x10) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(timeToHit);
		}
		if ((base.syncVarDirtyBits & 0x20) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(startSpeed);
		}
		if ((base.syncVarDirtyBits & 0x40) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(endSpeed);
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
			origin = reader.ReadVector3();
			___targetGONetId = reader.ReadNetworkId();
			targetInitialposition = reader.ReadVector3();
			mps = reader.ReadSingle();
			timeToHit = reader.ReadSingle();
			startSpeed = reader.ReadVector3();
			endSpeed = reader.ReadVector3();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			origin = reader.ReadVector3();
		}
		if ((num & 2) != 0)
		{
			targetGO = reader.ReadGameObject();
		}
		if ((num & 4) != 0)
		{
			targetInitialposition = reader.ReadVector3();
		}
		if ((num & 8) != 0)
		{
			mps = reader.ReadSingle();
		}
		if ((num & 0x10) != 0)
		{
			timeToHit = reader.ReadSingle();
		}
		if ((num & 0x20) != 0)
		{
			startSpeed = reader.ReadVector3();
		}
		if ((num & 0x40) != 0)
		{
			endSpeed = reader.ReadVector3();
		}
	}

	public override void PreStartClient()
	{
		if (!___targetGONetId.IsEmpty())
		{
			NetworktargetGO = ClientScene.FindLocalObject(___targetGONetId);
		}
	}
}
