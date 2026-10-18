using System.Collections;
using Brix.Audio;
using UnityEngine;

namespace Brix.Engine.Loop;

public class MusicLoop : MonoBehaviour
{
	public bool musiqueCommencee;

	public bool musiqueReady;

	public string heure;

	private void Start()
	{
		GameSignals.Once(GameSignals.LevelReady, OnTerrainReady);
		GameSignals.DisableUntil(GameSignals.LevelReady, this);
	}

	private void Update()
	{
		string text = "Jour";
		if (Journee.instance.Heure > 5.6f && Journee.instance.Heure < 6.6f)
		{
			text = "Aube";
		}
		if (Journee.instance.Heure > 17.5f && Journee.instance.Heure < 18.5f)
		{
			text = "Crepuscule";
		}
		if (Journee.instance.Heure >= 18.5f || Journee.instance.Heure <= 5.6f)
		{
			text = "Nuit";
		}
		if (text != heure)
		{
			heure = text;
			switch (heure)
			{
			case "Aube":
				SoundEngine.Play("Time_Dawn");
				break;
			case "Crepuscule":
				SoundEngine.Play("Time_Dusk");
				break;
			case "Nuit":
				SoundEngine.Play("Time_Night");
				break;
			case "Jour":
				SoundEngine.Play("Time_Day");
				break;
			}
		}
	}

	private void OnDestroy()
	{
		ArreterLaMusique();
	}

	private void OnTerrainReady()
	{
		SoundEngine.ApplyAmbienceVolume();
		StartCoroutine(CoroutineAttendre(600f));
	}

	private void MusiqueReady()
	{
		if (!musiqueReady)
		{
			musiqueReady = true;
			StartCoroutine(CoroutineMusique());
		}
	}

	public IEnumerator CoroutineAttendre(float secondes)
	{
		GameSignals.Once(GameSignals.BlockPlaced, delegate
		{
			MusiqueReady();
		});
		yield return new WaitForSeconds(secondes);
		MusiqueReady();
	}

	private IEnumerator CoroutineMusique()
	{
		yield return new WaitForSeconds(3f);
		while (true)
		{
			CommencerLaMusique();
			yield return new WaitForSeconds(300f);
			ArreterLaMusique();
			yield return new WaitForSeconds(180f + Random.value * 420f);
		}
	}

	public void CommencerLaMusique()
	{
		if (!musiqueCommencee)
		{
			SoundEngine.Play("Music");
			musiqueCommencee = true;
		}
	}

	public void ArreterLaMusique()
	{
		if (musiqueCommencee)
		{
			SoundEngine.Play("Music_Stop");
			musiqueCommencee = false;
		}
	}
}
