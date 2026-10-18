using System;
using UnityEngine;

namespace Brix.Utils;

public static class ProjectilePhysics
{
	public struct CollisionTrajectory
	{
		public RaycastHit hit;

		public Vector3 velocityAtImpact;

		public float traveledDistance;

		public float flightDuration;

		public bool IsNull => hit.collider == null;
	}

	public static bool showDebug = true;

	public static readonly float G = Physics.gravity.y;

	public static readonly float HalfG = G / 2f;

	public static Vector3 ComputeFiringVelocity(float power, Vector3 relativeTarget, float elevationAngle)
	{
		float num = Mathf.Cos(elevationAngle);
		float y = Mathf.Sin(elevationAngle);
		Vector3 normalized = new Vector3(relativeTarget.x, relativeTarget.z).normalized;
		return new Vector3(num * normalized.x, y, num * normalized.y) * power;
	}

	public static Vector3 ComputeFiringVelocity(float power, Vector3 relativeTarget, bool lob = false)
	{
		float elevationAngle = ComputeElevationAngle(power, relativeTarget);
		return ComputeFiringVelocity(power, relativeTarget, elevationAngle);
	}

	public static Vector3 ComputeFiringVelocity(Vector3 relativeTarget, float elevationAngle)
	{
		float power = ComputeFiringPower(elevationAngle, new Vector2(relativeTarget.x, relativeTarget.z).magnitude, relativeTarget.y);
		return ComputeFiringVelocity(power, relativeTarget, elevationAngle);
	}

	public static float ComputeFlightDuration(Vector3 initialVelocity, Vector3 relativeTarget)
	{
		Vector2 vector = new Vector2(initialVelocity.x, initialVelocity.z);
		return Mathf.Sqrt(new Vector3(relativeTarget.x, relativeTarget.y, relativeTarget.z).sqrMagnitude / vector.sqrMagnitude);
	}

	public static Vector3 ComputeNextPosition(Vector3 currentPosition, ref Vector3 currentVelocity, float deltaT)
	{
		currentVelocity.y += HalfG * deltaT;
		currentPosition += currentVelocity * deltaT;
		currentVelocity.y += HalfG * deltaT;
		return currentPosition;
	}

	public static Vector3 ComputeNextVelocity(Vector3 currentVelocity, float deltaT)
	{
		currentVelocity.y += G * deltaT;
		return currentVelocity;
	}

	public static Vector3 ComputeVelocity(Vector3 initialVelocity, Vector3 relativeTarget)
	{
		float num = ComputeFlightDuration(initialVelocity, relativeTarget);
		return (!(num > 0f)) ? initialVelocity : ComputeNextVelocity(initialVelocity, num);
	}

	public static bool IsWithinReach(float power, Vector3 relativeTarget)
	{
		float g = G;
		float sqrMagnitude = new Vector2(relativeTarget.x, relativeTarget.z).sqrMagnitude;
		float y = 0f - relativeTarget.y;
		float v = power * power;
		float num = ComputeDiscriminant(sqrMagnitude, y, v, g);
		return num >= 0f;
	}

	private static float ComputeDiscriminant(float x2, float y, float v2, float g)
	{
		return v2 * v2 - g * (g * x2 + 2f * y * v2);
	}

	public static float ComputeElevationAngle(float power, float horizontalDistance, float verticalDistance, bool lob = false)
	{
		float g = G;
		float y = 0f - verticalDistance;
		float num = power * power;
		float a = ComputeDiscriminant(horizontalDistance * horizontalDistance, y, num, g);
		return Mathf.Atan2(num + Mathf.Sqrt(Mathf.Max(a, 0f)) * (float)(lob ? 1 : (-1)), (0f - g) * horizontalDistance);
	}

	public static float ComputeElevationAngle(float power, Vector3 relativeTarget, bool lob = false)
	{
		return ComputeElevationAngle(power, new Vector2(relativeTarget.x, relativeTarget.z).magnitude, relativeTarget.y, lob);
	}

	public static Vector3 ComputeFiringVelocityWithBias(float power, Vector3 relativeTarget, Vector3 velocityBias, bool lob = false)
	{
		float num = power * 0.01f;
		float num2 = num * num;
		Vector3 vector = Vector3.zero;
		Vector3 vector2 = vector;
		float num3 = 0f;
		for (int i = 0; i < 100; i++)
		{
			Vector3 relativeTarget2 = relativeTarget + velocityBias * num3;
			vector = ComputeFiringVelocity(power, relativeTarget2, lob);
			num3 = ComputeFlightDuration(vector, relativeTarget2);
			float sqrMagnitude = (vector - vector2).sqrMagnitude;
			if (sqrMagnitude < num2)
			{
				break;
			}
			vector2 = vector;
		}
		return vector;
	}

