using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Steamworks;

public class DllCheck
{
	[DllImport("kernel32.dll")]
	public static extern IntPtr GetModuleHandle(string lpModuleName);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
	private static extern int GetModuleFileName(IntPtr hModule, StringBuilder strFullPath, int nSize);

	public static bool Test()
	{
		return true;
	}

	private static bool CheckSteamAPIDLL()
	{
		return true;
	}
}
