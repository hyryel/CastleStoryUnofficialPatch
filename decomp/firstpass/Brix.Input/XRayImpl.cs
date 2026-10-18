using System;
using Brix.Engine;
using Brix.Engine.Geometrie;
using Brix.Engine.Volumes;
using Brix.Legacy;
using Brix.Util;
using UnityEngine;

namespace Brix.Input;

public class XRayImpl : XRay
{
	public Material[] materials;

	public Shader[] xrayShaders;

	public MeshRenderer meshRenderer;

	public MeshFilter meshFilter;

	public Transform pivot;

	public Transform viewer;

	public bool autoDetectCamera;

	public static XRayImpl instance;

	[Range(0f, 1f)]
	public float rangeFactor = 0.5f;

	public float padding;

	public float cameraGroundPadding = 1.5f;

	public bool useMidpoint;

	public Vector3 maxExtents = Vector3.one * 24f;

	public Vector3 position;

	public Vector3 extents;

	public bool flipBottom;

	private bool isActive;

	private Shader[] originalShaders;

	private Shader originalTerrainShader;

	private Shader originalTreeShader;

	private Vector3 roundedPosition;

	private Vector3 roundedExtents;

	private Bounds roundedBounds;

	private float lastCheckedVoxelChange;

	private bool savedHugTerrain;

	private bool savedPivotCanGoUnderground;

	private XYZ exStart;

	private XYZ exEnd;

	private bool exFlipBottom;

	private bool exActive;

	private MeshAugmentable _meshInfo = new MeshAugmentableDebordant(0.3f);

	private Vector3[] _sommets = new Vector3[9];

	public override bool IsActive => isActive;

	public override Bounds Bounds => roundedBounds;

	private void Awake()
	{
		instance = this;
		if (meshRenderer == null)
		{
			meshRenderer = GetComponent<MeshRenderer>();
		}
		if (meshFilter == null)
		{
			meshFilter = GetComponent<MeshFilter>();
		}
		if (autoDetectCamera)
		{
			CameraScript component = Camera.main.GetComponent<CameraScript>();
			if (viewer == null)
			{
				viewer = component.transform;
			}
			if (pivot == null)
			{
				pivot = component.GizmoTarget.transform;
			}
		}
		originalShaders = new Shader[xrayShaders.Length];
	}

	private void OnEnable()
	{
		for (int i = 0; i < materials.Length; i++)
		{
			if (materials[i] != null && originalShaders[i] == null && xrayShaders[i] != null)
			{
				originalShaders[i] = materials[i].shader;
				materials[i].shader = xrayShaders[i];
			}
		}
		if (meshRenderer != null)
		{
			meshRenderer.enabled = true;
		}
		if (autoDetectCamera)
		{
			CameraScript component = Camera.main.GetComponent<CameraScript>();
			savedHugTerrain = component.HugTerrain;
			savedPivotCanGoUnderground = component.pivotCanGoUnderground;
			component.AutoAdjustWhenIdle = false;
			component.pivotCanGoUnderground = true;
		}
		exActive = false;
		isActive = true;
	}

	private void OnDisable()
	{
		Shader.SetGlobalVector("_XRayFocus", Vector3.zero);
		Shader.SetGlobalVector("_XRayRange", Vector3.zero);
		for (int i = 0; i < materials.Length; i++)
		{
			if (materials[i] != null && originalShaders[i] != null)
			{
				materials[i].shader = originalShaders[i];
				originalShaders[i] = null;
			}
		}
		if (meshRenderer != null)
		{
			meshRenderer.enabled = false;
		}
		if (autoDetectCamera)
		{
			CameraScript component = Camera.main.GetComponent<CameraScript>();
			component.AutoAdjustWhenIdle = savedHugTerrain;
			component.pivotCanGoUnderground = savedPivotCanGoUnderground;
		}
		exActive = true;
		isActive = false;
	}

