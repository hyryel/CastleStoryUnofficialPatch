using System.Collections;
using System.Collections.Generic;
using System.IO;
using Brix.Steam;
using Newtonsoft.Json;
using Smooth.Slinq;

namespace Brix.Assets;

public class Asset_GamemodePreset : Asset_Base<Asset_GamemodePreset>
{
	public class PresetHandler : AssetHandler<Asset_GamemodePreset>
	{
		public override void RefreshCache()
		{
			_assets.Clear();
			if (Directory.Exists(officialPath))
			{
				AddFilesToList(Directory.GetFiles(officialPath), AssetAuthorType.Official, isFolder: false);
			}
			if (Directory.Exists(ugcPath))
			{
				AddFilesToList(Directory.GetFiles(ugcPath), AssetAuthorType.UGC, isFolder: false);
			}
			AddFilesToList(WorkshopItem.GetAllSubscribedItemPathsOfType(WorkshopItemType.Preset), AssetAuthorType.Workshop, isFolder: false);
			List<Asset_Gamemode> list = AssetDispensary.gamemodes.SendQuery();
			for (int i = 0; i != list.Count; i++)
			{
				if (Directory.Exists(list[i].path + "Presets/"))
				{
					AddFilesToList(Directory.GetFiles(list[i].path + "Presets/"), list[i].authorType, isFolder: false);
				}
			}
			_cacheFilledAtLeastOnce = true;
		}

		public Asset_GamemodePreset SendCodenameQuery(string codename, string ofGamemode = null, AssetQueryParameters qparams = AssetQueryParameters.AllSources, AssetPlayableContext pcontexts = AssetPlayableContext.Any)
		{
			List<Asset_GamemodePreset> list = SendQuery(ofGamemode, qparams, pcontexts);
			return list.Slinq().FirstOrDefault((Asset_GamemodePreset x) => x.codename == codename);
		}

		public List<Asset_GamemodePreset> SendQuery(string ofGamemode, AssetQueryParameters qparams = AssetQueryParameters.AllSources, AssetPlayableContext pcontexts = AssetPlayableContext.Any)
		{
			List<Asset_GamemodePreset> list = SendQuery(qparams, pcontexts);
			if (ofGamemode != null)
			{
				return (from x in list.Slinq()
					where x.targetGamemode == ofGamemode
					select x).ToList();
			}
			return list;
		}
	}

	public string targetGamemode;

	public int order = -1;

	public bool isDefault;

	private Dictionary<string, object> _settings;

	public Dictionary<string, object> settings
	{
		get
		{
			if (_settings == null && !IsShell())
			{
				_settings = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(base.path));
				_settings.Remove("__CODENAME");
				_settings.Remove("__DISPLAYNAME");
				_settings.Remove("__DESCRIPTION");
				_settings.Remove("__PLAYABLECONTEXT");
				_settings.Remove("__PRESET_GAMEMODE");
				_settings.Remove("__PRESET_ORDER");
				_settings.Remove("__PRESET_ISDEFAULT");
			}
			return _settings;
		}
		set
		{
			_settings = value;
		}
	}

	static Asset_GamemodePreset()
	{
		Asset_Base<Asset_GamemodePreset>.__CURRENTVERSIONS.Add(typeof(Asset_GamemodePreset), 1);
	}

	public Asset_GamemodePreset(string path)
		: base(path)
	{
		Disk_ReadMeta();
	}

	private object TryGetFromSettings(string name)
	{
		if (_settings == null && !IsShell() && _settings.ContainsKey(name))
		{
			return _settings[name];
		}
		return null;
	}

	public override void Disk_ReadMeta()
	{
		Hashtable hashtable = JsonConvert.DeserializeObject<Hashtable>(File.ReadAllText(base.path));
		ReadBaseMeta(hashtable);
		if (hashtable.ContainsKey("__PRESET_GAMEMODE"))
		{
			targetGamemode = (string)hashtable["__PRESET_GAMEMODE"];
		}
		if (hashtable.ContainsKey("__PRESET_ORDER"))
		{
			order = (int)(long)hashtable["__PRESET_ORDER"];
		}
		if (hashtable.ContainsKey("__PRESET_ISDEFAULT"))
		{
			isDefault = (bool)hashtable["__PRESET_ISDEFAULT"];
		}
	}

	protected override void Disk_WriteMeta()
	{
		Hashtable table = new Hashtable
		{
			{ "__PRESET_GAMEMODE", targetGamemode },
			{ "__PRESET_ORDER", order },
			{ "__PRESET_ISDEFAULT", isDefault }
		};
		FillBaseMeta(table);
		settings?.Slinq().ForEach(delegate(KeyValuePair<string, object> x)
		{
			table[x.Key] = x.Value;
		});
		Asset_Base<Asset_GamemodePreset>.Disk_WriteJsonData(table, base.path, humanReadable: true);
	}
}
