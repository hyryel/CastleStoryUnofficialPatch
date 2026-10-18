using System.Collections.Generic;
using Brix.Engine.Volumes;
using Brix.Game.Batchers;
using Brix.Game.Components;
using Brix.Game.Components.Volumes;
using Brix.Legacy;
using Brix.Lua;
using Brix.Network;
using Brix.Util;
using Brix.Utils;
using Brix.Utils.Collections;
using Brix.Utils.Signals;
using Brix.Utils.Unity;
using Smooth.Slinq;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Engine;

public class Mergeback : NetworkGameComponent
{
	public struct GarnotteData
	{
		public Garnotte garnotte;

		public SphereCollection.Sphere sphere;

		public float spawnTime;

		public bool ready;

		public int iGroup;
	}

	public class GarnotteRegion : UnionRegion<GarnotteData>
	{
	}

	private struct TerrainAddData
	{
		public readonly XYZ Top;

		public readonly int Amount;

		public readonly VoxelTerrainType Type;

		public TerrainAddData(XYZ top, VoxelTerrainType type, int amount)
		{
			Top = top;
			Amount = amount;
			Type = type;
		}
	}

	public float groupDistance = 1.5f;

	public float mergeDelay = 30f;

	public float mergeValue = 1f;

	public float mergeDelayRandomness = 2f;

	public float mergeDelayIfMoved = 5f;

	public float updateDelay = 0.5f;

	public float sinkingDelay = 0.5f;

	public float maxAcceptedSpeed = 0.3f;

	[LuaDrivable("allowMergeback")]
	public bool allowMergeback = true;

	private SphereCollection sc = new SphereCollection();

	private readonly List<GarnotteRegion> _garnottes = new List<GarnotteRegion>();

	private readonly List<GarnotteRegion> _merging = new List<GarnotteRegion>();

	private readonly Queue<GarnotteRegion> _discarded = new Queue<GarnotteRegion>();

	[SerializeField]
	[DisplayRules(DisplayFlags.ReadOnly)]
	private float mergingTime;

	private const float garnotteSize = 0.5f;

	private float lastUpdate;

	private bool MergebackEnabled => allowMergeback;

	[ServerCallback]
	protected override void Start()
	{
		if (NetworkServer.active)
		{
			base.Start();
			Garnotte.Enabled.Connect(delegate(Garnotte g, Mergeback self)
			{
				self.OnGarnotteEnabled(g);
			}, this).DisconnectWhen(base.OnReleaseOrDestroySignal);
			Garnotte.Disabled.Connect(delegate(Garnotte g, Mergeback self)
			{
				self.OnGarnotteDisabled(g);
			}, this).DisconnectWhen(base.OnReleaseOrDestroySignal);
		}
	}

	private void OnGarnotteEnabled(Garnotte g)
	{
		if (g.CanMergeback)
		{
			SphereCollection.Sphere sphere = sc.Add(g.transform.position, 0.5f);
			float spawnTime = Time.time - Random.value * mergeDelayRandomness;
			_garnottes.Add(new GarnotteRegion
			{
				Value = 
				{
					garnotte = g,
					sphere = sphere,
					spawnTime = spawnTime
				}
			});
		}
	}

	private void OnGarnotteDisabled(Garnotte g)
	{
		if (g.CanMergeback)
		{
			_garnottes.RemoveAll((GarnotteRegion data) => data.Value.garnotte == g);
		}
	}

	[ServerCallback]
	private void Update()
	{
		if (!NetworkServer.active || !MergebackEnabled)
		{
			return;
		}
		UpdateDiscarding();
		if (!UpdateMerging())
		{
			float time = Time.time;
			if (!(time < lastUpdate + updateDelay))
			{
				lastUpdate = time;
				UpdateGrouping();
				SpreadCollisions();
				UpdateSinking();
			}
		}
	}

	private void UpdateDiscarding()
	{
		if (_discarded.Count != 0)
		{
			GarnotteRegion garnotteRegion = _discarded.Dequeue();
			garnotteRegion.Value.garnotte.gameObject.SafeNetworkDestroy();
		}
	}

	private bool UpdateMerging()
	{
		if (_merging.Count > 0)
		{
			if (mergingTime <= 0f)
			{
				mergingTime = Time.time + sinkingDelay;
			}
			else if (mergingTime <= Time.time)
			{
				mergingTime = 0f;
				_merging.Clear();
			}
			return true;
		}
		return false;
	}

