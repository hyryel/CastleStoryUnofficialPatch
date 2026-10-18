using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using ArabicSupport;
using UnityEngine;

namespace I2.Loc;

public static class LocalizationManager
{
	public delegate void OnLocalizeCallback();

	private static string mCurrentLanguage;

	private static string mLanguageCode;

	private static bool mChangeCultureInfo = false;

	public static bool IsRight2Left = false;

	public static List<LanguageSource> Sources = new List<LanguageSource>();

	public static string[] GlobalSources = new string[1] { "I2Languages" };

	public static List<ILocalizationParamsManager> ParamManagers = new List<ILocalizationParamsManager>();

	private static string[] LanguagesRTL = new string[20]
	{
		"ar-DZ", "ar", "ar-BH", "ar-EG", "ar-IQ", "ar-JO", "ar-KW", "ar-LB", "ar-LY", "ar-MA",
		"ar-OM", "ar-QA", "ar-SA", "ar-SY", "ar-TN", "ar-AE", "ar-YE", "he", "ur", "ji"
	};

	public static string CurrentLanguage
	{
		get
		{
			InitializeIfNeeded();
			return mCurrentLanguage;
		}
		set
		{
			string supportedLanguage = GetSupportedLanguage(value);
			if (!string.IsNullOrEmpty(supportedLanguage) && mCurrentLanguage != supportedLanguage)
			{
				SetLanguageAndCode(supportedLanguage, GetLanguageCode(supportedLanguage));
			}
		}
	}

	public static string CurrentLanguageCode
	{
		get
		{
			InitializeIfNeeded();
			return mLanguageCode;
		}
		set
		{
			if (mLanguageCode != value)
			{
				string languageFromCode = GetLanguageFromCode(value);
				if (!string.IsNullOrEmpty(languageFromCode))
				{
					SetLanguageAndCode(languageFromCode, value);
				}
			}
		}
	}

	public static string CurrentRegion
	{
		get
		{
			string currentLanguage = CurrentLanguage;
			int num = currentLanguage.IndexOfAny("/\\".ToCharArray());
			if (num > 0)
			{
				return currentLanguage.Substring(num + 1);
			}
			num = currentLanguage.IndexOfAny("[(".ToCharArray());
			int num2 = currentLanguage.LastIndexOfAny("])".ToCharArray());
			if (num > 0 && num != num2)
			{
				return currentLanguage.Substring(num + 1, num2 - num - 1);
			}
			return string.Empty;
		}
		set
		{
			string text = CurrentLanguage;
			int num = text.IndexOfAny("/\\".ToCharArray());
			if (num > 0)
			{
				CurrentLanguage = text.Substring(num + 1) + value;
				return;
			}
			num = text.IndexOfAny("[(".ToCharArray());
			int num2 = text.LastIndexOfAny("])".ToCharArray());
			if (num > 0 && num != num2)
			{
				text = text.Substring(num);
			}
			CurrentLanguage = text + "(" + value + ")";
		}
	}

	public static string CurrentRegionCode
	{
		get
		{
			string currentLanguageCode = CurrentLanguageCode;
			int num = currentLanguageCode.IndexOfAny(" -_/\\".ToCharArray());
			return (num >= 0) ? currentLanguageCode.Substring(num + 1) : string.Empty;
		}
		set
		{
			string text = CurrentLanguageCode;
			int num = text.IndexOfAny(" -_/\\".ToCharArray());
			if (num > 0)
			{
				text = text.Substring(0, num);
			}
			CurrentLanguageCode = text + "-" + value;
		}
	}

	public static event OnLocalizeCallback OnLocalizeEvent;

	private static void InitializeIfNeeded()
	{
		if (string.IsNullOrEmpty(mCurrentLanguage))
		{
			UpdateSources();
			SelectStartupLanguage();
		}
	}

