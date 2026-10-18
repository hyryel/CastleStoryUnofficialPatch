using System;
using System.Collections.Generic;
using Brix.Components;
using Brix.Engine.Abstract;
using Brix.Engine.Environment;
using Brix.Engine.Environment.Configs;
using Brix.Engine.Nature;
using Brix.External.Factories;
using Brix.Legacy;
using Brix.Utils;
using Brix.Utils.Logging;
using Smooth.Algebraics;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Engine;

public class MegaVoxel
{
	public enum NatureType
	{
		Tree,
		Plant,
		Boulder,
		Iron,
		Pyrite
	}

	public class MegaVoxelCommon
	{
		public Material[] treeMaterials;

		public Material[][] plantMaterials;

		public Material[] boulderMaterials;

		public Material[] ironMaterials;

		public Material[] pyriteMaterials;

		public MegaVoxelTreePrinter treeDrawer;

		public MegaVoxelPlantPrinter plantDrawer;

		public MegaVoxelBoulderPrinter boulderDrawer;

		public MegaVoxelIronPrinter ironDrawer;

		public MegaVoxelPyritePrinter pyriteDrawer;

		public MegaVoxelTreePrinter distantTreeDrawer;

		public MegaVoxelPlantPrinter distantPlantDrawer;

		private readonly Dictionary<XYZ, DistantBatchedTrees> _distantTrees = new Dictionary<XYZ, DistantBatchedTrees>();

		private readonly Dictionary<Tuple<XYZ, int>, DistantBatchedPlants> _distantPlants = new Dictionary<Tuple<XYZ, int>, DistantBatchedPlants>();

		public void Init()
		{
			Biome biome = GameParam.BiomeToLoad();
			treeMaterials = biome.Trees.GetComponent<LodMaterialList>().ToArray(5);
			plantMaterials = new Material[6][]
			{
				biome.Plants[0].GetComponent<LodMaterialList>().ToArray(6),
				biome.Plants[1].GetComponent<LodMaterialList>().ToArray(6),
				biome.Plants[2].GetComponent<LodMaterialList>().ToArray(6),
				biome.Plants[3].GetComponent<LodMaterialList>().ToArray(6),
				biome.Plants[4].GetComponent<LodMaterialList>().ToArray(6),
				biome.Plants[5].GetComponent<LodMaterialList>().ToArray(6)
			};
			boulderMaterials = biome.Boulders.GetComponent<LodMaterialList>().ToArray(5);
			ironMaterials = Factory.Peek(Brix.External.Factories.Environment.IronMat).GetComponent<LodMaterialList>().ToArray(5);
			pyriteMaterials = Factory.Peek(Brix.External.Factories.Environment.PyriteMat).GetComponent<LodMaterialList>().ToArray(5);
			treeDrawer = new MegaVoxelTreePrinter();
			plantDrawer = new MegaVoxelPlantPrinter();
			boulderDrawer = new MegaVoxelBoulderPrinter();
			ironDrawer = new MegaVoxelIronPrinter();
			pyriteDrawer = new MegaVoxelPyritePrinter();
			distantTreeDrawer = new MegaVoxelTreePrinter();
			distantPlantDrawer = new MegaVoxelPlantPrinter();
		}

		public void Destroy()
		{
			treeDrawer.Destroy();
			plantDrawer.Destroy();
			boulderDrawer.Destroy();
			ironDrawer.Destroy();
			pyriteDrawer.Destroy();
			distantTreeDrawer.Destroy();
			distantPlantDrawer.Destroy();
			foreach (DistantBatchedTrees value in _distantTrees.Values)
			{
				value.Destroy();
			}
			_distantTrees.Clear();
			foreach (DistantBatchedPlants value2 in _distantPlants.Values)
			{
				value2.Destroy();
			}
			_distantPlants.Clear();
		}

		public DistantNatureBatchedObject GetOrCreateDistantTrees(XYZ mvOffset)
		{
			return _distantTrees.GetValueOrCreate(mvOffset / 64, () => new DistantBatchedTrees
			{
				MvOffset = mvOffset / 64 * 64
			});
		}

		public void DestroyDistantTrees(XYZ mvOffset)
		{
			_distantTrees.GetValueOrDefaultAndRemove(mvOffset / 64, null)?.Destroy();
		}

