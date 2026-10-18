using System.Collections.Generic;
using UnityEngine;

namespace Brix.Engine;

public class TerrainDamageTracker : MonoBehaviour
{
	public static TerrainDamageTracker Instance;

	private readonly Dictionary<XYZ, float> _damageData = new Dictionary<XYZ, float>();

	private void Awake()
	{
		Instance = this;
	}

	private void OnDestroy()
	{
		Instance = null;
	}

	public bool TryGetDamageAt(XYZ position, out float dmg)
	{
		return _damageData.TryGetValue(position, out dmg);
	}

	public float GetDamageAt(XYZ position)
	{
		return _damageData.GetValueOrDefault(position);
	}

	public void SetDamageAt(XYZ position, float dmg)
	{
		_damageData[position] = dmg;
	}

	public void ClearDamageAt(XYZ position)
	{
		_damageData.Remove(position);
	}

	public bool HasDamageAt(XYZ position)
	{
		return _damageData.ContainsKey(position);
	}

	public Dictionary<XYZ, float> AllTerrainDamageData()
	{
		return _damageData;
	}
}
