using System;
using System.IO;
using Brix.Components;
using Brix.Engine.FlatWorld;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.Engine.Environment.Configs;

public class NatureLodManager : BrixSingleton<NatureLodManager>
{
	public enum QualityLevels
	{
		Low,
		Medium,
		High,
		Extreme
	}

	[Serializable]
	public class VegetationTrack : LodTrack, IDistantMergeLod
	{
		[JsonProperty]
		public LodTransition Lod1 = new LodTransition(25, 5);

		[JsonProperty]
		public LodTransition Lod2 = new LodTransition(50, 15);

		[JsonProperty]
		public LodTransition Lod3 = new LodTransition(100, 25);

		[JsonProperty]
		public LodTransition Lod4 = new LodTransition(200, 25);

		[JsonProperty]
		public LodTransition Lod5 = new LodTransition(400, 25);

		[JsonProperty]
		public LodTransition Lod6 = new LodTransition(600, 25);

		[JsonProperty]
		public int DistantMergeLod = 4;

		[JsonProperty]
		public int DisappearLod = 6;

		int IDistantMergeLod.DistantMergeLod => DistantMergeLod;

		public VegetationTrack()
		{
			Init(Lod1, Lod2, Lod3, Lod4, Lod5, Lod6);
		}
	}

	[Serializable]
	public class BouldersTrack : LodTrack, IColliderLod
	{
		[JsonProperty]
		public LodTransition Lod1 = new LodTransition(125, 25);

		[JsonProperty]
		public LodTransition Lod2 = new LodTransition(225, 25);

		[JsonProperty]
		public LodTransition Lod3 = new LodTransition(300, 25);

		[JsonProperty]
		public int ColliderLod = 3;

		int IColliderLod.ColliderLod => ColliderLod;

		public BouldersTrack()
		{
			Init(Lod1, Lod2, Lod3);
		}
	}

	[Serializable]
	public class MineralsTrack : LodTrack, IColliderLod
	{
		[JsonProperty]
		public LodTransition Lod1 = new LodTransition(125, 25);

		[JsonProperty]
		public LodTransition Lod2 = new LodTransition(225, 25);

		[JsonProperty]
		public int ColliderLod = 2;

		int IColliderLod.ColliderLod => ColliderLod;

		public MineralsTrack()
		{
			Init(Lod1, Lod2);
		}
	}

	[Serializable]
	public class TreesTrack : LodTrack, IColliderLod, IDistantMergeLod
	{
		[JsonProperty]
		public LodTransition Lod1 = new LodTransition(125, 25);

		[JsonProperty]
		public LodTransition Lod2 = new LodTransition(225, 25);

		[JsonProperty]
		public LodTransition Lod3 = new LodTransition(300, 25);

		[JsonProperty]
		public int ColliderLod = 3;

		[JsonProperty]
		public int DistantMergeLod = 3;

		int IColliderLod.ColliderLod => ColliderLod;

		int IDistantMergeLod.DistantMergeLod => DistantMergeLod;

		public TreesTrack()
		{
			Init(Lod1, Lod2, Lod3);
		}
	}

	[Serializable]
	public class TerrainTrack : LodTrack
	{
		[JsonProperty]
		public LodTransition Lod1 = new LodTransition(200, 0);

		[JsonProperty]
		public LodTransition Lod2 = new LodTransition(400, 0);

		[JsonProperty]
		public LodTransition Lod3 = new LodTransition(800, 0);

		private LodRangeDriver _rangeDriver;

		public TerrainTrack()
		{
			Init(Lod1, Lod2, Lod3);
		}

		public override void Refresh()
		{
			base.Refresh();
			if (_rangeDriver == null)
			{
				_rangeDriver = UnityEngine.Object.FindObjectOfType<LodRangeDriver>();
			}
			if (_rangeDriver != null)
			{
				_rangeDriver.qualityLevel = LodRangeDriver.QualityLevel.Custom;
				RearViewMirror.CallMethod("LodRangeDriver", _rangeDriver, "SetRanges", Lod1.Start, Lod2.Start, Lod3.Start);
			}
		}
	}

