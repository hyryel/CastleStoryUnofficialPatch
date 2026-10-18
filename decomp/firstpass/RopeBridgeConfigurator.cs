using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Brix.Engine;
using Brix.External.Factories;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Legacy;
using Brix.Network;
using Brix.Util;
using Brix.Utils;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class RopeBridgeConfigurator : NetworkGameComponent
{
	public Transform EndA;

	public Transform EndB;

	public Transform RopeLeft;

	public Transform RopeRight;

	protected XYZ A;

	protected XYZ B;

	[JsonProperty]
	[SyncVar]
	private Vector3 _a;

	[JsonProperty]
	[SyncVar(hook = "OnEndPointsChanged")]
	private Vector3 _b;

	public int ShortestDistanceH = 4;

	public int LongestDistanceH = 12;

	public int ShortestDistanceV = 2;

	public int LongestDistanceV = 6;

	public int PreviewDistanceV = 12;

	public float PlankAlignment = -0.7f;

	public float MinCurviness = 0.3f;

	public float MaxCurviness = 1f;

	public bool IsPlacedAtInvalidPosition;

	public Mesh RopeUnitMesh;

	private bool _started;

	private MeshModifiable _ropeUnitMeshInfo;

	private MeshAugmentablePrealloue _ropeMeshInfo;

	public XYZ StartPoint => A;

	public XYZ EndPoint => B;

	private int HorizontalLength => XYZ.MaxMagnitude((B - A).Flatten());

	private int NumberOfPlanks => HorizontalLength - 1;

	public XYZ Direction
	{
		get
		{
			XYZ xYZ = (B - A).Flatten();
			return xYZ / XYZ.MaxMagnitude(xYZ);
		}
	}

	public XYZ SideDirection => (A.z == B.z) ? (XYZ.forward * ((A.x > B.x) ? 1 : (-1))) : (XYZ.right * ((B.z > A.z) ? 1 : (-1)));

	private float Curviness => Mathf.Lerp(MinCurviness, MaxCurviness, Mathf.InverseLerp(ShortestDistanceH, LongestDistanceH, HorizontalLength));

	public Vector3 Network_a
	{
		get
		{
			return _a;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref _a, 1u);
		}
	}

	public Vector3 Network_b
	{
		get
		{
			return _b;
		}
		[param: In]
		set
		{
			ref Vector3 b = ref _b;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				OnEndPointsChanged(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref b, 2u);
		}
	}

	public override void OnStartAnywhere()
	{
		base.OnStartAnywhere();
		_started = true;
		OnEndPointsChanged(_b);
	}

	private void OnEndPointsChanged(Vector3 b)
	{
		if (_started)
		{
			XYZ xYZ = XYZ.FromVector3(_a);
			XYZ xYZ2 = XYZ.FromVector3(b);
			if (A != xYZ || B != xYZ2)
			{
				SetEndPoints(xYZ, xYZ2);
				CreateVolume(GetComponent<Blueprint>() != null);
				InitRopes();
				CreatePlankColliders();
				RefreshPlankMeshes();
			}
		}
	}

	public static RopeBridgeConfigurator GetRopeBridgeFromFrameAt(XYZ framePos)
	{
		return VolumeRegistry.VolumeWithPropertyAt(framePos, VPropertyMask.occupy | VPropertyMask.fantome, (IVolume volume) => volume.GetComponent<RopeBridgeConfigurator>() != null)?.GetComponent<RopeBridgeConfigurator>();
	}

	public static RopeBridgeConfigurator GetRopeBridgeFromPlankAt(XYZ plankPos)
	{
		return VolumeRegistry.VolumeWithPropertyAt(plankPos, VProperty.ropeSupport)?.GetComponent<RopeBridgeConfigurator>();
	}

	public XYZ ComputeEndPoint(XYZ start, XYZ dir, int skip, out bool success)
	{
		XYZ a = A;
		XYZ b = B;
		A = start;
		B = start + dir;
		XYZ result = ComputeEndPoint_Internal(skip, out success);
		A = a;
		B = b;
		return result;
	}

	private XYZ ComputeEndPoint_Internal(int skip, out bool success)
	{
		AbstractValidator validator = GetComponent<ValidatorComponent>().Validator;
		int rotationIndex = PreprocessedRotations.RotationToDiscreteIndex(Quaternion.Euler(0f, 180f, 0f) * base.transform.rotation);
		FreeVolume freeVolume = FreeVolume.Make(AssetKey, A, rotationIndex, 0, faction);
		ValidationData vData = freeVolume.VData;
		XYZ direction = Direction;
		XYZ sideDirection = SideDirection;
		XYZ xYZ = A + direction * (LongestDistanceH + 1);
		XYZ xYZ2 = xYZ;
		for (int i = ShortestDistanceH; i <= LongestDistanceH; i++)
		{
			int previewDistanceV = PreviewDistanceV;
			B = A + direction * (i + 1) - XYZ.up * previewDistanceV;
			for (int j = -previewDistanceV; j <= previewDistanceV; j++)
			{
				XYZ position = (vData.Position = B + sideDirection);
				freeVolume.Position = position;
				if (validator.Validate(ref vData) >= ValidationResult.Valid && skip-- <= 0)
				{
					success = ValidateFinalDistances() && ValidatePlanksToBe();
					if (success)
					{
						return B;
					}
					if (xYZ2 == xYZ)
					{
						xYZ2 = B;
					}
				}
				B += XYZ.up;
			}
		}
		success = false;
		return xYZ2;
	}

	private bool ValidateFinalDistances()
	{
		int num = Mathf.RoundToInt((float)ShortestDistanceV + (float)((LongestDistanceV - ShortestDistanceV) * (NumberOfPlanks - ShortestDistanceH)) / (float)(LongestDistanceH - ShortestDistanceH));
		return Mathf.Abs(B.y - A.y) <= num;
	}

	private bool ValidatePlanksToBe()
	{
		int numberOfPlanks = NumberOfPlanks;
		if (numberOfPlanks > 0)
		{
			XYZ direction = Direction;
			XYZ sideDirection = SideDirection;
			for (int i = 0; i < numberOfPlanks; i++)
			{
				XYZ xYZ = A + direction * (i + 1);
				float relativeHeightAtPlankCenter = GetRelativeHeightAtPlankCenter(i);
				xYZ.y = A.y + Mathf.RoundToInt(relativeHeightAtPlankCenter);
				if (Voxel.IsNonEmpty(xYZ) || Voxel.IsNonEmpty(xYZ + sideDirection))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void SetEndPoints(XYZ start, XYZ end)
	{
		A = start;
		B = end;
		Network_a = A.ToVector3();
		Network_b = B.ToVector3();
		UpdateChildTransforms();
	}

	private void UpdateChildTransforms()
	{
		if (EndA != null)
		{
			EndA.position = base.voxel.GetOffsettedPosition(A);
		}
		if (EndB != null)
		{
			EndB.position = base.voxel.GetOffsettedPosition(B);
		}
	}

	public void GetPlankPositions(ICollection<XYZ> results)
	{
		int numberOfPlanks = NumberOfPlanks;
		if (numberOfPlanks > 0)
		{
			XYZ direction = Direction;
			for (int i = 0; i < numberOfPlanks; i++)
			{
				XYZ item = A + direction * (i + 1);
				float relativeHeightAtPlankCenter = GetRelativeHeightAtPlankCenter(i);
				item.y = A.y + Mathf.RoundToInt(relativeHeightAtPlankCenter);
				results.Add(item);
			}
		}
	}

	public void GetPlankTransform(XYZ plankPos, out Vector3 position, out Quaternion rotation)
	{
		int num = XYZ.MaxMagnitude((plankPos - A).Flatten()) - 1;
		float relativeHeightAtPlankEdge = GetRelativeHeightAtPlankEdge(num);
		float relativeHeightAtPlankEdge2 = GetRelativeHeightAtPlankEdge(num + 1);
		position = base.voxel.GetOffsettedPosition(plankPos);
		position.y = (float)A.y + (relativeHeightAtPlankEdge + relativeHeightAtPlankEdge2) / 2f;
		Vector3 vector = Direction.ToVector3() + Vector3.up * (relativeHeightAtPlankEdge2 - relativeHeightAtPlankEdge);
		Vector3 upwards = Vector3.Cross(vector, base.transform.right);
		rotation = Quaternion.LookRotation(vector, upwards);
	}

	public Matrix4x4 GetPlankTransform(XYZ plankPos)
	{
		GetPlankTransform(plankPos, out var position, out var rotation);
		return Matrix4x4.TRS(position, rotation, Vector3.one);
	}

	public Vector3 GetPlankExactPosition(XYZ plankPos)
	{
		GetPlankTransform(plankPos, out var position, out var _);
		return position;
	}

	public void CreateVolume(bool isBlueprint)
	{
		Volume component = GetComponent<Volume>();
		FreeVolume volume = component.InternalVolume;
		List<XYZ>[] d = new List<XYZ>[64];
		Dictionary<XYZ, VPropertyMask> e = new Dictionary<XYZ, VPropertyMask>();
		Action<XYZ, VProperty> action = delegate(XYZ pos, VProperty prop)
		{
			XYZ xYZ11 = pos - volume.Position;
			(d[(int)prop] ?? (d[(int)prop] = new List<XYZ>())).Add(xYZ11);
			e[xYZ11] = e.GetValueOrDefault(xYZ11) | prop.ToMask();
		};
		List<XYZ> list = CollectionPool.RequestList<XYZ>();
		GetPlankPositions(list);
		XYZ sideDirection = SideDirection;
		foreach (XYZ item in list)
		{
			action(item, VProperty.ropeSupport);
			action(item + sideDirection, VProperty.ropeSupport);
		}
		list.Dispose();
		XYZ xYZ = A - sideDirection;
		XYZ xYZ2 = A + sideDirection * 2;
		XYZ xYZ3 = B - sideDirection;
		XYZ xYZ4 = B + sideDirection * 2;
		XYZ[] array = new XYZ[4] { xYZ, xYZ2, xYZ3, xYZ4 };
		if (isBlueprint)
		{
			XYZ[] array2 = array;
			foreach (XYZ xYZ5 in array2)
			{
				action(xYZ5 - XYZ.up, VProperty.mainSupportReq);
				action(xYZ5, VProperty.fantome);
				action(xYZ5 + XYZ.up, VProperty.fantome);
				foreach (XYZ item2 in Calcule.VolumeBetween(-XYZ.one, XYZ.one + XYZ.up))
				{
					if (item2 != XYZ.zero)
					{
						action(xYZ5 + item2, VProperty.access);
					}
				}
			}
			XYZ xYZ6 = XYZ.FromVector3((xYZ - xYZ2).ToVector3().normalized);
			XYZ xYZ7 = XYZ.FromVector3((xYZ3 - xYZ4).ToVector3().normalized);
			action(xYZ - xYZ6, VProperty.fantome);
			action(xYZ - xYZ6 * 2, VProperty.fantome);
			action(xYZ3 - xYZ7, VProperty.fantome);
			action(xYZ3 - xYZ7 * 2, VProperty.fantome);
		}
		else
		{
			XYZ[] array3 = array;
			foreach (XYZ xYZ8 in array3)
			{
				action(xYZ8 - XYZ.up, VProperty.mainSupportReq);
				action(xYZ8, VProperty.occupy);
				action(xYZ8 + XYZ.up, VProperty.occupy);
				action(xYZ8, VProperty.obstacle);
				action(xYZ8 + XYZ.up, VProperty.obstacle);
				action(xYZ8, VProperty.deblocTerrain);
				action(xYZ8 + XYZ.up, VProperty.deblocTerrain);
				foreach (XYZ item3 in Calcule.VolumeBetween(-XYZ.one, XYZ.one))
				{
					if (item3 != XYZ.zero && item3.x * item3.z == 0)
					{
						action(xYZ8 + item3, VProperty.access);
					}
				}
			}
			XYZ xYZ9 = XYZ.FromVector3((xYZ - xYZ2).ToVector3().normalized);
			XYZ xYZ10 = XYZ.FromVector3((xYZ3 - xYZ4).ToVector3().normalized);
			action(xYZ - xYZ9, VProperty.occupy);
			action(xYZ - xYZ9 * 2, VProperty.occupy);
			action(xYZ3 - xYZ10, VProperty.occupy);
			action(xYZ3 - xYZ10 * 2, VProperty.occupy);
		}
		List<XYZ>[] array4 = d;
		for (int num3 = 0; num3 < array4.Length; num3++)
		{
			array4[num3]?.Sort();
		}
		volume.VolumetricPropertyIndices = d;
		volume.IndexedVolumetricProperties = e;
		component.Register();
	}

	public void CreatePlankColliders()
	{
		Vector3 size = Factory.Peek(Blueprints.RopeBridgePlank).GetComponentInChildren<MeshFilter>(includeInactive: true).sharedMesh.bounds.size;
		List<XYZ> list = CollectionPool.RequestList<XYZ>();
		GetPlankPositions(list);
		for (int num = base.transform.childCount - 1; num >= 0; num--)
		{
			Transform child = base.transform.GetChild(num);
			if (child.name.StartsWith("PlankCollider"))
			{
				UnityEngine.Object.Destroy(child.gameObject);
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			XYZ plankPos = list[i];
			GameObject gameObject = new GameObject("PlankCollider " + i);
			gameObject.layer = 11;
			GetPlankTransform(plankPos, out var position, out var rotation);
			gameObject.transform.position = position;
			gameObject.transform.rotation = rotation;
			gameObject.transform.SetParent(base.transform);
			BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
			boxCollider.size = size * 0.9f;
		}
		list.Dispose();
	}

	public void RefreshPlankMeshes()
	{
		List<XYZ> list = CollectionPool.RequestList<XYZ>();
		GetPlankPositions(list);
		for (int i = 0; i < list.Count; i++)
		{
			XYZ t = list[i];
			GameSignals.Invoke(GameSignals.VoxelChanged, t);
		}
		list.Dispose();
	}

	public void InitLineRenderers()
	{
		InitLineRenderer(RopeLeft, -base.transform.right * 1f - base.transform.up * 0.19f);
		InitLineRenderer(RopeRight, base.transform.right * 1f - base.transform.up * 0.19f);
	}

	public void InitLineRenderer(Transform lineParent, Vector3 offset)
	{
		if (lineParent == null)
		{
			return;
		}
		LineRenderer component = lineParent.GetComponent<LineRenderer>();
		if (!(component == null))
		{
			int numberOfPlanks = NumberOfPlanks;
			component.startColor = Color.blue;
			component.endColor = Color.red;
			component.startWidth = 0.1f;
			component.endWidth = 0.1f;
			component.positionCount = numberOfPlanks + 1;
			for (int i = 0; i <= numberOfPlanks; i++)
			{
				Vector3 p = Vector3.forward * ((float)i + 0.5f);
				p = base.transform.TransformPoint(TransformMeshPoint(p)) + offset;
				component.SetPosition(i, p);
			}
		}
	}

	public void ShowLineRenderers(bool show)
	{
		ShowLineRenderer(RopeLeft, show);
		ShowLineRenderer(RopeRight, show);
	}

	public void ShowLineRenderer(Transform lineParent, bool show)
	{
		if (!(lineParent == null))
		{
			LineRenderer component = lineParent.GetComponent<LineRenderer>();
			if (!(component == null))
			{
				component.enabled = show;
			}
		}
	}

	public void InitRopes()
	{
		InitRope(RopeLeft);
		InitRope(RopeRight);
	}

	public void InitRope(Transform ropeParent)
	{
		if (!(ropeParent == null))
		{
			if (RopeUnitMesh == null)
			{
				RopeUnitMesh = BuildUnitMesh();
			}
			if (_ropeUnitMeshInfo == null)
			{
				_ropeUnitMeshInfo = new BuiltinMeshInfo(RopeUnitMesh);
			}
			if (_ropeMeshInfo == null)
			{
				_ropeMeshInfo = new MeshAugmentablePrealloue();
			}
			BuildRopeMesh(_ropeMeshInfo, _ropeUnitMeshInfo);
			ropeParent.GetComponent<MeshFilter>().mesh = _ropeMeshInfo.Rendre(ropeParent.GetComponent<MeshFilter>().mesh);
		}
	}

	public Mesh BuildUnitMesh()
	{
		List<Vector3> list = new List<Vector3>();
		List<Vector3> list2 = new List<Vector3>();
		List<Vector4> list3 = new List<Vector4>();
		List<Vector2> list4 = new List<Vector2>();
		List<int> list5 = new List<int>();
		Vector3 right = Vector3.right;
		Vector3 up = Vector3.up;
		Vector3 forward = Vector3.forward;
		for (int i = 0; i <= 12; i++)
		{
			float f = (float)(i * 2) * (float)Math.PI / 12f;
			Vector3 vector = right * Mathf.Cos(f) + up * Mathf.Sin(f);
			Vector4 item = Vector3.Cross(vector, forward);
			item.w = 1f;
			for (int j = 0; j <= 3; j++)
			{
				list.Add(forward * (-0.5f + (float)j / 3f) + vector * 0.1f);
				list2.Add(vector);
				list3.Add(item);
				list4.Add(new Vector2((float)j / 3f, (float)i / 12f));
			}
		}
		for (int k = 0; k < 12; k++)
		{
			for (int l = 0; l < 3; l++)
			{
				int num = k * 4 + l;
				int num2 = (k + 1) * 4 + l;
				list5.Add(num);
				list5.Add(num + 1);
				list5.Add(num2 + 1);
				list5.Add(num);
				list5.Add(num2 + 1);
				list5.Add(num2);
			}
		}
		Mesh mesh = new Mesh();
		mesh.vertices = list.ToArray();
		mesh.normals = list2.ToArray();
		mesh.tangents = list3.ToArray();
		mesh.uv = list4.ToArray();
		mesh.triangles = list5.ToArray();
		Mesh mesh2 = mesh;
		mesh2.RecalculateBounds();
		return mesh2;
	}

	public void BuildRopeMesh(MeshAugmentablePrealloue meshInfo, MeshModifiable meshUnit)
	{
		Vector3[] vertices = meshUnit.GetVertices();
		Vector3[] normals = meshUnit.GetNormals();
		Vector4[] tangents = meshUnit.GetTangents();
		Vector2[] uv = meshUnit.GetUv();
		int[] triangles = meshUnit.GetTriangles();
		int num = vertices.Length;
		int num2 = triangles.Length;
		List<XYZ> list = CollectionPool.RequestList<XYZ>();
		GetPlankPositions(list);
		meshInfo.AllocTriangles(num2 * list.Count);
		meshInfo.AllocVertices(num * list.Count, meshUnit.HasTangents());
		meshInfo.nv = (meshInfo.nt = 0);
		for (int i = 0; i < list.Count; i++)
		{
			XYZ plankPos = list[i];
			Matrix4x4 plankTransform = GetPlankTransform(plankPos);
			for (int j = 0; j < vertices.Length; j++)
			{
				Vector3 p = vertices[j] + Vector3.forward * (i + 1);
				p = TransformMeshPoint(p);
				meshInfo.vertices[meshInfo.nv + j] = p;
			}
			Matrix4x4 transpose = plankTransform.inverse.transpose;
			MeshInfo.CopyNormals(normals, meshInfo.normals, transpose, meshInfo.nv, num);
			MeshInfo.CopyTangents(tangents, meshInfo.tangents, transpose, meshInfo.nv, num);
			MeshInfo.CopyUv(uv, meshInfo.uv, meshInfo.nv, num);
			MeshInfo.CopyTriangles(triangles, meshInfo.triangles, meshInfo.nv, meshInfo.nt, num2);
			meshInfo.nv += num;
			meshInfo.nt += num2;
		}
		list.Dispose();
	}

	private Vector3 TransformMeshPoint(Vector3 p)
	{
		return p + Vector3.up * GetRelativeHeightAtPlankEdge(p.z - 0.5f);
	}

	private float GetRelativeHeightAtPlankEdge(float plankEdge)
	{
		float num = plankEdge / (float)NumberOfPlanks;
		float num2 = -4f * num * (1f - num) * Curviness;
		return (float)(B.y - A.y) * num + num2 + PlankAlignment;
	}

	private float GetRelativeHeightAtPlankCenter(int iPlank)
	{
		return GetRelativeHeightAtPlankEdge((float)iPlank + 0.5f);
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write(_a);
			writer.Write(_b);
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
			writer.Write(_a);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(_b);
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
			_a = reader.ReadVector3();
			_b = reader.ReadVector3();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			_a = reader.ReadVector3();
		}
		if ((num & 2) != 0)
		{
			OnEndPointsChanged(reader.ReadVector3());
		}
	}
}