	public static void SetLanguageAndCode(string LanguageName, string LanguageCode, bool RememberLanguage = true, bool Force = false)
	{
		if (mCurrentLanguage != LanguageName || mLanguageCode != LanguageCode || Force)
		{
			if (RememberLanguage)
			{
				PlayerPrefs.SetString("I2 Language", LanguageName);
			}
			mCurrentLanguage = LanguageName;
			mLanguageCode = LanguageCode;
			if (mChangeCultureInfo)
			{
				SetCurrentCultureInfo();
			}
			else
			{
				IsRight2Left = IsRTL(mLanguageCode);
			}
			LocalizeAll(Force);
		}
	}

	private static CultureInfo GetCulture(string code)
	{
		try
		{
			return CultureInfo.CreateSpecificCulture(code);
		}
		catch (Exception)
		{
			return CultureInfo.InvariantCulture;
		}
	}

	public static void EnableChangingCultureInfo(bool bEnable)
	{
		if (!mChangeCultureInfo && bEnable)
		{
			SetCurrentCultureInfo();
		}
		mChangeCultureInfo = bEnable;
	}

	private static void SetCurrentCultureInfo()
	{
		Thread.CurrentThread.CurrentCulture = GetCulture(mLanguageCode);
		IsRight2Left = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft;
	}

	private static void SelectStartupLanguage()
	{
		string text = PlayerPrefs.GetString("I2 Language", string.Empty);
		string text2 = Application.systemLanguage.ToString();
		if (text2 == "ChineseSimplified")
		{
			text2 = "Chinese (Simplified)";
		}
		if (text2 == "ChineseTraditional")
		{
			text2 = "Chinese (Traditional)";
		}
		if (HasLanguage(text, AllowDiscartingRegion: true, Initialize: false))
		{
			CurrentLanguage = text;
			return;
		}
		string supportedLanguage = GetSupportedLanguage(text2);
		if (!string.IsNullOrEmpty(supportedLanguage))
		{
			SetLanguageAndCode(supportedLanguage, GetLanguageCode(supportedLanguage), RememberLanguage: false);
			return;
		}
		int i = 0;
		for (int count = Sources.Count; i < count; i++)
		{
			if (Sources[i].mLanguages.Count > 0)
			{
				SetLanguageAndCode(Sources[i].mLanguages[0].Name, Sources[i].mLanguages[0].Code, RememberLanguage: false);
				break;
			}
		}
	}

	public static string GetTermTranslation(string Term)
	{
		return GetTermTranslation(Term, IsRight2Left, 0, ignoreRTLnumbers: true);
	}

	public static string GetTermTranslation(string Term, bool FixForRTL)
	{
		return GetTermTranslation(Term, FixForRTL, 0, ignoreRTLnumbers: true);
	}

	public static string GetTermTranslation(string Term, bool FixForRTL, int maxLineLengthForRTL)
	{
		return GetTermTranslation(Term, FixForRTL, maxLineLengthForRTL, ignoreRTLnumbers: true);
	}

	public static string GetTermTranslation(string Term, bool FixForRTL, int maxLineLengthForRTL, bool ignoreRTLnumbers)
	{
		if (TryGetTermTranslation(Term, out var Translation, FixForRTL, maxLineLengthForRTL, ignoreRTLnumbers))
		{
			return Translation;
		}
		return string.Empty;
	}

	public static bool TryGetTermTranslation(string Term, out string Translation)
	{
		return TryGetTermTranslation(Term, out Translation, FixForRTL: false, 0, ignoreRTLnumbers: true);
	}

	public static bool TryGetTermTranslation(string Term, out string Translation, bool FixForRTL)
	{
		return TryGetTermTranslation(Term, out Translation, FixForRTL, 0, ignoreRTLnumbers: true);
	}

