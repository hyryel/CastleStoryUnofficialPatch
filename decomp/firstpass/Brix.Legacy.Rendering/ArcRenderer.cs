using UnityEngine;

namespace Brix.Legacy.Rendering;

public class ArcRenderer : MonoBehaviour
{
	public bool autoUpdate;

	public bool actif;

	public bool deuxCotes;

	public bool vertical;

	public float angleMin;

	public float angleMax;

	public float petitRayon;

	public float grandRayon;

	public float espace;

	public float densite;

	public Material material;

	public GameObject obj;

	public bool shareMesh;

	private MeshFilter meshFilter;

	private Renderer meshRenderer;

	private bool exActif;

	private bool exDeuxCotes;

	private bool exVertical;

	private float exAngleMin;

	private float exAngleMax;

	private float exPetitRayon;

	private float exGrandRayon;

	private float exEspace;

	private float exDensite;

	private Vector3 p = Vector3.zero;

	private Vector3 u = Vector3.right;

	private Vector3 v = Vector3.forward;

	private Vector3 w = Vector3.up;

	private static readonly MeshInfoList MeshInfo = new MeshInfoList();

	public Color Color
	{
		get
		{
			return material.color;
		}
		set
		{
			material.color = value;
		}
	}

	public float Alpha
	{
		get
		{
			return material.color.a;
		}
		set
		{
			Color color = material.color;
			color.a = value;
			material.color = color;
		}
	}

	private void Start()
	{
		if (obj == null)
		{
			obj = base.gameObject;
		}
		meshFilter = obj.GetOrAddComponent<MeshFilter>();
		meshRenderer = obj.GetOrAddComponent<MeshRenderer>();
		if (material != null)
		{
			if (!Application.isPlaying)
			{
				meshRenderer.sharedMaterial = material;
			}
			else
			{
				meshRenderer.material = material;
			}
		}
		else if (!Application.isPlaying)
		{
			material = meshRenderer.sharedMaterial;
		}
		else
		{
			material = meshRenderer.material;
		}
		if (shareMesh || !Application.isPlaying)
		{
			if (meshFilter.sharedMesh == null)
			{
				meshFilter.sharedMesh = new Mesh();
			}
		}
		else if (meshFilter.mesh == null)
		{
			meshFilter.mesh = new Mesh();
		}
	}

	private void Update()
	{
		if (actif != exActif || deuxCotes != exDeuxCotes || vertical != exVertical || angleMin != exAngleMin || angleMax != exAngleMax || petitRayon != exPetitRayon || grandRayon != exGrandRayon || espace != exEspace || densite != exDensite)
		{
			exActif = actif;
			exDeuxCotes = deuxCotes;
			exVertical = vertical;
			exAngleMin = angleMin;
			exAngleMax = angleMax;
			exPetitRayon = petitRayon;
			exGrandRayon = grandRayon;
			exEspace = espace;
			exDensite = densite;
			if (!actif)
			{
				meshFilter.sharedMesh.Clear();
			}
			else if (autoUpdate)
			{
				RebuildMesh();
			}
		}
	}

	private void RebuildMesh()
	{
		MeshInfo.Clear();
		Vector3 vector = ((!vertical) ? v : w);
		Vector3 vector2 = ((!vertical) ? w : v);
		ProceduralCube.Arc(p, u, vector, vector2, angleMin, angleMax, petitRayon, grandRayon, MeshInfo, espace, densite);
		if (deuxCotes)
		{
			ProceduralCube.Arc(p, u, -vector, -vector2, 0f - angleMax, 0f - angleMin, petitRayon, grandRayon, MeshInfo, espace, densite);
		}
		MeshInfo.Rendre((!shareMesh && Application.isPlaying) ? meshFilter.mesh : meshFilter.sharedMesh);
	}
}
