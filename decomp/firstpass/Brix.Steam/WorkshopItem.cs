using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using Brix.Assets;
using Brix.Engine;
using Brix.External.Signals;
using Brix.UI.Builder.Menu;
using Brix.Utils;
using Brix.Utils.Collections;
using Brix.Utils.Logging;
using Newtonsoft.Json;
using Smooth.Slinq;
using Smooth.Slinq.Context;
using Steamworks;
using UnityEngine;

namespace Brix.Steam;

public class WorkshopItem
{
	private static BrixLogger logger = new BrixLogger(typeof(WorkshopItem).FullName);

	private static List<WorkshopItem> _workshopItemsList = new List<WorkshopItem>();

	public string language = "english";

	public string previewPath = string.Empty;

	public string updateComment = string.Empty;

	public string targetGamemode = string.Empty;

	private string _contentPath = string.Empty;

	public WorkshopItemType itemType;

	public WorkshopItemTags itemTags;

	private SteamUGCDetails_t _details;

	public bool isLocal;

	private uint _state;

	private PublishedFileId_t _fileID;

	private UGCUpdateHandle_t _updateHandle;

	private bool _updateInProgress;

	public bool __UI_ISSHOWING;

	public Signal ItemCreated = new Signal();

	public Signal ItemDeleted = new Signal();

	private CallResult<CreateItemResult_t> _createItemCallResult;

	private CallResult<SubmitItemUpdateResult_t> _submitItemResultCallResult;

	private CallResult<SteamUGCQueryCompleted_t> _querryCompleted;

	public const string SandboxShortCodename = "sandbox";

	public const string InvasionShortCodename = "invasion";

	public const string ConquestShortCodename = "conquest";

	public static List<WorkshopItemTags> WorkshopItemTagList = new List<WorkshopItemTags>
	{
		WorkshopItemTags.Sandbox,
		WorkshopItemTags.Invasion,
		WorkshopItemTags.Conquest
	};

	public string contentPath
	{
		get
		{
			if (string.IsNullOrEmpty(_contentPath) && SteamUGC.GetItemInstallInfo(filedID, out var _, out var pchFolder, 1024u, out var _))
			{
				_contentPath = pchFolder;
				if (itemType == WorkshopItemType.Preset)
				{
					_contentPath = Directory.GetFiles(_contentPath)[0];
				}
			}
			return _contentPath;
		}
		set
		{
			_contentPath = value;
		}
	}

	public string title
	{
		get
		{
			return _details.m_rgchTitle ?? string.Empty;
		}
		set
		{
			_details.m_rgchTitle = value;
		}
	}

	public string description
	{
		get
		{
			return _details.m_rgchDescription ?? string.Empty;
		}
		set
		{
			_details.m_rgchDescription = value;
		}
	}

	public string tags
	{
		get
		{
			ConvertEnumsToTags();
			return _details.m_rgchTags ?? string.Empty;
		}
		private set
		{
			_details.m_rgchTags = value;
		}
	}

	public ERemoteStoragePublishedFileVisibility visibility
	{
		get
		{
			return _details.m_eVisibility;
		}
		set
		{
			_details.m_eVisibility = value;
		}
	}

	public EItemState state => (EItemState)_state;

	public PublishedFileId_t filedID
	{
		get
		{
			return _fileID;
		}
		set
		{
			_fileID = value;
		}
	}

	private WorkshopItem()
	{
		_createItemCallResult = CallResult<CreateItemResult_t>.Create(OnCreateItemCallResult);
		_submitItemResultCallResult = CallResult<SubmitItemUpdateResult_t>.Create(OnSubmitItemResultCallResult);
		_querryCompleted = CallResult<SteamUGCQueryCompleted_t>.Create(OnQueryCompleted);
	}

	public void UpdateState()
	{
		uint itemState = SteamUGC.GetItemState(filedID);
		if (itemState != _state)
		{
			SteamWorkshopAgent.OnStateChanged.Invoke(filedID, (EItemState)_state, (EItemState)itemState);
		}
		_state = itemState;
	}

	public bool HasFileID()
	{
		return _fileID.m_PublishedFileId != 0;
	}

	public bool HasUpdateHandle()
	{
		return _updateHandle.m_UGCUpdateHandle != 0;
	}

