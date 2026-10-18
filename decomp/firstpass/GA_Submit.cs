using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class GA_Submit
{
	public delegate void SubmitSuccessHandler(List<Item> items, bool success);

	public delegate void SubmitErrorHandler(List<Item> items);

	public enum CategoryType
	{
		GA_User,
		GA_Event,
		GA_Purchase,
		GA_Error
	}

	public struct Item
	{
		public CategoryType Type;

		public Hashtable Parameters;

		public float AddTime;

		public int Count;
	}

	public static Dictionary<CategoryType, string> Categories = new Dictionary<CategoryType, string>
	{
		{
			CategoryType.GA_User,
			"user"
		},
		{
			CategoryType.GA_Event,
			"design"
		},
		{
			CategoryType.GA_Purchase,
			"business"
		},
		{
			CategoryType.GA_Error,
			"error"
		}
	};

	private string _publicKey;

	private string _privateKey;

	private static string _baseURL = "://api.gameanalytics.com";

	private static string _version = "1";

	public void SetupKeys(string publicKey, string privateKey)
	{
		_publicKey = publicKey;
		_privateKey = privateKey;
	}

	public void SubmitQueue(List<Item> queue, SubmitSuccessHandler successEvent, SubmitErrorHandler errorEvent, bool gaTracking, string pubKey, string priKey)
	{
		if ((_publicKey.Equals(string.Empty) || _privateKey.Equals(string.Empty)) && (pubKey.Equals(string.Empty) || priKey.Equals(string.Empty)))
		{
			if (!gaTracking)
			{
				GA.LogError("Game Key and/or Secret Key not set. Open GA_Settings to set keys.");
			}
			return;
		}
		Dictionary<CategoryType, List<Item>> dictionary = new Dictionary<CategoryType, List<Item>>();
		foreach (Item item in queue)
		{
			if (dictionary.ContainsKey(item.Type))
			{
				if (!item.Parameters.ContainsKey(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.UserID]))
				{
					item.Parameters.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.UserID], GA.API.GenericInfo.UserID);
				}
				if (!item.Parameters.ContainsKey(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.SessionID]))
				{
					item.Parameters.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.SessionID], GA.API.GenericInfo.SessionID);
				}
				if (!item.Parameters.ContainsKey(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.Build]))
				{
					item.Parameters.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.Build], GA.SettingsGA.Build);
				}
				dictionary[item.Type].Add(item);
				continue;
			}
			if (!item.Parameters.ContainsKey(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.UserID]))
			{
				item.Parameters.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.UserID], GA.API.GenericInfo.UserID);
			}
			if (!item.Parameters.ContainsKey(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.SessionID]))
			{
				item.Parameters.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.SessionID], GA.API.GenericInfo.SessionID);
			}
			if (!item.Parameters.ContainsKey(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.Build]))
			{
				item.Parameters.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.Build], GA.SettingsGA.Build);
			}
			dictionary.Add(item.Type, new List<Item> { item });
		}
		Submit(dictionary, successEvent, errorEvent, gaTracking, pubKey, priKey);
	}

	public void Submit(Dictionary<CategoryType, List<Item>> categories, SubmitSuccessHandler successEvent, SubmitErrorHandler errorEvent, bool gaTracking, string pubKey, string priKey)
	{
		if (pubKey.Equals(string.Empty))
		{
			pubKey = _publicKey;
		}
		if (priKey.Equals(string.Empty))
		{
			priKey = _privateKey;
		}
		foreach (KeyValuePair<CategoryType, List<Item>> category in categories)
		{
			List<Item> value = category.Value;
			if (value.Count == 0)
			{
				continue;
			}
			CategoryType type = value[0].Type;
			string uRL = GetURL(Categories[type], pubKey);
			List<Hashtable> list = new List<Hashtable>();
			for (int i = 0; i < value.Count; i++)
			{
				if (type != value[i].Type)
				{
					GA.LogWarning("GA Error: All messages in a submit must be of the same service/category type.");
					errorEvent?.Invoke(value);
				}
				if (!value[i].Parameters.ContainsKey(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.UserID]))
				{
					value[i].Parameters.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.UserID], GA.API.GenericInfo.UserID);
				}
				else if (value[i].Parameters[GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.UserID]] == null)
				{
					value[i].Parameters[GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.UserID]] = GA.API.GenericInfo.UserID;
				}
				Hashtable item = ((value[i].Count <= 1) ? value[i].Parameters : value[i].Parameters);
				list.Add(item);
			}
			string text = DictToJson(list);
			if (GA.SettingsGA.ArchiveData && !gaTracking && !GA.SettingsGA.InternetConnectivity)
			{
				if (GA.SettingsGA.DebugMode)
				{
					GA.Log("GA: Archiving data (no network connection).");
				}
				GA.API.Archive.ArchiveData(text, type);
				successEvent?.Invoke(value, success: true);
			}
			else if (!GA.SettingsGA.InternetConnectivity)
			{
				if (!gaTracking)
				{
					GA.LogWarning("GA Error: No network connection.");
				}
				errorEvent?.Invoke(value);
			}
			string jsonHash = CreateMD5Hash(text + priKey);
			WWW www = CreateSubmitWWW(uRL, text, jsonHash);
			GA.RunCoroutine(SendWWW(www, successEvent, errorEvent, gaTracking, text, jsonHash, value));
		}
	}

	public static WWW CreateSubmitWWW(string url, string json, string jsonHash)
	{
		WWW wWW = null;
		byte[] bytes = Encoding.UTF8.GetBytes(json);
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("Authorization", jsonHash);
		return new WWW(url, bytes, dictionary);
	}

	public static IEnumerator SendWWW(WWW www, SubmitSuccessHandler successEvent, SubmitErrorHandler errorEvent, bool gaTracking, string json, string jsonHash, List<Item> items)
	{
		www.threadPriority = ThreadPriority.Low;
		yield return www;
		if (GA.SettingsGA.DebugMode && !gaTracking)
		{
			GA.Log("GA URL: " + www.url);
			GA.Log("GA Submit: " + json);
			GA.Log("GA Hash: " + jsonHash);
		}
		try
		{
			if (!string.IsNullOrEmpty(www.error) && !CheckServerReply(www))
			{
				throw new Exception(www.error);
			}
			Hashtable hashtable = (Hashtable)GA_MiniJSON.JsonDecode(www.text);
			if ((hashtable != null && hashtable.ContainsKey("status") && hashtable["status"].ToString().Equals("ok")) || CheckServerReply(www))
			{
				if (GA.SettingsGA.DebugMode && !gaTracking)
				{
					GA.Log("GA Result: " + www.text);
				}
				successEvent?.Invoke(items, success: true);
			}
			else if (hashtable != null && hashtable.ContainsKey("message") && hashtable["message"].ToString().Equals("Game not found") && hashtable.ContainsKey("code") && hashtable["code"].ToString().Equals("400"))
			{
				if (!gaTracking)
				{
					GA.LogWarning("GA Error: " + www.text + " (NOTE: make sure your Game Key and Secret Key match the keys you recieved from the Game Analytics website. It might take a few minutes before a newly added game will be able to recieve data.)");
				}
				errorEvent?.Invoke(null);
			}
			else
			{
				if (!gaTracking)
				{
					GA.LogWarning("GA Error: " + www.text);
				}
				errorEvent?.Invoke(items);
			}
		}
		catch (Exception ex)
		{
			if (!gaTracking)
			{
				GA.LogWarning("GA Error: " + ex.Message);
			}
			if (ex.Message.Contains("400 Bad Request"))
			{
				errorEvent?.Invoke(null);
			}
			else
			{
				errorEvent?.Invoke(items);
			}
		}
	}

	public static string GetBaseURL(bool inclVersion)
	{
		if (inclVersion)
		{
			return GetUrlStart() + _baseURL + "/" + _version;
		}
		return GetUrlStart() + _baseURL;
	}

	public static string GetURL(string category, string pubKey)
	{
		return GetUrlStart() + _baseURL + "/" + _version + "/" + pubKey + "/" + category;
	}

	private static string GetUrlStart()
	{
		if (Application.absoluteURL.StartsWith("https"))
		{
			return "https";
		}
		return "http";
	}

	public static string CreateMD5Hash(string input)
	{
		MD5 mD = new MD5CryptoServiceProvider();
		byte[] bytes = Encoding.UTF8.GetBytes(input);
		byte[] array = mD.ComputeHash(bytes);
		string text = string.Empty;
		byte[] array2 = array;
		foreach (byte b in array2)
		{
			text += $"{b:x2}";
		}
		return text;
	}

	public static string CreateSha1Hash(string input)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(input);
		SHA1CryptoServiceProvider sHA1CryptoServiceProvider = new SHA1CryptoServiceProvider();
		return BitConverter.ToString(sHA1CryptoServiceProvider.ComputeHash(bytes)).Replace("-", string.Empty);
	}

	public string GetPrivateKey()
	{
		return _privateKey;
	}

	public static bool CheckServerReply(WWW www)
	{
		try
		{
			if (!string.IsNullOrEmpty(www.error))
			{
				string text = www.error.Substring(0, 3);
				if (text.Equals("201") || text.Equals("202") || text.Equals("203") || text.Equals("204") || text.Equals("205") || text.Equals("206"))
				{
					return true;
				}
			}
			if (!www.responseHeaders.ContainsKey("STATUS"))
			{
				return false;
			}
			string text2 = www.responseHeaders["STATUS"];
			string[] array = text2.Split(' ');
			if (array.Length > 1 && int.TryParse(array[1], out var result) && result >= 200 && result < 300)
			{
				return true;
			}
			return false;
		}
		catch
		{
			return false;
		}
	}

	public static string DictToJson(List<Hashtable> list)
	{
		string text = "[";
		int num = 0;
		int num2 = 0;
		foreach (Hashtable item in list)
		{
			text += '{';
			num2 = 0;
			foreach (object key in item.Keys)
			{
				num2++;
				string text2 = text;
				text = string.Concat(text2, "\"", key, "\":\"", item[key], "\"");
				if (num2 < item.Keys.Count)
				{
					text += ',';
				}
			}
			text += '}';
			num++;
			if (num < list.Count)
			{
				text += ',';
			}
		}
		return text + "]";
	}
}
