using System.Collections;
using Brix.Legacy;
using UnityEngine;

namespace Brix.Engine.Loop;

public class PhysicsLoop : MonoBehaviour
{
	private void Start()
	{
		GameSignals.Once(GameSignals.TerrainReady, OnCollidersAbsent);
	}

	private void OnCollidersAbsent()
	{
		StartCoroutine(WaitForPhysicsReady());
	}

	private IEnumerator WaitForPhysicsReady()
	{
		while (!Architecte.AuRepos())
		{
			yield return new WaitForSeconds(1f);
		}
		OnCollidersReady();
	}

	private void OnCollidersReady()
	{
		GameSignals.Invoke(GameSignals.PhysicsReady);
		BlocLibre[] array = Object.FindObjectsOfType<BlocLibre>();
		foreach (BlocLibre blocLibre in array)
		{
			if (!(blocLibre == null))
			{
				blocLibre.ReactiverPhysics();
			}
		}
		Roche[] array2 = Object.FindObjectsOfType<Roche>();
		foreach (Roche roche in array2)
		{
			if (!(roche == null))
			{
				roche.ReactiverPhysics();
			}
		}
	}
}
