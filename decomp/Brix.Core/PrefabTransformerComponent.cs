using System;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class PrefabTransformerComponent : MonoBehaviour
{
	public enum ProcessOption
	{
		DontChange,
		LocalTransform,
		SelfTransform,
		FullTransform
	}

	public GameObject model;

	public GameObject target;

	[TaggedList("GetTag")]
	public ProcessOption[] processOptions;

	private GameObject exModel;

	private GameObject exTarget;

	private bool exModelNull;

	private bool exTargetNull;

	public MeshFilter[] modelFilters;

	public MeshFilter[] targetFilters;

	public bool createValidTarget;

	public string GetTag(int i)
	{
		return modelFilters[i].name;
	}

	private void Update()
	{
		if (exModel != model || exTarget != target || exModelNull != (model == null) || exTargetNull != (target == null))
		{
			Init();
		}
		exModel = model;
		exTarget = target;
		exModelNull = model == null;
		exTargetNull = target == null;
	}

	[ContextMenu("Reload Model")]
	public void Init()
	{
		modelFilters = GetMeshFilters(model);
		targetFilters = GetMeshFilters(target);
		int num = modelFilters.Length;
		if (processOptions == null)
		{
			processOptions = new ProcessOption[num];
		}
		if (processOptions.Length != num)
		{
			Array.Resize(ref processOptions, num);
		}
	}

	[ContextMenu("Apply Transforms")]
	public void Process()
	{
		if (!Ready())
		{
			if (!createValidTarget || model == null)
			{
				throw new Exception("Invalid Target. Target is not compatible with Model.");
			}
			Debug.LogWarning("PrefabTransformerComponent: Overwriting target for " + model.name);
			OverwriteTarget();
			Init();
		}
		for (int i = 0; i < modelFilters.Length; i++)
		{
			MeshFilter meshFilter = modelFilters[i];
			ProcessOption processOption = processOptions[i];
			Matrix4x4 matrix = Matrix4x4.identity;
			switch (processOption)
			{
			case ProcessOption.LocalTransform:
				matrix = Matrix4x4.TRS(meshFilter.transform.localPosition, meshFilter.transform.localRotation, meshFilter.transform.localScale);
				break;
			case ProcessOption.SelfTransform:
				matrix = targetFilters[i].transform.worldToLocalMatrix * meshFilter.transform.localToWorldMatrix;
				break;
			case ProcessOption.FullTransform:
				matrix = meshFilter.transform.localToWorldMatrix;
				break;
			}
			if (processOption != ProcessOption.DontChange)
			{
				MeshInfoSimple meshInfoSimple = new MeshInfoSimple();
				meshInfoSimple.Lire(meshFilter.sharedMesh);
				meshInfoSimple.Transform(matrix);
				Mesh mesh = new Mesh();
				meshInfoSimple.Rendre(mesh);
				mesh.name = meshFilter.sharedMesh.name;
				targetFilters[i].mesh = mesh;
			}
			else
			{
				targetFilters[i].sharedMesh = meshFilter.sharedMesh;
			}
			meshFilter = targetFilters[i];
			switch (processOptions[i])
			{
			case ProcessOption.DontChange:
				meshFilter.transform.position = modelFilters[i].transform.position;
				meshFilter.transform.rotation = modelFilters[i].transform.rotation;
				meshFilter.transform.localScale = modelFilters[i].transform.localScale;
				continue;
			case ProcessOption.LocalTransform:
				if (meshFilter.transform.parent == null)
				{
					break;
				}
				meshFilter.transform.position = modelFilters[i].transform.parent.position;
				meshFilter.transform.rotation = modelFilters[i].transform.parent.rotation;
				meshFilter.transform.localScale = modelFilters[i].transform.parent.localScale;
				continue;
			case ProcessOption.FullTransform:
				break;
			default:
				continue;
			}
			meshFilter.transform.position = Vector3.zero;
			meshFilter.transform.rotation = Quaternion.identity;
			meshFilter.transform.localScale = Vector3.one;
		}
	}

	public void OverwriteTarget()
	{
		string text = ((target != null) ? target.name : (model.name + "_Processed"));
		target = UnityEngine.Object.Instantiate(model, base.transform.position, base.transform.rotation);
		target.name = text;
	}

	public void CleanupTarget()
	{
		if (target != null)
		{
			Destroy(target);
			Init();
		}
	}

	private MeshFilter[] GetMeshFilters(GameObject prefab)
	{
		if (prefab == null)
		{
			return new MeshFilter[0];
		}
		MeshFilter[] componentsInChildren = prefab.GetComponentsInChildren<MeshFilter>(includeInactive: true);
		return componentsInChildren.OrderBy((MeshFilter f) => GetLongName(f.transform)).ToArray();
	}

	private string GetLongName(Transform t)
	{
		if (base.transform.parent == null)
		{
			return "root";
		}
		return GetLongName(base.transform.parent) + "/" + t.name;
	}

	public bool Ready()
	{
		if (modelFilters.Length != targetFilters.Length)
		{
			return false;
		}
		for (int i = 0; i < modelFilters.Length; i++)
		{
			if (modelFilters[i] == null || targetFilters[i] == null)
			{
				return false;
			}
			if (GetLongName(modelFilters[i].transform) != GetLongName(targetFilters[i].transform))
			{
				return false;
			}
		}
		return true;
	}

	private static void Destroy(GameObject g)
	{
		UnityEngine.Object.Destroy(g);
	}
}
