using System.Collections.Generic;
using Brix.Common.Collections;
using Brix.External.Factories;
using Brix.Game.AI.Damage;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Legacy;
using Brix.Util;
using Brix.Utils;
using Brix.Utils.Collections;
using Brix.Utils.Logging;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using UnityEngine;

namespace Brix.Engine.Nature;

public class BoulderRegistry : GameComponent
{
	public static BoulderRegistry Instance;

	private static readonly XYZ MvSize = XYZ.one * 16;

	private readonly Dictionary<XYZ, BoulderData> _boulders = new Dictionary<XYZ, BoulderData>();

	private readonly Dictionary<XYZ, HashSet<XYZ>> _mvRootPositions = new Dictionary<XYZ, HashSet<XYZ>>();

	private Dictionary<BoulderData, DamagedBoulderData> _damagedBoulders = new Dictionary<BoulderData, DamagedBoulderData>();

	private IDamageReceiver BoulderSmallDamageReceiver;

	private IDamageReceiver BoulderMediumDamageReceiver;

	private IDamageReceiver BoulderLargeDamageReceiver;

	protected static BrixLogger logger = new BrixLogger(typeof(BoulderRegistry).FullName);

	public const int garnottePerHit = 4;

	public const float HealingPerSecond = 5f;

	public override void Initialize()
	{
		base.Initialize();
		Instance = this;
	}

	public override void OnRequested()
	{
		base.OnRequested();
		BoulderSmallDamageReceiver = Factory.Peek(Brix.External.Factories.Environment.BoulderSmall).GetComponent<SimpleDamageReceiver>();
		BoulderMediumDamageReceiver = Factory.Peek(Brix.External.Factories.Environment.BoulderMedium).GetComponent<SimpleDamageReceiver>();
		BoulderLargeDamageReceiver = Factory.Peek(Brix.External.Factories.Environment.BoulderLarge).GetComponent<SimpleDamageReceiver>();
	}

	public static void AddAllBoulderPositionsToList(ref List<XYZ> list)
	{
		list.AddRange(Instance._boulders.Keys);
	}

	public static bool HasBoulders(XYZ pos)
	{
		return Instance._boulders.ContainsKey(pos);
	}

	public static bool HasBoulders(List<XYZ> plist)
	{
		foreach (XYZ item in plist)
		{
			if (Instance._boulders.ContainsKey(item))
			{
				return true;
			}
		}
		return false;
	}

	public static BoulderData GetBoulder(XYZ pos)
	{
		return Instance._boulders.GetValueOrDefault(pos);
	}

	public static void GetBoulders(List<XYZ> positions, ref HashSet<BoulderData> resultBoulders)
	{
		resultBoulders.Clear();
		foreach (XYZ position in positions)
		{
			BoulderData boulder = GetBoulder(position);
			if (boulder != null)
			{
				resultBoulders.Add(boulder);
			}
		}
	}

	public static void Add(Vector3 centerPosition, float growth, Vector3 offset, BoulderSize size)
	{
		List<XYZ> list = PreviewBoulderSpace(centerPosition, size, offset);
		foreach (XYZ item in list)
		{
			if (NatureRegistry.HasNatureAt(item))
			{
				return;
			}
		}
		BoulderData boulderData = new BoulderData(growth, SizeToVariation(size), centerPosition, offset, size);
		Instance._mvRootPositions.GetValueOrCreate(XYZ.Div(boulderData.RootPos, MvSize)).Add(boulderData.RootPos);
		foreach (XYZ @case in boulderData.Volume.GetCases(VProperty.occupy))
		{
			Instance._boulders.Add(@case, boulderData);
			GameSignals.Invoke(GameSignals.BouldersChanged, @case);
		}
	}