		public DistantNatureBatchedObject GetOrCreateDistantPlants(XYZ mvOffset, int variation)
		{
			return _distantPlants.GetValueOrCreate(Tuple.Create(mvOffset / 64, variation), () => new DistantBatchedPlants
			{
				MvOffset = mvOffset / 64 * 64,
				Variation = variation
			});
		}

		public void DestroyDistantPlants(XYZ mvOffset, int variation)
		{
			_distantPlants.GetValueOrDefaultAndRemove(Tuple.Create(mvOffset / 64, variation), null)?.Destroy();
		}
	}

	public class BatchedTrees : NatureBatchedObject
	{
		private MegaVoxelTreePrinter Drawer => Common.treeDrawer;

		public override LodTrack LodProfile => BrixSingleton<NatureLodManager>.Instance.Trees;

		protected override Material[] Materials => Common.treeMaterials;

		protected override string Category => "Trees";

		protected override bool HasDistant => true;

		public override void GetWarpTex()
		{
			Drawer.GetWarpTex(ref WarpTex, Positions, MvOffset);
		}

		protected override Mesh GetCombinedMesh()
		{
			return Drawer.GetCombinedMesh(Count, base.MaxCountSoFar, Mathf.Min(Lod, LodProfile.LodMax), WarpTex.width, 0);
		}

		protected override MeshModifiable ComputeColliderMeshInfo()
		{
			return Drawer.ComputeColliderMeshInfo(Positions, BrixSingleton<NatureLodManager>.Instance.Trees.ColliderLod, base.Center);
		}

		public override void InitDistantBatchedObject()
		{
			Distant = Common.GetOrCreateDistantTrees(MvOffset);
			Distant.AddChild(this);
		}

		public override void Destroy()
		{
			base.Destroy();
			Distant.RemoveChild(this);
			if (Distant.Children.Count == 0)
			{
				Common.DestroyDistantTrees(MvOffset);
			}
		}
	}

	public class BatchedPlants : NatureBatchedObject
	{
		public int Variation;

		private MegaVoxelPlantPrinter Drawer => Common.plantDrawer;

		public override LodTrack LodProfile => BrixSingleton<NatureLodManager>.Instance.Plants;

		protected override Material[] Materials => Common.plantMaterials[Variation];

		protected override string Category => "Plants";

		protected override bool HasCollider => false;

		protected override bool HasDistant => true;

		public override void GetWarpTex()
		{
			Drawer.GetWarpTex(ref WarpTex, Positions, MvOffset);
		}

		protected override Mesh GetCombinedMesh()
		{
			return Drawer.GetCombinedMesh(Count, base.MaxCountSoFar, Mathf.Min(Lod, LodProfile.LodMax), WarpTex.width, Variation);
		}

		public override void InitDistantBatchedObject()
		{
			Distant = Common.GetOrCreateDistantPlants(MvOffset, Variation);
			Distant.AddChild(this);
		}

		public override void Destroy()
		{
			base.Destroy();
			Distant.RemoveChild(this);
			if (Distant.Children.Count == 0)
			{
				Common.DestroyDistantPlants(MvOffset, Variation);
			}
		}
	}

	public class BatchedBoulders : NatureBatchedObject<BoulderData>
	{
		private MegaVoxelBoulderPrinter Drawer => Common.boulderDrawer;

		public override LodTrack LodProfile => BrixSingleton<NatureLodManager>.Instance.Boulders;

		protected override Material[] Materials => Common.boulderMaterials;

		protected override string Category => "Boulders";

		public override void GetWarpTex()
		{
			Drawer.GetWarpTex(ref WarpTex, Data, MvOffset);
		}

		protected override Mesh GetCombinedMesh()
		{
			return Drawer.GetCombinedMesh(Data, Mathf.Min(Lod, LodProfile.LodMax), WarpTex.width);
		}

		protected override MeshModifiable ComputeColliderMeshInfo()
		{
			return Drawer.ComputeColliderMeshInfo(Data, BrixSingleton<NatureLodManager>.Instance.Boulders.ColliderLod, base.Center);
		}
	}

	public class BatchedIrons : NatureBatchedObject
	{
		private MegaVoxelIronPrinter Drawer => Common.ironDrawer;

