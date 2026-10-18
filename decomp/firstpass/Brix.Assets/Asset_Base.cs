using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Brix.IO.Serialization;
using Brix.Steam;
using Brix.Utils;
using Brix.Utils.Logging;
using Brix.Utils.Signals;
using Newtonsoft.Json;
using Smooth.Slinq;
using Steamworks;
using UnityEngine;

namespace Brix.Assets;

public abstract class Asset_Base<D> where D : Asset_Base<D>
{
	private static BrixLogger logger = new BrixLogger(typeof(Asset_Base<D>).FullName);

	protected string _codename = "nocodename";

	protected string _rawDisplayName = "NoDisplayName";

	public string customName = string.Empty;

	protected string _rawDescription = string.Empty;

	public int version = -1;

	private int _creationTime;

	private long _modifiedTime;

	private float _rating = 0.5f;

	protected ulong _workshopID;

	public WorkshopItemType workshopType;

	public WorkshopItemTags workshopTags;

	public Texture2D thumbnail;

	public Texture2D heightmap;

	private bool _alive;

	private string _fullPath;

	private string _stagePath;

	private AssetPlayableContext _playContext;

	private AssetAuthorType _author;

	protected static Dictionary<Type, int> __CURRENTVERSIONS = new Dictionary<Type, int>();

	public string codename
	{
		get
		{
			return _codename;
		}
		set
		{
			_codename = Regex.Replace(value, "_(\\{){0,1}[0-9a-fA-F]{8}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{12}(\\}){0,1}$", string.Empty, RegexOptions.IgnoreCase) + "_" + Guid.NewGuid();
		}
	}

	public string shortCodename => Regex.Replace(_codename, "_(\\{){0,1}[0-9a-fA-F]{8}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{12}(\\}){0,1}$", string.Empty, RegexOptions.IgnoreCase);

	public string displayName => I2Helper.TryGet(_rawDisplayName);

	public string rawDisplayName
	{
		get
		{
			return _rawDisplayName;
		}
		set
		{
			_rawDisplayName = value;
		}
	}

	public string description => I2Helper.TryGet(_rawDescription);

	public string rawDescription
	{
		get
		{
			return _rawDescription;
		}
		set
		{
			_rawDescription = value;
		}
	}

	public float Rating => _rating;

	public ulong workshopID => _workshopID;

	public Texture2D Thumbnail
	{
		get
		{
			if (thumbnail == null)
			{
				Disk_ReadThumbnail();
			}
			return thumbnail;
		}
	}

	public bool Alive => _alive;

	public long ModifiedTime => _modifiedTime;

	public string dateCreated => dateTimeCreated.ToShortDateString();

	public string timeCreated => dateTimeCreated.ToLongTimeString();

	public DateTime dateTimeCreated
	{
		get
		{
			if (IsShell())
			{
				return DateTime.MinValue;
			}
			return File.GetLastWriteTime(path);
		}
	}

	public string path
	{
		get
		{
			return (!IsStaging()) ? _fullPath : _stagePath;
		}
		protected set
		{
			_fullPath = ((!string.IsNullOrEmpty(value)) ? Path.GetFullPath(value.Replace("\\", "/")).Replace("\\", "/") : string.Empty);
		}
	}

	public AssetPlayableContext playableContext
	{
		get
		{
			return _playContext;
		}
		set
		{
			_playContext = value;
		}
	}

	public AssetAuthorType authorType
	{
		get
		{
			return _author;
		}
		set
		{
			_author = value;
		}
	}

	protected Asset_Base(string path)
	{
		this.path = path;
		_alive = true;
	}

	public virtual Texture2D Disk_ReadThumbnail()
	{
		return thumbnail;
	}

	public virtual void Disk_ReadThumbnailAsync(Action<Texture2D> callback)
	{
		callback(Disk_ReadThumbnail());
	}

	public virtual void FreeTextureAlloc()
	{
		_alive = false;
	}

	public static int GetCurrentVersion()
	{
		return __CURRENTVERSIONS[typeof(D)];
	}

	public bool IsShell()
	{
		return string.IsNullOrEmpty(_fullPath);
	}

	public bool IsStaging()
	{
		return !string.IsNullOrEmpty(_stagePath);
	}

	public bool IsCurrentVersion()
	{
		return version == GetCurrentVersion();
	}

	public virtual bool Disk_Exists()
	{
		return Directory.Exists(path);
	}

