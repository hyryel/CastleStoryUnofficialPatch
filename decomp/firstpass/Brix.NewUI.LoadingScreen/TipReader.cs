using System.Collections.Generic;
using System.IO;
using Brix.Utils.Logging;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.NewUI.LoadingScreen;

public class TipReader : MonoBehaviour
{
	private static BrixLogger logger = new BrixLogger(typeof(TipReader).FullName);

	private static string TipPath = "Info/Tips/tips.json";

	private static string TriviaPath = "Info/Tips/trivia.json";

	public static List<Tip> ReadTips()
	{
		return ReadTipsAt(TipPath);
	}

	public static List<Tip> ReadTrivia()
	{
		return ReadTipsAt(TriviaPath);
	}

	private static List<Tip> ReadTipsAt(string path)
	{
		try
		{
			string value = File.ReadAllText(path);
			return JsonConvert.DeserializeObject<List<Tip>>(value);
		}
		catch
		{
			logger.Warn("Can't find Tip file");
			return null;
		}
	}
}