	private void UpdateGrouping()
	{
		float time = Time.time;
		float num = Calcule.Sqr(maxAcceptedSpeed * updateDelay);
		foreach (GarnotteRegion garnotte in _garnottes)
		{
			Vector3 center = garnotte.Value.sphere.center;
			Vector3 position = garnotte.Value.garnotte.transform.position;
			if ((center - position).sqrMagnitude >= num)
			{
				garnotte.Value.spawnTime = Mathf.Max(garnotte.Value.spawnTime, time - mergeDelay + mergeDelayIfMoved);
			}
			garnotte.Value.ready = time >= garnotte.Value.spawnTime + mergeDelay;
			garnotte.Value.sphere.radius = groupDistance;
			garnotte.Value.sphere.center = position;
			garnotte.UnionParent = garnotte;
		}
	}

	private void UpdateSinking()
	{
		foreach (GarnotteRegion garnotte in _garnottes)
		{
			if (!garnotte.Value.ready)
			{
				garnotte.TopParent.Value.ready = false;
			}
		}
		if (!_garnottes.Slinq().Any((GarnotteRegion g) => g == g.TopParent && g.Value.ready))
		{
			return;
		}
		int num = 0;
		int count = _garnottes.Count;
		for (int num2 = 0; num2 < count; num2++)
		{
			_garnottes[num2].Value.iGroup = 0;
		}
		for (int num3 = 0; num3 < count; num3++)
		{
			GarnotteRegion garnotteRegion = _garnottes[num3];
			UnionRegion<GarnotteData> topParent = garnotteRegion.TopParent;
			if (topParent.Value.ready)
			{
				if (topParent.Value.iGroup == 0)
				{
					if (HasTerrainOnBottom(XYZ.FromVector3(topParent.Value.sphere.center)))
					{
						topParent.Value.iGroup = 1;
					}
					else
					{
						topParent.Value.iGroup = 2;
					}
				}
				if (topParent.Value.iGroup == 1)
				{
					_merging.Add(garnotteRegion);
					garnotteRegion.Value.garnotte.SinkIfPossible();
				}
				else
				{
					_discarded.Enqueue(garnotteRegion);
				}
			}
			else
			{
				_garnottes[num] = garnotteRegion;
				num++;
			}
		}
		if (num < count)
		{
			_garnottes.RemoveRange(num, count - num);
		}
	}

	private void SpreadCollisions()
	{
		for (int i = 0; i < _garnottes.Count; i++)
		{
			GarnotteRegion garnotteRegion = _garnottes[i];
			if (!garnotteRegion.Value.ready)
			{
				continue;
			}
			for (int j = 0; j < i; j++)
			{
				GarnotteRegion garnotteRegion2 = _garnottes[j];
				if (garnotteRegion2.Value.ready && garnotteRegion.TopParent != garnotteRegion2.TopParent && sc.Collides(garnotteRegion.Value.sphere, garnotteRegion2.Value.sphere))
				{
					garnotteRegion.UnionWith(garnotteRegion2);
				}
			}
		}
	}

	private void MergeInTerrain(List<GarnotteRegion> group)
	{
		Vector3 v = group.Slinq().Aggregate(Vector3.zero, (Vector3 current, GarnotteRegion g) => current + g.Value.sphere.center) / group.Count;
		List<XYZ> list = CollectionPool.RequestList<XYZ>();
		FindTerrainTops(XYZ.FromVector3(v), list);
		if (list.Count > 0)
		{
			MergeInTerrain(group, list);
		}
		list.Dispose();
	}

	private void MergeInTerrain(List<GarnotteRegion> group, List<XYZ> tops)
	{
		Dictionary<VoxelTerrainType, List<GarnotteRegion>> dictionary = group.Slinq().ToPooledLookup((GarnotteRegion g) => g.Value.garnotte.terrainType);
		List<Vector3> list = tops.Slinq().Select((XYZ p, Mergeback self) => new Vector3(p.x, (float)p.y + self.GetFullness(p), p.z), this).ToPooledList();
		List<TerrainAddData> list2 = CollectionPool.RequestList<TerrainAddData>();
		foreach (KeyValuePair<VoxelTerrainType, List<GarnotteRegion>> item in dictionary)
		{
			VoxelTerrainType key = item.Key;
			List<GarnotteRegion> value = item.Value;
			List<Vector3> list3 = (from g in value.Slinq()
				select g.Value.sphere.center).ToPooledList();
			List<float> list4 = list.Slinq().Select(GetGaussianContribution, list3).ToPooledList();
			int totalAmount = Mathf.RoundToInt((float)list3.Count * mergeValue);
			GetTerrainAddData(tops, list4, totalAmount, key, list2);
			list3.Dispose();
			list4.Dispose();
			value.Dispose();
		}
		AddToTerrainTop(list2);
		dictionary.Dispose();
		list.Dispose();
		list2.Dispose();
	}

