using UnityEngine;

public class MeshInfoCompact : MeshAbstrait
{
	public Index<Vector3> p2i;

	public Index<Vector3> n2i;

	public Index<Vector3> uv2i;

	public Index<Vector3> uv22i;

	public Index<long> s2i;

	public Index<long> t2i;

	public MeshInfoCompact()
	{
		p2i = new Index<Vector3>();
		n2i = new Index<Vector3>();
		uv2i = new Index<Vector3>();
		uv22i = new Index<Vector3>();
		s2i = new Index<long>();
		t2i = new Index<long>();
	}

	public override int GetVertexCount()
	{
		return s2i.Count;
	}

	public override int GetTriangleCount()
	{
		return t2i.Count;
	}

	public int Sommet(Vector3 p, Vector3 n, Vector2 uv, Vector2 uv2)
	{
		short a = (short)p2i[p];
		short b = (short)n2i[n];
		short c = (short)uv2i[uv];
		short d = (short)uv22i[uv2];
		long t = Donnees.LongFrom4Short(a, b, c, d);
		return s2i[t];
	}

	public bool Triangle(int a, int b, int c)
	{
		long t = Donnees.LongFrom4Short((short)a, (short)b, (short)c, 0);
		int num = t2i[t];
		return num == t2i.Count - 1;
	}

	public void Compresser()
	{
		p2i.Seal();
		n2i.Seal();
		uv2i.Seal();
		uv22i.Seal();
		s2i.Seal();
		t2i.Seal();
	}

	public override MeshModifiable Transcrire(MeshModifiable meshDst)
	{
		int count = s2i.Count;
		Vector3[] array = new Vector3[count];
		Vector3[] array2 = new Vector3[count];
		Vector2[] array3 = new Vector2[count];
		Vector2[] array4 = new Vector2[count];
		for (int i = 0; i < count; i++)
		{
			Donnees.LongTo4Short(s2i[i], out var a, out var b, out var c, out var d);
			ref Vector3 reference = ref array[i];
			reference = p2i[a];
			ref Vector3 reference2 = ref array2[i];
			reference2 = n2i[b];
			ref Vector2 reference3 = ref array3[i];
			reference3 = uv2i[c];
			ref Vector2 reference4 = ref array4[i];
			reference4 = uv22i[d];
		}
		int count2 = t2i.Count;
		int[] array5 = new int[3 * count2];
		int num = 0;
		for (int j = 0; j < count2; j++)
		{
			Donnees.LongTo4Short(t2i[j], out var a2, out var b2, out var c2, out var _);
			array5[num++] = a2;
			array5[num++] = b2;
			array5[num++] = c2;
		}
		meshDst.Clear();
		meshDst.SetVertices(array);
		meshDst.SetNormals(array2);
		meshDst.SetUv(array3);
		meshDst.SetUv2(array4);
		meshDst.SetTriangles(array5);
		return meshDst;
	}
}
