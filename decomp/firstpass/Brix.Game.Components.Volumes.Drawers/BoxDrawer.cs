using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.Engine.Geometrie;
using Brix.Engine.Volumes;
using Brix.External.Factories;
using Brix.Legacy;
using Brix.Lifecycle.Pooling;
using Brix.Util;
using Brix.Utils;
using Brix.Utils.Collections;
using Brix.Utils.Unity;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Game.Components.Volumes.Drawers;

public class BoxDrawer : MonoBehaviour
{
	[Serializable]
	public class Spawner
	{
		[DisplayRules(DisplayFlags.ReadOnly)]
		[SerializeField]
		private BoxDrawer instance;

		[DisplayRules(DisplayFlags.HideAtRuntime)]
		[SerializeField]
		private string name;

		[DisplayRules(DisplayFlags.HideAtRuntime)]
		[SerializeField]
		private Transform parent;

		[DisplayRules(DisplayFlags.HideAtRuntime)]
		[SerializeField]
		private Material material;

		[DisplayRules(DisplayFlags.HideAtRuntime)]
		[SerializeField]
		private Mode mode;

		[DisplayRules(DisplayFlags.HideAtRuntime)]
		[SerializeField]
		private List<Faces> onlyTheseFaces;

		public BoxDrawer Instance => instance;

		public void Spawn()
		{
			if (mode != Mode.None)
			{
				ObjectPoolSingleton.RequestComponent<BoxDrawer>(out instance, VolumesUI.VolumeDrawer);
				instance.name = name;
				instance.transform.SetParent(parent);
				instance.transform.localPosition = Vector3.zero;
				instance.transform.localRotation = Quaternion.identity;
				instance.transform.localScale = Vector3.one;
				instance.referencePoint = parent;
				instance._renderer.sharedMaterial = material;
				instance.mode = mode;
				instance.onlyTheseFaces = onlyTheseFaces;
			}
		}

		public void Release()
		{
			ObjectPoolSingleton.Release(Instance.gameObject);
		}
	}

	public abstract class VolumeView
	{
		private static readonly VolumeView Box = new BoxView();

		private static readonly VolumeView Inside = new InsideView();

		private static readonly VolumeView Outside = new OutsideView();

		private static readonly VolumeView Outline = new OutlineView();

		private static readonly VolumeView IntersectionOutline = new IntersectionOutline();

		public abstract Niveaux NiveauxAt(XYZ pos, int config);

		public abstract IVoxelVolume FilterVolume(IVoxelVolume volume);

		public abstract void BuildMesh(IVoxelVolume volume, int configMask, MeshAugmentable meshInfo);

		public static VolumeView For(Mode mode)
		{
			return mode switch
			{
				Mode.Box => Box, 
				Mode.InsideTerrain => Inside, 
				Mode.OutsideTerrain => Outside, 
				Mode.Outline => Outline, 
				Mode.IntersectionOutline => IntersectionOutline, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}

		public static MeshAugmentable MeshFor(Mode mode)
		{
			return new MeshAugmentablePrealloue(0, useTangents: true);
		}

		protected static void BuildVolumeMesh(IVoxelVolume volume, int configMask, MeshAugmentablePrealloue meshInfo, VolumeView view)
		{
			IVoxelVolume v = view.FilterVolume(volume);
			Dictionary<XYZ, VoxelSurfaceConfig> dictionary = CollectionPool.RequestDictionary<XYZ, VoxelSurfaceConfig>();
			v.ComputeSurfaceConfigs(dictionary);
			PreAllocMeshFaces(meshInfo, dictionary);
			foreach (KeyValuePair<XYZ, VoxelSurfaceConfig> item in dictionary)
			{
				XYZ key = item.Key;
				int num = item.Value.config & configMask;
				Niveaux niveaux = view.NiveauxAt(key, num);
				for (int i = 0; i < 6; i++)
				{
					if ((num & (1 << i)) != 0)
					{
						Brix.Legacy.ProceduralCube.TunnelOuterFace(key, niveaux.top, niveaux.bottom, num, i, GetUv, meshInfo);
					}
				}
			}
			dictionary.Dispose();
		}
	}