	private void FindTerrainTops(XYZ center, List<XYZ> results)
	{
		VoxelCylinder voxelCylinder = new VoxelCylinder();
		voxelCylinder.center = center;
		voxelCylinder.extents = new XYZ(3, 0, 3);
		voxelCylinder.orientation = VoxelCylinder.Orientation.Up;
		VoxelCylinder voxelCylinder2 = voxelCylinder;
		(from p in voxelCylinder2.Voxels.Slinq().Select((XYZ p, Mergeback self) => new XYZ(p.x, self.FindTerrainTop(p), p.z), this)
			where p.y != 0 && HasNothingOnTop(p)
			select p).AddTo(results);
	}

	private void GetTerrainAddData(List<XYZ> tops, List<float> weights, int totalAmount, VoxelTerrainType terrainType, List<TerrainAddData> outData)
	{
		float num = weights.Slinq().Sum();
		for (int i = 0; i < tops.Count; i++)
		{
			int num2 = Mathf.RoundToInt(weights[i] * (float)totalAmount / num);
			totalAmount -= num2;
			num -= weights[i];
			if (num2 != 0)
			{
				outData.Add(new TerrainAddData(tops[i], terrainType, num2));
			}
		}
	}

	private static void AddToTerrainTop(IList<TerrainAddData> data)
	{
		int count = data.Count;
		int[] array = new int[count];
		int[] array2 = new int[count];
		int[] array3 = new int[count];
		int[] array4 = new int[count];
		int[] array5 = new int[count];
		for (int i = 0; i < count; i++)
		{
			TerrainAddData terrainAddData = data[i];
			int num = i;
			XYZ top = terrainAddData.Top;
			array[num] = top.x;
			int num2 = i;
			XYZ top2 = terrainAddData.Top;
			array2[num2] = top2.y;
			int num3 = i;
			XYZ top3 = terrainAddData.Top;
			array3[num3] = top3.z;
			array4[i] = (int)terrainAddData.Type;
			array5[i] = terrainAddData.Amount;
		}
		VoxelRequestBatcher.Instance.CallRpcAddTerrainResources_Multi(array, array2, array3, array4, array5, compacted: false, pushOut: true);
	}

	private int FindTerrainTop(XYZ p, int maxDelta = 2)
	{
		if (IsEmpty(p))
		{
			int num = Mathf.Max(1, p.y - maxDelta);
			p.y--;
			while (p.y >= num)
			{
				if (!IsEmpty(p))
				{
					return p.y;
				}
				p.y--;
			}
		}
		else
		{
			int num2 = Mathf.Min(255, p.y + maxDelta + 1);
			p.y++;
			while (p.y <= num2)
			{
				if (IsEmpty(p))
				{
					return p.y - 1;
				}
				p.y++;
			}
		}
		return 0;
	}

	private bool HasNothingOnTop(XYZ p)
	{
		if (VolumeRegistry.HasVolumesWithPropertyAt(p + XYZ.up, VProperty.occupy))
		{
			return false;
		}
		if (TreeRegistry.Instance.IsNearATree(p))
		{
			return false;
		}
		return true;
	}

	private bool HasTerrainOnBottom(XYZ p)
	{
		return Voxel.IsTerrain(p) || Voxel.IsTerrain(p - XYZ.up);
	}

	private int GetVolume(XYZ p)
	{
		return Voxel.Engine.GetVoxelVolume(Voxel.Engine.GetVoxelHandle(p));
	}

	private float GetFullness(XYZ p)
	{
		int num = GetVolume(p);
		return (num <= 1) ? 0f : ((num < 4) ? ((float)(num - 1) / 3f) : 1f);
	}

	private bool IsEmpty(XYZ p)
	{
		return GetVolume(p) == 0;
	}

	private static float GetGaussianContribution(Vector3 center, IList<Vector3> points)
	{
		float num = 0f;
		int count = points.Count;
		for (int i = 0; i < count; i++)
		{
			num += Mathf.Exp((points[i] - center).sqrMagnitude * -0.49999997f);
		}
		return num;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		bool flag2 = default(bool);
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
	}
}
