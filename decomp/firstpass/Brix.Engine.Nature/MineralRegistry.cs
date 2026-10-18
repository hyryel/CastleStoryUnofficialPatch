using System.Collections.Generic;
using System.Linq;
using Brix.Audio;
using Brix.External.Factories;
using Brix.Game.AI.Damage;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Input;
using Brix.Legacy;
using Brix.Utils;
using UnityEngine;

namespace Brix.Engine.Nature;

public abstract class MineralRegistry
{
	private readonly Dictionary<XYZ, MineralData> _minerals = new Dictionary<XYZ, MineralData>();

	private Dictionary<MineralData, DamagedMineralData> _damagedMinerals = new Dictionary<MineralData, DamagedMineralData>();

	private IDamageReceiver _mineralDamageReceiver;

	public const int GarnottesPerHit = 1;

	protected virtual string AddedSoundEvent => string.Empty;

	protected virtual string RemovedSoundEvent => string.Empty;

	protected IDamageReceiver MineralDamageReceiver
	{
		get
		{
			if (_mineralDamageReceiver == null)
			{
				_mineralDamageReceiver = Factory.Peek(Brix.External.Factories.Environment.MineralMesh).GetComponent<IDamageReceiver>();
			}
			return _mineralDamageReceiver;
		}
	}

	protected abstract Factory.AssetKey garnotteKey { get; }

	protected abstract MineralData CreateMineral(float growth, int variation, XYZ rootPos);

	public void AddAllMineralPositionsToList(ref List<XYZ> list)
	{
		list.AddRange(_minerals.Keys);
	}

	public bool HasMinerals(XYZ p)
	{
		return _minerals.ContainsKey(p);
	}

	public MineralData GetMineral(XYZ p)
	{
		return _minerals.GetValueOrDefault(p);
	}

	public void Add(XYZ position, float growth)
	{
		if (!NatureRegistry.HasNatureAt(position))
		{
			MineralData value = CreateMineral(growth, 0, position);
			_minerals.Add(position, value);
			GameSignals.Invoke(GameSignals.MineralsChanged, position);
			if (InputModeController.GetCurrentMode() == InputMode.worldEditor)
			{
				SoundEngine.Play(AddedSoundEvent);
			}
		}
	}

	public static void DeblocCrust(MineralData mineral)
	{
		foreach (XYZ @case in mineral.Volume.GetCases(VProperty.occupy))
		{
			using IVoxel voxel = Voxel.GetVoxel(@case);
			if (voxel.IsCrust)
			{
				voxel.DeBlockClient(-1);
			}
		}
	}

	public void Remove(XYZ position)
	{
		MineralData mineral = GetMineral(position);
		if (mineral != null)
		{
			mineral.Unregister();
			_minerals.Remove(position);
			GameSignals.Invoke(GameSignals.MineralsChanged, position);
			mineral.Dispose();
			if (InputModeController.GetCurrentMode() == InputMode.worldEditor)
			{
				SoundEngine.Play(RemovedSoundEvent);
			}
		}
	}

	public bool AddOrGrow(XYZ position, float growth)
	{
		bool result = false;
		NatureData natureData = NatureRegistry.NatureAt(position);
		if (natureData is MineralData || !natureData.IsValid)
		{
			if (natureData is MineralData { Growth: var growth2 } mineralData)
			{
				mineralData.Growth += growth;
				if (mineralData.Growth != growth2)
				{
					result = true;
					GameSignals.Invoke(GameSignals.MineralsChanged, position);
				}
			}
			else
			{
				Add(position, growth);
				result = true;
			}
		}
		return result;
	}

	public bool RemoveOrUngrow(XYZ position, float ungrowth)
	{
		bool result = false;
		MineralData mineral = GetMineral(position);
		if (mineral != null)
		{
			mineral.Growth -= ungrowth;
			if (mineral.Growth == 0f)
			{
				Remove(position);
			}
			else
			{
				GameSignals.Invoke(GameSignals.MineralsChanged, mineral.RootPos);
			}
			result = true;
		}
		return result;
	}