	public void ConvertEnumsToTags()
	{
		string text = string.Empty;
		if (itemType != WorkshopItemType.Unset)
		{
			text += Enum.GetName(typeof(WorkshopItemType), itemType);
		}
		text = ((WorkshopItemTags[])Enum.GetValues(typeof(WorkshopItemTags))).Where((WorkshopItemTags itemTag) => (itemTags & itemTag) == itemTag).Aggregate(text, (string current, WorkshopItemTags itemTag) => current + "," + itemTag);
		tags = text;
	}

	public void SendUpdate()
	{
		_updateHandle = SteamUGC.StartItemUpdate(SteamWorkshopAgent.AppID, _fileID);
		if (!SteamUGC.SetItemTitle(_updateHandle, title))
		{
			logger.Info("WORKSHOP::SendUpdate - Bad Title");
		}
		if (!SteamUGC.SetItemDescription(_updateHandle, description))
		{
			logger.Info("WORKSHOP::SendUpdate - Bad Description");
		}
		if (!SteamUGC.SetItemUpdateLanguage(_updateHandle, language))
		{
			logger.Info("WORKSHOP::SendUpdate - Bad Language");
		}
		if (!SteamUGC.SetItemTags(_updateHandle, tags.Split(',')))
		{
			logger.Info("WORKSHOP::SendUpdate - Bad Tags");
		}
		if (!SteamUGC.SetItemVisibility(_updateHandle, visibility))
		{
			logger.Info("WORKSHOP::SendUpdate - Bad Visibility");
		}
		if (!SteamUGC.SetItemMetadata(_updateHandle, SerialiseMetadata()))
		{
			logger.Info("WORKSHOP::SendUpdate - Bad Metadata");
		}
		if (!SteamUGC.SetItemContent(_updateHandle, contentPath))
		{
			logger.Info("WORKSHOP::SendUpdate - Bad Content");
		}
		if (!SteamUGC.SetItemPreview(_updateHandle, GetBestPreviewUnder1MB(previewPath)))
		{
			logger.Info("WORKSHOP::SendUpdate - Bad Preview");
		}
		SteamAPICall_t hAPICall = SteamUGC.SubmitItemUpdate(_updateHandle, updateComment);
		_submitItemResultCallResult.Set(hAPICall);
		_updateInProgress = true;
	}

	public static string GetBestPreviewUnder1MB(string path)
	{
		if (!File.Exists(path))
		{
			return string.Empty;
		}
		byte[] array = File.ReadAllBytes(path);
		if (array.Length / 1000 >= 950)
		{
			byte[] array2 = array;
			int num = 115;
			Texture2D texture2D = new Texture2D(2, 2);
			while (array2.Length / 1000 >= 950)
			{
				if (num - 20 > 0)
				{
					num -= 20;
					texture2D.LoadImage(array2);
					array2 = texture2D.EncodeToJPG(num);
					continue;
				}
				GameSignals.Invoke(GameSignals.OnAutoFadeDialog, "Preview image for workshop item is too big, discarding...", 5f);
				return string.Empty;
			}
			path = string.Concat(Path.GetTempPath(), "/", Guid.NewGuid(), ".jpg");
			File.WriteAllBytes(path, array2);
		}
		return path;
	}

	public void GetDetails()
	{
		UGCQueryHandle_t handle = SteamUGC.CreateQueryUGCDetailsRequest(new PublishedFileId_t[1] { filedID }, 1u);
		SteamUGC.SetReturnLongDescription(handle, bReturnLongDescription: true);
		SteamUGC.SetReturnMetadata(handle, bReturnMetadata: true);
		SteamAPICall_t hAPICall = SteamUGC.SendQueryUGCRequest(handle);
		_querryCompleted.Set(hAPICall);
	}

	public float GetSendUpdateProgress()
	{
		if (_updateInProgress && _updateHandle.m_UGCUpdateHandle != 0)
		{
			SteamUGC.GetItemUpdateProgress(_updateHandle, out var punBytesProcessed, out var punBytesTotal);
			if (punBytesTotal == 0)
			{
				return 0f;
			}
			return (float)punBytesProcessed / (float)punBytesTotal;
		}
		return 1f;
	}

