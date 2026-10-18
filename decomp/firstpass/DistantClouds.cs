using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DistantClouds : MonoBehaviour
{
	[Serializable]
	public class CloudInfo
	{
		public Sprite sprite;

		public Vector3 bottomLeft;

		public Vector3 bottomRight;

		public Vector3 up;

		public int i;

		public IEnumerable<Vector3> GetVertices()
		{
			yield return bottomLeft;
			yield return bottomLeft + up;
			yield return bottomRight + up;
			yield return bottomRight;
		}

		public IEnumerable<Vector3> GetNormals()
		{
			yield return -bottomLeft.normalized;
			yield return -bottomLeft.normalized;
			yield return -bottomRight.normalized;
			yield return -bottomRight.normalized;
		}

		public IEnumerable<Vector4> GetTangents()
		{
			Vector3 right = (bottomRight - bottomLeft).normalized;
			Vector4 tangent = new Vector4(right.x, right.y, right.z, -1f);
			yield return tangent;
			yield return tangent;
			yield return tangent;
			yield return tangent;
		}

		public IEnumerable<Vector2> GetUv()
		{
			Rect rect = sprite.rect;
			Vector2 a = new Vector2(rect.x / (float)sprite.texture.width, rect.y / (float)sprite.texture.height);
			Vector2 d = new Vector2(rect.width / (float)sprite.texture.width, rect.height / (float)sprite.texture.height);
			yield return a;
			yield return a + Vector2.up * d.y;
			yield return a + d;
			yield return a + Vector2.right * d.x;
		}

		public IEnumerable<int> GetTriangles()
		{
			int j = 4 * i;
			yield return j;
			yield return j + 1;
			yield return j + 3;
			yield return j + 3;
			yield return j + 1;
			yield return j + 2;
		}
	}

	public float distance = 200f;

	public float width = 10f;

	public float height = 10f;

	public bool generateTangents;

	public Sprite[] sprites;

	private void Start()
	{
	}

	[ContextMenu("Generate Mesh")]
	public void GenerateMesh()
	{
		if (GetComponent<MeshFilter>() == null)
		{
			base.gameObject.AddComponent<MeshFilter>();
		}
		if (GetComponent<MeshRenderer>() == null)
		{
			base.gameObject.AddComponent<MeshRenderer>();
		}
		if (GetComponent<MeshRenderer>().sharedMaterial == null)
		{
			GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Diffuse"));
		}
		CloudInfo[] clouds = GetClouds();
		GetComponent<MeshFilter>().sharedMesh = GetMesh(clouds, generateTangents);
		GetComponent<MeshRenderer>().sharedMaterial.mainTexture = GetTexture(sprites);
	}

	private CloudInfo[] GetClouds()
	{
		float magnitude = new Vector2(distance, width / 2f).magnitude;
		float halfAngle = Mathf.Atan(width / (2f * distance));
		return GetClouds(sprites, Vector3.forward * magnitude, Vector3.up * height, halfAngle).ToArray();
	}

	private static Texture GetTexture(Sprite[] sprites)
	{
		if (sprites.Length == 0)
		{
			return null;
		}
		return sprites[0].texture;
	}

	private static Mesh GetMesh(CloudInfo[] clouds, bool generateTangents)
	{
		Mesh mesh = new Mesh();
		mesh.vertices = (from c in clouds
			from v in c.GetVertices()
			select v).ToArray();
		mesh.normals = (from c in clouds
			from n in c.GetNormals()
			select n).ToArray();
		if (generateTangents)
		{
			mesh.tangents = (from c in clouds
				from t in c.GetTangents()
				select t).ToArray();
		}
		mesh.uv = (from c in clouds
			from u in c.GetUv()
			select u).ToArray();
		mesh.triangles = (from c in clouds
			from t in c.GetTriangles()
			select t).ToArray();
		return mesh;
	}

	private static IEnumerable<CloudInfo> GetClouds(Sprite[] sprites, Vector3 forward, Vector3 up, float halfAngle)
	{
		Vector3 start = GetRotation((0f - halfAngle) * (float)sprites.Length) * forward;
		Quaternion angleIncrement = GetRotation(halfAngle * 2f);
		Vector3[] pointsOnCircle = GetPointsOnCircle(start, angleIncrement, sprites.Length + 1).ToArray();
		for (int i = 0; i < sprites.Length; i++)
		{
			yield return new CloudInfo
			{
				sprite = sprites[i],
				bottomLeft = pointsOnCircle[i],
				bottomRight = pointsOnCircle[i + 1],
				up = up,
				i = i
			};
		}
	}

	private static Quaternion GetRotation(float angle)
	{
		return Quaternion.Euler(0f, angle * 57.29578f, 0f);
	}

	private static IEnumerable<Vector3> GetPointsOnCircle(Vector3 start, Quaternion angleIncrement, int count)
	{
		for (int i = 0; i < count; i++)
		{
			yield return start;
			start = angleIncrement * start;
		}
	}
}
