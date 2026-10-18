using System.Collections;
using Brix.Game.Components;
using UnityEngine;

namespace Brix.Legacy;

public class AutoDelete : GameComponent
{
	private float delais = 20f;

	private bool waitForChildren = true;

	protected override void Start()
	{
		base.Start();
		StartCoroutine(Depop(delais + GetRandom()));
	}

	private IEnumerator Depop(float attente)
	{
		yield return new WaitForSeconds(attente);
		while (waitForChildren && base.transform.childCount != 0)
		{
			yield return new WaitForSeconds(1f);
		}
		Object.Destroy(base.gameObject);
	}

	public float GetRandom()
	{
		return (0.5f + Random.value) * delais;
	}
}
