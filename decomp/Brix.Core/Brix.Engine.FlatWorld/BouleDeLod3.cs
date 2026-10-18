using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Brix.Engine.FlatWorld;

public class BouleDeLod3 : MonoBehaviour
{
	private class LodRange
	{
		private const int SqrtMaxInt = 32767;

		private const int DistanceMaxValue = 16383;

		private int _distanceMin;

		private int _distanceMax;

		private XYZ _mvSize;

		public void Init(int distanceMin, int distanceMax, XYZ mvSize)
		{
			int num = Mathf.Clamp(distanceMin, 0, 16383);
			int num2 = Mathf.Clamp(distanceMax, 0, 16383);
			_distanceMin = num * num * 4;
			_distanceMax = num2 * num2 * 4;
			_mvSize = mvSize;
		}

		public int Distance(XYZ p, XYZ mvStart)
		{
			XYZ xYZ = XYZ.Abs(p - mvStart);
			XYZ xYZ2 = XYZ.Abs(p - (mvStart + _mvSize));
			return XYZ.SqrMagnitude(xYZ + xYZ2 - _mvSize);
		}

		public int PessimisticDistance(XYZ p, XYZ mvStart)
		{
			XYZ xYZ = XYZ.Abs(p - mvStart);
			XYZ xYZ2 = XYZ.Abs(p - (mvStart + _mvSize));
			return XYZ.SqrMagnitude(xYZ + xYZ2);
		}

		public int ClosestDistance(XYZ pStart, XYZ pSize, XYZ mvStart)
		{
			XYZ xYZ = pStart + pSize;
			XYZ xYZ2 = mvStart + _mvSize;
			XYZ xYZ3 = XYZ.Abs(XYZ.MinTowardsZero(pStart - mvStart, xYZ - mvStart));
			XYZ xYZ4 = XYZ.Abs(XYZ.MinTowardsZero(pStart - xYZ2, xYZ - xYZ2));
			return XYZ.SqrMagnitude(xYZ3 + xYZ4 - _mvSize);
		}

		public int FurthestDistance(XYZ pStart, XYZ pSize, XYZ mvStart)
		{
			XYZ xYZ = pStart + pSize;
			XYZ xYZ2 = mvStart + _mvSize;
			XYZ xYZ3 = XYZ.Max(XYZ.Abs(pStart - mvStart), XYZ.Abs(xYZ - mvStart));
			XYZ xYZ4 = XYZ.Max(XYZ.Abs(pStart - xYZ2), XYZ.Abs(xYZ - xYZ2));
			return XYZ.SqrMagnitude(xYZ3 + xYZ4 - _mvSize);
		}

		public int CloserDistance(XYZ pStart, XYZ mvStart)
		{
			return ClosestDistance(pStart - _mvSize, _mvSize << 1, mvStart);
		}

		public int FurtherDistance(XYZ pStart, XYZ mvStart)
		{
			return FurthestDistance(pStart - _mvSize, _mvSize << 1, mvStart);
		}

		public bool IsTooClose(int r)
		{
			return r < _distanceMin;
		}

		public bool IsTooFar(int r)
		{
			return r > _distanceMax;
		}
	}

	public TerrainLodProvider Test;

	[Range(1f, 1600f)]
	public int R1 = 200;

	[Range(1f, 1600f)]
	public int R2 = 400;

	[Range(1f, 1600f)]
	public int R3 = 800;

	[Range(0f, 10f)]
	public float Bias = 1.5f;

	public int MaxWorkPerFrame = 300;

	public float RefreshRate = 0.1f;

	public float SlowRefreshRate = 2f;

	public bool UseSceneCam = false;

	public bool UseUnscaledTime = true;

	public XYZ Center;

	private IEnumerator _workEnumerator;

	private bool _initialized = false;

	private float nextUpdate = 0f;

	private float nextSlow = 0f;

	private LodRange[] _lodRanges;

	private float Time => UseUnscaledTime ? UnityEngine.Time.unscaledTime : UnityEngine.Time.time;

	private void Start()
	{
		Init();
	}

	private void OnEnable()
	{
		Init();
	}

	private void OnDestroy()
	{
		Clear();
	}