	public class BoxView : VolumeView
	{
		public override void BuildMesh(IVoxelVolume volume, int configMask, MeshAugmentable meshInfo)
		{
			VolumeView.BuildVolumeMesh(volume, configMask, meshInfo as MeshAugmentablePrealloue, this);
		}

		public override Niveaux NiveauxAt(XYZ pos, int config)
		{
			return Niveaux.Default;
		}

		public override IVoxelVolume FilterVolume(IVoxelVolume volume)
		{
			return volume;
		}
	}

	public class InsideView : VolumeView
	{
		public override void BuildMesh(IVoxelVolume volume, int configMask, MeshAugmentable meshInfo)
		{
			VolumeView.BuildVolumeMesh(volume, configMask, meshInfo as MeshAugmentablePrealloue, this);
		}

		public override Niveaux NiveauxAt(XYZ pos, int config)
		{
			int x = Voxel.GetNiveaux(pos);
			int y = -1;
			if ((x & 0x3333) != 13107 && (config & 0x20) != 0 && Voxel.IsEmpty(pos - XYZ.up))
			{
				Donnees.Swap(ref x, ref y);
			}
			return new Niveaux(x, y);
		}

		public override IVoxelVolume FilterVolume(IVoxelVolume volume)
		{
			VoxelVolumeWithProperty voxelVolumeWithProperty = new VoxelVolumeWithProperty();
			voxelVolumeWithProperty.v = volume;
			voxelVolumeWithProperty.HasProperty = Voxel.IsTerrain;
			return voxelVolumeWithProperty;
		}
	}

	public class OutsideView : VolumeView
	{
		public override void BuildMesh(IVoxelVolume volume, int configMask, MeshAugmentable meshInfo)
		{
			VolumeView.BuildVolumeMesh(volume, configMask, meshInfo as MeshAugmentablePrealloue, this);
		}

		public override Niveaux NiveauxAt(XYZ pos, int config)
		{
			if (!Voxel.IsTerrain(pos))
			{
				return new Niveaux(-1, -1);
			}
			int top = -1;
			int bottom = 13107 - (0x3333 & Voxel.GetNiveaux(pos));
			return new Niveaux(top, bottom);
		}

		public override IVoxelVolume FilterVolume(IVoxelVolume volume)
		{
			VoxelVolumeWithProperty voxelVolumeWithProperty = new VoxelVolumeWithProperty();
			voxelVolumeWithProperty.v = volume;
			voxelVolumeWithProperty.HasProperty = (XYZ p) => !Voxel.IsTerrain(p) || !Voxel.IsFull(p);
			return voxelVolumeWithProperty;
		}
	}

	public class OutlineView : VolumeView
	{
		public override void BuildMesh(IVoxelVolume volume, int mask, MeshAugmentable meshInfo)
		{
			List<XYZ> list = volume.Voxels.Slinq().ToPooledList();
			PreAllocMeshContour(meshInfo as MeshAugmentablePrealloue, list, volume, volume, mask);
			foreach (XYZ voxel in volume.Voxels)
			{
				Brix.Legacy.ProceduralCube.ContourVoxel(voxel, volume, volume, meshInfo, mask);
			}
			list.Dispose();
		}

		public override IVoxelVolume FilterVolume(IVoxelVolume volume)
		{
			throw new NotImplementedException();
		}

		public override Niveaux NiveauxAt(XYZ pos, int config)
		{
			throw new NotImplementedException();
		}
	}

	public class IntersectionOutline : VolumeView
	{
		private List<Faces> ForLeftRight = new List<Faces>
		{
			Faces.up,
			Faces.front,
			Faces.back
		};

		private List<Faces> ForFrontBack = new List<Faces>
		{
			Faces.up,
			Faces.left,
			Faces.right
		};

		private List<Faces> ForCornerFrontLeft = new List<Faces>
		{
			Faces.up,
			Faces.back,
			Faces.right
		};

