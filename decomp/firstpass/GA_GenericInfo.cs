using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GA_GenericInfo
{
	private string _userID = string.Empty;

	private string _sessionID;

	private bool _settingUserID;

	public string UserID
	{
		get
		{
			if ((_userID == null || _userID == string.Empty) && !GA.SettingsGA.CustomUserID)
			{
				_userID = GetUserUUID();
			}
			return _userID;
		}
	}

	public string SessionID
	{
		get
		{
			if (_sessionID == null)
			{
				_sessionID = GetSessionUUID();
			}
			return _sessionID;
		}
	}

	public List<Hashtable> GetGenericInfo(string message)
	{
		List<Hashtable> list = new List<Hashtable>();
		list.Add(AddSystemSpecs(GA_Error.SeverityType.info, "unity_sdk " + GA_Settings.VERSION, message));
		list.Add(AddSystemSpecs(GA_Error.SeverityType.info, "os:" + SystemInfo.operatingSystem, message));
		list.Add(AddSystemSpecs(GA_Error.SeverityType.info, "processor_type:" + SystemInfo.processorType, message));
		list.Add(AddSystemSpecs(GA_Error.SeverityType.info, "gfx_name:" + SystemInfo.graphicsDeviceName, message));
		list.Add(AddSystemSpecs(GA_Error.SeverityType.info, "gfx_version:" + SystemInfo.graphicsDeviceVersion, message));
		return list;
	}

	public static string GetUserUUID()
	{
		try
		{
			NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
			string text = string.Empty;
			NetworkInterface[] array = allNetworkInterfaces;
			foreach (NetworkInterface networkInterface in array)
			{
				PhysicalAddress physicalAddress = networkInterface.GetPhysicalAddress();
				if (physicalAddress.ToString() != string.Empty && text == string.Empty)
				{
					text = GA_Submit.CreateSha1Hash(physicalAddress.ToString());
				}
			}
			return text;
		}
		catch
		{
			return SystemInfo.deviceUniqueIdentifier;
		}
	}

	public static string GetSessionUUID()
	{
		return Guid.NewGuid().ToString();
	}

	public void SetSessionUUID(string newSessionID)
	{
		if (newSessionID == null)
		{
			_sessionID = GetSessionUUID();
		}
		else
		{
			_sessionID = newSessionID;
		}
	}

	public void SetCustomUserID(string customID)
	{
		_userID = customID;
	}

	private Hashtable AddSystemSpecs(GA_Error.SeverityType severity, string type, string message)
	{
		string text = string.Empty;
		if (message != string.Empty)
		{
			text = ": " + message;
		}
		Hashtable hashtable = new Hashtable();
		hashtable.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.Severity], severity.ToString());
		hashtable.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.Message], type + text);
		hashtable.Add(GA_ServerFieldTypes.Fields[GA_ServerFieldTypes.FieldType.Level], (!GA.SettingsGA.CustomArea.Equals(string.Empty)) ? GA.SettingsGA.CustomArea : SceneManager.GetActiveScene().name);
		return hashtable;
	}

	public static string GetSystem()
	{
		return "LINUX";
	}
}