	public void Init(bool forced = false)
	{
		if (!_initialized || forced)
		{
			_initialized = true;
			Refresh();
			SlowRefresh();
			_workEnumerator = WorkEnumerator();
		}
	}

	private void Clear()
	{
		if (_initialized)
		{
			_initialized = false;
			_workEnumerator = null;
		}
	}

	private void Update()
	{
		if (Time >= nextUpdate)
		{
			nextUpdate = Time + RefreshRate;
			Refresh();
		}
		if (SlowRefreshRate > 0f && Time >= nextSlow)
		{
			nextSlow = Time + SlowRefreshRate;
			SlowRefresh();
		}
		if (_workEnumerator != null && !_workEnumerator.MoveNext())
		{
			_workEnumerator = null;
		}
	}

	private IEnumerator WorkEnumerator()
	{
		int worked = 0;
		int exWorked = -1;
		while (true)
		{
			if (exWorked == worked)
			{
				yield return null;
			}
			exWorked = worked;
			LodSorter lodSorter = Test.GetLodSorter();
			if (lodSorter == null)
			{
				continue;
			}
			for (int lod = 0; lod <= lodSorter.lodMax; lod++)
			{
				List<LodSorter.Child> children = lodSorter.GetLodChildren(lod);
				if (_lodRanges == null || lod >= _lodRanges.Length)
				{
					continue;
				}
				LodRange range = _lodRanges[lod];
				if (range == null)
				{
					continue;
				}
				for (int i = 0; i < children.Count; i++)
				{
					Process(children[i], range, lod);
					int num = worked + 1;
					worked = num;
					if (num % MaxWorkPerFrame == 0)
					{
						yield return null;
					}
				}
			}
		}
	}

	private void Process(LodSorter.Child child, LodRange range, int lod)
	{
		MegaVoxelGeo megaVoxel = child.MegaVoxel;
		int r = range.Distance(Center, child.start);
		if (megaVoxel.lodWanted < lod)
		{
			if (!range.IsTooClose(r))
			{
				megaVoxel.lodWanted++;
				megaVoxel.SetVisible(megaVoxel.lodWanted == lod);
			}
		}
		else if (megaVoxel.lodWanted > lod)
		{
			if (!range.IsTooFar(r))
			{
				megaVoxel.lodWanted--;
				megaVoxel.SetVisible(megaVoxel.lodWanted == lod);
			}
		}
		else if (range.IsTooClose(r))
		{
			megaVoxel.lodWanted--;
			megaVoxel.SetVisible(visible: false);
		}
		else if (range.IsTooFar(r))
		{
			megaVoxel.lodWanted++;
			megaVoxel.SetVisible(visible: false);
		}
	}

	private void Refresh()
	{
		RefreshPosition();
	}

	private void SlowRefresh()
	{
		RefreshLodRanges();
	}

	private int GetProfileThreshold(int lod)
	{
		return lod switch
		{
			0 => 0, 
			1 => R1, 
			2 => R2, 
			3 => R3, 
			_ => 5000, 
		};
	}

	private void RefreshLodRanges()
	{
		LodSorter lodSorter = Test.GetLodSorter();
		if (lodSorter == null)
		{
			return;
		}
		if (_lodRanges == null || _lodRanges.Length != lodSorter.lodMax + 1)
		{
			Array.Resize(ref _lodRanges, lodSorter.lodMax + 1);
		}
		for (int i = 0; i <= lodSorter.lodMax; i++)
		{
			if (!lodSorter.HasProfile(i))
			{
				_lodRanges[i] = null;
				continue;
			}
			XYZ profileUnitSize = lodSorter.GetProfileUnitSize(i);
			int profileThreshold = GetProfileThreshold(i);
			int distanceMax = GetProfileThreshold(i + 1) + Mathf.RoundToInt(Bias * Mathf.Sqrt(XYZ.SqrMagnitude(profileUnitSize)));
			if (_lodRanges[i] == null)
			{
				_lodRanges[i] = new LodRange();
			}
			_lodRanges[i].Init(profileThreshold, distanceMax, profileUnitSize);
		}
	}

	private void RefreshPosition()
	{
		Center = XYZ.FromVector3(UseSceneCam ? GetSceneCamPosition() : base.transform.position);
	}

	private Vector3 GetSceneCamPosition()
	{
		Transform transform = base.transform;
		return transform.position;
	}
}
