using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Brix.Steam;
using Smooth.Slinq;

namespace Brix.Assets;

public class AssetHandler<T> where T : Asset_Base<T>
{
	protected List<T> _assets = new List<T>();

	public string officialPath;

	public string ugcPath;

	public string downloadPath;

	public WorkshopItemType workshopType;

	protected bool _cacheFilledAtLeastOnce;

	public List<T> SendQuery()
	{
		return SendQuery(AssetQueryParameters.AllSPSources, AssetPlayableContext.Any);
	}

	public List<T> SendQuery(AssetQueryParameters qparams, AssetPlayableContext pcontexts, bool loadSubassets = false)
	{
		return GenericQuery(qparams, pcontexts, loadSubassets);
	}

	public T SendCodenameQuery(string codename)
	{
		return SendCodenameQuery(codename, AssetQueryParameters.AllSPSources, AssetPlayableContext.Any);
	}

	public T SendCodenameQuery(string codename, AssetQueryParameters qparams, AssetPlayableContext pcontexts)
	{
		return SendQuery(qparams, pcontexts).Slinq().FirstOrDefault((T x) => x.codename == codename);
	}

	public virtual string GetProperPath(T asset)
	{
		asset.workshopType = workshopType;
		_assets.Add(asset);
		if (asset.authorType == AssetAuthorType.Official)
		{
			return officialPath;
		}
		if (asset.authorType == AssetAuthorType.UGC)
		{
			return ugcPath;
		}
		if (asset.authorType == AssetAuthorType.Workshop)
		{
			return ugcPath;
		}
		throw new InvalidOperationException("AssetHandler: Something went wrong when officialising asset.");
	}

	public virtual void RefreshCache()
	{
		_assets.Clear();
		if (Directory.Exists(officialPath))
		{
			AddFilesToList(Directory.GetDirectories(officialPath), AssetAuthorType.Official);
		}
		if (Directory.Exists(ugcPath))
		{
			AddFilesToList(Directory.GetDirectories(ugcPath), AssetAuthorType.UGC);
		}
		if (Directory.Exists(downloadPath))
		{
			AddFilesToList(Directory.GetDirectories(downloadPath), AssetAuthorType.UGC);
		}
		if (workshopType != WorkshopItemType.Unset)
		{
			AddFilesToList(WorkshopItem.GetAllSubscribedItemPathsOfType(workshopType), AssetAuthorType.Workshop);
		}
		_cacheFilledAtLeastOnce = true;
	}

	private List<T> GenericQuery(AssetQueryParameters qparams, AssetPlayableContext pcontexts, bool loadSubassets = false)
	{
		if (!_cacheFilledAtLeastOnce || qparams.HasFlag(AssetQueryParameters.RefreshCache))
		{
			RefreshCache();
		}
		List<T> list = _assets.Slinq().Where(delegate(T x)
		{
			if ((x.authorType == AssetAuthorType.Official && qparams.HasFlag(AssetQueryParameters.Official)) || (x.authorType == AssetAuthorType.UGC && qparams.HasFlag(AssetQueryParameters.UGC)) || (x.authorType == AssetAuthorType.UGC && qparams.HasFlag(AssetQueryParameters.Multiplayer)) || (x.authorType == AssetAuthorType.Workshop && qparams.HasFlag(AssetQueryParameters.Workshop)))
			{
				if (pcontexts.HasFlag(AssetPlayableContext.Singleplayer) && x.playableContext.HasFlag(AssetPlayableContext.Singleplayer))
				{
					return true;
				}
				if (pcontexts.HasFlag(AssetPlayableContext.Multiplayer) && x.playableContext.HasFlag(AssetPlayableContext.Multiplayer))
				{
					return true;
				}
				if (pcontexts.HasFlag(AssetPlayableContext.WorldEditor) && x.playableContext.HasFlag(AssetPlayableContext.WorldEditor))
				{
					return true;
				}
				return false;
			}
			return false;
		}).ToList();
		if (loadSubassets)
		{
			LoadSubassets(list);
		}
		return list;
	}

	protected virtual void LoadSubassets(List<T> assets)
	{
	}

	protected void AddFilesToList(IList<string> files, AssetAuthorType author, bool isFolder = true)
	{
		Type typeFromHandle = typeof(T);
		Type[] types = new Type[1] { typeof(string) };
		ConstructorInfo constructor = typeFromHandle.GetConstructor(types);
		for (int i = 0; i != files.Count; i++)
		{
			if (isFolder && !string.IsNullOrEmpty(files[i]) && !files[i].EndsWith("/"))
			{
				files[i] += "/";
			}
			object[] parameters = new object[1] { files[i] };
			T val = (T)constructor.Invoke(parameters);
			val.authorType = author;
			val.workshopType = workshopType;
			_assets.Add(val);
		}
	}
}
