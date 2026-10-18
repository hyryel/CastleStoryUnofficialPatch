using System.Runtime.InteropServices;
using Brix.Common.Collections;
using Brix.Engine;
using Brix.Game.AI;
using Brix.Game.AI.Damage;
using Brix.Network;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Networking;

public class Sticky : NetworkGameComponent
{
	public float StickingPenetration = 0.5f;

	[SerializeField]
	private KinematicProjectile projectile;

	[SerializeField]
	private Transferable transferable;

	[SyncVar(hook = "projectileHitPosChanged")]
	protected Vector3 ProjectileHitPos;

	[SyncVar(hook = "hitObjectChanged")]
	private GameObject networkHitObject;

	[SyncVar(hook = "hitVoxelChanged")]
	private Vector3 hitVoxel;

	[SyncVar]
	private Vector3 hitNormal;

	private NetworkInstanceId ___networkHitObjectNetId;

	public Vector3 NetworkProjectileHitPos
	{
		get
		{
			return ProjectileHitPos;
		}
		[param: In]
		set
		{
			ref Vector3 projectileHitPos = ref ProjectileHitPos;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				projectileHitPosChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref projectileHitPos, 1u);
		}
	}

	public GameObject NetworknetworkHitObject
	{
		get
		{
			return networkHitObject;
		}
		[param: In]
		set
		{
			ref GameObject gameObjectField = ref networkHitObject;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				hitObjectChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVarGameObject(value, ref gameObjectField, 2u, ref ___networkHitObjectNetId);
		}
	}

	public Vector3 NetworkhitVoxel
	{
		get
		{
			return hitVoxel;
		}
		[param: In]
		set
		{
			ref Vector3 fieldValue = ref hitVoxel;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				hitVoxelChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 4u);
		}
	}

	public Vector3 NetworkhitNormal
	{
		get
		{
			return hitNormal;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref hitNormal, 8u);
		}
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		if (NetworkServer.active)
		{
			NetworkServer.UnSpawn(base.gameObject);
		}
		NetworkProjectileHitPos = Vector3.zero;
		NetworknetworkHitObject = null;
		NetworkhitVoxel = Vector3.zero;
		NetworkhitNormal = Vector3.zero;
	}

	public bool TryStick(RaycastHit hit, DamageReceiver receiver)
	{
		projectile.MoveToImpact(hit);
		return TryStickInObject(receiver) || TryStickInVoxel(hit);
	}

	private bool TryStickInVoxel(RaycastHit hit)
	{
		XYZ position = XYZ.FromVector3(hit.point - hit.normal * 0.1f);
		NetworkhitVoxel = hit.point;
		NetworkhitNormal = hit.normal;
		if (Voxel.IsNonEmpty(position))
		{
			StickInVoxel(hit.point, hit.normal);
			return true;
		}
		return false;
	}

	private void StickInVoxel(Vector3 impactPoint, Vector3 impactNormal)
	{
		Vector3 vector = Vector3.Slerp(base.transform.forward, -impactNormal, 0.3f);
		base.transform.rotation = Quaternion.LookRotation(vector, base.transform.up);
		base.transform.position = impactPoint + vector * StickingPenetration;
	}

	private bool TryStickInObject(DamageReceiver receiver)
	{
		if (receiver == null || !receiver.HasStickyParts())
		{
			return false;
		}
		Transform nearestStickyPart = GetNearestStickyPart(receiver.StickyParts, ProjectileHitPos);
		if (nearestStickyPart != null)
		{
			StickInObject(nearestStickyPart, ProjectileHitPos);
			return true;
		}
		return false;
	}

	private static Transform GetNearestStickyPart(GameObject[] stickyParts, Vector3 projectileStickPos)
	{
		Transform transform = null;
		float num = 0f;
		foreach (GameObject item in stickyParts.Slinq().NoLeak())
		{
			Transform transform2 = item.transform;
			float num2 = Vector3.Distance(projectileStickPos, transform2.position);
			if (transform == null || num2 < num)
			{
				transform = transform2;
				num = num2;
			}
		}
		return transform;
	}

	private void StickInObject(Transform stickyPart, Vector3 projectileStickPos)
	{
		base.transform.position = stickyPart.position;
		base.transform.parent = stickyPart;
		NetworknetworkHitObject = stickyPart.GetComponentInParent<NetworkIdentity>().gameObject;
		NetworkProjectileHitPos = stickyPart.position;
	}

	private void projectileHitPosChanged(Vector3 value)
	{
		NetworkProjectileHitPos = value;
	}

	private void hitVoxelChanged(Vector3 value)
	{
		NetworkhitVoxel = value;
		if (!NetworkServer.active && hitVoxel != Vector3.zero)
		{
			StickInVoxel(hitVoxel, hitNormal);
			projectile.Deactivate();
		}
	}

	private void hitObjectChanged(GameObject value)
	{
		NetworknetworkHitObject = value;
		if (!NetworkServer.active)
		{
			if (networkHitObject != null)
			{
				TryStickInObject(SignificantParent<DamageReceiver>.Of(networkHitObject));
				projectile.Deactivate();
			}
			else
			{
				projectile.Physicalize();
			}
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write(ProjectileHitPos);
			writer.Write(networkHitObject);
			writer.Write(hitVoxel);
			writer.Write(hitNormal);
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
			writer.Write(ProjectileHitPos);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(networkHitObject);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(hitVoxel);
		}
		if ((base.syncVarDirtyBits & 8) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(hitNormal);
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
			ProjectileHitPos = reader.ReadVector3();
			___networkHitObjectNetId = reader.ReadNetworkId();
			hitVoxel = reader.ReadVector3();
			hitNormal = reader.ReadVector3();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			projectileHitPosChanged(reader.ReadVector3());
		}
		if ((num & 2) != 0)
		{
			hitObjectChanged(reader.ReadGameObject());
		}
		if ((num & 4) != 0)
		{
			hitVoxelChanged(reader.ReadVector3());
		}
		if ((num & 8) != 0)
		{
			hitNormal = reader.ReadVector3();
		}
	}

	public override void PreStartClient()
	{
		if (!___networkHitObjectNetId.IsEmpty())
		{
			NetworknetworkHitObject = ClientScene.FindLocalObject(___networkHitObjectNetId);
		}
	}
}
