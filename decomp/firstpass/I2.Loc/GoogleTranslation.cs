using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace I2.Loc;

public static class GoogleTranslation
{
	public static bool CanTranslate()
	{
		return LocalizationManager.Sources.Count > 0 && !string.IsNullOrEmpty(LocalizationManager.GetWebServiceURL());
	}

	public static void Translate(string text, string LanguageCodeFrom, string LanguageCodeTo, Action<string> OnTranslationReady)
	{
		WWW translationWWW = GetTranslationWWW(text, LanguageCodeFrom, LanguageCodeTo);
		CoroutineManager.pInstance.StartCoroutine(WaitForTranslation(translationWWW, OnTranslationReady, text));
	}

	private static IEnumerator WaitForTranslation(WWW www, Action<string> OnTranslationReady, string OriginalText)
	{
		yield return www;
		if (!string.IsNullOrEmpty(www.error))
		{
			Debug.LogError(www.error);
			OnTranslationReady(string.Empty);
		}
		else
		{
			string html = Encoding.UTF8.GetString(www.bytes);
			string obj = ParseTranslationResult(html, OriginalText);
			OnTranslationReady(obj);
		}
	}

	public static string ForceTranslate(string text, string LanguageCodeFrom, string LanguageCodeTo)
	{
		WWW translationWWW = GetTranslationWWW(text, LanguageCodeFrom, LanguageCodeTo);
		while (!translationWWW.isDone)
		{
		}
		if (!string.IsNullOrEmpty(translationWWW.error))
		{
			Debug.LogError("-- " + translationWWW.error);
			return string.Empty;
		}
		string html = Encoding.UTF8.GetString(translationWWW.bytes);
		return ParseTranslationResult(html, text);
	}

	public static WWW GetTranslationWWW(string text, string LanguageCodeFrom, string LanguageCodeTo)
	{
		LanguageCodeFrom = GoogleLanguages.GetGoogleLanguageCode(LanguageCodeFrom);
		LanguageCodeTo = GoogleLanguages.GetGoogleLanguageCode(LanguageCodeTo);
		if (TitleCase(text) == text && text.ToUpper() != text)
		{
			text = text.ToLower();
		}
		string url = $"{LocalizationManager.GetWebServiceURL()}?action=Translate&list={LanguageCodeFrom}:{LanguageCodeTo}={Uri.EscapeUriString(text)}";
		return new WWW(url);
	}

	public static string ParseTranslationResult(string html, string OriginalText)
	{
		try
		{
			string text = html;
			if (TitleCase(OriginalText) == OriginalText && OriginalText.ToUpper() != OriginalText)
			{
				text = TitleCase(text);
			}
			return text;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message);
			return string.Empty;
		}
	}

	public static void Translate(List<TranslationRequest> requests, Action<List<TranslationRequest>> OnTranslationReady)
	{
		WWW translationWWW = GetTranslationWWW(requests);
		CoroutineManager.pInstance.StartCoroutine(WaitForTranslation(translationWWW, OnTranslationReady, requests));
	}

	public static WWW GetTranslationWWW(List<TranslationRequest> requests)
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = true;
		foreach (TranslationRequest request in requests)
		{
			if (!flag)
			{
				stringBuilder.Append("<I2Loc>");
			}
			stringBuilder.Append(request.LanguageCode);
			stringBuilder.Append(":");
			for (int i = 0; i < request.TargetLanguagesCode.Length; i++)
			{
				if (i != 0)
				{
					stringBuilder.Append(",");
				}
				stringBuilder.Append(request.TargetLanguagesCode[i]);
			}
			stringBuilder.Append("=");
			string stringToEscape = ((!(TitleCase(request.Text) == request.Text)) ? request.Text : request.Text.ToLowerInvariant());
			stringBuilder.Append(Uri.EscapeUriString(stringToEscape));
			flag = false;
			if (stringBuilder.Length > 4000)
			{
				break;
			}
		}
		return new WWW($"{LocalizationManager.GetWebServiceURL()}?action=Translate&list={stringBuilder.ToString()}");
	}

	private static IEnumerator WaitForTranslation(WWW www, Action<List<TranslationRequest>> OnTranslationReady, List<TranslationRequest> requests)
	{
		yield return www;
		if (!string.IsNullOrEmpty(www.error))
		{
			Debug.LogError(www.error);
			OnTranslationReady(requests);
		}
		else
		{
			string html = Encoding.UTF8.GetString(www.bytes);
			ParseTranslationResult(html, requests);
			OnTranslationReady(requests);
		}
	}

	public static string ParseTranslationResult(string html, List<TranslationRequest> requests)
	{
		if (html.StartsWith("<!DOCTYPE html>") || html.StartsWith("<HTML>"))
		{
			if (html.Contains("Service invoked too many times in a short time"))
			{
				return string.Empty;
			}
			return "There was a problem contacting the WebService. Please try again later";
		}
		string[] array = html.Split(new string[1] { "<I2Loc>" }, StringSplitOptions.None);
		string[] separator = new string[1] { "<i2>" };
		for (int i = 0; i < Mathf.Min(requests.Count, array.Length); i++)
		{
			TranslationRequest value = requests[i];
			value.Results = array[i].Split(separator, StringSplitOptions.None);
			if (TitleCase(value.Text) == value.Text)
			{
				for (int j = 0; j < value.Results.Length; j++)
				{
					value.Results[j] = TitleCase(value.Results[j]);
				}
			}
			requests[i] = value;
		}
		return string.Empty;
	}

	public static string UppercaseFirst(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return string.Empty;
		}
		char[] array = s.ToLower().ToCharArray();
		array[0] = char.ToUpper(array[0]);
		return new string(array);
	}

	public static string TitleCase(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return string.Empty;
		}
		return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s);
	}
}