		public override LodTrack LodProfile => BrixSingleton<NatureLodManager>.Instance.Minerals;

		protected override Material[] Materials => Common.ironMaterials;

		protected override string Category => "Irons";

		public override void GetWarpTex()
		{
			Drawer.GetWarpTex(ref WarpTex, Positions, MvOffset);
		}

		protected override Mesh GetCombinedMesh()
		{
			return Drawer.GetCombinedMesh(Count, base.MaxCountSoFar, Mathf.Min(Lod, LodProfile.LodMax), WarpTex.width, 0);
		}

		protected override MeshModifiable ComputeColliderMeshInfo()
		{
			return Drawer.ComputeColliderMeshInfo(Positions, BrixSingleton<NatureLodManager>.Instance.Minerals.ColliderLod, base.Center);
		}
	}

	public class BatchedPyrites : NatureBatchedObject
	{
		private MegaVoxelPyritePrinter Drawer => Common.pyriteDrawer;

		public override LodTrack LodProfile => BrixSingleton<NatureLodManager>.Instance.Minerals;

		protected override Material[] Materials => Common.pyriteMaterials;

		protected override string Category => "Pyrites";

		public override void GetWarpTex()
		{
			Drawer.GetWarpTex(ref WarpTex, Positions, MvOffset);
		}

		protected override Mesh GetCombinedMesh()
		{
			return Drawer.GetCombinedMesh(Count, base.MaxCountSoFar, Mathf.Min(Lod, LodProfile.LodMax), WarpTex.width, 0);
		}

		protected override MeshModifiable ComputeColliderMeshInfo()
		{
			return Drawer.ComputeColliderMeshInfo(Positions, BrixSingleton<NatureLodManager>.Instance.Minerals.ColliderLod, base.Center);
		}
	}

	public class DistantBatchedTrees : DistantNatureBatchedObject
	{
		private MegaVoxelTreePrinter Drawer => Common.distantTreeDrawer;

		public override LodTrack LodProfile => BrixSingleton<NatureLodManager>.Instance.Trees;

		protected override Material[] Materials => Common.treeMaterials;

		protected override string Category => "DistantTrees";

		public DistantBatchedTrees()
			: base(64, BrixSingleton<NatureLodManager>.Instance.Trees.LodCount)
		{
		}

		public override void GetWarpTex()
		{
			Drawer.GetWarpTex(ref WarpTex, Positions, MvOffset);
		}

		protected override Mesh GetCombinedMesh()
		{
			return GetDistantBoundedMesh(Drawer.GetCombinedMesh(Count, base.MaxCountSoFar, Mathf.Min(Lod, LodProfile.LodMax), WarpTex.width, 0), 64);
		}
	}

	public class DistantBatchedPlants : DistantNatureBatchedObject
	{
		public int Variation;

		private MegaVoxelPlantPrinter Drawer => Common.distantPlantDrawer;

		public override LodTrack LodProfile => BrixSingleton<NatureLodManager>.Instance.Plants;

		protected override Material[] Materials => Common.plantMaterials[Variation];

		protected override string Category => "DistantPlants";

		public DistantBatchedPlants()
			: base(64, BrixSingleton<NatureLodManager>.Instance.Plants.LodCount)
		{
			Drawer.TranslationPacking = 3;
		}

		public override void GetWarpTex()
		{
			Drawer.GetWarpTex(ref WarpTex, Positions, MvOffset);
		}

		protected override Mesh GetCombinedMesh()
		{
			return GetDistantBoundedMesh(Drawer.GetCombinedMesh(Count, base.MaxCountSoFar, Mathf.Min(Lod, LodProfile.LodMax), WarpTex.width, Variation), 64);
		}

		protected override void InitMaterial(Material m)
		{
			base.InitMaterial(m);
			m.SetFloat("_TPacking", Drawer.TranslationPacking);
		}
	}

	public class BaseNatureBatchedObject
	{
		public List<XYZ> Positions = new List<XYZ>();

		public GameObject GameObject;

		public Texture2D WarpTex;

		public Mesh ColliderMesh;

		public Material[] LodMaterials;

		public int Lod;

		public DistantNatureBatchedObject Distant;

		public XYZ MvOffset;

		public bool IsVisible = true;

		private int _maxCountSoFar;

		public virtual int Count => Positions.Count;