	public void SendToWorkshop(string hackTitle, string hackDescription, string hackComment, ERemoteStoragePublishedFileVisibility visibility)
	{
		if (IsShell())
		{
			return;
		}
		WorkshopItem item;
		if (_workshopID == 0)
		{
			item = WorkshopItem.CreateItem();
		}
		else
		{
			item = WorkshopItem.RecoverItem(new PublishedFileId_t(_workshopID), makeLocal: true);
		}
		if (item != null)
		{
			item.ItemCreated.LateConnectOnce(delegate
			{
				_workshopID = item.filedID.m_PublishedFileId;
				Disk_WriteMeta();
				FillAndSendWorkshopUpdate(item, hackTitle, hackDescription, hackComment, visibility);
			});
		}
	}

	public WorkshopItem GetWorkshopItem()
	{
		WorkshopItem result = null;
		if (_workshopID != 0)
		{
			result = WorkshopItem.RecoverItem(new PublishedFileId_t(_workshopID), makeLocal: true);
		}
		return result;
	}

	protected void FillAndSendWorkshopUpdate(WorkshopItem item, string hackTitle, string hackDescription, string hackComment, ERemoteStoragePublishedFileVisibility visibility)
	{
		item.title = ((!string.IsNullOrEmpty(hackTitle)) ? hackTitle : ((!string.IsNullOrEmpty(customName)) ? customName : displayName));
		item.description = ((!string.IsNullOrEmpty(hackDescription)) ? hackDescription : description);
		item.language = "english";
		item.visibility = visibility;
		item.itemType = workshopType;
		item.itemTags = workshopTags;
		item.updateComment = hackComment;
		item.contentPath = path;
		item.previewPath = GetThumbnailPath();
		item.SendUpdate();
	}

	public void MoveToStaging()
	{
		_stagePath = GetTempPath();
		if (IsShell())
		{
			Directory.CreateDirectory(_stagePath);
			return;
		}
		try
		{
			Disk_CopyFrom(_fullPath, _stagePath);
		}
		catch (UnauthorizedAccessException ex)
		{
			logger.ErrorFormat(ex, "The asset {0} is locked by another process.", _fullPath);
			throw ex;
		}
	}

	public void CancelStaging()
	{
		if (IsStaging())
		{
			Directory.Delete(_stagePath, recursive: true);
			_stagePath = string.Empty;
		}
	}

	public void ApplyStaging()
	{
		if (IsStaging())
		{
			string stagePath = _stagePath;
			_stagePath = string.Empty;
			Disk_Delete();
			Disk_CopyFrom(stagePath, path);
			Directory.Delete(stagePath, recursive: true);
		}
	}

	public virtual string Disk_CreateBackup()
	{
		string text = AssetDispensary.BackupsPath + shortCodename + "_BACKUP";
		Disk_CopyFrom(path, text);
		return text;
	}

	public void Disk_Delete()
	{
		if (path.Contains("/Info/") && path != "/")
		{
			Disk_SpecialisedDeletion();
		}
	}

	protected virtual void Disk_SpecialisedDeletion()
	{
		if (!Directory.Exists(path))
		{
			return;
		}
		try
		{
			Directory.Delete(path, recursive: true);
		}
		catch (Exception e)
		{
			if (Directory.Exists(path))
			{
				logger.WarnFormat(e, "Directory.Delete() failed (but succeeded?) at {0}", _fullPath);
			}
			else
			{
				logger.WarnFormat(e, "Directory.Delete() failed at {0}", _fullPath);
			}
		}
	}

	public abstract void Disk_ReadMeta();

	protected void ReadBaseMeta(Hashtable table)
	{
		if (table == null)
		{
			logger.ErrorFormat("ReadBaseMeta() unexpected null table at {0}", _fullPath);
			return;
		}
		if (table.ContainsKey("__CODENAME"))
		{
			_codename = (string)table["__CODENAME"];
		}
		if (table.ContainsKey("__DISPLAYNAME"))
		{
			_rawDisplayName = (string)table["__DISPLAYNAME"];
		}
		if (table.ContainsKey("__CUSTOMNAME"))
		{
			customName = (string)table["__CUSTOMNAME"];
		}
		if (table.ContainsKey("__DESCRIPTION"))
		{
			_rawDescription = (string)table["__DESCRIPTION"];
		}
		if (table.ContainsKey("__ASSETVERSION"))
		{
			version = (int)(long)table["__ASSETVERSION"];
		}
		if (table.ContainsKey("__CREATIONTIME"))
		{
			_creationTime = (int)(long)table["__CREATIONTIME"];
		}
		if (table.ContainsKey("__MODIFIEDTIME"))
		{
			_modifiedTime = (int)(long)table["__MODIFIEDTIME"];
		}
		if (table.ContainsKey("__RATING"))
		{
			_rating = (float)(double)table["__RATING"];
		}
		if (table.ContainsKey("__WORKSHOPID"))
		{
			_workshopID = (ulong)(long)table["__WORKSHOPID"];
		}
		if (table.ContainsKey("__WORKSHOPTAGS"))
		{
			workshopTags = (WorkshopItemTags)(long)table["__WORKSHOPTAGS"];
		}
		if (table.ContainsKey("__PLAYABLECONTEXT"))
		{
			playableContext = (AssetPlayableContext)(long)table["__PLAYABLECONTEXT"];
		}
	}