		private List<Faces> ForCornerFrontRight = new List<Faces>
		{
			Faces.up,
			Faces.back,
			Faces.left
		};

		private List<Faces> ForCornerBackLeft = new List<Faces>
		{
			Faces.up,
			Faces.front,
			Faces.right
		};

		private List<Faces> ForCornerBackRight = new List<Faces>
		{
			Faces.up,
			Faces.front,
			Faces.left
		};

		public override void BuildMesh(IVoxelVolume volume, int mask, MeshAugmentable meshInfo)
		{
			IVoxelVolume voxelVolume = FilterVolume(volume);
			IVoxelVolume totalVolume = new VoxelBounds(volume.Slinq().Min() - XYZ.up * 50, volume.Slinq().Max() + XYZ.up * 50);
			List<XYZ> list = voxelVolume.Voxels.Slinq().ToPooledList();
			PreAllocMeshContour(meshInfo as MeshAugmentablePrealloue, list, voxelVolume, totalVolume, mask);
			foreach (XYZ item in list)
			{
				Brix.Legacy.ProceduralCube.ContourVoxel(item, voxelVolume, totalVolume, meshInfo, MaskFor(volume, item), Voxel.GetNiveaux(item));
			}
			list.Dispose();
		}

		private static bool IntersectsTerrain(XYZ p)
		{
			return Voxel.IsTerrain(p);
		}

		public override IVoxelVolume FilterVolume(IVoxelVolume volume)
		{
			VoxelVolumeWithProperty voxelVolumeWithProperty = new VoxelVolumeWithProperty();
			voxelVolumeWithProperty.v = volume;
			voxelVolumeWithProperty.HasProperty = IntersectsTerrain;
			return voxelVolumeWithProperty;
		}

		public override Niveaux NiveauxAt(XYZ pos, int config)
		{
			throw new NotImplementedException();
		}

		private int MaskFor(IVoxelVolume volume, XYZ p)
		{
			bool flag = IsOnFace(volume, Faces.front, p);
			bool flag2 = IsOnFace(volume, Faces.back, p);
			bool flag3 = IsOnFace(volume, Faces.right, p);
			bool flag4 = IsOnFace(volume, Faces.left, p);
			if (flag && flag3)
			{
				return MaskFromFaces(ForCornerFrontRight);
			}
			if (flag && flag4)
			{
				return MaskFromFaces(ForCornerFrontLeft);
			}
			if (flag2 && flag3)
			{
				return MaskFromFaces(ForCornerBackRight);
			}
			if (flag2 && flag4)
			{
				return MaskFromFaces(ForCornerBackLeft);
			}
			if (flag || flag2)
			{
				return MaskFromFaces(ForFrontBack);
			}
			if (flag4 || flag3)
			{
				return MaskFromFaces(ForLeftRight);
			}
			return 0;
		}