	public static bool TryGetTermTranslation(string Term, out string Translation, bool FixForRTL, int maxLineLengthForRTL, bool ignoreRTLnumbers)
	{
		Translation = string.Empty;
		if (string.IsNullOrEmpty(Term))
		{
			return false;
		}
		InitializeIfNeeded();
		int i = 0;
		for (int count = Sources.Count; i < count; i++)
		{
			if (Sources[i].TryGetTermTranslation(Term, out Translation))
			{
				if (IsRight2Left && FixForRTL)
				{
					Translation = ApplyRTLfix(Translation, maxLineLengthForRTL, ignoreRTLnumbers);
				}
				return true;
			}
		}
		return false;
	}

	public static string ApplyRTLfix(string line)
	{
		return ApplyRTLfix(line, 0, ignoreNumbers: true);
	}

	public static string ApplyRTLfix(string line, int maxCharacters, bool ignoreNumbers)
	{
		bool flag = true;
		bool flag2 = true;
		MatchCollection matchCollection = null;
		if (flag2 || ignoreNumbers)
		{
			Regex regex = new Regex((!ignoreNumbers) ? "<ignoreRTL>(?<val>.*)<\\/ignoreRTL>" : "<ignoreRTL>(?<val>.*)<\\/ignoreRTL>|(?<val>\\d+)");
			matchCollection = regex.Matches(line);
			line = regex.Replace(line, "¬");
		}
		MatchCollection matchCollection2 = null;
		if (flag)
		{
			Regex regex2 = new Regex("(?></?\\w+)(?>(?:[^>'\"]+|'[^']*'|\"[^\"]*\")*)>|\\[.*?\\]");
			matchCollection2 = regex2.Matches(line);
			line = regex2.Replace(line, "¶");
		}
		if (maxCharacters <= 0)
		{
			line = ArabicFixer.Fix(line);
		}
		else
		{
			Regex regex3 = new Regex(".{0," + maxCharacters + "}(\\s+|$)", RegexOptions.Multiline);
			line = line.Replace("\r\n", "\n");
			line = regex3.Replace(line, "$0\n");
			line = line.Replace("\n\n", "\n");
			string[] array = line.Split('\n');
			int i = 0;
			for (int num = array.Length; i < num; i++)
			{
				array[i] = ArabicFixer.Fix(array[i]);
			}
			line = string.Join("\n", array);
		}
		if (flag && matchCollection != null)
		{
			int count = matchCollection.Count;
			int startIndex = 0;
			for (int num2 = count - 1; num2 >= 0; num2--)
			{
				startIndex = line.IndexOf('¬', startIndex);
				line = line.Remove(startIndex, 1).Insert(startIndex, matchCollection[num2].Groups["val"].Value);
			}
		}
		if (flag && matchCollection2 != null)
		{
			int count2 = matchCollection2.Count;
			int startIndex2 = 0;
			for (int j = 0; j < count2; j++)
			{
				startIndex2 = line.IndexOf('¶', startIndex2);
				line = line.Remove(startIndex2, 1).Insert(startIndex2, matchCollection2[j].Value);
			}
		}
		return line;
	}

	public static string FixRTL_IfNeeded(string text, int maxCharacters = 0, bool ignoreNumber = false)
	{
		if (IsRight2Left)
		{
			return ApplyRTLfix(text, maxCharacters, ignoreNumber);
		}
		return text;
	}