	public static void Remove(XYZ position)
	{
		BoulderData boulder = GetBoulder(position);
		if (boulder == null)
		{
			return;
		}
		HashSet<XYZ> valueOrDefault = Instance._mvRootPositions.GetValueOrDefault(XYZ.Div(boulder.RootPos, MvSize));
		if (valueOrDefault != null && valueOrDefault.Remove(boulder.RootPos) && valueOrDefault.Count == 0)
		{
			Instance._mvRootPositions.Remove(XYZ.Div(boulder.RootPos, MvSize));
		}
		boulder.Unregister();
		foreach (XYZ @case in boulder.Volume.GetCases(VProperty.occupy))
		{
			Instance._boulders.Remove(@case);
			GameSignals.Invoke(GameSignals.BouldersChanged, @case);
		}
		boulder.Dispose();
	}

	public static void DeblocCrust(BoulderData boulder)
	{
		foreach (XYZ @case in boulder.Volume.GetCases(VProperty.occupy))
		{
			using IVoxel voxel = Voxel.GetVoxel(@case);
			if (voxel.IsCrust)
			{
				voxel.DeBlockClient(-1);
			}
		}
	}

	public static bool AddOrGrow(Vector3 centerPosition, float growth, Vector3 offset, BoulderSize size)
	{
		bool result = false;
		List<XYZ> list = PreviewBoulderSpace(centerPosition, size, offset);
		HashSet<BoulderData> resultBoulders = CollectionPool.RequestHashSet<BoulderData>();
		GetBoulders(list, ref resultBoulders);
		if (resultBoulders.Count > 0)
		{
			foreach (BoulderData item in resultBoulders)
			{
				float growth2 = item.Growth;
				item.Growth += growth;
				if (item.Growth != growth2)
				{
					result = true;
				}
			}
			foreach (XYZ item2 in list)
			{
				GameSignals.Invoke(GameSignals.BouldersChanged, item2);
			}
		}
		else
		{
			foreach (XYZ item3 in list)
			{
				NatureData natureData = NatureRegistry.NatureAt(item3);
				if (natureData.IsValid && !(natureData is BoulderData))
				{
					resultBoulders.Dispose();
					return result;
				}
			}
			BoulderData boulderData = new BoulderData(growth, SizeToVariation(size), centerPosition, offset, size);
			Instance._mvRootPositions.GetValueOrCreate(XYZ.Div(boulderData.RootPos, MvSize)).Add(boulderData.RootPos);
			foreach (XYZ @case in boulderData.Volume.GetCases(VProperty.occupy))
			{
				Instance._boulders.Add(@case, boulderData);
				GameSignals.Invoke(GameSignals.BouldersChanged, @case);
				result = true;
			}
		}
		resultBoulders.Dispose();
		return result;
	}

	public static bool RemoveOrUngrow(List<XYZ> positions, float ungrowth)
	{
		bool result = false;
		HashSet<BoulderData> resultBoulders = CollectionPool.RequestHashSet<BoulderData>();
		GetBoulders(positions, ref resultBoulders);
		foreach (BoulderData item in resultBoulders)
		{
			item.Growth -= ungrowth;
			if (item.Growth == 0f)
			{
				HashSet<XYZ> valueOrDefault = Instance._mvRootPositions.GetValueOrDefault(XYZ.Div(item.RootPos, MvSize));
				if (valueOrDefault != null && valueOrDefault.Remove(item.RootPos) && valueOrDefault.Count == 0)
				{
					Instance._mvRootPositions.Remove(XYZ.Div(item.RootPos, MvSize));
				}
				item.Unregister();
				foreach (XYZ @case in item.Volume.GetCases(VProperty.occupy))
				{
					Instance._boulders.Remove(@case);
					GameSignals.Invoke(GameSignals.BouldersChanged, @case);
				}
				item.Dispose();
			}
			else
			{
				GameSignals.Invoke(GameSignals.BouldersChanged, item.RootPos);
			}
			result = true;
		}
		resultBoulders.Dispose();
		return result;
	}

	public static void GrowAndClusterize(float intensity, Vector3 offset, Vector3 centerPos, BoulderSize size)
	{
		AddOrGrow(centerPos, intensity, offset, size);
		List<XYZ> positions = PreviewBoulderSpace(centerPos, size, offset);
		Clusterize(positions, centerPos + offset, intensity);
	}

