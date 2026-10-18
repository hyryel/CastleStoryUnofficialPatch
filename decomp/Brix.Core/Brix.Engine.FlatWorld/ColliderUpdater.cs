using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Brix.Engine.FlatWorld.Layers;
using Brix.Engine.FlatWorld.SoftAlgos;
using Brix.Engine.Geometrie;
using UnityEngine;

namespace Brix.Engine.FlatWorld;

public class ColliderUpdater : MonoBehaviour
{
	private struct MeshDescriptor
	{
		public Vector3[] Vertices;

		public int[] Triangles;
	}

	public XYZ mvPosition;

	private static TestWorldConfig3 _test;

	private MeshCollider _meshCollider;

	private BoxCollider _boxCollider;

	private Mesh _mesh;

	private float _activeUntil;

	private LevelConfigNormalUvs _levelConfigNormalUvs = new LevelConfigNormalUvs();

	private static readonly MeshDescriptor[,] Configs = GetColliderConfigLookup(dessous: false);

	private bool IsMeshColliderActive => _meshCollider != null;

	private MeshCollider MeshCollider
	{
		get
		{
			if (_meshCollider == null)
			{
				_meshCollider = base.gameObject.GetOrAddComponent<MeshCollider>();
			}
			return _meshCollider;
		}
	}

	private BoxCollider BoxCollider
	{
		get
		{
			if (_boxCollider == null)
			{
				_boxCollider = base.gameObject.GetOrAddComponent<BoxCollider>();
				_boxCollider.size = _test.MvSize.ToVector3();
				_boxCollider.center = (_boxCollider.size - Vector3.one) / 2f;
				_boxCollider.isTrigger = true;
			}
			return _boxCollider;
		}
	}

	private void Start()
	{
		if (_test == null)
		{
			_test = Object.FindObjectOfType<TestWorldConfig3>();
		}
		UpdatePosition();
	}

	private void OnDestroy()
	{
		if (_mesh != null)
		{
			Object.Destroy(_mesh);
			_mesh = null;
		}
	}

	private void OnTriggerStay(Collider collider)
	{
		if (!collider.isTrigger)
		{
			_activeUntil = 1f;
			if (!IsMeshColliderActive)
			{
				RefreshCollider();
				StartCoroutine(WaitAndDestroyCollider());
			}
		}
	}

	private IEnumerator WaitAndDestroyCollider()
	{
		while (_activeUntil > 0f)
		{
			yield return new WaitForEndOfFrame();
			_activeUntil -= Time.deltaTime;
		}
		Object.Destroy(MeshCollider);
		Object.Destroy(_mesh);
		_mesh = null;
	}

	[ContextMenu("UpdatePosition")]
	private void UpdatePosition()
	{
		mvPosition = XYZ.Div(XYZ.FromVector3(base.transform.position), _test.MvSize);
	}

	[ContextMenu("RefreshCollider")]
	private void RefreshCollider()
	{
		XYZ mvSize = _test.MvSize;
		XYZ border = _test.Border;
		XYZ xYZ = mvPosition;
		XYZ xYZ2 = XYZ.Mul(xYZ, mvSize);
		_test.InitLayers();
		IFlatWorldColors cachedWorld = _test.drawer.GetCachedWorld(xYZ);
		_test.InitFromLayers(cachedWorld, xYZ2, mvSize, border, force: true);
		if (cachedWorld is FlatWorld)
		{
			(cachedWorld as FlatWorld).InitPreRender(_test.Common, xYZ2, 0, _test.softAlgos, force: true);
		}
		else
		{
			cachedWorld.InitPreRender(_test.Common, xYZ2, 0, _levelConfigNormalUvs);
		}
		Color32[] pixels = cachedWorld.Offset.GetPixels32();
		Color32[] pixels2 = cachedWorld.Config.GetPixels32();
		Color32[] pixels3 = cachedWorld.Level.GetPixels32();
		int countOffsets = cachedWorld.CountOffsets;
		CountMeshSizeForCollider(pixels, pixels2, pixels3, countOffsets, out var vertexCount, out var triangleCount);
		Vector3[] vertices = new Vector3[vertexCount];
		int[] triangles = new int[triangleCount];
		RenderMeshForCollider(pixels, pixels2, pixels3, countOffsets, vertices, triangles);
		AssignMeshForCollider(vertices, triangles);
	}

