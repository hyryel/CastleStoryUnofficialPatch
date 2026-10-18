using System.Collections.Generic;
using UnityEngine;

public abstract class MeshMultiple : MeshModifiable
{
	public class Profile
	{
		public int id;

		public string name;

		public Material material;

		public int layer;

		public MeshModifiable meshInfo;

		public Profile(int id, string name, Material material)
		{
			this.id = id;
			this.name = name;
			this.material = material;
		}

		public void Clear()
		{
			if (meshInfo != null)
			{
				meshInfo.Clear();
			}
			meshInfo = null;
		}
	}

	protected Classeur<Profile> profiles = new Classeur<Profile>();

	protected Dictionary<string, int> s2i = new Dictionary<string, int>();

	public Profile this[int i]
	{
		get
		{
			if (i == profiles.Count)
			{
				profiles.Add(new Profile(i, null, null));
			}
			return profiles[i];
		}
	}

	public Profile this[string s]
	{
		get
		{
			if (!s2i.TryGetValue(s, out var value))
			{
				value = profiles.Count;
				profiles.Add(new Profile(value, s, null));
				s2i.Add(s, value);
			}
			return profiles[value];
		}
	}

	public int Count => profiles.Count;

	public int InitProfile(string name, Material material)
	{
		if (s2i.TryGetValue(name, out var value))
		{
			return value;
		}
		value = profiles.Count;
		profiles.Add(new Profile(value, name, material));
		s2i.Add(name, value);
		return value;
	}

	public override void Clear()
	{
		for (int i = 0; i < profiles.Count; i++)
		{
			profiles[i].Clear();
		}
	}

	public new abstract GameObject Rendre(GameObject gameObject);

	public override int GetVertexCount()
	{
		return this[0].meshInfo.GetVertexCount();
	}

	public override int GetTriangleCount()
	{
		return this[0].meshInfo.GetTriangleCount();
	}

	public override Vector3[] GetVertices()
	{
		return this[0].meshInfo.GetVertices();
	}

	public override Vector3[] GetNormals()
	{
		return this[0].meshInfo.GetNormals();
	}

	public override Vector2[] GetUv()
	{
		return this[0].meshInfo.GetUv();
	}

	public override Vector2[] GetUv2()
	{
		return this[0].meshInfo.GetUv2();
	}

	public override Vector4[] GetTangents()
	{
		return this[0].meshInfo.GetTangents();
	}

	public override int[] GetTriangles()
	{
		return this[0].meshInfo.GetTriangles();
	}

	public override void SetVertices(Vector3[] other)
	{
		this[0].meshInfo.SetVertices(other);
	}

	public override void SetNormals(Vector3[] other)
	{
		this[0].meshInfo.SetNormals(other);
	}

	public override void SetUv(Vector2[] other)
	{
		this[0].meshInfo.SetUv(other);
	}

	public override void SetUv2(Vector2[] other)
	{
		this[0].meshInfo.SetUv2(other);
	}

	public override void SetTangents(Vector4[] other)
	{
		this[0].meshInfo.SetTangents(other);
	}

	public override void SetTriangles(int[] other)
	{
		this[0].meshInfo.SetTriangles(other);
	}
}