	public static void UngrowAndClusterize(List<XYZ> positions, float intensity, Vector3 clusterCenter)
	{
		RemoveOrUngrow(positions, intensity);
		Clusterize(positions, clusterCenter, intensity);
	}

	public static void Clusterize(List<XYZ> positions, Vector3 center, float distance)
	{
		HashSet<BoulderData> resultBoulders = CollectionPool.RequestHashSet<BoulderData>();
		GetBoulders(positions, ref resultBoulders);
		foreach (BoulderData item in resultBoulders)
		{
			Vector3 vector = center - item.CenterPos;
			Vector3 vector2 = vector.normalized * distance;
			if (vector.sqrMagnitude < vector2.sqrMagnitude)
			{
				vector2 = vector;
			}
			item.Offset += vector2;
		}
	}

	public static void TranslateAllBoulders(XYZ delta)
	{
		List<BoulderData> list = Instance._boulders.Values.Slinq().Distinct().ToPooledList();
		foreach (BoulderData item in list)
		{
			Remove(item.RootPos);
		}
		foreach (BoulderData item2 in list)
		{
			item2.Translate(delta.ToVector3());
			Add(item2.CenterPos, item2.Growth, item2.Offset, item2.BoulderSize);
		}
		list.Dispose();
	}

	public static BoulderSerializedData[] GetSerializedBoulders()
	{
		BoulderSerializedData[] array = new BoulderSerializedData[Instance._boulders.Values.Slinq().Distinct().Count()];
		int num = 0;
		foreach (BoulderData item in Instance._boulders.Values.Slinq().Distinct().NoLeak())
		{
			BoulderSerializedData boulderSerializedData = new BoulderSerializedData
			{
				positions = new XYZ[(int)item.BoulderSize],
				displayPos = item.CenterPos,
				offset = item.Offset,
				growth = item.Growth,
				variation = item.Variation
			};
			array[num] = boulderSerializedData;
			num++;
		}
		return array;
	}

	public static void BuildBouldersFromSerialized(BoulderSerializedData[] serializedBoulders)
	{
		Instance._boulders.Clear();
		Instance._mvRootPositions.Clear();
		for (int i = 0; i < serializedBoulders.Length; i++)
		{
			BoulderSerializedData boulderSerializedData = serializedBoulders[i];
			Add(boulderSerializedData.displayPos, boulderSerializedData.growth, boulderSerializedData.offset, (BoulderSize)boulderSerializedData.positions.Length);
		}
	}

	public static Slinq<XYZ, PredicateContext<XYZ, SelectSlinqContext<XYZ, HashSetSlinqContext<XYZ>, XYZ, IEnumerableContext<XYZ>, Dictionary<XYZ, HashSet<XYZ>>>, int>> BoulderRootPositionsInRange(XYZ center, XYZ extents)
	{
		XYZ min = XYZ.Div(center - extents, MvSize);
		XYZ max = XYZ.Div(center + extents + MvSize - XYZ.one, MvSize);
		return Calcule.VolumeBetween(min, max).Slinq().SelectMany((XYZ q, Dictionary<XYZ, HashSet<XYZ>> mvs) => Instance._mvRootPositions.GetValueOrDefault(q, Empty.HashSet<XYZ>()).Slinq(), Instance._mvRootPositions)
			.Where((XYZ p, int d2) => XYZ.SqrMagnitude(p - center) <= d2, XYZ.SqrMagnitude(extents));
	}

	private static int SizeToVariation(BoulderSize size)
	{
		return size switch
		{
			BoulderSize.Big => 2, 
			BoulderSize.Medium => 1, 
			_ => 0, 
		};
	}

	private static List<XYZ> PreviewBoulderSpace(Vector3 center, BoulderSize size, Vector3 offset)
	{
		XYZ position = XYZ.FromVector3(center + new Vector3(Mathf.Abs(offset.x), offset.y, Mathf.Abs(offset.z)));
		FreeVolume freeVolume = size switch
		{
			BoulderSize.Big => FreeVolume.Make(NatureVolume.BigMineral, position, 0), 
			BoulderSize.Medium => FreeVolume.Make(NatureVolume.MediumMineral, position, 0), 
			_ => FreeVolume.Make(NatureVolume.SmallMineral, position, 0), 
		};
		List<XYZ> result = freeVolume.GetCases(VProperty.occupy).Slinq().ToList();
		freeVolume.Dispose();
		return result;
	}

