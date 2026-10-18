using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Brix.Engine;
using Brix.External.Factories;
using Brix.Utils;
using Brix.Utils.Collections;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Noise;

public class VoxelDrawerComponent : MonoBehaviour
{
	[HideInInspector]
	public List<VoxelViewerTypes> VoxelTypes = new List<VoxelViewerTypes>();

	public Dictionary<VoxelViewerTypes, int> CountPerVoxelType = new Dictionary<VoxelViewerTypes, int>();

	public MondeAtlasComponent AtlasComponent;

	public Texture2D HeightMap;

	[Range(0f, 1000f)]
	public float MegaVoxelDisplay;

	public bool FillEmpty;

	public Color32 FillColor;

	private NoiseViewerMeshGenerator _noiseViewerMeshGenerator = new NoiseViewerMeshGenerator();

	private Dictionary<Vector3, GameObject> megaVoxelInstances = new Dictionary<Vector3, GameObject>();

	private uint MegaVoxelSize;

	private int WorldCubicSize;

	public GameObject MegaVoxelPrefab;

	public GameObject TerrainContainer;

	private float previousMegaVoxelDisplay;

	private bool started;

	private Queue<Vector3> RequestedMegaVoxels = new Queue<Vector3>();

	private bool isRunning = true;

	private void Initialize()
	{
		if (HeightMap != null)
		{
			_noiseViewerMeshGenerator.SetHeightMap(HeightMap);
		}
		MegaVoxelSize = 16u;
		WorldCubicSize = 7;
		VoxelNoiseConfigurator.Init();
		_noiseViewerMeshGenerator.SetMegaVoxelSize(MegaVoxelSize);
		LaunchWorkersThreads();
		StartCoroutine(InstantiateMegaVoxel());
		started = true;
	}

	private void LaunchWorkersThreads()
	{
		for (int i = 0; i < 1; i++)
		{
			Thread thread = new Thread(FetchRequestAndProcess);
			thread.Start();
		}
	}

	private void FetchRequestAndProcess()
	{
		while (isRunning)
		{
			Vector3 position = Vector3.zero;
			bool flag = false;
			lock (RequestedMegaVoxels)
			{
				if (RequestedMegaVoxels.Count > 0)
				{
					position = RequestedMegaVoxels.Dequeue();
					flag = true;
				}
			}
			if (flag)
			{
				_noiseViewerMeshGenerator.GenerateMegaVoxel(position);
			}
			else
			{
				Thread.Sleep(1);
			}
		}
	}

