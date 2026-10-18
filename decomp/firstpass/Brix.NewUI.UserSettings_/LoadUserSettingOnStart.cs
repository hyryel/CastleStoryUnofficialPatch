using System.Collections;
using Brix.Network;
using Brix.UI.Builder.Menu;
using Brix.Utils;
using UnityEngine;

namespace Brix.NewUI.UserSettings_;

public class LoadUserSettingOnStart : MonoBehaviour
{
	private void Start()
	{
		InitUserSettings();
		UNetManager.instance.Disconnect();
		StartCoroutine(InitLanguageSettings());
	}

	private void InitUserSettings()
	{
		UserSettings settings = UserSettingsReader.Read();
		settings = UserSettingManager.OverwriteWithPreGameSettings(settings);
		UserSettingsReader.Save(settings);
		UserSettingManager.SetSettingIntoGame(settings);
	}

	private IEnumerator InitLanguageSettings()
	{
		while (OkDialogContainerPopulator.ConnectedDialog == null)
		{
			yield return null;
		}
		I2Helper.ShowErrorModalOnFirstInitFailed();
	}
}
