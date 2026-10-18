using System.Collections;
using System.IO;
using Brix.Lua;
using MoonSharp.Interpreter;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace Brix.Assets;

public class Asset_Mod : Asset_Base<Asset_Mod>
{
	private static readonly string __F_META;

	private static readonly string __F_CLIENTSIDE;

	private static readonly string __F_SERVERSIDE;

	private string _luaRelativePath => base.path.Substring(base.path.IndexOf("Info/Lua/") + 9);

	static Asset_Mod()
	{
		__F_META = "meta.json";
		__F_CLIENTSIDE = "clientside.lua";
		__F_SERVERSIDE = "serverside.lua";
		Asset_Base<Asset_Mod>.__CURRENTVERSIONS.Add(typeof(Asset_Mod), 1);
	}

	public Asset_Mod(string path)
		: base(path)
	{
		Disk_ReadMeta();
	}

	public override void Disk_ReadMeta()
	{
		if (File.Exists(base.path + __F_META))
		{
			Hashtable table = JsonConvert.DeserializeObject<Hashtable>(File.ReadAllText(base.path + __F_META));
			ReadBaseMeta(table);
		}
	}

	public void Disk_LoadMod(out Script outscript)
	{
		Script outscript2 = null;
		if (File.Exists(base.path + __F_CLIENTSIDE))
		{
			LuaLoader.Load(out outscript2, _luaRelativePath + __F_CLIENTSIDE, LuaLibs.BigLib, null);
		}
		if (NetworkServer.active && File.Exists(base.path + __F_SERVERSIDE))
		{
			if (outscript2 != null)
			{
				outscript2.DoFile(_luaRelativePath + __F_SERVERSIDE);
			}
			else
			{
				LuaLoader.Load(out outscript2, _luaRelativePath + __F_SERVERSIDE, LuaLibs.BigLib, null);
			}
		}
		outscript = outscript2;
	}

	protected override void Disk_WriteMeta()
	{
		Hashtable hashtable = new Hashtable();
		FillBaseMeta(hashtable);
		Asset_Base<Asset_Mod>.Disk_WriteJsonData(hashtable, base.path + __F_META, humanReadable: true);
	}
}