	public void TranslateAllMinerals(XYZ delta)
	{
		List<XYZ> list = _minerals.Keys.ToList();
		Dictionary<XYZ, MineralData> dictionary = _minerals.ToDictionary((KeyValuePair<XYZ, MineralData> kv) => kv.Key + delta, (KeyValuePair<XYZ, MineralData> kv) => kv.Value);
		foreach (XYZ item in list)
		{
			Remove(item);
			GameSignals.Invoke(GameSignals.MineralsChanged, item);
		}
		foreach (KeyValuePair<XYZ, MineralData> item2 in dictionary)
		{
			item2.Value.Translate(delta.ToVector3());
			Add(item2.Key, item2.Value.Growth);
			GameSignals.Invoke(GameSignals.MineralsChanged, item2.Key);
		}
	}

	public MineralSerializedData[] GetSerializedMinerals()
	{
		MineralSerializedData[] array = new MineralSerializedData[_minerals.Count];
		int num = 0;
		foreach (KeyValuePair<XYZ, MineralData> mineral in _minerals)
		{
			MineralSerializedData mineralSerializedData = default(MineralSerializedData);
			MineralData value = mineral.Value;
			mineralSerializedData.position = value.RootPos;
			mineralSerializedData.growth = value.Growth;
			mineralSerializedData.variation = value.Variation;
			array[num] = mineralSerializedData;
			num++;
		}
		return array;
	}

	public void BuildMineralsFromSerialized(MineralSerializedData[] serializedMinerals)
	{
		_minerals.Clear();
		for (int i = 0; i < serializedMinerals.Length; i++)
		{
			MineralSerializedData mineralSerializedData = serializedMinerals[i];
			MineralData value = CreateMineral(mineralSerializedData.growth, mineralSerializedData.variation, mineralSerializedData.position);
			_minerals.Add(mineralSerializedData.position, value);
			GameSignals.Invoke(GameSignals.MineralsChanged, mineralSerializedData.position);
		}
	}

	public float GetMaxHp(XYZ p)
	{
		return GetMaxHp(_minerals.GetValueOrDefault(p));
	}

	private float GetMaxHp(MineralData boulder)
	{
		if (boulder == null)
		{
			return 0f;
		}
		return MineralDamageReceiver.MaxHP;
	}

	public float GetCurrentHp(XYZ p)
	{
		return GetMaxHp(p) - GetDamage(p);
	}

	public void DamageAt(XYZ p, float amount)
	{
		if (amount <= 0f)
		{
			return;
		}
		MineralData valueOrDefault = _minerals.GetValueOrDefault(p);
		if (valueOrDefault == null)
		{
			return;
		}
		float maxHp = GetMaxHp(p);
		DamagedMineralData valueOrCreate = _damagedMinerals.GetValueOrCreate(valueOrDefault);
		float damage = valueOrCreate.Damage;
		valueOrCreate.Damage = damage + amount;
		if (damage <= 0f && valueOrCreate.Damage < maxHp)
		{
			ShowDamagedMineral(valueOrDefault, valueOrCreate);
		}
		else if (damage > 0f && valueOrCreate.Damage >= maxHp)
		{
			HideDamagedMineral(valueOrDefault, valueOrCreate);
			Break(valueOrDefault);
		}
		else if (!Mathf.Approximately(damage, valueOrCreate.Damage))
		{
			UpdateDamagedMineral(valueOrDefault, valueOrCreate);
		}
		if (!(valueOrCreate.GameObject != null))
		{
			return;
		}
		Brasseur brasseur = valueOrCreate.GameObject.AddComponent<Brasseur>();
		brasseur.translation = 0.001f * amount;
		Neo.Relais(delegate
		{
			if (brasseur != null)
			{
				Object.Destroy(brasseur);
			}
		}, 0.1f, "mineral brasseur");
	}

	public void Break(MineralData mineral)
	{
		if (mineral != null)
		{
			DeblocCrust(mineral);
			Remove(mineral.RootPos);
		}
	}

	public void BreakAt(XYZ p)
	{
		MineralData valueOrDefault = _minerals.GetValueOrDefault(p);
		Break(valueOrDefault);
	}

