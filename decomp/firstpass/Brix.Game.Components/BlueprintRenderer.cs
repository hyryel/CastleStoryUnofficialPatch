using System.Collections.Generic;
using System.Linq;
using Brix.Engine;
using Brix.Engine.Geometrie;
using Brix.External.Factories;
using Brix.External.Signals;
using Brix.Game.Components.Volumes;
using Brix.Util;
using Brix.Utils;
using Brix.Utils.Logging;
using Brix.Utils.Signals;
using UnityEngine;

namespace Brix.Game.Components;

[RequireComponent(typeof(Blueprint))]
[RequireComponent(typeof(SignalHandler))]
public class BlueprintRenderer : MonoBehaviour
{
	private static BrixLogger logger = new BrixLogger(typeof(BlueprintRenderer).FullName);

	public Mesh initialMesh;

	public XYZ[] cases;

	public int[] configs;

	public int[] selfConfigs;

	protected Signal Disabled = new Signal();

	private int[] exConfigs;

	private Blueprint _blueprint;

	private void Awake()
	{
		if (initialMesh == null)
		{
			initialMesh = GetComponent<MeshFilter>().sharedMesh;
		}
		_blueprint = GetComponent<Blueprint>();
	}

	private void OnEnable()
	{
		GetComponent<BlueprintVolume>().VolumeChanged.Connect(OnVolumeChange).DisconnectWhen(Disabled);
	}

	private void OnDisable()
	{
		Disabled.Invoke();
	}

	private void Start()
	{
		if (!GetComponent<Blueprint>().IsAnti)
		{
			OnVolumeChange();
		}
	}

	[ContextMenu("Refresh")]
	private void Refresh()
	{
		if (!Donnees.Equals(exConfigs, configs))
		{
			exConfigs = (int[])configs.Clone();
			RebuildMesh();
		}
	}

	[ContextMenu("RebuildMesh")]
	private void RebuildMesh()
	{
		Factory.AssetKey assetKey = GetComponent<AssetKeyHolder>().AssetKey;
		GameObject gameObject = Factory.Peek(assetKey);
		Factory.AssetKey key = new Factory.AssetKey("Blocks_MeshDescriptors", gameObject.name);
		if (!Factory.Exist(key))
		{
			GetComponent<MeshFilter>().sharedMesh = initialMesh;
			if ((bool)GetComponent<MeshCollider>())
			{
				GetComponent<MeshCollider>().sharedMesh = initialMesh;
			}
			return;
		}
		List<Mesh> list = new List<Mesh>();
		List<Vector3> list2 = new List<Vector3>();
		List<Vector3> list3 = new List<Vector3>();
		List<Quaternion> list4 = new List<Quaternion>();
		Dictionary<XYZ, int> dictionary = Enumerable.Range(0, cases.Length).ToDictionary((int i) => cases[i], (int i) => i);
		Transform transform = base.transform;
		Vector3 vector = transform.position + transform.rotation * GetComponent<VoxelTransform>().CenterOffset;
		Quaternion rotation = transform.rotation;
		VoxelizedMesh voxelMesh = Factory.Peek(key).GetComponent<VoxelizedMeshDescriptor>().voxelMesh;
		VoxelizedMesh.VoxelData[] voxels = voxelMesh.voxels;
		for (int num = 0; num < voxels.Length; num++)
		{
			Vector3 vector2 = vector + rotation * voxels[num].position.ToVector3();
			XYZ key2 = XYZ.FromVector3(Calcule.RoundVecteur(vector2));
			int config = configs[dictionary[key2]];
			config = Calcule.InverseRotatedConfig(config, transform.rotation);
			list.Add(voxels[num].partialMeshes[config]);
			list2.Add(voxels[num].position.ToVector3() - voxelMesh.pivot);
			list3.Add(vector2);
			list4.Add(Quaternion.identity);
		}
		List<Vector2> list5 = new List<Vector2>();
		List<MeshCombineUtility.MeshInstance> list6 = new List<MeshCombineUtility.MeshInstance>(list.Count);
		for (int num2 = 0; num2 < list.Count; num2++)
		{
			XYZ xYZ = XYZ.FromVector3(list3[num2]);
			Vector2 item = new Vector2(xYZ.x | (xYZ.z << 10), xYZ.y);
			int vertexCount = list[num2].vertexCount;
			for (int num3 = 0; num3 < vertexCount; num3++)
			{
				list5.Add(item);
			}
			list6.Add(new MeshCombineUtility.MeshInstance
			{
				mesh = list[num2],
				subMeshIndex = 0,
				transform = Matrix4x4.TRS(list2[num2], list4[num2], Vector3.one)
			});
		}
		Mesh mesh = MeshCombineUtility.Combine(list6, generateStrips: false);
		mesh.uv2 = list5.ToArray();
		MeshFilter component = GetComponent<MeshFilter>();
		if (component.mesh != null && component.mesh != initialMesh)
		{
			Object.Destroy(component.mesh);
		}
		component.mesh = mesh;
	}

	[ContextMenu("RefreshCases")]
	private void RefreshCases()
	{
		cases = GetComponent<IVolume>().GetCases(VProperty.fantome).ToArray();
	}