		protected int MaxCountSoFar
		{
			get
			{
				if (Count == 0)
				{
					_maxCountSoFar = 0;
				}
				else
				{
					_maxCountSoFar = Mathf.Max(_maxCountSoFar, Count);
				}
				return _maxCountSoFar;
			}
			set
			{
				_maxCountSoFar = value;
			}
		}

		public Vector3 Center => MvOffset.ToVector3() + offsetMilieu;

		public virtual void Clear()
		{
			Positions.Clear();
		}

		public virtual void Destroy()
		{
			Positions.Clear();
			MaxCountSoFar = 0;
			Lod = 0;
			IsVisible = true;
			if (GameObject != null)
			{
				UnityEngine.Object.Destroy(GameObject);
				GameObject = null;
			}
			if (WarpTex != null)
			{
				UnityEngine.Object.Destroy(WarpTex);
				WarpTex = null;
			}
			if (ColliderMesh != null)
			{
				UnityEngine.Object.Destroy(ColliderMesh);
				ColliderMesh = null;
			}
			if (LodMaterials == null)
			{
				return;
			}
			for (int i = 0; i < LodMaterials.Length; i++)
			{
				if (LodMaterials[i] != null)
				{
					UnityEngine.Object.Destroy(LodMaterials[i]);
				}
			}
			LodMaterials = null;
		}

		public virtual void SetVisible(bool visible)
		{
			IsVisible = visible;
			if (GameObject != null)
			{
				GameObject.GetComponent<MeshRenderer>().enabled = visible;
			}
		}

		public void SetLod(int newLod)
		{
			int lod = Lod;
			Lod = newLod;
			if (Distant != null)
			{
				Distant.UpdateChildLod(this, lod, newLod);
			}
		}
	}

	public abstract class NatureBatchedObject : BaseNatureBatchedObject
	{
		public abstract LodTrack LodProfile { get; }

		protected abstract Material[] Materials { get; }

		protected abstract string Category { get; }

		protected virtual bool HasCollider => true;

		protected virtual bool HasDistant => false;

		public abstract void GetWarpTex();

		protected abstract Mesh GetCombinedMesh();

		protected virtual MeshModifiable ComputeColliderMeshInfo()
		{
			return null;
		}

		public virtual void InitDistantBatchedObject()
		{
		}

		public void UpdateObject()
		{
			if (Count > 0)
			{
				CreateGameObject();
				UpdateMesh();
				UpdateCollider();
			}
			else
			{
				Destroy();
			}
		}

		public void CreateGameObject()
		{
			if (GameObject == null)
			{
				GameObject = CreateGameObject(Category, HasCollider);
			}
		}

		private GameObject CreateGameObject(string category, bool hasCollider)
		{
			GameObject gameObject = new GameObject(category + " " + MvOffset, typeof(MeshRenderer), typeof(MeshFilter));
			gameObject.transform.position = base.Center;
			gameObject.isStatic = true;
			gameObject.layer = 17;
			if (hasCollider)
			{
				gameObject.AddComponent<MeshCollider>();
			}
			Neo.ClasseMoi("MegaVoxels/" + category, gameObject);
			return gameObject;
		}

		private Material GetMaterial(int lod, Material[] originalMaterials)
		{
			if (LodMaterials == null)
			{
				LodMaterials = new Material[originalMaterials.Length];
			}
			lod = Mathf.Clamp(lod, 0, LodMaterials.Length - 1);
			if (LodMaterials[lod] == null)
			{
				LodMaterials[lod] = new Material(originalMaterials[lod]);
				InitMaterial(LodMaterials[lod]);
			}
			return LodMaterials[lod];
		}

		protected virtual void InitMaterial(Material m)
		{
			m.SetTexture("_WarpTex", WarpTex);
		}

		public void UpdateMesh()
		{
			for (int num = GameObject.transform.childCount - 1; num >= 0; num--)
			{
				UnityEngine.Object.Destroy(GameObject.transform.GetChild(num).gameObject);
			}
			GameObject.GetComponent<MeshFilter>().sharedMesh = GetCombinedMesh();
			GameObject.GetComponent<Renderer>().sharedMaterial = GetMaterial(Lod, Materials);
		}