	internal static void LocalizeAll(bool Force = false)
	{
		Localize[] array = (Localize[])Resources.FindObjectsOfTypeAll(typeof(Localize));
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			Localize localize = array[i];
			localize.OnLocalize(Force);
		}
		if (LocalizationManager.OnLocalizeEvent != null)
		{
			LocalizationManager.OnLocalizeEvent();
		}
		ResourceManager.pInstance.CleanResourceCache();
	}

	internal static void ApplyLocalizationParams(ref string translation, Localize localizeCmp)
	{
		Regex regex = new Regex("{\\[(.*?)\\]}");
		MatchCollection matchCollection = regex.Matches(translation);
		int i = 0;
		for (int count = matchCollection.Count; i < count; i++)
		{
			Match match = matchCollection[i];
			string value = match.Groups[match.Groups.Count - 1].Value;
			string localizationParam = GetLocalizationParam(value, localizeCmp);
			if (localizationParam != null)
			{
				translation = translation.Replace(match.Value, localizationParam);
			}
		}
	}

	internal static string GetLocalizationParam(string ParamName, Localize localizeCmp)
	{
		string text = null;
		if ((bool)localizeCmp)
		{
			MonoBehaviour[] components = localizeCmp.GetComponents<MonoBehaviour>();
			int i = 0;
			for (int num = components.Length; i < num; i++)
			{
				if (components[i] is ILocalizationParamsManager localizationParamsManager)
				{
					text = localizationParamsManager.GetParameterValue(ParamName);
					if (text != null)
					{
						return text;
					}
				}
			}
		}
		int j = 0;
		for (int count = ParamManagers.Count; j < count; j++)
		{
			text = ParamManagers[j].GetParameterValue(ParamName);
			if (text != null)
			{
				return text;
			}
		}
		return null;
	}

	public static bool UpdateSources()
	{
		UnregisterDeletededSources();
		RegisterSourceInResources();
		RegisterSceneSources();
		return Sources.Count > 0;
	}

	private static void UnregisterDeletededSources()
	{
		for (int num = Sources.Count - 1; num >= 0; num--)
		{
			if (Sources[num] == null)
			{
				RemoveSource(Sources[num]);
			}
		}
	}

	private static void RegisterSceneSources()
	{
		LanguageSource[] array = (LanguageSource[])Resources.FindObjectsOfTypeAll(typeof(LanguageSource));
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			if (!Sources.Contains(array[i]))
			{
				AddSource(array[i]);
			}
		}
	}

	private static void RegisterSourceInResources()
	{
		string[] globalSources = GlobalSources;
		foreach (string name in globalSources)
		{
			GameObject asset = ResourceManager.pInstance.GetAsset<GameObject>(name);
			LanguageSource languageSource = ((!asset) ? null : asset.GetComponent<LanguageSource>());
			if ((bool)languageSource && !Sources.Contains(languageSource))
			{
				AddSource(languageSource);
			}
		}
	}

	internal static void AddSource(LanguageSource Source)
	{
		if (!Sources.Contains(Source))
		{
			Sources.Add(Source);
			Source.Import_Google_FromCache();
			if (Source.GoogleUpdateDelay > 0f)
			{
				Source.Invoke("Delayed_Import_Google", Source.GoogleUpdateDelay);
			}
			else
			{
				Source.Import_Google();
			}
			if (Source.mDictionary.Count == 0)
			{
				Source.UpdateDictionary(force: true);
			}
		}
	}

	internal static void RemoveSource(LanguageSource Source)
	{
		Sources.Remove(Source);
	}

	public static bool IsGlobalSource(string SourceName)
	{
		return Array.IndexOf(GlobalSources, SourceName) >= 0;
	}

	public static bool HasLanguage(string Language, bool AllowDiscartingRegion = true, bool Initialize = true)
	{
		if (Initialize)
		{
			InitializeIfNeeded();
		}
		int i = 0;
		for (int count = Sources.Count; i < count; i++)
		{
			if (Sources[i].GetLanguageIndex(Language, AllowDiscartingRegion: false) >= 0)
			{
				return true;
			}
		}
		if (AllowDiscartingRegion)
		{
			int j = 0;
			for (int count2 = Sources.Count; j < count2; j++)
			{
				if (Sources[j].GetLanguageIndex(Language) >= 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static string GetSupportedLanguage(string Language)
	{
		int i = 0;
		for (int count = Sources.Count; i < count; i++)
		{
			int languageIndex = Sources[i].GetLanguageIndex(Language, AllowDiscartingRegion: false);
			if (languageIndex >= 0)
			{
				return Sources[i].mLanguages[languageIndex].Name;
			}
		}
		int j = 0;
		for (int count2 = Sources.Count; j < count2; j++)
		{
			int languageIndex2 = Sources[j].GetLanguageIndex(Language);
			if (languageIndex2 >= 0)
			{
				return Sources[j].mLanguages[languageIndex2].Name;
			}
		}
		return string.Empty;
	}

	public static string GetLanguageCode(string Language)
	{
		int i = 0;
		for (int count = Sources.Count; i < count; i++)
		{
			int languageIndex = Sources[i].GetLanguageIndex(Language);
			if (languageIndex >= 0)
			{
				return Sources[i].mLanguages[languageIndex].Code;
			}
		}
		return string.Empty;
	}

	public static string GetLanguageFromCode(string Code)
	{
		int i = 0;
		for (int count = Sources.Count; i < count; i++)
		{
			int languageIndexFromCode = Sources[i].GetLanguageIndexFromCode(Code);
			if (languageIndexFromCode >= 0)
			{
				return Sources[i].mLanguages[languageIndexFromCode].Name;
			}
		}
		return string.Empty;
	}

	public static List<string> GetAllLanguages()
	{
		List<string> list = new List<string>();
		int i = 0;
		for (int count = Sources.Count; i < count; i++)
		{
			int j = 0;
			for (int count2 = Sources[i].mLanguages.Count; j < count2; j++)
			{
				if (!list.Contains(Sources[i].mLanguages[j].Name))
				{
					list.Add(Sources[i].mLanguages[j].Name);
				}
			}
		}
		return list;
	}

	public static List<string> GetCategories()
	{
		List<string> list = new List<string>();
		int i = 0;
		for (int count = Sources.Count; i < count; i++)
		{
			Sources[i].GetCategories(OnlyMainCategory: false, list);
		}
		return list;
	}

	public static List<string> GetTermsList()
	{
		if (Sources.Count == 0)
		{
			UpdateSources();
		}
		if (Sources.Count == 1)
		{
			return Sources[0].GetTermsList();
		}
		HashSet<string> hashSet = new HashSet<string>();
		int i = 0;
		for (int count = Sources.Count; i < count; i++)
		{
			hashSet.UnionWith(Sources[i].GetTermsList());
		}
		return new List<string>(hashSet);
	}

	public static TermData GetTermData(string term)
	{
		int i = 0;
		for (int count = Sources.Count; i < count; i++)
		{
			TermData termData = Sources[i].GetTermData(term);
			if (termData != null)
			{
				return termData;
			}
		}
		return null;
	}

	public static LanguageSource GetSourceContaining(string term, bool fallbackToFirst = true)
	{
		if (!string.IsNullOrEmpty(term))
		{
			int i = 0;
			for (int count = Sources.Count; i < count; i++)
			{
				if (Sources[i].GetTermData(term) != null)
				{
					return Sources[i];
				}
			}
		}
		return (!fallbackToFirst || Sources.Count <= 0) ? null : Sources[0];
	}

	public static UnityEngine.Object FindAsset(string value)
	{
		int i = 0;
		for (int count = Sources.Count; i < count; i++)
		{
			UnityEngine.Object obj = Sources[i].FindAsset(value);
			if ((bool)obj)
			{
				return obj;
			}
		}
		return null;
	}

	public static string GetVersion()
	{
		return "2.6.9 f1";
	}

	public static int GetRequiredWebServiceVersion()
	{
		return 4;
	}

	public static string GetWebServiceURL(LanguageSource source = null)
	{
		if (source != null && !string.IsNullOrEmpty(source.Google_WebServiceURL))
		{
			return source.Google_WebServiceURL;
		}
		for (int i = 0; i < Sources.Count; i++)
		{
			if (Sources[i] != null && !string.IsNullOrEmpty(Sources[i].Google_WebServiceURL))
			{
				return Sources[i].Google_WebServiceURL;
			}
		}
		return string.Empty;
	}

	private static bool IsRTL(string Code)
	{
		return Array.IndexOf(LanguagesRTL, Code) >= 0;
	}
}