	public static float ComputeFiringPower(float elevationAngle, float horizontalDistance, float verticalDistance)
	{
		float g = G;
		float num = Mathf.Tan(elevationAngle);
		float num2 = 0f - verticalDistance;
		float f = 0.5f * g * horizontalDistance * horizontalDistance * (num * num + 1f) / (horizontalDistance * num + num2);
		return Mathf.Sqrt(Mathf.Abs(f));
	}

	public static float ComputeFiringPower(float elevationAngle, Vector3 relativeTarget)
	{
		return ComputeFiringPower(elevationAngle, new Vector2(relativeTarget.x, relativeTarget.z).magnitude, relativeTarget.y);
	}

	public static Vector3[] ComputeTrajectory(Vector3 initialPosition, Vector3 initialVelocity, float duration, int nPoints)
	{
		if (nPoints <= 0)
		{
			return new Vector3[0];
		}
		if (nPoints == 1)
		{
			return new Vector3[1] { initialPosition };
		}
		Vector3[] array = new Vector3[nPoints];
		float deltaT = duration / (float)(nPoints - 1);
		for (int i = 0; i < nPoints; i++)
		{
			array[i] = initialPosition;
			initialPosition = ComputeNextPosition(initialPosition, ref initialVelocity, deltaT);
		}
		return array;
	}

	public static Vector3[] ComputeTrajectory(Vector3 initialPosition, Vector3 initialVelocity, Vector3 targetPosition, int nPoints)
	{
		if (nPoints <= 0)
		{
			return new Vector3[0];
		}
		if (nPoints == 1)
		{
			return new Vector3[1] { initialPosition };
		}
		Vector3[] array = new Vector3[nPoints];
		Vector3 vector = targetPosition - initialPosition;
		float num = ((!(Mathf.Abs(vector.x) > Mathf.Abs(vector.z))) ? (vector.z / initialVelocity.z) : (vector.x / initialVelocity.x));
		float y = initialPosition.y;
		float num2 = initialVelocity.y * num;
		float num3 = vector.y - num2;
		float num4 = num2;
		float num5 = y;
		float num6 = 1f / (float)(nPoints - 1);
		for (int i = 0; i < nPoints; i++)
		{
			float num7 = (float)i * num6;
			array[i].x = Mathf.Lerp(initialPosition.x, targetPosition.x, num7);
			array[i].y = (num3 * num7 + num4) * num7 + num5;
			array[i].z = Mathf.Lerp(initialPosition.z, targetPosition.z, num7);
		}
		return array;
	}

	public static CollisionTrajectory ComputeExpectedCollision(Vector3 initialPosition, Vector3 initialVelocity, float distanceMin = 0f, float distanceMax = float.PositiveInfinity, float durationMax = float.PositiveInfinity, int layerMask = -1, Func<RaycastHit, bool> filterFunc = null)
	{
		if (filterFunc == null)
		{
			return ComputeExpectedCollision(initialPosition, initialVelocity, distanceMin, distanceMax, durationMax, layerMask, null, 0);
		}
		return ComputeExpectedCollision(initialPosition, initialVelocity, distanceMin, distanceMax, durationMax, layerMask, (RaycastHit h, Func<RaycastHit, bool> f) => f(h), filterFunc);
	}

	public static CollisionTrajectory ComputeExpectedCollision<T>(Vector3 initialPosition, Vector3 initialVelocity, float distanceMin = 0f, float distanceMax = float.PositiveInfinity, float durationMax = float.PositiveInfinity, int layerMask = -1, Func<RaycastHit, T, bool> filterFunc = null, T arg = default(T))
	{
		Vector3 vector = initialPosition;
		Vector3 currentVelocity = initialVelocity;
		float num = 0f;
		float num2 = 0f;
		while (num < distanceMax && num2 <= durationMax)
		{
			float num3 = 1f / (currentVelocity.magnitude + Mathf.Abs(G));
			Vector3 vector2 = ComputeNextPosition(vector, ref currentVelocity, num3);
			Vector3 vector3 = vector2 - vector;
			float magnitude = vector3.magnitude;
			num += magnitude;
			num2 += num3;
			Vector3 direction = vector3 / magnitude;
			Ray ray = new Ray(vector, direction);
			vector = vector2;
			if (Physics.Raycast(ray, out var hitInfo, magnitude, layerMask, QueryTriggerInteraction.Ignore) && Vector3.Distance(initialPosition, hitInfo.point) > distanceMin && (filterFunc == null || filterFunc(hitInfo, arg)))
			{
				return new CollisionTrajectory
				{
					flightDuration = num2,
					hit = hitInfo,
					traveledDistance = num,
					velocityAtImpact = currentVelocity
				};
			}
		}
		return default(CollisionTrajectory);
	}

	public static void DebugTrajectory(Vector3[] trajectory, Color color)
	{
	}
}
