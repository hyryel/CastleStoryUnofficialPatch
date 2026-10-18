using System;
using Brix.Lua;
using MoonSharp.Interpreter;
using UnityEngine;

namespace Brix.Skybox;

public class Orbit : MonoBehaviour
{
	public Journee Journee;

	public float CurrentHeure;

	private Animator OrbitAnimator;

	public Transform TCardinal;

	public Transform TElevation;

	public Transform TDistance;

	public Transform TPitch;

	public Transform TYaw;

	public Transform TCelestialBody;

	public float Elevation;

	public AnimationCurve ElevationBias;

	[LuaDrivable("SunElevationLimits")]
	public Vector2 ElevationLimits;

	public float LimitedElevation;

	[Range(0f, 1f)]
	[LuaDrivable("CardinalAngle")]
	public float CardinalOffset;

	public float CardinalOffsetTime;

	[Range(1f, 20f)]
	[LuaDrivable("OrbitDistance")]
	public float OrbitDistance;

	[SerializeField]
	private bool halloween;

	[SerializeField]
	private bool oscillateX;

	[SerializeField]
	private bool oscillateY;

	[SerializeField]
	private float rotSpeed;

	[SerializeField]
	private Vector3 eulerRotOffset;

	private float speedMultiplier;

	[SerializeField]
	private float pitchUpperBound;

	[SerializeField]
	private float pitchLowerBound;

	[SerializeField]
	private float yawUpperBound;

	[SerializeField]
	private float yawLowerBound;

	private float _biasedElevation;

	public float BiasedElevation => _biasedElevation;

	public float CurrentTime => Journee.Heure / 24f;

	private void Start()
	{
		OrbitAnimator = base.transform.GetComponent<Animator>();
	}

	private void Update()
	{
		CurrentHeure = CurrentTime;
		Elevation = 1f - (Mathf.Cos(2f * CurrentTime * (float)Math.PI) / 2f + 0.5f);
		LimitedElevation = Mathf.Lerp(ElevationLimits.x, ElevationLimits.y, Elevation);
		_biasedElevation = ElevationBias.Evaluate(LimitedElevation);
		CardinalOffsetTime = (CurrentTime + CardinalOffset) % 1f;
		if (halloween)
		{
			if (oscillateX)
			{
				eulerRotOffset.x = 1f - (Mathf.Cos(2f * CurrentTime * (float)Math.PI * rotSpeed) / 2f + 0.5f) * pitchUpperBound;
			}
			if (oscillateY)
			{
				eulerRotOffset.y = 1f - (Mathf.Cos(2f * CurrentTime * (float)Math.PI * rotSpeed) / 2f + 0.5f) * yawUpperBound;
			}
			TCelestialBody.LookAt(Vector3.zero, Vector3.up);
			TCelestialBody.Rotate(eulerRotOffset);
		}
		OrbitAnimator.SetFloat("Cardinal", CardinalOffsetTime);
		OrbitAnimator.SetFloat("Elevation", _biasedElevation);
		OrbitAnimator.SetFloat("Distance", OrbitDistance);
	}

	[LuaDrivable("ToggleHalloween")]
	public static DynValue lua_ToggleHalloween(ScriptExecutionContext context, CallbackArguments args)
	{
		Orbit orbit = args.AsComponent<Orbit>(0, "ToggleHalloween");
		orbit.oscillateX = (orbit.halloween = args.AsBool(1, "ToggleHalloween"));
		return DynValue.Void;
	}
}
