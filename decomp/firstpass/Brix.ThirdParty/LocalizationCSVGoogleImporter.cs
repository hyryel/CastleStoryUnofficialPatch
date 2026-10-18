using System.Collections;
using System.Text;
using I2.Loc;
using UnityEngine;

namespace Brix.ThirdParty;

public class LocalizationCSVGoogleImporter : MonoBehaviour
{
	private const string PrefabName = "I2Languages";

	private const string MasterPath = "Info/Languages/master.csv";

	private const string EnglishPath = "Info/Languages/English/language.csv";

	private const string FrenchPath = "Info/Languages/French/language.csv";

	public LanguageSource LanguageSource;

	[ContextMenu("Test Import")]
	public void TestImportFromGoogle()
	{
		CoroutineManager.pInstance.StartCoroutine(ImportCoroutine(LanguageSource, debug: true));
	}

	private static IEnumerator ImportCoroutine(LanguageSource languageSource, bool debug)
	{
		WWW www = languageSource.Import_Google_CreateWWWcall(ForceUpdate: true);
		if (debug)
		{
			Debug.Log("Sending request: " + www.url);
		}
		yield return www;
		if (debug)
		{
			Debug.Log("Done");
		}
		if (string.IsNullOrEmpty(www.error))
		{
			string text = Encoding.UTF8.GetString(www.bytes);
			if (debug)
			{
				Debug.Log("Importing: \n" + text);
			}
			Import(text);
			Split();
		}
		else
		{
			Debug.LogError(www.error);
		}
	}

	private static void Import(string text)
	{
		LocalizationCSVSplitter.ConvertI2ToCSV("Info/Languages/master.csv", text);
	}

	private static void Split()
	{
		LocalizationCSVSplitter.Split("Info/Languages/master.csv", "Info/Languages/English/language.csv", "Info/Languages/French/language.csv");
	}
}
