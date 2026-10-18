using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GA_AdSupport : MonoBehaviour
{
	public enum GAEventCat
	{
		Design,
		Business
	}

	public enum GAEventType
	{
		AdsEnabled,
		LevelChange,
		Custom
	}

	public enum GAAdNetwork
	{
		Any,
		iAd,
		Chartboost
	}

	public static GA_AdSupport GA_ADSUPPORT;

	public bool AdsEnabled;

	private bool _adShowing;

	private string _eventTriggerID = string.Empty;

	private float _timePlayed;

	private int _sessionsPlayed;

	private static bool _sessionRecorded;

	private void Awake()
	{
		if (GA_ADSUPPORT != null)
		{
			GA.LogWarning("Destroying dublicate GA_ADSUPPORT - only one is allowed per scene!");
			Object.Destroy(base.gameObject);
			return;
		}
		GA_ADSUPPORT = this;
		Object.DontDestroyOnLoad(base.gameObject);
		SaveConditions();
		if (!GA.SettingsGA.IAD_enabled)
		{
		}
	}

	private void Start()
	{
		if (GA.SettingsGA.Start_AlwaysShowAds)
		{
			EnableAds();
		}
		SceneManager.sceneLoaded += LevelLoaded;
	}

	private void Update()
	{
		if (GA.SettingsGA.Start_TimePlayed)
		{
			_timePlayed += Time.deltaTime;
			if (_timePlayed >= (float)GA.SettingsGA.TimePlayed)
			{
				EnableAds();
			}
		}
	}

	private void OnDestroy()
	{
		if (GA_ADSUPPORT == this)
		{
			GA_ADSUPPORT = null;
		}
	}

	private void OnApplicationPause(bool paused)
	{
		if (paused)
		{
			SaveConditions();
		}
	}

	private void LevelLoaded(Scene scene, LoadSceneMode mode)
	{
		SaveConditions();
		ShowAd(GAEventType.LevelChange, GAEventCat.Design, null);
	}

	public static void ShowAdStatic(GAEventType eventType, GAEventCat eventCategory, string eventID)
	{
		if (GA_ADSUPPORT != null)
		{
			GA_ADSUPPORT.ShowAd(eventType, eventCategory, eventID);
		}
	}

	private void ShowAd(GAEventType eventType, GAEventCat eventCategory, string eventID)
	{
		if (!AdsEnabled)
		{
			return;
		}
		switch (eventType)
		{
		case GAEventType.AdsEnabled:
			if (GA.SettingsGA.Trigger_AdsEnabled)
			{
				ShowAdNow("AdsEnabled");
			}
			break;
		case GAEventType.LevelChange:
			if (GA.SettingsGA.Trigger_SceneChange)
			{
				ShowAdNow("LevelChange");
			}
			break;
		case GAEventType.Custom:
		{
			foreach (GA_CustomAdTrigger customAdTrigger in GA.SettingsGA.CustomAdTriggers)
			{
				if (eventCategory == customAdTrigger.eventCat && eventID.Equals(customAdTrigger.eventID))
				{
					ShowAdNow("Custom:" + customAdTrigger.eventID);
				}
			}
			break;
		}
		}
	}

	private void ShowAdNow(string eventID)
	{
		if (_adShowing)
		{
			return;
		}
		GA.Log("GA Show Ad Now");
		bool flag = false;
		bool flag2 = false;
		_eventTriggerID = eventID;
		if (flag && flag2)
		{
			switch (Random.Range(0, 2))
			{
			case 0:
				ShowIad();
				break;
			case 1:
				ShowCB();
				break;
			}
		}
		else if (flag)
		{
			ShowIad();
		}
		else if (flag2)
		{
			ShowCB();
		}
	}

	private void ShowIad()
	{
	}

	private void ShowCB()
	{
	}

	private IEnumerator CloseAd(float duration)
	{
		yield return new WaitForSeconds(duration);
		_adShowing = false;
	}

	private void OnBannerLoaded()
	{
	}

	private void OnBannerClicked()
	{
		GA.API.Design.NewEvent("Clicks:iAD:" + _eventTriggerID);
	}

	private void SaveConditions()
	{
		bool flag = false;
		if (GA.SettingsGA.Start_TimePlayed)
		{
			float num = PlayerPrefs.GetFloat("GA_TimePlayed");
			if (num > _timePlayed)
			{
				_timePlayed = num;
			}
			PlayerPrefs.SetFloat("GA_TimePlayed", _timePlayed);
			flag = true;
		}
		if (GA.SettingsGA.Start_Sessions && !_sessionRecorded)
		{
			int num2 = PlayerPrefs.GetInt("GA_Sessions");
			if (num2 > _sessionsPlayed)
			{
				_sessionsPlayed = num2;
			}
			_sessionRecorded = true;
			_sessionsPlayed++;
			PlayerPrefs.SetInt("GA_Sessions", _sessionsPlayed);
			if (_sessionsPlayed > GA.SettingsGA.Sessions)
			{
				EnableAds();
			}
			flag = true;
		}
		if (flag)
		{
			PlayerPrefs.Save();
		}
	}

	public static void EnableAds()
	{
		if (GA_ADSUPPORT != null)
		{
			GA_ADSUPPORT.EnableShowingAds();
		}
	}

	public void EnableShowingAds()
	{
		if (!AdsEnabled)
		{
			GA.Log("GA Ads Enabled");
			AdsEnabled = true;
			ShowAd(GAEventType.AdsEnabled, GAEventCat.Design, null);
		}
	}
}
