using System.Collections.Generic;
using System.IO;
using Brix.IO.Saves;
using Brix.Utils.Logging;
using Newtonsoft.Json.Linq;

public static class UpdateUtils
{
	public delegate string StrToStr(string s);

	private static BrixLogger logger = new BrixLogger(typeof(UpdateUtils).FullName);

	public static JContainer LoadJson(string path, StrToStr f = null)
	{
		string text = File.ReadAllText(path);
		if (f != null)
		{
			text = f(text);
		}
		JArray result = JArray.Parse(text);
		logger.Info("Loaded Json file as Jarray sucessfully: " + path);
		return result;
	}

	public static JContainer getFirstJson(JObjectFetcher fetcher, JContainer inputJson)
	{
		IEnumerator<JContainer> enumerator = fetcher.Fetch(inputJson).GetEnumerator();
		enumerator.MoveNext();
		return enumerator.Current;
	}

	public static JContainer ConcatJson(JContainer json1, JContainer json2)
	{
		string text = json1.ToString().TrimEnd(']');
		text = text + "," + json2.ToString().TrimStart('[');
		return JArray.Parse(text);
	}

	public static string RemoveSquareBrackets(string json)
	{
		return json.Trim('[', ']');
	}

	public static string EncloseInSqrBracket(string json)
	{
		return "[" + json + "]";
	}
}
