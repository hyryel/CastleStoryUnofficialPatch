using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.Game.Components.Volumes;
using Brix.Util;
using Brix.Utils;
using Brix.Utils.Locomotion;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components;

public class MagicBridge : Volume
{
	public MeshFilter meshFilter;

	public MeshCollider meshCollider;

	public Transform start;

	public Transform end;

	public float elevationAngle = 45f;

	public float width = 4f;

	public bool locked = true;

	private bool exLocked;

	public override XYZ Position => XYZ.zero;

	protected override void Start()
	{
		base.Start();
		UpdateMesh();
	}

	private void Update()
	{
		if (!locked)
		{
			UpdateMesh();
		}
		UpdateVolume();
	}

	public void UpdateMesh()
	{
		Vector3 vector = (end.position - start.position).Flatten();
		Vector3 vector2 = Vector3.Cross(vector.normalized, Vector3.up) * width / 2f;
		int num = Mathf.CeilToInt(vector.magnitude);
		int num2 = num * 4;
		int num3 = num * 6;
		Vector3[] array = new Vector3[num2];
		int[] array2 = new int[num3];
		Vector3 vector3 = start.position;
		for (int i = 0; i < num; i++)
		{
			float num4 = ((float)i + 1f) / (float)num;
			Vector3 vector4 = start.position + vector * num4 + Vector3.up * GetDeltaHeight(num4);
			ref Vector3 reference = ref array[i * 4];
			reference = base.transform.InverseTransformPoint(vector3 - vector2);
			ref Vector3 reference2 = ref array[i * 4 + 1];
			reference2 = base.transform.InverseTransformPoint(vector3 + vector2);
			ref Vector3 reference3 = ref array[i * 4 + 2];
			reference3 = base.transform.InverseTransformPoint(vector4 + vector2);
			ref Vector3 reference4 = ref array[i * 4 + 3];
			reference4 = base.transform.InverseTransformPoint(vector4 - vector2);
			array2[i * 6] = i * 4;
			array2[i * 6 + 1] = i * 4 + 1;
			array2[i * 6 + 2] = i * 4 + 2;
			array2[i * 6 + 3] = i * 4;
			array2[i * 6 + 4] = i * 4 + 2;
			array2[i * 6 + 5] = i * 4 + 3;
			vector3 = vector4;
		}
		Mesh mesh = new Mesh();
		mesh.vertices = array;
		mesh.triangles = array2;
		mesh.normals = new Vector3[num2];
		Mesh mesh2 = mesh;
		mesh2.RecalculateNormals();
		meshFilter.sharedMesh = mesh2;
		if (meshCollider != null)
		{
			meshCollider.sharedMesh = mesh2;
		}
	}

	public void UpdateVolume()
	{
		if (exLocked != locked)
		{
			exLocked = locked;
			if (locked)
			{
				Register();
			}
			else
			{
				Unregister();
			}
		}
	}

	public int GetNiveauxAt(XYZ i)
	{
		Vector3 vector = i.ToVector3() - base.transform.position;
		float t = GetT(vector);
		int num = Mathf.RoundToInt(GetDeltaHeight(t));
		int niveau = GetNiveau(GetDeltaHeight(vector - Vector3.right / 2f + Vector3.forward / 2f) - (float)num);
		int niveau2 = GetNiveau(GetDeltaHeight(vector + Vector3.right / 2f + Vector3.forward / 2f) - (float)num);
		int niveau3 = GetNiveau(GetDeltaHeight(vector + Vector3.right / 2f - Vector3.forward / 2f) - (float)num);
		int niveau4 = GetNiveau(GetDeltaHeight(vector - Vector3.right / 2f - Vector3.forward / 2f) - (float)num);
		return (niveau << 0) | (niveau2 << 4) | (niveau3 << 8) | (niveau4 << 12);
	}

	private int GetNiveau(float y)
	{
		return Mathf.RoundToInt(Mathf.Clamp01(y) * 3f);
	}

	private bool IsBetween(float x, float a, float b)
	{
		return x >= a && x <= b;
	}

	public float GetT(Vector3 p)
	{
		Vector3 vector = (end.position - start.position).Flatten();
		return Vector3.Project(p - start.position, vector.normalized).magnitude / vector.magnitude;
	}

	public float GetDeltaHeight(Vector3 p)
	{
		return GetDeltaHeight(GetT(p));
	}

	public float GetDeltaHeight(float t)
	{
		Vector3 vector = end.position - start.position;
		float magnitude = new Vector2(vector.x, vector.z).magnitude;
		float y = vector.y;
		float num = Mathf.Tan(elevationAngle * ((float)Math.PI / 180f));
		float num2 = (y - num * magnitude) / (magnitude * magnitude);
		return (num2 * t * magnitude + num) * t * magnitude;
	}

	public IEnumerable<XYZ> GetSurfaceVoxels()
	{
		Vector3 d = (end.position - start.position).Flatten();
		Vector3 e = Vector3.Cross(d.normalized, Vector3.up) * (width / 2f - 1f);
		XYZ a = XYZ.FromVector3(start.position);
		XYZ b = XYZ.FromVector3(end.position);
		XYZ c = XYZ.FromVector3(Calcule.FloorVecteur(e));
		foreach (XYZ v in new VoxelBounds(new XYZ[4]
		{
			a - c,
			a + c,
			b - c,
			b + c
		}).Voxels)
		{
			Vector3 p = v.ToVector3() - start.position;
			if (IsBetween(Vector3.Dot(p, d), 0f, d.sqrMagnitude) && IsBetween(Vector3.Dot(p, e), 0f - e.sqrMagnitude, e.sqrMagnitude))
			{
				float t = Vector3.Project(p, d.normalized).magnitude / d.magnitude;
				float dy = GetDeltaHeight(t);
				yield return v + XYZ.up * Mathf.RoundToInt(dy);
			}
		}
	}

	private void UpdateVolumetricSettings(Dictionary<XYZ, VPropertyMask> dico)
	{
		foreach (XYZ surfaceVoxel in GetSurfaceVoxels())
		{
			dico[surfaceVoxel] |= VPropertyMask.walkable;
		}
	}

	private void UpdateVolumetricIndices(List<XYZ>[] dico)
	{
		List<XYZ> list = dico[3] ?? CollectionPool.RequestList<XYZ>();
		foreach (XYZ surfaceVoxel in GetSurfaceVoxels())
		{
			list.Add(surfaceVoxel);
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		bool flag2 = default(bool);
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
	}
}
