using System.Collections;
using System.IO;
using Newtonsoft.Json;

namespace Brix.Assets;

public class Asset_Language : Asset_Base<Asset_Language>
{
	private static readonly string __F_META;

	private static readonly string __F_LANGUAGE;

	private static readonly string __F_THUMBNAIL;

	public string language_code;

	public string language_name;

	static Asset_Language()
	{
		__F_META = "meta.json";
		__F_LANGUAGE = "language.csv";
		__F_THUMBNAIL = "thumbnail.png";
		Asset_Base<Asset_Language>.__CURRENTVERSIONS.Add(typeof(Asset_Language), 1);
	}

	public Asset_Language(string path)
		: base(path)
	{
		Disk_ReadMeta();
	}

	protected override string GetThumbnailPath()
	{
		return $"{base.path}{__F_THUMBNAIL}";
	}

	public static Asset_Language CreateShell()
	{
		return new Asset_Language(string.Empty);
	}

	public override void Disk_ReadMeta()
	{
		if (File.Exists(base.path + __F_META))
		{
			Hashtable hashtable = JsonConvert.DeserializeObject<Hashtable>(File.ReadAllText(base.path + __F_META));
			ReadBaseMeta(hashtable);
			if (hashtable.ContainsKey("__LANGUAGE_CODE"))
			{
				language_code = hashtable["__LANGUAGE_CODE"].ToString();
			}
			if (hashtable.ContainsKey("__LANGUAGE_NAME"))
			{
				language_name = hashtable["__LANGUAGE_NAME"].ToString();
			}
		}
	}

	public string Disk_ReadCSV()
	{
		if (File.Exists(base.path + __F_LANGUAGE))
		{
			string[] array = File.ReadAllLines(base.path + __F_LANGUAGE);
			array[0] = $"Keys,{language_name},";
			return string.Join("\r\n", array);
		}
		return string.Empty;
	}

	protected override void Disk_WriteMeta()
	{
		Hashtable hashtable = new Hashtable();
		hashtable.Add("__LANGUAGE_CODE", language_code);
		hashtable.Add("__LANGUAGE_NAME", language_name);
		Hashtable hashtable2 = hashtable;
		FillBaseMeta(hashtable2);
		Asset_Base<Asset_Language>.Disk_WriteJsonData(hashtable2, base.path + __F_META, humanReadable: true);
	}
}
