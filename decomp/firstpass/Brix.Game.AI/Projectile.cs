using System;
using Brix.Network;
using UnityEngine;
using UnityEngine.Networking;

namespace Brix.Game.AI;

public abstract class Projectile : NetworkGameComponent
{
	public const float MaxDeviationFactor = 10f;

	public static readonly float MinDeviationExp = Mathf.Exp(-5f);

	public LayerMask LayerCollisionMask;

	public abstract ProjectileAttack GetAttack();

	public abstract Vector3 GetVelocity();

	public virtual Vector3 GetImpactVector()
	{
		return GetVelocity().normalized;
	}

	private static float GetRandomInaccuracyAngle(float inaccuracy)
	{
		return UnityEngine.Random.Range(0f - inaccuracy, inaccuracy);
	}

	private static void GetRandomInaccucaryAngles(float inaccuracy, out float a0, out float a1)
	{
		float value = UnityEngine.Random.value;
		float value2 = UnityEngine.Random.value;
		float num = Mathf.Sqrt(-2f * Mathf.Log(Mathf.Max(value, MinDeviationExp)));
		float f = (float)Math.PI * 2f * value2;
		float num2 = num * Mathf.Cos(f);
		float num3 = num * Mathf.Sin(f);
		a0 = num2 * inaccuracy;
		a1 = num3 * inaccuracy;
	}

	public static Vector3 GetInaccurateVelocity(Vector3 velocity, float inaccuracy)
	{
		if (inaccuracy > 0f)
		{
			GetRandomInaccucaryAngles(inaccuracy, out var a, out var a2);
			velocity = Quaternion.Euler(a, a2, 0f) * velocity;
		}
		return velocity;
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