	[ContextMenu("RefreshSelfConfigs")]
	private void RefreshSelfConfigs()
	{
		selfConfigs = new int[cases.Length];
		Factory.AssetKey assetKey = this.GetInterface<AssetKeyHolder>().AssetKey;
		GameObject gameObject = Factory.Peek(assetKey);
		Factory.AssetKey key = new Factory.AssetKey("Blocks_MeshDescriptors", gameObject.name);
		if (!Factory.Exist(key))
		{
			for (int i = 0; i < cases.Length; i++)
			{
				selfConfigs[i] = 63;
			}
			return;
		}
		VoxelizedMesh voxelMesh = Factory.Peek(key).GetComponent<VoxelizedMeshDescriptor>().voxelMesh;
		VoxelizedMesh.VoxelData[] voxels = voxelMesh.voxels;
		Transform transform = base.transform;
		Vector3 vector = transform.position + transform.rotation * GetComponent<VoxelTransform>().CenterOffset;
		Quaternion rotation = transform.rotation;
		for (int j = 0; j < voxels.Length; j++)
		{
			Vector3 vecteur = vector + rotation * voxels[j].position.ToVector3();
			XYZ xYZ = XYZ.FromVector3(Calcule.RoundVecteur(vecteur));
			int num = 0;
			VoxelizedMesh.FaceData[] faces = voxels[j].faces;
			foreach (VoxelizedMesh.FaceData faceData in faces)
			{
				if (faceData.type == VoxelizedMesh.FaceType.Full)
				{
					num |= 1 << (int)faceData.direction;
				}
			}
			int num2 = Calcule.InverseRotatedConfig(num, transform.rotation);
			for (int l = 0; l < cases.Length; l++)
			{
				if (cases[l] == xYZ)
				{
					selfConfigs[l] = num2;
				}
			}
		}
	}

	[ContextMenu("RefreshConfigs")]
	private void RefreshConfigs()
	{
		configs = new int[cases.Length];
		for (int i = 0; i < cases.Length; i++)
		{
			configs[i] = CalculateConfig(cases[i]);
		}
	}

	private static XYZ RotateAndTranslateXYZ(XYZ p, Transform t)
	{
		Vector3 vector = t.position + t.rotation * t.GetComponent<VoxelTransform>().CenterOffset;
		Quaternion rotation = t.rotation;
		return XYZ.FromVector3(vector + rotation * p.ToVector3());
	}

	private int CalculateConfig(XYZ index)
	{
		int num = 0;
		for (int i = 0; i < 6; i++)
		{
			if (!HasOccluderAt(index, i))
			{
				num |= 1 << i;
			}
		}
		return num;
	}

	private bool HasOccluderAt(XYZ index, int faceIndex)
	{
		XYZ xYZ = index + CubeModele.XYZDirs[faceIndex];
		Component component = VolumeRegistry.VolumesWithPropertyAt(xYZ, VProperty.fantome).Slinq().FirstOrDefault() as Component;
		if (component == null)
		{
			return false;
		}
		if (!IsInSameProject(component.GetComponent<Blueprint>()))
		{
			return false;
		}
		Factory.AssetKey assetKey = component.GetInterface<AssetKeyHolder>().AssetKey;
		Factory.AssetKey key = new Factory.AssetKey("Blocks_MeshDescriptors", assetKey.Name);
		if (!Factory.Exist(key))
		{
			return false;
		}
		VoxelizedMesh voxelMesh = Factory.Peek(key).GetComponent<VoxelizedMeshDescriptor>().voxelMesh;
		Transform transform = component.transform;
		XYZ xYZ2 = xYZ;
		int faceIndex2 = Calcule.InvertFaceIndex(faceIndex);
		int config = Calcule.FaceIndexToConfig(faceIndex2);
		config = Calcule.InverseRotatedConfig(config, transform.rotation);
		faceIndex2 = Calcule.ScalaireToFaceIndex(Calcule.ConfigToScalaire(config));
		VoxelizedMesh.VoxelData voxelData = null;
		VoxelizedMesh.VoxelData[] voxels = voxelMesh.voxels;
		foreach (VoxelizedMesh.VoxelData voxelData2 in voxels)
		{
			if (RotateAndTranslateXYZ(voxelData2.position, transform) == xYZ2)
			{
				voxelData = voxelData2;
				break;
			}
		}
		if (voxelData == null)
		{
			logger.Error(string.Concat("Error when trying to hide blueprint faces. VolumeRegistry says there is a blueprint at position ", xYZ2, ", but blueprint says there isn't!"));
			return false;
		}
		VoxelizedMesh.FaceData faceData = null;
		VoxelizedMesh.FaceData[] faces = voxelData.faces;
		foreach (VoxelizedMesh.FaceData faceData2 in faces)
		{
			if (faceData2.direction == (VoxelizedMesh.FaceDirection)faceIndex2)
			{
				faceData = faceData2;
				break;
			}
		}
		if (faceData == null)
		{
			return false;
		}
		return faceData.type == VoxelizedMesh.FaceType.Full;
	}

	private bool IsInSameProject(Blueprint other)
	{
		return other != null && _blueprint.Project == other.Project;
	}

	private void OnVolumeChange()
	{
		RefreshCases();
		RefreshConfigs();
		Refresh();
	}
}
