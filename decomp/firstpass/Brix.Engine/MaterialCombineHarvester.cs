using System.Collections.Generic;
using Brix.Engine.Blocks;
using Brix.Utils;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Engine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]
public class MaterialCombineHarvester : MonoBehaviour
{
	private CompCache<MeshRenderer> _renderer;

	private CompCache<MeshFilter> _filter;

	private CompCache<MeshCollider> _collider;

	private List<Material> _matList;

	private List<VoxelDisplayData> _displayDataList;

	private string _blockId;

	private void Awake()
	{
		_renderer = new CompCache<MeshRenderer>(base.gameObject);
		_filter = new CompCache<MeshFilter>(base.gameObject);
		_collider = new CompCache<MeshCollider>(base.gameObject);
		_renderer.Get.enabled = false;
		_collider.Get.enabled = false;
		_matList = new List<Material>();
		_displayDataList = new List<VoxelDisplayData>();
	}

	public bool Matches(string id, Material[] mats)
	{
		return _blockId == id && MatchesMaterials(mats);
	}

	private bool MatchesMaterials(Material[] mats)
	{
		return mats.Length == _matList.Count && mats.Slinq().All((Material mat, MaterialCombineHarvester self) => self._matList.Contains(mat), this);
	}

	public void ConfigureHarvester(string id, Material[] mats)
	{
		_blockId = id;
		mats.Slinq().AddTo(_matList);
		_renderer.Get.sharedMaterials = mats;
		_renderer.Get.enabled = true;
	}

	public void Add(VoxelDisplayData voxelDisplayDatas)
	{
		_displayDataList.Add(voxelDisplayDatas);
	}

	public void ApplyCombine()
	{
		int num = 0;
		List<Vector2> list = CollectionPool.RequestList<Vector2>();
		List<Vector2> list2 = CollectionPool.RequestList<Vector2>();
		List<MeshCombineUtility.MeshInstance> list3 = CollectionPool.RequestList<MeshCombineUtility.MeshInstance>();
		foreach (VoxelDisplayData displayData in _displayDataList)
		{
			XYZ xYZ = XYZ.FromVector3(displayData.Position);
			Vector2 uvTilingOffset = displayData.uvTilingOffset;
			Mesh mesh = displayData.Mesh;
			Vector2[] uv = mesh.uv;
			Vector2 item = new Vector2(xYZ.x | (xYZ.z << 10), xYZ.y);
			int vertexCount = displayData.Mesh.vertexCount;
			for (int i = 0; i < vertexCount; i++)
			{
				list.Add(uv[i] + uvTilingOffset);
				list2.Add(item);
			}
			list3.Add(new MeshCombineUtility.MeshInstance
			{
				mesh = displayData.Mesh,
				subMeshIndex = 0,
				transform = displayData.Matrix
			});
			num++;
		}
		Mesh mesh2 = MeshCombineUtility.Combine(list3, generateStrips: false);
		mesh2.SetUVs(0, list);
		mesh2.SetUVs(1, list2);
		list.Dispose();
		list2.Dispose();
		list3.Dispose();
		ClearMesh();
		ClearCollider();
		_filter.Get.mesh = mesh2;
		_filter.Get.mesh.RecalculateBounds();
		_collider.Get.sharedMesh = _filter.Get.mesh;
		_collider.Get.enabled = true;
	}

	public void Clear()
	{
		_displayDataList.Clear();
		ClearMaterials();
		ClearMesh();
		ClearCollider();
	}

	private void ClearMesh()
	{
		Object.Destroy(_filter.Get.mesh);
		_filter.Get.mesh = null;
	}

	private void ClearCollider()
	{
		Object.Destroy(_collider.Get.sharedMesh);
		_collider.Get.sharedMesh = null;
		_collider.Get.enabled = false;
	}

	private void ClearMaterials()
	{
		_renderer.Get.sharedMaterials = Empty.Array<Material>();
		_renderer.Get.enabled = false;
	}
}
