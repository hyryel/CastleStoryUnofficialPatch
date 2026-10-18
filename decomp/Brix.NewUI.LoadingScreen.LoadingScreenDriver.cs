using Brix.Engine;
using Brix.Utils.Signals;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Brix.NewUI.LoadingScreen;

public class LoadingScreenDriver : MonoBehaviour
{
	[SerializeField]
	private LoadingScreen loadingScreen;

	private int maxProgress;

	private int progress;

	private void Awake()
	{
	}

	private void Start()
	{
		GameSignals.Connect(GameSignals.LoadingGame, OnLoadGame);
		GameSignals.Connect(GameSignals.SaveUpdateNeeded, OnSaveUpdateNeeded);
		GameSignals.Connect(GameSignals.MaxLoadingProgress, OnMaxLoadingProgress);
		GameSignals.Connect(GameSignals.LoadingProgress, OnLoadingProgress);
		GameSignals.Connect(GameSignals.LoadingProgressLeap, OnLoadingProgressLeap);
		GameSignals.Once(GameSignals.LevelReady, OnGameReady);
		GameSignals.Connect(GameSignals.StartError, OnStartError);
		loadingScreen.BackButtonPressed.Connect(OnBackButtonPressed);
		loadingScreen.AnimationStart.Connect(OnAnimationStart);
		loadingScreen.AnimationEnd.Connect(OnAnimationEnd);
	}

	private void OnSaveUpdateNeeded()
	{
		loadingScreen.Warning("Upgrading a large save file might take several minutes. Do you want to continue? \n\n This only happens once. \n\n  For safety a backup will be made.");
		loadingScreen.OkButtonPressed.Connect(OnSaveUpdateAccepted).DisconnectWhen(GameSignals.UserAcceptedUpdate);
		loadingScreen.CancelButtonPressed.Connect(OnSaveUpdateRefused).DisconnectWhen(GameSignals.UserAcceptedUpdate);
	}

	private void OnSaveUpdateRefused()
	{
		GameSignals.Invoke(GameSignals.UserAcceptedUpdate, -1);
	}

	private void OnSaveUpdateAccepted()
	{
		GameSignals.Invoke(GameSignals.UserAcceptedUpdate, 1);
	}

	private void OnStartError(string error)
	{
		loadingScreen.LoadingAbort(error);
	}

	private void OnBackButtonPressed()
	{
		SceneManager.LoadScene(1);
	}

	private void OnMaxLoadingProgress(int max)
	{
		progress = 0;
		maxProgress = max;
	}

	private void OnLoadingProgress()
	{
		progress++;
		float num = (float)progress / (float)maxProgress;
		loadingScreen.SetProgress(num);
	}

	private void OnLoadingProgressLeap(int progressLeap)
	{
		progress += progressLeap;
		float num = (float)progress / (float)maxProgress;
		loadingScreen.SetProgress(num);
	}

	private void OnAnimationStart()
	{
		GameSignals.Invoke(GameSignals.LoadingScreenAnimStart);
	}

	private void OnAnimationEnd()
	{
		GameSignals.Invoke(GameSignals.LoadingScreenAnimEnd);
	}

	private void OnLoadGame(GameSignals.loadingContext context)
	{
		if (context == GameSignals.loadingContext.menu)
		{
			loadingScreen.Close();
		}
		if (context == GameSignals.loadingContext.ingame)
		{
			loadingScreen.InstantClose();
			loadingScreen.Tip("##loading_tip_right_click_pie_menu");
		}
	}

	private void OnAnimationEndGameBegins()
	{
		base.gameObject.SetActive(value: false);
	}

	private void OnGameReady()
	{
		loadingScreen.Open();
		GameSignals.Once(GameSignals.LoadingScreenAnimEnd, OnAnimationEndGameBegins);
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.1.0.9782' (yours is '9.1.0.7988')
