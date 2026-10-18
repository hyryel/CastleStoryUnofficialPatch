using System;
using System.Collections;
using Brix.External.Signals;
using Brix.Utils.Logging;
using Steamworks;
using UnityEngine;

namespace Brix.Steam;

public class SteamWorkshopAgent : MonoBehaviour
{
	protected static BrixLogger logger = new BrixLogger(typeof(SteamWorkshopAgent).FullName);

	public static AppId_t AppID;

	private static bool _checkStateChanges = true;

	private static uint _subscribedItemCount = 0u;

	private static PublishedFileId_t[] _subscribedItemsList;

	public static Signal<PublishedFileId_t> OnSubscription = new Signal<PublishedFileId_t>();

	public static Signal<PublishedFileId_t> OnUnsubscription = new Signal<PublishedFileId_t>();

	public static Signal<PublishedFileId_t> OnItemInstalled = new Signal<PublishedFileId_t>();

	public static Signal<PublishedFileId_t, EResult> OnItemDownloaded = new Signal<PublishedFileId_t, EResult>();

	public static Signal<PublishedFileId_t> OnNewItemCreated = new Signal<PublishedFileId_t>();

	public static Signal<PublishedFileId_t> OnItemUpdated = new Signal<PublishedFileId_t>();

	public static Signal<PublishedFileId_t, EItemState, EItemState> OnStateChanged = new Signal<PublishedFileId_t, EItemState, EItemState>();

	private Callback<RemoteStoragePublishedFileSubscribed_t> _onSubscription;

	private Callback<RemoteStoragePublishedFileUnsubscribed_t> _onUnsubscription;

	private Callback<ItemInstalled_t> _onItemInstalled;

	private Callback<DownloadItemResult_t> _onItemDownloaded;

	private static readonly WaitForSeconds WaitForTwoSeconds = new WaitForSeconds(2f);

	private static readonly WaitForSeconds WaitForFiveSeconds = new WaitForSeconds(5f);

	public static bool IsConnected => NativeMethods.SteamClient() != IntPtr.Zero;

	private void Awake()
	{
		if (!SteamAPI.IsSteamRunning())
		{
			base.enabled = false;
			return;
		}
		AppID = SteamUtils.GetAppID();
		_onSubscription = Callback<RemoteStoragePublishedFileSubscribed_t>.Create(_OnSubscription);
		_onUnsubscription = Callback<RemoteStoragePublishedFileUnsubscribed_t>.Create(_OnUnsubscription);
		_onItemInstalled = Callback<ItemInstalled_t>.Create(_OnItemInstalled);
		_onItemDownloaded = Callback<DownloadItemResult_t>.Create(_OnItemDownloaded);
		_UpdateSubscriptionItems();
		StartCoroutine(StateChecksInfiniteLoop());
	}

	private void OnDestroy()
	{
		OnSubscription.Clear();
		OnUnsubscription.Clear();
		OnItemInstalled.Clear();
		OnItemDownloaded.Clear();
	}

	private IEnumerator StateChecksInfiniteLoop()
	{
		while (_checkStateChanges)
		{
			for (int i = 0; i != WorkshopItem.GetAllWorkshopItems().Count; i++)
			{
				WorkshopItem.GetAllWorkshopItems()[i].UpdateState();
				yield return WaitForTwoSeconds;
			}
			yield return WaitForFiveSeconds;
		}
	}

	public static int GetSubscribedItemCount()
	{
		return (int)_subscribedItemCount;
	}

	public static PublishedFileId_t[] GetSubscribedItems()
	{
		if (_subscribedItemsList == null)
		{
			_UpdateSubscriptionItems();
		}
		return _subscribedItemsList;
	}

	private void _OnSubscription(RemoteStoragePublishedFileSubscribed_t callback)
	{
		if (callback.m_nAppID.m_AppId == AppID.m_AppId)
		{
			logger.InfoFormat("WORKSHOP: New item subscribed - {0}", callback.m_nPublishedFileId);
			_UpdateSubscriptionItems();
			OnSubscription.Invoke(callback.m_nPublishedFileId);
		}
	}

	private void _OnUnsubscription(RemoteStoragePublishedFileUnsubscribed_t callback)
	{
		if (callback.m_nAppID.m_AppId == AppID.m_AppId)
		{
			logger.InfoFormat("WORKSHOP: New item unsubscribed - {0}", callback.m_nPublishedFileId);
			_UpdateSubscriptionItems();
			OnUnsubscription.Invoke(callback.m_nPublishedFileId);
		}
	}

	private void _OnItemInstalled(ItemInstalled_t callback)
	{
		if (callback.m_unAppID.m_AppId == AppID.m_AppId)
		{
			logger.InfoFormat("WORKSHOP: New item installed - {0}", callback.m_nPublishedFileId);
			OnItemInstalled.Invoke(callback.m_nPublishedFileId);
		}
	}

	private void _OnItemDownloaded(DownloadItemResult_t callback)
	{
		if (callback.m_unAppID.m_AppId == AppID.m_AppId)
		{
			logger.InfoFormat("WORKSHOP: New item downloaded - {0} result {1}", callback.m_nPublishedFileId, Enum.GetName(typeof(EResult), callback.m_eResult));
			OnItemDownloaded.Invoke(callback.m_nPublishedFileId, callback.m_eResult);
		}
	}

	private static void _UpdateSubscriptionItems()
	{
		_subscribedItemCount = SteamUGC.GetNumSubscribedItems();
		_subscribedItemsList = new PublishedFileId_t[_subscribedItemCount];
		SteamUGC.GetSubscribedItems(_subscribedItemsList, _subscribedItemCount);
		WorkshopItem.UpdateFiles(_subscribedItemsList);
	}
}
