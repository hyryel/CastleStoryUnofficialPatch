using System.Runtime.InteropServices;
using Brix.Engine;
using Brix.Engine.Abstract;
using Brix.Util;
using Brix.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components;

public class BaseVoxelTransform : UpdatableNetworkGameComponent
{
	[SerializeField]
	protected Vector3 centerOffset = new Vector3(0f, 0f, 0f);

	[SerializeField]
	[SyncVar]
	protected Vector3 placementOffset = new Vector3(0f, 0f, 0f);

	[SerializeField]
	protected Vector3 rangeTargetOffset = new Vector3(0f, 0f, 0f);

	[SyncVar(hook = "OnRotationChanged")]
	public Quaternion NetworkRotation;

	public virtual Vector3 RangeTargetOffset
	{
		get
		{
			return rangeTargetOffset;
		}
		set
		{
			rangeTargetOffset = value;
		}
	}

	public virtual Vector3 PlacementOffset
	{
		get
		{
			return placementOffset;
		}
		set
		{
			NetworkplacementOffset = value;
		}
	}

	public Vector3 CenterOffset => centerOffset + placementOffset;

	public Vector3 InitialCenterOffset
	{
		get
		{
			return centerOffset;
		}
		set
		{
			centerOffset = value;
		}
	}

	public int index => position.ToScalaire();

	public XYZ position
	{
		get
		{
			return PositionToXYZ();
		}
		set
		{
			XYZToPosition(value);
		}
	}

	public AxisOrient AxisOrient => (AxisOrient)RotationToFaceIndex();

	public int RotationIndex
	{
		get
		{
			return RotationToRotationIndex();
		}
		set
		{
			FaceIndexToRotation(value);
		}
	}

	public Quaternion CanonicalRotation => Quaternion.LookRotation(Calcule.AbsVecteur(base.transform.rotation * Vector3.forward), base.transform.rotation * Vector3.up);

	public Vector3 NetworkplacementOffset
	{
		get
		{
			return placementOffset;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref placementOffset, 1u);
		}
	}

	public Quaternion NetworkNetworkRotation
	{
		get
		{
			return NetworkRotation;
		}
		[param: In]
		set
		{
			ref Quaternion networkRotation = ref NetworkRotation;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				OnRotationChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref networkRotation, 2u);
		}
	}

	public override void OnStartServer()
	{
		NetworkNetworkRotation = base.transform.rotation;
	}

	private void OnRotationChanged(Quaternion rotation)
	{
		NetworkNetworkRotation = rotation;
		if (!NetworkServer.active)
		{
			base.transform.rotation = rotation;
		}
	}

	public virtual bool MoveTransform(Vector3 p)
	{
		if (p == base.transform.position)
		{
			return false;
		}
		base.transform.position = p;
		return true;
	}

	public virtual bool MovePivot(XYZ p)
	{
		if (position == p)
		{
			return false;
		}
		position = p;
		return true;
	}

	protected virtual XYZ PositionToXYZ()
	{
		return XYZ.FromVector3(base.transform.position + base.transform.TransformDirection(CenterOffset));
	}

	protected virtual int RotationToFaceIndex()
	{
		return Calcule.VecteurToFaceIndex(Calcule.GetDiscreteVector(base.transform.forward));
	}

	protected virtual void XYZToPosition(XYZ xyz)
	{
		base.transform.position = xyz.ToVector3() - base.transform.TransformDirection(CenterOffset);
	}

	protected virtual void FaceIndexToRotation(int faceIndex)
	{
		base.transform.rotation = GetRotation(faceIndex);
	}

	protected virtual int RotationToRotationIndex()
	{
		return PreprocessedRotations.RotationToDiscreteIndex(base.transform.rotation);
	}

	protected virtual void RotationIndexToRotation(int rotationIndex)
	{
		base.transform.rotation = PreprocessedRotations.DiscreteRotationForIndexes[rotationIndex];
	}

	public virtual Quaternion GetSnappedRotation()
	{
		return GetRotation((int)AxisOrient);
	}

	public Vector3 GetOffsettedPosition()
	{
		return GetOffsettedPosition(base.transform.position, base.transform.rotation);
	}

	public Vector3 GetRangeOffsettedPosition()
	{
		return GetRangeOffsettedPosition(base.transform.position, base.transform.rotation);
	}

	public Vector3 GetRangeOffsettedPosition(Vector3 p, Quaternion r)
	{
		return p + r * rangeTargetOffset;
	}

	public Vector3 GetOffsettedPosition(XYZ p)
	{
		return GetOffsettedPosition(p.ToVector3(), base.transform.rotation);
	}

	public Vector3 GetOffsettedPosition(Vector3 p)
	{
		return GetOffsettedPosition(p, base.transform.rotation);
	}

	public Vector3 GetOffsettedPosition(Vector3 p, Quaternion r)
	{
		return p - r * CenterOffset;
	}

	public int LocalToGlobalSafe(int value)
	{
		Vector3 signedVector = Calcule.GetSignedVector(value);
		signedVector += CenterOffset;
		Vector3 vecteur = TransformPoint(signedVector);
		vecteur -= CenterOffset;
		return Brix.Engine.Abstract.Voxel.GetScalaire(Calcule.RoundVecteur(vecteur));
	}

	public virtual Vector3 TransformPoint(Vector3 v)
	{
		return base.transform.TransformPoint(v) + CenterOffset;
	}

	public Vector3 InverseTransformPoint(Vector3 v)
	{
		return base.transform.InverseTransformPoint(v - CenterOffset);
	}

	protected static Quaternion GetRotation(int axisOrient)
	{
		if (axisOrient < 0 || axisOrient >= 6)
		{
			axisOrient = 1;
		}
		return PreprocessedRotations.RotationForAxisOrients[axisOrient];
	}

	public override void OnRequested()
	{
		base.OnRequested();
		if (NetworkServer.active)
		{
			NetworkNetworkRotation = base.transform.rotation;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (NetworkServer.active)
		{
			NetworkNetworkRotation = base.transform.rotation;
		}
	}

	public override void OnStartClient()
	{
		base.OnStartClient();
		if (!NetworkServer.active)
		{
			Vector3 vector = base.transform.position;
			base.transform.rotation = NetworkRotation;
			base.transform.position = vector;
		}
	}

	public override void ResetComponent()
	{
		base.ResetComponent();
		NetworkNetworkRotation = Quaternion.identity;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write(placementOffset);
			writer.Write(NetworkRotation);
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
			writer.Write(placementOffset);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(NetworkRotation);
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
			placementOffset = reader.ReadVector3();
			NetworkRotation = reader.ReadQuaternion();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			placementOffset = reader.ReadVector3();
		}
		if ((num & 2) != 0)
		{
			OnRotationChanged(reader.ReadQuaternion());
		}
	}
}
