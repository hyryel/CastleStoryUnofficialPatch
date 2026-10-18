using System;
using System.Collections.Generic;
using Brix.Engine;
using Brix.Game.AI.Damage;
using Brix.Game.Rules;
using Brix.Util;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.Components;

public class ExplosionGameplay : GameComponent
{
	[Serializable]
	public class FalloffCurve
	{
		public float minRange = 1f;

		public float maxRange = 5f;

		public AnimationCurve falloff = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

		public float intensityFactor = 600f;

		public float Evaluate(float r)
		{
			if (r > maxRange)
			{
				return 0f;
			}
			if (r < minRange)
			{
				return intensityFactor;
			}
			float time = (r - minRange) / (maxRange - minRange);
			return falloff.Evaluate(time) * intensityFactor;
		}
	}

	private struct DamageInfo
	{
		public float Amount;

		public Vector3 Thrust;
	}

	public FalloffCurve unitImpulseCurve;

	public FalloffCurve objectImpulseCurve;

	public FalloffCurve unitDamageCurve;

	public FalloffCurve terrainDamageCurve;

	public FalloffCurve brickDamageCurve;

	public FalloffCurve woodDamageCurve;

	public float blockAnnihilationRange;

	private Dictionary<GameObject, DamageInfo> damageQueue = new Dictionary<GameObject, DamageInfo>();

	private ExplosionAttack _attack;

	private static Collider[] hitsUnit = new Collider[1000];

	private static Collider[] hitsObject = new Collider[1000];

	protected override void Start()
	{
		base.Start();
		if (NetworkServer.active)
		{
			_attack = new ExplosionAttack
			{
				Explosion = this
			};
			Explosion();
		}
	}

	private void Explosion()
	{
		damageQueue.Clear();
		ApplyTerrainDammage(base.transform.position, base.voxel.position);
		ApplyUnitDammage(base.transform.position);
		Neo.Relais(delegate
		{
			ApplyObjectExplosionForce(base.transform.position);
		}, 0.05f, "ApplyExplosionForce");
	}

	private void ApplyTerrainDammage(Vector3 explosionCenter, XYZ voxelCenter)
	{
		float num = Mathf.Max(terrainDamageCurve.maxRange, brickDamageCurve.maxRange, woodDamageCurve.maxRange);
		XYZ xYZ = XYZ.one * Mathf.RoundToInt(num);
		foreach (XYZ item in Calcule.VolumeBetween(-xYZ, xYZ))
		{
			if ((float)XYZ.SqrMagnitude(item) > num * num)
			{
				continue;
			}
			XYZ position = voxelCenter - item;
			using IVoxel voxel = Voxel.GetVoxel(position);
			if (voxel.IsNull)
			{
				continue;
			}
			Vector3 vector = position.ToVector3();
			using IDamageReceiver receiver = voxel.DamageReceiver;
			if (voxel.IsTerrain)
			{
				_attack.DamageValue = (int)GetIntensity(vector, explosionCenter, terrainDamageCurve);
			}
			else if (voxel.BlockInfo.Ressource == ResourceType.WoodenBlock)
			{
				_attack.DamageValue = (int)GetIntensity(vector, explosionCenter, woodDamageCurve);
			}
			else
			{
				_attack.DamageValue = (int)GetIntensity(vector, explosionCenter, brickDamageCurve);
			}
			if (_attack.DamageValue > 0 && TerrainRules.Main.CanExplodeAt(vector))
			{
				if ((position.ToVector3() - explosionCenter).magnitude < blockAnnihilationRange)
				{
					_attack.ApplyAsAbsolute(receiver);
				}
				else
				{
					_attack.Apply(receiver);
				}
			}
		}
	}

	private void ApplyUnitDammage(Vector3 explosionCenter)
	{
		float maxRange = unitImpulseCurve.maxRange;
		int layerMask = 69632;
		int num = Physics.OverlapSphereNonAlloc(base.voxel.position.ToVector3(), maxRange, hitsUnit, layerMask);
		for (int i = 0; i < num; i++)
		{
			float intensity = GetIntensity(hitsUnit[i].transform.position, explosionCenter, unitDamageCurve);
			Vector3 thrust = ComputeUnitThrust(hitsUnit[i].transform);
			SetDamage(SignificantParent.Of(hitsUnit[i]), intensity, thrust);
		}
		Array.Clear(hitsUnit, 0, num);
		ApplyDamage();
	}

	private void ApplyObjectExplosionForce(Vector3 explosionCenter)
	{
		float maxRange = objectImpulseCurve.maxRange;
		int layerMask = 24576;
		int num = Physics.OverlapSphereNonAlloc(base.voxel.position.ToVector3(), maxRange, hitsObject, layerMask);
		for (int i = 0; i < num; i++)
		{
			if (!(hitsObject[i].GetComponent<Rigidbody>() == null) && !hitsObject[i].GetComponent<Rigidbody>().isKinematic)
			{
				float intensity = GetIntensity(hitsObject[i].transform.position, explosionCenter, objectImpulseCurve);
				hitsObject[i].GetComponent<Rigidbody>().AddExplosionForce(intensity, base.voxel.position.ToVector3(), intensity * 3f, 2f, ForceMode.Impulse);
			}
		}
		Array.Clear(hitsObject, 0, num);
	}

	private static GameObject GetUnit(object o)
	{
		if (o is GameObject)
		{
			return (GameObject)o;
		}
		if (o is Component)
		{
			return ((Component)o).gameObject;
		}
		return null;
	}

	private float GetIntensity(Vector3 position, Vector3 explosionCenter, FalloffCurve curve)
	{
		return curve.Evaluate((position - explosionCenter).magnitude);
	}

	private void SetDamage(UnityEngine.Object o, float qte, Vector3 thrust)
	{
		GameObject unit = GetUnit(o);
		if (!unit)
		{
			return;
		}
		if (damageQueue.TryGetValue(unit, out var value))
		{
			if (qte > value.Amount)
			{
				value.Amount = qte;
				value.Thrust = thrust;
				damageQueue[unit] = value;
			}
		}
		else
		{
			value = new DamageInfo
			{
				Amount = qte,
				Thrust = thrust
			};
			damageQueue.Add(unit, value);
		}
	}

	private void ApplyDamage()
	{
		foreach (KeyValuePair<GameObject, DamageInfo> item in damageQueue)
		{
			GameObject key = item.Key;
			DamageInfo value = item.Value;
			int num = Mathf.RoundToInt(value.Amount);
			if (num > 0)
			{
				DamageReceiver component = key.GetComponent<DamageReceiver>();
				if (!component.IsNullOrReleased())
				{
					DoDamage(component, num, value.Thrust);
				}
			}
		}
	}

	private void DoDamage(DamageReceiver receiver, int qte, Vector3 thrust)
	{
		_attack.DamageValue = qte;
		_attack.Thrust = thrust;
		_attack.Apply(receiver);
	}

	private Vector3 ComputeUnitThrust(Transform receiver)
	{
		Vector3 normalized = (receiver.position - base.transform.position).normalized;
		float r = Vector3.Distance(receiver.position, base.transform.position);
		return normalized * unitImpulseCurve.Evaluate(r);
	}
}
