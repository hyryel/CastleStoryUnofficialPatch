using Brix.Components;
using UnityEngine;

namespace Brix.Audio;

public class GameplayAudio : BrixSingleton<GameplayAudio>
{
	private Transform camTransform;

	[SerializeField]
	private float battleIntensity;

	[SerializeField]
	private float battleIntensityAlliedMultiplier = 2f;

	[SerializeField]
	private float battleIntensityHalfLife = 2f;

	[SerializeField]
	private float battleIntensityUpdatePrecision = 1f;

	[SerializeField]
	private float workIntensity;

	[SerializeField]
	private float workIntensityDistanceMinThreshold = 30f;

	[SerializeField]
	private float workIntensityDistanceScale = 30f;

	[SerializeField]
	private float workIntensityDistancePowerFactor = 1f;

	[SerializeField]
	private float workIntensityHalfLife = 2f;

	[SerializeField]
	private float workIntensityUpdatePrecision = 0.1f;

	private float exBattleIntensity;

	private float exWorkIntensity;

	public static void AddBattleIntensity(float amount, bool isAllied)
	{
		BrixSingleton<GameplayAudio>.Instance._AddBattleIntensity(amount, isAllied);
	}

	public static void AddWorkIntensity(Vector3 worldPosition, float amount)
	{
		BrixSingleton<GameplayAudio>.Instance._AddWorkIntensity(worldPosition, amount);
	}

	private void _AddBattleIntensity(float amount, bool isAllied)
	{
		battleIntensity += ((!isAllied) ? 1f : battleIntensityAlliedMultiplier) * amount;
	}

	private void _AddWorkIntensity(Vector3 worldPosition, float amount)
	{
		float num = Mathf.Max((worldPosition - camTransform.position).sqrMagnitude, workIntensityDistanceMinThreshold * workIntensityDistanceMinThreshold);
		num /= workIntensityDistanceScale * workIntensityDistanceScale;
		workIntensity += amount * Mathf.Pow(num, (0f - workIntensityDistancePowerFactor) / 2f);
	}

	protected override void Start()
	{
		base.Start();
		camTransform = Camera.main.transform;
	}

	private void Update()
	{
		battleIntensity *= Mathf.Pow(0.5f, Time.deltaTime / battleIntensityHalfLife);
		workIntensity *= Mathf.Pow(0.5f, Time.deltaTime / workIntensityHalfLife);
		if (Mathf.Abs(exBattleIntensity - battleIntensity) >= battleIntensityUpdatePrecision)
		{
			exBattleIntensity = battleIntensity;
			SoundEngine.SetParam("Battle_Intensity", battleIntensity);
		}
		if (Mathf.Abs(exWorkIntensity - workIntensity) >= workIntensityUpdatePrecision)
		{
			exWorkIntensity = workIntensity;
			SoundEngine.SetParam("Local_Work_Intensity", workIntensity);
		}
	}
}
