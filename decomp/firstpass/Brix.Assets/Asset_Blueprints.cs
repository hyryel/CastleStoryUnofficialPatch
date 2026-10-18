using System.Collections;
using System.IO;
using Brix.Game.Components.Validators.Graphs;
using Newtonsoft.Json;
using UnityEngine;

namespace Brix.Assets;

public class Asset_Blueprints : Asset_Base<Asset_Blueprints>
{
	private static readonly string __F_META;

	private static readonly string __F_GRAPH;

	private static readonly string __F_THUMBNAIL;

	private BlueprintGraph _graph;

	public BlueprintGraph Graph
	{
		get
		{
			if (_graph == null)
			{
				_graph = TryDisk_ReadBlueprintGraph();
			}
			return _graph;
		}
		set
		{
			_graph = value;
		}
	}

	static Asset_Blueprints()
	{
		__F_META = "meta.json";
		__F_GRAPH = "graph.json";
		__F_THUMBNAIL = "thumbnail.png";
		Asset_Base<Asset_Blueprints>.__CURRENTVERSIONS.Add(typeof(Asset_Blueprints), 1);
	}

	public Asset_Blueprints(string path)
		: base(path)
	{
		Disk_ReadMeta();
		Disk_ReadThumbnail();
	}

	public void InitGraph()
	{
		_graph = TryDisk_ReadBlueprintGraph();
	}

	protected override string GetThumbnailPath()
	{
		return base.path + __F_THUMBNAIL;
	}

	public static Asset_Blueprints CreateShell()
	{
		return new Asset_Blueprints(string.Empty);
	}

	public override void Disk_ReadMeta()
	{
		base.playableContext = AssetPlayableContext.Any;
		if (File.Exists(base.path + __F_META))
		{
			Hashtable table = JsonConvert.DeserializeObject<Hashtable>(File.ReadAllText(base.path + __F_META));
			ReadBaseMeta(table);
		}
	}

	private BlueprintGraph TryDisk_ReadBlueprintGraph()
	{
		BlueprintGraph data = null;
		if (File.Exists(base.path + __F_GRAPH))
		{
			Disk_ReadJsonData<BlueprintGraph>(base.path + __F_GRAPH, out data);
		}
		return data;
	}

	public override Texture2D Disk_ReadThumbnail()
	{
		thumbnail = SubAsset_Texture2D.GetAssetSync(base.path + __F_THUMBNAIL, clearOnSceneChanged: true);
		return thumbnail;
	}

	public int Disk_ReadBlueprintCount()
	{
		return 0;
	}

	protected override void Disk_WriteMeta()
	{
		Hashtable hashtable = new Hashtable();
		hashtable.Add("__SIZE", "Calculate X*Y*Z");
		hashtable.Add("__BLUEPRINTCOUNT", "Calculate X*Y*Z");
		Hashtable hashtable2 = hashtable;
		FillBaseMeta(hashtable2);
		Asset_Base<Asset_Blueprints>.Disk_WriteJsonData(hashtable2, base.path + __F_META, humanReadable: true);
	}

	private void Disk_WriteGraph()
	{
		if (_graph != null)
		{
			Asset_Base<Asset_Blueprints>.Disk_WriteJsonData(_graph, base.path + __F_GRAPH);
		}
	}

	protected override void Disk_WriteThumbnail()
	{
		if (thumbnail != null)
		{
			File.WriteAllBytes(base.path + __F_THUMBNAIL, thumbnail.EncodeToPNG());
		}
	}

	public override void Disk_Overwrite()
	{
		MoveToStaging();
		Disk_Save();
		ApplyStaging();
	}

	public override void Disk_WriteAsNew(AssetAuthorType author)
	{
		base.authorType = author;
		MoveToStaging();
		Disk_Save();
		base.path = AssetDispensary.blueprints.GetProperPath(this) + customName + "/";
		ApplyStaging();
	}

	private void Disk_Save()
	{
		Disk_WriteMeta();
		Disk_WriteGraph();
		Disk_WriteThumbnail();
	}
}