	private void OnCreateItemCallResult(CreateItemResult_t callback, bool bIOFailure)
	{
		if (bIOFailure)
		{
			logger.Info("WORKSHOP::OnCreateItemCallResult - IOFailure");
			return;
		}
		_fileID = callback.m_nPublishedFileId;
		if (callback.m_eResult == EResult.k_EResultInsufficientPrivilege)
		{
			logger.Info("WORKSHOP::OnCreateItemCallResult - Insufficient Privileges");
			GameSignals.Invoke(GameSignals.OnDisplayOkDialog, "Error", "Could not upload to Steam Workshop.\nYou do not have the permission to upload this item.", "OK", delegate
			{
			});
		}
		else if (callback.m_eResult == EResult.k_EResultTimeout)
		{
			logger.Info("WORKSHOP::OnCreateItemCallResult - Timed out");
			GameSignals.Invoke(GameSignals.OnDisplayOkDialog, "Error", "Could not upload to Steam Workshop.\nConnection timed out.", "OK", delegate
			{
			});
		}
		else if (callback.m_eResult == EResult.k_EResultNotLoggedOn)
		{
			logger.Info("WORKSHOP::OnCreateItemCallResult - Not logged in");
			GameSignals.Invoke(GameSignals.OnDisplayOkDialog, "Error", "Could not upload to Steam Workshop.\nYou are not logged in to Steam.", "OK", delegate
			{
			});
		}
		else if (callback.m_bUserNeedsToAcceptWorkshopLegalAgreement)
		{
			logger.Info("WORKSHOP::OnCreateItemCallResult - Need to agree legal agreement");
			MapWorkshopWaitingModalMenu.CloseSignal.Invoke();
			GameSignals.Invoke(GameSignals.OnDisplayDialog, I2Helper.TryGet("##mode_worldeditor_workshop_error").ToUpper(), I2Helper.TryGet("##mode_worldeditor_workshop_error_eula_info"), new string[2]
			{
				I2Helper.TryGet("##mode_worldeditor_workshop_error_eula_open").ToUpper(),
				I2Helper.TryGet("##mode_worldeditor_workshop_error_eula_ok").ToUpper()
			}, new Action[2]
			{
				delegate
				{
					SteamFriends.ActivateGameOverlayToWebPage("http://www.steamcommunity.com/workshop/workshoplegalagreement/?appid=227860");
				},
				delegate
				{
					GameSignals.Invoke(GameSignals.OnCloseDialog);
				}
			});
		}
		else
		{
			ItemCreated.Invoke();
			SteamWorkshopAgent.OnNewItemCreated.Invoke(_fileID);
		}
	}

	private void OnSubmitItemResultCallResult(SubmitItemUpdateResult_t callback, bool bIOFailure)
	{
		if (bIOFailure)
		{
			logger.Info("WORKSHOP::OnSubmitItemResultCallResult - IOFailure");
		}
		else if (callback.m_eResult == EResult.k_EResultOK)
		{
			logger.Info("WORKSHOP::OnSubmitItemResultCallResult - Update OK");
			_updateInProgress = false;
			SteamWorkshopAgent.OnItemUpdated.Invoke(_fileID);
			GetDetails();
		}
		else
		{
			logger.Info("WORKSHOP::OnSubmitItemResultCallResult - Update not ok: " + Enum.GetName(typeof(EResult), callback.m_eResult));
			MapWorkshopWaitingModalMenu.CloseSignal.Invoke();
			GameSignals.Invoke(GameSignals.OnDisplayOkDialog, I2Helper.TryGet("##mode_worldeditor_workshop_error_publish").ToUpper(), I2Helper.TryGet("##mode_worldeditor_workshop_error_publish_info").Replace("#x", Enum.GetName(typeof(EResult), callback.m_eResult)), I2Helper.TryGet("##mode_worldeditor_workshop_error_ok").ToUpper(), delegate
			{
			});
		}
	}

	private void OnQueryCompleted(SteamUGCQueryCompleted_t callback, bool bIOFailure)
	{
		if (bIOFailure)
		{
			logger.Info("WORKSHOP::OnSubmitItemResultCallResult - IOFailure");
			return;
		}
		SteamUGC.GetQueryUGCResult(callback.m_handle, 0u, out _details);
		SteamUGC.GetQueryUGCMetadata(callback.m_handle, 0u, out var pchMetadata, 1024u);
		Hashtable hashtable = DeserialiseMetadata(pchMetadata);
		itemType = (WorkshopItemType)(long)hashtable["ItemType"];
		itemTags = (WorkshopItemTags)(long)hashtable["ItemTags"];
		uint itemState = SteamUGC.GetItemState(filedID);
		if (SteamUser.GetSteamID().m_SteamID != _details.m_ulSteamIDOwner)
		{
			isLocal = false;
		}
		SteamWorkshopAgent.OnItemUpdated.Invoke(_fileID);
	}

