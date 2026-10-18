using Brix.External.Signals;
using Brix.Steam;
using Brix.Utils.Signals;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace Brix.UI;

public class WorkshopDownloadUIDriver : MonoBehaviour
{
	public GameObject mainGO;

	public Image preview;

	public Text description;

	public Text title;

	public Image progressBar;

	private Signal OnDestroyed = new Signal();

	private PublishedFileId_t _downloadingItem;

	private int _needsUpdate;

	private int needsUpdate
	{
		get
		{
			return _needsUpdate;
		}
		set
		{
			if (value < 0)
			{
				value = 0;
			}
			if (_needsUpdate == 0 && value != 0)
			{
				mainGO.SetActive(CanShow);
			}
			if (_needsUpdate != 0 && value == 0)
			{
				mainGO.SetActive(value: false);
			}
			_needsUpdate = value;
		}
	}

	private bool CanShow => false;

	private void Awake()
	{
		SteamWorkshopAgent.OnStateChanged.Connect(OnItemUpdate).DisconnectWhen(OnDestroyed);
	}

	private void Update()
	{
		if (!CanShow)
		{
			mainGO.SetActive(value: false);
			return;
		}
		if (_downloadingItem.m_PublishedFileId != 0)
		{
			description.text = "Downloading Workshop Item...";
			if (SteamUGC.GetItemDownloadInfo(_downloadingItem, out var punBytesDownloaded, out var punBytesTotal) && (float)punBytesTotal != 0f)
			{
				progressBar.fillAmount = (float)punBytesDownloaded / (float)punBytesTotal;
			}
			else
			{
				progressBar.fillAmount = 0.05f;
			}
		}
		if (needsUpdate > 0 && _downloadingItem.m_PublishedFileId == 0)
		{
			description.text = "Reading Workshop...";
			title.text = string.Empty;
			progressBar.fillAmount = 0.05f;
		}
	}

	private void OnDestroy()
	{
		OnDestroyed.Invoke();
	}

	private void OnItemUpdate(PublishedFileId_t item, EItemState oldState, EItemState newState)
	{
		if ((oldState & EItemState.k_EItemStateDownloading) == 0 && (newState & EItemState.k_EItemStateDownloading) == EItemState.k_EItemStateDownloading)
		{
			_downloadingItem = item;
			WorkshopItem itemFromID = WorkshopItem.GetItemFromID(_downloadingItem);
			title.text = itemFromID.title;
			progressBar.fillAmount = 0f;
		}
		else if ((newState & EItemState.k_EItemStateDownloading) == 0 && (oldState & EItemState.k_EItemStateDownloading) == EItemState.k_EItemStateDownloading && _downloadingItem.m_PublishedFileId == item.m_PublishedFileId)
		{
			_downloadingItem = default(PublishedFileId_t);
		}
		if ((oldState & EItemState.k_EItemStateNeedsUpdate) == 0 && (newState & EItemState.k_EItemStateNeedsUpdate) == EItemState.k_EItemStateNeedsUpdate)
		{
			needsUpdate++;
		}
		else if ((newState & EItemState.k_EItemStateNeedsUpdate) == 0 && (oldState & EItemState.k_EItemStateNeedsUpdate) == EItemState.k_EItemStateNeedsUpdate)
		{
			needsUpdate--;
		}
	}
}
