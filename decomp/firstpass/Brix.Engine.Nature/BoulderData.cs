using Brix.External.Factories;
using Brix.Game.Components.Volumes;
using UnityEngine;

namespace Brix.Engine.Nature;

public class BoulderData : NatureData
{
	private IVolume _volume;

	private Vector3 _offset;

	private float _minOffset = -0.5f;

	private float _maxOffset = 0.5f;

	private static readonly Factory.AssetKey[] BoulderGoalAssets = new Factory.AssetKey[3]
	{
		Test.SmallHarvestGoal,
		Test.MediumHarvestGoal,
		Test.LargeHarvestGoal
	};

	public IVolume Volume => _volume;

	public BoulderSize BoulderSize { get; private set; }

	public int Variation { get; private set; }

	public Vector3 CenterPos { get; private set; }

	public override NatureType Type => NatureType.boulder;

	public override int MaxRessources
	{
		get
		{
			if (BoulderSize == BoulderSize.Big)
			{
				return 240;
			}
			return 90;
		}
	}

	public override int CurrentRessources { get; set; }

	public override XYZ RootPos => XYZ.FromVector3(CenterPos + Offset);

	public override Vector3 GoalPosition => CenterPos;

	public Vector3 Offset
	{
		get
		{
			return _offset;
		}
		set
		{
			_offset = new Vector3(Mathf.Clamp(value.x, _minOffset, _maxOffset), 0f, Mathf.Clamp(value.z, _minOffset, _maxOffset));
		}
	}

	public BoulderData(float growth, int variation, Vector3 centerPos, Vector3 offset, BoulderSize size)
	{
		Growth = growth;
		Variation = variation;
		CenterPos = centerPos;
		Offset = offset;
		BoulderSize = size;
		CurrentRessources = MaxRessources;
		XYZ position = XYZ.FromVector3(centerPos + new Vector3(Mathf.Abs(offset.x), offset.y, Mathf.Abs(offset.z)));
		_volume = size switch
		{
			BoulderSize.Big => FreeVolume.Make(NatureVolume.BigMineral, position, 0), 
			BoulderSize.Medium => FreeVolume.Make(NatureVolume.MediumMineral, position, 0), 
			_ => FreeVolume.Make(NatureVolume.SmallMineral, position, 0), 
		};
		_volume.Register();
	}

	public int GetMeshVariationIndex()
	{
		return Variation;
	}

	public int GetVariationIndex(XYZ position)
	{
		Random.InitState(position.GetHashCode());
		return Random.Range(0, 16);
	}

	public override Factory.AssetKey GetGoalAsset()
	{
		return BoulderGoalAssets[Variation];
	}

	public Vector3 GetDisplacement(XYZ position)
	{
		Vector3 vector = CenterPos - RootPos.ToVector3();
		return vector + Offset;
	}

	public void Unregister()
	{
		_volume.Unregister();
	}

	public void Dispose()
	{
		_volume.Dispose();
	}

	public void Translate(Vector3 delta)
	{
		CenterPos += delta;
	}
}