	private void Update()
	{
		if (pivot != null)
		{
			Vector3 vector = pivot.position;
			if (viewer != null)
			{
				float num = (pivot.position - viewer.position).magnitude * rangeFactor;
				extents = new Vector3(num + padding, num, num + padding);
				if (maxExtents != Vector3.zero)
				{
					extents = Calcule.MinVecteur(extents, maxExtents);
				}
				float t = ((!useMidpoint) ? 0f : 0.5f);
				vector.x = Mathf.Lerp(vector.x, viewer.position.x, t);
				vector.z = Mathf.Lerp(vector.z, viewer.position.z, t);
				vector.y = Mathf.Min(vector.y, viewer.position.y - cameraGroundPadding);
			}
			position = vector + Vector3.up * (1f + extents.y);
		}
		XYZ xYZ = XYZ.FromVector3(position - extents);
		XYZ xYZ2 = XYZ.FromVector3(position + extents);
		if (!(exStart == xYZ) || !(exEnd == xYZ2) || exFlipBottom != flipBottom || exActive != isActive || !(lastCheckedVoxelChange >= Architecte.instance.LastVoxelChangedTime))
		{
			exStart = xYZ;
			exEnd = xYZ2;
			exFlipBottom = flipBottom;
			exActive = isActive;
			lastCheckedVoxelChange = Architecte.instance.LastVoxelChangedTime;
			roundedPosition = (xYZ2 + xYZ).ToVector3() * 0.5f;
			roundedExtents = (xYZ2 - xYZ).ToVector3() * 0.5f;
			roundedBounds = new Bounds(roundedPosition, roundedExtents * 2f);
			Shader.SetGlobalVector("_XRayFocus", roundedPosition - Vector3.one * 0.5f);
			Shader.SetGlobalVector("_XRayRange", roundedExtents - Vector3.one * 0.001f);
			if (meshFilter != null)
			{
				_meshInfo.Clear(realloc: false);
				CreateMesh(_meshInfo, xYZ, xYZ2);
				meshFilter.sharedMesh = _meshInfo.Rendre(meshFilter.sharedMesh);
			}
		}
	}

	private void CreateMesh(MeshAugmentable meshInfo, XYZ start, XYZ end)
	{
		VoxelBounds voxelBounds = new VoxelBounds(start, end - XYZ.one);
		int num = 0;
		foreach (VoxelSurfaceElement item in voxelBounds.Surface)
		{
			if (Voxel.IsEmpty(item.InnerVoxel) || Voxel.IsEmpty(item.OuterVoxel))
			{
				continue;
			}
			int niveaux = Voxel.GetNiveaux(item.OuterVoxel);
			Vector3[] sommets = _sommets;
			Array.Copy(CubeModele.SommetPositions, sommets, 8);
			Brix.Legacy.ProceduralCube.GetSommetsDessus(niveaux, sommets);
			Vector3 vector = item.OuterVoxel.ToVector3();
			int vertexCount = meshInfo.GetVertexCount();
			int num2 = item.Inverse.faceIndex;
			Vector3 n = Cubeur.Dirs[num2];
			bool flag = false;
			if (flipBottom && Voxel.IsEmpty(item.OuterVoxel - XYZ.up))
			{
				if ((niveaux & 0x3333) == 0)
				{
					continue;
				}
				for (int i = 0; i < 8; i++)
				{
					sommets[i].y = 0f - sommets[i].y;
				}
				switch (num2)
				{
				case 0:
					num2 = 5;
					break;
				case 5:
					num2 = 0;
					break;
				}
				flag = true;
			}
			for (int j = 0; j < 4; j++)
			{
				meshInfo.Sommet(vector + sommets[Cubeur.SommetIndices[4 * num2 + j]], n, new Vector2(j & 1, j >> 1), Vector2.zero);
			}
			if (flag)
			{
				meshInfo.Triangle(vertexCount, vertexCount + 2, vertexCount + 3);
				meshInfo.Triangle(vertexCount + 3, vertexCount + 1, vertexCount);
			}
			else
			{
				meshInfo.Triangle(vertexCount, vertexCount + 1, vertexCount + 3);
				meshInfo.Triangle(vertexCount + 3, vertexCount + 2, vertexCount);
			}
			num++;
		}
	}
}