	public static void DamageAt(XYZ p, float amount)
	{
		if (amount <= 0f)
		{
			return;
		}
		BoulderData valueOrDefault = Instance._boulders.GetValueOrDefault(p);
		if (valueOrDefault == null)
		{
			return;
		}
		DamagedBoulderData valueOrCreate = Instance._damagedBoulders.GetValueOrCreate(valueOrDefault);
		float damage = valueOrCreate.Damage;
		float maxHp = GetMaxHp(p);
		valueOrCreate.Damage = damage + amount;
		if (damage <= 0f && valueOrCreate.Damage < maxHp)
		{
			ShowDamagedBoulder(valueOrDefault, valueOrCreate);
		}
		else if (damage > 0f && valueOrCreate.Damage <= 0f)
		{
			HideDamagedBoulder(valueOrDefault, valueOrCreate);
		}
		else if (!Mathf.Approximately(damage, valueOrCreate.Damage))
		{
			UpdateDamagedBoulder(valueOrDefault, valueOrCreate);
		}
		if (!(valueOrCreate.GameObject != null))
		{
			return;
		}
		Brasseur brasseur = valueOrCreate.GameObject.AddComponent<Brasseur>();
		brasseur.translation = 0.001f * amount / (float)valueOrDefault.BoulderSize;
		Neo.Relais(delegate
		{
			if (brasseur != null)
			{
				Object.Destroy(brasseur);
			}
		}, 0.1f, "boulder brasseur");
	}

	public static void Break(BoulderData boulder)
	{
		if (boulder != null)
		{
			DamagedBoulderData valueOrCreate = Instance._damagedBoulders.GetValueOrCreate(boulder);
			if (valueOrCreate != null)
			{
				HideDamagedBoulder(boulder, valueOrCreate);
			}
			DeblocCrust(boulder);
			Remove(boulder.RootPos);
		}
	}

	public static void BreakAt(XYZ p)
	{
		BoulderData valueOrDefault = Instance._boulders.GetValueOrDefault(p);
		Break(valueOrDefault);
	}

	public static void Harvest(XYZ p)
	{
		BoulderData valueOrDefault = Instance._boulders.GetValueOrDefault(p);
		if (valueOrDefault != null)
		{
			Garnotte.Nouvelle(valueOrDefault.RootPos.ToVector3() + new Vector3(0f, 0.5f, 0f), 4, RawResources.RawStone);
			valueOrDefault.CurrentRessources -= 4;
			if (valueOrDefault.CurrentRessources <= 0)
			{
				Break(valueOrDefault);
			}
		}
	}

	public static float GetDamage(XYZ p)
	{
		BoulderData valueOrDefault = Instance._boulders.GetValueOrDefault(p);
		if (valueOrDefault == null)
		{
			return 0f;
		}
		return Instance._damagedBoulders.GetValueOrDefault(valueOrDefault)?.Damage ?? 0f;
	}

	public static float GetMaxHp(XYZ p)
	{
		return GetMaxHp(Instance._boulders.GetValueOrDefault(p));
	}

	public static float GetMaxHp(BoulderData boulder)
	{
		if (boulder == null)
		{
			return 0f;
		}
		return boulder.BoulderSize switch
		{
			BoulderSize.Big => Instance.BoulderLargeDamageReceiver.MaxHP, 
			BoulderSize.Medium => Instance.BoulderMediumDamageReceiver.MaxHP, 
			BoulderSize.Small => Instance.BoulderSmallDamageReceiver.MaxHP, 
			_ => 0f, 
		};
	}

	public static float GetCurrentHp(XYZ p)
	{
		return GetMaxHp(p) - GetDamage(p);
	}

