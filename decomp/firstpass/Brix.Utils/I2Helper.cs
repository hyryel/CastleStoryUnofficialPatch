using System;
using System.Collections.Generic;
using System.IO;
using Brix.Engine;
using Brix.NewUI.UserSettings_;
using I2.Loc;
using UnityEngine;

namespace Brix.Utils;

public static class I2Helper
{
	public static class DateTimeUtility
	{
		public static class WeekDay
		{
			public static string Key = "#weekday";

			public static Dictionary<string, string> Map = new Dictionary<string, string>
			{
				{ "Sunday", "##datetime_weekday_sunday" },
				{ "Monday", "##datetime_weekday_monday" },
				{ "Tuesday", "##datetime_weekday_tuesday" },
				{ "Wednesday", "##datetime_weekday_wednesday" },
				{ "Thursday", "##datetime_weekday_thursday" },
				{ "Friday", "##datetime_weekday_friday" },
				{ "Saturday", "##datetime_weekday_saturday" }
			};
		}

		public static class MonthDay
		{
			public static string Key = "#monthday";
		}

		public static class Month
		{
			public static string Key = "#month";

			public static Dictionary<int, string> Map = new Dictionary<int, string>
			{
				{ 1, "##datetime_month_january" },
				{ 2, "##datetime_month_february" },
				{ 3, "##datetime_month_march" },
				{ 4, "##datetime_month_april" },
				{ 5, "##datetime_month_may" },
				{ 6, "##datetime_month_june" },
				{ 7, "##datetime_month_july" },
				{ 8, "##datetime_month_august" },
				{ 9, "##datetime_month_september" },
				{ 10, "##datetime_month_october" },
				{ 11, "##datetime_month_november" },
				{ 12, "##datetime_month_december" }
			};
		}

		public static class Year
		{
			public static string Key = "#year";
		}

		public static string GetLongDateString(DateTime dateTime)
		{
			string text = TryGet("##datetime_dateformat");
			string key = dateTime.DayOfWeek.ToString();
			string term = WeekDay.Map[key];
			text = text.Replace(WeekDay.Key, TryGet(term));
			string newValue = dateTime.Day.ToString();
			text = text.Replace(MonthDay.Key, newValue);
			int month = dateTime.Month;
			string term2 = Month.Map[month];
			text = text.Replace(Month.Key, TryGet(term2));
			string newValue2 = dateTime.Year.ToString();
			return text.Replace(Year.Key, newValue2);
		}
	}

	public static GameObject I2Prefab;

	private const string FallbackLanguage = "English (Canada)";

	private static bool _initAttempted;

	private static string _initialLanguage;

	private static string _wantedLanguage;

	private static bool _firstInitEvaluation;

	private const string _titleFr = "Erreur";

	private const string _contentFr = "Vos paramètres de langue n'ont pas pu être récupérés !";

	private const string _okFr = "OK";

	private const string _titleSp = "Error";

	private const string _contentSp = "La configuración de idioma no se pudieron ser recuperados !";

	private const string _okSp = "OK";

	private const string _titleIt = "Errore";

	private const string _contentIt = "Le impostazioni della lingua non potevano essere recuperati !";

	private const string _okIt = "OK";

	private const string _titleEn = "Error";

	private const string _contentEn = "Your language settings could not be loaded!";

	private const string _okEn = "OK";

	public static bool IsDebug => false;

	public static string MissingSuffix => "*";

	public static string MissingSuffixB => "^";

	public static string FoundSuffix => "|";

	public static void InitIfNeeded()
	{
		if (!_initAttempted)
		{
			_initAttempted = true;
			if (LocalizationManager.CurrentLanguage == string.Empty)
			{
			}
			UserSettings userSettings = UserSettingsReader.Read();
			_wantedLanguage = LocalizationManager.GetSupportedLanguage(userSettings.localization.language);
			LocalizationManager.CurrentLanguage = _wantedLanguage;
			_initialLanguage = LocalizationManager.CurrentLanguage;
			if (_initialLanguage != _wantedLanguage)
			{
				LocalizationManager.CurrentLanguage = LocalizationManager.GetSupportedLanguage("English (Canada)");
				_initialLanguage = LocalizationManager.CurrentLanguage;
				userSettings.localization.language = _initialLanguage;
				UserSettingsReader.Save(userSettings);
			}
		}
	}

	public static string TryGet(string term)
	{
		if (string.IsNullOrEmpty(term))
		{
			return string.Empty;
		}
		if (term.Length > 1 && term[0] == '&' && term[1] == '&')
		{
			return term.Replace("&&", string.Empty);
		}
		if (LocalizationManager.CurrentLanguage == string.Empty)
		{
		}
		InitIfNeeded();
		string termTranslation = LocalizationManager.GetTermTranslation(term);
		termTranslation = termTranslation ?? string.Empty;
		if (termTranslation == term)
		{
			if (IsDebug)
			{
				termTranslation += MissingSuffixB;
			}
		}
		else if (termTranslation == string.Empty)
		{
			termTranslation = term;
			if (IsDebug)
			{
				termTranslation += MissingSuffix;
			}
		}
		else if (IsDebug)
		{
			termTranslation += FoundSuffix;
		}
		termTranslation = termTranslation.Replace("\\n", "\n");
		return termTranslation.Replace("\\t", "\t");
	}

	public static void MergeNewLanguageCSV(string path)
	{
		string cSVstring = File.ReadAllText(path);
		LanguageSource languageSource = LocalizationManager.Sources[0];
		languageSource.Import_CSV(null, cSVstring, eSpreadsheetUpdateMode.Merge);
		LocalizationManager.AddSource(languageSource);
	}

	public static void ShowErrorModalOnFirstInitFailed()
	{
		if (!_firstInitEvaluation)
		{
			_firstInitEvaluation = true;
			InitIfNeeded();
			if (_initialLanguage != _wantedLanguage)
			{
				ShowErrorModal(_initialLanguage, _wantedLanguage);
			}
		}
	}

	private static void ShowErrorModal(string activeLanguage, string wantedLanguage)
	{
		if (FailedLoadingLanguage(activeLanguage, wantedLanguage, "French"))
		{
			ShowModal("Erreur", "Vos paramètres de langue n'ont pas pu être récupérés !", "OK");
		}
		else if (FailedLoadingLanguage(activeLanguage, wantedLanguage, "Spanish"))
		{
			ShowModal("Error", "La configuración de idioma no se pudieron ser recuperados !", "OK");
		}
		else if (FailedLoadingLanguage(activeLanguage, wantedLanguage, "Italian"))
		{
			ShowModal("Errore", "Le impostazioni della lingua non potevano essere recuperati !", "OK");
		}
		else if (FailedLoadingLanguage(activeLanguage, wantedLanguage, "English"))
		{
			ShowModal("Error", "Your language settings could not be loaded!", "OK");
		}
	}

	private static void ShowModal(string title, string content, string ok)
	{
		GameSignals.Invoke(GameSignals.OnDisplayOkDialog, title, content, ok, delegate
		{
		});
	}

	private static bool FailedLoadingLanguage(string activeLanguage, string wantedLangage, string name)
	{
		return IsLanguage(wantedLangage, name) && !IsLanguage(activeLanguage, name);
	}

	private static bool IsLanguage(string language, string name)
	{
		return language?.ToLower().Contains(name.ToLower()) ?? false;
	}

	public static void Save()
	{
	}

	public static void Load()
	{
	}
}