	private void CountMeshSizeForCollider(Color32[] offsetColors, Color32[] configColors, Color32[] levelColors, int count, out int vertexCount, out int triangleCount)
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < count; i++)
		{
			byte r = configColors[i].r;
			if (r != 0)
			{
				MeshDescriptor meshDescriptor = Configs[r, 0];
				num += meshDescriptor.Vertices.Length;
				num2 += meshDescriptor.Triangles.Length;
			}
		}
		vertexCount = num;
		triangleCount = num2;
	}

	private void RenderMeshForCollider(Color32[] offsetColors, Color32[] configColors, Color32[] levelColors, int count, Vector3[] vertices, int[] triangles)
	{
		XYZ border = _test.Border;
		int offset = 0;
		int offset2 = 0;
		for (int i = 0; i < count; i++)
		{
			byte r = configColors[i].r;
			if (r != 0)
			{
				Color32 c = offsetColors[i];
				XYZ xYZ = Soft.DecodeXYZ(Soft.DecodeInt(c));
				Color32 color = levelColors[i];
				int num = ((color.r + 42) / 85) | ((color.g + 42) / 85 << 2) | ((color.b + 42) / 85 << 4) | ((color.a + 42) / 85 << 6);
				MeshDescriptor meshDescriptor = Configs[r, num];
				CopyTriangles(meshDescriptor.Triangles, triangles, ref offset2, offset);
				CopyVertices(meshDescriptor.Vertices, vertices, ref offset, (xYZ - border).ToVector3());
			}
		}
	}

	private void AssignMeshForCollider(Vector3[] vertices, int[] triangles)
	{
		MeshCollider.sharedMesh = null;
		if (_mesh == null)
		{
			_mesh = new Mesh
			{
				vertices = vertices,
				triangles = triangles
			};
		}
		else
		{
			_mesh.Clear();
			_mesh.vertices = vertices;
			_mesh.triangles = triangles;
		}
		MeshCollider.sharedMesh = _mesh;
	}

	private void CopyVertices(Vector3[] src, Vector3[] dst, ref int offset, Vector3 add)
	{
		int num = src.Length;
		for (int i = 0; i < num; i++)
		{
			dst[offset++] = src[i] + add;
		}
	}

	private void CopyTriangles(int[] src, int[] dst, ref int offset, int add)
	{
		int num = src.Length;
		for (int i = 0; i < num; i++)
		{
			dst[offset++] = src[i] + add;
		}
	}

	private static MeshDescriptor GetCubeVertexIndicesFromConfig(int config, int niveaux, bool dessous)
	{
		IntClasse intClasse = new IntClasse();
		for (int i = 0; i < 6; i++)
		{
			if ((config & (1 << i)) != 0)
			{
				for (int j = 0; j < 4; j++)
				{
					intClasse.Ajout(CubeModele.SommetIndices[i * 4 + j]);
				}
			}
		}
		List<Vector3> list = new List<Vector3>();
		Vector3[] sommetPositions = CubeModele.SommetPositions;
		for (int k = 0; k < intClasse.Count; k++)
		{
			int num = intClasse[k];
			float num2 = (float)((niveaux >> 2 * num) & 3) / 3f;
			Vector3 item = sommetPositions[num];
			if (num < 4 && !dessous)
			{
				item.y = num2 - 0.5f;
			}
			else if (num >= 4 && dessous)
			{
				item.y = 0.5f - num2;
			}
			list.Add(item);
		}
		List<int> list2 = new List<int>();
		for (int l = 0; l < 6; l++)
		{
			if ((config & (1 << l)) != 0)
			{
				int[] a = new int[4];
				for (int m = 0; m < 4; m++)
				{
					a[m] = intClasse.Find(CubeModele.SommetIndices[l * 4 + m]);
				}
				list2.AddRange(new int[6] { 0, 1, 2, 2, 1, 3 }.Select((int t) => a[t]));
			}
		}
		return new MeshDescriptor
		{
			Vertices = list.ToArray(),
			Triangles = list2.ToArray()
		};
	}

	private static MeshDescriptor[,] GetColliderConfigLookup(bool dessous)
	{
		MeshDescriptor[,] array = new MeshDescriptor[64, 256];
		for (int i = 0; i < 64; i++)
		{
			for (int j = 0; j < 256; j++)
			{
				array[i, j] = GetCubeVertexIndicesFromConfig(i, j, dessous);
			}
		}
		return array;
	}
}
