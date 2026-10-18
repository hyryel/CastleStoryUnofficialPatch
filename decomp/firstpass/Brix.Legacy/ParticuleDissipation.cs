using Brix.Lifecycle.Pooling;
using Smooth.Slinq;
using UnityEngine;

namespace Brix.Legacy;

public class ParticuleDissipation : MonoBehaviour
{
	public ParticleSystem[] liste;

	public float dureeMin = 5f;

	private float naissance;

	private void Start()
	{
		liste = GetComponentsInChildren<ParticleSystem>();
		naissance = Time.time;
	}

	private void Update()
	{
		if (Time.time > naissance + dureeMin)
		{
			CheckDissipation();
		}
	}

	private void CheckDissipation()
	{
		if (liste.Slinq().All((ParticleSystem p) => p.particleCount == 0))
		{
			ObjectPoolSingleton.Release(base.gameObject);
		}
	}
}