	private void Update()
	{
		RegenerateBoulders(Time.deltaTime * 5f);
	}

	private static void RegenerateBoulders(float healAmount)
	{
		if (healAmount <= 0f)
		{
			return;
		}
		Dictionary<BoulderData, DamagedBoulderData> dictionary = CollectionPool.RequestDictionary<BoulderData, DamagedBoulderData>();
		foreach (KeyValuePair<BoulderData, DamagedBoulderData> damagedBoulder in Instance._damagedBoulders)
		{
			float damage = damagedBoulder.Value.Damage;
			if (damage <= healAmount)
			{
				HideDamagedBoulder(damagedBoulder.Key, damagedBoulder.Value);
			}
			else if (!damagedBoulder.Value.GameObject.IsNullOrReleased())
			{
				dictionary[damagedBoulder.Key] = damagedBoulder.Value;
				dictionary[damagedBoulder.Key].Damage = damage - healAmount;
				UpdateDamagedBoulder(damagedBoulder.Key, damagedBoulder.Value);
			}
		}
		CollectionPool.Release(ref Instance._damagedBoulders);
		Instance._damagedBoulders = dictionary;
	}

	private static void ShowDamagedBoulder(BoulderData boulder, DamagedBoulderData damaged)
	{
		GameSignals.Invoke(GameSignals.BouldersChanged, boulder.RootPos);
		CreateDamagedBoulderGameObject(boulder, damaged);
	}

	private static void HideDamagedBoulder(BoulderData boulder, DamagedBoulderData damaged)
	{
		DestroyDamagedBoulderGameObject(damaged);
		GameSignals.Invoke(GameSignals.BouldersChanged, boulder.RootPos);
	}

	private static void UpdateDamagedBoulder(BoulderData boulder, DamagedBoulderData damaged)
	{
		Material sharedMaterial = damaged.GameObject.GetComponent<MeshRenderer>().sharedMaterial;
		sharedMaterial.SetFloat("_Threshold", damaged.Damage / GetMaxHp(boulder));
	}

	private static void CreateDamagedBoulderGameObject(BoulderData boulder, DamagedBoulderData damaged)
	{
		GameObject gameObject = new GameObject("Damaged Boulder " + boulder.RootPos, typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider));
		gameObject.transform.position = boulder.RootPos.ToVector3();
		gameObject.isStatic = true;
		gameObject.layer = 17;
		Neo.ClasseMoi("MegaVoxels/DamagedBoulders", gameObject);
		Texture2D resultTex = null;
		HashSet<BoulderData> hashSet = new HashSet<BoulderData>();
		hashSet.Add(boulder);
		HashSet<BoulderData> mineralsData = hashSet;
		MegaVoxel.Common.boulderDrawer.GetWarpTex(ref resultTex, mineralsData, boulder.RootPos);
		Mesh combinedMesh = MegaVoxel.Common.boulderDrawer.GetCombinedMesh(mineralsData, 0, resultTex.width);
		Material material = new Material(MegaVoxel.Common.boulderMaterials[0]);
		material.SetTexture("_WarpTex", resultTex);
		material.SetVector("_TCenter", Vector3.zero);
		material.SetFloat("_Threshold", damaged.Damage / GetMaxHp(boulder));
		gameObject.GetComponent<MeshRenderer>().sharedMaterial = material;
		gameObject.GetComponent<MeshFilter>().sharedMesh = combinedMesh;
		gameObject.GetComponent<MeshCollider>().sharedMesh = combinedMesh;
		damaged.GameObject = gameObject;
		damaged.DataMat = material;
		damaged.DataTex = resultTex;
	}

	private static void DestroyDamagedBoulderGameObject(DamagedBoulderData damaged)
	{
		if (damaged.GameObject != null)
		{
			Object.Destroy(damaged.GameObject);
		}
		if (damaged.DataMat != null)
		{
			Object.Destroy(damaged.DataMat);
		}
		if (damaged.DataTex != null)
		{
			Object.Destroy(damaged.DataTex);
		}
	}
}
