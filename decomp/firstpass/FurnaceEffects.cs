using Brix.Audio;
using UnityEngine;

public class FurnaceEffects : MonoBehaviour
{
	[SerializeField]
	private Animator animator;

	[SerializeField]
	private ParticleSystem particles;

	private bool burning;

	private float burnUntil;

	public void BurnForSeconds(int seconds)
	{
		burnUntil = Time.time + (float)seconds;
	}

	private void StartBurning()
	{
		burning = true;
		animator.SetBool("Active", value: true);
		ParticleSystem.EmissionModule emission = particles.emission;
		emission.enabled = true;
		particles.gameObject.SetActive(value: true);
		SoundEngine.Play("Furnace_InProgress_Start", base.gameObject);
	}

	private void StopBurning()
	{
		burning = false;
		animator.SetBool("Active", value: false);
		ParticleSystem.EmissionModule emission = particles.emission;
		emission.enabled = false;
		SoundEngine.Play("Furnace_InProgress_Stop", base.gameObject);
	}

	private void Update()
	{
		if (Time.time < burnUntil && !burning)
		{
			StartBurning();
		}
		else if (Time.time > burnUntil && burning)
		{
			StopBurning();
		}
	}
}
