using Brix.Audio;
using UnityEngine;

namespace Brix.UI.MainMenu;

public class MainMenuMusic : MonoBehaviour
{
	private bool isPlaying;

	private void Start()
	{
		isPlaying = false;
	}

	private void Update()
	{
		if (SoundEngine.EngineIsReady && !isPlaying)
		{
			isPlaying = true;
			SoundEngine.Play("Mus_Title_Menu_Start");
		}
	}
}