	private void GenerateAll(int width, int depth, int height)
	{
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				for (int k = 0; k < depth; k++)
				{
					Vector3 item = new Vector3(i * MegaVoxelSize, j * MegaVoxelSize, k * MegaVoxelSize);
					RequestedMegaVoxels.Enqueue(item);
				}
			}
		}
	}

	private void Stop()
	{
		isRunning = false;
	}

	private void OnDestroy()
	{
		Stop();
	}

	public Dictionary<VoxelViewerTypes, int> GetVoxelStats()
	{
		return _noiseViewerMeshGenerator.TypeStats;
	}

	private void Update()
	{
		if (MegaVoxelDisplay == previousMegaVoxelDisplay)
		{
			return;
		}
		previousMegaVoxelDisplay = MegaVoxelDisplay;
		foreach (KeyValuePair<Vector3, GameObject> megaVoxelInstance in megaVoxelInstances)
		{
			bool active = megaVoxelInstance.Key.x > MegaVoxelDisplay;
			megaVoxelInstance.Value.SetActive(active);
		}
	}

	public void Refresh()
	{
		if (!started)
		{
			Factory.Instantiate<VoxelNoiseConfigurator>(Brix.External.Factories.Noise.NOISE_CONFIG);
			VoxelNoiseConfigurator.Init();
			Initialize();
		}
		int width = ((!(HeightMap != null)) ? WorldCubicSize : (HeightMap.width / 16));
		int depth = ((!(HeightMap != null)) ? WorldCubicSize : (HeightMap.height / 16));
		int height = ((!(HeightMap != null)) ? WorldCubicSize : 16);
		lock (_noiseViewerMeshGenerator.MeshDataQueue)
		{
			_noiseViewerMeshGenerator.MeshDataQueue.Clear();
		}
		CountPerVoxelType.Clear();
		VoxelTypes.Clear();
		megaVoxelInstances.Slinq().ForEach(delegate(KeyValuePair<Vector3, GameObject> m)
		{
			Object.Destroy(m.Value);
		});
		megaVoxelInstances.Clear();
		RefreshNoises();
		GenerateAll(width, depth, height);
	}

	public void RefreshNoises()
	{
		List<KeyValuePair<VoxelTerrainType, VoxelNoiseConfigurator.NoiseConfig>> list = VoxelNoiseConfigurator.GetTerrainTypesWithNoise().ToPooledList();
		foreach (KeyValuePair<VoxelTerrainType, VoxelNoiseConfigurator.NoiseConfig> item in list)
		{
			VoxelTerrainType type = item.Key;
			VoxelNoiseConfigurator.NoiseConfig value = item.Value;
			VoxelViewerTypes voxelViewerTypes = new VoxelViewerTypes();
			voxelViewerTypes.Color = (from t in AtlasComponent.atlas.deepTypeColors.Slinq()
				where t.type == (int)type
				select t).FirstOrDefault().color;
			voxelViewerTypes.Name = AtlasComponent.atlas.GetTypeName((int)type);
			voxelViewerTypes.NoiseComponent = value.NoiseComponent;
			voxelViewerTypes.UseRange = value.UseRange;
			voxelViewerTypes.Start = value.Start;
			voxelViewerTypes.End = value.End;
			voxelViewerTypes.Threshold = value.Threshold;
			VoxelTypes.Add(voxelViewerTypes);
		}
		CollectionPool.Release(ref list);
		_noiseViewerMeshGenerator.SetVoxelTypes(VoxelTypes);
		_noiseViewerMeshGenerator.SetFillColor(FillColor);
		_noiseViewerMeshGenerator.SetFillEmpty(FillEmpty);
		_noiseViewerMeshGenerator.RefreshNoises();
	}

	private IEnumerator InstantiateMegaVoxel()
	{
		while (true)
		{
			InstantiateNextMegaVoxel();
			yield return new WaitForSeconds(0.01f);
		}
	}

	private void InstantiateNextMegaVoxel()
	{
		if (_noiseViewerMeshGenerator.MeshDataQueue.Count <= 0)
		{
			return;
		}
		MeshData meshData;
		lock (_noiseViewerMeshGenerator.MeshDataQueue)
		{
			meshData = _noiseViewerMeshGenerator.MeshDataQueue.Dequeue();
		}
		megaVoxelInstances.TryGetValue(meshData.Position, out var value);
		if (value == null)
		{
			value = Object.Instantiate(MegaVoxelPrefab);
			value.transform.SetParent(TerrainContainer.transform);
			megaVoxelInstances[meshData.Position] = value;
		}
		else
		{
			value = megaVoxelInstances[meshData.Position];
		}
		value.transform.position = meshData.Position;
		MeshFilter component = value.GetComponent<MeshFilter>();
		component.mesh.Clear(keepVertexLayout: false);
		component.mesh.vertices = meshData.Vertices.ToArray();
		component.mesh.triangles = meshData.Triangles.ToArray();
		component.mesh.colors32 = meshData.Colors.ToArray();
		component.mesh.RecalculateNormals();
		meshData.VoxelCount.Slinq().ForEach(delegate(KeyValuePair<VoxelViewerTypes, int> vc, VoxelDrawerComponent self)
		{
			if (!self.CountPerVoxelType.ContainsKey(vc.Key))
			{
				self.CountPerVoxelType[vc.Key] = 0;
			}
			self.CountPerVoxelType[vc.Key] += vc.Value;
		}, this);
	}
}
