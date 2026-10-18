using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GA_Settings : ScriptableObject
{
	public enum HelpTypes
	{
		None,
		IncludeSystemSpecsHelp,
		ProvideCustomUserID
	}

	public enum MessageTypes
	{
		None,
		Error,
		Info,
		Warning
	}

	public struct HelpInfo
	{
		public string Message;

		public MessageTypes MsgType;

		public HelpTypes HelpType;
	}

	public enum InspectorStates
	{
		Account,
		Basic,
		Debugging,
		Pref,
		Ads
	}

	[HideInInspector]
	public static string VERSION = "0.7.0";

	public int TotalMessagesSubmitted;

	public int TotalMessagesFailed;

	public int DesignMessagesSubmitted;

	public int DesignMessagesFailed;

	public int QualityMessagesSubmitted;

	public int QualityMessagesFailed;

	public int ErrorMessagesSubmitted;

	public int ErrorMessagesFailed;

	public int BusinessMessagesSubmitted;

	public int BusinessMessagesFailed;

	public int UserMessagesSubmitted;

	public int UserMessagesFailed;

	public string CustomArea = string.Empty;

	[SerializeField]
	public string GameKey = string.Empty;

	[SerializeField]
	public string SecretKey = string.Empty;

	[SerializeField]
	public string Build = "0.1";

	public bool SignUpOpen = true;

	public string FirstName = string.Empty;

	public string LastName = string.Empty;

	public string StudioName = string.Empty;

	public string GameName = string.Empty;

	public string PasswordConfirm = string.Empty;

	public bool EmailOptIn = true;

	public string EmailGA = string.Empty;

	[NonSerialized]
	public string PasswordGA = string.Empty;

	[NonSerialized]
	public string TokenGA = string.Empty;

	[NonSerialized]
	public string ExpireTime = string.Empty;

	[NonSerialized]
	public string LoginStatus = "Not logged in.";

	[NonSerialized]
	public int SelectedStudio;

	[NonSerialized]
	public int SelectedGame;

	[NonSerialized]
	public bool JustSignedUp;

	[NonSerialized]
	public bool HideSignupWarning;

	public bool IntroScreen = true;

	[NonSerialized]
	public List<Studio> Studios;

	public bool DebugMode = true;

	public bool DebugAddEvent;

	public bool SendExampleGameDataToMyGame;

	public bool RunInEditorPlayMode = true;

	public bool UseBundleVersion;

	public bool AllowRoaming = true;

	public bool ArchiveData = true;

	public bool NewSessionOnResume = true;

	public bool AutoSubmitUserInfo = true;

	public bool DelayQuitToSendData = true;

	public long ArchiveMaxFileSize = 2000L;

	public bool CustomUserID;

	public float SubmitInterval = 10f;

	public bool InternetConnectivity;

	public bool Start_AlwaysShowAds = true;

	public bool Start_TimePlayed;

	public bool Start_Sessions;

	public int TimePlayed = 300;

	public int Sessions = 1;

	public bool Trigger_AdsEnabled;

	public GA_AdSupport.GAAdNetwork Trigger_AdsEnabled_network;

	public bool Trigger_SceneChange = true;

	public GA_AdSupport.GAAdNetwork Trigger_SceneChange_network;

	public bool IAD_foldout = true;

	public static bool IAD_DEFAULT = true;

	public static bool CB_DEFAULT;

	public bool IAD_enabled = IAD_DEFAULT;

	public bool CB_enabled = CB_DEFAULT;

	public Vector2 IAD_position = Vector2.zero;

	public float IAD_Duration = 10f;

	public bool CB_foldout = true;

	public InspectorStates CurrentInspectorState;

	public List<HelpTypes> ClosedHints = new List<HelpTypes>();

	public bool DisplayHints;

	public Vector2 DisplayHintsScrollState;

	public Texture2D Logo;

	public Texture2D UpdateIcon;

	[NonSerialized]
	public GUIStyle SignupButton;

	[SerializeField]
	public List<GA_CustomAdTrigger> CustomAdTriggers = new List<GA_CustomAdTrigger>();

	public List<HelpInfo> GetHelpMessageList()
	{
		List<HelpInfo> list = new List<HelpInfo>();
		if (GameKey.Equals(string.Empty) || SecretKey.Equals(string.Empty))
		{
			list.Add(new HelpInfo
			{
				Message = "Please fill in your Game Key and Secret Key, obtained from the GameAnalytics website where you created your game.",
				MsgType = MessageTypes.Warning,
				HelpType = HelpTypes.None
			});
		}
		if (Build.Equals(string.Empty))
		{
			list.Add(new HelpInfo
			{
				Message = "Please fill in a name for your build, representing the current version of the game. Updating the build name for each version of the game will allow you to filter by build when viewing your data on the GA website.",
				MsgType = MessageTypes.Warning,
				HelpType = HelpTypes.None
			});
		}
		if (CustomUserID && !ClosedHints.Contains(HelpTypes.ProvideCustomUserID))
		{
			list.Add(new HelpInfo
			{
				Message = "You have indicated that you will provide a custom user ID - no data will be submitted until it is provided. This should be defined from code through: GA.Settings.SetCustomUserID",
				MsgType = MessageTypes.Info,
				HelpType = HelpTypes.ProvideCustomUserID
			});
		}
		return list;
	}

	public HelpInfo GetHelpMessage()
	{
		if (GameKey.Equals(string.Empty) || SecretKey.Equals(string.Empty))
		{
			return new HelpInfo
			{
				Message = "Please fill in your Game Key and Secret Key, obtained from the GameAnalytics website where you created your game.",
				MsgType = MessageTypes.Warning,
				HelpType = HelpTypes.None
			};
		}
		if (Build.Equals(string.Empty))
		{
			return new HelpInfo
			{
				Message = "Please fill in a name for your build, representing the current version of the game. Updating the build name for each version of the game will allow you to filter by build when viewing your data on the GA website.",
				MsgType = MessageTypes.Warning,
				HelpType = HelpTypes.None
			};
		}
		if (CustomUserID && !ClosedHints.Contains(HelpTypes.ProvideCustomUserID))
		{
			return new HelpInfo
			{
				Message = "You have indicated that you will provide a custom user ID - no data will be submitted until it is provided. This should be defined from code through: GA.Settings.SetCustomUserID",
				MsgType = MessageTypes.Info,
				HelpType = HelpTypes.ProvideCustomUserID
			};
		}
		return new HelpInfo
		{
			Message = "No hints to display. The \"Reset Hints\" button resets closed hints.",
			MsgType = MessageTypes.None,
			HelpType = HelpTypes.None
		};
	}

	public IEnumerator CheckInternetConnectivity(bool startQueue)
	{
		if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork && !GA.SettingsGA.AllowRoaming)
		{
			InternetConnectivity = false;
		}
		else
		{
			WWW www = new WWW(GA_Submit.GetBaseURL(inclVersion: true) + "/ping");
			yield return www;
			try
			{
				if (GA_Submit.CheckServerReply(www))
				{
					InternetConnectivity = true;
				}
				else if (!string.IsNullOrEmpty(www.error))
				{
					InternetConnectivity = false;
				}
				else
				{
					Hashtable hashtable = (Hashtable)GA_MiniJSON.JsonDecode(www.text);
					if (hashtable != null && hashtable.ContainsKey("status") && hashtable["status"].ToString().Equals("ok"))
					{
						InternetConnectivity = true;
					}
					else
					{
						InternetConnectivity = false;
					}
				}
			}
			catch
			{
				InternetConnectivity = false;
			}
		}
		if (!startQueue)
		{
			yield break;
		}
		if (InternetConnectivity)
		{
			GA.Log("GA has confirmed connection to the server..");
		}
		else
		{
			GA.Log("GA has no connection to the server..");
		}
		if (AddUniqueIDs())
		{
			while (GA.SettingsGA.CustomUserID && GA.API.GenericInfo.UserID == string.Empty)
			{
				yield return new WaitForSeconds(5f);
			}
			GA_Queue.ForceSubmit();
			GameObject fbGameObject = new GameObject("GA_FacebookSDK");
			fbGameObject.AddComponent<GA_FacebookSDK>();
		}
		else
		{
			GA.LogWarning("GA failed to add unique IDs and will not send any data. If you are using iOS or Android please see the readme file in the iOS/Android folder in the GameAnalytics/Plugins directory.");
		}
	}

	private bool AddUniqueIDs()
	{
		bool flag = false;
		string deviceModel = SystemInfo.deviceModel;
		string text = string.Empty;
		string[] array = SystemInfo.operatingSystem.Split(' ');
		if (array.Length > 0)
		{
			text = array[0];
		}
		GA.API.User.NewUser(GA_User.Gender.Unknown, null, null, null, null, (!AutoSubmitUserInfo) ? null : GA_GenericInfo.GetSystem(), (!AutoSubmitUserInfo) ? null : deviceModel, (!AutoSubmitUserInfo) ? null : text, (!AutoSubmitUserInfo) ? null : SystemInfo.operatingSystem, "GA Unity SDK " + VERSION, null);
		return true;
	}

	public string GetUniqueIDiOS()
	{
		return string.Empty;
	}

	public string GetAdvertisingIDAndroid()
	{
		return null;
	}

	public void SetCustomUserID(string customID)
	{
		if (customID != string.Empty)
		{
			GA.API.GenericInfo.SetCustomUserID(customID);
		}
	}

	public void SetCustomArea(string customArea)
	{
		CustomArea = customArea;
	}

	public void SetKeys(string gamekey, string secretkey)
	{
		GA.API.Submit.SetupKeys(gamekey, secretkey);
		GameKey = gamekey;
		SecretKey = secretkey;
	}
}