	protected bool Disk_ReadJsonData<T>(string destinationFile, out T data)
	{
		string text = Path.Combine(path, destinationFile);
		data = default(T);
		if (!File.Exists(text))
		{
			return false;
		}
		using (StreamReader reader = File.OpenText(text))
		{
			using JsonReader reader2 = new JsonTextReader(reader);
			data = Io.GenericSerializer.Deserialize<T>(reader2);
		}
		return true;
	}

	protected virtual string GetThumbnailPath()
	{
		return string.Empty;
	}

	public virtual string Disk_GetMD5Hash()
	{
		List<string> list = (from p in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).Slinq()
			orderby p
			select p).ToList();
		MD5 mD = MD5.Create();
		for (int num = 0; num < list.Count; num++)
		{
			string text = list[num];
			string text2 = text.Substring(path.Length + 1);
			byte[] bytes = Encoding.UTF8.GetBytes(text2.ToLower());
			mD.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
			byte[] array = File.ReadAllBytes(text);
			if (num == list.Count - 1)
			{
				mD.TransformFinalBlock(array, 0, array.Length);
			}
			else
			{
				mD.TransformBlock(array, 0, array.Length, array, 0);
			}
		}
		return BitConverter.ToString(mD.Hash).Replace("-", string.Empty).ToLower();
	}

	protected abstract void Disk_WriteMeta();

	protected void FillBaseMeta(Hashtable table)
	{
		table.Add("__CODENAME", _codename);
		table.Add("__DISPLAYNAME", _rawDisplayName);
		table.Add("__CUSTOMNAME", customName);
		table.Add("__DESCRIPTION", _rawDescription);
		table.Add("__ASSETVERSION", GetCurrentVersion());
		table.Add("__PLAYABLECONTEXT", playableContext);
		table.Add("__CREATIONTIME", _creationTime);
		table.Add("__MODIFIEDTIME", DateTime.Now.ToBinary());
		table.Add("__RATING", _rating);
		table.Add("__WORKSHOPID", _workshopID);
		table.Add("__WORKSHOPTAGS", workshopTags);
	}

	private static string GetTempPath(bool isFolder = true)
	{
		return (Path.GetTempPath() + "/" + Path.GetRandomFileName() + ((!isFolder) ? string.Empty : "/")).Replace("\\", "/");
	}

	public static void Disk_WriteJsonData<T>(T data, string destination, bool humanReadable = false)
	{
		using StreamWriter textWriter = File.CreateText(destination);
		using JsonWriter jsonWriter = new JsonTextWriter(textWriter);
		if (humanReadable)
		{
			jsonWriter.Formatting = Formatting.Indented;
		}
		Io.GenericSerializer.Serialize(jsonWriter, data);
	}

	private static void Disk_CopyFrom(string srcFolder, string dstFolder)
	{
		if (!Directory.Exists(dstFolder))
		{
			Directory.CreateDirectory(dstFolder);
		}
		string[] files = Directory.GetFiles(srcFolder);
		foreach (string sourceFileName in files)
		{
			File.Copy(sourceFileName, Path.Combine(dstFolder, Path.GetFileName(sourceFileName)), overwrite: true);
		}
		string[] directories = Directory.GetDirectories(srcFolder);
		foreach (string srcFolder2 in directories)
		{
			string dstFolder2 = Path.Combine(dstFolder, new DirectoryInfo(srcFolder2).Name);
			if (!Directory.Exists(dstFolder2))
			{
				Directory.CreateDirectory(dstFolder2);
			}
			Disk_CopyFrom(srcFolder2, dstFolder2);
		}
	}

	protected virtual void Disk_WriteThumbnail()
	{
		throw new NotImplementedException();
	}

	public virtual void Disk_Overwrite()
	{
		throw new NotImplementedException();
	}

	public virtual void Disk_WriteAsNew(AssetAuthorType author)
	{
		throw new NotImplementedException();
	}

	public void Disk_OverwriteOrWriteAsNew(AssetAuthorType author)
	{
		if (IsShell() || !Disk_Exists())
		{
			Disk_WriteAsNew(author);
		}
		else
		{
			Disk_Overwrite();
		}
	}
}