	[Serializable]
	public class LodProfile
	{
		public TreesTrack Trees = new TreesTrack();

		public VegetationTrack Vegetation = new VegetationTrack();

		public BouldersTrack Boulders = new BouldersTrack();

		public MineralsTrack Minerals = new MineralsTrack();

		public TerrainTrack Terrain = new TerrainTrack();
	}

	[JsonObject(MemberSerialization.Fields)]
	public class QualityProfiles
	{
		public LodProfile Low = new LodProfile();

		public LodProfile Medium = new LodProfile();

		public LodProfile High = new LodProfile();

		public LodProfile Extreme = new LodProfile();
	}

	public LodProfile Profile = new LodProfile();

	[NonSerialized]
	public QualityLevels LoadFromQualityLevel = QualityLevels.High;

	[NonSerialized]
	public QualityLevels SaveAsQualityLevel = QualityLevels.High;

	private int exQualityLevel;

	public TreesTrack Trees => Profile.Trees;

	public VegetationTrack Plants => Profile.Vegetation;

	public VegetationTrack Vegetation => Profile.Vegetation;

	public BouldersTrack Boulders => Profile.Boulders;

	public MineralsTrack Minerals => Profile.Minerals;

	public TerrainTrack Terrain => Profile.Terrain;

	protected override void Start()
	{
		base.Start();
		GameSignals.LateConnectOnce(GameSignals.BeforeTerrainLoaded, Init);
	}

	private void Update()
	{
		if (exQualityLevel != Neo.terrainLodQuality)
		{
			exQualityLevel = Neo.terrainLodQuality;
			Init();
		}
	}

	private void Init()
	{
		LoadFromQualityLevel = (QualityLevels)Neo.terrainLodQuality;
		LoadQualityLevel();
		ApplyChanges();
	}

	public void ApplyChanges()
	{
		Vegetation.Refresh();
		Boulders.Refresh();
		Trees.Refresh();
		Terrain.Refresh();
		Minerals.Refresh();
	}

	public void LoadQualityLevel()
	{
		QualityProfiles qualityProfiles = LoadQualityProfiles();
		if (LoadFromQualityLevel == QualityLevels.Low)
		{
			Profile = qualityProfiles.Low;
		}
		else if (LoadFromQualityLevel == QualityLevels.Medium)
		{
			Profile = qualityProfiles.Medium;
		}
		else if (LoadFromQualityLevel == QualityLevels.High)
		{
			Profile = qualityProfiles.High;
		}
		else if (LoadFromQualityLevel == QualityLevels.Extreme)
		{
			Profile = qualityProfiles.Extreme;
		}
		ApplyChanges();
		SaveAsQualityLevel = LoadFromQualityLevel;
	}

	public void SaveQualityLevel()
	{
		QualityProfiles qualityProfiles = LoadQualityProfiles();
		if (SaveAsQualityLevel == QualityLevels.Low)
		{
			qualityProfiles.Low = Profile;
		}
		else if (SaveAsQualityLevel == QualityLevels.Medium)
		{
			qualityProfiles.Medium = Profile;
		}
		else if (SaveAsQualityLevel == QualityLevels.High)
		{
			qualityProfiles.High = Profile;
		}
		else if (SaveAsQualityLevel == QualityLevels.Extreme)
		{
			qualityProfiles.Extreme = Profile;
		}
		SaveQualityProfiles(qualityProfiles);
	}

	private QualityProfiles LoadQualityProfiles()
	{
		if (!File.Exists("Info/LodManagerSettings.json"))
		{
			return new QualityProfiles();
		}
		string value = File.ReadAllText("Info/LodManagerSettings.json");
		return JsonConvert.DeserializeObject<QualityProfiles>(value) ?? new QualityProfiles();
	}

	private void SaveQualityProfiles(QualityProfiles profiles)
	{
		string contents = JsonConvert.SerializeObject(profiles, Formatting.Indented);
		File.WriteAllText("Info/LodManagerSettings.json", contents);
	}
}
