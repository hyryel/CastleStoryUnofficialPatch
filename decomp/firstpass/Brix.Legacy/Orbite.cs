using System;
using Brix.Lua;
using UnityEngine;

namespace Brix.Legacy;

public class Orbite : MonoBehaviour
{
	public enum RotationMode
	{
		OrbitTarget,
		TargetOrbit
	}

	public enum LookAtAxis
	{
		None,
		X,
		Y,
		Z
	}

	public enum Spin
	{
		None,
		X,
		Y,
		Z
	}

	public Journee Journee;

	public float Eloignement;

	[Range(-90f, 90f)]
	[LuaDrivable("ZenithAngle")]
	public float AngleElevationAuZenith;

	[Range(0f, 360f)]
	[LuaDrivable("CardinalAngle")]
	public float AngleDecalageCardinal;

	[Range(0f, 1f)]
	public float DecalagePhase;

	public bool DirectionInverse;

	public RotationMode ProcessionMode;

	public LookAtAxis lookAtAxis = LookAtAxis.Z;

	public Spin SpinAxis;

	public float SpinSpeed;

	public bool showGizmos;

	[HideInInspector]
	public float Phase;

	[HideInInspector]
	public Vector3 Position;

	private void Update()
	{
		Phase = Journee.Heure / 24f;
		float f = AngleElevationAuZenith * ((float)Math.PI / 180f);
		float f2 = (AngleDecalageCardinal + 180f) * ((float)Math.PI / 180f);
		float f3 = ((float)(DirectionInverse ? 1 : (-1)) * (Phase + DecalagePhase) - 0.25f) * (float)Math.PI * 2f;
		float num = Mathf.Cos(f3);
		float num2 = Mathf.Sin(f3) * Mathf.Sin(f);
		float num3 = Mathf.Sin(f3) * Mathf.Cos(f);
		Vector3 vector = new Vector3(Mathf.Cos(f2), 0f, Mathf.Sin(f2));
		Vector3 up = Vector3.up;
		Vector3 vector2 = new Vector3(0f - Mathf.Sin(f2), 0f, Mathf.Cos(f2));
		Vector3 vector3 = num * vector + num2 * up + num3 * vector2;
		Position = vector3 * Eloignement;
		switch (ProcessionMode)
		{
		case RotationMode.OrbitTarget:
			base.transform.position = Position;
			if (lookAtAxis != LookAtAxis.None)
			{
				if (base.transform.parent == null)
				{
					base.transform.LookAt(Vector3.zero);
				}
				else
				{
					base.transform.LookAt(base.transform.parent.position);
				}
			}
			break;
		case RotationMode.TargetOrbit:
			base.transform.LookAt(Position);
			break;
		}
		switch (lookAtAxis)
		{
		case LookAtAxis.X:
			base.transform.Rotate(0f, -90f, 0f);
			break;
		case LookAtAxis.Y:
			base.transform.Rotate(90f, 0f, 0f);
			break;
		case LookAtAxis.Z:
			base.transform.Rotate(0f, 0f, -90f);
			break;
		case LookAtAxis.None:
			switch (SpinAxis)
			{
			case Spin.X:
				base.transform.Rotate(Vector3.up, SpinSpeed);
				break;
			case Spin.Y:
				base.transform.Rotate(Vector3.forward, SpinSpeed);
				break;
			case Spin.Z:
				base.transform.Rotate(Vector3.right, SpinSpeed);
				break;
			}
			break;
		}
	}
}