	public void Harvest(XYZ p)
	{
		MineralData mineral = GetMineral(p);
		if (mineral != null)
		{
			Garnotte.Nouvelle(mineral.RootPos.ToVector3() + new Vector3(0f, 0.5f, 0f), 1, garnotteKey);
			mineral.CurrentRessources--;
			if (mineral.CurrentRessources <= 0)
			{
				Break(mineral);
			}
		}
	}

	public float GetDamage(XYZ p)
	{
		MineralData valueOrDefault = _minerals.GetValueOrDefault(p);
		if (valueOrDefault == null)
		{
			return 0f;
		}
		return _damagedMinerals.GetValueOrDefault(valueOrDefault)?.Damage ?? 0f;
	}

	public void RegenerateMinerals(float healAmount)
	{
		if (healAmount <= 0f)
		{
			return;
		}
		Dictionary<MineralData, DamagedMineralData> dictionary = CollectionPool.RequestDictionary<MineralData, DamagedMineralData>();
		foreach (KeyValuePair<MineralData, DamagedMineralData> damagedMineral in _damagedMinerals)
		{
			float damage = damagedMineral.Value.Damage;
			healAmount /= GetMaxHp(damagedMineral.Key.RootPos);
			if (damage <= healAmount)
			{
				HideDamagedMineral(damagedMineral.Key, damagedMineral.Value);
			}
			else if (!damagedMineral.Value.GameObject.IsNullOrReleased())
			{
				dictionary[damagedMineral.Key] = damagedMineral.Value;
				dictionary[damagedMineral.Key].Damage = damage - healAmount;
				UpdateDamagedMineral(damagedMineral.Key, damagedMineral.Value);
			}
		}
		CollectionPool.Release(ref _damagedMinerals);
		_damagedMinerals = dictionary;
	}

	private void ShowDamagedMineral(MineralData mineral, DamagedMineralData damaged)
	{
		GameSignals.Invoke(GameSignals.MineralsChanged, mineral.RootPos);
		CreateDamagedMineralGameObject(mineral, damaged);
	}

	private void HideDamagedMineral(MineralData mineral, DamagedMineralData damaged)
	{
		DestroyDamagedMineralGameObject(damaged);
		GameSignals.Invoke(GameSignals.MineralsChanged, mineral.RootPos);
	}

	private void UpdateDamagedMineral(MineralData mineral, DamagedMineralData damaged)
	{
		Material sharedMaterial = damaged.GameObject.GetComponent<MeshRenderer>().sharedMaterial;
		sharedMaterial.SetFloat("_Threshold", damaged.Damage / GetMaxHp(mineral));
	}

	protected abstract void GetWarpTex(ref Texture2D resultTex, List<XYZ> positions, XYZ mvOffset);

	protected abstract Mesh GetCombinedMesh(int count, int maxCountSoFar, int variation, int lod, int texWidth);

	protected abstract Material GetMaterial();

	private void CreateDamagedMineralGameObject(MineralData mineral, DamagedMineralData damaged)
	{
		GameObject gameObject = new GameObject("Damaged Mineral " + mineral.RootPos, typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider));
		gameObject.transform.position = mineral.RootPos.ToVector3();
		gameObject.isStatic = true;
		gameObject.layer = 17;
		Neo.ClasseMoi("MegaVoxels/DamagedMinerals", gameObject);
		Texture2D resultTex = null;
		List<XYZ> list = new List<XYZ>();
		list.Add(mineral.RootPos);
		List<XYZ> list2 = list;
		GetWarpTex(ref resultTex, list2, mineral.RootPos);
		Mesh combinedMesh = GetCombinedMesh(list2.Count, list2.Count, 0, 0, resultTex.width);
		Material material = GetMaterial();
		material.SetTexture("_WarpTex", resultTex);
		material.SetVector("_TCenter", Vector3.zero);
		material.SetFloat("_Threshold", damaged.Damage / GetMaxHp(mineral));
		gameObject.GetComponent<MeshRenderer>().sharedMaterial = material;
		gameObject.GetComponent<MeshFilter>().sharedMesh = combinedMesh;
		gameObject.GetComponent<MeshCollider>().sharedMesh = combinedMesh;
		damaged.GameObject = gameObject;
		damaged.DataMat = material;
		damaged.DataTex = resultTex;
	}

	private void DestroyDamagedMineralGameObject(DamagedMineralData damaged)
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