		private void UpdateCollider()
		{
			if (HasCollider)
			{
				MeshModifiable meshModifiable = ComputeColliderMeshInfo();
				if (meshModifiable != null)
				{
					AssignCollider(meshModifiable);
				}
			}
		}

		private void AssignCollider(MeshModifiable meshInfo)
		{
			MeshCollider component = GameObject.GetComponent<MeshCollider>();
			if (ColliderMesh == null)
			{
				ColliderMesh = new Mesh();
			}
			meshInfo.Rendre(ColliderMesh);
			component.sharedMesh = null;
			component.sharedMesh = ColliderMesh;
		}

		public bool CalculateLod(Vector3 cameraPosition)
		{
			float sqrMagnitude = (cameraPosition - base.Center).sqrMagnitude;
			LodRange lodRange = LodProfile.GetLodRange(Lod);
			float num = Mathf.Max(Screen.height, 720f) / 1080f;
			if (Lod > 0 && sqrMagnitude < (float)(lodRange.Min * lodRange.Min) * num * num)
			{
				SetLod(Lod - 1);
				return true;
			}
			if (Lod < LodProfile.LodMax && sqrMagnitude > (float)(lodRange.Max * lodRange.Max) * num * num)
			{
				SetLod(Lod + 1);
				return true;
			}
			return false;
		}
	}

	public abstract class NatureBatchedObject<TData> : NatureBatchedObject
	{
		public HashSet<TData> Data = new HashSet<TData>();

		public override int Count => (Data.Count <= 0) ? Positions.Count : Data.Count;

		public override void Clear()
		{
			base.Clear();
			Data.Clear();
		}

		public override void Destroy()
		{
			base.Destroy();
			Data.Clear();
		}
	}

	public abstract class DistantNatureBatchedObject : NatureBatchedObject
	{
		public const int DistantMergeLod = 4;

		public List<BaseNatureBatchedObject> Children = new List<BaseNatureBatchedObject>();

		public int[] LodCounts;

		public int DistantMegaVoxelSize;

		public bool IsListDirty;

		protected override bool HasCollider => false;

		public int WantedLod
		{
			get
			{
				for (int i = 0; i < LodCounts.Length; i++)
				{
					if (LodCounts[i] > 0)
					{
						return i;
					}
				}
				return 0;
			}
		}

		public DistantNatureBatchedObject(int distantMegaVoxelSize, int lodCount)
		{
			DistantMegaVoxelSize = distantMegaVoxelSize;
			LodCounts = new int[lodCount];
		}

		public override void Destroy()
		{
			base.Destroy();
			Children.Clear();
			Array.Clear(LodCounts, 0, LodCounts.Length);
			IsListDirty = false;
		}

		public void AddChild(BaseNatureBatchedObject child)
		{
			Children.Add(child);
		}

		public void RemoveChild(BaseNatureBatchedObject child)
		{
			Children.Remove(child);
		}

		public void UpdateChildLod(BaseNatureBatchedObject child, int oldLod, int newLod)
		{
			LodCounts[oldLod]--;
			LodCounts[newLod]++;
		}

		public void UpdateList()
		{
			Positions.Clear();
			foreach (BaseNatureBatchedObject child in Children)
			{
				child.Positions.Slinq().AddTo(Positions);
			}
			IsListDirty = false;
		}

		public bool UpdateVisibility()
		{
			if (IsListDirty && Lod >= (LodProfile as IDistantMergeLod).DistantMergeLod)
			{
				UpdateList();
				GetWarpTex();
				UpdateObject();
			}
			bool flag = Lod >= (LodProfile as IDistantMergeLod).DistantMergeLod && Count > 0;
			if (flag != IsVisible)
			{
				SetVisible(flag);
				foreach (BaseNatureBatchedObject child in Children)
				{
					child.SetVisible(!flag);
				}
			}
			return flag;
		}

		public bool CalculateLod()
		{
			int wantedLod = WantedLod;
			if (Lod != wantedLod)
			{
				Lod = wantedLod;
				return true;
			}
			return false;
		}
	}

	private static BrixLogger logger = new BrixLogger(typeof(MegaVoxel).FullName);

	public const int nVoxels = 4096;

	public const int nMegaVoxels = 262144;

	public const int DistantTreeSize = 64;

	public const int DistantPlantSize = 64;

	public const int ArbreLodMax = 3;

