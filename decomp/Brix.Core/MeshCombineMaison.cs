using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshCombineMaison
{
	public const int maxTriangles = 64998;

	public const int maxVertices = 65000;

	public static MeshModifiable Combine(IEnumerable<MeshModifiable> combines, MeshModifiable meshOut = null)
	{
		int num = 0;
		int num2 = 0;
		bool flag = false;
		bool flag2 = false;
		MeshModifiable[] array = combines.ToArray();
		MeshModifiable[] array2 = array;
		foreach (MeshModifiable meshModifiable in array2)
		{
			if (meshModifiable != null)
			{
				num += meshModifiable.GetVertexCount();
				num2 += meshModifiable.GetTriangleCount();
				flag = flag || meshModifiable.HasUv2();
				flag2 = flag2 || meshModifiable.HasTangents();
			}
		}
		Vector3[] array3 = new Vector3[num];
		Vector3[] array4 = new Vector3[num];
		Vector2[] array5 = new Vector2[num];
		Vector2[] array6 = (flag ? new Vector2[num] : null);
		Vector4[] array7 = (flag2 ? new Vector4[num] : null);
		int[] array8 = new int[num2];
		int num3 = 0;
		int num4 = 0;
		MeshModifiable[] array9 = array;
		foreach (MeshModifiable meshModifiable2 in array9)
		{
			if (meshModifiable2 == null)
			{
				Donnees.Log("Warning: MeshCombineMaison.Combine a recu un mesh vide. Il sera ignore.");
				continue;
			}
			Vector3[] vertices = meshModifiable2.GetVertices();
			Vector3[] normals = meshModifiable2.GetNormals();
			Vector2[] uv = meshModifiable2.GetUv();
			int[] triangles = meshModifiable2.GetTriangles();
			for (int k = 0; k < triangles.Length; k++)
			{
				array8[num4] = triangles[k] + num3;
				num4++;
			}
			int num5 = num3;
			for (int k = 0; k < vertices.Length; k++)
			{
				array3[num5] = vertices[k];
				array4[num5] = normals[k];
				array5[num5] = uv[k];
				num5++;
			}
			if (meshModifiable2.HasUv2())
			{
				num5 = num3;
				Vector2[] uv2 = meshModifiable2.GetUv2();
				for (int k = 0; k < vertices.Length; k++)
				{
					array6[num5] = uv2[k];
					num5++;
				}
			}
			if (meshModifiable2.HasTangents())
			{
				num5 = num3;
				Vector4[] tangents = meshModifiable2.GetTangents();
				for (int k = 0; k < vertices.Length; k++)
				{
					array7[num5] = tangents[k];
					num5++;
				}
			}
			num3 += vertices.Length;
		}
		if (meshOut == null)
		{
			meshOut = new MeshInfoSimple();
		}
		meshOut.SetVertices(array3);
		meshOut.SetNormals(array4);
		meshOut.SetUv(array5);
		meshOut.SetUv2(array6);
		meshOut.SetTangents(array7);
		meshOut.SetTriangles(array8);
		return meshOut;
	}

	public static IEnumerable<MeshModifiable> Split(MeshModifiable meshInfo)
	{
		if (meshInfo.GetVertexCount() <= 65000 && meshInfo.GetTriangleCount() <= 64998)
		{
			yield return meshInfo;
			yield break;
		}
		Vector3[] vertices = meshInfo.GetVertices();
		Vector3[] normals = meshInfo.GetNormals();
		Vector4[] tangents = meshInfo.GetTangents();
		Vector2[] uv = meshInfo.GetUv();
		Vector2[] uv2 = meshInfo.GetUv2();
		int[] triangles = meshInfo.GetTriangles();
		int max = 64998;
		int nt = triangles.Length;
		int nm = (nt + max - 1) / max;
		int j = 0;
		for (int i = 0; i < nm; i++)
		{
			IntClasse vused = new IntClasse();
			int k;
			for (k = j; j < triangles.Length && j - k < max; j++)
			{
				vused.Ajout(triangles[j]);
			}
			Vector3[] verticesOut = new Vector3[vused.Count];
			Vector3[] normalsOut = new Vector3[vused.Count];
			Vector4[] tangentsOut = (MeshModifiable.HasTangents(tangents) ? new Vector4[vused.Count] : null);
			Vector2[] uvOut = new Vector2[vused.Count];
			Vector2[] uv2Out = new Vector2[vused.Count];
			int[] trianglesOut = new int[j - k];
			for (j = 0; j < vused.Count; j++)
			{
				verticesOut[j] = vertices[vused[j]];
				normalsOut[j] = normals[vused[j]];
				uvOut[j] = uv[vused[j]];
				uv2Out[j] = uv2[vused[j]];
			}
			if (tangentsOut != null)
			{
				for (j = 0; j < vused.Count; j++)
				{
					tangentsOut[j] = tangents[vused[j]];
				}
			}
			for (j = k; j < triangles.Length && j - k < max; j++)
			{
				trianglesOut[j - k] = vused.Find(triangles[j]);
			}
			yield return new MeshInfoSimple
			{
				vertices = verticesOut,
				normals = normalsOut,
				tangents = tangentsOut,
				uv = uvOut,
				uv2 = uv2Out,
				triangles = trianglesOut
			};
		}
	}

	private static IEnumerable<Mesh> SplitToMeshes_Internal(MeshModifiable meshIn, IEnumerable<Mesh> meshOut)
	{
		IEnumerator<Mesh> iMeshes = meshOut?.GetEnumerator();
		foreach (MeshModifiable meshInfo in Split(meshIn))
		{
			Mesh mesh = ((iMeshes != null && iMeshes.MoveNext()) ? iMeshes.Current : new Mesh());
			meshInfo.Rendre(mesh);
			yield return mesh;
		}
	}

	public static IEnumerable<Mesh> SplitToMeshes(MeshModifiable meshIn, IEnumerable<Mesh> meshOut, bool immediate)
	{
		IEnumerable<Mesh> enumerable = SplitToMeshes_Internal(meshIn, meshOut);
		IEnumerable<Mesh> result;
		if (!immediate)
		{
			result = enumerable;
		}
		else
		{
			IEnumerable<Mesh> enumerable2 = new List<Mesh>(enumerable);
			result = enumerable2;
		}
		return result;
	}

	private static Mesh MeshFromObject(GameObject gameObject, GameObject modele)
	{
		MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
		if (meshFilter == null)
		{
			meshFilter = gameObject.AddComponent<MeshFilter>();
			if (modele != null)
			{
				Renderer component = modele.GetComponent<Renderer>();
				if (component != null)
				{
					MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
					meshRenderer.sharedMaterials = component.sharedMaterials;
					meshRenderer.shadowCastingMode = component.shadowCastingMode;
					meshRenderer.receiveShadows = component.receiveShadows;
				}
			}
		}
		if (meshFilter.mesh == null)
		{
			meshFilter.mesh = new Mesh();
		}
		return meshFilter.mesh;
	}

	public static IEnumerable<Mesh> PopulateMeshes(GameObject gameObject, int maxCount)
	{
		if (maxCount <= 0)
		{
			yield break;
		}
		yield return MeshFromObject(gameObject, null);
		if (maxCount <= 1)
		{
			yield break;
		}
		int i = 1;
		foreach (Transform child in gameObject.transform)
		{
			yield return MeshFromObject(child.gameObject, gameObject);
			if (i++ >= maxCount)
			{
				yield break;
			}
		}
		for (; i < maxCount; i++)
		{
			string name = "SubMesh " + i;
			GameObject newObject = new GameObject(name);
			newObject.transform.parent = gameObject.transform;
			yield return MeshFromObject(newObject, gameObject);
		}
	}

	public static void ClearMeshes(GameObject gameObject, int start = 0, bool verbose = false)
	{
		int num = 1;
		foreach (Transform item in gameObject.transform)
		{
			if (num++ >= start)
			{
				Clear(item.gameObject, verbose);
			}
		}
		if (0 >= start)
		{
			Object.Destroy(gameObject.GetComponent<MeshFilter>());
		}
	}

	private static void Clear(GameObject gameObject, bool verbose = false)
	{
		if (verbose)
		{
			Debug.Log("MeshCombineMaison.Clear() : " + gameObject.name);
		}
		Object.Destroy(gameObject);
	}
}
