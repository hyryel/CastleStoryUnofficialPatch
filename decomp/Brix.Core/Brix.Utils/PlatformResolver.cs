using System;
using UnityEngine;

namespace Brix.Utils;

public class PlatformResolver
{
	public static Platform GetPlatform()
	{
		OperatingSystem oSVersion = Environment.OSVersion;
		switch (oSVersion.Platform)
		{
		case PlatformID.Unix:
		{
			string operatingSystem = SystemInfo.operatingSystem;
			if (operatingSystem.StartsWith("Mac OS"))
			{
				return Platform.MacOS;
			}
			return Platform.Unix;
		}
		case PlatformID.MacOSX:
			return Platform.MacOS;
		case PlatformID.Xbox:
			return Platform.WebPlayer;
		default:
			return Platform.WebPlayer;
		case PlatformID.Win32S:
		case PlatformID.Win32Windows:
		case PlatformID.Win32NT:
		case PlatformID.WinCE:
			if (oSVersion.Version.Major >= 6)
			{
				return Platform.WindowsVista7;
			}
			return Platform.WindowsXP;
		}
	}
}