	public static readonly Vector3 offsetMilieu = Vector3.one * 16f / 2f;

	public XYZ scalaire;

	public int gScalaire;

	public XYZ MvOffset;

	public Vector3 Center;

	public HashSet<XYZ> voxelData = new HashSet<XYZ>();

	public List<IVoxel> surface = new List<IVoxel>();

	public List<IBlockData> factoried = new List<IBlockData>();

	private readonly MaterialCombineDispatcher _combineDispatch = new MaterialCombineDispatcher();

	private const int NatureTypeCount = 5;

	private Dictionary<int, NatureBatchedObject> _natureObjects = new Dictionary<int, NatureBatchedObject>();

	public static MegaVoxelCommon Common = new MegaVoxelCommon();

	protected static Transform CamTransform;

	public bool HasNature => _natureObjects.Count > 0;

	public List<XYZ> TreePositions
	{
		get
		{
			NatureBatchedObject natureOrDefault = GetNatureOrDefault(NatureType.Tree);
			return (natureOrDefault == null) ? Empty.List<XYZ>() : natureOrDefault.Positions;
		}
	}

	public List<XYZ> PyritePositions
	{
		get
		{
			NatureBatchedObject natureOrDefault = GetNatureOrDefault(NatureType.Pyrite);
			return (natureOrDefault == null) ? Empty.List<XYZ>() : natureOrDefault.Positions;
		}
	}

	public MegaVoxel(Vector3 v)
	{
		MvOffset = XYZ.FromVector3(v);
		Center = v + offsetMilieu;
		gScalaire = Brix.Engine.Abstract.Voxel.GetGScalaire(v);
		scalaire = XYZ.FromVector3(v);
		if (CamTransform == null)
		{
			CamTransform = ((!Camera.main) ? null : Camera.main.transform);
		}
		GameSignals.Connect(GameSignals.LeaveGame, Destroy);
		if (v.y >= 1000f)
		{
			logger.WarnFormat("MegaVoxel con: {0}", v);
		}
	}

	private NatureBatchedObject GetNatureOrDefault(NatureType type, int variation = 0)
	{
		int key = (int)(type + 5 * variation);
		return _natureObjects.GetValueOrDefault(key);
	}

	private TNature GetOrCreateNature<TNature>(NatureType type, int variation, Func<NatureType, int, TNature> createFunc) where TNature : NatureBatchedObject
	{
		int key = (int)(type + 5 * variation);
		if (!_natureObjects.TryGetValue(key, out var value))
		{
			value = createFunc(type, variation);
			value.MvOffset = MvOffset;
			value.InitDistantBatchedObject();
			_natureObjects.Add(key, value);
		}
		return value as TNature;
	}

	private void DestroyNature(NatureType type, int variation)
	{
		int key = (int)(type + 5 * variation);
		if (_natureObjects.TryGetValue(key, out var value))
		{
			value.Destroy();
			_natureObjects.Remove(key);
		}
	}

	private void Destroy()
	{
		foreach (KeyValuePair<int, NatureBatchedObject> natureObject in _natureObjects)
		{
			natureObject.Value.Destroy();
		}
		_natureObjects.Clear();
	}

	public static void StaticInit()
	{
		Common.Init();
	}

	public static void StaticDestroy()
	{
		Common.Destroy();
	}

	public void AddVoxel(XYZ position)
	{
		voxelData.Add(position);
	}

	public HashSet<XYZ> GetVoxels()
	{
		return voxelData;
	}

	public void List()
	{
		HashSet<IBlockData> hashSet = CollectionPool.RequestHashSet<IBlockData>();
		surface.Clear();
		foreach (XYZ voxel in GetVoxels())
		{
			IVoxel rootOrSelf = Voxel.GetVoxel(voxel).RootOrSelf;
			if (!rootOrSelf.IsNull)
			{
				if (rootOrSelf.IsTerrain)
				{
					surface.Add(rootOrSelf);
				}
				else
				{
					hashSet.Add((IBlockData)rootOrSelf);
				}
			}
		}
		factoried.Clear();
		hashSet.Slinq().AddTo(factoried);
		hashSet.Dispose();
	}

	public void AfficherSpecial()
	{
		_combineDispatch.ClearAll();
		if (factoried.Count > 0)
		{
			_combineDispatch.Add(factoried);
			_combineDispatch.ApplyCombines();
			_combineDispatch.SetLayer(10);
		}
	}