	public void Delete()
	{
		if (_workshopItemsList.Contains(this))
		{
			_workshopItemsList.Remove(this);
		}
		ItemDeleted.Invoke();
	}

	private string SerialiseMetadata()
	{
		JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
		jsonSerializerSettings.TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple;
		Hashtable hashtable = new Hashtable();
		hashtable.Add("ItemType", itemType);
		hashtable.Add("ItemTags", itemTags);
		return JsonConvert.SerializeObject(hashtable, Formatting.Indented, jsonSerializerSettings);
	}

	private static Hashtable DeserialiseMetadata(string text)
	{
		return JsonConvert.DeserializeObject<Hashtable>(text);
	}

	public static WorkshopItem CreateItem()
	{
		if (NativeMethods.SteamClient() == IntPtr.Zero)
		{
			logger.Info("WORKSHOP::CreateItem - No Steam manager");
			GameSignals.Invoke(GameSignals.OnDisplayOkDialog, "Error", "Could not upload to Steam Workshop.\nCastle Story is not connected to Steam.", "OK", delegate
			{
			});
			return null;
		}
		WorkshopItem workshopItem = new WorkshopItem();
		workshopItem.isLocal = true;
		_workshopItemsList.Add(workshopItem);
		SteamAPICall_t hAPICall = SteamUGC.CreateItem(SteamWorkshopAgent.AppID, EWorkshopFileType.k_EWorkshopFileTypeFirst);
		workshopItem._createItemCallResult.Set(hAPICall);
		return workshopItem;
	}

	public static WorkshopItem RecoverItem(PublishedFileId_t id, bool makeLocal = false)
	{
		if (NativeMethods.SteamClient() == IntPtr.Zero)
		{
			return null;
		}
		WorkshopItem workshopItem = _workshopItemsList.Slinq().FirstOrDefault((WorkshopItem x) => x.filedID == id);
		if (workshopItem == null)
		{
			workshopItem = new WorkshopItem();
			workshopItem.filedID = id;
			workshopItem.isLocal = makeLocal;
			_workshopItemsList.Add(workshopItem);
			workshopItem.GetDetails();
			workshopItem.ItemCreated.Invoke();
		}
		return workshopItem;
	}

	public static void UpdateFiles(PublishedFileId_t[] filesIDs)
	{
		Slinq<WorkshopItem, PredicateContext<WorkshopItem, IListContext<WorkshopItem>, PublishedFileId_t[]>> slinq = _workshopItemsList.ToPooledList().Slinq().Where((WorkshopItem item, PublishedFileId_t[] ids) => !ids.Contains(item.filedID), filesIDs);
		slinq.ForEach(delegate(WorkshopItem item)
		{
			item.Delete();
		});
		slinq.Dispose();
		for (int num = 0; num < filesIDs.Length; num++)
		{
			PublishedFileId_t id = filesIDs[num];
			if (_workshopItemsList.Slinq().FirstOrDefault((WorkshopItem x) => x.filedID.m_PublishedFileId == id.m_PublishedFileId) == null)
			{
				RecoverItem(id);
			}
		}
	}

	public static WorkshopItem GetItemFromID(PublishedFileId_t id)
	{
		return _workshopItemsList.Slinq().FirstOrDefault((WorkshopItem x) => x.filedID.m_PublishedFileId == id.m_PublishedFileId);
	}

	public static List<WorkshopItem> GetAllWorkshopItems()
	{
		return _workshopItemsList;
	}

	public static List<WorkshopItem> GetAllItemsOfType(WorkshopItemType type)
	{
		return (from x in _workshopItemsList.Slinq()
			where x.itemType == type
			select x).ToList();
	}

	public static List<string> GetAllSubscribedItemPathsOfType(WorkshopItemType type)
	{
		return (from x in _workshopItemsList.Slinq()
			where x.itemType == type
			select x.contentPath).ToList();
	}

	public static WorkshopItemTags GetWorkshopItemTag(Asset_Gamemode mode)
	{
		return mode.shortCodename.ToLower() switch
		{
			"sandbox" => WorkshopItemTags.Sandbox, 
			"invasion" => WorkshopItemTags.Invasion, 
			"conquest" => WorkshopItemTags.Conquest, 
			_ => WorkshopItemTags.Sandbox, 
		};
	}
}
