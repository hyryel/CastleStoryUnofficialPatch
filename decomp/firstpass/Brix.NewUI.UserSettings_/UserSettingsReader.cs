using System;
using System.Collections.Generic;
using System.IO;
using Brix.Utils;
using Brix.Utils.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Brix.NewUI.UserSettings_;

public static class UserSettingsReader
{
	private static BrixLogger logger = new BrixLogger(typeof(UserSettingsReader).FullName);

	public const string defaultSettingsPath = "Info/UserSettings/Default.json";

	public const string currentSettingPath = "Info/UserSettings/Current.json";

	public static UserSettings GetDefault()
	{
		return ReadSettingAt("Info/UserSettings/Default.json");
	}

	public static UserSettings CreateDefault()
	{
		return new UserSettings();
	}

	public static UserSettings Read()
	{
		RemoveBadSettings();
		return ReadSettingAt("Info/UserSettings/Current.json");
	}

	private static UserSettings ReadSettingAt(string path)
	{
		UserSettings userSettings;
		try
		{
			string value = File.ReadAllText("Info/UserSettings/Default.json");
			userSettings = JsonConvert.DeserializeObject<UserSettings>(value);
		}
		catch (Exception exception)
		{
			logger.Warn(exception, "Failed to load default settings");
			userSettings = CreateDefault();
		}
		if (File.Exists(path))
		{
			try
			{
				string value2 = File.ReadAllText(path);
				JsonConvert.PopulateObject(value2, userSettings);
			}
			catch (Exception exception2)
			{
				logger.Warn(exception2, "Failed to load user settings");
			}
		}
		return userSettings;
	}

	public static void Save(UserSettings settings)
	{
		string contents = JsonConvert.SerializeObject(settings, Formatting.Indented);
		File.WriteAllText("Info/UserSettings/Current.json", contents);
	}

	public static void RemoveBadSettings()
	{
		try
		{
			_RemoveBadSettings();
		}
		catch (Exception exception)
		{
			logger.Warn(exception, "Failed to normalize version for user settings");
			try
			{
				if (File.Exists("Info/UserSettings/Current.json"))
				{
					File.Delete("Info/UserSettings/Current.json");
				}
			}
			catch (Exception exception2)
			{
				logger.Warn(exception2, "Failed to delete outdated user settings");
			}
		}
	}

	private static void _RemoveBadSettings()
	{
		string json = File.ReadAllText("Info/UserSettings/Default.json");
		string json2 = File.ReadAllText("Info/UserSettings/Current.json");
		JObject jObject = JObject.Parse(json);
		JObject jObject2 = JObject.Parse(json2);
		List<string> list = CollectionPool.RequestList<string>();
		foreach (JToken item in jObject2.Children())
		{
			if (item.Type != JTokenType.Property)
			{
				continue;
			}
			JProperty jProperty = item as JProperty;
			if (jObject[jProperty.Name] == null)
			{
				list.Add(jProperty.Name);
				continue;
			}
			JToken jToken = jObject2[jProperty.Name]["_version"];
			JToken jToken2 = jObject[jProperty.Name]["_version"];
			if (jToken2 != null && (jToken == null || jToken.Value<int>() < jToken2.Value<int>()))
			{
				list.Add(jProperty.Name);
			}
			if (jProperty.Name == "input")
			{
				JToken jToken3 = jObject2[jProperty.Name]["_configsVersion"];
				JToken jToken4 = jObject[jProperty.Name]["_configsVersion"];
				int num = jToken3?.Value<int>() ?? 0;
				int num2 = jToken4?.Value<int>() ?? 0;
				if (num < num2 && File.Exists("Info/Lua/userconfigs.lua"))
				{
					File.WriteAllText("Info/Lua/userconfigs.lua", string.Empty);
				}
			}
		}
		foreach (string item2 in list)
		{
			jObject2.Remove(item2);
		}
		list.Dispose();
		File.WriteAllText("Info/UserSettings/Current.json", jObject2.ToString(Formatting.Indented));
	}
}