	public void UpdateNatureLists()
	{
		bool hasNature = HasNature;
		foreach (KeyValuePair<int, NatureBatchedObject> natureObject in _natureObjects)
		{
			natureObject.Value.Clear();
		}
		foreach (XYZ voxel in GetVoxels())
		{
			if (TreeRegistry.Instance.HasTree(voxel))
			{
				if (Brix.Engine.Abstract.Voxel.GetGScalaire(voxel.ToScalaire()) != gScalaire)
				{
					logger.WarnFormat("Invalid tree position: wrong megavoxel. Proceeding anyway. {0}", voxel);
				}
				else if (!Voxel.IsCrust(voxel))
				{
					logger.WarnFormat("Invalid tree position: wrong ground type. Proceeding anyway. {0}", voxel);
				}
				if (!DamagedTree.AllDamagedTrees.ContainsKey(voxel))
				{
					GetOrCreateNature(NatureType.Tree, 0, (NatureType t, int v) => new BatchedTrees()).Positions.Add(voxel);
				}
			}
			if (PlantRegistry.IsPlant(voxel))
			{
				int plantType = PlantRegistry.PlantAt(voxel).PlantType;
				GetOrCreateNature(NatureType.Plant, plantType, (NatureType t, int v) => new BatchedPlants
				{
					Variation = v
				}).Positions.Add(voxel);
			}
			BoulderData boulder = BoulderRegistry.GetBoulder(voxel);
			if (boulder != null && boulder.RootPos == voxel && BoulderRegistry.GetDamage(boulder.RootPos) <= 0f)
			{
				GetOrCreateNature(NatureType.Boulder, 0, (NatureType t, int v) => new BatchedBoulders()).Data.Add(boulder);
			}
			if (IronRegistry.HasMinerals(voxel) && IronRegistry.GetDamage(voxel) <= 0f)
			{
				GetOrCreateNature(NatureType.Iron, 0, (NatureType t, int v) => new BatchedIrons()).Positions.Add(voxel);
			}
			if (PyriteRegistry.HasMinerals(voxel) && PyriteRegistry.GetDamage(voxel) <= 0f)
			{
				GetOrCreateNature(NatureType.Pyrite, 0, (NatureType t, int v) => new BatchedPyrites()).Positions.Add(voxel);
			}
		}
		foreach (KeyValuePair<int, NatureBatchedObject> natureObject2 in _natureObjects)
		{
			NatureBatchedObject value = natureObject2.Value;
			if (value.Count == 0)
			{
				value.Destroy();
			}
		}
		_natureObjects.RemoveAll((KeyValuePair<int, NatureBatchedObject> kv) => kv.Value.Count == 0);
		foreach (KeyValuePair<int, NatureBatchedObject> natureObject3 in _natureObjects)
		{
			NatureBatchedObject value2 = natureObject3.Value;
			value2.GetWarpTex();
			value2.UpdateObject();
			if (value2.Distant != null)
			{
				value2.Distant.IsListDirty = true;
			}
		}
		if (hasNature != HasNature)
		{
			if (hasNature)
			{
				Architecte.instance.EnleverDuJardinier(this);
			}
			else
			{
				Architecte.instance.AjouterAuJardinier(this);
			}
		}
	}

	public void CalculateNatureLod()
	{
		if (_natureObjects.Count == 0)
		{
			return;
		}
		Vector3 position = CamTransform.position;
		foreach (KeyValuePair<int, NatureBatchedObject> natureObject in _natureObjects)
		{
			NatureBatchedObject value = natureObject.Value;
			if (value.CalculateLod(position))
			{
				value.UpdateMesh();
				if (value.Distant != null && value.Distant.CalculateLod() && value.Distant.UpdateVisibility())
				{
					value.Distant.UpdateMesh();
				}
			}
		}
	}

	private static Mesh GetDistantBoundedMesh(Mesh mesh, int distantMegaVoxelSize)
	{
		Bounds bounds = default(Bounds);
		bounds.SetMinMax(-offsetMilieu - Vector3.one / 2f, Vector3.one * distantMegaVoxelSize);
		mesh.bounds = bounds;
		return mesh;
	}
}
