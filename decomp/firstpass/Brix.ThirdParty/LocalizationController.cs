using System.Collections.Generic;
using Brix.Assets;
using I2.Loc;
using UnityEngine;

namespace Brix.ThirdParty;

public class LocalizationController : MonoBehaviour
{
	public string language;

	public bool Refresh;

	private void Init()
	{
		UpdateLanguages();
	}

	public static void UpdateLanguages()
	{
		List<Asset_Language> list = AssetDispensary.languages.SendQuery(AssetQueryParameters.AllSources | AssetQueryParameters.RefreshCache, AssetPlayableContext.Any);
		if (LocalizationManager.CurrentLanguage == string.Empty)
		{
		}
		LanguageSource languageSource = LocalizationManager.Sources[0];
		foreach (Asset_Language item in list)
		{
			languageSource.Import_CSV(null, item.Disk_ReadCSV(), eSpreadsheetUpdateMode.Merge);
		}
	}

	private void Start()
	{
		Init();
	}

	private void Update()
	{
	}

	private void OnLevelWasLoaded(int level)
	{
		Init();
	}
}