		private bool IsOnFace(IVoxelVolume volume, Faces face, XYZ p)
		{
			return face switch
			{
				Faces.up => p.y == volume.Bounds.Max.y, 
				Faces.front => p.z == volume.Bounds.Max.z, 
				Faces.back => p.z == volume.Bounds.Min.z, 
				Faces.left => p.x == volume.Bounds.Min.x, 
				Faces.right => p.x == volume.Bounds.Max.x, 
				Faces.bottom => p.y == volume.Bounds.Min.y, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
	}

	public struct Niveaux
	{
		public int top;

		public int bottom;

		public static Niveaux Default = new Niveaux(-1, -1);

		public Niveaux(int top, int bottom)
		{
			this.top = top;
			this.bottom = bottom;
		}
	}

	public enum Mode
	{
		None,
		Box,
		Outline,
		InsideTerrain,
		OutsideTerrain,
		IntersectionOutline
	}

	[DisplayRules(DisplayFlags.HideInEditMode)]
	[SerializeField]
	private Transform referencePoint;

	[DisplayRules(DisplayFlags.HideInEditMode)]
	[SerializeField]
	private Mode mode = Mode.Box;

	[DisplayRules(DisplayFlags.HideInEditMode)]
	[SerializeField]
	private List<Faces> onlyTheseFaces;

	[DisplayRules(DisplayFlags.HideAtRuntime)]
	[SerializeField]
	private MeshFilter filter;

	[DisplayRules(DisplayFlags.HideAtRuntime)]
	[SerializeField]
	private MeshRenderer _renderer;

	private MeshAugmentable meshInfo;

	private IVoxelVolume volumeToDraw;

	private static readonly Vector2 UvMid = new Vector2(0.5f, 0.5f);

	public bool Showing
	{
		get
		{
			return _renderer.enabled;
		}
		set
		{
			_renderer.enabled = value;
		}
	}

	public Material Material
	{
		get
		{
			return _renderer.sharedMaterial;
		}
		set
		{
			_renderer.sharedMaterial = value;
		}
	}

	public List<Faces> OnlyTheseFaces => onlyTheseFaces;

	private static void PreAllocMeshContour(MeshAugmentablePrealloue mesh, List<XYZ> voxels, IVoxelVolume volume, IVoxelVolume totalVolume, int mask)
	{
		CountContour(voxels, volume, totalVolume, mask, out var vertices, out var triangles);
		mesh.AllocVertices(vertices, useTangents: true);
		mesh.AllocTriangles(triangles * 3);
	}

	private static void CountContour(List<XYZ> voxels, IVoxelVolume volume, IVoxelVolume TotalVolume, int mask, out int vertices, out int triangles)
	{
		vertices = 0;
		triangles = 0;
		foreach (XYZ voxel in voxels)
		{
			Brix.Legacy.ProceduralCube.ContourVoxel_Count(voxel, volume, TotalVolume, mask, ref vertices, ref triangles);
		}
	}

	private static void PreAllocMeshFaces(MeshAugmentablePrealloue mesh, Dictionary<XYZ, VoxelSurfaceConfig> configs)
	{
		int num = CountFaces(configs);
		mesh.AllocVertices(num * 4, useTangents: true);
		mesh.AllocTriangles(num * 6);
	}

	private static int CountFaces(Dictionary<XYZ, VoxelSurfaceConfig> configs)
	{
		int num = 0;
		foreach (KeyValuePair<XYZ, VoxelSurfaceConfig> config2 in configs)
		{
			int config = config2.Value.config;
			num += config.CountBits();
		}
		return num;
	}

	private void Awake()
	{
		base.enabled = false;
	}

	private void Update()
	{
		base.enabled = false;
		int configMask = MaskFromFaces(onlyTheseFaces);
		PrepareMesh();
		VolumeView.For(mode).BuildMesh(volumeToDraw, configMask, meshInfo);
		filter.mesh = meshInfo.Rendre(filter.mesh, referencePoint.worldToLocalMatrix, MeshRendreOptions.Disruptive);
	}

	private void PrepareMesh()
	{
		if (meshInfo == null)
		{
			meshInfo = VolumeView.MeshFor(mode);
		}
		else
		{
			meshInfo.Clear(realloc: false);
		}
	}

	public void Redraw(IVoxelVolume volume)
	{
		if (volume == null)
		{
			throw new ArgumentNullException("Trying to draw a null Volume");
		}
		volumeToDraw = volume;
		base.enabled = true;
	}

	private static Vector2 GetUv(int i, Vector2 uv)
	{
		return new Vector2(i % 4, i / 4) + PadUv(uv);
	}

	private static Vector2 PadUv(Vector2 uv)
	{
		float num = 0.88f;
		return (uv - UvMid) * num + UvMid;
	}

	public static int MaskFromFaces(List<Faces> faces)
	{
		if (faces.Count == 0)
		{
			return -1;
		}
		List<int> list = (from f in faces.Slinq()
			select (int)f).ToPooledList();
		int result = Config.FromFaceIndices(list);
		list.Dispose();
		return result;
	}

	private void OnDestroy()
	{
		UnityEngine.Object.Destroy(filter.mesh);
	}
}
