using Brix.Core;
using UnityEngine;

namespace Brix.Engine.Geometrie;

public class CubeDrawer
{
	private Vector3[] _sommets = new Vector3[14];

	private Vector3[] _normales = new Vector3[30];

	private Vector3[] _tangentes = new Vector3[30];

	private Vector2[] _uv = new Vector2[5];

	public CubeDrawer()
	{
		SetUvDecals(2, 0, 1024, 1024);
	}

	public void SetUvDecals(int uvDecal, int uvOuterDecal, int uvWidth, int uvOuterWidth)
	{
		float num = (float)uvDecal / (float)uvWidth;
		float num2 = (float)uvOuterDecal / (float)uvOuterWidth;
		Vector2 vector = new Vector2(num2, num2);
		_uv[0] = new Vector2(num, num) + vector;
		_uv[1] = new Vector2(1f - num, num) + vector;
		_uv[2] = new Vector2(num, 1f - num) + vector;
		_uv[3] = new Vector2(1f - num, 1f - num) + vector;
		_uv[4] = new Vector2(0.5f, 0.5f) + vector;
	}

	private void InitSommets(IGeometrie modele, Config config, Vector3 p)
	{
		for (int i = 0; i < config.sommetsUtiles.Length; i++)
		{
			_sommets[i] = p + modele.GetSommet(config.sommetsUtiles[i]);
		}
	}

	private void InitNormales(IGeometrie modele, Config config)
	{
		for (int i = 0; i < config.normalesUtiles.Length; i++)
		{
			_normales[i] = modele.GetNormale(config.normalesUtiles[i]);
		}
	}

	private void InitTangentes(IGeometrie modele, Config config)
	{
		for (int i = 0; i < config.normalesUtiles.Length; i++)
		{
			_tangentes[i] = modele.GetTangente(config.normalesUtiles[i]);
		}
	}

	private void AddSommets(MeshAugmentable mesh, Config config, Vector2 uv, Vector2 uv2, bool uv2Decal)
	{
		for (int i = 0; i < config.iSommets.Length; i++)
		{
			Vector2 vector = _uv[config.iUv[i]];
			mesh.Sommet(_sommets[config.iSommets[i]], _normales[config.iNormales[i]], uv + vector, uv2Decal ? (uv2 + vector) : uv2);
		}
	}

	private void AddSommetsWithTangents(MeshAugmentable mesh, Config config, Vector2 uv, Vector2 uv2, bool uv2Decal)
	{
		for (int i = 0; i < config.iSommets.Length; i++)
		{
			Vector2 vector = _uv[config.iUv[i]];
			Vector3 vector2 = _tangentes[config.iNormales[i]];
			mesh.Sommet(_sommets[config.iSommets[i]], _normales[config.iNormales[i]], uv + vector, uv2Decal ? (uv2 + vector) : uv2, new Vector4(vector2.x, vector2.y, vector2.z, -1f));
		}
	}

	private void AddTriangles(MeshAugmentable mesh, Config config, int n)
	{
		for (int i = 0; i < config.triangles.Length; i += 3)
		{
			mesh.Triangle(n + config.triangles[i], n + config.triangles[i + 1], n + config.triangles[i + 2]);
		}
	}

	public void Render(IGeometrie modele, MeshAugmentable mesh, Vector3 p, Vector2 uv, Vector2 uv2, Config config, bool uv2Decal)
	{
		InitSommets(modele, config, p);
		InitNormales(modele, config);
		int vertexCount = mesh.GetVertexCount();
		AddSommets(mesh, config, uv, uv2, uv2Decal);
		AddTriangles(mesh, config, vertexCount);
	}

	public void RenderWithTangents(IGeometrie modele, MeshAugmentable mesh, Vector3 p, Vector2 uv, Vector2 uv2, Config config, bool uv2Decal)
	{
		InitSommets(modele, config, p);
		InitNormales(modele, config);
		InitTangentes(modele, config);
		int vertexCount = mesh.GetVertexCount();
		AddSommetsWithTangents(mesh, config, uv, uv2, uv2Decal);
		AddTriangles(mesh, config, vertexCount);
	}
}
