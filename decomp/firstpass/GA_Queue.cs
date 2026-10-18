using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GA_Queue
{
	public delegate void EventSuccess();

	public static int MAXQUEUESIZE = 800;

	public static bool QUITONSUBMIT = false;

	private static List<GA_Submit.Item> _queue = new List<GA_Submit.Item>();

	private static List<GA_Submit.Item> _tempQueue = new List<GA_Submit.Item>();

	private static List<GA_Submit.Item> _errorQueue = new List<GA_Submit.Item>();

	private static bool _submittingData = false;

	private static int _submitCount = 0;

	private static bool _endsubmit = false;

	private static bool _userEventSuccess = false;

	public static event EventSuccess OnSuccess;

	public static void AddItem(Hashtable parameters, GA_Submit.CategoryType type, bool stack)
	{
		if (!_endsubmit && (!Application.isEditor || GA.SettingsGA.RunInEditorPlayMode))
		{
			GA_Submit.Item item = new GA_Submit.Item
			{
				Type = type,
				Parameters = parameters,
				AddTime = Time.time
			};
			if (!_userEventSuccess && type != GA_Submit.CategoryType.GA_User)
			{
				_tempQueue.Add(item);
			}
			else if (_submittingData)
			{
				_tempQueue.Add(item);
			}
			else
			{
				_queue.Add(item);
			}
		}
	}

	public static IEnumerator SubmitQueue()
	{
		while (!_endsubmit)
		{
			while (GA.SettingsGA.CustomUserID && GA.API.GenericInfo.UserID == string.Empty)
			{
				GA.LogWarning("GameAnalytics: User ID not set. No data will be sent until Custom User ID is set.");
				yield return new WaitForSeconds(10f);
			}
			while (_submittingData)
			{
				yield return new WaitForSeconds(0.5f);
			}
			ForceSubmit();
			yield return new WaitForSeconds(GA.SettingsGA.SubmitInterval);
		}
	}

	public static void ForceSubmit()
	{
		bool flag = true;
		if (!_userEventSuccess)
		{
			flag = false;
		}
		if (flag)
		{
			GA_SpecialEvents.SubmitAverageFPS();
			if (GA.SettingsGA.ArchiveData && GA.SettingsGA.InternetConnectivity)
			{
				List<GA_Submit.Item> archivedData = GA.API.Archive.GetArchivedData();
				if (archivedData != null && archivedData.Count > 0)
				{
					foreach (GA_Submit.Item item in archivedData)
					{
						AddItem(item.Parameters, item.Type, stack: false);
					}
					if (GA.SettingsGA.DebugMode)
					{
						GA.Log("GA: Network connection detected. Adding archived data to next submit queue.");
					}
				}
			}
		}
		if (_queue.Count > 0 && !_submittingData && !_endsubmit)
		{
			_submittingData = true;
			GA.Log("GameAnalytics: Submit started");
			GA.API.Submit.SubmitQueue(_queue, Submitted, SubmitError, gaTracking: false, string.Empty, string.Empty);
		}
	}

	public static void EndSubmit()
	{
		GA.Log("GA: Ending all data submission");
		_endsubmit = true;
	}

	public static void RestartSubmit()
	{
		if (_endsubmit)
		{
			GA.Log("GA: Restarting data submission");
			_endsubmit = false;
			GA.RunCoroutine(SubmitQueue());
		}
	}

	private static void Submitted(List<GA_Submit.Item> items, bool success)
	{
		_submitCount += items.Count;
		if (success)
		{
			if (GA_Queue.OnSuccess != null)
			{
				GA_Queue.OnSuccess();
			}
			GA.SettingsGA.TotalMessagesSubmitted += items.Count;
			using (List<GA_Submit.Item>.Enumerator enumerator = items.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					switch (enumerator.Current.Type)
					{
					case GA_Submit.CategoryType.GA_Event:
						GA.SettingsGA.DesignMessagesSubmitted++;
						break;
					case GA_Submit.CategoryType.GA_Error:
						GA.SettingsGA.ErrorMessagesSubmitted++;
						break;
					case GA_Submit.CategoryType.GA_Purchase:
						GA.SettingsGA.BusinessMessagesSubmitted++;
						break;
					case GA_Submit.CategoryType.GA_User:
						GA.SettingsGA.UserMessagesSubmitted++;
						break;
					}
				}
			}
			if (!_userEventSuccess)
			{
				_userEventSuccess = true;
				GA.RunCoroutine(SubmitQueue());
				GA.Log("GameAnalytics: Submission queue started.");
			}
		}
		if (_submitCount >= _queue.Count)
		{
			if (GA.SettingsGA.DebugMode)
			{
				GA.Log("GA: Queue submit over");
			}
			if (QUITONSUBMIT)
			{
				Application.Quit();
			}
			_queue = _tempQueue;
			_tempQueue = new List<GA_Submit.Item>();
			if (success)
			{
				_queue.AddRange(_errorQueue);
				_errorQueue = new List<GA_Submit.Item>();
			}
			_submitCount = 0;
			_submittingData = false;
		}
	}

	private static void SubmitError(List<GA_Submit.Item> items)
	{
		if (items == null)
		{
			GA.Log("GA: Ending all data submission after this timer interval");
			_endsubmit = true;
			return;
		}
		GA.SettingsGA.TotalMessagesFailed += items.Count;
		using (List<GA_Submit.Item>.Enumerator enumerator = items.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				switch (enumerator.Current.Type)
				{
				case GA_Submit.CategoryType.GA_Event:
					GA.SettingsGA.DesignMessagesFailed++;
					break;
				case GA_Submit.CategoryType.GA_Error:
					GA.SettingsGA.ErrorMessagesFailed++;
					break;
				case GA_Submit.CategoryType.GA_Purchase:
					GA.SettingsGA.BusinessMessagesFailed++;
					break;
				case GA_Submit.CategoryType.GA_User:
					GA.SettingsGA.UserMessagesFailed++;
					break;
				}
			}
		}
		GA.RunCoroutine(GA.SettingsGA.CheckInternetConnectivity(startQueue: false));
		_errorQueue.AddRange(items);
		if (_errorQueue.Count > MAXQUEUESIZE)
		{
			_errorQueue.Sort(new ItemComparer());
			_errorQueue.RemoveRange(MAXQUEUESIZE, _errorQueue.Count - MAXQUEUESIZE);
		}
		Submitted(items, success: false);
	}
}
